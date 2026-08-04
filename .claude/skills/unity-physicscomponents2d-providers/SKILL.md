---
name: unity-physicscomponents2d-providers
description: The shared behavior behind every com.unity.2d.physics component — the PhysicsWorldProvider and PhysicsPoseProvider base classes, the two different `source` properties, which Apply method to call after changing what (Apply vs ApplyGeometry vs ApplyDefinition), rebuild timing and WORM safety, undo, play-mode edits, and the recipe for editing components in bulk from an editor script. Use this whenever you need to change a PhysicsSimulation, PhysicsPose, PhysicsArea or PhysicsConstraint from code, or when a change appears to have no effect on the live physics.
---

# Component base classes and applying changes

Read this before scripting any edit to a package component.
The properties you need usually live on a base class, and setting a property is not enough on its own: almost nothing takes effect until you call the right `Apply` method.

## `source` means two different things

Both provider bases expose a property called `source`, with a different enum on each.
Getting this wrong is the most common mistake, because the property name gives no hint.

| Component | Base | `source` enum | Values | Paired reference |
|---|---|---|---|---|
| `PhysicsPose` | `PhysicsWorldProvider` | `SimulationSource` | `DefaultWorld`, `SimulationWorld` | `simulationWorld` |
| `PhysicsArea*` | `PhysicsPoseProvider` | `PoseSource` | `Auto`, `Custom` | `pose` |
| `PhysicsConstraint*` | its own | `PoseSource` | `Auto`, `Custom` | `poseA`, `poseB` |
| `PhysicsSimulation` | none | no `source` | | `simulationWorld` |

So "which world does this use" is a `PhysicsPose` question, and "which body does this attach to" is an area or constraint question.

`PhysicsSimulation` is the exception with no `source` at all: it only holds a `simulationWorld` asset reference, and assigning it takes effect immediately while enabled.

`Auto` searches this GameObject and its parents for a `PhysicsPose`; `Custom` uses the explicit reference.
An unresolved `Auto` does not stay dead: it subscribes to `PhysicsPose.PoseCreated` and resolves itself when a suitable pose appears.

### Guard on the paired reference

Assigning `simulationWorld` on a `PhysicsPose` while `source` is not `SimulationWorld` **throws**. Clearing it to `null` is always allowed.
Set `source` first, then the reference.

## Which Apply do I call?

Three separate methods, and they are not interchangeable.

| Method | On | What it re-resolves | Cost |
|---|---|---|---|
| `Apply()` | providers, constraints | the world or pose reference | rebuilds only if the resolved target changed |
| `ApplyGeometry()` | `PhysicsArea` | this area's geometry | in-place shape update for a single-shape primitive, otherwise a shape rebuild |
| `ApplyDefinition()` | `PhysicsPose`, `PhysicsArea`, `PhysicsConstraint` | the definition or definition asset | in place, keeps the object alive |

Pick by what you changed:

- Changed `source`, `simulationWorld`, `pose`, `poseA`/`poseB` → `Apply()`
- Changed geometry on an area → `ApplyGeometry()`
- Changed a definition or definition asset → `ApplyDefinition()`

`Apply()` is deliberately cheap when nothing moved: `PhysicsWorldProvider` and `PhysicsPoseProvider` compare the newly resolved target against the one the current physics was built on, and skip the rebuild when they match, so dependents survive untouched.

A constraint is the exception: a joint bakes both bodies at creation and cannot have them reassigned, so `PhysicsConstraint.Apply()` always destroys and recreates the joint.

`PhysicsPose.ApplyDefinition()` writes the full body state, so calling it during play resets linear and angular velocity and overwrites anything you set directly on the `body` handle since the last apply.

## Rebuild timing and WORM safety

Creating and destroying physics objects is illegal while the world is stepping.
The bases handle that for you through two protected methods:

- `Rebuild()` tears down and rebuilds immediately. Only safe when you know no step is running.
- `RequestRebuild()` rebuilds now in edit mode, and in play mode defers to the next `PhysicsEvents.PostSimulate`, coalescing repeated requests into one rebuild. Safe from anywhere, including inside a physics callback.

Prefer `RequestRebuild()` from your own component code.

## Undo does not re-run OnEnable

Undo restores a component's serialized data without re-running `OnEnable`, so the live physics would be gone or stale.
Every component implements `IPhysicsRebuildable.RebuildAfterUndo()`, and the editor's undo handler calls them in `PhysicsExecutionOrder` order so worlds rebuild before bodies before shapes before joints.

If you override `NeedsUndoRebuild()` to return false, you are asserting your live object is unaffected by the undo.

## Play-mode edits are off by default

Inspector edits, `OnValidate`'s auto-apply, and the scene geometry tools refuse to change components while the Editor is playing unless the user opts in.
Components check this before reconciling in `OnValidate`.

This only affects authoring paths. Calling `Apply()`, `ApplyGeometry()` or `ApplyDefinition()` directly from a script always works, in play mode included, which is why the runtime API is the right tool during play.

## Recipe: change a property on every component in a scene

This is the pattern for a bulk edit from an editor script or an evaluated snippet. It covers the four things that are easy to miss: inactive objects, undo, applying, and marking the scene dirty.

```csharp
var poses = Object.FindObjectsByType<PhysicsPose>(FindObjectsInactive.Include);

foreach (var pose in poses)
{
    if (pose.source == PhysicsWorldProvider.SimulationSource.DefaultWorld && pose.simulationWorld == null)
        continue;

    Undo.RecordObject(pose, "Use Default World");

    pose.source = PhysicsWorldProvider.SimulationSource.DefaultWorld;
    pose.simulationWorld = null;

    pose.Apply();
    EditorUtility.SetDirty(pose);
}
```

Points worth keeping:

- `FindObjectsInactive.Include` or you silently skip disabled objects. Never pass a `FindObjectsSortMode` argument.
- Skip components already in the target state, so the undo entry and the dirty flag reflect real changes only.
- One `Undo.RecordObject` per object gives the user a single undo step for the whole batch.
- `Apply()` is what makes the live physics match; without it you have only changed serialized data.
- `SetDirty` so the scene is marked unsaved.
- Clear the paired reference as well as setting the enum. The components' own `Reset()` does both, which is the behavior to mirror.

## Lifecycle override points

When deriving a base, these are the methods available. The naming distinguishes once-per-enable setup from per-build work.

| Override | Runs | Use for |
|---|---|---|
| `OnCreatePhysics` | every build | create your physics objects |
| `OnDestroyPhysics` | every teardown | drop handles; the owning object may already be gone |
| `OnProviderEnable` / `OnProviderDisable` | once per enable/disable, not on undo | native storage, transform-change registration |
| `OnBeforeCreatePhysics` / `OnAfterDestroyPhysics` | every build/teardown, undo included | subscriptions |
| `OnProviderEnableComplete` | end of enable, after the first build | announce state that needs resolution to be done |
| `OnProviderReset` | editor Reset | your own serialized fields only; the base resets its own |

The engine cascades a body's shapes and joints away when the body is destroyed, so `OnDestroyPhysics` must not assume the owning object is still valid. Drop handles or check `isValid`.

## Where to go next

Anything about the objects these components produce is engine API, not package: see `unity-physicscore2d` and its sub-skills.
For orientation, the family list and the definition-asset table, see `unity-physicscomponents2d`.
