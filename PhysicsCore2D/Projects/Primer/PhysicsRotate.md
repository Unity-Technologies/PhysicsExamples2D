# PhysicsRotate

In 2D physics, rotation is a single scalar (float) around one axis.
The `PhysicsRotate` type is used as the rotation component of a [PhysicsTransform](PhysicsTransform.md) and to set a body’s rotation via [PhysicsBody.rotation](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-rotation.html).

The `PhysicsRotate` type exists for two main reasons:
- **Clarity**: It makes it clear that a value represents a rotation, reducing confusion with other single floating-point numbers.
- **Internal Representation**: It hides the internal detail that rotation is stored as a normalized `Vector2` direction, which is how the physics system uses it internally. You can specify rotation either as an angle or directly as a direction but in both cases, a unit-vector is created.

## Creation

There are various ways to create a `PhysicsRotate` to represent a specific rotation as shown here:
```csharp
void Run()
{
    // Create an identity (zero) rotation.
    var rotate1 = new PhysicsRotate();

    // Create an identity (zero) rotation
    var rotate2 = PhysicsRotate.identity;
    
    // Create a rotation in radians.
    var rotate3 = new PhysicsRotate(PhysicsMath.PI);
    
    // Create a 45-degree rotation.
    var rotate4 = new PhysicsRotate(PhysicsMath.ToDegrees(45f));

    // Create a rotation in a unit-direction.
    // NOTE: This effectively produces a +90-degree rotation.
    var rotate5 = new PhysicsRotate(Vector2.up);

    // Get the 2D rotation from the "player" transform.
    // NOTE: We can choose the transform plane too.
    var playerTransform = GameObject.Find("Player").transform;
    var rotate6 = new PhysicsRotate(playerTransform.rotation, PhysicsWorld.TransformPlane.XY);
    
    // Create various fixed rotations.
    var rotateZero = PhysicsRotate.right;
    var rotate90 = PhysicsRotate.up;
    var rotate180 = PhysicsRotate.left;
    var rotate270 = PhysicsRotate.down;     
}
```

## Operations

The `PhysicsRotate` provides a set of commonly useful operations:
- **Multiply Rotation**: You can [multiply](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.MultiplyRotation.html) and [https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.InverseMultiplyRotation.html) `PhysicsRotate` together.
- **Rotating Vectors**: You can [rotate](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.RotateVector.html) and [inverse-rotate](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.InverseRotateVector.html) vectors.
- **Modify Rotations**: You can [rotate](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.html) an existing `PhysicsRotate` by a specified angle.
- **Various Utilities**: You can [lerp-rotation](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.LerpRotation.html), [integrate-rotation](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.IntegrateRotation.html), calculate [angular-velocity](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.AngularVelocity.html), get [relative-angle](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.GetRelativeAngle.html), get a rotation [Matrix4x4](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.GetMatrix.html) etc.
