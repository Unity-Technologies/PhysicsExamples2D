# Unity PhysicsCore2D - Settings and Configuration

Expert guidance on configuring PhysicsCoreSettings2D for Unity PhysicsCore2D.

## Overview

**PhysicsCoreSettings2D** is a project-wide asset that stores configuration for both Editor and runtime. It's "notably the only `class` type in the API" for physics settings management.

Key configuration areas:
- Physics layers and collision matrix
- Default definitions for worlds and bodies
- Global simulation parameters
- Debug drawing settings

## Creating the Settings Asset

### Step 1: Create Asset
In Unity Editor:
1. Navigate to menu: `Assets > Create > 2D > PhysicsCore Settings 2D`
2. Save the asset in your project (e.g., `Assets/Settings/PhysicsCoreSettings2D.asset`)

### Step 2: Assign to Project
1. Open `Project Settings > PhysicsCore 2D`
2. Drag the created asset into the settings property field
3. The asset is now the active configuration

### Step 3: Edit Settings
Two ways to edit:
- **Inspector**: Select asset in Project window, edit in Inspector
- **Project Settings**: Click "Open in Project Settings" button

## Configuration Sections

### 1. Layers Configuration

#### Enable 64-bit Physics Layers
```csharp
// In settings asset
Use Full Layers: true/false
```

- **false** (default): Use Unity's 32-bit LayerMask system
- **true**: Enable dedicated 64-bit PhysicsMask system

**Benefits of 64-bit:**
- 64 independent physics layers (vs 32 global)
- Doesn't compete with Unity's layer system
- Better for complex collision scenarios

#### Configure Layer Names
```csharp
Physics Layer Names:
  Layer 0: "Default"
  Layer 1: "Player"
  Layer 2: "Enemy"
  Layer 3: "Ground"
  Layer 4: "Projectile"
  // ... up to 64 layers
```

**Accessing in code:**
```csharp
// Get settings instance
var settings = PhysicsCoreSettings2D.instance;

// Check if using full layers
bool using64Bit = settings.useFullLayers;

// Get layer names (read-only at runtime)
string[] layerNames = settings.physicsLayerNames;
```

#### Collision Matrix Setup
Configure which layers can collide with which:

```csharp
// In settings
Layer Collision Matrix:
  Player x Ground: ✓
  Player x Enemy: ✓
  Enemy x Ground: ✓
  Enemy x Enemy: ✗
  // etc.
```

### 2. Default Definitions

#### PhysicsBodyDefinition Defaults

Configure default values for new physics bodies:

```csharp
// Settings asset fields
Default Body Type: Dynamic
Default Body Mass: 1.0
Default Linear Damping: 0.0
Default Angular Damping: 0.05
Default Gravity Scale: 1.0
Default Fixed Rotation: false
Default Is Sensor: false
Default Continuous Collision: false
```

**Accessing in code:**
```csharp
var settings = PhysicsCoreSettings2D.instance;

// Get default body definition
var bodyDef = settings.defaultBodyDefinition;

// Create body using defaults (automatically applied)
var body = world.CreateBody(bodyDef);

// Or create with custom values
var customDef = settings.defaultBodyDefinition;
customDef.type = PhysicsBodyType.Static;
customDef.mass = 10f;
var customBody = world.CreateBody(customDef);
```

#### PhysicsWorldDefinition Defaults

Configure default values for physics worlds:

```csharp
// Settings asset fields
Default Gravity: (0, -9.81)
Default Velocity Iterations: 8
Default Position Iterations: 3
Default Time Step: 0.02 (50Hz)
Default Sub Steps: 1
Default Auto Sync Transforms: true
```

**Accessing in code:**
```csharp
var settings = PhysicsCoreSettings2D.instance;

// Get default world definition
var worldDef = settings.defaultWorldDefinition;

// Create world using defaults
var world = PhysicsWorld.Create(worldDef);

// Or customize
var customWorldDef = settings.defaultWorldDefinition;
customWorldDef.gravity = new Vector2(0, -20f); // Higher gravity
customWorldDef.velocityIterations = 12; // More accuracy
var customWorld = PhysicsWorld.Create(customWorldDef);
```

**Important:** Modifying default definitions doesn't affect already-created objects. Changes take effect after:
- Restarting the Editor
- Re-entering Play mode
- Creating new objects

### 3. Global Settings

#### Simulation Parameters

```csharp
// Performance and behavior
Worker Thread Count: 4              // Parallel simulation workers
Enable Multithreading: true         // Use job system
Max Concurrent Simulations: 4       // Parallel world simulations
Warm Start: true                    // Reuse solver data
Allow Sleep: true                   // Bodies can sleep for performance
Sleep Velocity Threshold: 0.01      // Sleep when below this velocity
Sleep Time Threshold: 0.5           // Time before sleeping
```

