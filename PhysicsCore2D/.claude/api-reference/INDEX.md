# PhysicsCore2D API Reference — Index

_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._

All `Unity.U2D.Physics` top-level types, grouped by cluster file.

## [Bodies](bodies.md)

| Type | Summary |
|------|---------|
| [`PhysicsBody`](bodies.md#physicsbody) | A body is contained within a world and has 3 degrees-of-freedom, two for position and one for rotation. A body can have forces, torques and impulses applied to it. A body has three distinct types: - Static: This type of body does not move under simulation and behaves as if it has infinite mass, essentially an immovable object. Static bodies never interact with other Static or Kinematic bodies. - Dynamic: This type of body is fully simulated and moves according to forces and torques applied to its linearangular velocities. It can interact with all other body types. It always has finite, non-zero mass. - Kinematic: This type of body moves under simulation and moves according to its linearangular velocities and never uses forces or torques. It only interacts with Dynamic body types. It behaves as if it has infinite mass. A body is automatically destroyed when the world it is in is destroyed. A body cannot exist outside a world. |
| [`PhysicsBodyDefinition`](bodies.md#physicsbodydefinition) | A PhysicsBody definition used to specify important initial properties. |

## [Chains](chains.md)

| Type | Summary |
|------|---------|
| [`PhysicsChain`](chains.md#physicschain) | A dedicated shape that produces a chain of shapes connected together to produce a continuous surface. Chain shapes provide a smooth, continuous surface that will not produce "ghost" collisions. A PhysicsChain is automatically destroyed when the body it is in is destroyed. A PhysicsChain cannot exist unattached from a body. This will produce shapes of type PhysicsShape.ShapeType.ChainSegment. - Chains are one-sided. - A chain has no mass and should be used on static bodies. - A chain can have a counter-clockwise winding order (normal points right of segment direction). - A chain is either a loop or open. - A chain must have at least 4 points. - The distance between any two points must be greater than PhysicsWorld._linearSlop. - A chain should not self intersect (this is not validated). - An open chain has no collision on the first and final edge. - You may overlap two open chains on their first three and/or last three points to get smooth collision. |
| [`PhysicsChainDefinition`](chains.md#physicschaindefinition) | A PhysicsChain definition used to specify the chain of vertices that will produce multiple ChainSegmentGeometry shape types. Additionally, non-geometric properties can be specified here. |

## [Composer](composer.md)

| Type | Summary |
|------|---------|
| [`PhysicsComposer`](composer.md#physicscomposer) | Provides the ability to compose geometry using specific operations on layers in a specific order. |

## [Destructor](destructor.md)

| Type | Summary |
|------|---------|
| [`PhysicsDestructor`](destructor.md#physicsdestructor) | Provides the ability to destruct (fragment and Slice) geometry. |

## [Events & Callbacks](events.md)

| Type | Summary |
|------|---------|
| [`PhysicsCallbacks`](events.md#physicscallbacks) | All callback interfaces and targets. |
| [`PhysicsEvents`](events.md#physicsevents) | Various events that can be retrieved during and after the simulation has completed. See PhysicsWorld.Simulate and PhysicsWorld.Simulate. |

## [Geometry Primitives](geometry.md)

| Type | Summary |
|------|---------|
| [`CapsuleGeometry`](geometry.md#capsulegeometry) | The geometry of a closed capsule which can be viewed as two semi-circles connected by a rectangle. See PhysicsBody.CreateShape. |
| [`ChainGeometry`](geometry.md#chaingeometry) | The geometry of a chain of ChainSegment. |
| [`ChainSegmentGeometry`](geometry.md#chainsegmentgeometry) | The geometry of a chain line segment with one-sided collision which only collides on the "right" side. Several of these are generated for a chain, connected as ghost1 -> point1 -> point2 -> ghost2. |
| [`CircleGeometry`](geometry.md#circlegeometry) | The geometry of a closed circle. See PhysicsBody.CreateShape. |
| [`PhysicsAABB`](geometry.md#physicsaabb) | Represents a 2D axis-aligned bounding-box. |
| [`PhysicsPlane`](geometry.md#physicsplane) | Represents a 2D plane. |
| [`PolygonGeometry`](geometry.md#polygongeometry) | The geometry of a closed convex polygon. The geometry has a fixed maximum number of vertices as defined by the constant PhysicsConstants.MaxPolygonVertices. Polygon regions that require a larger quantity of vertices or are concave are defined by multiple polygon geometry using the PhysicsComposer or the PolygonGeometry.CreatePolygons utility. See PhysicsBody.CreateShape. |
| [`SegmentGeometry`](geometry.md#segmentgeometry) | The geometry of a line segment. See PhysicsBody.CreateShape. |

## [Helpers](helpers.md)

| Type | Summary |
|------|---------|
| [`PhysicsRotate`](helpers.md#physicsrotate) | Represents a 2D rotation. |
| [`PhysicsTransform`](helpers.md#physicstransform) | Represents a 2D transformation combining a translation and a PhysicsRotate. |
| [`PhysicsUserData`](helpers.md#physicsuserdata) | Custom user data. The physics system doesn't use this data, it is entirely for custom use. |

## [Joints](joints.md)

| Type | Summary |
|------|---------|
| [`PhysicsDistanceJoint`](joints.md#physicsdistancejoint) | Connects an anchor point on body A with an anchor point on body B via a line segment of a specified distance. |
| [`PhysicsDistanceJointDefinition`](joints.md#physicsdistancejointdefinition) | A joint definition used to specify properties when creating a PhysicsDistanceJoint. |
| [`PhysicsFixedJoint`](joints.md#physicsfixedjoint) | A joint to constrain a pair of bodies together rigidly. This constraint provides springs to mimic soft-body simulation. The approximate solver cannot always hold many bodies together completely rigidly. |
| [`PhysicsFixedJointDefinition`](joints.md#physicsfixedjointdefinition) | A joint definition used to specify properties when creating a PhysicsFixedJoint. |
| [`PhysicsHingeJoint`](joints.md#physicshingejoint) | A joint where an anchor point on body B is fixed to an anchor point on body A. This joint allows relative rotation. |
| [`PhysicsHingeJointDefinition`](joints.md#physicshingejointdefinition) | A joint definition used to specify properties when creating a PhysicsHingeJoint. |
| [`PhysicsIgnoreJoint`](joints.md#physicsignorejoint) | A joint used to ignore collision between two specific bodies. As a side effect of being a joint, it also keeps the two bodies in the same simulation island meaning they'll wake/sleep at the same time and be solved together on the same thread. |
| [`PhysicsIgnoreJointDefinition`](joints.md#physicsignorejointdefinition) | A joint definition used to specify properties when creating a PhysicsIgnoreJoint. |
| [`PhysicsJoint`](joints.md#physicsjoint) | A joint is used to constrain bodies to the world or to each other in various ways. A joint is automatically destroyed when either body it is attached to is destroyed. A joint cannot exist unattached from a body. |
| [`PhysicsRelativeJoint`](joints.md#physicsrelativejoint) | A joint constraint used to control the relative movement two bodies while still being responsive to collisions. A spring controls the position and rotation and velocity control allows for simulated friction such as seen in top-down games. A typical usage is to control the movement of a dynamic body with respect to the ground. |
| [`PhysicsRelativeJointDefinition`](joints.md#physicsrelativejointdefinition) | A joint definition used to specify properties when creating a PhysicsRelativeJoint. |
| [`PhysicsSliderJoint`](joints.md#physicssliderjoint) | A joint that requires defining a line of motion defined by the local anchor A. Body B may slide along the axis defined by the local anchor A. Body B cannot rotate relative to body A. The joint translation is zero when the local anchor origins coincide in world space. The joint uses local anchors so that the initial configuration can violate the constraint slightly. |
| [`PhysicsSliderJointDefinition`](joints.md#physicssliderjointdefinition) | A joint definition used to specify properties when creating a PhysicsSliderJoint. |
| [`PhysicsWheelJoint`](joints.md#physicswheeljoint) | A joint that requires defining a line of motion using an axis and an anchor point. The joint translation is zero when the local anchors coincide in world space. The joint uses local anchors so that the initial configuration can violate the constraint slightly. |
| [`PhysicsWheelJointDefinition`](joints.md#physicswheeljointdefinition) | A joint definition used to specify properties when creating a PhysicsWheelJoint. |

## [Layers & Masks](layers.md)

| Type | Summary |
|------|---------|
| [`PhysicsLayers`](layers.md#physicslayers) | This provides a common method to retrieving layer information. If a PhysicsCoreSettings2D asset is assigned then the full layers (PhysicsCoreSettings2D._physicsLayerNames) will be used if PhysicsCoreSettings2D._usePhysicsLayers is also active. If no PhysicsCoreSettings2D asset is assigned then the global layers (See LayerMask) will be used. |
| [`PhysicsMask`](layers.md#physicsmask) | A 64-bit mask, effectively 64 flags. The default enumerator will iterate all the bits that are set (1). |

## [Math Utilities](math.md)

| Type | Summary |
|------|---------|
| [`PhysicsMath`](math.md#physicsmath) | A set of mathematical operations that are useful for physics. These operations do not form a fully comprehensive mathematics library, they simply provide operations that are usually required when interacting with physics. |

## [Queries](queries.md)

| Type | Summary |
|------|---------|
| [`PhysicsQuery`](queries.md#physicsquery) | Various physics queries. |

## [Shapes](shapes.md)

| Type | Summary |
|------|---------|
| [`PhysicsShape`](shapes.md#physicsshape) | A shape is attached to a body and defines an area to which two distinct types of behaviour are handled: - Collision: Contacts between shapes produce a collision response on their respective bodies, assuming their body type is Dynamic. - Trigger: Contacts between shapes do not produce a collision response, only the fact that they're overlapping is reported. An unlimited number of shapes can be attached to a single body, known as a compound body. A shape is automatically destroyed when the body it is attached to is destroyed. A shape cannot exist unattached from a body. |
| [`PhysicsShapeDefinition`](shapes.md#physicsshapedefinition) | A PhysicsShape definition used to specify important initial properties. |

## [World & Simulation](world.md)

| Type | Summary |
|------|---------|
| [`PhysicsConstants`](world.md#physicsconstants) | Contacts used by physics. |
| [`PhysicsCoreSettings2D`](world.md#physicscoresettings2d) | PhysicsCore Settings Asset. This contains all the global physics options along with the default values for the following definitions: - PhysicsWorldDefinition - PhysicsBodyDefinition - PhysicsShapeDefinition - PhysicsChainDefinition - PhysicsDistanceJointDefinition - PhysicsFixedJointDefinition - PhysicsHingeJointDefinition - PhysicsRelativeJointDefinition - PhysicsSliderJointDefinition - PhysicsWheelJointDefinition |
| [`PhysicsWorld`](world.md#physicsworld) | A world is a container for all other physics objects such as PhysicsBody, PhysicsShape, PhysicsJoint etc. A world can be simulated in isolation from all other worlds. The maximum number of worlds that can be created at one time is defined by PhysicsCoreSettings2D._maximumWorlds. A world is completely isolated from all other worlds. |
| [`PhysicsWorldDefinition`](world.md#physicsworlddefinition) | A PhysicsWorld definition used to specify important initial properties. |

## Skill → Reference Files

Each `unity-physicscore2d-*` skill should consult the listed reference files when its content needs updating.

| Skill | Reference files |
|-------|-----------------|
| `unity-physicscore2d` | [world](world.md), [bodies](bodies.md), [shapes](shapes.md), [joints](joints.md) |
| `unity-physicscore2d-batching` | [bodies](bodies.md), [queries](queries.md) |
| `unity-physicscore2d-collision` | [shapes](shapes.md), [events](events.md), [world](world.md) |
| `unity-physicscore2d-components` | [bodies](bodies.md), [shapes](shapes.md), [world](world.md) |
| `unity-physicscore2d-composer` | [composer](composer.md), [shapes](shapes.md) |
| `unity-physicscore2d-debug` | [world](world.md) |
| `unity-physicscore2d-destruction` | [destructor](destructor.md), [shapes](shapes.md) |
| `unity-physicscore2d-destructor` | [destructor](destructor.md) |
| `unity-physicscore2d-events` | [events](events.md), [world](world.md) |
| `unity-physicscore2d-factories` | [bodies](bodies.md), [shapes](shapes.md), [joints](joints.md), [chains](chains.md) |
| `unity-physicscore2d-filtering` | [layers](layers.md), [shapes](shapes.md), [queries](queries.md) |
| `unity-physicscore2d-forces` | [bodies](bodies.md), [shapes](shapes.md) |
| `unity-physicscore2d-geometry` | [geometry](geometry.md) |
| `unity-physicscore2d-helpers` | [helpers](helpers.md), [layers](layers.md), [math](math.md) |
| `unity-physicscore2d-joints` | [joints](joints.md) |
| `unity-physicscore2d-layers` | [layers](layers.md), [shapes](shapes.md) |
| `unity-physicscore2d-materials` | [shapes](shapes.md) |
| `unity-physicscore2d-math` | [math](math.md), [helpers](helpers.md) |
| `unity-physicscore2d-performance` | [world](world.md), [bodies](bodies.md), [queries](queries.md) |
| `unity-physicscore2d-queries` | [queries](queries.md), [geometry](geometry.md) |
| `unity-physicscore2d-settings` | [world](world.md) |
| `unity-physicscore2d-shapes-advanced` | [shapes](shapes.md), [chains](chains.md), [geometry](geometry.md) |
| `unity-physicscore2d-stacking` | [bodies](bodies.md), [shapes](shapes.md), [joints](joints.md) |
