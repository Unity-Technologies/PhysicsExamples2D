# Authoring Sandbox Examples (LLM Guide)

This document tells an automated agent (LLM) everything it needs to **add a new example
to the Sandbox project, and to modify an existing one**.

It is the authoritative recipe for this project. Read it fully before creating files.

> **PhysicsCore2D rules still apply.** This project is **PhysicsCore2D only**
> (`Unity.U2D.Physics`). Never use the legacy `Physics2D` component system
> (`Rigidbody2D`, `Collider2D`, …). Verify every PhysicsCore2D API against the
> bundled `unity-physicscore2d-*` skills — never guess a signature, never use an
> `[Obsolete]` member. See the repo-root `CLAUDE.md`.

> **Architecture.** Every example derives from **`SandboxExampleBehaviour`** and is tagged with
> **`[ExampleScene]`**, with the repeated menu chrome factored into shared infrastructure. All
> examples use this pattern; the old inline `MonoBehaviour` boilerplate is fully retired.
> **Author all new and modified examples with the
> new pattern below.**

---

## 1. What an "example" is

An example is a **single `.cs` file** inside `Assets/Examples/`. There are no subfolders and no
scene files. An example's category comes solely from its `[ExampleScene("<Category>", …)]`
attribute:

```
Assets/Examples/<Name>.cs        ← a SandboxExampleBehaviour subclass tagged [ExampleScene]
Assets/Examples/<Name>.cs.meta   ← asset GUID (Unity auto-generates this on import)
```

If the example needs serialised assets (sprites, materials, etc.), create a companion
`<Name>Data : ExampleSceneData` ScriptableObject class and a matching `.asset` instance in
`Assets/Examples/Assets/` — see §5.

There is **no per-example `.uxml`.** Option controls are built in code (see §6); the
`UIDocument` renders the single shared `ExampleChrome.uxml`.

Examples are **loaded at runtime** by instantiating a new `GameObject` and calling
`AddComponent<T>()` — no scene loading, no build settings management.

| Component / type | Role |
|---|---|
| `SandboxManager` | Global controls (pause/step/reset/colors/debug), world draw options, shared deterministic `Random`, per-example reset hook. |
| `SceneManifest`  | ScriptableObject registry of all examples. Switches examples by destroying the old `GameObject` and adding the new component. |
| `CameraManipulator` | Drives the runtime camera (pan/zoom/drag/explode). Each example sets initial framing via overrides. |
| `SandboxExampleBehaviour` | The base class every example derives from. Resolves the infra, finds the chrome regions, titles/describes the panel, wires Reset, exposes the `AddSlider`/`AddToggle`/`AddEnum`/… control-builder helpers, and clears overrides — so the example file is just its physics + options. |

**Rendering:** examples do **not** need a `SpriteRenderer`/`GameObject` per body. The
PhysicsCore2D **debug renderer draws all shapes/joints automatically**. Use a Unity
`Transform` (via `body.transformObject`) only when you want a sprite/visual to track a body.

---

## 2. Adding an example — the full recipe

**Step 1 — Create `Assets/Examples/<Name>.cs`** from the §7 template. Set the class name,
`[ExampleScene]` category and description, camera framing, options, and physics content.
Unity auto-generates the `.meta` on import.

**Step 2 — Register.** Run **`Tools > 2D > Physics > Rebuild Sandbox Registry`**.

That's it. The tool scans every `[ExampleScene]`-tagged type that derives from
`SandboxExampleBehaviour`, writes the assembly-qualified `TypeName` into
`Assets/Framework/SceneManifest.asset`, and removes entries for types that no longer exist.
No GUIDs to generate, no scene files to author, no build settings to edit.

**Step 3 — (Optional) Iterate.** Set `StartScene:` on the `SandboxManager` in `Sandbox.unity`
to your example's display name to boot straight into it (revert before committing if needed).

---

## 3. Registering — what the tool does

