# PhysicsCore2D — Events & Callbacks

_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsCallbacks`, `PhysicsEvents`.

## PhysicsCallbacks

> All callback interfaces and targets.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks`  
**Docs:** [Unity.U2D.Physics.PhysicsCallbacks](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.html)

### Nested Types

- **BodyUpdateCallbackTargets** — Contains all the body update callback targets returned from PhysicsWorld.GetJointThresholdCallbackTargets.
- **ContactCallbackTargets** — Contains all the contact callback targets returned from PhysicsWorld.GetContactCallbackTargets.
- **JointThresholdCallbackTargets** — Contains all the joint callback targets returned from PhysicsWorld.GetJointThresholdCallbackTargets.
- **TriggerCallbackTargets** — Contains all the trigger callback targets returned from PhysicsWorld.GetTriggerCallbackTargets.

### BodyUpdateCallbackTargets

> Contains all the body update callback targets returned from PhysicsWorld.GetJointThresholdCallbackTargets.

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
| `bodyTarget` | The callback target (PhysicsShape._callbackTarget) associated with PhysicsEvents.BodyUpdateEvent. This returns any implemented PhysicsCallbacks.IBodyUpdateCallback or NULL if not implemented or no target. |
| `bodyUpdateEvent` | The event. |

### ContactCallbackTargets

> Contains all the contact callback targets returned from PhysicsWorld.GetContactCallbackTargets.

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
| `shapeTargetA` | The callback target (PhysicsShape._callbackTarget) associated with PhysicsEvents.ContactBeginEvent._shapeA. This returns any implemented PhysicsCallbacks.IContactCallback or NULL if not implemented or no target. |
| `shapeTargetB` | The callback target (PhysicsShape._callbackTarget) associated with PhysicsEvents.ContactBeginEvent._shapeB. This returns any implemented PhysicsCallbacks.IContactCallback or NULL if not implemented or no target. |

#### ContactEndTarget

> Contact end event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.ContactCallbackTargets.ContactEndTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `endEvent` | The event. |
| `shapeTargetA` | The callback target (PhysicsShape._callbackTarget) associated with PhysicsEvents.ContactEndEvent._shapeA. This returns any implemented PhysicsCallbacks.IContactCallback or NULL if not implemented or no target. |
| `shapeTargetB` | The callback target (PhysicsShape._callbackTarget) associated with PhysicsEvents.ContactEndEvent._shapeB. This returns any implemented PhysicsCallbacks.IContactCallback or NULL if not implemented or no target. |

### JointThresholdCallbackTargets

> Contains all the joint callback targets returned from PhysicsWorld.GetJointThresholdCallbackTargets.

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
| `jointTarget` | The PhysicsShape target (PhysicsShape._callbackTarget) associated with PhysicsEvents.JointThresholdEvent._joint. This returns any implemented PhysicsCallbacks.IJointThresholdCallback or NULL if not implemented or no target. |
| `jointThresholdEvent` | The event. |

### TriggerCallbackTargets

> Contains all the trigger callback targets returned from PhysicsWorld.GetTriggerCallbackTargets.

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
| `triggerShapeTarget` | The callback target (PhysicsShape._callbackTarget) associated with PhysicsEvents.TriggerBeginEvent._triggerShape. This returns any implemented PhysicsCallbacks.ITriggerCallback or NULL if not implemented or no target. |
| `visitorShapeTarget` | The callback target (PhysicsShape._callbackTarget) associated with PhysicsEvents.TriggerBeginEvent._visitorShape. This returns any implemented PhysicsCallbacks.ITriggerCallback or NULL if not implemented or no target. |

#### TriggerEndTarget

> Trigger end event target for callbacks.

**Full name:** `Unity.U2D.Physics.PhysicsCallbacks.TriggerCallbackTargets.TriggerEndTarget`  

##### Properties

| Name | Summary |
|------|---------|
| `endEvent` | The trigger end event. |
| `triggerShapeTarget` | The callback target (PhysicsShape._callbackTarget) associated with PhysicsEvents.TriggerEndEvent._triggerShape. This returns any implemented PhysicsCallbacks.ITriggerCallback or NULL if not implemented or no target. |
| `visitorShapeTarget` | The callback target (PhysicsShape._callbackTarget) associated with PhysicsEvents.TriggerEndEvent._visitorShape. This returns any implemented PhysicsCallbacks.ITriggerCallback or NULL if not implemented or no target. |

## PhysicsEvents

> Various events that can be retrieved during and after the simulation has completed. See PhysicsWorld.Simulate and PhysicsWorld.Simulate.

**Full name:** `Unity.U2D.Physics.PhysicsEvents`  
**Docs:** [Unity.U2D.Physics.PhysicsEvents](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.html)

### Nested Types

