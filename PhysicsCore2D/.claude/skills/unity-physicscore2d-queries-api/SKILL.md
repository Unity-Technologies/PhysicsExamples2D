---
name: unity-physicscore2d-queries-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for Queries. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsQuery. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Queries

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsQuery`.

## PhysicsQuery

> Various physics queries.

**Full name:** `Unity.U2D.Physics.PhysicsQuery`  
**Docs:** [Unity.U2D.Physics.PhysicsQuery](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.html)

### Methods

#### `CapsuleAndCapsule(CapsuleGeometry, PhysicsTransform, CapsuleGeometry, PhysicsTransform)`

Check the intersection between Capsule and Capsule geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `CapsuleAndCircle(CapsuleGeometry, PhysicsTransform, CircleGeometry, PhysicsTransform)`

Check the intersection between Capsule and Circle geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `CastShapes(PhysicsQuery.CastShapePairInput)`

Cast two shape proxies against each other. Initially touching shapes are treated as a miss. You should check for overlap first if initial overlap is required.

**Params:**
- `castShapePairInput` — The input describing the shape proxies and how they should move.

#### `ChainSegmentAndCapsule(ChainSegmentGeometry, PhysicsTransform, CapsuleGeometry, PhysicsTransform)`

Check the intersection between ChainSegment and Capsule geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `ChainSegmentAndCircle(ChainSegmentGeometry, PhysicsTransform, CircleGeometry, PhysicsTransform)`

Check the intersection between ChainSegment and Circle geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `ChainSegmentAndPolygon(ChainSegmentGeometry, PhysicsTransform, PolygonGeometry, PhysicsTransform)`

Check the intersection between ChainSegment and Polygon geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `CircleAndCircle(CircleGeometry, PhysicsTransform, CircleGeometry, PhysicsTransform)`

Check the intersection between Circle and Circle geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `PolygonAndCapsule(PolygonGeometry, PhysicsTransform, CapsuleGeometry, PhysicsTransform)`

Check the intersection between Polygon and Capsule geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `PolygonAndCircle(PolygonGeometry, PhysicsTransform, CircleGeometry, PhysicsTransform)`

Check the intersection between Polygon and Circle geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `PolygonAndPolygon(PolygonGeometry, PhysicsTransform, PolygonGeometry, PhysicsTransform)`

Check the intersection between Polygon and Polygon geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `SegmentAndCapsule(SegmentGeometry, PhysicsTransform, CapsuleGeometry, PhysicsTransform)`

Check the intersection between Segment and Capsule geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `SegmentAndCircle(SegmentGeometry, PhysicsTransform, CircleGeometry, PhysicsTransform)`

Check the intersection between Segment and Circle geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `SegmentAndPolygon(SegmentGeometry, PhysicsTransform, PolygonGeometry, PhysicsTransform)`

Check the intersection between Segment and Polygon geometries.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `SegmentDistance(SegmentGeometry, PhysicsTransform, SegmentGeometry, PhysicsTransform)`

Calculate the distance and closest points between two segments.

**Params:**
- `geometryA` — The first geometry to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `geometryB` — The second geometry to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The segment distance results.

#### `ShapeAndShape(PhysicsShape, PhysicsTransform, PhysicsShape, PhysicsTransform)`

Check the intersection between two PhysicsShape.

**Params:**
- `shapeA` — The first shape to use.
- `transformA` — The transform used to specify where the first geometry is positioned.
- `shapeB` — The second shape to use.
- `transformB` — The transform used to specify where the first geometry is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `ShapeDistance(PhysicsQuery.DistanceInput)`

Calculate the distance and closest points between two shape proxies.

**Params:**
- `distanceInput` — The input describing the shape proxies and how they should move.

**Returns:** The distance result.

#### `ShapeTimeOfImpact(PhysicsQuery.TimeOfImpactInput)`

Calculate the upper bound on time before two shape proxies penetrate i.e. the time-of-impact. Time is represented as a fraction in the range [0, maxInterval]. This uses a swept separating axis and may miss some intermediate, non-tunneling collisions.

**Params:**
- `toiInput` — The input describing the shapes and how they should move.

**Returns:** The time of impact result.

### Nested Types

- **CastRayInput** — Cast-Ray arguments used by CastRay queries.
- **CastResult** — Cast result when performing ray-cast or shape-cast queries against geometry.
- **CastShapeInput** — Cast shape arguments used by CastShape queries. To use existing shape geometries, use the helper constructors that allow creation via a specific shape geometry type.
- **CastShapePairInput** — Cast two shape proxies against each other. To use existing shape geometries, use the helper constructors that allow creation via a specific shape geometry type.
- **DistanceInput** — An input used for shape distance queries.
- **DistanceResult** — Distance result from shape distance queries.
- **QueryFilter** — A query filter is used to filter query results known as "hits". For example, you may want a ray-cast representing a projectile to hit players and the static environment but not debris.
- **SegmentDistanceResult** — Segment distance result from segment distance queries.
- **ShapeSweep** — Describes the motion of a shape for a time-of-impact calculation. The shape is defined with respect to the body origin.
- **TimeOfImpactInput** — The input for time-of-impact query.
- **TimeOfImpactResult** — Time-of-impact result from time-of-impact query.
- **WorldCastMode** — Controls what results are returned from a cast query against the PhysicsWorld.
- **WorldCastResult** — The results from performing any Cast query against the PhysicsWorld.
- **WorldMoverInput** — The world mover arguments used by the world mover.
- **WorldMoverResult** — The world mover result used by the world mover.
- **WorldOverlapResult** — The results from performing any Overlap query.

### CastRayInput

> Cast-Ray arguments used by CastRay queries.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.CastRayInput`  

