---
name: unity-physicscore2d-misc-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for Miscellaneous. Lists every type, property, field, method (with signatures, params, returns) for: IPhysicsJoint, PhysicsBuffer, PhysicsBufferPair, PhysicsEditorOnly, PhysicsTransformTweener, PhysicsTransformWatcher, PhysicsTransformWriter, PhysicsWorldRenderer. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Miscellaneous

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `IPhysicsJoint`, `PhysicsBuffer`, `PhysicsBufferPair`, `PhysicsEditorOnly`, `PhysicsTransformTweener`, `PhysicsTransformWatcher`, `PhysicsTransformWriter`, `PhysicsWorldRenderer`.

## IPhysicsJoint

> Defines a common joint interface. This is a helper implementation interface (used for commonality/consistency) and should not be used to access a joint.

**Full name:** `Unity.U2D.Physics.IPhysicsJoint`  
**Docs:** [Unity.U2D.Physics.IPhysicsJoint](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.IPhysicsJoint.html)

### Properties

| Name | Summary |
|------|---------|
| `bodyA` | — |
| `bodyB` | — |
| `callbackTarget` | — |
| `collideConnected` | — |
| `currentAngularSeparationError` | — |
| `currentConstraintForce` | — |
| `currentConstraintTorque` | — |
| `currentLinearSeparationError` | — |
| `drawScale` | — |
| `forceThreshold` | — |
| `isOwned` | — |
| `isValid` | — |
| `jointType` | — |
| `localAnchorA` | — |
| `localAnchorB` | — |
| `ownerUserData` | — |
| `torqueThreshold` | — |
| `tuningDamping` | — |
| `tuningFrequency` | — |
| `userData` | — |
| `world` | — |
| `worldDrawing` | — |

### Methods

#### `Destroy(int)`

#### `Draw()`

#### `GetOwner()`

#### `SetOwner(Object, int)`

#### `SetOwner(Object)`

#### `SetOwnerUserData(PhysicsUserData, int)`

#### `WakeBodies()`

## PhysicsBuffer

> Internal buffer used to marshal results efficiently from the native engine.

**Full name:** `Unity.U2D.Physics.Scripting2D.PhysicsBuffer`  

### Properties

| Name | Summary |
|------|---------|
| `allocator` | — |
| `buffer` | — |
| `isEmpty` | — |
| `isValid` | — |
| `size` | — |

### Methods

#### `new()`

#### `new(IntPtr, int, Unity.Collections.Allocator)`

#### `As``1(int)`

#### `AsEngineObject``1(int)`

#### `Dispose()`

This should NOT be called if a NativeArray or Span are currently active and being accessed otherwise bad things will happen. Typically, the NativeArray should be disposed of but in other cases, this can be used.

#### `FromNativeArray``1(Unity.Collections.NativeArray{`})`

#### `FromSpan``1(ReadOnlySpan{`})`

#### `ToNativeArray``1()`

#### `ToReadOnlySpan``1()`

#### `ToSpan``1()`

#### `ToString()`

## PhysicsBufferPair

> Internal buffer pair used to marshal results efficiently from the native engine.

**Full name:** `Unity.U2D.Physics.Scripting2D.PhysicsBufferPair`  

### Fields

| Name | Summary |
|------|---------|
| `buffer1` | — |
| `buffer2` | — |

## PhysicsEditorOnly

**Full name:** `Unity.U2D.Physics.PhysicsEditorOnly`  
**Docs:** [Unity.U2D.Physics.PhysicsEditorOnly](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsEditorOnly.html)

### Properties

| Name | Summary |
|------|---------|
| `physicsSettings` | — |

### Methods

#### `ReadProjectSettings()`

## PhysicsTransformTweener

**Full name:** `Unity.U2D.Physics.PhysicsTransformTweener`  
**Docs:** [Unity.U2D.Physics.PhysicsTransformTweener](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsTransformTweener.html)

### Methods

#### `CreateWorldTransformAccessArray(PhysicsWorld, int, int)`

#### `DestroyWorldTransformAccessArray(PhysicsWorld)`

#### `GetWorldTransformAccessArray(PhysicsWorld)`

#### `WriteTransformTweensCustom(object, PhysicsWorld, float, float, PhysicsWorld.TransformWriteMode, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, Scripting2D.PhysicsBuffer)`

#### `WriteTransformTweensTask(bool, float, float, PhysicsWorld.TransformWriteMode, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, Unity.Collections.NativeArray{Unity.U2D.Physics.PhysicsBody.TransformWriteTween})`

### Nested Types

- **WriteTransformTweensParallelJob** — —

### WriteTransformTweensParallelJob

**Full name:** `Unity.U2D.Physics.PhysicsTransformTweener.WriteTransformTweensParallelJob`  

## PhysicsTransformWatcher

**Full name:** `Unity.U2D.Physics.PhysicsTransformWatcher`  
**Docs:** [Unity.U2D.Physics.PhysicsTransformWatcher](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsTransformWatcher.html)

### Methods

#### `TransformChangedCallback(Scripting2D.PhysicsBuffer)`

#### `TransformParentHierarchyChangedCallback(Scripting2D.PhysicsBuffer)`

## PhysicsTransformWriter

