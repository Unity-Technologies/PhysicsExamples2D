# Creating & Modifying Sandbox Examples (LLM Guide)

This document tells an automated agent (LLM) everything it needs to **add a new example
scene to the Sandbox project, and to modify an existing one, purely by editing files** —
without a running Unity Editor to wire references via drag-and-drop.

It is the authoritative recipe for this project. Read it fully before creating files.

> **PhysicsCore2D rules still apply.** This project is **PhysicsCore2D only**
> (`Unity.U2D.Physics`). Never use the legacy `Physics2D` component system
> (`Rigidbody2D`, `Collider2D`, …). Verify every PhysicsCore2D API against the
> bundled `unity-physicscore2d-*` skills — never guess a signature, never use an
> `[Obsolete]` member. See `PhysicsCore2D/CLAUDE.md`.

---

## 1. What an "example" is

Each example is a **self-contained scene** living in its own category folder:

```
Assets/Scenes/<Category>/<Name>.meta  ← folder GUID (Unity needs every folder to have one)
Assets/Scenes/<Category>/<Name>/
├── <Name>.cs            ← MonoBehaviour driving the example
├── <Name>.cs.meta       ← asset GUID for the script
├── <Name>Menu.uxml      ← per-example options UI (UI Toolkit)
├── <Name>Menu.uxml.meta ← asset GUID for the uxml
├── <Name>.unity         ← the scene asset
└── <Name>.unity.meta    ← asset GUID for the scene
```

> **Folder `.meta`:** every folder is itself an asset, so the new `<Name>` folder needs a
> sibling `<Name>.meta`. If you ever open the project in the Editor it will auto-generate
> this; but in a pure file-only workflow (committing without launching Unity) you should
> author it yourself with its own fresh GUID so git state is deterministic — see Step 1b.

Scenes are **loaded additively at runtime** by the shared infrastructure that lives in
`Assets/Sandbox.unity` (the startup scene). That infrastructure provides:

| Component / type | Role |
|---|---|
| `SandboxManager` | Global controls (pause/step/reset/colors/debug), world draw options, shared deterministic `Random`, per-scene reset hook. Lives on the same GameObject as `SceneManifest` + the main-menu `UIDocument`. |
| `SceneManifest`  | The registry of all examples (`SceneItems` list). Loads/unloads scenes additively by build index. |
| `CameraManipulator` | Drives the real runtime camera (pan/zoom/drag/explode). Each example sets its initial framing. |

**Rendering:** examples do **not** need a `SpriteRenderer`/`GameObject` per body. The
PhysicsCore2D **debug renderer draws all shapes/joints automatically** (the SandboxManager
enables draw options). You create bodies/shapes and they appear. Use a Unity `Transform`
(via `body.transformObject`) only when you specifically want a sprite/visual to track a body.

There is a ready-made blank template example: **`Assets/Scenes/Custom/`** (class `Custom`).
The fastest way to author a new example is to **copy `Custom` and rename**, then edit.

---

## 2. Two registrations are mandatory

Creating the files is not enough. A scene only appears (and only loads) if it is in **both**:

1. **The build list** — `ProjectSettings/EditorBuildSettings.asset`
   (the scene must be in the player build; `SceneManifest` loads it by build index).
2. **The manifest** — the `SceneItems` list on the `SceneManifest` component inside
   `Assets/Sandbox.unity` (this is what populates the category/scene dropdowns and supplies
   the description).

Miss either one and the example will be invisible or fail to load.

---

## 3. The invariant chain (why GUIDs matter)

Because we are authoring without the Editor, **GUIDs must be hand-managed and kept
consistent**. The links that must line up:

```
<Name>.cs.meta:guid ────────────► <Name>.unity  m_Script.guid  (the MonoBehaviour component)
<Name>Menu.uxml.meta:guid ──────► <Name>.unity  sourceAsset.guid (the UIDocument)
<Name>.unity.meta:guid ─────────► EditorBuildSettings.asset  guid (build list entry)
class <Name>  (in <Name>.cs) ───► <Name>.unity  m_EditorClassIdentifier: Assembly-CSharp::<Name>
<Name>.unity ScenePath ─────────► Sandbox.unity SceneItems[].ScenePath  AND  build list path
```

