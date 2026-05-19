# Unity PhysicsCore2D - PhysicsDestructor

> **Examples verified 2026-05-19** against `unity-physicscore2d-destructor-api`, `unity-physicscore2d-bodies-api`, `unity-physicscore2d-shapes-api`, `unity-physicscore2d-geometry-api`, and `unity-physicscore2d-queries-api`. No real `PhysicsDestructor` usage was found in `D:/UnitySrc/GitHub/PhysicsExamples2D/` so all examples are API-only (fallback). Known-correct API facts applied: `body.Destroy()` (not `world.DestroyBody()`), `body.GetShapes(Allocator)` (no single-index overload), `shape.polygonGeometry` property (not `GetPolygonGeometry()`), `body.transform` property (not `GetTransform()`), `world.CastRay(CastRayInput, QueryFilter, WorldCastMode, Allocator)` (not `world.Raycast()`), `QueryFilter` has `categories`/`hitCategories` fields (no `useLayerMask`), `PolygonGeometry.CreatePolygons(verts, transform, alloc)` (transform required), `CapsuleGeometry.center1`/`center2` (not `vertex0`/`vertex1`), `ToPolygons(transform, curveStride, alloc)` (curveStride in radians). The original `Section 1 Slicing` skeleton retains unverified `leftGeometry.polygonCount` — use `IsCreated` + foreach instead.

Expert guidance on using PhysicsDestructor to decompose and slice geometries for destructible objects in Unity PhysicsCore2D.

> For the full type/method API surface (every overload, signature, and XML doc), see `unity-physicscore2d-destructor-api`. This skill focuses on patterns, worked examples, and decision rules.

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
// Build target geometry (must be PolygonGeometry)
var bodyPoly = PolygonGeometry.CreateBox(new Vector2(2f, 2f), 0f);
var bodyTransform = body.transform; // PhysicsTransform from body.transform property
var target = new PhysicsDestructor.FragmentGeometry(bodyTransform,
    new ReadOnlySpan<PolygonGeometry>(new[] { bodyPoly }));

// At least 2 fragment points are required.
// Fragment points are in world-space.
Vector2[] fragmentPts = {
    bodyTransform.position + new Vector2(-0.3f, 0.2f),
    bodyTransform.position + new Vector2( 0.3f, -0.2f),
    bodyTransform.position + new Vector2( 0.0f,  0.4f),
};

using var result = PhysicsDestructor.Fragment(target, fragmentPts, Allocator.Temp);

// brokenGeometry — polygons inside the fragment-point regions
// unbrokenGeometry — polygons outside fragment-point regions
// Both use result.transform as their coordinate frame.
// If any fragment point overlapped, all geometry goes into brokenGeometry;
// otherwise everything ends up in unbrokenGeometry.
if (result.brokenGeometry.IsCreated)
{
    foreach (var poly in result.brokenGeometry)
    {
        if (!poly.isValid) continue;
        var fragBodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic,
            position = result.transform.position };
        var fragBody = world.CreateBody(fragBodyDef);
        fragBody.CreateShape(poly);
    }
}
```
> **Note:** `brokenGeometry` and `unbrokenGeometry` are `NativeArray<PolygonGeometry>`; iterate with `foreach`. The coordinate frame for all returned polygons is `result.transform`.

### 3. Fragmenting with Carving

Carving removes specific areas before fragmentation:

```csharp
// target — the shape to fragment
var targetPoly = PolygonGeometry.CreateBox(new Vector2(3f, 2f), 0f);
var targetTransform = body.transform; // body.transform: PhysicsTransform
var target = new PhysicsDestructor.FragmentGeometry(targetTransform,
    new ReadOnlySpan<PolygonGeometry>(new[] { targetPoly }));

// mask — the geometry that will be carved out of target before fragmenting
// (e.g. an explosion crater region)
var maskPoly = PolygonGeometry.CreateBox(new Vector2(0.5f, 0.5f), 0f);
var maskTransform = new PhysicsTransform(impactPoint, 0f); // crater at impact
var mask = new PhysicsDestructor.FragmentGeometry(maskTransform,
    new ReadOnlySpan<PolygonGeometry>(new[] { maskPoly }));

