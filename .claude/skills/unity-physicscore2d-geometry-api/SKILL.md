---
name: unity-physicscore2d-geometry-api
description: Authoritative Unity 6000.7 PhysicsCore2D API reference for Geometry Primitives. Lists every type, property, field, method (with signatures, params, returns) for: CapsuleGeometry, ChainGeometry, ChainSegmentGeometry, CircleGeometry, PhysicsAABB, PhysicsPlane, PolygonGeometry, SegmentGeometry. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Geometry Primitives

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.7, so it should be treated as the source of truth for member names, signatures, and documentation strings.

Top-level types in this file: `CapsuleGeometry`, `ChainGeometry`, `ChainSegmentGeometry`, `CircleGeometry`, `PhysicsAABB`, `PhysicsPlane`, `PolygonGeometry`, `SegmentGeometry`.

## CapsuleGeometry

> The geometry of a closed capsule which can be viewed as two semi-circles connected by a rectangle. See PhysicsBody.CreateShape.

**Full name:** `Unity.U2D.Physics.CapsuleGeometry`

### Fields

| Name | Summary |
|------|---------|
| `defaultGeometry` | Get the default Capsule. |

### Properties

| Name | Summary |
|------|---------|
| `areEdgesValid` | Whether CapsuleGeometry.center1 and CapsuleGeometry.center2 are far enough apart to form a valid edge. |
| `center1` | Local center of the first semi-circle. |
| `center2` | Local center of the second semi-circle. |
| `isValid` | Check if the geometry is valid or not. |
| `radius` | The radius of the semi-circles. |

### Methods

#### `new()`

Create a default Capsule. See CapsuleGeometry.defaultGeometry.

#### `CalculateAABB(PhysicsTransform)`

Calculate the AABB of the geometry.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.

**Returns:** The bounds of the geometry.

#### `CalculateMassConfiguration(float)`

Calculate the mass configuration of the geometry.

**Params:**
- `density` — The density to use.

**Returns:** The calculated mass configuration.

#### `CastRay(PhysicsQuery.CastRayInput)`

Calculate if a world ray intersects the geometry. See PhysicsQuery.CastResult.

**Params:**
- `castRayInput` — The configuration of the ray to cast.

**Returns:** The results of the intersection test.

#### `CastShape(PhysicsQuery.CastShapeInput)`

Calculate if a cast shape intersects the geometry. Initially touching shapes are treated as a miss. You should check for overlap first if initial overlap is required. See PhysicsQuery.CastShapeInput and PhysicsQuery.CastResult.

**Params:**
- `input` — The cast shape input used to check for intersection.

**Returns:** The results of the intersection test.

#### `ClosestPoint(Vector2)`

Calculate the closest point on this geometry to the specified point.

**Params:**
- `point` — The point to check.

**Returns:** The closest point on the geometry to the specified point.

#### `Create(Vector2, Vector2, float)`

Create a Capsule.

**Params:**
- `center1` — The first local center of the capsule end.
- `center2` — The second local center of the capsule end.
- `radius` — The radius of the capsule.

**Returns:** The created geometry.

#### `CreateShapeProxy()`

Create a shape proxy from the geometry.

#### `CreateShapeProxy(PhysicsTransform)`

Create a shape proxy from the geometry, transformed by the specified transform.

**Params:**
- `transform` — The transform used to position the geometry.

#### `CreateShapeProxy(Matrix4x4, bool)`

Create a shape proxy from the geometry, transformed by the specified transform. The maximum absolute value component from the scale will be used to scale the radius when scaleRadius is true.

**Params:**
- `transform` — The transform used to position the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

#### `Intersect(PhysicsTransform, CircleGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, CapsuleGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, PolygonGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, SegmentGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `InverseTransform(PhysicsTransform)`

Inverse-Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Matrix4x4, bool)`

Inverse-Transform the geometry. The maximum (minimum in the inverse) absolute value component from the scale will be used to scale the CapsuleGeometry.radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Span<CapsuleGeometry>, PhysicsTransform)`

Inverse-Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.

#### `InverseTransform(Span<CapsuleGeometry>, Matrix4x4, bool)`

Inverse-Transform a batch of geometry in place. The minimum absolute value component from the inverted scale will be used to scale the CapsuleGeometry.radius.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.
- `scaleRadius` — Whether to scale the radius of the shape.

#### `operator implicit(CapsuleGeometry)`

#### `OverlapPoint(Vector2)`

Calculate if a point overlaps the geometry.

**Params:**
- `point` — The point to check.

**Returns:** If the point overlaps the geometry.

