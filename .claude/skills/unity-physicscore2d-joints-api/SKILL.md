---
name: unity-physicscore2d-joints-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for Joints. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsDistanceJoint, PhysicsDistanceJointDefinition, PhysicsFixedJoint, PhysicsFixedJointDefinition, PhysicsHingeJoint, PhysicsHingeJointDefinition, PhysicsIgnoreJoint, PhysicsIgnoreJointDefinition, PhysicsJoint, PhysicsRelativeJoint, PhysicsRelativeJointDefinition, PhysicsSliderJoint, PhysicsSliderJointDefinition, PhysicsWheelJoint, PhysicsWheelJointDefinition. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Joints

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsDistanceJoint`, `PhysicsDistanceJointDefinition`, `PhysicsFixedJoint`, `PhysicsFixedJointDefinition`, `PhysicsHingeJoint`, `PhysicsHingeJointDefinition`, `PhysicsIgnoreJoint`, `PhysicsIgnoreJointDefinition`, `PhysicsJoint`, `PhysicsRelativeJoint`, `PhysicsRelativeJointDefinition`, `PhysicsSliderJoint`, `PhysicsSliderJointDefinition`, `PhysicsWheelJoint`, `PhysicsWheelJointDefinition`.

## PhysicsDistanceJoint

> Connects an anchor point on body A with an anchor point on body B via a line segment of a specified distance.

**Full name:** `Unity.U2D.Physics.PhysicsDistanceJoint`  
**Docs:** [Unity.U2D.Physics.PhysicsDistanceJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJoint.html)

### Fields

| Name | Summary |
|------|---------|
| `m_Id` | The base joint Id. |

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The second body the joint constrains. |
| `bodyB` | A local anchor point on the first body for the constraint. |
| `callbackTarget` | Get/Set the that event callbacks for this joint will be sent to. Care should be taken with any assigned as a callback target that isn't a as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A with call . |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newtons. |
| `currentDistance` | Get the current distance. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `currentMotorForce` | The current motor force, usually in newtons. |
| `distance` | The desired distance constraint i.e. the rest length of this joint. This has a lower stable limit of just above zero. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `enableLimit` | Enable/Disable the joint distance limit. |
| `enableMotor` | Enable/Disable the joint motor. |
| `enableSpring` | Enable/Disable the spring behaviour. If false then the joint will be rigid, overriding the limit and motor. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See . |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See . |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `maxDistanceLimit` | Maximum distance limit of this joint. Must be greater than or equal to the minimum length. |
| `maxMotorForce` | The maximum force the motor can apply, usually in newtons. |
| `minDistanceLimit` | Minimum distance limit of this joint. This will be clamped to a lower stable limit. |
| `motorSpeed` | The desired motor speed, usually in meters per second. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `springDamping` | The spring linear damping, non-dimensional. |
| `springFrequency` | The spring linear stiffness frequency, in cycles per second. |
| `springLowerForce` | The lower spring force controls how much tension the spring can sustain. |
| `springUpperForce` | The upper spring force controls how much compression the spring can sustain. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsJoint)`

