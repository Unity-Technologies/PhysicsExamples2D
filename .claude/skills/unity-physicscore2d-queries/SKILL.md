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
using Unity.U2D.Physics;

// Match every shape in every category — fastest to set up, broadest hit set.
var everything = PhysicsQuery.QueryFilter.Everything;

// Same semantics but expressed as the default constant:
var def = PhysicsQuery.QueryFilter.defaultFilter;

// Narrow filter: "I am a Mover (bit 1), hit only Static (bit 0) and Dynamic (bit 2)".
// categories   = what category *this query* belongs to (used by the other side's hitCategories).
// hitCategories = which categories I am willing to detect.
var moverFilter = new PhysicsQuery.QueryFilter
{
    categories    = new PhysicsMask(1),                            // MoverBit
    hitCategories = new PhysicsMask(0) | new PhysicsMask(2)        // StaticBit | DynamicBit
};

// Three-arg constructor — provide all three fields explicitly.
var customFilter = new PhysicsQuery.QueryFilter(
    categories:    PhysicsMask.All,
    hitCategories: new PhysicsMask(3),   // only "destructible" layer
    ignoreFilter:  default);
```
> Source: `PhysicsQueryJob.cs` (`PhysicsQuery.QueryFilter.Everything`), `CharacterMover.cs` (`categories`/`hitCategories` pattern), `queries-api/SKILL.md` (field list).

**Cost:** filtering happens early in the broadphase, so a tight filter is dramatically cheaper than processing every hit and discarding most.

## Pattern 1 — Sight line (raycast)

"Can A see B without walls in the way?"

```csharp
using Unity.U2D.Physics;

// Build the ray: origin + translation vector (not a direction — it is the full displacement).
// CastRayInput.FromTo(from, to) is a convenience alternative to the constructor.
var rayInput = new PhysicsQuery.CastRayInput(origin: eyePosition, translation: targetPosition - eyePosition);
// -- or --
var rayInput2 = PhysicsQuery.CastRayInput.FromTo(eyePosition, targetPosition);

// Fire. WorldCastMode.Closest returns only the nearest hit and is cheapest for sight-line tests.
// "using" disposes the NativeArray automatically (Allocator.Temp is default).
using var results = world.CastRay(
    rayInput,
    PhysicsQuery.QueryFilter.Everything,
    PhysicsQuery.WorldCastMode.Closest);

bool canSee = results.Length == 0;   // no hit = clear line of sight

if (!canSee)
{
    var hit = results[0];             // WorldCastResult
    Vector2 hitPoint  = hit.point;   // world-space contact point
    Vector2 hitNormal = hit.normal;  // surface normal pointing away from the hit shape
    float   fraction  = hit.fraction; // 0 = at origin, 1 = at full translation end
    var     hitShape  = hit.shape;    // PhysicsShape that was struck
}
```
> Source: `CastRayQuery.cs` (exact pattern), `Slicing.cs` (CastRayInput struct literal + QueryFilter with hitCategories), `world-api/SKILL.md` (CastRay signature), `queries-api/SKILL.md` (WorldCastResult fields, CastRayInput fields).

`hit.fraction` is in 0..1 along the cast (0 = origin, 1 = full distance). `hit.point` is the world-space hit; `hit.normal` is the surface normal.

## Pattern 2 — Ground check (overlap)

A small overlap test at the character's feet is more reliable than a single raycast (which can fall through gaps between ground shapes):

```csharp
using Unity.U2D.Physics;

// A small circle placed at the character's feet (world-space center).
// CircleGeometry is a value type — cache it as a field so it isn't rebuilt every frame.
var feetCircle = new CircleGeometry
{
    center = (Vector2)transform.position + Vector2.down * 0.55f,
    radius = 0.25f
};

// Only hit "ground" category (bit 0). If you have multiple ground layers, OR them together.
var groundFilter = new PhysicsQuery.QueryFilter
{
    categories    = PhysicsMask.All,
    hitCategories = new PhysicsMask(0)   // StaticBit / ground layer
};

// TestOverlapGeometry returns bool — no allocation, no hit list.
bool isGrounded = world.TestOverlapGeometry(feetCircle, groundFilter);

