# Physics 2D LowLevel Primer Examples

The following examples provide a good start to understanding some of the more important features provided by the low-level Physics API and test package provided.
Below will add an extra detail not covered in the example itself.
Each example has at least a single "Example" GameObject with an example script which you should examine to understand how the example works.
<b>Note:</b> Gaps in the example numbering is intentional, leaving space for addition examples to be added.

---
## 01 - Create Physics World
This example demonstrates creating and destroying a `PhysicsWorld` which is completely isolated from other `PhysicsWorld.

---
## 02 - Use Default Physics World
Whilst it is powerful to be able to create your own isolated `PhysicsWorld`, it is more common to use a single `PhysicsWorld` which is why Unity automaticaly creates one for you.
This example shows you how you access the default world.

---
## 03 - Create Physics Body
This example shows you how to create a `PhysicsBody` in a `PhysicsWorld`.

---
## 04 - Create Physics Shape
This example shows you how to create a `PhysicsShape` on a `PhysicsBody`.

---
## 05 - Reuse Definitions
This example goes into more detail showing you how you can/should reuse definitions when creating object types.

---
## 06 - Physics Shape Custom Colors
This example shows how you can select a custom color for each `PhysicsShape` which is useful when using the debug renderer.
A custom color is specified in the shapes `SurfaceMaterial` along with its physics-based properties such as Friction, Bounciness etc.

---
## 07 - Physics Shape Types
This example shows the main primitive types of shapes, each with their own geometry type.

---
## 08 - Cast Ray Query
This example shows you how to use one of the many queries available, in this case, using CastRay.

---
## 09 - Cast Geometry Query
This example shows you how to use the geometry query to cast geometry through the world, detecting its contents.

---
## 10 - Physics Shape Contact
This example shows you the basics of configuring how `PhysicsShape` come into contact and how events are produced.

---
## 11 - Physics Shape Contact Callback
This example shows you the basics of configuring a script callback when a pair of `PhysicsShape` come into contact.

---
## 12 - Physids Shape Trigger Callback
This example shows you the basics of configuring a script callback when a pair of `PhysicsShape` overlap when either of the pair are a trigger.

---
## 13 - Transform Write
This example shows you how to control if and how a `PhysicsBody` writes to a specific Unity Transform.

---
## 14 - Transform Plane Write
This example shows you how to configure the `PhysicsWorld` so that it writes to a selected 3D Transform plane rather than always the XY plane.

---
## 15 - Physics User Data
Physics user data is not used by the physics system but allows you to get/set it to multiple objects for your own customisable purposes.

This example shows you how to create and assign `PhysicsUserData` to a `PhysicsBody` and `PhysicsShape` although it is available to multiple objects types of:
- PhysicsWorld
- PhysicsBody
- PhysicsShape
- PhysicsChain
- PhysicJoint (all)

---

---

---
## 20 - Physics Composer Geometry
This example shows the basics of how to use the `PhysicsComposer` to add geometry to layers, specifying order and operation and producing polygon or chain output which can then be used to create `PhysicsShape`.

---
## 21 - Physics Destructor Slice Geometry
This example shows the basics of how to use the `PhysicsDestructor.Slice` to slice geometry in two, producing Polygon geometry which can then be used to create `PhysicsShape`.

---
## 22 - Physics Destructor Fragment Geometry
This example shows the basics of how to use the `PhysicsDestructor.Fragment` to fragment geometry using fragment points, producing Polygon geometry which can then be used to create `PhysicsShape`.

---

---

---


# Low Level Extras Package

The following examples show how to use the test package `com.unity.2d.physics.lowlevelextras`.

This package is an example-only package which wraps the API you've seen so far into Unity components.
It is not designed to be used in production projects however you are free to do so if you wish.
Most editing of properties can only be done in Edit mode as the components are only examples and are not fully featured.
It exposes most features for each object type and allows editing their configuration in the inspector.
For `PhysicsShape` components, scene tooling is available allowing you to edit geometry visually; this also supports multiple TransformPlane editing.
The components aim to demonstrate how you can create your own components using the API.

All components begin with a "Scene" prefix and can be found in the component menu `Physics 2D > LowLevel`:

- SceneWorld - Wraps ` PhysicsWorld` allowing you to select either the default world or a custom world. 
- SceneBody - Wraps a `PhysicsBody` allowing you to create them in a world.
- SceneShape - Wraps a single `PhysicsShape` allowing you to select the shape type.
- SceneOutlineShape - Wraps multiple `PhysicsShape` created from an editable concave/convex outline (identical to the [PolygonCollider2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PolygonCollider2D.html))
- SceneSpriteShape - Wraps multiple `PhysicsShape` created from a selected [Sprite](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Sprite.html).
- SceneChain - Wraps a `PhysicsChain`.
- Joints
  - SceneDistanceJoint - Wraps a `PhysicsDistanceJoint` (similar to the [DistanceJoint2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/DistanceJoint2D.html))
  - SceneFixedJoint - Wraps a `PhysicsFixedJoint` (similar to the [FixedJoint2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/FixedJoint2D.html))
  - SceneHingeJoint - Wraps a `PhysicsHingeJoint` (similar to the [HingeJoint2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/HingeJoint2D.html))
  - SceneIgnoreJoint - Wraps a `PhysicsIgnoreJoint`
  - SceneRelativeJoint - Wraps a `PhysicsRelativeJoint` (similar to the [RelativeJoint2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/RelativeJoint2D.html))
  - SceneSliderJoint - Wraps a `PhysicsSliderJoint` (similar to the [SliderJoint2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SliderJoint2D.html))
  - SceneWheelJoint - Wraps a `PhysicsWheelJoint` (similar to the [WheelJoint2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/WheelJoint2D.html))

---
## 50 - Scene Body And Shape
This example shows how to create a body with a shape attached to it.

The `SceneBody` exposes:
- The `PhysicsBodyDefinition` used to create the body
- `PhysicsUserData` assigned to `PhysicsBody.userData`
- The callback target for event callbacks assigned to `PhysicsBody.callbackTarget`
- The ability to use the default world via the "Use Default World" checkbox. When unchecked, you can select a specific `SceneWorld` component.

The `PhysicsShape` exposes:
- The `PhysicsShapeDefinition` used to create the shape.
- `PhysicsUserData` assigned to `PhysicsShape.userData`
- The callback target for event callbacks assigned to `PhysicsShape.callbackTarget`
- The specific `SceneBody` to create the shape on. For convenience, this is automatically populated by searching the current parent hierarchy for a `SceneBody`. Without it, a shape cannot be created.

---
## 51 - Scene World
This example is identical to example "50" in that it creates a body with a shape attached to it.
The difference with this example is that there is a new GameObject with a `SceneWorld` component and the `SceneBody` has its "Use Default World" disabled and has selected the `SceneWorld` component on the `MyWorld` GameObject.
The result of this is that the `SceneBody` is created in the `PhysicsWorld` the `SceneWorld` component creates.

This means you can have multiple worlds and multiple objects within those worlds in a single Unity scene if you wish.
Whilst the `SceneBody` is connected to the `SceneWorld`, you can configure the `SceneWorld` to simply represent the default world by enabling its "Use Default World" checkbox.
Multiple `SceneWorld` using the default world all represent the same default world that Unity implicitly createds.
Only with that option disabled does the `SceneWorld` create a `PhysicsWorld`.

