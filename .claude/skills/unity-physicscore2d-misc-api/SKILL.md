---
name: unity-physicscore2d-misc-api
description: Authoritative Unity 6000.7 PhysicsCore2D API reference for Miscellaneous. Lists every type, property, field, method (with signatures, params, returns) for: IPhysicsResultFilter, PhysicsHandle, PhysicsResultEnumerable, PhysicsResultExtensions, PhysicsSpace. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Miscellaneous

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.7, so it should be treated as the source of truth for member names, signatures, and documentation strings.

Top-level types in this file: `IPhysicsResultFilter`, `PhysicsHandle`, `PhysicsResultEnumerable`, `PhysicsResultExtensions`, `PhysicsSpace`.

## IPhysicsResultFilter

> A filter applied to each element of a physics result array during enumeration.

**Full name:** `Unity.U2D.Physics.IPhysicsResultFilter`

### Methods

#### `Keep(T)`

Decide whether a single result element is kept.

**Params:**
- `item` — The result element being tested.

**Returns:** True to keep the element in the enumeration, false to skip it.

## PhysicsHandle

> An abstract handle that can be used for custom purposes such as handling miscellaneous physics object types abstractly. You can create a handle with PhysicsHandle.Create or PhysicsHandle.CreateBatch. You can destroy a handle with PhysicsHandle.Destroy or PhysicsHandle.DestroyBatch. You can also get a handle from one of the following physics objects: PhysicsBody.physicsHandle, PhysicsShape.physicsHandle, PhysicsChain.physicsHandle, PhysicsJoint.physicsHandle, PhysicsDistanceJoint.physicsHandle, PhysicsFixedJoint.physicsHandle, PhysicsHingeJoint.physicsHandle, PhysicsIgnoreJoint.physicsHandle, PhysicsRelativeJoint.physicsHandle, PhysicsSliderJoint.physicsHandle and PhysicsWheelJoint.physicsHandle. NOTE: When retrieving the handle from another physics object, the object type is not encoded so that must be handled separately. Because of this, it's entirely possible for two handles to be equal, differing only by the type they came from so care must be taken or the object type explicitly stored against handles.

**Full name:** `Unity.U2D.Physics.PhysicsHandle`

### Properties

| Name | Summary |
|------|---------|
| `generation` | Get the handle generation. |
| `index` | Get the handle index. |
| `isPoolHandle` | Checks if the physics handle is from the physics handle pool or not. This will return false unless thePhysicsHandle was explicitly created with PhysicsHandle.Create or PhysicsHandle.CreateBatch. |
| `isValid` | Checks if the physics handle is valid in the physics handle pool. This will only work correctly if the PhysicsHandle was explicitly created with PhysicsHandle.Create or PhysicsHandle.CreateBatch. If the handle comes from another physics object, it will not validate that object and a warning will be issued. |
| `world` | Get the handle world index. |

### Methods

#### `AsWorld(PhysicsWorld)`

Get a copy of this handle that refers to the specified PhysicsWorld. The index and generation are preserved and only the world is changed, so the result refers to the same object slot in the specified world. This is useful when one world shares an identical handle layout with another, such as a world created from a snapshot of, or a clone of, the original.

**Params:**
- `world` — The world the returned handle should refer to.

**Returns:** A handle referring to the same slot and generation in the specified world.

#### `Create()`

Create a PhysicsHandle.

**Returns:** The created physics handle.

#### `CreateBatch(int, Unity.Collections.Allocator)`

Create a batch of PhysicsHandle.

**Params:**
- `handleCount` — The quantity of physics handles to create, in the range 1 to 100000.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The physics handles. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `Destroy()`

Destroy the handle. This will only work correctly if the PhysicsHandle was explicitly created with PhysicsHandle.Create or PhysicsHandle.CreateBatch. NOTE: If the handle comes from another physics object, it will not destroy that object and a warning will be issued.

#### `DestroyBatch(ReadOnlySpan<PhysicsHandle>)`