// If you need to know *which* shape was touched, use OverlapGeometry instead:
using var hits = world.OverlapGeometry(feetCircle, groundFilter, Unity.Collections.Allocator.Temp);
if (hits.Length > 0)
{
    var groundShape = hits[0].shape;   // WorldOverlapResult.shape
}
```
> Source: `PhysicsQueryJob.cs` (`TestOverlapPoint` pattern — same boolean-test idiom), `CharacterMover.cs` (`categories`/`hitCategories`), `world-api/SKILL.md` (`TestOverlapGeometry(CircleGeometry, QueryFilter)` and `OverlapGeometry(CircleGeometry, QueryFilter, Allocator)` signatures).

`TestOverlapShape` returns a bool — much cheaper than `OverlapShape` if you don't need the hit list.

## Pattern 3 — AOE damage (overlap, iterate)

"Apply damage to everything in this circle."

```csharp
using Unity.U2D.Physics;

// World-space explosion circle — position it at the blast centre.
var blastCircle = new CircleGeometry
{
    center = blastPosition,
    radius = blastRadius
};

// Hit every category (no friendly-fire filtering needed here).
var aoeFilter = new PhysicsQuery.QueryFilter
{
    categories    = PhysicsMask.All,
    hitCategories = PhysicsMask.All
};

// OverlapGeometry returns a NativeArray<WorldOverlapResult>. Dispose when done.
using var hits = world.OverlapGeometry(blastCircle, aoeFilter, Unity.Collections.Allocator.Temp);

for (int i = 0; i < hits.Length; i++)
{
    var result = hits[i];
    if (!result.isValid)
        continue;

    var hitBody = result.shape.body;
    if (!hitBody.isValid)
        continue;

    // Scale damage by inverse-square falloff from blast centre.
    float dist = Vector2.Distance(blastPosition, hitBody.position);
    float damage = maxDamage * (1f - dist / blastRadius);

    ApplyDamage(hitBody, damage);
}
```
> Source: `TestShadowRegion.cs` (`world.OverlapGeometry(polygonGeometry, QueryFilter, allocator)` — exact call pattern), `SpriteDestruction.cs` (`OverlapPoint` with category filter — same idiom for geometry), `world-api/SKILL.md` (`OverlapGeometry(CircleGeometry, QueryFilter, Allocator)` signature), `queries-api/SKILL.md` (`WorldOverlapResult.shape`, `WorldOverlapResult.isValid`).

Always `Dispose` the results array (or use `using`). Allocator.Temp is fastest for per-frame queries.

## Pattern 4 — Projectile sweep (shape cast)

For fast-moving projectiles, a single raycast misses thin geometry (tunneling). Cast the actual projectile shape over its movement:

```csharp
using Unity.U2D.Physics;

// The projectile is a circle of the same radius as the bullet sprite.
// CastGeometry takes a world-space geometry + translation — no separate transform needed.
var bulletCircle = new CircleGeometry
{
    center = projectilePosition,   // already in world-space
    radius = bulletRadius
};

var travelVector = projectileVelocity * Time.fixedDeltaTime;

// Only hit static environment and enemy bodies; ignore friendly bullets (category 3).
var projectileFilter = new PhysicsQuery.QueryFilter
{
    categories    = new PhysicsMask(2),  // projectile category
    hitCategories = new PhysicsMask(0) | new PhysicsMask(1)  // StaticBit | EnemyBit
};

// CastGeometry(CircleGeometry, translation, filter, castMode, allocator)
// WorldCastMode.Closest — only the first hit matters for a bullet.
using var hits = world.CastGeometry(
    bulletCircle,
    travelVector,
    projectileFilter,
    PhysicsQuery.WorldCastMode.Closest);

