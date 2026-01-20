# PhysicsAABB

A [PhysicsAABB](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB.html) (Axis-Aligned Bounding Box) is a 2D rectangular boundary used for various purposes like querying shape bounds, performing world queries, and debug drawing.
It's defined by its `lowerBound` and `upperBound` `Vector2` positions.
The type also provides functions to calculate its center, size, perimeter, and to check for ray casts, overlaps with other bounds, or points.

## Creation

You can create a `PhysicsAABB` like so:

```csharp
void Run()
{
    // Create a physics AABB.
    var physicsAABB = new PhysicsAABB
    {
        lowerBound = new Vector2(-3f, -2f),
        upperBound = new Vector2(4f, 6f)
    };
}
```

## Operations

The `PhysicsAABB` type provides a set of commonly useful operations:
- **Queries**: You can perform spatial queries against the bounds with [Contains](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB.Contains.html), [Overlap](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB.Overlap.html), [OverlapPoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB.OverlapPoint.html) and [CastRay](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB.CastRay.html).
- **Validity**: You can check the bounds for [validity](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB-isValid.html) or [normalize](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB.Normalized.html) them so they are valid.
- **Reading**: You can read useful bounds properties such as the [center](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB-center.html), [extents](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB-extents.html) and [perimeter](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB-perimeter.html).
