# PhysicsCore2D — Bodies

_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsBody`, `PhysicsBodyDefinition`.

## PhysicsBody

> A body is contained within a world and has 3 degrees-of-freedom, two for position and one for rotation. A body can have forces, torques and impulses applied to it. A body has three distinct types: - Static: This type of body does not move under simulation and behaves as if it has infinite mass, essentially an immovable object. Static bodies never interact with other Static or Kinematic bodies. - Dynamic: This type of body is fully simulated and moves according to forces and torques applied to its linearangular velocities. It can interact with all other body types. It always has finite, non-zero mass. - Kinematic: This type of body moves under simulation and moves according to its linearangular velocities and never uses forces or torques. It only interacts with Dynamic body types. It behaves as if it has infinite mass. A body is automatically destroyed when the world it is in is destroyed. A body cannot exist outside a world.

**Full name:** `Unity.U2D.Physics.PhysicsBody`  
**Docs:** [Unity.U2D.Physics.PhysicsBody](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html)

### Properties

| Name | Summary |
|------|---------|
| `angularDamping` | The angular damping of the body. This will reduce the angular velocity over time. See PhysicsBody._angularVelocity. |
| `angularVelocity` | The angular velocity of the body, in degrees per second. |
| `awake` | The awake state of the body. |
| `callbackTarget` | Get/Set the Object that event callbacks for this body will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.BodyUpdateEvent with call PhysicsCallbacks.IBodyUpdateCallback. |
| `collisionThreshold` | A threshold used to control when continuous collision detection is used when a body moves. The value is used to compare the body linear velocity movement against the extents of all the shapes added to the body scaled by this threshold. If the movement exceeds the extents scaled by the threshold then continuous collision detection is used to stop tunneling. Lower values reduce the distance the body must move before continuous collision detection is used and can have a considerable impact on performance! Higher values increase the distance the body must move before continuous collision detection is used. Too low a threshold will result in continuous collision detection being used more often therefore affecting performance so this should be limited to specific bodies only. The default threshold is 0.5 which equates to half the total shape extents. The threshold is clamped to a range of 0.0 to 1.0 with 0.0 meaning continuous collision detection will always be used. |
| `constraints` | Get/Set the degrees of freedom constraints (locks) for the body of Linear X, Linear Y and Rotation Z. |
| `definition` | Get/Set a body definition by accessing all of its current properties. This is provided as convenience only and should not be used when performance is important as all the properties defined in the definition are accessed sequentially. You should try to only use the specific properties you need rather than using this feature. |
| `enabled` | The enabled state of the body. If false, the body and anything attached to it will not participate in the simulation. |
| `fastCollisionsAllowed` | Treat this body as high speed object that performs continuous collision detection against dynamic and kinematic bodies, but not other high speed bodies. Fast collision bodies should be used sparingly. They are not a solution for general dynamic-versus-dynamic continuous collision. |
| `fastRotationAllowed` | This allows this body to bypass rotational speed limits. This should only be used for circular objects, such as wheels, balls etc. |
| `gravityScale` | Scales the world gravity that is applied to this body. Setting the gravity scale to zero stops any gravity being applied. Likewise, a negative value inverts gravity. See PhysicsWorld._gravity. |
| `isOwned` | Get if the body is owned. See PhysicsBody.SetOwner. |
| `isValid` | Checks if a body is valid. |
| `jointCount` | Get the number of joints attached to this body. Use PhysicsBody.GetJoints to retrieve the joints. |
| `linearDamping` | The linear damping of the body. This will reduce the linear velocity over time. See PhysicsBody._linearVelocity. |
| `linearVelocity` | The linear velocity of the body. |
| `localCenterOfMass` | The center of mass position of the body in local space. This can be accessed as a union of PhysicsBody._mass, PhysicsBody._rotationalInertia and PhysicsBody._localCenterOfMass using PhysicsBody._massConfiguration. |
| `mass` | The calculated mass of the body, usually in kilograms. This can be accessed as a union of PhysicsBody._mass, PhysicsBody._rotationalInertia and PhysicsBody._localCenterOfMass using PhysicsBody._massConfiguration. |
| `massConfiguration` | The body mass configuration comprised of the PhysicsBody._mass, PhysicsBody._rotationalInertia and PhysicsBody._localCenterOfMass. Normally this is computed automatically as each PhysicsShape is added, removed or changed on a body. This will automatically change if the body type changes, for instance, a Static or Kinematic body always have zero mass and rotational inertia. The individual properties of the PhysicsBody._massConfiguration and be accessed using PhysicsBody._mass, PhysicsBody._rotationalInertia and PhysicsBody._localCenterOfMass. The PhysicsBody.MassConfiguration will be overwritten when setting this property or if PhysicsBody.ApplyMassFromShapes is called or when adding, removing or changing PhysicsShape with PhysicsShapeDefinition._startMassUpdate enabled. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `position` | The position of the body in the world. |
| `rotation` | The rotation of the body. |
| `rotationalInertia` | The rotational inertia of the body, usually in kg*m^2. This can be accessed as a union of PhysicsBody._mass, PhysicsBody._rotationalInertia and PhysicsBody._localCenterOfMass using PhysicsBody._massConfiguration. |
| `shapeCount` | Get the number of shapes attached to this body. Use PhysicsBody.GetShapes to retrieve the shapes. |
| `sleepingAllowed` | The sleeping ability of the body. If false, the body will never sleep and will be woken up. See PhysicsBody._awake. |
| `sleepThreshold` | The threshold below which the body will sleep, in meters/sec. |
| `transform` | The full transform of the body composed of position and rotation. |
| `transformObject` | GetSet the transform object associated with the body. This can be used as a write transform andor as a depth-hint for PhysicsWorld drawing. See PhysicsBody._transformWriteMode. |
| `transformWriteMode` | Get/Set how the PhysicsBody._transformObject should be written to after the simulation has completed. Transform write will only occur if it is enabled on the world using PhysicsWorld._transformWriteMode. |
| `type` | A body is one of these three body types, Dynamic, Kinematic or Static, each of which determines how the body behaves in the simulation. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldCenterOfMass` | Get the center of mass position of the body in world space. This changes as the body moves i.e. as the PhysicsBody._transform is changed. |
| `worldDrawing` | Controls whether this body is automatically drawn when the world is drawn. |

