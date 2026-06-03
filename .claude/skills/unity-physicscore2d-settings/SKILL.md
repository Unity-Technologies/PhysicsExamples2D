# Unity PhysicsCore2D - Settings and Configuration

> **⚠ UNVERIFIED — examples below may contain API errors.** Casing patch applied
> (`PhysicsBody.BodyType.X` → `PhysicsBody.BodyType.X`); other symbols not audited. Cross-check
> against `unity-physicscore2d-bodies-api` / `unity-physicscore2d-world-api` and real code in
> `D:/UnitySrc/GitHub/PhysicsExamples2D/` before relying on any example. See memory
> `skill-examples-must-be-verified-api`.

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
// PhysicsCoreSettings2D is a ScriptableObject asset assigned in Project Settings.
// 'instance' is used throughout the ecosystem but is not listed in the generated API;
// treat it as unverified — the property and asset reference are set up in Project Settings.
var settings = PhysicsCoreSettings2D.instance;

// Check whether 64-bit PhysicsMask layers are active
// (world-api: PhysicsCoreSettings2D.usePhysicsLayers)
bool using64Bit = settings.usePhysicsLayers;

// Alternatively, query the same flag from a live world:
// PhysicsWorld.usePhysicsLayers reflects the settings value.
bool worldUsing64Bit = PhysicsWorld.defaultWorld.usePhysicsLayers;

// Get the layer mask for named layers (PhysicsLayers.GetLayerMask takes a string[])
// layers-api: GetLayerMask(string[])
if (using64Bit)
{
    PhysicsMask playerMask = PhysicsLayers.GetLayerMask(new[] { "Player" });
    PhysicsMask groundMask = PhysicsLayers.GetLayerMask(new[] { "Ground" });
}
```
<!-- Source: world-api (usePhysicsLayers at line 60, PhysicsWorld.usePhysicsLayers at line 194),
     layers-api (GetLayerMask(string[]) at line 31), batching/examples/Boids.cs (PhysicsWorld.defaultWorld) -->

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
// Option A — read the default definition from the settings asset property
// (world-api: PhysicsCoreSettings2D.physicsBodyDefinition)
var settings = PhysicsCoreSettings2D.instance;
PhysicsBodyDefinition bodyDef = settings.physicsBodyDefinition;

// Inspect default values (bodies-api: PhysicsBodyDefinition properties)
PhysicsBody.BodyType defaultType = bodyDef.type;       // typically Dynamic
float defaultGravityScale = bodyDef.gravityScale;      // typically 1.0
bool defaultSleepingAllowed = bodyDef.sleepingAllowed; // typically true

// Option B — construct a definition that pulls from settings automatically
// (bodies-api: PhysicsBodyDefinition.new(bool useSettings))
var defFromSettings = new PhysicsBodyDefinition(useSettings: true);

// Use the definition to create a body (real-code pattern from filtering/examples)
var world = PhysicsWorld.defaultWorld;
var body = world.CreateBody(defFromSettings);
```
<!-- Source: world-api (physicsBodyDefinition line 47), bodies-api (PhysicsBodyDefinition properties
     lines 856-876, new(bool) line 888), filtering/examples/BasicContactFiltering.cs (world.CreateBody pattern) -->

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
// Option A — read the default definition from the settings asset property
// (world-api: PhysicsCoreSettings2D.physicsWorldDefinition)
var settings = PhysicsCoreSettings2D.instance;
PhysicsWorldDefinition worldDef = settings.physicsWorldDefinition;

// Inspect default values (world-api: PhysicsWorldDefinition properties lines 2022-2061)
Vector2 defaultGravity = worldDef.gravity;                   // typically (0, -9.81)
int defaultSubSteps = worldDef.simulationSubSteps;           // NOT 'subSteps'
bool defaultSleeping = worldDef.sleepingAllowed;             // NOT 'allowSleep'
bool defaultContinuous = worldDef.continuousAllowed;         // NOT 'continuousCollision'

// Option B — construct a definition that pulls from settings automatically
// (world-api: PhysicsWorldDefinition.new(bool useSettings))
var defFromSettings = new PhysicsWorldDefinition(useSettings: true);

