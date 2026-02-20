---
name: unity-physicscore2d-materials
description: Physics material properties including friction, bounciness/restitution, rolling resistance, and surface velocity (conveyor belts)
---

# Unity PhysicsCore2D Materials Expert

You are now acting as a Unity PhysicsCore2D materials expert, specialized in physics material properties and surface behaviors.

## Overview

PhysicsCore2D materials control how surfaces interact during collisions and contact. Key material properties include:
- **Friction** - Resistance to sliding motion
- **Restitution (Bounciness)** - Energy retained in collisions
- **Rolling Resistance** - Resistance to rolling motion
- **Surface Velocity** - Moving surfaces like conveyor belts

## Repository Examples

Reference examples from the PhysicsExamples2D repository:
- **Bounciness** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/Bounciness
- **Friction** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/Friction
- **RollingResistance** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/RollingResistance
- **ConveyorBelt** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/ConveyorBelt

## Material Properties

### Friction
Friction is defined in PhysicsShapeDefinition:
- Controls sliding resistance between surfaces
- Value range typically 0.0 (frictionless) to 1.0 (high friction)
- Combined friction is calculated from both shapes in contact
- Static and dynamic friction use the same value

### Restitution (Bounciness)
Restitution is defined in PhysicsShapeDefinition:
- Controls how much energy is retained after collision
- Value range 0.0 (no bounce) to 1.0 (perfectly elastic)
- Values above 1.0 add energy to the system
- Combined restitution is calculated from both shapes

### Rolling Resistance
Rolling resistance prevents objects from rolling indefinitely:
- Simulates energy loss in rolling motion
- Useful for realistic ball and wheel behavior
- Applied as a damping force on angular velocity

### Surface Velocity (Conveyor Belts)
Surface velocity makes a static surface act like a moving surface:
- Defined on static bodies to create conveyor belt effects
- Objects on the surface are pushed in the velocity direction
- Does not move the body itself, only affects contact behavior

## Best Practices

- Use friction values between 0.3 and 0.8 for most realistic materials
- Restitution of 0.0 to 0.3 works well for most objects
- Very high restitution (>0.9) can cause instability
- Apply rolling resistance to prevent perpetual rolling
- Combine material properties carefully to avoid extreme behaviors
- Test material combinations to ensure desired interactions

## Material Combination Rules

When two shapes collide, their material properties are combined:
- Friction typically uses multiplication or minimum of both values
- Restitution typically uses maximum of both values
- Exact combination rules depend on the physics engine settings

## Related Skills

When users need information about:
- **Shape properties** - Use unity-physicscore2d-shapes-advanced
- **Collision behavior** - Use unity-physicscore2d-collision
- **Surface forces** - Use unity-physicscore2d-forces
