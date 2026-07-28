---
name: unity-physicscore2d-world-api
description: Authoritative Unity 6000.7 PhysicsCore2D API reference for World & Simulation. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsConstants, PhysicsCoreSettings2D, PhysicsWorld, PhysicsWorldDefinition. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — World & Simulation

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.7, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.7.0a3 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsConstants`, `PhysicsCoreSettings2D`, `PhysicsWorld`, `PhysicsWorldDefinition`.

## PhysicsConstants

> Constants used throughout the 2D physics system.

**Full name:** `Unity.U2D.Physics.PhysicsConstants`  
**Docs:** [Unity.U2D.Physics.PhysicsConstants](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsConstants.html)

### Fields

| Name | Summary |
|------|---------|
| `MaxPolygonVertices` | The maximum number of supported vertices in [PolygonGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html). |
| `MaxWorkers` | A constant defining the maximum number of worker threads supported by physics simulation. The current device may support fewer or more than this. |
| `MaxWorldCapacity` | The maximum number of [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) that can exist at one time. The world array grows on demand up to this ceiling, which is the limit of the 16-bit world index carried in physics handles. |

## PhysicsCoreSettings2D

> PhysicsCore Settings Asset. This contains all the global physics options along with the default values for the following definitions: - [PhysicsWorldDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition.html) - [PhysicsBodyDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBodyDefinition.html) - [PhysicsShapeDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShapeDefinition.html) - [PhysicsChainDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChainDefinition.html) - [PhysicsDistanceJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJointDefinition.html) - [PhysicsFixedJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsFixedJointDefinition.html) - [PhysicsHingeJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJointDefinition.html) - [PhysicsRelativeJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJointDefinition.html) - [PhysicsSliderJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJointDefinition.html) - [PhysicsWheelJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJointDefinition.html)

**Full name:** `Unity.U2D.Physics.PhysicsCoreSettings2D`  
**Docs:** [Unity.U2D.Physics.PhysicsCoreSettings2D](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D.html)

### Properties

| Name | Summary |
|------|---------|
| `alwaysDrawWorlds` | Controls if worlds are always drawn independent of whether rendering is currently active or not as specified by [PhysicsWorld.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-renderingMode.html). When true, world drawing is always active and a [PhysicsEvents.WorldDrawResults](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents-WorldDrawResults.html) event is produced containing the [PhysicsWorld.DrawResults](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawResults.html). When false, world drawing only occurs depending on the [PhysicsWorld.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-renderingMode.html) setting. CAUTION: Drawing the world has a performance cost associated with it therefore when using this without rendering, that cost can become hidden. |
| `concurrentSimulations` | Controls how many simulations can be started in parallel. Each one is started on its own worker and acts as its own main-thread. Workers should ideally be left free for the solver otherwise it may degrade solving performance. The actual quantity of workers used will always be capped to those available on the current device. If the total number of workers available is below 4 then parallel simulation won't occur however parallel solving using workers will. This should not be confused with the quantity of workers used when solving a simulation. See [PhysicsWorldDefinition.simulationWorkers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition-simulationWorkers.html). |
| `contactFilterGroupMode` | The mode used for the [PhysicsShape.ContactFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactFilter.html) when determining if two [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) can contact. See [PhysicsShape.ContactFilterGroupMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactFilterGroupMode.html). |
| `contactFilterMode` | The mode used for the [PhysicsShape.ContactFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactFilter.html) when determining if two [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) can contact. See [PhysicsShape.ContactFilterMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactFilterMode.html). |
| `disableSimulation` | Controls the simulation of any [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) temporarily removing simulation overhead. When true, no automatic simulation will occur. When false, normal operation occurs with automatic simulation. |
| `initialWorldCapacity` | Get/Set the number of worlds allocated up-front when the physics system starts. Worlds are allocated as a single contiguous block, so a larger capacity uses more memory immediately and care must be taken. When all worlds are in use, more are allocated on demand up to the supported maximum, so this is an initial capacity and not a hard limit. Growing the array reallocates it, so set this to the number of worlds you expect in order to avoid reallocations during gameplay. Setting this value to one reduces start-up memory usage to a minimum while still allowing more worlds to be created later. The value must be in the range of 1 to 1024. Any change will only be handled by Exiting Play mode in the Editor or restarting the player build. A single [PhysicsWorld.defaultWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-defaultWorld.html) is automatically created therefore occupies one of the allocated worlds. See [PhysicsWorld.allocatedWorldCapacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-allocatedWorldCapacity.html) for the number currently allocated, which can grow at runtime. |
| `lengthUnitsPerMeter` | The internal length units per meter. The physics system relates all length units on meters but you may need different units for your project. You can set this value to use different units but it should only be modified before any other calls to the physics system occur and only modified once. Changing this value after any physics object has been created can result in severe simulation instabilities. Essentially there are some internal tolerances, such as how close two shapes need to be before they are considered to be touching or when two vertices of a hull are so close that they should be considered the same point. For example, internally a value of 5mm (0.005 meters) is used as a value tuned to work well with most situations with game-sized objects described in meters. If you decide to work in a different unit system (such as pixels) then 0.005 pixels is not a good value for this constant and would be too precise, leading to numerical problems, especially far from the origin. Instead you should determine roughly how many pixels you have per meter. For example, say you want 32 pixels per meter then you should set the `lengthUnitsPerMeter` to be 32.0f. Setting a value of (say) 32.05 would result in the 5mm being scaled up to 0.16 meters, which is a more reasonable value for determining if shapes are touching and hull vertices are too close. A good rule of thumb is to pass the pixel height of your player character to this function, so if your player character is 32 pixels high, then pass 32 to this function. Then you may confidently use pixels for all the length values sent to the physics system. All length values returned from the physics system will also then naturally be in pixels because the physics system does not do any scaling internally, however, you are now responsible for creating appropriate values for gravity, density, and forces. |
| `physicsBodyDefinition` | Get/Set the [PhysicsBodyDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBodyDefinition.html). |
| `physicsChainDefinition` | Get/Set the [PhysicsChainDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChainDefinition.html). |
| `physicsDistanceJointDefinition` | Get/Set the [PhysicsDistanceJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJointDefinition.html). |
| `physicsFixedJointDefinition` | Get/Set the [PhysicsFixedJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsFixedJointDefinition.html). |
| `physicsHingeJointDefinition` | Get/Set the [PhysicsHingeJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJointDefinition.html). |
| `physicsLayerNames` | A set of 64 "layer" names associated with each bit in a [PhysicsMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html) when used for contacts and queries. |
| `physicsRelativeJointDefinition` | Get/Set the [PhysicsRelativeJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJointDefinition.html). |
| `physicsShapeDefinition` | Get/Set the [PhysicsShapeDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShapeDefinition.html). |
| `physicsSliderJointDefinition` | Get/Set the [PhysicsSliderJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJointDefinition.html). |
| `physicsWheelJointDefinition` | Get/Set the [PhysicsWheelJointDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJointDefinition.html). |
| `physicsWorldDefinition` | Get/Set the [PhysicsWorldDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition.html). |
| `renderingMode` | Controls drawing and rendering is allowed. NOTE: Drawing and rendering are always available in the Unity Editor however rendering requires compute buffer support on any device it is used without which no rendering will occur. See [PhysicsWorld.RenderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.RenderingMode.html). |
| `transformChangeMode` | Defines when changes to [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) that has are registered with [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html) are called. NOTE: In the Unity Editor when not in Play Mode, Transform change callbacks are always and only sent at the start of the frame for authoring purposes. See [PhysicsWorld.TransformChangeMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformChangeMode.html). |
| `usePhysicsLayers` | Controls if the physics 64-bit layers are used based upon [PhysicsCoreSettings2D.physicsLayerNames](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-physicsLayerNames.html) or if not, the standard 32-bit layers based upon [UnityEngine.LayerMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.LayerMask.html). If a [PhysicsCoreSettings2D](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D.html) asset is assigned then the physics layers ([PhysicsCoreSettings2D.physicsLayerNames](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-physicsLayerNames.html)) will be used if [PhysicsCoreSettings2D.usePhysicsLayers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-usePhysicsLayers.html) is also active. If no [PhysicsCoreSettings2D](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D.html) asset is assigned then the global layers (See [UnityEngine.LayerMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.LayerMask.html)) will be used. |

### Methods

#### `new()`

## PhysicsWorld

> A world is a container for all other physics objects such as [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html), [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) etc. A world can be simulated in isolation from all other worlds. The number of worlds allocated up-front is defined by [PhysicsCoreSettings2D.initialWorldCapacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-initialWorldCapacity.html), after which more are allocated on demand as required. A world is completely isolated from all other worlds.

**Full name:** `Unity.U2D.Physics.PhysicsWorld`  
**Docs:** [Unity.U2D.Physics.PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html)

### Properties

| Name | Summary |
|------|---------|
| `aabbMargin` | Get the distance used to expand AABBs in the broadphase dynamic tree, in meters. This allows broadphase proxies to move by a small amount without triggering a tree adjustment. This value is 0.05f * [PhysicsWorld.lengthUnitsPerMeter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-lengthUnitsPerMeter.html). Normally this is 5cm. |
| `allocatedWorldCapacity` | Get the number of worlds currently allocated. The world array grows on demand, so this can exceed [PhysicsCoreSettings2D.initialWorldCapacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-initialWorldCapacity.html) and can also differ from it if that setting was changed without restarting the physics system. |
| `alwaysDrawWorlds` | Get if worlds are always drawn independent of whether rendering is currently active or not as specified by [PhysicsWorld.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-renderingMode.html). When true, world drawing is always active and a [PhysicsEvents.WorldDrawResults](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents-WorldDrawResults.html) event is produced containing the [PhysicsWorld.DrawResults](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawResults.html). When false, world drawing only occurs depending on the [PhysicsWorld.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-renderingMode.html) setting. This can be controlled via [PhysicsCoreSettings2D.alwaysDrawWorlds](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-alwaysDrawWorlds.html). |
| `autoBodyUpdateCallbacks` | Controls if body update callback targets are automatically called. See [PhysicsWorld.SendBodyUpdateCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendBodyUpdateCallbacks.html) |
| `autoClearCustom` | Controls whether this world's custom draw is automatically cleared each frame. When enabled (the default), custom draw elements with a lifetime of zero are cleared each frame so they must be submitted every frame to remain visible. When disabled, custom draw is retained until [PhysicsWorld.ClearDraw](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-ClearDraw.html) is called, so it persists across frames and repaints without being resubmitted. |
| `autoJointThresholdCallbacks` | Controls if joint threshold callback targets are automatically called. See [PhysicsWorld.SendJointThresholdCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendJointThresholdCallbacks.html) |
| `awakeBodyCount` | Get the number of awake bodies in the world. |
| `bodyMaxRotation` | Get the maximum rotation of a body per time step, in degrees. This limit is very large and is used to prevent numerical problems. This value is approximately 45-degrees or 0.25f * [PhysicsMath.PI](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMath-PI.html) radians. |
| `bodyTimeToSleep` | Get the time that a body must be still before it will go to sleep, in seconds. This value is 0.5 seconds. |
| `bodyUpdateEvents` | Get the body events from the last simulation. The [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see [PhysicsBody.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-isValid.html)). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See [PhysicsEvents.BodyUpdateEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html). |
| `bounceThreshold` | Adjust the bounce threshold, usually in meters per second. It is recommended not to make this value very small because it will prevent bodies from sleeping. |
| `bounds` | Get the bounding box that encloses all the shapes in the world. |
| `capacity` | Get the current world capacities reached since the world was created. This reflects the peak object counts and can be used to presize a [PhysicsWorldDefinition.capacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition-capacity.html) for similar worlds. See [PhysicsWorldDefinition.capacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition-capacity.html). |
| `concurrentSimulations` | Gets how many simulations can be started in parallel. Whilst running simulations in parallel can improver overall performance, workers should ideally be left free for the simulation solver otherwise it may degrade solving performance. The actual quantity of workers used will always be capped to those available on the current device. If the total number of workers available is below 4 then parallel simulation won't occur as generally this would reduce overall performance, however parallel solving of each simulation using workers will still be used. This should not be confused with the quantity of workers used when solving a simulation. |
| `contactBeginEvents` | Get the contact begin events from the last simulation. The [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html)). The [PhysicsShape.Contact](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.Contact.html) objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced. Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See [PhysicsEvents.ContactBeginEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactBeginEvent.html). |
| `contactDamping` | The contact bounciness with 1 being critical damping (non-dimensional). |
| `contactEndEvents` | Get the contact end events from the last simulation. The [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html)). The [PhysicsShape.Contact](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.Contact.html) objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced. Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See [PhysicsEvents.ContactEndEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactEndEvent.html). |
| `contactFilterCallbacks` | Controls if contact filter callbacks will be called. A contact filter callback allows direct control over whether a contact will be created between a pair of shapes. This applies to both triggers and non-triggers but only with Dynamic bodies. These are relatively expensive so disabling them can provide a significant performance benefit. A contact filter callback will call the [PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html) for both shapes involved if they implement [PhysicsCallbacks.IContactFilterCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactFilterCallback.html). |
| `contactFilterGroupMode` | Get the current value of [PhysicsCoreSettings2D.contactFilterGroupMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-contactFilterGroupMode.html). |
| `contactFilterMode` | Get the current value of [PhysicsCoreSettings2D.contactFilterMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-contactFilterMode.html). |
| `contactFrequency` | The contact stiffness, in cycles per second. |
| `contactHitEvents` | Get the contact hit events from the last simulation. The [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html)). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See [PhysicsEvents.ContactHitEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactHitEvent.html). |
| `contactHitEventThreshold` | The contact hit event threshold controls the collision speed needed to generate a contact hit event, usually in meters per second. See [PhysicsEvents.ContactHitEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactHitEvent.html). |
| `contactRecycleDistance` | The contact recycle distance, in meters. Setting this to zero disables contact point recycling. Contact recycling reuses contact points across simulation time-steps when the relative movement is small. This feature improves stability and performance by around 25% (approximately). Contact points are not recalculated until shapes move more than 5cm (default) relative to each other. Contact recycling skips some updates such as friction, pre-solve (etc) until the contacts are no longer recycled. |
| `contactSpeed` | The contact speed used to solve overlaps, in meters per second. |
| `continuousAllowed` | Controls if continuous collision detection will be used between Dynamic and Static bodies. Generally you should keep continuous collision enabled to prevent fast moving objects from going through Static objects. The performance gain from disabling continuous collision is minor. |
| `counters` | Get the world counters. |
| `defaultWorld` | Get the default world created at start-up. This world cannot be destroyed as it is permanently owned by Unity itself. See [PhysicsWorld.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SetOwner.html) and [PhysicsWorld.isOwned](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-isOwned.html). |
| `definition` | Get/Set a world definition by accessing all of its current properties. This is provided as convenience only and should not be used when performance is important as all the properties defined in the definition are accessed sequentially. You should try to only use the specific properties you need rather than using this feature. |
| `disableSimulation` | Get if the automatic simulation of any [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) is temporarily disabled. When true, no automatic simulation will occur. When false, normal operation occurs with automatic simulation. This can be controlled via [PhysicsCoreSettings2D.disableSimulation](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-disableSimulation.html). |
| `drawColors` | Controls what colors are used to draw [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html), [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) etc. |
| `drawContactType` | Controls the [PhysicsWorld.DrawContactType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawContactType.html) used when drawing contact points. |
| `drawFillAlpha` | Controls the draw fill alpha. This is used to scale the interior fill alpha and is only used when [PhysicsWorld.DrawFillOptions.Outline](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions-Outline.html) is used so that the interior color can be distinguished from the outline color by transparency. |
| `drawFillOptions` | Controls how shape geometry is filled when drawing. See [PhysicsWorld.DrawFillOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions.html). |
| `drawFilter` | Limits what gets drawn to a narrow selection. This only affects [PhysicsWorld.DrawOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawOptions.html) that are drawing all bodies, shapes etc. It does not affect selected elements or custom drawing. See [PhysicsWorld.IgnoreFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.IgnoreFilter.html). |
| `drawForceScale` | Controls the joint contact force scale used when drawing contact forces. |
| `drawNormalScale` | Controls the joint contact normal scale used when drawing contact normals. |
| `drawOptions` | Limits what gets drawn to a broad selection. See [PhysicsWorld.DrawOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawOptions.html). |
| `drawOrder` | Controls the relative order this world is drawn in across all worlds. Worlds with a lower draw order are drawn first, so worlds with a higher draw order are drawn on top. See [PhysicsWorldDefinition.drawOrder](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition-drawOrder.html). |
| `drawPointScale` | Controls the draw point scale used when drawing points. |
| `drawTarget` | Controls which Unity editor views this world is drawn into (Scene view, Game view or both). This only filters the built-in viewport rendering; it does not control whether drawing occurs at all. See [PhysicsWorldDefinition.drawTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition-drawTarget.html). |
| `drawThickness` | Controls the draw thickness (outline and orientation). |
| `elementDepth` | Controls the element depth. When using custom drawing of geometry or primitive shapes there is no reference to the orthogonal axis used with respect to the current [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlane.html). The element depth is in world-space and for each transform plan is defined as: - Element depth is rendered along the Z axis when using [PhysicsWorld.TransformPlane.XY](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-XY.html). - Element depth is rendered along the Y axis when using [PhysicsWorld.TransformPlane.XZ](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-XZ.html). - Element depth is rendered along the X axis when using [PhysicsWorld.TransformPlane.ZY](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-ZY.html). You should set the element depth before performing any custom draw. The element depth will be reset to zero when rendering is complete. |
| `eventGroupingAllowed` | Controls if contact and trigger begin/end events for shapes assigned a group are marked as being the first or last event between the two groups involved. This allows the many events produced between two groups of shapes to be reduced to a single begin and end, such as when treating multiple shapes as a single object. The marking is only calculated for shapes assigned a group and only when a begin or end event is produced, so the cost of leaving this enabled is minor. |
| `generation` | Get the world handle generation. |
| `globalCounters` | Get the world counters, summed for all the active worlds. |
| `globalProfile` | Get the world timing profile, summed for all the active worlds. |
| `gravity` | Get/Set the gravity vector applied to all bodies in the world, usually in m/s^2. |
| `hugeWorldExtent` | Gets what physics considers a large extent in the world. Positions greater than approximately 16km will have precision problems, so 100km as a limit should be fine in all cases. This is used to detect bad values. This value is 100000.0f * [PhysicsWorld.lengthUnitsPerMeter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-lengthUnitsPerMeter.html). |
| `index` | Get the world handle index. |
| `isDefaultWorld` | Check if this is the default [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html). The default world is automatically created at start-up. |
| `isEmpty` | Check if the world is empty as defined by having no bodies, shapes or joints. |
| `isOwned` | Get if the world is owned. See [PhysicsWorld.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SetOwner.html). |
| `isRenderingAllowed` | Get if rendering is currently allowed. Rendering is always allowed in the Editor however it is only allowed elsewhere depending on [PhysicsCoreSettings2D.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-renderingMode.html). |
| `isValid` | Check if the world is valid. |
| `jointThresholdEvents` | Get the joint events from the last simulation. An event is produced by a Joint which exceeds either its [PhysicsJoint.forceThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-forceThreshold.html) or [PhysicsJoint.torqueThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-torqueThreshold.html). The [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see [PhysicsJoint.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-isValid.html)). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See [PhysicsEvents.JointThresholdEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.JointThresholdEvent.html). |
| `lastSimulationDeltaTime` | Get the delta-time used for the last simulation run. |
| `lastSimulationTimestamp` | Get the timestamp when the last simulation was run. |
| `lengthUnitsPerMeter` | Get the internal length units per meter. Changes won't take effect until exiting play mode. The physics system relates all length units on meters but you may need different units for your project. You can set this value to use different units but it should only be modified before any other calls to the physics system occur and only modified once. Changing this value after any physics object has been created can result in severe simulation instabilities. For example, if your game uses pixels for units you can use pixels for all length values sent to the physics system. There should be no extra cost however, the physics system has some internal tolerances and thresholds that have been tuned for meters. By calling this function, the physics system is better able to adjust those tolerances and thresholds to improve accuracy. A good rule of thumb is to pass the height of your player character to this function. So if your player character is 32 pixels high, then pass 32 to this function. Then you may confidently use pixels for all the length values sent to the physics system. All length values returned from the physics system will also then be in pixels because the physics system does not do any scaling internally, however, you are now on the hook for coming up with good values for gravity, density, and forces. The default value is 1. |
| `linearSlop` | Get the small length used as a collision and constraint tolerance, in meters. Usually it is chosen to be numerically significant, but visually insignificant. This value is 0.005f * [PhysicsWorld.lengthUnitsPerMeter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-lengthUnitsPerMeter.html). Normally this is 0.5cm. |
| `maximumLinearSpeed` | Get/Set the maximum linear speed. |
| `owner` | The owner object associated with this world, or NULL if no owner has been specified. This is a convenience property that returns the same value as [PhysicsWorld.GetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-GetOwner.html). |
| `ownerUserData` | Get [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) that can be used for any purpose, typically by the owner only. |
| `paused` | Get/Set if the world is paused. When paused, any simulation attempted will be ignored whether it be automatic or manual. |
| `preSolveCallbacks` | Controls if pre-solve callbacks will be called. This only applies to Dynamic bodies and is ignored for triggers. These are relatively expensive so disabling them can provide a significant performance benefit. A pre-solve callback will call the [PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html) for both shapes involved if they implement [PhysicsCallbacks.IPreSolveCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IPreSolveCallback.html). |
| `profile` | Get the world timing profile. |
| `renderingMode` | Get the current value of [PhysicsCoreSettings2D.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-renderingMode.html). NOTE: Drawing and rendering are always available in the Unity Editor however rendering requires compute buffer support on any device it is used without which no rendering will occur. |
| `safetyLocksEnabled` | Get/Set whether safety threading locks are enabled or not. Locks are enabled by default however on platforms that do not support threading, locks are not used. Disabling locks can result in a small performance boost however, please note the following EXTREME CAUTIONS. Typically, per-world, multiple read operations can happen in parallel however only a single write operation can occur concurrently. Read and write operations can never happen at the same time. Locking is a self-balancing reader-preferred system that tries to reduce writers "starving". Once a writer is in a queue, it registers incoming readers as waiting readers and, once active readers are handled, it starts processing a single writer. After that writer has been handled, it flips waiting readers into active readers and processes them. Whilst this system is extremely fast, it does have a very small overhead. Disabling this system can give a small performance boost but is nearly always not worth it therefore this option should be used for testing only. EXTREME CAUTION should be taken if disabling locks on platforms that support threading! A majority of this API is thread-safe and is is due to the safety locks! Locks are used to ensure that read and write operations do not interfere with each other. Locks also ensure that no read or write operations happen during a simulation step. Overlapping read or write operations will almost certainly result in corruptions and a subsequent crash, so unless you are absolutely sure this is not the case, do not disable locks! |
| `simulationSubSteps` | Get/Set the simulation sub-steps to use during simulation. See [PhysicsWorld.Simulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Simulate.html). |
| `simulationType` | Get/Set the simulation type which controls when or if the simulation will be automatically simulated. See [PhysicsWorld.SimulationType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.SimulationType.html). |
| `simulationWorkers` | Get/Set the simulation worker count for the world. A single simulation worker is always used for simulation therefore a worker count of one means single thread simulation only. The actual quantity of workers used will always be capped to those available on the current device and reading the property will return the number of workers actually being used by the device. Changing the worker count continuously is not recommend and will impact performance as it requires the task queue be recreated. See [PhysicsWorldDefinition.simulationWorkers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition-simulationWorkers.html). |
| `sleepingAllowed` | Controls if bodies go to sleep when not moving and not interacting. Sleeping can provide a significant performance improvement when many Dynamic or Kinematic bodies are in the world. |
| `speculativeContactDistance` | Get the distance at which speculative contacts will be calculated. This reduces jitter. This value is 4.0f * [PhysicsWorld.lengthUnitsPerMeter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-lengthUnitsPerMeter.html). Normally this is 2cm. |
| `syncInterpolation` | Controls if an extra write pass prior to the script fixed-update callback is made for any interpolation tweens to ensure that transforms are synchronized to the final body pose. Because this is an extra write pass, it has an impact on overall performance so only enable if you require transforms synchronized this way. NOTE: This only affects [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) that have their [PhysicsBody.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-transformWriteMode.html) set to [PhysicsBody.TransformWriteMode.Interpolate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteMode-Interpolate.html). |
| `transformChangeMode` | Get the current value of [PhysicsCoreSettings2D.transformChangeMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-transformChangeMode.html). See [PhysicsWorld.TransformChangeMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformChangeMode.html). |
| `transformPlane` | Controls the transform plane that the world uses when writing transforms. See [PhysicsWorld.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteMode.html). |
| `transformPlaneCustom` | Controls the transformation for the [PhysicsWorld.TransformPlane.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-Custom.html) to allow transformation writing and reading to/from a custom 2D plane. See [PhysicsWorld.TransformPlaneCustom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlaneCustom.html). |
| `transformTweenMode` | Controls if and how Transform tweens are calculated and/or written. Transform tweening is where bodies that have their [PhysicsBody.transformObject](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-transformObject.html) set, write to the [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) each frame depending on the specific body [PhysicsBody.TransformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteMode.html) set. Regardless of this setting, Transform tweening is never used if the [PhysicsWorld.simulationType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-simulationType.html) is [PhysicsWorld.SimulationType.Update](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.SimulationType-Update.html) or [PhysicsWorld.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteMode.html) is [PhysicsWorld.TransformWriteMode.Off](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformWriteMode-Off.html). |
| `transformWriteCallbackTarget` | Get/Set the custom [System.Object](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/System.Object.html) that implements the [PhysicsCallbacks.ITransformWriteCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback.html) to which [PhysicsEvents.TransformWriteEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformWriteEvent.html) and [PhysicsEvents.TransformTweenWriteEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformTweenWriteEvent.html) will be sent. The callback will only occur if [PhysicsWorld.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteMode.html) is set to [PhysicsWorld.TransformWriteMode.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformWriteMode-Custom.html) and there are [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html) available. The object assigned here will be kept alive, not allowing the GC to dispose of it. To remove the object assigned here, set the callback target to NULL. |
| `transformWriteMode` | Controls how transform writing is handled. Only bodies that have their [PhysicsBody.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-transformWriteMode.html) active and produce a [PhysicsEvents.BodyUpdateEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html) will write to a transform. See [PhysicsWorld.TransformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformWriteMode.html). |
| `triggerBeginEvents` | Get the trigger begin events from the last simulation. The [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html)). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See [PhysicsEvents.TriggerBeginEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerBeginEvent.html). |
| `triggerEndEvents` | Get the trigger end events from the last simulation. The [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) objects returned should be checked to see if they are valid before accessing as they may have been deleted since this event was produced (see [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html)). Any change to the world state can invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. See [PhysicsEvents.TriggerEndEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerEndEvent.html). |
| `usePhysicsLayers` | Get if the option of [PhysicsCoreSettings2D.usePhysicsLayers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-usePhysicsLayers.html) is active or not. If no [PhysicsCoreSettings2D](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D.html) asset is assigned, this option will return false (inactive). When active, the physics 64-bit layers are used (see [PhysicsCoreSettings2D.physicsLayerNames](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-physicsLayerNames.html)) for property drawers and [PhysicsLayers.GetLayerMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsLayers-GetLayerMask.html). When inactive, the 32-bit layers are used (see [UnityEngine.LayerMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.LayerMask.html)) for property drawers and [PhysicsLayers.GetLayerMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsLayers-GetLayerMask.html). In all cases, the physics system itself will always use the full 64-bit layers assigned, however when using 32-bit layers, the top 32-bits will be set to zero. |
| `userData` | Get/Set [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) that can be used for any purpose. This cannot be set on the [PhysicsWorld.defaultWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-defaultWorld.html) and will always be at the default. The physics system doesn't use this data, it is entirely for custom use. |
| `warmStartingAllowed` | Is warm-starting allowed in the world? Disabling warming-starting will severely impact stability. This is typically used for testing only! |
| `worldCount` | Get the number of created worlds. This will be a value in the range of 1 to [PhysicsWorld.allocatedWorldCapacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-allocatedWorldCapacity.html). |

### Methods

#### `ApplyBuoyancy(PhysicsAABB, PhysicsBody.BuoyancyInput, float)`

Apply buoyancy, flow and damping forces to every dynamic body shape that overlaps `aabb` in this world. The same [PhysicsBody.BuoyancyInput](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.BuoyancyInput.html) is applied to all overlapping shapes. Shapes whose body is not [PhysicsBody.BodyType.Dynamic](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.BodyType-Dynamic.html) are silently skipped. Forces and torques are continuous (not impulses), so this is expected to be called every simulation step.

**Params:**
- `aabb` — The world-space axis-aligned box describing the fluid volume. Only shapes whose broadphase AABB overlaps this box are processed.
- `input` — The fluid and force configuration. See [PhysicsBody.BuoyancyInput](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.BuoyancyInput.html).
- `deltaTime` — The simulation step duration in seconds. Used to clamp damping so it cannot overshoot in a single step.

#### `ApplySnapshot(PhysicsWorld.Snapshot)`

Restore this world to the state captured in `snapshot`, in place. The world keeps the same handles, so any [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) and [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) you already hold remain valid.

**Params:**
- `snapshot` — A snapshot produced by [PhysicsWorld.CreateSnapshot](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CreateSnapshot.html) on a compatible world.

**Returns:** Whether the world was restored. Returns false if the snapshot or world is invalid, or the image is rejected.

#### `ApplyWind(PhysicsAABB, PhysicsBody.WindInput)`

Apply wind forces to every dynamic body shape that overlaps `aabb` in this world. The same [PhysicsBody.WindInput](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.WindInput.html) is applied to all overlapping shapes; shapes whose body is not [PhysicsBody.BodyType.Dynamic](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.BodyType-Dynamic.html) are silently skipped. Forces are continuous (not impulses), so this is expected to be called every simulation step.

**Params:**
- `aabb` — The world-space axis-aligned box describing the wind volume. Only shapes whose broadphase AABB overlaps this box are processed.
- `input` — The wind configuration. See [PhysicsBody.WindInput](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.WindInput.html).

#### `CastGeometry(CircleGeometry, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Circle geometry as it is cast through the world. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldCastMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastMode.html), [PhysicsQuery.WorldCastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html)

