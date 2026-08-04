---
name: unity-physicscore2d-joints-api
description: Authoritative Unity 6000.7 PhysicsCore2D API reference for Joints. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsDistanceJoint, PhysicsDistanceJointDefinition, PhysicsFixedJoint, PhysicsFixedJointDefinition, PhysicsHingeJoint, PhysicsHingeJointDefinition, PhysicsIgnoreJoint, PhysicsIgnoreJointDefinition, PhysicsJoint, PhysicsRelativeJoint, PhysicsRelativeJointDefinition, PhysicsSliderJoint, PhysicsSliderJointDefinition, PhysicsWheelJoint, PhysicsWheelJointDefinition. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Joints

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.7, so it should be treated as the source of truth for member names, signatures, and documentation strings.

Top-level types in this file: `PhysicsDistanceJoint`, `PhysicsDistanceJointDefinition`, `PhysicsFixedJoint`, `PhysicsFixedJointDefinition`, `PhysicsHingeJoint`, `PhysicsHingeJointDefinition`, `PhysicsIgnoreJoint`, `PhysicsIgnoreJointDefinition`, `PhysicsJoint`, `PhysicsRelativeJoint`, `PhysicsRelativeJointDefinition`, `PhysicsSliderJoint`, `PhysicsSliderJointDefinition`, `PhysicsWheelJoint`, `PhysicsWheelJointDefinition`.

## PhysicsDistanceJoint

> Connects an anchor point on body A with an anchor point on body B via a line segment of a specified distance.

**Full name:** `Unity.U2D.Physics.PhysicsDistanceJoint`

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `callbackTarget` | Get/Set the Object that event callbacks for this joint will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.JointThresholdEvent with call PhysicsCallbacks.IJointThresholdCallback. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newton-meters. |
| `currentDistance` | Get the current distance. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `currentMotorForce` | The current motor force, usually in newtons. |
| `definition` | Get or set the joint definition. Reading returns the joint's current configuration, including PhysicsDistanceJoint.bodyA and PhysicsDistanceJoint.bodyB. Writing applies every configurable property in place but does not change the connected bodies, which are fixed when the joint is created. |
| `distance` | The desired distance constraint i.e. the rest length of this joint. This has a lower stable limit of just above zero. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `enableLimit` | Enable/Disable the joint distance limit. |
| `enableMotor` | Enable/Disable the joint motor. |
| `enableSpring` | Enable/Disable the spring behaviour. If false then the joint will be rigid, overriding the limit and motor. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See PhysicsJoint.SetOwner. |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See PhysicsJoint.JointType. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `maxDistanceLimit` | Maximum distance limit of this joint. |
| `maxMotorForce` | The maximum force the motor can apply, usually in newtons. |
| `minDistanceLimit` | Minimum distance limit of this joint. This will be clamped to a lower stable limit. |
| `motorSpeed` | The desired motor speed, usually in meters per second. |
| `owner` | The owner object associated with this joint, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsDistanceJoint.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `springDamping` | The spring linear damping, non-dimensional. |
| `springFrequency` | The spring linear stiffness frequency, in cycles per second. |
| `springLowerForce` | The lower spring force controls how much tension the spring can sustain. |
| `springUpperForce` | The upper spring force controls how much compression the spring can sustain. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a joint from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `new(PhysicsJoint)`