### Methods

#### `ApplyAngularImpulse(float, bool)`

Apply an angular impulse. This should be used for one-shot impulses. If you need a steady torque, use a torque instead, which will work better with the sub-stepping solver.

**Params:**
- `impulse` — The angular impulse, usually in units of kg"m"m/s.
- `wake` — Should the body be woken up.

#### `ApplyForce(Vector2, Vector2, bool)`

Apply a force at a world point. If the force is not applied at the center of mass, it will generate a torque and affect the angular velocity.

**Params:**
- `force` — The world force vector, usually in newtons (N)
- `point` — The world position of the point of application.
- `wake` — Should the body be woken up.

#### `ApplyForceToCenter(Vector2, bool)`

Apply a force to the center of mass.

**Params:**
- `force` — The world force vector, usually in newtons (N).
- `wake` — Should the body be woken up.

#### `ApplyLinearImpulse(Vector2, Vector2, bool)`

Apply an impulse at a point. This immediately modifies the velocity and also modifies the angular velocity if the point of application is not at the center of mass. This should be used for one-shot impulses. If you need a steady force, use a force instead, which will work better with the sub-stepping solver.

**Params:**
- `impulse` — The world impulse vector, usually in N"s or kg"m/s.
- `point` — The world position of the point of application.
- `wake` — Should the body be woken up.

#### `ApplyLinearImpulseToCenter(Vector2, bool)`

Apply an impulse to the center of mass. This immediately modifies the velocity. This should be used for one-shot impulses. If you need a steady force, use a force instead, which will work better with the sub-stepping solver.

**Params:**
- `impulse` — The world impulse vector, usually in N"s or kg"m/s.
- `wake` — Should the body be woken up.

#### `ApplyMassFromShapes()`

Typically a body will automatically calculate the PhysicsBody.MassConfiguration using all the attached shapes. The PhysicsBody.MassConfiguration is automatically updated whenever a PhysicsShape is added, removed or modified. When adding many shapes to a body, you can choose to stop this automatic calculation, therefore improving performance, by disabling PhysicsShapeDefinition._startMassUpdate for each shape being added to the body. This call will result in the PhysicsBody.MassConfiguration being calculated using the currently added PhysicsShape so is typically called after many shapes are added if they have PhysicsShapeDefinition._startMassUpdate disabled. Alternately, if you wish to assign your own PhysicsBody.MassConfiguration then disabling the automatic calculation also makes sense. In either case, you must call this method or set PhysicsBody._massConfiguration before any simulation step occurs otherwise the PhysicsBody will exhibit unstable collision behaviour. The PhysicsBody.MassConfiguration will be overwritten when calling PhysicsBody.ApplyMassFromShapes, if PhysicsBody._massConfiguration is set or when adding, removing or changing PhysicsShape with PhysicsShapeDefinition._startMassUpdate enabled.