**Params:**
- `geometry` — The Circle geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastGeometry(CapsuleGeometry, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Capsule geometry as it is cast through the world. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldCastMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastMode.html), [PhysicsQuery.WorldCastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Capsule geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastGeometry(PolygonGeometry, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Polygon geometry as it is cast through the world. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldCastMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastMode.html), [PhysicsQuery.WorldCastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Polygon geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastMover(PhysicsQuery.WorldMoverInput)`

Cast a "Mover" which is geometry designed to collide with the world and solve its movement. Everything is specified via the [PhysicsQuery.WorldMoverInput](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldMoverInput.html) with results returned in [PhysicsQuery.WorldMoverResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldMoverResult.html).

**Params:**
- `input` — The configuration of the mover to cast.

**Returns:** The solved mover results.

#### `CastRay(PhysicsQuery.CastRayInput, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Ray. Technically this is a line-segment and not an infinite ray. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldCastMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastMode.html), [PhysicsQuery.WorldCastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `input` — The configuration of the ray to cast.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastShape(PhysicsShape, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified shape as it is cast through the world. The selected shape is excluded from any results and must be in this world otherwise a warning will be produced. Neither [PhysicsShape.ShapeType.Segment](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-Segment.html) or [PhysicsShape.ShapeType.ChainSegment](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-ChainSegment.html) shape types are supported. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldCastMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastMode.html), [PhysicsQuery.WorldCastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `shape` — The shape used to cast through the world.
- `translation` — The translation relative to the shape pose defining the direction the shape geometry will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastShapeProxy(PhysicsShape.ShapeProxy, Vector2, PhysicsQuery.QueryFilter, PhysicsQuery.WorldCastMode, Unity.Collections.Allocator)`

Returns the shape(s) that intersect the specified Circle geometry as it is cast through the world. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldCastMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastMode.html), [PhysicsQuery.WorldCastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `shapeProxy` — The shape proxy to use. This must be in world-space.
- `translation` — The translation relative to the shape proxy defining the direction the shape proxy will move through the world.
- `filter` — The filter to control what results are returned.
- `castMode` — Controls how many and in what order the results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CheckTransformChanges()`

Checks for any transform changes. Anything using [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html) will immediately be notified of any changes. This should be used sparingly otherwise it may impact performance. The preference should be not using this but instead control transform changes to be monitored with [PhysicsWorld.transformChangeMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformChangeMode.html).

**Returns:** The number of changed transforms that were detected.

#### `ClearDraw()`

Clear all the custom drawn items.

#### `Clone()`

Creates a new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) that is a copy of this world, including all of its [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) and [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html).

**Returns:** The cloned world.

#### `Create()`

Create a PhysicsWorld using the [PhysicsWorldDefinition.defaultDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition-defaultDefinition.html).

**Returns:** The created world.

#### `Create(PhysicsWorldDefinition)`

Create a PhysicsWorld.

**Params:**
- `definition` — The world definition to use.

**Returns:** The created world.

#### `Create(PhysicsWorld.Snapshot, PhysicsWorldDefinition)`

Creates a new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) from `snapshot`, using `definition` for the settings a snapshot does not store.

**Params:**
- `snapshot` — A snapshot produced by [PhysicsWorld.CreateSnapshot](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CreateSnapshot.html).
- `definition` — The world definition supplying the settings the snapshot does not store.

**Returns:** The created world, restored to the snapshot state.

#### `CreateBody()`

Create a body using the [PhysicsBodyDefinition.defaultDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBodyDefinition-defaultDefinition.html) in the world. See [PhysicsBody.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-Create.html).

**Returns:** The created body.

#### `CreateBody(PhysicsBodyDefinition)`

Create a body in the world. See [PhysicsBody.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-Create.html).

**Params:**
- `definition` — The body definition to use.

**Returns:** The created body.

#### `CreateBodyBatch(PhysicsBodyDefinition, int, Unity.Collections.Allocator)`

Create a batch of bodies in the world.

**Params:**
- `definition` — The body definition to use.
- `bodyCount` — The number of bodies to create.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The created bodies. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateBodyBatch(ReadOnlySpan<PhysicsBodyDefinition>, Unity.Collections.Allocator)`

Create a batch of bodies in the world.

**Params:**
- `definitions` — The definitions used to create the bodies. The number of bodies produced is implicitly controlled by the number of definitions in this span.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The created bodies. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateGroup()`

Create a new globally unique group. Groups are created from a rolling index, so the first group created has a group index of one and a group index of zero always means no group.

**Returns:** The new group.

#### `CreateJoint(PhysicsDistanceJointDefinition)`

Create a PhysicsDistanceJoint in the world. See [PhysicsDistanceJoint.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJoint-Create.html).

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsRelativeJointDefinition)`

Create a PhysicsRelativeJoint in the world. See [PhysicsRelativeJoint.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJoint-Create.html).

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsIgnoreJointDefinition)`

Create an PhysicsIgnoreJoint in the world. See [PhysicsIgnoreJoint.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsIgnoreJoint-Create.html).

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsSliderJointDefinition)`

Create a PhysicsSliderJoint in the world. See [PhysicsSliderJoint.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJoint-Create.html).

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsHingeJointDefinition)`

