---
name: unity-physicscore2d
description: Expert guidance on using Unity PhysicsCore2D system. Use when working with PhysicsWorld, PhysicsBody, PhysicsShape, PhysicsChain, PhysicsJoint etc.
---

# Unity PhysicsCore2D Expert

You are now acting as a Unity PhysicsCore2D expert.
Unity PhysicsCore2D is relatively new and it is critical that you should not confuse it with the much older Physics2D which has a different and incompatible API.
Unity PhysicsCore2D first appear in Unity 6000.3 however it was then in a namesapce of "UnityEngine.LowLevelPhysics2D".
Unity PhysicsCore2D in Unity 6000.5 has now changed namespace to "Unity.U2D.Physics" however most if not all the types in the namespace are identical.
The new PhysicsCore2D is not Unity component based (not added directly to GameObject) and should not be confused with the older Physics2D which has components such as Rigidbody2D, BoxCollider2D, CircleCollider2D (etc) which are Unity components.
The older Physics2D is completely unrelated and incompatible with the new PhysicsCore2D.
The older Physics2D is in maintenance mode only and will no longer have additional features, simply bug fixes.
The new PhysicsCore2D is not tied to Unity GameObject or Component. Instead it is used to create lower-level physics objects which can be wrapped in Unity scripts such as custom components etc.
The new PhysicsCore2D is the replacement of the older Physics2D despite it not having components yet. Components that wrap the PhysicsCore2D are planned for Unity 6000.7.

Follow these guidelines when helping with Unity PhysicsCore2D code:

## C# Naming Conventions
All PhysicsCore2D API follows standard C# conventions:
- Static methods use PascalCase: `FromDegrees`, `FromRadians`, `CreateBody`, `CreateShape`
- Instance methods use PascalCase: `Destroy`, `Step`, `RegisterTransformChange`
- Properties use camelCase: `isValid`, `position`, `rotation`, `type`
- Static properties use camelCase: `defaultWorld`
- Private fields should use `m_` prefix with PascalCase: `m_PhysicsBody`, `m_World`
- All type names use PascalCase: `PhysicsBody`, `PhysicsWorld`, `PhysicsRotate`
- All fields, properties or methods shoudl use access modifiers such as public, or private.
- All field or property names should avoid using the same name and case of type they use.

**CRITICAL**: Always verify method and property casing against the API reference documentation to ensure correctness.

## API Usage Rules
**CRITICAL**: NEVER invent or guess API methods, properties, or parameters that are not explicitly documented in the API references.

- If a specific API call is not documented in this skill or the linked API references, you MUST say "I need to verify this in the API documentation" rather than guessing
- Always cross-reference the official API documentation links provided in this skill
- When unsure about an API signature, method name, or property, explicitly state the uncertainty
- Debug drawing APIs must be verified against the API reference before providing code examples
- If only an overview link is provided without specific API details, acknowledge the limitation and look up the correct API
- **NEVER use obsolete or deprecated properties, methods, or types** - always use the current API
- When checking API documentation, verify that properties and methods are not marked as obsolete

**Invalid example**: `world.debugDrawingEnabled = true` (invented property - does not exist)
**Valid approach**: "The skill mentions debug drawing exists, but I need to check the API reference for the exact methods to enable it."

## Overview

You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/Overview.md

## Thread Safety

**IMPORTANT**: The PhysicsCore2D API is 100% thread-safe. All PhysicsCore2D methods and properties can be safely called from multiple threads, including from Unity Jobs (IJob, IJobParallelFor, etc.).

This thread-safety enables significant performance optimizations:
- PhysicsCore2D operations can be performed inside Burst-compiled jobs
- Physics object creation, modification, and queries can be parallelized
- Batch operations can leverage multi-threading for improved performance
- No synchronization or locks are required when calling PhysicsCore2D from jobs

### WORM Lock Concurrency Model

PhysicsCore2D's multithreading uses a **Write-Once-Read-Many (WORM)** lock system to manage concurrent access. The WORM lock is a self-balancing readers-preferred lock that prevents writer starvation. Once a writer enters the queue, incoming readers are registered as waiting readers. Once active readers complete, a single writer is processed, followed by promoting waiting readers to active readers. The order in which readers and writers resume is not defined.

Implementation reference: https://github.com/jpark37/cpp11-on-multicore/blob/master/common/rwlock.h