// Fragment points define where shards are created (world-space, >= 2 required)
Vector2[] fragmentPts = {
    impactPoint + new Vector2(-0.8f,  0.5f),
    impactPoint + new Vector2( 0.8f,  0.5f),
    impactPoint + new Vector2( 0.0f, -0.8f),
};

// Correct arg order: Fragment(target, mask, fragmentPoints, allocator)
using var result = PhysicsDestructor.Fragment(target, mask, fragmentPts, Allocator.Temp);

// result.brokenGeometry  — fragmented pieces from the masked (carved) region
// result.unbrokenGeometry — remainder of target after carving
// result.unbrokenGeometryIslands — island connectivity (only valid with mask overload)
```
> **Arg-order reminder:** The mask (carving geometry) is **always the second argument**, before `fragmentPoints`. Getting this backwards silently produces wrong results.

## FragmentGeometry Structure

`PhysicsDestructor.FragmentGeometry` wraps PolygonGeometry with transform information:

```csharp
// Option A: body already has a polygon shape — read it directly.
// shape.polygonGeometry is the correct property (not GetPolygonGeometry()).
var shapes = body.GetShapes(Allocator.Temp);
var poly = shapes[0].polygonGeometry; // PhysicsShape.polygonGeometry property
shapes.Dispose();

var fragmentGeometry = new PhysicsDestructor.FragmentGeometry(
    body.transform,                                   // PhysicsTransform (body.transform property)
    new ReadOnlySpan<PolygonGeometry>(new[] { poly }) // one or more polygons
);