Create a from the specified base joint. The provided joint must be a joint type of .

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsDistanceJointDefinition)`

Create a in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using .

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint})`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see ). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsDistanceJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using .

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator ==(PhysicsDistanceJoint, PhysicsDistanceJoint)`

#### `operator implicit()`

Cast to the base .

**Params:**
- `joint` — The current joint.

#### `operator implicit()`

Cast to a from the base . The provided joint must be a joint type of .

**Params:**
- `joint` — The base joint to cast.

#### `operator !=(PhysicsDistanceJoint, PhysicsDistanceJoint)`

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with . The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using .

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsDistanceJointDefinition

> A joint definition used to specify properties when creating a .

**Full name:** `Unity.U2D.Physics.PhysicsDistanceJointDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsDistanceJointDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJointDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Get a default definition. |
| `distance` | The desired distance constraint i.e. the rest length of this joint. This has a lower stable limit of just above zero. |
| `drawScale` | Controls the scaling of the joint drawing. Not all joints have scalable elements but those that do will use this scaling. |
| `enableLimit` | Enable/disable the joint limit. |
| `enableMotor` | Enable/Disable the joint motor. |
| `enableSpring` | Enable/Disable the distance constraint to behave like a spring. If false then the distance joint will be rigid, overriding the limit and motor. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `maxDistanceLimit` | Maximum length limit of this joint. Must be greater than or equal to the minimum length. |
| `maxMotorForce` | The maximum force the motor can apply, usually in newtons. |
| `minDistanceLimit` | Minimum length limit of this joint. This will be clamped to a lower stable limit. |
| `motorSpeed` | The desired motor speed, usually in meters per second. |
| `springDamping` | The spring linear damping, non-dimensional. Use 1 for critical damping. |
| `springFrequency` | The spring linear stiffness frequency, in cycles per second. |
| `springLowerForce` | The lower spring force controls how much tension the spring can sustain. |
| `springUpperForce` | The upper spring force controls how much compression the spring can sustain. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See . |

### Methods

#### `new()`

Create a default definition.

#### `new(bool)`

Create a default definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsFixedJoint

> A joint to constrain a pair of bodies together rigidly. This constraint provides springs to mimic soft-body simulation. The approximate solver cannot always hold many bodies together completely rigidly.

**Full name:** `Unity.U2D.Physics.PhysicsFixedJoint`  
**Docs:** [Unity.U2D.Physics.PhysicsFixedJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsFixedJoint.html)

### Fields

| Name | Summary |
|------|---------|
| `m_Id` | The base joint Id. |

### Properties

| Name | Summary |
|------|---------|
| `angularDamping` | Angular damping, non-dimensional. Use 1 for critical damping. |
| `angularFrequency` | Angular stiffness in cycles per second. Use zero for maximum stiffness. |
| `bodyA` | The second body the joint constrains. |
| `bodyB` | A local anchor point on the first body for the constraint. |
| `callbackTarget` | Get/Set the that event callbacks for this joint will be sent to. Care should be taken with any assigned as a callback target that isn't a as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A with call . |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newtons. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See . |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See . |
| `linearDamping` | Linear damping, non-dimensional. Use 1 for critical damping. |
| `linearFrequency` | Linear stiffness in cycles per second. Use zero for maximum stiffness. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsJoint)`

Create a from the specified base joint. The provided joint must be a joint type of .

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsFixedJointDefinition)`

Create a in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using .

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint})`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see ). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsFixedJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using .

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator ==(PhysicsFixedJoint, PhysicsFixedJoint)`

#### `operator implicit()`

Cast to the base .

**Params:**
- `joint` — The current joint.

#### `operator implicit()`

Cast to a from the base . The provided joint must be a joint type of .

**Params:**
- `joint` — The base joint to cast.

#### `operator !=(PhysicsFixedJoint, PhysicsFixedJoint)`

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with . The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using .

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsFixedJointDefinition

> A joint definition used to specify properties when creating a .

