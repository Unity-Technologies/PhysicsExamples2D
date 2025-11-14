# PhysicsBody

A `PhysicsBody` exists within a `PhysicsWorld`.
It primarily maintains its position, rotation, linear velocity and angular velocity for movement within the `PhysicsWorld`.
You can attach `PhysicsShape` instances to it to define regions relative to the body and configure their behaviors.

## Body Definition

A `PhysicsBody` uses its own [PhysicsBodyDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBodyDefinition.html) to specify how the body should be created.
While a `PhysicsBodyDefinition` provides many configuration options, a few key settings are especially important to highlight, as properly configuring them during object creation can have a significant impact on performance.

- **Body Types**: A `PhysicsBody` can be one of three general types which actively controls its dynamic behaviour:
  - **Static**: A static (non-moving) body is typically used when you want an object to have a fixed position and rotation. This type of body does not move or respond to forces, and it won’t interact directly with other body types.  However, dynamic bodies can interact with static bodies, though in these cases, it’s the dynamic body that responds to the interaction.  Static bodies are computationally inexpensive because the physics system doesn’t need to update their position or calculate collisions for them.
They are commonly used for non-moving elements of a level, such as walls or platforms, that still need to interact with dynamic bodies in the game.
  - **Dynamic**: A dynamic body is essentially the opposite of a static body, its primary purpose is to move and interact with all other body types. Dynamic bodies respond to external forces such as collisions and gravity, and offer numerous features for controlling their motion and behavior. Because dynamic bodies are involved in complex interactions and physics calculations, they are more computationally expensive than static bodies. Dynamic bodies generate contacts and can be linked with joints, both of which act as constraints that are automatically managed by the physics solver.
  - **Kinematic**: A kinematic body serves as a middle ground between static and dynamic bodies. Like a static body, it does not respond to external forces, but like a dynamic body, it can move when explicitly controlled by the user using linear and angular velocity. While kinematic bodies themselves do not interact with other body types, dynamic bodies can interact with kinematic bodies, though in these cases, it’s the dynamic body that reacts to the interaction.
- **Position and Rotation**: A `PhysicsBody` has a position and rotation which is critical to performance to be correct when the object is created.
- **Transform Write Mode**: A `PhysicsBody` has a position and rotation, but it's often desirable to write this pose to a specific Unity [Transform](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Transform.html). The specific `Transform` object is selected using [PhysicsBody.transformObject](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-transformObject.html) with the method used to write to the `Transform` being controlled using [PhysicsBody.transformWriteMode](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody.TransformWriteMode.html).
- **Linear and Angular Velocity**: A `PhysicsBody` can have an initial linear and angular velocity. This is useful for various dynamic objects such as projectiles.
- **Linear and Angular Damping**: A `PhysicsBody` can automatically reduce (dampen) both the linear and angular velocity over time. This is useful when you require movement to eventually cease.
- **Body Constraints**: A PhysicsBody has three degrees of freedom (DoF), two for position and one for rotation. Body constraints let you remove these DoF individually; for example, you might prevent the body from rotating.

## Body Origin and Center of Mass

A `PhysicsBody` has two key positions to understand:
- **Origin**: The reference point to which `PhysicsShape` and `PhysicsJoint` are attached.
- **Center of mass**: A local position computed from the mass distribution of attached `PhysicsShape` instances (or explicitly overridden).

These are often different, so treat them as separate properties. For example, setting the `PhysicsBody`’s position changes its `origin`, but not its local `center of mass`.

## Sleeping

Simulating many `PhysicsBody` instances is expensive, so performance improves as the number of active bodies decreases.
When a `PhysicsBody` (or a group of bodies) comes to rest, the physics system puts it to sleep, reducing CPU usage until interaction occurs.
You should typically leave the sleeping configuration at defaults but you can control and monitor the sleeping of a `PhysicsBody` using:
- **Allowing Sleep**: [PhysicsBody.sleepingAllowed](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-sleepingAllowed.html)
- **Sleeping Threshold**: [PhysicsBody.sleepingThreshold](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-sleepThreshold.html)
- **Monitor/Change Sleep State**: [PhysicsBody.awake](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-awake.html) 