- **BodyUpdateEvent** — An event produced by a PhysicsBody that indicates the simulation changed the body in one of the following ways: - The body transform was changed. - The body fell asleep. See PhysicsWorld._bodyUpdateEvents.
- **ContactBeginEvent** — An event produced by a pair of Shapes, neither of which are a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. See PhysicsWorld._contactBeginEvents.
- **ContactEndEvent** — An event produced by a pair of Shapes, neither of which are a trigger, stopped touching. You will get an end event if you do anything that destroys contacts prior to the last world simulation step which include things like setting the body transform, destroying a body etc. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. See PhysicsWorld._contactEndEvents.
- **ContactFilterEvent** — An event produced when a pair of PhysicsShape come into contact. This can be used to decide if a contact between the two shapes should be created or not.
- **ContactHitEvent** — An event produced when a pair of PhysicsShape come into contact at relative speed exceeding the PhysicsWorld._contactHitEventThreshold. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. This may be reported for speculative contacts that have a confirmed impulse. See PhysicsWorld._contactHitEvents.
- **JointThresholdEvent** — An event produced by a Joint which exceeds either its PhysicsJoint._forceThreshold or PhysicsJoint._torqueThreshold.
- **PostSimulateEventHandler** — Event handler for a post-simulate event callback. This is called after the simulation has finished running and is always called on the main-thread. See PhysicsWorld and PhysicsEvents.PostSimulate.
- **PreSimulateEventHandler** — Event handler for a pre-simulate event callback. This is called prior to the simulation running and is always called on the main-thread. See PhysicsWorld and PhysicsEvents.PreSimulate.
- **PreSolveEvent** — An event produced when a contact between a pair of PhysicsShape is updated, used to provide the ability to decide if the contact should be disabled or not.
- **TransformChangeEvent** — An event produced after registering via PhysicsWorld.RegisterTransformChange.
- **TransformTweenWriteEvent** — An event produced and sent to the callback target set with PhysicsWorld._transformWriteCallbackTarget which must implement PhysicsCallbacks.ITransformWriteCallback which will have PhysicsCallbacks.ITransformWriteCallback.OnTransformTweenWrite called allowing custom transform writing.
- **TransformWriteEvent** — An event produced and sent to the callback target set with PhysicsWorld._transformWriteCallbackTarget which must implement PhysicsCallbacks.ITransformWriteCallback which will have PhysicsCallbacks.ITransformWriteCallback.OnTransformWrite called allowing custom transform writing.
- **TriggerBeginEvent** — An event produced when a pair of Shapes, one of which was a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. See PhysicsWorld._triggerBeginEvents.
- **TriggerEndEvent** — An event produced when a pair of Shapes, one of which was a trigger, stopped touching. An end event will be produced anything that destroys contacts happens, prior to the last world simulation step, which include things like setting the body transform, destroying a body or shape or changing a contact filter etc. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. See PhysicsWorld._triggerEndEvents.
- **WorldDefinitionChangeEventHandler** — Event handler for a world definition change event callback.
- **WorldDrawResultsEventHandler** — Event handler for a world draw results event callback. This is only called if the world is currently rendering as specified by PhysicsWorld._renderingMode or if PhysicsCoreSettings2D._alwaysDrawWorlds is true. CAUTION: The world is READ locked during this event so ANY write operation on the world will cause an immediate deadlock.

### BodyUpdateEvent

> An event produced by a PhysicsBody that indicates the simulation changed the body in one of the following ways: - The body transform was changed. - The body fell asleep. See PhysicsWorld._bodyUpdateEvents.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `body` | The body this event relates to. |
| `fellAsleep` | Whether the body fell asleep or not. |
| `transform` | The current transform of the body. |

### ContactBeginEvent

> An event produced by a pair of Shapes, neither of which are a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. See PhysicsWorld._contactBeginEvents.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactBeginEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `contactId` | The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with PhysicsShape.ContactId._isValid. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

### ContactEndEvent

> An event produced by a pair of Shapes, neither of which are a trigger, stopped touching. You will get an end event if you do anything that destroys contacts prior to the last world simulation step which include things like setting the body transform, destroying a body etc. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. See PhysicsWorld._contactEndEvents.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactEndEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `contactId` | The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with PhysicsShape.ContactId._isValid. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

### ContactFilterEvent

> An event produced when a pair of PhysicsShape come into contact. This can be used to decide if a contact between the two shapes should be created or not.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactFilterEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `physicsWorld` | The physics world both shapes are within. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

### ContactHitEvent

