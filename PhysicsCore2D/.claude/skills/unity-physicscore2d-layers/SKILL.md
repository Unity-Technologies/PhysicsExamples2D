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
// Set up collision filtering
var shape = body.GetShape(0);

// Only collide with "Ground" and "Wall" layers
PhysicsMask collisionMask = PhysicsLayers.GetLayerMask("Ground", "Wall");
shape.contactFilter.SetLayerMask(collisionMask);

// Set this shape's layer
shape.contactFilter.SetLayer(PhysicsLayers.GetLayerOrdinal("Player"));
```

### ContactFilter Properties

```csharp
// Full ContactFilter configuration
var filter = shape.contactFilter;

// Set which layers this shape can collide with
filter.SetLayerMask(collisionMask);

// Set this shape's layer
filter.SetLayer(layerOrdinal);

// Enable/disable layer filtering
filter.useLayerMask = true;

// Other filter properties
filter.useTriggers = false; // Ignore triggers
filter.useDepth = false;    // Ignore depth filtering
```

## Query Filtering with QueryFilter

The `PhysicsQuery.QueryFilter` controls which shapes are returned from physics queries (raycasts, overlaps, etc.):

```csharp
// Create query filter
var queryFilter = new PhysicsQuery.QueryFilter
{
    // Only query against "Enemy" and "Obstacle" layers
    layerMask = PhysicsLayers.GetLayerMask("Enemy", "Obstacle"),
    useLayerMask = true
};

// Use in raycast
var results = world.Raycast(origin, direction, distance, queryFilter);

// Only shapes on specified layers will be returned
```

### QueryFilter Configuration

```csharp
var queryFilter = new PhysicsQuery.QueryFilter();

// Set layers to query
queryFilter.layerMask = PhysicsLayers.GetLayerMask("Enemy", "Player");
queryFilter.useLayerMask = true;

// Include/exclude triggers
queryFilter.useTriggers = true;

// Depth filtering
queryFilter.useDepth = false;
queryFilter.minDepth = -10f;
queryFilter.maxDepth = 10f;
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
PhysicsMask everything = PhysicsMask.Everything;

// Nothing mask
PhysicsMask nothing = PhysicsMask.Nothing;
```

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
// Player can jump through platforms from below
var platformShape = platformBody.GetShape(0);
var playerShape = playerBody.GetShape(0);

// Platform layer
platformShape.contactFilter.SetLayer(PhysicsLayers.GetLayerOrdinal("Platform"));
platformShape.contactFilter.SetLayerMask(PhysicsLayers.GetLayerMask("Player"));

// Player collides with platforms only when above
// (Implement in IContactFilterCallback)
public bool OnContactFilter2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
{
    if (IsPlatform(shapeA) && IsPlayer(shapeB))
    {
        // Allow collision only if player is above platform
        return playerBody.position.y > platformBody.position.y;
    }
    return true;
}
```

### Friendly Fire Prevention
```csharp
// Allies don't collide with each other
var allyMask = PhysicsLayers.GetLayerMask("Ally");
var enemyMask = PhysicsLayers.GetLayerMask("Enemy");

// Ally shape - only collides with enemies and environment
allyShape.contactFilter.SetLayer(PhysicsLayers.GetLayerOrdinal("Ally"));
allyShape.contactFilter.SetLayerMask(enemyMask | groundMask | wallMask);

// Enemy shape - only collides with allies and environment
enemyShape.contactFilter.SetLayer(PhysicsLayers.GetLayerOrdinal("Enemy"));
enemyShape.contactFilter.SetLayerMask(allyMask | groundMask | wallMask);
```

### Projectile Filtering
```csharp
// Projectiles ignore the shooter
var projectileShape = projectileBody.GetShape(0);

// Set projectile layer
projectileShape.contactFilter.SetLayer(PhysicsLayers.GetLayerOrdinal("Projectile"));

// Collide with everything except shooter's layer
PhysicsMask targetMask = PhysicsMask.Everything & ~shooterLayerMask;
projectileShape.contactFilter.SetLayerMask(targetMask);
```

### Layer-Based Raycasts
```csharp
// Raycast only against enemies
var enemyFilter = new PhysicsQuery.QueryFilter
{
    layerMask = PhysicsLayers.GetLayerMask("Enemy", "Boss"),
    useLayerMask = true,
    useTriggers = false
};

var hits = world.Raycast(origin, direction, maxDistance, enemyFilter);

// Raycast against everything except player
var worldFilter = new PhysicsQuery.QueryFilter
{
    layerMask = PhysicsMask.Everything & ~PhysicsLayers.GetLayerMask("Player"),
    useLayerMask = true
};

var worldHits = world.Raycast(origin, direction, maxDistance, worldFilter);
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
