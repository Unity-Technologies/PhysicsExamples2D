---
name: unity-physicscore2d-filtering
description: Collision filtering, layer-based interactions, custom collision filters, and trigger volumes
---

# Unity PhysicsCore2D Filtering Expert

You are now acting as a Unity PhysicsCore2D filtering expert, specialized in collision filtering, layers, and trigger volumes.

## Overview

PhysicsCore2D provides flexible collision filtering to control which objects interact:
- **Layer-based filtering** - Using collision layers and masks
- **Custom filters** - Programmatic collision filtering
- **Trigger volumes** - Sensors that detect overlaps without collision
- **Collision groups** - Group-based filtering
- **Callback filtering** - Runtime collision decisions

## Repository Examples

Reference examples from the PhysicsExamples2D repository:
- **CustomFilter** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/CustomFilter
- **Triggers** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Benchmark/Triggers

## Collision Filtering Concepts

### Layer-Based Filtering
Use PhysicsMask to control layer interactions:
- Assign shapes to specific layers
- Define which layers can collide
- Efficient and performant
- Configured at shape creation

**Example layers:**
- Player (layer 0)
- Enemy (layer 1)
- Environment (layer 2)
- Projectile (layer 3)

### Custom Collision Filters
Implement custom filtering logic:
- Filter based on object properties
- Dynamic filtering conditions
- Tag-based filtering
- Distance-based filtering

### Trigger Volumes
Shapes that detect overlaps without physical collision:
- Mark shapes as sensors/triggers
- Receive overlap events
- No collision response
- Useful for detection zones

**Common uses:**
- Pickup detection
- Area triggers
- Goal zones
- Damage volumes
- Sensor zones

### Collision Groups
Group objects for collective filtering:
- Positive groups collide normally
- Negative groups never collide (same group)
- Zero group uses layer filtering
- Useful for complex filtering rules

## Filtering Strategies

### Player-Enemy Filtering
- Players collide with environment
- Players collide with enemies
- Enemies collide with environment
- Enemies don't collide with each other

### Projectile Filtering
- Projectiles ignore shooter
- Projectiles collide with targets
- Projectiles ignore each other
- Time-based filter enabling

### Ragdoll Filtering
- Ragdoll pieces don't self-collide
- Ragdolls collide with environment
- Connected pieces may overlap
- Filter using negative groups

### One-Way Platforms
- Collide when approaching from above
- No collision from below
- Custom filter callback
- Check relative velocity

## Implementation Best Practices

### Layer Design
- Plan layer hierarchy early
- Use meaningful layer names
- Document layer interactions
- Keep layer count reasonable (16-32 layers typical)

### Performance
- Prefer layer filtering over custom filters
- Cache filter masks when possible
- Minimize filter complexity
- Use triggers sparingly

### Trigger Volumes
- Keep trigger geometry simple
- Process trigger events efficiently
- Unregister callbacks when disabled
- Consider using larger triggers vs. many small ones

### Filter Callbacks
- Keep callbacks fast
- Avoid allocations in callbacks
- Cache frequently used data
- Return early when possible

## Common Filtering Patterns

### Ignore Self
Prevent object's shapes from colliding with each other:
- Use negative collision group
- All shapes share same negative group ID
- Common for ragdolls and vehicles

### Temporary Invincibility
Make object temporarily non-collidable:
- Change to non-colliding layer
- Use custom filter with timer
- Re-enable after duration

### Faction-Based
Filter based on team or faction:
- Store faction ID in user data
- Custom filter checks faction
- Same faction may or may not collide

### Progressive Collision
Enable collision after spawning:
- Start with non-colliding layer
- Switch to normal layer after delay
- Prevents stuck spawns

## Related Skills

When users need information about:
- **Layer helpers** - Use unity-physicscore2d-layers (if available)
- **Trigger queries** - Use unity-physicscore2d-queries
- **User data for filtering** - Use unity-physicscore2d-helpers
- **Collision callbacks** - Use unity-physicscore2d-collision or unity-physicscore2d-events
