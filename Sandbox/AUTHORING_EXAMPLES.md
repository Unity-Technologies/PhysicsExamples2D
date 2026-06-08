# Authoring Sandbox Examples (LLM Guide)

This document tells an automated agent (LLM) everything it needs to **add a new example
scene to the Sandbox project, and to modify an existing one**.

It is the authoritative recipe for this project. Read it fully before creating files.

> **Physics API rules.** This project uses Unity's **low-level 2D physics**. The namespace differs
> by Unity version: it is **`UnityEngine.LowLevelPhysics2D`** on Unity **6000.3**, and was renamed
> to **`Unity.U2D.Physics`** ("PhysicsCore2D") in **6000.5**. The type and member names are
> otherwise the same. Scripts stay compilable on **both** versions via a using-guard — see the
> note below and the §8 template. Never use the legacy `Physics2D` component system
> (`Rigidbody2D`, `Collider2D`, …), and never use an `[Obsolete]` member.

> **Cross-version using-guard (required in every script that uses physics types).**
> ```csharp
> #if UNITY_6000_5_OR_NEWER
> using Unity.U2D.Physics;
> #else
> using UnityEngine.LowLevelPhysics2D;
> #endif
> ```
> Use this block instead of a bare `using` for the physics namespace. Everything else in the file
> is identical across versions.

> **Architecture.** Every example derives from **`SandboxExampleBehaviour`** and is tagged with
> **`[ExampleScene]`**, with the repeated menu chrome factored into shared infrastructure. All
> scenes — including the `Example` starter — use this pattern; the old inline `MonoBehaviour`
> boilerplate is fully retired. **Author all new and modified examples with the new pattern below.**

---

## 1. What an "example" is

Each example is a **self-contained scene** living in its own folder directly under
`Assets/Scenes/`, named after the example. There are **no category sub-folders** — an example's
category comes solely from its `[ExampleScene("<Category>", …)]` attribute, so the folder name is
just the example `<Name>`:

```
Assets/Scenes/<Name>.meta  ← folder GUID (Unity needs every folder to have one)
Assets/Scenes/<Name>/
├── <Name>.cs            ← a SandboxExampleBehaviour subclass tagged [ExampleScene]
├── <Name>.cs.meta       ← asset GUID for the script
├── <Name>.unity         ← the scene asset
└── <Name>.unity.meta    ← asset GUID for the scene
```

There is **no per-example `.uxml`.** Option controls are built in code (see §7); the scene's
`UIDocument` renders the single shared `ExampleChrome.uxml`.

> **Folder `.meta`:** every folder is itself an asset, so the new `<Name>` folder needs a
> sibling `<Name>.meta`. The Editor auto-generates this on import; in a pure file-only
> workflow author it yourself with a fresh GUID so git state is deterministic (see Step 1b).

Scenes are **loaded additively at runtime** by the shared infrastructure in
`Assets/Sandbox.unity` (the startup scene):

| Component / type | Role |
|---|---|
| `SandboxManager` | Global controls (pause/step/reset/colors/debug), world draw options, shared deterministic `Random`, per-scene reset hook. |
| `SceneManifest`  | The registry of all examples (`SceneItems` list). Loads/unloads scenes additively by build index. |
| `CameraManipulator` | Drives the runtime camera (pan/zoom/drag/explode). Each example sets initial framing via overrides. |
| `SandboxExampleBehaviour` | The base class every example derives from. Resolves the infra, finds the chrome regions, titles/describes the panel, wires Reset, exposes the `AddSlider`/`AddToggle`/`AddEnum`/… control-builder helpers, and clears overrides — so the example file is just its physics + options. |

**Rendering:** examples do **not** need a `SpriteRenderer`/`GameObject` per body. The
debug renderer draws all shapes/joints automatically. Use a Unity `Transform`
(via `body.transformObject`) only when you want a sprite/visual to track a body.

---

## 2. Registering the example

The runtime discovers scenes from two places: the **build list**
(`ProjectSettings/EditorBuildSettings.asset`) and the **`SceneItems` list** on the
`SceneManifest` in `Assets/Sandbox.unity`. You no longer maintain those by hand.

**Recommended (in-Editor): the `[ExampleScene]` attribute + the registry tool.**
1. Tag the class: `[ExampleScene("<Category>", "<description>")]`.
2. Run **`Tools/2D/Physics/Rebuild Sandbox Registry`**.

