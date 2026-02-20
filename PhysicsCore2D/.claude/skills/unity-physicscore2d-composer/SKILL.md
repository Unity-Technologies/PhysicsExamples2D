# Unity PhysicsCore2D - PhysicsComposer

Expert guidance on using PhysicsComposer to create complex collision shapes through boolean geometry operations in Unity PhysicsCore2D.

## Overview

**PhysicsComposer** is a geometry composition utility that combines multiple simple geometries using boolean operations to create complex collision shapes. It's ideal for:
- Creating complex collision shapes from simple primitives
- Procedural shape generation
- Level geometry composition
- Custom compound colliders

## Boolean Operations

PhysicsComposer supports four boolean operations via `PhysicsComposer.Operation`:

- **OR (Union)** - Combines shapes, creates the union of all geometries
- **AND (Intersection)** - Keeps only overlapping regions
- **NOT (Subtraction)** - Removes one shape from another
- **XOR (Exclusive Or)** - Keeps non-overlapping regions only

## Core Workflow

### 1. Create Composer Instance
```csharp
// Create composer with memory allocation
var composer = PhysicsComposer.Create(Allocator.Temp);

// Or with custom initial capacity
var composer = PhysicsComposer.Create(1024, Allocator.Temp);
```

### 2. Add Geometry Layers
```csharp
// Add layers with boolean operations
composer.AddLayer(geometry1, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);
composer.AddLayer(geometry2, PhysicsComposer.Operation.AND, transform2);
composer.AddLayer(geometry3, PhysicsComposer.Operation.NOT, transform3);
```

### 3. Generate Output Geometry
```csharp
// Generate filled polygon geometry
var polygonGeometry = composer.CreatePolygonGeometry(Allocator.Temp);

// Or generate outline/chain geometry
var chainGeometry = composer.CreateChainGeometry(Allocator.Temp);
```

### 4. Create Physics Shapes
```csharp
// Create shape from composed geometry
var shape = body.CreateShape(polygonGeometry);

// Dispose geometry when done
polygonGeometry.Dispose();
```

### 5. Cleanup
```csharp
// Dispose composer when finished
composer.Dispose();
```

## API Reference

### PhysicsComposer Methods

#### Create
```csharp
// Create new composer
static PhysicsComposer Create(Allocator allocator);
static PhysicsComposer Create(int initialCapacity, Allocator allocator);
```

#### AddLayer
```csharp
// Add geometry layer with operation and transform
void AddLayer(CircleGeometry geometry, Operation op, PhysicsTransform transform);
void AddLayer(CapsuleGeometry geometry, Operation op, PhysicsTransform transform);
void AddLayer(PolygonGeometry geometry, Operation op, PhysicsTransform transform);
void AddLayer(PhysicsShape shape, Operation op, PhysicsTransform transform);
void AddLayer(NativeArray<Vector2> points, Operation op, PhysicsTransform transform);
void AddLayer(ReadOnlySpan<Vector2> points, Operation op, PhysicsTransform transform);
```

#### RemoveLayer
```csharp
// Remove layer by index
void RemoveLayer(int index);
```

#### CreatePolygonGeometry
```csharp
// Generate filled polygon output
PolygonGeometry CreatePolygonGeometry(Allocator allocator);
```

#### CreateChainGeometry
```csharp
// Generate outline/edge output
ChainGeometry CreateChainGeometry(Allocator allocator);
```

#### Destroy
```csharp
// Release resources
void Dispose();
```

## Supported Input Geometry Types

PhysicsComposer accepts multiple geometry types:

### CircleGeometry
```csharp
var circle = new CircleGeometry { radius = 1.0f };
composer.AddLayer(circle, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);
```

### CapsuleGeometry
```csharp
var capsule = new CapsuleGeometry
{
    vertex0 = new Vector2(-1, 0),
    vertex1 = new Vector2(1, 0),
    radius = 0.5f
};
composer.AddLayer(capsule, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);
```

### PolygonGeometry
```csharp
var polygon = PolygonGeometry.CreatePolygon(vertices, Allocator.Temp);
composer.AddLayer(polygon, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);
polygon.Dispose();
```