#### `ToPolygons(PhysicsTransform, float, Unity.Collections.Allocator)`

Creates multiple PolygonGeometry from the geometry. A limit is imposed on small vertex distances so it is recommended that the geometry is scaled appropriately rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.
- `curveStride` — The curve stride used when creating curves, in radians. Valid range is [PhysicsComposer.MinCurveStride, 1.0].
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created polygon geometry. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `Transform(PhysicsTransform)`

Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The transformed geometry.

#### `Transform(Matrix4x4, bool)`

Transform the geometry. The maximum absolute value component from the scale will be used to scale the CapsuleGeometry.radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The transformed geometry.

#### `Transform(Span<CapsuleGeometry>, PhysicsTransform)`

Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.

#### `Transform(Span<CapsuleGeometry>, Matrix4x4, bool)`

Transform a batch of geometry in place. The maximum absolute value component from the scale will be used to scale the CapsuleGeometry.radius.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.
- `scaleRadius` — Whether to scale the radius of the shape.

#### `Validate()`

Get a validated version of the geometry, if possible.

**Returns:** A validated copy of the geometry with an updated length and radius if required. See CapsuleGeometry.isValid.

## ChainGeometry

> The geometry of a chain of ChainSegment.

**Full name:** `Unity.U2D.Physics.ChainGeometry`

### Properties

| Name | Summary |
|------|---------|
| `isValid` | Check if the geometry is valid or not. |
| `vertices` | Get the geometry vertices. |

### Methods

#### `new(Unity.Collections.NativeArray<Vector2>)`

Create the geometry of a Chain using the specified vertices.

**Params:**
- `vertices` — The vertices that will create the ChainSegment shapes.

#### `CalculateAABB(PhysicsTransform)`

Calculate the AABB of the geometry.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.

**Returns:** The bounds of the geometry.

#### `CastRay(PhysicsQuery.CastRayInput, bool)`

Calculate if a world ray intersects the geometry. See PhysicsQuery.CastResult.

**Params:**
- `castRayInput` — The configuration of the ray to cast.
- `oneSided` — Whether to treat the segment as having one-sided collision. The "left" side collision is ignored.

**Returns:** The results of the intersection test.

#### `CastShape(PhysicsQuery.CastShapeInput)`

Calculate if a cast shape intersects the geometry. Initially touching shapes are treated as a miss. You should check for overlap first if initial overlap is required. See PhysicsQuery.CastShapeInput and PhysicsQuery.CastResult.

**Params:**
- `input` — The cast shape input used to check for intersection.

**Returns:** The results of the intersection test.

#### `ClosestPoint(Vector2)`

Calculate the closest point on this geometry to the specified point.

**Params:**
- `point` — The point to check.

**Returns:** The closest point on the geometry to the specified point.

## ChainSegmentGeometry

> The geometry of a chain line segment with one-sided collision which only collides on the "right" side. Several of these are generated for a chain, connected as ghost1 -> point1 -> point2 -> ghost2.

**Full name:** `Unity.U2D.Physics.ChainSegmentGeometry`

### Fields

| Name | Summary |
|------|---------|
| `defaultGeometry` | Get the default Chain Segment. |

### Properties

| Name | Summary |
|------|---------|
| `areEdgesValid` | Whether the two points defining ChainSegmentGeometry.segment are far enough apart to form a valid edge. |
| `ghost1` | The tail ghost vertex. A ghost vertex is used by the solver to define how a collision response should be handled when a contact with the vertex occurs. |
| `ghost2` | The head ghost vertex A ghost vertex is used by the solver to define how a collision response should be handled when a contact with the vertex occurs. |
| `isValid` | Check if the geometry is valid or not. |
| `segment` | The Segment. |

### Methods

#### `new()`

Create a default ChainSegment. See ChainSegmentGeometry.defaultGeometry.

#### `new(SegmentGeometry, Vector2, Vector2)`

Create a default ChainSegment.

**Params:**
- `segmentGeometry` — The segment geometry.
- `ghost1` — The 'ghost' vertex preceding SegmentGeometry.point1.
- `ghost2` — The 'ghost' vertex following SegmentGeometry.point2.

#### `CalculateAABB(PhysicsTransform)`

Calculate the AABB of the geometry.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.

**Returns:** The bounds of the geometry.

#### `CastRay(PhysicsQuery.CastRayInput, bool)`

Calculate if a world ray intersects the geometry. See PhysicsQuery.CastResult.

**Params:**
- `castRayInput` — The configuration of the ray to cast.
- `oneSided` — Whether to treat the segment as having one-sided collision. The "left" side collision is ignored.

