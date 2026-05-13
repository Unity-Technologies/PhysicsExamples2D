# PhysicsCore2D — Helpers

_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsRotate`, `PhysicsTransform`, `PhysicsUserData`.

## PhysicsRotate

> Represents a 2D rotation.

**Full name:** `Unity.U2D.Physics.PhysicsRotate`  
**Docs:** [Unity.U2D.Physics.PhysicsRotate](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.html)

### Fields

| Name | Summary |
|------|---------|
| `direction` | The rotation direction where X = cos(rotation) and Y = sin(rotation). This should always be normalized otherwise warnings will be produced when used, however this is not enforced. See PhysicsRotate._isNormalized and PhysicsRotate.Normalized. |

### Properties

| Name | Summary |
|------|---------|
| `cos` | The cosine of the rotation angle. |
| `degrees` | Get the rotation, in degrees. |
| `down` | A rotation of -PI/2 Radians (-90 Degrees or +270 Degrees). |
| `identity` | The identity rotation i.e. no rotation. |
| `isNormalized` | Is the rotation normalized? If not, it should be normalized using PhysicsRotate.Normalized. |
| `isValid` | Check if the rotation is valid (not NaN and Normalized). |
| `left` | A rotation of +PI Radians (+/- 180 Degrees). |
| `radians` | Get the rotation, in radians. |
| `right` | A rotation of zero Radians. This is the same as identity. See PhysicsRotate._identity. |
| `sin` | The sine of the rotation angle. |
| `up` | A rotation of +PI/2 Radians (+90 Degrees). |

### Methods

#### `new()`

Create an identity rotation.

#### `new(Vector2)`

Create a rotation with the specified direction.

**Params:**
- `direction` — The direction to use. This cannot be zero length.

#### `new()`

Create a rotation with the specified Quaternion.

**Params:**
- `rotation` — The Quaternion rotation to use.
- `transformPlane` — The transform plane to use.

**Returns:** The 2D rotation extracted from the specified Quaternion.

#### `AngularVelocity(PhysicsRotate, float)`

Calculate the angular velocity necessary to rotate the current rotation and the specified rotation over a give time.

**Params:**
- `rotation` — The rotation used as a calculation to rotate to.
- `deltaTime` — The delta time over which the rotation would take place.

**Returns:** The angular velocity required to rotate to the specified rotation.

#### `AngularVelocity(PhysicsRotate, PhysicsRotate, float)`

Calculate the angular velocity necessary to rotate between two rotations over a give time.

**Params:**
- `rotationA` — The rotation to rotate from.
- `rotationB` — The rotation to rotate to.
- `deltaTime` — The delta time over which the rotation would take place.

**Returns:** The angular velocity required to rotate between the specified rotations.

#### `FromDegrees(float)`

Create a rotation with the specified rotation, in degrees. See PhysicsRotate.FromRadians.

**Params:**
- `degrees` — The rotation angle specified, in degrees.

**Returns:** The rotation represented by the specified rotation, in degrees.

#### `FromRadians(float)`

Create a rotation with the specified rotation, in radians. See PhysicsRotate.FromDegrees.

**Params:**
- `radians` — The rotation angle specified, in radians.

**Returns:** The rotation represented by the specified rotation, in radians.

#### `GetMatrix(PhysicsWorld.TransformPlane)`

Calculate a rotation Matrix4x4 using the specified transform plane.

**Params:**
- `transformPlane` — The transform plane to use.

**Returns:** The rotation matrix.

#### `GetRelativeAngle(PhysicsRotate)`

Get the relative angle between this rotation and the specified rotation. The limits of this are +/- PhysicsMath.PI.

**Params:**
- `rotation` — The rotation to calculate the relative angle against.

**Returns:** The relative angle, in radians.

#### `IntegrateRotation(float)`

Integrate the rotation using the specified angle change.

**Params:**
- `deltaAngle` — The angle to integrate the rotation with, in radians.

**Returns:** The integrated rotation.

#### `Inverse()`

Calculate a new inverse rotation of the current rotation.

**Returns:** The inverse rotation of the current rotation as (cosine, -sine).

#### `InverseMultiplyRotation(PhysicsRotate)`

Inverse Multiply a rotation with this rotation.

**Params:**
- `rotation` — The rotation to inverse multiply by.

**Returns:** The result of the inverse multiply rotation.

#### `InverseRotateVector(Vector2)`

Inverse Rotate a vector.

**Params:**
- `vector` — The vector to inverse rotate.

**Returns:** The result of the inverse vector rotation.

#### `LerpRotation(PhysicsRotate, float)`

Calculate the normalized linear interpolation of this rotation and the specified rotation using the specified interval.

**Params:**
- `rotation` — The rotation to lerp with against the current rotation.
- `interval` — The lerp interval, typically in the range [0, 1]. A value outside of this range performs normalized linear extrapolation.

**Returns:** The normalized linear interpolation/extrapolation.

#### `LerpRotation(PhysicsRotate, PhysicsRotate, float)`

Calculate the normalized linear interpolation between two rotations using the specified fraction.

**Params:**
- `rotationA` — The rotation to lerp from.
- `rotationB` — The rotation to lerp to.
- `interval` — The lerp interval, typically in the range [0, 1]. A value outside of this range performs normalized linear extrapolation.

**Returns:** The normalized linear interpolation/extrapolation.

#### `MultiplyRotation(PhysicsRotate)`

Multiply a rotation with this rotation.

**Params:**
- `rotation` — The rotation to multiply by.

**Returns:** The result of the multiply rotation.

#### `Normalized()`

Create a normalized rotation.

#### `Rotate(float)`

Calculate a new rotation of the current rotation by the specified angle.

**Params:**
- `deltaAngle` — The change in angle, in radians.

#### `RotateVector(Vector2)`

Rotate a vector.

**Params:**
- `vector` — The vector to rotate.

**Returns:** The result of the vector rotation.

#### `UnwindAngle(float)`

Convert any angle into the range [-pi, pi].

**Params:**
- `angle` — The angle to convert, in radians.

**Returns:** The angle converted into the range [-pi, pi].

## PhysicsTransform

> Represents a 2D transformation combining a translation and a PhysicsRotate.

**Full name:** `Unity.U2D.Physics.PhysicsTransform`  
**Docs:** [Unity.U2D.Physics.PhysicsTransform](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsTransform.html)

### Fields

| Name | Summary |
|------|---------|
| `position` | The translation for the transformation. |
| `rotation` | The rotation for the transformation. |

### Properties

| Name | Summary |
|------|---------|
| `identity` | The identity transformation i.e. a transformation with no translation or rotation. |
| `isValid` | Check if the PhysicsTransform is valid (position is not NaN and PhysicsRotate._isValid). |

### Methods

#### `new()`

Create an identity transform.

#### `new(Vector2)`

Create a transformation with the specified translation and no rotation.

**Params:**
- `position` — The translation for the transformation.

#### `new(Vector2, PhysicsRotate)`

Create a transformation with the specified translation and rotation.

**Params:**
- `position` — The translation for the transformation.
- `rotation` — The rotation for the transformation.

#### `GetPositionAndRotation(Vector2, PhysicsRotate)`

Get both the position and rotation.

**Params:**
- `position` — The translation for the transformation.
- `rotation` — The rotation for the transformation.

#### `InverseMultiplyTransform(PhysicsTransform)`

Inverse Multiply the specified transform with the current transform.

**Params:**
- `transform` — The transform to inverse multiply with.

**Returns:** The resultant multiplied transform.

#### `InverseTransformPoint(Vector2)`

Inverse Transform a point using the current transform translation and rotation.

**Params:**
- `point` — The point to inverse transform.

**Returns:** The inverse transformed point.

#### `MultiplyTransform(PhysicsTransform)`

Multiply the specified transform with the current transform.

**Params:**
- `transform` — The transform to multiply with.

**Returns:** The resultant multiplied transform.

#### `TransformPoint(Vector2)`

Transform a point using the current transform translation and rotation.

**Params:**
- `point` — The point to transform.

**Returns:** The transformed point.

## PhysicsUserData

> Custom user data. The physics system doesn't use this data, it is entirely for custom use.

**Full name:** `Unity.U2D.Physics.PhysicsUserData`  
**Docs:** [Unity.U2D.Physics.PhysicsUserData](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html)

### Properties

| Name | Summary |
|------|---------|
| `boolValue` | A custom bool. |
| `floatValue` | A custom 32-bit float. |
| `int64Value` | A custom 64-bit long. |
| `intValue` | A custom 32-bit int. |
| `objectValue` | A custom Unity object. To get the EntityId of the object, use PhysicsUserData._objectValueId. |
| `objectValueId` | The EntityId of a Unity object. This is the object referred to with PhysicsUserData._objectValue |
| `physicsMaskValue` | A custom 64-bit PhysicsMask. |