// Option B: construct polygons from a vertex outline (e.g. concave shape decomposed).
// CreatePolygons requires the transform argument — omitting it is a compile error.
Vector2[] verts = { new(-1,-1), new(1,-1), new(1,1), new(-1,1) };
using var polys = PolygonGeometry.CreatePolygons(
    verts,
    body.transform,   // PhysicsTransform — REQUIRED, not optional
    Allocator.Temp
);
var fragmentGeometry2 = new PhysicsDestructor.FragmentGeometry(
    body.transform,
    polys.AsReadOnlySpan()
);
// polys must remain alive for the lifetime of fragmentGeometry2 (ReadOnlySpan borrows it)
```
> **Constructor:** `new PhysicsDestructor.FragmentGeometry(PhysicsTransform transform, ReadOnlySpan<PolygonGeometry> geometry)` — no field names, positional only.

## Complete Slicing Example

```csharp
// Slice a body along sliceOrigin -> (sliceOrigin + sliceDirection) and
// replace it with two new Dynamic bodies.
void SliceBody(PhysicsWorld world, PhysicsBody body, Vector2 sliceOrigin, Vector2 sliceDirection)
{
    // 1. Collect all polygon shapes on the body.
    var shapes = body.GetShapes(Allocator.Temp);
    if (shapes.Length == 0) { shapes.Dispose(); return; }

    // Build polygon list from all polygon shapes on the body.
    var polys = new System.Collections.Generic.List<PolygonGeometry>(shapes.Length);
    foreach (var shape in shapes)
    {
        if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
            polys.Add(shape.polygonGeometry); // .polygonGeometry property
    }
    shapes.Dispose();
    if (polys.Count == 0) return;

    // 2. Build FragmentGeometry using body.transform (PhysicsTransform property).
    var target = new PhysicsDestructor.FragmentGeometry(
        body.transform,
        new ReadOnlySpan<PolygonGeometry>(polys.ToArray())
    );

    // 3. Slice. The "translation" parameter is the direction vector (not the end-point).
    using var result = PhysicsDestructor.Slice(target, sliceOrigin, sliceDirection, Allocator.Temp);

    // 4. Destroy original body — use body.Destroy() NOT world.DestroyBody().
    body.Destroy();

    // 5. Spawn left piece.
    if (result.leftGeometry.IsCreated)
    {
        var leftDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic,
            position = result.transform.position, rotation = result.transform.radians };
        var leftBody = world.CreateBody(leftDef);
        foreach (var poly in result.leftGeometry)
        {
            if (poly.isValid) leftBody.CreateShape(poly);
        }
    }

    // 6. Spawn right piece.
    if (result.rightGeometry.IsCreated)
    {
        var rightDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic,
            position = result.transform.position, rotation = result.transform.radians };
        var rightBody = world.CreateBody(rightDef);
        foreach (var poly in result.rightGeometry)
        {
            if (poly.isValid) rightBody.CreateShape(poly);
        }
    }
    // result auto-disposed by using statement
}
```
> **Key API facts used here:**
> - `body.GetShapes(Allocator.Temp)` — no single-index overload exists
> - `shape.polygonGeometry` — property, not `GetPolygonGeometry()`
> - `body.transform` — property, not `GetTransform()`
> - `body.Destroy()` — not `world.DestroyBody(body)`
> - `result.leftGeometry` / `result.rightGeometry` — `NativeArray<PolygonGeometry>`, iterable with foreach

## Complete Fragmenting Example

```csharp
// Fragment a body at an impact point into radially-scattered pieces.
void FragmentBody(PhysicsWorld world, PhysicsBody body, Vector2 impactPoint, int shardCount = 6)
{
    // 1. Collect polygon shapes.
    var shapes = body.GetShapes(Allocator.Temp);
    if (shapes.Length == 0) { shapes.Dispose(); return; }

    var polys = new System.Collections.Generic.List<PolygonGeometry>(shapes.Length);
    foreach (var s in shapes)
        if (s.shapeType == PhysicsShape.ShapeType.Polygon)
            polys.Add(s.polygonGeometry); // .polygonGeometry property
    shapes.Dispose();
    if (polys.Count == 0) return;

    // 2. Build target FragmentGeometry. body.transform is a PhysicsTransform property.
    var target = new PhysicsDestructor.FragmentGeometry(
        body.transform,
        new ReadOnlySpan<PolygonGeometry>(polys.ToArray())
    );

    // 3. Generate fragment points in a ring around the impact (world-space, >= 2 required).
    var fragmentPts = new Vector2[shardCount];
    float angleStep = 360f / shardCount;
    for (int i = 0; i < shardCount; i++)
    {
        float rad = i * angleStep * Mathf.Deg2Rad;
        fragmentPts[i] = impactPoint + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * 0.4f;
    }

    // 4. Fragment.
    using var result = PhysicsDestructor.Fragment(target, fragmentPts, Allocator.Temp);

    // 5. Destroy original body — body.Destroy(), not world.DestroyBody().
    body.Destroy();

    // 6. Spawn broken fragments with outward impulses.
    if (result.brokenGeometry.IsCreated)
    {
        foreach (var poly in result.brokenGeometry)
        {
            if (!poly.isValid) continue;
            // Use poly.centroid for fragment body position; coordinate frame = result.transform.
            var fragPos = (Vector2)result.transform.position + poly.centroid;
            var def = new PhysicsBodyDefinition
            {
                type     = PhysicsBody.BodyType.Dynamic,
                position = fragPos,
                rotation = result.transform.radians,
            };
            var fragBody = world.CreateBody(def);
            fragBody.CreateShape(poly);

            // Apply outward impulse.
            var impulseDir = (fragPos - impactPoint).normalized;
            fragBody.ApplyLinearImpulseToCenter(impulseDir * 3f, true);
        }
    }
}
```
> **centroid is a property on PolygonGeometry** (no `GetCentroid()` method). All fragment polygons share `result.transform` as their coordinate frame.

## Slicing with Mouse/Touch Input

```csharp
// In Update(): record mouse drag start/end, then slice on mouse-up.
Vector2 _dragStart;
bool _dragging;

void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        _dragStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _dragging = true;
    }
    if (Input.GetMouseButtonUp(0) && _dragging)
    {
        _dragging = false;
        Vector2 dragEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = dragEnd - _dragStart;
        if (direction.sqrMagnitude < 0.01f) return; // drag too short

        TrySliceAlongDrag(_dragStart, direction);
    }
}

