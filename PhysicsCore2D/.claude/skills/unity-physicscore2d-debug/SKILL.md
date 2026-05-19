# Unity PhysicsCore2D - Debug Drawing

> **⚠ UNVERIFIED — examples below contain known API errors and must not be used verbatim.**
> Trivial casing issues (`.Identity`→`.identity`, `.isValid`→`.isValid`) have been patched, but
> deeper structural errors remain: references to methods that don't exist (e.g. `world.Raycast`,
> `body.GetTransform()`, `world.GetContactManifold(...)`, `shape.GetAABB()`), wrong field paths,
> and other API drift. Cross-check every symbol against the authoritative `unity-physicscore2d-world-api`
> / `unity-physicscore2d-shapes-api` / etc. skills or real source in `D:/UnitySrc/GitHub/PhysicsExamples2D/`
> before using any example from this file. See memory `skill-examples-must-be-verified-api`.

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
// PhysicsCoreSettings2D.renderingMode controls whether drawing/rendering is
// allowed in player builds (always available in the Editor).
// RenderingMode.EditorOnly = draw only in Editor (default)
// RenderingMode.EditorAndDevelopmentBuild = also draw in Development builds
// RenderingMode.Always = draw in all builds (requires compute shader support)

// Configure in code (must be set before the physics system initialises):
var settings = PhysicsCoreSettings2D.instance;
settings.renderingMode = PhysicsWorld.RenderingMode.EditorAndDevelopmentBuild;
```

## Appearance Control Options

Per-world configuration properties:

### Draw Options
```csharp
// world.drawOptions is a flags field — combine values with | to build a mask.
// Real enum values (PhysicsWorld.DrawOptions):
//   Off, AllShapes, AllBodies, AllJoints, AllShapeBounds,
//   AllContactPoints, AllContactNormal, AllContactImpulse,
//   AllContactForces, AllContactFriction, AllSolverIslands,
//   AllCustom, SelectedShapes, SelectedBodies, SelectedJoints,
//   SelectedShapeBounds, DefaultAll, DefaultSelected

var world = PhysicsWorld.defaultWorld;

// Draw shapes, joints, and custom drawing (the standard default):
world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

// Add contact-point visualisation on top of the default:
world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll
                  | PhysicsWorld.DrawOptions.AllContactPoints
                  | PhysicsWorld.DrawOptions.AllContactNormal;

// Turn everything off:
world.drawOptions = PhysicsWorld.DrawOptions.Off;

// Toggle a single flag without touching the rest:
var opts = world.drawOptions;
world.drawOptions = opts | PhysicsWorld.DrawOptions.AllShapeBounds;   // add
world.drawOptions = opts & ~PhysicsWorld.DrawOptions.AllShapeBounds;  // remove
```

### Draw Fill Options
```csharp
// PhysicsWorld.DrawFillOptions controls which visual aspects of a primitive are drawn.
// Values: Outline, Interior, Orientation, All (= Outline | Interior | Orientation)

var world = PhysicsWorld.defaultWorld;
var xf = PhysicsTransform.identity;

// Outline only (wireframe look):
world.DrawCircle(Vector2.zero, 1f, Color.cyan, 0f, PhysicsWorld.DrawFillOptions.Outline);

// Filled interior only (silhouette look):
world.DrawCircle(Vector2.zero, 1f, Color.cyan, 0f, PhysicsWorld.DrawFillOptions.Interior);

// Outline + interior fill + orientation marker:
world.DrawBox(xf, Vector2.one, 0f, Color.yellow, 0f, PhysicsWorld.DrawFillOptions.All);

// Combine flags manually — equivalent to All:
var opts = PhysicsWorld.DrawFillOptions.Outline
         | PhysicsWorld.DrawFillOptions.Interior
         | PhysicsWorld.DrawFillOptions.Orientation;
world.DrawBox(xf, Vector2.one, 0f, Color.yellow, 0f, opts);