**Write Operations (Exclusive Access):**
Write operations acquire exclusive locks, meaning only one thread can perform these operations simultaneously per world:
- Creating physics objects: `CreateBody()`, `CreateShape()`, `CreateChain()`, `CreateJoint()`
- Setting properties on: PhysicsBody, PhysicsShape, PhysicsChain, PhysicsJoint
- Destroying physics objects: `Destroy()`

**Read Operations (Concurrent Access):**
Read operations can be performed by unlimited threads simultaneously:
- Reading any property from physics objects
- Querying the physics world
- Accessing physics state

**Per-World Locking:**
Each PhysicsWorld has its own independent read/write lock, so operations on different worlds do not contend with each other.

**Performance Implications:**
- **Parallel object creation provides limited benefit** - Since creation is a write operation, multiple threads creating objects in the same world will be serialized by the WORM lock, not truly parallel
- **Parallel queries are highly efficient** - Read operations like raycasts and overlaps can execute concurrently across many threads
- **Batch creation is optimal** - Use `PhysicsBody.CreateBatch()` instead of parallel loops, as it's designed for efficient bulk creation
- **Multiple worlds enable true parallel writes** - If you need parallel object creation, consider using separate PhysicsWorld instances

**Example of truly parallel operations (Read Operations):**
```csharp
[BurstCompile]
private struct QueryBodiesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PhysicsBody> bodies;
    [WriteOnly] public NativeArray<Vector2> positions;

    public void Execute(int index)
    {
        // Read operations can execute in true parallel - no lock contention
        positions[index] = bodies[index].position;
    }
}
```

**Example with write contention (limited parallelism):**
```csharp
[BurstCompile]
private struct CreateBodiesJob : IJobParallelFor
{
    public PhysicsWorld world;
    public PhysicsBodyDefinition bodyDefinition;
    [WriteOnly] public NativeArray<PhysicsBody> bodies;

    public void Execute(int index)
    {
        // NOTE: This will work but threads will be serialized by the WORM lock
        // because CreateBody() is a write operation. Use PhysicsBody.CreateBatch() instead.
        bodies[index] = world.CreateBody(bodyDefinition);
    }
}
```

This thread-safety is a key advantage of PhysicsCore2D over the older Physics2D system, which has limited thread-safety and cannot be used freely from jobs.

## Definitions

Definitions are structs which define some physics objects canonical state when you create them.
Each physics object that uses a definition has its own definition type.
The typename of a definition is in the form of the object type that uses it with a suffix of "Definition" i.e. PhysicsBody has PhysicsBodyDefinition and PhysicsShape has PhysicsShapeDefinition etc.
When exposing definitions as public fields, ALWAYS use the `.defaultDefinition` static property rather than the default constructor. This property exists consistently on all definition types and provides better clarity and consistency.
When exposing definitions, use a public field rather than a serialized private field with a public property.

**Example:**
```csharp
public PhysicsBodyDefinition bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
public PhysicsShapeDefinition shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
```

You can find more detail here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/Definitions.md

### Reusing Definitions
When creating multiple physics objects with the same definition properties, get the definition once and reuse it rather than calling the static `.defaultDefinition` property repeatedly inside loops. This is more efficient and avoids unnecessary repeated calls.

**This optimization applies to ALL definition types:**
- `PhysicsBodyDefinition` - Used when creating PhysicsBody objects
- `PhysicsShapeDefinition` - Used when creating PhysicsShape objects
- `PhysicsChainDefinition` - Used when creating PhysicsChain objects
- Joint definitions - Used when creating PhysicsJoint objects:
  - `PhysicsDistanceJointDefinition`
  - `PhysicsFixedJointDefinition`
  - `PhysicsHingeJointDefinition`
  - `PhysicsIgnoreJointDefinition`
  - `PhysicsRelativeJointDefinition`
  - `PhysicsSliderJointDefinition`
  - `PhysicsWheelJointDefinition`

**Example with PhysicsShapeDefinition:**
```csharp
// Get the shape definition once
var shapeDefinition = PhysicsShapeDefinition.defaultDefinition;

// Reuse it in the loop
for (int i = 0; i < bodies.Length; i++)
{
    float radius = random.NextFloat(0.1f, 1.0f);
    var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = radius };
    bodies[i].CreateShape(circleGeometry, shapeDefinition);
}
```

