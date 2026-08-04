---
name: unity-physicscore2d-bodies-api
description: Authoritative Unity 6000.7 PhysicsCore2D API reference for Bodies. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsBody, PhysicsBodyDefinition. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Bodies

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.7, so it should be treated as the source of truth for member names, signatures, and documentation strings.

Top-level types in this file: `PhysicsBody`, `PhysicsBodyDefinition`.

## PhysicsBody

> A body is contained within a world and has 3 degrees-of-freedom, two for position and one for rotation. A body can have forces, torques and impulses applied to it. A body has three distinct types: - Static: This type of body does not move under simulation and behaves as if it has infinite mass, essentially an immovable object. Static bodies never interact with other Static or Kinematic bodies. - Dynamic: This type of body is fully simulated and moves according to forces and torques applied to its linear/angular velocities. It can interact with all other body types. It always has finite, non-zero mass. - Kinematic: This type of body moves under simulation and moves according to its linear/angular velocities and never uses forces or torques. It only interacts with Dynamic body types. It behaves as if it has infinite mass. A body is automatically destroyed when the world it is in is destroyed. A body cannot exist outside a world.

**Full name:** `Unity.U2D.Physics.PhysicsBody`

### Properties

| Name | Summary |
|------|---------|
| `angularDamping` | The angular damping of the body. This will reduce the angular velocity over time. See PhysicsBody.angularVelocity. |
| `angularVelocity` | The angular velocity of the body, in degrees per second. |
| `awake` | The awake state of the body. |
| `callbackTarget` | Get/Set the Object that event callbacks for this body will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.BodyUpdateEvent with call PhysicsCallbacks.IBodyUpdateCallback. |
| `collisionThreshold` | A threshold used to control when continuous collision detection is used when a body moves. The value is used to compare the body linear velocity movement against the extents of all the shapes added to the body scaled by this threshold. If the movement exceeds the extents scaled by the threshold then continuous collision detection is used to stop tunneling. Lower values reduce the distance the body must move before continuous collision detection is used and can have a considerable impact on performance! Higher values increase the distance the body must move before continuous collision detection is used. Too low a threshold will result in continuous collision detection being used more often therefore affecting performance so this should be limited to specific bodies only. The default threshold is 0.5 which equates to half the total shape extents. The threshold is clamped to a range of 0.0 to 1.0 with 0.0 meaning continuous collision detection will always be used. |
| `constraints` | Get/Set the degrees of freedom constraints (locks) for the body of Linear X, Linear Y and Rotation Z. |
| `contactRecyclingAllowed` | Controls contact recycling for this body. Enabled by default. Contact recycling reuses contact manifolds when bodies move only slightly, improving performance. Disabling it can avoid ghost collisions, at the cost of higher simulation time. Both bodies in a contact must have recycling enabled for that contact to be recycled. Existing contacts retain their prior setting; only contacts created after a change will be recycled. See PhysicsBodyDefinition.contactRecyclingAllowed. |
| `definition` | Get/Set a body definition by accessing all of its current properties. This is provided as convenience only and should not be used when performance is important as all the properties defined in the definition are accessed sequentially. You should try to only use the specific properties you need rather than using this feature. |
| `enabled` | The enabled state of the body. If false, the body and anything attached to it will not participate in the simulation. |
| `fastCollisionsAllowed` | Treat this body as high speed object that performs continuous collision detection against dynamic and kinematic bodies, but not other high speed bodies. Fast collision bodies should be used sparingly, not because they are slow but because everything using fast collisions does not work well. They are not a solution for general dynamic-versus-dynamic continuous collision. They also may interfere with joint constraints. |
| `fastRotationAllowed` | This allows this body to bypass rotational speed limits. This should only be used for circular objects, such as wheels, balls etc. |
| `gravityScale` | Scales the world gravity that is applied to this body. Setting the gravity scale to zero stops any gravity being applied. Likewise, a negative value inverts gravity. See PhysicsWorld.gravity. |
| `isOwned` | Get if the body is owned. See PhysicsBody.SetOwner. |
| `isValid` | Checks if a body is valid. |
| `jointCount` | Get the number of joints attached to this body. Use PhysicsBody.GetJoints to retrieve the joints. |
| `linearDamping` | The linear damping of the body. This will reduce the linear velocity over time. See PhysicsBody.linearVelocity. |
| `linearVelocity` | The linear velocity of the body. |
| `localCenterOfMass` | The center of mass position of the body in local space. This can be accessed as a union of PhysicsBody.mass, PhysicsBody.rotationalInertia and PhysicsBody.localCenterOfMass using PhysicsBody.massConfiguration. When this value is overridden with PhysicsBody.massOverride, setting it authors the override value, which persists across mass recalculations. |
| `mass` | The calculated mass of the body, usually in kilograms. This can be accessed as a union of PhysicsBody.mass, PhysicsBody.rotationalInertia and PhysicsBody.localCenterOfMass using PhysicsBody.massConfiguration. When this value is overridden with PhysicsBody.massOverride, setting it authors the override value, which persists across mass recalculations. |
| `massConfiguration` | The body mass configuration comprised of the PhysicsBody.mass, PhysicsBody.rotationalInertia and PhysicsBody.localCenterOfMass. Normally this is computed automatically as each PhysicsShape is added, removed or changed on a body. This will automatically change if the body type changes, for instance, a Static or Kinematic body always have zero mass and rotational inertia. The individual properties of the PhysicsBody.massConfiguration and be accessed using PhysicsBody.mass, PhysicsBody.rotationalInertia and PhysicsBody.localCenterOfMass. The PhysicsBody.MassConfiguration will be overwritten when setting this property or if PhysicsBody.ApplyMassFromShapes is called or when adding, removing or changing PhysicsShape with PhysicsShapeDefinition.startMassUpdate enabled. To make a value persist across those recalculations, override it with PhysicsBody.massOverride; with anything overridden, setting this property authors the override values instead of writing the body directly, so overridden values persist and the rest are rederived from the attached shapes. |
| `massOverride` | Selects which of the mass, rotational inertia and center of mass are overridden with authored values instead of being computed from the attached shapes. |
| `owner` | The owner object associated with this body, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsBody.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `position` | The position of the body in the world. |
| `rotation` | The rotation of the body. |
| `rotationalInertia` | The rotational inertia of the body, usually in kg*m^2. This can be accessed as a union of PhysicsBody.mass, PhysicsBody.rotationalInertia and PhysicsBody.localCenterOfMass using PhysicsBody.massConfiguration. When this value is overridden with PhysicsBody.massOverride, setting it authors the override value, which persists across mass recalculations. |
| `shapeCount` | Get the number of shapes attached to this body. Use PhysicsBody.GetShapes to retrieve the shapes. |
| `sleepingAllowed` | The sleeping ability of the body. If false, the body will never sleep and will be woken up. See PhysicsBody.awake. |
| `sleepThreshold` | The threshold below which the body will sleep, in meters/sec. |
| `totalForce` | The total user force that has been applied to this body since the last simulation step. Setting this value overrides any force that was previously requested. |
| `totalTorque` | The total users torque that has been applied to this body since the last simulation step. Setting this value overrides any torque that was previously requested. |
| `transform` | The full transform of the body composed of position and rotation. |
| `transformObject` | Get/Set the transform object associated with the body. This can be used as a write transform and/or as a depth-hint for PhysicsWorld drawing. See PhysicsBody.transformWriteMode. |
| `transformWriteMode` | Get/Set how the PhysicsBody.transformObject should be written to after the simulation has completed. Transform write will only occur if it is enabled on the world using PhysicsWorld.transformWriteMode. |
| `type` | A body is one of these three body types, Dynamic, Kinematic or Static, each of which determines how the body behaves in the simulation. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the body is attached to. |
| `worldCenterOfMass` | Get the center of mass position of the body in world space. This changes as the body moves i.e. as the PhysicsBody.transform is changed. |
| `worldDrawing` | Controls whether this body is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a body from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `ApplyAngularImpulse(float, bool)`

