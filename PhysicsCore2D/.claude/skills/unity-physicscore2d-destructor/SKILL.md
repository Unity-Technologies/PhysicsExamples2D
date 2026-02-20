# Unity PhysicsCore2D - PhysicsDestructor

Expert guidance on using PhysicsDestructor to decompose and slice geometries for destructible objects in Unity PhysicsCore2D.

## Overview

**PhysicsDestructor** is a geometry decomposition utility that breaks down shapes using two primary methods:
- **Slicing** - Cut geometry along a line/ray
- **Fragmenting** - Break geometry into pieces using fragment points

Perfect for:
- Destructible objects and environments
- Dynamic shape splitting
- Fracture effects
- Cutting mechanics

**Important:** PhysicsDestructor operates exclusively on **PolygonGeometry**. Other geometry types must be converted first.

## Core Operations

### 1. Slicing

Slicing divides geometry into two groups along a specified ray, functioning like a 2D plane intersection.

```csharp
// Slice geometry along a ray
var result = PhysicsDestructor.Slice(
    targetGeometry,          // FragmentGeometry to slice
    rayOrigin,              // Ray start point
    rayDirection,           // Ray direction (extends infinitely)
    Allocator.Temp          // Memory allocator
);

// Result contains left and right portions
var leftGeometry = result.leftGeometry;
var rightGeometry = result.rightGeometry;

// Check if slicing produced results
if (leftGeometry.IsCreated && leftGeometry.polygonCount > 0)
{
    // Create body for left portion
}

if (rightGeometry.IsCreated && rightGeometry.polygonCount > 0)
{
    // Create body for right portion
}

// Dispose when done
result.Dispose();
```

### 2. Fragmenting

Fragmenting uses fragment points to create fragment islands. Where fragment points overlap the input geometry, fragments are produced.

```csharp
// Fragment geometry using fragment points
var result = PhysicsDestructor.Fragment(
    targetGeometry,          // FragmentGeometry to fragment
    fragmentPoints,          // NativeArray<Vector2> of fragment points
    Allocator.Temp          // Memory allocator
);

// Result contains broken and unbroken portions
var brokenGeometry = result.brokenGeometry;   // Overlapping fragments
var unbrokenGeometry = result.unbrokenGeometry; // Non-overlapping areas

// Process fragments
for (int i = 0; i < brokenGeometry.polygonCount; i++)
{
    var fragment = brokenGeometry.GetPolygon(i, Allocator.Temp);
    // Create body for fragment
    fragment.Dispose();
}

// Dispose when done
result.Dispose();
```

### 3. Fragmenting with Carving

Carving removes specific areas before fragmentation:

```csharp
// Fragment with carving mask
var result = PhysicsDestructor.Fragment(
    targetGeometry,          // FragmentGeometry to fragment
    fragmentPoints,          // Fragment points
    carvingMask,            // FragmentGeometry mask to remove areas
    Allocator.Temp          // Memory allocator
);

// Any input geometry NOT overlapped by the mask becomes "unbroken"
var brokenGeometry = result.brokenGeometry;
var unbrokenGeometry = result.unbrokenGeometry;

// Dispose when done
result.Dispose();
```

## FragmentGeometry Structure

`PhysicsDestructor.FragmentGeometry` wraps PolygonGeometry with transform information:

```csharp
// Create fragment geometry from polygon
var polygonGeometry = PolygonGeometry.CreatePolygons(vertices, Allocator.Temp);
var transform = new PhysicsTransform(position, rotation);

var fragmentGeometry = new PhysicsDestructor.FragmentGeometry
{
    geometry = polygonGeometry,
    transform = transform
};
```

## Complete Slicing Example

```csharp
// Slice a box along a diagonal line
void SliceBox(PhysicsWorld world, PhysicsBody body)
{
    // Get original shape
    var originalShape = body.GetShape(0);

    // Convert to polygon geometry
    var polygonGeometry = originalShape.GetPolygonGeometry(Allocator.Temp);

    // Create fragment geometry
    var fragmentGeometry = new PhysicsDestructor.FragmentGeometry
    {
        geometry = polygonGeometry,
        transform = body.GetTransform()
    };

    // Define slice ray (diagonal cut)
    Vector2 sliceOrigin = body.position;
    Vector2 sliceDirection = new Vector2(1, 1).normalized;

    // Perform slice
    var result = PhysicsDestructor.Slice(
        fragmentGeometry,
        sliceOrigin,
        sliceDirection,
        Allocator.Temp
    );

    // Destroy original body
    world.DestroyBody(body);

    // Create left piece
    if (result.leftGeometry.IsCreated && result.leftGeometry.polygonCount > 0)
    {
        var leftBody = world.CreateBody(new PhysicsBody.Definition
        {
            type = PhysicsBodyType.Dynamic,
            position = body.position,
            rotation = body.rotation
        });

        for (int i = 0; i < result.leftGeometry.polygonCount; i++)
        {
            var polygon = result.leftGeometry.GetPolygon(i, Allocator.Temp);
            leftBody.CreateShape(polygon);
            polygon.Dispose();
        }
    }

    // Create right piece
    if (result.rightGeometry.IsCreated && result.rightGeometry.polygonCount > 0)
    {
        var rightBody = world.CreateBody(new PhysicsBody.Definition
        {
            type = PhysicsBodyType.Dynamic,
            position = body.position,
            rotation = body.rotation
        });

        for (int i = 0; i < result.rightGeometry.polygonCount; i++)
        {
            var polygon = result.rightGeometry.GetPolygon(i, Allocator.Temp);
            rightBody.CreateShape(polygon);
            polygon.Dispose();
        }
    }

    // Cleanup
    result.Dispose();
    polygonGeometry.Dispose();
}
```