// world.drawFillOptions sets the automatic drawing fill globally:
world.drawFillOptions = PhysicsWorld.DrawFillOptions.Outline;
```

### Colors
```csharp
// world.drawColors returns a ref-struct — mutate and assign back.
// Real fields (PhysicsWorld.DrawColors):
//   bodyAwake, bodyStatic, bodyKinematic, bodySpeedCapped, bodyMovingFast,
//   bodyBad, bodyDisabled, bodyFastCollisions, bodyTimeOfImpactEvent,
//   shapeOther, shapeBounds, shapeTrigger,
//   contactNormal, contactPersisted, contactAdded,
//   contactImpulse, contactFriction, contactSpeculative,
//   solverIsland, transformAxisX, transformAxisY

var world = PhysicsWorld.defaultWorld;

var colors = world.drawColors;
colors.bodyAwake    = Color.limeGreen;   // awake dynamic bodies
colors.bodyStatic   = Color.gray;        // static bodies
colors.bodyKinematic = Color.cyan;       // kinematic bodies
colors.shapeTrigger  = Color.yellow;     // trigger shapes
colors.contactNormal = Color.red;        // contact normal arrows
world.drawColors = colors;
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
// DrawBox(PhysicsTransform transform, Vector2 size, float radius,
//         Color color, float lifetime, PhysicsWorld.DrawFillOptions drawFillOptions)
// The first arg is a PhysicsTransform (position + rotation), not a plain Vector2.

var world = PhysicsWorld.defaultWorld;

// Axis-aligned unit box at the origin:
var xf = PhysicsTransform.identity;
world.DrawBox(xf, Vector2.one, 0f, Color.cyan, 0f, PhysicsWorld.DrawFillOptions.Outline);

// Rotated, filled box at a custom position (adapted from Drawing.cs):
var physicsTransform = new PhysicsTransform
{
    position = new Vector2(2f, 1f),
    rotation = PhysicsRotate.FromRadians(0.5f)
};
var size   = new Vector2(1.5f, 0.75f);
var radius = 0.1f;  // rounded corners; use 0f for sharp corners
world.DrawBox(physicsTransform, size, radius, Color.yellow, 2f,
              PhysicsWorld.DrawFillOptions.All);
```

### DrawCapsule
```csharp
// DrawCapsule(PhysicsTransform transform,
//             Vector2 center1, Vector2 center2, float radius,
//             Color color, float lifetime, PhysicsWorld.DrawFillOptions drawFillOptions)
// center1/center2 are in LOCAL space; transform positions them in the world.

var world = PhysicsWorld.defaultWorld;

// Horizontal capsule centred at (3, 0) (adapted from Drawing.cs):
var physicsTransform = new PhysicsTransform
{
    position = new Vector2(3f, 0f),
    rotation = PhysicsRotate.identity
};
var center1 = new Vector2(-0.5f, 0f);
var center2 = new Vector2( 0.5f, 0f);
world.DrawCapsule(physicsTransform, center1, center2, 0.25f,
                  Color.magenta, 0f, PhysicsWorld.DrawFillOptions.All);
```

### DrawCircle
```csharp
// DrawCircle(Vector2 center, float radius, Color color,
//            float lifetime, PhysicsWorld.DrawFillOptions drawFillOptions)

var world = PhysicsWorld.defaultWorld;

// Outline circle at the origin, single frame:
world.DrawCircle(Vector2.zero, 1f, Color.cyan, 0f, PhysicsWorld.DrawFillOptions.Outline);

// Filled circle at a position, persisted for 2 seconds (adapted from Drawing.cs):
var center = new Vector2(2f, 3f);
world.DrawCircle(center, 0.5f, Color.orange, 2f, PhysicsWorld.DrawFillOptions.Interior);

// Outline + interior + orientation (full):
world.DrawCircle(new Vector2(-1f, 0f), 0.75f, Color.yellow, 1f,
                 PhysicsWorld.DrawFillOptions.All);
```

### DrawGeometry
```csharp
// DrawGeometry overloads (all share the same parameter shape):
//   DrawGeometry(CircleGeometry,  PhysicsTransform, Color, float, DrawFillOptions)
//   DrawGeometry(CapsuleGeometry, PhysicsTransform, Color, float, DrawFillOptions)
//   DrawGeometry(PolygonGeometry, PhysicsTransform, Color, float, DrawFillOptions)
//   DrawGeometry(SegmentGeometry, PhysicsTransform, Color, float)   // no fill options
// The last two params have defaults so they may be omitted (as seen in real examples).