#### Properties

| Name | Summary |
|------|---------|
| `maxFraction` | The maximum fraction of the translation to consider in the range (0 to 1), typically 1. |
| `origin` | The origin (start) of the ray. |
| `translation` | The translation relative to the PhysicsQuery.CastRayInput._origin of the ray. |

#### Methods

##### `new()`

Create a default Cast Ray input.

##### `new(Vector2, Vector2)`

Create a Cast-Ray with a default fraction of 1.

**Params:**
- `origin` — The origin (start) of the ray.
- `translation` — The translation relative to the PhysicsQuery.CastRayInput._origin of the ray.

##### `FromTo(Vector2, Vector2)`

Calculate a Cast-Ray given two positions.

**Params:**
- `from` — The position the ray starts.
- `to` — The position the ray ends.

### CastResult

> Cast result when performing ray-cast or shape-cast queries against geometry.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.CastResult`  

#### Properties

| Name | Summary |
|------|---------|
| `fraction` | The fraction of the input translation at collision in the range (0 to 1). |
| `isValid` | Check if the result is valid. |
| `iterations` | The number of iterations used in the calculation. |
| `normal` | The surface normal at the point of contact. In all non-overlapped cases, this will be a unit-normal. If there was an initial overlap, the normal will be zero (degenerate) along with the PhysicsQuery.CastResult._fraction being zero. |
| `point` | The point of contact. |

### CastShapeInput

> Cast shape arguments used by CastShape queries. To use existing shape geometries, use the helper constructors that allow creation via a specific shape geometry type.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.CastShapeInput`  

#### Properties

| Name | Summary |
|------|---------|
| `canEncroach` | Allow cast shape to encroach when initially touching. This only works if the radius is greater than zero. |
| `maxFraction` | The maximum fraction of the translation to consider in the range (0 to 1), typically 1. |
| `shapeProxy` | A proxy to the shape. |
| `translation` | Translation of the cast shape. |

#### Methods

##### `new()`

Create a default cast shape input.

##### `new(CircleGeometry, Vector2)`

Create a CastShapeInput the specified CircleGeometry. You should transform the geometry into the space you require.

**Params:**
- `circleGeometry` — The geometry to use.
- `translation` — The cast translation to use.

##### `new(CapsuleGeometry, Vector2)`

Create a CastShapeInput the specified CapsuleGeometry. You should transform the geometry into the space you require.

**Params:**
- `capsuleGeometry` — The geometry to use.
- `translation` — The cast translation to use.

##### `new(SegmentGeometry, Vector2)`

Create a CastShapeInput the specified SegmentGeometry. You should transform the geometry into the space you require.

**Params:**
- `segmentGeometry` — The geometry to use.
- `translation` — The cast translation to use.

##### `new(PolygonGeometry, Vector2)`

Create a CastShapeInput the specified PolygonGeometry. You should transform the geometry into the space you require.

**Params:**
- `polygonGeometry` — The geometry to use.
- `translation` — The cast translation to use.

##### `new(ChainSegmentGeometry, Vector2)`

Create a CastShapeInput the specified ChainSegmentGeometry. You should transform the geometry into the space you require.

**Params:**
- `chainSegmentGeometry` — The geometry to use.
- `translation` — The cast translation to use.

##### `FromShape(PhysicsShape, Vector2)`