## Complete Fragmenting Example

```csharp
// Fragment geometry into pieces
void FragmentObject(PhysicsWorld world, PhysicsBody body, Vector2 impactPoint)
{
    // Get original shape
    var originalShape = body.GetShape(0);

    // Convert to polygon geometry
    var polygonGeometry = originalShape.GetPolygonGeometry(Allocator.Temp);

    // Create fragment geometry
    var fragmentGeometry = new PhysicsDestructor.FragmentGeometry
    {
        geometry = polygonGeometry,
        transform = body.GetTransform()
    };

    // Create fragment points (e.g., radial pattern from impact)
    var fragmentPoints = new NativeArray<Vector2>(8, Allocator.Temp);
    for (int i = 0; i < 8; i++)
    {
        float angle = (i / 8f) * Mathf.PI * 2f;
        float radius = 0.5f;
        fragmentPoints[i] = impactPoint + new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius
        );
    }

    // Perform fragmentation
    var result = PhysicsDestructor.Fragment(
        fragmentGeometry,
        fragmentPoints,
        Allocator.Temp
    );

    // Destroy original body
    world.DestroyBody(body);

    // Create fragment bodies
    if (result.brokenGeometry.IsCreated && result.brokenGeometry.polygonCount > 0)
    {
        for (int i = 0; i < result.brokenGeometry.polygonCount; i++)
        {
            var polygon = result.brokenGeometry.GetPolygon(i, Allocator.Temp);

            // Create body for fragment
            var fragmentBody = world.CreateBody(new PhysicsBody.Definition
            {
                type = PhysicsBodyType.Dynamic,
                position = body.position,
                rotation = body.rotation
            });

            fragmentBody.CreateShape(polygon);

            // Apply explosion force
            Vector2 fragmentCenter = polygon.GetCentroid();
            Vector2 direction = (fragmentCenter - impactPoint).normalized;
            fragmentBody.linearVelocity = direction * 5f;

            polygon.Dispose();
        }
    }

    // Handle unbroken portions
    if (result.unbrokenGeometry.IsCreated && result.unbrokenGeometry.polygonCount > 0)
    {
        var unbrokenBody = world.CreateBody(new PhysicsBody.Definition
        {
            type = PhysicsBodyType.Dynamic,
            position = body.position,
            rotation = body.rotation
        });

        for (int i = 0; i < result.unbrokenGeometry.polygonCount; i++)
        {
            var polygon = result.unbrokenGeometry.GetPolygon(i, Allocator.Temp);
            unbrokenBody.CreateShape(polygon);
            polygon.Dispose();
        }
    }

    // Cleanup
    result.Dispose();
    fragmentPoints.Dispose();
    polygonGeometry.Dispose();
}
```

## Slicing with Mouse/Touch Input

```csharp
// Slice objects along a drawn line
void SliceAlongLine(PhysicsWorld world, Vector2 lineStart, Vector2 lineEnd)
{
    // Calculate ray from line
    Vector2 rayOrigin = lineStart;
    Vector2 rayDirection = (lineEnd - lineStart).normalized;

    // Query all bodies along the line
    var queryFilter = new PhysicsQuery.QueryFilter { useLayerMask = false };
    var hits = world.Raycast(lineStart, rayDirection, Vector2.Distance(lineStart, lineEnd), queryFilter);

    foreach (var hit in hits)
    {
        var body = hit.body;
        if (!body.IsValid || body.type != PhysicsBodyType.Dynamic)
            continue;

        // Get shape geometry
        var shape = body.GetShape(0);
        var polygonGeometry = shape.GetPolygonGeometry(Allocator.Temp);

        // Transform ray to local space
        var bodyTransform = body.GetTransform();
        var localRayOrigin = bodyTransform.InverseTransformPoint(rayOrigin);
        var localRayDirection = bodyTransform.InverseTransformDirection(rayDirection);

        // Create fragment geometry
        var fragmentGeometry = new PhysicsDestructor.FragmentGeometry
        {
            geometry = polygonGeometry,
            transform = PhysicsTransform.Identity // Already in local space
        };

        // Slice
        var result = PhysicsDestructor.Slice(
            fragmentGeometry,
            localRayOrigin,
            localRayDirection,
            Allocator.Temp
        );

        // Create sliced pieces (similar to previous example)
        // ... create left and right bodies ...

        // Cleanup
        result.Dispose();
        polygonGeometry.Dispose();
    }
}
```

