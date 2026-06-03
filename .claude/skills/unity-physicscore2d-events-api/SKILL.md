---
name: unity-physicscore2d-events-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for Events & Callbacks. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsCallbacks, PhysicsEvents. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Events & Callbacks

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsCallbacks`, `PhysicsEvents`.

## PhysicsCallbacks

> All callback interfaces and targets.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks`  
**Docs:** [Unity.U2D.Physics.PhysicsCallbacks](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.html)

### Nested Types

- **BodyUpdateCallbackTargets** — Contains all the body update callback targets returned from .
- **ContactCallbackTargets** — Contains all the contact callback targets returned from .
- **IBodyUpdateCallback** — An interface that when implemented, can be called as a target by .
- **IContactCallback** — An interface that when implemented, can be called as a target by .
- **IContactFilterCallback** — An interface that when implemented, can be called as a target when a has set to true. The the is in also has to have its set to true.
- **IJointThresholdCallback** — An interface that when implemented, can be called as a target by .
- **IPreSolveCallback** — An interface that when implemented by a , can be called as a target when a has set to true. The the is in also has to have its set to true.
- **ITransformChangedCallback** — An interface that when implemented, can be called when using .
- **ITransformWriteCallback** — An interface that when implemented, can be called as a target set with .
- **ITriggerCallback** — An interface that when implemented, can be called as a target by .
- **JointThresholdCallbackTargets** — Contains all the joint callback targets returned from .
- **TriggerCallbackTargets** — Contains all the trigger callback targets returned from .

### BodyUpdateCallbackTargets

> Contains all the body update callback targets returned from .

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.BodyUpdateCallbackTargets`  

#### Properties

| Name | Summary |
|------|---------|
| `bodyUpdateCallbackTargets` | The body update targets. |

#### Methods

##### `Dispose()`

Dispose of any allocated memory. This must be called if any targets are returned otherwise memory leaks will occur.

#### Nested Types

- **BodyUpdateTarget** — Joint threshold event target for callbacks.

#### BodyUpdateTarget

> Joint threshold event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.BodyUpdateCallbackTargets.BodyUpdateTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `bodyTarget` | The callback target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |
| `bodyUpdateEvent` | The event. |

### ContactCallbackTargets

> Contains all the contact callback targets returned from .

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
| `shapeTargetA` | The callback target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |
| `shapeTargetB` | The callback target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |

#### ContactEndTarget

> Contact end event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ContactCallbackTargets.ContactEndTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `endEvent` | The event. |
| `shapeTargetA` | The callback target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |
| `shapeTargetB` | The callback target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |

### IBodyUpdateCallback

> An interface that when implemented, can be called as a target by .

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IBodyUpdateCallback`  

#### Methods

##### `OnBodyUpdate2D(PhysicsEvents.BodyUpdateEvent)`

Called when a for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `bodyUpdateEvent` — The event that occurred.

### IContactCallback

> An interface that when implemented, can be called as a target by .

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IContactCallback`  

#### Methods

##### `OnContactBegin2D(PhysicsEvents.ContactBeginEvent)`

Called when a for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `beginEvent` — The event that occurred.

##### `OnContactEnd2D(PhysicsEvents.ContactEndEvent)`

Called when a for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `endEvent` — The event that occurred.

### IContactFilterCallback

> An interface that when implemented, can be called as a target when a has set to true. The the is in also has to have its set to true.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IContactFilterCallback`  

#### Methods

##### `OnContactFilter2D(PhysicsEvents.ContactFilterEvent)`

Called when a pair of shapes are determined to be in contact. This is called to decide if a contact will be created for these shapes, allowing contact creation to be bypassed so a contact will not go to the solver. This is only called if the has set to true. An event is only produced if one of the have set to true. This is called for both triggers and non-triggers but only with Dynamic bodies. Extreme care must be taken with this callback!! This callback occurs during the simulation step and can be called from any thread, therefore it must be thread-safe. During this time, the simulation state is undefined for the broadphase, events etc. For this reason, any attempt to perform a write operation will result in a deadlock as the world itself is write locked. Performing simple read operations on , or is safe, such as reading velocity or getting the geometry of a shape however, more complex operations involving the world such as performing a query can result in corruption or crashes. A recommendation is reading from any object which is a completely safe read operation therefore any required information should be encoded there if possible.