if (hits.Length > 0)
{
    var hit = hits[0];                       // WorldCastResult
    Vector2 impactPoint  = hit.point;
    Vector2 impactNormal = hit.normal;
    float   travelFrac   = hit.fraction;     // fraction of travelVector covered before impact
    var     hitShape     = hit.shape;

    SpawnImpactVFX(impactPoint, impactNormal);
    hitShape.body.ApplyLinearImpulseToCenter(travelVector.normalized * impactForce);
}
```
> Source: `CastGeometryQuery.cs` (exact `world.CastGeometry(geometry, castTranslation, QueryFilter.Everything)` pattern), `CharacterMover.cs` (`CastShapeProxy` with category filter — same sweep concept), `world-api/SKILL.md` (`CastGeometry(CircleGeometry, Vector2, QueryFilter, WorldCastMode, Allocator)` signature), `queries-api/SKILL.md` (`WorldCastResult` fields).

For very high-speed bodies, prefer enabling continuous collision (`continuousAllowed` on the world plus `continuous = true` on the body) over manual sweeps — but for one-shot logic (raycasts, hitscan), shape casts are simpler and don't require dedicated bodies.

## Pattern 5 — Closest-point query (AI awareness)

"Where is the nearest wall surface to me?"

```csharp
using Unity.U2D.Physics;

// Given: agentShape is the AI body's PhysicsShape, obstacleShape is the wall PhysicsShape.
// Both shapes must be valid and in the same world.

// CreateShapeProxy() builds a compact ShapeProxy from the shape's current geometry.
var distInput = new PhysicsQuery.DistanceInput
{
    shapeProxyA = agentShape.CreateShapeProxy(),
    transformA  = agentShape.body.transform,       // PhysicsTransform (world pose)
    shapeProxyB = obstacleShape.CreateShapeProxy(),
    transformB  = obstacleShape.body.transform,
    useRadii    = true   // include shape radii in distance — usually what you want
};

// PhysicsQuery.ShapeDistance is a static method — no world reference needed.
PhysicsQuery.DistanceResult dist = PhysicsQuery.ShapeDistance(distInput);

// dist.distance == 0 means the shapes overlap.
if (dist.distance < alertRange)
{
    Vector2 nearestOnAgent    = dist.pointA;   // closest point on agentShape surface
    Vector2 nearestOnObstacle = dist.pointB;   // closest point on obstacleShape surface
    Vector2 separationAxis    = dist.normal;   // unit vector from A toward B (invalid when distance == 0)
}
```
> Source: `PhysicsAPIExtensions.cs` (exact `PhysicsQuery.ShapeDistance(new PhysicsQuery.DistanceInput { shapeProxyA, shapeProxyB, transformA, transformB, useRadii })` pattern — real project code), `queries-api/SKILL.md` (`DistanceInput` fields, `DistanceResult` fields, `ShapeDistance` signature).

`PhysicsQuery.SegmentDistanceResult` gives both endpoints' nearest pair on a single line segment, useful for cone-of-vision and beam attacks.

## Pattern 6 — Will these two collide if I move? (geometry-level cast)

When you don't want to disturb the world but need to test "if my character moved by `delta`, would my proposed shape hit this specific obstacle?", use the geometry-level `CastShape` directly:

```csharp
using Unity.U2D.Physics;

// Scenario: character controller predicts whether its capsule would collide
// if it moved by `desiredMove`. The capsule geometry is already in world-space.
var capsuleGeometry = new CapsuleGeometry
{
    center1 = characterFeet,           // world-space bottom center
    center2 = characterHead,           // world-space top center
    radius  = 0.4f
};

// CastGeometry on the world accepts a geometry value + translation vector.
// No shape or body is required — pure geometry query.
using var hits = world.CastGeometry(
    capsuleGeometry,
    desiredMove,                        // how far/where to sweep
    new PhysicsQuery.QueryFilter
    {
        categories    = PhysicsMask.All,
        hitCategories = new PhysicsMask(0)  // only static geometry (walls/floors)
    },
    PhysicsQuery.WorldCastMode.Closest);

if (hits.Length > 0)
{
    var hit = hits[0];
    // Slide along the wall instead of stopping dead.
    Vector2 blocked  = hit.normal * Vector2.Dot(desiredMove, hit.normal);
    Vector2 slideDir = desiredMove - blocked;
    desiredMove = slideDir;
}
```
> Source: `CastGeometryQuery.cs` (`world.CastGeometry(geometry, translation, QueryFilter.Everything)` — direct match), `world-api/SKILL.md` (`CastGeometry(CapsuleGeometry, Vector2, QueryFilter, WorldCastMode, Allocator)` overload), `queries-api/SKILL.md` (`WorldCastResult.normal`, `WorldCastResult.fraction`).

Useful for character controllers that want to test their next step without first creating a shape.

## Pattern 7 — Time of impact (precise CCD)

For deterministic "when exactly do these two moving bodies touch?" use `PhysicsQuery.TimeOfImpactInput`:

```csharp
using Unity.U2D.Physics;