Create a PhysicsDistanceJoint from the specified base joint. The provided joint must be a joint type of PhysicsJoint.JointType.DistanceJoint.

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsDistanceJointDefinition)`

Create a PhysicsDistanceJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with PhysicsJoint.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsJoint>)`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see PhysicsJoint.SetOwner). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsDistanceJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using PhysicsJoint.SetOwner.

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator implicit(PhysicsDistanceJoint)`

Cast to the base PhysicsJoint.

**Params:**
- `joint` — The current joint.

#### `operator implicit(PhysicsJoint)`

Cast to a PhysicsDistanceJoint from the base PhysicsJoint. The provided joint must be a joint type of PhysicsJoint.JointType.DistanceJoint.

**Params:**
- `joint` — The base joint to cast.

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with PhysicsJoint.Destroy. The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to PhysicsJoint.Destroy when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsDistanceJointDefinition

> A joint definition used to specify properties when creating a PhysicsDistanceJoint.

**Full name:** `Unity.U2D.Physics.PhysicsDistanceJointDefinition`

### Properties

| Name | Summary |
|------|---------|
| `autoAnchorA` | When set, PhysicsDistanceJointDefinition.localAnchorA is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `autoAnchorB` | When set, PhysicsDistanceJointDefinition.localAnchorB is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `autoDistance` | When set, PhysicsDistanceJointDefinition.distance is recomputed at create from the world separation of the two anchors. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Get a default PhysicsDistanceJoint definition. |
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
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See PhysicsJoint.worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsDistanceJoint definition.

#### `new(bool)`

Create a default PhysicsDistanceJoint definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsFixedJoint

> A joint to constrain a pair of bodies together rigidly. This constraint provides springs to mimic soft-body simulation. The approximate solver cannot always hold many bodies together completely rigidly.

**Full name:** `Unity.U2D.Physics.PhysicsFixedJoint`

### Properties

| Name | Summary |
|------|---------|
| `angularDamping` | Angular damping, non-dimensional. Use 1 for critical damping. |
| `angularFrequency` | Angular stiffness in cycles per second. Use zero for maximum stiffness. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `callbackTarget` | Get/Set the Object that event callbacks for this joint will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.JointThresholdEvent with call PhysicsCallbacks.IJointThresholdCallback. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newton-meters. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `definition` | Get or set the joint definition. Reading returns the joint's current configuration, including PhysicsFixedJoint.bodyA and PhysicsFixedJoint.bodyB. Writing applies every configurable property in place but does not change the connected bodies, which are fixed when the joint is created. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See PhysicsJoint.SetOwner. |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See PhysicsJoint.JointType. |
| `linearDamping` | Linear damping, non-dimensional. Use 1 for critical damping. |
| `linearFrequency` | Linear stiffness in cycles per second. Use zero for maximum stiffness. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `owner` | The owner object associated with this joint, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsFixedJoint.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a joint from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `new(PhysicsJoint)`

Create a PhysicsFixedJoint from the specified base joint. The provided joint must be a joint type of PhysicsJoint.JointType.FixedJoint.

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsFixedJointDefinition)`

Create a PhysicsFixedJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with PhysicsJoint.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsJoint>)`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see PhysicsJoint.SetOwner). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsFixedJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using PhysicsJoint.SetOwner.

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator implicit(PhysicsFixedJoint)`

Cast to the base PhysicsJoint.

**Params:**
- `joint` — The current joint.

#### `operator implicit(PhysicsJoint)`

Cast to a PhysicsFixedJoint from the base PhysicsJoint. The provided joint must be a joint type of PhysicsJoint.JointType.FixedJoint.

**Params:**
- `joint` — The base joint to cast.

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with PhysicsJoint.Destroy. The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to PhysicsJoint.Destroy when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsFixedJointDefinition

> A joint definition used to specify properties when creating a PhysicsFixedJoint.

**Full name:** `Unity.U2D.Physics.PhysicsFixedJointDefinition`

### Properties

| Name | Summary |
|------|---------|
| `angularDamping` | Angular damping, non-dimensional. Use 1 for critical damping. |
| `angularFrequency` | Angular stiffness frequency, in cycles per second. Use zero for maximum stiffness. |
| `autoAnchorA` | When set, PhysicsFixedJointDefinition.localAnchorA is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `autoAnchorB` | When set, PhysicsFixedJointDefinition.localAnchorB is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Get a default PhysicsFixedJoint definition. |
| `drawScale` | Controls the scaling of the joint drawing. Not all joints have scalable elements but those that do will use this scaling. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `linearDamping` | Linear damping, non-dimensional. Use 1 for critical damping. |
| `linearFrequency` | Linear stiffness frequency, in cycles per second. Use zero for maximum stiffness. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See PhysicsJoint.worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsFixedJoint definition.

#### `new(bool)`

Create a default PhysicsFixedJoint definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsHingeJoint

> A joint where an anchor point on body B is fixed to an anchor point on body A. This joint allows relative rotation.

**Full name:** `Unity.U2D.Physics.PhysicsHingeJoint`

### Properties

