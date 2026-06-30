# Unity PhysicsCore2D - Layers and Collision Filtering

Expert guidance on using physics layers and collision filtering in Unity PhysicsCore2D.

## Layer Systems Overview

Unity supports two distinct layer systems for 2D physics:

### 1. Global Layer System (32-bit)
- Uses Unity's built-in `LayerMask`
- Each bit represents one layer (32 total)
- Shared across all Unity systems (rendering, raycasts, etc.)
- Limited capacity for complex games

### 2. Physics Layer System (64-bit)
- Uses dedicated `PhysicsMask`
- 64 independent layers for physics
- Separate from Unity's global layers
- Recommended for complex collision scenarios

**Why 64 layers?** The documentation explains: "32 layers can become limiting because many systems and components rely on them." The physics layer system provides dedicated layers without competing with other Unity features.

## Enabling Physics Layer System

Configure in `PhysicsCoreSettings2D`:

1. Create or open PhysicsCoreSettings2D asset
2. Toggle **"Use Full Layers"** to enable 64-bit physics layers
3. Configure **"Physics Layer Names"** for custom layer names

**Safe to switch:** Switching between 32-bit and 64-bit systems is safe. `PhysicsMask` values remain unchanged; only their mapped names differ.

## PhysicsLayers API

The `PhysicsLayers` static class provides conversion methods between layer names, numbers, and masks:

### GetLayerMask
Get a mask from one or more layer names:

```csharp
// Single layer
PhysicsMask playerMask = PhysicsLayers.GetLayerMask("Player");

// Multiple layers
PhysicsMask enemiesMask = PhysicsLayers.GetLayerMask("Enemy", "Boss", "Projectile");

// Create mask that includes these layers
```

### GetLayerOrdinal
Get the layer number from a layer name:

```csharp
// Get layer index
int playerLayer = PhysicsLayers.GetLayerOrdinal("Player");
// Returns: 8 (for example)
```

### GetLayerName
Get the layer name from a layer number:

```csharp
// Get layer name
string layerName = PhysicsLayers.GetLayerName(8);
// Returns: "Player"
```

## Collision Filtering with ContactFilter

The `PhysicsShape.ContactFilter` controls which shape pairs can create physical contacts (collisions):

```csharp
// Source: PhysicsShapeContactFiltering.cs (Primer project)
using Unity.U2D.Physics;
using UnityEngine;

// ContactFilter controls which shape pairs produce physical contacts.
// categories = which physics layers this shape belongs to (64-bit PhysicsMask).
// contacts   = which physics layers this shape will collide with.
var shapeDef = new PhysicsShapeDefinition
{
    // This is equivalent to PhysicsShape.ContactFilter.defaultFilter:
    // belongs to layer bit-0 (PhysicsMask.One), collides with everything.
    contactFilter = new PhysicsShape.ContactFilter
    {
        categories = PhysicsMask.One,
        contacts   = PhysicsMask.All
    },
    surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0.8f }
};

var world = PhysicsWorld.Create();
var body  = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic });
body.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
```

### ContactFilter Properties

```csharp
// Source: CharacterMover.cs (Sandbox) and Slicing.cs (Sandbox)
// PhysicsShape.ContactFilter has exactly three fields: categories, contacts, groupIndex.
using Unity.U2D.Physics;

// --- categories / contacts ---
// Declare named masks using bit indices (bit 0 = PhysicsMask.One).
var groundMask      = new PhysicsMask(0);   // bit 0
var destructibleMask = new PhysicsMask(3);  // bit 3

// Ground shape: belongs to groundMask, collides with everything.
var groundContactFilter = new PhysicsShape.ContactFilter
{
    categories = groundMask,
    contacts   = PhysicsMask.All
};

// Destructible shape: belongs to destructibleMask, collides with everything.
var destructibleContactFilter = new PhysicsShape.ContactFilter
{
    categories = destructibleMask,
    contacts   = PhysicsMask.All
};

// --- groupIndex (negative = never contact, positive = always contact) ---
// Source: SoftbodyFactory.cs — soft-body segments never collide with each other.
var softBodyFilter = new PhysicsShape.ContactFilter
{
    categories = PhysicsMask.One,
    contacts   = PhysicsMask.All,
    groupIndex = -1   // identical negative group: siblings never contact
};

// --- CanContact helper ---
// Check whether two filters would produce a contact before creating shapes.
bool wouldContact = groundContactFilter.CanContact(destructibleContactFilter);
```

## Query Filtering with QueryFilter