**Returns:** The results of the intersection test.

#### `CastShape(PhysicsQuery.CastShapeInput)`

Calculate if a cast shape intersects the geometry. Initially touching shapes are treated as a miss. You should check for overlap first if initial overlap is required. See PhysicsQuery.CastShapeInput and PhysicsQuery.CastResult.

**Params:**
- `input` — The cast shape input used to check for intersection.

**Returns:** The results of the intersection test.

#### `ClosestPoint(Vector2)`

Calculate the closest point on this geometry to the specified point.

**Params:**
- `point` — The point to check.

**Returns:** The closest point on the geometry to the specified point.

#### `CreateSegments(ReadOnlySpan<Vector2>, PhysicsTransform, bool, Unity.Collections.Allocator)`

Create multiple ChainSegmentGeometry from a set of vertices. The rules for interpreting the specified vertices when creating segments is described in PhysicsChain.

**Params:**
- `vertices` — The vertices to create the ChainSegmentGeometry from.
- `transform` — The transform used to specify where the geometry is positioned.
- `isLoop` — Indicates a closed chain formed by connecting the first and last vertices specified. This changes how the vertices are interpreted.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created ChainSegment geometry. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeProxy()`

Create a shape proxy from the geometry.

#### `CreateShapeProxy(PhysicsTransform)`

Create a shape proxy from the geometry, transformed by the specified transform.

**Params:**
- `transform` — The transform used to position the geometry.

#### `CreateShapeProxy(Matrix4x4)`

Create a shape proxy from the geometry, transformed by the specified transform.

**Params:**
- `transform` — The transform used to position the geometry.

#### `InverseTransform(PhysicsTransform)`

Inverse-Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Matrix4x4)`

Inverse-Transform the geometry.

**Params:**
- `transform` — The transform to be used on the geometry.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Span<ChainSegmentGeometry>, PhysicsTransform)`

Inverse-Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.

#### `InverseTransform(Span<ChainSegmentGeometry>, Matrix4x4)`

Inverse-Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.

#### `operator implicit(ChainSegmentGeometry)`

#### `Transform(PhysicsTransform)`

Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The transformed geometry.

#### `Transform(Matrix4x4)`

Transform the geometry.

**Params:**
- `transform` — The transform to be used on the geometry.

**Returns:** The transformed geometry.

#### `Transform(Span<ChainSegmentGeometry>, PhysicsTransform)`

Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.

#### `Transform(Span<ChainSegmentGeometry>, Matrix4x4)`

Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.

#### `UpdateSegments(Span<ChainSegmentGeometry>, ReadOnlySpan<Vector2>, PhysicsTransform, bool)`

Update an existing batch of ChainSegmentGeometry from a set of vertices, writing the results into the specified span in place. This pairs with ChainSegmentGeometry.CreateSegments: it recalculates the same segment and ghost vertices, but reuses a caller-provided span rather than allocating a new one. The span length must equal the segment count the vertices produce, which is the vertex count when isLoop is true, otherwise the vertex count minus three.

**Params:**
- `segments` — The segment batch to update in place. Its length must equal the segment count the vertices produce.
- `vertices` — The vertices to recalculate the segments from.
- `transform` — The transform used to specify where the geometry is positioned.
- `isLoop` — Indicates a closed chain formed by connecting the first and last vertices specified. This changes how the vertices are interpreted.

## CircleGeometry

> The geometry of a closed circle. See PhysicsBody.CreateShape.

**Full name:** `Unity.U2D.Physics.CircleGeometry`

### Fields

| Name | Summary |
|------|---------|
| `defaultGeometry` | Get the default Circle. |

### Properties

| Name | Summary |
|------|---------|
| `center` | The local center. |
| `isValid` | Check if the geometry is valid or not. |
| `radius` | The radius. |

### Methods

#### `new()`

Create a default Circle. See CircleGeometry.defaultGeometry.

#### `CalculateAABB(PhysicsTransform)`

Calculate the AABB of the geometry.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.

**Returns:** The bounds of the geometry.

#### `CalculateMassConfiguration(float)`

Calculate the mass configuration of the geometry.

**Params:**
- `density` — The density to use.

**Returns:** The calculated mass configuration.

#### `CastRay(PhysicsQuery.CastRayInput)`

Calculate if a world ray intersects the geometry. See PhysicsQuery.CastResult.

**Params:**
- `castRayInput` — The configuration of the ray to cast.

**Returns:** The results of the intersection test.

#### `CastShape(PhysicsQuery.CastShapeInput)`

Calculate if a cast shape intersects the geometry. Initially touching shapes are treated as a miss. You should check for overlap first if initial overlap is required. See PhysicsQuery.CastShapeInput and PhysicsQuery.CastResult.

**Params:**
- `input` — The cast shape input used to check for intersection.

**Returns:** The results of the intersection test.

#### `ClosestPoint(Vector2)`

Calculate the closest point on this geometry to the specified point.

**Params:**
- `point` — The point to check.

**Returns:** The closest point on the geometry to the specified point.

#### `Create(float)`

Create Circle.

**Params:**
- `radius` — The radius to use.

**Returns:** The created geometry.

#### `Create(float, Vector2)`

Create a Circle.

**Params:**
- `radius` — The radius to use.
- `center` — The local center of the circle.

**Returns:** The created geometry.

#### `CreateShapeProxy()`

Create a shape proxy from the geometry.

#### `CreateShapeProxy(PhysicsTransform)`

Create a shape proxy from the geometry, transformed by the specified transform.

**Params:**
- `transform` — The transform used to position the geometry.

#### `CreateShapeProxy(Matrix4x4, bool)`

Create a shape proxy from the geometry, transformed by the specified transform. The maximum absolute value component from the scale will be used to scale the radius when scaleRadius is true.

**Params:**
- `transform` — The transform used to position the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

#### `Intersect(PhysicsTransform, CircleGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, CapsuleGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, PolygonGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, SegmentGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `InverseTransform(PhysicsTransform)`

Inverse-Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Matrix4x4, bool)`

Inverse-Transform the geometry. The maximum (minimum in the inverse) absolute value component from the scale will be used to scale the CircleGeometry.radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Span<CircleGeometry>, PhysicsTransform)`

