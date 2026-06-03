# Unity PhysicsCore2D - Events and Callbacks

Expert guidance on using physics events and callback systems in Unity PhysicsCore2D.

> For the full type/method API surface — every event struct, callback interface, and member with XML doc — see `unity-physicscore2d-events-api`. This skill focuses on enabling patterns, dispatch-mode decisions, and worked examples.

## Event Types

The `PhysicsWorld` generates seven event types after simulation completes:

### Contact Events
- **contactHitEvents** - When a contact for a pair of shapes starts beyond a specified threshold speed
- **contactBeginEvents** - Triggered when shapes first make contact
- **contactEndEvents** - Triggered when shapes separate

### Trigger Events
- **triggerBeginEvents** - Activated when trigger overlaps commence
- **triggerEndEvents** - Activated when trigger overlaps terminate

### Other Events
- **bodyUpdateEvents** - Occurs when bodies update during simulation
- **jointThresholdEvents** - When a joint exceeds its force or torque threshold

All events are accessed via `ReadOnlySpan<EventType>` for direct memory access.

## Enabling Events

To generate events for specific shapes, you must explicitly configure them using these properties:

```csharp
// Enable contact hit events (high-speed impacts)
physicsShape.hitEvents = true;

// Enable contact begin/end events
physicsShape.contactEvents = true;

// Enable trigger begin/end events
physicsShape.triggerEvents = true;
```

## Reading Events Directly

Events can be read directly from the PhysicsWorld after simulation:

```csharp
// After world.Simulate(deltaTime)
var contactBegin = world.contactBeginEvents;
foreach (var evt in contactBegin)
{
    // Process contact begin event
    var shapeA = evt.shapeA;
    var shapeB = evt.shapeB;
    // ... handle event
}
```

## Callback Interfaces

Six callback interfaces are available for automatic event processing:

### Contact Callbacks

Implement `PhysicsCallbacks.IContactCallback` to receive begin/end contact notifications. Set `shape.contactEvents = true` and `shape.callbackTarget = this` to wire up the shape. Enable `world.autoContactCallbacks = true` (or call `world.SendContactCallbacks()` manually after simulation).

```csharp
// Source: PhysicsShapeContactCallback.cs (Primer example)
using Unity.U2D.Physics;
using UnityEngine;

public class ContactReceiver : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    private PhysicsWorld m_World;

    private void OnEnable()
    {
        m_World = PhysicsWorld.Create();
        m_World.autoContactCallbacks = true;

        var body = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = Vector2.up * 3f
        });

        var shape = body.CreateShape(CircleGeometry.defaultGeometry);

        // One of the pair must have contactEvents enabled.
        shape.contactEvents = true;
        // Route callbacks here.
        shape.callbackTarget = this;
    }

    private void OnDisable() => m_World.Destroy();

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        // Always validate — shapes may have been destroyed before this fires.
        if (!beginEvent.shapeA.isValid || !beginEvent.shapeB.isValid)
            return;

        // Read the contact manifold via the contactId.
        var contact = beginEvent.contactId.contact;
        foreach (var pt in contact.manifold)
            Debug.Log($"Contact at {pt.point}");
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent)
    {
        // Called when the previously-touching pair separates.
    }
}
```

### Trigger Callbacks

Implement `PhysicsCallbacks.ITriggerCallback` to respond to trigger overlaps. **Both** the trigger shape and the visitor shape must have `triggerEvents = true`. The event exposes `.triggerShape` and `.visitorShape` (not `.shapeA`/`.shapeB`).