Create a PhysicsHingeJoint in the world. See [PhysicsHingeJoint.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJoint-Create.html).

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsFixedJointDefinition)`

Create a PhysicsFixedJoint in the world. See [PhysicsFixedJoint.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsFixedJoint-Create.html).

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateJoint(PhysicsWheelJointDefinition)`

Create a PhysicsWheelJoint in the world. See [PhysicsWheelJoint.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJoint-Create.html).

**Params:**
- `definition` — The joint definition to use.

**Returns:** The created joint.

#### `CreateOwnerKey(Object)`

Create an owner key.

**Params:**
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended as the owner key can use the hash-code of the object to generate a more unique key.

**Returns:** The new owner key.

#### `CreateSnapshot(Unity.Collections.Allocator)`

Capture the current simulation state of this world into a [PhysicsWorld.Snapshot](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.Snapshot.html).

**Params:**
- `allocator` — The allocator for the snapshot memory. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** A snapshot of the world that must be disposed of after use. The snapshot is not created if the world or allocator is invalid.

#### `Destroy(int)`

Destroy a world, destroying all objects contained within it such as all [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) and attached [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) and [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html). If the object is owned with [PhysicsWorld.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SetOwner.html) then you must provide the owner key it returned. Failing to do so will return a warning and the world will not be destroyed. You cannot destroy the [PhysicsWorld.defaultWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-defaultWorld.html) as it is permanently owned by Unity itself.

