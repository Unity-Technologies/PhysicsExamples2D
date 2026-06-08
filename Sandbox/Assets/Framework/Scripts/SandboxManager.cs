using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;
using Unity.U2D.Physics.Extras;
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

public class SandboxManager : MonoBehaviour, IShapeColorProvider, IFoldable
{
    public ref Random Random => ref m_Random;
    public bool WorldPaused { get; private set; }

    public bool WorldSleeping
    {
        get => m_SleepingElement.value;
        set => m_SleepingElement.value = value;
    }

    public Color ShapeColorState => ColorShapeState ? new Color() : Color.HSVToRGB(m_Random.NextFloat(0f, 1f), m_Random.NextFloat(0.7f, 1f) * m_SaturationScaleElement.value, m_Random.NextFloat(0.5f, 1f));
    bool IShapeColorProvider.IsShapeColorActive => ColorShapeState;

    public float CameraZoom
    {
        get => m_CameraZoomElement.value;
        set => m_CameraZoomElement.value = value;
    }

    // The per-scene controls container in the MainMenu "Scenes" tab. Examples build their controls
    // here via SandboxExampleBehaviour's AddX helpers; it's cleared on scene build/teardown.
    public VisualElement SceneOptionsContent => m_SceneOptionsContent;
    public void SetSceneDescription(string text) => m_SceneDescription.text = text;
    public void ClearSceneOptions()
    {
        m_SceneOptionsContent.Clear();
        m_SceneDescription.text = string.Empty;
        m_SceneOptionsHeader.style.display = DisplayStyle.None;
    }

    // Called by the loaded example after it builds its controls: shows the "Options" section header
    // only when controls were actually added, and re-applies the collapse state.
    public void RefreshSceneOptionsSection()
    {
        m_SceneOptionsHeader.style.display = m_SceneOptionsContent.childCount > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        SetSceneOptionsCollapsed(m_SceneOptionsCollapsed);
    }

    // Collapses/expands the per-scene controls under the "Options" header (flips the caret).
    private void SetSceneOptionsCollapsed(bool collapsed)
    {
        m_SceneOptionsCollapsed = collapsed;
        m_SceneOptionsContent.style.display = collapsed ? DisplayStyle.None : DisplayStyle.Flex;
        var caret = collapsed ? "▶" : "▼";
        m_SceneOptionsHeader.text = $"{SandboxUtility.HighlightColor}{caret}{SandboxUtility.EndHighlightColor}<size=50%> </size>Options";
    }

    private void ToggleSceneOptions() => SetSceneOptionsCollapsed(!m_SceneOptionsCollapsed);

    public enum FrequencySelection
    {
        Hertz15,
        Hertz30,
        Hertz60,
        Hertz120,
        Variable
    }

    public FrequencySelection Frequency
    {
        get => m_FrequencySelection;
        private set
        {
            m_FrequencySelection = value;

            var fixedRate = m_FrequencySelection != FrequencySelection.Variable;
            if (fixedRate)
            {
                Time.fixedDeltaTime = 1.0f / m_FrequencySelection switch
                {
                    FrequencySelection.Hertz15 => 15f,
                    FrequencySelection.Hertz30 => 30f,
                    FrequencySelection.Hertz60 => 60f,
                    FrequencySelection.Hertz120 => 120f,
                    _ => throw new ArgumentOutOfRangeException(nameof(m_FrequencySelection), m_FrequencySelection, null)
                };
            }

            // Update the worlds.
            using var worlds = PhysicsWorld.GetWorlds();
            foreach (var world in worlds)
                world.simulationType = fixedRate ? PhysicsWorld.SimulationType.FixedUpdate : PhysicsWorld.SimulationType.Update;

        }
    }

    private bool ColorShapeState { get; set; }

    // Migrated global-control buttons (now live in the BottomLeftMenu's Shortcuts section).
    private Button m_PausePlayButton;
    private Button m_SingleStepButton;
    private Button m_InteractionButton;
    private Button m_ColorsButton;
    private Button m_FoldAllButton;

    // Fold All / Unfold All state.
    private bool m_AllFolded;

    public string StartScene = string.Empty;
    public Action SceneResetAction;
    public DebugView DebugView;
    public BottomLeftMenu BottomLeftMenu;
    public ControlsMenu ControlsMenu;

    // Override state.
    private FrequencySelection m_FrequencySelection;
    private PhysicsWorld.DrawOptions m_OverrideDrawOptions;
    private PhysicsWorld.DrawOptions m_OverridePreviousDrawOptions;
    private bool m_OverrideColorShapeState;
    private bool m_OverridePreviousColorShapeState;

    private struct MenuDefaults
    {
        // PhysicsWorld.
        public int Workers;
        public int SubSteps;
        public FrequencySelection Frequency;
        public bool WarmStarting;
        public bool Sleeping;
        public bool Continuous;

        // Draw.
        public float ExplodeImpulse;
        public float CameraZoom;
        public bool ColorShapeState;
        public float SaturationScale;
        public float DrawThickness;
        public float DrawPointScale;
        public float DrawNormalScale;
        public float DrawImpulseScale;
        public PhysicsWorld.DrawOptions DrawOptions;
    }

