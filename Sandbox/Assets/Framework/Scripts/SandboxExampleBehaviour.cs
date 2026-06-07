using System;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

/// <summary>
/// Base class for every Sandbox example. It absorbs the infrastructure that used to be copied
/// into each example: finding the shared managers, wiring the options-menu chrome, hooking the
/// Reset action, and clearing override state on teardown.
///
/// A concrete example only needs to:
///   * tag itself with <see cref="ExampleSceneAttribute"/>,
///   * implement <see cref="SetupScene"/> (the actual PhysicsCore2D code),
///   * optionally override <see cref="CameraSize"/>/<see cref="CameraPosition"/> for framing,
///   * optionally override <see cref="SetupOptions"/> to wire its own menu controls,
///   * optionally override <see cref="OnExampleEnable"/>/<see cref="OnExampleDisable"/> for
///     per-example setup/teardown (events, native collections, world overrides).
/// </summary>
public abstract class SandboxExampleBehaviour : MonoBehaviour
{
    // Shared infrastructure, resolved once in OnEnable.
    protected SandboxManager SandboxManager { get; private set; }
    protected SceneManifest SceneManifest { get; private set; }
    protected CameraManipulator CameraManipulator { get; private set; }

    // Convenience accessors so examples read like the physics they teach.
    protected PhysicsWorld World => PhysicsWorld.defaultWorld;
    protected ref Unity.Mathematics.Random Random => ref SandboxManager.Random;
    protected Color ShapeColor => SandboxManager.ShapeColorState;

    /// <summary>
    /// The container the option-builder helpers add controls to. This lives in the persistent
    /// MainMenu's "Scenes" tab (<see cref="SandboxManager.SceneOptionsContent"/>) and is cleared on
    /// scene build and teardown. Use it directly with <see cref="AddElement{T}"/> for widgets the
    /// typed helpers don't cover.
    /// </summary>
    protected VisualElement OptionsContent { get; private set; }

    /// <summary>Base orthographic camera size for this example. Override to reframe.</summary>
    protected virtual float CameraSize => 6f;

    /// <summary>Initial camera position for this example. Override to reframe.</summary>
    protected virtual Vector2 CameraPosition => Vector2.zero;

    private void OnEnable()
    {
        // Resolve the shared infrastructure (lives in the always-loaded Sandbox.unity).
        SandboxManager = FindFirstObjectByType<SandboxManager>();
        SceneManifest = FindFirstObjectByType<SceneManifest>();
        CameraManipulator = FindFirstObjectByType<CameraManipulator>();

        // Frame the camera.
        CameraManipulator.CameraSize = CameraSize;
        CameraManipulator.CameraPosition = CameraPosition;

        // The Reset (R) control rebuilds the scene.
        SandboxManager.SceneResetAction = RebuildScene;

        // Per-example setup (state, world overrides, event subscriptions, native allocation).
        OnExampleEnable();

        // Build this example's option controls into the shared MainMenu "Scenes" tab.
        BuildOptionsUI();

        // Build the initial scene.
        RebuildScene();
    }

    private void OnDisable()
    {
        // Per-example teardown (event unsubscription, native disposal, world restoration).
        OnExampleDisable();

        // Remove this example's controls + description from the persistent MainMenu so they don't
        // linger after the scene unloads.
        SandboxManager.ClearSceneOptions();

        // Clear any overrides the example set. These early-return if nothing was overridden,
        // so calling them unconditionally is safe.
        SandboxManager.ResetOverrideDrawOptions();
        SandboxManager.ResetOverrideColorShapeState();
    }

    /// <summary>
    /// Reset the world to a clean state and rebuild the example scene. Call this from option
    /// callbacks that change the scene structure. It is also the registered Reset action.
    /// </summary>
    protected void RebuildScene()
    {
        OnBeforeResetScene();
        SandboxManager.ResetSceneState();
        SetupScene();
    }

    // Populates this example's controls + description into the persistent MainMenu "Scenes" tab.
    private void BuildOptionsUI()
    {
        // The controls now live in the persistent MainMenu, so clear the previous scene's first.
        OptionsContent = SandboxManager.SceneOptionsContent;
        OptionsContent.Clear();

        // Populate the scene description.
        SandboxManager.SetSceneDescription(SceneManifest.LoadedSceneDescription);

        // Let the example build its controls (into OptionsContent, via the AddX helpers).
        SetupOptions();

        // Show/hide + collapse the "Options" section header now the controls (if any) are added.
        SandboxManager.RefreshSceneOptionsSection();
    }

