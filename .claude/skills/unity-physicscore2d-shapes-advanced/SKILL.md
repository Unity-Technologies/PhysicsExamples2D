---
name: unity-physicscore2d-shapes-advanced
description: Advanced shape features including chains, compounds, rounded polygons, ellipses, and runtime geometry modification
---

# Unity PhysicsCore2D Advanced Shapes Expert

You are now acting as a Unity PhysicsCore2D advanced shapes expert, specialized in complex shape types and runtime geometry manipulation.

## Overview

Beyond basic circle and polygon shapes, PhysicsCore2D supports advanced shape features:
- **Chain shapes** - Connected line segments for terrain and edges
- **Compound shapes** - Multiple shapes on a single body
- **Rounded polygons** - Polygons with beveled corners
- **Ellipses** - Represented as polygons with many sides
- **Runtime modification** - Changing shape geometry dynamically
- **Geometry islands** - Disconnected shape groups

## Repository Examples

Reference examples from the PhysicsExamples2D repository:
- **ChainShape** - https://github.com/Unity-Technologies/PhysicsExamples2D/blob/master/Sandbox/Assets/Examples/ChainShape.cs
- **Compound** - https://github.com/Unity-Technologies/PhysicsExamples2D/blob/master/Sandbox/Assets/Examples/Compound.cs
- **RoundedPolygons** - https://github.com/Unity-Technologies/PhysicsExamples2D/blob/master/Sandbox/Assets/Examples/RoundedPolygons.cs
- **EllipsePolygons** - https://github.com/Unity-Technologies/PhysicsExamples2D/blob/master/Sandbox/Assets/Examples/EllipsePolygons.cs
- **ModifyGeometry** - https://github.com/Unity-Technologies/PhysicsExamples2D/blob/master/Sandbox/Assets/Examples/ModifyGeometry.cs
- **GeometryIslands** - https://github.com/Unity-Technologies/PhysicsExamples2D/blob/master/Sandbox/Assets/Examples/GeometryIslands.cs

## Advanced Shape Types

### Chain Shapes
Chain shapes connect multiple vertices to form edges:
- Ideal for terrain, platforms, and level boundaries
- Can be open (line) or closed (loop)
- More efficient than multiple box colliders
- Support ghost vertices to prevent snagging

**Use cases:**
- Platform edges and terrain
- Track boundaries
- Rope and cable physics
- Level geometry

#### Building chains: `PhysicsChain` vs standalone `ChainSegmentGeometry`

There are **two ways** to put a connected chain of edges into the world. Pick deliberately — they have different ownership and mutation rules.

**1. `PhysicsChain.Create(body, ChainGeometry, PhysicsChainDefinition)` — owned chain.**
The chain owns its `ShapeType.ChainSegment` shapes; ghost vertices are stitched automatically from the vertex list and `isLoop`. Individual segments **cannot** be destroyed via `PhysicsShape.Destroy` — only via `PhysicsChain.Destroy` (which tears down all segments together). Use this when the chain is one logical entity (a piece of terrain, a track, a closed loop).