Apply an angular impulse. This should be used for one-shot impulses. If you need a steady torque, use a torque instead, which will work better with the sub-stepping solver.

**Params:**
- `impulse` — The angular impulse, usually in units of kg*m*m/s.
- `wake` — Should the body be woken up.

#### `ApplyBuoyancy(PhysicsBody.BuoyancyInput, float)`

Apply buoyancy, flow and damping forces to the body based on how its attached shapes are submerged in a fluid plane. Forces and torques are continuous (not impulses), so this is expected to be called every simulation step. The body must be PhysicsBody.BodyType.Dynamic; otherwise a warning is logged and the call is a no-op.

**Params:**
- `input` — The fluid and force configuration. See PhysicsBody.BuoyancyInput.
- `deltaTime` — The simulation step duration in seconds. Used to clamp damping so it cannot overshoot in a single step.

#### `ApplyBuoyancy(PhysicsBody.BuoyancyInput, ReadOnlySpan<PhysicsBody>, float)`

Apply buoyancy, flow and damping forces to every body in bodies based on how their attached shapes are submerged in a fluid plane. The same PhysicsBody.BuoyancyInput is applied to all bodies. Each body must be PhysicsBody.BodyType.Dynamic; non-dynamic or invalid bodies log a warning and are skipped. Forces and torques are continuous (not impulses), so this is expected to be called every simulation step.