Create a CastShapeInput the specified world shape. The geometry will automatically be translated into world-space.

**Params:**
- `shape` — The shape to use.
- `translation` — The cast translation to use.

### CastShapePairInput

> Cast two shape proxies against each other. To use existing shape geometries, use the helper constructors that allow creation via a specific shape geometry type.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.CastShapePairInput`  

#### Properties

| Name | Summary |
|------|---------|
| `canEncroach` | Allow cast shape proxies to encroach when initially touching. This only works if the radius is greater than zero. |
| `maxFraction` | The maximum fraction of the translation to consider in the range (0 to 1), typically 1. |
| `shapeProxyA` | A proxy to the shape A. |
| `shapeProxyB` | A proxy to the shape B. |
| `transformA` | The world transform for shape A |
| `transformB` | The world transform for shape B |
| `translationB` | Translation of the shape proxy B. |

### DistanceInput

> An input used for shape distance queries.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.DistanceInput`  

#### Properties

| Name | Summary |
|------|---------|
| `shapeProxyA` | The proxy for shape A. |
| `shapeProxyB` | The proxy for shape B. |
| `transformA` | The world transform for shape A |
| `transformB` | The world transform for shape B |
| `useRadii` | Should the proxy radius be considered? |

### DistanceResult

> Distance result from shape distance queries.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.DistanceResult`  

#### Properties

| Name | Summary |
|------|---------|
| `distance` | The distance between the points, zero if overlapped. |
| `iterations` | The number of iterations used in the calculation. |
| `normal` | A Normal vector that points from A to B. This is invalid if the distance is zero. |
| `pointA` | Closest point on shape A. |
| `pointB` | Closest point on shape B. |

### QueryFilter

> A query filter is used to filter query results known as "hits". For example, you may want a ray-cast representing a projectile to hit players and the static environment but not debris.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.QueryFilter`  

#### Fields

| Name | Summary |
|------|---------|
| `DefaultCategories` | The default categories used. |
| `defaultFilter` | Get the default query filter that hits everything. See PhysicsQuery.QueryFilter.Everything. |
| `DefaultHitCategories` | The default hit categories used. |
| `DefaultIgnoreFilter` | The default ignore filter used. |
| `Everything` | Get a query filter that is all categories, hits everything and doesn't ignore any objects. |

#### Properties

| Name | Summary |
|------|---------|
| `categories` | The categories this query is using. Usually you would only set one bit but multiple are allowed. |
| `hitCategories` | The categories this query will produce hits with. |
| `ignoreFilter` | The filter used to ignore items when filtering. |

#### Methods

##### `new()`

Create a default filter set as PhysicsQuery.QueryFilter._defaultFilter.

##### `new()`

Create a query filter.

**Params:**
- `categories` — A PhysicsMask defining the categories this query is using.
- `hitCategories` — A PhysicsMask defining the categories this query will produce hits with.
- `ignoreFilter` — A filter used to ignore items when filtering.

### SegmentDistanceResult

> Segment distance result from segment distance queries.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.SegmentDistanceResult`  

#### Properties

| Name | Summary |
|------|---------|
| `closest1` | The closest point on the first segment |
| `closest2` | The closest point on the second segment |
| `distance` | The distance between the closest points |
| `fraction1` | The barycentric coordinate on the first segment |
| `fraction2` | The barycentric coordinate on the second segment |

### ShapeSweep

> Describes the motion of a shape for a time-of-impact calculation. The shape is defined with respect to the body origin.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.ShapeSweep`  

#### Properties

| Name | Summary |
|------|---------|
| `localCOM` | The local center of mass. |
| `positionEnd` | The world center of mass end position. |
| `positionStart` | The world center of mass start position. |
| `rotationEnd` | The world rotation end. |
| `rotationStart` | The world rotation start. |

### TimeOfImpactInput

> The input for time-of-impact query.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.TimeOfImpactInput`  

#### Properties

| Name | Summary |
|------|---------|
| `maxFraction` | The sweep interval in the range [0, maxFraction]. |
| `shapeProxyA` | The proxy for shape A. |
| `shapeProxyB` | The proxy for shape B. |
| `shapeSweepA` | The movement of shape A. |
| `shapeSweepB` | The movement of shape B. |

### TimeOfImpactResult

> Time-of-impact result from time-of-impact query.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.TimeOfImpactResult`  

#### Properties

| Name | Summary |
|------|---------|
| `fraction` | The sweep time of the collision interval in the range [0, maxFraction]. |
| `impactState` | The impact state. |
| `normal` | The surface normal at the point of contact. |
| `point` | The point of contact. |