**Params:**
- `ownerKey` — Optional owner key returned when using [PhysicsWorld.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SetOwner.html).

**Returns:** If the world was destroyed or not.

#### `DestroyBodyBatch(ReadOnlySpan<PhysicsBody>)`

Destroy a batch of bodies. Any invalid bodies will be ignored. Owned bodies will produce a warning and will not be destroyed (See [PhysicsBody.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-SetOwner.html)).

**Params:**
- `bodies` — The bodies to destroy.

#### `DestroyJointBatch(ReadOnlySpan<PhysicsJoint>)`

Destroy a batch of joints. Any invalid joints will be ignored. Owned joints will produce a warning and will not be destroyed ([PhysicsJoint.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-SetOwner.html)).

**Params:**
- `joints` — The joints to destroy.

#### `DestroyShapeBatch(ReadOnlySpan<PhysicsShape>, bool)`

Destroy a batch of shapes, destroying all [PhysicsShape.Contact](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.Contact.html) the shapes are involved in. Any invalid shapes will be ignored including chain segment shapes created via a [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html) (the chain must be destroyed)." Owned shapes will produce a warning and will not be destroyed ([PhysicsShape.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-SetOwner.html)). See [PhysicsBody.MassConfiguration](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.MassConfiguration.html).

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

Draw a Capsule outline. For further information on the parameters, see [CapsuleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html).

**Params:**
- `transform` — The transform to use on the specified centers.
- `center1` — The local center of the first semi-circle.
- `center2` — The local center of the second semi-circle.
- `radius` — The radius of the capsule.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawCircle(Vector2, float, Color, float, PhysicsWorld.DrawFillOptions)`

Draw a Circle outline. For further information on the parameters, see [CircleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html).

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

#### `DrawGeometry(ReadOnlySpan<CircleGeometry>, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

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

#### `DrawGeometry(ReadOnlySpan<CapsuleGeometry>, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

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

#### `DrawGeometry(ReadOnlySpan<PolygonGeometry>, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

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

#### `DrawGeometry(ReadOnlySpan<SegmentGeometry>, PhysicsTransform, Color, float)`

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

#### `DrawLineStrip(PhysicsTransform, ReadOnlySpan<Vector2>, bool, Color, float)`

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

Draw the [PhysicsWorld.CastGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CastGeometry.html) query input.

**Params:**
- `geometry` — The Circle geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the geometry at the end of the translation or not.

#### `DrawQueryCastGeometry(CapsuleGeometry, Vector2, Color, float, PhysicsWorld.DrawFillOptions, bool)`

Draw the [PhysicsWorld.CastGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CastGeometry.html) query input.

**Params:**
- `geometry` — The Circle geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the geometry at the end of the translation or not.

#### `DrawQueryCastGeometry(PolygonGeometry, Vector2, Color, float, PhysicsWorld.DrawFillOptions, bool)`

Draw the [PhysicsWorld.CastGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CastGeometry.html) query input.

**Params:**
- `geometry` — The Circle geometry used to cast through the world. This must be in world-space.
- `translation` — The translation relative to the geometry defining the direction the geometry will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the geometry at the end of the translation or not.

#### `DrawQueryCastRay(PhysicsQuery.CastRayInput, Color, float, bool)`

Draw the [PhysicsWorld.CastRay](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CastRay.html) query input.

**Params:**
- `input` — The query input to draw.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawEnd` — Whether to draw the arrow at the end of the translation or not.

#### `DrawQueryCastRay(ReadOnlySpan<PhysicsQuery.CastRayInput>, Color, float, bool)`

Draw the [PhysicsWorld.CastRay](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CastRay.html) query inputs.

**Params:**
- `inputs` — The query inputs to draw.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawEnd` — Whether to draw the arrow at the end of the translation or not.

#### `DrawQueryCastShape(PhysicsShape, Vector2, Color, float, PhysicsWorld.DrawFillOptions, bool)`

Draw the [PhysicsWorld.CastShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CastShape.html) query input.

**Params:**
- `shape` — The shape used to cast through the world.
- `translation` — The translation relative to the shape pose defining the direction the geometry will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the shape at the end of the translation or not.

#### `DrawQueryCastShapeProxy(PhysicsShape.ShapeProxy, Vector2, Color, float, PhysicsWorld.DrawFillOptions, bool)`

Draw the [PhysicsWorld.CastShapeProxy](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CastShapeProxy.html) query input.

**Params:**
- `shapeProxy` — The shape proxy to use. This must be in world-space.
- `translation` — The translation relative to the shape proxy defining the direction the shape proxy will move through the world.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.
- `drawEnd` — Whether to draw the shape proxy at the end of the translation or not.

#### `DrawQueryResult(PhysicsQuery.CastResult, Color, float, bool, bool)`

Draw the [PhysicsQuery.CastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.CastResult.html) returned from multiple queries. Only a result where [PhysicsQuery.CastResult.hit](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.CastResult-hit.html) is true is drawn.

**Params:**
- `result` — The result to use.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawPoint` — Whether to draw the point in the result or not.
- `drawNormal` — Whether to draw the normal in the result or not.

#### `DrawQueryResult(ReadOnlySpan<PhysicsQuery.CastResult>, Color, float, bool, bool)`

Draw the [PhysicsQuery.CastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.CastResult.html) returned from multiple queries. Only a result where [PhysicsQuery.CastResult.hit](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.CastResult-hit.html) is true is drawn.

**Params:**
- `results` — The results to use.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawPoint` — Whether to draw the point in the result or not.
- `drawNormal` — Whether to draw the normal in the result or not.

#### `DrawQueryResult(PhysicsQuery.WorldCastResult, Color, float, bool, bool)`

Draw the [PhysicsQuery.WorldCastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastResult.html) returned from multiple queries.

**Params:**
- `result` — The result to use.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawPoint` — Whether to draw the point in the result or not.
- `drawNormal` — Whether to draw the normal in the result or not.

#### `DrawQueryResult(ReadOnlySpan<PhysicsQuery.WorldCastResult>, Color, float, bool, bool)`

Draw the [PhysicsQuery.WorldCastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldCastResult.html) returned from multiple queries.

**Params:**
- `results` — The results to use.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawPoint` — Whether to draw the point in the result or not.
- `drawNormal` — Whether to draw the normal in the result or not.

#### `DrawShapeProxy(PhysicsShape.ShapeProxy, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw a [PhysicsShape.ShapeProxy](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeProxy.html).

**Params:**
- `shapeProxy` — The ShapeProxy to draw.
- `transform` — The transform to use on the specified points.
- `color` — The color to draw with.
- `lifetime` — How long the element should be drawn for, in seconds. The default is zero indicating that it should only be drawn once. Lifetime is only used when the world is playing.
- `drawFillOptions` — Controls what aspects of the primitive is drawn.

#### `DrawShapeProxy(ReadOnlySpan<PhysicsShape.ShapeProxy>, PhysicsTransform, Color, float, PhysicsWorld.DrawFillOptions)`

Draw the specified span of [PhysicsShape.ShapeProxy](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeProxy.html).

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

Get all the active [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) in the specified world.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The active body results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetBodyUpdateCallbackTargets(Unity.Collections.Allocator)`

Get all current [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html) where either of the [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) involved are valid (see [PhysicsBody.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-isValid.html)) and have a callback target assigned (see [PhysicsBody.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-callbackTarget.html)).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The contact callback target results. This must be disposed of after use otherwise leaks will occur. The exception to this is if there are no targets returned.

#### `GetBodyUpdateOwnerUserData(Unity.Collections.Allocator)`

Get all [PhysicsBody.ownerUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-ownerUserData.html) assigned to each [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) returned with [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html). The Native Array returned will be of the same length and be ordered the same as the [PhysicsEvents.BodyUpdateEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html) returned with [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html). Any [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) that are not valid will return a default [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** A Native Array containing all [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) for each [PhysicsEvents.BodyUpdateEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html) returned with [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html).

#### `GetBodyUpdateUserData(Unity.Collections.Allocator)`

Get all [PhysicsBody.userData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-userData.html) assigned to each [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) returned with [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html). The Native Array returned will be of the same length and be ordered the same as the [PhysicsEvents.BodyUpdateEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html) returned with [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html). Any [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) that are not valid will return a default [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** A Native Array containing all [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) for each [PhysicsEvents.BodyUpdateEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html) returned with [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html).

#### `GetContactCallbackTargets(Unity.Collections.Allocator)`

Get all current [PhysicsWorld.contactBeginEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactBeginEvents.html) and [PhysicsWorld.contactEndEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactEndEvents.html) where either of the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) involved are valid (see [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html)) and have a callback target assigned (see [PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The contact callback target results. This must be disposed of after use otherwise leaks will occur. The exception to this is if there are no targets returned.

#### `GetHashCode()`

#### `GetJoints(Unity.Collections.Allocator)`

Get all the active [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) in the specified world.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The active joints results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetJointThresholdCallbackTargets(Unity.Collections.Allocator)`

Get all current [PhysicsWorld.jointThresholdEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-jointThresholdEvents.html) where either of the [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) involved are valid (see [PhysicsJoint.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-isValid.html)) and have a callback target assigned (see [PhysicsJoint.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-callbackTarget.html)).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The joint callback target results. This must be disposed of after use otherwise leaks will occur. The exception to this is if there are no targets returned.

#### `GetOwnedTransforms()`

Get the [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) that owns each [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) and [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) in this world. Only owners that are a [UnityEngine.Component](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Component.html) contribute a Transform, and each Transform is included only once. This must be called from the main thread.

**Returns:** The owned transform results as a [UnityEngine.Jobs.TransformAccessArray](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Jobs.TransformAccessArray.html). This must be disposed of after use otherwise leaks will occur.

#### `GetOwner()`

Get the owner object associated with this world as specified using [PhysicsWorld.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SetOwner.html).

**Returns:** The owner object associated with this world or NULL if no owner has been specified.

#### `GetTransformWriteTweens()`

Gets all the existing Transform Write Tweens that are handled per-frame. If the [PhysicsWorld.transformTweenMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformTweenMode.html) is [PhysicsWorld.TransformTweenMode.Sequential](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformTweenMode-Sequential.html) then the tweens are sorted into ascending transform depth allowing writing to the Transform hierarchy by simply iterating the tweens . If the [PhysicsWorld.transformTweenMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformTweenMode.html) is [PhysicsWorld.TransformTweenMode.Sequential](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformTweenMode-Sequential.html) then the tweens are unsorted as a [UnityEngine.Jobs.TransformAccessArray](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Jobs.TransformAccessArray.html) is used to write them. See [PhysicsBody.TransformWriteTween](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteTween.html) and [PhysicsBody.TransformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteMode.html). The returned [Unity.Collections.NativeArray](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.NativeArray.html) aliases the per-frame internal buffer owned by the world; it does not own its memory (so disposing it does nothing). The contents are only valid until the next simulation step runs, after which the buffer may be reused or destroyed. If a longer-lived copy is required, copy the contents into a caller-owned [Unity.Collections.NativeArray](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.NativeArray.html).

**Returns:** A world-owned view of the existing Transform Write Tweens that are handled per-frame. Contents are invalidated by the next simulation step.

#### `GetTriggerCallbackTargets(Unity.Collections.Allocator)`

Get all current [PhysicsWorld.triggerBeginEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-triggerBeginEvents.html) and [PhysicsWorld.triggerEndEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-triggerEndEvents.html) where either of the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) involved are valid (see [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html)) and have a callback target assigned (see [PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)).

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The trigger callback target results. This must be disposed of after use otherwise leaks will occur. The exception to this is if there are no targets returned.