Destroy the specified span of PhysicsHandle. NOTE: If any of the handles come from another physics object, it will not destroy that object and a warning will be issued.

**Params:**
- `physicsHandles` — The physics handles to destroy.

#### `Equals(object)`

#### `Equals(PhysicsHandle)`

#### `GetHashCode()`

#### `ToString()`

## PhysicsResultEnumerable

> ⚠️ **[Obsolete]** (no longer compiles, it is a hard error): Types with embedded references are not supported in this version of your compiler.
>
> A lazy, allocation-free view over a physics result array that yields only the elements a filter keeps.

**Full name:** `Unity.U2D.Physics.PhysicsResultEnumerable`

### Methods

#### `new(Unity.Collections.NativeArray<T>, TFilter)`

Create a filtered view over a physics result array.

**Params:**
- `source` — The result array to iterate.
- `filter` — The filter deciding which elements are kept.

#### `GetEnumerator()`

Get an enumerator that walks the source array and yields only the kept elements.

**Returns:** A value-type enumerator over the filtered elements.

### Nested Types

- **Enumerator** — ⚠️ **[Obsolete]** (no longer compiles, it is a hard error): Types with embedded references are not supported in this version of your compiler. — Walks a physics result array and yields only the elements the filter keeps.

### Enumerator

> ⚠️ **[Obsolete]** (no longer compiles, it is a hard error): Types with embedded references are not supported in this version of your compiler.
>
> Walks a physics result array and yields only the elements the filter keeps.

**Full name:** `Unity.U2D.Physics.PhysicsResultEnumerable.Enumerator`

#### Properties

| Name | Summary |
|------|---------|
| `Current` | Get the element at the current position of the enumerator. |

#### Methods

##### `new(Unity.Collections.NativeArray<T>, TFilter)`

Create an enumerator over a result array and its filter.

**Params:**
- `source` — The result array to iterate.
- `filter` — The filter deciding which elements are kept.

##### `MoveNext()`

Advance to the next element the filter keeps.

**Returns:** True if a kept element was found, false once the source is exhausted.

## PhysicsResultExtensions

> Fluent filtering extensions for the physics result arrays returned by queries, events, and other physics operations.

**Full name:** `Unity.U2D.Physics.PhysicsResultExtensions`

### Methods

#### `PhysicsResultEnumerable(Unity.Collections.NativeArray<T>, TFilter)`

## PhysicsSpace

> Provides the ability to store and query information in a spatial database.

**Full name:** `Unity.U2D.Physics.PhysicsSpace`

### Properties

| Name | Summary |
|------|---------|
| `isValid` | Check if a Physics Space is valid. |
| `memoryAllocated` | Get the total memory allocated for the space, in bytes. |
| `proxyCount` | Get the proxy count in the space. |
| `rootAABB` | Get the root bounds that contain all the AABB proxies. |
| `sourceWorld` | Get the world this space is bound to, or a default (invalid) world if the space is not bound. A space is bound by creating it with the world overload of Create. |

### Methods

#### `CastRay(PhysicsQuery.CastRayInput, PhysicsMask, Unity.Collections.Allocator)`

Find proxies that intersect the specified ray. The results indicate that the proxy AABB intersect the specified cast ray, in no specific order.

