using System;
using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Scripting.LifecycleManagement;

namespace Unity.U2D.Physics
{
    /// <summary>
    /// Merges overlapping and touching geometry into the surfaces of their union, at a cost proportional to what changed rather than to the whole set.
    /// </summary>
    /// <remarks>
    /// Contour groups go in and chain segments come out. Adding and removing are bookkeeping only; all the work happens in <see cref="Compute"/>.
    /// Each produced segment occupies a slot that persists for as long as that segment exists, so a caller's shapes stay married to their slots and only the reported slots need touching.
    /// </remarks>
    public sealed partial class PhysicsGeometrySegmenter : IDisposable
    {
        /// <summary>
        /// Create a segmenter.
        /// </summary>
        /// <remarks>
        /// The tolerance decides how far out of true two pieces of geometry can be and still merge, so it is the one value worth
        /// choosing deliberately. Deriving it from <see cref="PhysicsWorld.linearSlop"/> is usually right, since that is already
        /// the distance physics treats as insignificant, and passing zero or less does exactly that. See <see cref="defaultTolerance"/>.
        /// </remarks>
        /// <param name="tolerance">How far apart two vertices, or two lines, may be and still be treated as one, in world units. Zero or less follows the project's scale.</param>
        public PhysicsGeometrySegmenter(float tolerance = 0f)
        {
            m_Tolerance = tolerance > 0f ? tolerance : defaultTolerance;

            m_Vertices = new NativeList<float2>(1024, Allocator.Persistent);
            m_ContourStart = new NativeList<int>(64, Allocator.Persistent);
            m_ContourCount = new NativeList<int>(64, Allocator.Persistent);

            m_GroupContourStart = new NativeList<int>(64, Allocator.Persistent);
            m_GroupContourCount = new NativeList<int>(64, Allocator.Persistent);
            m_GroupBounds = new NativeList<PhysicsAABB>(64, Allocator.Persistent);
            m_GroupLive = new NativeList<bool>(64, Allocator.Persistent);
            m_GroupHandles = new NativeList<PhysicsHandle>(64, Allocator.Persistent);
            m_GroupProxies = new NativeList<PhysicsSpace.ProxyHandle>(64, Allocator.Persistent);
            m_FreeGroups = new NativeList<int>(16, Allocator.Persistent);

            m_ClippedStart = new NativeList<int>(64, Allocator.Persistent);
            m_ClippedCount = new NativeList<int>(64, Allocator.Persistent);
            m_Clipped = new NativeList<Segment>(1024, Allocator.Persistent);
            m_LineOfClipped = new NativeList<int>(1024, Allocator.Persistent);

            m_Working = new NativeList<Segment>(1024, Allocator.Persistent);
            m_Slots = new NativeList<Segment>(1024, Allocator.Persistent);
            m_SlotUsed = new NativeList<bool>(1024, Allocator.Persistent);

            m_DirtyFlags = new NativeList<bool>(64, Allocator.Persistent);
            m_DirtyGroups = new NativeList<int>(64, Allocator.Persistent);
            m_StreamIndexOfGroup = new NativeList<int>(64, Allocator.Persistent);
            m_NeighbourStart = new NativeList<int>(64, Allocator.Persistent);
            m_NeighbourCount = new NativeList<int>(64, Allocator.Persistent);
            m_NeighbourData = new NativeList<int>(256, Allocator.Persistent);

            m_ClippedPrevious = new NativeList<Segment>(1024, Allocator.Persistent);
            m_ClippedStartPrevious = new NativeList<int>(64, Allocator.Persistent);
            m_ClippedCountPrevious = new NativeList<int>(64, Allocator.Persistent);
            m_LineOfClippedPrevious = new NativeList<int>(1024, Allocator.Persistent);

            m_WorkingPrevious = new NativeList<Segment>(1024, Allocator.Persistent);
            m_Clustered = new NativeList<Segment>(1024, Allocator.Persistent);

            m_RepresentativeOfPoint = new NativeHashMap<PointKey, float2>(2048, Allocator.Persistent);
            m_MembersOfRepresentative = new NativeParallelMultiHashMap<PointKey, float2>(2048, Allocator.Persistent);
            m_PointBuckets = new NativeParallelMultiHashMap<int, float2>(2048, Allocator.Persistent);

            m_LineMembers = new NativeList<int>(1024, Allocator.Persistent);
            m_LineStart = new NativeList<int>(256, Allocator.Persistent);
            m_LineCount = new NativeList<int>(256, Allocator.Persistent);

            m_LineOfSegment = new NativeHashMap<Segment, int>(1024, Allocator.Persistent);
            m_LineSeed = new NativeList<Segment>(256, Allocator.Persistent);
            m_LineLive = new NativeList<bool>(256, Allocator.Persistent);
            m_LineMemberCount = new NativeList<int>(256, Allocator.Persistent);
            m_LineFreeIds = new NativeList<int>(16, Allocator.Persistent);
            m_LineBuckets = new NativeParallelMultiHashMap<int, int>(256, Allocator.Persistent);
            m_PendingRemovals = new NativeList<int>(16, Allocator.Persistent);

            m_EndingAt = new NativeParallelMultiHashMap<PointKey, int>(1024, Allocator.Persistent);
            m_StartingAt = new NativeParallelMultiHashMap<PointKey, int>(1024, Allocator.Persistent);

            m_Dirty = new NativeList<int>(64, Allocator.Persistent);
            m_Changed = new NativeList<int>(64, Allocator.Persistent);
            m_Added = new NativeList<int>(64, Allocator.Persistent);
            m_Removed = new NativeList<int>(64, Allocator.Persistent);

            m_HandleToGroup = new NativeHashMap<PhysicsHandle, int>(64, Allocator.Persistent);

            // An unbound space, so its proxies stand for this segmenter's own contour groups rather than for shapes in a world.
            // Nothing else can appear in it, which is why a query needs no filtering by owner.
            m_Space = PhysicsSpace.Create();
        }

        /// <summary>
        /// Add a contour group, placed by the given transform.
        /// </summary>
        /// <remarks>
        /// The contours are copied, so the caller need not keep them. This only records the group; call <see cref="Compute"/> to produce segments.
        /// </remarks>
        /// <param name="group">The geometry to add.</param>
        /// <param name="transform">Where to place it.</param>
        /// <returns>A handle identifying the group, for use with <see cref="Remove"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the group is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the segmenter has been disposed.</exception>
        public PhysicsHandle Add(ContourGroupGeometry group, PhysicsTransform transform)
        {
            ThrowIfDisposed();
            ThrowIfPending();

            if (group == null)
                throw new ArgumentNullException(nameof(group));

            var index = AcquireGroup();
            var handle = PhysicsHandle.Create();

            m_GroupHandles[index] = handle;
            m_HandleToGroup.Add(handle, index);

            StoreContours(index, group, transform);
            m_GroupLive[index] = true;

            // One proxy per group, carrying the group's bounds already grown by the tolerance, so a query finds
            // groups that merely touch without every caller having to remember to inflate what it asks for.
            m_GroupProxies[index] = m_Space.CreateProxy(m_GroupBounds[index], PhysicsMask.All, handle);

            MarkDirty(index);
            return handle;
        }

        /// <summary>
        /// Remove a previously added contour group.
        /// </summary>
        /// <param name="handle">The handle returned when the group was added.</param>
        /// <returns>Whether a group was removed.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the segmenter has been disposed.</exception>
        public bool Remove(PhysicsHandle handle)
        {
            ThrowIfDisposed();
            ThrowIfPending();

            if (!m_HandleToGroup.TryGetValue(handle, out var index))
                return false;

            // Anything that was overlapping this group has to be reconsidered, since its own edges may have been buried by it.
            MarkNeighboursDirty(index);

            // The group's vertices stay in the store until the next compact, but they no longer count as live.
            var from = m_GroupContourStart[index];
            var to = from + m_GroupContourCount[index];
            for (var c = from; c < to; ++c)
                m_LiveVertices -= m_ContourCount[c];

            m_Space.DestroyProxy(m_GroupProxies[index]);
            m_GroupProxies[index] = default;

            m_HandleToGroup.Remove(handle);
            m_GroupLive[index] = false;
            m_GroupHandles[index] = default;
            m_GroupContourCount[index] = 0;

            // The clipped range is left as it was rather than zeroed here, so the next compute can still see what this
            // group last contributed and take those segments out of the line registry. GatherClippedJob then zeroes it
            // going forward on its own, since a group with groupLive false never contributes to the new store.
            m_PendingRemovals.Add(index);
            m_FreeGroups.Add(index);

            handle.Destroy();
            return true;
        }

        /// <summary>
        /// Start producing the segments for everything added so far, recomputing only what the pending changes affect.
        /// </summary>
        /// <remarks>
        /// The work runs on worker threads, so the returned handle is what says when it has finished. Reading any result waits for
        /// it, while adding, removing or computing again before the results have been read throws, since the segmenter would
        /// otherwise change underneath work already in flight. Once the results have been read, <see cref="changedIndices"/>,
        /// <see cref="addedIndices"/> and <see cref="removedIndices"/> describe what a caller must do to stay in step.
        /// </remarks>
        /// <returns>A handle for the scheduled work.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a previous compute's results have not been read yet.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the segmenter has been disposed.</exception>
        public JobHandle Compute()
        {
            ThrowIfDisposed();
            ThrowIfPending();

            // Reclaim the store before anything reads it, once removed groups account for most of what it holds.
            // Compacting rewrites every live group's ranges, so it is not worth doing for a small amount of waste.
            if (m_Vertices.Length > CompactFloor && m_LiveVertices * 2 < m_Vertices.Length)
                Compact();

            ExpandDirtyByOneHop();
            m_ReconsideredGroups = m_Dirty.Length;

            // The space answers what each group has to be clipped against, and it can only be asked from the main thread, so that
            // question is settled here and handed to the job as a flat table.
            BuildNeighbourTable();

            m_Handle = Schedule();
            m_Pending = true;

            return m_Handle;
        }

        /// <summary>
        /// Whether a slot currently holds a segment.
        /// </summary>
        /// <param name="slot">The slot to test.</param>
        /// <returns>True when the slot holds a segment.</returns>
        public bool IsSlotOccupied(int slot)
        {
            CompleteResults();

            return slot >= 0 && slot < m_SlotUsed.Length && m_SlotUsed[slot];
        }

        /// <summary>
        /// The tolerance suited to the project's scale, derived from the distance physics already treats as insignificant.
        /// </summary>
        /// <remarks>
        /// Twice <see cref="PhysicsWorld.linearSlop"/>. Chain segments require their vertices to be further apart than one
        /// linear slop, so a merge tolerance has to sit above that to be usable at all.
        /// </remarks>
        public static float defaultTolerance => PhysicsWorld.linearSlop * 2f;