**Full name:** `Unity.U2D.Physics.PhysicsFixedJointDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsFixedJointDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsFixedJointDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `angularDamping` | Angular damping, non-dimensional. Use 1 for critical damping. |
| `angularFrequency` | Angular stiffness frequency, in cycles per second. Use zero for maximum stiffness. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Get a default definition. |
| `drawScale` | Controls the scaling of the joint drawing. Not all joints have scalable elements but those that do will use this scaling. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `linearDamping` | Linear damping, non-dimensional. Use 1 for critical damping. |
| `linearFrequency` | Linear stiffness frequency, in cycles per second. Use zero for maximum stiffness. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See . |

### Methods

#### `new()`

Create a default definition.

#### `new(bool)`

Create a default definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsHingeJoint

> A joint where an anchor point on body B is fixed to an anchor point on body A. This joint allows relative rotation.

**Full name:** `Unity.U2D.Physics.PhysicsHingeJoint`  
**Docs:** [Unity.U2D.Physics.PhysicsHingeJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJoint.html)

### Fields

| Name | Summary |
|------|---------|
| `m_Id` | The base joint Id. |

### Properties

| Name | Summary |
|------|---------|
| `angle` | Get the current angle of the joint, in degrees. |
| `bodyA` | The second body the joint constrains. |
| `bodyB` | A local anchor point on the first body for the constraint. |
| `callbackTarget` | Get/Set the that event callbacks for this joint will be sent to. Care should be taken with any assigned as a callback target that isn't a as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A with call . |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newtons. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `currentMotorTorque` | Get the current motor torque. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `enableLimit` | Enable/Disable the joint rotation limit. |
| `enableMotor` | Enable/Disable the joint motor. |
| `enableSpring` | Enable/Disable the rotational spring. |
| `enableUnpinned` | Enable/Disable unpinned mode where only Body A is affected and body B and its local anchor point is ignored. Body B must still be assigned so it is typical to assign a static ground body, preferably shared/reused. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See . |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See . |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `lowerAngleLimit` | Get the lower angle limit, in degrees. |
| `maxMotorTorque` | The maximum torque the motor can apply, usually in newton-meters. |
| `motorSpeed` | The desired motor speed, usually in degrees per second. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `springDamping` | The spring damping, non-dimensional. |
| `springFrequency` | The spring stiffness, in cycles per second. |
| `springTargetAngle` | The spring target angle, in degrees. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `upperAngleLimit` | Get the upper angle limit, in degrees. |
| `userData` | Get/Set that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsJoint)`

Create a from the specified base joint. The provided joint must be a joint type of .

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsHingeJointDefinition)`

Create a PhysicsHingeJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using .

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint})`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see ). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsHingeJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using .

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator ==(PhysicsHingeJoint, PhysicsHingeJoint)`

#### `operator implicit()`

Cast to the base .

**Params:**
- `joint` — The current joint.

#### `operator implicit()`

Cast to a from the base . The provided joint must be a joint type of .

**Params:**
- `joint` — The base joint to cast.

#### `operator !=(PhysicsHingeJoint, PhysicsHingeJoint)`

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with . The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using .

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsHingeJointDefinition

> A joint definition used to specify properties when creating a .

**Full name:** `Unity.U2D.Physics.PhysicsHingeJointDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsHingeJointDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJointDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Create a default definition. |
| `drawScale` | Controls the scaling of the joint drawing. Not all joints have scalable elements but those that do will use this scaling. |
| `enableLimit` | Enable/disable the joint angle limit. |
| `enableMotor` | Enable/disable the joint motor. |
| `enableSpring` | Enable/Disable the rotational spring. |
| `enableUnpinned` | Enable/Disable unpinned mode where only Body A is affected and body B and its local anchor point is ignored. Body B must still be assigned so it is typical to assign a static ground body, preferably shared/reused. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `lowerAngleLimit` | The lower angle limit, in degrees. |
| `maxMotorTorque` | The maximum torque the motor can apply, usually in newton-meters. |
| `motorSpeed` | The desired motor speed, usually in degrees per second. |
| `springDamping` | The spring damping, non-dimensional. Use 1 for critical damping. |
| `springFrequency` | The spring stiffness frequency, in cycles per second. |
| `springTargetAngle` | The spring target angle, in degrees. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `upperAngleLimit` | The upper angle limit, in degrees. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See . |

### Methods

#### `new()`

Create a default definition.

#### `new(bool)`

Create a default definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsIgnoreJoint

> A joint used to ignore collision between two specific bodies. As a side effect of being a joint, it also keeps the two bodies in the same simulation island meaning they'll wake/sleep at the same time and be solved together on the same thread.

**Full name:** `Unity.U2D.Physics.PhysicsIgnoreJoint`  
**Docs:** [Unity.U2D.Physics.PhysicsIgnoreJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsIgnoreJoint.html)

