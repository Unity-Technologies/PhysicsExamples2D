---
name: unity-physicscore2d-composer-api
description: Authoritative Unity 6000.5 PhysicsCore2D API reference for Composer. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsComposer. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Composer

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.5, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsComposer`.

## PhysicsComposer

> Provides the ability to compose geometry using specific operations on layers in a specific order.

**Full name:** `Unity.U2D.Physics.PhysicsComposer`  
**Docs:** [Unity.U2D.Physics.PhysicsComposer](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsComposer.html)

### Fields

| Name | Summary |
|------|---------|
| `DefaultCurveStride` | The default curve stride used when composing geometry that has a non-zero radius. |
| `MinCurveStride` | The minimum curve stride allowed when composing geometry that has a non-zero radius. |

### Properties

| Name | Summary |
|------|---------|
| `isValid` | Check if a Physics Composer is valid. |
| `layerCount` | Get the number of layers currently added to the Physics Composer. |
| `layerHandles` | Get the layer handles added to the Physics Composer. |
| `maxPolygonVertices` | Get/Set the maximum number of polygon vertices to be used when composing polygon output. This should be in the range of 3 to . The default is . |
| `rejectedGeometryCount` | Get the number of geometries that were rejected during the last Geometry Composition. Geometry can be rejected for a number of reasons such as vertices being collinear or too close etc. Whilst "pure" geometry is always valid, this geometry is meant to be used by physics which has constraints on what it can accept. All geometry successfully created will always be valid when used by physics. If you notice thin/small gaps in the composition, this is likely to be rejected geometry. Checking this property will help determine that. |
| `useDelaunay` | Get/Set if Delaunay tessellation is to be used. Delaunay tessellation is enabled by default and produces far superior results. When Delaunay tessellation is disabled, curved areas can produce invalid geometry which is rejected therefore increase the . |

### Methods

#### `AddLayer(CircleGeometry, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

Add a Circle Geometry layer to the Physics Composer.

**Params:**
- `geometry` — The Circle Geometry to use.
- `transform` — The transform to use on the geometry.
- `operation` — The composer operation to use.
- `order` — The order to perform the composer operation.
- `curveStride` — The curve stride controls how many vertices are used to approximate the circle geometry. Lower values produce more vertices whereas larger values produce fewer vertices. The valid range is [ , 1.0] although values over 0.3 tend to produce relatively poor results.
- `reverseWinding` — Whether the winding should be reversed. Typically winding is generated anti-clockwise, reversed winding is therefore clockwise.

**Returns:** A handle to the new layer.

#### `AddLayer(ReadOnlySpan{Unity.U2D.Physics.CircleGeometry}, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

Add multiple Circle Geometry layer to the Physics Composer.

**Params:**
- `geometry` — The Circle Geometry to use. This geometry will be copied so the geometry the span is referring to can be disposed of afterwards if required.
- `transform` — The transform to use on the geometry.
- `operation` — The composer operation to use.
- `order` — The order to perform the composer operation.
- `curveStride` — The curve stride controls how many vertices are used to approximate the circle geometry. Lower values produce more vertices whereas larger values produce fewer vertices. The valid range is [ , 1.0] although values over 0.3 tend to produce relatively poor results.
- `reverseWinding` — Whether the winding should be reversed. Typically winding is generated anti-clockwise, reversed winding is therefore clockwise.

**Returns:** A handle to the new layer.

#### `AddLayer(CapsuleGeometry, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

Add a Capsule Geometry layer to the Physics Composer.

**Params:**
- `geometry` — The Capsule Geometry to use.
- `transform` — The transform to use on the geometry.
- `operation` — The composer operation to use.
- `order` — The order to perform the composer operation.
- `curveStride` — The curve stride controls how many vertices are used to approximate the curved end-caps of the capsule geometry. Lower values produce more vertices whereas larger values produce fewer vertices. The valid range is [ , 1.0] although values over 0.3 tend to produce relatively poor results.
- `reverseWinding` — Whether the winding should be reversed. Typically winding is generated anti-clockwise, reversed winding is therefore clockwise.

**Returns:** A handle to the new layer.

#### `AddLayer(ReadOnlySpan{Unity.U2D.Physics.CapsuleGeometry}, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

Add multiple Capsule Geometry layer to the Physics Composer.

**Params:**
- `geometry` — The Capsule Geometry to use. This geometry will be copied so the geometry the span is referring to can be disposed of afterwards if required.
- `transform` — The transform to use on the geometry.
- `operation` — The composer operation to use.
- `order` — The order to perform the composer operation.
- `curveStride` — The curve stride controls how many vertices are used to approximate the curved end-caps of the capsule geometry. Lower values produce more vertices whereas larger values produce fewer vertices. The valid range is [ , 1.0] although values over 0.3 tend to produce relatively poor results.
- `reverseWinding` — Whether the winding should be reversed. Typically winding is generated anti-clockwise, reversed winding is therefore clockwise.

**Returns:** A handle to the new layer.

#### `AddLayer(PolygonGeometry, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

Add a Polygon Geometry layer to the Physics Composer. Note that the physics composer will convert the polygon geometry into outlines (edges) approximating the geometry, therefore if the a non-zero radius is used, multiple edges are used for the radius meaning a single polygon geometry would not be returned if used as the only operation. when using Polygon Geometry with a non-zero radius, the composer will no longer see that as a single polygon but will instead convert the

**Params:**
- `geometry` — The Polygon Geometry to use.
- `transform` — The transform to use on the geometry.
- `operation` — The composer operation to use.
- `order` — The order to perform the composer operation.
- `curveStride` — The curve stride is only used when a non-zero radius is used. It controls how many vertices are used to approximate the curved polygon geometry. Lower values produce more vertices whereas larger values produce fewer vertices. The valid range is [ , 1.0] although values over 0.3 tend to produce relatively poor results.
- `reverseWinding` — Whether the winding should be reversed. Typically winding is generated anti-clockwise, reversed winding is therefore clockwise.

**Returns:** A handle to the new layer.

#### `AddLayer(ReadOnlySpan{Unity.U2D.Physics.PolygonGeometry}, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

Add multiple Polygon Geometry layer to the Physics Composer. Note that the physics composer will convert the polygon geometry into outlines (edges) approximating the geometry, therefore if the a non-zero radius is used, multiple edges are used for the radius meaning a single polygon geometry would not be returned if used as the only operation.

**Params:**
- `geometry` — The Polygon Geometry to use. This geometry will be copied so the geometry the span is referring to can be disposed of afterwards if required.
- `transform` — The transform to use on the geometry.
- `operation` — The composer operation to use.
- `order` — The order to perform the composer operation.
- `curveStride` — The curve stride is only used when a non-zero radius is used. It controls how many vertices are used to approximate the curved polygon geometry. Lower values produce more vertices whereas larger values produce fewer vertices. The valid range is [ , 1.0] although values over 0.3 tend to produce relatively poor results.
- `reverseWinding` — Whether the winding should be reversed. Typically winding is generated anti-clockwise, reversed winding is therefore clockwise.

**Returns:** A handle to the new layer.

#### `AddLayer(PhysicsShape, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

Add a PhysicsShape layer to the Physics Composer. Only PhysicsShape with a geometry of , or will be used. All other types will be ignored.

**Params:**
- `shape` — The PhysicsShape to use.
- `transform` — The transform to use on the geometry.
- `operation` — The composer operation to use.
- `order` — The order to perform the composer operation.
- `curveStride` — The curve stride is used to approximately the curved geometry but is applied according to the specific shape geometry type. Lower values produce more vertices, larger values fewer vertices. The valid range is [ , 1.0] although values over 0.3 tend to produce relatively poor results.
- `reverseWinding` — Whether the winding should be reversed. Typically winding is generated anti-clockwise, reversed winding is therefore clockwise.

**Returns:** A handle to the new layer.

#### `AddLayer(ReadOnlySpan{Unity.U2D.Physics.PhysicsShape}, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

Add multiple PhysicsShape layer to the Physics Composer. Only PhysicsShape with a geometry of , or will be used. All other types will be ignored.

**Params:**
- `shapes` — The PhysicsShapes to use. The geometry these shapes used will be copied so the geometry the span is referring to can changed afterwards if required.
- `transform` — The transform to use on the geometry.
- `operation` — The composer operation to use.
- `order` — The order to perform the composer operation.
- `curveStride` — The curve stride is used to approximately the curved geometry but is applied according to the specific shape geometry type. Lower values produce more vertices, larger values fewer vertices. The valid range is [ , 1.0] although values over 0.3 tend to produce relatively poor results.
- `reverseWinding` — Whether the winding should be reversed. Typically winding is generated anti-clockwise, reversed winding is therefore clockwise.

**Returns:** A handle to the new layer.

#### `AddLayer(ReadOnlySpan{UnityEngine.Vector2}, PhysicsTransform, PhysicsComposer.Operation, int, bool)`

Add a vertices layer to the Physics Composer.

**Params:**
- `vertices` — A span of vertices. This geometry will be copied so the geometry the span is referring to can be disposed of afterwards if required.
- `transform` — The transform to use on the geometry.
- `operation` — The composer operation to use.
- `order` — The order to perform the composer operation.
- `reverseWinding` — Whether the winding should be reversed. Typically winding is generated anti-clockwise, reversed winding is therefore clockwise.

**Returns:** A handle to the new layer.

#### `ClearLayers()`

Remove all layers from the Physics Composer.

#### `Create(Unity.Collections.Allocator)`

Create a Physics Composer. NOTE: The composer implements which allows you to use the "using" statement on the returned composer object to ensure it's disposed in the current scope which is only useful when using the allocator. However, because the composer is a struct, will not be called automatically so in this case disposing or simply calling must be done explicitly.

**Params:**
- `allocator` — The memory allocator to use when adding layers. It is not used to create the composer itself which must be destroyed. This can only be , or .

**Returns:** The new Physics Composer.

#### `CreateChainGeometry(Unity.Collections.NativeArray{UnityEngine.Vector2}, Unity.Collections.Allocator)`

Create from the composition by iterating all the layers added to the composition in the layer order specified, applying each operation specified. A limit is imposed on small vertex distances so be aware that this overload uses a vertex scale of so consider using the overload which allows you to increase this if required.

**Params:**
- `vertices` — The total set of vertices that the chain geometry uses. This must be disposed.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A NativeArray containing the Chain Geometry created from the composer. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateChainGeometry(Unity.Collections.NativeArray{UnityEngine.Vector2}, Vector2, Unity.Collections.Allocator)`

Create from the composition by iterating all the layers added to the composition in the layer order specified, applying each operation specified. A limit is imposed on small vertex distances so it is recommended that scaling is applied here rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `vertices` — The total set of vertices that the chain geometry uses. This must be disposed.
- `vertexScale` — The scaling to be applied to the composer vertices.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A NativeArray containing the Chain Geometry created from the composer. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateConvexHulls(Unity.Collections.Allocator)`

Create from the composition by iterating all the layers added to the composition in the layer order specified, applying each operation specified. A default vertex scale of is used here.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A NativeArray containing the Polygon Geometry convex hull created from the composer. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreateConvexHulls(Vector2, Unity.Collections.Allocator)`

Create from the composition by iterating all the layers added to the composition in the layer order specified, applying each operation specified.

**Params:**
- `vertexScale` — The scaling to be applied to the composer vertices.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A NativeArray containing the Polygon Geometry convex hull created from the composer. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreatePolygonGeometry(Unity.Collections.Allocator)`

Create from the composition by iterating all the layers added to the composition in the layer order specified, applying each operation specified. A limit is imposed on small vertex distances so be aware that this overload uses a vertex scale of so consider using the overload which allows you to increase this if required.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A NativeArray containing the Polygon Geometry created from the composer. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreatePolygonGeometry(Vector2, Unity.Collections.Allocator)`

Create from the composition by iterating all the layers added to the composition in the layer order specified, applying each operation specified. A limit is imposed on small vertex distances so it is recommended that scaling is applied here rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `vertexScale` — The scaling to be applied to the composer vertices.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A NativeArray containing the Polygon Geometry created from the composer. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `CreatePolygonGeometry(Vector2, float, Unity.Collections.Allocator)`

Create from the composition by iterating all the layers added to the composition in the layer order specified, applying each operation specified. A limit is imposed on small vertex distances so it is recommended that scaling is applied here rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `vertexScale` — The scaling to be applied to the composer vertices.
- `radius` — The radius to apply to all generated polygons. Note that this will likely mean that the same polygon region defined by the vertices will not match.
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A NativeArray containing the Polygon Geometry created from the composer. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `Destroy()`

Destroy the Physics Composer.

**Returns:** If the composer was destroyed or not.

#### `DestroyAll()`

Destroy all active Physics Composer.

#### `Dispose()`

Dispose of the composer. This simply calls .

#### `Equals(object)`

#### `Equals(PhysicsComposer)`

#### `GetComposers(Unity.Collections.Allocator)`

Get all the currently active composers.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** All the currently active composers.

#### `GetGeometryIslands(Unity.Collections.Allocator)`

Get the geometry islands from a previous polygon geometry composition i.e. a call to or . Each generated polygon or convex-hull belongs to a unique island defining a set of polygons that are connected together as they share edges. The array returned contains a series of ranges where each range is a unique connected island where the range indicates both the start index and length of the original polygon indices. The number of discovered unique islands is defined by the size of the returned array.

**Params:**
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** A NativeArray containing a series of ranges where each range is uniquely connected island where the range indicates both the start and end indices of the original polygon indices.

#### `GetHashCode()`

#### `operator ==(PhysicsComposer, PhysicsComposer)`

#### `operator !=(PhysicsComposer, PhysicsComposer)`

#### `RemoveLayer(PhysicsComposer.LayerHandle)`

Remove a layer from the Physics Composer.

**Params:**
- `layerHandle` — The layer to remove.

#### `ToPolygons(CircleGeometry, PhysicsTransform, float, Unity.Collections.Allocator)`

Creates multiple from the specified geometry. A limit is imposed on small vertex distances so it is recommended that the geometry is scaled appropriately rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `geometry` — The geometry to convert to polygon geometry.
- `transform` — The transform used to specify where the geometry is positioned.
- `curveStride` — The curve stride only used when creating curves i.e. shapes with a non-zero radius. Lower values produce more vertices, larger values fewer vertices. The valid range is [ , 1.0].
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The created polygon geometries. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `ToPolygons(CapsuleGeometry, PhysicsTransform, float, Unity.Collections.Allocator)`

Creates multiple from the specified geometry. A limit is imposed on small vertex distances so it is recommended that the geometry is scaled appropriately rather than on the returned geometry so geometry is not discarded due to it being invalid.

**Params:**
- `geometry` — The geometry to convert to polygon geometry.
- `transform` — The transform used to specify where the geometry is positioned.
- `curveStride` — The curve stride only used when creating curves i.e. shapes with a non-zero radius. Lower values produce more vertices, larger values fewer vertices. The valid range is [ , 1.0].
- `allocator` — The memory allocator to use for the results. This can only be , or .

**Returns:** The created polygon geometries. This NativeArray must be disposed of after use otherwise leaks will occur. The exception to this is if the array is empty.

#### `ToString()`

### Nested Types

- **Layer** — A composer layer containing individual or spans of shape geometries, active world shapes (from which geometry will be extracted) or contiguous set of vertices.
- **LayerHandle** — A composer layer handle.
- **Operation** — A composer operation.

### Layer

> A composer layer containing individual or spans of shape geometries, active world shapes (from which geometry will be extracted) or contiguous set of vertices.

**Full name:** `Unity.U2D.Physics.PhysicsComposer.Layer`  

#### Properties

| Name | Summary |
|------|---------|
| `curveStride` | The curve stride used when creating curves, in radians. |
| `dataBuffer` | The buffer that contains the geometry, shape or vertex data. |
| `geometryType` | The geometry type in composition. |
| `layerType` | The layer type indicating which buffer contains the layer information. |
| `operation` | The composition operation. |
| `order` | The composition order. |
| `reverseWinding` | Whether the winding should be reversed. Generated shape geometry are normally generated with an anti-clockwise winding however this option will reverse the winding to be clockwise. When tessellation occurs, opposite windings result in holes being generated. |
| `transform` | The transform for the shape. |

#### Methods

##### `new(ReadOnlySpan{Unity.U2D.Physics.CircleGeometry}, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

##### `new(ReadOnlySpan{Unity.U2D.Physics.CapsuleGeometry}, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

##### `new(ReadOnlySpan{Unity.U2D.Physics.PolygonGeometry}, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

##### `new(ReadOnlySpan{Unity.U2D.Physics.PhysicsShape}, PhysicsTransform, PhysicsComposer.Operation, int, float, bool)`

##### `new(Scripting2D.PhysicsBuffer, PhysicsTransform, PhysicsComposer.Operation, int, bool)`

#### Nested Types

- **LayerType** — Indicates the layer type.

#### LayerType

> Indicates the layer type.

**Full name:** `Unity.U2D.Physics.PhysicsComposer.Layer.LayerType`  

##### Fields

| Name | Summary |
|------|---------|
| `Geometry` | The layer is composed of a geometry span indicated by the geometry type. |
| `Shape` | The layer is composed of a shape span. |
| `Vertex` | The layer is composed of a vertex span. |

### LayerHandle

> A composer layer handle.

**Full name:** `Unity.U2D.Physics.PhysicsComposer.LayerHandle`  

#### Methods

##### `ToString()`

### Operation

> A composer operation.

**Full name:** `Unity.U2D.Physics.PhysicsComposer.Operation`  

#### Fields

| Name | Summary |
|------|---------|
| `AND` | Perform an AND operation (geometric intersection). |
| `NOT` | Perform a NOT operation (geometric difference). |
| `OR` | Perform an OR operation (geometric merge). |
| `XOR` | Perform an XOR operation (geometric flip). |

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.5.0b9 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
