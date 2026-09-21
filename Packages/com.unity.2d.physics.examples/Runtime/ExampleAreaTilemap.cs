using System.Collections.Generic;

using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Unity.U2D.Physics.Examples
{
    /// <summary>
    /// Produces shapes from the occupied cells of a Tilemap.
    /// </summary>
    /// <remarks>
    /// This owns its shapes rather than deriving from <see cref="PhysicsArea"/>, because a tilemap holds no geometry of its own
    /// and its cells should be free to carry different surface materials, neither of which the area contract allows.
    /// Owning the shapes is also what lets <see cref="OutputType.Segments"/> update only the shapes an edit disturbed.
    /// </remarks>
    [AddComponentMenu("Physics 2D (Core)/Example Area Tilemap", 41)]
    [Icon("Packages/com.unity.2d.physics.examples/Editor/Icons/ExampleAreaTilemap.png")]
    [PhysicsCustomProperties]
    public sealed class ExampleAreaTilemap : PhysicsPoseProvider, PhysicsCallbacks.ITransformChangedCallback, IPhysicsSelectable
    {
        /// <summary>
        /// How the occupied cells become shapes.
        /// </summary>
        public enum OutputType
        {
            /// <summary>
            /// Filled polygons per occupied cell, with the surfaces between adjacent cells left in place.
            /// </summary>
            Polygons,

            /// <summary>
            /// Chain segments describing only the outside of the combined solid, with the surfaces between adjacent cells removed.
            /// </summary>
            Segments
        }

        /// <summary>
        /// How the occupied cells become shapes.
        /// </summary>
        /// <remarks>
        /// Changing this releases any segmenter already built and forces a full rebuild of every shape.
        /// <see cref="OutputType.Segments"/> also updates incrementally as tiles change, while <see cref="OutputType.Polygons"/> always rebuilds the whole map on the next edit.
        /// </remarks>
        public OutputType output
        {
            get => m_Output;
            set
            {
                if (m_Output == value)
                    return;

                m_Output = value;
                ReleaseSegmenter();
                Rebuild();
            }
        }

        /// <summary>
        /// How far out of true two cells' outlines can be and still merge into one surface, in world units.
        /// </summary>
        /// <remarks>
        /// Only used by <see cref="OutputType.Segments"/>. Leave it at zero to follow the project's scale, which uses twice
        /// <see cref="PhysicsWorld.linearSlop"/>. Negative values are clamped away, and a value below one linear slop is
        /// raised to it in <see cref="activeSegmentTolerance"/>, since anything smaller could never merge two pieces of geometry.
        /// Raise it when tile outlines are authored loosely and their joins are failing to merge. Raising it too far starts
        /// pulling apart geometry that was meant to stay separate, since anything closer than this counts as touching.
        /// </remarks>
        public float segmentTolerance
        {
            get => m_SegmentTolerance;
            set
            {
                var clamped = Mathf.Max(value, 0f);
                if (Mathf.Approximately(m_SegmentTolerance, clamped))
                    return;

                m_SegmentTolerance = clamped;

                // The tolerance is fixed for a segmenter's lifetime, so changing it means starting a new one.
                ReleaseSegmenter();
                Rebuild();
            }
        }

        /// <summary>
        /// The segment-merge tolerance actually in effect.
        /// </summary>
        /// <remarks>
        /// Follows the project's scale when <see cref="segmentTolerance"/> is zero, and is never below
        /// <see cref="PhysicsWorld.linearSlop"/>, because chain segments need their vertices further apart than that,
        /// so anything smaller could never merge two pieces of geometry.
        /// </remarks>
        public float activeSegmentTolerance => m_SegmentTolerance > 0f
            ? Mathf.Max(m_SegmentTolerance, PhysicsWorld.linearSlop)
            : ExampleGeometrySegmenter.defaultTolerance;

        /// <summary>
        /// The shape definition every produced shape is created with.
        /// </summary>
        /// <remarks>
        /// One definition for the whole map for now, since per-cell definitions are what would let this component own its shapes individually.
        /// Regardless of <see cref="PhysicsShapeDefinition.startMassUpdate"/>, mass updates once for the whole map after every build, not once per shape.
        /// </remarks>
        public PhysicsShapeDefinition definition
        {
            get => m_Definition;
            set
            {
                m_Definition = value;
                Rebuild();
            }
        }

        /// <summary>
        /// How many shapes this component currently owns.
        /// </summary>
        public int shapeCount
        {
            get
            {
                if (!m_Shapes.IsCreated)
                    return 0;

                var count = 0;
                for (var i = 0; i < m_Shapes.Length; ++i)
                {
                    if (m_Shapes[i].isValid)
                        ++count;
                }

                return count;
            }
        }

        /// <summary>
        /// The shapes this component currently owns.
        /// </summary>
        /// <remarks>
        /// Skips the empty entries left where a segment was removed, so the result is only the live shapes.
        /// </remarks>
        /// <returns>The live shapes.</returns>
        public IEnumerable<PhysicsShape> GetShapes()
        {
            if (!m_Shapes.IsCreated)
                yield break;

            for (var i = 0; i < m_Shapes.Length; ++i)
            {
                if (m_Shapes[i].isValid)
                    yield return m_Shapes[i];
            }
        }

        // An inspector edit writes the serialized field directly, so the work the property setters do has to happen here as well.
        // The tolerance is clamped in case a negative value was typed or pasted, and the segmenter is released because both
        // fields decide how it was built rather than merely what it produces.
        private void OnValidate()
        {
            m_SegmentTolerance = Mathf.Max(m_SegmentTolerance, 0f);

            // This also runs while the component is being deserialized, before the provider has been enabled and allocated the
            // shape storage a build writes into, so the storage existing is what says a rebuild is possible at all.
            if (!isActiveAndEnabled || !m_Shapes.IsCreated)
                return;

            ReleaseSegmenter();
            Rebuild();
        }

        #region Overrides

        // The component's fields describe what geometry the cells produce, so the section reads as Geometry rather than the generic Properties.
        // That also keeps it recognizable alongside the area components, which is what a user coming from those will expect.
        protected override string propertiesSectionTitle => "Geometry";

        // Allocate the shape storage and start watching this component's Transform, since the geometry is placed relative to the owning pose.
        protected override void OnProviderEnable()
        {
            if (!m_Shapes.IsCreated)
                m_Shapes = new NativeList<PhysicsShape>(InitialShapeCapacity, Allocator.Persistent);

            PhysicsWorld.RegisterTransformChange(transform, this);
        }

        protected override void OnProviderDisable()
        {
            PhysicsWorld.UnregisterTransformChange(transform, this);

            ReleaseSegmenter();

            if (m_Shapes.IsCreated)
                m_Shapes.Dispose();
        }

        // Watch the Tilemap's painted content and its tiles' sprite outlines, so the shapes track the map live in edit mode and play.
        protected override void OnBeforeCreatePhysics()
        {
            Tilemap.tilemapTileChanged += OnTilemapTileChanged;
            PhysicsSpriteExtensions.physicsGeometryChanged += OnSpriteGeometryChanged;
        }

        protected override void OnAfterDestroyPhysics()
        {
            Tilemap.tilemapTileChanged -= OnTilemapTileChanged;
            PhysicsSpriteExtensions.physicsGeometryChanged -= OnSpriteGeometryChanged;
        }

        // Build every shape from scratch. Incremental updates go through OnTilemapTileChanged instead, which never tears down.
        protected override void OnCreatePhysics(PhysicsBody resolvedBody)
        {
            m_Body = resolvedBody;

            if (m_Output == OutputType.Segments)
                BuildSegments(resolvedBody);
            else
                BuildPolygons(resolvedBody);

            if (resolvedBody.isValid)
                resolvedBody.ApplyMassFromShapes();

            // A rebuild replaces every shape, so a component that was already selected has to say so again.
            if (m_Selected)
                SetSelectedDrawing(true);
        }

        protected override void OnDestroyPhysics()
        {
            DestroyAllShapes();
            m_Body = default;
        }

        // The geometry is authored in the Tilemap's local space and built relative to the owning pose, so any move between the two invalidates it.
        void PhysicsCallbacks.ITransformChangedCallback.OnTransformChanged(PhysicsEvents.TransformChangeEvent transformChangeEvent)
        {
            if (isActiveAndEnabled)
                Rebuild();
        }

        // Enabled unconditionally: the world draw options decide what is drawn, never whether the state is set.
        // The selected state is remembered because this component creates shapes outside a full rebuild, and a shape created then would otherwise miss it.
        void IPhysicsSelectable.Select()
        {
            m_Selected = true;

            SetSelectedDrawing(true);
        }

        // Only this component's own shapes are disabled, never every shape in the world: another package or a user script can be drawing objects it does not own.
        void IPhysicsSelectable.Deselect()
        {
            m_Selected = false;

            SetSelectedDrawing(false);
        }

        // Write the selected-drawing state to every shape this component owns.
        // One batch call for the whole map: the world is locked once, and any invalid shape is skipped natively.
        private void SetSelectedDrawing(bool selectedDrawing)
        {
            if (!m_Shapes.IsCreated || m_Shapes.Length == 0)
                return;

            PhysicsShape.SetSelectedDrawing(m_Shapes.AsReadOnlySpan(), selectedDrawing);
        }

        #endregion

        #region Building

        // One set of filled polygons per occupied cell, which leaves the surfaces between adjacent cells in place.
        //
        // Every cell's polygons are gathered into one list and created in a single batch, so the engine validates and locks once for the whole map rather than once per cell.
        private void BuildPolygons(PhysicsBody body)
        {
            if (!TryGetRelative(out var relative))
                return;

            var map = tilemap;
            if (map == null)
                return;

            map.GetTiles(map.cellBounds, out var positions, out var tiles, withinBounds: false);

            var geometry = new NativeList<PolygonGeometry>(InitialShapeCapacity, Allocator.Temp);
            var cells = new NativeList<Vector3Int>(InitialShapeCapacity, Allocator.Temp);

            try
            {
                foreach (var cell in positions)
                {
                    var sprite = map.GetSprite(cell);
                    if (sprite == null)
                        continue;

                    // Read the shared decomposition rather than a copy of it, since each polygon is transformed into the gathered list anyway.
                    var polygons = PhysicsSpriteCache.GetPolygons(sprite, PhysicsConstants.MaxPolygonVertices, useDelaunay: true, out _);
                    if (polygons.Length == 0)
                        continue;

                    var cellMatrix = relative * Matrix4x4.Translate(map.GetCellCenterLocal(cell)) * map.GetTransformMatrix(cell);

                    for (var i = 0; i < polygons.Length; ++i)
                    {
                        var placed = polygons[i].Transform(cellMatrix, scaleRadius: false);
                        if (!placed.isValid)
                            continue;

                        geometry.Add(placed);
                        cells.Add(cell);
                    }
                }

                if (geometry.Length == 0)
                    return;

                var created = body.CreateShapeBatch(geometry.AsArray().AsReadOnlySpan(), buildDefinition);

                // Stamp every shape with the cell it came from, so a contact, query or overlap result traces back to the tile that produced it.
                // The cells were gathered alongside the geometry, so they are already in the same order as the created shapes.
                var userDatas = new NativeArray<PhysicsUserData>(created.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                for (var i = 0; i < created.Length; ++i)
                    userDatas[i] = new PhysicsUserData { vector3IntValue = cells[i] };

                PhysicsShape.SetOwnerUserData(created.AsReadOnlySpan(), userDatas.AsReadOnlySpan());
                userDatas.Dispose();

                for (var i = 0; i < created.Length; ++i)
                    m_Shapes.Add(created[i]);

                created.Dispose();
            }
            finally
            {
                cells.Dispose();
                geometry.Dispose();
                positions.Dispose();
                tiles.Dispose();
            }
        }

        // The merged surface of every occupied cell as chain segments, so the joins between adjacent cells carry no collision.
        //
        // Every occupied slot's segment is gathered into one list and created in a single batch, so the engine validates and locks once for the whole map rather than once per slot.
        private void BuildSegments(PhysicsBody body)
        {
            EnsureSegmenter();
            m_Segmenter.Compute();

            // Reading the slot count is what finishes the segmenter's work, and that has to happen whether or not the shapes can
            // be placed, since leaving the work in flight would refuse the next cell change.
            var slots = m_Segmenter.slotCount;

            if (!TryGetRelative(out var relative))
                return;

            // A full build takes every occupied slot, so the reported index lists are not needed here.
            // Clearing the storage first leaves an empty entry wherever a slot produces no shape, which the gathering pass then skips over.
            m_Shapes.Clear();
            m_Shapes.Resize(slots, NativeArrayOptions.ClearMemory);

            var geometry = new NativeList<ChainSegmentGeometry>(slots, Allocator.Temp);
            var filled = new NativeList<int>(slots, Allocator.Temp);

            for (var slot = 0; slot < slots; ++slot)
            {
                if (!m_Segmenter.IsSlotOccupied(slot))
                    continue;

                var placed = m_Segmenter[slot].Transform(relative);
                if (!placed.isValid)
                    continue;

                geometry.Add(placed);
                filled.Add(slot);
            }

            if (geometry.Length > 0)
            {
                var created = body.CreateShapeBatch(geometry.AsArray().AsReadOnlySpan(), buildDefinition);
                for (var i = 0; i < created.Length; ++i)
                {
                    m_Shapes[filled[i]] = created[i];
                    StampCell(created[i], filled[i]);
                }

                created.Dispose();
            }

            filled.Dispose();
            geometry.Dispose();
        }

        // Record which cell a segment's surface came from, the same as the polygon output does, so anything reading a contact can
        // tell which tile it touched. A merged surface never spans two cells, so there is always exactly one answer.
        private void StampCell(PhysicsShape shape, int slot)
        {
            if (!shape.isValid)
                return;

            if (m_GroupCells.TryGetValue(m_Segmenter.GetSlotSource(slot), out var cell))
                shape.SetOwnerUserData(new PhysicsUserData { vector3IntValue = cell });
        }

        // Destroy every shape this component holds, in one batch, and update the body mass once afterward rather than once per shape.
        // A shape can already be invalid here when the body was destroyed first and took its shapes with it, so only the live ones are collected.
        private void DestroyAllShapes()
        {
            if (!m_Shapes.IsCreated || m_Shapes.Length == 0)
                return;

            var shapes = new NativeList<PhysicsShape>(m_Shapes.Length, Allocator.Temp);

            for (var i = 0; i < m_Shapes.Length; ++i)
            {
                if (m_Shapes[i].isValid)
                    shapes.Add(m_Shapes[i]);
            }

            if (shapes.Length > 0)
            {
                PhysicsWorld.DestroyShapeBatch(shapes.AsArray().AsReadOnlySpan(), false);

                if (m_Body.isValid)
                    m_Body.ApplyMassFromShapes();
            }

            shapes.Dispose();
            m_Shapes.Clear();
        }

        #endregion

        #region Incremental

        // A SyncTile names the cell that changed and carries a null tile when it was erased, so segmentation updates just those
        // cells and then touches only the shapes the recompute disturbed. Polygon output holds no per-cell state, so it rebuilds.
        private void OnTilemapTileChanged(Tilemap changed, Tilemap.SyncTile[] tiles)
        {
            if (changed != tilemap)
                return;

            if (m_Output != OutputType.Segments || m_Segmenter == null || !m_Body.isValid)
            {
                Rebuild();
                return;
            }

            foreach (var tile in tiles)
            {
                RemoveCell(tile.position);

                if (tile.tile != null)
                    AddCell(tile.position);
            }

            m_Segmenter.Compute();
            ApplySegmentChanges();
        }

        // Turn the segmenter's three index lists into the smallest set of shape operations that keeps the shapes in step.
        private void ApplySegmentChanges()
        {
            // Reading the slot count is what finishes the segmenter's work, and that has to happen whether or not the shapes can
            // be placed, since leaving the work in flight would refuse the next cell change.
            var slots = m_Segmenter.slotCount;

            if (!TryGetRelative(out var relative))
                return;

            // The slot table only ever grows, so the shape array is extended to match before anything is addressed by index.
            while (m_Shapes.Length < slots)
                m_Shapes.Add(default);

            UpdateChangedShapes(relative);
            CreateAddedShapes(relative);
            DestroyRemovedShapes();

            if (m_Body.isValid)
                m_Body.ApplyMassFromShapes();
        }

        // Assign new geometry to the shapes already standing in the changed slots, in one batch.
        // The shapes and their geometry are gathered first because the changed slots are scattered and the batch call wants two parallel runs.
        private void UpdateChangedShapes(Matrix4x4 relative)
        {
            var changed = m_Segmenter.changedIndices;
            if (changed.Length == 0)
                return;

            var shapes = new NativeList<PhysicsShape>(changed.Length, Allocator.Temp);
            var geometry = new NativeList<ChainSegmentGeometry>(changed.Length, Allocator.Temp);

            // A slot whose shape was dropped earlier as degenerate has nothing to update, so it is created instead, gathered here for its own batch.
            var revivedGeometry = new NativeList<ChainSegmentGeometry>(changed.Length, Allocator.Temp);
            var revivedSlots = new NativeList<int>(changed.Length, Allocator.Temp);

            foreach (var slot in changed)
            {
                var placed = m_Segmenter[slot].Transform(relative);
                if (!placed.isValid)
                    continue;

                if (!m_Shapes[slot].isValid)
                {
                    revivedGeometry.Add(placed);
                    revivedSlots.Add(slot);
                    continue;
                }

                // A changed slot can have changed owner as well as geometry, so the cell is stamped again rather than assumed.
                StampCell(m_Shapes[slot], slot);

                shapes.Add(m_Shapes[slot]);
                geometry.Add(placed);
            }

            if (shapes.Length > 0)
                PhysicsShape.SetBatchGeometry(shapes.AsArray().AsReadOnlySpan(), geometry.AsArray().AsReadOnlySpan());

            if (revivedGeometry.Length > 0)
            {
                var created = m_Body.CreateShapeBatch(revivedGeometry.AsArray().AsReadOnlySpan(), buildDefinition);
                for (var i = 0; i < created.Length; ++i)
                {
                    m_Shapes[revivedSlots[i]] = created[i];
                    StampCell(created[i], revivedSlots[i]);
                }

                created.Dispose();
            }

            revivedSlots.Dispose();
            revivedGeometry.Dispose();
            geometry.Dispose();
            shapes.Dispose();
        }

        // Create a shape for each newly filled slot, in one batch, and record the handles against those slots.
        private void CreateAddedShapes(Matrix4x4 relative)
        {
            var added = m_Segmenter.addedIndices;
            if (added.Length == 0)
                return;

            var geometry = new NativeList<ChainSegmentGeometry>(added.Length, Allocator.Temp);
            var slots = new NativeList<int>(added.Length, Allocator.Temp);

            foreach (var slot in added)
            {
                var placed = m_Segmenter[slot].Transform(relative);
                if (!placed.isValid)
                    continue;

                geometry.Add(placed);
                slots.Add(slot);
            }

            if (geometry.Length > 0)
            {
                var created = m_Body.CreateShapeBatch(geometry.AsArray().AsReadOnlySpan(), buildDefinition);
                for (var i = 0; i < created.Length; ++i)
                {
                    m_Shapes[slots[i]] = created[i];
                    StampCell(created[i], slots[i]);
                }

                // These arrive outside a rebuild, so the Editor never sees them created and cannot enable the selected drawing itself.
                if (m_Selected)
                    PhysicsShape.SetSelectedDrawing(created.AsReadOnlySpan(), true);

                created.Dispose();
            }

            slots.Dispose();
            geometry.Dispose();
        }

        // Destroy the shapes standing in emptied slots, in one batch, and clear those entries.
        private void DestroyRemovedShapes()
        {
            var removed = m_Segmenter.removedIndices;
            if (removed.Length == 0)
                return;

            var shapes = new NativeList<PhysicsShape>(removed.Length, Allocator.Temp);

            foreach (var slot in removed)
            {
                if (m_Shapes[slot].isValid)
                    shapes.Add(m_Shapes[slot]);

                m_Shapes[slot] = default;
            }

            if (shapes.Length > 0 && m_Body.isValid)
                PhysicsWorld.DestroyShapeBatch(shapes.AsArray().AsReadOnlySpan(), false);

            shapes.Dispose();
        }

        #endregion

        #region Segmenter

        // Create the segmenter and hand it every occupied cell, which is the one time the whole map is walked.
        private void EnsureSegmenter()
        {
            if (m_Segmenter != null)
                return;

            m_Segmenter = new ExampleGeometrySegmenter(activeSegmentTolerance);
            m_CellGroups = new NativeHashMap<Vector3Int, PhysicsHandle>(InitialShapeCapacity, Allocator.Persistent);
            m_GroupCells = new NativeHashMap<PhysicsHandle, Vector3Int>(InitialShapeCapacity, Allocator.Persistent);

            var map = tilemap;
            if (map == null)
                return;

            map.GetTiles(map.cellBounds, out var positions, out var tiles, withinBounds: false);

            try
            {
                foreach (var cell in positions)
                    AddCell(cell);
            }
            finally
            {
                positions.Dispose();
                tiles.Dispose();
            }
        }

        // Hand one cell's sprite outline to the segmenter, placed at the cell in the Tilemap's local space.
        private void AddCell(Vector3Int cell)
        {
            var map = tilemap;
            if (map == null)
                return;

            var sprite = map.GetSprite(cell);
            if (sprite == null)
                return;

            var group = PhysicsSpriteCache.GetContourGroup(sprite);
            if (group == null || !group.isValid)
                return;

            // Place the sprite-local contours at the cell: translate to the cell centre, then apply the tile's own flip and rotation.
            var placed = group.Transform(Matrix4x4.Translate(map.GetCellCenterLocal(cell)) * map.GetTransformMatrix(cell));

            var handle = m_Segmenter.Add(placed, PhysicsTransform.identity);

            m_CellGroups[cell] = handle;
            m_GroupCells[handle] = cell;
        }

        private void RemoveCell(Vector3Int cell)
        {
            if (!m_CellGroups.TryGetValue(cell, out var handle))
                return;

            m_Segmenter.Remove(handle);
            m_CellGroups.Remove(cell);
            m_GroupCells.Remove(handle);
        }

        // A tile sprite's cached geometry was invalidated, for instance its outline or pixels-per-unit changed and it reimported.
        // Every cell built from that sprite is stale, so the segmenter is dropped and rebuilt rather than patched.
        private void OnSpriteGeometryChanged(Sprite sprite)
        {
            var map = tilemap;
            if (map == null)
                return;

            // Check the map's distinct used sprites, a native GC-free set, rather than walking every cell.
            using var usedSprites = map.GetUsedSprites();
            foreach (var used in usedSprites)
            {
                if (used != sprite)
                    continue;

                ReleaseSegmenter();
                Rebuild();
                return;
            }
        }

        private void ReleaseSegmenter()
        {
            m_Segmenter?.Dispose();
            m_Segmenter = null;

            if (m_CellGroups.IsCreated)
                m_CellGroups.Dispose();

            if (m_GroupCells.IsCreated)
                m_GroupCells.Dispose();
        }

        #endregion

        #region Internal

        // The Tilemap whose occupied cells become the shapes; always the one on this GameObject.
        private Tilemap tilemap => GetComponent<Tilemap>();

        // The authored definition with the per-shape mass update taken out, which is what every create uses.
        // A mass update walks every shape already on the body rather than only the new one, so the body's mass is applied once after a build instead.
        private PhysicsShapeDefinition buildDefinition
        {
            get
            {
                var definition = m_Definition;
                definition.startMassUpdate = false;

                return definition;
            }
        }

        // The matrix mapping geometry in this component's local space into the owning body's space.
        // Returns false when there is nothing to build against.
        private bool TryGetRelative(out Matrix4x4 relative)
        {
            relative = Matrix4x4.identity;

            if (resolvedPose == null)
                return false;

            var anchor = resolvedPose.transform;
            if (anchor == transform)
                return true;

            relative = anchor.worldToLocalMatrix * transform.localToWorldMatrix;
            return true;
        }

        // How many shapes the storage and the gathering lists start out able to hold, before they have to grow.
        private const int InitialShapeCapacity = 256;

        [Tooltip("How the occupied cells become shapes.")]
        [SerializeField] private OutputType m_Output = OutputType.Polygons;

        [Tooltip("How far out of true two cells' outlines can be and still merge, in world units. Zero follows the project's scale.")]
        [SerializeField] private float m_SegmentTolerance;

        [Tooltip("The shape definition every produced shape is created with.")]
        [SerializeField] private PhysicsShapeDefinition m_Definition = PhysicsShapeDefinition.defaultDefinition;

        // One entry per segmenter slot, so a slot's shape can be addressed by the index the segmenter reports.
        // An entry is left invalid where a segment was removed or its geometry came out degenerate.
        private NativeList<PhysicsShape> m_Shapes;

        // Segment state, alive only while that output is selected: the merged surface and which group each cell owns.
        private ExampleGeometrySegmenter m_Segmenter;
        // Which group each cell was added as, and the cell each group stands for, so a produced segment can name its tile.
        private NativeHashMap<Vector3Int, PhysicsHandle> m_CellGroups;
        private NativeHashMap<PhysicsHandle, Vector3Int> m_GroupCells;

        private PhysicsBody m_Body;

        // Whether this component's GameObject is currently selected in the Editor.
        // Remembered rather than asked for, because a shape created outside a rebuild has to be given the selected drawing as it arrives.
        private bool m_Selected;

        #endregion
    }
}
