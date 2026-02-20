# Unity PhysicsCore2D - Debug Drawing

Expert guidance on using debug drawing features for visualization and debugging in Unity PhysicsCore2D.

## Overview

Each `PhysicsWorld` includes built-in debug drawing that visualizes physics objects in the Editor's Game and Scene views. Debug drawing is essential for:
- Authoring PhysicsShape objects
- Debugging physics issues
- Visualizing collision boundaries
- Understanding physics behavior

**Platform Support:** Works in Development player builds on platforms supporting compute shaders.

## Enabling Debug Drawing

### In Editor
Debug drawing is automatically enabled in the Unity Editor for Game and Scene views.

### In Builds
Enable in built players through settings:

```csharp
// Enable drawing in development builds
PhysicsCoreSettings2D.drawInBuild = true;
```

## Appearance Control Options

Per-world configuration properties:

### Draw Options
```csharp
// Select which features to visualize
world.drawOptions = PhysicsWorld.DrawOptions.All;

// Or combine specific options
world.drawOptions =
    PhysicsWorld.DrawOptions.Shapes |
    PhysicsWorld.DrawOptions.Bodies |
    PhysicsWorld.DrawOptions.Joints;
```

### Draw Fill Options
```csharp
// Configure area fill behavior
world.drawFillOptions = PhysicsWorld.DrawFillOptions.Filled;

// Options:
// - None: No fill
// - Filled: Fill shapes with color
// - Wireframe: Outline only
```

### Colors
```csharp
// Set colors for different features
world.drawColors.dynamicBody = Color.green;
world.drawColors.staticBody = Color.gray;
world.drawColors.kinematicBody = Color.yellow;
world.drawColors.sleepingBody = Color.blue;
world.drawColors.shape = Color.white;
world.drawColors.joint = Color.cyan;
```

### Fill Alpha
```csharp
// Adjust global transparency (0-1)
world.drawFillAlpha = 0.3f; // 30% opacity
```

### Line Thickness
```csharp
// Control line and border width (in pixels)
world.drawThickness = 2.0f;
```

## Draw Functions

The PhysicsWorld provides explicit drawing methods for custom visualization:

### DrawBox
```csharp
// Draw a rectangular box
world.DrawBox(
    center: new Vector2(0, 0),
    size: new Vector2(2, 1),
    angle: 0f,
    color: Color.red
);

// With lifetime (in seconds)
world.DrawBox(center, size, angle, Color.red, lifetime: 2.0f);
```

### DrawCapsule
```csharp
// Draw a capsule shape
world.DrawCapsule(
    vertex0: new Vector2(-1, 0),
    vertex1: new Vector2(1, 0),
    radius: 0.5f,
    color: Color.blue
);

// With lifetime
world.DrawCapsule(vertex0, vertex1, radius, Color.blue, lifetime: 1.0f);
```

### DrawCircle
```csharp
// Draw a circle
world.DrawCircle(
    center: new Vector2(0, 0),
    radius: 1.0f,
    color: Color.green
);

// With lifetime
world.DrawCircle(center, radius, Color.green, lifetime: 0.5f);
```

### DrawGeometry
```csharp
// Draw any geometry type
CircleGeometry circle = new CircleGeometry { radius = 1.0f };
world.DrawGeometry(
    geometry: circle,
    transform: PhysicsTransform.Identity,
    color: Color.yellow
);

// Supports CircleGeometry, CapsuleGeometry, PolygonGeometry
CapsuleGeometry capsule = new CapsuleGeometry
{
    vertex0 = new Vector2(-1, 0),
    vertex1 = new Vector2(1, 0),
    radius = 0.5f
};
world.DrawGeometry(capsule, transform, Color.magenta, lifetime: 1.0f);

// With polygon geometry
PolygonGeometry polygon = PolygonGeometry.CreateBox(2, 1, Allocator.Temp);
world.DrawGeometry(polygon, transform, Color.cyan);
polygon.Dispose();
```

### DrawLine
```csharp
// Draw a line segment
world.DrawLine(
    start: new Vector2(0, 0),
    end: new Vector2(5, 5),
    color: Color.white
);

// With lifetime
world.DrawLine(start, end, Color.white, lifetime: 2.0f);
```

### DrawLineStrip
```csharp
// Draw connected line segments
var points = new Vector2[]
{
    new Vector2(0, 0),
    new Vector2(1, 1),
    new Vector2(2, 0),
    new Vector2(3, 1)
};

world.DrawLineStrip(
    points: points,
    color: Color.red
);

// With lifetime
world.DrawLineStrip(points, Color.red, lifetime: 1.5f);
```

### DrawPoint
```csharp
// Draw a point (pixel-sized)
world.DrawPoint(
    position: new Vector2(2, 3),
    color: Color.yellow
);

// With lifetime
world.DrawPoint(position, Color.yellow, lifetime: 1.0f);
```