var world = PhysicsWorld.defaultWorld;

// Circle geometry (adapted from ContactManifold.cs):
var circle = new CircleGeometry { center = Vector2.zero, radius = 0.5f };
var xf1 = new PhysicsTransform { position = new Vector2(-1f, 0f), rotation = PhysicsRotate.identity };
world.DrawGeometry(circle, xf1, Color.cornflowerBlue);       // defaults: lifetime=0, fill=All

// Polygon (box) geometry  — PolygonGeometry.CreateBox takes Vector2 size (adapted from ContactManifold.cs):
var box = PolygonGeometry.CreateBox(new Vector2(2f, 1f));
var xf2 = new PhysicsTransform { position = new Vector2(2f, 0f), rotation = PhysicsRotate.identity };
world.DrawGeometry(box, xf2, Color.yellow, 0f, PhysicsWorld.DrawFillOptions.Outline);

// Wedge with rounded corners shown as both fill and wireframe (adapted from ContactManifold.cs):
var wedge = PolygonGeometry.Create(new Vector2[] { new(-0.1f, -0.5f), new(0.1f, -0.5f), new(0f, 0.5f) }, radius: 0.05f);
var xf3 = PhysicsTransform.identity;
world.DrawGeometry(wedge, xf3, Color.limeGreen, 0f, PhysicsWorld.DrawFillOptions.All);
wedge.radius = 0f;
world.DrawGeometry(wedge, xf3, Color.limeGreen, 0f, PhysicsWorld.DrawFillOptions.Outline);
```

### DrawLine
```csharp
// DrawLine(Vector2 point0, Vector2 point1, Color color, float lifetime)
// Named params are point0/point1 — NOT start/end.
// lifetime defaults to 0 (single frame) and may be omitted.

var world = PhysicsWorld.defaultWorld;

// Single-frame line from origin to hit point (adapted from CastRayQuery.cs):
var hitPoint = new Vector2(0f, 5f);
world.DrawLine(Vector2.zero, hitPoint, Color.cornflowerBlue);

// Persistent line (adapted from ContactManifold.cs — chain segment visualization):
var p1 = new Vector2(-1f, 0f);
var p2 = new Vector2( 1f, 0f);
world.DrawLine(p1, p2, Color.white, 2f);

// Draw a contact normal arrow (point + direction):
var contactPoint = new Vector2(0.5f, 0f);
var normal       = Vector2.up;
world.DrawLine(contactPoint, contactPoint + normal * 0.5f, Color.white);
```

### DrawLineStrip
```csharp
// DrawLineStrip(PhysicsTransform transform, ReadOnlySpan<Vector2> vertices,
//               bool loop, Color color, float lifetime)
// Minimum 2 vertices required. loop=true joins the last vertex back to the first.

using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;

var world = PhysicsWorld.defaultWorld;

// Open polyline — e.g. a trajectory path (adapted from Drawing.cs):
var waypoints = new NativeArray<Vector2>(4, Allocator.Temp);
waypoints[0] = new Vector2(-3f, 0f);
waypoints[1] = new Vector2(-1f, 2f);
waypoints[2] = new Vector2( 1f, 1f);
waypoints[3] = new Vector2( 3f, 3f);

world.DrawLineStrip(PhysicsTransform.identity, waypoints, loop: false, Color.cyan, 5f);
waypoints.Dispose();

// Closed loop (polygon outline) using identity transform:
var square = new Vector2[] { new(-1f,-1f), new(1f,-1f), new(1f,1f), new(-1f,1f) };
world.DrawLineStrip(PhysicsTransform.identity, square, loop: true, Color.yellow, 0f);
```

### DrawPoint
```csharp
// DrawPoint(Vector2 position, float radius, Color color, float lifetime)
// radius is in PIXELS (screen-space), not world units.
// Typical values: 5–25 pixels for easily-visible dots.

var world = PhysicsWorld.defaultWorld;

