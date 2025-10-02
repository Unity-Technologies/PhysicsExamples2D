# PhysicsTransform

The physics system models objects with three degrees of freedom (3-DOF): two for position and one for rotation.
The [PhysicsTransform](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsTransform.html) bundles these into a single structure with `position` and `rotation` properties.
The `position` is defined with a [Vector2](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Vector2.html) whereas the `rotation` is defined with a dedicated physics type of [PhysicsRotate](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsRotate.html), 
The `PhysicsTransform` is  commonly used to get or set a `PhysicsBody.transform`, and to transform geometry (for example, when creating shapes or performing debug drawing).

See [PhysicsRotate](PhysicsRotate.md) for more information on rotation.

## Creation

There are various ways to create a `PhysicsTransform` shown here:
```csharp
void Run()
{
    // Create an identity transform.
    var transform1 = new PhysicsTransform();

    // Create an identity transform.
    var transform2 = PhysicsTransform.identity;

    // Create an identity transform but set its position to (10, 20) using the implicit Vector2 operator.
    var transform3 = new PhysicsTransform();
    transform3 = new Vector2(10f, 20f);
    
    // Create a transform to a position (10, 20) with no rotation.
    var transform4 = new PhysicsTransform(new Vector2(10f, 20f));
    
    // Create a transform to a position (10f, 20f) at a 45-degree roation.
    var transform5 = new PhysicsTransform(new Vector2(10f, 20f), new PhysicsRotate(PhysicsMath.ToRadians(45f)));
}  
```

## Operations

The `PhysicsTransform` provides a few simple operations for transforming other `PhysicsTransform` and `Vector2`:
- **Multiply Transform**: You can [multiply](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsTransform.MultiplyTransform.html) and [inverse-multiply](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsTransform.InverseMultiplyTransform.html) `PhysicsTransform` together.
- **Transform Point**: You can [transform](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsTransform.TransformPoint.html) and [inverse-transform](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsTransform.InverseTransformPoint.html) `Vector2`.

## Geometry

All `PhysicsShape` geometry allows for transformation and uses the `PhysicsTransform` to specify the transformation required.
All geometry provides both a `Transform` and `InverseTransform` method to transform the geometry.

For instance, you can transform `CapsuleGeometry` like so:

```csharp
void Run()
{
    // Create a transform to translate by (2, 3).
    var transform = new PhysicsTransform(new Vector2(2f, 3f));

    // Create a default capsule geometry but transform it.
    var geometry = CapsuleGeometry.defaultGeometry.Transform(transform);
}  
```
