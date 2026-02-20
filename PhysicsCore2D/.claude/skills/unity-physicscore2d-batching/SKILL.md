---
name: unity-physicscore2d-batching
description: Batching techniques for queries, projectiles/shooters, and swarm/flocking behaviors (boids)
---

# Unity PhysicsCore2D Batching Expert

You are now acting as a Unity PhysicsCore2D batching expert, specialized in optimizing performance through batching techniques.

## Overview

Batching operations in PhysicsCore2D significantly improves performance:
- **Body creation batching** - Create many bodies at once
- **Query batching** - Perform multiple queries efficiently
- **Projectile systems** - Efficient bullet management
- **Swarm behaviors** - Flocking and boids simulation
- **Batch updates** - Update many bodies together

## Repository Examples

Reference examples from the PhysicsExamples2D repository:
- **Boids** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Batching/Boids
- **Queries** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Batching/Queries
- **Shooter** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Batching/Shooter

## Batch Creation

### Creating Bodies in Batches
Use `PhysicsBody.CreateBatch` instead of loops:
- Single allocation for all bodies
- Faster than individual CreateBody calls
- Returns NativeArray of bodies
- Must dispose array properly

**Example:**
```csharp
// Create 1000 bodies at once
var bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
var bodies = PhysicsBody.CreateBatch(world, bodyDefinition, 1000, Allocator.Persistent);

// Dispose when done
PhysicsBody.DestroyBatch(bodies);
bodies.Dispose();
```

### Batch Shape Creation
Create shapes for multiple bodies efficiently:
- Reuse shape definitions
- Batch similar geometries
- Minimize individual CreateShape calls

## Query Batching

### Multiple Overlap Queries
Perform many overlap tests efficiently:
- Batch query allocations
- Process results together
- Reduce GC allocations
- Use job system for parallel processing

### Raycast Batching
Cast multiple rays in one operation:
- Single native array for results
- Parallel processing
- Useful for projectile hit detection
- Line-of-sight checks

### Spatial Queries
Query multiple areas simultaneously:
- Find all bodies in multiple regions
- Batch circle/box overlap tests
- Cache frequently used queries

## Projectile Systems

### Bullet Pool Management
Efficient projectile handling:
- Pre-create bullet bodies
- Reuse inactive projectiles
- Enable/disable instead of create/destroy
- Batch position updates

**Shooter system pattern:**
1. Create bullet pool at startup
2. Disable unused bullets
3. Enable and position when firing
4. Disable bullets on impact or timeout
5. Reuse from pool

### Trajectory Updates
Update many projectiles together:
- Batch velocity calculations
- Parallel processing
- Collision detection batching
- Culling inactive projectiles

## Swarm and Flocking (Boids)

### Boid Simulation
Simulate many agents efficiently:
- Batch neighbor queries
- Calculate flocking rules in parallel
- Update all boid positions together
- Use spatial partitioning

**Flocking behaviors:**
- **Separation** - Avoid crowding neighbors
- **Alignment** - Steer toward average heading
- **Cohesion** - Move toward average position

### Spatial Partitioning
Optimize neighbor searches:
- Grid-based partitioning
- Hash spatial grid
- Reduce query complexity
- Update partitioning incrementally

### Boid Optimization
- Limit neighbor search radius
- Update boids in groups
- Use LOD for distant boids
- Cull off-screen boids

## Batch Updates

### Transform Synchronization
Update many transforms at once:
- Batch GetPosition/GetRotation calls
- Parallel transform updates
- Reduce overhead
- Job system integration

### Property Updates
Modify multiple bodies together:
- Batch velocity changes
- Mass updates
- Layer changes
- Flag modifications

## Performance Patterns

### Object Pooling
Reuse physics bodies:
- Pre-create object pools
- Disable instead of destroy
- Reset properties when reusing
- Batch activation/deactivation

### Job System Integration
Parallelize operations:
- Use Unity Jobs for batch processing
- Process bodies in parallel
- Calculate forces in jobs
- Update results together

### Memory Management
Minimize allocations:
- Use NativeArray with Persistent allocator
- Reuse arrays across frames
- Batch disposal operations
- Pool query result arrays

## Best Practices

### Body Batching
- Always use batch creation for > 10 bodies
- Specify correct allocator lifetime
- Dispose batched arrays properly
- Reuse definitions in batches

### Query Optimization
- Batch similar query types together
- Cache query results when possible
- Use appropriate query bounds
- Avoid redundant queries

### Projectile Systems
- Pool at least 2x expected concurrent bullets
- Use kinematic bodies for simple projectiles
- Batch collision checks
- Implement timeout cleanup

### Swarm Systems
- Limit flock size to 100-500 boids
- Use grid partitioning for > 100 agents
- Update in groups per frame
- Adjust behavior complexity by count

### Profiling
- Profile before optimizing
- Measure batch vs. individual performance
- Monitor allocation patterns
- Check parallel job efficiency

## Common Pitfalls

- Creating bodies individually in loops
- Allocating new arrays every frame
- Not disposing NativeArrays
- Over-batching (diminishing returns)
- Incorrect allocator usage
- Forgetting to destroy batched bodies

## Advanced Techniques

### Hierarchical Batching
- Batch by object type
- Group similar operations
- Cascade batch updates
- Multi-level optimization

### Adaptive Batching
- Scale batch size by load
- Dynamic pooling
- LOD-based batching
- Performance-driven decisions

## Related Skills

When users need information about:
- **Batch body creation** - Covered in main unity-physicscore2d skill
- **Queries** - Use unity-physicscore2d-queries
- **Performance optimization** - Use unity-physicscore2d-performance
- **Collections** - Use Unity.Collections NativeArray
