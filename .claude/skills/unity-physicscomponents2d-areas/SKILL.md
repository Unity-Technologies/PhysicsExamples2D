---
name: unity-physicscomponents2d-areas
description: The area family of com.unity.2d.physics — the nine PhysicsArea components that attach shapes to a pose's body. Covers the geometry/geometryAsset/activeGeometry pattern, ApplyGeometry versus ApplyDefinition, the compound transformation and scaleRadius, contour output as polygons or segments, sprite-driven areas, and PhysicsAreaComposite's layer model. Use for questions about adding or changing shapes via components, why a geometry edit had no effect, sprite outlines, contour decomposition, or combining geometry from several sources.
---

# Area family

An area attaches **one or more `PhysicsShape`** to the body of the `PhysicsPose` that owns it.
Areas derive `PhysicsPoseProvider`, run at `PhysicsExecutionOrder.Area` (after poses, before constraints), and are the only components that create shapes.

Unlike a pose, an area is **not** `[DisallowMultipleComponent]`: stack several on one GameObject to build a compound object.

## The nine components

| Component | Produces | Base |
|---|---|---|
| `PhysicsAreaCircle` | one circle | `PhysicsAreaPrimitiveBase` |
| `PhysicsAreaCapsule` | one capsule | `PhysicsAreaPrimitiveBase` |
| `PhysicsAreaPolygon` | one polygon | `PhysicsAreaPrimitiveBase` |
| `PhysicsAreaSegment` | one segment | `PhysicsAreaPrimitiveBase` |
| `PhysicsAreaPrimitive` | one shape, type switchable at author time | `PhysicsAreaPrimitiveBase` |
| `PhysicsAreaContour` | many, from a contour group | `PhysicsAreaContourBase` |
| `PhysicsAreaSprite` | many, from a sprite's outline | `PhysicsAreaContourBase` |
| `PhysicsAreaPath` | many, a run of chain segments | `PhysicsArea` |
| `PhysicsAreaComposite` | many, from merged layers | `PhysicsArea` |

`PhysicsAreaPrimitive` is the general one: a `shapeType` selector plus a separate geometry field per type (`circleGeometry`, `capsuleGeometry`, and so on), each with its own asset and `active*Geometry` resolver. The four single-type components exist so the inspector shows only the fields that matter.

## Reaching the shapes

An area is an `IEnumerable<PhysicsShape>`, which matters because most areas produce more than one.

| Member | Meaning |
|---|---|
| `shape` | the first shape, or an invalid handle when there are none |
| `this[int]` | indexer |
| `shapeCount`, `hasShapes` | count and emptiness |
| `foreach (var s in area)` | iterate, via a struct enumerator (no allocation) |
| `ShapeCreated` / `ShapeDestroyed` | events |

Use `shape` only where you know the area is single-shape. On a contour or composite it silently gives you one of many.

## Geometry: inline, asset, resolved

Every area follows the same three-property pattern:

| Property | Meaning |
|---|---|
| `geometry` | the inline geometry struct |
| `geometryAsset` | an optional shared `Physics*Geometry` asset that overrides it |
| `activeGeometry` | read-only, the one actually in effect |

The asset wins when assigned. Assets live in `Unity.U2D.Physics.Assets` and are created from **Assets > Create > 2D > Physics (Core)**.

## Two different applies

This is the distinction that catches people, because both look like "update the shape":

| Changed | Call | Effect |
|---|---|---|
| geometry, `transformation`, `scaleRadius`, contour output settings | `ApplyGeometry()` | in-place update for a single-shape primitive, otherwise a shape rebuild |
| `definition`, `definitionAsset`, `contactAsset`, `materialAsset` | `ApplyDefinition()` | in place, shapes stay alive |
| `source`, `pose` | `Apply()` | re-resolves the owning body |

**Geometry is not part of the definition.** A shape definition carries the filter, material, density and flags; the geometry is separate. That is why they have separate apply methods, and why calling the wrong one appears to do nothing.