| Name | Summary |
|------|---------|
| `angle` | Get the current angle of the joint, in degrees. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `callbackTarget` | Get/Set the Object that event callbacks for this joint will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.JointThresholdEvent with call PhysicsCallbacks.IJointThresholdCallback. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newton-meters. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `currentMotorTorque` | Get the current motor torque. |
| `definition` | Get or set the joint definition. Reading returns the joint's current configuration, including PhysicsHingeJoint.bodyA and PhysicsHingeJoint.bodyB. Writing applies every configurable property in place but does not change the connected bodies, which are fixed when the joint is created. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `enableLimit` | Enable/Disable the joint rotation limit. |
| `enableMotor` | Enable/Disable the joint motor. |
| `enableSpring` | Enable/Disable the rotational spring. |
| `enableUnpinned` | Enable/Disable unpinned mode where only Body A is affected and body B and its local anchor point is ignored. Body B must still be assigned so it is typical to assign a static ground body, preferably shared/reused. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See PhysicsJoint.SetOwner. |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See PhysicsJoint.JointType. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `lowerAngleLimit` | Get the lower angle limit, in degrees. |
| `maxMotorTorque` | The maximum torque the motor can apply, usually in newton-meters. |
| `motorSpeed` | The desired motor speed, usually in degrees per second. |
| `owner` | The owner object associated with this joint, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsHingeJoint.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `springDamping` | The spring damping, non-dimensional. |
| `springFrequency` | The spring stiffness, in cycles per second. |
| `springTargetAngle` | The spring target angle, in degrees. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `upperAngleLimit` | Get the upper angle limit, in degrees. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a joint from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `new(PhysicsJoint)`

Create a PhysicsHingeJoint from the specified base joint. The provided joint must be a joint type of PhysicsJoint.JointType.HingeJoint.

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsHingeJointDefinition)`

Create a PhysicsHingeJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with PhysicsJoint.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsJoint>)`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see PhysicsJoint.SetOwner). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsHingeJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using PhysicsJoint.SetOwner.

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator implicit(PhysicsHingeJoint)`

Cast to the base PhysicsJoint.

**Params:**
- `joint` — The current joint.

#### `operator implicit(PhysicsJoint)`

Cast to a PhysicsHingeJoint from the base PhysicsJoint. The provided joint must be a joint type of PhysicsJoint.JointType.HingeJoint.

**Params:**
- `joint` — The base joint to cast.

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with PhysicsJoint.Destroy. The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to PhysicsJoint.Destroy when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsHingeJointDefinition

> A joint definition used to specify properties when creating a PhysicsHingeJoint.

**Full name:** `Unity.U2D.Physics.PhysicsHingeJointDefinition`

### Properties

| Name | Summary |
|------|---------|
| `autoAnchorA` | When set, PhysicsHingeJointDefinition.localAnchorA is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `autoAnchorB` | When set, PhysicsHingeJointDefinition.localAnchorB is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Create a default PhysicsHingeJoint definition. |
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
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See PhysicsJoint.worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsHingeJoint definition.

#### `new(bool)`

Create a default PhysicsHingeJoint definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsIgnoreJoint

> A joint used to ignore collision between two specific bodies. As a side effect of being a joint, it also keeps the two bodies in the same simulation island meaning they'll wake/sleep at the same time and be solved together on the same thread.

**Full name:** `Unity.U2D.Physics.PhysicsIgnoreJoint`

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `callbackTarget` | Get/Set the Object that event callbacks for this joint will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.JointThresholdEvent with call PhysicsCallbacks.IJointThresholdCallback. |
| `collideConnected` | This is unused in this specific joint and is always false. Typically this gets whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | This is unused in this specific joint. Typically this would get the current angular separation error for this joint. |
| `currentConstraintForce` | This is unused in this specific joint. Typically this would get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | This is unused in this specific joint. Typically this would get the current constraint torque used by the joint, usually in newton-meters. |
| `currentLinearSeparationError` | This is unused in this specific joint. Typically this would get the current linear separation error for this joint. |
| `definition` | Get or set the joint definition. Reading returns the joint's current configuration, including PhysicsIgnoreJoint.bodyA and PhysicsIgnoreJoint.bodyB. Writing applies every configurable property in place but does not change the connected bodies, which are fixed when the joint is created. |
| `drawScale` | This is unused in this specific joint. Typically this would control the scaling of the joint drawing. |
| `forceThreshold` | This is unused in this specific joint. Typically this is the force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See PhysicsJoint.SetOwner. |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See PhysicsJoint.JointType. |
| `localAnchorA` | This is unused in this specific joint. Typically this is the local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | This is unused in this specific joint. Typically this is the local anchor frame constraint relative to bodyB's origin. |
| `owner` | The owner object associated with this joint, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsIgnoreJoint.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `torqueThreshold` | This is unused in this specific joint. Typically this is the torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | This is unused in this specific joint. Typically this would control the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | This is unused in this specific joint. Typically this would control the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a joint from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `new(PhysicsJoint)`

Create a PhysicsIgnoreJoint from the specified base joint. The provided joint must be a joint type of PhysicsJoint.JointType.IgnoreJoint.

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsIgnoreJointDefinition)`