### PhysicsShape
```csharp
var existingShape = body.GetShape(0);
composer.AddLayer(existingShape, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);
```

### Custom Point Arrays
```csharp
var points = new Vector2[]
{
    new Vector2(0, 0),
    new Vector2(1, 0),
    new Vector2(1, 1),
    new Vector2(0, 1)
};
composer.AddLayer(points, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);
```

## Practical Examples

### Creating a Capsule-Ended Rectangle
```csharp
using (var composer = PhysicsComposer.Create(Allocator.Temp))
{
    // Main rectangle body
    var rect = PolygonGeometry.CreateBox(2.0f, 1.0f, Allocator.Temp);
    composer.AddLayer(rect, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);

    // Left circle end
    var leftCircle = new CircleGeometry { radius = 0.5f };
    var leftTransform = new PhysicsTransform(new Vector2(-1.0f, 0), 0);
    composer.AddLayer(leftCircle, PhysicsComposer.Operation.OR, leftTransform);

    // Right circle end
    var rightCircle = new CircleGeometry { radius = 0.5f };
    var rightTransform = new PhysicsTransform(new Vector2(1.0f, 0), 0);
    composer.AddLayer(rightCircle, PhysicsComposer.Operation.OR, rightTransform);

    // Generate final geometry
    var finalGeometry = composer.CreatePolygonGeometry(Allocator.Temp);
    var shape = body.CreateShape(finalGeometry);

    finalGeometry.Dispose();
    rect.Dispose();
}
```

### Creating a Donut Shape
```csharp
using (var composer = PhysicsComposer.Create(Allocator.Temp))
{
    // Outer circle
    var outerCircle = new CircleGeometry { radius = 2.0f };
    composer.AddLayer(outerCircle, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);

    // Inner circle (subtract to create hole)
    var innerCircle = new CircleGeometry { radius = 1.5f };
    composer.AddLayer(innerCircle, PhysicsComposer.Operation.NOT, PhysicsTransform.Identity);

    var donutGeometry = composer.CreatePolygonGeometry(Allocator.Temp);
    var shape = body.CreateShape(donutGeometry);

    donutGeometry.Dispose();
}
```

### Creating Complex Building Shape
```csharp
using (var composer = PhysicsComposer.Create(Allocator.Temp))
{
    // Main building body
    var mainBuilding = PolygonGeometry.CreateBox(4.0f, 6.0f, Allocator.Temp);
    composer.AddLayer(mainBuilding, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);

    // Tower on top
    var tower = PolygonGeometry.CreateBox(2.0f, 3.0f, Allocator.Temp);
    var towerTransform = new PhysicsTransform(new Vector2(0, 4.5f), 0);
    composer.AddLayer(tower, PhysicsComposer.Operation.OR, towerTransform);

    // Cut out door
    var door = PolygonGeometry.CreateBox(1.0f, 2.0f, Allocator.Temp);
    var doorTransform = new PhysicsTransform(new Vector2(0, -2.0f), 0);
    composer.AddLayer(door, PhysicsComposer.Operation.NOT, doorTransform);

    // Cut out windows
    for (int i = -1; i <= 1; i++)
    {
        var window = PolygonGeometry.CreateBox(0.6f, 0.8f, Allocator.Temp);
        var windowTransform = new PhysicsTransform(new Vector2(i * 1.5f, 1.0f), 0);
        composer.AddLayer(window, PhysicsComposer.Operation.NOT, windowTransform);
        window.Dispose();
    }

    var buildingGeometry = composer.CreatePolygonGeometry(Allocator.Temp);
    var shape = body.CreateShape(buildingGeometry);

    buildingGeometry.Dispose();
    mainBuilding.Dispose();
    tower.Dispose();
    door.Dispose();
}
```