The `PhysicsQuery.QueryFilter` controls which shapes are returned from physics queries (raycasts, overlaps, etc.):

```csharp
// Source: CastRayQuery.cs (Primer) and Slicing.cs (Sandbox)
// QueryFilter controls which shapes a query hits. Fields: categories, hitCategories, ignoreFilter.
// categories    = the "identity" mask of the query (what layer the query comes from).
// hitCategories = which physics layers the query can detect.
using Unity.U2D.Physics;
using UnityEngine;

var world = PhysicsWorld.defaultWorld;

// Simplest case: hit everything (uses QueryFilter.Everything).
using var allHits = world.CastRay(
    new PhysicsQuery.CastRayInput { origin = Vector2.zero, translation = Vector2.up * 10f },
    PhysicsQuery.QueryFilter.Everything);

// Targeted case: a projectile query that only hits ground and destructible layers.
var groundMask      = new PhysicsMask(2);  // bit 2
var destructibleMask = new PhysicsMask(3); // bit 3

using var filteredHits = world.CastRay(
    new PhysicsQuery.CastRayInput { origin = Vector2.zero, translation = Vector2.up * 10f },
    new PhysicsQuery.QueryFilter
    {
        categories    = PhysicsMask.All,
        hitCategories = destructibleMask | groundMask
    });
```

### QueryFilter Configuration

```csharp
// Source: CharacterMover.cs (Sandbox) — real ground-check and mover-cast filters
using Unity.U2D.Physics;

// Declare named masks by bit index.
// PhysicsMask(int[]) sets multiple bits; new PhysicsMask(n) sets bit n alone.
var staticBit  = new PhysicsMask(0);   // Static environment
var moverBit   = new PhysicsMask(1);   // Character movers
var dynamicBit = new PhysicsMask(2);   // Dynamic rigidbodies

// Pogo/ground-check filter: query is "from" the mover, hits static + dynamic only.
var groundCheckFilter = new PhysicsQuery.QueryFilter
{
    categories    = moverBit,
    hitCategories = staticBit | dynamicBit
};

// Overlap filter: mover checks for depenetration against all solid layers.
var overlapFilter = new PhysicsQuery.QueryFilter
{
    categories    = moverBit,
    hitCategories = staticBit | dynamicBit | moverBit
};

// Cast filter: sweeps only against static + dynamic (movers use soft collision).
var castFilter = new PhysicsQuery.QueryFilter
{
    categories    = moverBit,
    hitCategories = staticBit | dynamicBit
};

// Use with any world query, e.g.:
var world = PhysicsWorld.defaultWorld;
using var hits = world.CastRay(
    new PhysicsQuery.CastRayInput { origin = UnityEngine.Vector2.zero, translation = UnityEngine.Vector2.down * 5f },
    groundCheckFilter);
```

## PhysicsMask Operations

`PhysicsMask` is a 64-bit mask that supports bitwise operations:

```csharp
// Create masks
PhysicsMask playerMask = PhysicsLayers.GetLayerMask("Player");
PhysicsMask enemyMask = PhysicsLayers.GetLayerMask("Enemy");
PhysicsMask groundMask = PhysicsLayers.GetLayerMask("Ground");

// Combine masks (OR)
PhysicsMask combinedMask = playerMask | enemyMask | groundMask;

// Remove layers (AND NOT)
PhysicsMask withoutPlayer = combinedMask & ~playerMask;

// Check if layer is in mask
bool hasPlayer = (combinedMask & playerMask) != 0;

// Everything mask
PhysicsMask everything = PhysicsMask.All;

// Nothing mask
PhysicsMask nothing = PhysicsMask.None;
```

### Prefer the named constants over raw bitMask comparisons

`PhysicsMask` exposes three canonical constants. Always use them for construction, comparison, and validation. Do not reach into `.bitMask` for these cases.

| Constant            | Meaning                                                  | Backing value         |
|---------------------|----------------------------------------------------------|-----------------------|
| `PhysicsMask.None`  | No bits set. Equivalent to `default(PhysicsMask)`.       | `bitMask == 0x0`      |
| `PhysicsMask.One`   | Only bit 0 set. The default for new shapes.              | `bitMask == 0x1`      |
| `PhysicsMask.All`   | Every bit set (all 64 layers).                           | `bitMask == ulong.MaxValue` |

