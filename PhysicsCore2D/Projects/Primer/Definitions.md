# Definitions

A common approach in Unity when creating various objects is to instantiate them with default settings and then configure their properties—often while the object is already active.
While this method feels straightforward and intuitive, it has a significant drawback: modifying properties or calling methods on some objects can be CPU-intensive, sometimes resulting in redundant work being performed multiple times.
This is particularly true for physics objects, leading to decreased performance, especially when creating many objects in batches.

To address this, the low-level physics system introduces the concept of a Definition.
A definition encapsulates a complete set of values for a physics object and can be supplied at the time of creation.
By providing a definition up front, you configure the physics object before it becomes active, eliminating the performance overhead caused by repeated configuration steps.

In essence, a definition functions as a blueprint or preset, containing all the necessary settings for a specific type of physics object.
Each object type has its own corresponding definition type, for example:
* A [PhysicsWorld](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorld.html) uses a [PhysicsWorldDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorldDefinition.html)
* A [PhysicsBody](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody.html) uses a [PhysicsBodyDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBodyDefinition.html)
* A [PhysicsShape](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.html) uses a [PhysicsShapeDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.html)
* etc.

**NOTE:** Definitions are only used for physics objects that can exist in a `PhysicsWorld` along with creating a `PhysicsWorld` itself.

## Default Definitions

While definitions offer great flexibility, specifying every single property each time would be tedious.
To streamline this process, every definition comes with a built-in default configuration, known as the default definition.
You can quickly access these defaults using the static `defaultDefinition` property on each definition type like this:
* [PhysicsWorldDefinition.defaultDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsWorldDefinition-defaultDefinition.html)
* [PhysicsBodyDefinition.defaultDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBodyDefinition-defaultDefinition.html)
* [PhysicsShapeDefinition.defaultDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.html)
* etc.

You can also get a default definition implicitly by using the C# `new` keyword with the default constructor of the definition type:

```csharp
void Run()
{
    // Both create a default body definition.
    var bodyDef1 = PhysicsBodyDefinition.defaultDefinition;
    var bodyDef2 = new PhysicsBodyDefinition();
    
    // Create a default body definition but override specific properties.
    var bodyDef3 = new PhysicsBodyDefinition
    {
        linearVelocity = Vector2.right * 4f,
        gravityScale = 0f
    };
}
```

**Note:** The values in a default definition are not fixed, you can customize these defaults to suit your project's needs. For more information, see [Project Settings](PhysicsLowLevelSettings2D.md).

## Creating Objects with Definitions

With a definition, you can pass it directly into the create method for the respective physics object type like so:

```csharp
void Run()
{
    // ...

    // These all create a body using the default definition.
    var body1 = world.CreateBody(PhysicsBodyDefinition.default);
    var body2 = world.CreateBody(new PhysicsBodyDefinition());
    var body3 = world.CreateBody(); // Shortcut for default
        
    // Create a custom body definition.
    var bodyDef = new PhysicsBodyDefinition
    {
        linearVelocity = Vector2.right * 4f,
        gravityScale = 0f
    };
    
    // Create a body using our custom definition.
    var body4 = world.CreateBody(bodyDef);
    
    // IMPORTANT: Modifying 'bodyDef' after 'body4' is created does NOT change 'body4'.
    // Definitions are 'structs' and are used by-value (like copies).
    bodyDef.gravityScale = -9.81f;
    
    // Create another body using the modified definition.
    var body5 = world.CreateBody(bodyDef);
}
```

## Batch Object Creation

Definitions are especially powerful when used for batch creation, which is supported for both `PhysicsBody` and `PhysicsShape` objects.
By supplying a definition, you can create multiple physics objects simultaneously, eliminating the overhead of configuring each object individually.
```csharp
void Run()
{
    // ...

    // Create a custom body definition.
    var bodyDef = new PhysicsBodyDefinition
    {
        linearVelocity = Vector2.right * 4f,
        gravityScale = 0f
    };
    
    // Create 500 bodies with our definition.
    var bodies = world.CreateBodyBatch(bodyDef, 500);
}
```

## Definitions vs Respective Object Properties

In nearly all cases, the properties available in a definition are named identically to the respective property on the object instance.
This makes it easier to relate the definition property to a property that can be used after an object has been created.
For example:
- [PhysicsBodyDefinition.type](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBodyDefinition-type.html) and [PhysicsBody.type](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-type.html)
- [PhysicsBodyDefinition.linearVelocity](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBodyDefinition-linearVelocity.html) and [PhysicsBody.linearVelocity](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody-linearVelocity.html)
- [PhysicsShapeDefinition.isTrigger](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShapeDefinition-isTrigger.html) and [PhysicsShape.isTrigger](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape-isTrigger.html)
- etc.

## Definition Extras

Because definitions are `structs`, they are value types—meaning they are copied whenever they are used or passed around.
This makes it easy to reuse and modify definitions without affecting any objects that have already been created from them.

If you need to pass definitions around within your code, it’s recommended to use the `ref` or `in` keywords to pass them by reference and avoid unnecessary copying.
Since definitions are relatively large structs, using these keywords can help prevent memory overhead, especially in code that runs at a high frequency.

All definitions are also designed to be serializable, so you can conveniently expose and edit them in the Unity Editor’s Inspector when creating components.
Other structs involved in configuring objects or performing queries are also serializable for the same reason.