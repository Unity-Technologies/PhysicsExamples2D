---
name: unity-physicscomponents2d-pose
description: The PhysicsPose component of com.unity.2d.physics — the component that manages a PhysicsBody and keeps it synced to the Unity Transform. Covers definition versus definitionAsset, ApplyDefinition and what it resets, the BodyCreated/BodyDestroyed events, the four callback source modes (Off/Pose/Bodies/Events) and their guards, and compositeMode for a body-less pose. Use for questions about creating or configuring a body from a component, why a body property change had no effect, wiring body callbacks in the inspector, or a pose that has no body.
---

# PhysicsPose

One `PhysicsPose` manages one `PhysicsBody`, and keeps it in sync with the Unity Transform.
It is `[DisallowMultipleComponent]` and runs at `PhysicsExecutionOrder.Pose`, after the simulation tier and before areas.

Shapes are not its job: `PhysicsArea` components on the same GameObject attach shapes to its body.

## The Transform owns the pose

**The Unity Transform is the source of truth for position and rotation, always.**
The body follows world and local position and rotation, in edit mode and in play. That tracking set is fixed, not configurable.

This is why `PhysicsPoseDefinition` hides the body definition's position and rotation fields in its inspector: they would never be read. A pose seeds the body's placement from the Transform, overwriting whatever the definition carries.

Reach the live body with the `body` property. It is invalid while the pose is disabled, or when `compositeMode` is set.

## Definition versus definition asset

Two sources of body configuration, one winner:

| Property | Type | Meaning |
|---|---|---|
| `definition` | `PhysicsBodyDefinition` | the local, inline definition |
| `definitionAsset` | `PhysicsPoseDefinition` | an optional shared asset that overrides the local one |
| `activeDefinition` | `PhysicsBodyDefinition` | read-only: the asset's value when assigned, else the local definition |

`activeDefinition` reflects **authored** settings, not live state. For live state read the `body` handle.

`PhysicsPoseDefinition` is in the `Unity.U2D.Physics.Assets` namespace, created from **Assets > Create > 2D > Physics (Core) > Pose > Pose Definition**.

## ApplyDefinition and what it costs

Changing `definition`, `definitionAsset`, `callbackSource` or `callbackTarget` does **nothing** to the running body until you call `ApplyDefinition()`.

It updates the body in place, keeping it and its attached shapes alive. But a body definition writes the full state, so during play it:

- resets linear and angular velocity,
- overwrites anything you changed directly on the `body` handle since the last apply.

So `ApplyDefinition()` is an authoring operation. During play, prefer setting properties on the `body` handle directly. It has no effect when no body exists.

Do not confuse it with `Apply()`, which is the inherited `PhysicsWorldProvider` method for re-resolving *which world* the body lives in. See `unity-physicscomponents2d-providers`.

## Choosing the world

Inherited from `PhysicsWorldProvider`: `source` is a `SimulationSource` (`DefaultWorld` or `SimulationWorld`), paired with the `simulationWorld` asset, and changes need `Apply()`.

Assigning `simulationWorld` while `source` is not `SimulationWorld` throws. See `unity-physicscomponents2d-simulation` for world lifetime.

## Events

| Event | When | Contract |
|---|---|---|
| `BodyCreated` | after the body is created | the handle is valid |
| `BodyDestroyed` | after the body is destroyed | **the handle is invalid**: drop references, do not use it |

When `BodyDestroyed` fires, anything attached to the body (shapes, joints) is already gone via the engine's cascade.

Subscribe with a cached handler delegate rather than a method group, so repeated subscribe/unsubscribe does not allocate.

## Callback modes

`callbackSource` picks how body callbacks are dispatched. Four modes, and the choice determines what your receiving code has to implement:

| Mode | Receiver | Implements |
|---|---|---|
| `Off` | nothing | body registers no callback target |
| `Pose` | `callbackTarget` | `PhysicsPoseCallbacks.IBodyUpdateCallback`, receives the *pose* |
| `Bodies` | `callbackTarget` | the engine `PhysicsCallbacks` interfaces, receives the *body* |
| `Events` | this component | nothing; wire a method in the inspector |

`Pose` mode exists so your code works with the component rather than a raw handle. The event still carries the original engine event as `bodyUpdateEvent`, so body-level data is available when you need it.

A single class can implement both the pose-centric and the engine interface at once: the methods differ by parameter type, so the compiler keeps them apart.

Two guards on `callbackTarget`, both throwing:

- Assigning non-null while `callbackSource` is neither `Bodies` nor `Pose` throws `InvalidOperationException`. Clearing to null is always allowed.
- Assigning **any physics component** throws `ArgumentException`, because those receive callbacks rather than handle them, so yours would never run.

Body-update callbacks also depend on the world: they only fire when the world has `autoBodyUpdateCallbacks` enabled, or callbacks are sent manually. See `unity-physicscore2d-events`.

## compositeMode

Set it and the pose **never creates a body**. Instead it only supplies its areas' geometry to a `PhysicsAreaComposite`'s Pose layer.

It is authoring-only, and enforced as such: settable while there is no live body, but **throws `InvalidOperationException` once a body exists**, because flipping it on a running body would discard transient runtime state (velocity, sleep state, joint attachments) that cannot be restored.

In the editor, changing it rebuilds or tears down the body automatically, so no explicit call is needed there.

A `compositeMode` pose still raises the internal "a pose appeared" notification when enabled, so an `Auto`-source area or constraint elsewhere can still resolve against it.

## Where to go next

For the body API itself (type, mass, velocity, sleep, forces) see `unity-physicscore2d-bodies` and `unity-physicscore2d-forces`.
For attaching shapes see `unity-physicscomponents2d-areas`.
For the shared `source`/`Apply` model see `unity-physicscomponents2d-providers`.