**Params:**
- `input` — The fluid and force configuration. See PhysicsBody.BuoyancyInput.
- `bodies` — The bodies that buoyancy should be applied to.
- `deltaTime` — The simulation step duration in seconds. Used to clamp damping so it cannot overshoot in a single step.

#### `ApplyBuoyancy(PhysicsWorld, PhysicsAABB, PhysicsBody.BuoyancyInput, float)`

Apply buoyancy, flow and damping forces to every dynamic body shape that overlaps aabb in world. The same PhysicsBody.BuoyancyInput is applied to all overlapping shapes. Shapes whose body is not PhysicsBody.BodyType.Dynamic are silently skipped. Forces and torques are continuous (not impulses), so this is expected to be called every simulation step.

**Params:**
- `world` — The world to query for overlapping shapes.
- `aabb` — The world-space axis-aligned box describing the fluid volume. Only shapes whose broadphase AABB overlaps this box are processed.
- `input` — The fluid and force configuration. See PhysicsBody.BuoyancyInput.
- `deltaTime` — The simulation step duration in seconds. Used to clamp damping so it cannot overshoot in a single step.

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
- `impulse` — The world impulse vector, usually in N*s or kg*m/s.
- `point` — The world position of the point of application.
- `wake` — Should the body be woken up.

#### `ApplyLinearImpulseToCenter(Vector2, bool)`

Apply an impulse to the center of mass. This immediately modifies the velocity. This should be used for one-shot impulses. If you need a steady force, use a force instead, which will work better with the sub-stepping solver.

**Params:**
- `impulse` — The world impulse vector, usually in N*s or kg*m/s.
- `wake` — Should the body be woken up.

#### `ApplyMassFromShapes()`

Typically a body will automatically calculate the PhysicsBody.MassConfiguration using all the attached shapes. The PhysicsBody.MassConfiguration is automatically updated whenever a PhysicsShape is added, removed or modified. When adding many shapes to a body, you can choose to stop this automatic calculation, therefore improving performance, by disabling PhysicsShapeDefinition.startMassUpdate for each shape being added to the body. This call will result in the PhysicsBody.MassConfiguration being calculated using the currently added PhysicsShape so is typically called after many shapes are added if they have PhysicsShapeDefinition.startMassUpdate disabled. Alternately, if you wish to assign your own PhysicsBody.MassConfiguration then disabling the automatic calculation also makes sense. In either case, you must call this method or set PhysicsBody.massConfiguration before any simulation step occurs otherwise the PhysicsBody will exhibit unstable collision behaviour. The PhysicsBody.MassConfiguration will be overwritten when calling PhysicsBody.ApplyMassFromShapes, if PhysicsBody.massConfiguration is set or when adding, removing or changing PhysicsShape with PhysicsShapeDefinition.startMassUpdate enabled. Values overridden with PhysicsBody.massOverride keep their authored values through this call; it refreshes the values that are computed from the attached shapes.

#### `ApplyTorque(float, bool)`

Apply a torque. This affects the angular velocity without affecting the linear velocity.

**Params:**
- `torque` — Torque, usually in N*m.
- `wake` — Should the body be woken up.

#### `ApplyWind(PhysicsBody.WindInput)`

Apply wind forces to this body's attached shapes. Forces are continuous (not impulses) and are computed per shape by Box2D using the drag/lift coefficients in input; this method is expected to be called every simulation step while the body is exposed to the wind. The body must be PhysicsBody.BodyType.Dynamic; otherwise a warning is logged and the call is a no-op. Sleeping bodies are woken automatically by Box2D when the per-shape force is non-trivial.

**Params:**
- `input` — The wind configuration. See PhysicsBody.WindInput.

#### `ApplyWind(PhysicsBody.WindInput, ReadOnlySpan<PhysicsBody>)`

Apply wind forces to every body in bodies by iterating each body's attached shapes. The same PhysicsBody.WindInput is applied to all bodies. Each body must be PhysicsBody.BodyType.Dynamic; non-dynamic or invalid bodies log a warning and are skipped. Forces are continuous (not impulses), so this is expected to be called every simulation step.

**Params:**
- `input` — The wind configuration. See PhysicsBody.WindInput.
- `bodies` — The bodies that wind should be applied to.

#### `ApplyWind(PhysicsWorld, PhysicsAABB, PhysicsBody.WindInput)`