// Ray origin and hit point (adapted from CastRayQuery.cs):
world.DrawPoint(Vector2.zero,          10f, Color.lawnGreen, 0f);  // origin
world.DrawPoint(new Vector2(0f, 5f),   10f, Color.orange,    0f);  // hit

// Contact point with a longer lifetime (adapted from PhysicsShapeContactCallback.cs):
var contactPos = new Vector2(1f, 2f);
world.DrawPoint(position: contactPos, radius: 25f, color: Color.softYellow, lifetime: 10f);

// Chain-segment vertex marker (adapted from ContactManifold.cs, PointScale = 0.01f world units → pixel-equivalent):
const float PointScale = 0.01f;
world.DrawPoint(new Vector2(-1f, 0f), 4f * PointScale, Color.cornflowerBlue);
```

### DrawTransformAxis
```csharp
// DrawTransformAxis(PhysicsTransform transform, float scale, float lifetime)
// Draws X/Y axis arrows using drawColors.transformAxisX/Y — NO color parameter.

var world = PhysicsWorld.defaultWorld;

// Draw a transform axis at the world mouse position, scale=1, single frame
// (adapted from Queries.cs line 157):
var worldPosition = new Vector2(2f, 3f);
world.DrawTransformAxis(new PhysicsTransform(worldPosition), 1f);

// Draw a body's transform axes with a 0.5 second lifetime:
PhysicsBody body = default; // assume a valid body handle
var bodyXf = body.transform;  // body.transform is a PROPERTY, not a method
world.DrawTransformAxis(bodyXf, 0.5f, 0.5f);
```

### ClearDraw
```csharp
// Clear all user-submitted debug primitives
world.ClearDraw();
```

## Lifetime Management

By default, all drawing functions render for a **single frame**. To persist drawings longer, provide a lifetime parameter:

```csharp
// Pass a non-zero lifetime (seconds) to keep a primitive visible across frames.
// lifetime=0 means draw for one frame only (default).

var world = PhysicsWorld.defaultWorld;

// Persist a circle for 2 seconds then auto-remove:
world.DrawCircle(new Vector2(0f, 2f), 0.5f, Color.orange, 2f, PhysicsWorld.DrawFillOptions.Outline);

// Persist a line for 5 seconds (useful for logging collision events):
world.DrawLine(Vector2.zero, new Vector2(3f, 3f), Color.red, 5f);

// Persist a point for 10 seconds (e.g. contact callback, adapted from PhysicsShapeContactCallback.cs):
world.DrawPoint(new Vector2(1f, 0f), 25f, Color.softYellow, 10f);

// Single-frame drawing (lifetime=0 or omitted) — re-draw every Update/FixedUpdate:
void Update()
{
    // Redrawn every frame; no need to clear.
    world.DrawCircle(Vector2.up, 1f, Color.cyan, 0f, PhysicsWorld.DrawFillOptions.Outline);
}
```

## Practical Examples

### Visualizing Raycast Results
```csharp
// PhysicsWorld has NO world.Raycast() — use world.CastRay() or world.DrawQueryCastRay().
// world.CastRay(CastRayInput, QueryFilter) returns results; DrawQueryCastRay draws the input.

using Unity.U2D.Physics;
using UnityEngine;

// Adapted from CastRayQuery.cs (Primer):
private void Update()
{
    var world = m_PhysicsWorld;  // assume a valid PhysicsWorld

    var input = new PhysicsQuery.CastRayInput
    {
        origin      = Vector2.zero,
        translation = Vector2.up * 10f
    };

    // Visualise the ray input (arrow from origin along translation):
    world.DrawQueryCastRay(input, Color.yellow, 0f, drawEnd: true);

    // Perform the actual cast:
    using var results = world.CastRay(input, PhysicsQuery.QueryFilter.Everything);
    if (results.Length == 0)
        return;

    var hit = results[0];
    world.DrawLine(Vector2.zero, hit.point, Color.cornflowerBlue);
    world.DrawPoint(Vector2.zero,  10f, Color.lawnGreen);   // origin
    world.DrawPoint(hit.point,     10f, Color.orange);       // impact

    // Or draw the result struct directly (point + normal arrow):
    world.DrawQueryResult(hit, Color.red, 0f, drawPoint: true, drawNormal: true);
}
```

### Visualizing Body Velocity
```csharp
// body.transform is a PROPERTY (PhysicsTransform) — NOT a method.
// DrawTransformAxis takes (PhysicsTransform, float scale, float lifetime) — no color param.