**Params:**
- `input` — The configuration of the ray to cast.
- `categories` — The categories to query for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CastShape(PhysicsQuery.CastShapeInput, PhysicsMask, Unity.Collections.Allocator)`

Find proxies that intersect the specified shape. The results indicate that the proxy AABB intersect the specified cast shape, in no specific order.

**Params:**
- `input` — The configuration of the shape to cast.
- `categories` — The categories to query for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The query cast results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `ClearProxies()`

Clear all space proxies. You should no longer use any previously returned PhysicsSpace.ProxyHandle as they may be invalid or direct to the wrong proxy in the future. The space will continue with a similar proxy capacity therefore if full de-allocation is required, the space should be destroyed and a new one created.

**Returns:** If the proxies were destroyed. If the space is invalid, no proxies will be destroyed.

#### `Clone(PhysicsWorld, PhysicsQuery.QueryFilter, bool)`

Clear any existing proxies and clone all PhysicsShape found in the specified PhysicsWorld. Each proxy created will have a user-handle assigned as PhysicsShape.physicsHandle. This means you can get the referenced shape by using PhysicsShape.#ctor.

**Params:**
- `world` — The world to find the PhysicsShape in. On a world-bound space this must be the world the space is bound to.
- `filter` — The filter to control what proxies are created.
- `destroyExistingProxies` — Controls if any existing proxies are destroyed before cloning from the specified world. If false, care should be taken that any existing proxies refer to PhysicsShape otherwise a mix of user-handles will be present.

**Returns:** How many proxies were cloned.

#### `Clone(PhysicsWorld, PhysicsQuery.QueryFilter, PhysicsAABB, bool)`

Clear any existing proxies and clone any PhysicsShape found in the specified PhysicsWorld overlapping the specified PhysicsAABB. Each proxy created will have a user-handle assigned as PhysicsShape.physicsHandle. This means you can get the referenced shape by using PhysicsShape.#ctor.

**Params:**
- `world` — The world to find the PhysicsShape in. On a world-bound space this must be the world the space is bound to.
- `aabb` — The AABB used to discover PhysicsShape in the specified world. If the AABB size is size (default) then the whole world will be discovered.
- `filter` — The filter to control what proxies are created.
- `destroyExistingProxies` — Controls if any existing proxies are destroyed before cloning from the specified world. If false, care should be taken that any existing proxies refer to PhysicsShape otherwise a mix of user-handles will be present.

**Returns:** How many proxies were cloned.

#### `Clone(PhysicsQuery.QueryFilter, bool)`

Clear any existing proxies and clone every PhysicsShape found in the world this space is bound to. Each proxy created will have a user-handle assigned as PhysicsShape.physicsHandle. This only applies to a space bound to a world.

**Params:**
- `filter` — The filter to control what proxies are created.
- `destroyExistingProxies` — Controls if any existing proxies are destroyed before cloning from the bound world. If false, care should be taken that any existing proxies refer to PhysicsShape otherwise a mix of user-handles will be present.

**Returns:** How many proxies were cloned.

#### `Clone(PhysicsQuery.QueryFilter, PhysicsAABB, bool)`

Clear any existing proxies and clone any PhysicsShape found in the world this space is bound to overlapping the specified PhysicsAABB. Each proxy created will have a user-handle assigned as PhysicsShape.physicsHandle. This only applies to a space bound to a world.

**Params:**
- `aabb` — The AABB used to discover PhysicsShape in the bound world. If the AABB size is size (default) then the whole world will be discovered.
- `filter` — The filter to control what proxies are created.
- `destroyExistingProxies` — Controls if any existing proxies are destroyed before cloning from the bound world. If false, care should be taken that any existing proxies refer to PhysicsShape otherwise a mix of user-handles will be present.

**Returns:** How many proxies were cloned.

#### `Create()`

Create a Physics Space.

**Returns:** The new Physics Space.

#### `Create(PhysicsWorld)`

Create a Physics Space bound to the specified world so its proxies represent shapes in that world. While bound, any proxy user handle must be a live shape in this world and you can refresh the proxies from their shapes. The binding lasts for the lifetime of the space and cannot be changed.

**Params:**
- `world` — The world whose shapes this space's proxies will represent.

**Returns:** The new Physics Space bound to the specified world.

#### `CreateProxy(PhysicsAABB, PhysicsMask, PhysicsHandle)`

Create a space proxy.

**Params:**
- `aabb` — The AABB the proxy covers.
- `categories` — The categories as a physics mask associated with the proxy. This can be used when querying the space. If not used, it should be PhysicsMask.All.
- `userHandle` — The custom user handle associated with the proxy. On a world-bound space this must be a live shape in the bound world.

**Returns:** The created proxy handle used to refer to the proxy.

#### `CreateProxyShapes(ReadOnlySpan<PhysicsShape>, Unity.Collections.Allocator)`

Create one proxy per shape, taking each proxy AABB, categories and user handle directly from the shape. This only applies to a space bound to a world, and every shape must be a live shape in that world.

**Params:**
- `shapes` — The shapes to create proxies for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The created proxy handles, one per shape in the same order. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `Destroy()`

Destroy the Physics Space.

**Returns:** If the space was destroyed or not.

#### `DestroyAll()`

Destroy all active Physics Space.

#### `DestroyProxy(PhysicsSpace.ProxyHandle)`

Destroy a space proxy.

**Params:**
- `proxyHandle` — The proxy to destroy.

**Returns:** If the proxy was destroyed. If the proxy handle is invalid, no proxy will be destroyed.

#### `Equals(object)`

#### `Equals(PhysicsSpace)`

#### `GetBatchProxyAABB(ReadOnlySpan<PhysicsSpace.ProxyHandle>, Unity.Collections.Allocator)`

Get a batch of proxy AABB. If any proxy handle in the batch is invalid, no results are returned and an empty array is produced.

**Params:**
- `proxyHandles` — The proxies to get.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The proxy AABB, one per proxy in the same order. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetBatchProxyCategories(ReadOnlySpan<PhysicsSpace.ProxyHandle>, Unity.Collections.Allocator)`