### DrawTransformAxis
```csharp
// Draw 2D transform axes (X=red, Y=green)
world.DrawTransformAxis(
    transform: new PhysicsTransform(new Vector2(0, 0), Mathf.PI / 4),
    size: 1.0f,
    color: Color.white
);

// With lifetime
world.DrawTransformAxis(transform, size, Color.white, lifetime: 2.0f);
```

### ClearDraw
```csharp
// Clear all user-submitted debug primitives
world.ClearDraw();
```

## Lifetime Management

By default, all drawing functions render for a **single frame**. To persist drawings longer, provide a lifetime parameter:

```csharp
// Draws for 1 frame (default)
world.DrawCircle(center, radius, Color.red);

// Draws for 5 seconds
world.DrawCircle(center, radius, Color.red, lifetime: 5.0f);

// Draws for 0.1 seconds (useful for rapid events)
world.DrawCircle(center, radius, Color.red, lifetime: 0.1f);
```

## Practical Examples

### Visualizing Raycast Results
```csharp
void DebugRaycast(PhysicsWorld world, Vector2 origin, Vector2 direction, float distance)
{
    // Draw the ray
    world.DrawLine(origin, origin + direction * distance, Color.yellow, lifetime: 1.0f);

    // Perform raycast
    var hits = world.Raycast(origin, direction, distance);

    foreach (var hit in hits)
    {
        // Draw hit point
        world.DrawPoint(hit.position, Color.red, lifetime: 1.0f);

        // Draw hit normal
        world.DrawLine(
            hit.position,
            hit.position + hit.normal * 0.5f,
            Color.green,
            lifetime: 1.0f
        );

        // Draw circle at hit
        world.DrawCircle(hit.position, 0.1f, Color.red, lifetime: 1.0f);
    }
}
```

### Visualizing Body Velocity
```csharp
void DebugBodyVelocity(PhysicsWorld world, PhysicsBody body)
{
    if (!body.IsValid)
        return;

    // Draw linear velocity
    world.DrawLine(
        body.position,
        body.position + body.linearVelocity,
        Color.cyan,
        lifetime: 0.0f // Single frame
    );

    // Draw angular velocity as rotating axis
    float angularSpeed = Mathf.Abs(body.angularVelocity);
    Color angularColor = Color.Lerp(Color.white, Color.red, angularSpeed / 10f);
    world.DrawTransformAxis(
        body.GetTransform(),
        1.0f,
        angularColor,
        lifetime: 0.0f
    );
}
```

### Visualizing Contact Points
```csharp
void DebugContactPoints(PhysicsWorld world)
{
    // After simulation
    var contactEvents = world.contactBeginEvents;

    foreach (var contact in contactEvents)
    {
        // Get contact manifold
        var manifold = world.GetContactManifold(contact.shapeA, contact.shapeB);

        if (manifold.IsValid)
        {
            // Draw each contact point
            for (int i = 0; i < manifold.pointCount; i++)
            {
                var point = manifold.GetPoint(i);

                // Draw contact point
                world.DrawPoint(point.position, Color.red, lifetime: 0.5f);

                // Draw contact normal
                world.DrawLine(
                    point.position,
                    point.position + manifold.normal * 0.3f,
                    Color.yellow,
                    lifetime: 0.5f
                );

                // Draw penetration depth
                float depth = point.separation;
                if (depth < 0)
                {
                    world.DrawCircle(
                        point.position,
                        Mathf.Abs(depth),
                        Color.red * 0.5f,
                        lifetime: 0.5f
                    );
                }
            }
        }
    }
}
```

### Visualizing Joint Constraints
```csharp
void DebugJoint(PhysicsWorld world, PhysicsJoint joint)
{
    if (!joint.IsValid)
        return;

    var bodyA = joint.bodyA;
    var bodyB = joint.bodyB;

    // Get anchor points in world space
    var anchorA = bodyA.GetWorldPoint(joint.localAnchorA);
    var anchorB = bodyB.GetWorldPoint(joint.localAnchorB);

    // Draw anchors
    world.DrawCircle(anchorA, 0.1f, Color.green, lifetime: 0.0f);
    world.DrawCircle(anchorB, 0.1f, Color.blue, lifetime: 0.0f);

    // Draw connection
    world.DrawLine(anchorA, anchorB, Color.yellow, lifetime: 0.0f);

    // Draw bodies
    world.DrawCircle(bodyA.position, 0.05f, Color.green, lifetime: 0.0f);
    world.DrawCircle(bodyB.position, 0.05f, Color.blue, lifetime: 0.0f);
}
```

### Visualizing Shape AABB
```csharp
void DebugShapeAABB(PhysicsWorld world, PhysicsShape shape)
{
    if (!shape.IsValid)
        return;

    var aabb = shape.GetAABB();

    // Draw AABB as box
    Vector2 center = aabb.center;
    Vector2 size = aabb.extents * 2;

    world.DrawBox(
        center,
        size,
        0f, // No rotation
        Color.magenta,
        lifetime: 0.0f
    );

    // Draw corner points
    world.DrawPoint(aabb.min, Color.red, lifetime: 0.0f);
    world.DrawPoint(aabb.max, Color.red, lifetime: 0.0f);
}
```