#### `ApplyTorque(float, bool)`

Apply a torque. This affects the angular velocity without affecting the linear velocity.

**Params:**
- `torque` — Torque, usually in N*m.
- `wake` — Should the body be woken up.

#### `ClearForces()`

Clear any user forces that have been applied to this body. Forces on a body are automatically cleared when a simulation step completes, however under some circumstances it may be desirable to clear the forces explicitly.

#### `Create(PhysicsWorld)`

Create a body using PhysicsBodyDefinition._defaultDefinition in the specified world.

**Params:**
- `world` — The world to create the body in.

**Returns:** The created body.

#### `Create(PhysicsWorld, PhysicsBodyDefinition)`

Create a body in the specified world.

**Params:**
- `world` — The world to create the body in.
- `definition` — The body definition to use.

**Returns:** The created body.

#### `CreateBatch(PhysicsWorld, PhysicsBodyDefinition, int, Unity.Collections.Allocator)`

Create a batch of bodies in the specified world.

**Params:**
- `world` — The world to create the bodies in.
- `definition` — The body definition to use for all bodies.
- `bodyCount` — The number of bodies to create.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created bodies. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateBatch(PhysicsWorld, ReadOnlySpan<PhysicsBodyDefinition>, Unity.Collections.Allocator)`

Create a batch of bodies in the specified world.

**Params:**
- `world` — The world to create the bodies in.
- `definitions` — The definitions used to create the bodies. The number of bodies produced is implicitly controlled by the number of definitions in this span.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created bodies. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateChain(ChainGeometry, PhysicsChainDefinition)`

Create a Chain attached to this body.

**Params:**
- `geometry` — The geometry to use.
- `definition` — The chain definition to use.

**Returns:** The created chain.

#### `CreateShape(CircleGeometry)`

Create a Circle shape, using its default definition, attached to this body.

**Params:**
- `geometry` — The geometry to use.

**Returns:** The created shape.

#### `CreateShape(CircleGeometry, PhysicsShapeDefinition)`

Create a Circle shape attached to this body.

**Params:**
- `geometry` — The geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShape(PolygonGeometry)`

Create a Polygon shape, using its default definition, attached to this body.

**Params:**
- `geometry` — The geometry to use.

**Returns:** The created shape.

#### `CreateShape(PolygonGeometry, PhysicsShapeDefinition)`

Create a Polygon shape attached to this body.

**Params:**
- `geometry` — The geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShape(CapsuleGeometry)`

Create a Capsule shape, using its default definition, attached to this body.

**Params:**
- `geometry` — The geometry to use.

**Returns:** The created shape.

#### `CreateShape(CapsuleGeometry, PhysicsShapeDefinition)`

Create a Capsule shape attached to this body.

**Params:**
- `geometry` — The geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShape(SegmentGeometry)`

Create a Segment shape, using its default definition, attached to this body.

**Params:**
- `geometry` — The geometry to use.

**Returns:** The created shape.

#### `CreateShape(SegmentGeometry, PhysicsShapeDefinition)`

Create a Segment shape attached to this body.

**Params:**
- `geometry` — The geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShape(ChainSegmentGeometry)`

Create a Chain Segment shape, using its default definition, attached to this body.

**Params:**
- `geometry` — The geometry to use.

**Returns:** The created shape.

#### `CreateShape(ChainSegmentGeometry, PhysicsShapeDefinition)`

Create a Chain Segment shape attached to this body.