Create a PhysicsIgnoreJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with PhysicsJoint.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsJoint>)`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see PhysicsJoint.SetOwner). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsIgnoreJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using PhysicsJoint.SetOwner.

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator implicit(PhysicsIgnoreJoint)`

Cast to the base PhysicsJoint.

**Params:**
- `joint` — The current joint.

#### `operator implicit(PhysicsJoint)`

Cast to a PhysicsIgnoreJoint from the base PhysicsJoint. The provided joint must be a joint type of PhysicsJoint.JointType.IgnoreJoint.

**Params:**
- `joint` — The base joint to cast.

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with PhysicsJoint.Destroy. The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to PhysicsJoint.Destroy when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsIgnoreJointDefinition

> A joint definition used to specify properties when creating a PhysicsIgnoreJoint.

**Full name:** `Unity.U2D.Physics.PhysicsIgnoreJointDefinition`

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `defaultDefinition` | Create a default PhysicsIgnoreJoint definition. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See PhysicsJoint.worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsIgnoreJoint definition.

## PhysicsJoint

> A joint is used to constrain bodies to the world or to each other in various ways. A joint is automatically destroyed when either body it is attached to is destroyed. A joint cannot exist unattached from a body.

**Full name:** `Unity.U2D.Physics.PhysicsJoint`

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `callbackTarget` | Get/Set the Object object that event callbacks for this joint will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.JointThresholdEvent with call PhysicsCallbacks.IJointThresholdCallback. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newton-meters. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `drawScale` | Controls the scaling of the joint drawing. Not all joints have scalable elements but those that do will use this scaling. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See PhysicsJoint.SetOwner. |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See PhysicsJoint.JointType. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `owner` | The owner object associated with this joint, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsJoint.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a joint from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `CreateJoint(PhysicsWorld, PhysicsDistanceJointDefinition)`

Create a PhysicsDistanceJoint in the world. See PhysicsDistanceJoint.Create.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsRelativeJointDefinition)`

Create a PhysicsRelativeJoint in the world. See PhysicsRelativeJoint.Create.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsIgnoreJointDefinition)`

Create an IgnoreJoint in the world. See PhysicsIgnoreJoint.Create.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsSliderJointDefinition)`

Create a SliderJoint in the world. See PhysicsSliderJoint.Create.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsHingeJointDefinition)`

Create a PhysicsHingeJoint in the world. See PhysicsHingeJoint.Create.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsFixedJointDefinition)`

Create a FixedJoint in the world. See PhysicsFixedJoint.Create.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWorld, PhysicsWheelJointDefinition)`