Inverse-Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.

#### `InverseTransform(Span<CircleGeometry>, Matrix4x4, bool)`

Inverse-Transform a batch of geometry in place. The minimum absolute value component from the inverted scale will be used to scale the CircleGeometry.radius.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.
- `scaleRadius` — Whether to scale the radius of the shape.

#### `operator implicit(CircleGeometry)`

#### `OverlapPoint(Vector2)`

Calculate if a point overlaps the geometry.

**Params:**
- `point` — The point to check.

**Returns:** If the point overlaps the geometry.

#### `ToPolygons(PhysicsTransform, float, Unity.Collections.Allocator)`

Creates multiple PolygonGeometry from the geometry. A limit is imposed on small vertex distances so it is recommended that the geometry is scaled appropriately rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.
- `curveStride` — The curve stride used when creating curves, in radians. Valid range is [PhysicsComposer.MinCurveStride, 1.0].
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created polygon geometry. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `Transform(PhysicsTransform)`

Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The transformed geometry.

#### `Transform(Matrix4x4, bool)`

Transform the geometry. The maximum absolute value component from the scale will be used to scale the CircleGeometry.radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The transformed geometry.

#### `Transform(Span<CircleGeometry>, PhysicsTransform)`

Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.

#### `Transform(Span<CircleGeometry>, Matrix4x4, bool)`

Transform a batch of geometry in place. The maximum absolute value component from the scale will be used to scale the CircleGeometry.radius.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.
- `scaleRadius` — Whether to scale the radius of the shape.

## PhysicsAABB

> Represents a 2D axis-aligned bounding-box.

**Full name:** `Unity.U2D.Physics.PhysicsAABB`

### Properties

| Name | Summary |
|------|---------|
| `center` | Get the center of the AABB. |
| `extents` | Get the extents (half size) of the AABB. |
| `isValid` | Check if the AABB is valid. To be valid, PhysicsAABB.upperBound should be equal to or above PhysicsAABB.lowerBound. |
| `lowerBound` | The lower-left bounding vertex. This should be equal to or lower than PhysicsAABB.upperBound. |
| `normalized` | Get a new normalized copy of the AABB ensuring that PhysicsAABB.lowerBound is lower than or equal to PhysicsAABB.upperBound. |
| `perimeter` | Get the surface area (perimeter length) of the AABB. |
| `upperBound` | The upper-right bounding vertex. This should be equal to or above PhysicsAABB.lowerBound. |

### Methods

#### `new(Vector2, Vector2)`

Create an axis-aligned bounding-box with the specified bounds.

**Params:**
- `lowerBound` — The lower-left bounding vertex. This should be equal to or lower than upperBound.
- `upperBound` — The upper-right bounding vertex. This should be equal to or above lowerBound.

#### `new(Vector2)`

Create an axis-aligned bounding-box that encapsulates the specified point.

**Params:**
- `point` — The point which the AABB should encapsulate.