## Carving Example (Damage Holes)

```csharp
// Carve a hole in geometry at impact point
void CarveDamageHole(PhysicsWorld world, PhysicsBody body, Vector2 impactPoint, float radius)
{
    // Get original geometry
    var originalShape = body.GetShape(0);
    var polygonGeometry = originalShape.GetPolygonGeometry(Allocator.Temp);

    var fragmentGeometry = new PhysicsDestructor.FragmentGeometry
    {
        geometry = polygonGeometry,
        transform = body.GetTransform()
    };

    // Create carving mask (circle at impact point)
    var circleGeometry = new CircleGeometry { radius = radius };
    var maskTransform = new PhysicsTransform(impactPoint, 0);

    // Convert circle to polygon for carving
    var maskPolygon = circleGeometry.ToPolygonGeometry(16, Allocator.Temp);
    var carvingMask = new PhysicsDestructor.FragmentGeometry
    {
        geometry = maskPolygon,
        transform = maskTransform
    };

    // Create single fragment point at impact
    var fragmentPoints = new NativeArray<Vector2>(1, Allocator.Temp);
    fragmentPoints[0] = impactPoint;

    // Fragment with carving
    var result = PhysicsDestructor.Fragment(
        fragmentGeometry,
        fragmentPoints,
        carvingMask,
        Allocator.Temp
    );

    // Replace body with carved geometry
    world.DestroyBody(body);

    if (result.unbrokenGeometry.IsCreated && result.unbrokenGeometry.polygonCount > 0)
    {
        var newBody = world.CreateBody(new PhysicsBody.Definition
        {
            type = body.type,
            position = body.position,
            rotation = body.rotation
        });

        for (int i = 0; i < result.unbrokenGeometry.polygonCount; i++)
        {
            var polygon = result.unbrokenGeometry.GetPolygon(i, Allocator.Temp);
            newBody.CreateShape(polygon);
            polygon.Dispose();
        }
    }

    // Cleanup
    result.Dispose();
    fragmentPoints.Dispose();
    maskPolygon.Dispose();
    polygonGeometry.Dispose();
}
```

## Converting Other Geometry Types

PhysicsDestructor requires PolygonGeometry:

```csharp
// Convert circle to polygon
var circle = new CircleGeometry { radius = 1.0f };
var polygonFromCircle = circle.ToPolygonGeometry(32, Allocator.Temp); // 32 segments

// Convert capsule to polygon
var capsule = new CapsuleGeometry
{
    vertex0 = new Vector2(-1, 0),
    vertex1 = new Vector2(1, 0),
    radius = 0.5f
};
var polygonFromCapsule = capsule.ToPolygonGeometry(16, Allocator.Temp); // 16 segments per cap

// Get polygon from existing shape
var shape = body.GetShape(0);
var polygonFromShape = shape.GetPolygonGeometry(Allocator.Temp);
```

## Memory Management

Always dispose of native allocations:

```csharp
using (var result = PhysicsDestructor.Slice(fragmentGeometry, origin, direction, Allocator.Temp))
{
    // Use result.leftGeometry and result.rightGeometry
} // Automatically disposed

// Or manual disposal
var result = PhysicsDestructor.Slice(fragmentGeometry, origin, direction, Allocator.Temp);
// ... use result ...
result.Dispose();
```

## Best Practices

- **Check IsCreated and polygonCount** - Results may be empty
- **Dispose all geometries** - Prevent memory leaks
- **Use Allocator.Temp when possible** - Best performance
- **Transform to local space** - Slice rays should be in geometry's local space
- **Limit fragment count** - Too many fragments hurts performance
- **Pool bodies** - Reuse bodies instead of creating/destroying
- **Apply forces to fragments** - Make destruction feel dynamic
- **Copy material properties** - Transfer friction, restitution, etc.
- **Consider LOD** - Use simpler geometry for distant objects
- **Clean up small fragments** - Remove tiny pieces after timeout

## Performance Considerations

- Slicing/fragmenting is CPU-intensive
- Perform operations on impact, not every frame
- Limit simultaneous destructions
- Use simpler geometries when possible
- Consider pre-computed fracture patterns
- Profile complex destructions
- Clean up debris after some time

## Common Use Cases

- **Destructible walls** - Slice along projectile path
- **Explosive fragmentation** - Radial fragment points
- **Cutting mechanics** - Mouse/touch-drawn slice lines
- **Damage holes** - Carving with explosion geometry
- **Progressive destruction** - Multiple smaller fragments
- **Ice breaking** - Realistic fracture patterns
- **Glass shattering** - Radial cracks from impact

## Thread Safety

PhysicsDestructor operations can be used in jobs:

```csharp
[BurstCompile]
struct SliceJob : IJob
{
    public PhysicsDestructor.FragmentGeometry geometry;
    public Vector2 sliceOrigin;
    public Vector2 sliceDirection;

    public void Execute()
    {
        using (var result = PhysicsDestructor.Slice(
            geometry, sliceOrigin, sliceDirection, Allocator.Temp))
        {
            // Process result
        }
    }
}
```