        /// <summary>
        /// Release everything the segmenter owns.
        /// </summary>
        public void Dispose()
        {
            if (m_Disposed)
                return;

            // Nothing can be released while work in flight is still reading it.
            m_Handle.Complete();
            m_Pending = false;

            var handles = m_HandleToGroup.GetKeyArray(Allocator.Temp);
            for (var i = 0; i < handles.Length; ++i)
                handles[i].Destroy();

            handles.Dispose();
            m_HandleToGroup.Dispose();

            if (m_Space.isValid)
                m_Space.Destroy();

            m_Vertices.Dispose();
            m_ContourStart.Dispose();
            m_ContourCount.Dispose();

            m_GroupContourStart.Dispose();
            m_GroupContourCount.Dispose();
            m_GroupBounds.Dispose();
            m_GroupLive.Dispose();
            m_GroupHandles.Dispose();
            m_GroupProxies.Dispose();
            m_FreeGroups.Dispose();

            m_ClippedStart.Dispose();
            m_ClippedCount.Dispose();
            m_Clipped.Dispose();
            m_LineOfClipped.Dispose();

            m_Working.Dispose();
            m_Slots.Dispose();
            m_SlotUsed.Dispose();

            m_DirtyFlags.Dispose();
            m_DirtyGroups.Dispose();
            m_StreamIndexOfGroup.Dispose();
            m_NeighbourStart.Dispose();
            m_NeighbourCount.Dispose();
            m_NeighbourData.Dispose();

            m_ClippedPrevious.Dispose();
            m_ClippedStartPrevious.Dispose();
            m_ClippedCountPrevious.Dispose();
            m_LineOfClippedPrevious.Dispose();

            m_WorkingPrevious.Dispose();
            m_Clustered.Dispose();

            m_RepresentativeOfPoint.Dispose();
            m_MembersOfRepresentative.Dispose();
            m_PointBuckets.Dispose();

            m_LineMembers.Dispose();
            m_LineStart.Dispose();
            m_LineCount.Dispose();

            m_LineOfSegment.Dispose();
            m_LineSeed.Dispose();
            m_LineLive.Dispose();
            m_LineMemberCount.Dispose();
            m_LineFreeIds.Dispose();
            m_LineBuckets.Dispose();
            m_PendingRemovals.Dispose();

            m_EndingAt.Dispose();
            m_StartingAt.Dispose();

            m_Dirty.Dispose();
            m_Changed.Dispose();
            m_Added.Dispose();
            m_Removed.Dispose();

            m_Disposed = true;
        }

        /// <summary>
        /// The geometry held in a slot, ready to be given to a shape.
        /// </summary>
        /// <remarks>
        /// Only meaningful for an occupied slot. See <see cref="IsSlotOccupied"/>.
        /// </remarks>
        public ChainSegmentGeometry this[int slot]
        {
            get
            {
                CompleteResults();

                var segment = m_Slots[slot];
                return new ChainSegmentGeometry(SegmentGeometry.Create(segment.start, segment.end), segment.ghost1, segment.ghost2);
            }
        }

        /// <summary>
        /// The group a slot's segment came from, as the handle that group was added with.
        /// </summary>
        /// <remarks>
        /// A merged surface never spans two groups, so every segment has exactly one origin. Use this to carry whatever belongs to
        /// the original geometry, such as a surface material or a piece of user data, onto the shape built from it. Only meaningful
        /// for an occupied slot. See <see cref="IsSlotOccupied"/>.
        /// </remarks>
        /// <param name="slot">The slot to read.</param>
        /// <returns>The handle of the group the slot's segment came from.</returns>
        public PhysicsHandle GetSlotSource(int slot)
        {
            CompleteResults();

            var owner = m_Slots[slot].owner;

            return owner >= 0 && owner < m_GroupHandles.Length ? m_GroupHandles[owner] : default;
        }

        /// <summary>
        /// The slots whose segment changed in the last compute, so their existing shape needs new geometry.
        /// </summary>
        public ReadOnlySpan<int> changedIndices
        {
            get
            {
                CompleteResults();

                return m_Changed.AsArray().AsReadOnlySpan();
            }
        }

        /// <summary>
        /// The slots filled for the first time in the last compute, so they need a shape creating.
        /// </summary>
        /// <remarks>
        /// Never non-empty at the same time as <see cref="removedIndices"/>, because emptied slots are reused before new ones are handed out.
        /// </remarks>
        public ReadOnlySpan<int> addedIndices
        {
            get
            {
                CompleteResults();

                return m_Added.AsArray().AsReadOnlySpan();
            }
        }

        /// <summary>
        /// The slots emptied in the last compute, so their shape needs destroying.
        /// </summary>
        public ReadOnlySpan<int> removedIndices
        {
            get
            {
                CompleteResults();

                return m_Removed.AsArray().AsReadOnlySpan();
            }
        }

        /// <summary>
        /// How many slots exist, occupied or not.
        /// </summary>
        public int slotCount
        {
            get
            {
                CompleteResults();

                return m_Slots.Length;
            }
        }

        /// <summary>
        /// How many contour groups are currently held.
        /// </summary>
        public int groupCount => m_HandleToGroup.Count;

        /// <summary>
        /// How many contour groups the last compute had to reconsider.
        /// </summary>
        /// <remarks>
        /// A change spreads one step outwards through touching groups, so this counts the changed groups plus their neighbours.
        /// </remarks>
        public int reconsideredGroupCount => m_ReconsideredGroups;

        /// <summary>
        /// How far apart two vertices, or two lines, may be and still be treated as one.
        /// </summary>
        public float tolerance => m_Tolerance;

        #region Group storage

        // Take a group index, reusing one whose group was removed if there is one.
        private int AcquireGroup()
        {
            if (m_FreeGroups.Length > 0)
            {
                var reused = m_FreeGroups[m_FreeGroups.Length - 1];
                m_FreeGroups.RemoveAt(m_FreeGroups.Length - 1);
                return reused;
            }

            m_GroupContourStart.Add(0);
            m_GroupContourCount.Add(0);
            m_GroupBounds.Add(default);
            m_GroupLive.Add(false);
            m_GroupHandles.Add(default);
            m_GroupProxies.Add(default);
            m_ClippedStart.Add(0);
            m_ClippedCount.Add(0);

            return m_GroupContourStart.Length - 1;
        }

        // Copy a group's contours into native storage, transformed into place, and record its bounds.
        // Storage grows as groups arrive and is reclaimed by Compact once enough of it belongs to removed groups,
        // so repeated painting and erasing settles at a high-water mark rather than growing without bound.
        private void StoreContours(int index, ContourGroupGeometry group, PhysicsTransform transform)
        {
            var contourStart = m_ContourStart.Length;
            var startVertices = m_Vertices.Length;
            var contours = 0;

            var lower = new float2(float.MaxValue, float.MaxValue);
            var upper = new float2(float.MinValue, float.MinValue);

            for (var c = 0; c < group.contourCount; ++c)
            {
                var contour = group[c];
                if (!contour.isValid)
                    continue;

                var vertexStart = m_Vertices.Length;
                for (var v = 0; v < contour.vertexCount; ++v)
                {
                    var vertex = transform.TransformPoint(contour[v]);
                    m_Vertices.Add(vertex);

                    lower = math.min(lower, vertex);
                    upper = math.max(upper, vertex);
                }

                m_ContourStart.Add(vertexStart);
                m_ContourCount.Add(m_Vertices.Length - vertexStart);
                ++contours;
            }

            m_GroupContourStart[index] = contourStart;
            m_GroupContourCount[index] = contours;
            m_LiveVertices += m_Vertices.Length - startVertices;

            // The bounds are grown by the tolerance so groups that merely touch still register as neighbours.
            var margin = new float2(m_Tolerance, m_Tolerance);
            m_GroupBounds[index] = contours > 0
                ? new PhysicsAABB(lower - margin, upper + margin)
                : default;
        }

        // Rebuild the vertex and contour stores holding only the live groups, dropping whatever removed groups left behind.
        // Only worth doing once enough of the store is dead, since it rewrites every live group's ranges.
        private void Compact()
        {
            var vertices = new NativeList<float2>(m_LiveVertices, Allocator.Temp);
            var contourStart = new NativeList<int>(m_ContourStart.Length, Allocator.Temp);
            var contourCount = new NativeList<int>(m_ContourCount.Length, Allocator.Temp);

            for (var group = 0; group < m_GroupLive.Length; ++group)
            {
                if (!m_GroupLive[group])
                {
                    m_GroupContourStart[group] = 0;
                    m_GroupContourCount[group] = 0;
                    continue;
                }

                var from = m_GroupContourStart[group];
                var to = from + m_GroupContourCount[group];
                var movedStart = contourStart.Length;

                for (var c = from; c < to; ++c)
                {
                    var vertexFrom = m_ContourStart[c];
                    var vertexCount = m_ContourCount[c];

                    contourStart.Add(vertices.Length);
                    contourCount.Add(vertexCount);

                    for (var v = 0; v < vertexCount; ++v)
                        vertices.Add(m_Vertices[vertexFrom + v]);
                }

                m_GroupContourStart[group] = movedStart;
            }

            m_Vertices.Clear();
            m_Vertices.AddRange(vertices.AsArray());

            m_ContourStart.Clear();
            m_ContourStart.AddRange(contourStart.AsArray());

            m_ContourCount.Clear();
            m_ContourCount.AddRange(contourCount.AsArray());

            m_LiveVertices = m_Vertices.Length;

            contourCount.Dispose();
            contourStart.Dispose();
            vertices.Dispose();
        }

        private void MarkDirty(int index)
        {
            if (!m_Dirty.Contains(index))
                m_Dirty.Add(index);
        }

        // Mark every live group whose bounds meet this one's.
        private void MarkNeighboursDirty(int index)
        {
            var neighbours = FindNeighbours(index, Allocator.Temp);
            foreach (var neighbour in neighbours)
            {
                if (neighbour != index)
                    MarkDirty(neighbour);
            }

            neighbours.Dispose();
        }

        // A change to one group alters the segments of everything it touches, and those are what its neighbours cancel against,
        // so the stale set has to grow by one hop before anything is recomputed.
        private void ExpandDirtyByOneHop()
        {
            var seeds = m_Dirty.Length;
            for (var i = 0; i < seeds; ++i)
            {
                var neighbours = FindNeighbours(m_Dirty[i], Allocator.Temp);
                foreach (var neighbour in neighbours)
                    MarkDirty(neighbour);

                neighbours.Dispose();
            }
        }

        // The live groups whose bounds meet this group's, itself included, found through the space rather than by scanning.
        private NativeList<int> FindNeighbours(int index, Allocator allocator)
        {
            var results = new NativeList<int>(8, allocator);

            var overlaps = m_Space.OverlapAABB(m_GroupBounds[index], PhysicsMask.All);
            foreach (var overlap in overlaps)
            {
                if (m_HandleToGroup.TryGetValue(overlap.userHandle, out var group) && m_GroupLive[group])
                    results.Add(group);
            }

            overlaps.Dispose();
            return results;
        }

