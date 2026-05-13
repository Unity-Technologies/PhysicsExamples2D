---
name: unity-physicscore2d-geometry-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for Geometry Primitives. Lists every type, property, field, method (with signatures, params, returns) for: CapsuleGeometry, ChainGeometry, ChainSegmentGeometry, CircleGeometry, PhysicsAABB, PhysicsPlane, PolygonGeometry, SegmentGeometry. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Geometry Primitives

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `CapsuleGeometry`, `ChainGeometry`, `ChainSegmentGeometry`, `CircleGeometry`, `PhysicsAABB`, `PhysicsPlane`, `PolygonGeometry`, `SegmentGeometry`.

## CapsuleGeometry

> The geometry of a closed capsule which can be viewed as two semi-circles connected by a rectangle. See PhysicsBody.CreateShape.

**Full name:** `Unity.U2D.Physics.CapsuleGeometry`  
**Docs:** [Unity.U2D.Physics.CapsuleGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html)

### Fields

| Name | Summary |
|------|---------|
| `defaultGeometry` | Get the default Capsule. |

### Properties

| Name | Summary |
|------|---------|
| `center1` | Local center of the first semi-circle. |
| `center2` | Local center of the second semi-circle. |
| `isValid` | Check if the geometry is valid or not. |
| `radius` | The radius of the semi-circles. |

### Methods

#### `new()`

Create a default Capsule. See CapsuleGeometry._defaultGeometry.

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

Inverse-Transform the geometry. The maximum (minimum in the inverse) absolute value component from the scale will be used to scale the CapsuleGeometry._radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The inverse-transformed geometry.

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

**Returns:** The created polygon geometries. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `Transform(PhysicsTransform)`

Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The transformed geometry.

#### `Transform(Matrix4x4, bool)`

Transform the geometry. The maximum absolute value component from the scale will be used to scale the CapsuleGeometry._radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The transformed geometry.

#### `Validate()`

Get a validated version of the geometry, if possible.

**Returns:** A validated copy of the geometry with an updated length and radius if required. See CapsuleGeometry._isValid.

## ChainGeometry

> The geometry of a chain of ChainSegment.

**Full name:** `Unity.U2D.Physics.ChainGeometry`  
**Docs:** [Unity.U2D.Physics.ChainGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.ChainGeometry.html)

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

#### `new(ReadOnlySpan<Vector2>)`

Create the geometry of a chain using the specified vertices.

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
**Docs:** [Unity.U2D.Physics.ChainSegmentGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html)

### Fields

| Name | Summary |
|------|---------|
| `defaultGeometry` | Get the default Chain Segment. |

### Properties

| Name | Summary |
|------|---------|
| `ghost1` | The tail ghost vertex |
| `ghost2` | The head ghost vertex |
| `isValid` | Check if the geometry is valid or not. |
| `segment` | The Segment. |

### Methods

#### `new()`

Create a default ChainSegment. See ChainSegmentGeometry._defaultGeometry.

#### `new(SegmentGeometry, Vector2, Vector2)`

Create a default ChainSegment.

**Params:**
- `segmentGeometry` — The segment geometry.
- `ghost1` — The 'ghost' vertex preceding SegmentGeometry._point1.
- `ghost2` — The 'ghost' vertex following SegmentGeometry._point2.

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

#### `CreateShapeProxy()`

Create a shape proxy from the geometry.

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

## CircleGeometry

> The geometry of a closed circle. See PhysicsBody.CreateShape.

**Full name:** `Unity.U2D.Physics.CircleGeometry`  
**Docs:** [Unity.U2D.Physics.CircleGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html)

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

Create a default Circle. See CircleGeometry._defaultGeometry.

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

Inverse-Transform the geometry. The maximum (minimum in the inverse) absolute value component from the scale will be used to scale the CircleGeometry._radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The inverse-transformed geometry.

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

**Returns:** The created polygon geometries. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `Transform(PhysicsTransform)`

Transform the geometry.

**Params:**
- `transform` — The geometry to transform with.

**Returns:** The transformed geometry.

#### `Transform(Matrix4x4, bool)`

Transform the geometry. The maximum absolute value component from the scale will be used to scale the CircleGeometry._radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The transformed geometry.

## PhysicsAABB

> Represents a 2D axis-aligned bounding-box.

**Full name:** `Unity.U2D.Physics.PhysicsAABB`  
**Docs:** [Unity.U2D.Physics.PhysicsAABB](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsAABB.html)

### Properties

| Name | Summary |
|------|---------|
| `center` | Get the center of the AABB. |
| `extents` | Get the extents (half size) of the AABB. |
| `isValid` | Check if the AABB is valid. To be valid, PhysicsAABB._upperBound should be equal to or above PhysicsAABB._lowerBound. |
| `lowerBound` | The lower-left bounding vertex. This should be equal to or lower than PhysicsAABB._upperBound. |
| `normalized` | Get a new normalized copy of the AABB ensuring that PhysicsAABB._lowerBound is lower than or equal to PhysicsAABB._upperBound. |
| `perimeter` | Get the surface area (perimeter length) of the AABB. |
| `upperBound` | The upper-right bounding vertex. This should be equal to or above PhysicsAABB._lowerBound. |

