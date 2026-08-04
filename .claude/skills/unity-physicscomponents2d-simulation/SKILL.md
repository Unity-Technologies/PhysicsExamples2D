---
name: unity-physicscomponents2d-simulation
description: The simulation family of com.unity.2d.physics — the PhysicsSimulation component, the PhysicsSimulationWorld identity asset, PhysicsSimulationDefinition shared settings, world lifetime and reference counting, persistAcrossScenes, and how a component chooses the default world versus a named one. Use for questions about which world a component simulates in, creating or keeping alive a non-default world, multiple worlds in one scene, or when a world unexpectedly does or does not exist.
---

# Simulation family

This family answers one question: **which `PhysicsWorld` exists, and for how long.**

There are three pieces, and the relationship between them is the part worth learning:

| Piece | Kind | Role |
|---|---|---|
| `PhysicsSimulationWorld` | asset | *identity*. Names one world so components can address it. |
| `PhysicsSimulationDefinition` | asset | *settings*. Wraps a `PhysicsWorldDefinition`. |
| `PhysicsSimulation` | component | *lifetime*. Keeps a world alive while enabled. |

Both assets are in the `Unity.U2D.Physics.Assets` namespace. Create them from **Assets > Create > 2D > Physics (Core) > Simulation**.

## Identity is separate from settings

A `PhysicsSimulationWorld` asset *is* the world's name, and holds a reference to a `PhysicsSimulationDefinition` for its configuration.

That split is deliberate: **two worlds can share one definition while remaining distinct worlds.** Reuse the settings, keep the identities separate.

When a `PhysicsSimulationWorld` has no definition assigned, it uses `PhysicsWorldDefinition.defaultDefinition`. Its `activeDefinition` property resolves that for you.

## Worlds are reference counted, not owned by a component

This is the point most likely to surprise you: **nothing references a `PhysicsSimulation` component to reach its world.**

A world is created on demand by whatever references its asset, and destroyed when the last referencing owner releases it. A `PhysicsPose` set to a `SimulationWorld` asset brings that world to life by itself, with no `PhysicsSimulation` anywhere in the scene.

So what is `PhysicsSimulation` for? Keeping a world alive *independently of whether anything is currently in it*. Without it, a world blinks out as soon as its last pose is disabled. With it, the world persists as long as the component is enabled.

Several components may reference one asset. They share the single world that asset identifies, and it lives until the last of them releases it.

## Reaching the live world

| From | Property |
|---|---|
| `PhysicsSimulation` | `world` — the live world, or an invalid world when disabled or no asset is assigned |
| `PhysicsPose` (or any `PhysicsWorldProvider`) | `resolvedWorld` |

Both return an invalid `PhysicsWorld` rather than throwing, so check `isValid`.

## Choosing a world on a component

A `PhysicsPose` picks its world through the `PhysicsWorldProvider` base, not through this family:

- `source = SimulationSource.DefaultWorld` uses the default world and ignores the asset reference.
- `source = SimulationSource.SimulationWorld` uses the assigned `simulationWorld` asset, creating that world on demand and releasing it on disable.

Assigning `simulationWorld` while `source` is not `SimulationWorld` throws. Set the enum first. See `unity-physicscomponents2d-providers`.

## `persistAcrossScenes`

Off by default, giving the world a scene-scoped lifetime. Set it and the world's lifetime spans every scene load.

Three constraints, all enforced rather than documented-and-hoped:

- **Root GameObjects only.** Unity only allows a root object to persist, so it does nothing on a parented object.
- It affects the **whole GameObject**, not just this component. Everything else on that object persists too.
- Reparenting the object afterwards ends persistence, and the flag is **cleared back to false** when that happens.

## Editing the assets at runtime

A plain `ScriptableObject` has no change notification, so a live world would never notice an edit to the asset it was built from. `PhysicsSimulationWorld` fixes that: swapping its `definition` or changing its `userData` raises an internal change event, and the live world re-reads the asset.

That works from code as well as from the inspector, because the property setters raise it too. So assigning `simulationWorld.definition = someOtherDefinition` reaches the live world without any explicit apply call.

`PhysicsSimulation.simulationWorld` behaves the same way: assigning a different asset while enabled releases the previous world and acquires the new one immediately, with no `Apply()` needed. This family is the exception; every other component family requires an explicit apply.

## Other notes

- `[DisallowMultipleComponent]`, and it runs first of all the physics components (`PhysicsExecutionOrder.Simulation`).
- With no asset assigned, the component owns nothing and has no effect. It is not an error.
- For what you can configure *in* a world (gravity, iterations, event grouping, transform plane), that is `PhysicsWorldDefinition`: see `unity-physicscore2d-world-api` and `unity-physicscore2d-settings`.