### Visualizing Path/Trajectory
```csharp
void DebugTrajectory(PhysicsWorld world, Vector2 start, Vector2 velocity, float gravity, float time)
{
    int steps = 50;
    float dt = time / steps;
    Vector2 position = start;
    Vector2 currentVelocity = velocity;

    var points = new Vector2[steps + 1];
    points[0] = position;

    for (int i = 0; i < steps; i++)
    {
        currentVelocity.y -= gravity * dt;
        position += currentVelocity * dt;
        points[i + 1] = position;
    }

    // Draw trajectory path
    world.DrawLineStrip(points, Color.cyan, lifetime: 2.0f);

    // Draw start and end points
    world.DrawCircle(start, 0.1f, Color.green, lifetime: 2.0f);
    world.DrawCircle(position, 0.1f, Color.red, lifetime: 2.0f);
}
```

### Debug Custom Collision Area
```csharp
void DebugCollisionArea(PhysicsWorld world, PolygonGeometry geometry, PhysicsTransform transform)
{
    // Draw the geometry
    world.DrawGeometry(geometry, transform, Color.yellow, lifetime: 1.0f);

    // Draw vertices
    var vertices = geometry.GetVertices(Allocator.Temp);
    foreach (var vertex in vertices)
    {
        Vector2 worldVertex = transform.TransformPoint(vertex);
        world.DrawPoint(worldVertex, Color.red, lifetime: 1.0f);
    }
    vertices.Dispose();

    // Draw centroid
    Vector2 centroid = geometry.GetCentroid();
    Vector2 worldCentroid = transform.TransformPoint(centroid);
    world.DrawCircle(worldCentroid, 0.1f, Color.green, lifetime: 1.0f);
}
```

## DrawOptions Flags

Configure what to draw automatically:

```csharp
[Flags]
public enum DrawOptions
{
    None = 0,
    Shapes = 1 << 0,           // Draw all shapes
    Bodies = 1 << 1,           // Draw body centers
    Joints = 1 << 2,           // Draw joint constraints
    AABB = 1 << 3,             // Draw AABBs
    CenterOfMass = 1 << 4,     // Draw center of mass
    Velocity = 1 << 5,         // Draw velocities
    ContactPoints = 1 << 6,    // Draw contact points
    ContactNormals = 1 << 7,   // Draw contact normals
    All = ~0                   // Draw everything
}

// Usage
world.drawOptions = PhysicsWorld.DrawOptions.Shapes | PhysicsWorld.DrawOptions.Joints;
```

## DrawColors Structure

Customize colors for different physics objects:

```csharp
world.drawColors.dynamicBody = new Color(0, 1, 0, 1);      // Green
world.drawColors.staticBody = new Color(0.5f, 0.5f, 0.5f, 1); // Gray
world.drawColors.kinematicBody = new Color(1, 1, 0, 1);    // Yellow
world.drawColors.sleepingBody = new Color(0, 0, 1, 1);     // Blue
world.drawColors.shape = new Color(1, 1, 1, 1);            // White
world.drawColors.trigger = new Color(1, 0, 1, 1);          // Magenta
world.drawColors.joint = new Color(0, 1, 1, 1);            // Cyan
world.drawColors.contactPoint = new Color(1, 0, 0, 1);     // Red
world.drawColors.contactNormal = new Color(1, 1, 0, 1);    // Yellow
world.drawColors.aabb = new Color(1, 0, 1, 1);             // Magenta
```

## Best Practices

- **Use lifetime for persistent visualization** - Easier to see events
- **Use different colors** - Distinguish different types of information
- **Clear drawings when appropriate** - Prevent clutter
- **Draw only what's needed** - Too much debug info is hard to read
- **Use single-frame drawing for per-frame data** - Velocity, contacts
- **Use longer lifetime for events** - Collisions, triggers
- **Combine with conditional compilation** - Remove debug code in release
- **Use layers for organization** - Group related debug visualizations

## Performance Considerations

- Debug drawing has a cost, especially with many primitives
- Disable in release builds
- Use `drawOptions` to selectively enable features
- Clear old drawings with `ClearDraw()` when appropriate
- Shorter lifetimes = less memory usage
- Consider LOD for distant debug visualization

## Conditional Compilation

Remove debug drawing from release builds:

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    world.DrawCircle(position, radius, Color.red);
    DebugRaycast(world, origin, direction, distance);
#endif
```

## Common Use Cases

- **Shape authoring** - Visualize collision shapes while editing
- **Collision debugging** - See contact points and normals
- **Joint debugging** - Verify anchor points and constraints
- **Raycast visualization** - See ray paths and hit points
- **Velocity visualization** - Understand object motion
- **AABB debugging** - Check broad-phase bounds
- **Trigger debugging** - Visualize trigger zones
- **Path planning** - Show movement trajectories
