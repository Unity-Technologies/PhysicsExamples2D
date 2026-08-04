---
name: unity-physicscomponents2d
description: Orientation and routing guide for the com.unity.2d.physics package (namespace Unity.U2D.Physics) — the authoring components layered over the PhysicsCore2D engine API. Covers the four component families (simulation, pose, area, constraint), the type hierarchy, script execution order, definition assets, and when to use components versus the raw API. Use this FIRST for any question about PhysicsSimulation, PhysicsPose, PhysicsArea*, PhysicsConstraint*, their inspectors, or editing them from a script. For the physics objects the components produce (worlds, bodies, shapes, joints, queries, events) use the unity-physicscore2d* skills instead.
---

# 2D Physics Core components

The `com.unity.2d.physics` package layers authoring components over the PhysicsCore2D engine API.
Both live in the `Unity.U2D.Physics` namespace, so a type name alone does not tell you which layer you are in.

The split that matters: **components handle construction and lifetime, the engine API handles behavior.**
A component builds a world, body, shape or joint from serialized fields and keeps it alive.
Everything you then do with that object (queries, forces, events, batching) is engine API, covered by the `unity-physicscore2d*` skills.

## Which layer am I in?

| You are working with | Layer | Skill |
|---|---|---|
| A component on a GameObject, its inspector fields, its lifetime | Package | this set |
| `PhysicsWorld`, `PhysicsBody`, `PhysicsShape`, `PhysicsJoint` handles | Engine | `unity-physicscore2d*` |
| Casting, overlapping, distance tests | Engine | `unity-physicscore2d-queries` |
| Forces, impulses, wind, buoyancy | Engine | `unity-physicscore2d-forces` |
| Contact and trigger callbacks | Engine | `unity-physicscore2d-events` |
| Building physics with no components at all | Engine | `unity-physicscore2d` |

Every component exposes the object it built, so you cross from one layer to the other through a property:
`PhysicsSimulation.world`, `PhysicsPose.body`, `PhysicsArea.shape` (or index it for many), `PhysicsConstraint.joint`.

## The four families

Listed in build order, which is also script execution order: a world must exist before a body, a body before its shapes, and both bodies before a joint.

| Family | Component | Builds | Base |
|---|---|---|---|
| Simulation | `PhysicsSimulation` | keeps a `PhysicsWorld` alive | `MonoBehaviour` |
| Pose | `PhysicsPose` | a `PhysicsBody` | `PhysicsWorldProvider` |
| Area | `PhysicsArea*` (9 of them) | one or more `PhysicsShape` | `PhysicsPoseProvider` |
| Constraint | `PhysicsConstraint*` (7 of them) | a `PhysicsJoint` | `MonoBehaviour` |

Areas: `PhysicsAreaPrimitive`, `PhysicsAreaCircle`, `PhysicsAreaCapsule`, `PhysicsAreaPolygon`, `PhysicsAreaSegment`, `PhysicsAreaContour`, `PhysicsAreaSprite`, `PhysicsAreaPath`, `PhysicsAreaComposite`.

Constraints: `PhysicsConstraintDistance`, `PhysicsConstraintFixed`, `PhysicsConstraintHinge`, `PhysicsConstraintIgnore`, `PhysicsConstraintRelative`, `PhysicsConstraintSlider`, `PhysicsConstraintWheel`.

All of them appear under **Physics 2D (Core)** in the Add Component menu.

## Type hierarchy

Read this before touching a component from a script: the base class decides which properties exist, and two of the bases spell a property the same way while meaning different things.