### Fields

| Name | Summary |
|------|---------|
| `m_Id` | The base joint Id. |

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The second body the joint constrains. |
| `bodyB` | A local anchor point on the first body for the constraint. |
| `callbackTarget` | Get/Set the that event callbacks for this joint will be sent to. Care should be taken with any assigned as a callback target that isn't a as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A with call . |
| `collideConnected` | This is unused in this specific joint and is always false. Typically this gets whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | This is unused in this specific joint. Typically this would get the current angular separation error for this joint. |
| `currentConstraintForce` | This is unused in this specific joint. Typically this would get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | This is unused in this specific joint. Typically this would get the current constraint torque used by the joint, usually in newtons. |
| `currentLinearSeparationError` | This is unused in this specific joint. Typically this would get the current linear separation error for this joint. |
| `drawScale` | This is unused in this specific joint. Typically this would control the scaling of the joint drawing. |
| `forceThreshold` | This is unused in this specific joint. Typically this is the force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See . |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See . |
| `localAnchorA` | This is unused in this specific joint. Typically this is the local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | This is unused in this specific joint. Typically this is the local anchor frame constraint relative to bodyB's origin. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `torqueThreshold` | This is unused in this specific joint. Typically this is the torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | This is unused in this specific joint. Typically this would control the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | This is unused in this specific joint. Typically this would control the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsJoint)`

Create a from the specified base joint. The provided joint must be a joint type of .

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsIgnoreJointDefinition)`

Create a PhysicsIgnoreJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using .

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint})`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see ). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsIgnoreJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using .

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator ==(PhysicsIgnoreJoint, PhysicsIgnoreJoint)`

#### `operator implicit()`

Cast to the base .

**Params:**
- `joint` — The current joint.

#### `operator implicit()`

Cast to a from the base . The provided joint must be a joint type of .

**Params:**
- `joint` — The base joint to cast.

#### `operator !=(PhysicsIgnoreJoint, PhysicsIgnoreJoint)`

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with . The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using .

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsIgnoreJointDefinition

> A joint definition used to specify properties when creating a .

**Full name:** `Unity.U2D.Physics.PhysicsIgnoreJointDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsIgnoreJointDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsIgnoreJointDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `defaultDefinition` | Create a default definition. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See . |

### Methods

#### `new()`

Create a default definition.

## PhysicsJoint

> A joint is used to constrain bodies to the world or to each other in various ways. A joint is automatically destroyed when either body it is attached to is destroyed. A joint cannot exist unattached from a body.

**Full name:** `Unity.U2D.Physics.PhysicsJoint`  
**Docs:** [Unity.U2D.Physics.PhysicsJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html)

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The second body the joint constrains. |
| `bodyB` | A local anchor point on the first body for the constraint. |
| `callbackTarget` | Get/Set the object that event callbacks for this joint will be sent to. Care should be taken with any assigned as a callback target that isn't a as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A with call . |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newtons. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `drawScale` | Controls the scaling of the joint drawing. Not all joints have scalable elements but those that do will use this scaling. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See . |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See . |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `CreateJoint(PhysicsWorld, PhysicsDistanceJointDefinition)`

Create a PhysicsDistanceJoint in the world. See .

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsRelativeJointDefinition)`

Create a PhysicsRelativeJoint in the world. See .

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsIgnoreJointDefinition)`

Create an IgnoreJoint in the world. See .

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsSliderJointDefinition)`

Create a SliderJoint in the world. See .

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsHingeJointDefinition)`

Create a PhysicsHingeJoint in the world. See .

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsFixedJointDefinition)`

Create a FixedJoint in the world. See .

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsWheelJointDefinition)`

Create a WheelJoint in the world. See .

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using .

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint})`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see ). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using .

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator ==(PhysicsJoint, PhysicsJoint)`

#### `operator !=(PhysicsJoint, PhysicsJoint)`

