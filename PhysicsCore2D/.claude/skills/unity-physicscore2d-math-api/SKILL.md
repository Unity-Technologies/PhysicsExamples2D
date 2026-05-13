---
name: unity-physicscore2d-math-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for Math Utilities. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsMath. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Math Utilities

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsMath`.

## PhysicsMath

> A set of mathematical operations that are useful for physics. These operations do not form a fully comprehensive mathematics library, they simply provide operations that are usually required when interacting with physics.

**Full name:** `Unity.U2D.Physics.PhysicsMath`  
**Docs:** [Unity.U2D.Physics.PhysicsMath](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMath.html)

### Properties

| Name | Summary |
|------|---------|
| `PI` | Get the value of PI used internally by the physics system. Using this will help with determinism. |
| `TAU` | Get the value of tau (2 * PI) used internally by the physics system. Using this will help with determinism. |

### Methods

#### `AngularVelocityToQuaternion(float, float, PhysicsWorld.TransformPlane)`

Calculate a Quaternion given a 2D angular velocity and a time to integrate over using the selected transform plane. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `angularVelocity` — The 2D angular velocity, in radians.
- `deltaTime` — The time over which to apply the angular velocity, in seconds.
- `transformPlane` — The transform plane to use.

**Returns:** The transformed rotation.

#### `Atan2(float, float)`

Calculate the arc-tangent i.e. the angle of the provided slope y/x. This operates as deterministically as possible across platforms.

**Params:**
- `y` — The Y axis.
- `x` — The X axis.

**Returns:** The angle in radians.

#### `CosSin(float, float, float)`

Calculate both the Cosine and Sine of the specified angle.

**Params:**
- `angle` — The angle to calculate, in radians.
- `cosine` — The Cosine of the specified angle.
- `sine` — The Sine of the specified angle.

#### `CosSin(float)`

Calculate both the Cosine and Sine of the specified angle.

**Params:**
- `angle` — The angle to calculate, in radians.

**Returns:** A 2D vector where X is the Cosine and Y is the Sine of the specified angle.

#### `GetRelativeMatrix(Transform, Transform, PhysicsWorld.TransformPlane, bool)`

Get the relative transformation matrix between the two specified transforms using the specified transform plane. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `transformFrom` — The transform used as a reference to transform from.
- `transformTo` — The transform used as a reference to transform to.
- `transformPlane` — The transform plane to use.
- `useScale` — If the returned matrix should include scale.

**Returns:** The calculated relative transformation matrix.

#### `GetRelativeMatrix2D(Matrix4x4, Matrix4x4, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, bool)`

Get the relative transformation matrix between the two specified transform matrix using the specified transform plane to transform into 2D space i.e. XY position and Z rotation.

**Params:**
- `transformFrom` — The transform used as a reference to transform from.
- `transformTo` — The transform used as a reference to transform to.
- `transformPlane` — The transform plane to use.
- `transformPlaneCustom` — The custom transform plane to use (if required).
- `useScale` — If the returned matrix should include scale. Scale is not relative and always uses the transformTo lossyScale.

**Returns:** The calculated relative transformation matrix.

#### `GetRelativePose2D(Matrix4x4, Matrix4x4, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, Vector2, float, Vector2)`

Get the relative transformation pose (translation, rotation and scale) between the two specified transform matrix using the specified transform plane to transform into 2D space.

**Params:**
- `transformFrom` — The transform used as a reference to transform from.
- `transformTo` — The transform used as a reference to transform to.
- `transformPlane` — The transform plane to use.
- `transformPlaneCustom` — The custom transform plane to use (if required).
- `translation` — The 2D relative translation.
- `rotation` — The 2D relative rotation.
- `scale` — The 2D relative scale. Scale is not relative and always uses the transformTo lossyScale.

#### `GetRotationAxes(PhysicsWorld.TransformPlane)`

Get the used rotation axes, given the specified transform plane. This is the inverse of PhysicsMath.GetRotationIgnoredAxes. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `transformPlane` — The transform plane to use.

**Returns:** The used rotation axes. 0 indicates an axis is ignored whereas 1 indicates the axis is used.

#### `GetRotationIgnoredAxes(PhysicsWorld.TransformPlane)`

Get the ignored rotation axes, given the specified transform plane. This is the inverse of PhysicsMath.GetRotationAxes. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `transformPlane` — The transform plane to use.

**Returns:** The ignored rotation axes. 0 indicates an axis is used whereas 1 indicates the axis is ignored.

#### `GetTranslationAxes(PhysicsWorld.TransformPlane)`

Get the used translation axes, given the specified transform plane. This is the inverse of PhysicsMath.GetTranslationIgnoredAxes. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `transformPlane` — The transform plane to use.

**Returns:** The used translation axes. 0 indicates an axis is ignored whereas 1 indicates the axis is used.

#### `GetTranslationIgnoredAxes(PhysicsWorld.TransformPlane)`

Get the ignored translation axes, given the specified transform plane. This is the inverse of PhysicsMath.GetTranslationAxes. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `transformPlane` — The transform plane to use.

**Returns:** The ignored translation axes. 0 indicates an axis is used whereas 1 indicates the axis is ignored.

#### `GetTranslationIgnoredAxis(Vector3, PhysicsWorld.TransformPlane)`

Get the ignored translation axis, given the specified transform plane. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `position` — The position to extra the axis from.
- `transformPlane` — The transform plane to use.

**Returns:** The ignored translation axis value.

#### `MaxAbsComponent(Vector2)`

Get the maximum absolute value component from the specified vector.

**Params:**
- `vector` — The vector to examine.

**Returns:** The calculated component.

#### `MaxAbsComponent(Vector3)`

Get the maximum absolute value component from the specified vector.

**Params:**
- `vector` — The vector to examine.

**Returns:** The calculated component.

#### `MinAbsComponent(Vector2)`

Get the minimum absolute value component from the specified vector.

**Params:**
- `vector` — The vector to examine.

**Returns:** The calculated component.

#### `MinAbsComponent(Vector3)`

Get the minimum absolute value component from the specified vector.

**Params:**
- `vector` — The vector to examine.

**Returns:** The calculated component.

#### `SetTransformFast2D(PhysicsTransform, Transform, PhysicsWorld.TransformPlane, bool)`

Set the Transform position and rotation using the specified PhysicsWorld.TransformPlane. For position, only two axis will be updated with the others remaining unchanged. For rotation, only a single rotation axis will be changed with the others set to zero. See: PhysicsMath.ToRotationFast3D and PhysicsWorld.SetTransform.

**Params:**
- `physicsTransform` — The physics transform to use as the source of the pose.
- `transform` — The Transform to set.
- `transformPlane` — The transform plane to use.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `SetTransformSlow3D(PhysicsTransform, Transform, PhysicsWorld.TransformPlane, bool)`

Set the Transform position and rotation using the specified PhysicsWorld.TransformPlane. For position, only two axis will be updated with the others remaining unchanged. For rotation, only a single rotation axis will be changed with the others remaining unchanged. See: PhysicsMath.ToRotationSlow3D and PhysicsWorld.SetTransform.

**Params:**
- `physicsTransform` — The physics transform to use as the source of the pose.
- `transform` — The Transform to set.
- `transformPlane` — The transform plane to use.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `SpringDamper(float, float, float, float, float)`

Calculate a one-dimensional mass-spring-damper simulation which drives towards a zero translation. You can then compute the new position using: "translation += newSpeed * deltaTime;"

**Params:**
- `frequency` — The frequency of the spring, in cycles per second.
- `damping` — The damping of the spring. Must be >= zero.
- `translation` — The current translation of the spring.
- `speed` — The current speed of the spring.
- `deltaTime` — The time over which to simulate the spring.

**Returns:** The new calculated spring speed.

#### `Swizzle(Vector3, PhysicsWorld.TransformPlane)`

Transform a 3D vector into a 3D vector using the selected transform plane. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `position` — The 3D vector to transform.
- `transformPlane` — The transform plane to use.

**Returns:** The transformed vector.

#### `Swizzle(Vector4, PhysicsWorld.TransformPlane)`

Transform a 3D position (with perspective divide) into a 3D position using the selected transform plane. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `position` — The 3D position to transform.
- `transformPlane` — The transform plane to use.

**Returns:** The transformed position.

#### `Swizzle(Matrix4x4, PhysicsWorld.TransformPlane)`

Transform a Matrix position (with perspective divide) into a Matrix position using the selected transform plane. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `matrix` — The Matrix position to transform.
- `transformPlane` — The transform plane to use.

**Returns:** The transformed matrix.

#### `ToDegrees(float)`

Convert radians to degrees. This operates as deterministically as possible across platforms. See PhysicsMath.ToRadians.

**Params:**
- `radians` — The radian value to convert to degrees.

**Returns:** The radian value converted to degrees.

#### `ToPhysicsTransform(Transform, PhysicsWorld.TransformPlane)`

Transform a 3D Transform position and rotation to a 2D PhysicsTransform. Scale is not part of a PhysicsTransform therefore it is ignored. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `transform` — The 3D transform to use.
- `transformPlane` — The transform plane to use.

**Returns:** The 2D transform.

#### `ToPosition2D(Vector3, PhysicsWorld.TransformPlane)`

Transform a 3D position into a 2D position using the selected transform plane. If TransformPlane.Custom is used then PhysicsWorld.TransformPlane.XY is used.

**Params:**
- `position` — The 3D position to transform.
- `transformPlane` — The transform plane to use.

**Returns:** The transformed position.

#### `ToPosition2D(Vector3, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom)`

Transform a 3D position into a 2D position using the selected transform plane. This can also be used for a 3D scale vector as it undergoes the same transformation.

**Params:**
- `position` — The 3D position to transform.
- `transformPlane` — The transform plane to use.
- `transformPlaneCustom` — The custom transform plane (if required).

**Returns:** The transformed position.

#### `ToPosition3D(Vector2, Vector3, PhysicsWorld.TransformPlane)`

Transform a 2D position into a 3D position using the selected transform plane. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `position` — The 2D position to transform.
- `reference` — The 3D position used as a reference.
- `transformPlane` — The transform plane to use.

**Returns:** The transformed position.

#### `ToPosition3D(Vector2, Vector3, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom)`

Transform a 2D position into a 3D position using the selected transform plane.

**Params:**
- `position` — The 2D position to transform.
- `reference` — The 3D position used as a reference.
- `transformPlane` — The transform plane to use.
- `transformPlaneCustom` — The custom transform plane (if required).

**Returns:** The transformed position.

#### `ToRadians(float)`

Convert degrees to radians. This operates as deterministically as possible across platforms. See PhysicsMath.ToDegrees.

**Params:**
- `degrees` — The degree value to convert to radians.

**Returns:** The value converted to radians.

#### `ToRotation2D(Quaternion, PhysicsWorld.TransformPlane)`

Transform a 3D rotation into a 2D angle using the selected transform plane. If TransformPlane.Custom is used then PhysicsWorld.TransformPlane.XY is used.

**Params:**
- `quaternion` — The 3D rotation to transform.
- `transformPlane` — The transform plane to use.

**Returns:** The transformed rotation in radians.

#### `ToRotation2D(Quaternion, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom)`

Transform a 3D rotation into a 2D angle using the selected transform plane.

**Params:**
- `rotation` — The 3D rotation to transform.
- `transformPlane` — The transform plane to use.
- `transformPlaneCustom` — —

**Returns:** The transformed rotation in radians.

#### `ToRotationFast3D(float, PhysicsWorld.TransformPlane)`

Transform a 2D angle into a 3D rotation using the selected transform plane (Fast). The transformation is fast because the rotation is simplified by the fact that only a single axis of rotation is handled. All other axis rotations are reset to zero. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `angle` — The 2D angle to transform in radians.
- `transformPlane` — The transform plane to use.

**Returns:** The transformed rotation.

#### `ToRotationSlow3D(float, Quaternion, PhysicsWorld.TransformPlane)`

Transform a 2D angle into a 3D rotation using the selected transform plane (Slow). The transformation is slower because the rotation is more complex due to the fact that changing a single axis of rotation requires it to not affect any other axis rotations. If PhysicsWorld.TransformPlane.Custom is used, PhysicsWorld.TransformPlane.XY will be used instead which may not provide the correct results.

**Params:**
- `angle` — The 2D angle to transform in radians.
- `reference` — The 3D rotation used as a reference.
- `transformPlane` — The transform plane to use.

---

_Generated by `.claude/api-reference/_generate.py` from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