**Params:**
- `contactFilterEvent` — The event that occurred.

**Returns:** Return false if you do not want a contact to be created during this simulation step. Returning true allows the contact to be created.

### IJointThresholdCallback

> An interface that when implemented, can be called as a target by .

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IJointThresholdCallback`  

#### Methods

##### `OnJointThreshold2D(PhysicsEvents.JointThresholdEvent)`

Called when a for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `thresholdEvent` — The event that occurred.

### IPreSolveCallback

> An interface that when implemented by a , can be called as a target when a has set to true. The the is in also has to have its set to true.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.IPreSolveCallback`  

#### Methods

##### `OnPreSolve2D(PhysicsEvents.PreSolveEvent)`

Called when a contact between a pair of shapes is updated. This allows a contact to be disabled before it goes to the solver. A typical use-case would be to implement a one-way behaviour based upon the provided contact. This is only called if the has set to true. An event is only produced if one of the have set to true. This is only called for Awake Dynamic bodies. This is not called for triggers. Extreme care must be taken with this callback!! This callback occurs during the simulation step and can be called from any thread, therefore it must be thread-safe. During this time, the simulation state is undefined for the broadphase, events etc. For this reason, any attempt to perform a write operation will result in a deadlock as the world itself is write locked. Performing simple read operations on , or is safe, such as reading velocity or getting the geometry of a shape, however more complex operations involving the world such as performing a query can result in corruption or crashes. A recommend is using the provided contact details to make a decision in the callback. An additional recommendation is reading from any object which is a completely safe read operation therefore any required information should be encoded there if possible.

**Params:**
- `preSolveEvent` — The event that occurred.

**Returns:** Return false if you want to disable the contact this simulation step. Returning true allows the contact.

### ITransformChangedCallback

> An interface that when implemented, can be called when using .

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ITransformChangedCallback`  

#### Methods

##### `OnTransformChanged(PhysicsEvents.TransformChangeEvent)`

Called when a for the object occurs. This will always be called on the main-thread.

**Params:**
- `transformChangeEvent` — —

### ITransformWriteCallback

> An interface that when implemented, can be called as a target set with .

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback`  

#### Methods

##### `OnTransformTweenWrite(PhysicsEvents.TransformTweenWriteEvent)`

The callback will only occur if is set to and there are transform write tweens available. This will always be called on the main-thread after the simulation has finished. You should avoid write operations on physics objects during this callback. NOTE: When transform tweening, you can calculate or write modes by using and respectively.

**Params:**
- `transformTweenWriteEvent` — The event that occurred.

##### `OnTransformWrite(PhysicsEvents.TransformWriteEvent)`

The callback will only occur if is set to and there are available. To aid in correctly calculating the write pose, can be used. The sent to this event will automatically be assigned to the world for tweening if is not . This will always be called on the main-thread after the simulation has finished. You should avoid write operations on physics objects during this callback. NOTE: When transform writing, the provides all the in preparation for transform writing and tweening.

**Params:**
- `transformWriteEvent` — The event that occurred.

### ITriggerCallback