#### `GetWorlds(Unity.Collections.Allocator)`

Get all the active [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html). This includes the [PhysicsWorld.defaultWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-defaultWorld.html) so will always contain at least a single world.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The active world results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapAABB(PhysicsAABB, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that potentially overlap the provided AABB. The overlap is between AABB of shapes in the world therefore it may not result in an exact overlap of the shape itself. See [PhysicsAABB](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsAABB.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `aabb` — The AABB used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapAABB(ReadOnlySpan<PhysicsAABB>, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that potentially overlap the provided AABBs. The overlap is between AABB of shapes in the world therefore it may not result in an exact overlap of the shape itself. See [PhysicsAABB](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsAABB.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `aabbs` — The AABBs used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(CircleGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Circle geometry. A circle with a radius of zero is equivalent to [PhysicsWorld.OverlapPoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-OverlapPoint.html). See [CircleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html)

**Params:**
- `geometry` — The Circle geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan<CircleGeometry>, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Circle geometry. A circle with a radius of zero is equivalent to [PhysicsWorld.OverlapPoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-OverlapPoint.html). See [CircleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html)

**Params:**
- `geometry` — The Circle geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(CapsuleGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Capsule geometry. See [CapsuleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Capsule geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan<CapsuleGeometry>, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Capsule geometry. See [CapsuleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Capsule geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(PolygonGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Polygon geometry. See [PolygonGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Polygon geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan<PolygonGeometry>, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Polygon geometry. See [PolygonGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Polygon geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(SegmentGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Segment geometry. See [SegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.SegmentGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Segment geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan<SegmentGeometry>, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Segment geometry. See [SegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.SegmentGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Segment geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ChainSegmentGeometry, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Chain-Segment geometry. See [ChainSegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Chain-Segment geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapGeometry(ReadOnlySpan<ChainSegmentGeometry>, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided Chain-Segment geometry. See [ChainSegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html), [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `geometry` — The Chain-Segment geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapPoint(Vector2, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided point. This first converts the shape to a [PhysicsShape.ShapeProxy](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeProxy.html) and uses [PhysicsWorld.TestOverlapShapeProxy](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-TestOverlapShapeProxy.html). See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `point` — The point used to check overlap. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapPoint(ReadOnlySpan<Vector2>, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided point(s). This first converts the shape to a [PhysicsShape.ShapeProxy](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeProxy.html) and uses [PhysicsWorld.TestOverlapShapeProxy](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-TestOverlapShapeProxy.html). See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html), [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `points` — The points used to check overlap. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapShape(PhysicsShape, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the provided shape. The selected shape is excluded from any results and must be in this world otherwise a warning will be produced.

**Params:**
- `shape` — The shape used to check overlap.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapShapeProxy(PhysicsShape.ShapeProxy, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the shape proxy. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html). [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `shapeProxy` — The shape proxy to use. This must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapShapeProxy(ReadOnlySpan<PhysicsShape.ShapeProxy>, PhysicsQuery.QueryFilter, Unity.Collections.Allocator)`

Returns all shapes that overlap the shape proxies. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html). [PhysicsQuery.WorldOverlapResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.WorldOverlapResult.html) and [Unity.Collections.Allocator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator.html).

**Params:**
- `shapeProxies` — The shape proxies to use. These must be in world-space.
- `filter` — The filter to control what results are returned.
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `RegisterTransformChange(Transform, PhysicsCallbacks.ITransformChangedCallback)`

Register a transform watcher to call the specified callback when a transform changes. See [PhysicsEvents.TransformChangeEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent.html) for the types of transform changes that are watched for. You MUST unregister this when no longer needed with [PhysicsWorld.UnregisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-UnregisterTransformChange.html) otherwise you will receive warnings.

**Params:**
- `transform` — The transform to watch for changes.
- `callback` — The callback to perform when a transform change is detected.

#### `Reset()`

Reset the world to a canonical state so that it will reproduce identical results each time. The world must be empty for this to be called otherwise a warning is produced.

#### `SendAllCallbacks()`

Send all callbacks to targets: - [PhysicsWorld.SendBodyUpdateCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendBodyUpdateCallbacks.html) - [PhysicsWorld.SendTriggerCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendTriggerCallbacks.html) - [PhysicsWorld.SendContactCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendContactCallbacks.html) - [PhysicsWorld.SendJointThresholdCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendJointThresholdCallbacks.html)

#### `SendBodyUpdateCallbacks()`

Send all current [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html) where the [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) involved are valid (see [PhysicsBody.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-isValid.html)) and have a callback target assigned (see [PhysicsBody.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-callbackTarget.html)). Only callback targets that implement [PhysicsCallbacks.IBodyUpdateCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IBodyUpdateCallback.html) will be called. This will be called automatically if [PhysicsWorld.autoBodyUpdateCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-autoBodyUpdateCallbacks.html) is true. This must be called on the main thread.

#### `SendContactCallbacks()`

Send all current [PhysicsWorld.contactBeginEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactBeginEvents.html) and [PhysicsWorld.contactEndEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactEndEvents.html) where either of the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) involved are valid (see [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html)) and have a callback target assigned (see [PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)). Only callback targets that implement [PhysicsCallbacks.IContactCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactCallback.html) will be called. This is called automatically every simulation step. This must be called on the main thread.

#### `SendJointThresholdCallbacks()`

Send all current [PhysicsWorld.jointThresholdEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-jointThresholdEvents.html) where the [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) involved are valid (see [PhysicsJoint.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-isValid.html)) and have a callback target assigned (see [PhysicsJoint.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-callbackTarget.html)). These events will only be created if the joint exceeds its [PhysicsJoint.forceThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-forceThreshold.html) or [PhysicsJoint.torqueThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-torqueThreshold.html). Only callback targets that implement [PhysicsCallbacks.IJointThresholdCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IJointThresholdCallback.html) will be called. This will be called automatically if [PhysicsWorld.autoJointThresholdCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-autoJointThresholdCallbacks.html) is true. This must be called on the main thread.

#### `SendTriggerCallbacks()`

Send all current [PhysicsWorld.triggerBeginEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-triggerBeginEvents.html) and [PhysicsWorld.triggerEndEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-triggerEndEvents.html) where either of the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) involved are valid (see [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html)) and have a callback target assigned (see [PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)). These events will only be created if one of the shape pairs has [PhysicsShape.triggerEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-triggerEvents.html) set to true. Only callback targets that implement [PhysicsCallbacks.ITriggerCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback.html) will be called. This is called automatically every simulation step. This must be called on the main thread.

#### `SetElementDepth3D(Vector3)`

Set the element depth using the specified 3D position. The relevant axis will be extracted using the current [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlane.html). If [PhysicsWorld.TransformPlane.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-Custom.html) is used, the element depth is always set to zero. For more details, see [PhysicsWorld.elementDepth](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-elementDepth.html).

**Params:**
- `position` — The 3D position to extract the element depth from.

#### `SetOwner(ReadOnlySpan<PhysicsWorld>, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `worlds` — The worlds to set ownership for.
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.
- `ownerKey` — The owner key to be used. The value must be non-zero. You can use [PhysicsWorld.CreateOwnerKey](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CreateOwnerKey.html) for this value although any non-zero integer will work.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use [PhysicsWorld.CreateOwnerKey](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CreateOwnerKey.html) for this value although any non-zero integer will work.

#### `SetOwner(Object)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.

**Returns:** The owner key assigned.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using [PhysicsWorld.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SetOwner.html).

#### `SetTransform(Transform, Vector3, bool)`

Set the [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) position without causing a [PhysicsEvents.TransformChangeEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent.html) to be generated by default. See [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html).

**Params:**
- `transform` — The transform to change.
- `position` — The global position to set the transform to.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `SetTransform(Transform, Vector3, Quaternion, bool)`

Set the [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) position and rotation without causing a [PhysicsEvents.TransformChangeEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent.html) to be generated. See [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html).

**Params:**
- `transform` — The transform to change.
- `position` — The global position to set the transform to.
- `rotation` — The global rotation to set the transform to.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `SetTransformAccess(Jobs.TransformAccess, Vector3, bool)`

Set the [UnityEngine.Jobs.TransformAccess](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Jobs.TransformAccess.html) position without causing a [PhysicsEvents.TransformChangeEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent.html) to be generated. See [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html).

**Params:**
- `transformAccess` — The [UnityEngine.Jobs.TransformAccess](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Jobs.TransformAccess.html)used to change the transform.
- `position` — The global position to set the transform to.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `SetTransformAccess(Jobs.TransformAccess, Vector3, Quaternion, bool)`

Set the [UnityEngine.Jobs.TransformAccess](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Jobs.TransformAccess.html) position and rotation without causing a [PhysicsEvents.TransformChangeEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent.html) to be generated. See [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html).

**Params:**
- `transformAccess` — The [UnityEngine.Jobs.TransformAccess](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Jobs.TransformAccess.html)used to change the transform.
- `position` — The global position to set the transform to.
- `rotation` — The global rotation to set the transform to.
- `transformChangedEvent` — By default, no transform changed event will be produced however this behaviour can be overridden with this argument.

#### `Simulate(float)`

Simulate the world. If `deltaTime` is zero then only contact and trigger events will be updated and no velocity or position integration or constraint updates will occur.

**Params:**
- `deltaTime` — The amount of time to forward simulate the world.

#### `Simulate(ReadOnlySpan<PhysicsWorld>, float)`

Simulate a batch of worlds. If `deltaTime` is zero then only contact and trigger events will be updated and no velocity or position integration or constraint updates will occur. The worlds can be simulated concurrently depending on the setting of [PhysicsCoreSettings2D.concurrentSimulations](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-concurrentSimulations.html).

**Params:**
- `worlds` — The worlds to forward simulate.
- `deltaTime` — The amount of time to forward simulate the world.

#### `TestOverlapAABB(PhysicsAABB, PhysicsQuery.QueryFilter)`

Tests if the provided AABB potentially overlaps any shapes. The overlap is between AABB of shapes in the world therefore it may not result in an exact overlap of any shape itself. See [PhysicsAABB](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsAABB.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `aabb` — The AABB used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapAABB(ReadOnlySpan<PhysicsAABB>, PhysicsQuery.QueryFilter)`

Tests if the provided AABBs potentially overlap any shapes. The overlap is between AABB of shapes in the world therefore it may not result in an exact overlap of any shape itself. See [PhysicsAABB](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsAABB.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `aabbs` — The AABB used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(CircleGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Circle geometry overlaps any shapes. A circle with a radius of zero is equivalent to [PhysicsWorld.TestOverlapPoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-TestOverlapPoint.html). See [CircleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Circle geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan<CircleGeometry>, PhysicsQuery.QueryFilter)`

Tests if the provided Circle geometry overlaps any shapes. A circle with a radius of zero is equivalent to [PhysicsWorld.TestOverlapPoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-TestOverlapPoint.html). See [CircleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Circle geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(CapsuleGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Capsule geometry overlaps any shapes. See [CapsuleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Capsule geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan<CapsuleGeometry>, PhysicsQuery.QueryFilter)`

Tests if the provided Capsule geometry overlaps any shapes. See [CapsuleGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Capsule geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(PolygonGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Polygon geometry overlaps any shapes. See [PolygonGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Polygon geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan<PolygonGeometry>, PhysicsQuery.QueryFilter)`

Tests if the provided Polygon geometry overlaps any shapes. See [PolygonGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Polygon geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(SegmentGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Segment geometry overlaps any shapes. See [SegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.SegmentGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Segment geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan<SegmentGeometry>, PhysicsQuery.QueryFilter)`

Tests if the provided Segment geometry overlaps any shapes. See [SegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.SegmentGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Segment geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ChainSegmentGeometry, PhysicsQuery.QueryFilter)`

Tests if the provided Chain-Segment geometry overlaps any shapes. See [ChainSegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Chain-Segment geometry used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapGeometry(ReadOnlySpan<ChainSegmentGeometry>, PhysicsQuery.QueryFilter)`

Tests if the provided Chain-Segment geometry overlaps any shapes. See [ChainSegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html) and [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `geometry` — The Chain-Segment geometry used to check overlap. These must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapPoint(Vector2, PhysicsQuery.QueryFilter)`

Tests if the provided point overlaps any shapes. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

**Params:**
- `point` — The point used to check overlap. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `TestOverlapPoint(ReadOnlySpan<Vector2>, PhysicsQuery.QueryFilter)`

Tests if the provided point(s) overlap any shapes. See [PhysicsQuery.QueryFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.QueryFilter.html).

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

#### `TestOverlapShapeProxy(ReadOnlySpan<PhysicsShape.ShapeProxy>, PhysicsQuery.QueryFilter)`

Test if the provided shape proxies overlaps any shapes.

**Params:**
- `shapeProxies` — The shape proxy to use. This must be in world-space.
- `filter` — The filter to control the result returned.

**Returns:** If the query overlaps anything.

#### `ToString()`

#### `UnregisterTransformChange(Transform, PhysicsCallbacks.ITransformChangedCallback)`

Unregister a transform watched to stop calling the specified callback when a transform changes. See [PhysicsEvents.TransformChangeEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent.html) for the types of transform changes that are watched for.

**Params:**
- `transform` — The transform to stop watching changes on.
- `callback` — The callback to stop being called when a transform change is detected.

### Nested Types

- **DrawColors** — The colors used to draw [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html), [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) etc.
- **DrawContactType** — Controls which properties of a [PhysicsShape.ContactManifold.ManifoldPoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPoint.html) are drawn when drawing contact points.
- **DrawFillOptions** — Controls how shape geometry is filled when drawing.
- **DrawOptions** — Draw Options limits what gets drawn to a broad selection.
- **DrawResults** — The draw results retrieved from the world. You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided.
- **DrawTarget** — Controls which Unity editor views a [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) is drawn into. This only filters the built-in viewport rendering and does not control whether drawing occurs at all, which is governed by [PhysicsWorld.DrawOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawOptions.html) and [PhysicsCoreSettings2D.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-renderingMode.html).
- **ExplosionDefinition** — Used to define the parameters when using [PhysicsWorld.Explode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Explode.html).
- **IgnoreFilter** — A ignore flags are a narrow selection of objects/types in the world which needs to be ignored.
- **PhysicsGroup** — An abstract group identity that can be shared by a set of physics objects. Each group is globally unique across all worlds.
- **RenderingMode** — Controls drawing and rendering is allowed. NOTE: Drawing and rendering are always available in the Unity Editor however rendering requires compute buffer support on any device it is used without which no rendering will occur.
- **SimulationType** — Defines when the simulation will run.
- **Snapshot** — An opaque, point-in-time capture of a [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) simulation state, produced by [PhysicsWorld.CreateSnapshot](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CreateSnapshot.html). Restore it into the same world with [PhysicsWorld.ApplySnapshot](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-ApplySnapshot.html), or create a new world from it with [PhysicsWorld.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Create.html).
- **TransformChangeMode** — Defines when changes to [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) that are registered with [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html) are called. NOTE: In the Unity Editor when not in Play Mode, Transform change callbacks are always and only sent at the start of the frame for authoring purposes.
- **TransformChangeReason** — Defines the reason why a [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) changed. Register and unregister for transform changes with [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html) and [PhysicsWorld.UnregisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-UnregisterTransformChange.html).
- **TransformPlane** — Defines the 2D Transform plane where Transform writes will occur. This also defines the rotation axis which will automatically be perpendicular to the selected plane. See [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlane.html).
- **TransformPlaneCustom** — A transformation applied to the transform write if [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlane.html) is set to [PhysicsWorld.TransformPlane.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-Custom.html).
- **TransformTweenMode** — Defines if and how Transform tweens are calculated and/or written.
- **TransformWriteMode** — Defines how the 2D Transforms from each [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) are written to the 3D Transform system.
- **WorldCapacity** — Describes the expected world capacities used to presize internal allocations when a [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) is created. All counts default to zero, in which case the engine uses its own minimum defaults. See [PhysicsWorldDefinition.capacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition-capacity.html) and [PhysicsWorld.capacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-capacity.html).
- **WorldCounters** — PhysicsWorld counters that give details of the world simulation size.
- **WorldProfile** — PhysicsWorld profile that contains the timings of specific world simulation stages. All times are in milliseconds.

### DrawColors

> The colors used to draw [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html), [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) etc.

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
| `shapeBounds` | The shape bounds. |
| `shapeOther` | The default color used when no other shape state is indicated. |
| `shapeTrigger` | A shape that is marked as a trigger. |
| `solverIsland` | A solver island region. |
| `transformAxisX` | The X component of the Transform axis. |
| `transformAxisY` | The Y component of the Transform axis. |

### DrawContactType

> Controls which properties of a [PhysicsShape.ContactManifold.ManifoldPoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPoint.html) are drawn when drawing contact points.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawContactType`  

#### Fields

| Name | Summary |
|------|---------|
| `AnchorA` | This will draw [PhysicsShape.ContactManifold.ManifoldPoint.anchorA](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPoint-anchorA.html). |
| `AnchorB` | This will draw [PhysicsShape.ContactManifold.ManifoldPoint.anchorB](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPoint-anchorB.html). |
| `Average` | This will draw the position half-way between [PhysicsShape.ContactManifold.ManifoldPoint.anchorA](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPoint-anchorA.html) and [PhysicsShape.ContactManifold.ManifoldPoint.anchorB](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPoint-anchorB.html). |
| `Point` | This will draw [PhysicsShape.ContactManifold.ManifoldPoint.point](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPoint-point.html). |

### DrawFillOptions

> Controls how shape geometry is filled when drawing.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawFillOptions`  

#### Fields

| Name | Summary |
|------|---------|
| `All` | A combination drawn of: - [PhysicsWorld.DrawFillOptions.Interior](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions-Interior.html) - [PhysicsWorld.DrawFillOptions.Outline](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions-Outline.html) - [PhysicsWorld.DrawFillOptions.Orientation](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions-Orientation.html) |
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
| `AllContactImpulse` | ⚠️ **[Obsolete]** (discouraged, but still compiles): Enum member DrawOptions.AllContactImpulse is deprecated. Use DrawOptions.AllContactForces (UnityUpgradable) -> AllContactForces — Draw all the contact forces in the world. |
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
| `capsuleGeometryArray` | Retrieve the Capsule Geometry Element. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `capsuleGeometrySpan` | Retrieve the Capsule Geometry Elements. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `circleGeometryArray` | Retrieve the Circle Geometry Elements. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `circleGeometrySpan` | Retrieve the Circle Geometry Elements. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `isValid` | Get if the draw results are valid i.e. they contain any data at all. |
| `lineArray` | Retrieve the Line Elements. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `lineSpan` | Retrieve the Line Elements. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `pointArray` | Retrieve the Point Elements. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `pointSpan` | Retrieve the Point Elements. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `polygonGeometryArray` | Retrieve the Polygon Geometry Elements. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |
| `polygonGeometrySpan` | Retrieve the Polygon Geometry Elements. Any new [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) drawing will invalidate this data so referring to this data afterwards may cause an unavoidable crash! You must immediately extract what information you need and not directly reference the returned data as it will be cleared immediately after being provided. |

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
| `drawFillOptions` | How the geometry element is filled with the color. See [PhysicsWorld.DrawFillOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions.html). |
| `elementDepth` | The depth of the element. |
| `length` | The length of the capsule element. |
| `radius` | The radius of the capsule element. |
| `transform` | The transform of the capsule element. |

##### Methods

###### `Size()`

The data size of the capsule element. This can be useful in understanding the memory stride in a [UnityEngine.ComputeBuffer](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.ComputeBuffer.html) or other structure.

**Returns:** The size in bytes.

#### CircleGeometryElement

> A Circle Geometry Element.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawResults.CircleGeometryElement`  

##### Fields

| Name | Summary |
|------|---------|
| `color` | The color of the circle element. |
| `drawFillOptions` | How the geometry element is filled with the color. See [PhysicsWorld.DrawFillOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions.html). |
| `elementDepth` | The depth of the element. |
| `radius` | The radius of the circle element. |
| `transform` | The transform of the circle element. |

##### Methods

###### `Size()`

The data size of the circle element. This can be useful in understanding the memory stride in a [UnityEngine.ComputeBuffer](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.ComputeBuffer.html) or other structure.

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

The data size of the line element. This can be useful in understanding the memory stride in a [UnityEngine.ComputeBuffer](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.ComputeBuffer.html) or other structure.

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

The data size of the point element. This can be useful in understanding the memory stride in a [UnityEngine.ComputeBuffer](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.ComputeBuffer.html) or other structure.

**Returns:** The size in bytes.

#### PolygonGeometryElement

> A Polygon Geometry Element.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawResults.PolygonGeometryElement`  

##### Fields

| Name | Summary |
|------|---------|
| `color` | The color of the polygon element. |
| `count` | The number of points in the polygon element. |
| `drawFillOptions` | How the geometry element is filled with the color. See [PhysicsWorld.DrawFillOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions.html). |
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

The data size of the polygon element. This can be useful in understanding the memory stride in a [UnityEngine.ComputeBuffer](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.ComputeBuffer.html) or other structure.

**Returns:** The size in bytes.

### DrawTarget

> Controls which Unity editor views a [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) is drawn into. This only filters the built-in viewport rendering and does not control whether drawing occurs at all, which is governed by [PhysicsWorld.DrawOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawOptions.html) and [PhysicsCoreSettings2D.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-renderingMode.html).

**Full name:** `Unity.U2D.Physics.PhysicsWorld.DrawTarget`  

#### Fields

| Name | Summary |
|------|---------|
| `Both` | The world is drawn in both the Scene view and the Game view. |
| `GameView` | The world is drawn only in the Game view. |
| `SceneView` | The world is drawn only in the Scene view. A player build has no Scene view, so selecting this means the world is never drawn in a build. |

### ExplosionDefinition

> Used to define the parameters when using [PhysicsWorld.Explode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Explode.html).

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
| `IgnoreCapsuleShapes` | Ignore [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) of type [PhysicsShape.ShapeType.Capsule](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-Capsule.html). See [PhysicsShape.shapeType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-shapeType.html). |
| `IgnoreChainSegmentShapes` | Ignore [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) of type [PhysicsShape.ShapeType.ChainSegment](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-ChainSegment.html). See [PhysicsShape.shapeType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-shapeType.html) and [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html). |
| `IgnoreCircleShapes` | Ignore [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) of type [PhysicsShape.ShapeType.Circle](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-Circle.html). See [PhysicsShape.shapeType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-shapeType.html). |
| `IgnoreDynamicBodies` | Ignore [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) of type [PhysicsBody.BodyType.Dynamic](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.BodyType-Dynamic.html). |
| `IgnoreKinematicBodies` | Ignore [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) of type [PhysicsBody.BodyType.Kinematic](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.BodyType-Kinematic.html). |
| `IgnoreNonTriggerShapes` | Ignore [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) that are not configured as a trigger. See [PhysicsShape.isTrigger](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isTrigger.html). |
| `IgnorePolygonShapes` | Ignore [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) of type [PhysicsShape.ShapeType.Polygon](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-Polygon.html). See [PhysicsShape.shapeType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-shapeType.html). |
| `IgnoreSegmentShapes` | Ignore [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) of type [PhysicsShape.ShapeType.Segment](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-Segment.html). See [PhysicsShape.shapeType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-shapeType.html). |
| `IgnoreStaticBodies` | Ignore [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) of type [PhysicsBody.BodyType.Static](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.BodyType-Static.html). |
| `IgnoreTriggerShapes` | Ignore [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) that are configured as a trigger. See [PhysicsShape.isTrigger](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isTrigger.html). |
| `None` | No draw filtering occurs. |

### PhysicsGroup

> An abstract group identity that can be shared by a set of physics objects. Each group is globally unique across all worlds.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.PhysicsGroup`  

#### Properties

| Name | Summary |
|------|---------|
| `groupIndex` | The group index. The value is unique per group; a value of zero means no group. |

#### Methods

##### `ToString()`

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
| `Script` | The simulation will only run when manually called with [PhysicsWorld.Simulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Simulate.html). |
| `Update` | The simulation will automatically run during the Update. |

### Snapshot

> An opaque, point-in-time capture of a [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) simulation state, produced by [PhysicsWorld.CreateSnapshot](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CreateSnapshot.html). Restore it into the same world with [PhysicsWorld.ApplySnapshot](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-ApplySnapshot.html), or create a new world from it with [PhysicsWorld.Create](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Create.html).

**Full name:** `Unity.U2D.Physics.PhysicsWorld.Snapshot`  

#### Properties

| Name | Summary |
|------|---------|
| `IsCreated` | Whether this snapshot holds a valid captured image. |
| `Length` | The size of the snapshot image in bytes. |

#### Methods

##### `Dispose()`

Release the native memory held by this snapshot.

### TransformChangeMode

> Defines when changes to [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) that are registered with [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html) are called. NOTE: In the Unity Editor when not in Play Mode, Transform change callbacks are always and only sent at the start of the frame for authoring purposes.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformChangeMode`  

#### Fields

| Name | Summary |
|------|---------|
| `FixedUpdate` | Transform Change callbacks are sent after the "FixedUpdate" script callbacks but before any "FixedUpdate" simulation(s). This is typically used when any changes to Transforms occur in the "FixedUpdate" script callbacks need to be handled before any "FixedUpdate" simulation(s). |
| `FrameStart` | Transform Change callbacks are sent at the start of a frame prior to the "FixedUpdate" or "Update" script callbacks. This is typically used when any changes to Transform from the previous frame need to be handled before anything else runs. |
| `Off` | Transform Change callbacks are not sent in play mode. |
| `Update` | Transform Change callbacks are sent after the "Update" script callbacks but before any "Update" simulation(s). This is typically used when any changes to Transforms during the "Update" script callbacks need to be handled before any "Update" simulation(s). |

### TransformChangeReason

> Defines the reason why a [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) changed. Register and unregister for transform changes with [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html) and [PhysicsWorld.UnregisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-UnregisterTransformChange.html).

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformChangeReason`  

#### Fields

| Name | Summary |
|------|---------|
| `Animation` | The animation system wrote a physics-based world-space TRS change. |
| `Any` | Any transform change. |
| `AnyLocal` | The local position, rotation or scale of the transform changed. |
| `AnyWorld` | The world-space position, rotation or scale of the transform changed. |
| `LocalPosition` | The local position of the transform changed. This does not propagate to children or parent transforms. See [UnityEngine.Transform.localPosition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform-localPosition.html). |
| `LocalRotation` | The local rotation of the transform changed. This does not propagate to children or parent transforms. See [UnityEngine.Transform.localRotation](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform-localRotation.html). |
| `LocalScale` | The local scale of the transform changed. This does not propagate to children or parent transforms. See [UnityEngine.Transform.localScale](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform-localScale.html). |
| `ParentHierarchy` | The parent transform hierarchy changed. Indicates that a direct or indirect parent has been added, removed or re-parented. |
| `WorldPosition` | The World-space position of the transform changed. Changing a parent results in an event in children transform too. See [UnityEngine.Transform.position](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform-position.html). |
| `WorldRotation` | The World-space rotation of the transform changed. Changing a parent results in an event in children transform too. See [UnityEngine.Transform.rotation](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform-rotation.html). |
| `WorldScale` | The World-space scale of the transform changed. Changing a parent results in an event in children transform too. See [UnityEngine.Transform.lossyScale](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform-lossyScale.html). |

### TransformPlane

> Defines the 2D Transform plane where Transform writes will occur. This also defines the rotation axis which will automatically be perpendicular to the selected plane. See [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlane.html).

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformPlane`  

#### Fields

| Name | Summary |
|------|---------|
| `Custom` | Use the assigned [PhysicsWorld.transformPlaneCustom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlaneCustom.html) to allow transformation writing and reading to/from a custom 2D plane. |
| `XY` | XY plane with anti-clockwise Z rotation. |
| `XZ` | XZ plane with anti-clockwise Y rotation. |
| `ZY` | ZY plane with anti-clockwise X rotation. |

### TransformPlaneCustom

> A transformation applied to the transform write if [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlane.html) is set to [PhysicsWorld.TransformPlane.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-Custom.html).

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformPlaneCustom`  

#### Properties

| Name | Summary |
|------|---------|
| `fromCustom` | Get the custom matrix defining how to transform from the custom world-space to the [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) space. NOTE: This is the inverse of the [PhysicsWorld.TransformPlaneCustom.toCustom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlaneCustom-toCustom.html) matrix. |
| `rotate` | Get the custom rotation. |
| `scale` | Get the uniform scale. |
| `toCustom` | Get the custom matrix defining how to transform from [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) space to the custom world-space. NOTE: This is the inverse of the [PhysicsWorld.TransformPlaneCustom.fromCustom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlaneCustom-fromCustom.html) matrix. |
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

Transform from a 3D custom world-space position back to a 2D [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) position.

**Params:**
- `position` — The 3D position to transform.

**Returns:** The transformed 2D position.

##### `ToPosition(Vector2)`

Transform a 2D [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) position to a 3D custom world-space position.

**Params:**
- `position` — The 2D position to transform.

**Returns:** The transformed 3D position.

### TransformTweenMode

> Defines if and how Transform tweens are calculated and/or written.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformTweenMode`  

#### Fields

| Name | Summary |
|------|---------|
| `Custom` | Transform tweens are not calculated or written. Instead, the callback target set with [PhysicsWorld.transformWriteCallbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteCallbackTarget.html) which must implement [PhysicsCallbacks.ITransformWriteCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback.html) will be have [PhysicsCallbacks.ITransformWriteCallback.OnTransformTweenWrite](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback-OnTransformTweenWrite.html) called allowing custom transform tween writing. |
| `Off` | Transform tweens are not calculated or written. |
| `Parallel` | Transform tweens are calculated and written in parallel using a [UnityEngine.Jobs.TransformAccess](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Jobs.TransformAccess.html). |
| `Sequential` | Transform tweens are calculated and written linearly on a single thread, likely the main-thread. This may be faster than using [PhysicsWorld.TransformTweenMode.Parallel](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformTweenMode-Parallel.html) if the majority of the [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) are not split across hierarchies so that they can be written in parallel. To further clarify, if most of the [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) are not interleaved across different hierarchies, this non-parallel (sequential) mode may be faster than [PhysicsWorld.TransformTweenMode.Parallel](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformTweenMode-Parallel.html), because it avoids the overhead of splitting and synchronizing work across multiple threads when there is not enough independent hierarchy work to parallelize efficiently. |

### TransformWriteMode

> Defines how the 2D Transforms from each [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) are written to the 3D Transform system.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.TransformWriteMode`  

#### Fields

| Name | Summary |
|------|---------|
| `Custom` | Transforms are not written. Instead, the callback target set with [PhysicsWorld.transformWriteCallbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteCallbackTarget.html) which must implement [PhysicsCallbacks.ITransformWriteCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback.html) will have [PhysicsCallbacks.ITransformWriteCallback.OnTransformWrite](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback-OnTransformWrite.html) called allowing custom transform writing. |
| `Fast2D` | Transforms are written but the rotation is converted to a [UnityEngine.Quaternion](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Quaternion.html) where only a single axis is written, all others will be set to zero rotation. This is the fastest method of writing transforms however, any 3D rotations or rotations on the unused axis will be reset to zero. The rotational axis written to depends on the current [PhysicsWorld.TransformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane.html) selected with [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlane.html) where it will always be perpendicular to the transform plane. |
| `Off` | Transforms are never written. This is the fastest operation. |
| `Slow3D` | Transforms are written but the rotation is converted to a [UnityEngine.Quaternion](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Quaternion.html) where the rotation of the body transform is merged into the existing 3D rotation. This is the slowest method of writing transforms however, all 3D rotations are preserved. The rotational axis written to depends on the current [PhysicsWorld.TransformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane.html) selected with [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlane.html) where it will always be perpendicular to the transform plane. |

### WorldCapacity

> Describes the expected world capacities used to presize internal allocations when a [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) is created. All counts default to zero, in which case the engine uses its own minimum defaults. See [PhysicsWorldDefinition.capacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition-capacity.html) and [PhysicsWorld.capacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-capacity.html).

**Full name:** `Unity.U2D.Physics.PhysicsWorld.WorldCapacity`  

#### Properties

| Name | Summary |
|------|---------|
| `contactCount` | The expected number of contacts. |
| `dynamicBodyCount` | The expected number of dynamic and kinematic bodies. |
| `dynamicShapeCount` | The expected number of dynamic and kinematic shapes. |
| `staticBodyCount` | The expected number of static bodies. |
| `staticShapeCount` | The expected number of static shapes. |

### WorldCounters

> PhysicsWorld counters that give details of the world simulation size.

**Full name:** `Unity.U2D.Physics.PhysicsWorld.WorldCounters`  

#### Properties

| Name | Summary |
|------|---------|
| `awakeContactCount` | The number of contacts active during the previous simulation step. |
| `bodyCount` | The number of all body types. |
| `broadphaseHeight` | The broadphase tree height for both Dynamic and Kinematic bodies. |
| `contactCount` | The number of contacts. |
| `islandCount` | The number of islands. |
| `jointCount` | The number of joints. |
| `recycledContactCount` | The number of contacts recycled in the previous simulation step. |
| `shapeCount` | The number of shapes. |
| `stackUsed` | The number of bytes assigned to the Stack allocator. |
| `staticBroadphaseHeight` | The broadphase tree height for Static bodies. |
| `taskCount` | The number of multi-threaded tasks requested solving the simulation. |
| `usedMemory` | The total byte allocation used by the physics system. |

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
| `constraints` | Time spent solving constraints. |
| `contactPairs` | Time spent updating collision pairs and creating contacts. |
| `contactUpdates` | Time spent updating contacts. |
| `fastTriggers` | Time spent calculate fast triggers for bodies. |
| `hitEvents` | Time spent generating contact hit events. |
| `integrateTransforms` | Time spent integrating transforms. |
| `integrateVelocities` | Time spent integrating velocities. |
| `jointEvents` | Time spent generating joint threshold events. |
| `prepareConstraints` | Time spent preparing joint and contact constraints. |
| `relaxImpulses` | Time spent relaxing constraint impulses. |
| `simulationStep` | Time spent stepping the simulation forward. |
| `sleepIslands` | Time spent updating islands that need to sleep. |
| `solveContinuous` | Time spent solving continuous collision detection. |
| `solveImpulses` | Time spent solving impulses. |
| `solverSetup` | Time spent setting up the solver. |
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

> A [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) definition used to specify important initial properties.

**Full name:** `Unity.U2D.Physics.PhysicsWorldDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsWorldDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `autoBodyUpdateCallbacks` | Controls if body update callback targets are automatically called. See [PhysicsWorld.SendBodyUpdateCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendBodyUpdateCallbacks.html). |
| `autoJointThresholdCallbacks` | Controls if joint threshold callback targets are automatically called. See [PhysicsWorld.SendJointThresholdCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendJointThresholdCallbacks.html). |
| `bounceThreshold` | Adjust the bounce threshold, usually in meters per second. It is recommended not to make this value very small because it will prevent bodies from sleeping. See [PhysicsWorld.bounceThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bounceThreshold.html). |
| `capacity` | The expected world capacities used to presize internal allocations when the [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) is created. All counts default to zero, in which case the engine uses its own minimum defaults. Presizing avoids reallocations during the first simulation steps for worlds with a known object count. See [PhysicsWorld.capacity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-capacity.html). |
| `contactDamping` | The contact bounciness with 1 being critical damping (non-dimensional). See [PhysicsWorld.contactDamping](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactDamping.html). |
| `contactFilterCallbacks` | Controls if contact filter callbacks will be called. A contact filter callback allows direct control over whether a contact will be created between a pair of shapes. This applies to both triggers and non-triggers but only with Dynamic bodies. These are relatively expensive so disabling them can provide a significant performance benefit. A contact filter callback will call the [PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html) for both shapes involved if they implement [PhysicsCallbacks.IContactFilterCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactFilterCallback.html). |
| `contactFrequency` | The contact stiffness, in cycles per second. See [PhysicsWorld.contactFrequency](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactFrequency.html). |
| `contactHitEventThreshold` | The contact hit event threshold controls the collision speed needed to generate a contact hit event, usually in meters per second. See [PhysicsEvents.ContactHitEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactHitEvent.html). See [PhysicsWorld.contactHitEventThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactHitEventThreshold.html). |
| `contactRecycleDistance` | The contact recycle distance, in meters. Setting this to zero disables contact point recycling. Contact recycling reuses contact points across simulation time-steps when the relative movement is small. This feature improves stability and performance by around 25% (approximately). Contact points are not recalculated until shapes move more than 5cm (default) relative to each other. Contact recycling skips some updates such as friction, pre-solve (etc) until the contacts are no longer recycled. See [PhysicsWorld.contactRecycleDistance](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactRecycleDistance.html). |
| `contactSpeed` | The contact speed used to solve overlaps, in meters per second. See [PhysicsWorld.contactSpeed](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactSpeed.html). |
| `continuousAllowed` | Controls if continuous collision detection will be used between Dynamic and Static bodies. Generally you should keep continuous collision enabled to prevent fast moving objects from going through Static objects. The performance gain from disabling continuous collision is minor. See [PhysicsWorld.continuousAllowed](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-continuousAllowed.html) |
| `defaultDefinition` | Get a default [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) definition. |
| `drawColors` | Controls what colors are used to draw [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html), [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) etc. See [PhysicsWorld.DrawColors](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawColors.html). |
| `drawContactType` | Controls how contact points are drawn. See [PhysicsWorld.DrawContactType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawContactType.html). |
| `drawFillAlpha` | Controls the draw fill alpha. This is used to scale the interior fill alpha and is only used when [PhysicsWorld.DrawFillOptions.Outline](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions-Outline.html) is used so that the interior color can be distinguished from the outline color by transparency. See [PhysicsWorld.drawFillAlpha](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-drawFillAlpha.html). |
| `drawFillOptions` | Controls how shape geometry is filled when drawing. See [PhysicsWorld.DrawFillOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawFillOptions.html). |
| `drawFilter` | Limits what gets drawn to a narrow selection. This only affects [PhysicsWorld.DrawOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawOptions.html) that are drawing all bodies, shapes etc. It does not affect selected elements or custom drawing. See [PhysicsWorld.IgnoreFilter](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.IgnoreFilter.html). |
| `drawForceScale` | Controls the joint contact force scale used when drawing contact forces. See [PhysicsWorld.drawForceScale](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-drawForceScale.html). |
| `drawNormalScale` | Controls the joint contact normal scale used when drawing contact normals. See [PhysicsWorld.drawNormalScale](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-drawNormalScale.html). |
| `drawOptions` | Limits what gets drawn to a broad selection. See [PhysicsWorld.DrawOptions](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.DrawOptions.html). |
| `drawOrder` | Controls the relative order this world is drawn in across all worlds. Worlds with a lower draw order are drawn first, so worlds with a higher draw order are drawn on top. See [PhysicsWorld.drawOrder](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-drawOrder.html). |
| `drawPointScale` | Controls the draw point scale used when drawing points. See [PhysicsWorld.drawPointScale](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-drawPointScale.html). |
| `drawTarget` | Controls which Unity editor views the world is drawn into (Scene view, Game view or both). This only filters the built-in viewport rendering; it does not control whether drawing occurs at all. See [PhysicsWorld.drawTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-drawTarget.html). |
| `drawThickness` | Controls the draw thickness (outline and orientation). See [PhysicsWorld.drawThickness](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-drawThickness.html). |
| `eventGroupingAllowed` | Controls if contact and trigger begin/end events for shapes assigned a group are marked as being the first or last event between the two groups involved. This allows the many events produced between two groups of shapes to be reduced to a single begin and end, such as when treating multiple shapes as a single object. The marking is only calculated for shapes assigned a group and only when a begin or end event is produced, so the cost of leaving this enabled is minor. See [PhysicsWorld.eventGroupingAllowed](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-eventGroupingAllowed.html) |
| `gravity` | Get/Set the gravity vector applied to all bodies in the world, usually in m/s^2. See [PhysicsWorld.gravity](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-gravity.html). |
| `maximumLinearSpeed` | Get/Set the maximum linear speed. See [PhysicsWorld.maximumLinearSpeed](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-maximumLinearSpeed.html). |
| `preSolveCallbacks` | Controls if pre-solve callbacks will be called. This only applies to Dynamic bodies and is ignored for triggers. These are relatively expensive so disabling them can provide a significant performance benefit. A pre-solve callback will call the [PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html) for both shapes involved if they implement [PhysicsCallbacks.IPreSolveCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IPreSolveCallback.html). |
| `simulationSubSteps` | Get/Set the simulation sub-steps to use during simulation. See [PhysicsWorld.Simulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Simulate.html). See [PhysicsWorld.simulationSubSteps](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-simulationSubSteps.html). |
| `simulationType` | Get/Set the simulation mode which controls when or if the simulation will be automatically simulated. See [PhysicsWorld.SimulationType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.SimulationType.html) and [PhysicsWorld.Simulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Simulate.html). |
| `simulationWorkers` | Get/Set the simulation worker count for the world. A single simulation worker is always used for simulation therefore a worker count of one means single thread simulation only. The actual quantity of workers used will always be capped to those available on the current device and reading the property will return the number of workers actually being used by the device. Changing the worker count continuously is not recommend and will impact performance as it requires the task queue be recreated. See [PhysicsWorld.simulationWorkers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-simulationWorkers.html). |
| `sleepingAllowed` | Controls if bodies go to sleep when not moving and not interacting. Sleeping can provide a significant performance improvement when many Dynamic or Kinematic bodies are in the world. See [PhysicsWorld.sleepingAllowed](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-sleepingAllowed.html) |
| `syncInterpolation` | Controls if an extra write pass prior to the script fixed-update callback is made for any interpolation tweens to ensure that transforms are synchronized to the final body pose. Because this is an extra write pass, it has an impact on overall performance so only enable if you require transforms synchronized this way. NOTE: This only affects [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) that have their [PhysicsBody.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-transformWriteMode.html) set to [PhysicsBody.TransformWriteMode.Interpolate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteMode-Interpolate.html). |
| `transformPlane` | Controls the transform plane that the world uses when writing transforms. See [PhysicsWorld.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteMode.html). See [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformPlane.html). |
| `transformPlaneCustom` | Controls the transformation for the [PhysicsWorld.TransformPlane.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-Custom.html) to allow transformation writing and reading to/from a custom space. See [PhysicsWorld.TransformPlaneCustom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlaneCustom.html). |
| `transformTweenMode` | Controls if and how Transform tweens are calculated and/or written. Transform tweening is where bodies that have their [PhysicsBody.transformObject](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-transformObject.html) set, write to the [UnityEngine.Transform](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Transform.html) each frame depending on the specific body [PhysicsBody.TransformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteMode.html) set. Regardless of this setting, Transform tweening is never used if the [PhysicsWorld.simulationType](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-simulationType.html) is [PhysicsWorld.SimulationType.Update](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.SimulationType-Update.html) or [PhysicsWorld.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteMode.html) is [PhysicsWorld.TransformWriteMode.Off](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformWriteMode-Off.html). |
| `transformWriteMode` | Controls how transform writing is handled. Only bodies that have their [PhysicsBody.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-transformWriteMode.html) active and produce a [PhysicsEvents.BodyUpdateEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html) will write to a transform. See [PhysicsWorld.TransformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformWriteMode.html). |

### Methods

#### `new()`

Create a default [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) definition.

#### `new(bool)`

Create a default [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) definition.

**Params:**
- `useSettings` — Controls whether the default settings come from the physics settings or not.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.7.0a3 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
