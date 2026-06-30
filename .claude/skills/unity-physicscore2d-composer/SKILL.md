# Unity PhysicsCore2D - PhysicsComposer

> **Examples verified against `unity-physicscore2d-composer-api` and real code in `D:/UnitySrc/GitHub/PhysicsExamples2D/Primer/Assets/Scripts/PhysicsComposerGeometry.cs` (2026-05-19).** `AddLayer` argument order corrected to `(geometry, transform, operation, order, curveStride, reverseWinding)`. `CreatePolygonGeometry` correctly used as returning `NativeArray<PolygonGeometry>`. `CreateChain` correctly called with two arguments. For the full type/member API surface see `unity-physicscore2d-composer-api`.

Expert guidance on using PhysicsComposer to create complex collision shapes through boolean geometry operations in Unity PhysicsCore2D.

> For the full type/method API surface (every overload, signature, and XML doc), see `unity-physicscore2d-composer-api`. This skill focuses on patterns, worked examples, and decision rules.

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
// PhysicsComposer.Create() allocates the composer itself (no Allocator arg needed).
// The optional Allocator overload controls which allocator is used for layer data.
var composer = PhysicsComposer.Create();

// Optional: tune tessellation quality before adding layers.
composer.useDelaunay = true;           // Delaunay on by default; produces better results
composer.maxPolygonVertices = 8;       // max verts per output polygon (default is API max)

// Always Destroy (or Dispose) when finished — never leak a composer.
composer.Destroy();

// Alternatively, use a 'using' scope for automatic disposal:
using (var c = PhysicsComposer.Create())
{
    // add layers, generate geometry …
} // Destroy() called automatically
```

> **Note:** The real-world pattern is `PhysicsComposer.Create()` with no arguments. `Create(Allocator)` is valid and controls the allocator used for internal layer buffers; use it when creating composers inside jobs (pass `Allocator.TempJob` or `Allocator.Persistent`).
> Source: `Primer/Assets/Scripts/PhysicsComposerGeometry.cs:48`; API: `composer-api/SKILL.md:170`

### 2. Add Geometry Layers

```csharp
// AddLayer signature (all geometry overloads follow this order):
//   AddLayer(geometry, transform, operation, order, curveStride, reverseWinding)
// 'transform', 'operation', 'order', 'curveStride', and 'reverseWinding' are all optional.

var composer = PhysicsComposer.Create();

// Layer 1 — circle at the origin, implicit OR (first layer always uses OR regardless).
var circle = new CircleGeometry { radius = 1f };
composer.AddLayer(circle, PhysicsTransform.identity);

// Layer 2 — capsule offset to the right, explicit OR to merge.
var capsule = CapsuleGeometry.defaultGeometry;
composer.AddLayer(capsule, new PhysicsTransform(Vector2.right * 0.75f),
    PhysicsComposer.Operation.OR);

// Layer 3 — polygon box, explicit NOT (subtract from accumulated result).
var box = PolygonGeometry.CreateBox(new Vector2(0.5f, 0.5f), radius: 0f, inscribe: false);
composer.AddLayer(box, PhysicsTransform.identity,
    PhysicsComposer.Operation.NOT);

// Layer 4 — raw vertex span, explicit AND at a specific order index.
ReadOnlySpan<Vector2> verts = stackalloc Vector2[]
{
    new(-0.5f, 0f), new(0.5f, 0f), new(0f, 0.8f)
};
composer.AddLayer(verts, PhysicsTransform.identity,
    PhysicsComposer.Operation.AND, order: 5);

// curveStride controls tessellation of curved geometries (lower = more verts):
composer.AddLayer(circle, PhysicsTransform.identity,
    PhysicsComposer.Operation.OR, order: 0, curveStride: 0.1f);

composer.Destroy();
```

> The **first layer processed always acts as OR** regardless of the operation specified — it has nothing to merge with and forms the base geometry. Source: `PhysicsComposerGeometry.cs:53`; API: `composer-api/SKILL.md:41`

### 3. Generate Output Geometry
```csharp
// Generate filled polygon geometry
var polygonGeometry = composer.CreatePolygonGeometry(Allocator.Temp);

// Or generate outline/chain geometry
var chainGeometry = composer.CreateChainGeometry(Allocator.Temp);
```

### 4. Create Physics Shapes

```csharp
// CreatePolygonGeometry returns NativeArray<PolygonGeometry>.
// CreateShape takes a single PolygonGeometry struct — NOT the array.
// Use CreateShapeBatch to attach all polygons at once (preferred).

var composer = PhysicsComposer.Create();
composer.AddLayer(new CircleGeometry { radius = 1f }, PhysicsTransform.identity);
composer.AddLayer(CapsuleGeometry.defaultGeometry,
    new PhysicsTransform(Vector2.right * 0.75f), PhysicsComposer.Operation.OR);