Apply wind forces to every dynamic body shape that overlaps aabb in world. The same PhysicsBody.WindInput is applied to all overlapping shapes. Shapes whose body is not PhysicsBody.BodyType.Dynamic are silently skipped. Forces are continuous (not impulses), so this is expected to be called every simulation step.

**Params:**
- `world` — The world to query for overlapping shapes.
- `aabb` — The world-space axis-aligned box describing the wind volume. Only shapes whose broadphase AABB overlaps this box are processed.
- `input` — The wind configuration. See PhysicsBody.WindInput.

#### `ClearForces()`

Clear any user forces that have been applied to this body. Forces on a body are automatically cleared when a simulation step completes, however under some circumstances it may be desirable to clear the forces explicitly.

#### `Create(PhysicsWorld)`

Create a body using PhysicsBodyDefinition.defaultDefinition in the specified world.

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

#### `CreateBatch(PhysicsWorld, PhysicsBodyDefinition, int, Object, int, Unity.Collections.Allocator)`

Create a batch of bodies in the specified world, assigning the given owner to every created body.

**Params:**
- `world` — The world to create the bodies in.
- `definition` — The body definition to use for all bodies.
- `bodyCount` — The number of bodies to create.
- `owner` — The owner object to assign to every created body. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created bodies. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateBatch(PhysicsWorld, ReadOnlySpan<PhysicsBodyDefinition>, Unity.Collections.Allocator)`

Create a batch of bodies in the specified world.

**Params:**
- `world` — The world to create the bodies in.
- `definitions` — The definitions used to create the bodies. The number of bodies produced is implicitly controlled by the number of definitions in this span.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created bodies. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateBatch(PhysicsWorld, ReadOnlySpan<PhysicsBodyDefinition>, Object, int, Unity.Collections.Allocator)`

Create a batch of bodies in the specified world, assigning the given owner to every created body.