The tool scans every `[ExampleScene]` type, finds its sibling `<Name>.unity`, and **upserts**
(by scene path) the manifest entry *and* the build-list entry. It also prunes entries whose
example folders you've removed or moved. `Category` and `Description` live in the attribute
(next to the code), not in the manifest.

**Fallback (pure file-only, no Editor):** the registry tool is an Editor menu command, so if
you're editing files without launching Unity you must still hand-register in both places —
see Step 4 (alt). Tag the class with `[ExampleScene]` anyway so a later tool run reconciles it.

---

## 3. The invariant chain (why GUIDs matter)

The scene file references the script **by GUID**, so these must line up exactly:

```
<Name>.cs.meta:guid ────────────► <Name>.unity  m_Script.guid  (the example MonoBehaviour)
<Name>.unity.meta:guid ─────────► EditorBuildSettings.asset  guid (build list entry)
class <Name> (in <Name>.cs) ────► <Name>.unity  m_EditorClassIdentifier: Assembly-CSharp::<Name>
<Name>.unity ScenePath ─────────► Sandbox.unity SceneItems[].ScenePath  AND  build list path
```

The UIDocument's `sourceAsset.guid` is the **constant shared-chrome guid**
`c705ca6fc9e54f969fcd4dfcc2160e43` (`Assets/Framework/UI/ExampleChrome.uxml`) — the same in every
example, not a per-example value.

There is **no asmdef** in this project, so all example scripts compile into the default
`Assembly-CSharp`; `m_EditorClassIdentifier` is always `Assembly-CSharp::<ClassName>` (it may also
be left empty — Unity then binds purely via `m_Script.guid`).

### Generating GUIDs
A Unity asset GUID is **32 lowercase hex characters**, no dashes:
```bash
python -c "import uuid;print(uuid.uuid4().hex)"
# or  openssl rand -hex 16   # or (PowerShell) [guid]::NewGuid().ToString('N')
```
Never reuse a GUID that already exists in the project.

---

## 4. Shared constants (copy verbatim into the scene)

Identical in every example scene — NOT per-asset:

| Reference | Value |
|---|---|
| `UIDocument` built-in script | `m_Script: {fileID: 19102, guid: 0000000000000000e000000000000000, type: 0}` |
| Shared `PanelSettings` | `m_PanelSettings: {fileID: 11400000, guid: b3cc2d097ffb6c846acc69277ba50b67, type: 2}` |
| UIDocument `sourceAsset` (the shared chrome) | `{fileID: 9197481963319205126, guid: c705ca6fc9e54f969fcd4dfcc2160e43, type: 3}` |

Every example's `UIDocument` points at the one shared `Assets/Framework/UI/ExampleChrome.uxml`
(menu region, tab, `options-content` placeholder, description). The base class finds its regions
at load, titles/describes the panel, and lets the example add controls into `options-content`.

---

## 5. Step-by-step recipe (add a new example)

**Step 0 — Naming.** `Category` (free-text group, e.g. `Shapes`); `<ClassName>`/file stem in
PascalCase (e.g. `MyExample`) used for the `.cs`, `.unity`, and folder.
The display name is derived from the class name (`MyExample` → "My Example").

**Step 1 — GUIDs.** Generate three fresh ones: `GUID_FOLDER`, `GUID_CS`, `GUID_SCENE`.

**Step 1b — Folder `.meta`** at `Assets/Scenes/<Name>.meta`:
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

**Step 2 — `<Name>.cs`** from the §8 template (set class name, `[ExampleScene]`, camera, options,
physics). `<Name>.cs.meta`:
```yaml
fileFormatVersion: 2
guid: GUID_CS
```

**Step 3 — `<Name>.unity`** from the §8 scene template. Swap in `GUID_CS` (MonoBehaviour
`m_Script.guid`) and `m_EditorClassIdentifier: Assembly-CSharp::<ClassName>`. Leave the UIDocument
`sourceAsset` pointing at the shared chrome guid `c705ca6fc9e54f969fcd4dfcc2160e43` (already in the
template — there is no per-example uxml). `<Name>.unity.meta`:
```yaml
fileFormatVersion: 2
guid: GUID_SCENE
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
```

**Step 4 — Register.** Run **`Tools/2D/Physics/Rebuild Sandbox Registry`** (with `[ExampleScene]`
on the class). Done.

