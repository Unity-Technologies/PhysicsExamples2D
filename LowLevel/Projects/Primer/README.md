# Physics 2D LowLevel Primer

## Overview

### Namespace

The low-level physics API discussed below exists entirely in the `UnityEngine.LowLevelPhysics2D` namespace
A good jumping off point is the `PhysicsWorld` type found [here](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.html).

---
### Objects
The low-level physics in Unity (referred to as the "API") directly exposes a high-performance physics engine [Box2D v3](https://github.com/erincatto/box2d) in a way that is not tied to using GameObjects or Components.
For this reason, the objects do not exist in the Unity Editor Inspector, however they can be used in scripts and as components so they can be exposed and therefore configured in the inspector.
All the API types are `structs` and many are serializable allowing them to be persisted and edited in components in the Editor inspector.

The API is designed to present a friendly, object-oriented way to create, configure and destroy objects however behind the scenes things are quite different!
All objects that are created are actually `read-only structs` that contain only a 64-bit opaque handle to the actual object that is stored inside the engine in an efficient, memory-access friendly way.
You should never explicitly care about this handle as it's implicitly used for you by the API however, this handle-based approach has the benefit that objects can be passed around efficiently, even off the main-thread in C# Jobs.
Because everything is a `struct`, they can also be store in native-containers which only support struct storage.

Finally, if an object is destroyed, its handle simply becomes invalid and any subsequent access using that handle will result in a nice clear message in the console indicating the issue.
The same is applied to all arguments for all methods on such an object; if any are out-of-range or invalid, a clear message will be displayed in the console.

---
### Multi-threading Support
Most of the API is thread-safe therefore you can freely perform read and write operations off the main-thread.
This is achieved by the use of WORM (Write-Once / Read-Many) lock system. This means you can perform unlimited read operations in parallel but only a single thread can perform a write operation.
The locking mechanism tries not to "starve" writers as when a write-operation is required, it blocks further reads, processes all current readers, performs the write-operation then processes readers again.
This locking has an extremely low overhead and provides the huge benefit of being able to use most of the API in C# Jobs.
Nevertheless, care must be taken when performing mixed read/write operations across many threads as performance can suffer if you're not careful.

A final note is that locking is per-world so for instance, you can perform write operations on different worlds in parallel without any read/write contentions.

---
### GC
All the API is GC friendly meaning no C# heap allocations are made.
This is primarily due to everything being allocated in native engine with objects being returned to C# as a `readonly struct` containing only  64-bit handle.

Another reason is that any method or property that returns a set of values or accepts a set of values uses `NativeArray<T>` and `ReadOnlySpan<T>` respectively.

For instance, performing a query will return a `NativeArray<T>` of results which you must dispose i.e. `using var results = world.CastRay(...);`.
The operations that return multiple results such as this (irrelevant of what is being returned) all offer the option to allocate using the [Temp](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Unity.Collections.Allocator.Temp.html), [TempJob](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Unity.Collections.Allocator.TempJob.html) or [Persistent](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Unity.Collections.Allocator.Persistent.html) allocator with the default being the [Temp](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Unity.Collections.Allocator.Temp.html) allocator.

When asking for events, those are not copied but are instead accessed <b>directly</b> from inside the engine itself so you can use `ReadOnlySpan<ContactBeginEvent> events = world.contactBeginEvents;` or `var events = world.contactBeginEvents;` as you prefer.
These do not require deallocation as they are direct memory access.

---
## Definitions
When you create an object, it is far more efficient to create it with all its properties already set.
Creating an object and then changing multiple properties is slower than having it setup correctly initially, more so if the properties have side effects causing recalculations.
To this end, whenever you create an object you can specify a definition with all object types having their own dedicated definition type. For instance, a [PhysicsWorld](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.htm) has a [PhysicsWorldDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorldDefinition.html)
and a [PhysicsBody](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody.html), a [PhysicsBodyDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBodyDefinition.html) etc.

All definitions also have a default which can always be accessed via a static `.defaultDefinition` property on the definition type itself i.e.
[PhysicsWorldDefinition.defaultDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorldDefinition-defaultDefinition.html),
[PhysicsBodyDefinition.defaultDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBodyDefinition-defaultDefinition.html) etc.

Also, methods used to create objects that accept the appropriate definition, also accept no arguments which will mean the object will implicitly use the appropriate default definition for convenience.

Even more powerful is that even these defaults are <b>not</b> hardwired but can themselves be configured via a dedicated asset of [PhysicsLowLevelSettings2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsLowLevelSettings2D.html) (the only `class` type in the API!)

This asset can be created via the Assets menu under `Create > 2D > Physics LowLevel Settings`:

![Physics LowLevel Settings Asset](./Images/LowLevelPhysicsSettings2D.png)

You can then drag that asset into the "Low Level" tab (new!) in `Project Settings > Physics 2D`:

![Project Settings / Physics 2D / Low Level](./Images/ProjectSettingsPhysics2D.png)

You can then select the asset and edit it directly in the Editor:

![Physics LowLevel Settings Inspector](./Images/LowLevelPhysicsSettings2D-Inspector.png)

This provides all the available defaults for all definitions and other important global settings such as:
- The number of concurrent world simulations that are allowed
- The length units-per-meter (used when larger scales are required i.e. pixels as meters etc)
- If the debug renderer is available in player builds
- If the whole low-level physics system should be bypassed (no simulation and rendering allowed)
- If the full 64-bit layer system should be used in which case, all the property drawers will display names from the specified `PhysicsLayers`
- The `PhysicsLayer` names
- etc.

You can be seen in both the `Sandbox` and `Primer` projects.

---
## Important New Features

- Ability to work with 2D planes other than the standard XY plane such as the more common to 3D, [PhysicsWorld.TransformPlane.XZ](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.TransformPlane.XZ.html) plane.
  Each `PhysicsWorld` has support for working in a selected [TransformPlane](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.TransformPlane.html) with [PhysicsWorld.transformPlane](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-transformPlane.html).
  Whilst the physics system will render in the selected plane, it is still a 2D physics engine which only works with `Vector2` therefore it is 
  your responsibility to convert the `Vector2` to/from `Vector3` however there are many `PhysicsMath` utilities to help make this easy such as:
  - [PhysicsMath.ToPhysicsTransform](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.ToPhysicsTransform.html)
  - [PhysicsMath.ToPosition3D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.ToPosition3D.html)
  - [PhysicsMath.ToPosition2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.ToPosition2D.html)
  - [PhysicsMath.ToRotation2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.ToRotation2D.html)
  - [PhysicsMath.ToRotationFast3D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.ToRotationFast3D.html)
  - [PhysicsMath.ToRotationSlow2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.ToRotationSlow3D.html)
  - [PhysicsMath.GetRotationAxes](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.GetRotationAxes.html)
  - [PhysicsMath.GetTranslationAxes](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.GetTranslationAxes.html)
  - Others!
- Ability to control how many additional worker threads are used (from 0 to 63) to solve the simulation of each world via [PhysicsWorld.simulationWorkers](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-simulationWorkers.html).
  This value is always clamped to the available threads on the device at runtime.
- Ability to simulate multiple worlds in parallel controlled via [PhysicsWorld.concurrentSimulations](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-concurrentSimulations.html)
- Ability to use more than the standard 32 layers when controlling contacts and performing queries, now increased to 64 layers via [PhysicsMask](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMask.html), [PhysicsLayers](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsLayers.html) and [PhysicsWorld.useFullLayers](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-useFullLayers.html).
- Ability to pause a world simulation via [PhysicsWorld.paused](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-paused.html)
- Dedicated types for physics operations:
  - [PhysicsTransform](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsTransform.html) - Handles three degrees of freedom (position and rotation)
  - [PhysicsRotate](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsRotate.html) - Handles single axis rotation (a subcomponent of `PhysicsTransform`)
  - [PhysicsLayers](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsLayers.html) - Handles 64 named layers using `PhysicsMask`. This is a 64-bit equivalent of `LayerMask`. 
  - [PhysicsMask](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMask.html) - Handles 64-bit masking for layers, contact control, queries etc. (a new UIElement type of [Mask64Field](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.Mask64Field.html) was created for this).
  - [PhysicsPlane](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsPlane.html) - Handles a 2D plane (limited use for now but future use with `PhysicsWorld.CastMover`)
  - [PhysicsAABB](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsAABB.html) - Handles 2D bounds
- Event system (read as `ReadOnlySpan<T>`):
  - [PhysicsEvents.PreSimulate](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsEvents.PreSimulate.html) - Callback per-world called prior to any simulation step
  - [PhysicsEvents.PostSimulate](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsEvents.PostSimulate.html) - Callback per-world called after any simulation step
  - [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-bodyUpdateEvents.html) - Event produced when a `PhysicsBody` is updated by the simulation
  - [PhysicsWorld.contactBeginEvents](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-contactBeginEvents.html) - Event produced when a pair of `PhysicsShape` come into contact
  - [PhysicsWorld.contactEndEvents](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-contactEndEvents.html) - Event produced when a pair of `PhysicsShape` stop contacting
  - [PhysicsWorld.contactHitEvents](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-contactHitEvents.html) - Event produced when a pair of `PhysicsShape` come into contact beyond a specified speed threshold
  - [PhysicsWorld.triggerBeginEvents](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-triggerBeginEvents.html) - Event produced when a pair of `PhysicsShape` (if either is configured as a [trigger](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape-isTrigger.html)) start overlapping
  - [PhysicsWorld.triggerEndEvents](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-triggerEndEvents.html) - Event produced when a pair of `PhysicsShape` (if either is configured as a [trigger](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape-isTrigger.html)) stop overlapping
  - [PhysicsWorld.jointThresholdEvents](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-jointThresholdEvents.html) - Event produced when a `PhysicsJoint` exceeds its [force](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsJoint-forceThreshold.html) or [torque](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsJoint-torqueThreshold.html) threshold
- Callback system producing callbacks to specified [MonoBehaviour](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.html) relevant objects:
  - Callback targets set via [xxx.callbackTarget](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/30_search.html?q=callbacktarget) and called if object implements:
    - [PhysicsCallbacks.IContactFilterCallback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsCallbacks.IContactFilterCallback.html) - Allows controlling if a contact will be allowed or not prior to being sent to the solver
    - [PhysicsCallbacks.IPreSolveCallback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsCallbacks.IPreSolveCallback.html) - Allows controlling if a contact is enabled or disabled prior to solving
    - [PhysicsCallbacks.IBodyUpdateCallback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsCallbacks.IBodyUpdateCallback.html) - Provides notification that a `PhysicsBody` was updated in position/rotation or fell asleep during the simulation step
    - [PhysicsCallbacks.IContactCallback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsCallbacks.IContactCallback.html) - Provides notification of both contact begin and contact end ends for a pair of `PhysicsShape`
    - [PhysicsCallbacks.ITriggerCallback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsCallbacks.ITriggerCallback.html) - Provides notification of both trigger begin and trigger end ends for a pair of `PhysicsShape` (if either is configured as a [trigger](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape-isTrigger.html))
    - [PhysicsCallbacks.IJointThresholdCallback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsCallbacks.IJointThresholdCallback.html) - Provides notification when a `PhysicsJoint` exceeds its [force](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsJoint-forceThreshold.html) or [torque](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsJoint-torqueThreshold.html) thresholds
  - Event-related callbacks are automatically sent from a world only if enabled via:
    - [PhysicsWorld.autoBodyUpdateCallbacks](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-autoBodyUpdateCallbacks.html)
    - [PhysicsWorld.autoContactCallbacks](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-autoContactCallbacks.html)
    - [PhysicsWorld.autoTriggerCallbacks](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-autoTriggerCallbacks.html)
    - [PhysicsWorld.autoJointThresholdCallbacks](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-autoJointThresholdCallbacks.html)
  - Event-related callbacks can be manually sent from a world at any time via:
    - [PhysicsWorld.sendAllCallbacks()](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-sendAllCallbacks.html)
    - [PhysicsWorld.sendBodyUpdateCallbacks()](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-sendBodyUpdateCallbacks.html)
    - [PhysicsWorld.sendContactCallbacks()](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-sendContactCallbacks.html)
    - [PhysicsWorld.sendTriggerCallbacks()](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-sendTriggerCallbacks.html)
    - [PhysicsWorld.sendJointThresholdCallbacks()](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-sendJointThresholdCallbacks.html)
- Dedicated math utilities: [PhysicsMath](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsMath.html).
- Speculative contact system via [PhysicsWorld.speculativeContactDistance](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-speculativeContactDistance.html).
- Definition-based configuration for all objects including the `PhysicsWorld` meaning each world can be configured independently.
- Support for per-object custom data to aid in scripting logic for identification, debugging (etc.) via [PhysicsUserData](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsUserData.html).
  - [PhysicsWorld.userData](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-userData.html)
  - [PhysicsBody.userData](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-userData.html)
  - [PhysicsShape.userData](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape-userData.html)
  - [PhysicsChain.userData](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsChain-userData.html)
  - [PhysicsJoint.userData](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsJoint-userData.html)
- Dedicated Geometry types used for creating `PhysicsShape`, queries and debug drawing:
  - Types:
    - [CircleGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CircleGeometry.html)
    - [CapsuleGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.html)
    - [PolygonGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PolygonGeometry.html)
    - [SegmentGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.SegmentGeometry.html)
    - [ChainSegmentGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.ChainSegmentGeometry.html)
    - [ChainGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.ChainGeometry.html)
  - Consistent geometry queries and operations for all geometry types, examples below for `CapsuleGeometry` but applies to all geometries:
    - [CapsuleGeometry.CalculateAABB](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.CalculateAABB.html)
    - [CapsuleGeometry.CalculateMassConfiguration](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.CalculateMassConfiguration.html)
    - [CapsuleGeometry.CastRay](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.CastRay.html)
    - [CapsuleGeometry.CastShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.CastShape.html)
    - [CapsuleGeometry.ClosestPoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.ClosestPoint.html)
    - [CapsuleGeometry.OverlapPoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.OverlapPoint.html)
    - [CapsuleGeometry.Intersect](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.Intersect.html)
    - [CapsuleGeometry.Transform](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.Transform.html)
    - [CapsuleGeometry.InverseTransform](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.InverseTransform.html)
- Dedicated Geometry Utilities (works in C# Jobs too):
  - Ability to <b>compose geometry</b> in layers using geometry [operations](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsComposer.Operation.html) via [PhysicsComposer](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsComposer.html)
    - Can compose `PolygonGeometry` via [PhysicsComposer.CreatePolygonGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsComposer.CreatePolygonGeometry.html)
    - Can compose `ChainGeometry` via `[PhysicsComposer.CreateChainGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsComposer.CreateChainGeometry.html)
  - Ability to <b>destruct `PolygonGeometry`</b> in two ways:
    - <b>Fragment</b> geometry using specified fracture points with masking support (for carving) via [PhysicsDestructor.Fragment](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsDestructor.Fragment.html)
    - <b>Slice</b> geometry in two along a ray via [PhysicsDestructor.Slice](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsDestructor.Slice.html)
- Much more!

---
## Primary Physics Types
### PhysicsWorld

A [PhysicsWorld](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.htm) also known simply as a "world" is an isolated container for all other objects.
Multiple worlds can be created and simulated concurrently with the maximum number of worlds allowed being specified by [PhysicsConstants.MaxWorlds](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsConstants.MaxWorlds.html).
The `PhysicsWorld` is the only object that has a fixed limit, all other objects created in a world can be created without limit.

Any objects in one world cannot (in any way) be affected by objects in another world and thus are completely isolated in behaviour and in memory.
This is why multiple worlds can be simulated concurrently.
All objects created encode the world they exist in inside their handle so it is not possible to accidentally work with an object outside a world it exists. The context is permanent and safe.

All worlds can control when and how they simulate via [PhysicsWorld.simulationMode](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-simulationMode.html) or
[PhysicsWorldDefinition.simulationMode](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorldDefinition-simulationMode.html).
When the simulation mode is either [SimulationMode.FixedUpdate](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SimulationMode2D.FixedUpdate.html) or
[SimulationMode.Update](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SimulationMode2D.Update.html), the world will automatically be simulated at those times.

 Each world also has many other properties to control a wide range of behaviour options for the objects contained within it.

Whilst you can create a new `PhysicsWorld`, <b>it is far more common to use a single world, therefore a single world is automatically created for you</b> and is known as the "default world" and can always be accessed via [PhysicsWorld.defaultWorld](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-defaultWorld.html).

It is common to see code that uses `var world = PhysicsWorld.defaultWorld;` and then proceeds to use the `world` variable to access the world, create/destroy objects etc.

---
### PhysicsBody
A [PhysicsBody](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody.html) also known simply as a "body" exists within a specific world.
The role of a body is to maintain a position and rotation within a world and allow both shapes and joints (constraints) to be connected to them.

A body is created in a world with code such as `var body = world.CreateBody();` or `var body = PhysicsBody.Create(world);`
A body is the only object in a world that can move as it is the only object that knows about position and rotation and as such it contains many properties that control movement with
properties such as [PhysicsBody.linearVelocity](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-linearVelocity.html) or
[PhysicsBody.angularVelocity](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-angularVelocity.html) etc.

---
### PhysicsShape
A [PhysicsShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.html) represents a primitive shape type attached to a `PhysicsBody`.
The role of a shape is to specify both a region relative to the `PhysicsBody` it is attached to and dynamic behaviour for when that region touches or overlaps another `PhysicsShape` on another `PhysicsBody`.
There are a fixed set of shape types as indicated by [PhysicsShape.shapeType](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape-shapeType.html) shown [here](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.ShapeType.html).

When creating a `PhysicsShape`, along with a [PhysicsShapeDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShapeDefinition.html) you specify the shape geometry.
Each shape type has its own geometry type i.e. [PhysicsShape.ShapeType.Circle](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.ShapeType.Circle.html) uses [CircleGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CircleGeometry.html),
[PhysicsShape.ShapeType.Capsule](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.ShapeType.Capsule.html) uses [CapsuleGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.html) etc.

A shape is created via a body in the world with code such as `var shape = body.CreateShape(geometry);` or `var shape = PhysicsShape.Create(body, geometry);` etc.

---
### PhysicsJoint
A [PhysicsJoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsJoint.html) represents a dynamic constraint between two `PhysicsBody`.
The role of a joint is one of a constraint behaviour ranging from maintaining a distance, restricting motions along an axis and/or rotation to ignoring contacts between two bodies.
They are typically used to model real-world dynamic behaviours, especially for physics-based mechanisms.
There are a fixed set of joint types as indicated by [PhysicsJoint.jointType](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsJoint-jointType.html) shown [here](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsJoint.JointType.html).

A joint is created in a world with code such as `var joint = world.CreateJoint(definition);` or the concrete call (depending on the joint type) of `var joint = PhysicsDistanceJoint.Create(world, definition);`.

---
## Queries
Many spatial queries are provided to allow interrogating the world and its contents, specifically the `PhysicsShape` that exist attached to `PhysicsBody`.
All queries are provided as methods of the `PhysicsWorld` type and are used on any instance of a `PhysicsWorld`.

### Overlap Queries
An overlap takes geometry and queries if it overlaps any `PhysicsShape` in the world, returning what it overlapped.
Each overlap query has an alterative which performs the same action however it only returns true/false (bool) if the overlap found anything, these are all prefixed with "Test".

- [PhysicsWorld.OverlapAABB](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.OverlapAABB.html)
- [PhysicsWorld.OverlapPoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.OverlapPoint.html)
- [PhysicsWorld.OverlapGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.OverlapGeometry.html)
- [PhysicsWorld.OverlapShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.OverlapShape.html)
- [PhysicsWorld.OverlapShapeProxy](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.OverlapShapeProxy.html)
- [PhysicsWorld.TestOverlapAABB](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.TestOverlapAABB.html)
- [PhysicsWorld.TestOverlapPoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.TestOverlapPoint.html)
- [PhysicsWorld.TestOverlapGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.TestOverlapGeometry.html)
- [PhysicsWorld.TestOverlapShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.TestOverlapShape.html)
- [PhysicsWorld.TestOverlapShapeProxy](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.TestOverlapShapeProxy.html)

### Cast Queries
A cast takes geometry, sweeps it through the world and queries if it contacts any `PhysicsShape`, returning what it contacted.

- [PhysicsWorld.CastRay](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.CastRay.html)
- [PhysicsWorld.CastGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.CastGeometry.html)
- [PhysicsWorld.CastShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.CastShape.html)
- [PhysicsWorld.CastShapeProxy](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.CastShapeProxy.html)
- [PhysicsWorld.CastMover](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.CastMover.html)

---
## Debug Drawing
A `PhysicsWorld` has its own debug drawing capabilities allowing its contents to be automatically drawn and therefore visualised within the Editor Game and Scene windows.
The drawing system uses a high-performance set of compute-based shaders with multi-instancing support to render as efficiently as possible.

Drawing helps with authoring, especially for `PhysicsShape` but also provides custom drawing draw of primitives which is extremely helpful in debugging projects.
For this reason, debug drawing is fully available in a `Development` player build too.
This does mean that this is only supported on platforms with compute support.
Debug drawing in a build has to be explicitly enabled in [PhysicsLowLevelSettings2D.drawInBuild](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsLowLevelSettings2D-drawInBuild.html) (see below for more info).

There are many properties available in each world to control the world drawing including [PhysicsWorld.drawOptions](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-drawOptions.html),
[PhysicsWorld.drawFillOptions](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-drawFillOptions.html),
[PhysicsWorld.drawColors](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-drawColors.html),
[PhysicsWorld.drawFillAlpha](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-drawFillAlpha.html),
[PhysicsWorld.drawThickness](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld-drawThickness.html)
and many more.

In addition to drawing the automatic drawing contents of a world you can explicitly perform draw operations yourself, specifying colors, fill-options, lifetime (etc.) for the drawing:
- [PhysicsWorld.DrawBox](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.DrawBox.html)
- [PhysicsWorld.DrawCapsule](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.DrawCapsule.html)
- [PhysicsWorld.DrawCircle](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.DrawCircle.html)
- [PhysicsWorld.DrawGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.DrawGeometry.html)
- [PhysicsWorld.DrawLine](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.DrawLine.html)
- [PhysicsWorld.DrawLineStrip](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.DrawLineStrip.html)
- [PhysicsWorld.DrawPoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.DrawPoint.html)
- [PhysicsWorld.DrawTransformAxis](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.DrawTransformAxis.html)
- [PhysicsWorld.ClearDraw](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.ClearDraw.html)

---
## Authoring Components

The API has been designed not only to provide a fast physics system but also to provide developers with the ability to create and package new higher-level componentry composed of the "low level" types provided in this API.
For instance, it is entirely possible to create a single Unity script component that exposes a handful of public properties that would allow a technical artist or game designer to configure the component.
There is nothing new in this however this API makes that process much cleaner, only exposing the properties required with no other visible components appearing the scene. 

For example, let's say we want a "Gear" component. We want properties to configure the gear radius, how many teeth, the size of the teeth, gear motor properties, what it can contact with etc.

With this API, this can be created as a single component, generating the appropriate `PhysicsBody`, `PhysicsShape`, `PhysicsJoint` (etc.) which are not visible external to the component.
This component can be placed in a scene and it will just work. Many of these components can be created and provided (for example) on the Asset Store and again, they will just work out-of-the-box.

Here's an example showing the component on a GameObject. No children GameObject are required to "hide" other components to support it, it's completely self-contained:

![Gear Component Custom](./Images/GearComponent-Custom.png)

---
## Ownership

When authoring components, especially when providing them to external developers, you want to ensure that objects you create with the API i.e. `PhysicsWorld`, `PhysicsBody`, `PhysicsShape`, `PhysicsChain`, `PhysicsJoint` etc. are not deleted.
For example, if you provide the "Gear" component above, and it is happily working in a scene and a developer performs a world query and detects one of the `PhysicsShape` which is a "tooth" in your gear.
The user decides this is a shape they wish to destroy so they perform a [PhysicsShape.Destroy()](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.Destroy.html) call.
This is a perfectly fine thing to do from the devs point-of-view however suddenly, the "Gear" component is missing a tooth both visibly (via the debug renderer) and no longer collides at that tooth position!
The dev may have accidentally destroyed this but from their point-of-view, the "Gear" component has "randomly" stopped working correctly!

To aid in stopping this helping this situation where components are authored containing "low level" types from the API, a feature known as "ownership" or "owning" is available.
This is simple feature but offers some gate-keeping measures to help the previously described situation.

Whenever you create an object such as a `PhysicsWorld`, `PhysicsBody`, `PhysicsShape`, `PhysicsChain` or `PhysicsJoint`, you can choose to "own" it.
To do this, you simply set ownership. As an example with a `PhysicsBody`, you can do the following:
```csharp
class GearComponent : MonoBehaviour
{
    PhysicsBody m_PhysicsBody;
    int m_OwnerKey;
    
    void OnEnable()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
          
        // Create a body.
        m_PhysicsBody = world.CreateBody();
        
        // Assign my component script as the owner (optional) and return an "owner key" (integer).
        // Once ownership is set, it cannot be changed!
        m_OwnerKey = m_PhysicsBody.SetOwner(this);
    }
}
```

That's it, but what has this done?  Well, if anyone were to perform the following script:

```csharp
// Be gone shape!
body.Destroy();
```
... the body would <b>not</b> be destroyed!

Instead, the API would output link to the object with a console warning of:
`UnityEngine.LowLevelPhysics2D.PhysicsBody.Destroy was called but encountered a problem: Cannot destroy a body that is owned without a valid owner key being specified.`

This means that to destroy that `PhysicsBody`, you must specify the "owner key" that was returned when you originally asked for ownership.

This is done like this:
```csharp
class GearComponent : MonoBehaviour
{
    PhysicsBody m_PhysicsBody;
    int m_OwnerKey;
    
    void OnEnable()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
          
        // Create a body.
        m_PhysicsBody = world.CreateBody();
        
        // Assign my component script as the owner (optional) and return an "owner key" (integer).
        // Once ownership is set, it cannot be changed!
        m_OwnerKey = m_PhysicsBody.SetOwner(this);
    }
    
    void OnDisable()
    {
      // Destroy the body.
      m_PhysicsBody.Destroy(m_OwnerKey);        
    }
}
```

This would allow the `PhysicsBody` to be destroyed.
As a component author, you may wish to allow this so you simply don't set yourself as an owner.
Typically, such components want their lifetime controlled by the component lifetime.
The aim here is not to provide cryptographic security but instead provide a deterrent mechanism which makes it difficult to perform actions which would lead to undesirable results.
A prime example of this is where Unity creates a default World, however it is owned by Unity and cannot be destroyed for this reason.

---
### WIP