**Accessing in code:**
```csharp
var settings = PhysicsCoreSettings2D.instance;

// Read global settings
int workerCount = settings.workerThreadCount;
bool multithreading = settings.enableMultithreading;
bool allowSleep = settings.allowSleep;

// Note: Most global settings are read-only at runtime
```

#### Debug Drawing Settings

```csharp
// Debug visualization
Draw In Build: false                // Enable debug drawing in player builds
Draw In Edit Mode: true             // Draw in edit mode
Default Draw Options: All           // What to draw by default
Default Draw Colors: <preset>       // Default color scheme
Default Draw Thickness: 1.0         // Line thickness
Default Draw Fill Alpha: 0.3        // Fill transparency
```

**Accessing in code:**
```csharp
var settings = PhysicsCoreSettings2D.instance;

// Check if drawing in builds
bool drawInBuild = settings.drawInBuild;

// Get default draw options
PhysicsWorld.DrawOptions drawOpts = settings.defaultDrawOptions;

// Get default colors
PhysicsWorld.DrawColors colors = settings.defaultDrawColors;
```

#### Contact Filtering

```csharp
// Contact generation
Enable Contact Events: true         // Generate contact events
Enable Trigger Events: true         // Generate trigger events
Max Contact Points Per Shape: 2     // Contacts per shape pair
Contact Offset: 0.01                // Contact generation distance
```

### 4. Advanced Settings

#### Determinism

```csharp
// Ensure deterministic simulation
Force Determinism: false            // Strict determinism mode
Use Deterministic Math: true        // Use PhysicsMath functions
Fixed Random Seed: 12345            // Reproducible random numbers
```

#### Memory Management

```csharp
// Memory allocation
Initial Body Capacity: 128          // Pre-allocate body slots
Initial Shape Capacity: 256         // Pre-allocate shape slots
Initial Joint Capacity: 64          // Pre-allocate joint slots
Initial Contact Capacity: 512       // Pre-allocate contact slots
```

## Practical Examples

### Configure High-Performance Settings
```csharp
void SetupHighPerformanceSettings()
{
    var settings = PhysicsCoreSettings2D.instance;

    // Modify default world definition for better performance
    var worldDef = settings.defaultWorldDefinition;
    worldDef.allowSleep = true;              // Enable sleeping
    worldDef.sleepVelocityThreshold = 0.02f; // Sleep faster
    worldDef.warmStart = true;               // Reuse solver data
    worldDef.subSteps = 1;                   // Single sub-step

    // Note: This affects NEW worlds, not existing ones
    // Must restart Play mode or create new world
}
```

### Configure High-Accuracy Settings
```csharp
void SetupHighAccuracySettings()
{
    var settings = PhysicsCoreSettings2D.instance;

    var worldDef = settings.defaultWorldDefinition;
    worldDef.velocityIterations = 12;        // More solver iterations
    worldDef.positionIterations = 4;         // More position correction
    worldDef.subSteps = 2;                   // Multiple sub-steps
    worldDef.continuousCollision = true;     // CCD for fast objects

    // Note: Higher accuracy = lower performance
}
```

### Setup Custom Layers at Runtime
```csharp
void InitializePhysicsLayers()
{
    var settings = PhysicsCoreSettings2D.instance;

    if (!settings.useFullLayers)
    {
        Debug.LogWarning("Enable 'Use Full Layers' in PhysicsCoreSettings2D");
        return;
    }

    // Layer names are configured in settings asset, not at runtime
    // Use PhysicsLayers API to work with layers
    var playerMask = PhysicsLayers.GetLayerMask("Player");
    var enemyMask = PhysicsLayers.GetLayerMask("Enemy");
    var groundMask = PhysicsLayers.GetLayerMask("Ground");

    // Setup collision filtering in code
    // (This example assumes you have bodies/shapes to configure)
}
```

### Access Default Definitions
```csharp
void CreateBodiesWithCustomDefaults()
{
    var settings = PhysicsCoreSettings2D.instance;

    // Get defaults
    var bodyDef = settings.defaultBodyDefinition;
    var worldDef = settings.defaultWorldDefinition;

    // Create world with defaults
    var world = PhysicsWorld.Create(worldDef);

    // Customize body definition
    bodyDef.type = PhysicsBodyType.Dynamic;
    bodyDef.mass = 2.0f;
    bodyDef.linearDamping = 0.1f;

    // Create body
    var body = world.CreateBody(bodyDef);

    // Default values are automatically applied from settings
}
```

### Enable Debug Drawing in Builds
```csharp
#if DEVELOPMENT_BUILD
void EnableDebugDrawing()
{
    var settings = PhysicsCoreSettings2D.instance;

    // Enable drawing in development builds
    // (Must be set in settings asset, not at runtime)
    // settings.drawInBuild = true; // Read-only at runtime!

    // Check if enabled
    if (settings.drawInBuild)
    {
        Debug.Log("Debug drawing enabled in build");
    }
}
#endif
```

