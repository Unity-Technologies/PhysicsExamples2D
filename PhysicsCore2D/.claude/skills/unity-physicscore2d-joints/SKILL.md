---
name: unity-physicscore2d-joints
description: All joint constraint types for Unity PhysicsCore2D
---

# Unity PhysicsCore2D Joints Expert

This sub-skill provides detailed information about PhysicsCore2D joint constraints.

## PhysicsJoint
A PhysicsJoint is the base of all joint constraints.
A single PhysicsJoint is defined as a constraint between two PhysicsBody.

The available PhysicsJoint types are:

### PhysicsDistanceJoint
Constrains the distance between two bodies.
- API reference: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJoint.html
- Definition: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJointDefinition.html

### PhysicsFixedJoint
Fixes two bodies together with no relative motion.
- API reference: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsFixedJoint.html
- Definition: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsFixedJointDefinition.html

### PhysicsHingeJoint
Constrains two bodies to rotate around a common anchor point (like a door hinge).
- API reference: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJoint.html
- Definition: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJointDefinition.html

### PhysicsIgnoreJoint
Causes two bodies to ignore collisions with each other.
- API reference: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsIgnoreJoint.html
- Definition: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsIgnoreJointDefinition.html

### PhysicsRelativeJoint
Maintains a relative position and rotation between two bodies.
- API reference: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJoint.html
- Definition: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJointDefinition.html

### PhysicsSliderJoint
Constrains two bodies to move along a line (like a sliding door).
- API reference: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJoint.html
- Definition: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJointDefinition.html

### PhysicsWheelJoint
Simulates a wheel suspension system.
- API reference: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJoint.html
- Definition: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJointDefinition.html

You can find more detail here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsJoint.md