**Params:**
- `geometry` — The geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShapeBatch(ReadOnlySpan<CircleGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Circle shapes attached to this body.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<PolygonGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Polygon shapes attached to this body.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<CapsuleGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Capsule shapes attached to this body.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<SegmentGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Segment shapes attached to this body.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<ChainSegmentGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Chain Segment shapes attached to this body.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `Destroy(int)`

Destroy a body, destroying all attached PhysicsShape and PhysicsJoint. If the object is owned with PhysicsBody.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the body will not be destroyed.

**Params:**
- `ownerKey` — Optional owner key returned when using PhysicsBody.SetOwner.

**Returns:** If the body was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsBody>)`

Destroy a batch of bodies, destroying all attached PhysicsShape and PhysicsJoint. Any invalid bodies will be ignored. Owned bodies will produce a warning and will not be destroyed (See PhysicsBody.SetOwner).

**Params:**
- `bodies` — The bodies to destroy.

#### `Distance(PhysicsShape, bool)`

Get the minimum distance between all the shapes attached to this body and the specified shape.

**Params:**
- `physicsShape` — The shape to check the distance of.
- `useRadii` — Whether to use the radii of all shapes or not.

**Returns:** The distance result.

#### `Draw()`

Draw a body that visually represents its current state in the world.

#### `GetAABB()`

Get the world AABB that bounds all the shapes attached to this body. If there are no shapes attached to the body then the returned AABB is empty and centered on the body origin.

**Returns:** The world AABB that bounds all the shapes attached to this body.

#### `GetBatchTransform(ReadOnlySpan<PhysicsBody>, Unity.Collections.Allocator)`

Get the transform for a batch of PhysicsBody.

**Params:**
- `bodies` — The bodies to retrieve the batch of transforms for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The batch of transform for the specified bodies.

#### `GetBatchVelocity(ReadOnlySpan<PhysicsBody>, Unity.Collections.Allocator)`

Get the velocity for a batch of PhysicsBody.

**Params:**
- `bodies` — The bodies to retrieve the batch of velocity for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The batch of velocity for the specified bodies.

#### `GetContacts(Unity.Collections.Allocator)`

Get all the touching contacts this body is currently participating in. Speculative collision is used so some contact points may be separated, a property available in the provided contact manifold.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The touching contacts this body is currently participating in. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetJoints(Unity.Collections.Allocator)`

Get the joints attached to this body.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The joints attached to this body. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetLocalPoint(Vector2)`

Gets a local point relative to the body given a world point.

**Params:**
- `worldPoint` — The world point to transform.

**Returns:** The local point relative to the body.

#### `GetLocalPointVelocity(Vector2)`

Get the linear velocity of a local point attached to a body. Usually in meters per second.

**Params:**
- `localPoint` — The local point to transform.

**Returns:** The linear velocity at the specified local point attached to a body.

#### `GetLocalVector(Vector2)`

Gets a local vector on a body given a world vector.

**Params:**
- `worldVector` — The world vector to transform.

**Returns:** The local vector relative to the body.

#### `GetOwner()`

Get the owner object associated with this body as specified using PhysicsBody.SetOwner.

**Returns:** The owner object associated with this body or NULL if no owner has been specified.

#### `GetShapes(Unity.Collections.Allocator)`

Get the shapes attached to this body.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The shapes attached to this body. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetWorldPoint(Vector2)`

Gets a world point transformed from a local point relative to the body.

**Params:**
- `localPoint` — The local point to transform.

**Returns:** The transformed world point.

#### `GetWorldPointVelocity(Vector2)`

Get the linear velocity of a world point attached to a body. Usually in meters per second.

**Params:**
- `worldPoint` — The world point to transform.

**Returns:** The linear velocity at the specified world point attached to a body.

#### `GetWorldVector(Vector2)`

Gets a world vector transformed from a local vector relative to the body.

**Params:**
- `localVector` — The local vector to transform.

**Returns:** The transformed world vector.

#### `ReadPose(Transform, Vector3, Quaternion)`

Read the full 3D position and rotation of the body given the specified Transform.

**Params:**
- `transform` — The Transform object to be used as a reference when converting from 2D positionrotation to 3D positionrotation, usually the same as any TransformObject assigned to the PhysicsBody.
- `position` — The calculated output position.
- `rotation` — The calculated output rotation.

#### `SetBatchForce(ReadOnlySpan<PhysicsBody.BatchForce>)`

Apply a force for a batch of PhysicsBody using a span of PhysicsBody.BatchForce. If invalid values are passed to the batch, they will simply be ignored. For best performance, the bodies contained in the batch should all be part of the same PhysicsWorld. If the bodies in the batch are not contained in the same PhysicsWorld, the batch should be sorted by the PhysicsWorld the bodies are contained within.

**Params:**
- `batch` — The batch of bodies and values to set.

#### `SetBatchImpulse(ReadOnlySpan<PhysicsBody.BatchImpulse>)`