> An event produced when a pair of PhysicsShape come into contact at relative speed exceeding the PhysicsWorld._contactHitEventThreshold. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. This may be reported for speculative contacts that have a confirmed impulse. See PhysicsWorld._contactHitEvents.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.ContactHitEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `approachSpeed` | The speed the shapes are approaching, typically in meters per second. This value is always positive. |
| `contactId` | The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with PhysicsShape.ContactId._isValid. |
| `normal` | Normal vector that always points in the direction from shape A to shape B. |
| `point` | Point where the shapes hit at the beginning of the time step. This is a mid-point between the two surfaces. It could be at speculative point where the two shapes were not touching at the beginning of the time step. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

### JointThresholdEvent

> An event produced by a Joint which exceeds either its PhysicsJoint._forceThreshold or PhysicsJoint._torqueThreshold.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.JointThresholdEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `joint` | The joint involved in the event. |

### PostSimulateEventHandler

> Event handler for a post-simulate event callback. This is called after the simulation has finished running and is always called on the main-thread. See PhysicsWorld and PhysicsEvents.PostSimulate.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.PostSimulateEventHandler`  

### PreSimulateEventHandler

> Event handler for a pre-simulate event callback. This is called prior to the simulation running and is always called on the main-thread. See PhysicsWorld and PhysicsEvents.PreSimulate.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.PreSimulateEventHandler`  

### PreSolveEvent

> An event produced when a contact between a pair of PhysicsShape is updated, used to provide the ability to decide if the contact should be disabled or not.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.PreSolveEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `normal` | The surface normal at the point of contact. |
| `physicsWorld` | The physics world both shapes are within. |
| `point` | The point of contact. |
| `shapeA` | One of the shapes involved in the event. |
| `shapeB` | The other shape involved in the event. |

### TransformChangeEvent

> An event produced after registering via PhysicsWorld.RegisterTransformChange.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `changeReason` | The reason(s) the transform changed. |
| `transform` | The transform that changed. |

### TransformTweenWriteEvent

> An event produced and sent to the callback target set with PhysicsWorld._transformWriteCallbackTarget which must implement PhysicsCallbacks.ITransformWriteCallback which will have PhysicsCallbacks.ITransformWriteCallback.OnTransformTweenWrite called allowing custom transform writing.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TransformTweenWriteEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `extrapolationTime` | The extrapolation time when the event was created, in the range [0, 1]. |
| `interpolationTime` | The interpolation time when the event was created, in the range [0, 1]. |
| `physicsWorld` | The physics world the event was created from. |
| `transfomPlaneCustom` | The transform plane (custom) of the physics world when the event was created. This maybe not be relevant unless the transform plane is PhysicsWorld.TransformPlane.Custom. |
| `transformPlane` | The transform plane of the physics world when the event was created. |
| `tweens` | The transform write tweens available to be configured. |

### TransformWriteEvent

> An event produced and sent to the callback target set with PhysicsWorld._transformWriteCallbackTarget which must implement PhysicsCallbacks.ITransformWriteCallback which will have PhysicsCallbacks.ITransformWriteCallback.OnTransformWrite called allowing custom transform writing.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TransformWriteEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `physicsWorld` | The physics world the event was created from. |
| `simulationType` | The simulation type of the physics world when the event was created. |
| `transfomPlaneCustom` | The transform plane (custom) of the physics world when the event was created. This maybe not be relevant unless the transform plane is PhysicsWorld.TransformPlane.Custom. |
| `transformPlane` | The transform plane of the physics world when the event was created. |
| `transformTweenMode` | The transform tween mode of the physics world when the event was created. |
| `tweens` | The transform write tweens available to be configured. |

### TriggerBeginEvent

> An event produced when a pair of Shapes, one of which was a trigger, began touching. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. See PhysicsWorld._triggerBeginEvents.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TriggerBeginEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `triggerShape` | The trigger shape involved in the event. |
| `visitorShape` | The shape that began touching the trigger shape. |

### TriggerEndEvent

> An event produced when a pair of Shapes, one of which was a trigger, stopped touching. An end event will be produced anything that destroys contacts happens, prior to the last world simulation step, which include things like setting the body transform, destroying a body or shape or changing a contact filter etc. The shapes provided may have been destroyed so they should always be validated with PhysicsShape._isValid. See PhysicsWorld._triggerEndEvents.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.TriggerEndEvent`  

#### Properties

| Name | Summary |
|------|---------|
| `triggerShape` | The trigger shape involved in the event. |
| `visitorShape` | The shape that stopped touching the trigger shape. |

### WorldDefinitionChangeEventHandler

> Event handler for a world definition change event callback.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.WorldDefinitionChangeEventHandler`  

### WorldDrawResultsEventHandler

> Event handler for a world draw results event callback. This is only called if the world is currently rendering as specified by PhysicsWorld._renderingMode or if PhysicsCoreSettings2D._alwaysDrawWorlds is true. CAUTION: The world is READ locked during this event so ANY write operation on the world will cause an immediate deadlock.

**Full name:** `Unity.U2D.Physics.PhysicsEvents.WorldDrawResultsEventHandler`
