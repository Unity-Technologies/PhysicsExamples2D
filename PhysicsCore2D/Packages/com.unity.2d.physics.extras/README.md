# Physics 2D LowLevel Extras
Physics2D LowLevel Extras is a package that contains extra scripts for use with the LowLevel 2D Physics features in Unity.

This package is unsupported and is provided as example only to support the low-level project examples.

---

This package contains simple Unity components that encapsule low-level physics objects making it easy to get up and running quickly and easily.

These components are all prefixed with "Scene" with the following being provided:
- **SceneWorld** - Creates a [PhysicsWorld](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html)
- **SceneBody** - Creates a [PhysicsBody](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html)
- Shapes:
  - **SceneShape** - Creates a single [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) with either [CircleGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html), [CapsuleGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html), [PolygonGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html), [SegmentGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.SegmentGeometry.html) or [ChainSegmentGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html).
  - **SceneOutlineShape** - Creates multiple [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) with [PolygonGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html) in the form of an arbitrary concave/convex outline.
  - **SceneSpriteShape** - Creates multiple [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) with [PolygonGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html) from the physics data contained in a [Sprite](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Sprite.html).
  - **SceneChain** - Creates a single continuous set of [PhysicsShape](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html) with [ChainSegmentGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html) with one-way feature.
- Joints:
  - **SceneDistanceJoint** - Creates a [PhysicsDistanceJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJoint.html).
  - **SceneFixedJoint** - Creates a [PhysicsFixedJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDFixedJoint.html).
  - **SceneHingeJoint** - Creates a [PhysicsHingeJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJoint.html).
  - **SceneIgnoreJoint** - Creates a [PhysicsIgnoreJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsIgnoreJoint.html).
  - **SceneRelativeJoint** - Creates a [PhysicsRelativeJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJoint.html).
  - **SceneSliderJoint** - Creates a [PhysicsSliderJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJoint.html).
  - **SceneWheelJoint** - Creates a [PhysicsWheelJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJoint.html).
  
When a *SceneShape* or *SceneChain* is added to the scene, in-scene editors are available to edit the shape geometry directly in the scene.

---
