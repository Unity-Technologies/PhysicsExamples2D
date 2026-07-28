---
name: unity-physicscore2d-layers-api
description: Authoritative Unity 6000.7 PhysicsCore2D API reference for Layers & Masks. Lists every type, property, field, method (with signatures, params, returns) for: PhysicsLayers, PhysicsMask. Use whenever working with these types in code.
---

# Unity PhysicsCore2D API — Layers & Masks

This skill is the auto-generated API surface for the listed types. It pre-dates Claude's training data on Unity 6000.7, so it should be treated as the source of truth for member names, signatures, and documentation strings.

_Generated from Unity 6000.7.0a3 `UnityEngine.PhysicsCore2DModule.xml`._

Top-level types in this file: `PhysicsLayers`, `PhysicsMask`.

## PhysicsLayers

> This provides a common method to retrieving layer information. If a [PhysicsCoreSettings2D](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D.html) asset is assigned then the full layers ([PhysicsCoreSettings2D.physicsLayerNames](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-physicsLayerNames.html)) will be used if [PhysicsCoreSettings2D.usePhysicsLayers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-usePhysicsLayers.html) is also active. If no [PhysicsCoreSettings2D](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D.html) asset is assigned then the global layers (See [UnityEngine.LayerMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.LayerMask.html)) will be used.

**Full name:** `Unity.U2D.Physics.PhysicsLayers`  
**Docs:** [Unity.U2D.Physics.PhysicsLayers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsLayers.html)

### Fields

| Name | Summary |
|------|---------|
| `InvalidLayerOrdinal` | Indicates an invalid layer ordinal. This is typically used when retrieving a layer ordinal but a name could not be found. |

### Methods

#### `GetLayerMask(string[])`

Get a [PhysicsMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html) for the specified layer name(s).

**Params:**
- `layerNames` — The layer names (case sensitive) to find a combined physics mask for.

**Returns:** The combined physics mask associated with the specified layer names or, if not found, [PhysicsMask.None](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask-None.html) will be returned in which case a console warning will also be produced.

#### `GetLayerName(int)`

Get a layer name for the specified layer ordinal (index).

**Params:**
- `layerOrdinal` — The layer ordinal (index). When using the full layers this should be within the range [0, 63] however if not then the range must be [0, 31].

**Returns:** The layer name. If no layer name is present then [System.String.Empty](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/System.String-Empty.html) is returned.

#### `GetLayerOrdinal(string)`

Get a layer ordinal (index) for the specified layer name. This is not a 32-bit mask but simply the layer ordinal (index) associated with the specified layer name.

**Params:**
- `layerName` — The layer name (case sensitive) to find the layer ordinal for.

**Returns:** The layer ordinal associated with the specified layer name or, if not found, [PhysicsLayers.InvalidLayerOrdinal](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsLayers-InvalidLayerOrdinal.html) will be returned in which case a console warning will also be produced.

### Nested Types

- **LayerNames** — —

### LayerNames

**Full name:** `Unity.U2D.Physics.PhysicsLayers.LayerNames`  

#### Methods

##### `new()`

##### `OnAfterDeserialize()`

##### `OnBeforeSerialize()`

## PhysicsMask

> A 64-bit mask, effectively 64 flags. The default enumerator will iterate all the bits that are set (1).

**Full name:** `Unity.U2D.Physics.PhysicsMask`  
**Docs:** [Unity.U2D.Physics.PhysicsMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html)

### Fields

| Name | Summary |
|------|---------|
| `All` | All 64 bits set (1) in the PhysicsMask. |
| `bitMask` | A 64-bit mask, effectively 64 flags. |
| `None` | No bits set in the PhysicsMask, effectively zero. |
| `One` | Bit #0 set (1) in the PhysicsMask. The remaining bits are reset (0). |

### Properties

| Name | Summary |
|------|---------|
| `resetBits` | Gets an enumerable group of bits that are currently reset (0). The bits are returned in ascending bit-index order. This uses [PhysicsMask.ResetBitIterator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.ResetBitIterator.html). |
| `setBits` | Gets an enumerable group of bits that are currently set (1). The bits are returned in ascending bit-index order. This uses [PhysicsMask.SetBitIterator](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.SetBitIterator.html). |

### Methods