// Create a world using this definition (world-api: PhysicsWorld.Create(PhysicsWorldDefinition))
var world = PhysicsWorld.Create(defFromSettings);
```
<!-- Source: world-api (physicsWorldDefinition line 57, PhysicsWorldDefinition properties lines
     2022-2061, new(bool) line 2068-2073, PhysicsWorld.Create(PhysicsWorldDefinition) line 307-314) -->

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
// Read global simulation parameters from settings
// (world-api: PhysicsCoreSettings2D properties lines 38-60)
var settings = PhysicsCoreSettings2D.instance;

// How many worlds can simulate in parallel
// (world-api: concurrentSimulations)
int maxParallel = settings.concurrentSimulations;

// Whether automatic simulation is globally suppressed
// (world-api: disableSimulation)
bool simDisabled = settings.disableSimulation;

// Per-world simulation parameters are on PhysicsWorld or PhysicsWorldDefinition,
// NOT on PhysicsCoreSettings2D. Configure them at world creation time:
var worldDef = new PhysicsWorldDefinition(useSettings: true);

// (world-api: simulationWorkers, simulationSubSteps, sleepingAllowed, continuousAllowed)
worldDef.simulationWorkers = 4;       // worker threads for this world
worldDef.simulationSubSteps = 1;      // sub-steps per fixed update
worldDef.sleepingAllowed = true;      // bodies may sleep for performance
worldDef.continuousAllowed = true;    // CCD against static bodies

var world = PhysicsWorld.Create(worldDef);

// After creation, read back live properties from the world instance
int liveWorkers = world.simulationWorkers;
bool liveSleep = world.sleepingAllowed;
```
<!-- Source: world-api (PhysicsCoreSettings2D.concurrentSimulations line 41, disableSimulation line 44;
     PhysicsWorldDefinition.simulationWorkers line 2054, simulationSubSteps line 2053,
     sleepingAllowed line 2055, continuousAllowed line 2037) -->

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
// The rendering mode is stored on PhysicsCoreSettings2D and controls in which
// builds debug drawing/rendering is allowed.
// (world-api: PhysicsCoreSettings2D.renderingMode, PhysicsWorld.RenderingMode enum)
var settings = PhysicsCoreSettings2D.instance;
PhysicsWorld.RenderingMode mode = settings.renderingMode;
// Enum values (world-api: RenderingMode fields lines 1755-1759):
//   PhysicsWorld.RenderingMode.EditorOnly        — Editor only (default)
//   PhysicsWorld.RenderingMode.DevelopmentPlayer — Editor + development builds
//   PhysicsWorld.RenderingMode.AnyPlayer         — Editor + all builds

// Check a live world to see if rendering is currently allowed
// (world-api: PhysicsWorld.renderingMode, PhysicsWorld.isRenderingAllowed)
var world = PhysicsWorld.defaultWorld;
bool canRender = world.isRenderingAllowed;

// Per-world draw configuration (set these at runtime on the world instance):
// (world-api: drawOptions, drawFillAlpha, drawThickness, drawColors)
world.drawOptions = PhysicsWorld.DrawOptions.All;  // real-code: debug/SKILL.md:35
world.drawFillAlpha = 0.3f;
world.drawThickness = 1.0f;
```
<!-- Source: world-api (PhysicsCoreSettings2D.renderingMode line 58, PhysicsWorld.RenderingMode
     enum lines 1747-1759, PhysicsWorld.isRenderingAllowed line 165, drawOptions line 154,
     drawFillAlpha line 149, drawThickness line 156),
     debug/SKILL.md line 35 (real-code usage of drawOptions) -->

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
// Call once at startup (before creating any worlds that use these defaults).
// Changes to the settings asset's physicsWorldDefinition only affect NEW worlds.
void SetupHighPerformanceSettings()
{
    // Build a world definition optimised for throughput.
    // Construct with useSettings:true so all other fields start from the
    // project's defaults, then override only what we need.
    // (world-api: PhysicsWorldDefinition.new(bool), properties lines 2022-2061)
    var worldDef = new PhysicsWorldDefinition(useSettings: true);

    worldDef.sleepingAllowed  = true;  // bodies sleep when idle — big win
    worldDef.simulationSubSteps = 1;   // one sub-step per fixed update
    worldDef.continuousAllowed  = false; // skip CCD (minor cost, only disable
                                         // if no fast-moving objects)
    worldDef.simulationWorkers  = 4;   // parallel solver workers

    // Create the world with these settings.
    // (world-api: PhysicsWorld.Create(PhysicsWorldDefinition) line 307)
    var world = PhysicsWorld.Create(worldDef);

    // Optionally disable global simulation temporarily while populating:
    // world.paused = true; ... add bodies ... world.paused = false;
}
```
<!-- Source: world-api (PhysicsWorldDefinition.sleepingAllowed line 2055,
     simulationSubSteps line 2053, continuousAllowed line 2037,
     simulationWorkers line 2054, PhysicsWorld.Create(def) lines 307-314) -->

