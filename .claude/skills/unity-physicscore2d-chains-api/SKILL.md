---
name: unity-physicscore2d-chains-api
description: Authoritative Unity 6000.7 PhysicsCore2D API reference for Chains. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsChain, PhysicsChainDefinition. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Chains

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.7, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.7.0a3 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsChain`, `PhysicsChainDefinition`.

## PhysicsChain

> A dedicated shape that produces a chain of shapes connected together to produce a continuous surface. Chain shapes provide a smooth, continuous surface that will not produce "ghost" collisions. A [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html) is automatically destroyed when the body it is in is destroyed. A [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html) cannot exist unattached from a body. This will produce shapes of type [PhysicsShape.ShapeType.ChainSegment](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-ChainSegment.html). - Chains are one-sided. - A chain has no mass therefore should ideally be used on static bodies. - A chain can have a counter-clockwise winding order (normal points right of segment direction). - A chain is either a loop or open. - A chain must have at least 4 points. - The distance between any two points must be greater than [PhysicsWorld.linearSlop](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-linearSlop.html). - A chain should not self intersect (this is not validated). - An open chain has no collision on the first and final edge. - You may overlap two open chains on their first three and/or last three points to get smooth collision.

**Full name:** `Unity.U2D.Physics.PhysicsChain`  
**Docs:** [Unity.U2D.Physics.PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html)

### Properties

| Name | Summary |
|------|---------|
| `aabb` | Get the world AABB that bounds this chain. The bounds of the shape is inflated slightly due to speculative collision detection. The inflation is smaller on Static shape types however it is not zero due to time-of-impact collision detection. If an exact AABB is required then you can retrieve that via the shape geometry. |
| `body` | The body which the chain is attached to. |
| `bounciness` | The bounciness of the chain. Usually this is within the range [0, 1]. Values higher than 1 will result in energy being added which can lead to an unstable simulation. |
| `bouncinessMixing` | Defines the method used when mixing the bounciness values of two shapes to form a shape contact. This is assigned to the current [PhysicsShape.surfaceMaterial](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-surfaceMaterial.html). |
| `callbackTarget` | Get/Set the [System.Object](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/System.Object.html) that callbacks for the shapes owned by this chain will be sent to. Care should be taken with any [System.Object](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/System.Object.html) assigned as a callback target that isn't a [UnityEngine.Object](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.Object.html) as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A [PhysicsEvents.ContactFilterEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactFilterEvent.html) with call [PhysicsCallbacks.IContactFilterCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactFilterCallback.html). - A [PhysicsEvents.PreSolveEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.PreSolveEvent.html) with call [PhysicsCallbacks.IPreSolveCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IPreSolveCallback.html). - A [PhysicsEvents.TriggerBeginEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerBeginEvent.html) with call [PhysicsCallbacks.ITriggerCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback.html). - A [PhysicsEvents.TriggerEndEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.TriggerEndEvent.html) with call [PhysicsCallbacks.ITriggerCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.ITriggerCallback.html). - A [PhysicsEvents.ContactBeginEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactBeginEvent.html) with call [PhysicsCallbacks.IContactCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactCallback.html). - A [PhysicsEvents.ContactEndEvent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEvents.ContactEndEvent.html) with call [PhysicsCallbacks.IContactCallback](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCallbacks.IContactCallback.html). |
| `friction` | The friction of the owned chain shapes. Usually this is within the range [0, 1]. Values higher than 1 will result in energy being added which can lead to an unstable simulation. |
| `frictionMixing` | Defines the method used when mixing the friction values of two shapes to form a shape contact. This is assigned to the current [PhysicsShape.surfaceMaterial](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape-surfaceMaterial.html). |
| `isOwned` | Get if the chain is owned. See [PhysicsChain.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain-SetOwner.html). |
| `isValid` | Check if the shape is valid. |
| `owner` | The owner object associated with this chain, or NULL if no owner has been specified. This is a convenience property that returns the same value as [PhysicsChain.GetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain-GetOwner.html). |
| `ownerUserData` | Get [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) that can be used for any purpose, typically by the owner only. |
| `physicsHandle` | Get the physics handle. |
| `segmentCount` | Get the number of Chain segments that this chain has created and owns. See [PhysicsShape.ShapeType.ChainSegment](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-ChainSegment.html). |
| `userData` | Get/Set [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the chain is attached to. |
| `worldDrawing` | Controls whether this chain is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a chain from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `CastRay(PhysicsQuery.CastRayInput, PhysicsShape)`

Check if a ray intersects the chain. See [PhysicsQuery.CastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.CastResult.html).

**Params:**
- `castRayInput` — The configuration of the ray to cast.
- `chainSegmentShape` — A reference to the chain segment shape that the query found.

**Returns:** The intersection details, if any, that were found.

#### `CastShape(PhysicsQuery.CastShapeInput, PhysicsShape)`

Calculate if a cast shape intersects the chain. See [PhysicsQuery.CastShapeInput](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.CastShapeInput.html) and [PhysicsQuery.CastResult](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsQuery.CastResult.html).

**Params:**
- `input` — The cast shape input used to check for intersection.
- `chainSegmentShape` — A reference to the chain segment shape that the query found.

**Returns:** The results of the intersection test.

#### `ClosestPoint(Vector2, PhysicsShape)`

Calculate the closest point on this chain to the specified point.

**Params:**
- `point` — The point to check.
- `chainSegmentShape` — A reference to the chain segment shape that the query found.

**Returns:** The closest point on the shape to the specified point.

#### `Create(PhysicsBody, ChainGeometry, PhysicsChainDefinition)`

Create a Chain of multiple shapes attached to the specified body which itself is within a world.

**Params:**
- `body` — The body to attach the shape(s) to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `Create(PhysicsBody, ReadOnlySpan<Vector2>, PhysicsChainDefinition)`

Create a Chain of multiple shapes attached to the specified body which itself is within a world.

**Params:**
- `body` — The body to attach the shape(s) to.
- `vertices` — The vertices that will create the ChainSegment shapes.
- `definition` — The shape definition to use.

**Returns:** The created chain.

#### `Destroy(int)`

Destroy the [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html) and all the [PhysicsShape.ShapeType.ChainSegment](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-ChainSegment.html) it owns. If the object is owned with [PhysicsChain.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain-SetOwner.html) then you must provide the owner key it returned. Failing to do so will return a warning and the chain will not be destroyed. The lifetime of the specified owner object is not linked to this chain i.e. this chain will still be owned by the owner object, even if it is destroyed. This is the only way to destroy shapes of type [PhysicsShape.ShapeType.ChainSegment](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsShape.ShapeType-ChainSegment.html) if they were created by a [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html).

**Params:**
- `ownerKey` — Optional owner key returned when using [PhysicsChain.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain-SetOwner.html).

**Returns:** If the chain was destroyed or not.

#### `Equals(object)`

#### `Equals(PhysicsChain)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this chain as specified using [PhysicsChain.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain-SetOwner.html).

