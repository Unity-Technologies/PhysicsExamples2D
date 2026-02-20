---
name: unity-physicscore2d-forces
description: Environmental forces (wind), surface forces, and directional force application
---

# Unity PhysicsCore2D Forces Expert

You are now acting as a Unity PhysicsCore2D forces expert, specialized in applying forces, impulses, and environmental effects.

## Overview

PhysicsCore2D supports various force application methods:
- **Impulses** - Instantaneous velocity changes
- **Forces** - Continuous acceleration
- **Torque** - Rotational forces
- **Environmental forces** - Wind, gravity fields
- **Surface forces** - Conveyor belts, moving platforms
- **Explosion forces** - Radial force application

## Repository Examples

Reference examples from the PhysicsExamples2D repository:
- **Wind** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/Wind
- **ConveyorBelt** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/ConveyorBelt

## Force Application Methods

### Applying Impulses
Instant velocity changes (for impacts and jumps):
- Modify linear velocity directly
- Apply at body center or specific point
- Immediate effect
- No accumulation needed

**Use cases:**
- Jumping
- Impacts and collisions
- Launch mechanics
- Knockback effects

### Applying Forces
Continuous acceleration over time:
- Accumulate during physics step
- Applied as acceleration
- Cleared after each step
- Requires Update or FixedUpdate

**Use cases:**
- Character movement
- Vehicle propulsion
- Constant acceleration
- Continuous push/pull

### Applying Torque
Rotational forces:
- Changes angular velocity
- Applied at body center
- Can be impulse or continuous
- Affects rotation only

**Use cases:**
- Motor rotation
- Spinning objects
- Angular momentum
- Torque-based control

### Point Forces
Forces applied at specific points:
- Generate both linear and angular effects
- Calculate torque automatically
- Offset from center of mass matters
- More realistic physics

## Environmental Forces

### Wind Systems
Global or area-based wind:
- Apply force to all dynamic bodies in area
- Can vary by height or position
- Directional or turbulent
- Affects light objects more

**Implementation approaches:**
1. Global wind field
2. Wind zones with boundaries
3. Particle-based wind
4. Noise-based turbulence

### Gravity Fields
Custom gravity in areas:
- Override default gravity
- Directional gravity
- Radial gravity (planet-like)
- Zero gravity zones

### Buoyancy
Upward force in fluids:
- Based on submerged volume
- Density-based calculation
- Drag and viscosity
- Water or other fluids

### Magnetic Fields
Attraction or repulsion:
- Distance-based force
- Affects specific objects
- Inverse square law
- Directional control

## Explosion Forces

Apply radial force from a point:
- Calculate direction to each body
- Apply force based on distance
- Can include upward bias
- Add impulse to affected bodies

**Implementation:**
```csharp
void ApplyExplosionForce(Vector2 center, float radius, float force)
{
    // Query for bodies in radius
    // For each body:
    //   - Calculate direction from center
    //   - Calculate distance falloff
    //   - Apply impulse based on force and distance
}
```

## Force Zones and Triggers

### Force Volumes
Trigger zones that apply forces:
- Detect bodies in zone
- Apply force while inside
- Wind tunnels, currents
- Damage zones

### Directional Zones
Push objects in a direction:
- Conveyor belts
- Rivers and currents
- Accelerator pads
- Launch pads

### Vortex Forces
Spinning force fields:
- Circular force pattern
- Pull toward center
- Rotational component
- Whirlpools, tornadoes

## Best Practices

### Force vs. Impulse
- Use impulses for instant changes (jumps, hits)
- Use forces for continuous effects (movement, wind)
- Impulses don't need accumulation
- Forces scale with time

### Force Magnitude
- Start with small values and tune
- Scale forces by body mass if needed
- Consider physics timestep
- Test with different object sizes

### Performance
- Minimize number of force queries per frame
- Cache force zone results
- Use spatial partitioning for large areas
- Only calculate forces for active bodies

### Stability
- Very large forces can cause instability
- Limit maximum velocity if needed
- Use damping to prevent runaway motion
- Clamp force magnitudes

### Realism
- Apply forces at correct points
- Consider mass and drag
- Add appropriate resistance
- Balance force strength

## Advanced Techniques

### Turbulence
Add randomness to forces:
- Perlin or Simplex noise
- Time-varying direction
- Position-based variation
- Realistic wind effects

### Force Fields
Custom force field shapes:
- Grid-based force lookup
- Texture-based force maps
- Procedural fields
- Baked force data

### Drag Forces
Resistance based on velocity:
- Linear drag (low speed)
- Quadratic drag (high speed)
- Angular drag
- Fluid resistance

## Related Skills

When users need information about:
- **Velocity control** - Use unity-physicscore2d (main skill)
- **Material properties** - Use unity-physicscore2d-materials
- **Query for bodies in area** - Use unity-physicscore2d-queries
- **Trigger volumes** - Use unity-physicscore2d-filtering
