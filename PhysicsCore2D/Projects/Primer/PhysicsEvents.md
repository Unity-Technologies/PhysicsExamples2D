# Physics Events and Callbacks

When a `PhysicsWorld` is simulated, multiple events are produced.
When the simulation is complete, these events are available to be read.

The events that are produced after the simulation has finished are:
* `PhysicsWorld.contactHitEvents` - When a contact for a pair of shapes starts beyond a specified threshold speed.
* `PhysicsWorld.contactBeginEvents` - When contact starts for a pair of shapes.
* `PhysicsWorld.contactEndEvents` - When contact ends for a pair of shapes.
* `PhysicsWorld.triggerBeginEvents` - When a trigger overlap starts for a pair of shapes.
* `PhysicsWorld.triggerEndEvents` - When a trigger overlap ends for a pair of shapes.
* `PhysicsWorld.bodyUpdateEvents` - When a body is updated during the simulation.
* `PhysicsWorld.jointThresholdEvents` - When a joint exceeds its force or torque threshold.

When reading these events, they are provided using direct memory access via a `ReadOnlySpan<(event-type)>`.

For the shape events to be produced, you must explicitly enable them on the shapes that require them using:
* `PhysicsShape.hitEvents` - Set to true/false depending on whether you want contact hit events or not.
* `PhysicsShape.contactEvents` - Set to true/false depending on whether you want the contact begin/end events or not.
* `PhysicsShape.triggerEvents` - Set to true/false depending on whether you want the trigger begin/end events or not.

### Event Callbacks

While reading events is useful, they are simply an unordered list of events which can be difficult to use if you want to process an event for a specific object.
To deal with this issue, **event callbacks** offer a more powerful way to receive events directly on the objects involved.
Callbacks are also available for events that happen during the simulation such as the ability to filter a contact (decide if it should occur) or before the contact goes to the solver.

Callbacks will essentially iterate over the current events in the world and perform a callback to a target `MonoBehaviour` that the object specifies.
The actual callback methods for each event type are defined as interfaces which you simply implement:
* `PhysicsCallback.IContactCallback`
* `PhysicsCallback.ITriggerCallback`
* `PhysicsCallback.IBodyUpdate`
* `PhysicsCallback.IJointThreshold`
* `PhysicsCallback.IContactFilterCallback`
* `PhysicsCallback.IPreSolveCallback`

To receive callbacks, an object (your script) must be set as the `callbackTarget` for the `PhysicsShape` (or `PhysicsBody`, `PhysicsJoint`).
This target script must also implement the appropriate interface(s) above for the event type(s) you're interested in.

Here's an example for receiving contact begin and contact end callbacks for a `PhysicsShape`:

```csharp
// This class implements the IContactCallback interface to receive contact events.
class MyTest : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    PhysicsBody m_PhysicsBody;
    PhysicsShape m_PhysicsShape;
    
    void OnEnable()
    {
        // Get the main physics world.
        var world = PhysicsWorld.defaultWorld;
        
        // Create a body and a shape.
        m_PhysicsBody = world.CreateBody();
        m_PhysicsShape = m_PhysicsBody.CreateShape(CircleGeometry.defaultGeometry);
        
        // Enable contact events for this shape. Without this, no events
        // will be generated for this shape. (Note: If another shape contacts this one
        // and *that* shape has contact events enabled, an event *will* be produced,
        // but this shape won't receive a callback without a callback target.)
        m_PhysicsShape.contactEvents = true;
        
        // Designate this script as the target for this shape's events.
        // Since this script implements IContactCallback, it will receive
        // ContactBeginEvent and ContactEndEvent calls.
        m_PhysicsShape.callbackTarget = this;
    }
    
    void OnDisable()
    {
        // Destroy the body (which also destroys its attached shapes).
        m_PhysicsBody.Destroy();
    }
    
    // This method is called when our shape starts touching another shape.
    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        Debug.Log("OnContactBegin2D");
    }

    // This method is called when our shape stops touching another shape.
    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent)
    {
        Debug.Log("OnContactEnd2D");
    }
}
```

Using interfaces for callbacks is more robust than relying on "magic" method names because with interfaces, errors will occur at compile time.
C# also allows explicit interface implementation, which keeps the interface methods "hidden" unless accessed directly through the interface itself:

```csharp
class MyTest : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    // ...
        
    // Explicit implementation for IContactCallback.OnContactBegin2D.
    void PhysicsCallbacks.IContactCallback.OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        Debug.Log("OnContactBegin2D");
    }

    // Explicit implementation for IContactCallback.OnContactEnd2D.
    void PhysicsCallbacks.IContactCallback.OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent)
    {
        Debug.Log("OnContactEnd2D");
    }
}
```

Even with the above configuration, the specific callbacks won't automatically happen unless you allow them on the `PhysicsWorld`.
This is done because the physics system tries to do as little work as possible by default to keep performance high.
You can configure automatically sending callbacks using:
* `PhysicsWorld.autoContactCallbacks`
* `PhysicsWorld.autoTriggerCallbacks`
* `PhysicsWorld.autoBodyUpdateCallbacks`
* `PhysicsWorld.autoJointThresholdCallbacks`

Callbacks can also be sent manually using:
* `PhysicsWorld.sendAllCallbacks`
* `PhysicsWorld.sendContactCallbacks`
* `PhysicsWorld.sendTriggerCallbacks`
* `PhysicsWorld.sendBodyUpdateCallbacks`
* `PhysicsWorld.sendJointThresholdcallbacks`

<b>NOTE:</b> You should avoid calling the manual callbacks if you have automatic callbacks enabled as you wil receive callbacks for the same events more than once which is wasteful.