    /// <summary>
    /// Build this example's option controls using the <c>AddSlider</c>/<c>AddSliderInt</c>/
    /// <c>AddToggle</c>/<c>AddEnum</c> helpers (which add to <see cref="OptionsContent"/>), or
    /// <see cref="AddElement{T}"/> for widgets those don't cover. Title and description are handled
    /// by the shared chrome — no need to touch them here.
    /// </summary>
    protected virtual void SetupOptions() { }

    // --- Option-control builder helpers -------------------------------------------------------
    // Each helper constructs a control, applies the shared conventions (focusable off, input field
    // shown for sliders), registers a value-changed callback that runs the example's handler and
    // optionally rebuilds the scene, adds it to OptionsContent, and returns it for caching.

    /// <summary>Add a float <see cref="Slider"/> to the options panel.</summary>
    protected Slider AddSlider(string label, float value, float low, float high, Action<float> onChanged, bool rebuild = false)
    {
        var slider = new Slider(label, low, high) { value = value, showInputField = true, fill = true, focusable = false };
        slider.RegisterValueChangedCallback(evt =>
        {
            onChanged?.Invoke(evt.newValue);
            if (rebuild) RebuildScene();
        });
        OptionsContent.Add(slider);
        return slider;
    }

    /// <summary>Add an integer <see cref="SliderInt"/> to the options panel.</summary>
    protected SliderInt AddSliderInt(string label, int value, int low, int high, Action<int> onChanged, bool rebuild = false)
    {
        var slider = new SliderInt(label, low, high) { value = value, showInputField = true, fill = true, focusable = false };
        slider.RegisterValueChangedCallback(evt =>
        {
            onChanged?.Invoke(evt.newValue);
            if (rebuild) RebuildScene();
        });
        OptionsContent.Add(slider);
        return slider;
    }

    /// <summary>Add a <see cref="Toggle"/> to the options panel.</summary>
    protected Toggle AddToggle(string label, bool value, Action<bool> onChanged, bool rebuild = false)
    {
        var toggle = new Toggle(label) { value = value, focusable = false };
        toggle.RegisterValueChangedCallback(evt =>
        {
            onChanged?.Invoke(evt.newValue);
            if (rebuild) RebuildScene();
        });
        OptionsContent.Add(toggle);
        return toggle;
    }

    /// <summary>Add an <see cref="EnumField"/> bound to enum type <typeparamref name="TEnum"/>.</summary>
    protected EnumField AddEnum<TEnum>(string label, TEnum value, Action<TEnum> onChanged, bool rebuild = false) where TEnum : Enum
    {
        var field = new EnumField(label, value) { focusable = false };
        field.RegisterValueChangedCallback(evt =>
        {
            onChanged?.Invoke((TEnum)evt.newValue);
            if (rebuild) RebuildScene();
        });
        OptionsContent.Add(field);
        return field;
    }

    /// <summary>
    /// Add an already-constructed element to the options panel and return it. Use for display-only
    /// or specialised widgets (e.g. <see cref="ProgressBar"/>, <c>MinMaxSlider</c>, read-only
    /// <c>FloatField</c>, <see cref="Label"/>) that the typed helpers above don't cover.
    /// </summary>
    protected T AddElement<T>(T element) where T : VisualElement
    {
        OptionsContent.Add(element);
        return element;
    }

    /// <summary>
    /// Runs at the very start of <see cref="RebuildScene"/>, before the world is reset. Override
    /// only if you must do work *before* non-owned bodies are destroyed (rare — e.g. resetting an
    /// external draw batch keyed by body).
    /// </summary>
    protected virtual void OnBeforeResetScene() { }

    /// <summary>Per-example setup, run before the options UI and the first scene build.</summary>
    protected virtual void OnExampleEnable() { }

    /// <summary>Per-example teardown, run before the base clears override state.</summary>
    protected virtual void OnExampleDisable() { }

    /// <summary>
    /// Create the physics objects for this example. The world has already been reset when this
    /// is called, so begin directly with the PhysicsCore2D code.
    /// </summary>
    protected abstract void SetupScene();
}