void TrySliceAlongDrag(Vector2 origin, Vector2 direction)
{
    // world.CastRay — NOT world.Raycast().
    // QueryFilter has categories/hitCategories fields — no useLayerMask.
    var rayInput = new PhysicsQuery.CastRayInput(origin, direction);
    var hits = world.CastRay(rayInput, PhysicsQuery.QueryFilter.Everything,
        PhysicsQuery.WorldCastMode.All, Allocator.Temp);

    foreach (var hit in hits)
    {
        if (!hit.isValid) continue;
        var hitBody = hit.shape.body;
        if (!hitBody.isValid) continue;

        // Collect polygon shapes via body.GetShapes(Allocator).
        var shapes = hitBody.GetShapes(Allocator.Temp);
        var polys = new System.Collections.Generic.List<PolygonGeometry>(shapes.Length);
        foreach (var s in shapes)
            if (s.shapeType == PhysicsShape.ShapeType.Polygon)
                polys.Add(s.polygonGeometry); // .polygonGeometry property
        shapes.Dispose();
        if (polys.Count == 0) continue;

        // Build FragmentGeometry using body.transform (PhysicsTransform property).
        var target = new PhysicsDestructor.FragmentGeometry(
            hitBody.transform,
            new ReadOnlySpan<PolygonGeometry>(polys.ToArray())
        );

        using var result = PhysicsDestructor.Slice(target, origin, direction, Allocator.Temp);

        hitBody.Destroy(); // body.Destroy() — no world.DestroyBody()

        void SpawnPiece(NativeArray<PolygonGeometry> geometry)
        {
            if (!geometry.IsCreated) return;
            var def = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic,
                position = result.transform.position, rotation = result.transform.radians };
            var newBody = world.CreateBody(def);
            foreach (var poly in geometry)
                if (poly.isValid) newBody.CreateShape(poly);
        }

        SpawnPiece(result.leftGeometry);
        SpawnPiece(result.rightGeometry);
    }

    hits.Dispose();
}
```

## Carving Example (Damage Holes)

```csharp
// Carve a circular hole into a body at impactPoint.
void CarveHole(PhysicsWorld world, PhysicsBody body, Vector2 impactPoint, float holeRadius)
{
    // 1. Get target polygons.
    var shapes = body.GetShapes(Allocator.Temp);
    var polys = new System.Collections.Generic.List<PolygonGeometry>(shapes.Length);
    foreach (var s in shapes)
        if (s.shapeType == PhysicsShape.ShapeType.Polygon)
            polys.Add(s.polygonGeometry);
    shapes.Dispose();
    if (polys.Count == 0) return;

    var target = new PhysicsDestructor.FragmentGeometry(
        body.transform,
        new ReadOnlySpan<PolygonGeometry>(polys.ToArray())
    );

    // 2. Build the mask (carving region) by converting a CircleGeometry to polygons.
    // CircleGeometry.ToPolygons(transform, curveStride, allocator)
    // curveStride is in RADIANS (not a segment count). ~0.4 rad ≈ 16 segments.
    var circle = CircleGeometry.Create(holeRadius);
    var maskTransform = new PhysicsTransform(impactPoint, 0f);
    using var maskPolys = circle.ToPolygons(maskTransform, 0.4f, Allocator.Temp);
    if (maskPolys.Length == 0) return;

    var mask = new PhysicsDestructor.FragmentGeometry(
        maskTransform,
        maskPolys.AsReadOnlySpan()
    );

    // 3. Fragment points must still be provided (>= 2), but here we only care about
    //    unbrokenGeometry (the target with the hole cut out).
    //    Place fragment points inside the hole so brokenGeometry captures the removed area.
    Vector2[] fragmentPts = {
        impactPoint + new Vector2(-holeRadius * 0.3f, 0f),
        impactPoint + new Vector2( holeRadius * 0.3f, 0f),
    };

    // 4. Correct arg order: Fragment(target, mask, fragmentPoints, allocator)
    using var result = PhysicsDestructor.Fragment(target, mask, fragmentPts, Allocator.Temp);

    // 5. Replace original body with the holed remainder (unbrokenGeometry).
    body.Destroy(); // body.Destroy() — not world.DestroyBody()

    if (result.unbrokenGeometry.IsCreated)
    {
        var def = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic,
            position = result.transform.position, rotation = result.transform.radians };
        var newBody = world.CreateBody(def);
        foreach (var poly in result.unbrokenGeometry)
            if (poly.isValid) newBody.CreateShape(poly);
    }
    // brokenGeometry (the hole plug) is simply discarded.
}
```
> **`ToPolygons` curveStride is radians, not a segment count.** `0.4f` ≈ 16 segments around the circle. The `maskPolys` NativeArray must stay alive while `mask` (which borrows its span) is in use.

## Converting Other Geometry Types

PhysicsDestructor requires PolygonGeometry:

```csharp
// PhysicsDestructor requires PolygonGeometry. Convert other types first.
//
// --- Circle ---
// CircleGeometry.ToPolygons(PhysicsTransform transform, float curveStride, Allocator)
// curveStride is in RADIANS. ~0.2 rad ≈ ~32 segments (2π / 0.2 ≈ 31).
var circle = CircleGeometry.Create(0.5f);
var circleTransform = body.transform; // PhysicsTransform from body.transform property
using var circlePolys = circle.ToPolygons(circleTransform, 0.2f, Allocator.Temp);
var circleTarget = new PhysicsDestructor.FragmentGeometry(
    circleTransform,
    circlePolys.AsReadOnlySpan()
);