Apply an impulse for a batch of PhysicsBody using a span of PhysicsBody.BatchImpulse. If invalid values are passed to the batch, they will simply be ignored. For best performance, the bodies contained in the batch should all be part of the same PhysicsWorld. If the bodies in the batch are not contained in the same PhysicsWorld, the batch should be sorted by the PhysicsWorld the bodies are contained within.

**Params:**
- `batch` — The batch of bodies and values to set.

#### `SetBatchTransform(ReadOnlySpan<PhysicsBody.BatchTransform>)`

Set the transform for a batch of PhysicsBody using a span of PhysicsBody.BatchTransform. If invalid values are passed to the batch, they will simply be ignored. For best performance, the bodies contained in the batch should all be part of the same PhysicsWorld. If the bodies in the batch are not contained in the same PhysicsWorld, the batch should be sorted by the PhysicsWorld the bodies are contained within.

**Params:**
- `batch` — The batch of bodies and values to set.

#### `SetBatchVelocity(ReadOnlySpan<PhysicsBody.BatchVelocity>)`

Set the velocity for a batch of PhysicsBody using a span of PhysicsBody.BatchVelocity. If invalid values are passed to the batch, they will simply be ignored. For best performance, the bodies contained in the batch should all be part of the same PhysicsWorld. If the bodies in the batch are not contained in the same PhysicsWorld, the batch should be sorted by the PhysicsWorld the bodies are contained within.

**Params:**
- `batch` — The batch of bodies and values to set.

#### `SetContactEvents(bool)`

Enable/disable contact events on all shapes attached to the body. See PhysicsShape._contactEvents.

**Params:**
- `contactEvents` — Whether contact events are allowed on all shapes attached to this body or not.

#### `SetHitEvents(bool)`

Enable/disable hit events on all shapes attached to the body. See PhysicsShape._hitEvents.

**Params:**
- `hitEvents` — Whether hit events are allowed on all shapes attached to this body or not.

#### `SetOwner(ReadOnlySpan<PhysicsBody>, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `bodies` — The bodies to set ownership for.
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.
- `ownerKey` — The owner key to be used. The value must be non-zero. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

**Returns:** The owner key assigned.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

**Returns:** The owner key assigned.

#### `SetOwner(Object)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.

**Returns:** The owner key assigned.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsBody.SetOwner.

#### `SetTransformTarget(PhysicsTransform, float)`

Set the PhysicsBody._linearVelocity and PhysicsBody._angularVelocity to reach the specified transform in the specified time. The resultant transform will be closed by may not be exact. This is designed ideally for Kinematic bodies but will work with Dynamic bodies if nothing changes the assigned velocities. This will be ignored if the calculated PhysicsBody._linearVelocity and PhysicsBody._angularVelocity would be below the PhysicsBody._sleepThreshold. This will automatically wake the body if it is asleep.

**Params:**
- `transform` — The transform target for the body.
- `deltaTime` — The timer over which to calculate the required velocities to move to the transform.

#### `WakeTouching()`

Wake any bodies that are touching this body via their shapes. This also works for Static bodies.

#### `WritePose()`

Write the full 3D position and rotation of the body to the currently set PhysicsBody._transformObject. If no PhysicsBody._transformObject is assigned, this method will do nothing and false will be returned.

**Returns:** Whether the PhysicsBody._transformObject was written to.

### Nested Types

- **BatchForce** — A batch item used to apply a force to a PhysicsBody.
- **BatchImpulse** — A batch item used to apply an impulse to a PhysicsBody.
- **BatchTransform** — A batch item used to get/set the pose of a PhysicsBody.
- **BatchVelocity** — A batch item used to set the velocity of a PhysicsBody.
- **BodyConstraints** — Body constrains constrain the degrees of freedom a body when solving the simulation.
- **BodyType** — A body is one of these three body types, Dynamic, Kinematic or Static, each of which determines how the body behaves in the simulation.
- **MassConfiguration** — This holds the mass configuration computed for a PhysicsBody.
- **TransformWriteMode** — The method used to Write the body pose to the Transform. See PhysicsWorld._transformWriteMode.
- **TransformWriteTween** — Used to define a Transform write "tween" for a body.

### BatchForce

> A batch item used to apply a force to a PhysicsBody.

**Full name:** `Unity.U2D.Physics.PhysicsBody.BatchForce`  

#### Properties

| Name | Summary |
|------|---------|
| `physicsBody` | The PhysicsBody to write to. |

#### Methods

