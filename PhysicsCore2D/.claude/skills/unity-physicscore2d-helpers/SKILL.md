---
name: unity-physicscore2d-helpers
description: Helper types for Unity PhysicsCore2D (PhysicsTransform, PhysicsRotate, PhysicsMask, etc.)
---

# Unity PhysicsCore2D Helpers Expert

This sub-skill provides detailed information about PhysicsCore2D helper types.

## Core Helper Objects

### PhysicsTransform
A PhysicsTransform contains configuration for the three degrees of freedom for a PhysicsBody i.e. 2D position and 1D rotation, although it can be used for any custom purpose.
It contains both a "UnityEngine.Vector2" position and a "PhysicsRotate" rotation.
A PhysicsTransform is serializable so it can be used as a field in Unity and configured in the Unity Inspector.

You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsTransform.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsTransform.md

### PhysicsRotate
A PhysicsRotate contains configuration for a single axis rotation.
A PhysicsRotate can be configured as an angle using either Radian or Degree units.
A PhysicsRotate does not store the angle which was used to create it, instead (for performance reasons) it stores a 2D unit vector ("UnityEngine.Vector2D") where the vector X is cos(angle) and Y is sin(angle).
A PhysicsRotate is serializable so it can be used as a field in Unity and configured in the Unity Inspector.

You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsRotate.md

### PhysicsMask
A PhysicsMask is a dedicated 64-bit bitmask used for many features in the PhysicsCore2D such as:
- Specifying "PhysicsShape.ContactFilter" categories and contacts which control if PhysicsShape can come into contact or not.
- Specifying "PhysicsQuery.QueryFilter" categories and hitCategories which control what PhysicsWorld queries detect.
- Specifying "PhysicsLayers" where each individual 64 layer has a name associated with it making it more user friendly when referring to a layer rather than a PhysicsMask bit position. These are shown rather than bit positions in the Unity Inspector.

A PhysicsMask is used because it is a compact memory representation used to represent 64 discrete items and bitmasking is a high performance way to compare different PhysicsMask.

You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsMask.md

### PhysicsUserData
PhysicsUserData is used to store custom data.
The PhysicsUserData is not used by physics but is useful for custom purposes such as identification when encountering various physics objects such as the results from physics queries.
The PhysicsUserData can quickly and easily both read from or written to any of the following physics object types:
- PhysicsWorld
- PhysicsBody
- PhysicsShape
- PhysicsChain
- PhysicsDistanceJoint
- PhysicsFixedJoint
- PhysicsHingeJoint
- PhysicsIgnoreJoint
- PhysicsRelativeJoint
- PhysicsSliderJoint
- PhysicsWheelJoint

You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsUserData.md

### PhysicsAABB
A PhysicsAABB defines a 2D axis aligned bounding box.
A PhysicsAABB is typically used by PhysicsShape to define their bounds in a world.

You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsAABB.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsAABB.md

### PhysicsMath
The PhysicsMath type has a set of methods specifically design to help with writing PhysicsCore2D code.
The PhysicsMath is not designed to be a comprehensive and generic math library.
The PhysicsMath can help when performing various math operations which require deterministic results such as cos, sin and atan2 as well as constants for both PI and TAU (2 * PI).
The PhysicsMath can help when using "PhysicsWorld.TransformPlane" which allows the 2D world to be written to and read from a custom 2D plane in a 3D world space.
The PhysicsMath can convert to and from degrees and radians.
The PhysicsMath has several miscellaneous functions for physics dynamics such as a spring-damper system.

You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMath.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsMath.md
