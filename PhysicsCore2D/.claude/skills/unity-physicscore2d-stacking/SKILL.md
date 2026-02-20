---
name: unity-physicscore2d-stacking
description: Stacking stability, structural physics (arches, card houses), and domino chain reactions
---

# Unity PhysicsCore2D Stacking Expert

You are now acting as a Unity PhysicsCore2D stacking expert, specialized in stable stacking, structures, and cascading physics.

## Overview

Stacking physics requires careful tuning for stability:
- **Stack stability** - Preventing collapse
- **Structural formations** - Arches, towers, bridges
- **Domino effects** - Chain reactions
- **Confined stacking** - Containers and piles
- **Balanced structures** - Precarious arrangements

## Repository Examples

Reference examples from the PhysicsExamples2D repository:
- **Arch** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Stacking/Arch
- **CardHouse** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Stacking/CardHouse
- **Confined** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Stacking/Confined
- **DoubleDomino** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Stacking/DoubleDomino
- **ShapeStack** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Stacking/ShapeStack

## Stacking Stability

### Factors Affecting Stability
- **Contact area** - Larger contact = more stable
- **Center of mass** - Lower is more stable
- **Mass distribution** - Bottom-heavy preferred
- **Friction** - Higher friction increases stability
- **Shape geometry** - Flat surfaces stack better
- **Solver iterations** - More iterations = more stable

### Stability Techniques

#### Shape Optimization
- Use flat contact surfaces
- Avoid sharp corners
- Round edges slightly (beveled polygons)
- Larger contact patches
- Appropriate shape complexity

#### Material Properties
- Higher friction on stacking surfaces
- Low restitution (no bouncing)
- Appropriate mass distribution
- Consistent material between layers

#### Physics Settings
- Increase solver iterations (8-16)
- Lower timestep if needed
- Enable position correction
- Tune contact tolerance
- Adjust sleep thresholds

## Structural Formations

### Arches
Self-supporting curved structures:
- Keystone at top provides stability
- Lateral forces balance
- Requires precise placement
- Foundation must be stable
- Use appropriate mass ratios

**Creation tips:**
1. Start with foundation blocks
2. Place side supports
3. Add curved arch segments
4. Place keystone last
5. Allow settling time

### Towers
Vertical stacks:
- Align centers of mass vertically
- Use uniform or tapering masses
- Larger base for stability
- Minimize wobble during creation
- Consider crossbracing

### Bridges
Span structures:
- Support at both ends
- Distribute load evenly
- Use arch or beam design
- Tension and compression balance
- Test with dynamic loads

### Card Houses
Precarious lean-to structures:
- Friction critical for stability
- Angle of lean important
- Balance forces carefully
- Very sensitive to disturbance
- Requires high solver quality

## Domino Chains

### Domino Setup
Chain reaction sequences:
- Proper spacing (typically domino height / 2)
- Consistent domino properties
- Stable placement on surface
- Trigger mechanism
- Environmental considerations

### Chain Reaction Design
- Test reliability of toppling
- Account for bounce and scatter
- Use guides or channels
- Multiple parallel chains
- Branching and merging paths

### Advanced Domino Effects
- **Spirals** - Curved domino paths
- **Stairs** - Multi-level chains
- **Splits** - One domino triggers multiple
- **Delays** - Slow sections
- **Jumps** - Gaps in chain

## Confined Stacking

### Container Physics
Stacking within boundaries:
- Wall friction affects settling
- Pressure distribution
- Particle packing behavior
- Flow and avalanche effects
- Stable vs. chaotic packing

### Piling Dynamics
Objects falling into container:
- Drop height affects energy
- Order affects final configuration
- Shape affects packing density
- Mass affects pressure
- Friction affects angle of repose

## Best Practices

### Creation Process
1. **Bottom-up** - Always build from base
2. **Settling time** - Let physics stabilize
3. **Incremental** - Add pieces gradually
4. **Positioning** - Precise initial placement
5. **Testing** - Verify stability before continuing

### Stability Tuning
- Start with high friction (0.5-0.8)
- Use low restitution (0.0-0.1)
- Increase solver iterations for tall stacks
- Enable sleeping for stable stacks
- Test with disturbances

### Common Pitfalls
- Creating entire stack at once
- Misaligned centers of mass
- Insufficient friction
- Too few solver iterations
- Sharp corners causing instability
- Excessive initial velocities

### Performance Considerations
- Large stacks are expensive
- Use sleeping for stable regions
- Consider static bodies for permanent structures
- Limit active collision pairs
- Profile with target stack size

## Gameplay Applications

### Building Games
Player-constructed structures:
- Provide stability feedback
- Show center of mass
- Highlight unstable connections
- Undo/redo for iteration
- Save/load structures

### Destruction Games
Knocking down structures:
- Score based on destruction
- Chain reactions increase score
- Precision vs. power tradeoffs
- Multiple attempt strategies
- Progressive difficulty

### Puzzle Games
Stability as puzzle mechanic:
- Required formations
- Limited pieces
- Precise placement challenges
- Physics-based solutions
- Time or move limits

## Advanced Techniques

### Active Stabilization
Helping unstable stacks:
- Temporary constraints
- Gradual activation
- Damping during creation
- Collision filtering during build
- Motor drives for correction

### Procedural Stacking
Automated structure creation:
- Algorithmic placement
- Stability testing
- Iterative adjustment
- Random variations
- Validated generation

### Destruction Analysis
Studying collapse:
- Identify failure points
- Force distribution
- Progressive collapse
- Energy dissipation
- Debris patterns

## Testing and Validation

### Stability Tests
- Apply small impulses
- Test with time acceleration
- Environmental disturbances
- Collision from projectiles
- Sustained loads

### Stress Testing
- Maximum stack height
- Overloading structures
- Worst-case configurations
- Edge case geometries
- Performance under load

## Related Skills

When users need information about:
- **Material properties** - Use unity-physicscore2d-materials
- **Collision behavior** - Use unity-physicscore2d-collision
- **Shape optimization** - Use unity-physicscore2d-shapes-advanced
- **Physics settings** - Use unity-physicscore2d-settings (if available)
- **Performance** - Use unity-physicscore2d-performance