### Creating Intersected Shape
```csharp
using (var composer = PhysicsComposer.Create(Allocator.Temp))
{
    // Two overlapping circles
    var circle1 = new CircleGeometry { radius = 1.5f };
    var transform1 = new PhysicsTransform(new Vector2(-0.5f, 0), 0);
    composer.AddLayer(circle1, PhysicsComposer.Operation.OR, transform1);

    // AND operation keeps only intersection
    var circle2 = new CircleGeometry { radius = 1.5f };
    var transform2 = new PhysicsTransform(new Vector2(0.5f, 0), 0);
    composer.AddLayer(circle2, PhysicsComposer.Operation.AND, transform2);

    // Result is lens-shaped intersection
    var intersectionGeometry = composer.CreatePolygonGeometry(Allocator.Temp);
    var shape = body.CreateShape(intersectionGeometry);

    intersectionGeometry.Dispose();
}
```

### Creating Outline/Chain Geometry
```csharp
using (var composer = PhysicsComposer.Create(Allocator.Temp))
{
    // Add shapes
    var circle = new CircleGeometry { radius = 1.0f };
    composer.AddLayer(circle, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);

    // Generate outline instead of filled polygon
    var chainGeometry = composer.CreateChainGeometry(Allocator.Temp);

    // Create chain shape (edges only, no fill)
    var chain = body.CreateChain(chainGeometry);

    chainGeometry.Dispose();
}
```

## Operation Order

Operations are applied in the order layers are added:

```csharp
// Result = ((A OR B) AND C) NOT D
composer.AddLayer(geometryA, Operation.OR, transform);  // Start with A
composer.AddLayer(geometryB, Operation.OR, transform);  // A OR B
composer.AddLayer(geometryC, Operation.AND, transform); // (A OR B) AND C
composer.AddLayer(geometryD, Operation.NOT, transform); // ((A OR B) AND C) NOT D
```

## Memory Management

PhysicsComposer uses native memory allocations:

```csharp
// Temp allocator - lives for 4 frames, fastest
var composer = PhysicsComposer.Create(Allocator.Temp);

// TempJob allocator - lives until job completes
var composer = PhysicsComposer.Create(Allocator.TempJob);

// Persistent allocator - lives until manually disposed
var composer = PhysicsComposer.Create(Allocator.Persistent);

// Always dispose when done
composer.Dispose();

// Or use 'using' for automatic disposal
using (var composer = PhysicsComposer.Create(Allocator.Temp))
{
    // Use composer
} // Automatically disposed
```

## Thread Safety

PhysicsComposer is thread-safe and can be used in jobs:

```csharp
[BurstCompile]
struct ComposeGeometryJob : IJob
{
    public void Execute()
    {
        using (var composer = PhysicsComposer.Create(Allocator.Temp))
        {
            // Compose geometry in job
            var circle = new CircleGeometry { radius = 1.0f };
            composer.AddLayer(circle, PhysicsComposer.Operation.OR, PhysicsTransform.Identity);

            var geometry = composer.CreatePolygonGeometry(Allocator.Temp);
            // Store or use geometry
            geometry.Dispose();
        }
    }
}
```

## Best Practices

- **Use OR for combining shapes** - Most common operation
- **Use NOT for cutting holes** - Windows, doors, damage
- **Use AND for intersections** - Special collision zones
- **Use XOR rarely** - Specific edge cases
- **Dispose geometries after use** - Prevent memory leaks
- **Use Allocator.Temp when possible** - Best performance
- **Cache composed shapes** - Don't recompose every frame
- **Simplify input geometry** - Fewer vertices = faster composition
- **Test operation order** - Order affects final result
- **Use chain geometry for edges** - More efficient than filled polygons

## Performance Considerations

- Boolean operations are CPU-intensive
- Compose shapes during initialization, not every frame
- Use simpler geometries when possible
- Consider pre-baking complex shapes
- Use chain geometry when you don't need filled collision
- Profile composition time for complex shapes

## Common Use Cases

- **Destructible objects** - Combine with PhysicsDestructor
- **Procedural level generation** - Dynamic collision shapes
- **Character collision shapes** - Body + limbs composition
- **Custom compound colliders** - Complex vehicle shapes
- **Level geometry** - Buildings, terrain features
- **Dynamic shape modification** - Gameplay-driven shape changes
