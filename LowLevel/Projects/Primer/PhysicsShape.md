# PhysicsShape

A `PhysicsShape` is a fundamental geometric region you attach to a `PhysicsBody`.
It defines a region in space relative to the `PhysicsBody`’s `origin` (position and rotation), and governs interactions—such as touching or overlapping—with other `PhysicsShape` objects.
You can add any number of `PhysicsShape` instances to a single `PhysicsBody`.

## Shape Definition

A `PhysicsShape` uses its own [PhysicsShapeDefinition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShapeDefinition.html) to specify how the shape should be created.
While a `PhysicsShapeDefinition` provides many configuration options, a few key settings are especially important to highlight, as properly configuring them during object creation can have a significant impact on performance.

- **Contact Filter**: For a `PhysicsShape` to generate contacts with other `PhysicsShape`, you must configure its [PhysicsShape.ContactFilter](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.ContactFilter.html). The contact filter lets you define which categories the `PhysicsShape` belongs to and which other categories it can interact with. These categories are specified as `PhysicsLayer` entries (see [PhysicsLayers](PhysicsLayers.md)).
- **Is Trigger**: A `PhysicsShape` can be configured to create contacts with other `PhysicsShape` objects that produce a collision response on its `PhysicsBody`. In some cases, you may not want a collision response; instead, you can allow shapes to overlap and receive notifications of the overlap only. You can achieve this by marking the `PhysicsShape` as a `trigger`.
- **Surface Material**: When a `PhysicsShape` contacts another `PhysicsShape` and neither is a `trigger`, the collision response can produce various behaviors. These are controlled by properties in the `PhysicsShape.SurfaceMaterial`, such as `bounciness`, `friction`, `rollingResistance`, and `tangentSpeed`. See [PhysicsShape.SurfaceMaterial](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.SurfaceMaterial.html) for details. You can also assign a customColor for use by the physics debug renderer.
- **Events** During a `PhysicsWorld` simulation, a `PhysicsShape` can produce various events such as if a trigger or non-trigger contact was produced. After the simulation has finished, these events can be read explicitly or can be directed as callbacks (see [PhysicsEvents](PhysicsEvents.md)).
- **Contact Filter Callbacks**: A `PhysicsShape` can override whether it produces any contact with another `PhysicsShape`. When enabled, this produces a callback where your script can decide whether to allow the contact.
- **PreSolve Callbacks**: When a `PhysicsShape` creates a contact, it can override whether that contact is passed to the physics solver—determining if it has any effect. This callback is more expensive than the `Contact Filter Callback` because the contact has already been generated, but it’s useful when decisions must be based on the contact’s details. When enabled, your script receives a callback to decide whether the contact should be processed by the solver.
- **Density**: When you add a `PhysicsShape`, it contributes mass to its `PhysicsBody` based on the product of its density and its geometric area. This also shifts the body’s center of mass, affecting how it rotates. The default density is `1`, but you can adjust it to fine-tune the `PhysicsBody`’s total mass.
- **Start Mass Update**: When you add a `PhysicsShape`, it increases its `PhysicsBody`’s mass. However, adding multiple shapes recalculates mass each time, which can hurt performance. Disable automatic updates to prevent this, then call `PhysicsBody.ApplyMassFromShapes` explicitly to update the mass once to improve performance.
- **Start Static Contacts**: By default, adding a `PhysicsShape` to a `static` `PhysicsBody` won’t produce a contact with a `dynamic` `PhysicsBody` until the dynamic body moves. This significantly improves creation-time performance. In specific cases, you can override this behavior for selected `PhysicsShape` instances.
- **Mover Data**: A `PhysicsWorld` provides a character-controller utility via `PhysicsWorld.CastMover`. It detects contacts with `PhysicsShape` objects and uses each shape’s `PhysicsShape.MoverData` to configure the appropriate response.

## Shape Geometry

A `PhysicsShape` can represent any of the available primitive types listed in [PhysicsShape.ShapeType](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.ShapeType.html).
Each shape type has a corresponding geometry type that you specify when creating a `PhysicsShape`:
- `PhysicsShape.ShapeType.Circle` uses `CircleGeometry`.
- `PhysicsShape.ShapeType.Capsule` uses `CapsuleGeometry`.
- `PhysicsShape.ShapeType.Segment` uses `SegmentGeometry`.
- `PhysicsShape.ShapeType.Polygon` uses `PolygonGeometry`.
- `PhysicsShape.ShapeType.ChainSegment` uses `ChainSegmentGeometry`.