### Methods

#### `new(Vector2, Vector2)`

Create an axis-aligned bounding-box with the specified bounds.

**Params:**
- `lowerBound` — The lower-left bounding vertex. This should be equal to or lower than .
- `upperBound` — The upper-right bounding vertex. This should be equal to or above .

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

#### `Normalized()`

Normalize the AABB ensuring that PhysicsAABB._lowerBound is lower than or equal to PhysicsAABB._upperBound.

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

#### `Union(PhysicsAABB)`

Create a union of the specified AABB and this AABB where resulting AABB completely encapsulates both AABB.

**Params:**
- `aabb` — The AABB to create a union with.

**Returns:** The results of the union.

## PhysicsPlane

> Represents a 2D plane.

**Full name:** `Unity.U2D.Physics.PhysicsPlane`  
**Docs:** [Unity.U2D.Physics.PhysicsPlane](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsPlane.html)

### Fields

| Name | Summary |
|------|---------|
| `normal` | The plane normal. This must be normalized for the plane be valid. |
| `offset` | The plane offset. |

### Properties

| Name | Summary |
|------|---------|
| `isValid` | Check if the plane is valid. To be valid, the PhysicsPlane._normal must be normalized. |

### Methods

#### `GetSeparation(Vector2)`

Get the signed separation of a point from a plane.

**Params:**
- `point` — The point to check the separation from the plane.

**Returns:** The signed separation of the point from the plan.

## PolygonGeometry

> The geometry of a closed convex polygon. The geometry has a fixed maximum number of vertices as defined by the constant PhysicsConstants.MaxPolygonVertices. Polygon regions that require a larger quantity of vertices or are concave are defined by multiple polygon geometry using the PhysicsComposer or the PolygonGeometry.CreatePolygons utility. See PhysicsBody.CreateShape.

**Full name:** `Unity.U2D.Physics.PolygonGeometry`  
**Docs:** [Unity.U2D.Physics.PolygonGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html)

### Fields

| Name | Summary |
|------|---------|
| `defaultGeometry` | Get the default Polygon. |
| `normals` | The geometry normal stored in a PhysicsShape.ShapeArray. |
| `vertices` | The geometry vertices stored in a PhysicsShape.ShapeArray. |

### Properties

| Name | Summary |
|------|---------|
| `centroid` | The centroid of the polygon. |
| `count` | The number of polygon vertices. |
| `isValid` | Check if the geometry is valid or not. |
| `radius` | The external radius for rounded polygons. |

### Methods

#### `new()`

Create a default Polygon. See PolygonGeometry._defaultGeometry.

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

#### `CreatePolygons(ReadOnlySpan<Vector2>, PhysicsTransform, Vector2, Unity.Collections.Allocator)`

Create multiple PolygonGeometry from a set of vertices. The vertices are assumed to produce a closed loop but can describe a concave shape if required. There must be at least 3 vertices. A limit is imposed on small vertex distances so it is recommended that scaling is applied here rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `vertices` — The vertices to create the polygons from..
- `transform` — The transform used to specify where the geometry is positioned.
- `vertexScale` — The scaling to be applied to the vertices.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created polygon geometries. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeProxy()`

Create a shape proxy from the geometry.

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

Inverse-Transform the geometry. The maximum (minimum in the inverse) absolute value component from the scale will be used to scale the PolygonGeometry._radius.

**Params:**
- `transform` — The transform to be used on the geometry.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The inverse-transformed geometry.

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

Transform the specified geometry. The maximum absolute value component from the scale will be used to scale the PolygonGeometry._radius.

**Params:**
- `transform` — The transform used to specify where the geometry is positioned.
- `scaleRadius` — Whether to scale the radius of the shape.

**Returns:** The transformed geometry.

#### `Validate()`

Get a validated version of the geometry, if possible.

**Returns:** A validated copy of the geometry with updated normals, centroid etc. Depending on the current geometry, the returned geometry may not be valid. See PolygonGeometry._isValid.

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
**Docs:** [Unity.U2D.Physics.SegmentGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.SegmentGeometry.html)

### Fields

| Name | Summary |
|------|---------|
| `defaultGeometry` | Get the default Segment. The line segment is directed towards the left. |

### Properties

| Name | Summary |
|------|---------|
| `backward` | Calculate the vector from SegmentGeometry._point2 to SegmentGeometry._point1. See SegmentGeometry._forward. |
| `forward` | Calculate the vector from SegmentGeometry._point1 to SegmentGeometry._point2. See SegmentGeometry._backward. |
| `isValid` | Check if the geometry is valid or not. |
| `midPoint` | The mid-point between SegmentGeometry._point1 and SegmentGeometry._point2. |
| `point1` | The first point. |
| `point2` | The second point. |

### Methods

#### `new()`

Create a default Segment. See SegmentGeometry._defaultGeometry.

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

#### `Scale(float)`

Scale the geometry along the SegmentGeometry._forward and SegmentGeometry._backward direction.

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

---

_Generated by `.claude/api-reference/_generate.py` from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