> An interface that when implemented, can be called as a target by .

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback`  

#### Methods

##### `OnTriggerBegin2D(PhysicsEvents.TriggerBeginEvent)`

Called when a for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `beginEvent` — The event that occurred.

##### `OnTriggerEnd2D(PhysicsEvents.TriggerEndEvent)`

Called when a for the object occurs. This will always be called on the main-thread after the simulation has finished.

**Params:**
- `endEvent` — The event that occurred.

### JointThresholdCallbackTargets

> Contains all the joint callback targets returned from .

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
| `jointTarget` | The target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |
| `jointThresholdEvent` | The event. |

### TriggerCallbackTargets

> Contains all the trigger callback targets returned from .

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
| `triggerShapeTarget` | The callback target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |
| `visitorShapeTarget` | The callback target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |

#### TriggerEndTarget

> Trigger end event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.TriggerCallbackTargets.TriggerEndTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `endEvent` | The trigger end event. |
| `triggerShapeTarget` | The callback target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |
| `visitorShapeTarget` | The callback target ( ) associated with . This returns any implemented or NULL if not implemented or no target. |

## PhysicsEvents

> Various events that can be retrieved during and after the simulation has completed. See and .

**Full name:** `Unity.U2D.Physics.PhysicsEvents`  
**Docs:** [Unity.U2D.Physics.PhysicsEvents](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.html)

### Events

| Name | Summary |
|------|---------|
| `PostSimulate` | Event callback for a post-simulate event. This is called after the simulation has finished running and is always called on the main-thread. See . |
| `PreSimulate` | Event callback for a pre-simulate event. This is called prior to the simulation running and is always called on the main-thread. See . |
| `WorldDefinitionChange` | Event callback for a world definition change event. |
| `WorldDrawResults` | Event callback for a world draw results event. This is only called if the world is currently rendering as specified by or if is true. CAUTION: The world is READ locked during this event so ANY write operation on the world will cause an immediate deadlock. See . |

### Methods

#### `InvokePostSimulate(PhysicsWorld, float)`

#### `InvokePreSimulate(PhysicsWorld, float)`

#### `InvokeWorldDefinitionChangeEvent(PhysicsWorld)`

#### `InvokeWorldDrawResultsEvent(PhysicsWorld, PhysicsWorld.DrawResults)`

#### `SendBodyUpdateCallbacks(PhysicsWorld)`

#### `SendContactCallbacks(PhysicsWorld)`

#### `SendContactFilterCallback(object, PhysicsEvents.ContactFilterEvent)`

#### `SendJointThresholdCallbacks(PhysicsWorld)`

#### `SendPreSolveCallback(object, PhysicsEvents.PreSolveEvent)`

#### `SendTriggerCallbacks(PhysicsWorld)`

### Nested Types

- **BodyUpdateEvent** — An event produced by a that indicates the simulation changed the body in one of the following ways: - The body transform was changed. - The body fell asleep. See .
- **ContactBeginEvent** — An event produced by a pair of Shapes, neither of which are a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with . See .
- **ContactEndEvent** — An event produced by a pair of Shapes, neither of which are a trigger, stopped touching. You will get an end event if you do anything that destroys contacts prior to the last world simulation step which include things like setting the body transform, destroying a body etc. The shapes provided may have been destroyed so they should always be validated with . See .
- **ContactFilterEvent** — An event produced when a pair of come into contact. This can be used to decide if a contact between the two shapes should be created or not.
- **ContactHitEvent** — An event produced when a pair of come into contact at relative speed exceeding the . The shapes provided may have been destroyed so they should always be validated with . This may be reported for speculative contacts that have a confirmed impulse. See .
- **JointThresholdEvent** — An event produced by a Joint which exceeds either its or .
- **PostSimulateEventHandler** — Event handler for a post-simulate event callback. This is called after the simulation has finished running and is always called on the main-thread. See and .
- **PreSimulateEventHandler** — Event handler for a pre-simulate event callback. This is called prior to the simulation running and is always called on the main-thread. See and .
- **PreSolveEvent** — An event produced when a contact between a pair of is updated, used to provide the ability to decide if the contact should be disabled or not.
- **TransformChangeEvent** — An event produced after registering via .
- **TransformTweenWriteEvent** — An event produced and sent to the callback target set with which must implement which will have called allowing custom transform writing.
- **TransformWriteEvent** — An event produced and sent to the callback target set with which must implement which will have called allowing custom transform writing.
- **TriggerBeginEvent** — An event produced when a pair of Shapes, one of which was a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with . See .
- **TriggerEndEvent** — An event produced when a pair of Shapes, one of which was a trigger, stopped touching. An end event will be produced anything that destroys contacts happens, prior to the last world simulation step, which include things like setting the body transform, destroying a body or shape or changing a contact filter etc. The shapes provided may have been destroyed so they should always be validated with . See .
- **WorldDefinitionChangeEventHandler** — Event handler for a world definition change event callback.
- **WorldDrawResultsEventHandler** — Event handler for a world draw results event callback. This is only called if the world is currently rendering as specified by or if is true. CAUTION: The world is READ locked during this event so ANY write operation on the world will cause an immediate deadlock.

### BodyUpdateEvent

> An event produced by a that indicates the simulation changed the body in one of the following ways: - The body transform was changed. - The body fell asleep. See .

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

> An event produced by a pair of Shapes, neither of which are a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with . See .

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactBeginEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `contactId` | The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with . |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

#### Methods

##### `ToString()`

### ContactEndEvent

> An event produced by a pair of Shapes, neither of which are a trigger, stopped touching. You will get an end event if you do anything that destroys contacts prior to the last world simulation step which include things like setting the body transform, destroying a body etc. The shapes provided may have been destroyed so they should always be validated with . See .

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactEndEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `contactId` | The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with . |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

#### Methods

##### `ToString()`

### ContactFilterEvent

> An event produced when a pair of come into contact. This can be used to decide if a contact between the two shapes should be created or not.

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

> An event produced when a pair of come into contact at relative speed exceeding the . The shapes provided may have been destroyed so they should always be validated with . This may be reported for speculative contacts that have a confirmed impulse. See .

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactHitEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `approachSpeed` | The speed the shapes are approaching, typically in meters per second. This value is always positive. |
| `contactId` | The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with . |
| `normal` | Normal vector that always points in the direction from shape A to shape B. |
| `point` | Point where the shapes hit at the beginning of the time step. This is a mid-point between the two surfaces. It could be at speculative point where the two shapes were not touching at the beginning of the time step. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

#### Methods

##### `ToString()`

### JointThresholdEvent

> An event produced by a Joint which exceeds either its or .

**Full name:** `Unity.U2D.Physics.PhysicsEvents.JointThresholdEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `joint` | The joint involved in the event. |