```csharp
// Source: PhysicsShapeTriggerCallback.cs (Primer example)
using Unity.U2D.Physics;
using UnityEngine;

public class TriggerReceiver : MonoBehaviour, PhysicsCallbacks.ITriggerCallback
{
    private PhysicsWorld m_World;

    private void OnEnable()
    {
        m_World = PhysicsWorld.Create();
        m_World.autoTriggerCallbacks = true;

        // Visitor: a dynamic body that falls into the trigger.
        var visitorBody = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = Vector2.up * 4f
        });
        var visitorShape = visitorBody.CreateShape(CircleGeometry.defaultGeometry);
        visitorShape.triggerEvents = true;
        visitorShape.callbackTarget = this;

        // Trigger: a static sensor zone.
        var triggerBody = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Static,
            position = Vector2.down * 2f
        });
        var triggerShapeDef = new PhysicsShapeDefinition { isTrigger = true };
        var triggerShape = triggerBody.CreateShape(CircleGeometry.defaultGeometry, triggerShapeDef);
        triggerShape.triggerEvents = true;          // required on both shapes
        triggerShape.callbackTarget = this;
    }

    private void OnDisable() => m_World.Destroy();

    public void OnTriggerBegin2D(PhysicsEvents.TriggerBeginEvent beginEvent)
    {
        // Tint the visitor when it enters the zone.
        if (!beginEvent.visitorShape.isValid)
            return;
        var mat = beginEvent.visitorShape.surfaceMaterial;
        mat.customColor = Color.cyan;
        beginEvent.visitorShape.surfaceMaterial = mat;
    }

    public void OnTriggerEnd2D(PhysicsEvents.TriggerEndEvent endEvent)
    {
        // Restore default color on exit.
        if (!endEvent.visitorShape.isValid)
            return;
        var mat = endEvent.visitorShape.surfaceMaterial;
        mat.customColor = Color.clear;
        endEvent.visitorShape.surfaceMaterial = mat;
    }
}
```

### Body Update Callbacks

Implement `PhysicsCallbacks.IBodyUpdateCallback` to be notified whenever a body's transform changes during simulation or it falls asleep. Set `body.callbackTarget = this` and enable `world.autoBodyUpdateCallbacks = true`. The event provides `.body`, `.transform`, and `.fellAsleep`.

```csharp
using Unity.U2D.Physics;
using UnityEngine;

public class BodyUpdateReceiver : MonoBehaviour, PhysicsCallbacks.IBodyUpdateCallback
{
    private PhysicsWorld m_World;
    private PhysicsBody m_TrackedBody;

    private void OnEnable()
    {
        m_World = PhysicsWorld.Create();
        m_World.autoBodyUpdateCallbacks = true;

        m_TrackedBody = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = Vector2.up * 3f
        });
        m_TrackedBody.CreateShape(CircleGeometry.defaultGeometry);

        // Route body-update callbacks to this script.
        m_TrackedBody.callbackTarget = this;
    }

    private void OnDisable() => m_World.Destroy();

    public void OnBodyUpdate2D(PhysicsEvents.BodyUpdateEvent bodyUpdateEvent)
    {
        if (bodyUpdateEvent.fellAsleep)
        {
            Debug.Log("Body fell asleep.");
            return;
        }

        // Mirror the physics transform back to a Unity Transform.
        var pt = bodyUpdateEvent.transform;
        transform.SetPositionAndRotation(pt.position, pt.rotation.quaternion);
    }
}
```

### Joint Threshold Callbacks

Implement `PhysicsCallbacks.IJointThresholdCallback` to receive a callback whenever a joint's constraint force or torque exceeds its `forceThreshold` / `torqueThreshold`. Set the joint's `callbackTarget` and enable `world.autoJointThresholdCallbacks = true`. The event exposes `.joint`.

```csharp
using Unity.U2D.Physics;
using UnityEngine;

public class JointThresholdReceiver : MonoBehaviour, PhysicsCallbacks.IJointThresholdCallback
{
    private PhysicsWorld m_World;

    private void OnEnable()
    {
        m_World = PhysicsWorld.Create();
        m_World.autoJointThresholdCallbacks = true;

        var anchor = m_World.CreateBody(); // static ground body

        var hanging = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = Vector2.down * 2f
        });
        hanging.CreateShape(CircleGeometry.defaultGeometry);

        // Create a distance joint between ground and the hanging body.
        var joint = m_World.CreateJoint(new PhysicsDistanceJointDefinition
        {
            bodyA = anchor,
            bodyB = hanging,
            localAnchorA = Vector2.zero,
            localAnchorB = Vector2.zero
        });

        // Fire the threshold callback when force exceeds 50 N.
        joint.forceThreshold = 50f;
        joint.callbackTarget = this;
    }

    private void OnDisable() => m_World.Destroy();

    public void OnJointThreshold2D(PhysicsEvents.JointThresholdEvent thresholdEvent)
    {
        Debug.Log($"Joint threshold exceeded: force={thresholdEvent.joint.currentConstraintForce}");
    }
}
```

### Contact Filter Callbacks

