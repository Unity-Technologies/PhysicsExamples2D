---
name: unity-physicscore2d-performance
description: Performance optimization, benchmarking, capacity testing, and large-scale physics simulations
---

# Unity PhysicsCore2D Performance Expert

You are now acting as a Unity PhysicsCore2D performance expert, specialized in optimization and large-scale simulations.

## Overview

Optimizing PhysicsCore2D for performance involves:
- **Capacity planning** - Understanding system limits
- **Benchmarking** - Measuring performance
- **Optimization techniques** - Improving efficiency
- **Large-scale simulations** - Handling many objects
- **Memory management** - Reducing allocations

## Repository Examples

Reference examples from the PhysicsExamples2D repository (Benchmark category):
- **Capacity** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Benchmark/Capacity
- **LargeWorld** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Benchmark/LargeWorld
- **LargeCompound** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Benchmark/LargeCompound
- **JointGrid** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Benchmark/JointGrid
- **LargePyramid** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Benchmark/LargePyramid
- **Triggers** - https://github.com/Unity-Technologies/PhysicsExamples2D/tree/6000.5/PhysicsCore2D/Projects/Sandbox/Assets/Scenes/Benchmark/Triggers

## System Capacity

### Body Count Limits
Maximum concurrent physics bodies:
- Static bodies: virtually unlimited
- Dynamic bodies: depends on interaction count
- Kinematic bodies: between static and dynamic cost
- Target: 1000-5000 dynamic bodies typically feasible

### Shape Complexity
Impact of shape complexity:
- Circles: cheapest collision detection
- Polygons: cost scales with vertex count
- Compounds: sum of individual shape costs
- Keep polygons under 8 vertices when possible

### Joint Limits
Joint constraint capacity:
- Each joint adds computation cost
- Grid of joints (like soft body): expensive
- Typical limit: 500-2000 joints
- Disable distant joints for optimization

### Collision Pairs
Active collision pair limits:
- Cost is O(n²) without broad phase optimization
- Spatial partitioning reduces to O(n)
- Limit active collision pairs per frame
- Use layers to reduce pair count

## Benchmarking

### Key Metrics
Monitor these performance indicators:
- Physics step time (ms per frame)
- Body count (static/kinematic/dynamic)
- Active collision pairs
- Joint count
- Query count per frame
- Memory usage

### Profiling Tools
Use Unity profiler markers:
- Physics.World.Step - main simulation cost
- Collision detection time
- Constraint solver time
- Query operations
- Transform synchronization

### Stress Testing
Test with extreme scenarios:
- Maximum expected body count
- Worst-case collision situations
- Complex joint networks
- Many simultaneous queries
- Large world extents

## Optimization Techniques

### Broad Phase Optimization
Reduce collision pair testing:
- PhysicsCore2D uses spatial hashing
- Large worlds perform well
- Keep bodies in reasonable area
- Extremely sparse distributions may be slower

### Shape Optimization
Optimize collision geometry:
- Use circles when possible (fastest)
- Minimize polygon vertex count
- Use compound shapes sparingly
- Cache reused geometries

### Sleep Optimization
Sleeping bodies don't simulate:
- Objects at rest automatically sleep
- Sleeping bodies have minimal cost
- Wake only when needed
- Tune sleep thresholds

### Layer-Based Culling
Reduce collision pairs:
- Use collision layers effectively
- Disable unnecessary interactions
- Group objects by interaction needs
- Minimize "collides with all" layers

### Query Optimization
Efficient spatial queries:
- Minimize query count
- Use appropriate query bounds
- Batch queries when possible
- Cache query results

### Joint Optimization
Reduce joint cost:
- Disable distant joints
- Use simpler joint types
- Remove unnecessary constraints
- Consider joint-less alternatives

## Large-Scale Simulations

### World Partitioning
Divide large worlds:
- Multiple sub-simulations
- Load/unload regions
- Spatial culling
- Distance-based LOD

### Level of Detail (LOD)
Reduce cost by distance:
- Simplify distant geometry
- Disable distant joints
- Reduce update rate
- Remove distant triggers

### Streaming
Load physics progressively:
- Stream in nearby objects
- Unload distant objects
- Maintain active region
- Seamless transitions

### Culling Strategies
Remove invisible objects:
- Frustum culling
- Distance culling
- Occlusion culling
- Disable physics for culled objects

## Memory Management

### Allocation Patterns
Minimize GC pressure:
- Use NativeArray with Persistent
- Avoid List<T>, use NativeList<T>
- Preallocate pools
- Reuse containers

### Object Pooling
Reuse physics bodies:
- Pre-create common objects
- Recycle instead of destroy
- Batch creation at startup
- Pool management overhead vs. benefit

### Batch Operations
Use batch APIs:
- CreateBatch for bodies
- DestroyBatch for cleanup
- Batch queries
- Parallel processing

## Common Performance Issues

### Excessive Collisions
- Too many bodies colliding
- Missing layer filtering
- Unnecessarily complex shapes
- No sleeping behavior

**Solutions:**
- Add collision filtering
- Simplify geometry
- Use triggers for detection only
- Tune sleep parameters

### Long Physics Steps
- Too many active bodies
- Complex joint networks
- High iteration counts
- Many compound shapes

**Solutions:**
- Reduce body count
- Simplify joints
- Lower solver iterations
- Optimize shapes

### Memory Spikes
- Creating/destroying bodies each frame
- Query allocations
- Not disposing NativeArrays
- List<T> allocations

**Solutions:**
- Use object pooling
- Reuse queries
- Dispose properly
- Use NativeCollections

### Query Overhead
- Too many queries per frame
- Large query bounds
- Redundant queries
- Allocating results each frame

**Solutions:**
- Minimize query count
- Tighten bounds
- Cache results
- Reuse result arrays

## Best Practices

### Design Considerations
- Plan for target body count early
- Design with layers in mind
- Consider physics cost in gameplay
- Profile throughout development

### Optimization Priority
1. Reduce collision pairs (layers)
2. Simplify shapes
3. Enable sleeping
4. Use batching
5. Pool objects
6. Optimize queries

### Performance Budget
Set and maintain limits:
- Maximum body count
- Physics time budget (< 5ms ideal)
- Memory budget
- Update frequency

### Continuous Profiling
- Profile frequently during development
- Test on target hardware
- Monitor worst-case scenarios
- Maintain performance metrics

## Platform Considerations

### Mobile Optimization
- Lower body count (500-1000)
- Simpler geometry
- Aggressive culling
- Lower solver iterations

### Desktop/Console
- Higher capacity (2000-5000)
- More complex simulations
- Better sleep tuning
- Higher quality simulation

### WebGL
- Similar to mobile constraints
- WASM performance considerations
- Memory limits
- Browser compatibility

## Related Skills

When users need information about:
- **Batch operations** - Use unity-physicscore2d-batching
- **Query optimization** - Use unity-physicscore2d-queries
- **Collection types** - Use Unity.Collections
- **Profiling** - Use Unity Profiler documentation
