# Authoring Workshop Examples

This guide explains how to **add a new example to the Workshop project, and how to modify an existing one**.
It is written for both people and automated agents (LLMs). Read it fully before creating files.
It is the authoritative recipe for this project.

> **PhysicsCore2D only.** This project uses `Unity.U2D.Physics` through the Physics Pose, Area and Constraint components.
> Never use the legacy `Physics2D` component system (`Rigidbody2D`, `Collider2D`, ...).
> Check every API against the
> [2D Physics Core manual](https://docs.unity3d.com/6000.7/Documentation/Manual/2d-physics-api/2d-physics-api-landing.html),
> the [package documentation](https://docs.unity3d.com/Packages/com.unity.2d.physics@latest/) and the scripting reference
> (`https://docs.unity3d.com/6000.7/Documentation/ScriptReference/`). Never guess a signature, and never use an `[Obsolete]` member.

> **Components first.** An example is a scene authored with components.
> Write a script only for what no component covers, such as spawning a stream of bodies, a custom drawing call or a per-frame behavior.
> If you want the same example written directly against the API, that is the [Sandbox](../Sandbox/README.md) project.

---

## 1. What an example is

An example is a folder under `Assets/Examples/`, named after the example:

```
Assets/Examples/<Name>/
    <Name>.unity                  the scene
    <Name>Info.asset              a WorkshopExampleInfo asset
    Scripts/<Name>Contents.cs     optional, a MonoBehaviour for anything no component can do
    Scripts/<Name>Options.cs      optional, a WorkshopOptionsProvider that adds menu controls
```

Prefabs, materials and other assets the example needs live in its own folder too, so an example can be copied or removed as a unit.

| Type | Role |
|---|---|
| `WorkshopManager` | The UI. Loads an example scene on top of the always loaded `Workshop.unity`, applies the example's global settings, and builds the menu. |
| `WorkshopManifest` | A generated list of every example. Never edit it by hand. |
| `WorkshopExampleInfo` | One per example. Holds the name, category, description, scene and global settings. |
| `WorkshopExampleState` | The global settings an example asks for, held by the info asset. |
| `WorkshopOptionsProvider` | Base class for an example's menu controls. |
| `CameraManipulator` | On the example scene's `Camera`. Pan, zoom and body dragging. |

Rendering needs nothing from you. The physics debug renderer draws the shapes and joints, so a body needs no sprite or mesh unless you want one.

## 2. Adding an example

**Step 1: Create the folder and scene.**
Create `Assets/Examples/<Name>/`. Duplicate the scene of a similar existing example into it (select the scene in the Project window and press Ctrl+D), rename it `<Name>.unity`, and delete the contents you do not want.
Keep the `Camera` object, because it carries the `CameraManipulator` the Workshop looks for. Set its position and orthographic size to frame your example.

**Step 2: Author the physics with components.**
- A **Physics Pose** is a body. Set its body type in its definition.
- A **Physics Area** component is a shape, on the same object as the pose or on a child of it. Choose the area type that matches the shape (circle, capsule, polygon, segment and so on).
- A **Physics Constraint** component is a joint. Set its two poses, switch off the automatic anchors if you are placing them yourself, and set its definition.
- For many copies of one thing, build one prefab and instantiate it, or use a Contents script.

Order matters because a constraint is created at the moment it is enabled, and it needs both of its poses to exist by then.
Put constraints on an object that starts inactive and is activated after the poses and areas, or leave them inactive in the scene and enable them from a script.

A prefab that a script instantiates is best authored inactive, configured, and then activated, so its components are not created half set up.

**Step 3: Add a Contents script, only if needed.**
Derive from `MonoBehaviour`, put it on an object in the scene, and build the example's content in `Start`.
Use the components' public API and the `Unity.U2D.Physics` API, and use a fixed random seed so the example plays the same way every time.
Pressing Reset (the R key, or the Reset button) reloads the example scene, so a scene that builds itself in `Start` rebuilds correctly.

**Step 4: Add an Options script, only if needed.**
Most examples need none, because their components are already tunable in the Inspector. Add one when a player should be able to change something while the example runs. See section 4.

**Step 5: Create the info asset.**
Right-click inside the example folder and choose **Create > 2D Physics > Workshop Example Info**. Name it `<Name>Info`, then set:

| Field | Meaning |
|---|---|
| Scene | The example scene. The path is stored for you. |
| Example Name | What the menu shows. Written as words, for example "Many Prismatics". |
| Category | The menu group, for example "Joints". Examples with the same category appear together. |
| Description | One sentence shown in the description panel. |
| State | Global settings the example needs (section 5). Leave it alone if it needs none. |

**Step 6: Register.**
Run **`Tools > 2D > Physics > Rebuild Workshop Registry`**. It finds every info asset, rewrites `WorkshopManifest.asset`, and rewrites the build settings scene list.
It only runs from that menu item, so run it after adding, renaming or removing an example. An info asset with no scene or no name is skipped with a warning.

**Step 7: Try it.**
Set `Start Scene` on the `WorkshopManager` in `Workshop.unity` to your example's name to start straight into it, and clear it again before committing.

## 3. How an example is loaded

When you choose an example, the Workshop:

1. Switches off and unloads the current example, clears its menu controls, and puts back any global settings it changed.
2. Resets the default physics world, so every example starts from the same state.
3. Applies the new example's `State`.
4. Loads the example scene additively and makes it the active scene.
5. Finds the scene's `CameraManipulator` and its `WorkshopOptionsProvider`, and builds the menu controls.

The example's components create their physics objects as they are enabled, so a scene of components is ready on its first frame.
A scene should contain at most one `WorkshopOptionsProvider`, because the Workshop connects the first one it finds.

## 4. Menu controls (`WorkshopOptionsProvider`)

Derive from `WorkshopOptionsProvider`, add it to an object in the scene, and override `SetupOptions()`. It is called once after the scene loads, with the panel already empty.

```csharp
public sealed class MyExampleOptions : WorkshopOptionsProvider
{
    [SerializeField] MyExampleContents m_Contents;

    protected override void SetupOptions()
    {
        AddSliderInt("Count", m_Contents.count, 1, 100, value => m_Contents.count = value);
    }
}
```

The helpers each add a control to the panel and return it:

```csharp
protected Slider    AddSlider(string label, float value, float low, float high, Action<float> onChanged);
protected SliderInt AddSliderInt(string label, int value, int low, int high, Action<int> onChanged);
protected Toggle    AddToggle(string label, bool value, Action<bool> onChanged);
protected EnumField AddEnum<TEnum>(string label, TEnum value, Action<TEnum> onChanged) where TEnum : Enum;
protected T         AddElement<T>(T element) where T : VisualElement;
```

- `value` is the control's starting value. Pass the field the control changes, so that field is the one place the default lives.
- A slider reports its value when the drag ends, not on every step of the drag, so a control that rebuilds the example does that once per drag.
- There is no `rebuild` argument. If a change should rebuild the example, have the callback call a method on your Contents script.
- Use `controlsMenu` for the two or three hold-down buttons an example may need, and `workshop` for the rare control that changes something the Workshop owns, such as `workshop.SetExampleSubSteps(value)`.

## 5. Global settings (`WorkshopExampleState`)

Some settings belong to the Workshop, not to one scene. An example declares what it needs in the **State** of its info asset.
The Workshop applies it before the scene loads and puts it back when the scene unloads, so an example never has to restore anything itself.

| Setting | Effect |
|---|---|
| Overridden Draw Options | The parts of the debug drawing the example takes over. Their menu toggles are greyed out. |
| Fixed Draw Options | What those parts are forced to. Naming a part in the override and leaving it out here turns it off. |
| Sleeping | Allow bodies to sleep, or not. |
| Frame Rate Visible | Show or hide the frame rate readout. |
| Catch Up Steps | Allow a slow frame to run several steps, or hold it to one. |
| Sub Steps | The world's sub-steps while the example is loaded. Zero leaves the menu's value alone. |

The "Default" value of the override settings leaves the menu as it is.
An example may also supply a control that moves one of these while it runs, as the Many Prismatics example does for sub-steps. The Workshop still restores the original on unload.

## 6. Modifying an existing example

- **Change what is simulated:** edit the example's scene, prefabs and Contents script.
- **Change the menu text or category:** edit the info asset, then run **Rebuild Workshop Registry**.
- **Change the default of a menu control:** change the field the control is passed, in the Contents script or the component.
- **Rename an example:** rename its folder, scene, info asset and scripts together in the Unity Project window so the `.meta` files move with them, update the info asset's name, and run the registry tool.

## 7. Pre-flight checklist

- [ ] `Assets/Examples/<Name>/` has a scene and a `<Name>Info.asset`.
- [ ] The scene has a `Camera` with a `CameraManipulator`, framed to show the whole example.
- [ ] The physics is authored with components, and a script exists only for what no component covers.
- [ ] Constraints are enabled after the poses and areas they connect.
- [ ] Any Contents script uses a fixed random seed and rebuilds correctly when the scene is reloaded.
- [ ] Any menu controls are in one `WorkshopOptionsProvider`, and global settings are in the info asset's State.
- [ ] The info asset has a scene, a name, a category and a one sentence description.
- [ ] Ran **`Tools > 2D > Physics > Rebuild Workshop Registry`**, and the example appears in the menu.
- [ ] No legacy `Physics2D` types and no `[Obsolete]` members.
