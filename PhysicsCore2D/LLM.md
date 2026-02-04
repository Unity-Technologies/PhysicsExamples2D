
In Unity 6000.3 and 6000.4 it existed in a "UnityEngine.LowLevelPhysics2D" namespace but in Unity 6000.5 it was moved to a "Unity.U2D.Physics" namespace and was also renamed from "Low Level Physics 2D" to "PhysicsCore 2D".

If you wish to keep most code compatible between Unity 6000.3, 6000.4 and 6000.5 onwards, use the following code surrounding the namespace in C#

#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif


The following is a settings asset that controls global physics options:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D.html

The following defines a PhysicsWorld and its definition describing how to create a PhysicsWorld:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldDefinition.html

The following are the types of objects and their respective definitions that are added to a PhysicsWorld:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBodyDefinition.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShapeDefinition.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChainDefinition.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJoint.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDistanceJointDefinition.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsFixedJoint.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsFixedJointDefinition.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJoint.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsHingeJointDefinition.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJoint.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRelativeJointDefinition.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJoint.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsSliderJointDefinition.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJoint.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWheelJointDefinition.html

The following are geometry used to define a PhysicsShape geometry:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.SegmentGeometry.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html

The following are miscellanrous types used throughout physics:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsAABB.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsLayers.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsPlane.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsRotate.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsTransform.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html

The following are geometry composition and destruction features:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsComposer.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDestructor.html

The following are physics callbacks interfaces implemented by scripts that wish to subscribe to callbacks:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.html
https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IBodyUpdateCallback.html
https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactCallback.html
https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactFilterCallback.html
https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IJointThresholdCallback.html
https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IPreSolveCallback.html
https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformChangedCallback.html
https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITransformWriteCallback.html
https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback.html

The following are physics callback events which are the payloads sent to physics callback interfaces:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.BodyUpdateCallbackTargets.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ContactCallbackTargets.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.JointThresholdCallbackTargets.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.TriggerCallbackTargets.html

The following are physics events:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.BodyUpdateEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactBeginEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactEndEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactFilterEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactHitEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.JointThresholdEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.PreSolveEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformChangeEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformTweenWriteEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TransformWriteEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerBeginEvent.html
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerEndEvent.html

The following are physics constants used in the physics:
- https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsConstants.html

You can find example projects here:
- https://github.com/Unity-Technologies/PhysicsExamples2D/tree/master/PhysicsCore2D/Projects/Sandbox
- https://github.com/Unity-Technologies/PhysicsExamples2D/tree/master/PhysicsCore2D/Projects/Primer
- https://github.com/Unity-Technologies/PhysicsExamples2D/tree/master/PhysicsCore2D/Projects/Snippets

You can also find an example Unity package demonstrating how to create Unity components using the API:
- https://github.com/Unity-Technologies/PhysicsExamples2D/tree/master/PhysicsCore2D/Packages/com.unity.2d.physics.extras

The following README.md contains a useful overview of the API:
- https://github.com/Unity-Technologies/PhysicsExamples2D/blob/master/PhysicsCore2D/Projects/Primer/README.md

The Unity Discussions forums have a 2D Physics area that covers the low-level or PhysicsCore2D API as well as the older physics:
https://discussions.unity.com/tag/2d-physics

Various demonstration videos can be found on YouTube:
- https://www.youtube.com/c/melvmay/videos