        #endregion

        #region Slots

        // Place the freshly computed segments into the slots a caller addresses its shapes by.
        // A segment identical to the one already in its slot stays put and is never reported; emptied slots are reused
        // before any new slot is handed out, which is what keeps additions and removals from ever being reported together.
        private void PlaceInSlots(NativeList<Segment> segments)
        {
            m_Changed.Clear();
            m_Added.Clear();
            m_Removed.Clear();

            var keeping = new NativeArray<bool>(m_Slots.Length, Allocator.Temp);
            var placed = new NativeArray<bool>(segments.Length, Allocator.Temp);

            // Index the occupied slots by the segment they hold, so an unchanged segment finds its own slot by lookup.
            // Cancellation leaves no duplicates, so one entry per segment is enough and the first writer wins on any collision.
            var slotOfSegment = new NativeHashMap<Segment, int>(m_Slots.Length, Allocator.Temp);
            for (var slot = 0; slot < m_Slots.Length; ++slot)
            {
                if (m_SlotUsed[slot])
                    slotOfSegment.TryAdd(m_Slots[slot], slot);
            }

            for (var i = 0; i < segments.Length; ++i)
            {
                if (!slotOfSegment.TryGetValue(segments[i], out var slot) || keeping[slot])
                    continue;

                keeping[slot] = true;
                placed[i] = true;
            }

            // Slots that still hold a stale segment are offered first, and empty ones only once those run out.
            // Reusing an occupied slot costs the caller a geometry assignment, whereas filling an empty one costs a
            // shape creation, so taking them in this order is what keeps a compute from ever reporting an addition and
            // a removal together. Offering empty slots first would create a shape here and destroy one there for no reason.
            var available = new NativeList<int>(16, Allocator.Temp);
            for (var slot = 0; slot < m_Slots.Length; ++slot)
            {
                if (!keeping[slot] && m_SlotUsed[slot])
                    available.Add(slot);
            }

            for (var slot = 0; slot < m_Slots.Length; ++slot)
            {
                if (!keeping[slot] && !m_SlotUsed[slot])
                    available.Add(slot);
            }

            var nextAvailable = 0;
            for (var i = 0; i < segments.Length; ++i)
            {
                if (placed[i])
                    continue;

                if (nextAvailable < available.Length)
                {
                    var slot = available[nextAvailable++];

                    // Overwriting a slot that held a segment is a geometry change; filling an empty one is an addition.
                    if (m_SlotUsed[slot])
                        m_Changed.Add(slot);
                    else
                        m_Added.Add(slot);

                    m_Slots[slot] = segments[i];
                    m_SlotUsed[slot] = true;
                }
                else
                {
                    m_Slots.Add(segments[i]);
                    m_SlotUsed.Add(true);
                    m_Added.Add(m_Slots.Length - 1);
                }
            }

            // Anything left over was holding a segment nobody claimed, so it is now surplus.
            for (var i = nextAvailable; i < available.Length; ++i)
            {
                var slot = available[i];
                if (!m_SlotUsed[slot])
                    continue;

                m_SlotUsed[slot] = false;
                m_Removed.Add(slot);
            }

            slotOfSegment.Dispose();
            available.Dispose();
            placed.Dispose();
            keeping.Dispose();
        }

        #endregion

        #region Guards

