# Sandbox → Toolbox conversion status

Build scripts must never replace the open scene: open the example scene with `OpenSceneMode.Additive`, save it, then `CloseScene(scene, removeScene: true)`.
Opening in `Single` mode from script silently discards unsaved changes in `Assets/Toolbox.unity`, such as the Start Example.

The Toolbox exists to show components: author everything with the natural component for it (Pose, Area, Constraint), and script only what has no component (input, spawning counts, live control changes).
Match what the Sandbox demonstrates, not raw-API internals; never contort components to copy engine-level details such as open-chain ghost vertices.
Only raise something before starting when no component can do it at all.

The Toolbox has no shape colour feature by design: ignore `ShapeColor`, `SetOverrideColorShapeState` and any `customColor` in the Sandbox, and never raise colours as a question.

The Shapes category is complete except CardHouse, held back pending the hull-tolerance fix. Next up: Batching, Benchmarks, Collision and the other remaining categories.
Any change to an example's values goes into both the Sandbox and the Toolbox so they stay the same.
A polygon area builds nothing when `PolygonGeometry.isValid` is false, which includes any edge shorter than the hull weld distance (4 × linearSlop, 0.02); check thin polygons and report rather than working around it.
Check every engine member against the 6000.7 scripting reference (create-2d-physics skill) before using it; an obsolete member stalls the editor on the Script Updating Consent dialog. `PhysicsRotate.angle` is obsolete: use `degrees` or `radians`.
Engine defaults the Sandbox relies on: `PhysicsChainDefinition.isLoop` is true (use a Contour with Segments output), `PhysicsDistanceJointDefinition.autoDistance` is true, hinge anchors default to zero; read other defaults from the editor rather than assuming.

- [x] Barrel
- [x] Capacity
- [x] Arch
- [x] BallAndChain
- [ ] Boids
- [ ] BounceHouse
- [ ] BounceRagdolls
- [x] Bounciness
- [x] Buoyancy
- [ ] CardHouse (removed from the Toolbox until the hull tolerance is resolved; 0.002 cards fail PolygonGeometry.isValid)
- [x] ChainShape
- [x] ChainShapeDeform
- [ ] CharacterMover
- [x] Compound
- [x] Confined
- [ ] ContactManifold
- [x] ConveyorBelt
- [x] CustomFilter
- [x] DistanceJoint
- [x] Doohickey
- [x] DoubleDomino
- [ ] Drawing
- [x] Driving
- [x] EllipsePolygons
- [x] Fragmenting
- [x] Friction
- [x] Funnel
- [x] GearLift
- [x] GeometryIslands
- [x] IgnoreJoint
- [x] JointGrid
- [x] LargeCompound
- [x] LargeKinematic
- [x] LargePyramid
- [ ] LargeWorld
- [ ] ManyTumblers
- [x] ModifyGeometry
- [ ] Queries
- [x] RollingResistance
- [x] RoundedPolygons
- [x] ScaleRagdoll
- [x] ScissorLift
- [x] ShapeStack
- [ ] Shooter
- [x] Slicing
- [x] SliderJoint
- [ ] Smash
- [x] SoftBody
- [ ] Spinner
- [x] SpriteDestruction
- [x] TopDownFriction
- [ ] Triggers
- [ ] Tumbler
- [x] UserJoint
- [ ] Washer
- [x] WheelJoint
- [x] Wind