### Configure High-Accuracy Settings
```csharp
// IMPORTANT: PhysicsWorldDefinition has no velocityIterations / positionIterations.
// The solver iteration count is not exposed as a definition property in this API version.
// Accuracy is tuned through sub-steps, CCD, and contact spring parameters instead.
// (world-api: PhysicsWorldDefinition properties, lines 2022-2061)
void SetupHighAccuracySettings()
{
    var worldDef = new PhysicsWorldDefinition(useSettings: true);

    // More sub-steps = smaller effective time-step = more stable/accurate
    // (world-api: simulationSubSteps)
    worldDef.simulationSubSteps = 3;

    // Enable CCD to prevent fast objects tunnelling through static geometry
    // (world-api: continuousAllowed — NOT 'continuousCollision')
    worldDef.continuousAllowed = true;

    // Tighter contact spring (higher frequency = stiffer, less penetration)
    // (world-api: contactFrequency, contactDamping)
    worldDef.contactFrequency = 60f;  // default is 30; raise carefully
    worldDef.contactDamping = 1f;     // critical damping

    // Bounce threshold: only bounce if relative speed exceeds this value (m/s)
    // (world-api: bounceThreshold) — keep non-zero or bodies won't sleep
    worldDef.bounceThreshold = 1f;

    var world = PhysicsWorld.Create(worldDef);
}
```
<!-- Source: world-api (simulationSubSteps line 2053, continuousAllowed line 2037,
     contactFrequency line 2033, contactDamping line 2031, bounceThreshold line 2030,
     PhysicsWorld.Create(def) lines 307-314) -->

### Setup Custom Layers at Runtime
```csharp
// Physics layer names are defined in the PhysicsCoreSettings2D asset (Editor only).
// At runtime you query them via PhysicsLayers, not by writing to the settings.
void InitializePhysicsLayers()
{
    // Confirm physics layers are active on the default world
    // (world-api: PhysicsWorld.usePhysicsLayers line 194 — reflects
    //  PhysicsCoreSettings2D.usePhysicsLayers plus whether a settings asset is assigned)
    if (!PhysicsWorld.defaultWorld.usePhysicsLayers)
    {
        Debug.LogWarning("usePhysicsLayers is false. " +
            "Enable 'Use Physics Layers' in the PhysicsCoreSettings2D asset " +
            "and make sure the asset is assigned in Project Settings > PhysicsCore 2D.");
        return;
    }

    // Get combined masks for one or more layer names.
    // IMPORTANT: GetLayerMask takes string[] not a single string.
    // (layers-api: PhysicsLayers.GetLayerMask(string[]) line 31)
    PhysicsMask playerMask     = PhysicsLayers.GetLayerMask(new[] { "Player" });
    PhysicsMask enemyMask      = PhysicsLayers.GetLayerMask(new[] { "Enemy" });
    PhysicsMask solidMask      = PhysicsLayers.GetLayerMask(new[] { "Ground", "Wall" });
    PhysicsMask everythingMask = PhysicsMask.All;

    // Use the masks to configure shape contact filters (see unity-physicscore2d-filtering)
    // e.g.: shapeDef.contactFilter = new PhysicsShape.ContactFilter
    //       { categories = playerMask, contacts = solidMask };

    // Get the integer ordinal for a single layer name
    // (layers-api: PhysicsLayers.GetLayerOrdinal(string) line 51)
    int playerOrdinal = PhysicsLayers.GetLayerOrdinal("Player");
}
```
<!-- Source: layers-api (GetLayerMask(string[]) line 31, GetLayerOrdinal(string) line 51),
     world-api (PhysicsWorld.usePhysicsLayers line 194),
     filtering/examples/BasicContactFiltering.cs (PhysicsShape.ContactFilter pattern) -->

