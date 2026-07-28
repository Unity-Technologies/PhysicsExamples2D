---
name: unity-physicscore2d-events-api
description: Authoritative Unity 6000.7 PhysicsCore2D API reference for Events & Callbacks. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsCallbacks, PhysicsEvents. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Events & Callbacks

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.7, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.7.0a3 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsCallbacks`, `PhysicsEvents`.

## PhysicsCallbacks

> All callback interfaces and targets.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks`  
**Docs:** [Unity.U2D.Physics.PhysicsCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.html)

### Nested Types

- **BodyUpdateCallbackTargets** — Contains all the body update callback targets returned from [PhysicsWorld.GetBodyUpdateCallbackTargets](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-GetBodyUpdateCallbackTargets.html).
- **ContactCallbackTargets** — Contains all the contact callback targets returned from [PhysicsWorld.GetContactCallbackTargets](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-GetContactCallbackTargets.html).
- **IBodyUpdateCallback** — An interface that when implemented, can be called as a target by [PhysicsWorld.SendBodyUpdateCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendBodyUpdateCallbacks.html).
- **IContactCallback** — An interface that when implemented, can be called as a target by [PhysicsWorld.SendContactCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendContactCallbacks.html).
- **IContactFilterCallback** — An interface that when implemented, can be called as a target when a [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) has [PhysicsShape.contactFilterCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-contactFilterCallbacks.html) set to true. The [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) is in also has to have its [PhysicsWorld.contactFilterCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactFilterCallbacks.html) set to true.
- **IJointThresholdCallback** — An interface that when implemented, can be called as a target by [PhysicsWorld.SendJointThresholdCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendJointThresholdCallbacks.html).
- **IPreSolveCallback** — An interface that when implemented by a [System.Object](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/System.Object.html), can be called as a target when a [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) has [PhysicsShape.preSolveCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-preSolveCallbacks.html) set to true. The [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) is in also has to have its [PhysicsWorld.preSolveCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-preSolveCallbacks.html) set to true.
- **ITransformChangedCallback** — An interface that when implemented, can be called when using [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html).
- **ITransformWriteCallback** — An interface that when implemented, can be called as a target set with [PhysicsWorld.transformWriteCallbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteCallbackTarget.html).
- **ITriggerCallback** — An interface that when implemented, can be called as a target by [PhysicsWorld.SendTriggerCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendTriggerCallbacks.html).
- **JointThresholdCallbackTargets** — Contains all the joint callback targets returned from [PhysicsWorld.GetJointThresholdCallbackTargets](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-GetJointThresholdCallbackTargets.html).
- **TriggerCallbackTargets** — Contains all the trigger callback targets returned from [PhysicsWorld.GetTriggerCallbackTargets](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-GetTriggerCallbackTargets.html).

### BodyUpdateCallbackTargets

> Contains all the body update callback targets returned from [PhysicsWorld.GetBodyUpdateCallbackTargets](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-GetBodyUpdateCallbackTargets.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.BodyUpdateCallbackTargets`  

#### Properties

| Name | Summary |
|------|---------|
| `bodyUpdateCallbackTargets` | The body update targets. |

#### Methods

##### `Dispose()`

Dispose of any allocated memory. This must be called if any targets are returned otherwise memory leaks will occur.

#### Nested Types

- **BodyUpdateTarget** — Body update event target for callbacks.

#### BodyUpdateTarget

> Body update event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.BodyUpdateCallbackTargets.BodyUpdateTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `bodyTarget` | The callback target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.BodyUpdateEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html). This returns any implemented [PhysicsCallbacks.IBodyUpdateCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IBodyUpdateCallback.html) or NULL if not implemented or no target. |
| `bodyUpdateEvent` | The event. |

### ContactCallbackTargets

> Contains all the contact callback targets returned from [PhysicsWorld.GetContactCallbackTargets](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-GetContactCallbackTargets.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ContactCallbackTargets`  

#### Properties

| Name | Summary |
|------|---------|
| `BeginCallbackTargets` | The begin targets. |
| `EndCallbackTargets` | The end targets. |

#### Methods

##### `Dispose()`

Dispose of any allocated memory. This must be called if any targets are returned otherwise memory leaks will occur.

#### Nested Types

- **ContactBeginTarget** — Contact begin event target for callbacks.
- **ContactEndTarget** — Contact end event target for callbacks.

#### ContactBeginTarget

> Contact begin event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ContactCallbackTargets.ContactBeginTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `beginEvent` | The event. |
| `shapeTargetA` | The callback target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.ContactBeginEvent.shapeA](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactBeginEvent-shapeA.html). This returns any implemented [PhysicsCallbacks.IContactCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactCallback.html) or NULL if not implemented or no target. |
| `shapeTargetB` | The callback target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.ContactBeginEvent.shapeB](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactBeginEvent-shapeB.html). This returns any implemented [PhysicsCallbacks.IContactCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactCallback.html) or NULL if not implemented or no target. |