        private void ThrowIfDisposed()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(PhysicsGeometrySegmenter));
        }

        // Changing what work in flight is reading would corrupt its answer, so anything that changes the input is refused until
        // the results have been read.
        private void ThrowIfPending()
        {
            if (m_Pending)
                throw new InvalidOperationException($"{nameof(PhysicsGeometrySegmenter)} holds results from a previous {nameof(Compute)} that have not been read. Read them before changing the geometry or computing again.");
        }

        // Wait for the scheduled work, then place its segments into slots, which is what turns them into results a caller can read.
        private void CompleteResults()
        {
            if (!m_Pending)
                return;

            m_Handle.Complete();
            m_Pending = false;

            PlaceInSlots(m_Clustered);
            m_Dirty.Clear();
        }

        // Diagnostic only: when set, Schedule completes and times each stage in turn and logs the breakdown, so the cost of a
        // compute can be attributed to a specific pass rather than measured only as one end-to-end number.
        [AutoStaticsCleanup] internal static bool logStageTimings;

        // Chain the passes, parallel wherever a pass's pieces cannot see each other.
        //
        // Clipping is per group, the sweep is per line and the ghosts are per segment, so those three run across worker threads.
        // Clustering claims points as it walks them, so its answer depends on the order it visits them and it stays on one thread.
        // Each parallel pass writes into its own stream buffer and a gathering pass reads those buffers back in order, which is what
        // keeps the result identical run to run however the work is spread.
        private JobHandle Schedule()
        {
            if (logStageTimings)
                return ScheduleTimed();

            // The gathering pass reads the previous clipped store while writing the new one, so the two swap rather than copy.
            Swap(ref m_Clipped, ref m_ClippedPrevious);
            Swap(ref m_ClippedStart, ref m_ClippedStartPrevious);
            Swap(ref m_ClippedCount, ref m_ClippedCountPrevious);
            Swap(ref m_LineOfClipped, ref m_LineOfClippedPrevious);

            // Clustering reads the previous raw sweep while writing the new one, for the same reason.
            Swap(ref m_Working, ref m_WorkingPrevious);

            var groups = new GroupView(m_Vertices, m_ContourStart, m_ContourCount, m_GroupContourStart, m_GroupContourCount);

            var clippedStream = new NativeStream(math.max(m_DirtyGroups.Length, 1), Allocator.TempJob);

            var clip = new ClipJob
            {
                groups = groups,
                dirtyGroups = m_DirtyGroups,
                neighbourStart = m_NeighbourStart,
                neighbourCount = m_NeighbourCount,
                neighbourData = m_NeighbourData,
                tolerance = m_Tolerance,
                output = clippedStream.AsWriter()
            }.Schedule(m_DirtyGroups.Length, 1);

            var gatherClipped = new GatherClippedJob
            {
                groupLive = m_GroupLive,
                streamIndexOfGroup = m_StreamIndexOfGroup,
                previous = m_ClippedPrevious,
                previousStart = m_ClippedStartPrevious,
                previousCount = m_ClippedCountPrevious,
                lineOfClippedPrevious = m_LineOfClippedPrevious,
                clipped = m_Clipped,
                clippedStart = m_ClippedStart,
                clippedCount = m_ClippedCount,
                lineOfClipped = m_LineOfClipped,
                input = clippedStream.AsReader()
            }.Schedule(clip);

            var group = new IncrementalLineGroupJob
            {
                clipped = m_Clipped,
                clippedStart = m_ClippedStart,
                clippedCount = m_ClippedCount,
                clippedPrevious = m_ClippedPrevious,
                clippedStartPrevious = m_ClippedStartPrevious,
                clippedCountPrevious = m_ClippedCountPrevious,
                dirtyGroups = m_DirtyGroups,
                lineOfClipped = m_LineOfClipped,
                pendingRemovals = m_PendingRemovals,
                lineOfSegment = m_LineOfSegment,
                lineSeed = m_LineSeed,
                lineLive = m_LineLive,
                lineMemberCount = m_LineMemberCount,
                lineFreeIds = m_LineFreeIds,
                lineBuckets = m_LineBuckets,
                lineMembers = m_LineMembers,
                lineStart = m_LineStart,
                lineCount = m_LineCount,
                tolerance = m_Tolerance
            }.Schedule(gatherClipped);

            // One buffer per line, and how many lines there are is only known once the grouping has run.
            var handle = NativeStream.ScheduleConstruct(out var sweptStream, m_LineStart, group, Allocator.TempJob);

            var sweep = new SweepJob
            {
                segments = m_Clipped.AsDeferredJobArray(),
                lineMembers = m_LineMembers.AsDeferredJobArray(),
                lineStart = m_LineStart.AsDeferredJobArray(),
                lineCount = m_LineCount.AsDeferredJobArray(),
                output = sweptStream.AsWriter()
            }.Schedule(m_LineStart, 1, handle);

            var gatherSwept = new GatherSweptJob
            {
                working = m_Working,
                input = sweptStream.AsReader()
            }.Schedule(sweep);

            var cluster = new IncrementalClusterJob
            {
                working = m_Working,
                workingPrevious = m_WorkingPrevious,
                representativeOfPoint = m_RepresentativeOfPoint,
                membersOfRepresentative = m_MembersOfRepresentative,
                pointBuckets = m_PointBuckets,
                clustered = m_Clustered,
                tolerance = m_Tolerance
            }.Schedule(gatherSwept);

            var ghostMaps = new GhostMapJob
            {
                segments = m_Clustered,
                endingAt = m_EndingAt,
                startingAt = m_StartingAt
            }.Schedule(cluster);

            var ghosts = new GhostJob
            {
                segments = m_Clustered.AsDeferredJobArray(),
                endingAt = m_EndingAt,
                startingAt = m_StartingAt
            }.Schedule(m_Clustered, 32, ghostMaps);

            // The streams outlive the jobs that filled them, so their release rides on the same handle the caller waits for.
            return JobHandle.CombineDependencies(clippedStream.Dispose(ghosts), sweptStream.Dispose(ghosts));
        }

        // Diagnostic only: the same pipeline as Schedule, but each stage is completed and timed in turn rather than left
        // to run across the chain unobserved, so the cost of one compute can be attributed to a specific pass.
        private JobHandle ScheduleTimed()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            var report = new System.Text.StringBuilder("stage timings (");
            report.Append(m_DirtyGroups.Length).Append(" dirty groups, ").Append(m_GroupLive.Length).Append(" live groups):");

            Swap(ref m_Clipped, ref m_ClippedPrevious);
            Swap(ref m_ClippedStart, ref m_ClippedStartPrevious);
            Swap(ref m_ClippedCount, ref m_ClippedCountPrevious);
            Swap(ref m_LineOfClipped, ref m_LineOfClippedPrevious);
            Swap(ref m_Working, ref m_WorkingPrevious);

            var groups = new GroupView(m_Vertices, m_ContourStart, m_ContourCount, m_GroupContourStart, m_GroupContourCount);

            var clippedStream = new NativeStream(math.max(m_DirtyGroups.Length, 1), Allocator.TempJob);

            stopwatch.Restart();
            new ClipJob
            {
                groups = groups,
                dirtyGroups = m_DirtyGroups,
                neighbourStart = m_NeighbourStart,
                neighbourCount = m_NeighbourCount,
                neighbourData = m_NeighbourData,
                tolerance = m_Tolerance,
                output = clippedStream.AsWriter()
            }.Schedule(m_DirtyGroups.Length, 1).Complete();
            report.Append(" clip=").Append(stopwatch.Elapsed.TotalMilliseconds.ToString("F3"));

            stopwatch.Restart();
            new GatherClippedJob
            {
                groupLive = m_GroupLive,
                streamIndexOfGroup = m_StreamIndexOfGroup,
                previous = m_ClippedPrevious,
                previousStart = m_ClippedStartPrevious,
                previousCount = m_ClippedCountPrevious,
                lineOfClippedPrevious = m_LineOfClippedPrevious,
                clipped = m_Clipped,
                clippedStart = m_ClippedStart,
                clippedCount = m_ClippedCount,
                lineOfClipped = m_LineOfClipped,
                input = clippedStream.AsReader()
            }.Schedule().Complete();
            report.Append(" gatherClipped=").Append(stopwatch.Elapsed.TotalMilliseconds.ToString("F3"));

            clippedStream.Dispose();

            stopwatch.Restart();
            new IncrementalLineGroupJob
            {
                clipped = m_Clipped,
                clippedStart = m_ClippedStart,
                clippedCount = m_ClippedCount,
                clippedPrevious = m_ClippedPrevious,
                clippedStartPrevious = m_ClippedStartPrevious,
                clippedCountPrevious = m_ClippedCountPrevious,
                dirtyGroups = m_DirtyGroups,
                lineOfClipped = m_LineOfClipped,
                pendingRemovals = m_PendingRemovals,
                lineOfSegment = m_LineOfSegment,
                lineSeed = m_LineSeed,
                lineLive = m_LineLive,
                lineMemberCount = m_LineMemberCount,
                lineFreeIds = m_LineFreeIds,
                lineBuckets = m_LineBuckets,
                lineMembers = m_LineMembers,
                lineStart = m_LineStart,
                lineCount = m_LineCount,
                tolerance = m_Tolerance
            }.Schedule().Complete();
            report.Append(" lineGroup=").Append(stopwatch.Elapsed.TotalMilliseconds.ToString("F3"));

            var sweptStream = new NativeStream(math.max(m_LineStart.Length, 1), Allocator.TempJob);

            stopwatch.Restart();
            new SweepJob
            {
                segments = m_Clipped.AsArray(),
                lineMembers = m_LineMembers.AsArray(),
                lineStart = m_LineStart.AsArray(),
                lineCount = m_LineCount.AsArray(),
                output = sweptStream.AsWriter()
            }.Schedule(m_LineStart, 1).Complete();
            report.Append(" sweep=").Append(stopwatch.Elapsed.TotalMilliseconds.ToString("F3"));

            stopwatch.Restart();
            new GatherSweptJob
            {
                working = m_Working,
                input = sweptStream.AsReader()
            }.Schedule().Complete();
            report.Append(" gatherSwept=").Append(stopwatch.Elapsed.TotalMilliseconds.ToString("F3"));

            sweptStream.Dispose();

            stopwatch.Restart();
            new IncrementalClusterJob
            {
                working = m_Working,
                workingPrevious = m_WorkingPrevious,
                representativeOfPoint = m_RepresentativeOfPoint,
                membersOfRepresentative = m_MembersOfRepresentative,
                pointBuckets = m_PointBuckets,
                clustered = m_Clustered,
                tolerance = m_Tolerance
            }.Schedule().Complete();
            report.Append(" cluster=").Append(stopwatch.Elapsed.TotalMilliseconds.ToString("F3"));

            stopwatch.Restart();
            new GhostMapJob
            {
                segments = m_Clustered,
                endingAt = m_EndingAt,
                startingAt = m_StartingAt
            }.Schedule().Complete();
            report.Append(" ghostMap=").Append(stopwatch.Elapsed.TotalMilliseconds.ToString("F3"));

            stopwatch.Restart();
            new GhostJob
            {
                segments = m_Clustered.AsDeferredJobArray(),
                endingAt = m_EndingAt,
                startingAt = m_StartingAt
            }.Schedule(m_Clustered, 32).Complete();
            report.Append(" ghost=").Append(stopwatch.Elapsed.TotalMilliseconds.ToString("F3"));

            UnityEngine.Debug.Log(report.ToString());

            return default;
        }

        // Exchange two lists, so a pass can read the previous contents of one while filling the other.
        private static void Swap<T>(ref NativeList<T> first, ref NativeList<T> second) where T : unmanaged
        {
            var held = first;
            first = second;
            second = held;
        }

        // Gather, for every group that has to be reclipped, the live groups whose bounds meet its own, as one flat table the job indexes by group.
        private void BuildNeighbourTable()
        {
            Fill(m_DirtyFlags, m_GroupLive.Length);
            Fill(m_NeighbourStart, m_GroupLive.Length);
            Fill(m_NeighbourCount, m_GroupLive.Length);

            Fill(m_StreamIndexOfGroup, m_GroupLive.Length);

            m_NeighbourData.Clear();
            m_DirtyGroups.Clear();

            foreach (var group in m_Dirty)
                m_DirtyFlags[group] = true;

            for (var group = 0; group < m_GroupLive.Length; ++group)
            {
                // A group with no buffer of its own is one the gathering pass takes from the previous store instead.
                m_StreamIndexOfGroup[group] = -1;

                if (!m_DirtyFlags[group] || !m_GroupLive[group])
                    continue;

                var neighbours = FindNeighbours(group, Allocator.Temp);

                m_NeighbourStart[group] = m_NeighbourData.Length;
                m_NeighbourCount[group] = neighbours.Length;
                m_NeighbourData.AddRange(neighbours.AsArray());

                neighbours.Dispose();

                // Clipping runs one iteration per dirty group, so each one is given the buffer at its own place in that list.
                m_StreamIndexOfGroup[group] = m_DirtyGroups.Length;
                m_DirtyGroups.Add(group);
            }
        }

        // Reset a per-group list to the given length, so it can be indexed by group without any further bounds checking.
        private static void Fill<T>(NativeList<T> list, int length) where T : unmanaged
        {
            list.Clear();
            list.Resize(length, NativeArrayOptions.ClearMemory);
        }

        // A total order on positions, so a cluster representative never depends on insertion order.
        // The job sorts by it and the point struct that carries it lives out here, so it belongs to the class rather than the job.
        private static int ComparePosition(float2 a, float2 b)
        {
            var byX = a.x.CompareTo(b.x);
            return byX != 0 ? byX : a.y.CompareTo(b.y);
        }

        #endregion

        #region Geometry

        // A read-only view of every group's stored contours, so a pass can ask what a group covers without carrying the five containers itself.
        private readonly struct GroupView
        {
            public GroupView(NativeList<float2> vertices, NativeList<int> contourStart, NativeList<int> contourCount, NativeList<int> groupContourStart, NativeList<int> groupContourCount)
            {
                this.vertices = vertices;
                this.contourStart = contourStart;
                this.contourCount = contourCount;
                this.groupContourStart = groupContourStart;
                this.groupContourCount = groupContourCount;
            }

            // Crossing-number test over every contour in the group, so the odd-winding rule decides what is solid.
            public bool Inside(int group, float2 point)
            {
                var crossings = 0;

                var contourFrom = groupContourStart[group];
                var contourTo = contourFrom + groupContourCount[group];

                for (var c = contourFrom; c < contourTo; ++c)
                {
                    var vertexFrom = contourStart[c];
                    var vertices = contourCount[c];

                    for (var v = 0; v < vertices; ++v)
                    {
                        var a = this.vertices[vertexFrom + v];
                        var b = this.vertices[vertexFrom + (v + 1) % vertices];

                        if ((a.y > point.y) == (b.y > point.y))
                            continue;

                        var t = (point.y - a.y) / (b.y - a.y);
                        if (a.x + t * (b.x - a.x) > point.x)
                            ++crossings;
                    }
                }

                return (crossings & 1) == 1;
            }

            // Whether the span runs along the group's boundary, where an inside test would answer arbitrarily and cancellation has to decide instead.
            //
            // Running along means near and parallel, not merely near. Where two boundaries cross steeply, the stretch either side
            // of the crossing point is short, so its middle sits within the tolerance of the other boundary while the stretch is
            // squarely inside it. Judging that stretch by nearness alone leaves the buried half of it in place, and its far end
            // then has nothing to continue into, which is a hole in the surface. Only a span genuinely lying on an edge is left
            // for cancellation.
            public bool LiesAlongBoundary(int group, float2 start, float2 end, float tolerance)
            {
                var span = new Segment(start, end, group);
                var middle = (start + end) * 0.5f;

                var contourFrom = groupContourStart[group];
                var contourTo = contourFrom + groupContourCount[group];

                for (var c = contourFrom; c < contourTo; ++c)
                {
                    var vertexFrom = contourStart[c];
                    var vertices = contourCount[c];

                    for (var v = 0; v < vertices; ++v)
                    {
                        var a = this.vertices[vertexFrom + v];
                        var b = this.vertices[vertexFrom + (v + 1) % vertices];

                        if (DistanceToSegment(middle, a, b) < tolerance && SharesLine(span, new Segment(a, b, group), tolerance))
                            return true;
                    }
                }

                return false;
            }

            // Collect the parameters along (a,b) where it crosses any edge of the group.
            public void AddCrossings(int group, float2 a, float2 b, NativeList<float> results)
            {
                var ab = b - a;

                var contourFrom = groupContourStart[group];
                var contourTo = contourFrom + groupContourCount[group];

                for (var contour = contourFrom; contour < contourTo; ++contour)
                {
                    var vertexFrom = contourStart[contour];
                    var vertices = contourCount[contour];

                    for (var v = 0; v < vertices; ++v)
                    {
                        var c = this.vertices[vertexFrom + v];
                        var d = this.vertices[vertexFrom + (v + 1) % vertices];
                        var cd = d - c;

                        var denominator = ab.x * cd.y - ab.y * cd.x;
                        if (math.abs(denominator) < 1e-9f)
                            continue;

                        var ac = c - a;
                        var t = (ac.x * cd.y - ac.y * cd.x) / denominator;
                        var u = (ac.x * ab.y - ac.y * ab.x) / denominator;

                        if (t > Epsilon && t < 1f - Epsilon && u > -Epsilon && u < 1f + Epsilon)
                            results.Add(t);
                    }
                }
            }

            public readonly NativeList<float2> vertices;
            public readonly NativeList<int> contourStart;
            public readonly NativeList<int> contourCount;
            public readonly NativeList<int> groupContourStart;
            public readonly NativeList<int> groupContourCount;
        }

        // Whether two segments lie on the same line, which takes both a distance and a direction test.
        //
        // The distance part is the perpendicular distance of each end to the other's line. On its own that is enough for long
        // segments, where a small difference in direction has grown into a large distance by the far end.
        //
        // It is not enough for short ones. Two neighbouring chords of a finely tessellated curve sit within a whisker of each
        // other's line however sharply the curve turns, so a distance test alone calls a curve straight. The sweep then
        // replaces a run of that curve with spans along one line, whose ends no longer meet the rest of the arc, and the
        // surface is left with holes in it. So the directions have to agree as well, in either orientation, since a line has
        // no preferred way round.
        private static bool SharesLine(Segment a, Segment b, float tolerance)
        {
            var directionA = math.normalizesafe(a.end - a.start);
            var directionB = math.normalizesafe(b.end - b.start);

            // The cross product of two unit vectors is the sine of the angle between them, so this is an angle limit
            // expressed without a trigonometric call.
            if (math.abs(directionA.x * directionB.y - directionA.y * directionB.x) > ParallelLimit)
                return false;

            return PerpendicularDistance(a.start, b.start, b.end) < tolerance &&
                   PerpendicularDistance(a.end, b.start, b.end) < tolerance &&
                   PerpendicularDistance(b.start, a.start, a.end) < tolerance &&
                   PerpendicularDistance(b.end, a.start, a.end) < tolerance;
        }

        // Distance from the point to the infinite line through a and b, not to the segment between them.
        private static float PerpendicularDistance(float2 point, float2 a, float2 b)
        {
            var ab = b - a;
            var length = math.length(ab);
            if (length < 1e-9f)
                return math.length(point - a);

            return math.abs((point.x - a.x) * ab.y - (point.y - a.y) * ab.x) / length;
        }

        private static float DistanceToSegment(float2 point, float2 a, float2 b)
        {
            var ab = b - a;
            var lengthSquared = math.lengthsq(ab);
            if (lengthSquared < 1e-12f)
                return math.length(point - a);

            var t = math.saturate(math.dot(point - a, ab) / lengthSquared);
            return math.length(point - (a + t * ab));
        }

        // Which bucket a segment's line falls in, as an angle bucket and a distance-from-origin bucket.
        // Opposite directions describe the same line, so the angle is folded into a half turn, which puts a half turn
        // back at zero rather than at the far end of the range. Folding also flips the normal, so the distance is taken
        // unsigned to keep both directions together.
        private static void LineBucket(Segment segment, out int angleBucket, out int offsetBucket)
        {
            var direction = math.normalizesafe(segment.end - segment.start);

            var angle = math.PI * math.frac(math.atan2(direction.y, direction.x) / math.PI);
            var normal = new float2(-direction.y, direction.x);

            angleBucket = (int)math.floor(angle / LineAngleBucket);
            offsetBucket = (int)math.floor(math.abs(math.dot(segment.start, normal)) / LineOffsetBucket);
        }

        // A coarse key for the line a segment sits on. Two collinear segments land in the same bucket or one either side of it.
        private static int LineKey(Segment segment)
        {
            LineBucket(segment, out var angleBucket, out var offsetBucket);
            return LineKey(angleBucket, offsetBucket);
        }

        // The angle wraps, because a bucket just under a half turn and a bucket just over zero describe the same line.
        private static int LineKey(int angleBucket, int offsetBucket)
        {
            var wrapped = ((angleBucket % LineAngleBuckets) + LineAngleBuckets) % LineAngleBuckets;
            return Mix((uint)(wrapped * 73856093 ^ offsetBucket * 19349663));
        }

        // Spread a hash's high bits down into its low ones.
        //
        // Every native hash container chooses a key's bucket from the low bits of its hash alone, so a hash whose variation sits higher up puts every key in one bucket and turns each lookup into a walk of the whole set.
        // A position is the worst case: a coordinate that is a whole or half number is a float with a long run of zero mantissa bits, and adding and multiplying by odd constants preserves that run, so a grid of positions ends up sharing its low bits exactly.
        private static int Mix(uint hash)
        {
            hash ^= hash >> 16;
            hash *= 0x85EBCA6Bu;
            hash ^= hash >> 13;
            hash *= 0xC2B2AE35u;
            hash ^= hash >> 16;

            return (int)hash;
        }

        // Gather every segment in this segment's own bucket and the ones adjacent to it, since a collinear pair can fall either side of a bucket edge.
        private static void CollectLineCandidates(NativeParallelMultiHashMap<int, int> buckets, Segment segment, NativeList<int> results)
        {
            LineBucket(segment, out var angleBucket, out var offsetBucket);

            for (var a = -1; a <= 1; ++a)
            {
                for (var o = -1; o <= 1; ++o)
                {
                    if (!buckets.TryGetFirstValue(LineKey(angleBucket + a, offsetBucket + o), out var index, out var iterator))
                        continue;

                    do
                    {
                        results.Add(index);
                    }
                    while (buckets.TryGetNextValue(out index, ref iterator));
                }
            }
        }

        // Walk one line, writing the runs covered by exactly one facing.
        private static void SweepLine(NativeArray<Segment> segments, NativeArray<int> members, int from, int count, ref NativeStream.Writer output)
        {
            // The first segment's direction defines the line, so every other segment is either along it or against it.
            var origin = segments[members[from]].start;
            var direction = math.normalizesafe(segments[members[from]].end - origin);

            var events = new NativeList<LineEvent>(count * 2, Allocator.Temp);
            for (var i = 0; i < count; ++i)
            {
                var segment = segments[members[from + i]];
                var t0 = math.dot(segment.start - origin, direction);
                var t1 = math.dot(segment.end - origin, direction);
                var forward = t1 > t0;

                events.Add(new LineEvent(math.min(t0, t1), forward, true, segment.owner));
                events.Add(new LineEvent(math.max(t0, t1), forward, false, segment.owner));
            }

            events.Sort(default(LineEventComparer));

            var forwardCount = 0;
            var backwardCount = 0;

            // Whichever segment most recently opened a facing is the one a run of that facing belongs to.
            var forwardOwner = 0;
            var backwardOwner = 0;

            // Runs are gathered before anything is written, so touching runs of the same facing can be joined into one.
            // Writing each run as it is found would break the surface: a run can be arbitrarily short, and a very short one
            // is neither worth a segment of its own nor safe to discard, since discarding it leaves a hole the width of that
            // run in a line that should be continuous. Joining is the only answer that keeps the surface whole.
            var runs = new NativeList<LineRun>(events.Length, Allocator.Temp);

            for (var i = 0; i < events.Length; ++i)
            {
                var current = events[i];
                if (current.forward)
                {
                    forwardCount += current.opening ? 1 : -1;

                    if (current.opening)
                        forwardOwner = current.owner;
                }
                else
                {
                    backwardCount += current.opening ? 1 : -1;

                    if (current.opening)
                        backwardOwner = current.owner;
                }

                if (i + 1 >= events.Length)
                    break;

                var start = current.position;
                var finish = events[i + 1].position;
                if (finish <= start)
                    continue;

                // Both facings present means the run is inside the combined solid, so nothing survives there.
                if (forwardCount > 0 && backwardCount > 0)
                    continue;

                if (forwardCount <= 0 && backwardCount <= 0)
                    continue;

                var forward = forwardCount > 0;
                var owner = forward ? forwardOwner : backwardOwner;

                // Extend the previous run rather than starting a new one when this carries straight on from it. Runs belonging to
                // different inputs are never joined, because one segment cannot report two origins and a caller that gives each
                // input its own surface material or user data would lose it.
                if (runs.Length > 0)
                {
                    var previous = runs[runs.Length - 1];
                    if (previous.forward == forward && previous.owner == owner && finish > previous.to && start - previous.to < Epsilon)
                    {
                        previous.to = finish;
                        runs[runs.Length - 1] = previous;
                        continue;
                    }
                }

                runs.Add(new LineRun(start, finish, forward, owner));
            }

            foreach (var run in runs)
            {
                var start = origin + direction * run.from;
                var end = origin + direction * run.to;

                output.Write(run.forward ? new Segment(start, end, run.owner) : new Segment(end, start, run.owner));
            }

            runs.Dispose();
            events.Dispose();
        }

        // Find the segment sharing the vertex whose direction is the smallest angular step away from ours, and return its far end.
        // The map holds only the segments that meet the vertex, keyed on it, so an arriving one contributes its start and a leaving one its end.
        private static float2 FindNeighbour(NativeArray<Segment> segments, NativeParallelMultiHashMap<PointKey, int> meeting, int self, float2 vertex, float2 far, bool arriving)
        {
            var ours = math.atan2(far.y - vertex.y, far.x - vertex.x);

            var bestAngle = float.MaxValue;
            var best = far;

            if (!meeting.TryGetFirstValue(vertex, out var index, out var iterator))
                return best;

            do
            {
                if (index == self)
                    continue;

                var candidate = segments[index];
                var candidateFar = arriving ? candidate.start : candidate.end;

                var theirs = math.atan2(candidateFar.y - vertex.y, candidateFar.x - vertex.x);

                var delta = arriving ? ours - theirs : theirs - ours;
                while (delta <= AngleEpsilon)
                    delta += 2f * math.PI;
                while (delta > 2f * math.PI)
                    delta -= 2f * math.PI;

                if (delta < bestAngle)
                {
                    bestAngle = delta;
                    best = candidateFar;
                }
            }
            while (meeting.TryGetNextValue(out index, ref iterator));

            return best;
        }

        #endregion

        #region Jobs

        // Cut one dirty group's edges wherever they cross a neighbour, keeping the parts that are not buried, and write what
        // survives into that group's own buffer. Groups cannot see each other's output, so they run side by side.
        //
        // A span lying on another group's boundary is neither inside nor outside it, so it is left for cancellation to resolve.
        // Only the groups gathered as this one's neighbours are considered: anything further away cannot cross or bury these edges.
        [BurstCompile]
        private struct ClipJob : IJobParallelFor
        {
            public void Execute(int index)
            {
                var group = dirtyGroups[index];

                var crossings = new NativeList<float>(16, Allocator.Temp);

                var neighbourFrom = neighbourStart[group];
                var neighbourTo = neighbourFrom + neighbourCount[group];

                var contourFrom = groups.groupContourStart[group];
                var contourTo = contourFrom + groups.groupContourCount[group];

                output.BeginForEachIndex(index);

                for (var c = contourFrom; c < contourTo; ++c)
                {
                    var vertexFrom = groups.contourStart[c];
                    var vertexCount = groups.contourCount[c];

                    for (var v = 0; v < vertexCount; ++v)
                    {
                        var a = groups.vertices[vertexFrom + v];
                        var b = groups.vertices[vertexFrom + (v + 1) % vertexCount];

                        crossings.Clear();
                        crossings.Add(0f);
                        crossings.Add(1f);

                        for (var n = neighbourFrom; n < neighbourTo; ++n)
                        {
                            var other = neighbourData[n];
                            if (other != group)
                                groups.AddCrossings(other, a, b, crossings);
                        }

                        crossings.Sort();

                        for (var k = 0; k + 1 < crossings.Length; ++k)
                        {
                            var t0 = crossings[k];
                            var t1 = crossings[k + 1];
                            if (t1 - t0 < Epsilon)
                                continue;

                            var start = math.lerp(a, b, t0);
                            var end = math.lerp(a, b, t1);
                            var middle = (start + end) * 0.5f;

                            var buried = false;
                            for (var n = neighbourFrom; n < neighbourTo; ++n)
                            {
                                var other = neighbourData[n];
                                if (other == group || groups.LiesAlongBoundary(other, start, end, tolerance) || !groups.Inside(other, middle))
                                    continue;

                                buried = true;
                                break;
                            }

                            if (!buried)
                                output.Write(new Segment(start, end, group));
                        }
                    }
                }

                output.EndForEachIndex();

                crossings.Dispose();
            }

            [ReadOnly] public GroupView groups;
            [ReadOnly] public NativeList<int> dirtyGroups;
            [ReadOnly] public NativeList<int> neighbourStart;
            [ReadOnly] public NativeList<int> neighbourCount;
            [ReadOnly] public NativeList<int> neighbourData;

            public float tolerance;
            public NativeStream.Writer output;
        }

        // Rebuild the flat clipped-segment store, taking each reclipped group from its buffer and every other group from the
        // previous store untouched. Walking the groups in order is what makes the result the same however the clipping was spread.
        [BurstCompile]
        private struct GatherClippedJob : IJob
        {
            public void Execute()
            {
                clipped.Clear();
                lineOfClipped.Clear();

                Fill(clippedStart, groupLive.Length);
                Fill(clippedCount, groupLive.Length);

                for (var group = 0; group < groupLive.Length; ++group)
                {
                    var start = clipped.Length;

                    if (groupLive[group])
                    {
                        var buffer = streamIndexOfGroup[group];

                        if (buffer >= 0)
                        {
                            // A reclipped group's segments are new, so they have no line yet; the grouping pass fills these in.
                            var remaining = input.BeginForEachIndex(buffer);
                            for (var i = 0; i < remaining; ++i)
                            {
                                clipped.Add(input.Read<Segment>());
                                lineOfClipped.Add(-1);
                            }

                            input.EndForEachIndex();
                        }
                        else
                        {
                            // An untouched group's segments keep the line they already had, which saves looking each one up by value.
                            var retained = previousCount[group];
                            for (var i = 0; i < retained; ++i)
                            {
                                clipped.Add(previous[previousStart[group] + i]);
                                lineOfClipped.Add(lineOfClippedPrevious[previousStart[group] + i]);
                            }
                        }
                    }

                    clippedStart[group] = start;
                    clippedCount[group] = clipped.Length - start;
                }
            }

            [ReadOnly] public NativeList<bool> groupLive;
            [ReadOnly] public NativeList<int> streamIndexOfGroup;
            [ReadOnly] public NativeList<Segment> previous;
            [ReadOnly] public NativeList<int> previousStart;
            [ReadOnly] public NativeList<int> previousCount;
            [ReadOnly] public NativeList<int> lineOfClippedPrevious;

            public NativeList<Segment> clipped;
            public NativeList<int> clippedStart;
            public NativeList<int> clippedCount;
            public NativeList<int> lineOfClipped;

            public NativeStream.Reader input;
        }

        // Update line membership for exactly what changed this compute, then flatten the current membership into the
        // per-line index lists the sweep reads.
        //
        // A segment carried over unchanged from an untouched group keeps the line it already had, found by a lookup
        // rather than by retesting. Only a group being reclipped, or a group removed since the last compute, has its
        // old segments taken out of the registry and its fresh segments tested for a line to join. Joining tests a
        // fixed seed chosen when a line was created rather than every current member, so a line's membership grows
        // without the seed itself ever moving. Removing and adding both claim entries in order, so this stays on one thread.
        [BurstCompile]
        private struct IncrementalLineGroupJob : IJob
        {
            public void Execute()
            {
                RemoveStaleMembers();
                AddFreshMembers();
                Flatten();
            }

            // Take every segment a reclipped or removed group last contributed out of the registry, since its old
            // line membership no longer applies. A group's fresh segments, if it has any, are added back separately.
            private void RemoveStaleMembers()
            {
                foreach (var group in dirtyGroups)
                    RemoveGroupsPreviousSegments(group);

                foreach (var group in pendingRemovals)
                    RemoveGroupsPreviousSegments(group);

                pendingRemovals.Clear();
            }

            private void RemoveGroupsPreviousSegments(int group)
            {
                var from = clippedStartPrevious[group];
                var count = clippedCountPrevious[group];

                for (var i = 0; i < count; ++i)
                {
                    var segment = clippedPrevious[from + i];
                    if (!lineOfSegment.TryGetValue(segment, out var line))
                        continue;

                    lineOfSegment.Remove(segment);
                    lineMemberCount[line] -= 1;

                    // A line with nothing left in it is retired, so its id can be handed out again. Its bucket entry
                    // is left in place: a stale entry only ever costs a wasted SharesLine test against whatever seed
                    // the id is next given, since a lookup never treats a dead id as a match without retesting.
                    if (lineMemberCount[line] == 0)
                    {
                        lineLive[line] = false;
                        lineFreeIds.Add(line);
                    }
                }
            }

            // Give every segment a reclipped group produced this compute a line: one an existing seed matches, or a new one.
            // One candidate list serves every segment, since a per-segment one would allocate once for each of them.
            private void AddFreshMembers()
            {
                var candidates = new NativeList<int>(16, Allocator.Temp);

                foreach (var group in dirtyGroups)
                {
                    var from = clippedStart[group];
                    var count = clippedCount[group];

                    for (var i = 0; i < count; ++i)
                        AddSegment(from + i, candidates);
                }

                candidates.Dispose();
            }

            private void AddSegment(int index, NativeList<int> candidates)
            {
                var segment = clipped[index];

                // Cancellation can leave two groups producing the same span, and PlaceInSlots already treats that as
                // one segment with the first writer winning; a second claim here follows the same rule, taking the
                // line the first claimer settled on so both copies still flatten onto it.
                if (lineOfSegment.TryGetValue(segment, out var claimed))
                {
                    lineOfClipped[index] = claimed;
                    return;
                }

                candidates.Clear();
                CollectLineCandidates(lineBuckets, segment, candidates);

                var joined = -1;
                foreach (var candidate in candidates)
                {
                    if (lineLive[candidate] && SharesLine(segment, lineSeed[candidate], tolerance))
                    {
                        joined = candidate;
                        break;
                    }
                }

                if (joined < 0)
                    joined = CreateLine(segment);

                lineOfSegment.TryAdd(segment, joined);
                lineMemberCount[joined] += 1;
                lineOfClipped[index] = joined;
            }

            private int CreateLine(Segment seed)
            {
                int id;

                if (lineFreeIds.Length > 0)
                {
                    id = lineFreeIds[lineFreeIds.Length - 1];
                    lineFreeIds.RemoveAt(lineFreeIds.Length - 1);
                }
                else
                {
                    lineSeed.Add(default);
                    lineLive.Add(false);
                    lineMemberCount.Add(0);
                    id = lineSeed.Length - 1;
                }

                lineSeed[id] = seed;
                lineLive[id] = true;
                lineMemberCount[id] = 0;

                lineBuckets.Add(LineKey(seed), id);
                return id;
            }

            // Write out the flat index lists the sweep reads, one contiguous run of segment indices per live line.
            //
            // This is a counting sort over the line each segment already belongs to: count the members of every line, turn those
            // counts into the start of each line's run, then place each segment index into its line's run. Line ids are visited in
            // ascending order and the placing pass walks the segments backwards, so a line's members always come out in descending
            // index order and the result never depends on how the work was spread.
            private void Flatten()
            {
                var lines = lineLive.Length;

                var counts = new NativeArray<int>(lines, Allocator.Temp);
                for (var i = 0; i < lineOfClipped.Length; ++i)
                    counts[lineOfClipped[i]] += 1;

                lineStart.Clear();
                lineCount.Clear();

                // Where each line's run begins, advanced as the run fills so the next index lands after the last.
                var cursor = new NativeArray<int>(lines, Allocator.Temp);
                var total = 0;

                for (var line = 0; line < lines; ++line)
                {
                    if (!lineLive[line])
                        continue;

                    cursor[line] = total;

                    lineStart.Add(total);
                    lineCount.Add(counts[line]);

                    total += counts[line];
                }

                lineMembers.Clear();
                lineMembers.Resize(total, NativeArrayOptions.UninitializedMemory);

                for (var i = lineOfClipped.Length - 1; i >= 0; --i)
                {
                    var line = lineOfClipped[i];

                    // A retired line contributes no run, so anything still pointing at one is left out rather than placed nowhere.
                    if (!lineLive[line])
                        continue;

                    lineMembers[cursor[line]] = i;
                    cursor[line] += 1;
                }

                cursor.Dispose();
                counts.Dispose();
            }

            [ReadOnly] public NativeList<Segment> clipped;
            [ReadOnly] public NativeList<int> clippedStart;
            [ReadOnly] public NativeList<int> clippedCount;

            [ReadOnly] public NativeList<Segment> clippedPrevious;
            [ReadOnly] public NativeList<int> clippedStartPrevious;
            [ReadOnly] public NativeList<int> clippedCountPrevious;

            [ReadOnly] public NativeList<int> dirtyGroups;

            public NativeList<int> lineOfClipped;
            public NativeList<int> pendingRemovals;

            public NativeHashMap<Segment, int> lineOfSegment;
            public NativeList<Segment> lineSeed;
            public NativeList<bool> lineLive;
            public NativeList<int> lineMemberCount;
            public NativeList<int> lineFreeIds;
            public NativeParallelMultiHashMap<int, int> lineBuckets;

            public NativeList<int> lineMembers;
            public NativeList<int> lineStart;
            public NativeList<int> lineCount;

            public float tolerance;
        }

        // Resolve one line: a stretch covered by one facing is a real surface, a stretch covered by both is interior and survives nowhere.
        // Every line resolves from its own members alone, so the lines run side by side.
        [BurstCompile]
        private struct SweepJob : IJobParallelForDefer
        {
            public void Execute(int line)
            {
                var from = lineStart[line];
                var count = lineCount[line];

                output.BeginForEachIndex(line);

                // A line holding a single segment has nothing to resolve against.
                if (count == 1)
                    output.Write(segments[lineMembers[from]]);
                else
                    SweepLine(segments, lineMembers, from, count, ref output);

                output.EndForEachIndex();
            }

            [ReadOnly] public NativeArray<Segment> segments;
            [ReadOnly] public NativeArray<int> lineMembers;
            [ReadOnly] public NativeArray<int> lineStart;
            [ReadOnly] public NativeArray<int> lineCount;

            public NativeStream.Writer output;
        }

        // Collect what the lines resolved to, in line order, so the surviving set is the same however the sweep was spread.
        [BurstCompile]
        private struct GatherSweptJob : IJob
        {
            public void Execute()
            {
                working.Clear();

                for (var line = 0; line < input.ForEachCount; ++line)
                {
                    var remaining = input.BeginForEachIndex(line);
                    for (var i = 0; i < remaining; ++i)
                        working.Add(input.Read<Segment>());

                    input.EndForEachIndex();
                }
            }

            public NativeList<Segment> working;
            public NativeStream.Reader input;
        }

        // Update which representative every raw point snaps to for exactly what changed this compute, then write out
        // the clustered segments the rest of the pipeline reads.
        //
        // A point identical to the one at the same place last compute keeps its representative by lookup. Only a
        // point that appeared or disappeared this compute is processed: disappearing takes it out of its cluster and,
        // if it was the representative, chooses a new one from whatever is left; appearing joins the nearest existing
        // cluster within tolerance, or becomes a new cluster's first point, and takes over as representative if it
        // sorts lower than the one already there. Both only ever touch the one cluster involved, so this stays cheap
        // regardless of how many points exist elsewhere, but claiming still depends on visit order, so it stays on one thread.
        [BurstCompile]
        private struct IncrementalClusterJob : IJob
        {
            public void Execute()
            {
                var current = new NativeHashSet<PointKey>(math.max(working.Length * 2, 1), Allocator.Temp);
                var previous = new NativeHashSet<PointKey>(math.max(workingPrevious.Length * 2, 1), Allocator.Temp);

                for (var i = 0; i < working.Length; ++i)
                {
                    current.Add(working[i].start);
                    current.Add(working[i].end);
                }

                for (var i = 0; i < workingPrevious.Length; ++i)
                {
                    previous.Add(workingPrevious[i].start);
                    previous.Add(workingPrevious[i].end);
                }

                // Removing first frees a departing point's place before anything new can claim it.
                foreach (var key in previous)
                {
                    if (!current.Contains(key))
                        RemovePoint(key.position);
                }

                foreach (var key in current)
                {
                    if (!previous.Contains(key))
                        AddPoint(key.position);
                }

                previous.Dispose();
                current.Dispose();

                BuildClustered();
                WeldShortSegments();
            }

            private void RemovePoint(float2 point)
            {
                if (!representativeOfPoint.TryGetValue(point, out var representative))
                    return;

                representativeOfPoint.Remove(point);
                RemoveMember(representative, point);

                if (representative.Equals(point))
                    Recanonicalize(representative);
            }

            private void AddPoint(float2 point)
            {
                var nearest = FindNearestRegisteredPoint(point, out var found);
                if (!found)
                {
                    representativeOfPoint.TryAdd(point, point);
                    membersOfRepresentative.Add(point, point);
                    pointBuckets.Add(PointCellKey(PointCell(point)), point);
                    return;
                }

                var representative = representativeOfPoint[nearest];

                if (ComparePosition(point, representative) < 0)
                    Repoint(representative, point);
                else
                    Join(representative, point);
            }

            // A cluster that just lost its representative needs a new one chosen from whatever is left, the same way
            // a from-scratch build would: the lowest of the current members, or nothing if none remain.
            private void Recanonicalize(float2 oldRepresentative)
            {
                var remaining = Detach(oldRepresentative);
                if (remaining.Length == 0)
                {
                    remaining.Dispose();
                    return;
                }

                var newRepresentative = remaining[0];
                for (var i = 1; i < remaining.Length; ++i)
                {
                    if (ComparePosition(remaining[i], newRepresentative) < 0)
                        newRepresentative = remaining[i];
                }

                Reattach(remaining, newRepresentative);
                remaining.Dispose();
            }

            // A new point sorts lower than its cluster's current representative, so it takes over: every existing
            // member is re-pointed to it, the same way a from-scratch build would have chosen it from the start.
            private void Repoint(float2 oldRepresentative, float2 newRepresentative)
            {
                var members = Detach(oldRepresentative);
                Reattach(members, newRepresentative);
                members.Dispose();

                representativeOfPoint.TryAdd(newRepresentative, newRepresentative);
                membersOfRepresentative.Add(newRepresentative, newRepresentative);
                pointBuckets.Add(PointCellKey(PointCell(newRepresentative)), newRepresentative);
            }

            private void Join(float2 representative, float2 point)
            {
                representativeOfPoint.TryAdd(point, representative);
                membersOfRepresentative.Add(representative, point);
                pointBuckets.Add(PointCellKey(PointCell(point)), point);
            }

            // Take every current member of a representative out of the registry and hand them back, so the caller can
            // either retire them or re-key them onto a different representative.
            private NativeList<float2> Detach(float2 representative)
            {
                var members = new NativeList<float2>(8, Allocator.Temp);

                if (membersOfRepresentative.TryGetFirstValue(representative, out var member, out var iterator))
                {
                    do
                    {
                        members.Add(member);
                    }
                    while (membersOfRepresentative.TryGetNextValue(out member, ref iterator));
                }

                membersOfRepresentative.Remove(representative);
                return members;
            }

            private void Reattach(NativeList<float2> members, float2 representative)
            {
                foreach (var member in members)
                {
                    representativeOfPoint.Remove(member);
                    representativeOfPoint.TryAdd(member, representative);
                    membersOfRepresentative.Add(representative, member);
                }
            }

            private void RemoveMember(float2 representative, float2 point)
            {
                var members = Detach(representative);

                foreach (var member in members)
                {
                    if (!member.Equals(point))
                        membersOfRepresentative.Add(representative, member);
                }

                members.Dispose();
            }

            // The closest currently-registered point within tolerance, searched through the neighbouring buckets since
            // a match can straddle a bucket edge. A bucket can hold a point no longer registered; that entry is simply
            // skipped rather than removed on the spot, since the next lookup at that spot re-validates it anyway.
            private float2 FindNearestRegisteredPoint(float2 point, out bool found)
            {
                found = false;
                var best = point;
                var bestDistanceSq = float.MaxValue;

                var cell = PointCell(point);
                for (var dx = -1; dx <= 1; ++dx)
                {
                    for (var dy = -1; dy <= 1; ++dy)
                    {
                        var key = PointCellKey(cell + new int2(dx, dy));
                        if (!pointBuckets.TryGetFirstValue(key, out var candidate, out var iterator))
                            continue;

                        do
                        {
                            if (!representativeOfPoint.ContainsKey(candidate))
                                continue;

                            var distanceSq = math.lengthsq(candidate - point);
                            if (distanceSq > tolerance * tolerance || distanceSq >= bestDistanceSq)
                                continue;

                            bestDistanceSq = distanceSq;
                            best = candidate;
                            found = true;
                        }
                        while (pointBuckets.TryGetNextValue(out candidate, ref iterator));
                    }
                }

                return best;
            }

            private int2 PointCell(float2 point) => (int2)math.floor(point / tolerance);

            private static int PointCellKey(int2 cell) => Mix((uint)(cell.x * 73856093 ^ cell.y * 19349663));

            // Write every current segment's clustered endpoints, dropping whatever collapsed to a single point.
            private void BuildClustered()
            {
                clustered.Clear();

                for (var i = 0; i < working.Length; ++i)
                {
                    var segment = working[i];
                    segment.start = representativeOfPoint[segment.start];
                    segment.end = representativeOfPoint[segment.end];

                    if (math.lengthsq(segment.end - segment.start) >= Epsilon * Epsilon)
                        clustered.Add(segment);
                }
            }

            // Pull the two ends of any segment no longer than the tolerance onto one point, then drop whatever collapsed.
            //
            // Clustering takes each point in turn and claims whatever is still unclaimed near it, so three points lying
            // inside one tolerance of each other can end up split two and one. That leaves a stub no longer than the
            // tolerance between the two clusters, and the strand running through it stops at the far end of the stub.
            // Welding closes that by treating the stub as the single point it was meant to be, which reconnects the
            // strand either side.
            private void WeldShortSegments()
            {
                var welds = new NativeHashMap<PointKey, float2>(16, Allocator.Temp);

                // A stub's two ends are each a cluster representative, and clustering never compared them to each other, so they
                // can sit a shade further apart than the tolerance itself. The limit therefore carries a little slack.
                var limit = tolerance + Epsilon;

                for (var i = 0; i < clustered.Length; ++i)
                {
                    var segment = clustered[i];
                    if (math.lengthsq(segment.end - segment.start) > limit * limit)
                        continue;

                    // The lower end in sort order is the one kept, so the outcome does not depend on the order segments arrived in.
                    // It also means a weld always leads downwards through that order, so following a chain of them always terminates.
                    var keepStart = ComparePosition(segment.start, segment.end) <= 0;
                    var keep = keepStart ? segment.start : segment.end;
                    var drop = keepStart ? segment.end : segment.start;

                    if (math.any(keep != drop))
                        welds.TryAdd(drop, keep);
                }

                if (!welds.IsEmpty)
                {
                    for (var i = 0; i < clustered.Length; ++i)
                    {
                        var segment = clustered[i];
                        segment.start = Weld(welds, segment.start);
                        segment.end = Weld(welds, segment.end);
                        clustered[i] = segment;
                    }
                }

                welds.Dispose();

                // A segment whose ends met is no longer a surface.
                for (var i = clustered.Length - 1; i >= 0; --i)
                {
                    if (math.lengthsq(clustered[i].end - clustered[i].start) < Epsilon * Epsilon)
                        clustered.RemoveAtSwapBack(i);
                }
            }

            // Follow a point through the welds, since a point welded onto another can itself have been welded on further.
            private static float2 Weld(NativeHashMap<PointKey, float2> welds, float2 point)
            {
                while (welds.TryGetValue(point, out var target))
                    point = target;

                return point;
            }

            [ReadOnly] public NativeList<Segment> working;
            [ReadOnly] public NativeList<Segment> workingPrevious;

            public NativeHashMap<PointKey, float2> representativeOfPoint;
            public NativeParallelMultiHashMap<PointKey, float2> membersOfRepresentative;
            public NativeParallelMultiHashMap<int, float2> pointBuckets;

            public NativeList<Segment> clustered;
            public float tolerance;
        }

        // Index the segments by the vertex at each of their ends, which turns the search for what meets a vertex into a walk of one
        // bucket. Clustering has already pulled coincident vertices onto one position, so an exact key is enough.
        [BurstCompile]
        private struct GhostMapJob : IJob
        {
            public void Execute()
            {
                endingAt.Clear();
                startingAt.Clear();

                if (endingAt.Capacity < segments.Length)
                {
                    endingAt.Capacity = segments.Length;
                    startingAt.Capacity = segments.Length;
                }

                for (var i = 0; i < segments.Length; ++i)
                {
                    endingAt.Add(segments[i].end, i);
                    startingAt.Add(segments[i].start, i);
                }
            }

            [ReadOnly] public NativeList<Segment> segments;

            public NativeParallelMultiHashMap<PointKey, int> endingAt;
            public NativeParallelMultiHashMap<PointKey, int> startingAt;
        }

        // Give every segment its two ghost vertices from the segments sharing each of its endpoints.
        // A segment leaving a vertex takes its first ghost from the nearest arriving segment clockwise; an arriving one takes its
        // second from the nearest leaving segment counter-clockwise. Each segment writes only its own two ghosts, so they run side by side.
        [BurstCompile]
        private struct GhostJob : IJobParallelForDefer
        {
            public void Execute(int index)
            {
                var segment = segments[index];

                segment.ghost1 = FindNeighbour(segments, endingAt, index, segment.start, segment.end, true);
                segment.ghost2 = FindNeighbour(segments, startingAt, index, segment.end, segment.start, false);

                segments[index] = segment;
            }

            // Every iteration writes its own element and reads the endpoints of others, which the safety system cannot tell apart
            // from two threads writing one element. Ghosts are read by nobody in this pass, so the reads only ever see the
            // positions clustering settled.
            [NativeDisableParallelForRestriction] public NativeArray<Segment> segments;

            [ReadOnly] public NativeParallelMultiHashMap<PointKey, int> endingAt;
            [ReadOnly] public NativeParallelMultiHashMap<PointKey, int> startingAt;
        }

        #endregion

        #region Types

        // A position standing as the key of a hash container, hashed so that the bucket it lands in genuinely depends on where the position is.
        //
        // A position cannot serve as a key directly, because float2's own hash sends a grid of positions to a single bucket and every lookup then walks the whole set.
        // Converting is implicit, so a container keyed on one of these still takes a plain position at every call.
        private readonly struct PointKey : IEquatable<PointKey>
        {
            public PointKey(float2 position)
            {
                this.position = position;
            }

            public bool Equals(PointKey other) => position.Equals(other.position);

            public override bool Equals(object other) => other is PointKey key && Equals(key);

            public override int GetHashCode() => Mix(math.hash(position));

            public static implicit operator PointKey(float2 position) => new PointKey(position);

            public readonly float2 position;
        }

        // One piece of surface with its two ghost vertices and the group it came from, which is what a caller needs to know both
        // where the surface is and which of its own inputs produced it.
        private struct Segment : IEquatable<Segment>
        {
            public Segment(float2 start, float2 end, int owner)
            {
                this.start = start;
                this.end = end;
                this.owner = owner;
                ghost1 = start;
                ghost2 = end;
            }

            // The owner counts, because a surface that has not moved but now belongs to a different input is still a change a
            // caller has to hear about: whatever it hangs off that input, a material or a piece of user data, has to follow.
            public bool Equals(Segment other)
            {
                return start.Equals(other.start) && end.Equals(other.end) && ghost1.Equals(other.ghost1) && ghost2.Equals(other.ghost2) && owner == other.owner;
            }

            // Hashed on all four points and the owner, so an unchanged segment can be matched to the slot already holding it by lookup rather than by scanning.
            // Mixed at the end, because the four positions only ever reach the high bits and a bucket is chosen from the low ones.
            public override int GetHashCode()
            {
                var hash = math.hash(start);
                hash = hash * 397u ^ math.hash(end);
                hash = hash * 397u ^ math.hash(ghost1);
                hash = hash * 397u ^ math.hash(ghost2);

                return Mix(hash * 397u ^ (uint)owner);
            }

            public override bool Equals(object other) => other is Segment segment && Equals(segment);

            public float2 start;
            public float2 end;
            public float2 ghost1;
            public float2 ghost2;

            // Index of the contour group whose edge this piece of surface came from.
            public int owner;
        }

        // One end of a segment projected onto the line it shares with others, used by the sweep.
        private readonly struct LineEvent
        {
            public LineEvent(float position, bool forward, bool opening, int owner)
            {
                this.position = position;
                this.forward = forward;
                this.opening = opening;
                this.owner = owner;
            }

            public readonly float position;
            public readonly bool forward;
            public readonly bool opening;
            public readonly int owner;
        }

        // A stretch of one line that survived the sweep, held as distances along that line so touching stretches can be joined.
        private struct LineRun
        {
            public LineRun(float from, float to, bool forward, int owner)
            {
                this.from = from;
                this.to = to;
                this.forward = forward;
                this.owner = owner;
            }

            public float from;
            public float to;
            public bool forward;
            public int owner;
        }

        // Sorted by position, then openings before closings so a run that carries straight on is never split, then by owner so
        // the order is total and the run's inherited owner is the same every time.
        private struct LineEventComparer : IComparer<LineEvent>
        {
            public int Compare(LineEvent x, LineEvent y)
            {
                var byPosition = x.position.CompareTo(y.position);
                if (byPosition != 0)
                    return byPosition;

                if (x.opening != y.opening)
                    return x.opening ? -1 : 1;

                return x.owner.CompareTo(y.owner);
            }
        }

        #endregion

        #region Internal

        private const float Epsilon = 1e-5f;
        private const float AngleEpsilon = 1e-5f;

        // Below this many stored vertices the waste is not worth a compaction pass.
        private const int CompactFloor = 4096;

        // Bucket sizes for grouping segments by the line they sit on. Coarse enough that a bucket holds few segments,
        // fine enough that a collinear pair never lands more than one bucket apart.
        private const float LineAngleBucket = 0.05f;
        private const float LineOffsetBucket = 1f;

        // How many angle buckets span a half turn, which is what the angle wraps around.
        private const int LineAngleBuckets = 63;

        // How far two segments' directions may differ and still count as the same line, as the sine of the angle between them.
        // About one degree: loose enough for geometry that is fractionally out of true, tight enough that neighbouring chords
        // of a tessellated curve are never mistaken for one straight line.
        private const float ParallelLimit = 0.0175f;

        // Every group's contour vertices, in world space, addressed through the contour and group ranges below.
        private NativeList<float2> m_Vertices;
        private NativeList<int> m_ContourStart;
        private NativeList<int> m_ContourCount;

        // One entry per group index, whether that index is currently in use or waiting on the free list.
        private NativeList<int> m_GroupContourStart;
        private NativeList<int> m_GroupContourCount;
        private NativeList<PhysicsAABB> m_GroupBounds;
        private NativeList<bool> m_GroupLive;
        private NativeList<PhysicsHandle> m_GroupHandles;
        private NativeList<PhysicsSpace.ProxyHandle> m_GroupProxies;
        private NativeList<int> m_FreeGroups;

        // Each group's clipped segments, retained between computes so an unaffected group is never reclipped.
        private NativeList<int> m_ClippedStart;
        private NativeList<int> m_ClippedCount;
        private NativeList<Segment> m_Clipped;

        // The line each clipped segment belongs to, held alongside the segments themselves and carried forward for any group
        // that was not reclipped, so flattening reads a line off an array rather than looking every segment up by value.
        private NativeList<int> m_LineOfClipped;

        // The raw, pre-cluster output of the sweep. Never mutated by clustering, so it can be compared against the
        // previous compute's version of itself to tell which points actually changed.
        private NativeList<Segment> m_Working;
        private NativeList<Segment> m_WorkingPrevious;

        // The segments after clustering and welding, which the slot placement reads.
        private NativeList<Segment> m_Clustered;

        // Which representative a raw point currently snaps to, and which raw points currently snap to a given
        // representative, so a point carried over unchanged keeps its representative by lookup, and adding or
        // removing a point only touches the small cluster it belongs to.
        private NativeHashMap<PointKey, float2> m_RepresentativeOfPoint;
        private NativeParallelMultiHashMap<PointKey, float2> m_MembersOfRepresentative;

        // Raw points bucketed by position, coarse enough that a bucket holds few points, so finding what is near a
        // new point costs a lookup rather than a scan. A bucket can hold a point no longer registered; a lookup
        // always confirms membership through m_RepresentativeOfPoint before trusting one.
        private NativeParallelMultiHashMap<int, float2> m_PointBuckets;

        // The slot table: one segment per slot, held for as long as that segment exists.
        private NativeList<Segment> m_Slots;
        private NativeList<bool> m_SlotUsed;

        // The previous compute's clipped store, which the gathering pass reads from while the new one is filled.
        private NativeList<Segment> m_ClippedPrevious;
        private NativeList<int> m_ClippedStartPrevious;
        private NativeList<int> m_ClippedCountPrevious;
        private NativeList<int> m_LineOfClippedPrevious;

        // Which groups have to be reclipped, what each one is clipped against, and which stream buffer each one writes into.
        private NativeList<bool> m_DirtyFlags;
        private NativeList<int> m_DirtyGroups;
        private NativeList<int> m_StreamIndexOfGroup;
        private NativeList<int> m_NeighbourStart;
        private NativeList<int> m_NeighbourCount;
        private NativeList<int> m_NeighbourData;

        // The segments gathered onto each shared line, which is what lets the sweep resolve every line independently.
        private NativeList<int> m_LineMembers;
        private NativeList<int> m_LineStart;
        private NativeList<int> m_LineCount;

        // Which line a segment, by value, currently belongs to, so a segment carried over unchanged from the previous
        // compute keeps its line without being retested. Only segments belonging to a reclipped or removed group are
        // taken out and re-tested; everything else survives across computes untouched.
        private NativeHashMap<Segment, int> m_LineOfSegment;

        // One line per id: a fixed seed segment chosen when the id is created, whether the id is currently in use, and
        // how many segments currently belong to it. The seed never changes for as long as the id lives, so a new
        // segment is tested against it directly rather than against every current member.
        private NativeList<Segment> m_LineSeed;
        private NativeList<bool> m_LineLive;
        private NativeList<int> m_LineMemberCount;
        private NativeList<int> m_LineFreeIds;

        // Candidate line ids for a new segment, bucketed the same coarse way as the segments themselves.
        private NativeParallelMultiHashMap<int, int> m_LineBuckets;

        // Groups removed since the last compute, whose old segments still need their line membership taken out.
        private NativeList<int> m_PendingRemovals;

        // The segments meeting each vertex, so ghost assignment reads a bucket rather than scanning.
        private NativeParallelMultiHashMap<PointKey, int> m_EndingAt;
        private NativeParallelMultiHashMap<PointKey, int> m_StartingAt;

        // The scheduled work, and whether its results are still waiting to be read.
        private JobHandle m_Handle;
        private bool m_Pending;

        private NativeList<int> m_Dirty;
        private NativeList<int> m_Changed;
        private NativeList<int> m_Added;
        private NativeList<int> m_Removed;

        // The spatial index over the contour groups, so finding what is near a change costs no scan.
        private PhysicsSpace m_Space;
        private NativeHashMap<PhysicsHandle, int> m_HandleToGroup;

        private readonly float m_Tolerance;

        // How many of the stored vertices belong to groups that still exist, which is what decides when to compact.
        private int m_LiveVertices;
        private int m_ReconsideredGroups;
        private bool m_Disposed;

        #endregion
    }
}
