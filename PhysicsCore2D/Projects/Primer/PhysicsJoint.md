# PhysicsJoint

A [PhysicsJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.html) creates a dynamic connection or constraint between two `PhysicsBody` objects.
Joints control how bodies move relative to each other, such as maintaining a distance, restricting motion along an axis, or preventing rotation.
They are commonly used to simulate real-world mechanical behaviors.

`PhysicsJoint` is the common base type for all joints. It provides functionality shared by every joint but does not impose any constraints on its own.

There are multiple joint types available via [PhysicsJoint.JointType](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint.JointType.html), each implementing a specific constraint behavior:
- **PhysicsDistanceJoint**: Keeps two bodies a set distance apart; supports a spring, motor, and linear limits. The linear counterpart to `PhysicsHingeJoint`.
- **PhysicsFixedJoint**: “Welds” two bodies together, locking relative position and rotation. Strength is configurable via frequency and damping.
- **PhysicsHingeJoint**: Lets two bodies rotate around a shared point (hinge); supports an angular spring, angular motor, and angular limits. The rotational counterpart to `PhysicsDistanceJoint` (aka a revolute joint).
- **PhysicsIgnoreJoint**: Makes shapes on the connected bodies ignore each other, preventing collisions and triggers (aka a filter joint).
- **PhysicsRelativeJoint**: Controls relative linear and angular velocity using springs, motors, and limits. Often used for top-down friction with a static ground body (aka a motor joint).
- **PhysicsSliderJoint**: Constrains motion to a single relative axis; supports a spring, motor, and linear limits (aka a prismatic joint).
- **PhysicsWheelJoint**: Constrains motion along an axle and allows wheel-like rotation; can include a spring, motor, and an axle linear limit.

## Creation and Destruction

A specific type of `PhysicsJoint` is created and destroyed in a `PhysicsWorld` like this:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create a pair of bodies using the default definition.
    var body1 = world.CreateBody(); 
    var body2 = world.CreateBody();

    // Create a distance joint definition.
    var distanceJointDef = new PhysicsDistanceJointDefinition
    {
        bodyA = body1,
        bodyB = body2,
        distance = 2f
    };
    
    // Create a distance joint.
    world.CreateJoint(distanceJointDef);
    
    // Destroy it immediately!
    // NOTE: Destroying either body will also destroy any connected joint.
    body1.Destroy();
    body1.Destroy();
}
```

## Shared Functionality

All `PhysicsJoint` share common functionality making it easier and consistent to configure them.
Some of the common functionality is:
- **Attachment**: The ability to specify two `PhysicsBody` ([bodyA](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-bodyA.html) and [bodyB](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-bodyB.html)) and the local anchors on both bodies ([localAnchorA](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-localAnchorA.html) and [localAnchorB](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-localAnchorB.html)).
- **Collisions**: The ability to configure if the specified two `PhysicsBody` can [collide](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-collideConnected.html) with each other.
- **Thresholds**: A limit can be specified for how much [force](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-forceThreshold.html) and [torque](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-torqueThreshold.html) the joint can apply. When exceeded, a [PhysicsEvent.JointThresholdEvent](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.JointThresholdEvent.html) is produced.
- **Tuning**: The ability to configure the joint [damping](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-tuningDamping.html) and [frequency](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-tuningFrequency.html).
- **Status**: The ability to read the current status of the joint including [constraint-force](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-currentConstraintForce.html), [constraint-torque](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-currentConstraintTorque.html), [linear-separation-error](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-currentLinearSeparationError.html) and [angular-separation-error](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsJoint-currentAngularSeparationError.html). 

