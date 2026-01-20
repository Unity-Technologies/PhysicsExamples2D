# Physics Composer

The [PhysicsComposer](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsComposer.html) composes input geometries using boolean operations ([PhysicsComposer.Operation](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsComposer.Operation.html): OR, AND, NOT, XOR).

- Add or remove layers with [Add](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsComposer.AddLayer.html) and [Remove](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsComposer.RemoveLayer.html), specifying each layer’s geometry, operation, and order.
- From the composed layers, generate filled polygons via [PhysicsComposer.CreatePolygonGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsComposer.CreatePolygonGeometry.html) or outlines via [PhysicsComposer.CreateChainGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsComposer.CreateChainGeometry.html).

Supported layer geometries include [CircleGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html), [CapsuleGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html), [PolygonGeometry](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html), geometry read from a `PhysicsShape`, or any closed region defined by a set of `Vector2` points.

## Creation and Destruction

You can create a `PhysicsComposer` (on any thread) like so:

```csharp
void Run()
{
    // Create a physics composer.
    // NOTE: The temp allocator is used here but various allocators are available.
    // The allocator here is to store the layers and their geometry, not the output.
    var physicsComposer = PhysicsComposer.Create(Allocator.Temp);

    // Create some geometry.
    var circleGeometry = new CircleGeometry { radius = 2f };
    var capsuleGeometry = new CapsuleGeometry { center1 = Vector2.down, center2 = Vector2.up, radius = 0.5f };

    // Add the circle geometry.
    physicsComposer.AddLayer(circleGeometry, PhysicsTransform.identity, PhysicsComposer.Operation.OR);
    
    // Add the capsule geometry and XOR it with the previous layer.
    physicsComposer.AddLayer(capsuleGeometry, PhysicsTransform.identity, PhysicsComposer.Operation.XOR);

    // Create polygon geometry by composing all the layer geometry.
    // NOTE: The temp allocator is used here but various allocators are available.
    // The allocator is used to allocator the requested output.
    using var outputGeometry = physicsComposer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Temp);
    
    // We can destroy the composer now.
    physicsComposer.Destroy();

    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create a body.
    var body = world.CreateBody();

    // Create the shapes on the body using the output geometry.
    using var shapes = body.CreateShapeBatch(outputGeometry, PhysicsShapeDefinition.defaultDefinition);

    // Destroy the body.
    // NOTE: This makes the code useless but is an example so this destroys the body and all its shapes.
    body.Destroy();
}
```
