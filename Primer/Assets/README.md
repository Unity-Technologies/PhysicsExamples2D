# PhysicsCore2D "Primer" Examples

The following examples are provided to give a good start in understanding some of the more important features provided by the PhysicsCore2D API (`Unity.U2D.Physics`).
Below will add any extra detail not covered in the examples themselves.

Each example has at least a single "Example" GameObject with an example script which you should examine to understand how the example works.
The scripts contain short annotation-style comments where appropriate to highlight the functionality.

<b>Note:</b> Gaps in the example numbering is intentional, leaving space for addition examples to be added.

---
## 01 - Create Physics World
This example demonstrates creating and destroying a `PhysicsWorld` which is completely isolated from other `PhysicsWorld.

---
## 02 - Use Default Physics World
Whilst it is powerful to be able to create your own isolated `PhysicsWorld`, it is more common to use a single `PhysicsWorld` which is why Unity automatically creates one for you.
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
## 12 - Physics Shape Trigger Callback
This example shows you the basics of configuring a script callback when a pair of `PhysicsShape` overlap when either of the pair are a trigger.

---
## 13 - Physics Shape Contact Filtering
This example shows you the basics of configuring how `PhysicsShape` come into contact but also how to intercept contact processing to stop contacts being created.

---
## 14 - Transform Write
This example shows you how to control if and how a `PhysicsBody` writes to a specific Unity Transform.

---
## 15 - Transform Plane Write
This example shows you how to configure the `PhysicsWorld` so that it writes to a selected 3D Transform plane rather than always the XY plane.

---
## 16 - Physics User Data
Physics user data is not used by the physics system but allows you to get/set it to multiple objects for your own customisable purposes.

This example shows you how to create and assign `PhysicsUserData` to a `PhysicsBody` and `PhysicsShape` although it is available to multiple objects types of:
- PhysicsWorld
- PhysicsBody
- PhysicsShape
- PhysicsChain
- PhysicJoint (all)

---
##17 - Physics Query Job
Whilst a majority of the API can be used in a C# Job, this examples specifically shows you can use queries in a C# Job.

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
## 23 - Physics Destructor Fragment Mask Geometry
This example shows the basics of how to use the `PhysicsDestructor.Fragment` to fragment geometry using fragment points but where a geometry mask is applied, producing Polygon geometry which can then be used to create `PhysicsShape`.
The option of using a mask in this example over the example 22 is that the target geometry first has a mask geometry removed (known as "carving") and that is returned as "unbroken" geometry.
The geometry removed by the mask is then the geometry that is fractured which is returned as "broken" geometry.
The net result is that the target geometry has a mask carved from it with the carved region then being fractured which effectively acts as if a region was broken from the target geometry.

---
## WIP