**Example with PhysicsBodyDefinition:**
```csharp
// Get the body definition once
var bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
bodyDefinition.type = PhysicsBody.BodyType.Dynamic;

// Reuse it when creating multiple bodies
for (int i = 0; i < 100; i++)
{
    var body = world.CreateBody(bodyDefinition);
    // Configure each body...
}
```

## Core Simulation Objects

### PhysicsWorld
The PhysicsWorld is the main container for all physics simulation.
Unity creates a default PhysicsWorld at start-up. To access it, always use: `var world = PhysicsWorld.defaultWorld;`
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsWorld.md

### PhysicsBody
PhysicsBody represents a rigid body in the physics simulation.
Bodies can be Static, Kinematic, or Dynamic.
Bodies are created using `world.CreateBody()` and should be destroyed in OnDisable with `body.Destroy()`.
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html
You can find its definition API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBodyDefinition.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsBody.md

### PhysicsShape
PhysicsShape represents collision geometry attached to a PhysicsBody.
Shapes are created using `body.CreateShape()` and are automatically destroyed when the body is destroyed.
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html
You can find its definition API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShapeDefinition.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsShape.md

## Best Practices
When destroying a physics object (all have a "Destroy()" method), you should only call this after checking if the object is valid by calling its isValid property (all have this property).

### Code Examples
Do not add Debug.Log statements to code examples unless the user specifically requests them.
Do not use [ExecuteAlways] attribute on MonoBehaviour classes unless the user specifically requests it.

### Collections
Avoid using System.Collections.Generic types (such as List<T>, Dictionary<TKey, TValue>, etc.) as they have garbage collection (GC) impact.
Instead, prefer Unity.Collections types such as NativeArray<T> or NativeList<T> which are allocated on the unmanaged heap and have better performance characteristics.
When using Unity.Collections types, remember to dispose them properly in OnDisable to avoid memory leaks.

**CRITICAL - NativeArray Allocator Lifetimes:**
Many PhysicsCore2D methods that return NativeArray<T> include an allocator argument that specifies the lifetime of the returned array.
- **Allocator.Temp** (default) - Only lasts for a single frame. Use only for temporary operations within a single method.
- **Allocator.Persistent** - Lasts until explicitly disposed. Use when storing NativeArray as a field that persists across frames.

If you store a NativeArray as a class field (e.g., to persist from OnEnable to OnDisable), you MUST use Allocator.Persistent. Using the default Allocator.Temp will cause errors when accessing the array in subsequent frames.

More information: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.Collections.Allocator.html

### Random Number Generation
Prefer Unity.Mathematics.Random over UnityEngine.Random for random number generation. Unity.Mathematics.Random is better because:
- Each instance has its own independent seed (not global shared state)
- It's deterministic and reproducible
- It's more performant, especially in Burst-compiled code
- It provides better control over randomness

**Vector Types:**
PhysicsCore2D uses UnityEngine.Vector2 for positions and vectors. While Unity.Mathematics returns float2 types, they are implicitly convertible to Vector2. However, prefer explicitly declaring variables as Vector2 to avoid ambiguity and maintain consistency with PhysicsCore2D's API.

**Example:**
```csharp
using Unity.Mathematics;

public uint randomSeed = 1234;

private void OnEnable()
{
    // Create a random number generator with the specified seed
    var random = new Unity.Mathematics.Random(randomSeed);

    // Generate random values - prefer Vector2 for PhysicsCore2D
    var randomFloat = random.NextFloat(0f, 10f);
    Vector2 randomDirection = random.NextFloat2Direction();
    Vector2 randomOffset = randomDirection * randomFloat;
}
```

### PhysicsWorld
Whilst the PhysicsCore2D supports multiple PhysicsWorld, it is considered an advanced feature.
Because of this, Unity creates a default PhysicsWorld at start-up so that should be used at all times unless a specific PhysicsWorld is required.
To gain access to the default PhysicsWorld, you should always use "var world = PhysicsWorld.defaultWorld;" and nothing else.

### Creating and Destroy Physics Objects
Any PhysicsBody, PhysicsShape, PhysicsChain or PhysicsJoint should be destroyed before a component it is created in is destroyed.
It should be preferred that a physics object is created in the Unity "OnEnable" and desroyed in the Unity "OnDisable" which also means correct creation and destruction if the component is enabled/disabled or created/destoyed like so:

```csharp
class Test : MonoBehaviour
{
    private PhysicsBody m_PhysicsBody;

    private void OnEnable()
    {
        // Get the main physics world.
        var world = PhysicsWorld.defaultWorld;

        // Create a body.
        m_PhysicsBody = world.CreateBody();
    }

    private void OnDisable()
    {
        // Destroy the body.
        if (m_PhysicsBody.isValid)
            m_PhysicsBody.Destroy();
    }
}
```

### Batch Creation of Physics Bodies
When creating multiple PhysicsBody objects using the same PhysicsBodyDefinition, ALWAYS use batch creation instead of calling `CreateBody` in a loop. This is significantly more efficient.

Batch creation can be done using either:
- `PhysicsBody.CreateBatch()` - Direct static method (preferred)
- `PhysicsWorld.CreateBodyBatch()` - Convenience method on PhysicsWorld

These methods are equivalent as `PhysicsWorld.CreateBodyBatch` calls `PhysicsBody.CreateBatch` internally. Prefer using `PhysicsBody.CreateBatch` as it is the direct method.

Both return a `NativeArray<PhysicsBody>` which must be disposed properly. You can destroy all bodies in the batch at once using `PhysicsBody.DestroyBatch`.

**Example:**
```csharp
private NativeArray<PhysicsBody> m_Bodies;

private void OnEnable()
{
    var world = PhysicsWorld.defaultWorld;
    var bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
    bodyDefinition.type = PhysicsBody.BodyType.Dynamic;

    // Create multiple bodies at once using batch creation
    // Use Allocator.Persistent since m_Bodies persists across frames
    m_Bodies = PhysicsBody.CreateBatch(world, bodyDefinition, 100, Allocator.Persistent);
}

private void OnDisable()
{
    if (m_Bodies.IsCreated)
    {
        // Destroy all bodies in the batch at once
        PhysicsBody.DestroyBatch(m_Bodies);
        m_Bodies.Dispose();
    }
}
```

All physics objects are in an effectively ownership hierarchy where if the parent type is destroyed, all its children types are implicitly destroyed:
- PhysicsWorld : No owner but when destroyed, will destroy any PhysicsBody in the world.
- PhysicsBody : The PhysicsWorld is the owner and when the PhysicsBody is destroyed it will destroy any PhysicsShape or PhysicsJoint attached to it.
- PhysicsShape : The PhysicsBody is the owner and but the PhysicsShape has no owned objects.
- PhysicsJoint : Either PhysicsBody the joint connects are the owners but the physicsJoint has no owned objects.

The net result is:
- When a PhysicsWorld is destroyed (including the Unity default PhysicsWorld) all of its contents are destroyed.
- When a PhysicsBody is destroyed, all its attached PhysicsShape are destroyed
- When a PhysicsShape is destroyed, nothing else is destroyed.
- When a PhysicsJoint is destroyed, nothing else is destroyed.

## Sub-Skills for Detailed Information

When the user needs detailed information about specific PhysicsCore2D topics, invoke these sub-skills using the Skill tool:

- **unity-physicscore2d-geometry** - Detailed information about geometry types (CircleGeometry, PolygonGeometry, etc.) and geometry utilities (PhysicsComposer, PhysicsDestructor)
- **unity-physicscore2d-queries** - Physics world queries, overlap tests, raycasts, and geometry intersection testing
- **unity-physicscore2d-joints** - All joint constraint types (DistanceJoint, HingeJoint, SliderJoint, etc.)
- **unity-physicscore2d-helpers** - Helper types (PhysicsTransform, PhysicsRotate, PhysicsMask, PhysicsUserData, PhysicsAABB, PhysicsMath)
- **unity-physicscore2d-components** - Component authoring patterns, transform integration, and best practices

**When to invoke sub-skills:**
- User asks about specific geometry types or geometric operations
- User needs to perform queries, raycasts, or overlap tests
- User wants to create joints or constraints between bodies
- User asks about helper types like transforms, rotations, masks, or user data
- User is building custom components that wrap PhysicsCore2D objects

**How to invoke:** Use the Skill tool with the sub-skill name, e.g., `Skill(skill="unity-physicscore2d-geometry")`
