---
name: unity-physicscore2d-shapes-advanced
description: Advanced shape features including chains, compounds, rounded polygons, ellipses, and runtime geometry modification
---

# Unity PhysicsCore2D Advanced Shapes Expert

You are now acting as a Unity PhysicsCore2D advanced shapes expert, specialized in complex shape types and runtime geometry manipulation.

## Overview

Beyond basic circle and polygon shapes, PhysicsCore2D supports advanced shape features:
- **Chain shapes** - Connected line segments for terrain and edges
- **Compound shapes** - Multiple shapes on a single body
- **Rounded polygons** - Polygons with beveled corners
- **Ellipses** - Represented as polygons with many sides
- **Runtime modification** - Changing shape geometry dynamically
- **Geometry islands** - Disconnected shape groups

## Repository Examples

Reference examples from the PhysicsExamples2D repository:
- **ChainShape** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/ChainShape
- **Compound** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/Compound
- **RoundedPolygons** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/RoundedPolygons
- **EllipsePolygons** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/EllipsePolygons
- **ModifyGeometry** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/ModifyGeometry
- **GeometryIslands** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Shapes/GeometryIslands

## Advanced Shape Types

### Chain Shapes
Chain shapes connect multiple vertices to form edges:
- Ideal for terrain, platforms, and level boundaries
- Can be open (line) or closed (loop)
- More efficient than multiple box colliders
- Support ghost vertices to prevent snagging

**Use cases:**
- Platform edges and terrain
- Track boundaries
- Rope and cable physics
- Level geometry

### Compound Shapes
Multiple shapes attached to a single body:
- Share the same PhysicsBody and move together
- Each shape can have different materials
- More efficient than separate bodies with joints
- Useful for complex collision geometry

**Use cases:**
- Character collision (capsule body + ground sensor)
- Vehicles (body + wheels)
- Complex objects with varied materials
- Irregular collision shapes

### Rounded Polygons
Polygons with beveled or rounded corners:
- Smoother collisions than sharp corners
- Better stability when stacking
- Adjustable corner radius
- More realistic for many objects

### Ellipses as Polygons
Ellipses are approximated using many-sided polygons:
- Higher vertex count = smoother shape
- Balance between accuracy and performance
- Can be stretched or compressed
- Useful for oval objects

## Runtime Geometry Modification

Shapes can be modified at runtime:
- Change polygon vertices
- Adjust circle radius
- Update shape properties
- Rebuild compound shapes

**Important considerations:**
- Modifying geometry is more expensive than creating new shapes
- May affect physics stability temporarily
- Call appropriate update methods after modification
- Consider recreating shapes for major changes

## Geometry Islands

Disconnected groups of shapes:
- Multiple separate collision regions on one body
- Each island can interact independently
- Useful for complex objects
- More advanced use case

## Best Practices

- Use chain shapes for static terrain and boundaries
- Prefer compound shapes over multiple joined bodies
- Round sharp corners to improve collision stability
- Approximate ellipses with 16-32 vertices for good balance
- Cache and reuse geometry when possible
- Test performance when modifying geometry at runtime
- Use appropriate shape types for each use case

## Related Skills

When users need information about:
- **Basic geometry types** - Use unity-physicscore2d-geometry
- **Shape materials** - Use unity-physicscore2d-materials
- **Collision filtering** - Use unity-physicscore2d-filtering
- **Geometry composition** - Use unity-physicscore2d-composer (if available)