// Scenario: deterministic bullet-vs-target CCD without a physics body.
// ShapeProxy wraps a geometry type for time-of-impact calculations.
var bulletProxy = new PhysicsShape.ShapeProxy(new CircleGeometry { center = Vector2.zero, radius = bulletRadius });
var targetProxy = new PhysicsShape.ShapeProxy(new CircleGeometry { center = Vector2.zero, radius = targetRadius });

// ShapeSweep describes a body's motion over the CCD interval.
// positionStart/End are world-space centre-of-mass positions.
var bulletSweep = new PhysicsQuery.ShapeSweep
{
    localCOM       = Vector2.zero,
    positionStart  = bulletStartPos,
    positionEnd    = bulletStartPos + bulletVelocity * deltaTime,
    rotationStart  = PhysicsRotate.identity,
    rotationEnd    = PhysicsRotate.identity
};

var targetSweep = new PhysicsQuery.ShapeSweep
{
    localCOM       = Vector2.zero,
    positionStart  = targetStartPos,
    positionEnd    = targetStartPos + targetVelocity * deltaTime,
    rotationStart  = PhysicsRotate.identity,
    rotationEnd    = PhysicsRotate.identity
};

var toiResult = PhysicsQuery.ShapeTimeOfImpact(new PhysicsQuery.TimeOfImpactInput
{
    shapeProxyA  = bulletProxy,
    shapeSweepA  = bulletSweep,
    shapeProxyB  = targetProxy,
    shapeSweepB  = targetSweep,
    maxFraction  = 1f            // search the full interval [0, 1]
});

if (toiResult.impactState == PhysicsQuery.TimeOfImpactResult.State.Hit)
{
    float hitFraction  = toiResult.fraction;  // in [0, maxFraction]
    Vector2 hitPoint   = toiResult.point;
    Vector2 hitNormal  = toiResult.normal;
    // Exact world-space impact position of bullet at moment of contact:
    Vector2 impactPos  = Vector2.Lerp(bulletStartPos, bulletStartPos + bulletVelocity * deltaTime, hitFraction);
}
// Other states: Overlapped (shapes started inside each other), Separated (no hit), Failed, Unknown.
```
> Source: `queries-api/SKILL.md` (`TimeOfImpactInput` fields: `shapeProxyA`, `shapeSweepA`, `shapeProxyB`, `shapeSweepB`, `maxFraction`; `TimeOfImpactResult` fields: `fraction`, `impactState`, `point`, `normal`; `State.Hit` enum value; `ShapeTimeOfImpact` static method signature). No real project file uses this method — API-only fallback.

Reach for this for replay systems, deterministic networking, or precise hit-prediction. Overkill for casual gameplay queries.

## QueryFilter — common recipes

```csharp
using Unity.U2D.Physics;

// --- Recipe 1: Hit everything (broadest, cheapest to configure) ---
var hitAll = PhysicsQuery.QueryFilter.Everything;

// --- Recipe 2: Sight-line that sees only solid geometry (ignores triggers/sensors) ---
// Assign category bit 0 to all "solid wall" shapes via PhysicsShape.ContactFilter.categories.
// The query hits shapes whose category mask overlaps hitCategories.
var sightFilter = new PhysicsQuery.QueryFilter
{
    categories    = PhysicsMask.All,
    hitCategories = new PhysicsMask(0)  // solid walls only
};

// --- Recipe 3: Projectile hits enemies AND environment, not other projectiles ---
// PhysicsMask(n) sets bit n. Combine layers with the | operator.
const int envLayer      = 0;
const int enemyLayer    = 1;
const int projectileLayer = 2;
var bulletFilter = new PhysicsQuery.QueryFilter
{
    categories    = new PhysicsMask(projectileLayer),
    hitCategories = new PhysicsMask(envLayer) | new PhysicsMask(enemyLayer)
};

