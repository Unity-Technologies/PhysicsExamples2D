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
// IContactCallback interface declaration stub
// Source: PhysicsCallbacks.IContactCallback (events-api, PhysicsShapeContactCallback.cs)
public interface IContactCallback
{
    // Called on the main thread after simulation, when two non-trigger shapes first touch.
    void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent);

    // Called on the main thread after simulation, when two non-trigger shapes stop touching.
    // Note: fires immediately if a body/shape is destroyed, or the body transform is set,
    // even if the shapes were still overlapping.
    void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent);
}
```

**Event struct members** (both `ContactBeginEvent` and `ContactEndEvent`):

| Member | Type | Description |
|--------|------|-------------|
| `shapeA` | `PhysicsShape` | One of the shapes; may be invalid if destroyed in an earlier callback |
| `shapeB` | `PhysicsShape` | The other shape; same caveat |
| `contactId` | `PhysicsShape.ContactId` | Volatile contact handle — call `.contact` to get the `PhysicsShape.Contact` |

**Accessing the contact manifold from a begin event:**
```csharp
var contact = beginEvent.contactId.contact;
var manifold = contact.manifold;           // PhysicsShape.ContactManifold
// manifold.normal        — world-space unit normal from shapeA to shapeB
// manifold.pointCount    — number of active contact points [0..2]
// manifold[i]            — ManifoldPoint via indexer
// foreach (var mp in manifold) — enumerable
```

### How Callbacks Work

1. Create a class that implements `PhysicsCallbacks.IContactCallback`
2. Set the `callbackTarget` property on each `PhysicsShape` you want to monitor
3. When that shape begins or ends contact with another shape, the callbacks are invoked during `PhysicsWorld.Step()`

### Implementation Example

```csharp
// Source: Primer/Assets/Scripts/PhysicsShapeContactCallback.cs (real-code)
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

// Implement IContactCallback on any class (MonoBehaviour, plain C#, etc.).
public class CollisionHandler : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    private PhysicsWorld m_PhysicsWorld;

    private void OnEnable()
    {
        m_PhysicsWorld = PhysicsWorld.Create();

        // Enable auto-dispatch of contact callbacks for this world.
        m_PhysicsWorld.autoContactCallbacks = true;

        // Shape definition: contactEvents = true is required for callbacks to fire.
        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter  = new PhysicsShape.ContactFilter { categories = PhysicsMask.One, contacts = PhysicsMask.All },
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0.8f },
            contactEvents  = true,   // <— REQUIRED to receive contact callbacks
        };

        var body1 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
            { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-0.25f, 2f) });
        var body2 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
            { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(0.25f, 4f) });

        var shape1 = body1.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
        var shape2 = body2.CreateShape(CircleGeometry.defaultGeometry, shapeDef);

        // Point this script as the callback target for both shapes.
        // The target object must not be garbage collected while shapes are live.
        shape1.callbackTarget = this;
        shape2.callbackTarget = this;
    }

    private void OnDisable()
    {
        m_PhysicsWorld.Destroy();
    }

    // Called on the main thread, once per frame after PhysicsWorld.Step(),
    // for every new contact pair that involves this callback target's shapes.
    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        // CRITICAL: Shapes can be destroyed by an earlier callback in the same frame.
        if (!beginEvent.shapeA.isValid || !beginEvent.shapeB.isValid)
            return;

        // Retrieve the live contact via contactId.
        var contact  = beginEvent.contactId.contact;
        var manifold = contact.manifold;

        // manifold.normal      — unit normal from shapeA to shapeB
        // manifold.pointCount  — how many contact points (0..2)
        // manifold[i].point    — world-space contact position (clip point)
        // manifold[i].anchorA  — position relative to shapeA's origin (prefer for game logic)
        // manifold[i].anchorB  — position relative to shapeB's origin
        // manifold[i].separation   — negative when penetrating
        // manifold[i].normalImpulse — impulse along normal this step

        Debug.Log($"Contact began: normal={manifold.normal}, points={manifold.pointCount}");

        // Iterate contact points via foreach (manifold is enumerable).
        foreach (var mp in manifold)
        {
            m_PhysicsWorld.DrawPoint(mp.point, radius: 20f, color: Color.yellow, lifetime: 1f);
        }

        // Alternatively, access by index:
        // for (int i = 0; i < manifold.pointCount; i++)
        //     Debug.Log(manifold[i].anchorA);
    }

    // Called when the contact pair separates, or whenever anything destroys contacts
    // (body destroyed, transform set, contact filter changed, etc.).
    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent)
    {
        // CRITICAL: Shapes may already be invalid here.
        if (!endEvent.shapeA.isValid || !endEvent.shapeB.isValid)
            return;

        Debug.Log("Contact ended");
    }
}
```

### Practical Example: Space Invaders Bullet Collision

```csharp
// Source: adapted from Sandbox/Scenes/Shapes/Fragmenting/Fragmenting.cs (real-code)
// Pattern: space-invader style — bullet (Dynamic, gravityScale=0) hits an enemy (Dynamic/Static);
// callback destroys both and records the hit position.
// NOTE: Callbacks require at least one Dynamic body in the contact pair.
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

