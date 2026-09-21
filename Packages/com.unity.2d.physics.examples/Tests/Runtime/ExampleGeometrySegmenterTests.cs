using System.Collections.Generic;

using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace Unity.U2D.Physics.Examples.Tests
{
    // Covers ExampleGeometrySegmenter producing the surfaces of the union of its contour groups, as chain segments with ghost vertices.
    // Three properties are checked: the surfaces match a known-good boolean union, the ghosts close into loops, and an
    // incremental build agrees exactly with a rebuild from nothing.
    // The reference union comes from PhysicsComposer, which already performs a correct merge, so these tests compare against
    // the engine rather than against a hand-written expectation.
    // Play-mode (runtime) tests so they run in a built player.
    public class ExampleGeometrySegmenterTests : PhysicsExamplesTestBase
    {
        [SetUp]
        public void CreateSegmenter() => m_Segmenter = new ExampleGeometrySegmenter(Tolerance);

        [TearDown]
        public void DisposeSegmenter()
        {
            m_Segmenter?.Dispose();
            m_Segmenter = null;
        }

        #region Surfaces

        [Test]
        public void TwoGroupsOverlapping_ProducesTheUnionBoundary()
        {
            // Two squares crossing at a corner: the buried parts of each go, and what is left is the outline of the L.
            AssertMatchesUnion(Group(Box(0f, 0f, 4f, 4f)), Group(Box(2f, 2f, 4f, 4f)));
        }

        [Test]
        public void TwoGroupsSharingAnEdge_CancelsThatEdge()
        {
            // Neither square buries any of the other, so the shared edge can only be removed by cancellation.
            AssertMatchesUnion(Group(Box(0f, 0f, 4f, 4f)), Group(Box(4f, 0f, 4f, 4f)));
        }

        [Test]
        public void TwoGroupsSharingPartOfAnEdge_CancelsOnlyTheSharedPart()
        {
            AssertMatchesUnion(Group(Box(0f, 0f, 4f, 4f)), Group(Box(4f, 1f, 4f, 2f)));
        }

        [Test]
        public void TwoGroupsTouchingAtOneCorner_KeepsBothOutlines()
        {
            // Four segments meet at the shared corner, which is the case the ghost assignment is most likely to get wrong.
            AssertMatchesUnion(Group(Box(0f, 0f, 4f, 4f)), Group(Box(4f, 4f, 4f, 4f)));
        }

        [Test]
        public void GroupInsideAnother_ProducesOnlyTheOuterBoundary()
        {
            AssertMatchesUnion(Group(Box(0f, 0f, 8f, 8f)), Group(Box(2f, 2f, 3f, 3f)));
        }

        [Test]
        public void TwoIdenticalGroups_CollapseToOneOutline()
        {
            // Same facing rather than opposite, so these must merge into one surface rather than cancelling to nothing.
            AssertMatchesUnion(Group(Box(0f, 0f, 4f, 4f)), Group(Box(0f, 0f, 4f, 4f)));
        }

        [Test]
        public void ThreeGroupsInARow_MergeIntoOneOutline()
        {
            AssertMatchesUnion(Group(Box(0f, 0f, 4f, 4f)), Group(Box(3f, 0f, 4f, 4f)), Group(Box(6f, 0f, 4f, 4f)));
        }

        [Test]
        public void ThreeGroupsMeetingAtOneVertex_ProducesTheUnionBoundary()
        {
            AssertMatchesUnion(Group(Box(0f, 0f, 4f, 4f)), Group(Box(4f, 0f, 4f, 4f)), Group(Box(0f, 4f, 4f, 4f)));
        }

        [Test]
        public void GroupWithAHole_KeepsTheHoleBoundary()
        {
            // The hole is a clockwise contour inside a counter-clockwise one, so the odd-winding rule makes it empty.
            AssertMatchesUnion(Group(Box(0f, 0f, 10f, 10f), Hole(3f, 3f, 4f, 4f)), Group(Box(4f, 4f, 2f, 2f)));
        }

        [Test]
        public void HoleExactlyFilled_LeavesNoHoleBoundary()
        {
            // The filling group's boundary and the hole's boundary are coincident and opposed, so both cancel away.
            AssertMatchesUnion(Group(Box(0f, 0f, 10f, 10f), Hole(3f, 3f, 4f, 4f)), Group(Box(3f, 3f, 4f, 4f)));
        }

        [Test]
        public void GroupsAroundACourtyard_KeepTheInwardFacingWalls()
        {
            // No clockwise contour anywhere in the input, yet the union encloses a hole, and the walls facing into it are real surfaces.
            AssertMatchesUnion(
                Group(Box(0f, 0f, 9f, 3f)),
                Group(Box(0f, 6f, 9f, 3f)),
                Group(Box(0f, 0f, 3f, 9f)),
                Group(Box(6f, 0f, 3f, 9f)));
        }

        #endregion

        #region Tolerance

        [Test]
        public void GroupsOffsetWithinTolerance_MergeAsIfExact()
        {
            // Offset by a fifth of the tolerance, so the shared edge does not line up exactly.
            // The expectation is the clean geometry it was perturbed from, because a boolean union would leave the sliver.
            AddGroups(Group(Box(0f, 0f, 4f, 4f)), Group(Box(4f + Tolerance * 0.2f, 0f, 4f, 4f)));
            Compute(m_Segmenter);

            AssertMatchesUnionOf(Group(Box(0f, 0f, 4f, 4f)), Group(Box(4f, 0f, 4f, 4f)));
        }

        [Test]
        public void GroupsRotatedWithinTolerance_MergeAsIfExact()
        {
            AddGroups(Group(Box(0f, 0f, 4f, 4f)), Group(Rotate(Box(4f, 0f, 4f, 4f), new Vector2(4f, 0f), 0.02f)));
            Compute(m_Segmenter);

            AssertMatchesUnionOf(Group(Box(0f, 0f, 4f, 4f)), Group(Box(4f, 0f, 4f, 4f)));
        }

        [Test]
        public void CornersApartWithinTolerance_MergeAsIfExact()
        {
            var offset = Tolerance * 0.1f;
            var skewed = new ContourGeometry(new[]
            {
                new Vector2(4f + offset, -offset),
                new Vector2(8f, 0f),
                new Vector2(8f, 4f),
                new Vector2(4f - offset, 4f + offset)
            });

            AddGroups(Group(Box(0f, 0f, 4f, 4f)), Group(skewed));
            Compute(m_Segmenter);

            AssertMatchesUnionOf(Group(Box(0f, 0f, 4f, 4f)), Group(Box(4f, 0f, 4f, 4f)));
        }

        #endregion

        #region Ghosts

        [Test]
        public void OneGroup_GhostsNameTheNeighbouringCorners()
        {
            AddGroups(Group(Box(0f, 0f, 1f, 1f)));
            Compute(m_Segmenter);

            Assert.That(OccupiedSlots().Count, Is.EqualTo(4), "A square should produce one segment per edge.");

            // Following each segment's second ghost must walk the loop and come back to the start.
            AssertGhostsFormClosedLoops();
        }

        [Test]
        public void GroupsAroundACourtyard_GhostsCloseBothLoops()
        {
            AddGroups(
                Group(Box(0f, 0f, 9f, 3f)),
                Group(Box(0f, 6f, 9f, 3f)),
                Group(Box(0f, 0f, 3f, 9f)),
                Group(Box(6f, 0f, 3f, 9f)));
            Compute(m_Segmenter);

            AssertGhostsFormClosedLoops();
        }

        #endregion

        #region Incremental

        [Test]
        public void AddingOneAtATime_MatchesBuildingAllAtOnce()
        {
            var groups = Field(5, 5, 3f, 4f);

            foreach (var group in groups)
            {
                m_Segmenter.Add(group, PhysicsTransform.identity);
                Compute(m_Segmenter);
            }

            var incremental = OccupiedSegments();

            using var fresh = new ExampleGeometrySegmenter(Tolerance);
            foreach (var group in groups)
                fresh.Add(group, PhysicsTransform.identity);

            Compute(fresh);

            AssertSameSegments(SegmentsOf(fresh), incremental);
        }

        [Test]
        public void RemovingAGroup_MatchesBuildingWithoutIt()
        {
            var groups = Field(4, 4, 3f, 4f);
            var handles = new List<PhysicsHandle>();

            foreach (var group in groups)
                handles.Add(m_Segmenter.Add(group, PhysicsTransform.identity));

            Compute(m_Segmenter);

            // Take out a group with neighbours on every side, so its removal has to uncover their buried edges.
            const int Removed = 5;
            m_Segmenter.Remove(handles[Removed]);
            Compute(m_Segmenter);

            using var fresh = new ExampleGeometrySegmenter(Tolerance);
            for (var i = 0; i < groups.Count; ++i)
            {
                if (i != Removed)
                    fresh.Add(groups[i], PhysicsTransform.identity);
            }

            Compute(fresh);

            AssertSameSegments(SegmentsOf(fresh), OccupiedSegments());
        }

        [Test]
        public void RepeatedAddAndRemove_KeepsTheSameAnswer()
        {
            var groups = Field(6, 6, 1f, 1f);
            var handles = new List<PhysicsHandle>();

            foreach (var group in groups)
                handles.Add(m_Segmenter.Add(group, PhysicsTransform.identity));

            Compute(m_Segmenter);
            var original = OccupiedSegments();

            // Take a whole row out and put it straight back, several times over.
            for (var pass = 0; pass < 5; ++pass)
            {
                for (var x = 0; x < 6; ++x)
                    m_Segmenter.Remove(handles[x]);

                Compute(m_Segmenter);

                for (var x = 0; x < 6; ++x)
                    handles[x] = m_Segmenter.Add(groups[x], PhysicsTransform.identity);

                Compute(m_Segmenter);
            }

            AssertSameSegments(original, OccupiedSegments());
        }

        #endregion

        #region Slots

        [Test]
        public void AddingToAnEmptySegmenter_ReportsOnlyAdditions()
        {
            AddGroups(Group(Box(0f, 0f, 1f, 1f)));
            Compute(m_Segmenter);

            Assert.That(m_Segmenter.addedIndices.Length, Is.EqualTo(4), "Every segment of the first group occupies a fresh slot.");
            Assert.That(m_Segmenter.changedIndices.Length, Is.Zero);
            Assert.That(m_Segmenter.removedIndices.Length, Is.Zero);
        }

        [Test]
        public void AnyCompute_NeverReportsBothAdditionsAndRemovals()
        {
            // Emptied slots are reused before new ones are handed out, so a compute either grows the set or shrinks it.
            var groups = Field(4, 4, 1f, 1f);
            var handles = new List<PhysicsHandle>();

            foreach (var group in groups)
            {
                handles.Add(m_Segmenter.Add(group, PhysicsTransform.identity));
                Compute(m_Segmenter);
                AssertNotBothAddedAndRemoved();
            }

            foreach (var handle in handles)
            {
                m_Segmenter.Remove(handle);
                Compute(m_Segmenter);
                AssertNotBothAddedAndRemoved();
            }
        }

        [Test]
        public void ApplyingOnlyTheReportedIndices_KeepsACallerInStep()
        {
            // A mirror of the slot table told nothing but the three index lists. If those lists are incomplete, it drifts.
            var groups = Field(4, 4, 3f, 4f);
            var handles = new List<PhysicsHandle>();

            var mirror = new List<ChainSegmentGeometry>();
            var mirrorUsed = new List<bool>();

            foreach (var group in groups)
            {
                handles.Add(m_Segmenter.Add(group, PhysicsTransform.identity));
                Compute(m_Segmenter);

                ApplyToMirror(mirror, mirrorUsed);
                AssertMirrorMatches(mirror, mirrorUsed);
            }

            foreach (var handle in handles)
            {
                m_Segmenter.Remove(handle);
                Compute(m_Segmenter);

                ApplyToMirror(mirror, mirrorUsed);
                AssertMirrorMatches(mirror, mirrorUsed);
            }

            // Everything is gone, so no slot the caller was told about should still be occupied.
            foreach (var used in mirrorUsed)
                Assert.That(used, Is.False, "A slot is still occupied after every group was removed.");
        }

        [Test]
        public void RemovingEverything_LeavesNoSegments()
        {
            var handleA = m_Segmenter.Add(Group(Box(0f, 0f, 4f, 4f)), PhysicsTransform.identity);
            var handleB = m_Segmenter.Add(Group(Box(4f, 0f, 4f, 4f)), PhysicsTransform.identity);
            Compute(m_Segmenter);

            m_Segmenter.Remove(handleA);
            m_Segmenter.Remove(handleB);
            Compute(m_Segmenter);

            Assert.That(OccupiedSlots(), Is.Empty);
            Assert.That(m_Segmenter.groupCount, Is.Zero);
        }

        #endregion

        #region Handles

        [Test]
        public void RemovingAnUnknownHandle_ReportsNoRemoval()
        {
            Assert.That(m_Segmenter.Remove(default), Is.False);
        }

        [Test]
        public void AddingNull_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => m_Segmenter.Add(null, PhysicsTransform.identity));
        }

        [Test]
        public void UsingAfterDispose_Throws()
        {
            m_Segmenter.Dispose();

            Assert.Throws<System.ObjectDisposedException>(() => m_Segmenter.Compute());
            Assert.Throws<System.ObjectDisposedException>(() => m_Segmenter.Add(Group(Box(0f, 0f, 1f, 1f)), PhysicsTransform.identity));

            // The teardown must not dispose it a second time.
            m_Segmenter = null;
        }

        [Test]
        public void AddedGroup_PlacedByItsTransform()
        {
            // The same unit square added at two separated places must not merge, so the transform has to be applied on the way in.
            m_Segmenter.Add(Group(Box(0f, 0f, 1f, 1f)), PhysicsTransform.identity);
            m_Segmenter.Add(Group(Box(0f, 0f, 1f, 1f)), new PhysicsTransform(new Vector2(10f, 0f)));
            Compute(m_Segmenter);

            Assert.That(OccupiedSlots().Count, Is.EqualTo(8), "Two separated squares should keep all eight of their edges.");
        }

        #endregion

        #region Assertions

        // Add the groups, compute, and check the resulting surfaces are the boundary of the union of the same groups.
        void AssertMatchesUnion(params ContourGroupGeometry[] groups)
        {
            AddGroups(groups);
            Compute(m_Segmenter);

            AssertMatchesUnionOf(groups);
            AssertGhostsFormClosedLoops();
        }

        // Compare what the segmenter produced against the composer's union of the given groups.
        // Compared by sampling along every segment in both directions, because the two sides may subdivide the same curve differently.
        void AssertMatchesUnionOf(params ContourGroupGeometry[] groups)
        {
            var produced = OccupiedSegments();
            var expected = UnionOutline(groups);

            Assert.That(produced, Is.Not.Empty, "The segmenter produced no surfaces at all.");

            AssertLiesOn(expected, produced, "The segmenter produced a surface the union does not have.");
            AssertLiesOn(produced, expected, "The segmenter is missing a surface the union has.");
        }

        // Every sampled point along each segment in 'from' must lie on some segment in 'to'.
        void AssertLiesOn(List<ChainSegmentGeometry> to, List<ChainSegmentGeometry> from, string message)
        {
            var allowed = Mathf.Max(Tolerance * 2f, 0.01f);

            foreach (var segment in from)
            {
                for (var sample = 1; sample <= 4; ++sample)
                {
                    var point = Vector2.Lerp(segment.segment.point1, segment.segment.point2, sample / 5f);

                    var closest = float.MaxValue;
                    foreach (var candidate in to)
                        closest = Mathf.Min(closest, DistanceToSegment(point, candidate.segment.point1, candidate.segment.point2));

                    Assert.That(closest, Is.LessThanOrEqualTo(allowed),
                        $"{message} {point} is {closest} from anything, on the segment {segment.segment.point1} to {segment.segment.point2}.");
                }
            }
        }

        // Following the ghosts must walk closed loops covering every segment, which is the property that lets the
        // ghosts be assigned per vertex without anything ever tracing a loop.
        void AssertGhostsFormClosedLoops()
        {
            var segments = OccupiedSegments();
            if (segments.Count == 0)
                return;

            var successor = new int[segments.Count];
            for (var i = 0; i < segments.Count; ++i)
            {
                successor[i] = -1;

                for (var j = 0; j < segments.Count; ++j)
                {
                    if (i == j)
                        continue;

                    if (segments[j].segment.point1 == segments[i].segment.point2 && segments[j].segment.point2 == segments[i].ghost2)
                    {
                        successor[i] = j;
                        break;
                    }
                }

                Assert.That(successor[i], Is.Not.EqualTo(-1),
                    $"The segment {segments[i].segment.point1} to {segments[i].segment.point2} has no successor matching its second ghost {segments[i].ghost2}.");
            }

            // Exactly one segment may follow each, or the loops branch instead of closing.
            var incoming = new int[segments.Count];
            foreach (var next in successor)
                ++incoming[next];

            for (var i = 0; i < incoming.Length; ++i)
            {
                Assert.That(incoming[i], Is.EqualTo(1),
                    $"The segment {segments[i].segment.point1} to {segments[i].segment.point2} follows {incoming[i]} segments, expected one.");
            }

            // The first ghost must name whatever precedes it, so the two ghosts agree about the loop.
            for (var i = 0; i < segments.Count; ++i)
            {
                var next = successor[i];
                Assert.That(segments[next].ghost1, Is.EqualTo(segments[i].segment.point1),
                    $"The segment {segments[next].segment.point1} to {segments[next].segment.point2} has a first ghost of {segments[next].ghost1}, expected {segments[i].segment.point1}.");
            }
        }

        void AssertNotBothAddedAndRemoved()
        {
            Assert.That(m_Segmenter.addedIndices.Length == 0 || m_Segmenter.removedIndices.Length == 0, Is.True,
                $"One compute reported {m_Segmenter.addedIndices.Length} additions and {m_Segmenter.removedIndices.Length} removals together.");
        }

        static void AssertSameSegments(List<ChainSegmentGeometry> expected, List<ChainSegmentGeometry> actual)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Count), "The two builds produced a different number of segments.");

            var matched = new bool[actual.Count];
            foreach (var want in expected)
            {
                var found = false;

                for (var i = 0; i < actual.Count && !found; ++i)
                {
                    if (matched[i] || !Same(actual[i], want))
                        continue;

                    matched[i] = true;
                    found = true;
                }

                Assert.That(found, Is.True,
                    $"No match for the segment {want.segment.point1} to {want.segment.point2} with ghosts {want.ghost1} and {want.ghost2}.");
            }
        }

        // Bring the mirror up to date using nothing but the reported index lists, exactly as a caller would.
        void ApplyToMirror(List<ChainSegmentGeometry> mirror, List<bool> mirrorUsed)
        {
            while (mirror.Count < m_Segmenter.slotCount)
            {
                mirror.Add(default);
                mirrorUsed.Add(false);
            }

            foreach (var slot in m_Segmenter.addedIndices)
            {
                mirror[slot] = m_Segmenter[slot];
                mirrorUsed[slot] = true;
            }

            foreach (var slot in m_Segmenter.changedIndices)
                mirror[slot] = m_Segmenter[slot];

            foreach (var slot in m_Segmenter.removedIndices)
                mirrorUsed[slot] = false;
        }

        void AssertMirrorMatches(List<ChainSegmentGeometry> mirror, List<bool> mirrorUsed)
        {
            for (var slot = 0; slot < m_Segmenter.slotCount; ++slot)
            {
                var believedOccupied = slot < mirrorUsed.Count && mirrorUsed[slot];
                Assert.That(believedOccupied, Is.EqualTo(m_Segmenter.IsSlotOccupied(slot)),
                    $"Slot {slot} is {(m_Segmenter.IsSlotOccupied(slot) ? "occupied" : "free")} but the caller was told otherwise.");

                if (believedOccupied)
                    Assert.That(Same(mirror[slot], m_Segmenter[slot]), Is.True, $"Slot {slot} holds different geometry than the caller was told.");
            }
        }

        #endregion

        #region Helpers

        // Computing hands back a job handle and the segmenter holds its results until one is read, so finishing the work means
        // reading something. A test that only wants the geometry settled before changing it again reads the slot count.
        static void Compute(ExampleGeometrySegmenter segmenter)
        {
            segmenter.Compute();

            _ = segmenter.slotCount;
        }

        void AddGroups(params ContourGroupGeometry[] groups)
        {
            foreach (var group in groups)
                m_Segmenter.Add(group, PhysicsTransform.identity);
        }

        List<int> OccupiedSlots()
        {
            var slots = new List<int>();
            for (var slot = 0; slot < m_Segmenter.slotCount; ++slot)
            {
                if (m_Segmenter.IsSlotOccupied(slot))
                    slots.Add(slot);
            }

            return slots;
        }

        List<ChainSegmentGeometry> OccupiedSegments() => SegmentsOf(m_Segmenter);

        static List<ChainSegmentGeometry> SegmentsOf(ExampleGeometrySegmenter segmenter)
        {
            var segments = new List<ChainSegmentGeometry>();
            for (var slot = 0; slot < segmenter.slotCount; ++slot)
            {
                if (segmenter.IsSlotOccupied(slot))
                    segments.Add(segmenter[slot]);
            }

            return segments;
        }

        // The boundary of the union of the given groups, straight from the composer, which already merges correctly.
        static List<ChainSegmentGeometry> UnionOutline(params ContourGroupGeometry[] groups)
        {
            var segments = new List<ChainSegmentGeometry>();
            var composer = PhysicsComposer.Create();

            foreach (var group in groups)
            {
                for (var c = 0; c < group.contourCount; ++c)
                {
                    var contour = group[c];
                    var vertices = new Vector2[contour.vertexCount];
                    for (var v = 0; v < vertices.Length; ++v)
                        vertices[v] = contour[v];

                    composer.AddLayer(vertices, PhysicsTransform.identity, PhysicsComposer.Operation.OR);
                }
            }

            var chains = composer.CreateChainGeometry(out var vertexBuffer, Vector2.one, Allocator.Temp);
            composer.Destroy();

            foreach (var chain in chains)
            {
                var vertices = chain.vertices;
                for (var i = 0; i < vertices.Length; ++i)
                {
                    var from = vertices[i];
                    var to = vertices[(i + 1) % vertices.Length];
                    segments.Add(new ChainSegmentGeometry(SegmentGeometry.Create(from, to), from, to));
                }
            }

            chains.Dispose();
            vertexBuffer.Dispose();
            return segments;
        }

        static bool Same(ChainSegmentGeometry a, ChainSegmentGeometry b)
        {
            return a.segment.point1 == b.segment.point1 && a.segment.point2 == b.segment.point2 &&
                   a.ghost1 == b.ghost1 && a.ghost2 == b.ghost2;
        }

        static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var lengthSquared = ab.sqrMagnitude;
            if (lengthSquared < 1e-12f)
                return (point - a).magnitude;

            var t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSquared);
            return (point - (a + t * ab)).magnitude;
        }

        static ContourGroupGeometry Group(params ContourGeometry[] contours) => new ContourGroupGeometry(contours);

        // A counter-clockwise box, which the odd-winding rule reads as solid.
        static ContourGeometry Box(float x, float y, float width, float height)
        {
            return new ContourGeometry(new[]
            {
                new Vector2(x, y),
                new Vector2(x + width, y),
                new Vector2(x + width, y + height),
                new Vector2(x, y + height)
            });
        }

        // A clockwise box, which the odd-winding rule reads as a hole when it sits inside a solid.
        static ContourGeometry Hole(float x, float y, float width, float height)
        {
            return new ContourGeometry(new[]
            {
                new Vector2(x, y),
                new Vector2(x, y + height),
                new Vector2(x + width, y + height),
                new Vector2(x + width, y)
            });
        }

        static ContourGeometry Rotate(ContourGeometry contour, Vector2 pivot, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);

            var rotated = new ContourGeometry(contour.vertexCount);
            for (var v = 0; v < contour.vertexCount; ++v)
            {
                var offset = contour[v] - pivot;
                rotated.Add(pivot + new Vector2(offset.x * cos - offset.y * sin, offset.x * sin + offset.y * cos));
            }

            return rotated;
        }

        // A field of overlapping or abutting squares, so a change in the middle has neighbours on every side.
        static List<ContourGroupGeometry> Field(int columns, int rows, float spacing, float size)
        {
            var groups = new List<ContourGroupGeometry>(columns * rows);
            for (var y = 0; y < rows; ++y)
            {
                for (var x = 0; x < columns; ++x)
                    groups.Add(Group(Box(x * spacing, y * spacing, size, size)));
            }

            return groups;
        }

        #endregion

        #region Internal

        const float Tolerance = 0.01f;

        ExampleGeometrySegmenter m_Segmenter;

        #endregion
    }
}
