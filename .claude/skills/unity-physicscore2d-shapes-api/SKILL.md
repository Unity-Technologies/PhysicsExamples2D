---
name: unity-physicscore2d-shapes-api
description: Authoritative Unity 6000.7 PhysicsCore2D API reference for Shapes. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsShape, PhysicsShapeDefinition. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Shapes

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.7, so it should be treated as the source of truth for member names, signatures, and documentation strings.

Top-level types in this file: `PhysicsShape`, `PhysicsShapeDefinition`.

## PhysicsShape

> A shape is attached to a body and defines an area to which two distinct types of behaviour are handled: - Collision: Contacts between shapes produce a collision response on their respective bodies, assuming their body type is Dynamic. - Trigger: Contacts between shapes do not produce a collision response, only the fact that they're overlapping is reported. An unlimited number of shapes can be attached to a single body, known as a compound body. A shape is automatically destroyed when the body it is attached to is destroyed. A shape cannot exist unattached from a body.

**Full name:** `Unity.U2D.Physics.PhysicsShape`

### Properties

| Name | Summary |
|------|---------|
| `aabb` | Get the world AABB that bounds this shape. The bounds of the shape is inflated slightly due to speculative collision detection. The inflation is smaller on Static shape types however it is not zero due to time-of-impact collision detection. If an exact AABB is required then you can retrieve that via the shape geometry. See CircleGeometry.CalculateAABB, CapsuleGeometry.CalculateAABB, PolygonGeometry.CalculateAABB and SegmentGeometry.CalculateAABB. |
| `body` | The body which the shape is attached to. |
| `bounciness` | The bounciness (coefficient of restitution) usually in the range [0, 1]. Values higher than 1 will result in energy being added which can lead to an unstable simulation. This is assigned to the current PhysicsShape.surfaceMaterial. |
| `bouncinessMixing` | Defines the method used when mixing the friction values of two shapes to form a contact. This is assigned to the current PhysicsShape.surfaceMaterial. |
| `bouncinessPriority` | The priority for combining the PhysicsShape.bounciness properties when two shapes come into contact. If the priority of one shape is higher than the other shape then the higher priority PhysicsShape.SurfaceMaterial.bouncinessMixing will be used. If the priority of both shapes are the same then simply the higher enumeration value of PhysicsMaterialCombine2D from both shapes will be used. This is assigned to the current PhysicsShape.surfaceMaterial. |
| `callbackTarget` | Get/Set the Object that callbacks for this shape will be sent to. Care should be taken with any Object assigned as a callback target that isn't a Object as this assignment will not in itself keep the object alive and can be garbage collected. To avoid this, you should have at least a single reference to the object in your code. To remove the object assigned here, set the callback target to NULL. This includes the following events: - A PhysicsEvents.ContactFilterEvent with call PhysicsCallbacks.IContactFilterCallback. - A PhysicsEvents.PreSolveEvent with call PhysicsCallbacks.IPreSolveCallback. - A PhysicsEvents.TriggerBeginEvent with call PhysicsCallbacks.ITriggerCallback. - A PhysicsEvents.TriggerEndEvent with call PhysicsCallbacks.ITriggerCallback. - A PhysicsEvents.ContactBeginEvent with call PhysicsCallbacks.IContactCallback. - A PhysicsEvents.ContactEndEvent with call PhysicsCallbacks.IContactCallback. |
| `capsuleGeometry` | Get/Set the Capsule associated with this shape. When getting the shape geometry, the shape type must match the geometry type otherwise a warning will be produced and invalid geometry will be returned. Setting the geometry will change the type of shape represented even if the shape type was different before. Setting the geometry will also result in waking the body the shape is attached to. |
| `chain` | Get the owning chain. The type of shape must be PhysicsShape.ShapeType.ChainSegment otherwise a warning will be produced. See PhysicsShape.isChainSegment and PhysicsChain. |
| `chainSegmentGeometry` | Get/Set the Chain Segment associated with this shape. When getting the shape geometry, the shape type must match the geometry type otherwise a warning will be produced and invalid geometry will be returned. Setting the geometry will change the type of shape represented even if the shape type was different before. Setting the geometry will also result in waking the body the shape is attached to. |
| `circleGeometry` | Get/Set the Circle associated with this shape. When getting the shape geometry, the shape type must match the geometry type otherwise a warning will be produced and invalid geometry will be returned. Setting the geometry will change the type of shape represented even if the shape type was different before. Setting the geometry will result in waking the body the shape is attached to. |
| `contactEvents` | Controls whether this shape produces contact events which can be retrieved after the simulation has completed. Any contact events can be used to call the assigned PhysicsShape.callbackTarget. A contact event is produced if either shapes involved have contactEvents enabled. A contact event will produce a PhysicsCallbacks.IContactCallback to the PhysicsShape.callbackTarget for both shapes involved. |
| `contactFilter` | The filter used when determining what contacts this shape participates in. |
| `contactFilterCallbacks` | Controls whether this shape produces contact filter callbacks. A contact filter callback allows direct control over whether a contact will be created between a pair of shapes. This applies to both triggers and non-triggers but only with to Dynamic bodies These are relatively expensive so disabling them can provide a significant performance benefit. A contact filter callback will call the PhysicsShape.callbackTarget for both shapes involved if they implement PhysicsCallbacks.IContactFilterCallback. |
| `customColor` | Custom debug draw color. Any color value other than Color.clear (RGBA=0) will be used to render the shape.. This value is passed back when using the PhysicsWorld drawing. The alpha value here is always ignored. This is assigned to the current PhysicsShape.surfaceMaterial. |
| `definition` | Get/Set a shape definition by accessing all of its current properties. This is provided as convenience only and should not be used when performance is important as all the properties defined in the definition are accessed sequentially. You should try to only use the specific properties you need rather than using this feature. |
| `friction` | The Coulomb (dry) friction coefficient, usually in the range [0, 1]. This is assigned to the current PhysicsShape.surfaceMaterial. |
| `frictionMixing` | Defines the method used when mixing the friction values of two shapes to form a contact. This is assigned to the current PhysicsShape.surfaceMaterial. |
| `frictionPriority` | The priority for combining the PhysicsShape.friction properties when two shapes come into contact. If the priority of one shape is higher than the other shape then the higher priority PhysicsShape.SurfaceMaterial.frictionMixing will be used. If the priority of both shapes are the same then simply the higher enumeration value of PhysicsMaterialCombine2D from both shapes will be used. This is assigned to the current PhysicsShape.surfaceMaterial. |
| `hitEvents` | Controls whether this shape produces hit events which can be retrieved after the simulation has completed. |
| `isChainSegment` | Check if the shape is a Chain type. A Chain type is owned by a chain. See PhysicsShape.chain and PhysicsChain. |
| `isOwned` | Get if the shape is owned. See PhysicsShape.SetOwner. |
| `isTrigger` | Get/Set if the shape is a trigger. Changing the state here is relatively expensive and should be avoided. See PhysicsShapeDefinition.isTrigger. |
| `isValid` | Check if the shape is valid. |
| `localCenter` | Get the center of the shape, in local-space. |
| `massConfiguration` | The shape mass configuration. Normally this only used on a body where the total of all shapes is used. This allows the calculation of this specific shape in isolation. See PhysicsBody.MassConfiguration. |
| `moverData` | The mover data for the shape mover. |
| `owner` | The owner object associated with this shape, or NULL if no owner has been specified. This is a convenience property that returns the same value as PhysicsShape.GetOwner. |
| `ownerUserData` | Get PhysicsUserData that can be used for any purpose, typically by the owner only. |
| `physicsGroup` | The group this shape belongs to. A shape starts with no group and can be assigned a group once only; the group then cannot be changed for the lifetime of the shape. Grouping marks the shape's contact and trigger begin/end events with whether they are the first or last event between the two groups involved. |
| `physicsHandle` | Get the physics handle. |
| `polygonGeometry` | Get/Set the Polygon associated with this shape. When getting the shape geometry, the shape type must match the geometry type otherwise a warning will be produced and invalid geometry will be returned. Setting the geometry will change the type of shape represented even if the shape type was different before. Setting the geometry will also result in waking the body the shape is attached to. |
| `preSolveCallbacks` | Controls whether this shape produces pre-solve callbacks. This only applies to Dynamic bodies and is ignored for triggers. These are relatively expensive so disabling them can provide a significant performance benefit. A pre-solve callback will call the PhysicsShape.callbackTarget for both shapes involved if they implement PhysicsCallbacks.IPreSolveCallback. |
| `rollingResistance` | The rolling resistance usually in the range [0, 1]. This is assigned to the current PhysicsShape.surfaceMaterial. |
| `segmentGeometry` | Get/Set the Segment associated with this shape. When getting the shape geometry, the shape type must match the geometry type otherwise a warning will be produced and invalid geometry will be returned. Setting the geometry will change the type of shape represented even if the shape type was different before. Setting the geometry will also result in waking the body the shape is attached to. |
| `shapeType` | The type of shape. See PhysicsShape.ShapeType. |
| `startMassUpdate` | Should the body update its mass properties when this shape is created. Disabling this improves performance when multiple shapes are being added to the same body. The mass of a body can then be explicitly updated by calling PhysicsBody.ApplyMassFromShapes See PhysicsShapeDefinition.startMassUpdate. |
| `startStaticContacts` | Normally shapes on Static bodies don't create contacts when they are added to the world. This overrides that behavior and causes contact creation. This significantly slows down Static body creation which can be important when there are many Static shapes. This is implicitly always true for Triggers, Dynamic bodies and Kinematic bodies. See PhysicsShapeDefinition.startStaticContacts. |
| `surfaceMaterial` | The surface material for the shape comprising of many properties such as friction, bounciness, rolling resistance etc. Setting the surface material overrides any individual settings for friction, bounciness, rolling resistance etc. |
| `tangentSpeed` | The tangent (surface) speed. This is assigned to the current PhysicsShape.surfaceMaterial. |
| `transform` | Get the shape transform. This is simply the body transform. See PhysicsBody.transform. |
| `triggerEvents` | Controls whether this shape produces triggers events which can be retrieved after the simulation has completed. A trigger event is only produced if both shapes involved have their triggerEvents enabled. A trigger event will produce a PhysicsCallbacks.ITriggerCallback to the PhysicsShape.callbackTarget for both shapes involved. |
| `userData` | Get/Set PhysicsUserData that can be used for any purpose. The physics system doesn't use this data, it is entirely for custom use. |
| `world` | Get the world the shape is attached to. |
| `worldDrawing` | Controls whether this shape is automatically drawn when the world is drawn. |