Key points:
- Sleeping reduces simulation cost; sleeping bodies consume minimal CPU.
- Treat sleep as an automatic performance feature and leave at defaults.
- A sleeping `PhysicsBody` wakes if:
  - It is collided with by an awake body.
  - An attached `PhysicsJoint` or `Contact` is destroyed.
  - You wake it manually.
- Bodies are virtually grouped into “contact islands” via mutual `Contact` on their attached `PhysicsShape` instances. Waking any body in an island wakes all bodies in that island.
- Avoid using the sleep state as a general indicator of “not moving,” because a body may wake due to island participation and still remain stationary.

## Fast Collisions

The physics simulation is `discrete`: each `PhysicsBody` moves from one transform (position and rotation) to the next per step.
A body may travel a large distance in a single step, causing it (and its attached `PhysicsShape`) to pass through others, known as `tunnelling`.

By default, continuous collision detection (CCD) prevents tunnelling only between `dynamic` and `static` bodies.
CCD sweeps the attached `PhysicsShape` from the old transform to the new, computes a time of impact (TOI), and moves the `PhysicsBody` to the TOI transform.

Because CCD is expensive when many interactions occur, it’s limited to `dynamic`-vs-`static` by default.
For fast-moving cases (e.g., projectiles) that must contact any body type, a `PhysicsBody` can opt in by enabling `PhysicsBody.fastCollisionsAllowed`, which enables CCD for that body.

## Fast Rotation

During a physics simulation, a limit is imposed on how fast a `PhysicsBody` can rotate to keep the simulation stable for rotating objects.
The reason for this limitation is that when a `PhysicsBody` rotates fast, any attached `PhysicsShape` may causes contact tunneling and large forces.

In some circumstances, this tunneling can be avoided when the `PhysicsShape` is circular i.e. wheels, balls etc.
In this case, rotating fast may be desirable so you can enable this feature using [PhysicsBody.fastRotationAllowed](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-fastRotationAllowed.html).

## Creation and Destruction

A `PhysicsBody` is created and destroyed in a `PhysicsWorld` like this: 

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create a new body using the default definition.
    var body = world.CreateBody(); 
    
    // Destroy it immediately!
    body.Destroy();
}
```

A `PhysicsBody` is the only physics object that controls position, rotation, and velocity within the world, but it does not define any area on its own.
To specify an area or areas, you attach multiple `PhysicsShape` to the `PhysicsBody`.
Each `PhysicsShape` defines an area relative to the `PhysicsBody`'s position and rotation.
For more information, see [PhysicsShape](PhysicsShape.md).


While you can create a body using a method of the `PhysicsWorld`, all physics object types have their own `static` method that allows you to create that object type directly, all being called `Create` like so:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create a new body using the static `Create` method.
    var body = PhysicsBody.Create(world); 
    
    // Destroy it immediately!
    body.Destroy();
}
```
The above two examples are functionality equivalent and their use is simply related to personal preference.
The API is consistent for all physics object types and is available for `PhysicsBody`, `PhysicsShape` and all `PhysicsJoint` types.

## Batch Creation and Destruction

A batch of `PhysicsBody` can be created in a `PhysicsWorld` using a single call using a single `PhysicsBodyDefinition` for all `PhysicsBody` created like so:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create a default body definition.
    var bodyDef = PhysicsBodyDefinition.defaultDefinition;
    
    // Create 10 bodies using the same body definition.
    // NOTE: 10 bodies are returning in a NativeArray.
    using var bodies = world.CreateBodyBatch(bodyDef, 10); 
    
    // Destroy them all immediately!
    PhysicsWorld.DestroyBodyBatch(bodies);
}
```
A more typical use-case is creating multiple `PhysicsBody`, each with their own individual `PhysicsBodyDefinition` like so:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    const int bodyCount = 10;
    
    // Create the body definitions.
    var bodyDefs = new NativeArray<PhysicsBodyDefinition>(bodyCount, Allocator.Temp);
    for (var i = 0; i < bodyCount; ++i)
        bodyDefs[i] = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = Vector2.right * i };

    // Create the bodies.
    using var bodies = world.CreateBodyBatch(bodyDefs);

    // Dispose of the body definitions.
    bodyDefs.Dispose();

    // Destroy them all immediately!
    PhysicsWorld.DestroyBodyBatch(bodies);
}
```