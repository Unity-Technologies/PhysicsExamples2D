---
name: unity-physicscomponents2d-constraints
description: The constraint family of com.unity.2d.physics — the seven PhysicsConstraint components that create a PhysicsJoint between two poses. Covers poseA and poseB resolution, why Apply always recreates the joint, ApplyDefinition for in-place definition changes, the anchor model including auto anchors, the callback modes and their guards, and hasAnchors for the ignore constraint. Use for questions about joining two bodies with components, a joint that will not appear, changing joint settings at runtime, or wiring joint break/threshold callbacks.
---

# Constraint family

A constraint creates **one `PhysicsJoint`** between the bodies of two `PhysicsPose` components.
It runs last of the physics tiers, at `PhysicsExecutionOrder.Constraint`, since both bodies must exist first.

## The seven components

`PhysicsConstraintDistance`, `PhysicsConstraintFixed`, `PhysicsConstraintHinge`, `PhysicsConstraintIgnore`, `PhysicsConstraintRelative`, `PhysicsConstraintSlider`, `PhysicsConstraintWheel`.

Each pairs with a matching definition asset (`PhysicsConstraint*Definition`) wrapping the engine's `Physics*JointDefinition`.

## Not a PoseProvider

`PhysicsConstraint` derives straight from `MonoBehaviour`, **not** from `PhysicsPoseProvider`, because it needs two poses rather than one. It declares its own `PoseSource` enum, so `PhysicsConstraint.PoseSource` and `PhysicsArea.PoseSource` are distinct types that happen to share a shape.

The asymmetry between the two sides is the thing to remember:

| | Resolution |
|---|---|
| `poseA` | `source = Auto` searches this GameObject and its parents, or `Custom` uses the reference |
| `poseB` | **always explicit.** There is no automatic way to find the far body. |

An unresolved `Auto` `poseA` waits for a `PhysicsPose` to appear anywhere and re-resolves itself. `poseB` never waits, because it has no search to retry.

Read the results with `resolvedPoseA` and `resolvedPoseB`. Reach the joint with `joint`, and test with `hasJoint`.

## Apply always recreates the joint

Unlike an area, whose shapes can be re-parented onto a new body, **a joint bakes both bodies at creation and cannot have them reassigned.**

So `Apply()` on a constraint is always a destroy-and-recreate, not the cheap no-op it is on the provider bases. Call it after changing `source`, `poseA` or `poseB`, and expect the joint to be new afterwards.

`ApplyDefinition()` is the in-place path: it writes the resolved definition onto the live joint without recreating it, and never touches the bodies. Use it for `definition`, `definitionAsset`, `callbackSource` and `callbackTarget` changes. It is a no-op when no joint exists.

| Changed | Call | Joint survives? |
|---|---|---|
| `source`, `poseA`, `poseB` | `Apply()` | no, recreated |
| `definition`, `definitionAsset`, callbacks | `ApplyDefinition()` | yes |

## Definition and asset

The usual trio, declared on each concrete constraint against its typed definition: `definition`, `definitionAsset`, and read-only `activeDefinition` (the asset when assigned, else the local one).

Assets are in `Unity.U2D.Physics.Assets`.

## Anchors

`hasAnchors` is true for every constraint except `PhysicsConstraintIgnore`, which only disables collision between two bodies and has no anchors at all. Check it before offering anchor editing in your own tooling.

Anchors are part of the joint definition, so changing one needs `ApplyDefinition()`.

Two caveats:

- **Auto-anchored sides ignore your anchor value.** The engine's `autoAnchorA`/`autoAnchorB` recompute that side from the bodies' placement at build time, so writing a local anchor for an auto side has no effect.
- The scene anchor tool only edits the **local** definition. When a definition asset is driving the constraint, the anchors live in the shared asset, so the tool leaves it alone rather than silently editing one constraint's worth of a shared value.

`PhysicsConstraintDistance` also has an auto rest distance. Because auto values bake from body placement, a connected body moving in edit mode makes them stale, and the constraint rebuilds to refresh them.

## Callback modes

`callbackSource` mirrors the pose's model, for joint threshold events:

| Mode | Receiver | Implements |
|---|---|---|
| `Off` | nothing | no target registered, so nothing is delivered regardless of the joint's force and torque thresholds |
| `Constraint` | `callbackTarget` | `PhysicsConstraintCallbacks.IJointThresholdCallback`, receives the *constraint* |
| `Joints` | `callbackTarget` | the engine `PhysicsCallbacks` interfaces, receives the *joint* |
| `Events` | this component | nothing; wire a method in the inspector |

The constraint-centric event carries both the resolved `constraint` and the original engine `thresholdEvent`, so joint-level data stays available. A class can implement both the constraint-centric and the engine interface at once, since the methods differ by parameter type.

Changing `callbackSource` or `callbackTarget` needs `ApplyDefinition()`.

The same guards as the pose apply to `callbackTarget`: assigning non-null while the mode uses neither explicit-target mode throws, and assigning any physics component throws, because those receive callbacks rather than handle them.

## Events

`JointCreated` and `JointDestroyed`, both `Action<PhysicsConstraint>`. Subscribe with a cached handler delegate rather than a method group.

## Extending

An out-of-assembly subclass can override `definitionAssetBase` so the shared editor and scene tools find its asset slot. A constraint whose joint type is not yet wired up creates nothing and shows a not-implemented notice in its inspector rather than failing.

## Where to go next

For choosing a joint type, tuning motors, limits, springs and break thresholds, that is the engine layer: `unity-physicscore2d-joints`.
For the shared component model see `unity-physicscomponents2d-providers`.
