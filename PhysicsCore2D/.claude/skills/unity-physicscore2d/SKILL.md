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

## ⚠️ API Usage Rules - READ THIS FIRST ⚠️

**ABSOLUTE RULE: NEVER INVENT OR GUESS API METHODS, PROPERTIES, OR PARAMETERS.**

This is the #1 most important rule when working with PhysicsCore2D. You MUST follow this protocol:

### Before Writing Any PhysicsCore2D API Call:

1. **STOP and CHECK**: Is this exact API call documented in this skill or the linked API references?
2. **If NO**: Do NOT write the code. Instead, say: "I need to verify this API in the documentation first" and ask the user or consult the API reference links provided.
3. **If UNSURE**: Do NOT guess. Say: "I'm not certain about this API. Let me verify the correct method/property name."

### Common Mistakes to Avoid:

❌ **DON'T invent singular/plural variations:**
- WRONG: `body.GetShape(0)` ← Invented
- RIGHT: `body.GetShapes(Allocator.Temp)` ← Documented
- **When unsure if a method exists or what it's called, ASK THE USER or state you need to verify**

❌ **DON'T assume properties exist:**
- WRONG: `world.debugDrawingEnabled = true` ← Invented
- RIGHT: Check API reference first, then use documented API

❌ **DON'T guess method signatures:**
- WRONG: `body.SetPosition(x, y)` ← Might not exist
- RIGHT: Use documented property: `body.position = new Vector2(x, y)`

### Verification Protocol:

**BEFORE writing code that uses PhysicsCore2D API:**
1. Check this skill document for documented examples
2. Check the API reference links provided (docs.unity3d.com URLs)
3. If not found in either place: **STOP, ASK USER, or state you need to verify**
4. Only write code using APIs that are explicitly documented

### When You Don't Know:

If you're uncertain about ANY PhysicsCore2D API:
- ✅ **DO**: Say "I need to verify the correct API for [operation] in the documentation"
- ✅ **DO**: Ask the user: "What method should I use to [operation]?"
- ❌ **DON'T**: Guess based on what seems logical
- ❌ **DON'T**: Invent variations of method names
- ❌ **DON'T**: Assume APIs exist without verification

### API Reference Links (Use These for Verification):
- PhysicsWorld: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld.html
- PhysicsBody: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsBody.html
- PhysicsShape: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html

**Remember: It's better to ask or verify than to invent incorrect API calls.**

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

#### Transform Integration with transformObject

**CRITICAL:** To automatically synchronize a PhysicsBody with a Unity GameObject's Transform, set the `PhysicsBody.transformObject` property to the Transform.

When `transformObject` is set:
- The Transform's position and rotation are automatically updated to match the PhysicsBody
- This happens automatically during the physics simulation
- No manual synchronization code is needed

**Example:**
```csharp
private PhysicsBody m_Body;
private GameObject m_GameObject;

private void OnEnable()
{
    var world = PhysicsWorld.defaultWorld;
    var bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
    bodyDefinition.type = PhysicsBody.BodyType.Dynamic;
    bodyDefinition.position = new Vector2(0, 0);

    m_Body = world.CreateBody(bodyDefinition);

    var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = 0.5f };
    var shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
    m_Body.CreateShape(circleGeometry, shapeDefinition);

    // Create GameObject for visualization
    m_GameObject = new GameObject("PhysicsObject");
    var spriteRenderer = m_GameObject.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = mySprite;

    // Link the PhysicsBody to the Transform for automatic sync
    m_Body.transformObject = m_GameObject.transform;
}

private void OnDisable()
{
    if (m_Body.isValid)
        m_Body.Destroy();
    if (m_GameObject != null)
        Destroy(m_GameObject);
}

// No Update() method needed - transform syncs automatically!
```

**Benefits:**
- Eliminates manual transform synchronization code
- Automatically handles rotation as well as position
- More efficient than manual updates every frame
- Keeps rendering in sync with physics simulation

**Important Notes:**
- Set `transformObject` AFTER creating the body and GameObject
- The Transform will be updated to match the PhysicsBody, not vice versa
- For Kinematic bodies that you move manually, you still set `body.position` - the Transform will follow
- Unsetting `transformObject` (set to null) stops automatic synchronization

### PhysicsShape
PhysicsShape represents collision geometry attached to a PhysicsBody.
Shapes are created using `body.CreateShape()` and are automatically destroyed when the body is destroyed.
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.html
You can find its definition API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShapeDefinition.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsShape.md

## Best Practices
When destroying a physics object (all have a "Destroy()" method), you should only call this after checking if the object is valid by calling its isValid property (all have this property).

**Transform Integration:**
- Always use `PhysicsBody.transformObject` to link physics bodies to Unity GameObjects
- This provides automatic synchronization between physics simulation and visual representation
- Never manually update Transform positions in Update() when using transformObject
- Set transformObject after creating both the PhysicsBody and GameObject

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

### Collections and Handle Structs

**CRITICAL: PhysicsCore2D types are handle structs, not value structs.**

PhysicsCore2D types (PhysicsBody, PhysicsShape, PhysicsChain, PhysicsJoint, PhysicsWorld) are **handle structs** that contain references/IDs to the actual physics objects. This has important implications when storing them in collections:

#### Storing in NativeArray/NativeList

Since PhysicsCore2D types are unmanaged handle structs, you CAN store them in NativeArray and NativeList:

```csharp
// Valid - PhysicsBody is an unmanaged handle struct
private NativeArray<PhysicsBody> m_Bodies;
private NativeList<PhysicsShape> m_Shapes;
```

#### Direct Property Modification

Because they are handles (not value structs), you can modify properties directly through NativeArray/NativeList indexing:

```csharp
// CORRECT - Direct property modification works because PhysicsBody is a handle
m_Bodies[i].position = newPosition;
m_Bodies[i].velocity = newVelocity;

// CORRECT - Method calls also work directly
m_Bodies[i].CreateShape(geometry, definition);
```

**You do NOT need the copy-modify-writeback pattern** that regular value structs require:

```csharp
// INCORRECT - Unnecessary for PhysicsCore2D handles
var body = m_Bodies[i];
body.position = newPosition;
m_Bodies[i] = body; // Don't need this!

// CORRECT - Just set directly
m_Bodies[i].position = newPosition;
```

#### C# Compiler Limitation with Setting Properties on By-Value Structs

**IMPORTANT:** While PhysicsCore2D handle structs support direct property modification, C# has a fundamental limitation: **you cannot set properties on a by-value struct unless it's stored in a local variable first.**

**This limitation ONLY applies to property setters.** Reading properties (getters) and calling methods work fine on by-value struct results.

**This limitation applies to ALL PhysicsCore2D handle struct types:**
- PhysicsWorld
- PhysicsBody
- PhysicsShape
- PhysicsChain
- PhysicsJoint (all joint types)

The compiler will not allow setting properties on by-value struct results:
```csharp
// COMPILER ERROR - Cannot modify the result of by-value expressions
m_Bodies[index].position = newPosition;           // From indexer - ERROR
m_Shapes[index].isSensor = true;                  // From indexer - ERROR
PhysicsWorld.defaultWorld.gravity = Vector2.down; // From static property - ERROR
SomeMethod().friction = 0.5f;                     // From method return - ERROR
```

But reading properties and calling methods work fine:
```csharp
// WORKS - Reading properties is fine
var pos = m_Bodies[index].position;
var isSensor = m_Shapes[index].isSensor;
var gravity = PhysicsWorld.defaultWorld.gravity;
bool isValid = m_Bodies[index].isValid;

// WORKS - Calling methods is fine
m_Bodies[index].CreateShape(geometry, definition);
m_Bodies[index].Destroy();
PhysicsWorld.defaultWorld.Step(Time.deltaTime);
```

**Workaround for setting properties:** Store the struct in a local variable first, then set its properties:

```csharp
// CORRECT - Read into local variable first
var body = m_Bodies[index];
body.position = newPosition;
// No writeback needed - modifications happen through the handle!

var shape = m_Shapes[index];
shape.isSensor = true;
// No writeback needed!

var chain = m_Chains[index];
chain.friction = 0.5f;
// No writeback needed!

var joint = m_Joints[index];
joint.maxForce = 100f;
// No writeback needed!
```

This is purely a C# compiler restriction, not a limitation of PhysicsCore2D. The underlying operation is valid (since all PhysicsCore2D types are handle structs), but you must use a local variable to satisfy the compiler. **You do NOT need to write the value back** to the array - the modifications take effect immediately through the handle.

**Examples in context:**
```csharp
// Setting position from a NativeArray<PhysicsBody>
var enemy = m_EnemyBodies[index];
enemy.position = newPosition;
// Done - no writeback needed

// Setting velocity from a NativeList<PhysicsBody>
var bullet = m_BulletBodies[i];
bullet.velocity = Vector2.up * speed;
// Done - no writeback needed

// Setting shape properties from a NativeArray<PhysicsShape>
var shape = m_Shapes[i];
shape.density = 2.0f;
// Done - no writeback needed

// Setting world properties from static property
var world = PhysicsWorld.defaultWorld;
world.gravity = Vector2.down * 9.81f;
// Done - no writeback needed

// Local variable doesn't need this pattern
m_PlayerBody.position = newPosition; // Works directly - already a local variable
```

#### GameObject Collections

GameObjects are managed reference types and cannot be stored in NativeArray or NativeList. Use standard C# collections:

```csharp
// INCORRECT - GameObject is a managed reference type
private NativeArray<GameObject> m_GameObjects; // Compiler error!

// CORRECT - Use managed arrays or List<T>
private GameObject[] m_GameObjects;
private System.Collections.Generic.List<GameObject> m_BulletGameObjects;
```

#### Summary Table

| Type | Collection Type | Direct Property Modification |
|------|----------------|------------------------------|
| PhysicsBody | NativeArray/NativeList | ✅ Yes - handle struct |
| PhysicsShape | NativeArray/NativeList | ✅ Yes - handle struct |
| PhysicsChain | NativeArray/NativeList | ✅ Yes - handle struct |
| PhysicsJoint | NativeArray/NativeList | ✅ Yes - handle struct |
| GameObject | Array or List<T> | N/A - managed type |
| bool | NativeArray/NativeList | ❌ No - value struct (need copy-modify-writeback) |
| int/float | NativeArray/NativeList | ❌ No - value struct (need copy-modify-writeback) |

**Key Takeaway:** PhysicsCore2D handle structs behave differently from regular value structs. When indexed from NativeArray/NativeList, you can directly modify their properties because the struct contains a reference to the underlying data, not the data itself.

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