    private CameraManipulator m_CameraManipulator;
    [SerializeField] private SceneManifest m_SceneManifest;
    private UIDocument m_MainMenuDocument;
    private DropdownField m_SceneCategories;
    private DropdownField m_Scenes;
    private VisualElement m_SceneOptionsContent;
    private Button m_SceneOptionsHeader;
    private bool m_SceneOptionsCollapsed;
    private Label m_SceneDescription;

    // Examples panel roll-up: the "Examples" header caret collapses the panel content (the header
    // stays). Joins Fold All via IFoldable.
    private Button m_ExamplesHeader;
    private VisualElement m_ExamplesDetails;
    private bool m_ExamplesFolded;
    private MenuDefaults m_MenuDefaults;
    private bool m_DisableUIRestarts;
    private Dictionary<PhysicsWorld.DrawOptions, Toggle> m_DrawFlagElements;

    // Draw Elements (color).
    private Slider m_SaturationScaleElement;

    // PhysicsWorld Elements.
    private SliderInt m_WorkersElement;
    private SliderInt m_SubStepsElement;
    private EnumField m_FrequencyElement;
    private Toggle m_WarmStartingElement;
    private Toggle m_SleepingElement;
    private Toggle m_ContinuousElement;

    // Draw Elements.
    private Slider m_ExplodeImpulseElement;
    private Slider m_CameraZoomElement;
    private Slider m_DrawThicknessElement;
    private Slider m_DrawPointScaleElement;
    private Slider m_DrawNormalScaleElement;
    private Slider m_DrawImpulseScaleElement;
    private Toggle m_DrawBodiesElement;
    private Toggle m_DrawShapesElement;
    private Toggle m_DrawJointsElement;
    private Toggle m_DrawBoundsElement;
    private Toggle m_DrawIslandsElement;
    private Toggle m_DrawContactPointsElement;
    private Toggle m_DrawContactNormalsElement;
    private Toggle m_DrawContactTangentsElement;
    private Toggle m_DrawContactImpulsesElement;

    private readonly List<TreeViewItemData<string>> m_ViewItems = new();
    private Random m_Random;
    private bool m_IgnoreAutoSceneSelection;
    private EventCallback<ChangeEvent<string>> m_SceneChangedCallback;

    /// <summary>The scene manifest asset. Exposed for <see cref="SandboxExampleBehaviour"/>.</summary>
    public SceneManifest SceneManifest => m_SceneManifest;

    private void Start()
    {
#if UNITY_EDITOR
        if (!SystemInfo.supportsComputeShaders)
            EditorUtility.DisplayDialog("Compute Shader Support Missing", "2D Physics requires compute shader support for its debug renderer. Without this, you will not see physics debug rendering although physics itself will be unaffected.", "OK");
#endif
        m_SceneManifest.Initialize();
        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_MainMenuDocument = GetComponent<UIDocument>();

        // Disable this because it's not needed and causing Input system problems.
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        
        // Show the bottom-left menu by default (it hosts the Sandbox + Shortcuts sections).
        BottomLeftMenu.gameObject.SetActive(true);

        // The Debug view is always present now (folding hides it); there's no longer a toggle for it.
        DebugView.gameObject.SetActive(true);

        // Reset the per-scene controls bar (now holds only scene-custom buttons).
        ControlsMenu.ResetControls();

        // Cache the migrated global-control buttons (hosted by the BottomLeftMenu).
        m_PausePlayButton = BottomLeftMenu.PausePlayButton;
        m_SingleStepButton = BottomLeftMenu.SingleStepButton;
        m_InteractionButton = BottomLeftMenu.InteractionButton;
        m_ColorsButton = BottomLeftMenu.ColorsButton;
        m_FoldAllButton = BottomLeftMenu.FoldAllButton;

        // Interaction: single toggle button; text shows the mode a click will switch to (see
        // UpdateInputModeVisual).
        m_InteractionButton.clicked += ToggleInputMode;

        // Colors: single toggle button; text shows the action it will perform (see UpdateColorsVisual).
        m_ColorsButton.clicked += ToggleColorShapeState;

        // Pause/Play: single toggle button; text shows the action it will perform. Single-Step is
        // on its own line and enabled only when paused.
        m_PausePlayButton.clicked += TogglePausePlay;
        m_SingleStepButton.clicked += SingleStep;
        m_SingleStepButton.text = $"Single-Step [{SandboxUtility.HighlightColor}S{SandboxUtility.EndHighlightColor}]";

        // Reset (also resets the camera — see ResetScene).
        BottomLeftMenu.ResetButton.clicked += ResetScene;
        BottomLeftMenu.ResetButton.text = $"Reset [{SandboxUtility.HighlightColor}R{SandboxUtility.EndHighlightColor}]";

        // Restart (resets all settings and reloads the scene).
        BottomLeftMenu.RestartButton.clicked += Restart;
        BottomLeftMenu.RestartButton.text = $"Restart [{SandboxUtility.HighlightColor}X{SandboxUtility.EndHighlightColor}]";

        // Fold All / Unfold All.
        m_FoldAllButton.clicked += ToggleFoldAll;
        UpdateFoldAllVisual();

        // Quit.
        BottomLeftMenu.QuitButton.clicked += QuitApplication;
        BottomLeftMenu.QuitButton.text = $"Quit [{SandboxUtility.HighlightColor}Esc{SandboxUtility.EndHighlightColor}]";

        // Hide the Quit button on the Web platform.
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            BottomLeftMenu.QuitButton.style.display = DisplayStyle.None;

            // Remove the Unity-side frame-rate cap on WebGL so the browser's requestAnimationFrame
            // can run above 60fps on high-refresh displays and avoids the snap-to-30fps halving.
            Application.targetFrameRate = -1;
        }

