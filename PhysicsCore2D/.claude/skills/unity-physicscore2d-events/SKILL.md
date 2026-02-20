# Unity PhysicsCore2D - Events and Callbacks

Expert guidance on using physics events and callback systems in Unity PhysicsCore2D.

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
// After world.Step()
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
```csharp
public class MyContactHandler : MonoBehaviour, PhysicsCallback.IContactCallback
{
    private PhysicsShape m_PhysicsShape;

    void Start()
    {
        // Register this script as the callback target
        m_PhysicsShape.callbackTarget = this;
    }

    public void OnContactBegin2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
    {
        // Handle contact begin
    }

    public void OnContactEnd2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
    {
        // Handle contact end
    }
}
```

### Trigger Callbacks
```csharp
public class MyTriggerHandler : MonoBehaviour, PhysicsCallback.ITriggerCallback
{
    public void OnTriggerBegin2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
    {
        // Handle trigger begin
    }

    public void OnTriggerEnd2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
    {
        // Handle trigger end
    }
}
```

### Body Update Callbacks
```csharp
public class MyBodyUpdateHandler : MonoBehaviour, PhysicsCallback.IBodyUpdate
{
    public void OnBodyUpdate2D(PhysicsWorld world, PhysicsBody body)
    {
        // Handle body update during simulation
    }
}
```

### Joint Threshold Callbacks
```csharp
public class MyJointHandler : MonoBehaviour, PhysicsCallback.IJointThreshold
{
    public void OnJointThreshold2D(PhysicsWorld world, PhysicsJoint joint, float force, float torque)
    {
        // Handle joint threshold exceeded
        // Break joint or trigger effects
    }
}
```

### Contact Filter Callbacks
```csharp
public class MyContactFilter : MonoBehaviour, PhysicsCallback.IContactFilterCallback
{
    public bool OnContactFilter2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
    {
        // Return true to allow contact, false to prevent it
        return true;
    }
}
```

### Pre-Solve Callbacks
```csharp
public class MyPreSolveHandler : MonoBehaviour, PhysicsCallback.IPreSolveCallback
{
    public void OnPreSolve2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB, ref ContactManifold manifold)
    {
        // Modify contact before solving
        // Can adjust friction, restitution, etc.
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
// After world.Step()
world.sendAllCallbacks();

// Or send specific callback types
world.sendContactCallbacks();
world.sendTriggerCallbacks();
world.sendBodyUpdateCallbacks();
world.sendJointThresholdCallbacks();
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
```csharp
public void OnContactBegin2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
{
    // Get impact velocity from contact manifold
    // Apply damage based on velocity
}
```

### Trigger Zones
```csharp
public void OnTriggerBegin2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
{
    // Player entered trigger zone
    // Activate trap, spawn enemies, etc.
}
```

### Breaking Joints
```csharp
public void OnJointThreshold2D(PhysicsWorld world, PhysicsJoint joint, float force, float torque)
{
    // Joint exceeded force threshold
    world.DestroyJoint(joint);
}
```

### Conditional Collision
```csharp
public bool OnContactFilter2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
{
    // Allow one-way platforms
    // Prevent friendly fire
    // etc.
    return shouldCollide;
}
```