using Unity.U2D.Physics;
using UnityEngine;

// Adapted from Determinism.cs (body.transform) + Boids.cs (DrawLine velocity trail):
private void Update()
{
    var world = PhysicsWorld.defaultWorld;

    using var bodies = world.GetBodies(Unity.Collections.Allocator.Temp);
    foreach (var body in bodies)
    {
        // body.transform is a PROPERTY — access directly:
        var bodyXf = body.transform;

        // Draw the body's local axes (X = red, Y = green per drawColors defaults):
        world.DrawTransformAxis(bodyXf, 0.5f, 0f);

        // Draw the velocity vector as a line from the body position:
        var vel = body.linearVelocity;
        if (vel.sqrMagnitude > 0.01f)
        {
            world.DrawLine(bodyXf.position, bodyXf.position + vel * Time.fixedDeltaTime,
                           Color.cyan, 0f);
        }
    }
}
```

### Visualizing Contact Points
```csharp
// PhysicsWorld has NO GetContactManifold(shapeA, shapeB).
// Access contacts via a contact callback (PhysicsCallbacks.IContactCallback) or
// via PhysicsQuery static manifold methods (CircleAndCircle, etc.).
// ContactManifold fields: pointCount, normal, indexer [i].
// ContactManifold.ManifoldPoint fields: point (NOT .position), anchorA, anchorB.

// Pattern 1 — callback-based (adapted from PhysicsShapeContactCallback.cs):
public class MyScript : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    private PhysicsWorld m_World;

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        var contact = beginEvent.contactId.contact;

        // Draw each manifold point for 10 seconds:
        foreach (var pt in contact.manifold)
            m_World.DrawPoint(pt.point, 25f, Color.softYellow, 10f);
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent _) { }
}

// Pattern 2 — query-based manifold (adapted from ContactManifold.cs DrawManifold):
static void DrawManifold(PhysicsWorld world, ref PhysicsShape.ContactManifold manifold)
{
    for (var i = 0; i < manifold.pointCount; ++i)
    {
        var contact = manifold[i];            // indexer returns ManifoldPoint
        var p1 = contact.point;               // field is .point, NOT .position
        var p2 = p1 + manifold.normal * 0.5f;
        world.DrawLine(p1, p2, Color.white);
        world.DrawPoint(p1, 5f, Color.blue);
    }
}
```

### Visualizing Joint Constraints
```csharp
// Enable automatic joint drawing via drawOptions:
var world = PhysicsWorld.defaultWorld;
world.drawOptions |= PhysicsWorld.DrawOptions.AllJoints;

// Or draw joint anchors manually for a hinge joint:
// (joint.bodyA, joint.bodyB, joint.localAnchorA, joint.localAnchorB are valid)
PhysicsHingeJoint joint = default; // assume a valid joint

var bodyAXf  = joint.bodyA.transform;  // body.transform is a PROPERTY
var bodyBXf  = joint.bodyB.transform;

// Convert local anchors to world space using PhysicsTransform.TransformPoint:
var worldAnchorA = bodyAXf.TransformPoint(joint.localAnchorA);
var worldAnchorB = bodyBXf.TransformPoint(joint.localAnchorB);

// Draw the anchor points and the line between them:
world.DrawCircle(worldAnchorA, 0.08f, Color.red,   0f, PhysicsWorld.DrawFillOptions.Interior);
world.DrawCircle(worldAnchorB, 0.08f, Color.green,  0f, PhysicsWorld.DrawFillOptions.Interior);
world.DrawLine(worldAnchorA, worldAnchorB, Color.yellow, 0f);
```

### Visualizing Shape AABB
```csharp
// shape.aabb is a PROPERTY (PhysicsAABB) — NOT a method.
// DrawAABB(PhysicsAABB aabb, Color color, float lifetime, DrawFillOptions drawFillOptions)