#### `CastRay(PhysicsQuery.CastRayInput)`

Perform a raycast against this AABB. Nothing will be detected if the ray starts inside the AABB. To check if the ray starts inside the AABB use PhysicsAABB.OverlapPoint.

**Params:**
- `castRayInput` — The configuration of the ray to cast.

**Returns:** The results of the intersection test.

#### `Contains(PhysicsAABB)`

Checks if the AABB contains (completely encapsulates) the specified AABB.

**Params:**
- `aabb` — The AABB to check being contained by this AABB.

**Returns:** True if the specified AABB is contained by this AABB. False if not.

#### `Normalize()`

Normalize the AABB ensuring that PhysicsAABB.lowerBound is lower than or equal to PhysicsAABB.upperBound.

#### `Overlap(PhysicsAABB)`

Check if the specified AABB overlaps this AABB.

**Params:**
- `aabb` — The AABB to check overlap with.

**Returns:** True if overlapped, false if not.

#### `Overlap(Vector2)`

Check if the specified point overlaps this AABB.

**Params:**
- `point` — The point to check overlap with.

**Returns:** True if overlapped, false if not.

#### `OverlapPoint(Vector2)`

Check if the specified point overlaps this AABB.

**Params:**
- `point` — The point to check for overlap.

**Returns:** True if the point overlaps, false if not.

#### `ToString()`

#### `Translate(Vector2)`

Create an AABB as a translated version of the current AABB.

**Params:**
- `translation` — The translation to use.

**Returns:** The translated AABB.

#### `Union(PhysicsAABB)`

Create a union of the specified AABB and this AABB where resulting AABB completely encapsulates both AABB.

**Params:**
- `aabb` — The AABB to create a union with.

**Returns:** The results of the union.

## PhysicsPlane

> Represents a 2D plane.

**Full name:** `Unity.U2D.Physics.PhysicsPlane`

### Fields

| Name | Summary |
|------|---------|
| `normal` | The plane normal. This must be normalized for the plane be valid. |
| `offset` | The plane offset. |

### Properties

| Name | Summary |
|------|---------|
| `isValid` | Check if the plane is valid. To be valid, the PhysicsPlane.normal must be normalized. |

### Methods

#### `GetSeparation(Vector2)`

Get the signed separation of a point from a plane.

**Params:**
- `point` — The point to check the separation from the plane.

**Returns:** The signed separation of the point from the plan.

#### `ToPositionAndNormal(Vector2, Vector2)`

Decompose the plane into a point lying on the plane and the plane's normal direction. The returned position is the foot of the perpendicular from the world origin to the plane, computed as normal * offset. This is the unique closest-to-origin point on the plane; any other point on the plane would describe the same plane equation but be non-deterministic. Useful for feeding a plane into APIs that take a (position, normal) pair such as PhysicsBody.BuoyancyInput.surfacePosition and PhysicsBody.BuoyancyInput.surfaceNormal.

**Params:**
- `position` — A point lying on the plane (the foot of the perpendicular from the origin).
- `normal` — The plane's outward normal, as stored.

#### `ToString()`

## PolygonGeometry

> The geometry of a closed convex polygon. The geometry has a fixed maximum number of vertices as defined by the constant PhysicsConstants.MaxPolygonVertices. Polygon regions that require a larger quantity of vertices or are concave are defined by multiple polygon geometry using the PhysicsComposer or the PolygonGeometry.CreatePolygons utility. See PhysicsBody.CreateShape.

**Full name:** `Unity.U2D.Physics.PolygonGeometry`

### Fields

| Name | Summary |
|------|---------|
| `defaultGeometry` | Get the default Polygon. |
| `normals` | The geometry normal stored in a PhysicsShape.ShapeArray. |
| `vertices` | The geometry vertices stored in a PhysicsShape.ShapeArray. |

### Properties

| Name | Summary |
|------|---------|
| `areEdgesValid` | Whether every edge of this polygon is far enough apart and no three consecutive vertices are collinear, matching the rules applied when building a polygon from vertices. |
| `centroid` | The centroid of the polygon. |
| `count` | The number of polygon vertices. |
| `isValid` | Check if the geometry is valid or not. |
| `radius` | The external radius for rounded polygons. |

### Methods

#### `new()`

Create a default Polygon. See PolygonGeometry.defaultGeometry.

#### `AsReadOnlySpan()`

Get the polygon vertices as a read-only span.

**Returns:** The read-only span representing the vertices in the geometry.

#### `AsSpan()`

Get the polygon vertices as a span.

**Returns:** The span representing the vertices in the geometry.