##### `new(PhysicsBody)`

Create a default batch force, assigning the PhysicsBody.

**Params:**
- `physicsBody` — The PhysicsBody to write to.

##### `ApplyForce(Vector2, Vector2, bool)`

Apply a force at a world point. If the force is not applied at the center of mass, it will generate a torque and affect the angular velocity. PhysicsBody.ApplyForce.

**Params:**
- `force` — The world force vector, usually in newtons (N)
- `point` — The world position of the point of application.
- `wake` — Should the body be woken up.

##### `ApplyForceToCenter(Vector2, bool)`

Apply a force to the center of mass. PhysicsBody.ApplyForceToCenter.

**Params:**
- `force` — The world force vector, usually in newtons (N).
- `wake` — Should the body be woken up.

##### `ApplyTorque(float, bool)`

Apply a torque. This affects the angular velocity without affecting the linear velocity. PhysicsBody.ApplyTorque.

**Params:**
- `torque` — Torque, usually in N*m.
- `wake` — Should the body be woken up.

### BatchImpulse

> A batch item used to apply an impulse to a PhysicsBody.

**Full name:** `Unity.U2D.Physics.PhysicsBody.BatchImpulse`  

#### Properties

| Name | Summary |
|------|---------|
| `physicsBody` | The PhysicsBody to write to. |

#### Methods

##### `new(PhysicsBody)`

Create a default batch impulse, assigning the PhysicsBody.

**Params:**
- `physicsBody` — The PhysicsBody to write to.

##### `ApplyAngularImpulse(float, bool)`

Apply an angular impulse. This should be used for one-shot impulses. If you need a steady torque, use a torque instead, which will work better with the sub-stepping solver. PhysicsBody.ApplyAngularImpulse.

**Params:**
- `impulse` — The angular impulse, usually in units of kg"m"m/s.
- `wake` — Should the body be woken up.

##### `ApplyLinearImpulse(Vector2, Vector2, bool)`

Apply an impulse at a point. This immediately modifies the velocity and also modifies the angular velocity if the point of application is not at the center of mass. This should be used for one-shot impulses. If you need a steady force, use a force instead, which will work better with the sub-stepping solver. PhysicsBody.ApplyLinearImpulse.

**Params:**
- `impulse` — The world impulse vector, usually in N"s or kg"m/s.
- `point` — The world position of the point of application.
- `wake` — Should the body be woken up.

##### `ApplyLinearImpulseToCenter(Vector2, bool)`

Apply an impulse to the center of mass. This immediately modifies the velocity. This should be used for one-shot impulses. If you need a steady force, use a force instead, which will work better with the sub-stepping solver. PhysicsBody.ApplyLinearImpulseToCenter.

**Params:**
- `impulse` — The world impulse vector, usually in N"s or kg"m/s.
- `wake` — Should the body be woken up.

### BatchTransform

> A batch item used to get/set the pose of a PhysicsBody.

**Full name:** `Unity.U2D.Physics.PhysicsBody.BatchTransform`  

#### Properties

| Name | Summary |
|------|---------|
| `physicsBody` | The PhysicsBody to write to. |
| `position` | The position of the body in the world. PhysicsBody._position. |
| `rotation` | The rotation of the body. PhysicsBody._rotation. |
| `transform` | The full transform of the body composed of position and rotation. PhysicsBody._transform. |

#### Methods

##### `new(PhysicsBody)`

Create a default batch transform, assigning the PhysicsBody.

**Params:**
- `physicsBody` — The PhysicsBody to write to.

### BatchVelocity

> A batch item used to set the velocity of a PhysicsBody.

**Full name:** `Unity.U2D.Physics.PhysicsBody.BatchVelocity`  

#### Properties

| Name | Summary |
|------|---------|
| `angularVelocity` | The angular velocity of the body, in degrees per second. PhysicsBody._angularVelocity. |
| `linearVelocity` | The linear velocity of the body. PhysicsBody._linearVelocity. |
| `physicsBody` | The PhysicsBody to write to. |

#### Methods

##### `new(PhysicsBody)`

Create a default batch velocity, assigning the PhysicsBody.

**Params:**
- `physicsBody` — The PhysicsBody to write to.

### BodyConstraints

> Body constrains constrain the degrees of freedom a body when solving the simulation.

**Full name:** `Unity.U2D.Physics.PhysicsBody.BodyConstraints`  

#### Fields