#### `new(int[])`

Create a PhysicsMask by specifying multiple bits to set (1).

**Params:**
- `bitIndices` — The indices of the bits to set in the mask. An index must be in the range [0, 63].

#### `new(LayerMask)`

Create a PhysicsMask from a LayerMask. A [UnityEngine.LayerMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.LayerMask.html) is only 32-bits wide so the PhysicsMask will have the upper 32-bits set to zero.

**Params:**
- `layerMask` — The LayerMask to use.

#### `AreAnyBitsSet(PhysicsMask)`

Checks if any of the provided PhysicsMask set bits are also set in this PhysicsMask.

**Params:**
- `physicsMask` — The PhysicsMask bits to compare to this PhysicsMask. If this is zero, false will always be returned.

**Returns:** True if any set bits in the specified PhysicsMask are also set in this PhysicsMask, false otherwise.

#### `AreBitsSet(PhysicsMask)`

Checks if all the provided PhysicsMask set bits are also set in this PhysicsMask.

**Params:**
- `physicsMask` — The PhysicsMask bits to compare to this PhysicsMask. If this is zero, false will always be returned.

**Returns:** True if all bits in the specified PhysicsMask are also set in this PhysicsMask, false otherwise.

#### `GetEnumerator()`

#### `IsBitSet(int)`

Is the specified bit set.

**Params:**
- `bitIndex` — The bit index in the range [0, 63].

**Returns:** Whether the specified bit is set or not.

#### `operator implicit()`

#### `operator implicit()`

#### `ResetBit(int)`

Reset (0) the specified bit.

**Params:**
- `bitIndex` — The bit index in the range [0, 63].

#### `SetBit(int)`

Set (1) the specified bit.

**Params:**
- `bitIndex` — The bit index in the range [0, 63].

#### `ToLayerMask()`

Convert the lower 32-bits of the 64-bit mask to the 32-bit [UnityEngine.LayerMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.LayerMask.html). A [UnityEngine.LayerMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/UnityEngine.LayerMask.html) is only 32-bits wide so the upper 32-bits of the PhysicsMask will be ignored.

**Returns:** A 32-bit layer-mask converted from the lower 32-bits of the 64-bit mask.

#### `ToString()`

### Nested Types

- **ResetBitIterator** — An iterator that will iterate only the bits that are reset (0) in a [PhysicsMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html)
- **SetBitIterator** — An iterator that will iterate only the bits that are set (1) in a [PhysicsMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html)
- **ShowAsPhysicsMaskAttribute** — When applied to a field/property of type [PhysicsMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html), the field/property drawer will not be display it as [PhysicsLayers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsLayers.html). Instead, the field/property will be displayed as bit numbers only i.e. a raw 64-bit mask allowing each bit to be (de)selected. This is only used when physics layers are active (see [PhysicsCoreSettings2D.usePhysicsLayers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-usePhysicsLayers.html)).

### ResetBitIterator

> An iterator that will iterate only the bits that are reset (0) in a [PhysicsMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html)

**Full name:** `Unity.U2D.Physics.PhysicsMask.ResetBitIterator`  

#### Methods

##### `new(PhysicsMask)`

##### `GetEnumerator()`

### SetBitIterator

> An iterator that will iterate only the bits that are set (1) in a [PhysicsMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html)

**Full name:** `Unity.U2D.Physics.PhysicsMask.SetBitIterator`  

#### Methods

##### `new(PhysicsMask)`

##### `GetEnumerator()`

### ShowAsPhysicsMaskAttribute

> When applied to a field/property of type [PhysicsMask](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.html), the field/property drawer will not be display it as [PhysicsLayers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsLayers.html). Instead, the field/property will be displayed as bit numbers only i.e. a raw 64-bit mask allowing each bit to be (de)selected. This is only used when physics layers are active (see [PhysicsCoreSettings2D.usePhysicsLayers](https://docs.unity3d.com/6000.7/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsCoreSettings2D-usePhysicsLayers.html)).

**Full name:** `Unity.U2D.Physics.PhysicsMask.ShowAsPhysicsMaskAttribute`  

#### Methods

##### `new()`

---

_Generated by `~/.claude/physicscore2d-api-generator/_generate.py` from Unity 6000.7.0a3 `UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the generator._