### Methods

#### `new(PhysicsHandle)`

Create a shape from a physics handle. NOTE: You must ensure that the physics handle represents the correct object type otherwise hard to detect bugs can occur.

**Params:**
- `physicsHandle` — The physics handle to use.

#### `ApplyBuoyancy(PhysicsBody.BuoyancyInput, float)`

Apply buoyancy, flow and damping forces to this shape based on how it is submerged in a fluid plane. Unlike PhysicsBody.ApplyBuoyancy, only this shape contributes to its owning body's buoyancy - sibling shapes on the same body are left alone. Forces and torques are continuous (not impulses), so this is expected to be called every simulation step. The shape's owning body must be PhysicsBody.BodyType.Dynamic; otherwise a warning is logged and the call is a no-op.

**Params:**
- `input` — The fluid and force configuration. See PhysicsBody.BuoyancyInput.
- `deltaTime` — The simulation step duration in seconds. Used to clamp damping so it cannot overshoot in a single step.

#### `ApplyBuoyancy(PhysicsBody.BuoyancyInput, ReadOnlySpan<PhysicsShape>, float)`

Apply buoyancy, flow and damping forces to every shape in shapes based on how each is submerged in a fluid plane. The same PhysicsBody.BuoyancyInput is applied to all listed shapes. Each shape's owning body must be PhysicsBody.BodyType.Dynamic; shapes on non-dynamic or invalid bodies log a warning and are skipped. Per-body sleep/wake decisions and damping clamps aggregate only across the listed shapes for each body, so siblings not in shapes contribute nothing - this is the difference from PhysicsBody.ApplyBuoyancy, which always processes every shape on each listed body. Forces and torques are continuous (not impulses), so this is expected to be called every simulation step.