Implement `PhysicsCallbacks.IContactFilterCallback` to intercept contact creation and allow/reject it per pair. Set `shape.contactFilterCallbacks = true` and enable `world.contactFilterCallbacks = true`. **Critical:** this callback fires on any thread during simulation — it must be thread-safe and must not perform physics write operations.

```csharp
// Source: PhysicsShapeContactFiltering.cs (Primer example) and CustomFilter.cs (Sandbox example)
using Unity.U2D.Physics;
using UnityEngine;

public class ContactFilterReceiver : MonoBehaviour, PhysicsCallbacks.IContactFilterCallback
{
    private PhysicsWorld m_World;

    private void OnEnable()
    {
        m_World = PhysicsWorld.Create();
        m_World.contactFilterCallbacks = true;      // must be on at world level too

        var body1 = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = new Vector2(-0.25f, 0f)
        });
        var body2 = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = new Vector2(0.25f, 4f)
        });

        var shape1 = body1.CreateShape(CircleGeometry.defaultGeometry);
        var shape2 = body2.CreateShape(CircleGeometry.defaultGeometry);

        // Enable filter callbacks on both shapes.
        shape1.contactFilterCallbacks = true;
        shape2.contactFilterCallbacks = true;

        shape1.callbackTarget = shape2.callbackTarget = this;
    }

    private void OnDisable() => m_World.Destroy();

    // Return true  → allow contact.
    // Return false → suppress contact (no collision response this step).
    // THREAD-SAFE: no write operations allowed here.
    public bool OnContactFilter2D(PhysicsEvents.ContactFilterEvent contactFilterEvent)
    {
        // Allow everything except circle-vs-circle contacts.
        var aIsCircle = contactFilterEvent.shapeA.shapeType == PhysicsShape.ShapeType.Circle;
        var bIsCircle = contactFilterEvent.shapeB.shapeType == PhysicsShape.ShapeType.Circle;
        return !(aIsCircle && bIsCircle);
    }
}
```

### Pre-Solve Callbacks

Implement `PhysicsCallbacks.IPreSolveCallback` to disable a contact on a per-step basis before it reaches the solver. Set `shape.preSolveCallbacks = true` and enable `world.preSolveCallbacks = true`. Like contact filter, this fires on any thread — it must be thread-safe with no physics write operations. Only called for awake Dynamic bodies; never called for triggers.

```csharp
using Unity.U2D.Physics;
using UnityEngine;

// One-way platform: bodies can pass through from below but land on top.
public class OneWayPlatform : MonoBehaviour, PhysicsCallbacks.IPreSolveCallback
{
    private PhysicsWorld m_World;

    private void OnEnable()
    {
        m_World = PhysicsWorld.Create();
        m_World.preSolveCallbacks = true;           // must be on at world level

        // Static one-way platform.
        var platformBody = m_World.CreateBody();
        var platformDef = new PhysicsShapeDefinition { preSolveCallbacks = true };
        var platformShape = platformBody.CreateShape(
            PolygonGeometry.CreateBox(new Vector2(4f, 0.2f)), platformDef);
        platformShape.callbackTarget = this;

        // Dynamic body above the platform.
        var fallingBody = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = Vector2.up * 3f
        });
        fallingBody.CreateShape(CircleGeometry.defaultGeometry);
    }

    private void OnDisable() => m_World.Destroy();

    // Return true  → allow the contact this step.
    // Return false → disable the contact this step (body passes through).
    // THREAD-SAFE: no write operations allowed here.
    public bool OnPreSolve2D(PhysicsEvents.PreSolveEvent preSolveEvent)
    {
        // Allow contact only when the body approaches from above (normal points up).
        // preSolveEvent.normal always points from shapeA toward shapeB.
        return preSolveEvent.normal.y > 0f;
    }
}
```

## Automatic vs Manual Callbacks

### Automatic Callbacks (Recommended)
Enable automatic callback dispatch on the PhysicsWorld:

```csharp
// Enable automatic callbacks (called after simulation)
world.autoContactCallbacks = true;
world.autoTriggerCallbacks = true;
world.autoBodyUpdateCallbacks = true;
world.autoJointThresholdCallbacks = true;
```

### Manual Callback Invocation
Alternatively, manually invoke callbacks:

```csharp
// After world.Simulate(deltaTime)
world.SendAllCallbacks();

// Or send specific callback types
world.SendContactCallbacks();
world.SendTriggerCallbacks();
world.SendBodyUpdateCallbacks();
world.SendJointThresholdCallbacks();
```