#### `CalculateAABB(PhysicsTransform)`

Calculate the AABB of the geometry.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.

**Returns:** The bounds of the geometry.

#### `CalculateMassConfiguration(float)`

Calculate the mass configuration of the geometry.

**Params:**
- `density` — The density to use.

**Returns:** The calculated mass configuration.

#### `CastRay(PhysicsQuery.CastRayInput)`

Calculate if a world ray intersects the geometry. See PhysicsQuery.CastResult.

**Params:**
- `castRayInput` — The configuration of the ray to cast.

**Returns:** The results of the intersection test.

#### `CastShape(PhysicsQuery.CastShapeInput)`

Calculate if a cast shape intersects the geometry. Initially touching shapes are treated as a miss. You should check for overlap first if initial overlap is required. See PhysicsQuery.CastShapeInput and PhysicsQuery.CastResult.

**Params:**
- `input` — The cast shape input used to check for intersection.

**Returns:** The results of the intersection test.

#### `ClosestPoint(Vector2)`

Calculate the closest point on this geometry to the specified point.

**Params:**
- `point` — The point to check.

**Returns:** The closest point on the geometry to the specified point.

#### `Create(ReadOnlySpan<Vector2>, float)`

Create a Polygon from the specified vertices. The number of vertices must be in the range 3 to PhysicsConstants.MaxPolygonVertices.

**Params:**
- `vertices` — The vertices to use.
- `radius` — The radius to use.

**Returns:** The created geometry.

#### `Create(ReadOnlySpan<Vector2>, float, PhysicsTransform)`

Create a Polygon from the specified vertices. The number of vertices must be in the range 3 to PhysicsConstants.MaxPolygonVertices.

**Params:**
- `vertices` — The vertices to use.
- `radius` — The radius to use.
- `transform` — The transform used to specify where the geometry is positioned.

**Returns:** The created geometry.

#### `Create(ReadOnlySpan<Vector2>, float, Matrix4x4)`

Create a Polygon from the specified vertices. The number of vertices must be in the range 3 to PhysicsConstants.MaxPolygonVertices.

**Params:**
- `vertices` — The vertices to use.
- `radius` — The radius to use.
- `transform` — The transform used to specify where the geometry is positioned.

**Returns:** The created geometry.

#### `Create(PolygonGeometry.ConvexHull, float)`

Create a Polygon from the specified convex hull.

**Params:**
- `convexHull` — The convex hull to create the polygon from.
- `radius` — The radius to use.

**Returns:** The created geometry.

#### `CreateBox(Vector2, float, bool)`

Create a Polygon as a four-sided box.

**Params:**
- `size` — The full size of the box.
- `radius` — The radius to use.
- `inscribe` — When true, the specified size will be inclusive of the specified radius. If true, a warning will be produced if the radius is greater-than or equal-to twice the specified size.

**Returns:** The created geometry.

#### `CreateBox(Vector2, float, PhysicsTransform, bool)`

Create a Polygon as a four-sided box.

**Params:**
- `size` — The full size of the box.
- `radius` — The radius to use.
- `transform` — The transform used to specify where the geometry is positioned.
- `inscribe` — When true, the specified size will be inclusive of the specified radius. If true, a warning will be produced if the radius is greater-than or equal-to twice the specified size.

**Returns:** The created geometry.

#### `CreatePolygons(ReadOnlySpan<Vector2>, PhysicsTransform, Unity.Collections.Allocator)`

Create multiple PolygonGeometry from a set of vertices. The vertices are assumed to produce a closed loop but can describe a concave shape if required. There must be at least 3 vertices. A limit is imposed on small vertex distances so be aware that this overload uses a vertex scale of Vector2.one so consider using the overload which allows you to increase this if required.

**Params:**
- `vertices` — The vertices to create the polygons from.
- `transform` — The transform used to specify where the geometry is positioned.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created polygon geometry. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreatePolygons(ReadOnlySpan<Vector2>, PhysicsTransform, Vector2, Unity.Collections.Allocator)`

Create multiple PolygonGeometry from a set of vertices. The vertices are assumed to produce a closed loop but can describe a concave shape if required. There must be at least 3 vertices. A limit is imposed on small vertex distances so it is recommended that scaling is applied here rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `vertices` — The vertices to create the polygons from.
- `transform` — The transform used to specify where the geometry is positioned.
- `vertexScale` — The scaling to be applied to the vertices.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created polygon geometry. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreatePolygons(ReadOnlySpan<Vector2>, PhysicsTransform, Vector2, float, bool, Unity.Collections.Allocator)`