// --- Recipe 4: AOE that hits destructibles and debris ---
var destructibleMask = new PhysicsMask(3);
var debrisMask       = new PhysicsMask(4);
var aoeFilter = new PhysicsQuery.QueryFilter
{
    categories    = PhysicsMask.All,
    hitCategories = destructibleMask | debrisMask
};

// --- Recipe 5: Ignore specific body pairs ---
// world.CreateIgnoreFilter() or use PhysicsWorld.IgnoreFilter to skip particular shape pairs.
// Pass the resulting ignore filter as the ignoreFilter field:
var ignoreFilter = world.ignoreFilter;   // current world ignore filter
var filteredQuery = new PhysicsQuery.QueryFilter
{
    categories    = PhysicsMask.All,
    hitCategories = PhysicsMask.All,
    ignoreFilter  = ignoreFilter
};
```
> Source: `PhysicsQueryJob.cs` (`QueryFilter.Everything`), `CharacterMover.cs` (`categories` + `hitCategories` pattern with PhysicsMask constants), `Slicing.cs` (`new PhysicsQuery.QueryFilter { categories = PhysicsMask.All, hitCategories = m_DestructibleMask | m_GroundMask }`), `SpriteDestruction.cs` (category/hitCategory filter on `OverlapPoint`), `queries-api/SKILL.md` (QueryFilter field list).

## Sorting and limiting hits

`OverlapShape` returns hits in broadphase order, **not sorted by distance**. If you need the closest:

```csharp
using Unity.U2D.Physics;
using Unity.Collections;

// --- Sort cast hits by distance (closest first) ---
// Use WorldCastMode.AllSorted to get results pre-sorted by ascending fraction.
using var sortedHits = world.CastRay(
    PhysicsQuery.CastRayInput.FromTo(origin, target),
    PhysicsQuery.QueryFilter.Everything,
    PhysicsQuery.WorldCastMode.AllSorted);   // already sorted; index 0 is closest

// --- Sort overlap results manually by distance from a reference point ---
// OverlapGeometry does NOT sort — iterate and track minimum.
var searchCircle = new CircleGeometry { center = searchOrigin, radius = searchRadius };
using var overlapHits = world.OverlapGeometry(searchCircle, PhysicsQuery.QueryFilter.Everything, Allocator.Temp);

PhysicsShape closestShape = default;
float closestDistSq = float.MaxValue;
for (int i = 0; i < overlapHits.Length; i++)
{
    var result = overlapHits[i];
    if (!result.isValid) continue;

    float distSq = ((Vector2)result.shape.body.position - searchOrigin).sqrMagnitude;
    if (distSq < closestDistSq)
    {
        closestDistSq = distSq;
        closestShape  = result.shape;
    }
}

// --- Limit to first hit only ---
// WorldCastMode.Closest stops after the nearest hit — cheapest for "did anything block me?".
using var firstHit = world.CastRay(
    new PhysicsQuery.CastRayInput(origin, direction * maxRange),
    PhysicsQuery.QueryFilter.Everything,
    PhysicsQuery.WorldCastMode.Closest);
bool blocked = firstHit.Length > 0;
```
> Source: `Slicing.cs` (`WorldCastMode.AllSorted` — exact usage in a reflection ray loop), `CastRayQuery.cs` + `PhysicsQueryJob.cs` (`WorldCastMode.Closest`), `world-api/SKILL.md` (`OverlapGeometry(CircleGeometry, QueryFilter, Allocator)` — 3-arg signature confirmed), `queries-api/SKILL.md` (`WorldCastMode` enum values).

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
- Forgetting `hitCategories` — `QueryFilter.Everything` hits triggers and sensors too; narrow `hitCategories` to exclude them for solid-geometry-only queries.
- Iterating an undisposed `OverlapShape` result — leaks Temp memory. Use `using`.
- Treating `hit.fraction == 1` as "no hit" — the cast hit at the very end of the translation. `0..1` are all valid hits.
- Querying inside a contact callback while the world is locked — see WORM model.
