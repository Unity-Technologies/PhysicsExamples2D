using System.Collections.Generic;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Unity.U2D.Physics.Extras.Tests
{
    // Covers PhysicsAreaTilemap turning a Tilemap's occupied cells into shapes on the body of its owning pose.
    // The focus is the two output modes and what a tile change costs: Polygons emits per-cell polygons, Segments emits
    // the merged surface as chain segments and reuses its shapes rather than recreating them.
    // The component derives PhysicsPoseProvider rather than PhysicsArea, because a tilemap holds no geometry of its own,
    // so it owns its shapes and there is no area machinery under it to lean on.
    // Play-mode (runtime) tests so they run in a built player.
    public class PhysicsAreaTilemapTests : PhysicsExtrasTestBase
    {
        #region Polygons

        [Test]
        public void Polygons_EmitsShapesPerOccupiedCell()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            area.output = PhysicsAreaTilemap.OutputType.Polygons;

            // The test sprite's outline is one square, so each of the nine cells contributes one polygon.
            Assert.That(area.shapeCount, Is.EqualTo(9));
        }

        [Test]
        public void Polygons_StampsTheOriginatingCellOnEachShape()
        {
            var area = NewTilemapArea(out var map);
            map.SetTile(new Vector3Int(2, 3, 0), m_Tile);

            area.output = PhysicsAreaTilemap.OutputType.Polygons;

            foreach (var shape in area.GetShapes())
                Assert.That(shape.ownerUserData.vector3IntValue, Is.EqualTo(new Vector3Int(2, 3, 0)));
        }

        #endregion

        #region Segments

        [Test]
        public void Segments_EmitsOnlyThePerimeter()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            area.output = PhysicsAreaTilemap.OutputType.Segments;

            // Twelve cell edges bound a solid three by three block; the twelve interior joins all cancel. Collinear edges are not
            // joined into one segment, because each belongs to a different cell and a segment reports the cell it came from.
            Assert.That(area.shapeCount, Is.EqualTo(12));
        }

        [Test]
        public void Segments_KeepsTheWallsOfAnEnclosedGap()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            area.output = PhysicsAreaTilemap.OutputType.Segments;
            map.SetTile(new Vector3Int(1, 1, 0), null);

            // The outer perimeter plus the four walls facing into the gap, which are real collidable surfaces.
            Assert.That(area.shapeCount, Is.EqualTo(16));
        }

        [Test]
        public void Segments_FillingAGapRemovesItsWalls()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            area.output = PhysicsAreaTilemap.OutputType.Segments;
            map.SetTile(new Vector3Int(1, 1, 0), null);
            map.SetTile(new Vector3Int(1, 1, 0), m_Tile);

            // Back to a solid block, so the gap's walls are gone and only the perimeter is left.
            Assert.That(area.shapeCount, Is.EqualTo(12));
        }

        [Test]
        public void Segments_ErasingEveryTileLeavesNoShapes()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            area.output = PhysicsAreaTilemap.OutputType.Segments;

            for (var y = 0; y < 3; ++y)
            {
                for (var x = 0; x < 3; ++x)
                    map.SetTile(new Vector3Int(x, y, 0), null);
            }

            Assert.That(area.shapeCount, Is.Zero);
        }

        [Test]
        public void Segments_OneCellProducesAClosedLoop()
        {
            var area = NewTilemapArea(out var map);
            map.SetTile(Vector3Int.zero, m_Tile);

            area.output = PhysicsAreaTilemap.OutputType.Segments;

            var segments = new List<ChainSegmentGeometry>();
            foreach (var shape in area.GetShapes())
                segments.Add(shape.chainSegmentGeometry);

            Assert.That(segments.Count, Is.EqualTo(4));

            // Each segment's second ghost must be the far end of whichever segment carries on from it.
            foreach (var segment in segments)
            {
                var found = false;
                foreach (var candidate in segments)
                {
                    if (candidate.segment.point1 != segment.segment.point2 || candidate.segment.point2 != segment.ghost2)
                        continue;

                    found = true;
                    break;
                }

                Assert.That(found, Is.True, $"The segment {segment.segment.point1} to {segment.segment.point2} has no successor matching its second ghost {segment.ghost2}.");
            }
        }

        [Test]
        public void Segments_StampsTheOriginatingCellOnEachShape()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            area.output = PhysicsAreaTilemap.OutputType.Segments;

            // Every surviving surface came from exactly one cell's outline, so each shape names a cell inside the painted block.
            foreach (var shape in area.GetShapes())
            {
                var cell = shape.ownerUserData.vector3IntValue;

                Assert.That(cell.x, Is.InRange(0, 2));
                Assert.That(cell.y, Is.InRange(0, 2));
            }
        }

        [Test]
        public void Segments_SwitchingBackToPolygonsRestoresPerCellShapes()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            area.output = PhysicsAreaTilemap.OutputType.Segments;
            Assert.That(area.shapeCount, Is.EqualTo(12));

            area.output = PhysicsAreaTilemap.OutputType.Polygons;
            Assert.That(area.shapeCount, Is.EqualTo(9));
        }

        #endregion

        #region Shape reuse

        [Test]
        public void Segments_ErasingAnInteriorTileKeepsTheExistingShapes()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 5, 5);

            area.output = PhysicsAreaTilemap.OutputType.Segments;

            var before = ShapeSnapshot(area);
            map.SetTile(new Vector3Int(2, 2, 0), null);
            var after = ShapeSnapshot(area);

            // Only the four walls of the new gap are new work; every surface already standing keeps its own shape.
            Assert.That(after.Count, Is.EqualTo(before.Count + 4));
            Assert.That(Survivors(before, after), Is.EqualTo(before.Count), "An edit recreated shapes it should have left alone.");
        }

        [Test]
        public void Segments_ErasingACornerTileReusesEveryShape()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 5, 5);

            area.output = PhysicsAreaTilemap.OutputType.Segments;

            var before = ShapeSnapshot(area);
            map.SetTile(new Vector3Int(0, 0, 0), null);
            var after = ShapeSnapshot(area);

            // A corner loses two edges and gains two, so the count holds and the geometry is reassigned in place.
            Assert.That(after.Count, Is.EqualTo(before.Count));
            Assert.That(Survivors(before, after), Is.EqualTo(before.Count), "An edit recreated shapes when it could have reassigned their geometry.");
        }

        #endregion

        #region Segment tolerance

        [Test]
        public void SegmentTolerance_UnsetFollowsTheProjectScale()
        {
            var area = NewTilemapArea(out _);

            Assert.That(area.segmentTolerance, Is.LessThanOrEqualTo(0f), "The serialized default should defer to the project rather than name a number.");
            Assert.That(area.activeSegmentTolerance, Is.EqualTo(PhysicsGeometrySegmenter.defaultTolerance));
        }

        [Test]
        public void SegmentTolerance_SetOverridesTheProjectScale()
        {
            var area = NewTilemapArea(out _);

            area.segmentTolerance = 0.25f;

            Assert.That(area.activeSegmentTolerance, Is.EqualTo(0.25f));
        }

        [Test]
        public void SegmentTolerance_NegativeIsClampedAway()
        {
            var area = NewTilemapArea(out _);

            area.segmentTolerance = -5f;

            Assert.That(area.segmentTolerance, Is.Zero);
            Assert.That(area.activeSegmentTolerance, Is.EqualTo(PhysicsGeometrySegmenter.defaultTolerance));
        }

        [Test]
        public void SegmentTolerance_BelowLinearSlopIsRaisedToIt()
        {
            var area = NewTilemapArea(out _);

            // Chain segments need their vertices further apart than one linear slop, so anything smaller could never merge.
            area.segmentTolerance = PhysicsWorld.linearSlop * 0.1f;

            Assert.That(area.segmentTolerance, Is.LessThan(PhysicsWorld.linearSlop), "The authored value is kept as typed.");
            Assert.That(area.activeSegmentTolerance, Is.EqualTo(PhysicsWorld.linearSlop));
        }

        [Test]
        public void SegmentTolerance_BelowTheGapLeavesCellsUnmerged()
        {
            // Cells whose outlines fall short of each other by more than the tolerance are separate pieces of geometry,
            // so every edge of every cell survives.
            var area = NewTilemapArea(out var map, cellFraction: 0.9f);
            Fill(map, 2, 1);

            area.segmentTolerance = 0.01f;
            area.output = PhysicsAreaTilemap.OutputType.Segments;

            Assert.That(area.shapeCount, Is.EqualTo(8), "A gap wider than the tolerance should stop the cells merging.");
        }

        [Test]
        public void SegmentTolerance_AboveTheGapMergesCells()
        {
            // The same map with a tolerance wider than the gap: the facing edges are treated as coincident and cancel.
            var area = NewTilemapArea(out var map, cellFraction: 0.9f);
            Fill(map, 2, 1);

            area.segmentTolerance = 0.2f;
            area.output = PhysicsAreaTilemap.OutputType.Segments;

            Assert.That(area.shapeCount, Is.EqualTo(6), "A tolerance wider than the gap should merge the cells.");
        }

        [Test]
        public void SegmentTolerance_ChangingItRebuilds()
        {
            var area = NewTilemapArea(out var map, cellFraction: 0.9f);
            Fill(map, 2, 1);

            area.output = PhysicsAreaTilemap.OutputType.Segments;
            area.segmentTolerance = 0.01f;
            Assert.That(area.shapeCount, Is.EqualTo(8));

            // The tolerance is fixed for a segmenter's lifetime, so this has to start a fresh one rather than being ignored.
            area.segmentTolerance = 0.2f;
            Assert.That(area.shapeCount, Is.EqualTo(6));
        }

        #endregion

        #region Lifecycle

        [Test]
        public void Disabling_DestroysEveryShape()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            area.output = PhysicsAreaTilemap.OutputType.Segments;
            var shapes = ShapeSnapshot(area);

            area.enabled = false;

            Assert.That(area.shapeCount, Is.Zero);
            foreach (var shape in shapes)
                Assert.That(shape.isValid, Is.False, "A shape outlived the component that owned it.");
        }

        [Test]
        public void Disabling_WithPolygonsOnlyNeverAllocatesTheSegmenter()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            // Default output is Polygons, so the segmenter and its cell maps are never created.
            Assert.DoesNotThrow(() => area.enabled = false, "Releasing a segmenter that was never allocated should be a no-op, not a Dispose on an uncreated map.");

            Assert.That(area.shapeCount, Is.Zero);
        }

        [Test]
        public void ReEnabling_RebuildsFromTheCurrentTiles()
        {
            var area = NewTilemapArea(out var map);
            Fill(map, 3, 3);

            area.output = PhysicsAreaTilemap.OutputType.Segments;
            area.enabled = false;

            // Change the map while nothing is watching, so the rebuild has to read the tiles rather than trust anything cached.
            map.SetTile(new Vector3Int(1, 1, 0), null);
            area.enabled = true;

            // The perimeter plus the four walls of the gap that opened while nothing was watching.
            Assert.That(area.shapeCount, Is.EqualTo(16));
        }

        [Test]
        public void WithNoTiles_ProducesNoShapes()
        {
            var area = NewTilemapArea(out _);

            area.output = PhysicsAreaTilemap.OutputType.Segments;

            Assert.That(area.shapeCount, Is.Zero);
        }

        #endregion

        #region Helpers

        // A tilemap with a pose and a tilemap area on the same GameObject, so the body-relative transform is identity.
        // The cell fraction sets how much of its cell each tile's outline covers, so a value below one leaves a deliberate
        // gap between neighbours for the tolerance tests to bridge.
        PhysicsAreaTilemap NewTilemapArea(out Tilemap map, float cellFraction = 1f)
        {
            var gridObject = NewObject("Grid");
            gridObject.AddComponent<Grid>();

            var mapObject = NewObject("Tilemap");
            mapObject.transform.SetParent(gridObject.transform, worldPositionStays: false);

            map = mapObject.AddComponent<Tilemap>();
            mapObject.AddComponent<PhysicsPose>();

            m_Tile = Track(ScriptableObject.CreateInstance<Tile>());
            m_Tile.sprite = NewCellSprite(cellFraction);

            return mapObject.AddComponent<PhysicsAreaTilemap>();
        }

        // A sprite whose physics outline covers the given fraction of its cell, centered.
        // At one it fills the cell, so neighbouring cells share an edge exactly and their joins can cancel. The base class
        // helper always insets its outline, which leaves a gap between cells and nothing to merge.
        // Outline points are in pixel space relative to the sprite rect, stored as (pixel - pivotInPixels) / pixelsPerUnit,
        // so with a 32 by 32 rect, a centered pivot, and 32 pixels per unit, the full rect maps to a unit square.
        Sprite NewCellSprite(float cellFraction)
        {
            const float Size = 32f;

            var texture = Track(new Texture2D((int)Size, (int)Size));
            var sprite = Track(Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), Size));

            var inset = Size * (1f - cellFraction) * 0.5f;
            var far = Size - inset;

            var outline = new[]
            {
                new Vector2(inset, inset), new Vector2(far, inset),
                new Vector2(far, far), new Vector2(inset, far)
            };

            sprite.OverridePhysicsOutline(new List<Vector2[]> { outline });

            // The cache keys on the Sprite, and a fresh sprite can reuse a just-destroyed one's native id, so clear any stale entry.
            sprite.InvalidatePhysicsGeometry();
            return sprite;
        }

        void Fill(Tilemap map, int columns, int rows)
        {
            for (var y = 0; y < rows; ++y)
            {
                for (var x = 0; x < columns; ++x)
                    map.SetTile(new Vector3Int(x, y, 0), m_Tile);
            }
        }

        static List<PhysicsShape> ShapeSnapshot(PhysicsAreaTilemap area)
        {
            var shapes = new List<PhysicsShape>();
            foreach (var shape in area.GetShapes())
                shapes.Add(shape);

            return shapes;
        }

        // How many of the shapes standing after an edit are the very same objects that stood before it.
        static int Survivors(List<PhysicsShape> before, List<PhysicsShape> after)
        {
            var kept = 0;
            foreach (var shape in after)
            {
                if (before.Contains(shape))
                    ++kept;
            }

            return kept;
        }

        #endregion

        #region Internal

        Tile m_Tile;

        #endregion
    }
}
