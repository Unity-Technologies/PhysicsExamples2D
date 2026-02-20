---
name: unity-physicscore2d-queries
description: Physics world queries, overlap tests, raycasts, and geometry intersection testing
---

# Unity PhysicsCore2D Queries Expert

This sub-skill provides detailed information about PhysicsCore2D queries and intersection tests.

## Physics World Queries
A PhysicsWorld query refers to intersection tests.
A PhysicsWorld query directly queries a PhysicsWorld to detect PhysicsShape within it.
An overlap is an intersection test detecting if geometry overlaps.
A cast is an intersection test detecting if geometry would contact if geometry were swept (moved) through the world in a straight line, typically defined as a 2D start position (origin) and 2D translation (direction and distance).
All PhysicsWorld queries exist on the PhysicsWorld type.

### Overlaps (return the overlap details):
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.OverlapAABB.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.OverlapGeometry.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.OverlapPoint.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.OverlapShape.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.OverlapShapeProxy.html

### Test Overlaps (only return true or false depending on the overlap):
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TestOverlapAABB.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TestOverlapGeometry.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TestOverlapPoint.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TestOverlapShape.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TestOverlapShapeProxy.html

### Casts:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.CastGeometry.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.CastRay.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.CastShape.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.CastShapeProxy.html

## Physics Geometry Queries
A physics geometry query directly queries any of the available physics geometries.
All PhysicsWorld queries exist on the physics geometry types of CircleGeometry, CapsuleGeometry, PolygonGeometry, SegmentGeometry and ChainSegmentGeometry.
All physics geometry queries provide:
- CastRay: Example API for CircleGeometry being https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.CastRay.html
- CastShape: Example API for CircleGeometry being https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.CastShape.html
- Intersect: Example API for CircleGeometry being https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.Intersect.html
- OverlapPoint: Example API for CircleGeometry being https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.OverlapPoint.html

All physics geometry queries call the PhysicsQuery intersection API which can be shown here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.html