Create multiple PolygonGeometry from a set of vertices. The vertices are assumed to produce a closed loop but can describe a concave shape if required. There must be at least 3 vertices. A limit is imposed on small vertex distances so it is recommended that scaling is applied here rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `vertices` — The vertices to create the polygons from.
- `transform` — The transform used to specify where the geometry is positioned.
- `vertexScale` — The scaling to be applied to the vertices.
- `radius` — The radius to apply to all generated polygons. Note that this will likely mean that the same polygon region defined by the vertices will not match.
- `useDelaunay` — Whether Delaunay tessellation will be used.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created polygon geometry. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeProxy()`

Create a shape proxy from the geometry.

#### `CreateShapeProxy(PhysicsTransform)`

Create a shape proxy from the geometry, transformed by the specified transform.

**Params:**
- `transform` — The transform used to position the geometry.

#### `CreateShapeProxy(Matrix4x4, bool)`

Create a shape proxy from the geometry, transformed by the specified transform. The maximum absolute value component from the scale will be used to scale the radius when scaleRadius is true.

**Params:**
- `transform` — The transform used to position the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

#### `DeleteVertex(PolygonGeometry, int)`

Delete a vertex from the geometry returning a new geometry with updated normals and centroid.

**Params:**
- `geometry` — The geometry to adjust.
- `index` — The vertex index to delete.

**Returns:** The new geometry with the deleted vertex.

#### `InsertVertex(PolygonGeometry, int, Vector2)`

Insert a vertex into the geometry returning a new geometry with updated normals and centroid.

**Params:**
- `geometry` — The geometry to adjust.
- `index` — The vertex index to insert at.
- `vertex` — The vertex to insert.

**Returns:** The new geometry with the inserted vertex.

#### `Intersect(PhysicsTransform, CircleGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, CapsuleGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, PolygonGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, SegmentGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `InverseTransform(PhysicsTransform)`

Inverse-Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Matrix4x4, bool)`

Inverse-Transform the geometry. The maximum (minimum in the inverse) absolute value component from the scale will be used to scale the PolygonGeometry.radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Span<PolygonGeometry>, PhysicsTransform)`

Inverse-Transform a batch of geometry in place. A transform that degenerates the geometry hull writes an invalid (zero-vertex) polygon in its place.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.

#### `InverseTransform(Span<PolygonGeometry>, Matrix4x4, bool)`

Inverse-Transform a batch of geometry in place. The minimum absolute value component from the inverted scale will be used to scale each PolygonGeometry.radius. A transform that degenerates the geometry hull writes an invalid (zero-vertex) polygon in its place.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.
- `scaleRadius` — Whether to scale the radius of the shape.

#### `operator implicit(PolygonGeometry)`

#### `OverlapPoint(Vector2)`

Calculate if a point overlaps the geometry.

**Params:**
- `point` — The point to check.

**Returns:** If the point overlaps the geometry.

#### `Transform(PhysicsTransform)`

Transform the specified geometry.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.

**Returns:** The transformed geometry.

#### `Transform(Matrix4x4, bool)`

Transform the specified geometry. The maximum absolute value component from the scale will be used to scale the PolygonGeometry.radius.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The transformed geometry.

#### `Transform(Span<PolygonGeometry>, PhysicsTransform)`

Transform a batch of geometry in place. A transform that degenerates the geometry hull writes an invalid (zero-vertex) polygon in its place.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.

#### `Transform(Span<PolygonGeometry>, Matrix4x4, bool)`

Transform a batch of geometry in place. The maximum absolute value component from the scale will be used to scale each PolygonGeometry.radius. A transform that degenerates the geometry hull writes an invalid (zero-vertex) polygon in its place.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.
- `scaleRadius` — Whether to scale the radius of the shape.

#### `Validate()`

Get a validated version of the geometry, if possible.

**Returns:** A validated copy of the geometry with updated normals, centroid etc. Depending on the current geometry, the returned geometry may not be valid. See PolygonGeometry.isValid.

### Nested Types

- **ConvexHull** — A simple convex hull. The hull is not validated by physics so cannot be used directly for shapes.

### ConvexHull

> A simple convex hull. The hull is not validated by physics so cannot be used directly for shapes.

**Full name:** `Unity.U2D.Physics.PolygonGeometry.ConvexHull`

#### Fields

| Name | Summary |
|------|---------|
| `vertices` | The geometry vertices stored in a PhysicsShape.ShapeArray. |

#### Properties

| Name | Summary |
|------|---------|
| `count` | The number of polygon vertices. |

#### Methods

##### `AsReadOnlySpan()`