**Step 4 (alt, no Editor) — Hand-register** in both:
- `ProjectSettings/EditorBuildSettings.asset` → add to `m_Scenes`:
  ```yaml
    - enabled: 1
      path: Assets/Scenes/<Name>/<Name>.unity
      guid: GUID_SCENE
  ```
- `Assets/Sandbox.unity` → add to the `SceneItems:` list on the `SceneManifest` MonoBehaviour:
  ```yaml
    - Name: <Display Name>
      Category: <Category>
      Description: <description>
      ScenePath: Assets/Scenes/<Name>/<Name>.unity
  ```

**Step 5 — (Optional) Iterate.** Set `StartScene:` on the `SandboxManager` in `Sandbox.unity`
to your display name to boot straight into it (revert before committing if needed).

---

## 6. The runtime contract (what `<Name>.cs` MUST do)

Derive from `SandboxExampleBehaviour` and implement the hooks. The base class resolves the
infra, sets `SceneOptionsUI`, applies your camera framing, registers Reset, finds the chrome
regions (title/description/`options-content`), and clears overrides on disable — so your file is
just physics + options.

```csharp
[ExampleScene("Shapes", "Demonstrating X.")]
public sealed class MyExample : SandboxExampleBehaviour
{
    protected override float   CameraSize     => 12f;          // optional framing overrides
    protected override Vector2 CameraPosition => Vector2.zero;

    protected override void OnExampleEnable()  { /* state, world overrides, event subscribe, native alloc */ }
    protected override void OnExampleDisable() { /* event unsubscribe, native dispose, world restore */ }

    protected override void SetupOptions()     { /* build controls with AddSlider/AddToggle/… */ }

    protected override void SetupScene()       { var world = World; /* create physics */ }
}
```

**Base-class call order on enable:** `OnExampleEnable()` → build chrome + `SetupOptions()`
→ `RebuildScene()` (which calls `ResetSceneState()` then your `SetupScene()`).

Key rules:
- **`SetupScene()` is called repeatedly** — first load, every Reset (R), and whenever you call
  `RebuildScene()` from an option callback. The world is already reset when it runs, so begin
  directly with physics. To rebuild after a structural option change, call **`RebuildScene()`**
  (not `SetupScene()` directly — that would skip the world reset).
- **The `World` accessor is for reads and method calls** (`World.gravity`, `World.CreateBody(...)`).
  To **set** a world property, copy to a local first — `PhysicsWorld` is a handle struct and
  C# forbids assigning a property on the by-value accessor result (compiler error CS1612):
  ```csharp
  var world = World;
  world.gravity = m_OldGravity * scale;   // ✅   (World.gravity = …  ❌ CS1612)
  ```
- **Gravity rule.** If a gravity-scale option **rebuilds the scene**, the intent is to apply that
  scale to each dynamic body as it spawns (`bodyDef.gravityScale = m_GravityScale`). If the option
  **does not rebuild** (takes effect live on everything already spawned), scale the world gravity
  instead (`var world = World; world.gravity = m_OldGravity * m_GravityScale;`).
- **Determinism:** use `Random` (the base's `ref` to the shared `Unity.Mathematics.Random`,
  re-seeded by `ResetSceneState()`), not `UnityEngine.Random`.
- **Shape colours:** set `shapeDef.surfaceMaterial.customColor = ShapeColor;` so the global
  "Colors" toggle works.
- **Overrides auto-reset.** If you call `SandboxManager.SetOverrideDrawOptions(...)` /
  `SetOverrideColorShapeState(...)` in `OnExampleEnable`, you do **not** need to reset them —
  the base `OnDisable` does it. Only undo things the base doesn't know about
  (e.g. `CameraManipulator.DisableManipulators`, world contact params, `PhysicsEvents` subs).
- **Do not create/destroy bodies during the simulation step** (WORM rule).
- **You can still add `Update()`/`FixedUpdate()`** and implement callback interfaces directly on
  your subclass.