**Params:**
- `input` — The fluid and force configuration. See PhysicsBody.BuoyancyInput.
- `shapes` — The shapes that buoyancy should be applied to.
- `deltaTime` — The simulation step duration in seconds. Used to clamp damping so it cannot overshoot in a single step.

#### `ApplyWind(PhysicsBody.WindInput)`

Apply wind forces to this shape. Force is computed by Box2D using the shape's projected area, the wind velocity (PhysicsBody.WindInput.force) and the drag/lift coefficients in input; circles ignore lift. Forces are continuous (not impulses), so this is expected to be called every simulation step. The shape's owning body must be PhysicsBody.BodyType.Dynamic; otherwise a warning is logged and the call is a no-op. Sleeping bodies are woken automatically by Box2D when the per-shape force is non-trivial.

**Params:**
- `input` — The wind configuration. See PhysicsBody.WindInput.

#### `ApplyWind(PhysicsBody.WindInput, ReadOnlySpan<PhysicsShape>)`

Apply wind forces to every shape in shapes. The same PhysicsBody.WindInput is applied to all listed shapes. Each shape's owning body must be PhysicsBody.BodyType.Dynamic; shapes on non-dynamic or invalid bodies log a warning and are skipped. Only the listed shapes contribute to wind on their owning bodies; sibling shapes not in shapes are left alone - this is the difference from PhysicsBody.ApplyWind, which processes every shape on each listed body. Forces are continuous (not impulses), so this is expected to be called every simulation step.

**Params:**
- `input` — The wind configuration. See PhysicsBody.WindInput.
- `shapes` — The shapes that wind should be applied to.

#### `CastRay(PhysicsQuery.CastRayInput)`

Check if a ray intersects the shape. See PhysicsQuery.CastResult.

**Params:**
- `castRayInput` — The configuration of the ray to cast.

**Returns:** The intersection details, if any, that were found.

#### `CastShape(PhysicsQuery.CastShapeInput)`

Calculate if a cast shape intersects the shape. Initially touching shapes are treated as a miss. You should check for overlap first if initial overlap is required. See PhysicsQuery.CastShapeInput and PhysicsQuery.CastResult.

**Params:**
- `input` — The cast shape input used to check for intersection.

**Returns:** The results of the intersection test.

#### `ClosestPoint(Vector2)`

Calculate the closest point on this shape to the specified point.

**Params:**
- `point` — The point to check.

**Returns:** The closest point on the shape to the specified point.

#### `CreateShape(PhysicsBody, CircleGeometry)`

Create a Circle shape, using its default definition, attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.

**Returns:** The created shape.

#### `CreateShape(PhysicsBody, CircleGeometry, PhysicsShapeDefinition)`

Create a Circle shape attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShape(PhysicsBody, PolygonGeometry)`

Create a Polygon shape, using its default definition, attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.

**Returns:** The created shape.

#### `CreateShape(PhysicsBody, PolygonGeometry, PhysicsShapeDefinition)`

Create a Polygon shape attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShape(PhysicsBody, CapsuleGeometry)`

Create a Capsule shape, using its default definition, attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.

**Returns:** The created shape.

#### `CreateShape(PhysicsBody, CapsuleGeometry, PhysicsShapeDefinition)`

Create a Capsule shape attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShape(PhysicsBody, SegmentGeometry)`

Create a Segment shape, using its default definition, attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.

**Returns:** The created shape.

#### `CreateShape(PhysicsBody, SegmentGeometry, PhysicsShapeDefinition)`

Create a Segment shape attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShape(PhysicsBody, ChainSegmentGeometry, PhysicsShapeDefinition)`

Create a Chain Segment shape attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.

**Returns:** The created shape.

#### `CreateShape(PhysicsBody, ChainSegmentGeometry)`

Create a Chain Segment shape, using its default definition, attached to the specified body. See PhysicsBody.CreateShape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.

**Returns:** The created shape.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<CircleGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Circle shapes attached to the specified body.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<CircleGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Circle shapes attached to the specified body, assigning the given owner to every created shape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<PolygonGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Polygon shapes attached to the specified body.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<PolygonGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Polygon shapes attached to the specified body, assigning the given owner to every created shape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<CapsuleGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Capsule shapes attached to the specified body.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<CapsuleGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Capsule shapes attached to the specified body, assigning the given owner to every created shape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<SegmentGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Segment shapes attached to the specified body.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<SegmentGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Segment shapes attached to the specified body, assigning the given owner to every created shape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<ChainSegmentGeometry>, PhysicsShapeDefinition, Unity.Collections.Allocator)`

Create a batch of Chain Segment shapes attached to the specified body.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeBatch(PhysicsBody, ReadOnlySpan<ChainSegmentGeometry>, PhysicsShapeDefinition, Object, int, Unity.Collections.Allocator)`

Create a batch of Chain Segment shapes attached to the specified body, assigning the given owner to every created shape.

