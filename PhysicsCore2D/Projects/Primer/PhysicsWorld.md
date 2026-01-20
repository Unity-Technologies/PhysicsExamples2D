# PhysicsWorld

A `PhysicsWorld` serves as a container for all other physics object types, acting as the stage where these objects exist and interact.
Each `PhysicsWorld` can be simulated independently and in parallel therefore physics objects within one world cannot interact with objects in another world.

Although Unity supports multiple `PhysicsWorld` instances, it’s most common to use just one.
For convenience, Unity automatically creates a default `PhysicsWorld`, which you can always access via `PhysicsWorld.defaultWorld`.

```csharp
void Run()
{
    // Get the default world.
    var world = PhysicsWorld.defaultWorld;

    // Display the current gravity.
    Debug.Log(world.gravity);
}
```

## Creating Worlds

Unity allows you to create multiple worlds which are both isolation and able to be simulated in parallel to other worlds.
They can be set to simulate automatically either during the `FixedUpdate` or `Update` or manually via `Script`.

```csharp
void Run()
{
    // Create a physics world with default settings.
    var world = PhysicsWorld.Create();

    // Create a body as a test.
    var body = world.CreateBody();
    
    // Destroy the world.
    // NOTE: The body we created will be implicitly destroyed too.
    world.Destroy();
}
```

## Manual Simulation

If a `PhysicsWorld` has a [simulation mode](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-simulationMode.html) set to [Script](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/SimulationMode2D.Script.html),
the world can be explicitly simulated in script using [PhysicsWorld.Simulate()](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.Simulate.html).

In all other simulation modes, Unity will automatically perform a simulation step at the appropriate time.

```csharp
void Run()
{
    // Create a world definition with `Script` simulation.
    var worldDefinition = new PhysicsWorldDefinition { simulationMode = SimulationMode2D.Script };
    
    // Create a physics world with default settings.
    var world = PhysicsWorld.Create(worldDefinition);

    // Create a body as a test.
    var body = world.CreateBody();
    
    // Perform a single simulation step.
    world.Simulate(1f/60f);
    
    // Destroy the world.
    // NOTE: The body we created will be implicitly destroyed too.
    world.Destroy();
}
```


## Lifetime

The physics system supports the creation of various object types, each with a lifetime that is tied to another object.

A `PhysicsWorld` directly manages the lifetime of any `PhysicsBody` it contains—when a `PhysicsWorld` is destroyed, all `PhysicsBody` instances within it are also destroyed.
Similarly, any `PhysicsShape` or `PhysicsChain` attached to a `PhysicsBody` will be destroyed when that `PhysicsBody` is removed.
Additionally, any `PhysicsJoint` connected to a `PhysicsBody` will be destroyed if either of the connected `PhysicsBody` objects is destroyed.

In the Editor, the default physics world is created when the Editor starts and is recreated whenever you enter or exit `Play` mode.
In a player build, the default world is initialized when the application starts and is destroyed when the application exits.

Any additional physics world instances must be created and destroyed through script, giving you explicit control over their lifetimes.
However, in the Editor, entering or exiting `Play` mode will automatically destroy any worlds you have created explicitly.