**2. `ChainSegmentGeometry.CreateSegments(vertices, transform, isLoop, allocator)` + `PhysicsShape.CreateShape(body, ChainSegmentGeometry, ...)` — unowned segments.**
`CreateSegments` returns a `NativeArray<ChainSegmentGeometry>` with ghosts already stitched from the vertex list (same rules as `PhysicsChain` — `isLoop` controls how the first/last segments' ghosts are filled). You then create one `PhysicsShape` per geometry yourself (or use `CreateShapeBatch`). The resulting shapes are **regular** `ShapeType.ChainSegment` shapes that you own — they can be destroyed individually with `PhysicsShape.Destroy`, mixed with segments built by hand, and renamed/replaced one-at-a-time without disturbing siblings.

The standalone path is what you want when:
- You're stitching segments from multiple sources (multiple polygons sharing a boundary, dynamic insert/remove)
- You want per-segment lifecycle (delete one segment without rebuilding the whole chain)
- You're computing ghosts yourself (e.g. radial-match across many independent polygons) and just need a convenient batch builder for the simple linear case

#### Mutating chain segments in place (Unity 6000.5.0b9+)

Two new mutation paths avoid destroy/recreate:

- **`PhysicsShape.chainSegmentGeometry { get; set; }`** — assign a new `ChainSegmentGeometry` to an existing chain-segment shape to update its endpoints and ghost vertices. Wakes the body. The shape's contact state is preserved (no destroy/recreate cost). Works on shapes you created via the standalone path **and** segments owned by a `PhysicsChain`.
- **`PhysicsChain.UpdateVertices(ReadOnlySpan<Vector2> vertices, bool isLoop)`** — bulk update of every segment in an owned chain. **Vertex count and `isLoop` must match the original** or you get a warning. Recalculates contacts; can produce overlaps/tunnelling if the new shape moves far from the old, so use carefully on dynamic/kinematic bodies.

For mass per-segment edits across a non-`PhysicsChain` set, prefer setting `chainSegmentGeometry` on each shape over destroying and recreating — it skips contact-graph teardown and AABB-tree churn.

### Compound Shapes
Multiple shapes attached to a single body:
- Share the same PhysicsBody and move together
- Each shape can have different materials
- More efficient than separate bodies with joints
- Useful for complex collision geometry

**Use cases:**
- Character collision (capsule body + ground sensor)
- Vehicles (body + wheels)
- Complex objects with varied materials
- Irregular collision shapes

### Rounded Polygons
Polygons with beveled or rounded corners:
- Smoother collisions than sharp corners
- Better stability when stacking
- Adjustable corner radius
- More realistic for many objects

### Ellipses as Polygons
Ellipses are approximated using many-sided polygons:
- Higher vertex count = smoother shape
- Balance between accuracy and performance
- Can be stretched or compressed
- Useful for oval objects

## Runtime Geometry Modification

Shapes can be modified at runtime:
- Change polygon vertices
- Adjust circle radius
- Update shape properties
- Rebuild compound shapes

**Important considerations:**
- Modifying geometry is more expensive than creating new shapes
- May affect physics stability temporarily
- Call appropriate update methods after modification
- Consider recreating shapes for major changes

### In-place `PolygonGeometry` mutation

For polygons specifically, prefer the **manual write + `Validate()`** pattern over the static `PolygonGeometry.Create(span, radius)` factory when:
- Input may be degenerate (collinear / coincident vertices) — `Create` logs a `verticesHullIsValid` error from inside the engine *before* it returns; checking `poly.isValid` after the fact does **not** suppress it. `Validate()` is silent and just sets `isValid = false`.
- You're updating an existing polygon's vertices and don't want to rebuild a span.
- You're inside a Burst job and want to skip the `stackalloc`/span ceremony.

```csharp
PolygonGeometry poly = default;            // or an existing polygon you're mutating
poly.count = 3;                            // 3..PhysicsConstants.MaxPolygonVertices
ref var pv = ref poly.vertices;            // ref into the inline ShapeArray
pv[0] = new Vector2(x0, y0);
pv[1] = new Vector2(x1, y1);
pv[2] = new Vector2(x2, y2);
poly = poly.Validate();                    // single engine call: refreshes centroid/normals, sets isValid
if (poly.isValid) shape.polygonGeometry = poly;
```

Same single-engine-call cost as `Create`.

**⚠️ Validation discipline:** any edit to `vertices`, `count`, or `normals` invalidates the polygon's snapshot (`isValid`, `centroid`, `normals` go stale). You **must** call `poly = poly.Validate()` and check `isValid` before assigning the polygon to a shape, passing it to a query, or storing it in a buffer that a later reader will trust. The one exception is `radius` — it's a Minkowski offset that doesn't affect hull validity or centroid, so it's safe to edit without re-validating. Capture `Validate()`'s return value; the receiver struct is **not** mutated in place.

See the `PolygonGeometry` section in `unity-physicscore2d-geometry-api` for the full edit-vs-validate table and decision rules.

## Geometry Islands

Disconnected groups of shapes:
- Multiple separate collision regions on one body
- Each island can interact independently
- Useful for complex objects
- More advanced use case

## Best Practices

- Use chain shapes for static terrain and boundaries
- Prefer compound shapes over multiple joined bodies
- Round sharp corners to improve collision stability
- Approximate ellipses with 16-32 vertices for good balance
- Cache and reuse geometry when possible
- Test performance when modifying geometry at runtime
- Use appropriate shape types for each use case

## Related Skills

When users need information about:
- **Basic geometry types** - Use unity-physicscore2d-geometry
- **Shape materials** - Use unity-physicscore2d-materials
- **Collision filtering** - Use unity-physicscore2d-filtering
- **Geometry composition** - Use unity-physicscore2d-composer (if available)

## Worked Examples

> All examples below assume the standard PhysicsCore2D `OnEnable`/`OnDisable` lifecycle. See the umbrella skill `unity-physicscore2d`, section "Creating and Destroy Physics Objects", for the canonical lifecycle pattern.

- [examples/CompoundShape.cs](examples/CompoundShape.cs) — multiple PhysicsShape primitives attached to a single dynamic PhysicsBody (tables + spaceships); `body.GetAABB()` for combined bounds, `IntrudeShape` to add shapes at runtime.
- [examples/ChainShape.cs](examples/ChainShape.cs) — ~20-point ChainGeometry ground for terrain; objects spawn upper-left and slide along; toggle `FastCollisionsAllowed` to compare CCD vs default.
- [examples/RoundedPolygons.cs](examples/RoundedPolygons.cs) — grid of dynamic random polygons each built with non-zero `radius` for rounded edges (Minkowski offset); inline random-convex-polygon helper.
- [examples/ModifyGeometry.cs](examples/ModifyGeometry.cs) — runtime geometry mutation via type-specific setter properties (`shape.circleGeometry`, `shape.polygonGeometry`, etc.) and shape destroy/recreate when the type changes.
- [examples/GeometryIslands.cs](examples/GeometryIslands.cs) — fragment a tall column then walk `unbrokenGeometryIslands` to build per-island bodies, choosing static vs dynamic based on virtual-ground intersection.
