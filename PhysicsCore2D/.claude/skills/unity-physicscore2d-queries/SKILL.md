---
name: unity-physicscore2d-queries
description: Patterns for single-shot Unity PhysicsCore2D queries — raycasts (sight lines, ground checks, bullet hit-tests), overlap tests (trigger zones, AOE damage, ground checks), shape casts (projectile sweeps), distance/closest-point queries (AI awareness), and time-of-impact tests. Covers QueryFilter setup, layer masking, sorting hits, and the geometry-vs-world variants. For bulk/batched queries see unity-physicscore2d-batching. For full member API see unity-physicscore2d-queries-api.
---

# Unity PhysicsCore2D — Query Patterns

Most physics gameplay logic is built from a handful of query primitives: cast a ray, overlap a shape, find the closest point. PhysicsCore2D exposes these on `PhysicsWorld` (world queries) and on individual `*Geometry` types (shape-vs-shape tests).

> For the full type/method API surface (every overload, signature, and XML doc), see `unity-physicscore2d-queries-api`. For batched / job-based queries see `unity-physicscore2d-batching`. This skill focuses on single-shot patterns.

## Quick decision rules

| Need | Tool |
|------|------|
| "What's directly between A and B?" | `world.CastRay` |
| "Is anything inside this circle/box right now?" | `world.OverlapShape` (or `OverlapAABB` for axis-aligned boxes) |
| "Will this projectile hit anything if it moves like this?" | `world.CastShape` (sweep) |
| "How far is the nearest enemy?" | `world.OverlapShape` with a search radius + iterate |
| "Will these two specific shapes collide if A moves?" | Geometry-level `CastShape` on the input geometry |
| "Just give me a yes/no, no contact info" | `world.TestOverlap*` (cheaper than full overlap) |
| "Bulk-query 1000 rays in parallel" | See `unity-physicscore2d-batching` |

## QueryFilter — always set one explicitly

Almost every query takes a `PhysicsQuery.QueryFilter`. Defaults match everything; you almost always want to narrow it:

```csharp
var filter = PhysicsQuery.QueryFilter.defaultFilter;
filter.useLayerMask = true;
filter.layerMask    = enemyLayerMask;          // your PhysicsMask of enemy layers
filter.queryTriggers = false;                   // ignore trigger shapes
filter.queryStaticBodies = true;                // include level geometry
filter.queryDynamicBodies = true;
filter.queryKinematicBodies = true;
```

**Cost:** filtering happens early in the broadphase, so a tight filter is dramatically cheaper than processing every hit and discarding most.

## Pattern 1 — Sight line (raycast)

"Can A see B without walls in the way?"

```csharp
Vector2 origin = guard.position;
Vector2 toPlayer = player.position - origin;
float maxDist = toPlayer.magnitude;

var input = new PhysicsQuery.CastRayInput
{
    origin = origin,
    translation = toPlayer.normalized * maxDist,
    filter = obstacleFilter,
};

if (world.CastRay(input, out var hit))
{
    if (hit.shape.body == player)
        SeesPlayer();
    else
        BlockedBy(hit.shape);
}
```

`hit.fraction` is in 0..1 along the cast (0 = origin, 1 = full distance). `hit.point` is the world-space hit; `hit.normal` is the surface normal.

## Pattern 2 — Ground check (overlap)

A small overlap test at the character's feet is more reliable than a single raycast (which can fall through gaps between ground shapes):

```csharp
var groundProbe = new CircleGeometry { center = Vector2.zero, radius = 0.1f };
var probeAt = character.position + new Vector2(0, -characterHalfHeight);

var filter = PhysicsQuery.QueryFilter.defaultFilter;
filter.useLayerMask = true;
filter.layerMask = groundLayerMask;
filter.queryTriggers = false;

bool grounded = world.TestOverlapShape(groundProbe, new PhysicsTransform(probeAt, 0), filter);
```

`TestOverlapShape` returns a bool — much cheaper than `OverlapShape` if you don't need the hit list.

## Pattern 3 — AOE damage (overlap, iterate)

"Apply damage to everything in this circle."

```csharp
var aoe = new CircleGeometry { center = Vector2.zero, radius = blastRadius };
var transform = new PhysicsTransform(blastCenter, 0);
var filter = PhysicsQuery.QueryFilter.defaultFilter;
filter.useLayerMask = true;
filter.layerMask = damageableLayerMask;

using var results = world.OverlapShape(aoe, transform, filter, Allocator.Temp);
foreach (var result in results)
{
    var body = result.shape.body;
    var distance = Vector2.Distance(body.position, blastCenter);
    var falloff = 1f - Mathf.Clamp01(distance / blastRadius);
    body.GetOwner<Damageable>()?.TakeDamage(blastDamage * falloff);
}
```

Always `Dispose` the results array (or use `using`). Allocator.Temp is fastest for per-frame queries.

## Pattern 4 — Projectile sweep (shape cast)

For fast-moving projectiles, a single raycast misses thin geometry (tunneling). Cast the actual projectile shape over its movement:

```csharp
var input = new PhysicsQuery.CastShapeInput
{
    geometry = bulletShape,                              // CircleGeometry, etc.
    initialTransform = new PhysicsTransform(bullet.position, bullet.rotation),
    translation = bullet.linearVelocity * Time.deltaTime,
    filter = bulletHitFilter,
};

if (world.CastShape(input, out var hit))
{
    bullet.position = Vector2.Lerp(bullet.position, bullet.position + input.translation, hit.fraction);
    OnBulletHit(hit.shape, hit.point, hit.normal);
    bullet.Destroy();
}
else
{
    bullet.position += input.translation;
}
```

