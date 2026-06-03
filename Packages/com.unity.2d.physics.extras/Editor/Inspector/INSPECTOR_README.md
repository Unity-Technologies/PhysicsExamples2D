# PhysicsCore Inspector 2D

An EditorWindow that lets you browse and edit the live state of `Unity.U2D.Physics` (PhysicsCore2D) `PhysicsWorld` / `PhysicsBody` / `PhysicsShape` / `PhysicsJoint` instances. Built for Unity 6000.5+ (where the namespace is `Unity.U2D.Physics`; older 6000.3 builds used `UnityEngine.LowLevelPhysics2D` — both are guarded by `#if UNITY_6000_5_OR_NEWER`).

Open via menu: **Tools → PhysicsCoreInspector2D**

---

## Files

```
PhysicsCoreInspector/
├── WorldBrowser.unity              # The scratchpad scene this was developed against (kept here for now)
├── README.md                       # this file
└── Editor/
    ├── PhysicsCoreInspectorEditor.cs    # Main EditorWindow class
    ├── PhysicsInspectorHolder.cs        # Single backing ScriptableObject (see "Holder pattern")
    └── Icons/
        ├── PhysicsWorld.png
        ├── PhysicsBody.png
        ├── PhysicsShape.png
        ├── PhysicsShapeGeometry.png
        ├── PhysicsChain.png
        ├── PhysicsDistanceJoint.png
        ├── PhysicsFixedJoint.png
        ├── PhysicsHingeJoint.png
        ├── PhysicsIgnoreJoint.png
        ├── PhysicsRelativeJoint.png
        ├── PhysicsSliderJoint.png
        └── PhysicsWheelJoint.png
```

Class: `PhysicsCore2DTools.Editor.PhysicsCoreInspectorEditor` (sealed, derives from `EditorWindow`).
Namespace: `PhysicsCore2DTools.Editor`.

The icons in `Editor/Icons/` were copied from `Packages/com.unity.2d.physics.extras/Editor/Icons/` (originally prefixed `Test*`) and renamed. Their `.meta` files were not copied across — Unity assigns fresh GUIDs.

---

## Top button row layout

```
[Select GameObject] [Find Selection] [Auto Select] [spacer] [Auto Read: <off|100|250|500|1000>] [spacer] [Reload]
```

- **Select GameObject** — selects the resolved Unity `GameObject` for the current tree node (via `GetOwner()` or, for a Body, `transformObject`). Disabled when Auto Select is on.
- **Find Selection** — picks the tree node whose owner GameObject matches the current Editor selection. Disabled when Auto Select is on.
- **Auto Select** — bidirectional auto-sync between tree selection and Editor `Selection.activeGameObject`. Persisted in `EditorPrefs`. Default **on**. The `m_LastSyncedGO` tracker breaks ping-pong loops in both directions.
- **Auto Read** — dropdown with `Off / 100ms / 250ms / 500ms / 1000ms`. When non-Off and **in Play mode**, re-renders the currently-selected definition panel at the chosen interval. Persisted in `EditorPrefs`. Default **250ms**. Disabled (greyed out) in Edit mode — users rely on explicit Read buttons there.
- **Reload** — full tree rebuild. Auto-fires on `EnteredEditMode` / `EnteredPlayMode`.

---

## Tree shape

```
[UnityLogo] (root, label empty)
├── [WorldIcon]   World #<handleIdx> (default) : "OwnerGameObject"
│   ├── [InfoIcon]  Counters       — per-world WorldCounters with Read button
│   ├── [InfoIcon]  Profile        — per-world WorldProfile with Read button
│   └── [BodyIcon]  Body #<handleIdx> : "OwnerGameObject"
│       ├── [TransformIcon] Transform: "OwnerGameObject"   (only when body.transformObject != null)
│       ├── [ShapeIcon] PhysicsShape #<handleIdx> (Polygon) : "OwnerGameObject"
│       │   └── [GeometryIcon] PolygonGeometry             — read/write geometry
│       ├── [ChainIcon] PhysicsShape #<handleIdx> (ChainSegment) : "OwnerGameObject"
│       │   └── [GeometryIcon] ChainSegmentGeometry        — read-only (see API gaps)
│       └── [JointIcon] HingeJoint #<handleIdx> → Body #<otherHandleIdx> : "OwnerGameObject"
├── [InfoIcon] Counters (Global)   — PhysicsWorld.globalCounters
└── [InfoIcon] Profile (Global)    — PhysicsWorld.globalProfile
```

