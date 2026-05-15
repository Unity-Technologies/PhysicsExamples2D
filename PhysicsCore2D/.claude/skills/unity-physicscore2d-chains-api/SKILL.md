---
name: unity-physicscore2d-chains-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for Chains. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsChain, PhysicsChainDefinition. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Chains

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsChain`, `PhysicsChainDefinition`.

## PhysicsChain

> A dedicated shape that produces a chain of shapes connected together to produce a continuous surface. Chain shapes provide a smooth, continuous surface that will not produce "ghost" collisions. A is automatically destroyed when the body it is in is destroyed. A cannot exist unattached from a body. This will produce shapes of type . - Chains are one-sided. - A chain has no mass therefore should ideally be used on static bodies. - A chain can have a counter-clockwise winding order (normal points right of segment direction). - A chain is either a loop or open. - A chain must have at least 4 points. - The distance between any two points must be greater than . - A chain should not self intersect (this is not validated). - An open chain has no collision on the first and final edge. - You may overlap two open chains on their first three and/or last three points to get smooth collision.

**Full name:** `Unity.U2D.Physics.PhysicsChain`  
**Docs:** [Unity.U2D.Physics.PhysicsChain](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html)

### Properties

| Name | Summary |
|------|---------|
| `aabb` | Get the world AABB that bounds this chain. The bounds of the shape is inflated slightly due to speculative collision detection. The inflation is smaller on Static shape types however it is not zero due to time-of-impact collision detection. If an exact AABB is required then you can retrieve that via the shape geometry. |
| `body` | The body which the chain is attached to. |
| `bounciness` | The bounciness of the chain. Usually this is within the range [0, 1]. Values higher than 1 will result in energy being added which can lead to an unstable simulation. |
| `bouncinessMixing` | Defines the method used when mixing the friction values of two shapes to form a shape contact. This is assigned to the current . |
| `callbackTarget` | Get/Set the that callbacks for the shapes owned by this chain will be sent to. Care should be taken with any assigned as a callback target that isn't a as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A with call . - A with call . - A with call . - A with call . - A with call . - A with call . |
| `friction` | The friction of the owned chain shapes. Usually this is within the range [0, 1]. Values higher than 1 will result in energy being added which can lead to an unstable simulation. |
| `frictionMixing` | Defines the method used when mixing the friction values of two shapes to form a shape contact. This is assigned to the current . |
| `isOwned` | Get if the chain is owned. See . |
| `isValid` | Check if the shape is valid. |
| `ownerUserData` | Get that can be used for any purpose, typically by the owner only. |
| `segmentCount` | Get the number of Chain segments that this chain has created and owns. See . |
| `userData` | Get/Set that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the chain is attached to. |
| `worldDrawing` | Controls whether this chain is automatically drawn when the world is drawn. |

### Methods

#### `CastRay(PhysicsQuery.CastRayInput, PhysicsShape)`

Check if a ray intersects the chain. See .

**Params:**
- `castRayInput` — The configuration of the ray to cast.
- `chainSegmentShape` — A reference to the chain segment shape that the query found.

**Returns:** The intersection details, if any, that were found.

#### `CastShape(PhysicsQuery.CastShapeInput, PhysicsShape)`

Calculate if a cast shape intersects the chain. See and .

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

#### `Create(PhysicsBody, ReadOnlySpan{UnityEngine.Vector2}, PhysicsChainDefinition)`

Create a Chain of multiple shapes attached to the specified body which itself is within a world.

**Params:**
- `body` — The body to attach the shape(s) to.
- `vertices` — The vertices that will create the ChainSegment shapes.
- `definition` — The shape definition to use.

**Returns:** The created chain.

#### `Destroy(int)`

Destroy the and all the it owns. If the object is owned with then you must provide the owner key it returned. Failing to do so will return a warning and the chain will not be destroyed. The lifetime of the specified owner object is not linked to this chain i.e. this chain will still be owned by the owner object, even if it is destroyed. This is the only way to destroy shapes of type if they were created by a .

**Params:**
- `ownerKey` — Optional owner key returned when using .

**Returns:** If the chain was destroyed or not.

#### `Equals(object)`

#### `Equals(PhysicsChain)`

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this chain as specified using .

**Returns:** The owner object associated with this chain or NULL if no owner has been specified.

#### `GetSegmentIndex(PhysicsShape)`

Get the index of the specified Chain Segment PhysicsShape.

**Params:**
- `chainSegmentShape` — The chain segment shape to find the index of.

**Returns:** The index of the chain segment shape in its parent chain. This is a value of zero to the number of chain segment shapes - 1.

#### `GetSegments(Unity.Collections.Allocator)`

Get all the Chain segments that this chain has created and owns.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The chain segments that this chain has created and owns. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `operator ==(PhysicsChain, PhysicsChain)`

#### `operator !=(PhysicsChain, PhysicsChain)`

#### `SetOwner(ReadOnlySpan{Unity.U2D.Physics.PhysicsChain}, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `chains` — The chains to set ownership for.
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.
- `ownerKey` — The owner key to be used. The value must be non-zero. You can use for this value although any non-zero integer will work.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use for this value although any non-zero integer will work.

#### `SetOwner(Object)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.

**Returns:** The owner key assigned.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using .

#### `ToString()`

#### `UpdateVertices(ReadOnlySpan{UnityEngine.Vector2}, bool)`

Update the existing ChainSegment shapes with the provided vertices. Modifying the vertices will cause contacts to be recalculated however it may cause overlaps and/or collision tunnelling if not used carefully. The number of vertices provided and looping option should be the same as was used when the Chain was originally created. Any mismatch between the two will result in a warning.

**Params:**
- `vertices` — The vertices used to update the existing ChainSegment shapes.
- `isLoop` — Indicates a closed chain formed by connecting the first and last vertices specified. This should match what was originally specified in .

## PhysicsChainDefinition

> A definition used to specify the chain of vertices that will produce multiple shape types. Additionally, non-geometric properties can be specified here.

**Full name:** `Unity.U2D.Physics.PhysicsChainDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsChainDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChainDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `contactFilter` | The contact filter used to control which contacts this shape can participate in. |
| `defaultDefinition` | Get a default definition. |
| `isLoop` | Indicates a closed chain formed by connecting the first and last vertices specified. When enabled, no ghost vertices should be defined in the with all being used to define with the ghost vertices being calculated automatically to force a closed loop. When disabled, the should define as the first vertex followed by at least two vertices or more defining the subsequent edges and finally a vertex, therefore there must be at least 4 vertices. |
| `surfaceMaterial` | The surface material for the shape comprising of many properties such as friction, bounciness, rolling resistance etc. |
| `triggerEvents` | Controls whether this chain produces trigger events which can be retrieved after the simulation has completed. This applies to triggers and non-triggers alike. |

### Methods

#### `new()`

Create a default definition.

#### `new(bool)`

Create a default definition.

**Params:**
- `useSettings` — Controls whether the default come settings from the physics settings or not.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