#### ContactEndTarget

> Contact end event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ContactCallbackTargets.ContactEndTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `endEvent` | The event. |
| `shapeTargetA` | The callback target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.ContactEndEvent.shapeA](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactEndEvent-shapeA.html). This returns any implemented [PhysicsCallbacks.IContactCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactCallback.html) or NULL if not implemented or no target. |
| `shapeTargetB` | The callback target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.ContactEndEvent.shapeB](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactEndEvent-shapeB.html). This returns any implemented [PhysicsCallbacks.IContactCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactCallback.html) or NULL if not implemented or no target. |

### IBodyUpdateCallback

> An interface that when implemented, can be called as a target by [PhysicsWorld.SendBodyUpdateCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendBodyUpdateCallbacks.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IBodyUpdateCallback`  

#### Methods

##### `OnBodyUpdate2D(PhysicsEvents.BodyUpdateEvent)`

Called when a [PhysicsEvents.BodyUpdateEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html) for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `bodyUpdateEvent` — The event that occurred.

### IContactCallback

> An interface that when implemented, can be called as a target by [PhysicsWorld.SendContactCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendContactCallbacks.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IContactCallback`  

#### Methods

##### `OnContactBegin2D(PhysicsEvents.ContactBeginEvent)`

Called when a [PhysicsEvents.ContactBeginEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactBeginEvent.html) for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `beginEvent` — The event that occurred.

##### `OnContactEnd2D(PhysicsEvents.ContactEndEvent)`

Called when a [PhysicsEvents.ContactEndEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactEndEvent.html) for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `endEvent` — The event that occurred.

### IContactFilterCallback

> An interface that when implemented, can be called as a target when a [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) has [PhysicsShape.contactFilterCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-contactFilterCallbacks.html) set to true. The [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) is in also has to have its [PhysicsWorld.contactFilterCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactFilterCallbacks.html) set to true.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IContactFilterCallback`  

#### Methods

##### `OnContactFilter2D(PhysicsEvents.ContactFilterEvent)`

Called when a pair of shapes are determined to be in contact. This is called to decide if a contact will be created for these shapes, allowing contact creation to be bypassed so a contact will not go to the solver. This is only called if the [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) has [PhysicsWorld.contactFilterCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactFilterCallbacks.html) set to true. An event is only produced if one of the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) have [PhysicsShape.contactFilterCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-contactFilterCallbacks.html) set to true. This is called for both triggers and non-triggers but only with Dynamic bodies. Extreme care must be taken with this callback!! This callback occurs during the simulation step and can be called from any thread, therefore it must be thread-safe. During this time, the simulation state is undefined for the broadphase, events etc. For this reason, any attempt to perform a write operation will result in a deadlock as the world itself is write locked. Performing simple read operations on [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) or [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) is safe, such as reading velocity or getting the geometry of a shape however, more complex operations involving the world such as performing a query can result in corruption or crashes. A recommendation is reading [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) from any object which is a completely safe read operation therefore any required information should be encoded there if possible.

**Params:**
- `contactFilterEvent` — The event that occurred.

**Returns:** Return false if you do not want a contact to be created during this simulation step. Returning true allows the contact to be created.

### IJointThresholdCallback

> An interface that when implemented, can be called as a target by [PhysicsWorld.SendJointThresholdCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendJointThresholdCallbacks.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IJointThresholdCallback`  

#### Methods

##### `OnJointThreshold2D(PhysicsEvents.JointThresholdEvent)`

Called when a [PhysicsEvents.JointThresholdEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.JointThresholdEvent.html) for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `thresholdEvent` — The event that occurred.

### IPreSolveCallback

> An interface that when implemented by a [System.Object](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/System.Object.html), can be called as a target when a [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) has [PhysicsShape.preSolveCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-preSolveCallbacks.html) set to true. The [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) is in also has to have its [PhysicsWorld.preSolveCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-preSolveCallbacks.html) set to true.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IPreSolveCallback`  

#### Methods

##### `OnPreSolve2D(PhysicsEvents.PreSolveEvent)`

Called when a contact between a pair of shapes is updated. This allows a contact to be disabled before it goes to the solver. A typical use-case would be to implement a one-way behaviour based upon the provided contact. This is only called if the [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) has [PhysicsWorld.preSolveCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-preSolveCallbacks.html) set to true. An event is only produced if one of the [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) have [PhysicsShape.preSolveCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-preSolveCallbacks.html) set to true. This is only called for Awake Dynamic bodies. This is not called for triggers. Extreme care must be taken with this callback!! This callback occurs during the simulation step and can be called from any thread, therefore it must be thread-safe. During this time, the simulation state is undefined for the broadphase, events etc. For this reason, any attempt to perform a write operation will result in a deadlock as the world itself is write locked. Performing simple read operations on [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html), [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) or [PhysicsJoint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) is safe, such as reading velocity or getting the geometry of a shape, however more complex operations involving the world such as performing a query can result in corruption or crashes. A recommendation is to use the provided contact details to make a decision in the callback. An additional recommendation is reading [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) from any object which is a completely safe read operation therefore any required information should be encoded there if possible.

**Params:**
- `preSolveEvent` — The event that occurred.

**Returns:** Return false if you want to disable the contact this simulation step. Returning true allows the contact.

### ITransformChangedCallback

> An interface that when implemented, can be called when using [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ITransformChangedCallback`  

#### Methods

##### `OnTransformChanged(PhysicsEvents.TransformChangeEvent)`

Called when a [PhysicsEvents.TransformChangeEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent.html) for the object occurs. This will always be called on the main-thread.

**Params:**
- `transformChangeEvent` — —

### ITransformWriteCallback

> An interface that when implemented, can be called as a target set with [PhysicsWorld.transformWriteCallbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteCallbackTarget.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback`  

#### Methods

##### `OnTransformTweenWrite(PhysicsEvents.TransformTweenWriteEvent)`

The callback will only occur if [PhysicsWorld.transformTweenMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformTweenMode.html) is set to [PhysicsWorld.TransformTweenMode.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformTweenMode-Custom.html) and there are transform write tweens available. This will always be called on the main-thread after the simulation has finished. You should avoid write operations on physics objects during this callback. NOTE: When transform tweening, you can calculate [PhysicsBody.TransformWriteMode.Interpolate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteMode-Interpolate.html) or [PhysicsBody.TransformWriteMode.Extrapolate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteMode-Extrapolate.html) write modes by using [PhysicsBody.TransformWriteTween.GetInterpolatedPose](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteTween-GetInterpolatedPose.html) and [PhysicsBody.TransformWriteTween.GetExtrapolatedPose](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteTween-GetExtrapolatedPose.html) respectively.

**Params:**
- `transformTweenWriteEvent` — The event that occurred.

##### `OnTransformWrite(PhysicsEvents.TransformWriteEvent)`

The callback will only occur if [PhysicsWorld.transformWriteMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteMode.html) is set to [PhysicsWorld.TransformWriteMode.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformWriteMode-Custom.html) and there are [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html) available. To aid in correctly calculating the write pose, [PhysicsBody.TransformWriteTween.GetPose](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteTween-GetPose.html) can be used. The [PhysicsBody.TransformWriteTween](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteTween.html) sent to this event will automatically be assigned to the world for tweening if [PhysicsWorld.transformTweenMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformTweenMode.html) is not [PhysicsWorld.TransformTweenMode.Off](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformTweenMode-Off.html). This will always be called on the main-thread after the simulation has finished. You should avoid write operations on physics objects during this callback. NOTE: When transform writing, the [PhysicsEvents.TransformWriteEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformWriteEvent.html) provides all the [PhysicsBody.TransformWriteTween](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.TransformWriteTween.html) in preparation for transform writing and tweening.

**Params:**
- `transformWriteEvent` — The event that occurred.

### ITriggerCallback

> An interface that when implemented, can be called as a target by [PhysicsWorld.SendTriggerCallbacks](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-SendTriggerCallbacks.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback`  

#### Methods

##### `OnTriggerBegin2D(PhysicsEvents.TriggerBeginEvent)`

Called when a [PhysicsEvents.TriggerBeginEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerBeginEvent.html) for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `beginEvent` — The event that occurred.

##### `OnTriggerEnd2D(PhysicsEvents.TriggerEndEvent)`

Called when a [PhysicsEvents.TriggerEndEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerEndEvent.html) for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `endEvent` — The event that occurred.

### JointThresholdCallbackTargets

> Contains all the joint callback targets returned from [PhysicsWorld.GetJointThresholdCallbackTargets](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-GetJointThresholdCallbackTargets.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.JointThresholdCallbackTargets`  

#### Properties

| Name | Summary |
|------|---------|
| `jointThresholdCallbackTargets` | The joint threshold targets. |

#### Methods

##### `Dispose()`

Dispose of any allocated memory. This must be called if any targets are returned otherwise memory leaks will occur.

#### Nested Types

- **JointThresholdTarget** — Joint threshold event target for callbacks.

#### JointThresholdTarget

> Joint threshold event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.JointThresholdCallbackTargets.JointThresholdTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `jointTarget` | The [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.JointThresholdEvent.joint](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.JointThresholdEvent-joint.html). This returns any implemented [PhysicsCallbacks.IJointThresholdCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IJointThresholdCallback.html) or NULL if not implemented or no target. |
| `jointThresholdEvent` | The event. |

### TriggerCallbackTargets

> Contains all the trigger callback targets returned from [PhysicsWorld.GetTriggerCallbackTargets](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-GetTriggerCallbackTargets.html).

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.TriggerCallbackTargets`  

#### Properties

| Name | Summary |
|------|---------|
| `BeginCallbackTargets` | The begin targets. |
| `EndCallbackTargets` | The end targets. |

#### Methods

##### `Dispose()`

Dispose of any allocated memory. This must be called if any targets are returned otherwise memory leaks will occur.

#### Nested Types

- **TriggerBeginTarget** — Trigger begin event target for callbacks.
- **TriggerEndTarget** — Trigger end event target for callbacks.

#### TriggerBeginTarget

> Trigger begin event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.TriggerCallbackTargets.TriggerBeginTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `beginEvent` | The trigger begin event. |
| `triggerShapeTarget` | The callback target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.TriggerBeginEvent.triggerShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerBeginEvent-triggerShape.html). This returns any implemented [PhysicsCallbacks.ITriggerCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback.html) or NULL if not implemented or no target. |
| `visitorShapeTarget` | The callback target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.TriggerBeginEvent.visitorShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerBeginEvent-visitorShape.html). This returns any implemented [PhysicsCallbacks.ITriggerCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback.html) or NULL if not implemented or no target. |

#### TriggerEndTarget

> Trigger end event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.TriggerCallbackTargets.TriggerEndTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `endEvent` | The trigger end event. |
| `triggerShapeTarget` | The callback target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.TriggerEndEvent.triggerShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerEndEvent-triggerShape.html). This returns any implemented [PhysicsCallbacks.ITriggerCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback.html) or NULL if not implemented or no target. |
| `visitorShapeTarget` | The callback target ([PhysicsShape.callbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-callbackTarget.html)) associated with [PhysicsEvents.TriggerEndEvent.visitorShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerEndEvent-visitorShape.html). This returns any implemented [PhysicsCallbacks.ITriggerCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback.html) or NULL if not implemented or no target. |