Get a batch of proxy categories. If any proxy handle in the batch is invalid, no results are returned and an empty array is produced.

**Params:**
- `proxyHandles` — The proxies to get.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The proxy categories, one per proxy in the same order. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetBatchProxyShapes(ReadOnlySpan<PhysicsSpace.ProxyHandle>, Unity.Collections.Allocator)`

Get a batch of proxy user handles as shapes, valid only on a space bound to a world. On a bound space every proxy user handle is a shape, so each is returned as a shape that the caller can check for validity. This only applies to a space bound to a world.

**Params:**
- `proxyHandles` — The proxies to get.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The proxy shapes, one per proxy in the same order. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetBatchProxyUserHandle(ReadOnlySpan<PhysicsSpace.ProxyHandle>, Unity.Collections.Allocator)`

Get a batch of proxy user handles. If any proxy handle in the batch is invalid, no results are returned and an empty array is produced.

**Params:**
- `proxyHandles` — The proxies to get.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The proxy user handles, one per proxy in the same order. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `GetHashCode()`

#### `GetProxyAABB(PhysicsSpace.ProxyHandle)`

Set the proxy physics AABB.

**Params:**
- `proxyHandle` — The proxy to get.

**Returns:** The proxy physics AABB.

#### `GetProxyCategories(PhysicsSpace.ProxyHandle)`

Get the proxy categories.

**Params:**
- `proxyHandle` — The proxy to get.

**Returns:** The proxy categories as a physics mask.

#### `GetProxyUserHandle(PhysicsSpace.ProxyHandle)`

Get the proxy user handle.

**Params:**
- `proxyHandle` — The proxy to get.

**Returns:** The proxy user handle.

#### `GetSpaces(Unity.Collections.Allocator)`