| Name | Summary |
|------|---------|
| `All` | Constrain rotation and motion along all axes. |
| `None` | No constraints |
| `Position` | Constrain motion along all axes. |
| `PositionX` | Constrain motion along the X-axis. |
| `PositionY` | Constrain motion along the Y-axis. |
| `Rotation` | FreConstraineze rotation along the Z-axis. |

### BodyType

> A body is one of these three body types, Dynamic, Kinematic or Static, each of which determines how the body behaves in the simulation.

**Full name:** `Unity.U2D.Physics.PhysicsBody.BodyType`  

#### Fields

| Name | Summary |
|------|---------|
| `Dynamic` | A dynamic body has positive mass, velocity determined by forces and is moved by solver. |
| `Kinematic` | A kinematic body has zero mass, velocity set by user and is moved by solver |
| `Static` | A static body has zero mass, zero velocity and may be manually moved. |

### MassConfiguration

> This holds the mass configuration computed for a PhysicsBody.

**Full name:** `Unity.U2D.Physics.PhysicsBody.MassConfiguration`  

#### Properties

| Name | Summary |
|------|---------|
| `center` | The position of the shape's centroid relative to the shape's origin. |
| `mass` | The mass of the shape, usually in kilograms. |
| `rotationalInertia` | The rotational inertia of the shape about the shape center. |

### TransformWriteMode

> The method used to Write the body pose to the Transform. See PhysicsWorld._transformWriteMode.

**Full name:** `Unity.U2D.Physics.PhysicsBody.TransformWriteMode`  

#### Fields

| Name | Summary |
|------|---------|
| `Current` | The current body pose will be written to the Transform. |
| `Extrapolate` | The pose extrapolated from the current body pose to a future pose based upon the current linear/angular velocities will be written to the Transform. The transform pose is essentially predictive. |
| `Interpolate` | The interpolated pose from the previous body pose to the current body pose will be written to the Transform. The transform pose is essentially historic. |
| `Off` | This body pose won't be written to the Transform. |

### TransformWriteTween

> Used to define a Transform write "tween" for a body.

**Full name:** `Unity.U2D.Physics.PhysicsBody.TransformWriteTween`  

#### Properties

| Name | Summary |
|------|---------|
| `angularVelocity` | The angular velocity of the body to be used during the lifetime of the tween, in degrees per second. This is typically used when the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate. |
| `body` | The body to be used during the lifetime of the tween. |
| `linearVelocity` | The linear velocity of the body to be used during the lifetime of the tween. This is typically used when the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate. |
| `physicsTransform` | The physics transform to be used during the lifetime of the tween. When the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Interpolate, this defines the target pose to move to. When the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate, this defines the source pose to move from. |
| `positionFrom` | The start position of the tween. When the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Current, this is set to the last Transform._position. but is not used. When the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Interpolate, this is set to the last Transform._position. When the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate, this will be calculated from PhysicsBody.TransformWriteTween._physicsTransform. See Transform._position. |
| `rotationFrom` | The start rotation of the tween. When the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Current, this is set to the last Transform._rotation but is not used. When the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Interpolate, this is set to the last Transform._rotation. When the PhysicsBody.TransformWriteTween._transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate, this will be calculated from PhysicsBody.TransformWriteTween._physicsTransform. See Transform._rotation. |
| `transform` | The Transform to be used during the lifetime of the tween. |
| `transformDepth` | The depth of the Transform in the hierarchy where zero is the root. When the PhysicsWorld._transformTweenMode is anything other than PhysicsWorld.TransformTweenMode.Parallel, all PhysicsBody.TransformWriteTween are sorted into ascending depth order so that writing the transforms in tween order will result in the deeper children correctly overwriting any parent transform writes. This is NOT set when the PhysicsWorld._transformTweenMode is set to PhysicsWorld.TransformTweenMode.Parallel and will be zero. |
| `transformWriteMode` | The transform write mode to be used during the lifetime of the tween. Anything other than PhysicsBody.TransformWriteMode.Interpolate or PhysicsBody.TransformWriteMode.Extrapolate will be removed. |

#### Methods

##### `GetExtrapolatedPose(PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, float, Vector3, Quaternion)`

Get the extrapolated pose for the current write tween.

**Params:**
- `transformPlane` — The transform plane to use to calculate a non-custom transform plane.
- `transformPlaneCustom` — The custom transform plane to use.
- `extrapolationTime` — The extrapolation time to use in the range [0, 1].
- `position` — The calculated position.
- `rotation` — The calculated rotation.

