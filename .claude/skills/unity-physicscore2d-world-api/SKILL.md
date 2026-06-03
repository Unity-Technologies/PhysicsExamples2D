---
name: unity-physicscore2d-world-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for World & Simulation. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsConstants, PhysicsCoreSettings2D, PhysicsWorld, PhysicsWorldDefinition. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — World & Simulation

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsConstants`, `PhysicsCoreSettings2D`, `PhysicsWorld`, `PhysicsWorldDefinition`.

## PhysicsConstants

> Contacts used by physics.

**Full name:** `Unity.U2D.Physics.PhysicsConstants`  
**Docs:** [Unity.U2D.Physics.PhysicsConstants](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsConstants.html)

### Fields

| Name | Summary |
|------|---------|
| `MaxPolygonVertices` | The maximum number of supported vertices in . |
| `MaxWorkers` | A constant defining the maximum number of worker threads supported by physics simulation. The current device may support fewer or more than this. |
| `SolverGraphColorCount` | The number of "colors" used for contact and joint constraints when solving the simulation. |

## PhysicsCoreSettings2D

> PhysicsCore Settings Asset. This contains all the global physics options along with the default values for the following definitions: - - - - - - - - - -

**Full name:** `Unity.U2D.Physics.PhysicsCoreSettings2D`  
**Docs:** [Unity.U2D.Physics.PhysicsCoreSettings2D](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D.html)

### Properties

| Name | Summary |
|------|---------|
| `alwaysDrawWorlds` | Controls if worlds are always drawn independent of whether rendering is currently active or not as specified by . When true, world drawing is always active and a event is produced containing the . When false, world drawing only occurs depending on the setting. CAUTION: Drawing the world has a performance cost associated with it therefore when using this without rendering, that cost can become hidden. |
| `concurrentSimulations` | Controls how many simulations can be started in parallel. Each one is started on its own worker and acts as its own main-thread. Workers should ideally be left free for the solver otherwise it may degrade solving performance. The actual quantity of workers used will always be capped to those available on the current device. If the total number of workers available is below 4 then parallel simulation won't occur however parallel solving using workers will. This should not be confused with the quantity of workers used when solving a simulation. See . |
| `contactFilterGroupMode` | The mode used for the when determining if two can contact. See . |
| `contactFilterMode` | The mode used for the when determining if two can contact. See . |
| `disableSimulation` | Controls the simulation of any temporarily removing simulation overhead. When true, no automatic simulation will occur. When false, normal operation occurs with automatic simulation. |
| `lengthUnitsPerMeter` | The internal length units per meter. The physics system relates all length units on meters but you may need different units for your project. You can set this value to use different units but it should only be modified before any other calls to the physics system occur and only modified once. Changing this value after any physics object has been created can result in severe simulation instabilities. Essentially there are some internal tolerances, such as how close two shapes need to be before they are considered to be touching or when two vertices of a hull are so close that they should be considered the same point. For example, internally a value of 5mm (0.005 meters) is used as a value tuned to work well with most situations with game-sized objects described in meters. If you decide to work in a different unit system (such as pixels) then 0.005 pixels is not a good value for this constant and would be too precise, leading to numerical problems, especially far from the origin. Instead you should determine roughly how many pixels you have per meter. For example, say you want 32 pixels per meter then you should set the `lengthUnitsPerMeter` to be 32.0f. Setting a value of (say) 32.05 would result in the 5mm being scaled up to 0.16 meters, which is a more reasonable value for determining if shapes are touching and hull vertices are too close. A good rule of thumb is to pass the pixel height of your player character to this function, so if your player character is 32 pixels high, then pass 32 to this function. Then you may confidently use pixels for all the length values sent to the physics system. All length values returned from the physics system will also then naturally be in pixels because the physics system does not do any scaling internally, however, you are now responsible for creating appropriate values for gravity, density, and forces. |
| `maximumWorlds` | Get/Set the maximum number of worlds that can be created. The larger the number of worlds, the more memory that is initially allocated so care must be taken. Setting this value to one will reduce start-up memory usage to a minimum but will not allow any additional worlds to be created. The maximum value must be in the range of 1 to 1024. Any change will only be handled by Exiting Play mode in the Editor or restarting the player build. A single is automatically created therefore occupies one of the available worlds. |
| `physicsBodyDefinition` | Get/Set the . |
| `physicsChainDefinition` | Get/Set the . |
| `physicsDistanceJointDefinition` | Get/Set the . |
| `physicsFixedJointDefinition` | Get/Set the . |
| `physicsHingeJointDefinition` | Get/Set the . |
| `physicsLayerNames` | A set of 64 "layer" names associated with each bit in a when used for contacts and queries. |
| `physicsRelativeJointDefinition` | Get/Set the . |
| `physicsShapeDefinition` | Get/Set the . |
| `physicsSliderJointDefinition` | Get/Set the . |
| `physicsWheelJointDefinition` | Get/Set the . |
| `physicsWorldDefinition` | Get/Set the . |
| `renderingMode` | Controls drawing and rendering is allowed. NOTE: Drawing and rendering are always available in the Unity Editor however rendering requires compute buffer support on any device it is used without which no rendering will occur. See . |
| `transformChangeMode` | Defines when changes to that has are registered with are called. NOTE: In the Unity Editor when not in Play Mode, Transform change callbacks are always and only sent at the start of the frame for authoring purposes. See . |
| `usePhysicsLayers` | Controls if the physics 64-bit layers are used based upon or if not, the standard 32-bit layers based upon . If a asset is assigned then the physics layers ( ) will be used if is also active. If no asset is assigned then the global layers (See ) will be used. |

### Methods

#### `new()`

#### `GetAlwaysDrawWorlds()`

#### `GetConcurrentSimulations()`

#### `GetContactFilterGroupMode()`

#### `GetContactFilterMode()`

#### `GetDisableSimulation()`

#### `GetLengthUnitsPerMeter()`

#### `GetMaximumWorlds()`

#### `GetPhysicsBodyDefinition(PhysicsBodyDefinition)`

#### `GetPhysicsChainDefinition(PhysicsChainDefinition)`

#### `GetPhysicsDistanceJointDefinition(PhysicsDistanceJointDefinition)`

#### `GetPhysicsFixedJointDefinition(PhysicsFixedJointDefinition)`

#### `GetPhysicsHingeJointDefinition(PhysicsHingeJointDefinition)`

#### `GetPhysicsLayerNames(PhysicsLayers.LayerNames)`

#### `GetPhysicsRelativeJointDefinition(PhysicsRelativeJointDefinition)`

#### `GetPhysicsShapeDefinition(PhysicsShapeDefinition)`

#### `GetPhysicsSliderJointDefinition(PhysicsSliderJointDefinition)`

#### `GetPhysicsWheelJointDefinition(PhysicsWheelJointDefinition)`

#### `GetPhysicsWorldDefinition(PhysicsWorldDefinition)`

#### `GetRenderingMode()`

#### `GetTransformChangeMode()`

#### `GetUsePhysicsLayers()`

## PhysicsWorld

> A world is a container for all other physics objects such as , , etc. A world can be simulated in isolation from all other worlds. The maximum number of worlds that can be created at one time is defined by . A world is completely isolated from all other worlds.

**Full name:** `Unity.U2D.Physics.PhysicsWorld`  
**Docs:** [Unity.U2D.Physics.PhysicsWorld](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html)

### Properties

| Name | Summary |
|------|---------|
| `aabbMargin` | Get the distance used to expand AABBs in the broadphase dynamic tree, in meters. This allows broadphase proxies to move by a small amount without triggering a tree adjustment. This value is 0.05f * . Normally this is 5cm. |
| `alwaysDrawWorlds` | Get if worlds are always drawn independent of whether rendering is currently active or not as specified by . When true, world drawing is always active and a event is produced containing the . When false, world drawing only occurs depending on the setting. This can be controlled via . |
| `autoBodyUpdateCallbacks` | Controls if body update callback targets are automatically called. See |
| `autoContactCallbacks` | Controls if shape contact callback targets are automatically called. See |
| `autoJointThresholdCallbacks` | Controls if joint threshold callback targets are automatically called. See |
| `autoTriggerCallbacks` | Controls if shape trigger callback targets are automatically called. See |
| `awakeBodyCount` | Get the number of awake bodies in the world. |
| `bodyMaxRotation` | Get the maximum rotation of a body per time step, in degrees. This limit is very large and is used to prevent numerical problems. This value is approximately 45-degrees or 0.25f * radians. |
| `bodyTimeToSleep` | Get the time that a body must be still before it will go to sleep, in seconds. This value is 0.5 seconds. |
| `bodyUpdateEvents` | Get the body events from the last simulation. The objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see ). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See . |
| `bounceThreshold` | Adjust the bounce threshold, usually in meters per second. It is recommended not to make this value very small because it will prevent bodies from sleeping. |
| `concurrentSimulations` | Gets how many simulations can be started in parallel. Whilst running simulations in parallel can improver overall performance, workers should ideally be left free for the simulation solver otherwise it may degrade solving performance. The actual quantity of workers used will always be capped to those available on the current device. If the total number of workers available is below 4 then parallel simulation won't occur as generally this would reduce overall performance, however parallel solving of each simulation using workers will still be used. This should not be confused with the quantity of workers used when solving a simulation. |
| `contactBeginEvents` | Get the contact begin events from the last simulation. The objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see ). The objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced. Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See . |
| `contactDamping` | The contact bounciness with 1 being critical damping (non-dimensional). |
| `contactEndEvents` | Get the contact end events from the last simulation. The objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see ). The objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced. Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See . |
| `contactFilterCallbacks` | Controls if contact filter callbacks will be called. A contact filter callback allows direct control over whether a contact will be created between a pair of shapes. This applies to both triggers and non-triggers but only with Dynamic bodies. These are relatively expensive so disabling them can provide a significant performance benefit. A contact filter callback will call the for both shapes involved if they implement . |
| `contactFilterGroupMode` | Get the current value of . |
| `contactFilterMode` | Get the current value of . |
| `contactFrequency` | The contact stiffness, in cycles per second. |
| `contactHitEvents` | Get the contact hit events from the last simulation. The objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see ). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See . |
| `contactHitEventThreshold` | The contact hit event threshold controls the collision speed needed to generate a contact hit event, usually in meters per second. See . |
| `contactRecycleDistance` | The contact recycle distance, in meters. Setting this to zero disables contact point recycling. Contact recycling reuses contact points across simulation time-steps when the relative movement is small. This feature improves stability and performance by around 25% (approximately). Contact points are not recalculated until shapes move more than 5cm (default) relative to each other. Contact recycling skips some updates such as friction, pre-solve (etc) until the contacts are no longer recycled. |
| `contactSpeed` | The contact speed used to solve overlaps, in meters per second. |
| `continuousAllowed` | Controls if continuous collision detection will be used between Dynamic and Static bodies. Generally you should keep continuous collision enabled to prevent fast moving objects from going through Static objects. The performance gain from disabling continuous collision is minor. |
| `counters` | Get the world counters. |
| `defaultWorld` | Get the default world created at start-up. This world cannot be destroyed as it is permanently owned by Unity itself. See and . |
| `definition` | Get/Set a world definition by accessing all of its current properties. This is provided as convenience only and should not be used when performance is important as all the properties defined in the definition are accessed sequentially. You should try to only use the specific properties you need rather than using this feature. |
| `disableSimulation` | Get if the automatic simulation of any is temporarily disabled. When true, no automatic simulation will occur. When false, normal operation occurs with automatic simulation. This can be controlled via . |
| `drawColors` | Controls what colors are used to draw , , etc. |
| `drawContactType` | Controls the used when drawing contact points. |
| `drawFillAlpha` | Controls the draw fill alpha. This is used to scale the interior fill alpha and is only used when is used so that the interior color can be distinguished from the outline color by transparency. |
| `drawFillOptions` | Controls how shape geometry is filled when drawing. See . |
| `drawFilter` | Limits what gets drawn to a narrow selection. This only affects that are drawing all bodies, shapes etc. It does not affect selected elements or custom drawing. See . |
| `drawForceScale` | Controls the joint contact force scale used when drawing contact forces. |
| `drawNormalScale` | Controls the joint contact normal scale used when drawing contact normals. |
| `drawOptions` | Limits what gets drawn to a broad selection. See . |
| `drawPointScale` | Controls the draw point scale used when drawing points. |
| `drawThickness` | Controls the draw thickness (outline and orientation). |
| `elementDepth` | Controls the element depth. When using custom drawing of geometry or primitive shapes there is no reference to the orthogonal axis used with respect to the current . The element depth is in world-space and for each transform plan is defined as: - Element depth is rendered along the Z axis when using . - Element depth is rendered along the Y axis when using . - Element depth is rendered along the X axis when using . You should set the element depth before performing any custom draw. The element depth will be reset to zero when rendering is complete. |
| `globalCounters` | Get the world counters, summed for all the active worlds. |
| `globalProfile` | Get the world timing profile, summed for all the active worlds. |
| `gravity` | Get/Set the gravity vector applied to all bodies in the world, usually in m/s^2. |
| `hugeWorldExtent` | Gets what physics considers a large extent in the world. Positions greater than approximately 16km will have precision problems, so 100km as a limit should be fine in all cases. This is used to detect bad values. This value is 100000.0f * . |
| `isDefaultWorld` | Check if this is the default . The default world is automatically created at start-up. |
| `isEmpty` | Check if the world is empty as defined by having no bodies, shapes or joints. |
| `isOwned` | Get if the world is owned. See . |
| `isRenderingAllowed` | Get if rendering is currently allowed. Rendering is always allowed in the Editor however it is only allowed elsewhere depending on . |
| `isValid` | Check if the world is valid. |
| `jointThresholdEvents` | Get the joint events from the last simulation. An event is produced by a Joint which exceeds either its or . The objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see ). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See . |
| `lastSimulationDeltaTime` | Get the delta-time used for the last simulation run. |
| `lastSimulationTimestamp` | Get the timestamp when the last simulation was run. |
| `lengthUnitsPerMeter` | Get the internal length units per meter. Changes won't take effect until exiting play mode. The physics system relates all length units on meters but you may need different units for your project. You can set this value to use different units but it should only be modified before any other calls to the physics system occur and only modified once. Changing this value after any physics object has been created can result in severe simulation instabilities. For example, if your game uses pixels for units you can use pixels for all length values sent to the physics system. There should be no extra cost however, the physics system has some internal tolerances and thresholds that have been tuned for meters. By calling this function, the physics system is better able to adjust those tolerances and thresholds to improve accuracy. A good rule of thumb is to pass the height of your player character to this function. So if your player character is 32 pixels high, then pass 32 to this function. Then you may confidently use pixels for all the length values sent to the physics system. All length values returned from the physics system will also then be in pixels because the physics system does not do any scaling internally, however, you are now on the hook for coming up with good values for gravity, density, and forces. The default value is 1. |
| `linearSlop` | Get the small length used as a collision and constraint tolerance, in meters. Usually it is chosen to be numerically significant, but visually insignificant. This value is 0.005f * . Normally this is 0.5cm. |
| `maximumLinearSpeed` | Get/Set the maximum linear speed. |
| `maximumWorldsAllocated` | Get the actual allocated maximum worlds available. This can differ at runtime from if it was changed and the physics system has not been restarted. Another reason this can differ would be if there was not available memory to allocate the requested maximum worlds. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `paused` | Get/Set if the world is paused. When paused, any simulation attempted will be ignored whether it be automatic or manual. |
| `preSolveCallbacks` | Controls if pre-solve callbacks will be called. This only applies to Dynamic bodies and is ignored for triggers. These are relatively expensive so disabling them can provide a significant performance benefit. A pre-solve callback will call the for both shapes involved if they implement . |
| `profile` | Get the world timing profile. |
| `renderingMode` | Get the current value of . NOTE: Drawing and rendering are always available in the Unity Editor however rendering requires compute buffer support on any device it is used without which no rendering will occur. |
| `safetyLocksEnabled` | Get/Set whether safety threading locks are enabled or not. Locks are enabled by default however on platforms that do not support threading, locks are not used. Disabling locks can result in a small performance boost however, please note the following EXTREME CAUTIONS. Typically, per-world, multiple read operations can happen in parallel however only a single write operation can occur concurrently. Read and write operations can never happen at the same time. Locking is a self-balancing reader-preferred system that tries to reduce writers "starving". Once a writer is in a queue, it registers incoming readers as waiting readers and, once active readers are handled, it starts processing a single writer. After that writer has been handled, it flips waiting readers into active readers and processes them. Whilst this system is extremely fast, it does have a very small overhead. Disabling this system can give a small performance boost but is nearly always not worth it therefore this option should be used for testing only. EXTREME CAUTION should be taken if disabling locks on platforms that support threading! A majority of this API is thread-safe and is is due to the safety locks! Locks are used to ensure that read and write operations do not interfere with each other. Locks also ensure that no read or write operations happen during a simulation step. Overlapping read or write operations will almost certainly result in corruptions and a subsequent crash, so unless you are absolutely sure this is not the case, do not disable locks! |
| `simulationSubSteps` | Get/Set the simulation sub-steps to use during simulation. See . |
| `simulationType` | Get/Set the simulation type which controls when or if the simulation will be automatically simulated. See . |
| `simulationWorkers` | Get/Set the simulation worker count for the world. A single simulation worker is always used for simulation therefore a worker count of one means single thread simulation only. The actual quantity of workers used will always be capped to those available on the current device and reading the property will return the number of workers actually being used by the device. Changing the worker count continuously is not recommend and will impact performance as it requires the task queue be recreated. See . |
| `sleepingAllowed` | Controls if bodies go to sleep when not moving and not interacting. Sleeping can provide a significant performance improvement when many Dynamic or Kinematic bodies are in the world. |
| `speculativeContactDistance` | Get the distance at which speculative contacts will be calculated. This reduces jitter. This value is 4.0f * . Normally this is 2cm. |
| `syncInterpolation` | Controls if an extra write pass prior to the script fixed-update callback is made for any interpolation tweens to ensure that transforms are synchronized to the final body pose. Because this is an extra write pass, it has an impact on overall performance so only enable if you require transforms synchronized this way. NOTE: This only affects that have their set to . |
| `transformChangeMode` | Get the current value of . See . |
| `transformPlane` | Controls the transform plane that the world uses when writing transforms. See . |
| `transformPlaneCustom` | Controls the transformation for the to allow transformation writing and reading to/from a custom 2D plane. See . |
| `transformTweenMode` | Controls if and how Transform tweens are calculated and/or written. Transform tweening is where bodies that have their set, write to the each frame depending on the specific body set. Regardless of this setting, Transform tweening is never used if the is or is . |
| `transformWriteCallbackTarget` | Get/Set the custom that implements the to which and will be sent. The callback will only occur if is set to and there are available. The object assigned here will be kept alive, not allowing the GC to dispose of it. To remove the object assigned here, set the callback target to NULL. |
| `transformWriteMode` | Controls how transform writing is handled. Only bodies that have their active and produce a will write to a transform. See . |
| `triggerBeginEvents` | Get the trigger begin events from the last simulation. The objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see ). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See . |
| `triggerEndEvents` | Get the trigger end events from the last simulation. The objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see ). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See . |
| `usePhysicsLayers` | Get if the option of is active or not. If no asset is assigned, this option will return false (inactive). When active, the physics 64-bit layers are used (see ) for property drawers and . When inactive, the 32-bit layers are used (see ) for property drawers and . In all cases, the physics system itself will always use the full 64-bit layers assigned, however when using 32-bit layers, the top 32-bits will be set to zero. |
| `userData` | Get/Set that can be used for any purpose. This cannot be set on the and will always be at the default. The physics system doesn't use this data, it is entirely for custom use. |
| `warmStartingAllowed` | Is warm-starting allowed in the world? Disabling warming-starting will severely impact stability. This is typically used for testing only! |
| `worldCount` | Get the number of created worlds. This will be a value in the range of 1 to . |

### Methods

#### `CastGeometry(CircleGeometry, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Circle geometry as it is cast through the world. See , , and

**Params:**
- `geometry` — The Circle geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastGeometry(CapsuleGeometry, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Capsule geometry as it is cast through the world. See , , and .

**Params:**
- `geometry` — The Capsule geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastGeometry(PolygonGeometry, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Polygon geometry as it is cast through the world. See , , and .

**Params:**
- `geometry` — The Polygon geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastMover(PhysicsQuery.WorldMoverInput)`

Cast a "Mover" which is geometry designed to collide with the world and solve its movement. Everything is specified via the with results returned in .

**Params:**
- `input` — The configuration of the mover to cast.

**Returns:** The solved mover results.

#### `CastRay(PhysicsQuery.CastRayInput, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Ray. Technically this is a line-segment and not an infinite ray. See , , and .

**Params:**
- `input` — The configuration of the ray to cast.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastShape(PhysicsShape, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified shape as it is cast through the world. The selected shape is excluded from any results and must be in this world otherwise a warning will be produced. Neither or shape types are supported. See , , and .

**Params:**
- `shape` — The shape used to cast through the world.
- `translation` — The translation relative to the shape pose defining the direction the shape geometry will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastShapeProxy(PhysicsShape.ShapeProxy, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Circle geometry as it is cast through the world. See , , and .

**Params:**
- `shapeProxy` — The shape proxy to use. This must be in world-space.
- `translation` — The translation relative to the shape proxy defining the direction the shape proxy will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CheckTransformChanges()`

Checks for any transform changes. Anything using will immediately be notified of any changes. This should be used sparingly otherwise it may impact performance. The preference should be not using this but instead control transform changes to be monitored with .

**Returns:** The number of changed transforms that were detected.

#### `ClearDraw()`

Clear all the custom drawn items.

#### `ClearTransformWriteTweens()`

Clear all the existing Transform Write Tweens. See and .

#### `Create()`

Create a PhysicsWorld using the .

**Returns:** The created world.

#### `Create(PhysicsWorldDefinition)`

Create a PhysicsWorld.

**Params:**
- `definition` — The world definition to use.

**Returns:** The created world.

#### `CreateBody()`

Create a body using the in the world. See .

**Returns:** The created body.

#### `CreateBody(PhysicsBodyDefinition)`

Create a body in the world. See .

**Params:**
- `definition` — The body definition to use.

**Returns:** The created body.

#### `CreateBodyBatch(PhysicsBodyDefinition, int, Unity.Collections.Allocator)`

Create a batch of bodies in the world.

**Params:**
- `definition` — The body definition to use.
- `bodyCount` — The number of bodies to create.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The created bodies. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateBodyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsBodyDefinition}, Unity.Collections.Allocator)`

Create a batch of bodies in the world.

**Params:**
- `definitions` — The definitions used to create the bodies. The number of bodies produced is implicitly controlled by the number of definitions in this span.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The created bodies. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateJoint(PhysicsDistanceJointDefinition)`

Create a PhysicsDistanceJoint in the world. See .

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsRelativeJointDefinition)`

Create a PhysicsRelativeJoint in the world. See .

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsIgnoreJointDefinition)`

Create an PhysicsIgnoreJoint in the world. See .

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsSliderJointDefinition)`

Create a PhysicsSliderJoint in the world. See .

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsHingeJointDefinition)`

Create a PhysicsHingeJoint in the world. See .

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsFixedJointDefinition)`

Create a PhysicsFixedJoint in the world. See .

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWheelJointDefinition)`

Create a PhysicsWheelJoint in the world. See .

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateOwnerKey(Object)`

Create an owner key.

**Params:**
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended as the owner key can use the hash-code of the object to generate a more unique key.

**Returns:** The new owner key.

#### `Destroy(int)`

Destroy a world, destroying all objects contained within it such as all and attached and . If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the world will not be destroyed. You cannot destroy the as it is permanently owned by Unity itself.

**Params:**
- `ownerKey` — Optional owner key returned when using .

**Returns:** If the world was destroyed or not.

#### `DestroyBodyBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsBody})`

Destroy a batch of bodies. Any invalid bodies will be ignored. Owned bodies will produce a warning and will not be destroyed (See ).

**Params:**
- `bodies` — The bodies to destroy.

#### `DestroyJointBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsJoint})`

Destroy a batch of joints. Any invalid joints will be ignored. Owned joints will produce a warning and will not be destroyed ( ).

**Params:**
- `joints` — The joints to destroy.

#### `DestroyShapeBatch(ReadOnlySpan{Unity.U2D.Physics.PhysicsShape}, bool)`

Destroy a batch of shapes, destroying all the shapes are involved in. Any invalid shapes will be ignored including chain segment shapes created via a (the chain must be destroyed)." Owned shapes will produce a warning and will not be destroyed ( ). See .

**Params:**
- `shapes` — The shapes to destroy.
- `updateBodyMass` — Whether to update the body mass configuration. Not doing so is faster, especially when destroying multiple shapes.

#### `DrawAABB(PhysicsAABB, Color, float, PhysicsWorld.DrawFillOptions)`

Draw an AABB.

**Params:**
- `aabb` — The AABB to draw.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawAllWorlds(PhysicsAABB)`

#### `DrawBox(PhysicsTransform, Vector2, float, Color, float, PhysicsWorld.DrawFillOptions)`

Draw a Box.

**Params:**
- `transform` — The transform to use on the specified points.
- `size` — The size of the box.
- `radius` — The radius of the box.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawCapsule(PhysicsTransform, Vector2, Vector2, float, Color, float, PhysicsWorld.DrawFillOptions)`

Draw a Capsule outline. For further information on the parameters, see .

**Params:**
- `transform` — The transform to use on the specified centers.
- `center1` — The local center of the first semi-circle.
- `center2` — The local center of the second semi-circle.
- `radius` — The radius of the capsule.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawCircle(Vector2, float, Color, float, PhysicsWorld.DrawFillOptions)`

Draw a Circle outline. For further information on the parameters, see .

**Params:**
- `center` — The center of the circle in PhysicsWorld space.
- `radius` — The radius of the circle.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawGeometry(CircleGeometry, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw the specified Circle Geometry.

**Params:**
- `geometry` — The geometry to draw.
- `transform` — The transform to use on the specified geometry.
- `color` — The color to draw with. Here, the color alpha is used only for the interior fill color but will never be completely opaque.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawGeometry(ReadOnlySpan{Unity.U2D.Physics.CircleGeometry}, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw the specified span of Circle Geometry.

**Params:**
- `geometry` — The geometry to draw.
- `transform` — The transform to use on the specified geometry.
- `color` — The color to draw with. Here, the color alpha is used only for the interior fill color but will never be completely opaque.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawGeometry(CapsuleGeometry, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw the specified Capsule geometry.

**Params:**
- `geometry` — The geometry to draw.
- `transform` — The transform to use on the specified geometry.
- `color` — The color to draw with. Here, the color alpha is used only for the interior fill color but will never be completely opaque.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawGeometry(ReadOnlySpan{Unity.U2D.Physics.CapsuleGeometry}, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw the specified span of Capsule geometry.

**Params:**
- `geometry` — The geometry to draw.
- `transform` — The transform to use on the specified geometry.
- `color` — The color to draw with. Here, the color alpha is used only for the interior fill color but will never be completely opaque.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawGeometry(PolygonGeometry, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw the specified Polygon geometry.

**Params:**
- `geometry` — The geometry to draw.
- `transform` — The transform to use on the specified geometry.
- `color` — The color to draw with. Here, the color alpha is used only for the interior fill color but will never be completely opaque.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawGeometry(ReadOnlySpan{Unity.U2D.Physics.PolygonGeometry}, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw the specified span of Polygon geometry.

**Params:**
- `geometry` — The geometry to draw.
- `transform` — The transform to use on the specified geometry.
- `color` — The color to draw with. Here, the color alpha is used only for the interior fill color but will never be completely opaque.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawGeometry(SegmentGeometry, PhysicsTransform, Color, float)`

Draw the specified Segment geometry.

**Params:**
- `geometry` — The geometry to draw.
- `transform` — The transform to use on the specified geometry.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.

#### `DrawGeometry(ReadOnlySpan{Unity.U2D.Physics.SegmentGeometry}, PhysicsTransform, Color, float)`

Draw the specified span of Segment geometry.

**Params:**
- `geometry` — The geometry to draw.
- `transform` — The transform to use on the specified geometry.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.

#### `DrawLine(Vector2, Vector2, Color, float)`

Draw a Line.

**Params:**
- `point0` — The start of the line.
- `point1` — The end of the line.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.

#### `DrawLineStrip(PhysicsTransform, ReadOnlySpan{UnityEngine.Vector2}, bool, Color, float)`

Draw a set of vertices as lines joined to each other.

**Params:**
- `transform` — The transform to use on the specified vertices.
- `vertices` — The vertices defining the lines. A minimum of two vertices must be present.
- `loop` — Should the first and last vertices be joined by a line?
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.

#### `DrawPoint(Vector2, float, Color, float)`

Draw a Point. A Point is similar to a filled Circle except the radius here is specified in pixels rather than world units.

**Params:**
- `position` — The position of the point in PhysicsWorld space.
- `radius` — The radius of the point, in pixels (approximately).
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.

#### `DrawQueryCastGeometry(CircleGeometry, Vector2, Color, float, PhysicsWorld.DrawFillOptions, bool)`

Draw the query input.

**Params:**
- `geometry` — The Circle geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the geometry at the end of the translation or not.

#### `DrawQueryCastGeometry(CapsuleGeometry, Vector2, Color, float, PhysicsWorld.DrawFillOptions, bool)`

Draw the query input.

**Params:**
- `geometry` — The Circle geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the geometry at the end of the translation or not.

#### `DrawQueryCastGeometry(PolygonGeometry, Vector2, Color, float, PhysicsWorld.DrawFillOptions, bool)`

Draw the query input.

**Params:**
- `geometry` — The Circle geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the geometry at the end of the translation or not.

#### `DrawQueryCastRay(PhysicsQuery.CastRayInput, Color, float, bool)`

Draw the query input.

**Params:**
- `input` — The query input to draw.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawEnd` — Whether to draw the arrow at the end of the translation or not.

#### `DrawQueryCastRay(ReadOnlySpan{Unity.U2D.Physics.PhysicsQuery.CastRayInput}, Color, float, bool)`

Draw the query inputs.

**Params:**
- `inputs` — The query inputs to draw.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawEnd` — Whether to draw the arrow at the end of the translation or not.

#### `DrawQueryCastShape(PhysicsShape, Vector2, Color, float, PhysicsWorld.DrawFillOptions, bool)`

Draw the query input.

**Params:**
- `shape` — The shape used to cast through the world.
- `translation` — The translation relative to the shape pose defining the direction the geometry will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the shape at the end of the translation or not.

#### `DrawQueryCastShapeProxy(PhysicsShape.ShapeProxy, Vector2, Color, float, PhysicsWorld.DrawFillOptions, bool)`

Draw the query input.

**Params:**
- `shapeProxy` — The shape proxy to use. This must be in world-space.
- `translation` — The translation relative to the shape proxy defining the direction the shape proxy will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the shape proxy at the end of the translation or not.

#### `DrawQueryResult(PhysicsQuery.CastResult, Color, float, bool, bool)`

Draw the returned from multiple queries. Only a result where is true is drawn.

**Params:**
- `result` — The result to use.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawPoint` — Whether to draw the point in the result or not.
- `drawNormal` — Whether to draw the normal in the result or not.

#### `DrawQueryResult(ReadOnlySpan{Unity.U2D.Physics.PhysicsQuery.CastResult}, Color, float, bool, bool)`

Draw the returned from multiple queries. Only a result where is true is drawn.

**Params:**
- `results` — The results to use.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawPoint` — Whether to draw the point in the result or not.
- `drawNormal` — Whether to draw the normal in the result or not.

#### `DrawQueryResult(PhysicsQuery.WorldCastResult, Color, float, bool, bool)`

Draw the returned from multiple queries.

**Params:**
- `result` — The result to use.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawPoint` — Whether to draw the point in the result or not.
- `drawNormal` — Whether to draw the normal in the result or not.

#### `DrawQueryResult(ReadOnlySpan{Unity.U2D.Physics.PhysicsQuery.WorldCastResult}, Color, float, bool, bool)`

Draw the returned from multiple queries.

**Params:**
- `results` — The results to use.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawPoint` — Whether to draw the point in the result or not.
- `drawNormal` — Whether to draw the normal in the result or not.

#### `DrawShapeProxy(PhysicsShape.ShapeProxy, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw a .

**Params:**
- `shapeProxy` — The ShapeProxy to draw.
- `transform` — The transform to use on the specified points.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawShapeProxy(ReadOnlySpan{Unity.U2D.Physics.PhysicsShape.ShapeProxy}, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw the specified span of .

**Params:**
- `shapeProxies` — The ShapeProxies to draw.
- `transform` — The transform to use on the specified points.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawTransformAxis(PhysicsTransform, float, float)`

Draw a Transform axis.

**Params:**
- `transform` — The Transform axis to draw.
- `scale` — —
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.

#### `Equals(object)`

#### `Equals(PhysicsWorld)`

#### `Explode(PhysicsWorld.ExplosionDefinition)`

Apply a radial explosion applying impulses away from the position to all bodies found within in the radius.

**Params:**
- `definition` — The explosion definition describing how the explosion should be handled.

#### `GetBodies(Unity.Collections.Allocator)`

Get all the active in the specified world.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The active body results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetBodyUpdateCallbackTargets(Unity.Collections.Allocator)`

Get all current where either of the involved are valid (see ) and have a callback target assigned (see ).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The contact callback target results. This must be disposed of after use otherwise leaks will occur. The exception to this is if there are no targets returned.

#### `GetBodyUpdateOwnerUserData(Unity.Collections.Allocator)`

Get all assigned to each returned with . The Native Array returned will be of the same length and be ordered the same as the returned with . Any that are not valid will return a default .

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A Native Array containing all for each returned with .

#### `GetBodyUpdateUserData(Unity.Collections.Allocator)`

Get all assigned to each returned with . The Native Array returned will be of the same length and be ordered the same as the returned with . Any that are not valid will return a default .

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A Native Array containing all for each returned with .

#### `GetContactCallbackTargets(Unity.Collections.Allocator)`

Get all current and where either of the involved are valid (see ) and have a callback target assigned (see ).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The contact callback target results. This must be disposed of after use otherwise leaks will occur. The exception to this is if there are no targets returned.

#### `GetHashCode()`

#### `GetJoints(Unity.Collections.Allocator)`

Get all the active in the specified world.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The active joints results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetJointThresholdCallbackTargets(Unity.Collections.Allocator)`

Get all current where either of the involved are valid (see ) and have a callback target assigned (see ).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The joint callback target results. This must be disposed of after use otherwise leaks will occur. The exception to this is if there are no targets returned.

#### `GetOwner()`

Get the owner object associated with this world as specified using .

**Returns:** The owner object associated with this world or NULL if no owner has been specified.

#### `GetTransformWriteTweens()`

Gets all the existing Transform Write Tweens that are handled per-frame. If the is then the tweens are sorted into ascending transform depth allowing writing to the Transform hierarchy by simply iterating the tweens . If the is then the tweens are unsorted as a is used to write them. See and .

**Returns:** All the existing Transform Write Tweens that are handled per-frame.

#### `GetTriggerCallbackTargets(Unity.Collections.Allocator)`

Get all current and where either of the involved are valid (see ) and have a callback target assigned (see ).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The trigger callback target results. This must be disposed of after use otherwise leaks will occur. The exception to this is if there are no targets returned.

#### `GetWorlds(Unity.Collections.Allocator)`

Get all the active . This includes the so will always contain at least a single world.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The active world results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `operator ==(PhysicsWorld, PhysicsWorld)`

#### `operator !=(PhysicsWorld, PhysicsWorld)`

#### `OverlapAABB(PhysicsAABB, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that potentially overlap the provided AABB. The overlap is between AABB of shapes in the world therefore it may not result in an exact overlap of the shape itself. See , , and .

**Params:**
- `aabb` — The AABB used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapAABB(ReadOnlySpan{Unity.U2D.Physics.PhysicsAABB}, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that potentially overlap the provided AABBs. The overlap is between AABB of shapes in the world therefore it may not result in an exact overlap of the shape itself. See , , and .

**Params:**
- `aabbs` — The AABBs used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(CircleGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Circle geometry. A circle with a radius of zero is equivalent to . See , , and

**Params:**
- `geometry` — The Circle geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.CircleGeometry}, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Circle geometry. A circle with a radius of zero is equivalent to . See , , and

**Params:**
- `geometry` — The Circle geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(CapsuleGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Capsule geometry. See , , and .

**Params:**
- `geometry` — The Capsule geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.CapsuleGeometry}, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Capsule geometry. See , , and .

**Params:**
- `geometry` — The Capsule geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(PolygonGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Polygon geometry. See , , and .

**Params:**
- `geometry` — The Polygon geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.PolygonGeometry}, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Polygon geometry. See , , and .

**Params:**
- `geometry` — The Polygon geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(SegmentGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Segment geometry. See , , and .

**Params:**
- `geometry` — The Segment geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.SegmentGeometry}, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Segment geometry. See , , and .

**Params:**
- `geometry` — The Segment geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ChainSegmentGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Chain-Segment geometry. See , , and .

**Params:**
- `geometry` — The Chain-Segment geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.ChainSegmentGeometry}, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Chain-Segment geometry. See , , and .

**Params:**
- `geometry` — The Chain-Segment geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapPoint(Vector2, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided point. This first converts the shape to a and uses . See , and .

**Params:**
- `point` — The point used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapPoint(ReadOnlySpan{UnityEngine.Vector2}, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided point(s). This first converts the shape to a and uses . See , and .

**Params:**
- `points` — The points used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapShape(PhysicsShape, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided shape. The selected shape is excluded from any results and must be in this world otherwise a warning will be produced.

**Params:**
- `shape` — The shape used to check overlap.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapShapeProxy(PhysicsShape.ShapeProxy, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the shape proxy. See . and .

**Params:**
- `shapeProxy` — The shape proxy to use. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapShapeProxy(ReadOnlySpan{Unity.U2D.Physics.PhysicsShape.ShapeProxy}, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the shape proxies. See . and .

**Params:**
- `shapeProxies` — The shape proxies to use. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `RegisterTransformChange(Transform, PhysicsCallbacks.ITransformChangedCallback)`

Register a transform watcher to call the specified callback when a transform changes. See for the types of transform changes that are watched for. You MUST unregister this when no longer needed with otherwise you will receive warnings.

**Params:**
- `transform` — The transform to watch for changes.
- `callback` — The callback to perform when a transform change is detected.

#### `Reset()`

Reset the world to a canonical state so that it will reproduce identical results each time. The world must be empty for this to be called otherwise a warning is produced.

#### `SendAllCallbacks()`

Send all callbacks to targets: - - - -

#### `SendBodyUpdateCallbacks()`

Send all current where the involved are valid (see ) and have a callback target assigned (see ). Only callback targets that implement will be called. This will be called automatically if is true. This must be called on the main thread.

#### `SendContactCallbacks()`

Send all current and where either of the involved are valid (see ) and have a callback target assigned (see ). These events will only be created if both of the shape pairs has set to true. Only callback targets that implement will be called. This will be called automatically if is true. This must be called on the main thread.

#### `SendJointThresholdCallbacks()`

Send all current where the involved are valid (see ) and have a callback target assigned (see ). These events will only be created if the joint exceeds its or . Only callback targets that implement will be called. This will be called automatically if is true. This must be called on the main thread.

#### `SendTriggerCallbacks()`

Send all current and where either of the involved are valid (see ) and have a callback target assigned (see ). These events will only be created if one of the shape pairs has set to true. Only callback targets that implement will be called. This will be called automatically if is true. This must be called on the main thread.

#### `SetElementDepth3D(Vector3)`

Set the element depth using the specified 3D position. The relevant axis will be extracted using the current . If is used, the element depth is always set to zero. For more details, see .

**Params:**
- `position` — The 3D position to extract the element depth from.

#### `SetOwner(ReadOnlySpan{Unity.U2D.Physics.PhysicsWorld}, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `worlds` — The worlds to set ownership for.
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

#### `SetTransform(Transform, Vector3, bool)`

Set the position without causing a to be generated by default. See .

**Params:**
- `transform` — The transform to change.
- `position` — The global position to set the transform to.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `SetTransform(Transform, Vector3, Quaternion, bool)`

Set the position and rotation without causing a to be generated. See .

**Params:**
- `transform` — The transform to change.
- `position` — The global position to set the transform to.
- `rotation` — The global rotation to set the transform to.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `SetTransformAccess(Jobs.TransformAccess, Vector3, bool)`

Set the position without causing a to be generated. See .

**Params:**
- `transformAccess` — The used to change the transform.
- `position` — The global position to set the transform to.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `SetTransformAccess(Jobs.TransformAccess, Vector3, Quaternion, bool)`

Set the position and rotation without causing a to be generated. See .

**Params:**
- `transformAccess` — The used to change the transform.
- `position` — The global position to set the transform to.
- `rotation` — The global rotation to set the transform to.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `SetTransformWriteTweens(ReadOnlySpan{Unity.U2D.Physics.PhysicsBody.TransformWriteTween})`

Sets all the Transform Write Tweens to be handled per-frame. See and .

**Params:**
- `transformWriteTweens` — The new transform write tweens to be used.

#### `Simulate(float)`

Simulate the world. If is zero then only contact and trigger events will be updated and no velocity or position integration or constraint updates will occur.

**Params:**
- `deltaTime` — The amount of time to forward simulate the world.

#### `Simulate(ReadOnlySpan{Unity.U2D.Physics.PhysicsWorld}, float)`

Simulate a batch of worlds. If is zero then only contact and trigger events will be updated and no velocity or position integration or constraint updates will occur. The worlds can be simulated concurrently depending on the setting of .

**Params:**
- `worlds` — The worlds to forward simulate.
- `deltaTime` — The amount of time to forward simulate the world.

#### `TestOverlapAABB(PhysicsAABB, PhysicsQuery.QueryFilter)`

Tests if the provided AABB potentially overlaps any shapes. The overlap is between AABB of shapes in the world therefore it may not result in an exact overlap of any shape itself. See and .

**Params:**
- `aabb` — The AABB used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapAABB(ReadOnlySpan{Unity.U2D.Physics.PhysicsAABB}, PhysicsQuery.QueryFilter)`

Tests if the provided AABBs potentially overlap any shapes. The overlap is between AABB of shapes in the world therefore it may not result in an exact overlap of any shape itself. See and .

**Params:**
- `aabbs` — The AABB used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(CircleGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Circle geometry overlaps any shapes. A circle with a radius of zero is equivalent to . See and .

**Params:**
- `geometry` — The Circle geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.CircleGeometry}, PhysicsQuery.QueryFilter)`

Tests if the provided Circle geometry overlaps any shapes. A circle with a radius of zero is equivalent to . See and .

**Params:**
- `geometry` — The Circle geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(CapsuleGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Capsule geometry overlaps any shapes. See and .

**Params:**
- `geometry` — The Capsule geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.CapsuleGeometry}, PhysicsQuery.QueryFilter)`

Tests if the provided Capsule geometry overlaps any shapes. See and .

**Params:**
- `geometry` — The Capsule geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(PolygonGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Polygon geometry overlaps any shapes. See and .

**Params:**
- `geometry` — The Polygon geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.PolygonGeometry}, PhysicsQuery.QueryFilter)`

Tests if the provided Polygon geometry overlaps any shapes. See and .

**Params:**
- `geometry` — The Polygon geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(SegmentGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Segment geometry overlaps any shapes. See and .

**Params:**
- `geometry` — The Segment geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.SegmentGeometry}, PhysicsQuery.QueryFilter)`

Tests if the provided Segment geometry overlaps any shapes. See and .

**Params:**
- `geometry` — The Segment geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ChainSegmentGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Chain-Segment geometry overlaps any shapes. See and .

**Params:**
- `geometry` — The Chain-Segment geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan{Unity.U2D.Physics.ChainSegmentGeometry}, PhysicsQuery.QueryFilter)`

Tests if the provided Chain-Segment geometry overlaps any shapes. See and .

**Params:**
- `geometry` — The Chain-Segment geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapPoint(Vector2, PhysicsQuery.QueryFilter)`

Tests if the provided point overlaps any shapes. See .

**Params:**
- `point` — The point used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapPoint(ReadOnlySpan{UnityEngine.Vector2}, PhysicsQuery.QueryFilter)`

Tests if the provided point(s) overlap any shapes. See .

**Params:**
- `points` — The points used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapShape(PhysicsShape, PhysicsQuery.QueryFilter)`

Tests if the provided shape overlaps any shapes. The selected shape is excluded from any results and must be in this world otherwise a warning will be produced.

**Params:**
- `shape` — The shape used to check overlap.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapShapeProxy(PhysicsShape.ShapeProxy, PhysicsQuery.QueryFilter)`

Test if the provided shape proxy overlaps any shapes.

**Params:**
- `shapeProxy` — The shape proxy to use. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapShapeProxy(ReadOnlySpan{Unity.U2D.Physics.PhysicsShape.ShapeProxy}, PhysicsQuery.QueryFilter)`

Test if the provided shape proxies overlaps any shapes.

**Params:**
- `shapeProxies` — The shape proxy to use. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `ToString()`

#### `UnregisterTransformChange(Transform, PhysicsCallbacks.ITransformChangedCallback)`

Unregister a transform watched to stop calling the specified callback when a transform changes. See for the types of transform changes that are watched for.

**Params:**
- `transform` — The transform to stop watching changes on.
- `callback` — The callback to stop being called when a transform change is detected.

### Nested Types

- **DrawColors** — The colors used to draw , , etc.
- **DrawContactType** — Controls which properties of a are drawn when drawing contact points.
- **DrawFillOptions** — Controls how shape geometry is filled when drawing.
- **DrawOptions** — Draw Options limits what gets drawn to a broad selection.
- **DrawResults** — The draw results retrieved from the world. You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided.
- **ExplosionDefinition** — Used to define the parameters when using .
- **IgnoreFilter** — A ignore flags are a narrow selection of objects/types in the world which needs to be ignored.
- **RenderingMode** — Controls drawing and rendering is allowed. NOTE: Drawing and rendering are always available in the Unity Editor however rendering requires compute buffer support on any device it is used without which no rendering will occur.
- **SimulationType** — Defines when the simulation will run.
- **TransformChangeMode** — Defines when changes to that are registered with are called. NOTE: In the Unity Editor when not in Play Mode, Transform change callbacks are always and only sent at the start of the frame for authoring purposes.
- **TransformChangeReason** — Defines the reason why a changed. Register and unregister for transform changes with and .
- **TransformPlane** — Defines the 2D Transform plane where Transform writes will occur. This also defines the rotation axis which will automatically be perpendicular to the selected plane. See .
- **TransformPlaneCustom** — A transformation applied to the transform write if is set to .
- **TransformTweenMode** — Defines if and how Transform tweens are calculated and/or written.
- **TransformWriteMode** — Defines how the 2D Transforms from each are written to the 3D Transform system.
- **WorldCounters** — PhysicsWorld counters that give details of the world simulation size.
- **WorldProfile** — PhysicsWorld profile that contains the timings of specific world simulation stages. All times are in milliseconds.

### DrawColors

> The colors used to draw , , etc.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawColors`  

#### Fields

| Name | Summary |
|------|---------|
| `bodyAwake` | A shape that is attached to an awake body. |
| `bodyBad` | A shape that is attached to a dynamic body with zero mass. |
| `bodyDisabled` | A shape that is attached to a disabled body. |
| `bodyFastCollisions` | A shape that is attached to a body that is awake and has fast collisions allowed. |
| `bodyKinematic` | A shape that is attached to a body with a Kinematic body type. |
| `bodyMovingFast` | A shape that is attached to a body that is currently moving fast. |
| `bodySpeedCapped` | A shape that is attached to a body that is currently having its speed capped. |
| `bodyStatic` | A shape that is attached to a body with a Static body type. |
| `bodyTimeOfImpactEvent` | A shape that is attached to a body that had a time-of-impact event. |
| `contactAdded` | A contact that was added during the last simulation step. |
| `contactFriction` | The contact friction being applied. |
| `contactImpulse` | The contact impulse being applied. |
| `contactNormal` | A contact normal. |
| `contactPersisted` | A contact that already existed at the start of the last simulation step. |
| `contactSpeculative` | A contact that is speculative. |
| `m_ConstraintGraph` | A collection of constraint graph colors. |
| `shapeBounds` | The shape bounds. |
| `shapeOther` | The default color used when no other shape state is indicated. |
| `shapeTrigger` | A shape that is marked as a trigger. |
| `solverIsland` | A solver island region. |
| `transformAxisX` | The X component of the Transform axis. |
| `transformAxisY` | The Y component of the Transform axis. |

#### Nested Types

- **ConstraintGraphArray** — —

#### ConstraintGraphArray

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawColors.ConstraintGraphArray`  

##### Fields

| Name | Summary |
|------|---------|
| `graphConstraint0` | — |
| `graphConstraint1` | — |
| `graphConstraint10` | — |
| `graphConstraint11` | — |
| `graphConstraint12` | — |
| `graphConstraint13` | — |
| `graphConstraint14` | — |
| `graphConstraint15` | — |
| `graphConstraint16` | — |
| `graphConstraint17` | — |
| `graphConstraint18` | — |
| `graphConstraint19` | — |
| `graphConstraint2` | — |
| `graphConstraint20` | — |
| `graphConstraint21` | — |
| `graphConstraint22` | — |
| `graphConstraint23` | — |
| `graphConstraint3` | — |
| `graphConstraint4` | — |
| `graphConstraint5` | — |
| `graphConstraint6` | — |
| `graphConstraint7` | — |
| `graphConstraint8` | — |
| `graphConstraint9` | — |

##### Properties

| Name | Summary |
|------|---------|
| `Item` | Accessor for the graph array. |

### DrawContactType

> Controls which properties of a are drawn when drawing contact points.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawContactType`  

#### Fields

| Name | Summary |
|------|---------|
| `AnchorA` | This will draw . |
| `AnchorB` | This will draw . |
| `Average` | This will draw the position half-way between and . |
| `Point` | This will draw . |

### DrawFillOptions

> Controls how shape geometry is filled when drawing.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawFillOptions`  

#### Fields

| Name | Summary |
|------|---------|
| `All` | A combination drawn of: - - - |
| `Interior` | The interior of the area is drawn. |
| `Orientation` | The orientation of the area is drawn (if applicable). This is only drawn if the Outline is drawn. |
| `Outline` | The outline of the area is drawn. |

### DrawOptions

> Draw Options limits what gets drawn to a broad selection.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawOptions`  

#### Fields

| Name | Summary |
|------|---------|
| `AllBodies` | Draw all bodies in the world. |
| `AllContactForces` | Draw all the contact forces in the world. |
| `AllContactFriction` | Draw all the contact friction (tangent) in the world. |
| `AllContactImpulse` | Draw all the contact forces in the world. |
| `AllContactNormal` | Draw all the contact normals in the world. |
| `AllContactPoints` | Draw all the contact points in the world. |
| `AllCustom` | Draw all the custom drawing. NOTE: This is only used in a player build as custom drawing is permanently enabled in the Unity Editor. |
| `AllJoints` | Draw all the joints in the world. |
| `AllShapeBounds` | Draw all the shape bounds in the world. |
| `AllShapes` | Draw all the shapes in the world. |
| `AllSolverIslands` | Draw all the solver islands in the world. |
| `DefaultAll` | The default drawing when drawing all. Draw all the shapes, joints and custom drawing in the world. |
| `DefaultSelected` | The default drawing when drawing selections. Draw selected shapes, joints and custom drawing in the world. |
| `Off` | No drawing. |
| `SelectedBodies` | Draw the selected bodies. |
| `SelectedJoints` | Draw the selected joints. |
| `SelectedShapeBounds` | Draw the selected shape bounds. |
| `SelectedShapes` | Draw the selected shapes. |

### DrawResults

> The draw results retrieved from the world. You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawResults`  

#### Properties

| Name | Summary |
|------|---------|
| `capsuleGeometryArray` | Retrieve the Capsule Geometry Element. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `capsuleGeometrySpan` | Retrieve the Capsule Geometry Elements. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `circleGeometryArray` | Retrieve the Circle Geometry Elements. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `circleGeometrySpan` | Retrieve the Circle Geometry Elements. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `isValid` | Get if the draw results are valid i.e. they contain any data at all. |
| `lineArray` | Retrieve the Line Elements. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `lineSpan` | Retrieve the Line Elements. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `pointArray` | Retrieve the Point Elements. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `pointSpan` | Retrieve the Point Elements. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `polygonGeometryArray` | Retrieve the Polygon Geometry Elements. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `polygonGeometrySpan` | Retrieve the Polygon Geometry Elements. Any new drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |

#### Methods

##### `ToString()`

#### Nested Types

- **CapsuleGeometryElement** — A Capsule Geometry Element.
- **CircleGeometryElement** — A Circle Geometry Element.
- **LineElement** — A Line Element.
- **PointElement** — A Point Element.
- **PolygonGeometryElement** — A Polygon Geometry Element.

#### CapsuleGeometryElement

> A Capsule Geometry Element.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawResults.CapsuleGeometryElement`  

##### Fields

| Name | Summary |
|------|---------|
| `color` | The color of the capsule element. |
| `drawFillOptions` | How the geometry element is filled with the color. See . |
| `elementDepth` | The depth of the element. |
| `length` | The length of the capsule element. |
| `radius` | The radius of the capsule element. |
| `transform` | The transform of the capsule element. |

##### Methods

###### `Size()`

The data size of the capsule element. This can be useful in understanding the memory stride in a or other structure.

**Returns:** The size in bytes.

#### CircleGeometryElement

> A Circle Geometry Element.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawResults.CircleGeometryElement`  

##### Fields

| Name | Summary |
|------|---------|
| `color` | The color of the circle element. |
| `drawFillOptions` | How the geometry element is filled with the color. See . |
| `elementDepth` | The depth of the element. |
| `radius` | The radius of the circle element. |
| `transform` | The transform of the circle element. |

##### Methods

###### `Size()`

The data size of the circle element. This can be useful in understanding the memory stride in a or other structure.

**Returns:** The size in bytes.

#### LineElement

> A Line Element.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawResults.LineElement`  

##### Fields

| Name | Summary |
|------|---------|
| `color` | The color of the line element. |
| `elementDepth` | The depth of the element. |
| `length` | The length of the line element. |
| `transform` | The transform of the line element. |

##### Methods

###### `Size()`

The data size of the line element. This can be useful in understanding the memory stride in a or other structure.

**Returns:** The size in bytes.

#### PointElement

> A Point Element.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawResults.PointElement`  

##### Fields

| Name | Summary |
|------|---------|
| `color` | The color of the point element. |
| `elementDepth` | The depth of the element. |
| `position` | The position of the point element. |
| `radius` | The radius of the point element (in pixels). |

##### Methods

###### `Size()`

The data size of the point element. This can be useful in understanding the memory stride in a or other structure.

**Returns:** The size in bytes.

#### PolygonGeometryElement

> A Polygon Geometry Element.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawResults.PolygonGeometryElement`  

##### Fields

| Name | Summary |
|------|---------|
| `color` | The color of the polygon element. |
| `count` | The number of points in the polygon element. |
| `drawFillOptions` | How the geometry element is filled with the color. See . |
| `elementDepth` | The depth of the element. |
| `p0` | The point #0 of the polygon element. |
| `p1` | The point #1 of the polygon element. |
| `p2` | The point #2 of the polygon element. |
| `p3` | The point #3 of the polygon element. |
| `p4` | The point #4 of the polygon element. |
| `p5` | The point #5 of the polygon element. |
| `p6` | The point #6 of the polygon element. |
| `p7` | The point #7 of the polygon element. |
| `radius` | The radius of the polygon element. |
| `transform` | The transform of the polygon element. |

##### Methods

###### `Size()`

The data size of the polygon element. This can be useful in understanding the memory stride in a or other structure.

**Returns:** The size in bytes.

### ExplosionDefinition

> Used to define the parameters when using .

**Full name:** `Unity.U2D.Physics.PhysicsWorld.ExplosionDefinition`  

#### Properties

| Name | Summary |
|------|---------|
| `defaultDefinition` | Create a default explode definition. |
| `falloff` | The falloff distance beyond the radius. Impulse is reduced to zero at this distance. |
| `hitCategories` | The categories that will produce hits. |
| `impulsePerLength` | Impulse per unit length. This applies an impulse according to the shape perimeter that is facing the explosion. Explosions only apply to circles, capsules, and polygons. This may be negative for implosions. |
| `position` | The center of the explosion in world space. |
| `radius` | The radius of the explosion. |

#### Methods

##### `new()`

Create a default explode definition.

### IgnoreFilter

> A ignore flags are a narrow selection of objects/types in the world which needs to be ignored.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.IgnoreFilter`  

#### Fields

| Name | Summary |
|------|---------|
| `IgnoreCapsuleShapes` | Ignore of type . See . |
| `IgnoreChainSegmentShapes` | Ignore of type . See and . |
| `IgnoreCircleShapes` | Ignore of type . See . |
| `IgnoreDynamicBodies` | Ignore of type . |
| `IgnoreKinematicBodies` | Ignore of type . |
| `IgnoreNonTriggerShapes` | Ignore that are not configured as a trigger. See . |
| `IgnorePolygonShapes` | Ignore of type . See . |
| `IgnoreSegmentShapes` | Ignore of type . See . |
| `IgnoreStaticBodies` | Ignore of type . |
| `IgnoreTriggerShapes` | Ignore that are configured as a trigger. See . |
| `None` | No draw filtering occurs. |

### RenderingMode

> Controls drawing and rendering is allowed. NOTE: Drawing and rendering are always available in the Unity Editor however rendering requires compute buffer support on any device it is used without which no rendering will occur.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.RenderingMode`  

#### Fields

| Name | Summary |
|------|---------|
| `AnyPlayer` | Drawing and rendering is available in the Editor and any player build. |
| `DevelopmentPlayer` | Drawing and rendering is available in both the Editor and a Development player build. |
| `EditorOnly` | Drawing and rendering is only available in the Editor and not in a player build. |

### SimulationType

> Defines when the simulation will run.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.SimulationType`  

#### Fields

| Name | Summary |
|------|---------|
| `FixedUpdate` | The simulation will automatically run during the FixedUpdate. |
| `Script` | The simulation will only run when manually called with . |
| `Update` | The simulation will automatically run during the Update. |

### TransformChangeMode

> Defines when changes to that are registered with are called. NOTE: In the Unity Editor when not in Play Mode, Transform change callbacks are always and only sent at the start of the frame for authoring purposes.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformChangeMode`  

#### Fields

| Name | Summary |
|------|---------|
| `FixedUpdate` | Transform Change callbacks are sent after the "FixedUpdate" script callbacks but before any "FixedUpdate" simulation*s). This is typically used when any changes to Transforms occur in the "FixedUpdate" script callbacks need to be handled before any "FixedUpdate" simulation(s). |
| `FrameStart` | Transform Change callbacks are sent at the start of a frame prior to the "FixedUpdate" or "Update" script callbacks. This is typically used when any changes to Transform from the previous frame need to be handled before anything else runs. |
| `Off` | Transform Change callbacks are not sent in play mode. |
| `Update` | Transform Change callbacks are sent after the "Update" script callbacks but before any "Update" simulation(s). This is typically used when any changes to Transforms during the "Update" script callbacks need to be handled before any "Update" simulation(s). |

### TransformChangeReason

> Defines the reason why a changed. Register and unregister for transform changes with and .

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformChangeReason`  

#### Fields

| Name | Summary |
|------|---------|
| `Animation` | The animation system wrote a physics-based world-space TRS change. |
| `Any` | Any transform change. |
| `AnyLocal` | The local position, rotation or scale of the transform changed. |
| `AnyWorld` | The world-space position, rotation or scale of the transform changed. |
| `LocalPosition` | The local position of the transform changed. This does not propagate to children or parent transforms. See . |
| `LocalRotation` | The local rotation of the transform changed. This does not propagate to children or parent transforms. See . |
| `LocalScale` | The local scale of the transform changed. This does not propagate to children or parent transforms. See . |
| `ParentHierarchy` | The parent transform hierarchy changed. Indicates that a direct or indirect parent has been added, removed or re-parented. |
| `WorldPosition` | The World-space position of the transform changed. Changing a parent results in an event in children transform too. See . |
| `WorldRotation` | The World-space rotation of the transform changed. Changing a parent results in an event in children transform too. See . |
| `WorldScale` | The World-space scale of the transform changed. Changing a parent results in an event in children transform too. See . |

### TransformPlane

> Defines the 2D Transform plane where Transform writes will occur. This also defines the rotation axis which will automatically be perpendicular to the selected plane. See .

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformPlane`  

#### Fields

| Name | Summary |
|------|---------|
| `Custom` | Use the assigned to allow transformation writing and reading to/from a custom 2D plane. |
| `XY` | XY plane with anti-clockwise Z rotation. |
| `XZ` | XZ plane with anti-clockwise Y rotation. |
| `ZY` | ZY plane with anti-clockwise X rotation. |

### TransformPlaneCustom

> A transformation applied to the transform write if is set to .

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformPlaneCustom`  

#### Properties

| Name | Summary |
|------|---------|
| `fromCustom` | Get the custom matrix defining how to transform from the custom world-space to the space. NOTE: This is the inverse of the matrix. |
| `rotate` | Get the custom rotation. |
| `scale` | Get the uniform scale. |
| `toCustom` | Get the custom matrix defining how to transform from space to the custom world-space. NOTE: This is the inverse of the matrix. |
| `translate` | Get the custom translation. |

#### Methods

##### `new()`

Create a transform plane custom as identity.

##### `new(Vector3, Vector3, float)`

Create a transform plane custom.

**Params:**
- `translate` — The custom translation.
- `rotate` — The custom EULER rotation.
- `scale` — The custom scale.

##### `FromPosition(Vector3)`

Transform from a 3D custom world-space position back to a 2D position.

**Params:**
- `position` — The 3D position to transform.

**Returns:** The transformed 2D position.

##### `PlaneProjection(PhysicsTransform, Vector3, Quaternion)`

Project the position and rotation to the custom transform plane.

**Params:**
- `physicsTransform` — The physics transform to project.
- `position` — The 3D transformed position.
- `rotation` — The 3D transformed rotation.

##### `ToPosition(Vector2)`

Transform a 2D position to a 3D custom world-space position.

**Params:**
- `position` — The 2D position to transform.

**Returns:** The transformed 3D position.

### TransformTweenMode

> Defines if and how Transform tweens are calculated and/or written.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformTweenMode`  

#### Fields

| Name | Summary |
|------|---------|
| `Custom` | Transform tweens are not calculated or written. Instead, the callback target set with which must implement will be have called allowing custom transform tween writing. |
| `Off` | Transform tweens are not calculated or written. |
| `Parallel` | Transform tweens are calculated and written in parallel using a . |
| `Sequential` | Transform tweens are calculated and written linearly on a single thread, likely the main-thread. This may be faster than using if the majority of the are not split across hierarchies so that they can be written in parallel. To further clarify, if most of the are not interleaved across different hierarchies, this non-parallel (sequential) mode may be faster than , because it avoids the overhead of splitting and synchronizing work across multiple threads when there is not enough independent hierarchy work to parallelize efficiently. |

### TransformWriteMode

> Defines how the 2D Transforms from each are written to the 3D Transform system.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformWriteMode`  

#### Fields

| Name | Summary |
|------|---------|
| `Custom` | Transforms are not written. Instead, the callback target set with which must implement will have called allowing custom transform writing. |
| `Fast2D` | Transforms are written but the rotation is converted to a where only a single axis is written, all others will be set to zero rotation. This is the fastest method of writing transforms however, any 3D rotations or rotations on the unused axis will be reset to zero. The rotational axis written to depends on the current selected with where it will always be perpendicular to the transform plane. |
| `Off` | Transforms are never written. This is the fastest operation. |
| `Slow3D` | Transforms are written but the rotation is converted to a where the rotation of the body transform is merged into the existing 3D rotation. This is the slowest method of writing transforms however, all 3D rotations are preserved. The rotational axis written to depends on the current selected with where it will always be perpendicular to the transform plane. |

### WorldCounters

> PhysicsWorld counters that give details of the world simulation size.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.WorldCounters`  

#### Properties

| Name | Summary |
|------|---------|
| `bodyCount` | The number of all body types. |
| `broadphaseHeight` | The broadphase tree height for both Dynamic and Kinematic bodies. |
| `contactCount` | The number of contacts. |
| `islandCount` | The number of islands. |
| `jointCount` | The number of joints. |
| `memoryUsed` | The total byte allocation used by the physics system. |
| `shapeCount` | The number of shapes. |
| `stackUsed` | The number of bytes assigned to the Stack allocator. |
| `staticBroadphaseHeight` | The broadphase tree height for Static bodies. |
| `taskCount` | The number of multi-threaded tasks requested solving the simulation. |

#### Methods

##### `Add(PhysicsWorld.WorldCounters, PhysicsWorld.WorldCounters)`

Add the specified world counters together.

**Params:**
- `countersA` — The first world counters to add.
- `countersB` — The second world counters to add.

**Returns:** The world counters added together.

##### `Maximum(PhysicsWorld.WorldCounters, PhysicsWorld.WorldCounters)`

Find the maximum values the specified world counters.

**Params:**
- `countersA` — The first world counters to find the maximum of.
- `countersB` — The second world counters to find the maximum of.

**Returns:** The maximum values from both world counters.

### WorldProfile

> PhysicsWorld profile that contains the timings of specific world simulation stages. All times are in milliseconds.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.WorldProfile`  

#### Properties

| Name | Summary |
|------|---------|
| `applyBounciness` | Time spent applying bounciness. |
| `bodyTransforms` | Time spent updating body transforms. |
| `broadphaseUpdates` | Time spent refitting the broadphase. |
| `contactPairs` | Time spent updating collision pairs and creating contacts. |
| `contactUpdates` | Time spent updating contacts. |
| `fastTriggers` | Time spent calculate fast triggers for bodies. |
| `hitEvents` | Time spent generating contact hit events. |
| `integrateTransforms` | Time spent integrating transforms. |
| `integrateVelocities` | Time spent integrating velocities. |
| `jointEvents` | Time spent generating joint threshold events. |
| `prepareConstraints` | Time spent preparing joint and contact constraints. |
| `prepareStages` | Time spent preparing simulation stages. |
| `relaxImpulses` | Time spent relaxing constraint impulses. |
| `simulationStep` | Time spent stepping the simulation forward. |
| `sleepIslands` | Time spent updating islands that need to sleep. |
| `solveConstraints` | Time spent solving constraints. |
| `solveContinuous` | Time spent solving continuous collision detection. |
| `solveImpulses` | Time spent solving impulses. |
| `solving` | Time spent integrating velocities, solving velocity constraints, and integrating positions. |
| `splitIslands` | Time spent splitting islands because some contacts and/or joints have been removed. |
| `storeImpulses` | Time spent storing impulses. |
| `updateTriggers` | Time spent updating triggers. |
| `warmStarting` | Time spent warm-starting. |
| `writeTransforms` | Time spent writing the body poses to the transform system. |

#### Methods

##### `Add(PhysicsWorld.WorldProfile, PhysicsWorld.WorldProfile)`

Add the specified world profiles together.

**Params:**
- `profileA` — The first world profiles to add.
- `profileB` — The second world profiles to add.

**Returns:** The world profiles added together.

##### `Maximum(PhysicsWorld.WorldProfile, PhysicsWorld.WorldProfile)`

Find the maximum values the specified world profiles.

**Params:**
- `profileA` — The first world profile to find the maximum of.
- `profileB` — The second world profile to find the maximum of.

**Returns:** The maximum values from both world profile.

## PhysicsWorldDefinition

> A definition used to specify important initial properties.

**Full name:** `Unity.U2D.Physics.PhysicsWorldDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsWorldDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `autoBodyUpdateCallbacks` | Controls if body update callback targets are automatically called. See . |
| `autoContactCallbacks` | Controls if shape contact callback targets are automatically called. See . |
| `autoJointThresholdCallbacks` | Controls if joint threshold callback targets are automatically called. See . |
| `autoTriggerCallbacks` | Controls if shape trigger callback targets are automatically called. See . |
| `bounceThreshold` | Adjust the bounce threshold, usually in meters per second. It is recommended not to make this value very small because it will prevent bodies from sleeping. See . |
| `contactDamping` | The contact bounciness with 1 being critical damping (non-dimensional). See . |
| `contactFilterCallbacks` | Controls if contact filter callbacks will be called. A contact filter callback allows direct control over whether a contact will be created between a pair of shapes. This applies to both triggers and non-triggers but only with Dynamic bodies. These are relatively expensive so disabling them can provide a significant performance benefit. A contact filter callback will call the for both shapes involved if they implement . |
| `contactFrequency` | The contact stiffness, in cycles per second. See . |
| `contactHitEventThreshold` | The contact hit event threshold controls the collision speed needed to generate a contact hit event, usually in meters per second. See . See . |
| `contactRecycleDistance` | The contact recycle distance, in meters. Setting this to zero disables contact point recycling. Contact recycling reuses contact points across simulation time-steps when the relative movement is small. This feature improves stability and performance by around 25% (approximately). Contact points are not recalculated until shapes move more than 5cm (default) relative to each other. Contact recycling skips some updates such as friction, pre-solve (etc) until the contacts are no longer recycled. See . |
| `contactSpeed` | The contact speed used to solve overlaps, in meters per second. See . |
| `continuousAllowed` | Controls if continuous collision detection will be used between Dynamic and Static bodies. Generally you should keep continuous collision enabled to prevent fast moving objects from going through Static objects. The performance gain from disabling continuous collision is minor. See |
| `defaultDefinition` | Get a default definition. |
| `drawColors` | Controls what colors are used to draw , , etc. See . |
| `drawContactType` | Controls how contact points are drawn. See . |
| `drawFillAlpha` | Controls the draw fill alpha. This is used to scale the interior fill alpha and is only used when is used so that the interior color can be distinguished from the outline color by transparency. See . |
| `drawFillOptions` | Controls how shape geometry is filled when drawing. See . |
| `drawFilter` | Limits what gets drawn to a narrow selection. This only affects that are drawing all bodies, shapes etc. It does not affect selected elements or custom drawing. See . |
| `drawForceScale` | Controls the joint contact force scale used when drawing contact forces. See . |
| `drawNormalScale` | Controls the joint contact normal scale used when drawing contact normals. See . |
| `drawOptions` | Limits what gets drawn to a broad selection. See . |
| `drawPointScale` | Controls the draw point scale used when drawing points. See . |
| `drawThickness` | Controls the draw thickness (outline and orientation). See . |
| `gravity` | Get/Set the gravity vector applied to all bodies in the world, usually in m/s^2. See . |
| `maximumLinearSpeed` | Get/Set the maximum linear speed. See . |
| `preSolveCallbacks` | Controls if pre-solve callbacks will be called. This only applies to Dynamic bodies and is ignored for triggers. These are relatively expensive so disabling them can provide a significant performance benefit. A pre-solve callback will call the for both shapes involved if they implement . |
| `simulateType` | Get/Set the simulation mode which controls when or if the simulation will be automatically simulated. See and . |
| `simulationSubSteps` | Get/Set the simulation sub-steps to use during simulation. See . See . |
| `simulationWorkers` | Get/Set the simulation worker count for the world. A single simulation worker is always used for simulation therefore a worker count of one means single thread simulation only. The actual quantity of workers used will always be capped to those available on the current device and reading the property will return the number of workers actually being used by the device. Changing the worker count continuously is not recommend and will impact performance as it requires the task queue be recreated. See . |
| `sleepingAllowed` | Controls if bodies go to sleep when not moving and not interacting. Sleeping can provide a significant performance improvement when many Dynamic or Kinematic bodies are in the world. See |
| `syncInterpolation` | Controls if an extra write pass prior to the script fixed-update callback is made for any interpolation tweens to ensure that transforms are synchronized to the final body pose. Because this is an extra write pass, it has an impact on overall performance so only enable if you require transforms synchronized this way. NOTE: This only affects that have their set to . |
| `transformPlane` | Controls the transform plane that the world uses when writing transforms. See . See . |
| `transformPlaneCustom` | Controls the transformation for the to allow transformation writing and reading to/from a custom space. See . |
| `transformTweenMode` | Controls if and how Transform tweens are calculated and/or written. Transform tweening is where bodies that have their set, write to the each frame depending on the specific body set. Regardless of this setting, Transform tweening is never used if the is or is . |
| `transformWriteMode` | Controls how transform writing is handled. Only bodies that have their active and produce a will write to a transform. See . |

### Methods

#### `new()`

Create a default definition.

#### `new(bool)`

Create a default definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