Get all the currently active spaces.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The currently active spaces. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapAABB(PhysicsAABB, PhysicsMask, Unity.Collections.Allocator)`

Find proxies that overlap the specified AABB. The results indicate that the proxy AABB overlap the specified AABB, in no specific order.

**Params:**
- `aabb` — The AABB to query.
- `categories` — The categories to query for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `OverlapPoint(Vector2, PhysicsMask, Unity.Collections.Allocator)`

Find proxies that overlap the specified point. The results indicate that the proxy AABB overlap the specified point, in no specific order.

**Params:**
- `point` — The point to query.
- `categories` — The categories to query for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The query overlap results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `SetBatchProxyAABB(ReadOnlySpan<PhysicsSpace.ProxyHandle>, ReadOnlySpan<PhysicsAABB>, bool)`

Set a batch of proxy AABB, where the AABB at each index is set on the corresponding proxy at the same index. The two spans must be the same length. Any invalid proxy handle or AABB in the batch is skipped and reported with a single warning.

**Params:**
- `proxyHandles` — The proxies to set.
- `aabbs` — The AABB to set on each corresponding proxy.
- `updateAncestors` — If the AABB have simply moved then this should be false however if you have changed their size then you should update the space ancestors which takes more time. This applies to the whole batch.

#### `SetBatchProxyCategories(ReadOnlySpan<PhysicsSpace.ProxyHandle>, ReadOnlySpan<PhysicsMask>)`

Set a batch of proxy categories, where the categories at each index are set on the corresponding proxy at the same index. The two spans must be the same length. This can be an expensive operation as all ancestors need to be recalculated. Any invalid proxy handle in the batch is skipped and reported with a single warning.

**Params:**
- `proxyHandles` — The proxies to set.
- `categories` — The categories as a physics mask to set on each corresponding proxy.

#### `SetProxyAABB(PhysicsSpace.ProxyHandle, PhysicsAABB, bool)`

Set the proxy AABB.

**Params:**
- `proxyHandle` — The proxy to set.
- `aabb` — The AABB to set the proxy to.
- `updateAncestors` — If the AABB has simply moved then this should be false however if you have changed its size then you should update the space ancestors which takes more time.

#### `SetProxyCategories(PhysicsSpace.ProxyHandle, PhysicsMask)`

Set the proxy categories. This can be an expensive operation as all ancestors need to be recalculated.

**Params:**
- `proxyHandle` — The proxy to set.
- `categories` — The categories as a physics mask to set.

#### `SetProxyUserHandle(PhysicsSpace.ProxyHandle, PhysicsHandle)`

Set the proxy user handle. On a world-bound space the user handle must be a live shape in the bound world.

**Params:**
- `proxyHandle` — The proxy to set.
- `userHandle` — The user handle to set. On a world-bound space this must be a live shape in the bound world.

#### `SyncShapes()`

Refresh every proxy from its shape, updating each proxy AABB and categories to match the live shape. This only applies to a space bound to a world. A proxy whose shape has been destroyed since it was added is skipped and reported with a single warning.

**Returns:** The number of proxies that were synced.

#### `SyncShapes(ReadOnlySpan<PhysicsSpace.ProxyHandle>)`

Refresh the specified proxies from their shapes, updating each proxy AABB and categories to match the live shape. This only applies to a space bound to a world. An invalid proxy handle, or a proxy whose shape has been destroyed, is skipped and reported with a single warning.

**Params:**
- `proxyHandles` — The proxies to sync.

**Returns:** The number of proxies that were synced.

#### `SyncShapes(PhysicsSpace.ProxyHandle)`

Refresh a single proxy from its shape, updating the proxy AABB and categories to match the live shape. This only applies to a space bound to a world. The proxy is skipped if its handle is invalid or its shape has been destroyed.

**Params:**
- `proxyHandle` — The proxy to sync.

**Returns:** The number of proxies that were synced (0 or 1).

#### `ToString()`

### Nested Types

- **CastResult** — The narrowphase cast results.
- **ProxyHandle** — A proxy identity added to the space.
- **ProxyResult** — A space result from PhysicsSpace.OverlapAABB, PhysicsSpace.CastRay or PhysicsSpace.CastShape.
- **ShapeSpace** — Query a PhysicsSpace assuming the PhysicsSpace.ProxyHandle are all PhysicsShape.

### CastResult

> The narrowphase cast results.

**Full name:** `Unity.U2D.Physics.PhysicsSpace.CastResult`

#### Properties

| Name | Summary |
|------|---------|
| `castResult` | The narrowphase result (actual). |
| `proxyResult` | The proxy result (proxy). |

#### Methods

##### `new(PhysicsSpace.ProxyResult, PhysicsQuery.CastResult)`

Create a narrowphase result.

**Params:**
- `proxyResult` — The proxy result (proxy).
- `castResult` — The narrowphase result (actual).

##### `Equals(object)`

##### `Equals(PhysicsSpace.CastResult)`

##### `GetHashCode()`

##### `ToString()`

#### Nested Types

- **SortAscendingOrder** — Ascending distance sort comparer.

#### SortAscendingOrder

> Ascending distance sort comparer.

**Full name:** `Unity.U2D.Physics.PhysicsSpace.CastResult.SortAscendingOrder`

##### Methods

###### `Compare(PhysicsSpace.CastResult, PhysicsSpace.CastResult)`

### ProxyHandle

> A proxy identity added to the space.

**Full name:** `Unity.U2D.Physics.PhysicsSpace.ProxyHandle`

#### Properties

| Name | Summary |
|------|---------|
| `Id` | The Id of the proxy. |
| `isValid` | Whether this handle refers to a valid proxy slot. A default-constructed handle is always invalid. This does not check that the proxy still exists in any specific space, that the underlying tree slot has not been reused, or that this handle belongs to the space it is passed to. Those are checked when the handle is used. |

#### Methods

##### `Equals(object)`

##### `Equals(PhysicsSpace.ProxyHandle)`

##### `GetHashCode()`

##### `ToString()`

### ProxyResult

> A space result from PhysicsSpace.OverlapAABB, PhysicsSpace.CastRay or PhysicsSpace.CastShape.

**Full name:** `Unity.U2D.Physics.PhysicsSpace.ProxyResult`

#### Properties

| Name | Summary |
|------|---------|
| `proxyHandle` | The proxy handle. |
| `userHandle` | The user handle. |

#### Methods

##### `Equals(object)`

##### `Equals(PhysicsSpace.ProxyResult)`

##### `GetHashCode()`

##### `ToString()`

### ShapeSpace

> Query a PhysicsSpace assuming the PhysicsSpace.ProxyHandle are all PhysicsShape.

**Full name:** `Unity.U2D.Physics.PhysicsSpace.ShapeSpace`

#### Methods

##### `CastRay(PhysicsSpace, PhysicsQuery.CastRayInput, PhysicsMask, Unity.Collections.Allocator)`

Find PhysicsShape that intersect the specified ray. The results indicate PhysicsShape that intersect the specified ray, in ascending order.

**Params:**
- `physicsSpace` — The PhysicsSpace to query.
- `input` — The configuration of the ray to cast.
- `categories` — The categories to query for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The query results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

##### `CastShape(PhysicsSpace, PhysicsQuery.CastShapeInput, PhysicsMask, Unity.Collections.Allocator)`

Find PhysicsShape that intersect the specified shape. The results indicate PhysicsShape that intersect the specified cast shape, in ascending order.

**Params:**
- `physicsSpace` — The PhysicsSpace to query.
- `input` — The configuration of the shape to cast.
- `categories` — The categories to query for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The query results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

##### `OverlapAABB(PhysicsSpace, PhysicsAABB, PhysicsMask, Unity.Collections.Allocator)`

Find PhysicsShape whose AABB overlap the specified AABB. The results indicate PhysicsShape AABB overlap the specified AABB, in no specific order.

**Params:**
- `physicsSpace` — The PhysicsSpace to query.
- `aabb` — The AABB to query.
- `categories` — The categories to query for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The query results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

##### `OverlapPoint(PhysicsSpace, Vector2, PhysicsMask, Unity.Collections.Allocator)`

Find PhysicsShape that overlap the specified point. The results indicate PhysicsShape overlap the specified point, in no specific order.

**Params:**
- `physicsSpace` — The PhysicsSpace to query.
- `point` — The point used to query.
- `categories` — The categories to query for.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The query results. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
