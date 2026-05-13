---
name: unity-physicscore2d-destructor-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for Destructor. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsDestructor. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Destructor

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsDestructor`.

## PhysicsDestructor

> Provides the ability to destruct (fragment and Slice) geometry.

**Full name:** `Unity.U2D.Physics.PhysicsDestructor`  
**Docs:** [Unity.U2D.Physics.PhysicsDestructor](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDestructor.html)

### Methods

#### `Fragment(PhysicsDestructor.FragmentGeometry, ReadOnlySpan<Vector2>, Unity.Collections.Allocator)`

Fragment the specified target geometry using the specified fragment points. The fragment points define areas where polygon fragments will be produced from the target geometry. If the resulting polygon fragments have more polygon vertices than can fit into a single PolygonGeometry then the fragment will be split into multiple polygon fragments. The maximum number of vertices a single polygon fragment can have is defined by PhysicsConstants.MaxPolygonVertices. If even a single fragment point overlaps the target geometry then all results will be returned in PhysicsDestructor.FragmentResult._brokenGeometry. If none of the fragment points overlap the target geometry then all the results will be returned in PhysicsDestructor.FragmentResult._unbrokenGeometry. See PhysicsDestructor.FragmentResult.

**Params:**
- `target` — The target geometry to fragment. There must be at least a single geometry element. Any target polygons with a non-zero radius will be ignored.
- `fragmentPoints` — The world-space fragment points used to define fragment regions. The number of fragment points must be greater than 1.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The fragment results. These results must be disposed of after use otherwise leaks will occur.

#### `Fragment(PhysicsDestructor.FragmentGeometry, PhysicsDestructor.FragmentGeometry, ReadOnlySpan<Vector2>, Unity.Collections.Allocator)`

Fragment the specified mask geometry using the specified fragment points, after the target geometry has the mask (carving) geometry removed from it. The target geometry is first clipped with the mask geometry using a PhysicsComposer.Operation.NOT operation. The resulting target geometry is returned in PhysicsDestructor.FragmentResult._unbrokenGeometry. The mask geometry is then clipped with the original target geometry using an PhysicsComposer.Operation.AND operation. If the clipped mask produces no geometry then no results are returned in PhysicsDestructor.FragmentResult._brokenGeometry. The fragment points define areas where polygon fragments will be produced from the clipped masked geometry. The resulting polygon fragments are returned in PhysicsDestructor.FragmentResult._brokenGeometry. If the resulting polygon fragments have more polygon vertices than can fit into a single PolygonGeometry then the fragment will be split into multiple polygon fragments. The maximum number of vertices a single polygon fragment can have is defined by PhysicsConstants.MaxPolygonVertices. See PhysicsDestructor.FragmentResult.

**Params:**
- `target` — The target geometry to fragment. There must be at least a single geometry element. Any target polygons with a non-zero radius will be ignored.
- `mask` — The mask geometry that will be used to clip the target geometry. There must be at least a single geometry element. Any mask polygons with a non-zero radius will be ignored.
- `fragmentPoints` — The world-space fragment points used to define fragment regions. The number of fragment points must be greater than 1.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The fragment results. The transform returned here is the one provided in the target geometry. These results must be disposed of after use otherwise leaks will occur.

#### `Slice(PhysicsDestructor.FragmentGeometry, Vector2, Vector2, Unity.Collections.Allocator)`

Slice the specified target geometry using the specified slice line. The target geometry is sliced using the specified ray as defined by and . The specified line segment and are extended to infinity and so defines a 2D intersection plane. All valid target geometry will returned in either the PhysicsDestructor.SliceResult._leftGeometry or PhysicsDestructor.SliceResult._rightGeometry depending on its side of the line (sliced or not). Left and Right are defined as "looking" along the ray in the direction defined by with Left being anything to the left of the ray and Right being anything to the right of the ray. See PhysicsDestructor.SliceResult.

**Params:**
- `target` — The target geometry to slice. There must be at least a single geometry element. Any target polygons with a non-zero radius will be ignored.
- `origin` — The start of the ray slice line.
- `translation` — The translation relative to the origin of the slice ray. This must have a non-zero magnitude.
- `allocator` — The memory allocator to use for the results. This can only be Allocator.Temp, Allocator.TempJob or Allocator.Persistent.

**Returns:** The slice results. The transform returned here is the one provided in the target geometry. These results must be disposed of after use otherwise leaks will occur.

### Nested Types

- **FragmentGeometry** — The polygon geometry used when fragmenting.
- **FragmentResult** — The result of a fragment operation. This must be disposed of after use otherwise leaks will occur. See PhysicsDestructor.Fragment and PhysicsDestructor.Fragment.
- **SliceResult** — The result of a slice operation. This must be disposed of after use otherwise leaks will occur. See PhysicsDestructor.Slice.

### FragmentGeometry

> The polygon geometry used when fragmenting.

**Full name:** `Unity.U2D.Physics.PhysicsDestructor.FragmentGeometry`  

#### Methods

##### `new(PhysicsTransform, ReadOnlySpan<PolygonGeometry>)`

Create fragment geometry.

**Params:**
- `transform` — The transform used to transform the specified Polygon geometry.
- `geometry` — The Polygon geometry to use.

### FragmentResult

> The result of a fragment operation. This must be disposed of after use otherwise leaks will occur. See PhysicsDestructor.Fragment and PhysicsDestructor.Fragment.

**Full name:** `Unity.U2D.Physics.PhysicsDestructor.FragmentResult`  

#### Properties

| Name | Summary |
|------|---------|
| `brokenGeometry` | The geometry that was fragmented (broken). |
| `transform` | The transform used when fragmenting. All returned geometry uses this. |
| `unbrokenGeometry` | The geometry that was not fragmented (unbroken). |
| `unbrokenGeometryIslands` | The geometry islands indicating how polygons are connected together. Each generated polygon belongs to a unique island defining a set of polygons that are connected together as they share edges. The array returned contains a series of ranges where each range is a unique connected island where the range indicates both the start index and length of the original polygon indices. The number of discovered unique islands is defined by the size of the returned array. This is only valid when fragmenting with a mask with PhysicsDestructor.Fragment. |

#### Methods

##### `Dispose()`

Dispose of the fragment result.

### SliceResult

> The result of a slice operation. This must be disposed of after use otherwise leaks will occur. See PhysicsDestructor.Slice.

**Full name:** `Unity.U2D.Physics.PhysicsDestructor.SliceResult`  

#### Properties

| Name | Summary |
|------|---------|
| `leftGeometry` | The geometry that was sliced on the "left" of the line. |
| `rightGeometry` | The geometry that was sliced on the "right" of the line. |
| `transform` | The transform used when slicing. All returned geometry uses this. |

#### Methods

##### `Dispose()`

Dispose of the fragment result.

---

_Generated by `.claude/api-reference/_generate.py` from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