There is **no asmdef** in this project, so all example scripts compile into the default
`Assembly-CSharp`. The `m_EditorClassIdentifier` is therefore always
`Assembly-CSharp::<ClassName>` (or empty — Unity fills it — but set it to be safe).

### Generating GUIDs

A Unity asset GUID is **32 lowercase hex characters**, no dashes. Generate three fresh,
unique ones (script, uxml, scene). Any of:

```bash
python -c "import uuid;print(uuid.uuid4().hex)"      # repeat 3×
# or
openssl rand -hex 16
# or (PowerShell)
powershell -Command "[guid]::NewGuid().ToString('N')"
```

Never reuse a GUID that already exists in the project.

---

## 4. Shared constants (copy verbatim)

These fileIDs/GUIDs are **identical in every example scene** and are NOT per-asset. Use them
as-is in the scene template:

| Reference | Value |
|---|---|
| `UIDocument` built-in script | `m_Script: {fileID: 19102, guid: 0000000000000000e000000000000000, type: 0}` |
| Shared `PanelSettings` | `m_PanelSettings: {fileID: 11400000, guid: b3cc2d097ffb6c846acc69277ba50b67, type: 2}` |
| UXML `sourceAsset` main-object fileID | `fileID: 9197481963319205126` (the `guid` is your uxml's guid, `type: 3`) |
| `SandboxStyleOverrides.uss` (referenced by uxml) | `guid: e140b35909eadcf48959626f553b2145` |
| UXML importer script (in `.uxml.meta`) | `script: {fileID: 13804, guid: 0000000000000000e000000000000000, type: 0}` |

---

## 5. Step-by-step recipe (add a new example)

**Step 0 — Decide naming.**
- `Category`: free-text grouping for the dropdown (existing: `Benchmarks`, `Collision`,
  `Joints`, `Shapes`, `Stacking`, `Batching`, `Custom`). Reuse one or invent a new one.
- `Name` (display, in the manifest): human spaced, e.g. `My Example`.
- `<ClassName>` / file stem: PascalCase, no spaces, e.g. `MyExample`. The `.cs`, `.uxml`
  (as `MyExampleMenu.uxml`), `.unity`, and folder all use this stem.

**Step 1 — Generate GUIDs.** You need three for the assets (`GUID_CS`, `GUID_UXML`,
`GUID_SCENE`) and one for the folder (`GUID_FOLDER`).

**Step 1b — Create the folder `.meta`** at `Assets/Scenes/<Category>/<Name>.meta`:
```yaml
fileFormatVersion: 2
guid: GUID_FOLDER
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
```

**Step 2 — Create `Assets/Scenes/<Category>/<Name>/<Name>.cs`** from the template in §8,
replacing the class name. Create `<Name>.cs.meta`:
```yaml
fileFormatVersion: 2
guid: GUID_CS
```

**Step 3 — Create `<Name>Menu.uxml`** from the template in §8 (rename control elements as
needed). Create `<Name>Menu.uxml.meta`:
```yaml
fileFormatVersion: 2
guid: GUID_UXML
ScriptedImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 2
  userData: 
  assetBundleName: 
  assetBundleVariant: 
  script: {fileID: 13804, guid: 0000000000000000e000000000000000, type: 0}
```

**Step 4 — Create `<Name>.unity`** from the scene template in §8. Swap in:
- the MonoBehaviour `m_Script.guid` → `GUID_CS`
- `m_EditorClassIdentifier: Assembly-CSharp::<ClassName>`
- the UIDocument `sourceAsset.guid` → `GUID_UXML`

Create `<Name>.unity.meta`:
```yaml
fileFormatVersion: 2
guid: GUID_SCENE
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
```

**Step 5 — Register in the build list.** In `ProjectSettings/EditorBuildSettings.asset`,
add an entry to `m_Scenes` (order is not significant for loading-by-path):
```yaml
  - enabled: 1
    path: Assets/Scenes/<Category>/<Name>/<Name>.unity
    guid: GUID_SCENE
```

**Step 6 — Register in the manifest.** In `Assets/Sandbox.unity`, inside the `SceneItems:`
list of the `SceneManifest` MonoBehaviour, add (indentation is two spaces for `- Name:`,
matching existing entries):
```yaml
  - Name: <Display Name>
    Category: <Category>
    Description: <One or two sentence description shown in the scene panel.>
    ScenePath: Assets/Scenes/<Category>/<Name>/<Name>.unity
```

**Step 7 — (Optional, for iteration) Set the start scene.** In `Assets/Sandbox.unity`, the
`SandboxManager` MonoBehaviour has `StartScene: Barrel`. Change it to your `<Display Name>`
so Play Mode boots straight into your example. (Remember to revert before committing if the
default should stay `Barrel`.)

**Step 8 — Done.** When the Editor next imports, the scene loads from the category dropdown.
There is nothing else to wire — the example finds the shared infra at runtime via
`FindAnyObjectByType<…>()`.

---

## 6. The runtime contract (what `<Name>.cs` MUST do)

Every example MonoBehaviour follows this lifecycle. Deviating breaks the shared controls.

```csharp
private void OnEnable()
{
    // 1. Cache the shared infra (it lives in the always-loaded Sandbox.unity).
    m_SandboxManager  = FindAnyObjectByType<SandboxManager>();
    m_SceneManifest   = FindAnyObjectByType<SceneManifest>();
    m_UIDocument      = GetComponent<UIDocument>();
    m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();

    // 2. Hand your options UI to the manager so it is shown/toggled with the global UI.
    m_SandboxManager.SceneOptionsUI = m_UIDocument;

    // 3. Frame the camera for this example.
    m_CameraManipulator.CameraSize = 12f;
    m_CameraManipulator.CameraPosition = Vector2.zero;

    // 4. Register the reset hook — invoked when the user presses Reset (R) in the menu.
    //    It must fully rebuild your scene from scratch.
    m_SandboxManager.SceneResetAction = SetupScene;

    // 5. Initialise your tunable state, build the options UI, then build the scene.
    SetupOptions();
    SetupScene();
}

private void OnDisable()
{
    // Restore ANY PhysicsWorld / SandboxManager override you changed in this example
    // (gravity, contact params, draw-option overrides, color overrides). The next scene
    // expects canonical state. If you changed nothing global, this can be empty.
}

private void SetupScene()
{
    m_SandboxManager.ResetSceneState();   // ALWAYS first: clears non-owned bodies, resets
                                          // the world + the shared Random to a fixed seed.
    var world = PhysicsWorld.defaultWorld;
    // ... create ground + content here ...
}
```

Key rules:
- **`SetupScene()` must be idempotent** — it is called on first load *and* on every Reset,
  and often when a structural option changes. Always call `ResetSceneState()` first so you
  start from a clean world; do not assume bodies from a previous run are gone.
- **Determinism:** use `m_SandboxManager.Random` (a shared `Unity.Mathematics.Random`,
  re-seeded by `ResetSceneState()`), not `UnityEngine.Random`.
- **Shape colours:** set `shapeDefinition.surfaceMaterial.customColor =
  m_SandboxManager.ShapeColorState;` so the global "Colors" toggle works. `ShapeColorState`
  returns a default colour when the toggle is off, or a random HSV colour when on.
- **Do not create/destroy bodies during the physics simulation step** (WORM rule). Do it in
  `OnEnable`/`SetupScene`/option callbacks/`Update`, not inside a physics callback.

### `SandboxManager` API an example may use

| Member | Purpose |
|---|---|
| `SceneOptionsUI` (set) | Register your `UIDocument` so it shows/hides with the global UI. |
| `SceneResetAction` (set) | Delegate invoked on Reset — point it at your `SetupScene`. |
| `ResetSceneState()` | Destroys all non-owned bodies in all worlds, clears debug draw, re-seeds `Random`, resets the default world. Call at the top of `SetupScene`. |
| `ref Random Random` | Shared deterministic RNG. |
| `Color ShapeColorState` | Per-shape colour honouring the global Colors toggle. |
| `SetOverrideDrawOptions(...)` / `ResetOverrideDrawOptions()` | Force specific debug-draw flags for your example; reset in `OnDisable`. |
| `SetOverrideColorShapeState(bool)` / `ResetOverrideColorShapeState()` | Force the colour state; reset in `OnDisable`. |
| `ShowFPS()` / `HideFPS()` | FPS overlay. |

### `CameraManipulator` members an example may use

`CameraSize`, `CameraPosition`, `CameraZoom`, `OverlapUI` (++/-- on pointer enter/leave of
your menu region so camera input is suppressed over UI), `ResetPanZoom()`, `ExplodeImpulse`,
`TouchMode`.

---

## 7. The UXML contract (`<Name>Menu.uxml`)

The options panel is UI Toolkit. Required structure (see template in §8):
- Root references `SandboxStyleOverrides.uss` (guid `e140b35909eadcf48959626f553b2145`).
- A `VisualElement` named **`menu-region`** (the panel; ~18% width). Your `.cs` wires its
  `PointerEnter`/`PointerLeave` to `CameraManipulator.OverlapUI`.
- A `Label` named **`scene-description`** — your `.cs` sets its text to the manifest name +
  description.
- Each control you query in `.cs` via `root.Q<T>("element-name")` must have a matching
  `name="element-name"` here. Control name ↔ `Q<>` name must agree exactly.

Common controls: `SliderInt`, `Slider`, `EnumField`, `Toggle`, `DropdownField`, `Button`.
Structural options (count, type) typically call `SetupScene()` in their change callback to
rebuild; live options (gravity, contact params) apply directly to the world.

---

## 8. Copy-paste templates

### `<Name>.cs`

```csharp
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class <Name> : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    // Example-specific tunable state.
    private int m_Count;

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 12f;
        m_CameraManipulator.CameraPosition = Vector2.zero;

        m_SandboxManager.SceneResetAction = SetupScene;

        // Initial state.
        m_Count = 10;

        SetupOptions();
        SetupScene();
    }

    private void OnDisable()
    {
        // Restore any global PhysicsWorld / SandboxManager overrides here.
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        // Menu region (suppresses camera input while the pointer is over the panel).
        var menuRegion = root.Q<VisualElement>("menu-region");
        menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
        menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

        // Example control.
        var count = root.Q<SliderInt>("count");
        count.value = m_Count;
        count.RegisterValueChangedCallback(evt => { m_Count = evt.newValue; SetupScene(); });

        // Scene description.
        var sceneDescription = root.Q<Label>("scene-description");
        sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
    }

    private void SetupScene()
    {
        m_SandboxManager.ResetSceneState();

        var world = PhysicsWorld.defaultWorld;

        // Ground.
        var groundBody = world.CreateBody();
        groundBody.CreateShape(
            new SegmentGeometry { point1 = new Vector2(-100f, 0f), point2 = new Vector2(100f, 0f) },
            PhysicsShapeDefinition.defaultDefinition);

        // Content — reuse one definition, vary per-iteration.
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;

        for (var i = 0; i < m_Count; ++i)
        {
            bodyDef.position = new Vector2(0f, 1f + i * 1.2f);
            var body = world.CreateBody(bodyDef);

            shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
            body.CreateShape(new CircleGeometry { center = Vector2.zero, radius = 0.5f }, shapeDef);
        }
    }
}
```

### `<Name>Menu.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements" editor-extension-mode="False">
    <Style src="project://database/Assets/Sandbox/UI/Styling/SandboxStyleOverrides.uss?fileID=7433441132597879392&amp;guid=e140b35909eadcf48959626f553b2145&amp;type=3#SandboxStyleOverrides"/>
    <ui:VisualElement name="VisualElement" picking-mode="Ignore" style="width: 100%; height: 100%; align-items: flex-start; justify-content: flex-end; align-content: auto; padding-left: 8px; padding-bottom: 8px;">
        <ui:VisualElement name="menu-region" style="width: 18%; height: auto; margin-top: 2px; margin-right: 2px; margin-bottom: 2px; margin-left: 2px; align-items: auto;">
            <ui:TabView tabindex="0" style="width: 100%; height: 100%;">
                <ui:Tab label="<Name> Options" tabindex="0" name="Tab">
                    <ui:SliderInt label="Count" value="10" high-value="100" name="count" low-value="1" fill="true" focusable="false" show-input-field="true"/>
                </ui:Tab>
            </ui:TabView>
            <ui:VisualElement name="description" style="flex-grow: 1; flex-shrink: 0; color: rgba(210, 210, 210, 0.5); -unity-text-align: middle-left; align-self: stretch;">
                <ui:Label text="..." name="scene-description" style="flex-wrap: nowrap; flex-grow: 1; flex-direction: column; white-space: normal; text-overflow: clip; -unity-text-align: upper-center;"/>
            </ui:VisualElement>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

### `<Name>.unity`

The scene boilerplate (occlusion/render/lightmap/navmesh settings) is identical to every
other example — the simplest correct approach is to **copy `Assets/Scenes/Custom/Custom.unity`
verbatim** and change only the two reference lines below. They are the only example-specific
parts:

```yaml
# --- the example MonoBehaviour component (on the "Configuration" GameObject) ---
--- !u!114 &<anyStableLocalId>
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: <ConfigurationGameObjectId>}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: GUID_CS, type: 3}     # ← your script GUID
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::<Name>          # ← your class name

# --- the UIDocument component (same GameObject) ---
#   m_Script        : keep 19102 / 0000000000000000e000000000000000 (built-in UIDocument)
#   m_PanelSettings : keep b3cc2d097ffb6c846acc69277ba50b67 (shared PanelSettings)
#   sourceAsset     : fileID 9197481963319205126, guid GUID_UXML, type 3  ← your uxml GUID
```

The scene also contains a `SampleCamera` GameObject (orthographic, tagged `MainCamera`).
It exists only for edit-time framing — `SceneManifest` **deactivates `SampleCamera` after
additive load**, and the runtime `CameraManipulator` takes over. Leave it as copied.

---

## 9. Modifying an existing example in a session

- The only files you normally touch are the example's **`<Name>.cs`** (behaviour) and
  **`<Name>Menu.uxml`** (controls). Keep the `Q<>(name)` lookups and the uxml `name=`
  attributes in sync.
- Adding a control: add the element to the uxml, then query + wire it in `SetupOptions()`.
- Changing the simulation: edit `SetupScene()`. Keep it idempotent and keep
  `ResetSceneState()` as the first line.
- If you change a **global** world setting (gravity, contact frequency/damping/speed, draw
  options) cache the old value in `OnEnable` and **restore it in `OnDisable`** (see
  `ShapeStack.cs` for the canonical pattern) — otherwise you pollute the next scene.
- You rarely need to edit the `.unity`/`.meta`/build list when only changing behaviour.
- After editing PhysicsCore2D calls, re-verify any unfamiliar member against the
  `unity-physicscore2d-*` skills before relying on it.

---

## 10. Pre-flight checklist (before declaring done)

- [ ] Folder `Assets/Scenes/<Category>/<Name>/` contains 6 files (`.cs`, `.cs.meta`,
      `Menu.uxml`, `Menu.uxml.meta`, `.unity`, `.unity.meta`), and the sibling folder
      `<Name>.meta` exists.
- [ ] Four GUIDs are fresh, unique, 32-char lowercase hex (folder, script, uxml, scene).
- [ ] `<Name>.unity` `m_Script.guid` == `<Name>.cs.meta` guid.
- [ ] `<Name>.unity` `m_EditorClassIdentifier` == `Assembly-CSharp::<ClassName>` and the
      class in `<Name>.cs` is named `<ClassName>`.
- [ ] `<Name>.unity` UIDocument `sourceAsset.guid` == `<Name>Menu.uxml.meta` guid.
- [ ] Build list entry in `EditorBuildSettings.asset` has the correct path and
      `guid` == `<Name>.unity.meta` guid.
- [ ] `SceneItems` entry added in `Sandbox.unity` with matching `ScenePath`.
- [ ] uxml has `menu-region` + `scene-description`, and every `Q<>` name has a matching element.
- [ ] `SetupScene()` calls `ResetSceneState()` first; `OnDisable` restores any global overrides.
- [ ] No legacy `Physics2D` types; no `[Obsolete]` PhysicsCore2D members.
```