**Params:**
- `world` — The world to create the bodies in.
- `definitions` — The definitions used to create the bodies. The number of bodies produced is implicitly controlled by the number of definitions in this span.
- `owner` — The owner object to assign to every created body. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created bodies. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateChain(ChainGeometry, PhysicsChainDefinition)`

Create a Chain attached to this body.

**Params:**
- `geometry` — The geometry to use.
- `definition` — The chain definition to use.

**Returns:** The created chain.

#### `CreateChain(ReadOnlySpan<Vector2>, PhysicsChainDefinition)`

Create a Chain of multiple shapes attached to this body.

**Params:**
- `vertices` — The vertices that will create the ChainSegment shapes.
- `definition` — The shape definition to use.

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

#### `CreateShapeBatch(ReadOnlySpan<CircleGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Circle shapes attached to this body, assigning the given owner to every created shape.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<PolygonGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Polygon shapes attached to this body.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<PolygonGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Polygon shapes attached to this body, assigning the given owner to every created shape.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<CapsuleGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Capsule shapes attached to this body.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<CapsuleGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Capsule shapes attached to this body, assigning the given owner to every created shape.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<SegmentGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Segment shapes attached to this body.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<SegmentGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Segment shapes attached to this body, assigning the given owner to every created shape.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<ChainSegmentGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Chain Segment shapes attached to this body.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(ReadOnlySpan<ChainSegmentGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Chain Segment shapes attached to this body, assigning the given owner to every created shape.

**Params:**
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
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

#### `DestroyBatch(ReadOnlySpan<PhysicsBody>, int)`

Destroy a batch of bodies, destroying all attached shapes and joints. Any invalid bodies are ignored. A body owned by a different owner key is skipped and left valid; a body with no owner, or one matching the given owner key, is destroyed. One summary warning reports how many bodies were skipped this way, rather than one warning per body.

**Params:**
- `bodies` — The bodies to destroy.
- `ownerKey` — Optional owner key returned when using PhysicsBody.SetOwner.

#### `Distance(PhysicsShape, bool)`

Get the minimum distance between all the shapes attached to this body and the specified shape.

**Params:**
- `physicsShape` — The shape to check the distance of.
- `useRadii` — Whether to use the radii of all shapes or not.

**Returns:** The distance result.

#### `Draw()`

Draw a body that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsBody)`

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

#### `GetHashCode()`

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
- `transform` — The Transform object to be used as a reference when converting from 2D position/rotation to 3D position/rotation, usually the same as any TransformObject assigned to the PhysicsBody.
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

#### `SetBatchTransform(ReadOnlySpan<PhysicsBody.BatchTransform>, bool)`

Set the transform for a batch of PhysicsBody using a span of PhysicsBody.BatchTransform, optionally writing each pose to the PhysicsBody.transformObject of its body. If invalid values are passed to the batch, they will simply be ignored. When writePoses is true, each pose set is also written to the PhysicsBody.transformObject of its body, without producing a PhysicsEvents.TransformChangeEvent. Writing poses requires all the valid bodies in the batch to be in the same PhysicsWorld and each to have its own PhysicsBody.transformObject assigned, not shared with another body in the batch. Invalid bodies in the batch are still simply ignored. The PhysicsWorld.transformWriteMode and PhysicsBody.transformWriteMode do not control this explicit write, except that PhysicsWorld.TransformWriteMode.Fast2D selects the faster rotation write. Writing poses must be done on the main thread.

**Params:**
- `batch` — The batch of bodies and values to set.
- `writePoses` — Whether to also write the poses to the PhysicsBody.transformObject of each body in the batch.

#### `SetBatchVelocity(ReadOnlySpan<PhysicsBody.BatchVelocity>)`

Set the velocity for a batch of PhysicsBody using a span of PhysicsBody.BatchVelocity. If invalid values are passed to the batch, they will simply be ignored. For best performance, the bodies contained in the batch should all be part of the same PhysicsWorld. If the bodies in the batch are not contained in the same PhysicsWorld, the batch should be sorted by the PhysicsWorld the bodies are contained within.

**Params:**
- `batch` — The batch of bodies and values to set.

#### `SetContactEvents(bool)`

Enable/disable contact events on all shapes attached to the body. See PhysicsShape.contactEvents.

**Params:**
- `contactEvents` — Whether contact events are allowed on all shapes attached to this body or not.

#### `SetHitEvents(bool)`

Enable/disable hit events on all shapes attached to the body. See PhysicsShape.hitEvents.

**Params:**
- `hitEvents` — Whether hit events are allowed on all shapes attached to this body or not.

#### `SetOwner(ReadOnlySpan<PhysicsBody>, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `bodies` — The bodies to set ownership for.
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
- `ownerKey` — Optional owner key returned when using PhysicsBody.SetOwner.

#### `SetOwnerUserData(ReadOnlySpan<PhysicsBody>, ReadOnlySpan<PhysicsUserData>, int)`

Set PhysicsUserData on a batch of bodies that can be used for any purpose, typically by the owner only. The bodies and userDatas spans must be the same length; bodies[n] receives userDatas[n].

**Params:**
- `bodies` — The bodies to set the owner user data on.
- `userDatas` — The user data to set, one entry per body.
- `ownerKey` — Optional owner key returned when using PhysicsBody.SetOwner.

#### `SetTransformTarget(PhysicsTransform, float)`

Set the PhysicsBody.linearVelocity and PhysicsBody.angularVelocity to reach the specified transform in the specified time. The resultant transform will be closed by may not be exact. This is designed ideally for Kinematic bodies but will work with Dynamic bodies if nothing changes the assigned velocities. This will be ignored if the calculated PhysicsBody.linearVelocity and PhysicsBody.angularVelocity would be below the PhysicsBody.sleepThreshold. This will automatically wake the body if it is asleep.

**Params:**
- `transform` — The transform target for the body.
- `deltaTime` — The timer over which to calculate the required velocities to move to the transform.

#### `ToString()`

#### `WakeTouching()`

Wake any bodies that are touching this body via their shapes. This also works for Static bodies.

#### `WritePose()`

Write the full 3D position and rotation of the body to the currently set PhysicsBody.transformObject. If no PhysicsBody.transformObject is assigned, this method will do nothing and false will be returned.

**Returns:** Whether the PhysicsBody.transformObject was written to.

### Nested Types

- **BatchForce** — A batch item used to apply a force to a PhysicsBody.
- **BatchImpulse** — A batch item used to apply an impulse to a PhysicsBody.
- **BatchTransform** — A batch item used to get/set the pose of a PhysicsBody.
- **BatchVelocity** — A batch item used to set the velocity of a PhysicsBody.
- **BodyConstraints** — Body constrains constrain the degrees of freedom a body when solving the simulation.
- **BodyType** — A body is one of these three body types, Dynamic, Kinematic or Static, each of which determines how the body behaves in the simulation.
- **BuoyancyInput** — Input to PhysicsBody.ApplyBuoyancy describing the fluid surface, density, flow and damping used to compute buoyancy, flow and damping forces for the body.
- **MassConfiguration** — This holds the mass configuration computed for a PhysicsBody.
- **MassOverride** — Selects which of the mass, rotational inertia and center of mass are overridden with authored values instead of being computed from the attached shapes.
- **TransformWriteMode** — The method used to Write the body pose to the Transform. See PhysicsWorld.transformWriteMode.
- **TransformWriteTween** — Used to define a Transform write "tween" for a body.
- **WindInput** — Input to PhysicsBody.ApplyWind describing the wind velocity, drag and lift coefficients and the shape filter used to compute aerodynamic forces per attached shape.

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
- `impulse` — The angular impulse, usually in units of kg*m*m/s.
- `wake` — Should the body be woken up.

##### `ApplyLinearImpulse(Vector2, Vector2, bool)`

Apply an impulse at a point. This immediately modifies the velocity and also modifies the angular velocity if the point of application is not at the center of mass. This should be used for one-shot impulses. If you need a steady force, use a force instead, which will work better with the sub-stepping solver. PhysicsBody.ApplyLinearImpulse.

**Params:**
- `impulse` — The world impulse vector, usually in N*s or kg*m/s.
- `point` — The world position of the point of application.
- `wake` — Should the body be woken up.

##### `ApplyLinearImpulseToCenter(Vector2, bool)`

Apply an impulse to the center of mass. This immediately modifies the velocity. This should be used for one-shot impulses. If you need a steady force, use a force instead, which will work better with the sub-stepping solver. PhysicsBody.ApplyLinearImpulseToCenter.

**Params:**
- `impulse` — The world impulse vector, usually in N*s or kg*m/s.
- `wake` — Should the body be woken up.

### BatchTransform

> A batch item used to get/set the pose of a PhysicsBody.

**Full name:** `Unity.U2D.Physics.PhysicsBody.BatchTransform`

#### Properties

| Name | Summary |
|------|---------|
| `physicsBody` | The PhysicsBody to write to. |
| `position` | The position of the body in the world. PhysicsBody.position. |
| `rotation` | The rotation of the body. PhysicsBody.rotation. |
| `transform` | The full transform of the body composed of position and rotation. PhysicsBody.transform. |

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
| `angularVelocity` | The angular velocity of the body, in degrees per second. PhysicsBody.angularVelocity. |
| `linearVelocity` | The linear velocity of the body. PhysicsBody.linearVelocity. |
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
| `Rotation` | Constrain rotation along the Z-axis. |

### BodyType

> A body is one of these three body types, Dynamic, Kinematic or Static, each of which determines how the body behaves in the simulation.

**Full name:** `Unity.U2D.Physics.PhysicsBody.BodyType`

#### Fields

| Name | Summary |
|------|---------|
| `Dynamic` | A dynamic body has positive mass, velocity determined by forces and is moved by solver. |
| `Kinematic` | A kinematic body has zero mass, velocity set by user and is moved by solver |
| `Static` | A static body has zero mass, zero velocity and may be manually moved. |

### BuoyancyInput

> Input to PhysicsBody.ApplyBuoyancy describing the fluid surface, density, flow and damping used to compute buoyancy, flow and damping forces for the body.

**Full name:** `Unity.U2D.Physics.PhysicsBody.BuoyancyInput`

#### Properties

| Name | Summary |
|------|---------|
| `angularDamping` | Angular damping coefficient. Slows the body's angular velocity while submerged. |
| `density` | The fluid density, used to compute the Archimedean buoyancy force per submerged unit area. Clamped to a lower bound of Mathf.Epsilon. |
| `flowDirection` | The direction of the fluid flow as a 2D rotation. Combined with PhysicsBody.BuoyancyInput.flowSpeed to produce the flow velocity vector applied as force per unit submerged area at the submerged centroid. |
| `flowSpeed` | The magnitude of the fluid flow along PhysicsBody.BuoyancyInput.flowDirection. The per-shape force contribution is flowDirection * flowSpeed * submergedArea. |
| `linearDamping` | Linear damping coefficient. Slows the body's linear velocity at the submerged centroid relative to the fluid. |
| `mask` | Category mask used to filter which attached shapes contribute. A shape participates iff (shape.contactFilter.categories.bitMask & mask.bitMask) != 0. Defaults to PhysicsMask.All when the input is created via the parameterless constructor. |
| `surfaceNormal` | The outward-pointing surface normal of the fluid, in world space. Points away from the submerged side; shape points with a negative separation from the plane are submerged. Defaults to Vector2.up (a flat horizontal water surface). Must be non-zero; the engine normalises it internally so it does not need to be unit length. |
| `surfacePosition` | A point in world space lying on the fluid surface. Together with PhysicsBody.BuoyancyInput.surfaceNormal this defines the infinite plane of the fluid. |
| `useTriggers` | When true, trigger shapes contribute to buoyancy alongside solid shapes. When false, trigger shapes are skipped. |

#### Methods

##### `new()`

Create a default PhysicsBody.BuoyancyInput. The PhysicsBody.BuoyancyInput.mask defaults to PhysicsMask.All so every attached shape contributes unless explicitly filtered out. The surface defaults to a flat horizontal water surface at the world origin (PhysicsBody.BuoyancyInput.surfacePosition = Vector2.zero, PhysicsBody.BuoyancyInput.surfaceNormal = Vector2.up).

### MassConfiguration

> This holds the mass configuration computed for a PhysicsBody.

**Full name:** `Unity.U2D.Physics.PhysicsBody.MassConfiguration`

#### Properties

| Name | Summary |
|------|---------|
| `center` | The position of the shape's centroid relative to the shape's origin. |
| `defaultConfiguration` | Get a default mass configuration with a mass of one, a rotational inertia of one and a centroid at the origin. |
| `mass` | The mass of the shape, usually in kilograms. |
| `rotationalInertia` | The rotational inertia of the shape about the shape center. |

### MassOverride

> Selects which of the mass, rotational inertia and center of mass are overridden with authored values instead of being computed from the attached shapes.

**Full name:** `Unity.U2D.Physics.PhysicsBody.MassOverride`

#### Fields

| Name | Summary |
|------|---------|
| `All` | Override the mass, rotational inertia and center of mass. |
| `CenterOfMass` | Override the center of mass. A rotational inertia that is not itself overridden is shifted to the overridden center. |
| `Mass` | Override the mass. A rotational inertia that is not itself overridden is scaled to the overridden mass, as if the same shapes had a different uniform density. |
| `None` | Nothing is overridden; the whole mass configuration is computed from the attached shapes. |
| `RotationalInertia` | Override the rotational inertia. |

### TransformWriteMode

> The method used to Write the body pose to the Transform. See PhysicsWorld.transformWriteMode.

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
| `angularVelocity` | The angular velocity of the body to be used during the lifetime of the tween, in degrees per second. This is typically used when the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate. |
| `body` | The body to be used during the lifetime of the tween. |
| `linearVelocity` | The linear velocity of the body to be used during the lifetime of the tween. This is typically used when the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate. |
| `physicsTransform` | The physics transform to be used during the lifetime of the tween. When the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Interpolate, this defines the target pose to move to. When the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate, this defines the source pose to move from. |
| `positionFrom` | The start position of the tween. When the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Current, this is set to the last Transform.position. but is not used. When the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Interpolate, this is set to the last Transform.position. When the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate, this will be calculated from PhysicsBody.TransformWriteTween.physicsTransform. See Transform.position. |
| `rotationFrom` | The start rotation of the tween. When the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Current, this is set to the last Transform.rotation but is not used. When the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Interpolate, this is set to the last Transform.rotation. When the PhysicsBody.TransformWriteTween.transformWriteMode is PhysicsBody.TransformWriteMode.Extrapolate, this will be calculated from PhysicsBody.TransformWriteTween.physicsTransform. See Transform.rotation. |
| `transform` | The Transform to be used during the lifetime of the tween. |
| `transformDepth` | The depth of the Transform in the hierarchy where zero is the root. When the PhysicsWorld.transformTweenMode is anything other than PhysicsWorld.TransformTweenMode.Parallel, all PhysicsBody.TransformWriteTween are sorted into ascending depth order so that writing the transforms in tween order will result in the deeper children correctly overwriting any parent transform writes. This is NOT set when the PhysicsWorld.transformTweenMode is set to PhysicsWorld.TransformTweenMode.Parallel and will be zero. |
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

### WindInput

> Input to PhysicsBody.ApplyWind describing the wind velocity, drag and lift coefficients and the shape filter used to compute aerodynamic forces per attached shape.

**Full name:** `Unity.U2D.Physics.PhysicsBody.WindInput`

#### Properties

| Name | Summary |
|------|---------|
| `drag` | Drag coefficient. Scales the wind contribution in the relative-velocity term that drives the per-shape aerodynamic force. |
| `force` | The wind velocity vector. Scaled by PhysicsBody.WindInput.drag when computing the per-shape aerodynamic relative velocity. |
| `lift` | Lift coefficient. Scales the perpendicular component of the per-edge aerodynamic force (capsules and polygons only; circles ignore lift). |
| `mask` | Category mask used to filter which attached shapes contribute. A shape participates iff (shape.contactFilter.categories.bitMask & mask.bitMask) != 0. Defaults to PhysicsMask.All when the input is created via the parameterless constructor. |
| `useTriggers` | When true, trigger shapes contribute to wind alongside solid shapes. When false, trigger shapes are skipped. |

#### Methods

##### `new()`

Create a default PhysicsBody.WindInput. The PhysicsBody.WindInput.mask defaults to PhysicsMask.All so every attached shape contributes unless explicitly filtered out.

## PhysicsBodyDefinition

> A PhysicsBody definition used to specify important initial properties.

**Full name:** `Unity.U2D.Physics.PhysicsBodyDefinition`

### Properties

| Name | Summary |
|------|---------|
| `angularDamping` | Angular damping is used to reduce the angular velocity over time i.e. slow down rotating bodies. The damping parameter can be larger than 1.0f but the damping effect becomes sensitive to the time step when the damping parameter is large. |
| `angularVelocity` | The initial angular velocity of the body, in degrees per second. |
| `awake` | Is this body initially awake or sleeping? |
| `collisionThreshold` | A threshold used to control when continuous collision detection is used when a body moves. The value is used to compare the body linear velocity movement against the extents of all the shapes added to the body scaled by this threshold. If the movement exceeds the extents scaled by the threshold then continuous collision detection is used to stop tunneling. Lower values reduce the distance the body must move before continuous collision detection is used and can have a considerable impact on performance! Higher values increase the distance the body must move before continuous collision detection is used. Too low a threshold will result in continuous collision detection being used more often therefore affecting performance so this should be limited to specific bodies only. The default threshold is 0.5 which equates to half the total shape extents. The threshold is clamped to a range of 0.0 to 1.0 with 0.0 meaning continuous collision detection will always be used. |
| `constraints` | The degrees of freedom constraints (locks) for the body of Linear X, Linear Y and Rotation Z. |
| `contactRecyclingAllowed` | Controls contact recycling for this body. Enabled by default. Contact recycling reuses contact manifolds when bodies move only slightly, improving performance. Disabling it can avoid ghost collisions, at the cost of higher simulation time. Both bodies in a contact must have recycling enabled for that contact to be recycled. See PhysicsBody.contactRecyclingAllowed. |
| `defaultDefinition` | Get a default PhysicsBody definition. |
| `enabled` | Used to disable a body. A disabled body does not move or collide. |
| `fastCollisionsAllowed` | Treat this body as high speed object that performs continuous collision detection against dynamic and kinematic bodies, but not other high speed bodies. Fast collision bodies should be used sparingly, not because they are slow but because everything using fast collisions does not work well. They are not a solution for general dynamic-versus-dynamic continuous collision. They also may interfere with joint constraints. |
| `fastRotationAllowed` | This allows this body to bypass rotational speed limits. This should only be used for circular objects, such as wheels, balls etc. |
| `gravityScale` | Scale the gravity applied to this body, non-dimensional. |
| `linearDamping` | Linear damping is use to reduce the linear velocity i.e. slow down translating bodies. The damping parameter can be larger than 1 but the damping effect becomes sensitive to the time step when the damping parameter is large. Generally linear damping is undesirable because it makes objects move slowly as if they are floating. |
| `linearVelocity` | The initial linear velocity of the body's origin, in meters/sec. |
| `massConfiguration` | The authored mass configuration applied for the overridden values. Anything without a matching override bit is ignored. See PhysicsBodyDefinition.massOverride and PhysicsBody.massConfiguration. |
| `massOverride` | Selects which of the mass, rotational inertia and center of mass are overridden with the authored values instead of being computed from the attached shapes. Anything without a matching bit is computed from the attached shapes as normal. Only applies to a Dynamic body. See PhysicsBody.massOverride. |
| `position` | The initial position of the body, in world-space. Bodies should be created with the desired position as creating bodies at the origin and then moving them nearly doubles the cost of body creation, especially if the body is moved after shapes have been added. |
| `rotation` | The initial rotation of the body, in world-space. Bodies should be created with the desired rotation as creating bodies at the origin and then rotating them nearly doubles the cost of body creation, especially if the body is moved after shapes have been added. |
| `sleepingAllowed` | Set this flag to false if this body should never fall asleep. |
| `sleepThreshold` | A speed threshold below which the body is allowed to sleep, in meters/sec. |
| `transformWriteMode` | The method used to Write the body pose to the Transform. |
| `type` | A body is one of these three body types, Dynamic, Kinematic or Static, each of which determines how the body behaves in the simulation. |
| `worldDrawing` | Controls whether this body is automatically drawn when the world is drawn. See PhysicsBody.worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsBody definition.

#### `new(bool)`

Create a default PhysicsBody definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