**Params:**
- `body` — The body to attach the shape to.
- `geometry` — The shape geometry to use.
- `definition` — The shape definition to use.
- `owner` — The owner object to assign to every created shape. If null, no owner is assigned.
- `ownerKey` — The owner key to assign. Must be non-zero when owner is not null. See PhysicsWorld.CreateOwnerKey.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created shapes. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateShapeProxy(bool)`

Create a shape proxy from the shape.

**Params:**
- `useWorldSpace` — Whether to create the shape proxy in world-space or not. World-space will transform by the body origin the shape is attached to.

#### `CreateShapeProxy(PhysicsTransform)`

Create a shape proxy from the shape, transformed by the specified transform.

**Params:**
- `transform` — The transform used to position the shape geometry.

#### `CreateShapeProxy(Matrix4x4, bool)`

Create a shape proxy from the shape, transformed by the specified transform. The maximum absolute value component from the scale will be used to scale the radius when scaleRadius is true. Shape types without a radius ignore the flag.

**Params:**
- `transform` — The transform used to position the shape geometry.
- `scaleRadius` — Whether to scale the radius of the shape. Only affects shape types that carry a radius (Circle, Capsule, Polygon).

#### `Destroy(bool, int)`

Destroy the shape, destroying all PhysicsShape.Contact the shape is involved in. If the object is owned with PhysicsShape.SetOwner then you must provide the owner key it returned. Failing to do so will return a warning and the shape will not be destroyed. The lifetime of the specified owner object is not linked to this shape i.e. this shape will still be owned by the owner object, even if it is destroyed. Shapes of type Chain cannot be destroyed here, they must be destroyed by their owning chain. See PhysicsChain and PhysicsBody.MassConfiguration.

**Params:**
- `updateBodyMass` — Optional flag indicating if the body mass configuration should be updated. Not doing so is faster, especially when destroying multiple shapes.
- `ownerKey` — Optional owner key returned when using PhysicsShape.SetOwner.

**Returns:** If the shape was destroyed or not.

#### `DestroyBatch(ReadOnlySpan<PhysicsShape>, bool)`

Destroy a batch of shapes, destroying all PhysicsShape.Contact the shapes are involved in. Any invalid shapes will be ignored including chain segment shapes created via a PhysicsChain (the chain must be destroyed)." Owned shapes will produce a warning and will not be destroyed (See PhysicsShape.SetOwner). See PhysicsBody.MassConfiguration.

**Params:**
- `shapes` — The shapes to destroy.
- `updateBodyMass` — Whether to update the body mass configuration. Not doing so is faster, especially when destroying multiple shapes.

#### `DestroyBatch(ReadOnlySpan<PhysicsShape>, bool, int)`

Destroy a batch of shapes, destroying all contacts the shapes are involved in. Any invalid shapes are ignored, including chain segment shapes created by a chain, which must be destroyed through their owning chain instead. A shape owned by a different owner key is skipped and left valid; a shape with no owner, or one matching the given owner key, is destroyed. One summary warning reports how many shapes were skipped this way, rather than one warning per shape.

**Params:**
- `shapes` — The shapes to destroy.
- `updateBodyMass` — Whether to update the body mass configuration. Not doing so is faster, especially when destroying multiple shapes.
- `ownerKey` — Optional owner key returned when using PhysicsShape.SetOwner.

#### `Distance(PhysicsShape, bool)`

Get the minimum distance between this shape and the specified shape.

**Params:**
- `otherShape` — The other shape to check the distance of.
- `useRadii` — Whether to use the radii of both shapes or not.

**Returns:** The distance result.

#### `Distance(PhysicsShape, PhysicsTransform, bool)`

Get the minimum distance between this shape and the specified shape.

**Params:**
- `otherShape` — The other shape to check the distance of.
- `otherTransform` — The transform used to specify where the other shape is positioned.
- `useRadii` — Whether to use the radii of both shapes or not.

**Returns:** The distance result.

#### `Distance(ReadOnlySpan<PhysicsShape>, bool)`

Get the minimum distance between this shape and the specified shape(s) span.

**Params:**
- `otherShapes` — A read-only span of the other shape to check the distance of.
- `useRadii` — Whether to use the radii of both shapes or not.

**Returns:** The distance result.

#### `Distance(PhysicsBody, bool)`

Get the minimum distance between this shape and all the shapes attached to the specified body.

**Params:**
- `physicsBody` — The body whose attached shape(s) will be used to check the distance of.
- `useRadii` — Whether to use the radii of all shapes or not.

#### `Draw()`

Draw the PhysicsShape that visually represents its current state in the world.

#### `Equals(object)`

#### `Equals(PhysicsShape)`

#### `GetContacts(Unity.Collections.Allocator)`

Get all the touching contacts this shape is currently participating in.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The touching contacts this shape is currently participating in. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetDensity()`

Get the shape density. See PhysicsBody.massConfiguration.

**Returns:** The density of the shape.

#### `GetHashCode()`

#### `GetOwner()`

Get the owner object associated with this shape as specified using PhysicsShape.SetOwner.

**Returns:** The owner object associated with this shape or NULL if no owner has been specified.

#### `GetPerimeter()`

Get the length of the perimeter of the shape.

**Returns:** The length of the perimeter of the shape.

#### `GetPerimeterProjected(Vector2)`

Get the length of the perimeter of the shape projected onto the specified axis.

**Params:**
- `axis` — The axis to project the perimeter of the shape.

**Returns:** The length of the perimeter of the shape projected onto the specified axis.

#### `GetTriggerVisitors(Unity.Collections.Allocator)`

Get all the trigger visitors for this shape. The shape must be a trigger, if not, no visitors will be returned.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The trigger visitors for this shape. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `Intersect(PhysicsShape)`

Check the intersection between this shape and another shape.

**Params:**
- `otherShape` — The other shape used to check intersection against.

**Returns:** The contact manifold fully detailing the intersection.

#### `Intersect(PhysicsTransform, PhysicsShape, PhysicsTransform)`

Check the intersection between this shape and another shape.

**Params:**
- `transform` — The transform used to specify where this shape is positioned.
- `otherShape` — The other shape used to check intersection against.
- `otherTransform` — The transform used to specify where the other shape is positioned.

**Returns:** The contact manifold fully detailing the intersection.

#### `operator implicit(PhysicsShape)`

#### `OverlapPoint(Vector2)`

Check if a point intersects the shape. This will only work on "closed" shapes. SeePhysicsShape.ShapeType.

**Params:**
- `point` — The world point to check.

**Returns:** Whether an intersection was found or not.

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, ReadOnlySpan<CircleGeometry>)`

Set a batch of PhysicsShape to the corresponding CircleGeometry. The two spans must be the same length; shape[i] is set to geometry[i]. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes or geometry in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on each corresponding shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid).

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, ReadOnlySpan<CapsuleGeometry>)`

Set a batch of PhysicsShape to the corresponding CapsuleGeometry. The two spans must be the same length; shape[i] is set to geometry[i]. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes or geometry in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on each corresponding shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid).

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, ReadOnlySpan<PolygonGeometry>)`

Set a batch of PhysicsShape to the corresponding PolygonGeometry. The two spans must be the same length; shape[i] is set to geometry[i]. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes or geometry in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on each corresponding shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid).

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, ReadOnlySpan<SegmentGeometry>)`

Set a batch of PhysicsShape to the corresponding SegmentGeometry. The two spans must be the same length; shape[i] is set to geometry[i]. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes or geometry in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on each corresponding shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid).

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, ReadOnlySpan<ChainSegmentGeometry>)`

Set a batch of PhysicsShape to the corresponding ChainSegmentGeometry. The two spans must be the same length; shape[i] is set to geometry[i]. If a shape is already a chain segment, its owning chain is preserved. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes or geometry in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on each corresponding shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid).

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, CircleGeometry)`

