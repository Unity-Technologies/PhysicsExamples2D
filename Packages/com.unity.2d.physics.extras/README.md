# PhysicsCore2D Extras
PhysicsCore2D Extras is a package that contains extra scripts for use with the 2D Physics features in Unity.

This package is unsupported and is provided as example only to support the project examples.

---

This package contains simple Unity components that encapsule physics objects making it easy to get up and running quickly and easily.

These components are all prefixed with "Test" with the following being provided:
- **TestWorld** - Creates a [PhysicsWorld](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html)
- **TestBody** - Creates a [PhysicsBody](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html)
- Shapes:
  - **TestShape** - Creates a single [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) with either [CircleGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html), [CapsuleGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html), [PolygonGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html), [SegmentGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.SegmentGeometry.html) or [ChainSegmentGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html).
  - **TestOutlineShape** - Creates multiple [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) with [PolygonGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html) in the form of an arbitrary concave/convex outline.
  - **TestSpriteShape** - Creates multiple [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) with [PolygonGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html) from the physics data contained in a [Sprite](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Sprite.html).
  - **TestChain** - Creates a single continuous set of [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) with [ChainSegmentGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html) with one-way feature.
- Joints:
  - **TestDistanceJoint** - Creates a [PhysicsDistanceJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJoint.html).
  - **TestFixedJoint** - Creates a [PhysicsFixedJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDFixedJoint.html).
  - **TestHingeJoint** - Creates a [PhysicsHingeJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJoint.html).
  - **TestIgnoreJoint** - Creates a [PhysicsIgnoreJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsIgnoreJoint.html).
  - **TestRelativeJoint** - Creates a [PhysicsRelativeJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJoint.html).
  - **TestSliderJoint** - Creates a [PhysicsSliderJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJoint.html).
  - **TestWheelJoint** - Creates a [PhysicsWheelJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJoint.html).
  
When a *TestShape* or *TestChain* is added to the scene, in-scene editors are available to edit the shape geometry directly in the scene.

---
