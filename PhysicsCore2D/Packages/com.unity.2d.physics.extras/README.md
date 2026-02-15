# Physics 2D LowLevel Extras
Physics2D LowLevel Extras is a package that contains extra scripts for use with the LowLevel 2D Physics features in Unity.

This package is unsupported and is provided as example only to support the low-level project examples.

---

This package contains simple Unity components that encapsule low-level physics objects making it easy to get up and running quickly and easily.

These components are all prefixed with "Test" with the following being provided:
- **TestWorld** - Creates a [PhysicsWorld](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.html)
- **TestBody** - Creates a [PhysicsBody](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody.html)
- Shapes:
  - **TestShape** - Creates a single [PhysicsShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.html) with either [CircleGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CircleGeometry.html), [CapsuleGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.CapsuleGeometry.html), [PolygonGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PolygonGeometry.html), [SegmentGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.SegmentGeometry.html) or [ChainSegmentGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.ChainSegmentGeometry.html).
  - **TestOutlineShape** - Creates multiple [PhysicsShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.html) with [PolygonGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PolygonGeometry.html) in the form of an arbitrary concave/convex outline.
  - **TestSpriteShape** - Creates multiple [PhysicsShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.html) with [PolygonGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PolygonGeometry.html) from the physics data contained in a [Sprite](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Sprite.html).
  - **TestChain** - Creates a single continuous set of [PhysicsShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.html) with [ChainSegmentGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.ChainSegmentGeometry.html) with one-way feature.
- Joints:
  - **TestDistanceJoint** - Creates a [PhysicsDistanceJoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsDistanceJoint.html).
  - **TestFixedJoint** - Creates a [PhysicsFixedJoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsDFixedJoint.html).
  - **TestHingeJoint** - Creates a [PhysicsHingeJoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsHingeJoint.html).
  - **TestIgnoreJoint** - Creates a [PhysicsIgnoreJoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsIgnoreJoint.html).
  - **TestRelativeJoint** - Creates a [PhysicsRelativeJoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsRelativeJoint.html).
  - **TestSliderJoint** - Creates a [PhysicsSliderJoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsSliderJoint.html).
  - **TestWheelJoint** - Creates a [PhysicsWheelJoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWheelJoint.html).
  
When a *TestShape* or *TestChain* is added to the scene, in-scene editors are available to edit the shape geometry directly in the scene.

---