## PhysicsEvents

> Various events that can be retrieved during and after the simulation has completed. See [PhysicsWorld.Simulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Simulate.html) and [PhysicsWorld.Simulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-Simulate.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents`  
**Docs:** [Unity.U2D.Physics.PhysicsEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.html)

### Events

| Name | Summary |
|------|---------|
| `PostSimulate` | Event callback for a post-simulate event. This is called after the simulation has finished running and is always called on the main-thread. See [PhysicsEvents.PostSimulateEventHandler](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.PostSimulateEventHandler.html). |
| `PreSimulate` | Event callback for a pre-simulate event. This is called prior to the simulation running and is always called on the main-thread. See [PhysicsEvents.PreSimulateEventHandler](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.PreSimulateEventHandler.html). |
| `WorldDefinitionChange` | Event callback for a world definition change event. |
| `WorldDrawResults` | Event callback for a world draw results event. This is only called if the world is currently rendering as specified by [PhysicsWorld.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-renderingMode.html) or if [PhysicsCoreSettings2D.alwaysDrawWorlds](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-alwaysDrawWorlds.html) is true. CAUTION: The world is READ locked during this event so ANY write operation on the world will cause an immediate deadlock. See [PhysicsEvents.WorldDrawResultsEventHandler](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.WorldDrawResultsEventHandler.html). |
| `WorldTransformPlaneChange` | Event callback for a world transform-plane change event. This only fires when the transform plane actually changes. |

### Nested Types

- **BodyUpdateEvent** — An event produced by a [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) that indicates the simulation changed the body in one of the following ways: - The body transform was changed. - The body fell asleep. See [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html).
- **ContactBeginEvent** — An event produced by a pair of Shapes, neither of which are a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). See [PhysicsWorld.contactBeginEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactBeginEvents.html).
- **ContactEndEvent** — An event produced by a pair of Shapes, neither of which are a trigger, stopped touching. You will get an end event if you do anything that destroys contacts prior to the last world simulation step which include things like setting the body transform, destroying a body etc. The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). See [PhysicsWorld.contactEndEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactEndEvents.html).
- **ContactFilterEvent** — An event produced when a pair of [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) come into contact. This can be used to decide if a contact between the two shapes should be created or not.
- **ContactHitEvent** — An event produced when a pair of [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) come into contact at relative speed exceeding the [PhysicsWorld.contactHitEventThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactHitEventThreshold.html). The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). This may be reported for speculative contacts that have a confirmed impulse. See [PhysicsWorld.contactHitEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactHitEvents.html).
- **JointThresholdEvent** — An event produced by a Joint which exceeds either its [PhysicsJoint.forceThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-forceThreshold.html) or [PhysicsJoint.torqueThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-torqueThreshold.html).
- **PostSimulateEventHandler** — Event handler for a post-simulate event callback. This is called after the simulation has finished running and is always called on the main-thread. See [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) and [PhysicsEvents.PostSimulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents-PostSimulate.html).
- **PreSimulateEventHandler** — Event handler for a pre-simulate event callback. This is called prior to the simulation running and is always called on the main-thread. See [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) and [PhysicsEvents.PreSimulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents-PreSimulate.html).
- **PreSolveEvent** — An event produced when a contact between a pair of [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) is updated, used to provide the ability to decide if the contact should be disabled or not.
- **TransformChangeEvent** — An event produced after registering via [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html).
- **TransformTweenWriteEvent** — An event produced and sent to the callback target set with [PhysicsWorld.transformWriteCallbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteCallbackTarget.html) which must implement [PhysicsCallbacks.ITransformWriteCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback.html) which will have [PhysicsCallbacks.ITransformWriteCallback.OnTransformTweenWrite](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback-OnTransformTweenWrite.html) called allowing custom transform writing.
- **TransformWriteEvent** — An event produced and sent to the callback target set with [PhysicsWorld.transformWriteCallbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteCallbackTarget.html) which must implement [PhysicsCallbacks.ITransformWriteCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback.html) which will have [PhysicsCallbacks.ITransformWriteCallback.OnTransformWrite](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback-OnTransformWrite.html) called allowing custom transform writing.
- **TriggerBeginEvent** — An event produced when a pair of Shapes, one of which was a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). See [PhysicsWorld.triggerBeginEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-triggerBeginEvents.html).
- **TriggerEndEvent** — An event produced when a pair of Shapes, one of which was a trigger, stopped touching. An end event will be produced anything that destroys contacts happens, prior to the last world simulation step, which include things like setting the body transform, destroying a body or shape or changing a contact filter etc. The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). See [PhysicsWorld.triggerEndEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-triggerEndEvents.html).
- **WorldDefinitionChangeEventHandler** — Event handler for a world definition change event callback.
- **WorldDrawResultsEventHandler** — Event handler for a world draw results event callback. This is only called if the world is currently rendering as specified by [PhysicsWorld.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-renderingMode.html) or if [PhysicsCoreSettings2D.alwaysDrawWorlds](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-alwaysDrawWorlds.html) is true. CAUTION: The world is READ locked during this event so ANY write operation on the world will cause an immediate deadlock.
- **WorldTransformPlaneChangeEventHandler** — Event handler for a world transform-plane change event callback.

### BodyUpdateEvent

> An event produced by a [PhysicsBody](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html) that indicates the simulation changed the body in one of the following ways: - The body transform was changed. - The body fell asleep. See [PhysicsWorld.bodyUpdateEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-bodyUpdateEvents.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `body` | The body this event relates to. |
| `fellAsleep` | Whether the body fell asleep or not. |
| `transform` | The current transform of the body. |

#### Methods

##### `ToString()`

### ContactBeginEvent

> An event produced by a pair of Shapes, neither of which are a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). See [PhysicsWorld.contactBeginEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactBeginEvents.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactBeginEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `contactId` | The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with [PhysicsShape.ContactId.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactId-isValid.html). |
| `firstGroup` | Whether this is the first contact event between the two groups the shapes belong to. Always true when either shape has no group or the world has event grouping disallowed. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

#### Methods

##### `ToString()`

### ContactEndEvent

> An event produced by a pair of Shapes, neither of which are a trigger, stopped touching. You will get an end event if you do anything that destroys contacts prior to the last world simulation step which include things like setting the body transform, destroying a body etc. The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). See [PhysicsWorld.contactEndEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactEndEvents.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactEndEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `contactId` | The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with [PhysicsShape.ContactId.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactId-isValid.html). |
| `lastGroup` | Whether this is the last contact event between the two groups the shapes belong to. Always true when either shape has no group or the world has event grouping disallowed. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

#### Methods

##### `ToString()`

### ContactFilterEvent

> An event produced when a pair of [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) come into contact. This can be used to decide if a contact between the two shapes should be created or not.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactFilterEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `physicsWorld` | The physics world both shapes are within. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

#### Methods

##### `ToString()`

### ContactHitEvent

> An event produced when a pair of [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) come into contact at relative speed exceeding the [PhysicsWorld.contactHitEventThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactHitEventThreshold.html). The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). This may be reported for speculative contacts that have a confirmed impulse. See [PhysicsWorld.contactHitEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-contactHitEvents.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactHitEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `approachSpeed` | The speed the shapes are approaching, typically in meters per second. This value is always positive. |
| `contactId` | The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with [PhysicsShape.ContactId.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ContactId-isValid.html). |
| `normal` | Normal vector that always points in the direction from shape A to shape B. |
| `point` | Point where the shapes hit at the beginning of the time step. This is a mid-point between the two surfaces. It could be at speculative point where the two shapes were not touching at the beginning of the time step. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

#### Methods

##### `ToString()`

### JointThresholdEvent

> An event produced by a Joint which exceeds either its [PhysicsJoint.forceThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-forceThreshold.html) or [PhysicsJoint.torqueThreshold](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-torqueThreshold.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.JointThresholdEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `joint` | The joint involved in the event. |

#### Methods

##### `ToString()`

### PostSimulateEventHandler

> Event handler for a post-simulate event callback. This is called after the simulation has finished running and is always called on the main-thread. See [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) and [PhysicsEvents.PostSimulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents-PostSimulate.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.PostSimulateEventHandler`  

### PreSimulateEventHandler

> Event handler for a pre-simulate event callback. This is called prior to the simulation running and is always called on the main-thread. See [PhysicsWorld](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html) and [PhysicsEvents.PreSimulate](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents-PreSimulate.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.PreSimulateEventHandler`  

### PreSolveEvent

> An event produced when a contact between a pair of [PhysicsShape](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) is updated, used to provide the ability to decide if the contact should be disabled or not.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.PreSolveEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `normal` | The surface normal at the point of contact. |
| `physicsWorld` | The physics world both shapes are within. |
| `point` | The point of contact. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

#### Methods

##### `ToString()`

### TransformChangeEvent

> An event produced after registering via [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-RegisterTransformChange.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `changeReason` | The reason(s) the transform changed. |
| `transform` | The transform that changed. |

### TransformTweenWriteEvent

> An event produced and sent to the callback target set with [PhysicsWorld.transformWriteCallbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteCallbackTarget.html) which must implement [PhysicsCallbacks.ITransformWriteCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback.html) which will have [PhysicsCallbacks.ITransformWriteCallback.OnTransformTweenWrite](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback-OnTransformTweenWrite.html) called allowing custom transform writing.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TransformTweenWriteEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `extrapolationTime` | The extrapolation time when the event was created, in the range [0, 1]. |
| `interpolationTime` | The interpolation time when the event was created, in the range [0, 1]. |
| `physicsWorld` | The physics world the event was created from. |
| `transformPlane` | The transform plane of the physics world when the event was created. |
| `transformPlaneCustom` | The transform plane (custom) of the physics world when the event was created. This maybe not be relevant unless the transform plane is [PhysicsWorld.TransformPlane.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-Custom.html). |
| `tweens` | The transform write tweens available to be configured. The returned [Unity.Collections.NativeArray](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.NativeArray.html) aliases the per-frame internal buffer owned by the world; it does not own its memory (so disposing it does nothing). The contents are only valid until the next simulation step runs, after which the buffer may be reused or destroyed. If a longer-lived copy is required, copy the contents into a caller-owned [Unity.Collections.NativeArray](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.NativeArray.html). |

### TransformWriteEvent

> An event produced and sent to the callback target set with [PhysicsWorld.transformWriteCallbackTarget](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-transformWriteCallbackTarget.html) which must implement [PhysicsCallbacks.ITransformWriteCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback.html) which will have [PhysicsCallbacks.ITransformWriteCallback.OnTransformWrite](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback-OnTransformWrite.html) called allowing custom transform writing.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TransformWriteEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `physicsWorld` | The physics world the event was created from. |
| `simulationType` | The simulation type of the physics world when the event was created. |
| `transformPlane` | The transform plane of the physics world when the event was created. |
| `transformPlaneCustom` | The transform plane (custom) of the physics world when the event was created. This maybe not be relevant unless the transform plane is [PhysicsWorld.TransformPlane.Custom](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.TransformPlane-Custom.html). |
| `transformTweenMode` | The transform tween mode of the physics world when the event was created. |
| `tweens` | The transform write tweens available to be configured. The returned [Unity.Collections.NativeArray](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.NativeArray.html) aliases the per-frame internal buffer owned by the world; it does not own its memory (so disposing it does nothing). The contents are only valid until the next simulation step runs, after which the buffer may be reused or destroyed. If a longer-lived copy is required, copy the contents into a caller-owned [Unity.Collections.NativeArray](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.NativeArray.html). |

### TriggerBeginEvent

> An event produced when a pair of Shapes, one of which was a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). See [PhysicsWorld.triggerBeginEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-triggerBeginEvents.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TriggerBeginEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `firstGroup` | Whether this is the first trigger event between the two groups the shapes belong to. Always true when either shape has no group or the world has event grouping disallowed. |
| `triggerShape` | The trigger shape involved in the event. |
| `visitorShape` | The shape that began touching the trigger shape. |

#### Methods

##### `ToString()`

### TriggerEndEvent

> An event produced when a pair of Shapes, one of which was a trigger, stopped touching. An end event will be produced anything that destroys contacts happens, prior to the last world simulation step, which include things like setting the body transform, destroying a body or shape or changing a contact filter etc. The shapes provided may have been destroyed so they should always be validated with [PhysicsShape.isValid](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-isValid.html). See [PhysicsWorld.triggerEndEvents](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-triggerEndEvents.html).

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TriggerEndEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `lastGroup` | Whether this is the last trigger event between the two groups the shapes belong to. Always true when either shape has no group or the world has event grouping disallowed. |
| `triggerShape` | The trigger shape involved in the event. |
| `visitorShape` | The shape that stopped touching the trigger shape. |

#### Methods

##### `ToString()`

### WorldDefinitionChangeEventHandler

> Event handler for a world definition change event callback.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.WorldDefinitionChangeEventHandler`  

### WorldDrawResultsEventHandler

> Event handler for a world draw results event callback. This is only called if the world is currently rendering as specified by [PhysicsWorld.renderingMode](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-renderingMode.html) or if [PhysicsCoreSettings2D.alwaysDrawWorlds](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-alwaysDrawWorlds.html) is true. CAUTION: The world is READ locked during this event so ANY write operation on the world will cause an immediate deadlock.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.WorldDrawResultsEventHandler`  

### WorldTransformPlaneChangeEventHandler

> Event handler for a world transform-plane change event callback.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.WorldTransformPlaneChangeEventHandler`

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.7.0a3 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
