# Debug Drawing

Each `PhysicsWorld` includes built-in debug drawing that visualizes its contents in the Editor’s Game and Scene views. This is essential for authoring `PhysicsShape` objects and for debugging.
Debug drawing is also available in `Development` player builds when the platform supports compute shaders.
To enable debug drawing in builds, set `PhysicsLowLevelSettings2D.drawInBuild` (see [PhysicsLowLevelSettings2D](PhysicsLowLevelSettings2D.md)).

Appearance controls (per world):
- `PhysicsWorld.drawOptions`: Choose which features to draw.
- `PhysicsWorld.drawFillOptions`: Configure area filling.
- `PhysicsWorld.drawColors`:  Set feature colors.
- `PhysicsWorld.drawFillAlpha`: Set global transparency.
- `PhysicsWorld.drawThickness`: Set line and border thickness.

Explicit draw calls (with custom colors, fill, and durations):
- `PhysicsWorld.DrawBox`: Draw a box.
- `PhysicsWorld.DrawCapsule`: Draw a capsule.
- `PhysicsWorld.DrawCircle`: Draw a circle.
- `PhysicsWorld.DrawGeometry`: Draw any provided geometry.
- `PhysicsWorld.DrawLine`: Draw a line segment.
- `PhysicsWorld.DrawLineStrip`: Draw a connected strip of line segments.
- `PhysicsWorld.DrawPoint`: Draw a point (in pixels).
- `PhysicsWorld.DrawTransformAxis`: Draw a 2D transform axis.
- `PhysicsWorld.ClearDraw`: Clear user-submitted draw primitives (useful when durations are used).

## Examples

Basic primitive drawing requires specifying the primitive details and a color.
The draw occurs for a single frame and does not persist (see “Lifetime” below).

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Draw a line from the origin to (30, 40) in "cornflower blue".
    world.DrawLine(Vector2.zero, new Vector2(30f, 40f), Color.cornflowerBlue);
    
    // Draw a circle from the origin to (30, 40) in "forest green".
    world.DrawCircle(new Vector2(30f, 40f), 2f, Color.forestGreen);

    // Draw a point with a 3 pixel radius at (-5, 10) in "gold".
    world.DrawPoint(new Vector2(-5f, 10f), 3f, Color.gold);
}
```

While primitive drawing is useful, you’ll often work with geometry types when creating `PhysicsShape` objects or performing intersection tests.
The drawing system supports rendering any geometry type, for example:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create various geometry.
    var circleGeometry = new CircleGeometry { radius = 3f };
    var capsuleGeometry = new CapsuleGeometry { center1 = Vector2.down, center2 = Vector2.up, radius = 2f };
    var polygonGeometry = PolygonGeometry.CreateBox(size: new Vector2(3f, 4f), radius: 0.5f);

    // Create a transform.
    var physicsTransform = new PhysicsTransform { position = new Vector2(5f, 6f) };
    
    // Draw the geometry.
    world.DrawGeometry(circleGeometry, physicsTransform, Color.cornflowerBlue);
    world.DrawGeometry(capsuleGeometry, physicsTransform, Color.forestGreen);
    world.DrawGeometry(polygonGeometry, physicsTransform, Color.orchid);

}
```

## Lifetime

By default, all drawing functions render for a single frame.
To make a draw element persist, provide a lifetime (in seconds) via the function’s lifetime parameter.

For example:

```csharp
void Run()
{
    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;

    // Create various geometry.
    var circleGeometry = new CircleGeometry { radius = 3f };
    var capsuleGeometry = new CapsuleGeometry { center1 = Vector2.down, center2 = Vector2.up, radius = 2f };
    var polygonGeometry = PolygonGeometry.CreateBox(size: new Vector2(3f, 4f), radius: 0.5f);

    // Create a transform.
    var physicsTransform = new PhysicsTransform { position = new Vector2(5f, 6f) };

    // Use a lifetime of 5 seconds.
    const float lifeTime = 5f;
    
    // Draw the geometry.
    world.DrawGeometry(circleGeometry, physicsTransform, Color.cornflowerBlue, lifeTime);
    world.DrawGeometry(capsuleGeometry, physicsTransform, Color.forestGreen, lifeTime);
    world.DrawGeometry(polygonGeometry, physicsTransform, Color.orchid, lifeTime);
    
    // Draw a line from the origin to (30, 40) in "cornflower blue".
    world.DrawLine(Vector2.zero, new Vector2(30f, 40f), Color.cornflowerBlue, lifeTime);
    
    // Draw a circle from the origin to (30, 40) in "forest green".
    world.DrawCircle(new Vector2(30f, 40f), 2f, Color.forestGreen, lifeTime);

    // Draw a point with a 3 pixel radius at (-5, 10) in "gold".
    world.DrawPoint(new Vector2(-5f, 10f), 3f, Color.gold, lifeTime);
}
```
