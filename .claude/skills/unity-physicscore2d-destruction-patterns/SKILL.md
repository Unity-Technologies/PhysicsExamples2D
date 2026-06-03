---
name: unity-physicscore2d-destruction-patterns
description: High-level patterns and gameplay strategies for destructible objects in Unity PhysicsCore2D — slicing mechanics, fragmenting on impact, sprite-based destructible systems, debris cleanup, performance budgeting. Use for "how should I design destructible X?" questions. For the PhysicsDestructor type API and worked code examples see unity-physicscore2d-destructor; for raw member signatures see unity-physicscore2d-destructor-api.
---

# Unity PhysicsCore2D Destruction Expert

You are now acting as a Unity PhysicsCore2D destruction expert, specialized in implementing destructible objects and fragmentation systems.

## Overview

PhysicsCore2D supports various destruction techniques:
- **Slicing** - Cutting objects along a line
- **Fragmenting** - Breaking objects into pieces
- **Sprite destruction** - Pixel-based destructible sprites
- **Pre-fractured objects** - Pre-calculated break patterns
- **Dynamic mesh splitting** - Runtime geometry division

## Repository Examples

Reference examples from the PhysicsExamples2D repository:
- **Slicing** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Slicing
- **Fragmenting** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Fragmenting
- **SpriteDestruction** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/SpriteDestruction

## Destruction Techniques

### Slicing
Cutting objects along a line or plane:
- Calculate intersection points with shape edges
- Split geometry into two pieces
- Create new bodies for each piece
- Apply impulses to separated pieces
- Transfer material properties to new shapes

**Implementation steps:**
1. Detect slice line collision with object
2. Calculate intersection points
3. Generate new polygon geometries
4. Create PhysicsBody for each piece
5. Destroy original body
6. Apply separation forces

### Fragmenting
Breaking objects into multiple pieces:
- Voronoi or pattern-based fragmentation
- Pre-calculated or runtime generation
- Each fragment becomes a new body
- Impulses based on impact point
- Visual debris and particle effects

**Fragmentation patterns:**
- Radial (explosion-like)
- Grid-based
- Voronoi cells
- Crack propagation
- Custom patterns

### Sprite Destruction
Pixel-perfect destructible sprites:
- Remove pixels based on collision
- Regenerate collision geometry
- Update sprite texture
- Create debris particles
- Optimize for performance

### Pre-fractured Objects
Objects designed to break along seams:
- Pieces held together with weak joints
- Break joints when force threshold exceeded
- More predictable results
- Better performance than runtime slicing
- Artistic control over break patterns

## Implementation Considerations

### Performance
- Limit number of fragments created
- Pool physics bodies for reuse
- Use simpler geometry for fragments
- Cull distant destruction events
- Batch similar destruction operations

### Visual Quality
- Match fragment geometry to visual mesh
- Add particle effects for impact
- Smooth transitions between states
- Scale fragments appropriately
- Consider material properties

### Gameplay Impact
- Determine what can be destroyed
- Balance destruction for gameplay
- Handle destroyed object cleanup
- Trigger events on destruction
- Manage physics body count

## Best Practices

- Pre-calculate fragmentation patterns when possible
- Use object pooling for fragments
- Set lifetime limits on debris
- Apply appropriate forces to fragments
- Clean up destroyed pieces after time
- Consider using LOD for distant destruction
- Test performance with maximum expected fragments
- Balance visual quality vs. performance

## Advanced Techniques

### Crack Propagation
- Start from impact point
- Spread based on material strength
- Create realistic break patterns
- Progressive destruction

### Stress-Based Breaking
- Monitor forces on objects
- Break when stress exceeds threshold
- Realistic structural failure
- Joints break under load

### Material-Based Destruction
- Different patterns for different materials
- Glass shatters, wood splinters
- Metal deforms then breaks
- Contextual destruction behavior

## Related Skills

When users need information about:
- **Geometry manipulation** - Use unity-physicscore2d-shapes-advanced or unity-physicscore2d-destructor
- **Forces and impulses** - Use unity-physicscore2d-forces
- **Joint breaking** - Use unity-physicscore2d-joints
- **Collision detection** - Use unity-physicscore2d-collision

## Worked Examples

> All examples below assume the standard PhysicsCore2D `OnEnable`/`OnDisable` lifecycle. See the umbrella skill `unity-physicscore2d`, section "Creating and Destroy Physics Objects", for the canonical lifecycle pattern.

- [examples/SlicingPattern.cs](examples/SlicingPattern.cs) — orbiting player fires a slice ray (with optional reflections) into a circular arena; demonstrates `PhysicsDestructor.Slice` plus body rebuild with separation impulse.
- [examples/FragmentingPattern.cs](examples/FragmentingPattern.cs) — top-down shooter that fragments a destructible block on projectile impact via `PhysicsDestructor.Fragment` with a circular mask + random seed points; rebuilds an unbroken-remainder body and a debris-body batch, with optional explosion impulse.
- [examples/SpriteDestructionPattern.cs](examples/SpriteDestructionPattern.cs) — destruct-at-position pattern that splits the unbroken remainder into geometry islands, choosing static vs dynamic per island based on intersection with a virtual ground line.