##### `GetInterpolatedPose(PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, bool, float, Vector3, Quaternion)`

Get the interpolated pose for the current write tween.

**Params:**
- `transformPlane` — The transform plane to use to calculate a non-custom transform plane.
- `transformPlaneCustom` — The custom transform plane to use.
- `fast2D` — Whether to perform fast 2D or slow 3D calculations. See PhysicsWorld.TransformWriteMode.
- `interpolationTime` — The interpolation time to use in the range [0, 1].
- `position` — The calculated position.
- `rotation` — The calculated rotation.

##### `GetPose(PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, bool, Vector3, Quaternion)`

Get the write pose for the current write tween.

**Params:**
- `transformPlane` — The transform plane to use to calculate a non-custom transform plane.
- `transformPlaneCustom` — The custom transform plane to use.
- `fast2D` — Whether to perform fast 2D or slow 3D calculations. See PhysicsWorld.TransformWriteMode.
- `position` — The calculated position.
- `rotation` — The calculated rotation.

## PhysicsBodyDefinition

> A PhysicsBody definition used to specify important initial properties.

**Full name:** `Unity.U2D.Physics.PhysicsBodyDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsBodyDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBodyDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `angularDamping` | Angular damping is used to reduce the angular velocity over time i.e. slow down rotating bodies. The damping parameter can be larger than 1.0f but the damping effect becomes sensitive to the time step when the damping parameter is large. |
| `angularVelocity` | The initial angular velocity of the body, in degrees per second. |
| `awake` | Is this body initially awake or sleeping? |
| `collisionThreshold` | A threshold used to control when continuous collision detection is used when a body moves. The value is used to compare the body linear velocity movement against the extents of all the shapes added to the body scaled by this threshold. If the movement exceeds the extents scaled by the threshold then continuous collision detection is used to stop tunneling. Lower values reduce the distance the body must move before continuous collision detection is used and can have a considerable impact on performance! Higher values increase the distance the body must move before continuous collision detection is used. Too low a threshold will result in continuous collision detection being used more often therefore affecting performance so this should be limited to specific bodies only. The default threshold is 0.5 which equates to half the total shape extents. The threshold is clamped to a range of 0.0 to 1.0 with 0.0 meaning continuous collision detection will always be used. |
| `constraints` | The degrees of freedom constraints (locks) for the body of Linear X, Linear Y and Rotation Z. |
| `defaultDefinition` | Get a default PhysicsBody definition. |
| `enabled` | Used to disable a body. A disabled body does not move or collide. |
| `fastCollisionsAllowed` | Treat this body as high speed object that performs continuous collision detection against dynamic and kinematic bodies, but not other high speed bodies. Fast collision bodies should be used sparingly. They are not a solution for general dynamic-versus-dynamic continuous collision. |
| `fastRotationAllowed` | This allows this body to bypass rotational speed limits. This should only be used for circular objects, such as wheels, balls etc. |
| `gravityScale` | Scale the gravity applied to this body, non-dimensional. |
| `linearDamping` | Linear damping is use to reduce the linear velocity i.e. slow down translating bodies. The damping parameter can be larger than 1 but the damping effect becomes sensitive to the time step when the damping parameter is large. Generally linear damping is undesirable because it makes objects move slowly as if they are floating. |
| `linearVelocity` | The initial linear velocity of the body's origin, in meters/sec. |
| `position` | The initial position of the body, in world-space. Bodies should be created with the desired position as creating bodies at the origin and then moving them nearly doubles the cost of body creation, especially if the body is moved after shapes have been added. |
| `rotation` | The initial rotation of the body, in world-space. Bodies should be created with the desired rotation as creating bodies at the origin and then rotating them nearly doubles the cost of body creation, especially if the body is moved after shapes have been added. |
| `sleepingAllowed` | Set this flag to false if this body should never fall asleep. |
| `sleepThreshold` | A speed threshold below which the body is allowed to sleep, in meters/sec. |
| `transformWriteMode` | The method used to Write the body pose to the Transform. |
| `type` | A body is one of these three body types, Dynamic, Kinematic or Static, each of which determines how the body behaves in the simulation. |
| `worldDrawing` | Controls whether this body is automatically drawn when the world is drawn. See PhysicsBody._worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsBody definition.

#### `new(bool)`

Create a default PhysicsBody definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.