public class SpaceInvadersCollision : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    // Layer masks — use distinct PhysicsMask bits so contact filters work.
    private readonly PhysicsMask m_BulletMask = new(1);
    private readonly PhysicsMask m_EnemyMask  = new(2);
    private readonly PhysicsMask m_WallMask   = new(3);

    private const float BulletSpeed  = 20f;
    private const float BulletRadius = 0.15f;

    private PhysicsWorld m_World;
    private Vector2      m_CannonPosition = new(0f, -4f);

    private void OnEnable()
    {
        m_World = PhysicsWorld.defaultWorld;

        // Enable contact callbacks for the world.
        m_World.autoContactCallbacks = true;

        SpawnEnemies();
        SpawnWalls();
    }

    // Call this from Update / UI button / Input to fire a bullet.
    public void Fire()
    {
        // Bullets must be Dynamic so IContactCallback fires.
        // gravityScale = 0 keeps them moving straight up.
        var bulletBody = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type            = PhysicsBody.BodyType.Dynamic,
            gravityScale    = 0f,
            fastCollisionsAllowed = true,           // CCD for fast-moving projectiles
            position        = m_CannonPosition + Vector2.up * (BulletRadius + 0.1f),
            linearVelocity  = Vector2.up * BulletSpeed,
        });

        var bulletShapeDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = m_BulletMask,
                contacts   = m_EnemyMask | m_WallMask,
            },
            contactEvents = true,  // required for IContactCallback to be dispatched
        };

        var bulletShape = bulletBody.CreateShape(
            new CircleGeometry { radius = BulletRadius }, bulletShapeDef);

        // Register this MonoBehaviour as the callback target for the bullet shape.
        bulletShape.callbackTarget = this;
    }

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        // Always validate — a previous callback in the same frame may have
        // destroyed a body, invalidating its shapes.
        if (!beginEvent.shapeA.isValid || !beginEvent.shapeB.isValid)
            return;

        var catA = beginEvent.shapeA.contactFilter.categories;
        var catB = beginEvent.shapeB.contactFilter.categories;

        // Bullet hit a wall — just remove the bullet.
        if (catA == m_WallMask || catB == m_WallMask)
        {
            var bulletShape = catA == m_BulletMask ? beginEvent.shapeA : beginEvent.shapeB;
            if (bulletShape.isValid)
                bulletShape.body.Destroy();
            return;
        }

        // Bullet hit an enemy.
        if (catA == m_EnemyMask || catB == m_EnemyMask)
        {
            // Record the first contact point for a hit effect.
            var contact  = beginEvent.contactId.contact;
            var hitPoint = contact.manifold.points[0].point;  // world-space clip point
            var hitNormal = contact.manifold.normal;           // normal from shapeA to shapeB

            Debug.Log($"Hit at {hitPoint}, normal {hitNormal}");
            // Queue spawn of explosion VFX at hitPoint here (main-thread safe).

            // Destroy both bodies (shape.body.Destroy() removes all shapes on that body).
            // Validity checks guard against double-destruction from multi-shape compound bodies.
            var bulletBody = catA == m_BulletMask ? beginEvent.shapeA.body : beginEvent.shapeB.body;
            var enemyBody  = catA == m_EnemyMask  ? beginEvent.shapeA.body : beginEvent.shapeB.body;

            if (bulletBody.isValid) bulletBody.Destroy();
            if (enemyBody.isValid)  enemyBody.Destroy();
        }
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent)
    {
        // No action needed on separation in this scenario.
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private void SpawnEnemies()
    {
        var enemyShapeDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = m_EnemyMask,
                contacts   = m_BulletMask,
            },
            contactEvents = true,
        };

        for (int col = 0; col < 5; col++)
        for (int row = 0; row < 3; row++)
        {
            // Static enemies — bullet is Dynamic, so callbacks still fire.
            var body = m_World.CreateBody(new PhysicsBodyDefinition
            {
                type     = PhysicsBody.BodyType.Static,
                position = new Vector2(col * 2f - 4f, row * 1.5f + 1f),
            });

            var shape = body.CreateShape(
                PolygonGeometry.CreateBox(new Vector2(0.8f, 0.6f)), enemyShapeDef);

            // Each enemy shape reports collisions back to this manager.
            shape.callbackTarget = this;
        }
    }

    private void SpawnWalls()
    {
        var wallShapeDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = m_WallMask,
                contacts   = m_BulletMask,
            },
        };
        var wallBody = m_World.CreateBody();  // Static by default
        wallBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 10f),
            offset: new Vector2(-6f, 0f)), wallShapeDef);
        wallBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 10f),
            offset: new Vector2( 6f, 0f)), wallShapeDef);
    }
}
```

> **Key reminder:** `shape.contactEvents = true` (or in the `PhysicsShapeDefinition`) must be set on at least one shape in the pair, and `world.autoContactCallbacks = true` must be set, or no callbacks will be dispatched. Callbacks fire on the **main thread** after `PhysicsWorld.Step()` completes.

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