### Access Default Definitions
```csharp
// Two patterns for reading project defaults at runtime.

// --- Pattern A: via the settings asset properties ---
// (world-api: PhysicsCoreSettings2D.physicsBodyDefinition line 47,
//             PhysicsCoreSettings2D.physicsWorldDefinition line 57)
void CreateBodiesWithCustomDefaults_A()
{
    var settings = PhysicsCoreSettings2D.instance;

    // Snapshot the current project defaults (struct copies)
    PhysicsBodyDefinition  bodyDef  = settings.physicsBodyDefinition;
    PhysicsWorldDefinition worldDef = settings.physicsWorldDefinition;

    // Mutate the copy — the asset's stored defaults are NOT changed
    // (bodies-api: PhysicsBodyDefinition properties lines 856-876)
    bodyDef.type         = PhysicsBody.BodyType.Dynamic;
    bodyDef.gravityScale = 0.5f;      // half gravity
    bodyDef.linearDamping = 0.1f;

    var world = PhysicsWorld.Create(worldDef);
    var body  = world.CreateBody(bodyDef);
}

// --- Pattern B: via the definition's own constructor (no settings reference needed) ---
// (bodies-api: PhysicsBodyDefinition.new(bool useSettings) line 888)
// (world-api:  PhysicsWorldDefinition.new(bool useSettings) line 2068)
void CreateBodiesWithCustomDefaults_B()
{
    // useSettings:true → pulls values from the assigned PhysicsCoreSettings2D asset
    var worldDef = new PhysicsWorldDefinition(useSettings: true);
    var bodyDef  = new PhysicsBodyDefinition (useSettings: true);

    // Override only the fields you need
    worldDef.gravity = new Vector2(0f, -15f);
    bodyDef.sleepingAllowed = false;

    var world = PhysicsWorld.Create(worldDef);
    var body  = world.CreateBody(bodyDef); // real-code: filtering/examples/BasicContactFiltering.cs
}
```
<!-- Source: world-api (physicsBodyDefinition line 47, physicsWorldDefinition line 57,
     PhysicsWorldDefinition.new(bool) lines 2068-2073, PhysicsWorld.Create(def) lines 307-314),
     bodies-api (PhysicsBodyDefinition properties lines 856-876, new(bool) line 888),
     filtering/examples/BasicContactFiltering.cs (world.CreateBody(def) real usage) -->

### Enable Debug Drawing in Builds
```csharp
// 'renderingMode' is set in the PhysicsCoreSettings2D asset (Project Settings).
// There is no 'drawInBuild' property; the correct property is 'renderingMode'.
// (world-api: PhysicsCoreSettings2D.renderingMode line 58,
//             PhysicsWorld.RenderingMode enum lines 1747-1759)
//
// To enable drawing in development builds, set the asset's renderingMode to
// PhysicsWorld.RenderingMode.DevelopmentPlayer in the Inspector.
// To enable in ALL builds, use PhysicsWorld.RenderingMode.AnyPlayer.
// This cannot be changed from code at runtime — change it in the asset before building.

// At runtime, check whether rendering is currently allowed for a world:
// (world-api: PhysicsWorld.isRenderingAllowed line 165,
//             PhysicsWorld.renderingMode line 178)
void EnableDebugDrawing()
{
    var world = PhysicsWorld.defaultWorld;

    // Reflects the settings asset's renderingMode at startup
    PhysicsWorld.RenderingMode effectiveMode = world.renderingMode;
    bool canRender = world.isRenderingAllowed;

    if (!canRender)
    {
        Debug.LogWarning($"Debug rendering is not active (mode={effectiveMode}). " +
            "Set PhysicsCoreSettings2D.renderingMode to DevelopmentPlayer or AnyPlayer " +
            "in the Inspector before building.");
        return;
    }

    // Configure what to draw on the world (real-code: debug/SKILL.md line 35,
    //   performance/examples/JointGrid.cs lines 21-22)
    world.drawOptions   = PhysicsWorld.DrawOptions.All;
    world.drawFillAlpha = 0.25f;
    world.drawThickness = 1.5f;
}
```
<!-- Source: world-api (renderingMode PhysicsCoreSettings2D line 58, PhysicsWorld.renderingMode
     line 178, isRenderingAllowed line 165, RenderingMode enum lines 1747-1759,
     drawOptions line 154, drawFillAlpha line 149, drawThickness line 156),
     debug/SKILL.md line 35 and performance/examples/JointGrid.cs lines 21-22 (real-code) -->