#### `SetOwner(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint}, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `joints` — The bodies to set ownership for.
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.
- `ownerKey` — The owner key to be used. The value must be non-zero. You can use for this value although any non-zero integer will work.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use for this value although any non-zero integer will work.

#### `SetOwner(Object)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.

**Returns:** The owner key assigned.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using .

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

### Nested Types

- **JointType** — The type of joint.

### JointType

> The type of joint.

**Full name:** `Unity.U2D.Physics.PhysicsJoint.JointType`  

#### Fields

| Name | Summary |
|------|---------|
| `DistanceJoint` | Constrain the distance between a pair of bodies. |
| `FixedJoint` | Constrain a fixed translation and rotation between a pair of bodies. This joint type is also know as a Weld joint. |
| `HingeJoint` | Constrain the rotation between a pair of bodies. This joint type is also know as a Revolute joint. |
| `IgnoreJoint` | Used to ignore collision between two specific bodies. As a side effect of being a joint, it also keeps the two bodies in the same simulation island. |
| `RelativeJoint` | Constrain the relative translation and rotation between a pair of bodies. This joint type is also know as a Motor joint. |
| `SliderJoint` | Constrain the relative translation along an axis between a pair of bodies. This joint type is also know as a Prismatic joint. |
| `WheelJoint` | Constrain a translation and rotation between a pair of bodies. |

## PhysicsRelativeJoint

> A joint constraint used to control the relative movement two bodies while still being responsive to collisions. A spring controls the position and rotation and velocity control allows for simulated friction such as seen in top-down games. A typical usage is to control the movement of a dynamic body with respect to the ground.

**Full name:** `Unity.U2D.Physics.PhysicsRelativeJoint`  
**Docs:** [Unity.U2D.Physics.PhysicsRelativeJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJoint.html)

### Fields

| Name | Summary |
|------|---------|
| `m_Id` | The base joint Id. |

### Properties

| Name | Summary |
|------|---------|
| `angularVelocity` | The desired angular velocity. |
| `bodyA` | The second body the joint constrains. |
| `bodyB` | A local anchor point on the first body for the constraint. |
| `callbackTarget` | Get/Set the that event callbacks for this joint will be sent to. Care should be taken with any assigned as a callback target that isn't a as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A with call . |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newtons. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See . |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See . |
| `linearVelocity` | The desired linear velocity. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `maxForce` | The maximum linear force, usually in newtons. A value of zero is a special case which turns the limit off. |
| `maxTorque` | The maximum torque, usually in newton-meters. A value of zero is a special case which turns the limit off. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `springAngularDamping` | The spring angular damping. |
| `springAngularFrequency` | The spring angular frequency, in cycles per second. A value of zero is a special case which turns the angular spring off. |
| `springLinearDamping` | The spring linear damping. |
| `springLinearFrequency` | The spring linear frequency, in cycles per second. A value of zero is a special case which turns the linear spring off. |
| `springMaxForce` | The spring maximum linear force, usually in newtons. A value of zero is a special case which turns the force limit off. |
| `springMaxTorque` | The spring maximum torque, usually in newton-meters. A value of zero is a special case which turns the torque limit off. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsJoint)`

Create a from the specified base joint. The provided joint must be a joint type of .

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsRelativeJointDefinition)`

Create a PhysicsRelativeJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — The owner key returned when using .

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint})`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see ). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsRelativeJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using .

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator ==(PhysicsRelativeJoint, PhysicsRelativeJoint)`

#### `operator implicit()`

Cast to the base .

**Params:**
- `joint` — The current joint.

#### `operator implicit()`

Cast to a from the base . The provided joint must be a joint type of .

**Params:**
- `joint` — The base joint to cast.

#### `operator !=(PhysicsRelativeJoint, PhysicsRelativeJoint)`

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with . The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using .

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsRelativeJointDefinition

> A joint definition used to specify properties when creating a .