### Members provided by `SandboxExampleBehaviour`
| Member | Purpose |
|---|---|
| `World` | `PhysicsWorld.defaultWorld` (reads/methods; local-copy to set properties). |
| `ref Random Random` | Shared deterministic RNG. |
| `Color ShapeColor` | Per-shape colour honouring the global Colors toggle. |
| `SandboxManager` / `SceneManifest` / `CameraManipulator` / `UIDocument` | The resolved infra. |
| `RebuildScene()` | `ResetSceneState()` + `SetupScene()`; call from option callbacks. |
| `CameraSize` / `CameraPosition` (override) | Initial framing. |
| `OnExampleEnable` / `OnExampleDisable` / `SetupOptions` / `SetupScene` (override) | Your hooks. |

### `SandboxManager` extras an example may use
`SetOverrideDrawOptions(...)` / `SetOverrideColorShapeState(bool)` (auto-reset by the base),
`WorldPaused`, `ControlsMenu[i]` (touch buttons).

---

## 7. Building option controls in code (`SetupOptions()`)

There is no per-example uxml. Override `SetupOptions()` and add controls with the base-class
helpers; each constructs the control, sets the shared conventions (`focusable = false`,
input-field shown for sliders), registers a value-changed callback, adds it to the panel
(`OptionsContent`, which is `PickingMode.Ignore` so empty space passes camera input through), and
returns it.

```csharp
protected Slider    AddSlider(string label, float value, float low, float high, Action<float> onChanged, bool rebuild = false);
protected SliderInt AddSliderInt(string label, int value, int low, int high, Action<int> onChanged, bool rebuild = false);
protected Toggle    AddToggle(string label, bool value, Action<bool> onChanged, bool rebuild = false);
protected EnumField AddEnum<TEnum>(string label, TEnum value, Action<TEnum> onChanged, bool rebuild = false) where TEnum : Enum;
protected T         AddElement<T>(T element) where T : VisualElement;   // display/custom widgets; returns it
```

- `value` is the control's initial value — pass your backing field (`m_Count`), so the field is
  the single source of truth for defaults.
- The `onChanged` lambda receives the new value; do your field assignment / live update there.
  Pass **`rebuild: true`** when the change should rebuild the scene (the helper calls
  `RebuildScene()` after your lambda) — don't call `RebuildScene()` yourself in that case.
- `AddEnum` infers `TEnum` from the value, so a private nested enum (or a physics enum like
  `PhysicsBody.BodyType`) works directly.
- For widgets the typed helpers don't cover (`ProgressBar`, `MinMaxSlider`, read-only `FloatField`,
  `Label`), construct them and pass to `AddElement(...)`; cache the return for later updates.
  Add the `hash-label` USS class for a bordered display box (see `SandboxStyleOverrides.uss`).

> **Self-referencing control callbacks.** If a callback needs to read/set the control it belongs to
> (e.g. a clamp that writes back `slider.value`), declare the variable first, then assign:
> `Slider slider = null; slider = AddSlider(…, v => { … slider.value = …; });`. Referencing the
> control inside its own initializer is a C# compile error (CS0841).

Controls appear in the order you add them.

---

## 8. Copy-paste templates

### `<Name>.cs`
```csharp
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

[ExampleScene("<Category>", "<One-sentence description shown in the options panel.>")]
public sealed class <Name> : SandboxExampleBehaviour
{
    private int m_Count = 10;

    protected override float CameraSize => 12f;
    protected override Vector2 CameraPosition => Vector2.zero;

    protected override void SetupOptions()
    {
        AddSliderInt("Count", m_Count, 1, 100, v => m_Count = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        var world = World;

        // Ground.
        var groundBody = world.CreateBody();
        groundBody.CreateShape(
            new SegmentGeometry { point1 = new Vector2(-100f, 0f), point2 = new Vector2(100f, 0f) },
            PhysicsShapeDefinition.defaultDefinition);

        // Content.
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;

        for (var i = 0; i < m_Count; ++i)
        {
            bodyDef.position = new Vector2(0f, 1f + i * 1.2f);
            var body = world.CreateBody(bodyDef);
            shapeDef.surfaceMaterial.customColor = ShapeColor;
            body.CreateShape(new CircleGeometry { center = Vector2.zero, radius = 0.5f }, shapeDef);
        }
    }
}
```
Add `OnExampleEnable`/`OnExampleDisable` overrides only if you need per-example setup/teardown
(world-property save/restore, `PhysicsEvents` subscribe/unsubscribe, `NativeArray`/`NativeList`
alloc+dispose, `DisableManipulators`, `ControlsMenu` buttons, `SetOverride*`).