// Pass vertexScale: Vector2.one to avoid geometry being discarded as too small.
using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Temp);
composer.Destroy();

if (polygons.Length == 0)
    return;  // operation produced no geometry (e.g., full subtraction)

var body = world.CreateBody();
var shapeDef = PhysicsShapeDefinition.defaultDefinition;

// Option A — batch (preferred when composer produces multiple polygons).
using var shapes = body.CreateShapeBatch(polygons, shapeDef);

// Option B — individual loop (use when you need per-shape control).
foreach (var poly in polygons)
    body.CreateShape(poly, shapeDef);
```

> `CreatePolygonGeometry` returns `NativeArray<PolygonGeometry>` — always dispose it after use. `CreateShapeBatch(ReadOnlySpan<PolygonGeometry>, …)` accepts the array directly and returns a `NativeArray<PhysicsShape>` that must also be disposed.
> Source: `PhysicsComposerGeometry.cs:60–75`; API: `bodies-api/SKILL.md:278` (`CreateShapeBatch`), `composer-api/SKILL.md:219`

### 5. Cleanup
```csharp
// Dispose composer when finished
composer.Dispose();
```

## Supported Input Geometry Types

`PhysicsComposer.AddLayer` accepts: `CircleGeometry`, `CapsuleGeometry`, `PolygonGeometry`, `PhysicsShape`, and raw `NativeArray<Vector2>` / `ReadOnlySpan<Vector2>` point lists. See `unity-physicscore2d-composer-api` for the full overload list.

> **Heads-up — `PolygonGeometry.CreatePolygons(...)` is implemented as a `PhysicsComposer` round-trip.** That static factory internally creates a transient composer, adds the supplied vertex span as a single layer, calls `CreatePolygonGeometry`, then disposes the composer. If you need *only* convex decomposition of one closed contour, the factory is a fine one-liner. If you need to add multiple layers (holes, unions, several contours), drive `PhysicsComposer` directly — using the factory and then immediately feeding its output into another composer pays for two composer round-trips for the same result. See `unity-physicscore2d-geometry-api` (`PolygonGeometry.CreatePolygons`) for the full cost note.

## Practical Examples

### Creating a Capsule-Ended Rectangle

A rectangle with semi-circular ends — useful for character bodies, rolling pegs, or pill-shaped platforms.

```csharp
// Strategy: OR a box with two circles placed at each short end.
// CapsuleGeometry is the simplest direct alternative, but this shows the composer approach.

var composer = PhysicsComposer.Create();

// Central rectangle.
var rect = PolygonGeometry.CreateBox(new Vector2(1f, 2f), radius: 0f, inscribe: false);
composer.AddLayer(rect, PhysicsTransform.identity);

// Circle cap at the top.
var topCap = new CircleGeometry { radius = 0.5f };
composer.AddLayer(topCap, new PhysicsTransform(Vector2.up), PhysicsComposer.Operation.OR);

// Circle cap at the bottom.
composer.AddLayer(topCap, new PhysicsTransform(Vector2.down), PhysicsComposer.Operation.OR);

using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Temp);
composer.Destroy();

if (polygons.Length > 0)
{
    var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic });
    using var shapes = body.CreateShapeBatch(polygons, new PhysicsShapeDefinition { density = 1f });
}
```

> **Tip:** For a true capsule without composer overhead, use `CapsuleGeometry.Create(center1, center2, radius)` directly. Use the composer version only when you also need additional boolean operations against other geometry.
> Source: adapted from `PhysicsComposerGeometry.cs`; API: `geometry-api/SKILL.md:850` (`CreateBox(Vector2, float, bool)`), `composer-api/SKILL.md:41`

### Creating a Donut Shape

A solid outer ring with a hollow centre — two circles combined with NOT.

```csharp
// OR a large outer circle, then NOT a smaller inner circle to punch the hole.
var composer = PhysicsComposer.Create();

var outerCircle = new CircleGeometry { radius = 2f };
var innerCircle = new CircleGeometry { radius = 1f };

// Outer ring — base layer (implicit OR).
composer.AddLayer(outerCircle, PhysicsTransform.identity);

// Inner hole — subtract the smaller circle at the same centre.
composer.AddLayer(innerCircle, PhysicsTransform.identity, PhysicsComposer.Operation.NOT);

// Lower curveStride = more vertices = smoother circle approximation.
// Default curveStride produces reasonable quality; reduce for higher-fidelity circles.

using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Temp);
composer.Destroy();