```csharp
// Good: read at the right level of abstraction.
if (input.mask == PhysicsMask.None)
    throw new ArgumentException("mask is empty");

if (filter.categories == PhysicsMask.All)
    return; // hits every layer, no further filtering needed

var defaultCategories = PhysicsMask.One;

// Avoid: reaching past the abstraction.
if (input.mask.bitMask == 0u) { ... }            // use PhysicsMask.None
if (filter.categories.bitMask == ulong.MaxValue) // use PhysicsMask.All
    ...
```

The constants are `static readonly` fields on `PhysicsMask`, so the comparison compiles to the same code as the raw `bitMask` check while keeping intent visible at the call site.

## Display Attributes

Use `PhysicsMask.ShowAsPhysicsMaskAttribute` when a `PhysicsMask` field represents arbitrary flag data rather than actual layers:

```csharp
public class MyComponent : MonoBehaviour
{
    // Displays as layer names in Inspector
    public PhysicsMask collisionLayers;

    // Displays as raw 64-bit mask (for flags, not layers)
    [PhysicsMask.ShowAsPhysicsMask]
    public PhysicsMask customFlags;
}
```

## Common Patterns

### One-Way Platforms
```csharp
// Source: PhysicsShapeContactFiltering.cs (Primer project)
// One-way platforms: use IContactFilterCallback to veto contacts at simulation time.
// THREAD-SAFETY REQUIREMENT: OnContactFilter2D can be called off the main thread.
// Do NOT perform write operations on the physics world inside the callback.
using Unity.U2D.Physics;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour, PhysicsCallbacks.IContactFilterCallback
{
    private PhysicsWorld m_PhysicsWorld;
    private PhysicsShape m_PlatformShape;

    private void OnEnable()
    {
        m_PhysicsWorld = PhysicsWorld.Create();

        // Enable contact-filter callbacks on the world.
        m_PhysicsWorld.contactFilterCallbacks = true;

        // Create a static platform body.
        var platformBody = m_PhysicsWorld.CreateBody();
        var platformDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = PhysicsMask.One,
                contacts   = PhysicsMask.All
            },
            // Mark this shape as needing filter callbacks.
            contactFilterCallbacks = true
        };
        m_PlatformShape = platformBody.CreateShape(
            PolygonGeometry.CreateBox(new Vector2(4f, 0.2f)),
            platformDef);

        // This MonoBehaviour is the callback target.
        m_PlatformShape.callbackTarget = this;
    }

    // Return false to suppress the contact (body passes through).
    // Return true to allow the contact (body lands on platform).
    // NOTE: Must be thread-safe — read userData/shapeType only.
    public bool OnContactFilter2D(PhysicsEvents.ContactFilterEvent contactFilterEvent)
    {
        // Identify which shape is the platform and which is the visitor.
        var platform = contactFilterEvent.shapeA == m_PlatformShape
            ? contactFilterEvent.shapeA
            : contactFilterEvent.shapeB;
        var visitor = platform == contactFilterEvent.shapeA
            ? contactFilterEvent.shapeB
            : contactFilterEvent.shapeA;

        // Allow contact only if the visitor is moving downward (approaching from above).
        // userData can carry a pre-computed "movingDown" flag set on the main thread.
        return visitor.userData.boolValue; // true = falling down, allow landing
    }

    private void OnDisable() => m_PhysicsWorld.Destroy();
}
```

### Friendly Fire Prevention
```csharp
// Source: RagdollFactory.cs (Sandbox) — ragdoll body parts that don't self-collide.
// Negative identical groupIndex: shapes with the same negative group NEVER contact each other.
// Positive identical groupIndex: shapes with the same positive group ALWAYS contact each other.
// A groupIndex of 0 falls back to normal categories/contacts mask evaluation.
using Unity.U2D.Physics;

// Assign each ragdoll a unique positive group index (e.g. from a counter).
int ragdollGroup = 7; // unique per ragdoll instance

// Body layer mask for this ragdoll.
var bodyLayer = new PhysicsMask(4); // bit 4 = "Character"
var envLayer  = new PhysicsMask(0); // bit 0 = "Environment"

// Torso shape: collides with environment but never with own ragdoll parts.
var torsoFilter = new PhysicsShape.ContactFilter
{
    categories = bodyLayer,
    contacts   = bodyLayer | envLayer,
    groupIndex = -ragdollGroup   // negative = never contact same-group shapes
};

// Alternative: allies that always contact each other regardless of masks.
var allyFilter = new PhysicsShape.ContactFilter
{
    categories = bodyLayer,
    contacts   = bodyLayer,
    groupIndex = +ragdollGroup   // positive = always contact same-group shapes
};

var shapeDef = new PhysicsShapeDefinition { contactFilter = torsoFilter };
var world = PhysicsWorld.defaultWorld;
var body  = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic });
body.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
```