#### Nested Types

- **State** — Describes the time-of-impact state.

#### State

> Describes the time-of-impact state.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.TimeOfImpactResult.State`  

##### Fields

| Name | Summary |
|------|---------|
| `Failed` | The query failed to find a good result. The query ran out of iterations finding an impact. |
| `Hit` | An impact was detected. |
| `Overlapped` | The shapes were initially overlapped. |
| `Separated` | No impact was detected during the interval. |
| `Unknown` | The query encountered an error and returned an unknown result. This should not happen unless a serious issue was encountered. |

### WorldCastMode

> Controls what results are returned from a cast query against the PhysicsWorld.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.WorldCastMode`  

#### Fields

| Name | Summary |
|------|---------|
| `All` | Return all the hits. |
| `AllSorted` | Return all the hits but also sort by ascending distance (closest first). |
| `Closest` | Return only the closest hit. |

### WorldCastResult

> The results from performing any Cast query against the PhysicsWorld.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.WorldCastResult`  

#### Properties

| Name | Summary |
|------|---------|
| `fraction` | The fraction of the query cast distance the shape would move to the point of detection, in the range [0, 1]. |
| `isValid` | Check if the result is valid. |
| `normal` | The surface normal at the point of contact. In all non-overlapped cases, this will be a unit-normal. If there was an initial overlap, the normal will be zero (degenerate) along with the PhysicsQuery.WorldCastResult._fraction being zero and PhysicsQuery.WorldCastResult._point being an arbitrary point in the overlapped region. See PhysicsQuery.WorldCastResult._point. |
| `point` | The point of contact. |
| `shape` | The shape that was detected by the cast. |

### WorldMoverInput

> The world mover arguments used by the world mover.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.WorldMoverInput`  

#### Properties

| Name | Summary |
|------|---------|
| `castFilter` | The filter to use for checking casts. The advantage of a separate filter to PhysicsQuery.WorldMoverInput._overlapFilter is that you can check for overlaps in a different way to what you can hit when moving. For instance, you may or may not want to check for other movers in they existing in the world when moving but you want to always check them for overlap initially. |
| `collisionResults` | Whether to return all the individual PhysicsShape.MoverCollision results for all iterations or not. All the collisions will be returned in the PhysicsQuery.WorldMoverResult results. |
| `defaultInput` | Create a default world mover input. |
| `geometry` | The mover geometry to use when checking for overlaps and casting. |
| `maxIterations` | Solving a movement is iterative and will continue until the maximum allowed iterations has been achieve, controlled by this value. The maximum allowed iterations will not always be used and solving will cease if the iteration movement falls below the square of the PhysicsQuery.WorldMoverInput._moveTolerance. |
| `moveTolerance` | Solving a movement will cease if the movement falls below the square of this value. By default, this value is extremely small. Too high a value will result in solving ceasing too quickly, too small will result in all allowed PhysicsQuery.WorldMoverInput._maxIterations being used. |
| `overlapFilter` | The filter to use for checking overlaps. |
| `targetPosition` | The position desired for the mover to achieve. This is typically calculated using the current velocity, any gravity required and time-integrated by the simulation time-step (delta-time). |
| `transform` | The transform used to transform the geometry i.e. the mover starting pose. |
| `velocity` | The velocity used to calculate the PhysicsQuery.WorldMoverInput._targetPosition. This is not used for movement but it will be returned, modified by any surfaces hit. This velocity can then be used in subsequent inputs for movement. |

#### Methods

##### `new()`

Create a default world mover input.

### WorldMoverResult

> The world mover result used by the world mover.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.WorldMoverResult`  

#### Properties

| Name | Summary |
|------|---------|
| `collisionResults` | All the individual PhysicsShape.MoverCollision results for all iterations. Multiple non-unique contacts for the same PhysicsShape may be returned due to iterations, overlapping and casting. This is only populated if PhysicsQuery.WorldMoverInput._collisionResults is true. |
| `transform` | The final transform the mover finished at. The transform rotation is always the same as the PhysicsQuery.WorldMoverInput._transform provided. |
| `velocity` | The final velocity the mover finished at. |

#### Methods

##### `Dispose()`

Dispose of any allocated memory for the collision results.

### WorldOverlapResult

> The results from performing any Overlap query.

**Full name:** `Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult`  

#### Properties

| Name | Summary |
|------|---------|
| `isValid` | Check if the result is valid. |
| `shape` | The shape that was detected by the overlap. |

---

_Generated by `.claude/api-reference/_generate.py` from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