if (polygons.Length > 0)
{
    var body = world.CreateBody();
    using var shapes = body.CreateShapeBatch(polygons, PhysicsShapeDefinition.defaultDefinition);
}
```

> The donut will be decomposed into multiple convex polygons because it is concave. Check `composer.rejectedGeometryCount` after `CreatePolygonGeometry` if you see unexpected gaps.
> Source: adapted from `GeometryIslands.cs:294`; API: `composer-api/SKILL.md:36` (`rejectedGeometryCount`), `composer-api/SKILL.md:41`

### Creating Complex Building Shape

A base box with an arch-shaped circular protrusion on top — OR two primitives.

```csharp
// Building: tall rectangular body + circular dome on the roof.
var composer = PhysicsComposer.Create();

// Base rectangle — PolygonGeometry.CreateBox(Vector2 size, float radius, bool inscribe).
var base_ = PolygonGeometry.CreateBox(new Vector2(4f, 3f), radius: 0f, inscribe: false);
composer.AddLayer(base_, PhysicsTransform.identity);

// Dome on top — circle centred at the top edge of the rectangle.
var dome = new CircleGeometry { radius = 1.5f };
composer.AddLayer(dome, new PhysicsTransform(Vector2.up * 2f),
    PhysicsComposer.Operation.OR);

using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Temp);
composer.Destroy();

if (polygons.Length > 0)
{
    var body = world.CreateBody();  // Static by default
    using var shapes = body.CreateShapeBatch(polygons, PhysicsShapeDefinition.defaultDefinition);
}
```

> Composer produces a convex decomposition, so this L/T/dome shape will be split into several `PolygonGeometry` pieces automatically. Use `GetGeometryIslands(Allocator)` after `CreatePolygonGeometry` to identify connected groups of polygons.
> API: `geometry-api/SKILL.md:850` (`CreateBox`), `composer-api/SKILL.md:41`, `composer-api/SKILL.md:278` (`GetGeometryIslands`)

### Creating Intersected Shape

AND two overlapping circles to produce only the lens-shaped overlapping region.

```csharp
// AND keeps only the area that exists in BOTH layers — the intersection.
var composer = PhysicsComposer.Create();

var circleA = new CircleGeometry { radius = 1.5f };
var circleB = new CircleGeometry { radius = 1.5f };

// First circle at the left.
composer.AddLayer(circleA, new PhysicsTransform(Vector2.left * 0.6f));

// Second circle at the right — AND retains only the overlapping lens.
composer.AddLayer(circleB, new PhysicsTransform(Vector2.right * 0.6f),
    PhysicsComposer.Operation.AND);

using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Temp);
composer.Destroy();

if (polygons.Length > 0)
{
    var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic });
    using var shapes = body.CreateShapeBatch(polygons, new PhysicsShapeDefinition { density = 1f });
}
```

> If the circles do not overlap, AND produces zero polygons — always check `polygons.Length > 0`. The `composer.rejectedGeometryCount` property reports how many polygons were discarded for being too thin or collinear.
> API: `composer-api/SKILL.md:36` (`rejectedGeometryCount`), `composer-api/SKILL.md:41`, `composer-api/SKILL.md:399` (`AND`)

### Creating Outline/Chain Geometry

Use `CreateChainGeometry` instead of `CreatePolygonGeometry` to produce hollow edge-only collision (no interior fill). Attach the result with `body.CreateChain(ChainGeometry, PhysicsChainDefinition)` — two arguments required.

```csharp
// Outline of a merged circle + capsule as a chain (one-sided edge collision).
var composer = PhysicsComposer.Create();

composer.AddLayer(new CircleGeometry { radius = 1f }, PhysicsTransform.identity);
composer.AddLayer(CapsuleGeometry.defaultGeometry,
    new PhysicsTransform(Vector2.right * 0.75f), PhysicsComposer.Operation.OR);

// CreateChainGeometry(vertices, vertexScale, allocator):
//   'vertices' — a NativeArray<Vector2> that will be filled with the backing vertex data.
//   Size it generously; unused capacity is fine. Must stay alive until after CreateChain calls.
// Use the vertexScale overload to avoid discarding small geometry.
var vertexBuffer = new NativeArray<Vector2>(4096, Allocator.Temp);
var chains = composer.CreateChainGeometry(vertexBuffer, Vector2.one, Allocator.Temp);
composer.Destroy();

// Build the chain definition.
var chainDef = new PhysicsChainDefinition
{
    isLoop = false,
    surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.3f }
};

// Create a static body and attach each chain.
var groundBody = world.CreateBody();
foreach (var chainGeometry in chains)
    groundBody.CreateChain(chainGeometry, chainDef);  // two args required