Create a WheelJoint in the world. See PhysicsWheelJoint.Create.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with PhysicsJoint.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsJoint>)`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see PhysicsJoint.SetOwner). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `DestroyBatch(ReadOnlySpan<PhysicsJoint>, int)`

Destroy a batch of joints. Any invalid joints are ignored. A joint owned by a different owner key is skipped and left valid; a joint with no owner, or one matching the given owner key, is destroyed. One summary warning reports how many joints were skipped this way, rather than one warning per joint.

**Params:**
- `joints` — The joints to destroy.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using PhysicsJoint.SetOwner.

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `SetOwner(ReadOnlySpan<PhysicsJoint>, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `joints` — The bodies to set ownership for.
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.
- `ownerKey` — The owner key to be used. The value must be non-zero. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwner(Object)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.

**Returns:** The owner key assigned.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

#### `SetOwnerUserData(ReadOnlySpan<PhysicsJoint>, ReadOnlySpan<PhysicsUserData>, int)`

Set PhysicsUserData on a batch of joints that can be used for any purpose, typically by the owner only. The joints and userDatas spans must be the same length; joints[n] receives userDatas[n].

**Params:**
- `joints` — The joints to set the owner user data on.
- `userDatas` — The user data to set, one entry per joint.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

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

### Properties

| Name | Summary |
|------|---------|
| `angularVelocity` | The desired angular velocity. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `callbackTarget` | Get/Set the Object that event callbacks for this joint will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.JointThresholdEvent with call PhysicsCallbacks.IJointThresholdCallback. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newton-meters. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `definition` | Get or set the joint definition. Reading returns the joint's current configuration, including PhysicsRelativeJoint.bodyA and PhysicsRelativeJoint.bodyB. Writing applies every configurable property in place but does not change the connected bodies, which are fixed when the joint is created. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See PhysicsJoint.SetOwner. |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See PhysicsJoint.JointType. |
| `linearVelocity` | The desired linear velocity. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `maxForce` | The maximum linear force, usually in newtons. A value of zero is a special case which turns the limit off. |
| `maxTorque` | The maximum torque, usually in newton-meters. A value of zero is a special case which turns the limit off. |
| `owner` | The owner object associated with this joint, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsRelativeJoint.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `springAngularDamping` | The spring angular damping. |
| `springAngularFrequency` | The spring angular frequency, in cycles per second. A value of zero is a special case which turns the angular spring off. |
| `springLinearDamping` | The spring linear damping. |
| `springLinearFrequency` | The spring linear frequency, in cycles per second. A value of zero is a special case which turns the linear spring off. |
| `springMaxForce` | The spring maximum linear force, usually in newtons. A value of zero is a special case which turns the force limit off. |
| `springMaxTorque` | The spring maximum torque, usually in newton-meters. A value of zero is a special case which turns the torque limit off. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a joint from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `new(PhysicsJoint)`

Create a PhysicsRelativeJoint from the specified base joint. The provided joint must be a joint type of PhysicsJoint.JointType.RelativeJoint.

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsRelativeJointDefinition)`

Create a PhysicsRelativeJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with PhysicsJoint.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — The owner key returned when using PhysicsJoint.SetOwner.

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsJoint>)`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see PhysicsJoint.SetOwner). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsRelativeJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using PhysicsJoint.SetOwner.

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator implicit(PhysicsRelativeJoint)`

Cast to the base PhysicsJoint.

**Params:**
- `joint` — The current joint.

#### `operator implicit(PhysicsJoint)`

Cast to a PhysicsRelativeJoint from the base PhysicsJoint. The provided joint must be a joint type of PhysicsJoint.JointType.RelativeJoint.

**Params:**
- `joint` — The base joint to cast.

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with PhysicsJoint.Destroy. The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to PhysicsJoint.Destroy when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsRelativeJointDefinition

> A joint definition used to specify properties when creating a PhysicsRelativeJoint.

**Full name:** `Unity.U2D.Physics.PhysicsRelativeJointDefinition`

### Properties

| Name | Summary |
|------|---------|
| `angularVelocity` | The desired angular velocity. |
| `autoAnchorA` | When set, PhysicsRelativeJointDefinition.localAnchorA is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `autoAnchorB` | When set, PhysicsRelativeJointDefinition.localAnchorB is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Create a default PhysicsRelativeJoint definition. |
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
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See PhysicsJoint.worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsRelativeJoint definition.

#### `new(bool)`

Create a default PhysicsRelativeJoint definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsSliderJoint

> A joint that requires defining a line of motion defined by the local anchor A. Body B may slide along the axis defined by the local anchor A. Body B cannot rotate relative to body A. The joint translation is zero when the local anchor origins coincide in world space. The joint uses local anchors so that the initial configuration can violate the constraint slightly.

**Full name:** `Unity.U2D.Physics.PhysicsSliderJoint`

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `callbackTarget` | Get/Set the Object that event callbacks for this joint will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.JointThresholdEvent with call PhysicsCallbacks.IJointThresholdCallback. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newton-meters. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `currentMotorForce` | Get the current motor force, usually in newtons. |
| `currentSpeed` | Get the current joint translation speed, usually in meters per second. |
| `currentTranslation` | Get the current joint translation, usually in meters. |
| `definition` | Get or set the joint definition. Reading returns the joint's current configuration, including PhysicsSliderJoint.bodyA and PhysicsSliderJoint.bodyB. Writing applies every configurable property in place but does not change the connected bodies, which are fixed when the joint is created. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `enableLimit` | Enable/Disable the joint translation limit. |
| `enableMotor` | Enable/Disable the joint motor. |
| `enableSpring` | Enable/Disable a spring along the slider joint axis. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See PhysicsJoint.SetOwner. |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See PhysicsJoint.JointType. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `lowerTranslationLimit` | Get the lower translation limit. |
| `maxMotorForce` | The maximum force the motor can apply, usually in newtons. |
| `motorSpeed` | The desired motor speed, usually in meters per second. |
| `owner` | The owner object associated with this joint, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsSliderJoint.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `springDamping` | The spring damping, non-dimensional. |
| `springFrequency` | The spring stiffness, in cycles per second. |
| `springTargetTranslation` | The spring target translation, usually in meters. The spring-damper will drive to this translation. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `upperTranslationLimit` | Get the upper translation limit. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a joint from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `new(PhysicsJoint)`

Create a PhysicsSliderJoint from the specified base joint. The provided joint must be a joint type of PhysicsJoint.JointType.SliderJoint.

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsSliderJointDefinition)`