### Override World Gravity
```csharp
// Two approaches: (a) set gravity at world-creation time via a definition,
// (b) change gravity on a live world instance.

// --- (a) Custom gravity at creation ---
// (world-api: PhysicsWorldDefinition.gravity line 2049,
//             PhysicsWorld.Create(PhysicsWorldDefinition) lines 307-314)
void CreateWorldWithCustomGravity_Definition()
{
    var worldDef = new PhysicsWorldDefinition(useSettings: true);

    // 'allowSleep' does not exist — the correct name is 'sleepingAllowed'
    // (world-api: PhysicsWorldDefinition.sleepingAllowed line 2055)
    worldDef.gravity         = new Vector2(0f, -20f); // double Earth gravity
    worldDef.sleepingAllowed = true;                  // keep sleep on for performance

    var world = PhysicsWorld.Create(worldDef);
}

// --- (b) Gravity override on an existing world ---
// (world-api: PhysicsWorld.gravity line 160 — get/set at any time)
void CreateWorldWithCustomGravity_Runtime()
{
    var world = PhysicsWorld.defaultWorld;

    // Read current gravity
    Vector2 current = world.gravity;

    // Set new gravity (immediate effect on next simulation step)
    world.gravity = new Vector2(0f, -4f);  // reduced gravity (moon-like)

    // Side-scrolling platformer: horizontal gravity component
    // world.gravity = new Vector2(-5f, -9.81f);
}
```
<!-- Source: world-api (PhysicsWorldDefinition.gravity line 2049, sleepingAllowed line 2055,
     PhysicsWorld.Create(def) lines 307-314, PhysicsWorld.gravity line 160) -->

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
// PhysicsCoreSettings2D is a ScriptableObject (the only class type in the API).
// It is assigned via Project Settings > PhysicsCore 2D in the Editor.
// 'instance' is the conventional access pattern used throughout the ecosystem;
// it is NOT listed in the generated world-api (lines 28-105) — treat as unverified
// and verify against the installed package if the build fails.

using Unity.U2D.Physics;
using UnityEngine;

public class PhysicsSettingsAccess : MonoBehaviour
{
    private void Awake()
    {
        // --- Singleton access (unverified, ecosystem-conventional) ---
        var settings = PhysicsCoreSettings2D.instance;

        if (settings == null)
        {
            Debug.LogError(
                "PhysicsCoreSettings2D not assigned. " +
                "Open Project Settings > PhysicsCore 2D and drag in the asset.");
            return;
        }

        // --- Verified properties (world-api: PhysicsCoreSettings2D, lines 38-60) ---

        // 64-bit physics layer mode
        bool using64Bit = settings.usePhysicsLayers;          // NOT useFullLayers

        // Concurrent world simulations allowed
        int maxParallel = settings.concurrentSimulations;

        // Whether automatic simulation is globally disabled
        bool simOff = settings.disableSimulation;

        // Build rendering mode
        PhysicsWorld.RenderingMode renderMode = settings.renderingMode;

        // Default definitions for bodies and worlds (struct snapshots — safe to mutate)
        PhysicsBodyDefinition  bodyDef  = settings.physicsBodyDefinition;  // NOT defaultBodyDefinition
        PhysicsWorldDefinition worldDef = settings.physicsWorldDefinition; // NOT defaultWorldDefinition

        // --- Alternative: construct definitions directly from settings ---
        // (bodies-api: new(bool), world-api: new(bool))
        var bodyFromSettings  = new PhysicsBodyDefinition (useSettings: true);
        var worldFromSettings = new PhysicsWorldDefinition(useSettings: true);

        // Create a world (real-code: filtering/examples/BasicContactFiltering.cs line 17)
        var world = PhysicsWorld.Create(worldFromSettings);
    }
}
```
<!-- Source: world-api (PhysicsCoreSettings2D properties lines 38-60: usePhysicsLayers,
     concurrentSimulations, disableSimulation, renderingMode, physicsBodyDefinition,
     physicsWorldDefinition; PhysicsWorld.Create(def) lines 307-314;
     PhysicsWorldDefinition.new(bool) lines 2068-2073),
     bodies-api (PhysicsBodyDefinition.new(bool) line 888),
     filtering/examples/BasicContactFiltering.cs line 17 (PhysicsWorld.Create() real usage) -->

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
