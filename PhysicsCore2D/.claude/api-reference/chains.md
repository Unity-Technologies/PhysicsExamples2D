# PhysicsCore2D — Chains

_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsChain`, `PhysicsChainDefinition`.

## PhysicsChain

> A dedicated shape that produces a chain of shapes connected together to produce a continuous surface. Chain shapes provide a smooth, continuous surface that will not produce "ghost" collisions. A PhysicsChain is automatically destroyed when the body it is in is destroyed. A PhysicsChain cannot exist unattached from a body. This will produce shapes of type PhysicsShape.ShapeType.ChainSegment. - Chains are one-sided. - A chain has no mass and should be used on static bodies. - A chain can have a counter-clockwise winding order (normal points right of segment direction). - A chain is either a loop or open. - A chain must have at least 4 points. - The distance between any two points must be greater than PhysicsWorld._linearSlop. - A chain should not self intersect (this is not validated). - An open chain has no collision on the first and final edge. - You may overlap two open chains on their first three and/or last three points to get smooth collision.

**Full name:** `Unity.U2D.Physics.PhysicsChain`  
**Docs:** [Unity.U2D.Physics.PhysicsChain](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChain.html)

### Properties

| Name | Summary |
|------|---------|
| `aabb` | Get the world AABB that bounds this chain. The bounds of the shape is inflated slightly due to speculative collision detection. The inflation is smaller on Static shape types however it is not zero due to time-of-impact collision detection. If an exact AABB is required then you can retrieve that via the shape geometry. |
| `body` | The body which the chain is attached to. |
| `bounciness` | The bounciness of the chain. Usually this is within the range [0, 1]. Values higher than 1 will result in energy being added which can lead to an unstable simulation. |
| `bouncinessMixing` | Defines the method used when mixing the friction values of two shapes to form a shape contact. This is assigned to the current PhysicsShape._surfaceMaterial. |
| `callbackTarget` | Get/Set the Object that callbacks for the shapes owned by this chain will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.ContactFilterEvent with call PhysicsCallbacks.IContactFilterCallback. - A PhysicsEvents.PreSolveEvent with call PhysicsCallbacks.IPreSolveCallback. - A PhysicsEvents.TriggerBeginEvent with call PhysicsCallbacks.ITriggerCallback. - A PhysicsEvents.TriggerEndEvent with call PhysicsCallbacks.ITriggerCallback. - A PhysicsEvents.ContactBeginEvent with call PhysicsCallbacks.IContactCallback. - A PhysicsEvents.ContactEndEvent with call PhysicsCallbacks.IContactCallback. |
| `friction` | The friction of the owned chain shapes. Usually this is within the range [0, 1]. Values higher than 1 will result in energy being added which can lead to an unstable simulation. |
| `frictionMixing` | Defines the method used when mixing the friction values of two shapes to form a shape contact. This is assigned to the current PhysicsShape._surfaceMaterial. |
| `isOwned` | Get if the chain is owned. See PhysicsChain.SetOwner. |
| `isValid` | Check if the shape is valid. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `segmentCount` | Get the number of Chain segments that this chain has created and owns. See PhysicsShape.ShapeType.ChainSegment. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the chain is attached to. |
| `worldDrawing` | Controls whether this chain is automatically drawn when the world is drawn. |

### Methods

#### `CastRay(PhysicsQuery.CastRayInput, PhysicsShape)`

Check if a ray intersects the chain. See PhysicsQuery.CastResult.

**Params:**
- `castRayInput` — The configuration of the ray to cast.
- `chainSegmentShape` — A reference to the chain segment shape that the query found.

**Returns:** The intersection details, if any, that were found.

#### `CastShape(PhysicsQuery.CastShapeInput, PhysicsShape)`

Calculate if a cast shape intersects the chain. See PhysicsQuery.CastShapeInput and PhysicsQuery.CastResult.

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

#### `Destroy(int)`

Destroy the PhysicsChain and all the PhysicsShape.ShapeType.ChainSegment it owns. If the object is owned with PhysicsChain.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the chain will not be destroyed. The lifetime of the specified owner object is not linked to this chain i.e. this chain will still be owned by the owner object, even if it is destroyed. This is the only way to destroy shapes of type PhysicsShape.ShapeType.ChainSegment if they were created by a PhysicsChain.

**Params:**
- `ownerKey` — Optional owner key returned when using PhysicsChain.SetOwner.

**Returns:** If the chain was destroyed or not.

#### `GetOwner()`

Get the owner object associated with this chain as specified using PhysicsChain.SetOwner.

**Returns:** The owner object associated with this chain or NULL if no owner has been specified.

#### `GetSegmentIndex(PhysicsShape)`

Get the index of the specified Chain Segment PhysicsShape.

**Params:**
- `chainSegmentShape` — The chain segment shape to find the index of.

**Returns:** The index of the chain segment shape in its parent chain. This is a value of zero to the number of chain segment shapes - 1.

#### `GetSegments(Unity.Collections.Allocator)`

Get all the Chain segments that this chain has created and owns.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The chain segments that this chain has created and owns. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `SetOwner(ReadOnlySpan<PhysicsChain>, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `chains` — The chains to set ownership for.
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.
- `ownerKey` — The owner key to be used. The value must be non-zero. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

**Returns:** The owner key assigned.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

**Returns:** The owner key assigned.

#### `SetOwner(Object)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.

**Returns:** The owner key assigned.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsChain.SetOwner.

## PhysicsChainDefinition

> A PhysicsChain definition used to specify the chain of vertices that will produce multiple ChainSegmentGeometry shape types. Additionally, non-geometric properties can be specified here.

**Full name:** `Unity.U2D.Physics.PhysicsChainDefinition`  
**Docs:** [Unity.U2D.Physics.PhysicsChainDefinition](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsChainDefinition.html)

### Properties

| Name | Summary |
|------|---------|
| `contactFilter` | The contact filter used to control which contacts this shape can participate in. |
| `defaultDefinition` | Get a default PhysicsChain definition. |
| `isLoop` | Indicates a closed chain formed by connecting the first and last vertices specified. |
| `surfaceMaterial` | The surface material for the shape comprising of many properties such as friction, bounciness, rolling resistance etc. |
| `triggerEvents` | Controls whether this chain produces trigger events which can be retrieved after the simulation has completed. This applies to triggers and non-triggers alike. |

### Methods

#### `new()`

Create a default PhysicsChain definition.

#### `new(bool)`

Create a default PhysicsChain definition.

**Params:**
- `useSettings` — Controls whether the default come settings from the physics settings or not.