**Returns:** The owner object associated with this chain or NULL if no owner has been specified.

#### `GetSegmentIndex(PhysicsShape)`

Get the index of the specified Chain Segment PhysicsShape.

**Params:**
- `chainSegmentShape` — The chain segment shape to find the index of.

**Returns:** The index of the chain segment shape in its parent chain. This is a value of zero to the number of chain segment shapes - 1.

#### `GetSegments(Unity.Collections.Allocator)`

Get all the Chain segments that this chain has created and owns.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be [Unity.Collections.Allocator.Temp](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Temp.html), [Unity.Collections.Allocator.TempJob](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-TempJob.html) or [Unity.Collections.Allocator.Persistent](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.Collections.Allocator-Persistent.html).

**Returns:** The chain segments that this chain has created and owns. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `SetOwner(ReadOnlySpan<PhysicsChain>, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `chains` — The chains to set ownership for.
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.
- `ownerKey` — The owner key to be used. The value must be non-zero. You can use [PhysicsWorld.CreateOwnerKey](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CreateOwnerKey.html) for this value although any non-zero integer will work.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use [PhysicsWorld.CreateOwnerKey](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorld-CreateOwnerKey.html) for this value although any non-zero integer will work.

#### `SetOwner(Object)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.

**Returns:** The owner key assigned.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set [PhysicsUserData](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html) that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using [PhysicsChain.SetOwner](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain-SetOwner.html).

#### `ToString()`

#### `UpdateVertices(ReadOnlySpan<Vector2>, bool)`

Update the existing ChainSegment shapes with the provided vertices. Modifying the vertices will cause contacts to be recalculated however it may cause overlaps and/or collision tunnelling if not used carefully. The number of vertices provided and looping option should be the same as was used when the Chain was originally created. Any mismatch between the two will result in a warning.

**Params:**
- `vertices` — The vertices used to update the existing ChainSegment shapes.
- `isLoop` — Indicates a closed chain formed by connecting the first and last vertices specified. This should match what was originally specified in [PhysicsChainDefinition.isLoop](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChainDefinition-isLoop.html).

## PhysicsChainDefinition

> A [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html) definition used to specify the chain of vertices that will produce multiple [ChainSegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html) shape types. Additionally, non-geometric properties can be specified here.

**Full name:** `Unity.U2D.Physics.PhysicsChainDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsChainDefinition](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChainDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `contactFilter` | The contact filter used to control which contacts this shape can participate in. |
| `defaultDefinition` | Get a default [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html) definition. |
| `isLoop` | Indicates a closed chain formed by connecting the first and last vertices specified. When enabled, no ghost vertices should be defined in the [ChainGeometry.vertices](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainGeometry-vertices.html) with all being used to define [ChainSegmentGeometry](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry.html) with the ghost vertices being calculated automatically to force a closed loop. When disabled, the [ChainGeometry.vertices](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainGeometry-vertices.html) should define [ChainSegmentGeometry.ghost1](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry-ghost1.html) as the first vertex followed by at least two vertices or more defining the subsequent edges and finally a [ChainSegmentGeometry.ghost2](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.ChainSegmentGeometry-ghost2.html) vertex, therefore there must be at least 4 vertices. |
| `surfaceMaterial` | The surface material for the shape comprising of many properties such as friction, bounciness, rolling resistance etc. |
| `triggerEvents` | Controls whether this chain produces trigger events which can be retrieved after the simulation has completed. This applies to triggers and non-triggers alike. |

### Methods

#### `new()`

Create a default [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html) definition.

#### `new(bool)`

Create a default [PhysicsChain](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html) definition.

**Params:**
- `useSettings` — Controls whether the default come settings from the physics settings or not.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.7.0a3 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
