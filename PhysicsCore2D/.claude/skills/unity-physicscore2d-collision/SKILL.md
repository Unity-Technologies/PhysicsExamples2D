---
name: unity-physicscore2d-collision
description: Collision detection, contact manifolds, collision responses, determinism, and character collision handling
---

# Unity PhysicsCore2D Collision Expert

You are now acting as a Unity PhysicsCore2D collision expert, specialized in collision detection, responses, and character movement.

## Overview

PhysicsCore2D provides comprehensive collision detection and response capabilities including:
- Contact point generation and manifolds
- Collision callbacks and events (via IContactCallback)
- Character controller collision handling
- Deterministic collision behavior
- Bounce and restitution control

## PhysicsCallbacks.IContactCallback

**API Reference:** https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactCallback.html

`PhysicsCallbacks.IContactCallback` is the interface for receiving collision events during physics simulation. Callbacks are assigned per-shape by setting the `PhysicsShape.callbackTarget` property.

### Callback Methods

The interface provides two callback methods:

```csharp
public interface IContactCallback
{
    // Called when two shapes first make contact
    void OnContactBegin2D(PhysicsShape shapeA, PhysicsShape shapeB, ContactManifold manifold);

    // Called when two shapes stop being in contact
    void OnContactEnd2D(PhysicsShape shapeA, PhysicsShape shapeB, ContactManifold manifold);
}
```

### How Callbacks Work

1. Create a class that implements `PhysicsCallbacks.IContactCallback`
2. Set the `callbackTarget` property on each `PhysicsShape` you want to monitor
3. When that shape begins or ends contact with another shape, the callbacks are invoked during `PhysicsWorld.Step()`

### Implementation Example

```csharp
using UnityEngine;
using Unity.U2D.Physics;

public class CollisionHandler : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    private PhysicsBody m_Body;
    private PhysicsShape m_Shape;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;
        var bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        bodyDefinition.type = PhysicsBody.BodyType.Dynamic;

        m_Body = world.CreateBody(bodyDefinition);

        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = 0.5f };
        var shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        m_Shape = m_Body.CreateShape(circleGeometry, shapeDefinition);

        // Set this object as the callback target for the shape
        m_Shape.callbackTarget = this;
    }

    private void OnDisable()
    {
        if (m_Body.isValid)
            m_Body.Destroy();
    }

    public void OnContactBegin2D(PhysicsShape shapeA, PhysicsShape shapeB, ContactManifold manifold)
    {
        // Called when collision starts
        Debug.Log($"Collision started between {shapeA.body} and {shapeB.body}");

        // Access contact information
        Vector2 contactPoint = manifold.point;
        Vector2 contactNormal = manifold.normal;
        float penetrationDepth = manifold.distance;
    }

    public void OnContactEnd2D(PhysicsShape shapeA, PhysicsShape shapeB, ContactManifold manifold)
    {
        // Called when collision ends
        Debug.Log($"Collision ended between {shapeA.body} and {shapeB.body}");
    }
}
```

### Practical Example: Space Invaders Bullet Collision

```csharp
public class SpaceInvadersCollision : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    private NativeList<PhysicsBody> m_PlayerBullets;
    private NativeArray<PhysicsBody> m_EnemyBodies;
    private NativeList<bool> m_PlayerBulletActive;
    private NativeArray<bool> m_EnemyAlive;

    private void CreateBullet(Vector2 position)
    {
        var world = PhysicsWorld.defaultWorld;
        var bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        bodyDefinition.type = PhysicsBody.BodyType.Dynamic; // Must be Dynamic for callbacks
        bodyDefinition.position = position;

        var bullet = world.CreateBody(bodyDefinition);

        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = 0.2f };
        var shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        var shape = bullet.CreateShape(circleGeometry, shapeDefinition);

        // Set callback target on the shape
        shape.callbackTarget = this;

        m_PlayerBullets.Add(bullet);
        m_PlayerBulletActive.Add(true);
    }

    public void OnContactBegin2D(PhysicsShape shapeA, PhysicsShape shapeB, ContactManifold manifold)
    {
        var bodyA = shapeA.body;
        var bodyB = shapeB.body;

        // Check if a player bullet hit an enemy
        if (IsPlayerBullet(bodyA) && IsEnemy(bodyB))
        {
            HandleBulletHitEnemy(bodyA, bodyB);
        }
        else if (IsPlayerBullet(bodyB) && IsEnemy(bodyA))
        {
            HandleBulletHitEnemy(bodyB, bodyA);
        }
    }

    public void OnContactEnd2D(PhysicsShape shapeA, PhysicsShape shapeB, ContactManifold manifold)
    {
        // Not needed for Space Invaders
    }

    private bool IsPlayerBullet(PhysicsBody body)
    {
        for (int i = 0; i < m_PlayerBullets.Length; i++)
        {
            if (m_PlayerBullets[i] == body)
                return true;
        }
        return false;
    }

    private bool IsEnemy(PhysicsBody body)
    {
        for (int i = 0; i < m_EnemyBodies.Length; i++)
        {
            if (m_EnemyBodies[i] == body)
                return true;
        }
        return false;
    }

    private void HandleBulletHitEnemy(PhysicsBody bullet, PhysicsBody enemy)
    {
        // Find indices and mark as inactive/dead
        // Update score, etc.
    }
}
```

