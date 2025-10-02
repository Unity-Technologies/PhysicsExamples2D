# Physics Math

The [PhysicsMath](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.html) type provides math utilities tailored to the physics system.
It isn’t a full math library; it focuses on commonly used physics-specific operations.

## TransformPlane Conversion

The physics system is 2D with three degrees of freedom (3‑DOF), but it can write to Unity’s `Transform` system, which uses 3D transforms. `PhysicsWorld` and its `PhysicsBody` instances can automatically write to `Transform` in various [`PhysicsWorld.TransformPlane`](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.TransformPlane.html) configurations, with no code required. When scripts need to convert between 2D and 3D positions or rotations, they do so relative to the current [TransformPlane](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.TransformPlane.html).

Position conversion:
- `TransformPlane.XY`: (X, Y, Z) to (X, Y).
- `TransformPlane.XZ`: (X, Y, Z) to (X, Z).
- `TransformPlane.ZY`: (X, Y, Z) to (Z, Y).
- 2D → 3D: Provide a 3D reference position to supply the missing axis.

Rotation conversion:
- 3D rotations use `Quaternion`, so extracting a single-axis 2D rotation requires mathematical extraction (not just reading a component).
- 2D to 3D: Provide a 3D reference rotation to supply the two missing axes.
- Fast vs. slow variants: “Fast” sets the missing axes to zero (faster); “Slow” integrates the 2D rotation into the existing 3D rotation (slower).

All required conversion functions are provided. Typically, you pass a 2D or 3D position/rotation and a `TransformPlane`; some conversions also require a reference 3D position or rotation.

For example, given a 3D `Transform` position, you may wish to convert to a 2D position:

```csharp
void Run(Transform playerTransform, Vector2 movement)
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Fetch the world transform plan.
    var transformPlane = world.transformPlane;

    // Fetch the 3D player position from the Transform.
    var playerPosition3D = playerTransform.position;
    
    // Read the 2D player position.
    // NOTE: This will work independent of the current transform plane.
    var playerPosition2D = PhysicsMath.ToPosition2D(playerPosition3D, transformPlane);

    // Perform some 2D operation independent of the transform plane.
    playerPosition2D += movement;
    
    // Write the 2D player position to the 3D Transform.
    playerTransform.position = PhysicsMath.ToPosition3D(playerPosition2D, playerPosition3D, transformPlane);
}
```
A similar set of operations for 3D rotation are available too:
```csharp
void Run(Transform playerTransform, float deltaRotation)
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Fetch the world transform plan.
    var transformPlane = world.transformPlane;

    // Fetch the 3D player rotation from the Transform.
    var playerRotation3D = playerTransform.rotation;
    
    // Read the 2D player rotation.
    // NOTE: This will work independent of the current transform plane.
    var playerRotation2D = PhysicsMath.ToRotation2D(playerRotation3D, transformPlane);

    // Perform some 2D operation independent of the transform plane.
    playerRotation2D += deltaRotation;
    
    // Write the 2D player rotation to the 3D Transform.
    // NOTE: The "Slow3D" here ensures the other two rotational axes are not modified.
    // We could've used "Fast3D" here which sets the other two rotational axes to zero which is much faster.
    playerTransform.rotation = PhysicsMath.ToRotationSlow3D(playerRotation2D, playerRotation3D, transformPlane);
}
```

In some cases, you may need to identify which axes are used or ignored for a given `TransformPlane`. The following helper functions are available:

- [GetTranslationAxes](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.GetTranslationAxes.html)
- [GetTranslationIgnoredAxis](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.GetTranslationIgnoredAxis.html)
- [GetTranslationIgnoredAxes](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.GetTranslationIgnoredAxes.html)
- [GetRotationAxes](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.GetRotationAxes.html)
- [GetRotationIgnoredAxes](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.GetRotationIgnoredAxes.html)

Finally, simpler helpers like [Swizzle](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.Swizzle.html) and [ToPhysicsTransform](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.ToPhysicsTransform.html) let you swap axes and convert a Unity `Transform` directly into a `PhysicsTransform`.

## Utility

Additional utility functions include [ToDegrees](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.ToDegrees.html), [ToRadians](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.ToRadians.html), [MaxAbsComponent](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.MaxAbsComponent.html), [MinAbsComponent](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.MinAbsComponent.html), [AngularVelocityToQuaternion](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.AngularVelocityToQuaternion.html), and [SpringDamper](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.SpringDamper.html).

## Determinism

Several features are provided purely to improve determinism, including [PI](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.PI.html), [TAU](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.TAU.html), [Atan2](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.Atan2.html), and [CosSin](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.CosSin.html).