Like definitions, each geometry type includes a default geometry available via the static `defaultGeometry` property, which provides a simple, valid shape.
In practice, you’ll typically customize the geometry yourself and reuse it when creating multiple shapes.

Example of creating some basic geometries:

```csharp
void Run()
{
    var circleGeometry = new CircleGeometry { radius = 1.5f };
    var capsuleGeometry = new CapsuleGeometry { center1 = Vector2.left, center2 = Vector2.right, radius = 0.5f };
    var segmentGeometry = new SegmentGeometry { point1 = Vector2.left, point2 = Vector2.right }
    var polygonGeometry = PolygonGeometry.CreateBox(size: Vector2.one, radius: 0.25f );
}
```




## Creation and Destruction

A `PhysicsShape` is created and destroyed on a `PhysicsBody` like this:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create a body first.
    var body = world.CreateBody();
    
    // Create a circle geometry.
    var circleGeometry = new CircleGeometry { radius = 1.5f };
    
    // Create a circle shape and attach it to the body.
    var shape = body.CreateShape(circleGeometry);
    
    // Destroy it immediately!
    shape.Destroy();
}
```

After you create a shape from a geometry, later changes to that geometry do not affect the already created shape.

In the following example, first a circle shape with radius 1.5 is created, and then a separate circle shape with radius 2 is created:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create a body first.
    var body = world.CreateBody();
    
    // Create a circle geometry.
    var circleGeometry = new CircleGeometry { radius = 1.5f };
    
    // Create a circle shape and attach it to the body.
    var shape1 = body.CreateShape(circleGeometry);
    
    // Change the geometry.
    circleGeometry.radius = 2f;
    
    // Create another circle shape and attach it to the body.
    var shape2 = body.CreateShape(circleGeometry);
}
```

You can also read the shape’s geometry by accessing the corresponding geometry property on the `PhysicsShape`, like this:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create a body first.
    var body = world.CreateBody();
    
    // Create a circle geometry.
    var circleGeometry = new CircleGeometry { radius = 1.5f };
    
    // Create a circle shape and attach it to the body.
    var shape = body.CreateShape(circleGeometry);
    
    // Show the shape type.
    Debug.Log(shape.shapeType);
    
    // We know this is a circle type so we can read the geometry back.
    Debug.Log(shape.circleGeometry);
    
    // This will produce a console warning.
    // You must ensure that you read the appropriate geometry property for the current shape type.
    Debug.Log(shape.capsuleGeometry);
}
```
In addition to reading the geometry type, you can modify the shape’s geometry as well, like this:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create a body first.
    var body = world.CreateBody();
    
    // Create a circle geometry.
    var circleGeometry = new CircleGeometry { radius = 1.5f };
    
    // Create a circle shape and attach it to the body.
    var shape = body.CreateShape(circleGeometry);
    
    // Create a segment geometry.
    var segmentGeometry = new SegmentGeometry { point1 = Vector2.left, point2 = Vector2.right };
    
    // Change the shape to use a segment geometry because we changed our minds!
    // NOTE: This example is bad practice, it simply shows the ability to change the shape geometry.
    shape.segmentGeometry = segmentGeometry;
}
```

## Batch Creation and Destruction

Just like batch creation and destruction with `PhysicsBody`, you can use the same batching features with `PhysicsShape`. For example:

```csharp
void Run()
{
// Get the main physics world.
var world = PhysicsWorld.defaultWorld;

    // Create a default body.
    var body = world.CreateBody();
    
    const int shapeCount = 10;

    // Create the shape geometries.
    var shapeGeometries = new NativeArray<CircleGeometry>(shapeCount, Allocator.Temp);
    for (var i = 0; i < shapeCount; ++i)
        shapeGeometries[i] = new CircleGeometry { center = Vector2.right * i, radius = (i + 1) * 0.1f };

    // Create a default shape definition.
    var shapeDef = PhysicsShapeDefinition.defaultDefinition;
    
    // Create the shapes.
    using var shapes = body.CreateShapeBatch(shapeGeometries, shapeDef);

    // Dispose of the shape geometries.
    shapeGeometries.Dispose();

    // Destroy them all immediately!
    // NOTE: We can also specify here if we want the body to have its mass updated.
    PhysicsShape.DestroyBatch(shapes, false);

    // Destroy the body.
    // NOTE: Destroying a body will automatically destroy any attached shapes so the batch destruction above isn't really required.
    body.Destroy();
}  
```