### Important Notes

**Setting Callback Targets:**
- Set `PhysicsShape.callbackTarget` property to an object implementing `IContactCallback`
- Each shape can have its own callback target
- Multiple shapes can share the same callback target object
- Callbacks are invoked during `PhysicsWorld.Step()`

**CRITICAL - Always Check Shape Validity:**
Multiple callbacks can fire in the same frame. If an earlier callback destroys a PhysicsBody, subsequent callbacks involving that body will have invalid shapes. Always check `shape.isValid` before accessing shape properties:

```csharp
public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent contactEvent)
{
    // CRITICAL: Check validity first!
    if (!contactEvent.shapeA.isValid || !contactEvent.shapeB.isValid)
        return; // One or both shapes were destroyed in a previous callback this frame

    var bodyA = contactEvent.shapeA.body;
    var bodyB = contactEvent.shapeB.body;

    // Now safe to process collision...
}

public void OnContactEnd2D(PhysicsEvents.ContactEndEvent contactEvent)
{
    // CRITICAL: Check validity first!
    if (!contactEvent.shapeA.isValid || !contactEvent.shapeB.isValid)
        return;

    // Now safe to process...
}
```

**CRITICAL - Dynamic Body Requirement:**
- **Callbacks only fire for collisions involving at least one Dynamic body**
- **Kinematic vs Kinematic collisions DO NOT trigger callbacks**
- **Static vs Static collisions DO NOT trigger callbacks**
- If using only Kinematic bodies (like in Space Invaders), you must either:
  1. Change at least one body type to Dynamic (e.g., make bullets Dynamic), OR
  2. Use manual overlap queries (from unity-physicscore2d-queries skill) instead

**Performance:**
- Callbacks are called during physics simulation, so keep them lightweight
- Avoid allocating memory in callbacks
- Cache frequently accessed data

**Thread Safety:**
- Contact callbacks may be called from the physics simulation thread
- Be careful when accessing Unity components or GameObjects
- Consider queuing actions to execute on the main thread if needed

### When to Use IContactCallback vs Manual Queries

**Use IContactCallback when:**
- You have at least one Dynamic body involved in collisions
- You need precise collision timing during physics simulation
- You want automatic collision detection with minimal overhead
- You need Begin/End lifecycle events

**Use Manual Queries (OverlapGeometry, etc.) when:**
- You're using only Kinematic bodies that don't trigger callbacks
- You need to check for potential collisions before they happen
- You need fine control over when collision checks occur
- You're implementing custom character controllers with kinematic bodies

## Repository Examples

Reference examples from the PhysicsExamples2D repository:
- **BounceHouse** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Collision/BounceHouse
- **BounceRagdolls** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Collision/BounceRagdolls
- **CharacterMover** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Collision/CharacterMover
- **ContactManifold** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Collision/ContactManifold
- **Determinism** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Collision/Determinism

## Key Collision Topics

### Contact Manifolds
Contact manifolds contain information about collision contact points between two shapes:
- Contact point positions
- Contact normals
- Penetration depths
- Contact IDs for tracking persistent contacts

### Character Movement
For character controllers that need collision detection:
- Use kinematic bodies for direct position control
- Query for overlaps before moving
- Resolve collisions manually for fine control
- Handle slopes and steps appropriately

### Determinism
PhysicsCore2D is deterministic when:
- Same initial conditions are used
- Same sequence of operations is performed
- Floating point precision is consistent
- Random seeds are controlled

### Collision Callbacks via IContactCallback
Monitor collisions by implementing `PhysicsCallbacks.IContactCallback` interface:
- OnContactBegin2D - when collision starts (first contact between shapes)
- OnContactEnd2D - when collision ends (shapes separate)

See the "PhysicsCallbacks.IContactCallback" section above for detailed implementation examples.

**Important:** Callbacks only fire for collisions involving at least one Dynamic body. Kinematic-only collisions require manual overlap queries.

## Best Practices

- Always check collision results before acting on them
- Use appropriate collision layers and filtering
- Consider using continuous collision detection for fast-moving objects
- Cache collision queries when performing multiple checks
- For character controllers, prefer swept queries over simple overlaps
- Set `PhysicsShape.callbackTarget` only on shapes that need collision notifications

## Related Skills

When users need information about:
- **Collision filtering** - Use unity-physicscore2d-filtering
- **Shape properties** - Use unity-physicscore2d-shapes-advanced
- **Physics queries** - Use unity-physicscore2d-queries
- **Materials and friction** - Use unity-physicscore2d-materials