using Unity.U2D.Physics;
using UnityEngine;

private void Update()
{
    var world = PhysicsWorld.defaultWorld;

    using var bodies = world.GetBodies(Unity.Collections.Allocator.Temp);
    foreach (var body in bodies)
    {
        using var shapes = body.GetShapes(Unity.Collections.Allocator.Temp);
        foreach (var shape in shapes)
        {
            // shape.aabb is a property — access directly:
            var aabb = shape.aabb;
            world.DrawAABB(aabb, Color.cyan, 0f, PhysicsWorld.DrawFillOptions.Outline);

            // Also mark the AABB centre:
            var centre = (aabb.lowerBound + aabb.upperBound) * 0.5f;
            world.DrawPoint(centre, 4f, Color.white, 0f);
        }
    }
}
```

### Visualizing Path/Trajectory
```csharp
// DrawLineStrip(PhysicsTransform transform, ReadOnlySpan<Vector2> vertices,
//               bool loop, Color color, float lifetime)
// DrawCircle(Vector2 center, float radius, Color color, float lifetime, DrawFillOptions)

using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;

// Simulate and draw a ballistic trajectory (projectile path):
private void DrawTrajectory(PhysicsWorld world, Vector2 origin, Vector2 velocity, float gravity, int steps, float dt)
{
    var points = new NativeArray<Vector2>(steps, Allocator.Temp);

    var pos = origin;
    var vel = velocity;
    for (var i = 0; i < steps; ++i)
    {
        points[i] = pos;
        vel += new Vector2(0f, gravity) * dt;
        pos += vel * dt;
    }

    // Open path (no loop), persisted for 1 second:
    world.DrawLineStrip(PhysicsTransform.identity, points, loop: false, Color.cyan, 1f);

    // Mark start and predicted landing:
    world.DrawCircle(points[0],          0.1f, Color.green,  1f, PhysicsWorld.DrawFillOptions.Interior);
    world.DrawCircle(points[steps - 1],  0.1f, Color.orange, 1f, PhysicsWorld.DrawFillOptions.Interior);

    points.Dispose();
}
```

### Debug Custom Collision Area
```csharp
// PolygonGeometry.CreateBox takes Vector2 size (NOT separate floats, NO Allocator arg).
// geometry.centroid is a PROPERTY.
// PhysicsTransform.TransformPoint() is a valid METHOD on the struct.
// DrawGeometry(PolygonGeometry, PhysicsTransform, Color, float, DrawFillOptions)

using Unity.U2D.Physics;
using UnityEngine;

private void Update()
{
    var world = PhysicsWorld.defaultWorld;

    // Build a custom collision zone as a rotated box:
    var box = PolygonGeometry.CreateBox(new Vector2(4f, 2f));   // Vector2 size, no Allocator
    var zoneXf = new PhysicsTransform
    {
        position = new Vector2(0f, 1f),
        rotation = PhysicsRotate.FromRadians(0.3f)
    };

    // Draw the filled zone with an outline (adapted from ContactManifold.cs pattern):
    world.DrawGeometry(box, zoneXf, new Color(1f, 0.5f, 0f, 0.4f), 0f,
                       PhysicsWorld.DrawFillOptions.All);

    // Mark the centroid in world space:
    // geometry.centroid is a PROPERTY (Vector2):
    var worldCentroid = zoneXf.TransformPoint(box.centroid);
    world.DrawPoint(worldCentroid, 8f, Color.red, 0f);
}
```

## DrawOptions Flags

Configure what to draw automatically:

```csharp
// PhysicsWorld.DrawOptions is a flags enum — values can be OR'd together.
// Set world.drawOptions to control what is drawn automatically each frame.
// Adapted from SandboxManager.cs (ConfigureDrawFlag pattern).

var world = PhysicsWorld.defaultWorld;

