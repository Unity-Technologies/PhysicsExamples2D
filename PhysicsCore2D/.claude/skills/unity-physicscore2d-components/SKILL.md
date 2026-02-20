---
name: unity-physicscore2d-components
description: Component authoring patterns and best practices for Unity PhysicsCore2D
---

# Unity PhysicsCore2D Component Authoring Expert

This sub-skill provides detailed information about authoring Unity components that wrap PhysicsCore2D objects.

## Component Authoring
The PhysicsCore2D is an API that is decoupled from Unity GameObject and Components however it is designed to be used in two ways:
1. The API is used directly in any scripts.
2. The API is used to create Unity components which only expose specific game authoring properties but the component uses the PhysicsCore2D API directly.

To make component authoring easier, the API has many features which make this easier such as:
- Ability to specify a Unity Transform to which a PhysicsBody will write by specifying the Transform using [PhysicsBody.transformObject](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody-transformObject.html).
- Ability to be notified when a Unity Transform changes so the component can update any relevant PhysicsCore2D objects using [PhysicsWorld.RegisterTransformChange](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.RegisterTransformChange.html) and [PhysicsWorld.UnregisterTransformChange](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.UnregisterTransformChange.html).
- Ability to set an owner object which returns a secret key which must be used when destroying a physics object. This means a component author can stop users of the component from accidentally destroying the physics objects it creates and needs to function correctly.

A complete example package is available which wraps all the available physics objects into Unity components, all of which demonstrate the above features.
The package can be found here: https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Packages/com.unity.2d.physics.extras
The package [runtime directory](https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Packages/com.unity.2d.physics.extras/Runtime) contains all the Unity components available.
All the example components begin with the prefix "Scene" i.e. SceneWorld, SceneBody, SceneShape etc.

You can find an overview of component authoring here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/ComponentAuthoring.md

## Debug Drawing
The debug drawing refers to the PhysicsWorld renderer which will automatically draw the world contents (PhysicsBody, PhysicsShape, PhysicsJoint etc).
The user can also explicitly ask for custom drawing on any PhysicsWorld to draw any geometry type, lines, line-strips, points, capsules, circles, boxes etc.

To control what is drawn in the world, use the `world.drawOptions` property (get/set). This property accepts an enumeration of things that should be drawn.
You can find the API reference for drawOptions here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-drawOptions.html

**Best Practice**: Individual components should NOT modify `world.drawOptions` as this is a global world-level setting. The user controls what is drawn at the world/scene level. If a component wants to conditionally draw something (like a circle, line, etc.), it should use a local boolean field (e.g., `public bool DrawCircle = true`) to control whether that specific component draws, not modify the global world settings.

You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/DebugDrawing.md