**Full name:** `Unity.U2D.Physics.PhysicsRelativeJointDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsRelativeJointDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJointDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `angularVelocity` | The desired angular velocity. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Create a default definition. |
| `drawScale` | Controls the scaling of the joint drawing. Not all joints have scalable elements but those that do will use this scaling. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `linearVelocity` | The desired linear velocity. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `maxForce` | The maximum linear force, usually in newtons. A value of zero is a special case which turns the limit off. |
| `maxTorque` | The maximum torque, usually in newton-meters. A value of zero is a special case which turns the limit off. |
| `springAngularDamping` | The spring angular damping. Use 1 for critical damping. |
| `springAngularFrequency` | The spring angular frequency, in cycles per second. A value of zero is a special case which turns the angular spring off. |
| `springLinearDamping` | The spring linear damping. Use 1 for critical damping. |
| `springLinearFrequency` | The spring linear frequency, in cycles per second. A value of zero is a special case which turns the linear spring off. |
| `springMaxForce` | The spring maximum linear force, usually in newtons. A value of zero is a special case which turns the force limit off. |
| `springMaxTorque` | The spring maximum torque, usually in newton-meters. A value of zero is a special case which turns the torque limit off. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See . |

### Methods

#### `new()`

Create a default definition.

#### `new(bool)`

Create a default definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsSliderJoint

> A joint that requires defining a line of motion defined by the local anchor A. Body B may slide along the axis defined by the local anchor A. Body B cannot rotate relative to body A. The joint translation is zero when the local anchor origins coincide in world space. The joint uses local anchors so that the initial configuration can violate the constraint slightly.

**Full name:** `Unity.U2D.Physics.PhysicsSliderJoint`  
**Docs:** [Unity.U2D.Physics.PhysicsSliderJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJoint.html)

### Fields

| Name | Summary |
|------|---------|
| `m_Id` | The base joint Id. |

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The second body the joint constrains. |
| `bodyB` | A local anchor point on the first body for the constraint. |
| `callbackTarget` | Get/Set the that event callbacks for this joint will be sent to. Care should be taken with any assigned as a callback target that isn't a as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A with call . |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newtons. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `currentMotorForce` | Get the current motor force, usually in newtons. |
| `currentSpeed` | Get the current joint translation speed, usually in meters per second. |
| `currentTranslation` | Get the current joint translation, usually in meters. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `enableLimit` | Enable/Disable the joint translation limit. |
| `enableMotor` | Enable/Disable the joint motor. |
| `enableSpring` | Enable/Disable a spring along the slider joint axis. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See . |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See . |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `lowerTranslationLimit` | Get the lower translation limit. |
| `maxMotorForce` | The maximum force the motor can apply, usually in newtons. |
| `motorSpeed` | The desired motor speed, usually in meters per second. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `springDamping` | The spring damping, non-dimensional. |
| `springFrequency` | The spring stiffness, in cycles per second. |
| `springTargetTranslation` | The spring target translation, usually in meters. The spring-damper will drive to this translation. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `upperTranslationLimit` | Get the upper translation limit. |
| `userData` | Get/Set that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsJoint)`

Create a from the specified base joint. The provided joint must be a joint type of .

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsSliderJointDefinition)`

Create a PhysicsSliderJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using .

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint})`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see ). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsSliderJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using .

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator ==(PhysicsSliderJoint, PhysicsSliderJoint)`

#### `operator implicit()`

Cast to the base .

**Params:**
- `joint` — The current joint.

#### `operator implicit()`

Cast to a from the base . The provided joint must be a joint type of .

**Params:**
- `joint` — The base joint to cast.

#### `operator !=(PhysicsSliderJoint, PhysicsSliderJoint)`

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with . The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using .

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsSliderJointDefinition

> A joint definition used to specify properties when creating a .

**Full name:** `Unity.U2D.Physics.PhysicsSliderJointDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsSliderJointDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJointDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Get a default definition. |
| `drawScale` | Controls the scaling of the joint drawing. Not all joints have scalable elements but those that do will use this scaling. |
| `enableLimit` | Enable/disable the joint translation limit. |
| `enableMotor` | Enable/disable the joint motor. |
| `enableSpring` | Enable/Disable a spring along the slider joint axis. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `lowerTranslationLimit` | The lower translation limit of this joint. This will be clamped to a lower stable limit. |
| `maxMotorForce` | The maximum force the motor can apply, usually in newtons. |
| `motorSpeed` | The desired motor speed, usually in meters per second. |
| `springDamping` | The spring damping, non-dimensional. Use 1 for critical damping. |
| `springFrequency` | The spring stiffness frequency, in cycles per second. |
| `springTargetTranslation` | The spring target translation, usually in meters. The spring-damper will drive to this translation. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `upperTranslationLimit` | The upper translation limit of this joint. Must be greater than or equal to the minimum length. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See . |