`ExampleRegistryBuilder` (`Assets/Editor/ExampleRegistryBuilder.cs`) runs on:
- the explicit **`Tools > 2D > Physics > Rebuild Sandbox Registry`** menu item, and
- any script import inside `Assets/Examples/` that introduces a new `[ExampleScene]` type.

It:
1. Calls `TypeCache.GetTypesWithAttribute<ExampleSceneAttribute>()` — no file scanning.
2. Validates that every found type derives from `SandboxExampleBehaviour` (logs a warning otherwise).
3. Upserts `SceneItem` entries by `TypeName` (assembly-qualified class name).
4. Auto-discovers companion `ExampleSceneData` assets by searching for a ScriptableObject whose
   asset name matches `{TypeName}Data` (e.g. `SpriteDestructionData.asset` for `SpriteDestruction`).
5. Ensures only `Assets/Sandbox.unity` is in `EditorBuildSettings.scenes`; removes any stale
   `Assets/Scenes/` entries left over from older project layouts.

---

## 4. How `SceneManifest` switches examples at runtime

`SceneManifest` is a ScriptableObject (`Assets/Framework/SceneManifest.asset`). `SandboxManager`
holds a direct serialised reference to it.

When switching to example `name`:

```
1. SetActive(false) on the current example GameObject → OnDisable fires immediately.
2. Destroy the current example GameObject.
3. Invoke the reset callback (clears physics, debug draw, RNG seed).
4. m_CurrentExampleGO = new GameObject(item.Name)   ← SetActive(false) first
   example = m_CurrentExampleGO.AddComponent(Type.GetType(item.TypeName))
   example.ExampleData = item.Data                  ← inject before OnEnable
   LoadedSceneName = item.Name
   LoadedSceneDescription = item.Description
   m_CurrentExampleGO.SetActive(true)               ← OnEnable fires here with all state set
```

The switch is **synchronous** — no coroutines, no async scene loading.

---

## 5. Examples with asset dependencies

Most examples are fully procedural. If an example needs a serialised asset (a `Sprite`, a
`Material`, a `PhysicsMaterial2D`, etc.), use the **companion data** pattern:

**Step A — Define a data class** (in `Assets/Examples/` or inline):
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "MyExampleData", menuName = "2D Physics/Example Data/My Example")]
public class MyExampleData : ExampleSceneData
{
    public Sprite MySprite;
    public Material MyMaterial;
}
```

**Step B — Read from `ExampleData` in `OnExampleEnable()`:**
```csharp
protected override void OnExampleEnable()
{
    var data = (MyExampleData)ExampleData;
    m_Sprite    = data.MySprite;
    m_Material  = data.MyMaterial;
}
```

**Step C — Create the `.asset` instance** inside Unity: right-click in `Assets/Examples/Assets/`,
choose *Create > 2D Physics > Example Data > My Example*, name it `MyExampleData`, then assign
the sprite/material references in the Inspector.

**Step D — Register.** Run **`Tools > 2D > Physics > Rebuild Sandbox Registry`**. The tool finds
`MyExampleData.asset` by convention (asset name matches `{TypeName}Data`) and wires it into the
`SceneManifest` entry automatically.

All example assets live in the single flat `Assets/Examples/Assets/` folder — no per-example
subfolders. This keeps the folder hierarchy flat and makes asset discovery trivial.

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
- **Determinism:** use `Random` (the base's `ref` to the shared `Unity.Mathematics.Random`,
  re-seeded by `ResetSceneState()`), not `UnityEngine.Random`.
- **Shape colours:** set `shapeDef.surfaceMaterial.customColor = ShapeColor;` so the global
  "Colors" toggle works.
- **Overrides auto-reset.** If you call `SandboxManager.SetOverrideDrawOptions(...)` /
  `SetOverrideColorShapeState(...)` in `OnExampleEnable`, you do **not** need to reset them —
  the base `OnDisable` does it. Only undo things the base doesn't know about
  (e.g. `CameraManipulator.DisableManipulators`, world contact params, `PhysicsEvents` subs).
- **Do not create/destroy bodies during the simulation step** (WORM rule).
- **You can still add `Update()`/`FixedUpdate()`** and implement callback interfaces
  (`PhysicsCallbacks.IContactCallback`, etc.) directly on your subclass.

### Members provided by `SandboxExampleBehaviour`
| Member | Purpose |
|---|---|
| `World` | `PhysicsWorld.defaultWorld` (reads/methods; local-copy to set properties). |
| `ref Random Random` | Shared deterministic RNG. |
| `Color ShapeColor` | Per-shape colour honouring the global Colors toggle. |
| `ExampleData` | Optional `ExampleSceneData` asset injected before `OnEnable` (null for most examples). |
| `SandboxManager` / `SceneManifest` / `CameraManipulator` / `UIDocument` | The resolved infra. |
| `RebuildScene()` | `ResetSceneState()` + `SetupScene()`; call from option callbacks. |
| `CameraSize` / `CameraPosition` (override) | Initial framing. |
| `OnExampleEnable` / `OnExampleDisable` / `SetupOptions` / `SetupScene` (override) | Your hooks. |

### `SandboxManager` extras an example may use
`SetOverrideDrawOptions(...)` / `SetOverrideColorShapeState(bool)` (auto-reset by the base),
`ShowFPS()` / `HideFPS()`, `WorldPaused`, `ControlsMenu[i]` (touch buttons), `WorldSleeping`.

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
- `AddEnum` infers `TEnum` from the value, so a private nested enum (or a PhysicsCore2D enum like
  `PhysicsBody.BodyType`) works directly.
- For widgets the typed helpers don't cover (`ProgressBar`, `MinMaxSlider`, read-only `FloatField`,
  `Label`), construct them and pass to `AddElement(...)`; cache the return for later updates.
  Add the `hash-label` USS class for a bordered display box (see `SandboxStyleOverrides.uss`).

Controls appear in the order you add them.

---

## 8. Copy-paste template

### `Assets/Examples/<Name>.cs`
```csharp
using UnityEngine;
using Unity.U2D.Physics;
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

No scene file is required. No GUIDs to generate. No build settings to edit.

---

## 9. Modifying an existing example

- Touch only **`Assets/Examples/<Name>.cs`** — both behaviour and controls live there.
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
  unlikely event you meet a plain `MonoBehaviour` with a `FindAnyObjectByType` preamble or a
  per-example `*Menu.uxml` (e.g. restored from old history), migrate it: derive from the base,
  delete the infra fields + `OnEnable` preamble, move setup into `OnExampleEnable`, rebuild the
  options in `SetupOptions()` with the `AddX` helpers, make `SetupScene` start at
  `var world = World;`, move teardown into `OnExampleDisable` (drop `ResetOverride*` — auto), add
  `[ExampleScene]`, and run the registry tool.
- After editing PhysicsCore2D calls, re-verify any unfamiliar member against the
  `unity-physicscore2d-*` skills.

---

## 10. Pre-flight checklist

- [ ] `Assets/Examples/<Name>.cs` exists (Unity will auto-generate `.meta` on import).
- [ ] Class derives from `SandboxExampleBehaviour`, tagged `[ExampleScene("<Category>", "…")]`,
      implements `SetupScene()`, overrides `SetupOptions`/camera/`OnExample*` as needed.
- [ ] No `World.<prop> = …` (use a local); uses `Random`/`ShapeColor`; rebuilds via `RebuildScene()`.
- [ ] `SetupOptions()` builds controls with the `AddX` helpers (no per-example uxml).
- [ ] If asset dependencies are needed: companion `<Name>Data : ExampleSceneData` class defined,
      `<Name>Data.asset` created in `Assets/Examples/Assets/`, assets assigned in Inspector.
- [ ] Ran **`Tools > 2D > Physics > Rebuild Sandbox Registry`** — example appears in the menu.
- [ ] No legacy `Physics2D` types; no `[Obsolete]` PhysicsCore2D members.