`ApplyGeometry()` is virtual: a single-shape primitive overrides it to update its one shape in place rather than recreating it, since the shape count cannot change. The base implementation rebuilds, which is what a contour or composite needs because their shape count varies with the input.

Both are no-ops when no shapes exist.

## Definition, with two targeted overrides

Areas have the usual `definition` / `definitionAsset` / `activeDefinition` trio, plus two extra asset slots that override just one field each:

- `contactAsset` (`PhysicsContactFilter`) replaces the contact filter.
- `materialAsset` (`PhysicsSurfaceMaterial`) replaces the surface material.

These apply **on top of** `activeDefinition` regardless of whether it came from the asset or the inline definition. So you can share one definition asset across many areas and still vary the material per area.

## The compound transformation

`transformation` (a `PhysicsCompoundTransform`) is a placement layered on top of the GameObject hierarchy: a position and rotation offset, a uniform `scale`, and a `scaleRadius` flag. The default is a no-op.

`scaleRadius` decides whether the scale also scales a rounded geometry's radius, or the radius keeps its authored size. It is exposed directly on the area as a convenience for `transformation.scaleRadius`.

Scale is clamped to zero or above.

Changing any of it needs `ApplyGeometry()`.

## Transform changes rebuild in edit mode only

Areas watch a wider set of Transform changes than poses do, because geometry depends on scale and hierarchy as well as position and rotation.

In **edit mode** any of those triggers a geometry rebuild. In **play** the shape rides the body, so no rebuild happens. That difference is deliberate, not a bug to work around.

## Contour areas

`PhysicsAreaContourBase` turns a contour group into shapes, and `output` decides how:

| `OutputType` | Result |
|---|---|
| `Polygons` | filled convex polygons; concavity and holes resolved by decomposition and winding |
| `Segments` | hollow, one-sided chain segments, one closed run per contour |

Supporting settings: `maxPolygonVertices`, `useDelaunay` (decomposition strategy), `reverseWinding`, and `rejectedShapeCount` for how many candidate shapes were discarded as degenerate.

Two concrete components:

- `PhysicsAreaContour` holds a `ContourGroupGeometry` inline or via a `PhysicsContourGroupGeometry` asset.
- `PhysicsAreaSprite` derives its contour from a sprite. It has a `spriteSource` (its own reference or a `SpriteRenderer`), a `spriteRendererUpdate` mode for following renderer changes, and `flipX`/`flipY`.

Sprite physics outlines are authored in the Sprite Editor, and only regenerate in play mode or at import, not from edit-mode scripting.

## PhysicsAreaComposite

The one area that takes geometry from **several sources and merges them** through a composer, emitting the result as shapes on its own body.

Its `geometry` is a `CompositeGeometry`: a mutable list of `CompositeLayer` with `Add`, `Insert`, `RemoveAt`, `Clear`, `layerCount`, an indexer, and `isValid`. As with other areas there is a `geometryAsset` (`PhysicsCompositeGeometry`) and a resolved `activeGeometry`.

Each `CompositeLayer` has a `type`:

| `LayerType` | Supplies |
|---|---|
| `Geometry` | one shape geometry or a contour group, inline or from an asset |
| `Pose` | **every area on a referenced `PhysicsPose`, taken as one unit** |

A layer carries its own `transformation`, `scale`, `scaleRadius`, per-layer composite settings, and an `enabled` flag, so you can position and toggle contributions independently.

The rule that bites: **a `Pose` layer's referenced pose must have `compositeMode` enabled.** That flag stops the pose creating a body of its own so its areas feed the composite instead. A referenced pose not in composite mode makes the layer invalid.

Composite-wide `settings` (a `CompositeSettings`) carry `output`, `useDelaunay` and `maxPolygonVertices` for the merged result.

## Where to go next

For the shape API itself (filters, materials, contacts, casting against it) see `unity-physicscore2d-shapes` and `unity-physicscore2d-filtering`.
For chains, compounds and runtime geometry mutation at the engine level see `unity-physicscore2d-shapes-advanced`.
For the shared `source`/`Apply` model and the bulk-edit recipe see `unity-physicscomponents2d-providers`.