### Methods

#### `new()`

Create a default definition.

#### `new(bool)`

Create a default definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsWheelJoint

> A joint that requires defining a line of motion using an axis and an anchor point. The joint translation is zero when the local anchors coincide in world space. The joint uses local anchors so that the initial configuration can violate the constraint slightly.

**Full name:** `Unity.U2D.Physics.PhysicsWheelJoint`  
**Docs:** [Unity.U2D.Physics.PhysicsWheelJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJoint.html)

### Fields

| Name | Summary |
|------|---------|
| `m_Id` | The base joint Id. |

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The second body the joint constrains. |
| `bodyB` | A local anchor point on the first body for the constraint. |
| `callbackTarget` | Get/Set the that event callbacks for this joint will be sent to. Care should be taken with any assigned as a callback target that isn't a as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A with call . |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newtons. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `currentMotorTorque` | Get the current motor torque, usually in newtons. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `enableLimit` | Enable/disable the joint limit. |
| `enableMotor` | Enable/Disable the joint motor. |
| `enableSpring` | Enable/Disable a spring along the joint axis. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See . |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See . |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `lowerTranslationLimit` | Get/Set the lower translation limit. |
| `maxMotorTorque` | The maximum torque the motor can apply, usually in newtons. |
| `motorSpeed` | The desired motor speed, usually in degrees per second. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `springDamping` | The spring damping, non-dimensional. |
| `springFrequency` | The spring stiffness, in cycles per second. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `upperTranslationLimit` | Get/Set the upper translation limit. |
| `userData` | Get/Set that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsJoint)`

Create a from the specified base joint. The provided joint must be a joint type of .

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsWheelJointDefinition)`

Create a PhysicsWheelJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using .

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint})`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see ). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsWheelJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using .

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator ==(PhysicsWheelJoint, PhysicsWheelJoint)`

#### `operator implicit()`

Cast to the base .

**Params:**
- `joint` — The current joint.

#### `operator implicit()`

Cast to a from the base . The provided joint must be a joint type of .

**Params:**
- `joint` — The base joint to cast.

#### `operator !=(PhysicsWheelJoint, PhysicsWheelJoint)`

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with . The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using .

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsWheelJointDefinition

> A joint definition used to specify properties when creating a .

**Full name:** `Unity.U2D.Physics.PhysicsWheelJointDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsWheelJointDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJointDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Get a default definition. |
| `drawScale` | Controls the scaling of the joint drawing. Not all joints have scalable elements but those that do will use this scaling. |
| `enableLimit` | Enable/disable the joint translation limit. |
| `enableMotor` | Enable/disable the joint motor. |
| `enableSpring` | Enable/Disable a spring along the joint axis. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `lowerTranslationLimit` | The lower translation limit. |
| `maxMotorTorque` | The maximum torque the motor can apply, usually in newton-meters. |
| `motorSpeed` | The desired motor speed, usually in degrees per second. |
| `springDamping` | The spring damping, non-dimensional. Use 1 for critical damping. |
| `springFrequency` | The spring stiffness frequency, in cycles per second. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `upperTranslationLimit` | The upper translation limit. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See . |

### Methods

#### `new()`

Create a default definition.

#### `new(bool)`

Create a default definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