// Common flags:
//   DefaultAll = AllShapes | AllJoints | AllCustom
//   AllShapes         — all shape outlines/fills
//   AllBodies         — body state colouring
//   AllJoints         — joint constraint visuals
//   AllShapeBounds    — AABB for every shape
//   AllContactPoints  — contact point dots
//   AllContactNormal  — contact normal arrows
//   AllContactImpulse — impulse magnitude arrows
//   AllContactFriction / AllContactForces / AllSolverIslands
//   SelectedShapes / SelectedBodies / SelectedJoints / SelectedShapeBounds
//   Off               — disable all automatic drawing

// Start from the sensible default, then layer on extras:
world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll
                  | PhysicsWorld.DrawOptions.AllContactPoints
                  | PhysicsWorld.DrawOptions.AllContactNormal
                  | PhysicsWorld.DrawOptions.AllShapeBounds;

// Toggle a flag (add / remove without touching others):
bool showBodies = true;
var current = world.drawOptions;
world.drawOptions = showBodies
    ? current |  PhysicsWorld.DrawOptions.AllBodies
    : current & ~PhysicsWorld.DrawOptions.AllBodies;

// Check if a flag is active:
bool contactsOn = world.drawOptions.HasFlag(PhysicsWorld.DrawOptions.AllContactPoints);
```

## DrawColors Structure

Customize colors for different physics objects:

```csharp
// world.drawColors returns the current DrawColors struct — mutate and assign back.
// ALL valid field names (PhysicsWorld.DrawColors):
//   bodyAwake, bodyStatic, bodyKinematic
//   bodySpeedCapped, bodyMovingFast, bodyBad, bodyDisabled
//   bodyFastCollisions, bodyTimeOfImpactEvent
//   shapeOther, shapeBounds, shapeTrigger
//   contactNormal, contactPersisted, contactAdded
//   contactImpulse, contactFriction, contactSpeculative
//   solverIsland, transformAxisX, transformAxisY

var world = PhysicsWorld.defaultWorld;

// Read-modify-write (the struct is returned by value):
var colors = world.drawColors;

// Body colours:
colors.bodyAwake     = Color.limeGreen;   // awake dynamic bodies
colors.bodyStatic    = Color.gray;        // static / sleeping bodies
colors.bodyKinematic = Color.cyan;        // kinematic bodies
colors.bodyBad       = Color.red;         // zero-mass dynamic bodies (error state)

// Shape colours:
colors.shapeOther    = new Color(0.2f, 0.6f, 1f);   // default shape tint
colors.shapeTrigger  = new Color(1f, 1f, 0f, 0.5f); // trigger shapes

// Contact colours:
colors.contactNormal    = Color.white;
colors.contactAdded     = Color.green;    // new contacts this step
colors.contactPersisted = Color.blue;     // ongoing contacts

// Transform-axis colours:
colors.transformAxisX = Color.red;
colors.transformAxisY = Color.green;

// Write back:
world.drawColors = colors;
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
// Use UNITY_EDITOR or DEVELOPMENT_BUILD preprocessor symbols to strip debug
// drawing from release player builds.  DrawCircle requires all 5 arguments.

using Unity.U2D.Physics;
using UnityEngine;

public class MyPhysicsDebugger : MonoBehaviour
{
    private PhysicsWorld m_World;

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DrawDebugInfo();
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void DrawDebugInfo()
    {
        // Draw a watched body's position:
        PhysicsBody watchedBody = default; // assume a valid body
        var xf = watchedBody.transform;    // PROPERTY — not a method
        m_World.DrawTransformAxis(xf, 0.5f, 0f);

        // Draw detection radius:
        m_World.DrawCircle(xf.position, 2f, Color.yellow, 0f,
                           PhysicsWorld.DrawFillOptions.Outline);

        // Draw a query ray using the correct API (world.CastRay, NOT world.Raycast):
        var input = new PhysicsQuery.CastRayInput
        {
            origin      = xf.position,
            translation = Vector2.down * 5f
        };
        m_World.DrawQueryCastRay(input, Color.cyan, 0f, drawEnd: true);

        using var hits = m_World.CastRay(input, PhysicsQuery.QueryFilter.Everything);
        if (hits.Length > 0)
            m_World.DrawPoint(hits[0].point, 10f, Color.red, 0f);
    }
#endif
}
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