**Full name:** `Unity.U2D.Physics.PhysicsTransformWriter`  
**Docs:** [Unity.U2D.Physics.PhysicsTransformWriter](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsTransformWriter.html)

### Methods

#### `WriteTransformsSequentialTask(Unity.Collections.NativeArray{Unity.U2D.Physics.PhysicsBody.TransformWriteTween}, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, bool, bool)`

#### `WriteWorldTransforms(PhysicsWorld, PhysicsWorld.SimulationType, PhysicsWorld.TransformWriteMode, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, PhysicsWorld.TransformTweenMode)`

#### `WriteWorldTransformsCustom(object, PhysicsWorld, PhysicsWorld.SimulationType, PhysicsWorld.TransformWriteMode, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, PhysicsWorld.TransformTweenMode)`

#### `WriteWorldTransformsGetPhysicsTransformPose3D(PhysicsBody.TransformWriteTween, PhysicsWorld.TransformPlane, PhysicsWorld.TransformPlaneCustom, bool, Vector3, Quaternion)`

### Nested Types

- **WriteTransformsParallelJob** — —

### WriteTransformsParallelJob

**Full name:** `Unity.U2D.Physics.PhysicsTransformWriter.WriteTransformsParallelJob`  

## PhysicsWorldRenderer

**Full name:** `Unity.U2D.Physics.PhysicsWorldRenderer`  
**Docs:** [Unity.U2D.Physics.PhysicsWorldRenderer](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsWorldRenderer.html)

### Methods

#### `GetCameraViewAABB(Camera)`

#### `GetMesh()`

#### `InitializeRendering()`

#### `IsCameraTypeValid(Camera)`

#### `RenderWorlds_BIRP(Camera)`

#### `RenderWorlds_SRP(Rendering.ScriptableRenderContext, Camera)`

#### `SendDrawResults(bool, bool, PhysicsWorld, PhysicsWorld.DrawResults, PhysicsWorld.TransformPlane, Matrix4x4, float, float)`

#### `ShutdownRendering()`

### Nested Types

- **DrawerGroup** — —

### DrawerGroup

**Full name:** `Unity.U2D.Physics.PhysicsWorldRenderer.DrawerGroup`  

#### Properties

| Name | Summary |
|------|---------|
| `isValid` | — |

#### Methods

##### `new()`

##### `Dispose()`

##### `Draw(Rendering.CommandBuffer, PhysicsWorld.DrawResults, float, float, PhysicsWorld.TransformPlane, Matrix4x4)`

#### Nested Types

- **BaseDrawer** — Base drawer when using an arbitrary drawing method.
- **CapsuleGeometryDrawer** — Capsule Geometry Drawer.
- **CircleGeometryDrawer** — Circle Geometry Drawer.
- **LineDrawer** — Line Drawer.
- **PointDrawer** — Point Drawer.
- **PolygonGeometryDrawer** — Polygon Geometry Drawer.

#### BaseDrawer

> Base drawer when using an arbitrary drawing method.

**Full name:** `Unity.U2D.Physics.PhysicsWorldRenderer.DrawerGroup.BaseDrawer`  

##### Methods

###### `Dispose()`

###### `Draw(Rendering.CommandBuffer, PhysicsWorld.DrawResults, float, float, PhysicsWorld.TransformPlane, Matrix4x4)`

#### CapsuleGeometryDrawer

> Capsule Geometry Drawer.

**Full name:** `Unity.U2D.Physics.PhysicsWorldRenderer.DrawerGroup.CapsuleGeometryDrawer`  

##### Methods

###### `new()`

###### `Draw(Rendering.CommandBuffer, PhysicsWorld.DrawResults, float, float, PhysicsWorld.TransformPlane, Matrix4x4)`

#### CircleGeometryDrawer

> Circle Geometry Drawer.

**Full name:** `Unity.U2D.Physics.PhysicsWorldRenderer.DrawerGroup.CircleGeometryDrawer`  

##### Methods

###### `new()`

###### `Draw(Rendering.CommandBuffer, PhysicsWorld.DrawResults, float, float, PhysicsWorld.TransformPlane, Matrix4x4)`

#### LineDrawer

> Line Drawer.

**Full name:** `Unity.U2D.Physics.PhysicsWorldRenderer.DrawerGroup.LineDrawer`  

##### Methods

###### `new()`

###### `Draw(Rendering.CommandBuffer, PhysicsWorld.DrawResults, float, float, PhysicsWorld.TransformPlane, Matrix4x4)`

#### PointDrawer

> Point Drawer.

**Full name:** `Unity.U2D.Physics.PhysicsWorldRenderer.DrawerGroup.PointDrawer`  

##### Methods

###### `new()`

###### `Draw(Rendering.CommandBuffer, PhysicsWorld.DrawResults, float, float, PhysicsWorld.TransformPlane, Matrix4x4)`

#### PolygonGeometryDrawer

> Polygon Geometry Drawer.

**Full name:** `Unity.U2D.Physics.PhysicsWorldRenderer.DrawerGroup.PolygonGeometryDrawer`  

##### Methods

###### `new()`

###### `Draw(Rendering.CommandBuffer, PhysicsWorld.DrawResults, float, float, PhysicsWorld.TransformPlane, Matrix4x4)`

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