For very high-speed bodies, prefer enabling continuous collision (`continuousAllowed` on the world plus `continuous = true` on the body) over manual sweeps — but for one-shot logic (raycasts, hitscan), shape casts are simpler and don't require dedicated bodies.

## Pattern 5 — Closest-point query (AI awareness)

"Where is the nearest wall surface to me?"

```csharp
// Search a generous radius first to find candidates.
var searchShape = new CircleGeometry { center = Vector2.zero, radius = awarenessRange };
using var nearby = world.OverlapShape(searchShape, new PhysicsTransform(npc.position, 0), wallFilter, Allocator.Temp);

float closestDist = float.MaxValue;
Vector2 closestPoint = default;
foreach (var result in nearby)
{
    // Geometry-level distance query for the precise closest point.
    var distInput = new PhysicsQuery.DistanceInput
    {
        shapeA = npc.GetShape(0).geometry,
        transformA = npc.transform,
        shapeB = result.shape.geometry,
        transformB = result.shape.body.transform,
    };
    var distResult = PhysicsQuery.Distance(distInput);
    if (distResult.distance < closestDist)
    {
        closestDist = distResult.distance;
        closestPoint = distResult.pointB;   // closest point on the wall
    }
}
```

`PhysicsQuery.SegmentDistanceResult` gives both endpoints' nearest pair on a single line segment, useful for cone-of-vision and beam attacks.

## Pattern 6 — Will these two collide if I move? (geometry-level cast)

When you don't want to disturb the world but need to test "if my character moved by `delta`, would my proposed shape hit this specific obstacle?", use the geometry-level `CastShape` directly:

```csharp
var input = new PhysicsQuery.CastShapeInput
{
    geometry = otherShape.geometry,
    initialTransform = otherShape.body.transform,
    translation = Vector2.zero,
};

if (myProposedGeometry.CastShape(input, out var hit))
{
    // Adjust proposed position by hit.fraction.
}
```

Useful for character controllers that want to test their next step without first creating a shape.

## Pattern 7 — Time of impact (precise CCD)

For deterministic "when exactly do these two moving bodies touch?" use `PhysicsQuery.TimeOfImpactInput`:

```csharp
var input = new PhysicsQuery.TimeOfImpactInput
{
    proxyA = shapeA.GetProxy(),
    sweepA = shapeA.body.GetSweep(0f, 1f, deltaTime),
    proxyB = shapeB.GetProxy(),
    sweepB = shapeB.body.GetSweep(0f, 1f, deltaTime),
    tMax = 1f,
};
var toi = PhysicsQuery.TimeOfImpact(input);
if (toi.state == PhysicsQuery.TimeOfImpactResult.State.Touching)
{
    float collisionTime = toi.t * deltaTime;
    // ...
}
```

Reach for this for replay systems, deterministic networking, or precise hit-prediction. Overkill for casual gameplay queries.

## QueryFilter — common recipes

```csharp
// Hit only level geometry.
var levelOnly = PhysicsQuery.QueryFilter.defaultFilter;
levelOnly.useLayerMask = true;
levelOnly.layerMask = levelLayerMask;
levelOnly.queryTriggers = false;
levelOnly.queryDynamicBodies = false;

// Pickup detection — triggers only.
var pickups = PhysicsQuery.QueryFilter.defaultFilter;
pickups.queryTriggers = true;
pickups.queryStaticBodies = false;
pickups.queryKinematicBodies = false;
pickups.queryDynamicBodies = false;
pickups.useLayerMask = true;
pickups.layerMask = pickupLayerMask;

// Faction-aware — exclude self via group filtering on the shape; broad layer match here.
var factionAware = PhysicsQuery.QueryFilter.defaultFilter;
factionAware.useLayerMask = true;
factionAware.layerMask = anyFactionMask;
// Combine with the shape's own ContactFilter group ID for "ignore my own faction".
```

## Sorting and limiting hits

`OverlapShape` returns hits in broadphase order, **not sorted by distance**. If you need the closest:

```csharp
using var hits = world.OverlapShape(shape, transform, filter, Allocator.Temp);
PhysicsQuery.WorldOverlapResult closest = default;
float closestSqr = float.MaxValue;
foreach (var h in hits)
{
    float sqr = (h.shape.body.position - origin).sqrMagnitude;
    if (sqr < closestSqr) { closestSqr = sqr; closest = h; }
}
```

For "first hit only", `CastRay` already returns the nearest along the ray.

## Best practices

- **Always set a `QueryFilter`** — defaults match everything and are slow.
- **Prefer `TestOverlap*` for boolean checks** — half the work of a full `Overlap*`.
- **Use `Allocator.Temp` for per-frame query results** — fastest, auto-freed within the frame.
- **Sweep, don't single-raycast, fast objects** — single rays miss between fixed-step samples.
- **Cache shape geometry** — reuse `CircleGeometry`/`CapsuleGeometry` value instances rather than allocating per call.
- **Run queries during simulation OR between simulations, not both** — see WORM rules in `unity-physicscore2d` umbrella for thread-safety details.

## Common pitfalls

- Casting a ray of length `0` — returns empty even if the origin overlaps. Use `OverlapPoint` for "is this point inside anything?".
- Forgetting `queryTriggers` — by default triggers ARE queried, which is usually wrong for "did my bullet hit a wall".
- Iterating an undisposed `OverlapShape` result — leaks Temp memory. Use `using`.
- Treating `hit.fraction == 1` as "no hit" — the cast hit at the very end of the translation. `0..1` are all valid hits.
- Querying inside a contact callback while the world is locked — see WORM model.