### `<Name>.unity`
Scene boilerplate is identical to every example — **copy `Assets/Scenes/Example/Example.unity`
verbatim** (the starter template) and change only the two MonoBehaviour reference lines:
```yaml
# --- the example MonoBehaviour (on the "Configuration" GameObject) ---
  m_Script: {fileID: 11500000, guid: GUID_CS, type: 3}     # ← your script GUID
  m_EditorClassIdentifier: Assembly-CSharp::<Name>          # ← your class name

# --- the UIDocument component (same GameObject) — leave all of this as copied ---
#   keep m_Script 19102 / 0000000000000000e000000000000000 (built-in UIDocument)
#   keep m_PanelSettings b3cc2d097ffb6c846acc69277ba50b67  (shared PanelSettings)
#   keep sourceAsset guid c705ca6fc9e54f969fcd4dfcc2160e43  (shared ExampleChrome.uxml)
```
**Do NOT add a Camera GameObject** (of any name) to the scene. Cameras are managed entirely
by `CameraManipulator` in `Sandbox.unity`; a per-scene camera is never needed and will be
ignored at runtime.

---

## 9. Modifying an existing example

- Touch only **`<Name>.cs`** — both behaviour and controls live there now.
- **Add a control:** add an `AddSlider`/`AddToggle`/`AddEnum`/… call (or `AddElement(...)`) in
  `SetupOptions()`.
- **Change a default value:** edit the `m_Field` initialiser in `.cs` (it's passed straight to the
  helper as the control's initial value). Edit the helper args for ranges/limits and label.
- **Change the simulation:** edit `SetupScene()`. It begins after a world reset; rebuild from
  option callbacks via `RebuildScene()` (or `rebuild: true` on the helper).
- **Set a world property:** copy `World` to a local first (`var world = World; world.gravity = …`)
  — see the CS1612 note in §6. Cache the old value in `OnExampleEnable` and restore it in
  `OnExampleDisable`; override draw/colour state is auto-reset by the base.
- Every example already uses the `SandboxExampleBehaviour` pattern with code-built options. In the
  unlikely event you meet a plain `MonoBehaviour` with a `FindFirstObjectByType` preamble or a
  per-example `*Menu.uxml` (e.g. restored from old history), migrate it: derive from the base,
  delete the infra fields + `OnEnable` preamble, move setup into `OnExampleEnable`, rebuild the
  options in `SetupOptions()` with the `AddX` helpers, make `SetupScene` start at
  `var world = World;`, move teardown into `OnExampleDisable` (drop `ResetOverride*` — auto), add
  `[ExampleScene]`, point the UIDocument `sourceAsset` at the shared chrome guid, and delete the
  uxml. Then run the registry tool.
- Keep the cross-version using-guard (§ top) on any script that references physics types.

---

## 10. Pre-flight checklist

- [ ] Folder `Assets/Scenes/<Name>/` has the 4 files (`.cs`, `.cs.meta`, `.unity`, `.unity.meta`),
      and the sibling `<Name>.meta` exists.
- [ ] Three fresh, unique, 32-char lowercase-hex GUIDs (folder, script, scene).
- [ ] `<Name>.cs`: derives from `SandboxExampleBehaviour`, tagged `[ExampleScene("<Category>", "…")]`,
      implements `SetupScene()`, overrides `SetupOptions`/camera/`OnExample*` as needed, and uses the
      cross-version `#if UNITY_6000_5_OR_NEWER` using-guard for the physics namespace.
- [ ] No `World.<prop> = …` (use a local); uses `Random`/`ShapeColor`; rebuilds via `RebuildScene()`.
- [ ] `SetupOptions()` builds controls with the `AddX` helpers (no per-example uxml).
- [ ] `<Name>.unity`: `m_Script.guid` == `.cs.meta` guid; `m_EditorClassIdentifier` ==
      `Assembly-CSharp::<ClassName>` (or empty); UIDocument `sourceAsset.guid` == shared chrome guid
      `c705ca6fc9e54f969fcd4dfcc2160e43`.
- [ ] Registered: ran `Tools/2D/Physics/Rebuild Sandbox Registry` (or hand-added build-list +
      `SceneItems` entries) — scene path matches in all places.
- [ ] No legacy `Physics2D` types; no `[Obsolete]` members.
- [ ] No Camera GameObject in the scene (cameras are global; `CameraManipulator` in `Sandbox.unity` handles all framing).