        var defaultWorld = PhysicsWorld.defaultWorld;
        m_MenuDefaults = new MenuDefaults
        {
            // PhysicsWorld.
            Workers = defaultWorld.simulationWorkers,
            SubSteps = defaultWorld.simulationSubSteps,
            Frequency = FrequencySelection.Hertz60,
            WarmStarting = defaultWorld.warmStartingAllowed,
            Sleeping = defaultWorld.sleepingAllowed,
            Continuous = defaultWorld.continuousAllowed,

            // Drawing.
            ExplodeImpulse = 30f,
            CameraZoom = 1f,
            ColorShapeState = false,
            SaturationScale = 0.65f,
            DrawThickness = defaultWorld.drawThickness,
            DrawPointScale = defaultWorld.drawPointScale,
            DrawNormalScale = defaultWorld.drawNormalScale,
            DrawImpulseScale = defaultWorld.drawForceScale,
            DrawOptions = defaultWorld.drawOptions
        };

        // We must set up the options prior to the scene selection controls as we trigger them during selection.
        SetupOptions();
        SetupSceneSelectionControls();

        // Apply the initial Pause/Play visual (sets the Shortcuts button text and disables
        // Single-Step while playing).
        UpdatePausePlayVisual();
    }

    private void OnEnable()
    {
        m_DrawFlagElements = new Dictionary<PhysicsWorld.DrawOptions, Toggle>(capacity: 8);

        // Overrides.
        m_OverrideDrawOptions = PhysicsWorld.DrawOptions.Off;
        m_OverridePreviousDrawOptions = PhysicsWorld.DrawOptions.Off;
    }

    private void Update()
    {
        // Controls.
        {
            var currentKeyboard = Keyboard.current;

            // Quit (no-op on the Web platform).
            if (currentKeyboard.escapeKey.wasPressedThisFrame)
            {
                QuitApplication();
                return;
            }

            // Single-Step.
            if (currentKeyboard.sKey.wasPressedThisFrame)
            {
                // Single-step.
                SingleStep();
            }

            // Pause/Play.
            if (currentKeyboard.pKey.wasPressedThisFrame)
            {
                TogglePausePlay();
            }

            // Reset.
            if (currentKeyboard.rKey.wasPressedThisFrame)
            {
                ResetScene();
            }

            // Restart.
            if (currentKeyboard.xKey.wasPressedThisFrame)
            {
                Restart();
            }

            // Fold All / Unfold All.
            if (currentKeyboard.tabKey.wasPressedThisFrame)
            {
                ToggleFoldAll();
                return;
            }

            // Toggle Color PhysicsShape State.
            if (currentKeyboard.cKey.wasPressedThisFrame)
            {
                ToggleColorShapeState();
                return;
            }

            // Interaction mode (toggles Drag <-> Explode).
            if (currentKeyboard.iKey.wasPressedThisFrame)
                ToggleInputMode();
        }
    }

    // Folds (true) or unfolds (false) every foldable panel, then updates the toggle button.
    private void ToggleFoldAll()
    {
        m_AllFolded = !m_AllFolded;

        // Re-gather each time so panels loaded after startup (e.g. the per-scene options panel)
        // automatically participate — implementing IFoldable is all that's required.
        foreach (var behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude))
        {
            if (behaviour is IFoldable foldable && behaviour.isActiveAndEnabled)
                foldable.SetFolded(m_AllFolded);
        }

        UpdateFoldAllVisual();
    }

    private void UpdateFoldAllVisual()
    {
        m_FoldAllButton.text = m_AllFolded
            ? $"Unfold All [{SandboxUtility.HighlightColor}Tab{SandboxUtility.EndHighlightColor}]"
            : $"Fold All [{SandboxUtility.HighlightColor}Tab{SandboxUtility.EndHighlightColor}]";
    }

    // IFoldable: "Fold All" collapses the Examples panel along with the other windows. (The Options
    // panel folds itself via its own IFoldable.)
    public void SetFolded(bool folded) => SetExamplesFolded(folded);

    // Rolls the Examples panel up to just its header (folded) or expands it; flips the header caret.
    private void SetExamplesFolded(bool folded)
    {
        m_ExamplesFolded = folded;

        m_ExamplesDetails.style.display = folded ? DisplayStyle.None : DisplayStyle.Flex;

        // Caret to the left of "Examples", with the same half-character spacing as the other panels.
        var caret = folded ? "▶" : "▼";
        m_ExamplesHeader.text = $"{SandboxUtility.HighlightColor}{caret}{SandboxUtility.EndHighlightColor}<size=50%> </size>Examples";
    }

    private void ToggleExamplesFolded() => SetExamplesFolded(!m_ExamplesFolded);

    // Quits the application: exits Play mode in the Editor, quits a build, no-op on WebGL.
    private void QuitApplication()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            return;

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Input mode is now held directly on the CameraManipulator (the Options dropdown was removed);
    // the Shortcuts "Interaction" toggle is the only UI for it.
    private void SetInputMode(CameraManipulator.InputMode mode)
    {
        m_CameraManipulator.TouchMode = mode;
        UpdateInputModeVisual();
    }

    // Toggles the interaction mode between Drag and Explode.
    private void ToggleInputMode()
    {
        var isDrag = m_CameraManipulator.TouchMode == CameraManipulator.InputMode.Drag;
        SetInputMode(isDrag ? CameraManipulator.InputMode.Explode : CameraManipulator.InputMode.Drag);
    }

    // The Interaction button is a single toggle: its text shows the mode a click will switch to.
    private void UpdateInputModeVisual()
    {
        var isDrag = m_CameraManipulator.TouchMode == CameraManipulator.InputMode.Drag;
        m_InteractionButton.text = isDrag
            ? $"Explode [{SandboxUtility.HighlightColor}I{SandboxUtility.EndHighlightColor}]"
            : $"Drag [{SandboxUtility.HighlightColor}I{SandboxUtility.EndHighlightColor}]";
    }

    // Colour state is held directly in the ColorShapeState property (the Options toggle was removed);
    // the Shortcuts "Colors" toggle is the only UI for it. Recolours by rebuilding the scene.
    private void ToggleColorShapeState()
    {
        if (m_OverrideColorShapeState)
            return;

        ColorShapeState = !ColorShapeState;
        UpdateColorsVisual();
        ResetScene();
    }

    // The Colors button is a single toggle: its text shows the action a click will perform.
    private void UpdateColorsVisual()
    {
        m_ColorsButton.text = ColorShapeState
            ? $"Colors Off [{SandboxUtility.HighlightColor}C{SandboxUtility.EndHighlightColor}]"
            : $"Colors On [{SandboxUtility.HighlightColor}C{SandboxUtility.EndHighlightColor}]";
    }

    public void ResetSceneState()
    {
        // Disable any "TestBody".
        foreach (var testBody in FindObjectsByType<TestBody>(FindObjectsInactive.Include))
            testBody.enabled = false;

        // Disable any "TestWorld".
        foreach (var testWorld in FindObjectsByType<TestWorld>(FindObjectsInactive.Include))
            testWorld.enabled = false;

        {
            var destroyBodies = new NativeList<PhysicsBody>(1000, Allocator.Temp);

            // Iterate all worlds.
            using var allWorlds = PhysicsWorld.GetWorlds();
            foreach (var world in allWorlds)
            {
                // Iterate all non-owned bodies.
                using var bodies = world.GetBodies();
                foreach (var body in bodies)
                {
                    if (!body.isOwned)
                        destroyBodies.Add(body);
                }
            }

            if (destroyBodies.Length > 0)
                PhysicsWorld.DestroyBodyBatch(destroyBodies.AsArray());

            // Dispose.
            destroyBodies.Dispose();
        }

        // Clear the debug draw.
        ClearDebugDraw();

        // Reset random generator.
        m_Random.InitState(0x32628473);

        // Reset the default world.
        PhysicsWorld.defaultWorld.Reset();

        // Enable any "TestWorld".
        foreach (var testWorld in FindObjectsByType<TestWorld>(FindObjectsInactive.Include))
            testWorld.enabled = true;

        // Enable all "TestBody" again.
        foreach (var testBody in FindObjectsByType<TestBody>(FindObjectsInactive.Include))
            testBody.enabled = true;
    }

    private void SetupOptions()
    {
        // The Sandbox (world/draw) controls now live in the merged BottomLeftMenu panel; query them
        // from its UIDocument root (it's a serialized reference, so no lookup is needed).
        var root = BottomLeftMenu.GetComponent<UIDocument>().rootVisualElement;

        // Saturation scale (colour).
        {
            m_SaturationScaleElement = root.Q<Slider>("shape-saturation");
            m_SaturationScaleElement.value = m_MenuDefaults.SaturationScale;
            m_SaturationScaleElement.RegisterValueChangedCallback(_ => ResetScene());
        }

        // PhysicsWorld.
        {
            // Workers.
            m_WorkersElement = root.Q<SliderInt>("workers");
            m_WorkersElement.highValue = math.min(PhysicsConstants.MaxWorkers, JobsUtility.JobWorkerMaximumCount);
            m_WorkersElement.value = m_MenuDefaults.Workers;
            m_WorkersElement.RegisterValueChangedCallback(evt =>
            {
                // Update the worlds.
                using var worlds = PhysicsWorld.GetWorlds();
                foreach (var world in worlds)
                    world.simulationWorkers = evt.newValue;
            });

            // Sub-steps.
            m_SubStepsElement = root.Q<SliderInt>("sub-steps");
            m_SubStepsElement.value = m_MenuDefaults.SubSteps;
            m_SubStepsElement.RegisterValueChangedCallback(evt =>
            {
                // Update the worlds.
                using var worlds = PhysicsWorld.GetWorlds();
                foreach (var world in worlds)
                    world.simulationSubSteps = evt.newValue;
            });

            // Frequency.
            m_FrequencyElement = root.Q<EnumField>("frequency");
            m_FrequencyElement.RegisterValueChangedCallback(evt => Frequency = (FrequencySelection)evt.newValue);
            m_FrequencyElement.value = m_MenuDefaults.Frequency;

            // Warm Starting.
            m_WarmStartingElement = root.Q<Toggle>("warm-starting");
            m_WarmStartingElement.value = m_MenuDefaults.WarmStarting;
            m_WarmStartingElement.RegisterValueChangedCallback(evt =>
            {
                // Update the worlds.
                using var worlds = PhysicsWorld.GetWorlds();
                foreach (var world in worlds)
                    world.warmStartingAllowed = evt.newValue;
            });

            // Sleeping.
            m_SleepingElement = root.Q<Toggle>("sleeping");
            m_SleepingElement.value = m_MenuDefaults.Sleeping;
            m_SleepingElement.RegisterValueChangedCallback(evt =>
            {
                // Update the worlds.
                using var worlds = PhysicsWorld.GetWorlds();
                foreach (var world in worlds)
                    world.sleepingAllowed = evt.newValue;
            });

            // Continuous.
            m_ContinuousElement = root.Q<Toggle>("continuous");
            m_ContinuousElement.value = m_MenuDefaults.Continuous;
            m_ContinuousElement.RegisterValueChangedCallback(evt =>
            {
                // Update the worlds.
                using var worlds = PhysicsWorld.GetWorlds();
                foreach (var world in worlds)
                    world.continuousAllowed = evt.newValue;
            });

        }

        // Options.
        {
            // Explode Impulse.
            m_ExplodeImpulseElement = root.Q<Slider>("explode-impulse");
            m_ExplodeImpulseElement.RegisterValueChangedCallback(evt => { m_CameraManipulator.ExplodeImpulse = evt.newValue; });
            m_ExplodeImpulseElement.value = m_MenuDefaults.ExplodeImpulse;

            // Camera Zoom.
            m_CameraZoomElement = root.Q<Slider>("camera-zoom");
            m_CameraZoomElement.value = m_MenuDefaults.CameraZoom;
            m_CameraZoomElement.RegisterValueChangedCallback(evt => m_CameraManipulator.CameraZoom = evt.newValue);

            // Draw Thickness.
            m_DrawThicknessElement = root.Q<Slider>("draw-thickness");
            m_DrawThicknessElement.RegisterValueChangedCallback(evt =>
            {
                // Update the worlds.
                using var worlds = PhysicsWorld.GetWorlds();
                foreach (var world in worlds)
                    world.drawThickness = evt.newValue;
            });
            m_DrawThicknessElement.value = m_MenuDefaults.DrawThickness;

            // Draw Point Scale.
            m_DrawPointScaleElement = root.Q<Slider>("draw-point-scale");
            m_DrawPointScaleElement.RegisterValueChangedCallback(evt =>
            {
                // Update the worlds.
                using var worlds = PhysicsWorld.GetWorlds();
                foreach (var world in worlds)
                    world.drawPointScale = evt.newValue;
            });
            m_DrawPointScaleElement.value = m_MenuDefaults.DrawPointScale;

            // Draw Normal Scale.
            m_DrawNormalScaleElement = root.Q<Slider>("draw-normal-scale");
            m_DrawNormalScaleElement.RegisterValueChangedCallback(evt =>
            {
                // Update the worlds.
                using var worlds = PhysicsWorld.GetWorlds();
                foreach (var world in worlds)
                    world.drawNormalScale = evt.newValue;
            });
            m_DrawNormalScaleElement.value = m_MenuDefaults.DrawNormalScale;

            // Draw Impulse Scale.
            m_DrawImpulseScaleElement = root.Q<Slider>("draw-impulse-scale");
            m_DrawImpulseScaleElement.RegisterValueChangedCallback(evt =>
            {
                // Update the worlds.
                using var worlds = PhysicsWorld.GetWorlds();
                foreach (var world in worlds)
                    world.drawForceScale = evt.newValue;
            });
            m_DrawImpulseScaleElement.value = m_MenuDefaults.DrawImpulseScale;

            // Colour state + input mode have no Options controls any more; initialise them and sync
            // the Shortcuts "Colors"/"Interaction" buttons (the only UI for them now).
            ColorShapeState = m_MenuDefaults.ColorShapeState;
            UpdateColorsVisual();
            SetInputMode(CameraManipulator.InputMode.Drag);

            // Bodies.
            m_DrawBodiesElement = ConfigureDrawFlag(root, "draw-bodies", PhysicsWorld.DrawOptions.AllBodies);
            m_DrawShapesElement = ConfigureDrawFlag(root, "draw-shapes", PhysicsWorld.DrawOptions.AllShapes);
            m_DrawJointsElement = ConfigureDrawFlag(root, "draw-joints", PhysicsWorld.DrawOptions.AllJoints);
            m_DrawBoundsElement = ConfigureDrawFlag(root, "draw-shape-bounds", PhysicsWorld.DrawOptions.AllShapeBounds);
            m_DrawIslandsElement = ConfigureDrawFlag(root, "draw-solver-islands", PhysicsWorld.DrawOptions.AllSolverIslands);
            m_DrawContactPointsElement = ConfigureDrawFlag(root, "draw-contact-points", PhysicsWorld.DrawOptions.AllContactPoints);
            m_DrawContactNormalsElement = ConfigureDrawFlag(root, "draw-contact-normals", PhysicsWorld.DrawOptions.AllContactNormal);
            m_DrawContactTangentsElement = ConfigureDrawFlag(root, "draw-contact-tangents", PhysicsWorld.DrawOptions.AllContactFriction);
            m_DrawContactImpulsesElement = ConfigureDrawFlag(root, "draw-contact-impulses", PhysicsWorld.DrawOptions.AllContactForces);
        }
    }

    private Toggle ConfigureDrawFlag(VisualElement root, string elementName, PhysicsWorld.DrawOptions targetDrawFlag)
    {
        var defaultWorld = PhysicsWorld.defaultWorld;

        var drawFlagElement = root.Q<Toggle>(elementName);
        drawFlagElement.value = defaultWorld.drawOptions.HasFlag(targetDrawFlag);
        drawFlagElement.RegisterValueChangedCallback(evt =>
        {
            // Finish if we're overriding this draw flag.
            if ((m_OverrideDrawOptions & targetDrawFlag) != 0)
                return;

            var currentDrawOptions = defaultWorld.drawOptions;
            var newDrawOptions = evt.newValue ? currentDrawOptions | targetDrawFlag : currentDrawOptions & ~targetDrawFlag;

            // Update the worlds.
            using var worlds = PhysicsWorld.GetWorlds();
            foreach (var world in worlds)
                world.drawOptions = newDrawOptions;
        });

        m_DrawFlagElements.Add(targetDrawFlag, drawFlagElement);

        return drawFlagElement;
    }

    private void SetupSceneSelectionControls()
    {
        var root = m_MainMenuDocument.rootVisualElement;

        // Fetch the controls.
        m_SceneCategories = root.Q<DropdownField>("scene-categories");
        m_Scenes = root.Q<DropdownField>("scenes");

        // Per-scene controls + description containers (populated by the loaded example).
        m_SceneOptionsContent = root.Q<VisualElement>("scene-controls");
        m_SceneDescription = root.Q<Label>("scene-description");

        // Collapsible "Options" section header above the per-scene controls. Hidden until a scene
        // actually adds controls (see RefreshSceneOptionsSection).
        m_SceneOptionsHeader = root.Q<Button>("scene-options-header");
        m_SceneOptionsHeader.clicked += ToggleSceneOptions;
        m_SceneOptionsHeader.style.display = DisplayStyle.None;

        // Examples panel roll-up: the "Examples" header caret collapses the panel content.
        m_ExamplesHeader = root.Q<Button>("examples-header");
        m_ExamplesDetails = root.Q<VisualElement>("examples-details");
        m_ExamplesHeader.clicked += ToggleExamplesFolded;
        SetExamplesFolded(false);

        // Suppress the spurious horizontal scrollbar that appears in these dropdown popups at
        // non-integer panel scales (e.g. 4k full-screen with the PanelSettings "Scale With Screen
        // Size" 1200x800 reference => 3.2x). See SuppressDropdownHorizontalScrollbar.
        SuppressDropdownHorizontalScrollbar(m_SceneCategories);
        SuppressDropdownHorizontalScrollbar(m_Scenes);

        // Add categories.
        m_SceneCategories.choices.AddRange(m_SceneManifest.GetCategories());
        
        // Register a category change.
        m_SceneCategories.RegisterValueChangedCallback(evt => SceneCategoryChanged(evt.newValue));
        
        // Resolve the configured start scene. If it's empty or no longer registered (e.g. the
        // example was disabled or removed), warn and fall back to the first registered scene so
        // startup never throws.
        if (m_SceneManifest.SceneItems.Count == 0)
        {
            Debug.LogWarning("[Sandbox] No scenes are registered in the SceneManifest; cannot select a start scene.");
        }
        else
        {
            if (!m_SceneManifest.TryGetSceneItem(StartScene, out var startItem))
            {
                startItem = m_SceneManifest.SceneItems[0];
                if (string.IsNullOrEmpty(StartScene))
                    Debug.LogWarning($"[Sandbox] No start scene set; using the first registered scene '{startItem.Name}'.");
                else
                    Debug.LogWarning($"[Sandbox] Start scene '{StartScene}' is not registered; using the first registered scene '{startItem.Name}'.");
            }

            // Select the resolved scene (suppress the category-change auto-selection so we land on
            // exactly this scene).
            m_IgnoreAutoSceneSelection = true;
            m_SceneCategories.value = startItem.Category;
            m_IgnoreAutoSceneSelection = false;
            m_Scenes.value = startItem.Name;
        }
    }

    // A DropdownField's popup is a GenericDropdownMenu created fresh each time it opens and parented
    // to the panel root (outside this UIDocument's subtree). At non-integer panel scales its internal
    // ScrollView reports a sub-pixel horizontal overflow and shows a phantom horizontal scrollbar even
    // though all item text fits. The scroller's visibility is driven by an inline style that USS cannot
    // override, so we set the official ScrollView.horizontalScrollerVisibility to Hidden on the popup
    // each time it opens (it persists for that popup's lifetime; the popup is rebuilt on the next open).
    private static void SuppressDropdownHorizontalScrollbar(DropdownField dropdown)
    {
        if (dropdown == null)
            return;

        dropdown.RegisterCallback<PointerDownEvent>(_ =>
        {
            // The popup is added synchronously on open, but defer one tick so it exists in the tree.
            dropdown.schedule.Execute(() =>
            {
                var popup = dropdown.panel?.visualTree.Q(className: "unity-base-dropdown");
                var scrollView = popup?.Q<ScrollView>();
                if (scrollView != null)
                    scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            });
        });
    }

    private void SceneCategoryChanged(string categoryName)
    {
        // Unregister via the cached delegate — unregistering a fresh lambda always fails silently.
        if (m_SceneChangedCallback != null)
            m_Scenes.UnregisterValueChangedCallback(m_SceneChangedCallback);

        // Add the category scenes.
        m_Scenes.choices.Clear();
        m_Scenes.index = -1;
        m_Scenes.choices.AddRange(m_SceneManifest.GetScenes(categoryName));

        // Register a scene change.
        m_SceneChangedCallback = evt => SceneChanged(evt.newValue);
        m_Scenes.RegisterValueChangedCallback(m_SceneChangedCallback);
        
        // Select the first scene (if not ignored).
        if (!m_IgnoreAutoSceneSelection)
            m_Scenes.index = 0;
    }

    private void SceneChanged(string sceneName)
    {
        // Ignore if invalid scene or the same as the currently loaded one.
        if (string.IsNullOrEmpty(sceneName) ||
            sceneName == m_SceneManifest.LoadedSceneName)
            return;

        // Unpause the world if paused.
        if (WorldPaused)
            TogglePausePlay();

        m_CameraManipulator.ResetPanZoom();
        m_CameraZoomElement.value = m_CameraManipulator.CameraZoom;

        DebugView.ResetStats();

        // Reset the controls.
        ControlsMenu.ResetControls();

        // Collapse the bottom-left panel so it can't overlap the new scene's options.
        BottomLeftMenu.Collapse();

        SceneResetAction = null;
        m_SceneManifest.LoadScene(sceneName, ResetSceneState);
    }

    private void ResetScene()
    {
        if (m_DisableUIRestarts)
            return;

        // Reset the camera pan/zoom back to the scene's framing (mirrors Restart/SceneChanged).
        m_CameraManipulator.ResetPanZoom();
        m_CameraZoomElement.value = m_MenuDefaults.CameraZoom;

        SceneResetAction?.Invoke();
    }

    // Reset the settings and reload the current scene.
    private void Restart()
    {
        m_DisableUIRestarts = true;

        // Worlds.
        m_WorkersElement.value = m_MenuDefaults.Workers;
        m_SubStepsElement.value = m_MenuDefaults.SubSteps;
        m_FrequencyElement.value = m_MenuDefaults.Frequency;
        m_WarmStartingElement.value = m_MenuDefaults.WarmStarting;
        m_SleepingElement.value = m_MenuDefaults.Sleeping;
        m_ContinuousElement.value = m_MenuDefaults.Continuous;

        // Drawing.
        m_ExplodeImpulseElement.value = m_MenuDefaults.ExplodeImpulse;
        m_CameraManipulator.ResetPanZoom();
        m_CameraZoomElement.value = m_MenuDefaults.CameraZoom;
        m_DrawThicknessElement.value = m_MenuDefaults.DrawThickness;
        m_DrawPointScaleElement.value = m_MenuDefaults.DrawPointScale;
        m_DrawNormalScaleElement.value = m_MenuDefaults.DrawNormalScale;
        m_DrawImpulseScaleElement.value = m_MenuDefaults.DrawImpulseScale;
        m_DrawBodiesElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllBodies);
        m_DrawShapesElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllShapes);
        m_DrawJointsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllJoints);
        m_DrawBoundsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllShapeBounds);
        m_DrawIslandsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllSolverIslands);
        m_DrawContactPointsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllContactPoints);
        m_DrawContactNormalsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllContactNormal);
        m_DrawContactTangentsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllContactFriction);
        m_DrawContactImpulsesElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllContactForces);

        m_SaturationScaleElement.value = m_MenuDefaults.SaturationScale;

        // Input mode + colour state have no Options controls; reset directly + sync Shortcuts buttons.
        SetInputMode(CameraManipulator.InputMode.Drag);
        ColorShapeState = m_MenuDefaults.ColorShapeState;
        UpdateColorsVisual();

        DebugView.ResetStats();

        // Reload the scene.
        SceneResetAction = null;
        m_SceneManifest.ReloadCurrentScene(ResetSceneState);

        m_DisableUIRestarts = false;
    }

    private void TogglePausePlay() => SetPaused(!WorldPaused);

    private void SetPaused(bool paused)
    {
        WorldPaused = paused;
        UpdatePausePlayVisual();

        // Update the worlds.
        using var worlds = PhysicsWorld.GetWorlds();
        foreach (var world in worlds)
            world.paused = WorldPaused;
    }

    // Pause/Play is a single Shortcuts toggle: the text shows the action a click will perform.
    // Single-Step is only meaningful while paused.
    private void UpdatePausePlayVisual()
    {
        m_PausePlayButton.text = WorldPaused
            ? $"Play [{SandboxUtility.HighlightColor}P{SandboxUtility.EndHighlightColor}]"
            : $"Pause [{SandboxUtility.HighlightColor}P{SandboxUtility.EndHighlightColor}]";

        m_SingleStepButton.enabledSelf = WorldPaused;
    }

    private void SingleStep()
    {
        if (!WorldPaused)
            return;

        var defaultWorld = PhysicsWorld.defaultWorld;

        // Update the worlds.
        using var worlds = PhysicsWorld.GetWorlds();
        foreach (var world in worlds)
        {
            var oldPaused = world.paused;
            var oldSimulationType = world.simulationType;

            world.simulationType = PhysicsWorld.SimulationType.Script;
            world.paused = false;
            world.Simulate(oldSimulationType == PhysicsWorld.SimulationType.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime);

            world.paused = oldPaused;
            world.simulationType = oldSimulationType;
        }
    }

    private static void ClearDebugDraw()
    {
        // Update the worlds.
        using var worlds = PhysicsWorld.GetWorlds();
        foreach (var world in worlds)
            world.ClearDraw();
    }

    public void SetOverrideDrawOptions(PhysicsWorld.DrawOptions overridenOptions, PhysicsWorld.DrawOptions fixedOptions)
    {
        // Finish if we're already overriding.
        if (m_OverrideDrawOptions != PhysicsWorld.DrawOptions.Off)
            return;

        // Disable the appropriate elements.
        foreach (var item in m_DrawFlagElements)
        {
            if ((item.Key & overridenOptions) == 0)
                continue;

            item.Value.enabledSelf = false;
        }

        // Set the override.
        m_OverridePreviousDrawOptions = PhysicsWorld.defaultWorld.drawOptions;
        m_OverrideDrawOptions = overridenOptions;
        UpdateOverrideWorldDrawOptions(fixedOptions);
    }

    public void ResetOverrideDrawOptions()
    {
        // Finish if we're not overriding.
        if (m_OverrideDrawOptions == PhysicsWorld.DrawOptions.Off)
            return;

        // Enable all the elements.
        foreach (var item in m_DrawFlagElements)
            item.Value.enabledSelf = true;

        // Restore previous draw flags.
        UpdateOverrideWorldDrawOptions(m_OverridePreviousDrawOptions);
        m_OverridePreviousDrawOptions = m_OverrideDrawOptions = PhysicsWorld.DrawOptions.Off;
    }

    private void UpdateOverrideWorldDrawOptions(PhysicsWorld.DrawOptions fixedOptions)
    {
        // Calculate new draw flags.
        var newDrawOptions = (PhysicsWorld.defaultWorld.drawOptions & ~m_OverrideDrawOptions) | fixedOptions;

        // Update the worlds.
        using var worlds = PhysicsWorld.GetWorlds();
        foreach (var world in worlds)
            world.drawOptions = newDrawOptions;
    }

    public void SetOverrideColorShapeState(bool colorShapeState)
    {
        // Finish if we're already in the requested state.
        if (m_OverrideColorShapeState)
            return;

        // Set the override.
        m_OverridePreviousColorShapeState = ColorShapeState;
        ColorShapeState = colorShapeState;
        m_OverrideColorShapeState = true;
        UpdateColorsVisual();

        // The example controls the colour state, so disable the Shortcuts "Colors" toggle.
        m_ColorsButton.enabledSelf = false;
    }

    public void ResetOverrideColorShapeState()
    {
        // Restore previous flag.
        ColorShapeState = m_OverridePreviousColorShapeState;
        m_OverrideColorShapeState = m_OverridePreviousColorShapeState = false;
        UpdateColorsVisual();

        // The example no longer controls the colour state, so re-enable the Shortcuts "Colors" toggle.
        m_ColorsButton.enabledSelf = true;
    }

    public void ShowFPS() => DebugView.ShowFPS();
    
    public void HideFPS() => DebugView.HideFPS();
}