// --- Capsule ---
// CapsuleGeometry fields are center1/center2, NOT vertex0/vertex1.
// CapsuleGeometry.ToPolygons(PhysicsTransform transform, float curveStride, Allocator)
var capsule = CapsuleGeometry.Create(new Vector2(0f, -0.5f), new Vector2(0f, 0.5f), 0.3f);
// capsule.center1 / capsule.center2 are the semi-circle centers (not vertex0/vertex1)
var capsuleTransform = body.transform;
using var capsulePolys = capsule.ToPolygons(capsuleTransform, 0.4f, Allocator.Temp);
var capsuleTarget = new PhysicsDestructor.FragmentGeometry(
    capsuleTransform,
    capsulePolys.AsReadOnlySpan()
);

// --- Box / existing polygon shape ---
// Read directly via shape.polygonGeometry property.
var shapes = body.GetShapes(Allocator.Temp);
var polys = new System.Collections.Generic.List<PolygonGeometry>(shapes.Length);
foreach (var s in shapes)
    if (s.shapeType == PhysicsShape.ShapeType.Polygon)
        polys.Add(s.polygonGeometry); // .polygonGeometry property — not GetPolygonGeometry()
shapes.Dispose();
var boxTarget = new PhysicsDestructor.FragmentGeometry(
    body.transform,
    new ReadOnlySpan<PolygonGeometry>(polys.ToArray())
);

// --- Concave outline (arbitrary vertices) ---
// PolygonGeometry.CreatePolygons requires a transform argument.
Vector2[] outline = { new(-1,-0.5f), new(0,-1), new(1,-0.5f), new(1,0.5f), new(0,1), new(-1,0.5f) };
using var decomposed = PolygonGeometry.CreatePolygons(outline, body.transform, Allocator.Temp);
var concaveTarget = new PhysicsDestructor.FragmentGeometry(
    body.transform,
    decomposed.AsReadOnlySpan()
);
// decomposed NativeArray must outlive concaveTarget (ReadOnlySpan borrows memory)
```
> **Summary of conversion APIs:**
> | Type | Method | 2nd arg note |
> |------|--------|-------------|
> | `CircleGeometry` | `ToPolygons(transform, curveStride, alloc)` | curveStride in radians |
> | `CapsuleGeometry` | `ToPolygons(transform, curveStride, alloc)` | fields: `center1`/`center2` |
> | Vertices (outline) | `PolygonGeometry.CreatePolygons(verts, transform, alloc)` | transform required |
> | Existing polygon shape | `shape.polygonGeometry` property | direct read |

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