chains.Dispose();
vertexBuffer.Dispose();
```

> `CreateChain` requires *both* a `ChainGeometry` and a `PhysicsChainDefinition`. The `vertexBuffer` passed to `CreateChainGeometry` is the backing store for all vertex data — keep it alive until after `CreateChain` completes, then dispose both arrays.
> API: `composer-api/SKILL.md:179` (`CreateChainGeometry`), `bodies-api/SKILL.md:162` (`CreateChain(ChainGeometry, PhysicsChainDefinition)`)

## Operation Order

Operations are applied in the order layers are added:

```csharp
// Implicit order: layers are processed in the order AddLayer is called.
// Explicit 'order' arg lets you insert a layer at a specific processing position.

var composer = PhysicsComposer.Create();

// Shape A — base (order 0, implicit first).
var bigBox = PolygonGeometry.CreateBox(new Vector2(4f, 4f), radius: 0f, inscribe: false);
composer.AddLayer(bigBox, PhysicsTransform.identity);  // order 0 implicit

// Shape B — a notch to subtract (order 1, explicit).
var notch = PolygonGeometry.CreateBox(new Vector2(1f, 2f), radius: 0f, inscribe: false);
composer.AddLayer(notch, new PhysicsTransform(Vector2.right * 1.5f),
    PhysicsComposer.Operation.NOT, order: 1);

// Shape C — a circle to add AFTER the NOT (order 2).
var circle = new CircleGeometry { radius = 0.8f };
composer.AddLayer(circle, new PhysicsTransform(Vector2.left * 1.5f),
    PhysicsComposer.Operation.OR, order: 2);

// Result: bigBox − notch + circle (left side fills back in after the subtraction).
// Swapping orders 1 and 2 would give: (bigBox + circle) − notch — a different shape!

using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Temp);
composer.Destroy();

if (polygons.Length > 0)
{
    var body = world.CreateBody();
    using var shapes = body.CreateShapeBatch(polygons, PhysicsShapeDefinition.defaultDefinition);
}
```

> The `order` parameter controls processing sequence — lower values run first. Layers with the same order value are processed in the order `AddLayer` was called (implicit tie-breaking).
> API: `composer-api/SKILL.md:41` (`AddLayer` params), `composer-api/SKILL.md:346` (`Layer.order`)

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
// PhysicsComposer is safe to use inside Burst-compatible jobs.
// Use Allocator.TempJob for the composer and any output arrays.
// The NativeArray<PolygonGeometry> result must be created with TempJob/Persistent
// so it outlives the job and can be consumed on the main thread.

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.U2D.Physics;
using UnityEngine;

[BurstCompile]
struct BuildComposedShapeJob : IJob
{
    // Output — caller allocates, job fills, caller disposes after schedule.
    public NativeArray<PolygonGeometry> Output;

    // Input geometry (value types — safe to copy into the job).
    public CircleGeometry   OuterCircle;
    public CircleGeometry   InnerCircle;
    public PhysicsTransform InnerTransform;

    public void Execute()
    {
        // Use TempJob inside a job — Temp is NOT allowed in jobs.
        var composer = PhysicsComposer.Create(Allocator.TempJob);
        composer.AddLayer(OuterCircle, PhysicsTransform.identity);
        composer.AddLayer(InnerCircle, InnerTransform, PhysicsComposer.Operation.NOT);

        // Write directly into the caller-provided output array.
        var polygons = composer.CreatePolygonGeometry(Vector2.one, Allocator.TempJob);
        composer.Destroy();

        // Copy results into the pre-sized output (caller must check Length).
        var copyLen = math.min(polygons.Length, Output.Length);
        NativeArray<PolygonGeometry>.Copy(polygons, Output, copyLen);
        polygons.Dispose();
    }
}

// --- Main thread scheduling ---
// var output = new NativeArray<PolygonGeometry>(64, Allocator.TempJob);
// var job = new BuildComposedShapeJob
// {
//     Output       = output,
//     OuterCircle  = new CircleGeometry { radius = 2f },
//     InnerCircle  = new CircleGeometry { radius = 1f },
//     InnerTransform = PhysicsTransform.identity
// };
// var handle = job.Schedule();
// handle.Complete();
// // use output polygons …
// output.Dispose();
```

> **Key rules for job usage:**
> - Use `Allocator.TempJob` (or `Persistent`) — `Allocator.Temp` is forbidden inside jobs.
> - `PhysicsComposer.Create(Allocator.TempJob)` passes the allocator to internal layer buffers.
> - The `NativeArray<PolygonGeometry>` returned by `CreatePolygonGeometry` inside a job must also be `TempJob`/`Persistent`; it cannot outlive the frame as a `Temp` allocation.
> - Bodies and shapes may only be created on the main thread (WORM constraint — see `unity-physicscore2d` orientation skill).
> API: `composer-api/SKILL.md:170` (`Create(Allocator)`), `composer-api/SKILL.md:219` (`CreatePolygonGeometry`)

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