Get the convex-hull vertices as a read-only span.

**Returns:** The read-only span representing the vertices in the geometry.

##### `AsSpan()`

Get the convex-hull vertices as a span.

**Returns:** The span representing the vertices in the geometry.

## SegmentGeometry

> The geometry of a line segment. See PhysicsBody.CreateShape.

**Full name:** `Unity.U2D.Physics.SegmentGeometry`

### Fields

| Name | Summary |
|------|---------|
| `defaultGeometry` | Get the default Segment. The line segment is directed towards the left. |

### Properties

| Name | Summary |
|------|---------|
| `areEdgesValid` | Whether SegmentGeometry.point1 and SegmentGeometry.point2 are far enough apart to form a valid edge. |
| `backward` | Calculate the vector from SegmentGeometry.point2 to SegmentGeometry.point1. See SegmentGeometry.forward. |
| `forward` | Calculate the vector from SegmentGeometry.point1 to SegmentGeometry.point2. See SegmentGeometry.backward. |
| `isValid` | Check if the geometry is valid or not. |
| `midPoint` | The mid-point between SegmentGeometry.point1 and SegmentGeometry.point2. |
| `point1` | The first point. |
| `point2` | The second point. |

### Methods

#### `new()`

Create a default Segment. See SegmentGeometry.defaultGeometry.

#### `CalculateAABB(PhysicsTransform)`

Calculate the AABB of the geometry.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.

**Returns:** The bounds of the geometry.

#### `CastRay(PhysicsQuery.CastRayInput, bool)`

Calculate if a world ray intersects the geometry. See PhysicsQuery.CastResult.

**Params:**
- `castRayInput` — The configuration of the ray to cast.
- `oneSided` — Whether to treat the segment as having one-sided collision. The "left" side collision is ignored.

**Returns:** The results of the intersection test.

#### `CastShape(PhysicsQuery.CastShapeInput)`

Calculate if a cast shape intersects the geometry. Initially touching shapes are treated as a miss. You should check for overlap first if initial overlap is required. See PhysicsQuery.CastShapeInput and PhysicsQuery.CastResult.

**Params:**
- `input` — The cast shape input used to check for intersection.

**Returns:** The results of the intersection test.

#### `ClosestPoint(Vector2)`

Calculate the closest point on this geometry to the specified point.

**Params:**
- `point` — The point to check.

**Returns:** The closest point on the geometry to the specified point.

#### `Create(Vector2, Vector2)`

Create a Segment.

**Params:**
- `point1` — The first local point.
- `point2` — The second local point.

**Returns:** The created geometry.

#### `CreateShapeProxy()`

Create a shape proxy from the geometry.

#### `CreateShapeProxy(PhysicsTransform)`

Create a shape proxy from the geometry, transformed by the specified transform.

**Params:**
- `transform` — The transform used to position the geometry.

#### `CreateShapeProxy(Matrix4x4)`

Create a shape proxy from the geometry, transformed by the specified transform.

**Params:**
- `transform` — The transform used to position the geometry.

#### `Intersect(PhysicsTransform, CircleGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, CapsuleGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, PolygonGeometry, PhysicsTransform)`

Check the intersection between this geometry and another.

**Params:**
- `transform` — The transform used to specify where this geometry is positioned.
- `otherGeometry` — The other geometry used to check intersection against.
- `otherTransform` — The transform used to specify where the other geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `InverseTransform(PhysicsTransform)`

Inverse-Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Matrix4x4)`

Inverse-Transform the geometry.

**Params:**
- `transform` — The transform to be used on the geometry.

**Returns:** The inverse-transformed geometry.

#### `InverseTransform(Span<SegmentGeometry>, PhysicsTransform)`

Inverse-Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.

#### `InverseTransform(Span<SegmentGeometry>, Matrix4x4)`

Inverse-Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to inverse-transform in place.
- `transform` — The transform to apply.

#### `operator implicit(SegmentGeometry)`

#### `Scale(float)`

Scale the geometry along the SegmentGeometry.forward and SegmentGeometry.backward direction.

**Params:**
- `scale` — —

#### `Transform(PhysicsTransform)`

Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The transformed geometry.

#### `Transform(Matrix4x4)`

Transform the geometry.

**Params:**
- `transform` — The transform to be used on the geometry.

**Returns:** The transformed geometry.

#### `Transform(Span<SegmentGeometry>, PhysicsTransform)`

Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.

#### `Transform(Span<SegmentGeometry>, Matrix4x4)`

Transform a batch of geometry in place.

**Params:**
- `geometry` — The geometry to transform in place.
- `transform` — The transform to apply.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
