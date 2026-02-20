---
name: unity-physicscore2d-factories
description: Factory patterns for creating complex physics objects (ragdolls, soft bodies, vehicles, gears, mechanical systems)
---

# Unity PhysicsCore2D Factories Expert

You are now acting as a Unity PhysicsCore2D factories expert, specialized in creating complex physics objects using factory patterns.

## Overview

Factory patterns help create complex multi-body physics systems:
- **Ragdolls** - Articulated character physics
- **Soft bodies** - Deformable objects
- **Vehicles** - Cars, bikes, and wheeled systems
- **Gears** - Mechanical components
- **Mechanical systems** - Complex interconnected objects
- **Doohickeys** - Miscellaneous composite objects

## Repository Examples

Reference factory implementations from the PhysicsExamples2D repository:
- **RagdollFactory.cs** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scripts/RagdollFactory.cs
- **SoftbodyFactory.cs** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scripts/SoftbodyFactory.cs
- **CarFactory.cs** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scripts/CarFactory.cs
- **GearFactory.cs** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scripts/GearFactory.cs
- **DoohickeyFactory.cs** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scripts/DoohickeyFactory.cs
- **GearComponent.cs** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scripts/GearComponent.cs

## Factory Pattern Benefits

### Code Organization
- Encapsulate complex creation logic
- Reusable object templates
- Centralized configuration
- Easier maintenance

### Consistency
- Standardized object creation
- Shared definitions and properties
- Predictable behavior
- Version control

### Performance
- Batch creation opportunities
- Shared geometry reuse
- Optimized initialization
- Memory pooling

## Ragdoll Factory

### Ragdoll Structure
Multi-body articulated character:
- Head body
- Torso bodies (upper/lower)
- Arm bodies (upper/lower/hand)
- Leg bodies (upper/lower/foot)
- Joints connecting body parts

### Ragdoll Creation Process
1. Define body part dimensions
2. Create PhysicsBody for each part
3. Add shapes to each body
4. Create joints between connected parts
5. Configure joint limits
6. Set collision filtering

### Ragdoll Considerations
- Mass distribution affects stability
- Joint limits for realistic motion
- Collision filtering to prevent self-collision
- Motor drives for active ragdolls
- Balance between stability and flexibility

## Soft Body Factory

### Soft Body Composition
Deformable objects using particles and joints:
- Grid or blob of connected bodies
- Distance joints maintain structure
- Pressure simulation (optional)
- Surface shapes for collision

### Soft Body Types
- **Pressure-based** - Inflated objects
- **Grid-based** - Cloth and deformables
- **Blob-based** - Jelly-like objects
- **Spring mesh** - Flexible structures

### Creation Steps
1. Generate particle positions
2. Create bodies for particles
3. Connect neighbors with joints
4. Add surface collision shapes
5. Configure joint stiffness
6. Set particle masses

### Tuning Parameters
- Stiffness: joint frequency
- Damping: joint damping ratio
- Particle mass and density
- Pressure (for inflated bodies)
- Surface shape properties

## Vehicle Factory

### Vehicle Components
Car or wheeled vehicle:
- Chassis body (main body)
- Wheel bodies (4+ wheels)
- Wheel joints (suspension)
- Motor drives for propulsion
- Steering mechanism

### Vehicle Creation
1. Create chassis body with shape
2. Create wheel bodies
3. Attach wheels with wheel joints
4. Configure suspension (joint springs)
5. Add motor drives to powered wheels
6. Setup steering control

### Vehicle Tuning
- Suspension stiffness and damping
- Wheel friction and mass
- Motor torque and speed
- Chassis center of mass
- Anti-roll bars (optional)

## Gear Factory

### Gear Systems
Rotating mechanical parts:
- Gear bodies with teeth
- Hinge joints at centers
- Tooth collision or gear ratios
- Rotation constraints
- Motor drives

### Gear Connections
- **Meshed gears** - Touching, opposite rotation
- **Gear ratios** - Size determines speed ratio
- **Gear trains** - Multiple connected gears
- **Planetary gears** - Advanced configurations

### Implementation Options
1. **Physics-based** - Actual tooth collision
2. **Constraint-based** - Joint with gear ratio
3. **Hybrid** - Constraints with visual teeth

## Doohickey Factory

### Miscellaneous Mechanisms
Complex composite objects:
- Multiple interacting parts
- Various joint types
- Mixed static and dynamic bodies
- Triggers and sensors
- Custom behaviors

### Examples
- Rube Goldberg machines
- Catapults and trebuchets
- Contraptions and gadgets
- Mechanical puzzles
- Interactive props

## Factory Implementation Pattern

### Basic Factory Structure
```csharp
public class MyObjectFactory
{
    // Configuration
    public float size = 1.0f;
    public PhysicsMask collisionMask;

    // Create method
    public void Create(PhysicsWorld world, Vector2 position)
    {
        // 1. Create bodies
        var bodies = CreateBodies(world, position);

        // 2. Add shapes
        AddShapes(bodies);

        // 3. Create joints
        CreateJoints(bodies);

        // 4. Configure properties
        ConfigureProperties(bodies);
    }

    // Helper methods
    private NativeArray<PhysicsBody> CreateBodies(PhysicsWorld world, Vector2 position)
    {
        // Body creation logic
    }

    private void AddShapes(NativeArray<PhysicsBody> bodies)
    {
        // Shape creation logic
    }

    private void CreateJoints(NativeArray<PhysicsBody> bodies)
    {
        // Joint creation logic
    }

    private void ConfigureProperties(NativeArray<PhysicsBody> bodies)
    {
        // Property configuration logic
    }
}
```

## Best Practices

### Design
- Start simple, iterate complexity
- Test individual components first
- Document factory parameters
- Provide sensible defaults
- Allow configuration customization

### Performance
- Use batch creation when possible
- Reuse definitions
- Pool complex objects
- Minimize allocations
- Cache shared geometries

### Maintenance
- Keep factories modular
- Separate creation from configuration
- Use clear naming
- Comment complex logic
- Version control parameters

### Debugging
- Add visual debugging
- Log creation steps
- Validate configurations
- Test edge cases
- Profile performance

## Common Patterns

### Procedural Generation
Generate variations:
- Randomize parameters
- Use seed for reproducibility
- Create unique instances
- Maintain factory template

### Object Pooling
Reuse complex objects:
- Pre-create pool
- Reset on return
- Validate before reuse
- Manage pool size

### Serialization
Save/load configurations:
- Export factory parameters
- Store object templates
- Load from files
- Runtime creation

### Prefab-Like System
Template-based creation:
- Define templates
- Instantiate from template
- Override parameters
- Maintain template library

## Advanced Techniques

### Procedural Ragdolls
Generate ragdolls from skeletons:
- Parse skeleton structure
- Generate bodies from bones
- Auto-create joints
- Configure limits automatically

### Soft Body Optimization
Reduce particle count:
- Variable resolution
- LOD for distance
- Simplify internal structure
- Optimize surface shapes

### Vehicle Dynamics
Advanced vehicle behavior:
- Active suspension
- Traction control
- Differential simulation
- Aerodynamics

## Related Skills

When users need information about:
- **Joints for factories** - Use unity-physicscore2d-joints
- **Batch creation** - Use unity-physicscore2d-batching
- **Collision filtering** - Use unity-physicscore2d-filtering
- **Performance** - Use unity-physicscore2d-performance