- The owner-name suffix `: "Foo"` only appears when `GetOwner()` returns a `GameObject` or a `Component`.
- Joint labels include the connected body's handle index, with `(other world)` if the bodies live in different worlds.
- Empty groups are suppressed (a body with no joints has no joint rows; a body with no shapes has no shape rows).
- Each tree row tooltip is the handle's `ToString()` for World/Body/Shape/Joint, and `Entity ID: <n>` for the Transform node.
- A "deleted" row (the underlying handle has gone invalid since enumeration) gets the **error icon** and a `(Deleted)` suffix, and the right panel shows a tombstone HelpBox. See "Eager deleted-state detection" below.

### Indices

The `#N` index in each label is the **handle index** parsed from the object's `ToString()` output. Until PhysicsCore2D exposes a direct handle-index accessor, `ExtractHandleIndex(string)` walks the string to extract the first digit run. Stable per handle within a world.

---

## Right panel — per-node-kind dispatch

`DisplayInspectorForCurrentSelection()` is the single dispatcher; it calls `m_Inspector.Clear()` then routes by `Node.kind`:

| Kind                | Display method                  | Inspector contents |
|---------------------|---------------------------------|--------------------|
| Root                | `DisplayRoot`                   | Cross-world summary (totals via `world.counters`, `world.awakeBodyCount`). Indented + zebra-striped. |
| World               | `DisplayWorld` → `DisplayDefinition` | `PhysicsWorldDefinition` editable PropertyFields, with **Read** + **Write** buttons. |
| Body                | `DisplayBody` → `DisplayDefinition`  | `PhysicsBodyDefinition` editable PropertyFields. |
| Shape               | `DisplayShape` → `DisplayDefinition` | `PhysicsShapeDefinition` editable PropertyFields. |
| GeometryInfo        | `DisplayShapeGeometry` → `DisplayDefinition` | The matching geometry struct (Circle/Capsule/Polygon/Segment). PolygonGeometry runs `Validate()` before write and shows a modal on failure. **ChainSegment** is read-only — `writeAction` is `null`, hiding the Write button. |
| Joint               | `DisplayJoint`                  | "Jump to Body #N" button at top (selects the same joint under the other body's tree subtree, falling back to just the body); base `PhysicsJoint` properties; per-type extras for `Hinge`/`Fixed`/`Relative`/`Slider`/`Wheel`. **Distance** and **Ignore** joints have no downcast constructor available — only base properties shown. |
| TransformInfo       | `DisplayTransformInfo`          | GameObject name, Entity ID, scene name, hierarchy path. Indented + zebra. |
| WorldCounters       | `DisplayWorldCounters` → `RenderCountersPanel` | Read button + 11-row table of `WorldCounters` fields. Indented + zebra. |
| WorldProfile        | `DisplayWorldProfile` → `RenderProfilePanel`  | Read button + 24-row table of `WorldProfile` field timings (formatted as seconds, F6 precision). Indented + zebra. |
| GlobalCounters      | `DisplayGlobalCounters` → `RenderCountersPanel` | Same shape; uses `PhysicsWorld.globalCounters`. No `awakeBodyCount` row (no static equivalent). |
| GlobalProfile       | `DisplayGlobalProfile` → `RenderProfilePanel`  | Same shape; uses `PhysicsWorld.globalProfile`. |

Definition panels and Counters/Profile use `AddButtonRegion(populate)` to render the Read/Write region above the section title with a subtle bottom border. Data rows live inside `WithIndent(...)` (12px paddingLeft, matching the PropertyField container indent) and `ApplyZebraStripes(container, skipFirstN: 1)` paints alternate rows.

---

## Holder pattern (single ScriptableObject backing every PropertyField)

`PhysicsInspectorHolder.cs` is a `ScriptableObject` with one public field per bindable struct type:

```csharp
public PhysicsWorldDefinition world;
public PhysicsBodyDefinition body;
public PhysicsShapeDefinition shape;
public CircleGeometry circle;
public CapsuleGeometry capsule;
public PolygonGeometry polygon;
public SegmentGeometry segment;
public ChainSegmentGeometry chainSegment;
```

Why one ScriptableObject:
- `PropertyField` only renders the bound property path, so unused fields stay dormant.
- Unity's "No script asset for X. Check that the definition is in a file of the same name." warning fires on any `ScriptableObject`-derived class that isn't in a matching-named file. Eight nested holder classes meant eight warnings; one top-level class in its own file silences them all.

Created via the `CreateHolder<T>` helper:

```csharp
private static T CreateHolder<T>() where T : ScriptableObject
{
    var so = ScriptableObject.CreateInstance<T>();
    so.hideFlags = HideFlags.DontSave;   // NOT HideAndDontSave — that includes NotEditable
    return so;
}
```

**Critical**: do not use `HideFlags.HideAndDontSave` — it includes `NotEditable`, which causes every PropertyField bound to the holder to render as disabled. `HideFlags.DontSave` (which is `DontSaveInBuild | DontSaveInEditor | DontUnloadUnusedAsset`) survives play-mode transitions while keeping the SO editable. The comment in `CreateHolder<T>` flags this footgun.

`BindHolder(holder, propertyPath)`:
- Checks for null/destroyed holder (graceful HelpBox instead of `ArgumentException`).
- Walks the children of the named property and emits a `PropertyField` per child (skips the wrapping struct foldout so users see fields directly).
- The container has `InspectorElement.ussClassName` so `PropertyField` labels participate in the inspector's label-width sync (otherwise labels misalign).
- 12px paddingLeft on the container so foldout chevrons aren't clipped by the panel edge.

---

## DisplayDefinition (the Read/Write button + bind helper)

```csharp
private void DisplayDefinition(
    Node node,
    ScriptableObject holder,
    string headerText,
    System.Func<bool> isValid,
    System.Action readAction,
    System.Action writeAction,    // pass null for read-only sources
    string propertyPath)
```

- Renders an `AddButtonRegion` with **Read** and (if `writeAction != null`) **Write** buttons above the header.
- `OnReadClicked` / `OnWriteClicked` validate via `isValid()` first; if invalid, calls `MarkNodeDeleted(node)` and the row tombstones.
- Calls `readAction()` once during render, then `BindHolder(holder, propertyPath)`.

---

## Owner resolution and bidirectional sync

`OwnerAsGameObject(Object owner)` returns the `GameObject` if the owner is a `GameObject`, the `gameObject` of a `Component` if it's a `Component`, otherwise `null`. (Owners may also be `ScriptableObject`s, `EditorWindow`s, etc. — those return `null`.)

`TryResolveOwnerGameObject(Node)` fans out by `NodeKind` (World/Body/Shape/Joint) calling the appropriate `GetOwner()`, with a Body-only fallback to `transformObject.gameObject` if no Component owner is set.

`TrySelectForEditorSelection()` (used by both the **Find Selection** button and `Refresh()`) does a priority-ordered pass: World → Body → Shape → Joint via owner match, then a final fallback to a Body whose `transformObject.gameObject == Selection.activeGameObject`.

`SelectNodeById(int id)` is the kind-agnostic "expand ancestors → select → centre vertically" helper. Used by Find Selection, Auto Select, and the joint Jump button. The vertical centring (`CenterItemInTree`) computes the visible row index by walking `m_TreeData` and respecting current expansion state, then sets the inner `ScrollView`'s `scrollOffset` directly. Deferred via `m_TreeView.schedule.Execute(...)` so any expand-on-select layout has settled.

---

## Eager deleted-state detection

The `Display*` methods (`DisplayWorld` / `DisplayBody` / `DisplayShape` / `DisplayShapeGeometry` / `DisplayJoint`) all start with:

```csharp
if (!node.<handle>.isValid)
{
    MarkNodeDeleted(node);
    return;
}
```

`MarkNodeDeleted(Node)` flips `node.deleted = true`, calls `m_TreeView.Rebuild()` (so `BindTreeRow` repaints with the error icon and `(Deleted)` suffix), and routes the inspector through the dispatcher's `node.deleted` guard which shows the tombstone HelpBox. The dispatcher's guard prevents recursion: once deleted, the type-specific Display method isn't re-entered.

This means an invalid handle is detected and made visible:
- on selection,
- on manual Read or Write,
- on every Auto Read tick,
- on every AutoRefresh tick (Counters/Profile).

---

## Auto-refresh schedules

Two independently-scheduled tasks bound to `m_Inspector` (so they die with the window):

1. **`AutoRefreshTick`** (always-on, **250ms**): when a Counters/Profile/Global Counters/Global Profile node is selected, re-renders the panel. Skipped while `EditorApplication.isPaused`.

2. **`AutoReadTick`** (variable interval, set by the **Auto Read** dropdown): when a definition node is selected (`World`/`Body`/`Shape`/`GeometryInfo`), re-renders the panel. Skipped in Edit mode (`!EditorApplication.isPlaying`) and while paused. The scheduled item handle is stored in `m_AutoReadScheduledItem`; `ApplyAutoReadInterval(int)` calls `Pause()` when the user selects "Off" and `Every(intervalMs).Resume()` otherwise.

---

## EditorPrefs keys

| Key                                                                            | Type | Default | Notes |
|--------------------------------------------------------------------------------|------|---------|-------|
| `PhysicsCore2DTools.PhysicsCoreInspectorEditor.AutoSelectGameObject`           | bool | `true`  | Auto Select toggle |
| `PhysicsCore2DTools.PhysicsCoreInspectorEditor.AutoReadInterval`               | int  | `250`   | Auto Read period in ms (0 = off) |

If the namespace changes during package migration, the keys need to change too — existing installations will reset to defaults.

---

## Known PhysicsCore2D API gaps

These are workarounds in the current code; revisit when the API closes them:

1. **No `PhysicsJoint.definition`.** World/Body/Shape have a get/set `definition`; PhysicsJoint does not. Joint inspector shows individual properties.
2. **`PhysicsDistanceJoint` and `PhysicsIgnoreJoint` lack a `new(PhysicsJoint)` downcast constructor.** Hinge/Fixed/Relative/Slider/Wheel have one. The two unconstructed types fall back to base properties only.
3. **No `PhysicsBody.GetChains()` or `PhysicsWorld.GetChains()`.** Chains aren't enumerable top-down. ChainSegment shapes appear in the body's shape list and carry `shape.chain` back to the owning chain. Plan to add a Chain node tier when enumeration lands.
4. **`PhysicsShape.chainSegmentGeometry` is read-only.** Setter coming soon (per Unity's roadmap). When it lands, swap the `null` writeAction in `DisplayShapeGeometry`'s `ChainSegment` case for `() => node.shape.chainSegmentGeometry = m_Holder.chainSegment;` and drop the `(read-only)` suffix.
5. **No direct handle-index accessor.** Currently parsed from `handle.ToString()` via `ExtractHandleIndex`. When a real accessor lands, swap the implementation.
6. **No "find body by Transform" API.** `TrySelectForEditorSelection` does an O(N) scan across all worlds and bodies.
7. **No "world contents changed" event.** The browser relies on the Reload button + auto-Refresh on play-mode transitions.
8. **No stable user-visible ID for selection persistence across Refresh.** Uses handle struct equality; works as long as the underlying physics handle is stable across operations between two refreshes.

---

## Migration to a package — checklist

When moving this folder into a UPM package (e.g. `com.unity.2d.physics.inspector`):

1. **Folder move**: copy `Editor/` (the two `.cs` files + `Icons/`) into the package's `Editor/` folder. Bring `.meta` files to preserve GUIDs (script asset references, scene references will survive).
2. **Asmdef**: add an `Editor`-platform `.asmdef` referencing the PhysicsCore2D module. Currently the project relies on the implicit `Assembly-CSharp-Editor`. Reference `Unity.U2D.Physics` as needed and confirm `PlatformOS = Editor`.
3. **Update `IconFolder`**: the constant in `PhysicsCoreInspectorEditor.cs` is now `"Packages/com.unity.2d.physics.extras/Editor/Inspector/Icons/"`. If the package id changes, update this path. A more robust alternative is to load icons via `MonoScript.FromScriptableObject` to compute the path relatively, or register them and load via `EditorGUIUtility.Load`.
4. **Namespace**: consider renaming `PhysicsCore2DTools.Editor` to match the package's namespace (e.g. `Unity.U2D.Physics.Inspector.Editor`). If the namespace changes, the EditorPrefs keys above also need updating to avoid orphaned settings.
5. **README**: this file should move with the package as its top-level README.
6. **Scene file**: `WorldBrowser.unity` is the scratchpad scene the tool was developed against; **leave it in the user's project**, not in the package. The window has no dependency on it.
7. **Sample**: optionally include a `Samples~/` folder in the package with a small scene + a script that creates a few PhysicsBodies/Joints owned by GameObjects, so users can try the tool out of the box.
8. **Verify after move**: open Unity, ensure the menu `Tools → PhysicsCoreInspector2D` still appears, the icons render correctly in the tree, EditorPrefs are read/written, and the window opens without "No script asset for X" warnings.

---

## Top-of-file imports (for quick package-side reference)

```csharp
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
```

---

## Quick test plan after moving

1. Open `Tools → PhysicsCoreInspector2D`. Tree should show `World #<idx> (default)` with `Counters` and `Profile` children, plus the two global rows below.
2. In Edit mode, the `Auto Read` dropdown is greyed out. Clicking `Reload` rebuilds.
3. Play a scene that creates a Dynamic body owned by a `MonoBehaviour`. Confirm the body row reads e.g. `Body #5 : "MyComponentName"`. Hover the row — tooltip shows `handle.ToString()`.
4. With `Auto Select` on, clicking a body in the tree selects + pings the GameObject; selecting a different GameObject in the Hierarchy moves the tree selection back. No infinite loop.
5. With `Auto Read` set to `250ms` in Play mode, the body's `position` field ticks live.
6. Destroy the body programmatically — the row flips to the error icon with `(Deleted)` suffix; the right panel shows the tombstone.
7. Click `Reload` — the tombstoned row is gone (since the body no longer enumerates).
