# Physics Queries

The physics system provides spatial queries to inspect a `PhysicsWorld` and its `PhysicsShape` contents. All queries are methods on a `PhysicsWorld` instance.
All queries are read-only and thread-safe, so you can call them from any thread in parallel without concern. 

## Overlap Queries

Overlap queries check whether a primitive or geometry overlaps any `PhysicsShape` currently in the world and return the overlapping shapes. “Test” variants (e.g., `TestOverlapAABB`) return `true`/`false` if any overlap is found.

- `PhysicsWorld.OverlapAABB`: Find overlaps of a [PhysicsAABB](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsAABB.html).
- `PhysicsWorld.OverlapPoint`: Find overlaps of a single point.
- `PhysicsWorld.OverlapGeometry`: Find overlaps of the specified geometry.
- `PhysicsWorld.OverlapShape`: Find overlaps of the specified [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html).
- `PhysicsWorld.OverlapShapeProxy`: Find overlaps of a generic [PhysicsShape.ShapeProxy](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeProxy.html).
- `PhysicsWorld.TestOverlapAABB`: Check for any overlap of a [PhysicsAABB](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsAABB.html).
- `PhysicsWorld.TestOverlapPoint`: Check for any overlap of a single point.
- `PhysicsWorld.TestOverlapGeometry`: Check for any overlap of the specified geometry.
- `PhysicsWorld.TestOverlapShape`: Check for any overlap of the specified [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html).
- `PhysicsWorld.TestOverlapShapeProxy`: Check for any overlap of a generic [PhysicsShape.ShapeProxy](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeProxy.html).

## Cast Queries

Cast queries sweep a primitive or geometry through the world to find contacts with `PhysicsShape` objects and return intersection details.

- `PhysicsWorld.CastRay`: Find intersections of the specified ray.
- `PhysicsWorld.CastGeometry`: Find intersections of the specified geometry.
- `PhysicsWorld.CastShape`: Find intersections of the specified [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html).
- `PhysicsWorld.CastShapeProxy`: Find intersections of a generic [PhysicsShape.ShapeProxy](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeProxy.html).
- `PhysicsWorld.CastMover`: Perform intersection and resolution for the character-controller mover.

## Query Results

Queries that return multiple results use `NativeArray`, so you must dispose the returned array.

See "Controlling Memory Allocation" in the [Overview](Overview.md).

## Geometry Intersection

The [`PhysicsQuery`](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.html) type offers comprehensive primitive-geometry intersection tests.
For example, you can test a capsule against a circle with [`PhysicsQuery.CapsuleAndCircle`](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.CapsuleAndCircle.html).

To simplify usage, each geometry type exposes `Intersect` overloads that call the correct underlying test.
For instance, [`CapsuleGeometry.Intersect`](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.Intersect.html) includes overloads for all other geometry types and dispatches to the appropriate intersection method automatically.