Set a batch of PhysicsShape all to the same CircleGeometry. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on every shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid). If the geometry is invalid, no shapes are set.

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, CapsuleGeometry)`

Set a batch of PhysicsShape all to the same CapsuleGeometry. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on every shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid). If the geometry is invalid, no shapes are set.

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, PolygonGeometry)`

Set a batch of PhysicsShape all to the same PolygonGeometry. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on every shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid). If the geometry is invalid, no shapes are set.

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, SegmentGeometry)`

Set a batch of PhysicsShape all to the same SegmentGeometry. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on every shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid). If the geometry is invalid, no shapes are set.

#### `SetBatchGeometry(ReadOnlySpan<PhysicsShape>, ChainSegmentGeometry)`

Set a batch of PhysicsShape all to the same ChainSegmentGeometry. If a shape is already a chain segment, its owning chain is preserved. Like the single geometry setters, this updates the shape type and broadphase but does not update body mass. Call PhysicsBody.ApplyMassFromShapes on any affected body if a mass update is required. Invalid shapes in the batch are ignored. For best performance, the shapes should all be part of the same PhysicsWorld, sorted by world otherwise.

**Params:**
- `shapes` — The shapes to set.
- `geometry` — The geometry to set on every shape.

**Returns:** The number of shapes that were ignored (not set because the shape was invalid). If the geometry is invalid, no shapes are set.

#### `SetDensity(float, bool)`

Set the shape density. See PhysicsBody.massConfiguration.

**Params:**
- `density` — The density to set.
- `updateBodyMass` — Whether to update the body mass configuration. Not doing so is faster, especially when setting multiple shapes.

#### `SetOwner(ReadOnlySpan<PhysicsShape>, Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.

**Params:**
- `shapes` — The shapes to set ownership for.
- `owner` — The object that owns this key. Whilst it is valid to not specify an owner object (NULL), it is recommended for debugging purposes.
- `ownerKey` — The owner key to be used. If zero then a new owner key is derived from the owner. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwner(Object, int)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.
- `ownerKey` — The owner key to be used. If zero then a new owner key is created. You can use PhysicsWorld.CreateOwnerKey for this value although any non-zero integer will work.

#### `SetOwner(Object)`

Set the owner object using the specified owner key. You can only set the owner once, multiple attempts will produce a warning. This call does not bind the lifetime of the specified owner object, it is simply a reference. It is also valid to not specify an owner object (NULL) to simply gain an owner key however it can be useful, if simply for debugging purposes and discovery, to know which object is the owner.

**Params:**
- `owner` — The object that owns this key. This can be NULL if not required but is recommended as the key is formed in part by the hash-code of the owner object.

**Returns:** The owner key assigned.

#### `SetOwnerUserData(PhysicsUserData, int)`

Set PhysicsUserData that can be used for any purpose, typically by the owner only.

**Params:**
- `physicsUserData` — The user data to set.
- `ownerKey` — Optional owner key returned when using PhysicsShape.SetOwner.

#### `SetOwnerUserData(ReadOnlySpan<PhysicsShape>, ReadOnlySpan<PhysicsUserData>, int)`

Set PhysicsUserData on a batch of shapes that can be used for any purpose, typically by the owner only. The shapes and userDatas spans must be the same length; shapes[n] receives userDatas[n].

**Params:**
- `shapes` — The shapes to set the owner user data on.
- `userDatas` — The user data to set, one entry per shape.
- `ownerKey` — Optional owner key returned when using PhysicsShape.SetOwner.

#### `ToString()`

### Nested Types

- **Contact** — The contact between two shapes. By convention the manifold normal points from shape A to shape B. See PhysicsBody.GetContacts and PhysicsShape.GetContacts.
- **ContactFilter** — A contact filter is used to control what contacts are created when intersecting other shapes. A contact filter contains a filter with the addition of a group index allowing overrides to the filter. See PhysicsShape.ContactFilterMode.
- **ContactFilterGroupMode** — The mode used to determine how PhysicsShape.ContactFilter.groupIndex is used.
- **ContactFilterMode** — The mode used for the PhysicsShape.ContactFilter when determining if two PhysicsShape can contact. See PhysicsCoreSettings2D.contactFilterMode.
- **ContactId** — The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity.
- **ContactManifold** — A contact manifold describes the contact points between colliding shapes. Speculative collision is used so some contact points may be separated, a property available per-contact.
- **MoverCollision** — Collision results optionally returned from PhysicsWorld.CastMover in PhysicsQuery.WorldMoverResult.
- **MoverData** — The mover data assigned to a PhysicsShape.moverData. This is used when PhysicsShape are encountered when using PhysicsWorld.CastMover.
- **ShapeArray** — Fixed vertex shape array.
- **ShapeProxy** — A proxy of a shape in a generic form suited to representing all support shape types. You can provide between 1 and PhysicsConstants.MaxPolygonVerticesand a radius. - A CircleGeometry is a single point with a non-zero radius (zero radius is allowed however and defines a point). - A CapsuleGeometry is two points with a non-zero radius. - A PolygonGeometry box is the points with and an optional radius. - A SegmentGeometry is two points with a zero radius. - A ChainSegmentGeometry is two points with a zero radius. To create a proxy, simply provide the geometry to the constructor.
- **ShapeType** — The type of shape. Some shapes are "closed" meaning they have an interior which will produce contacts. Some shapes are "open" meaning they do not have an interior and will only produce contacts when their boundary is intersected.
- **SurfaceMaterial** — Defines the dynamics of a surface on a shape.

### Contact

> The contact between two shapes. By convention the manifold normal points from shape A to shape B. See PhysicsBody.GetContacts and PhysicsShape.GetContacts.

**Full name:** `Unity.U2D.Physics.PhysicsShape.Contact`

#### Properties

| Name | Summary |
|------|---------|
| `contactId` | The unique Id of this contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity with PhysicsShape.ContactId.isValid. |
| `manifold` | The contact manifold describing the contact. |
| `shapeA` | One of the shapes involved in the contact. |
| `shapeB` | The other shape involved in the contact. |

### ContactFilter

> A contact filter is used to control what contacts are created when intersecting other shapes. A contact filter contains a filter with the addition of a group index allowing overrides to the filter. See PhysicsShape.ContactFilterMode.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ContactFilter`

#### Fields

| Name | Summary |
|------|---------|
| `DefaultCategories` | The default categories used. |
| `DefaultContacts` | The default contacts used. |
| `defaultFilter` | Get a default contact filter that contacts everything. |
| `Everything` | Get a contact filter that is all categories and contacts everything. |

#### Properties

| Name | Summary |
|------|---------|
| `categories` | The categories this object is in. Usually you would only set one bit but multiple are allowed. |
| `contacts` | The categories this object will produce contacts with. |
| `groupIndex` | The group which the contact filter uses to determine if the categories and contact masks are used. See PhysicsShape.ContactFilterGroupMode for more information. |

#### Methods

##### `new(PhysicsMask, PhysicsMask, int)`

Create a contact filter. See PhysicsShape.ContactFilterMode.

**Params:**
- `categories` — A PhysicsMask defining the categories this object is in.
- `contacts` — A PhysicsMask defining the categories this object will produce contacts with.
- `groupIndex` — The group index this filter belongs to. How this is used is determined by PhysicsShape.ContactFilterGroupMode.

##### `CanContact(PhysicsShape.ContactFilter)`

Will this contact filter produce a contact with the specified contact filter. The term "contact" here means that if these filters were used on two PhysicsShape, would a contact be produced.

**Params:**
- `filter` — The other contact filter to compare against.

**Returns:** Whether a contact would be produced by both contact filters or not.

### ContactFilterGroupMode

> The mode used to determine how PhysicsShape.ContactFilter.groupIndex is used.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ContactFilterGroupMode`

#### Fields

| Name | Summary |
|------|---------|
| `Filtering` | In this mode, the PhysicsShape.ContactFilter.groupIndex is used to filter if contacts are allowed to be created by the PhysicsShape.ContactFilter.categories and PhysicsShape.ContactFilter.contacts masks. The rules for two shapes coming into contact are: - If both shapes have an identical group then the PhysicsShape.ContactFilter.categories and PhysicsShape.ContactFilter.contacts masks are used. - If both shapes have a different group then they will never produce a contact irrelevant of the PhysicsShape.ContactFilter.categories and PhysicsShape.ContactFilter.contacts mask configuration. - A group of zero is used like any other group but is also the default therefore if unchanged, the PhysicsShape.ContactFilter.categories and PhysicsShape.ContactFilter.contacts masks are used by default. |
| `Group` | In this mode, the PhysicsShape.ContactFilter.groupIndex is used to control if contacts are never created (negative) or always created (positive). A non-zero group always overrides the PhysicsShape.ContactFilter.categories and PhysicsShape.ContactFilter.contacts masks. A group of zero has no effect. The rules for two shapes coming into contact are: - If either shape has a group of zero then the group is ignored and the PhysicsShape.ContactFilter.categories and PhysicsShape.ContactFilter.contacts masks are used. - If both shapes have a non-zero but different group then the PhysicsShape.ContactFilter.categories and PhysicsShape.ContactFilter.contacts masks are used. - If both shapes have an identical and positive group then they will always produce a contact. - If both shapes have an identical and negative group then they will never produce a contact. |

### ContactFilterMode

> The mode used for the PhysicsShape.ContactFilter when determining if two PhysicsShape can contact. See PhysicsCoreSettings2D.contactFilterMode.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ContactFilterMode`

#### Fields

| Name | Summary |
|------|---------|
| `Both` | This mode will produce a contact if both PhysicsShape agree, effectively an AND operation. A contact will be produced if the following is true: (contactFilterA.contacts AND-MASK contactFilterB.categories) AND (contactFilterA.categories AND-MASK contactFilterB.contacts). How the PhysicsShape.ContactFilter.groupIndex is used is determined by PhysicsShape.ContactFilterGroupMode. |
| `Either` | This mode will produce a contact if either PhysicsShape agree, effectively an OR operation. A contact will be produced if the following is true: (contactFilterA.contacts AND-MASK contactFilterB.categories ) OR (contactFilterA.categories AND-MASK contactFilterB.contacts). How the PhysicsShape.ContactFilter.groupIndex is used is determined by PhysicsShape.ContactFilterGroupMode. |

### ContactId

> The unique Id of the contact. This contact is volatile and may be destroyed automatically when the world is modified or simulated therefore it should always be checked for validity.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ContactId`

#### Properties

| Name | Summary |
|------|---------|
| `contact` | Get the contact. |
| `isValid` | Check if the contact is valid or not. |

#### Methods

##### `ToString()`

### ContactManifold

> A contact manifold describes the contact points between colliding shapes. Speculative collision is used so some contact points may be separated, a property available per-contact.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ContactManifold`

#### Properties

| Name | Summary |
|------|---------|
| `normal` | The unit normal vector in world space, points from shape A to bodyB |
| `pointCount` | The number of manifold points available, in the range [0, 2]. |
| `points` | The manifold points, up to two are possible. |
| `rollingImpulse` | Angular impulse applied for rolling resistance (N * m * s = kg * m^2 / s). |
| `speculativePointCount` | The number of manifold points available that are speculative, in the range [0, 2]. |

#### Methods

##### `GetEnumerator()`

#### Nested Types

- **ManifoldPoint** — Contains all the detail related to the geometry and dynamics of the contact. You may use the PhysicsShape.ContactManifold.ManifoldPoint.totalNormalImpulse to determine if there was an interaction during the time step.
- **ManifoldPointArray** — Fixed-sized manifold point array.
- **ManifoldPointIterator** — —

#### ManifoldPoint

> Contains all the detail related to the geometry and dynamics of the contact. You may use the PhysicsShape.ContactManifold.ManifoldPoint.totalNormalImpulse to determine if there was an interaction during the time step.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPoint`

##### Properties

| Name | Summary |
|------|---------|
| `anchorA` | Location of the contact point relative to shapeA's origin in world space. |
| `anchorB` | Location of the contact point relative to shapeB's origin in world space. |
| `id` | Uniquely identifies a contact point between two shapes. This should not be confused with PhysicsShape.ContactId. |
| `normalImpulse` | The impulse along the manifold normal vector. |
| `normalVelocity` | Relative normal velocity pre-solve. Used for hit events. If the normal impulse is zero then there was no hit. Negative means shapes are approaching. |
| `persisted` | Did this contact point exist the previous step? |
| `point` | Location of the contact point in world space. Subject to precision loss at large coordinates. This point lags behind when contact recycling is used. Preference should be to use anchorA and/or anchorB for game logic. This is also known as the "clip" point. |
| `separation` | The separation of the contact point, negative if penetrating. |
| `speculative` | Is the contact point speculative i.e. not currently interacting? |
| `tangentImpulse` | The friction impulse. |
| `totalNormalImpulse` | The total normal impulse applied across sub-stepping and restitution. This includes the warm starting impulse, the sub-step delta impulse, and the restitution impulse. This can be used to identify speculative contact points that had an interaction during the simulation step. |

#### ManifoldPointArray

> Fixed-sized manifold point array.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPointArray`

##### Properties

| Name | Summary |
|------|---------|
| `contactInfo0` | Manifold Point #0. |
| `contactInfo1` | Manifold Point #1. |
| `speculativePointCount` | The number of manifold points available that are speculative, in the range [0, 2]. |

#### ManifoldPointIterator

**Full name:** `Unity.U2D.Physics.PhysicsShape.ContactManifold.ManifoldPointIterator`

##### Methods

###### `new(PhysicsShape.ContactManifold)`

### MoverCollision

> Collision results optionally returned from PhysicsWorld.CastMover in PhysicsQuery.WorldMoverResult.

**Full name:** `Unity.U2D.Physics.PhysicsShape.MoverCollision`

#### Properties

| Name | Summary |
|------|---------|
| `normal` | The collision normal at the collision point on the shape. |
| `point` | The collision point on the shape. |
| `shape` | The shape the mover collided with. |

### MoverData

> The mover data assigned to a PhysicsShape.moverData. This is used when PhysicsShape are encountered when using PhysicsWorld.CastMover.

**Full name:** `Unity.U2D.Physics.PhysicsShape.MoverData`

#### Properties

| Name | Summary |
|------|---------|
| `clipVelocity` | Controls if this shape can clip the mover velocity. |
| `pushLimit` | Controls the amount this shape can push against a mover. To effectively set no limit, use Single.MaxValue. |

#### Methods

##### `new()`

Create a default mover data.

### ShapeArray

> Fixed vertex shape array.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ShapeArray`

#### Properties

| Name | Summary |
|------|---------|
| `vertex0` | Vertex #0. |
| `vertex1` | Vertex #1. |
| `vertex2` | Vertex #2. |
| `vertex3` | Vertex #3. |
| `vertex4` | Vertex #4. |
| `vertex5` | Vertex #5. |
| `vertex6` | Vertex #6. |
| `vertex7` | Vertex #7. |

#### Methods

##### `new(ReadOnlySpan<Vector2>)`

Construct with the specified vertices.

**Params:**
- `vertices` — The vertices to set.

##### `AsReadOnlySpan(int)`

Get the shape array as a read-only span, callable from a readonly context without copying.

**Params:**
- `count` — The number of shape array elements to return.

**Returns:** The read-only span representing the shape array.

##### `AsSpan(int)`

Get the shape array as a span.

**Params:**
- `count` — The number of shape array elements to return.

**Returns:** The span representing the shape array.

### ShapeProxy

> A proxy of a shape in a generic form suited to representing all support shape types. You can provide between 1 and PhysicsConstants.MaxPolygonVerticesand a radius. - A CircleGeometry is a single point with a non-zero radius (zero radius is allowed however and defines a point). - A CapsuleGeometry is two points with a non-zero radius. - A PolygonGeometry box is the points with and an optional radius. - A SegmentGeometry is two points with a zero radius. - A ChainSegmentGeometry is two points with a zero radius. To create a proxy, simply provide the geometry to the constructor.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ShapeProxy`

#### Properties

| Name | Summary |
|------|---------|
| `aabb` | Get the AABB that bounds the shape proxy. |
| `capsuleGeometry` | Get a CapsuleGeometry from the shape proxy. The PhysicsShape.ShapeProxy.count must be 2. |
| `circleGeometry` | Get a CircleGeometry from the shape proxy. The PhysicsShape.ShapeProxy.count must be 1. |
| `count` | The number of vertices. |
| `isValid` | Check if the shape proxy is valid. |
| `polygonGeometry` | Get a PolygonGeometry from the shape proxy. |
| `radius` | The radius around the vertices. |
| `segmentGeometry` | Get a SegmentGeometry from the shape proxy. The PhysicsShape.ShapeProxy.count must be 2. |
| `shapeType` | The shape type represented. |
| `vertices` | The shape vertices. |

#### Methods

##### `new(Vector2)`

Create a shape proxy representing a single point.

**Params:**
- `point` — The point to represent.

##### `new(CircleGeometry)`

Create a shape proxy using the specified Circle.

**Params:**
- `circleGeometry` — The geometry to use.

##### `new(CapsuleGeometry)`

Create a shape proxy using the specified Capsule.

**Params:**
- `capsuleGeometry` — The geometry to use.

##### `new(PolygonGeometry)`

Create a shape proxy using the specified Polygon.

**Params:**
- `polygonGeometry` — The geometry to use.

##### `new(SegmentGeometry)`

Create a shape proxy using the specified Segment.

**Params:**
- `segmentGeometry` — The geometry to use.

##### `new(ChainSegmentGeometry)`

Create a shape proxy using the specified ChainSegment.

**Params:**
- `chainSegmentGeometry` — The geometry to use.

##### `AsSpan()`

Get the convex-hull vertices as a span.

**Returns:** The span representing the vertices in the convex-hull.

### ShapeType

> The type of shape. Some shapes are "closed" meaning they have an interior which will produce contacts. Some shapes are "open" meaning they do not have an interior and will only produce contacts when their boundary is intersected.

**Full name:** `Unity.U2D.Physics.PhysicsShape.ShapeType`

#### Fields

| Name | Summary |
|------|---------|
| `Capsule` | A capsule is an extruded circle. This is a closed shape. See CapsuleGeometry. |
| `ChainSegment` | A Chain of line segments that are joined together with other line segments. This is an open shape. This is indirectly created and owned by a chain. See ChainSegmentGeometry and ChainGeometry. |
| `Circle` | A circle with an offset. This is a closed shape. See CircleGeometry. |
| `Polygon` | A convex polygon. This is a closed shape. See PolygonGeometry. |
| `Segment` | A line segment. This is an open shape. See SegmentGeometry. |

### SurfaceMaterial

> Defines the dynamics of a surface on a shape.

**Full name:** `Unity.U2D.Physics.PhysicsShape.SurfaceMaterial`

#### Properties

| Name | Summary |
|------|---------|
| `bounciness` | The bounciness (coefficient of restitution) usually in the range [0, 1]. |
| `bouncinessMixing` | Defines the method used when mixing the bounciness values of two shapes to form a contact. |
| `bouncinessPriority` | The priority for mixing the PhysicsShape.bounciness properties when two shapes come into contact. If the priority of one shape is higher than the other shape then the higher priority PhysicsShape.SurfaceMaterial.bouncinessMixing will be used. If the priority of both shapes are the same then simply the higher enumeration value of PhysicsShape.SurfaceMaterial.MixingMode from both shapes will be used. |
| `customColor` | Custom debug draw color. Any color value other than (0,0,0,0) will be used to render the shape. This value is passed back when using the PhysicsWorld drawing. The alpha value here is always ignored. |
| `defaultMaterial` | Get the default surface material. |
| `friction` | The Coulomb (dry) friction coefficient, usually in the range [0, 1]. |
| `frictionMixing` | Defines the method used when mixing the friction values of two shapes to form a contact. |
| `frictionPriority` | The priority for mixing the PhysicsShape.friction properties when two shapes come into contact. If the priority of one shape is higher than the other shape then the higher priority PhysicsShape.SurfaceMaterial.frictionMixing will be used. If the priority of both shapes are the same then simply the higher enumeration value of PhysicsShape.SurfaceMaterial.MixingMode from both shapes will be used. |
| `rollingResistance` | The rolling resistance usually in the range [0, 1]. |
| `tangentSpeed` | The tangent (surface) speed. |

#### Methods

##### `new()`

Create a default surface material.

##### `new(bool)`

Create a default surface material.

**Params:**
- `useSettings` — Controls whether the default come settings from the physics settings or not.

#### Nested Types

- **MixingMode** — The method used to mix friction or bounciness values.

#### MixingMode

> The method used to mix friction or bounciness values.

**Full name:** `Unity.U2D.Physics.PhysicsShape.SurfaceMaterial.MixingMode`

##### Fields

| Name | Summary |
|------|---------|
| `Average` | The average of both surface values. |
| `Maximum` | The maximum of both surface values. |
| `Mean` | The geometric mean of both surface values. |
| `Minimum` | The minium of both surface values. |
| `Multiply` | The product of both surface values. |

## PhysicsShapeDefinition

> A PhysicsShape definition used to specify important initial properties.

**Full name:** `Unity.U2D.Physics.PhysicsShapeDefinition`

### Fields

| Name | Summary |
|------|---------|
| `contactFilter` | The contact filter used to control which contacts this shape can participate in. |
| `moverData` | The mover data used for the shape mover. |
| `surfaceMaterial` | The surface material for the shape comprising of many properties such as friction, bounciness, rolling resistance etc. |

### Properties

| Name | Summary |
|------|---------|
| `contactEvents` | Controls whether this shape produces contact events which can be retrieved after the simulation has completed. This only applies to kinematic and dynamic bodies. A contact event is produced if either shapes involved have their contactEvents enabled. Changing this at run-time may lead to lost begin/end events. |
| `contactFilterCallbacks` | Controls whether this shape produces contact filter callbacks. A contact filter callback allows direct control over whether a contact will be created between a pair of shapes. This applies to both triggers and non-triggers but only with to Dynamic bodies These are relatively expensive so disabling them can provide a significant performance benefit. A contact filter callback will call the PhysicsShape.callbackTarget for both shapes involved if they implement PhysicsCallbacks.IContactFilterCallback. |
| `defaultDefinition` | Get a default PhysicsShape definition. |
| `density` | The density, usually in kg/m^2, defaults to 1. This is not part of the surface material because this is for the interior of the shape, which may have other considerations, such as being hollow. |
| `hitEvents` | Controls whether this shape produces hit events which can be retrieved after the simulation has completed. This only applies to kinematic and dynamic bodies. This is ignored for triggers. |
| `isTrigger` | Enable/Disable being a trigger shape. A trigger shape generates overlap events but never generates a collision response. Triggers do not collide with other triggers and do not have continuous collision, instead, use a ray or shape cast for those scenarios. Triggers still contribute to the body mass if they have non-zero density. The default is false. |
| `preSolveCallbacks` | Controls whether this shape produces pre-solve callbacks. This only applies to Dynamic bodies and is ignored for triggers. These are relatively expensive so disabling them can provide a significant performance benefit. A pre-solve callback will call the PhysicsShape.callbackTarget for both shapes involved if they implement PhysicsCallbacks.IPreSolveCallback. |
| `startMassUpdate` | Should the body update its mass properties when this shape is created. Disabling this improves performance when multiple shapes are being added to the same body. The mass of a body can then be explicitly updated by calling PhysicsBody.ApplyMassFromShapes See PhysicsShape.startMassUpdate. |
| `startStaticContacts` | Normally shapes on Static bodies don't create contacts when they are added to the world. This overrides that behavior and causes contact creation. This significantly slows down Static body creation which can be important when there are many Static shapes. This is implicitly always true for Triggers, Dynamic bodies and Kinematic bodies. See PhysicsShape.startStaticContacts. |
| `triggerEvents` | Controls whether this shape produces trigger events which can be retrieved after the simulation has completed. A trigger event is only produced if both shapes involved have their triggerEvents enabled. This applies to triggers and non-triggers alike. |
| `worldDrawing` | Controls whether this shape is automatically drawn when the world is drawn. See PhysicsShape.worldDrawing. |

### Methods

#### `new()`

Create a default PhysicsShape definition.

#### `new(bool)`

Create a default PhysicsShape definition.

**Params:**
- `useSettings` — Controls whether the default come settings from the physics settings or not.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