#### Methods

##### `ToString()`

### PostSimulateEventHandler

> Event handler for a post-simulate event callback. This is called after the simulation has finished running and is always called on the main-thread. See and .

**Full name:** `Unity.U2D.Physics.PhysicsEvents.PostSimulateEventHandler`  

### PreSimulateEventHandler

> Event handler for a pre-simulate event callback. This is called prior to the simulation running and is always called on the main-thread. See and .

**Full name:** `Unity.U2D.Physics.PhysicsEvents.PreSimulateEventHandler`  

### PreSolveEvent

> An event produced when a contact between a pair of is updated, used to provide the ability to decide if the contact should be disabled or not.

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

> An event produced after registering via .

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `changeReason` | The reason(s) the transform changed. |
| `transform` | The transform that changed. |

### TransformTweenWriteEvent

> An event produced and sent to the callback target set with which must implement which will have called allowing custom transform writing.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TransformTweenWriteEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `extrapolationTime` | The extrapolation time when the event was created, in the range [0, 1]. |
| `interpolationTime` | The interpolation time when the event was created, in the range [0, 1]. |
| `physicsWorld` | The physics world the event was created from. |
| `transfomPlaneCustom` | The transform plane (custom) of the physics world when the event was created. This maybe not be relevant unless the transform plane is . |
| `transformPlane` | The transform plane of the physics world when the event was created. |
| `tweens` | The transform write tweens available to be configured. |

### TransformWriteEvent

> An event produced and sent to the callback target set with which must implement which will have called allowing custom transform writing.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TransformWriteEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `physicsWorld` | The physics world the event was created from. |
| `simulationType` | The simulation type of the physics world when the event was created. |
| `transfomPlaneCustom` | The transform plane (custom) of the physics world when the event was created. This maybe not be relevant unless the transform plane is . |
| `transformPlane` | The transform plane of the physics world when the event was created. |
| `transformTweenMode` | The transform tween mode of the physics world when the event was created. |
| `tweens` | The transform write tweens available to be configured. |

### TriggerBeginEvent

> An event produced when a pair of Shapes, one of which was a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with . See .

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TriggerBeginEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `triggerShape` | The trigger shape involved in the event. |
| `visitorShape` | The shape that began touching the trigger shape. |

#### Methods

##### `ToString()`

### TriggerEndEvent

> An event produced when a pair of Shapes, one of which was a trigger, stopped touching. An end event will be produced anything that destroys contacts happens, prior to the last world simulation step, which include things like setting the body transform, destroying a body or shape or changing a contact filter etc. The shapes provided may have been destroyed so they should always be validated with . See .

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TriggerEndEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `triggerShape` | The trigger shape involved in the event. |
| `visitorShape` | The shape that stopped touching the trigger shape. |

#### Methods

##### `ToString()`

### WorldDefinitionChangeEventHandler

> Event handler for a world definition change event callback.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.WorldDefinitionChangeEventHandler`  

### WorldDrawResultsEventHandler

> Event handler for a world draw results event callback. This is only called if the world is currently rendering as specified by or if is true. CAUTION: The world is READ locked during this event so ANY write operation on the world will cause an immediate deadlock.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.WorldDrawResultsEventHandler`

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