### Projectile Filtering
```csharp
// Source: CharacterMover.cs (Sandbox) — shape ContactFilter + body GetShapes iteration
// Projectiles collide with environment and players but not with other projectiles.
using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;

// Layer declarations (cached as statics or fields — never call GetLayerMask every frame).
static readonly PhysicsMask ProjectileMask   = PhysicsLayers.GetLayerMask("Projectile");
static readonly PhysicsMask PlayerMask       = PhysicsLayers.GetLayerMask("Player");
static readonly PhysicsMask EnvironmentMask  = PhysicsLayers.GetLayerMask("Environment");

// Projectile shape: belongs to ProjectileMask, hits players + environment only.
var projectileFilter = new PhysicsShape.ContactFilter
{
    categories = ProjectileMask,
    contacts   = PlayerMask | EnvironmentMask   // NOT ProjectileMask → no projectile–projectile hits
};

var world          = PhysicsWorld.defaultWorld;
var projectileBody = world.CreateBody(new PhysicsBodyDefinition
{
    type     = PhysicsBody.BodyType.Dynamic,
    position = Vector2.zero
});
projectileBody.CreateShape(CircleGeometry.defaultGeometry,
    new PhysicsShapeDefinition { contactFilter = projectileFilter });

// If you need to update the filter on all shapes of an existing body at runtime:
using var shapes = projectileBody.GetShapes();   // body.GetShape(int) does NOT exist
foreach (var shape in shapes)
    shape.contactFilter = projectileFilter;
```

### Layer-Based Raycasts
```csharp
// Source: Slicing.cs (Sandbox) + CastRayQuery.cs (Primer)
// Layer-based raycasts use QueryFilter.hitCategories — there is NO layerMask field.
using Unity.U2D.Physics;
using UnityEngine;

var world = PhysicsWorld.defaultWorld;

// --- Sight-line check (hits everything) ---
using var sightHits = world.CastRay(
    new PhysicsQuery.CastRayInput { origin = Vector2.zero, translation = Vector2.up * 20f },
    PhysicsQuery.QueryFilter.Everything);

// --- Projectile raycast (named layers via PhysicsLayers) ---
// Cache masks as statics — PhysicsLayers.GetLayerMask takes a params string[].
var groundMask       = PhysicsLayers.GetLayerMask("Ground");
var destructibleMask = PhysicsLayers.GetLayerMask("Destructible");

using var projectileHits = world.CastRay(
    new PhysicsQuery.CastRayInput { origin = Vector2.zero, translation = Vector2.right * 30f },
    new PhysicsQuery.QueryFilter
    {
        categories    = PhysicsMask.All,       // "from" any layer
        hitCategories = groundMask | destructibleMask
    },
    PhysicsQuery.WorldCastMode.AllSorted);     // sorted nearest-first

// --- Read the hit shape's own category for branching ---
foreach (var hit in projectileHits)
{
    var hitCategory = hit.shape.contactFilter.categories;
    if (hitCategory == destructibleMask)
        Debug.Log("Hit destructible");
    else if (hitCategory == groundMask)
        Debug.Log("Hit ground");
}
```

## Best Practices

- **Use 64-bit physics layers for complex games** - More flexibility than 32-bit
- **Name layers clearly** - "Player", "Enemy", "Ground" vs "Layer8", "Layer9"
- **Set up collision matrix early** - Define which layers interact at project start
- **Use ContactFilter for gameplay rules** - One-way platforms, friendly fire, etc.
- **Use QueryFilter to optimize queries** - Only query relevant layers
- **Cache layer masks** - Don't call GetLayerMask every frame
- **Document layer usage** - Maintain a list of what each layer represents

## Collision Matrix Setup

Configure the collision matrix in `PhysicsCoreSettings2D`:

1. Enable "Use Full Layers" for 64-bit system
2. Define layer names in "Physics Layer Names"
3. Set up collision matrix in settings
4. Or configure programmatically in code:

```csharp
// Set up which layers collide with which
// (Implementation depends on your physics system version)
```

## Performance Considerations

- **Layer masks are fast** - Bitwise operations are very efficient
- **Fewer layers = simpler logic** - Don't use all 64 unless needed
- **Query filters reduce work** - Physics engine skips irrelevant shapes
- **Contact filters prevent unnecessary contacts** - Saves solver work