```
MonoBehaviour
├── PhysicsSimulation                        (no source property at all)
├── PhysicsWorldProvider                     source = SimulationSource
│   └── PhysicsPose
├── PhysicsPoseProvider                      source = PoseSource
│   └── PhysicsArea (abstract)
│       ├── PhysicsAreaPrimitiveBase → Primitive, Circle, Capsule, Polygon, Segment
│       ├── PhysicsAreaContourBase   → Contour, Sprite
│       ├── PhysicsAreaPath
│       └── PhysicsAreaComposite
└── PhysicsConstraint (abstract)             source = PoseSource, its own, not inherited
    └── Distance, Fixed, Hinge, Ignore, Relative, Slider, Wheel
```

`PhysicsConstraint` deliberately does not derive from `PhysicsPoseProvider`, because it needs two poses rather than one.
It declares its own `PoseSource` enum, so `PhysicsArea.PoseSource` and `PhysicsConstraint.PoseSource` are separate types with the same shape.

See `unity-physicscomponents2d-providers` for what the bases actually do. That is the skill to read before scripting any component edit.

## The rest of this set

| Skill | Covers |
|---|---|
| `unity-physicscomponents2d-providers` | the base classes, `source`, the three Apply methods, rebuild timing, bulk edits |
| `unity-physicscomponents2d-simulation` | `PhysicsSimulation`, world identity assets, world lifetime |
| `unity-physicscomponents2d-pose` | `PhysicsPose`, body definitions, callbacks, composite mode |
| `unity-physicscomponents2d-areas` | the nine area components, geometry, contours, sprites, composites |
| `unity-physicscomponents2d-constraints` | the seven constraint components, two-pose resolution, anchors |

Preferences and project settings are a thin editor-internal surface (a Project Settings page under **Physics 2D (Core)** carrying component defaults and default callback modes), so they have no skill of their own.

## Two namespaces

Components live in `Unity.U2D.Physics`.
**Every asset type lives in `Unity.U2D.Physics.Assets`**, so scripting against a definition, geometry, surface material or contact filter asset needs a second using directive.

## Definition assets

Most components can take their configuration inline or from a shared `ScriptableObject`.
Every such asset derives `PhysicsAssetBase<T>` wrapping one engine definition struct in a public `value` field, and raises a `Changed` event that live components subscribe to.

| Asset | Wraps |
|---|---|
| `PhysicsSimulationDefinition` | `PhysicsWorldDefinition` |
| `PhysicsPoseDefinition` | `PhysicsBodyDefinition` |
| `PhysicsAreaDefinition` | `PhysicsShapeDefinition` |
| `PhysicsConstraint*Definition` (7) | the matching `Physics*JointDefinition` |
| `PhysicsSurfaceMaterial` | `PhysicsShape.SurfaceMaterial` |
| `PhysicsContactFilter` | `PhysicsShape.ContactFilter` |
| `Physics*Geometry` (8) | the matching geometry struct |

Where a component has both, the asset wins when assigned, and the component exposes the winner as `activeDefinition`.

## Script execution order

`PhysicsExecutionOrder` holds the `[DefaultExecutionOrder]` values, and is also what the editor's undo handler reads to rebuild components in dependency order.

`First`, `Simulation`, `AfterSimulation`, `Pose`, `AfterPose`, `Area`, `AfterArea`, `Constraint`, `AfterConstraint`, `Last`.

Use an `After*` constant for your own component that must run just after a tier without being that kind of component.
Use `First` or `Last` for a manager that must run before any physics is set up or after all of it is.

## Extending the package

To add your own component, derive from a package base rather than hand-rolling a `MonoBehaviour` around the engine API.
The base gives you a guaranteed create/destroy pairing, world or pose resolution, undo rebuild, and WORM-safe deferred rebuilds.

- A custom shape source derives `PhysicsArea` and implements `GenerateShapes`.
- A component that builds several objects at once (bodies plus shapes plus joints) derives `PhysicsWorldProvider`.

`PhysicsWorldProvider` and `PhysicsPoseProvider` have internal constructors, so an external assembly cannot derive them directly; derive `PhysicsArea` instead.
Worked examples live in the `com.unity.2d.physics.extras` package in the `PhysicsExamples2D` repo.