Create a PhysicsSliderJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with PhysicsJoint.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsJoint>)`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see PhysicsJoint.SetOwner). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsSliderJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using PhysicsJoint.SetOwner.

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator implicit(PhysicsSliderJoint)`

Cast to the base PhysicsJoint.

**Params:**
- `joint` — The current joint.

#### `operator implicit(PhysicsJoint)`

Cast to a PhysicsSliderJoint from the base PhysicsJoint. The provided joint must be a joint type of PhysicsJoint.JointType.SliderJoint.

**Params:**
- `joint` — The base joint to cast.

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with PhysicsJoint.Destroy. The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to PhysicsJoint.Destroy when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsSliderJointDefinition

> A joint definition used to specify properties when creating a PhysicsSliderJoint.

**Full name:** `Unity.U2D.Physics.PhysicsSliderJointDefinition`

### Properties

| Name | Summary |
|------|---------|
| `autoAnchorA` | When set, PhysicsSliderJointDefinition.localAnchorA is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `autoAnchorB` | When set, PhysicsSliderJointDefinition.localAnchorB is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Get a default PhysicsSliderJoint definition. |
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
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See PhysicsJoint.worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsSliderJoint definition.

#### `new(bool)`

Create a default PhysicsSliderJoint definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

## PhysicsWheelJoint

> A joint that requires defining a line of motion using an axis and an anchor point. The joint translation is zero when the local anchors coincide in world space. The joint uses local anchors so that the initial configuration can violate the constraint slightly.

**Full name:** `Unity.U2D.Physics.PhysicsWheelJoint`

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `callbackTarget` | Get/Set the Object that event callbacks for this joint will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.JointThresholdEvent with call PhysicsCallbacks.IJointThresholdCallback. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `currentAngularSeparationError` | Get the current angular separation error for this joint, in degrees. This does not consider admissible movement. |
| `currentConstraintForce` | Get the current constraint force used by the joint, usually in newtons. |
| `currentConstraintTorque` | Get the current constraint torque used by the joint, usually in newton-meters. |
| `currentLinearSeparationError` | Get the current linear separation error for this joint, usually in meters. This does not consider admissible movement. |
| `currentMotorTorque` | Get the current motor torque, usually in newton-meters. |
| `definition` | Get or set the joint definition. Reading returns the joint's current configuration, including PhysicsWheelJoint.bodyA and PhysicsWheelJoint.bodyB. Writing applies every configurable property in place but does not change the connected bodies, which are fixed when the joint is created. |
| `drawScale` | Controls the scaling of the joint drawing. |
| `enableLimit` | Enable/disable the joint limit. |
| `enableMotor` | Enable/Disable the joint motor. |
| `enableSpring` | Enable/Disable a spring along the joint axis. |
| `forceThreshold` | The force threshold beyond which a joint event will be produced. |
| `isOwned` | Get if the joint is owned. See PhysicsJoint.SetOwner. |
| `isValid` | Checks if the joint is valid. |
| `jointType` | Gets the joint type. See PhysicsJoint.JointType. |
| `localAnchorA` | The local anchor frame constraint relative to bodyA's origin. |
| `localAnchorB` | The local anchor frame constraint relative to bodyB's origin. |
| `lowerTranslationLimit` | Get/Set the lower translation limit. |
| `maxMotorTorque` | The maximum torque the motor can apply, usually in newton-meters. |
| `motorSpeed` | The desired motor speed, usually in degrees per second. |
| `owner` | The owner object associated with this joint, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsWheelJoint.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `springDamping` | The spring damping, non-dimensional. |
| `springFrequency` | The spring stiffness, in cycles per second. |
| `torqueThreshold` | The torque threshold beyond which a joint event will be produced. |
| `tuningDamping` | Controls the joint stiffness damping, non-dimensional. Use 1 for critical damping. |
| `tuningFrequency` | Controls the joint stiffness frequency, in cycles per second. |
| `upperTranslationLimit` | Get/Set the upper translation limit. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a joint from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `new(PhysicsJoint)`