**Important:** Avoid combining automatic and manual callback approaches to prevent duplicate event processing.

## Implementation Pattern

To implement callbacks in your scripts:

1. Inherit from the appropriate callback interface(s)
2. Implement required callback methods
3. Set `physicsShape.callbackTarget = this` to register the handler
4. Enable the appropriate event types on the shape
5. Enable automatic callbacks on the world (or call sendCallbacks manually)

## Best Practices

- **Enable only needed events** - Event generation has a performance cost
- **Use callbacks for gameplay logic** - Easier than manually reading event arrays
- **Use direct event reading for batch processing** - More efficient when processing many events
- **Don't modify physics state during callbacks** - Can cause undefined behavior
- **Use Contact Filter for conditional collision** - More efficient than disabling collisions after contact
- **Check IsValid before using handles** - Shapes/bodies may be destroyed between events

## Common Use Cases

### Damage on Impact

Use `OnContactBegin2D` to read the contact manifold and apply damage based on impact speed or category. Validate both shapes before acting — a previous callback in the same frame may have already destroyed one of them.

```csharp
// Source: Fragmenting.cs / GeometryIslands.cs (Sandbox examples)
using Unity.U2D.Physics;
using UnityEngine;

public class ImpactDamage : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    private PhysicsWorld m_World;
    private readonly PhysicsMask m_ProjectileMask = new(1);
    private readonly PhysicsMask m_TargetMask     = new(2);

    private void OnEnable()
    {
        m_World = PhysicsWorld.defaultWorld;
        m_World.autoContactCallbacks = true;

        // Projectile body.
        var projBody = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = Vector2.up * 5f,
            linearVelocity = Vector2.down * 20f,
            gravityScale = 0f,
            fastCollisionsAllowed = true
        });
        var projDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter
                { categories = m_ProjectileMask, contacts = m_TargetMask },
            contactEvents = true
        };
        var projShape = projBody.CreateShape(new CircleGeometry { radius = 0.15f }, projDef);
        projShape.callbackTarget = this;
    }

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        var shapeA = beginEvent.shapeA;
        var shapeB = beginEvent.shapeB;

        // Guard: a prior callback may have already destroyed one shape.
        if (!shapeA.isValid || !shapeB.isValid)
            return;

        var catA = shapeA.contactFilter.categories;
        var catB = shapeB.contactFilter.categories;

        // Identify projectile and target.
        var projShape   = catA == m_ProjectileMask ? shapeA : shapeB;
        var targetShape = catA == m_TargetMask     ? shapeA : shapeB;

        if (!projShape.isValid || !targetShape.isValid)
            return;

        // Read impact point from the manifold.
        var contact = beginEvent.contactId.contact;
        Debug.Log($"Impact at {contact.manifold.points[0].point}");

        // Destroy projectile on impact.
        projShape.body.Destroy();
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent) { }
}
```

### Trigger Zones

Use `OnTriggerBegin2D`/`OnTriggerEnd2D` to track which bodies are inside a zone. Both shapes in the pair must have `triggerEvents = true`; the event gives `.triggerShape` and `.visitorShape`.

```csharp
// Source: PhysicsShapeTriggerCallback.cs (Primer example)
using System.Collections.Generic;
using Unity.U2D.Physics;
using UnityEngine;

public class TriggerZone : MonoBehaviour, PhysicsCallbacks.ITriggerCallback
{
    private PhysicsWorld m_World;
    private readonly HashSet<PhysicsShape> m_InsideZone = new();

    private void OnEnable()
    {
        m_World = PhysicsWorld.Create();
        m_World.autoTriggerCallbacks = true;

        // Zone: large static trigger covering a region.
        var zoneBody = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Static,
            position = Vector2.zero
        });
        var zoneDef = new PhysicsShapeDefinition { isTrigger = true };
        var zoneShape = zoneBody.CreateShape(
            PolygonGeometry.CreateBox(new Vector2(6f, 4f)), zoneDef);
        zoneShape.triggerEvents = true;
        zoneShape.callbackTarget = this;

        // Visitor: dynamic body entering the zone.
        var visitorBody = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = Vector2.up * 5f
        });
        var visitorShape = visitorBody.CreateShape(CircleGeometry.defaultGeometry);
        visitorShape.triggerEvents = true;           // required on both shapes
        visitorShape.callbackTarget = this;
    }

    private void OnDisable() => m_World.Destroy();

    public void OnTriggerBegin2D(PhysicsEvents.TriggerBeginEvent beginEvent)
    {
        if (beginEvent.visitorShape.isValid)
            m_InsideZone.Add(beginEvent.visitorShape);

        Debug.Log($"Bodies in zone: {m_InsideZone.Count}");
    }

    public void OnTriggerEnd2D(PhysicsEvents.TriggerEndEvent endEvent)
    {
        m_InsideZone.Remove(endEvent.visitorShape);
        Debug.Log($"Bodies in zone: {m_InsideZone.Count}");
    }
}
```