### Override World Gravity
```csharp
void CreateWorldWithCustomGravity()
{
    var settings = PhysicsCoreSettings2D.instance;

    // Start with defaults
    var worldDef = settings.defaultWorldDefinition;

    // Override specific settings
    worldDef.gravity = new Vector2(0, -20f); // Double gravity
    worldDef.allowSleep = false;             // No sleeping

    // Create custom world
    var world = PhysicsWorld.Create(worldDef);
}
```

## Best Practices

### Configuration
- **Create settings asset early** - Set up before building physics systems
- **Use defaults for consistency** - Easier to maintain common values
- **Document layer assignments** - Keep a reference of what each layer is for
- **Test layer matrix** - Verify collision filtering works as expected
- **Version control settings** - Include asset in source control

### Performance
- **Enable sleeping** - Significant performance gain
- **Tune iteration counts** - Balance accuracy vs performance
- **Use appropriate sub-steps** - More sub-steps = more stable but slower
- **Configure memory capacities** - Reduce runtime allocations
- **Enable multithreading** - Use available CPU cores

### Debugging
- **Enable draw in development builds** - Essential for debugging
- **Configure useful default colors** - Distinguish different object types
- **Use appropriate draw options** - Show only what you need
- **Test in Editor and builds** - Settings may behave differently

### Determinism
- **Force determinism for networked games** - Ensures consistency
- **Use fixed random seed** - Reproducible simulations
- **Use deterministic math** - PhysicsMath functions instead of Mathf
- **Fixed timestep** - Use same timestep across all clients

## Common Pitfalls

- **Modifying settings at runtime** - Most settings are read-only after initialization
- **Forgetting to restart Play mode** - Changes to defaults require restart
- **Not assigning settings asset** - Must be assigned in Project Settings
- **Layer name mismatches** - Ensure names match between settings and code
- **Over-tuning iterations** - Too many iterations hurts performance
- **Under-allocating capacity** - Causes runtime allocations and GC

## Runtime vs Editor Configuration

### Editor-Only Settings
These are configured in the asset, not modifiable at runtime:
- Layer names
- Collision matrix
- Default definitions (require restart to take effect)
- Draw in build flag

### Runtime-Accessible Settings
These can be read (but usually not modified) at runtime:
- Current layer configuration (PhysicsLayers API)
- Default definitions (can copy and modify for new objects)
- Debug draw flags (per world)

### Per-World Settings
These are set when creating worlds and can differ between worlds:
- Gravity
- Iteration counts
- Sub-steps
- Callback modes
- Draw options

## Settings Asset Location

**Recommended location:**
```
Assets/
  Settings/
    PhysicsCoreSettings2D.asset
```

**Why:**
- Easy to find
- Logical organization
- Clear purpose
- Version control friendly

## Accessing Settings Singleton

```csharp
// Get settings instance
var settings = PhysicsCoreSettings2D.instance;

// Check if settings exist
if (settings == null)
{
    Debug.LogError("PhysicsCoreSettings2D not assigned in Project Settings!");
    return;
}

// Access settings
var layerNames = settings.physicsLayerNames;
var defaultBody = settings.defaultBodyDefinition;
var defaultWorld = settings.defaultWorldDefinition;
```

## Multiple Settings Assets

While you can create multiple PhysicsCoreSettings2D assets, only one can be active at a time (assigned in Project Settings).

**Use cases:**
- Different settings for different scenes (manually load/assign)
- Test configurations
- Performance comparison testing

**Switching settings:**
```csharp
// Not recommended - settings are meant to be project-wide
// But if needed, assign in Project Settings manually
```

## Performance Impact of Settings

### High Performance Settings
- Lower iteration counts (6-8 velocity, 2-3 position)
- Enable sleeping
- Fewer sub-steps (1)
- Disable continuous collision
- Lower worker thread count (2-4)

### High Accuracy Settings
- Higher iteration counts (10-16 velocity, 4-6 position)
- Disable sleeping (or stricter thresholds)
- More sub-steps (2-4)
- Enable continuous collision
- Higher worker thread count (match CPU cores)

### Balanced Settings (Recommended)
- 8 velocity iterations
- 3 position iterations
- Enable sleeping
- 1 sub-step
- 4 worker threads
- Continuous collision for fast objects only

## Documentation Reference

The PhysicsCoreSettings2D documentation warns:
> "Be mindful when adjusting default definitions. Your code may depend on certain default values... potentially resulting in unexpected behavior if defaults are changed without updating dependent code."

Always test thoroughly after changing default definitions.