Create a PhysicsWheelJoint from the specified base joint. The provided joint must be a joint type of PhysicsJoint.JointType.WheelJoint.

**Params:**
- `physicsJoint` — The base joint to cast.

#### `Create(PhysicsWorld, PhysicsWheelJointDefinition)`

Create a PhysicsWheelJoint in the specified world.

**Params:**
- `world` — The world to create the joint in.
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `Destroy(int)`

Destroy the joint. If the object is owned with PhysicsJoint.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the joint will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

**Returns:** If the joint was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsJoint>)`

Destroy a batch of joints. Owned joints will produce a warning and will not be destroyed (see PhysicsJoint.SetOwner). Any invalid joints will be ignored.

**Params:**
- `joints` — The joints to destroy.

#### `Draw()`

Draw a PhysicsJoint that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsWheelJoint)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this joint as specified using PhysicsJoint.SetOwner.

**Returns:** The owner object associated with this joint or NULL if no owner has been specified.

#### `operator implicit(PhysicsWheelJoint)`

Cast to the base PhysicsJoint.

**Params:**
- `joint` — The current joint.

#### `operator implicit(PhysicsJoint)`

Cast to a PhysicsWheelJoint from the base PhysicsJoint. The provided joint must be a joint type of PhysicsJoint.JointType.WheelJoint.

**Params:**
- `joint` — The base joint to cast.

#### `SetOwner(Object)`

Set the (optional) owner object associated with this joint and return an owner key that must be specified when destroying the joint with PhysicsJoint.Destroy. The physics system provides access to all objects, including the ability to destroy them so this feature can be used to stop accidental destruction of objects that are owned by other objects. You can only set the owner once, multiple attempts will produce a warning. The lifetime of the specified owner object is not linked to this joint i.e. this joint will still be owned by the owner object, even if it is destroyed. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this joint. This can be NULL if not required.

**Returns:** An owner key that must be passed to PhysicsJoint.Destroy when destroying the joint.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsJoint.SetOwner.

#### `ToString()`

#### `WakeBodies()`

Wake the pair of bodies the joint is constraining.

## PhysicsWheelJointDefinition

> A joint definition used to specify properties when creating a PhysicsWheelJoint.

**Full name:** `Unity.U2D.Physics.PhysicsWheelJointDefinition`

### Properties

| Name | Summary |
|------|---------|
| `autoAnchorA` | When set, PhysicsWheelJointDefinition.localAnchorA is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `autoAnchorB` | When set, PhysicsWheelJointDefinition.localAnchorB is recomputed from the bodies' current placement at create so both anchor frames coincide in world space. |
| `bodyA` | The first body the joint constrains. |
| `bodyB` | The second body the joint constrains. |
| `collideConnected` | Whether the shapes on the pair of bodies can come into contact. |
| `defaultDefinition` | Get a default PhysicsWheelJoint definition. |
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
| `worldDrawing` | Controls whether this joint is automatically drawn when the world is drawn. See PhysicsJoint.worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsWheelJoint definition.

#### `new(bool)`

Create a default PhysicsWheelJoint definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