### Breaking Joints

Set `joint.forceThreshold` (and/or `joint.torqueThreshold`) then implement `PhysicsCallbacks.IJointThresholdCallback`. In the callback, call `joint.Destroy()` to break the joint. There is no `world.DestroyJoint()` method — destruction is always via `joint.Destroy()`.

```csharp
using Unity.U2D.Physics;
using UnityEngine;

public class BreakableJoint : MonoBehaviour, PhysicsCallbacks.IJointThresholdCallback
{
    private PhysicsWorld m_World;

    private void OnEnable()
    {
        m_World = PhysicsWorld.Create();
        m_World.autoJointThresholdCallbacks = true;

        // Ceiling anchor (static).
        var anchor = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Static,
            position = Vector2.up * 5f
        });

        // Heavy hanging body.
        var hanging = m_World.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = Vector2.zero
        });
        hanging.CreateShape(PolygonGeometry.CreateBox(Vector2.one));

        // Fixed joint connecting them.
        var joint = m_World.CreateJoint(new PhysicsFixedJointDefinition
        {
            bodyA = anchor,
            bodyB = hanging
        });

        // Break when constraint force exceeds 200 N.
        joint.forceThreshold = 200f;
        joint.callbackTarget = this;
    }

    private void OnDisable() => m_World.Destroy();

    public void OnJointThreshold2D(PhysicsEvents.JointThresholdEvent thresholdEvent)
    {
        var joint = thresholdEvent.joint;
        if (!joint.isValid)
            return;

        Debug.Log("Joint broken!");
        // Destroy the joint — NOT world.DestroyJoint(); that method does not exist.
        joint.Destroy();
    }
}
```

### Conditional Collision

Use `OnContactFilter2D` to allow collision only between shapes that share the same "team" or color group, while letting each team still collide with neutral geometry. Store team identity in `shape.userData.intValue` — reading `userData` is thread-safe.

```csharp
// Source: CustomFilter.cs (Sandbox example — color-group collision)
using Unity.U2D.Physics;
using UnityEngine;

public class TeamFilter : MonoBehaviour, PhysicsCallbacks.IContactFilterCallback
{
    private PhysicsWorld m_World;

    private void OnEnable()
    {
        m_World = PhysicsWorld.defaultWorld;
        m_World.contactFilterCallbacks = true;

        // Create bodies in two teams (0 = red, 1 = blue).
        for (var i = 0; i < 4; ++i)
        {
            var body = m_World.CreateBody(new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                position = new Vector2(-3f + i * 2f, 3f)
            });

            var team = i % 2; // alternate teams
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilterCallbacks = true,
                surfaceMaterial = new PhysicsShape.SurfaceMaterial
                {
                    customColor = team == 0 ? Color.red : Color.blue
                }
            };
            var shape = body.CreateShape(CircleGeometry.defaultGeometry, shapeDef);

            // Store team index in userData — safe to read from any thread.
            shape.userData = new PhysicsUserData { intValue = team, boolValue = true };
            shape.callbackTarget = this;
        }
    }

    // Called on any thread — must be thread-safe, no write operations.
    public bool OnContactFilter2D(PhysicsEvents.ContactFilterEvent contactFilterEvent)
    {
        var udA = contactFilterEvent.shapeA.userData;
        var udB = contactFilterEvent.shapeB.userData;

        // If either shape is not a "team" shape, allow contact normally.
        if (!udA.boolValue || !udB.boolValue)
            return true;

        // Suppress contact between different teams; allow within same team.
        return udA.intValue == udB.intValue;
    }
}
```
