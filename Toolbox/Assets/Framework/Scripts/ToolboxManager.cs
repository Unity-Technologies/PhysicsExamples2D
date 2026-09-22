using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

public class ToolboxManager : MonoBehaviour, IFoldable
{
    public ref Random Random => ref m_Random;
    public bool WorldPaused { get; private set; }

    public bool WorldSleeping
    {
        get => m_SleepingElement.value;
        set => m_SleepingElement.value = value;
    }

    public float CameraZoom
    {
        get => m_CameraZoomElement.value;
        set => m_CameraZoomElement.value = value;
    }

    // The per-scene controls container in the MainMenu "Scenes" tab. Examples build their controls
    // here via ToolboxExampleBehaviour's AddX helpers; it's cleared on scene build/teardown.
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
        m_SceneOptionsHeader.text = $"{ToolboxUtility.HighlightColor}{caret}{ToolboxUtility.EndHighlightColor}<size=50%> </size>Options";
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

    // Migrated global-control buttons (now live in the BottomLeftMenu's Shortcuts section).
    private Button m_PausePlayButton;
    private Button m_SingleStepButton;
    private Button m_InteractionButton;
    private Button m_FoldAllButton;

    // Fold All / Unfold All state.
    private bool m_AllFolded;

    public string StartScene = string.Empty;
    public DebugView DebugView;
    public BottomLeftMenu BottomLeftMenu;
    public ControlsMenu ControlsMenu;
    public LoadingOverlay LoadingOverlay;
    public GameObject FallbackCamera;

    // Override state.
    private FrequencySelection m_FrequencySelection;
    private PhysicsWorld.DrawOptions m_OverrideDrawOptions;
    private PhysicsWorld.DrawOptions m_OverridePreviousDrawOptions;

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
        public float DrawThickness;
        public float DrawPointScale;
        public float DrawNormalScale;
        public float DrawImpulseScale;
        public PhysicsWorld.DrawOptions DrawOptions;
    }

    private CameraManipulator m_CameraManipulator;
    private CameraManipulator.InputMode m_InputMode = CameraManipulator.InputMode.Drag;
    [SerializeField] private ToolboxManifest m_Manifest;
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

    // The example a switch is currently loading, and whether a switch is still running.
    // Both guard against a second request arriving while the scene work is still in flight.
    private string m_PendingExampleName = string.Empty;
    private bool m_Switching;

    // The example currently loaded on top of the UI scene, and its menu name.
    // Both are empty until the first example loads.
    private Scene m_LoadedExampleScene;
    private string m_LoadedExampleName = string.Empty;

    // The global settings the loaded example asked for, and the values they replaced.
    // Only the settings the example actually asked for are saved, so anything it left alone is never written back over a change the player made in the menu.
    private ToolboxExampleState m_AppliedExampleState;
    private bool m_SavedWorldSleeping;
    private float m_SavedMaximumDeltaTime;

    /// <summary>
    /// The generated list of every example available in the menu.
    /// </summary>
    public ToolboxManifest Manifest => m_Manifest;

    private void Start()
    {
#if UNITY_EDITOR
        if (!SystemInfo.supportsComputeShaders)
            EditorUtility.DisplayDialog("Compute Shader Support Missing", "2D Physics requires compute shader support for its debug renderer. Without this, you will not see physics debug rendering although physics itself will be unaffected.", "OK");
#endif
        m_MainMenuDocument = GetComponent<UIDocument>();

        // Disable this because it's not needed and causing Input system problems.
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;

        // Show the bottom-left menu by default (it hosts the Toolbox + Shortcuts sections).
        BottomLeftMenu.gameObject.SetActive(true);

        // The Debug view is always present now (folding hides it); there's no longer a toggle for it.
        DebugView.gameObject.SetActive(true);

        // Reset the per-scene controls bar (now holds only scene-custom buttons).
        ControlsMenu.ResetControls();

        // Cache the migrated global-control buttons (hosted by the BottomLeftMenu).
        m_PausePlayButton = BottomLeftMenu.PausePlayButton;
        m_SingleStepButton = BottomLeftMenu.SingleStepButton;
        m_InteractionButton = BottomLeftMenu.InteractionButton;
        m_FoldAllButton = BottomLeftMenu.FoldAllButton;

        // Interaction: single toggle button; text shows the mode a click will switch to (see
        // UpdateInputModeVisual).
        m_InteractionButton.clicked += ToggleInputMode;

        // Pause/Play: single toggle button; text shows the action it will perform. Single-Step is
        // on its own line and enabled only when paused.
        m_PausePlayButton.clicked += TogglePausePlay;
        m_SingleStepButton.clicked += SingleStep;
        m_SingleStepButton.text = $"Single-Step [{ToolboxUtility.HighlightColor}S{ToolboxUtility.EndHighlightColor}]";

        // Reset (also resets the camera — see ResetScene).
        BottomLeftMenu.ResetButton.clicked += ResetScene;
        BottomLeftMenu.ResetButton.text = $"Reset [{ToolboxUtility.HighlightColor}R{ToolboxUtility.EndHighlightColor}]";

        // Restart (resets all settings and reloads the scene).
        BottomLeftMenu.RestartButton.clicked += Restart;
        BottomLeftMenu.RestartButton.text = $"Restart [{ToolboxUtility.HighlightColor}X{ToolboxUtility.EndHighlightColor}]";

        // Fold All / Unfold All.
        m_FoldAllButton.clicked += ToggleFoldAll;
        UpdateFoldAllVisual();

        // Quit.
        BottomLeftMenu.QuitButton.clicked += QuitApplication;
        BottomLeftMenu.QuitButton.text = $"Quit [{ToolboxUtility.HighlightColor}Esc{ToolboxUtility.EndHighlightColor}]";

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
            ? $"Unfold All [{ToolboxUtility.HighlightColor}Tab{ToolboxUtility.EndHighlightColor}]"
            : $"Fold All [{ToolboxUtility.HighlightColor}Tab{ToolboxUtility.EndHighlightColor}]";
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
        m_ExamplesHeader.text = $"{ToolboxUtility.HighlightColor}{caret}{ToolboxUtility.EndHighlightColor}<size=50%> </size>Examples";
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

    // The interaction mode belongs to the UI rather than to the camera, because each example brings its own camera and the mode has to survive a switch.
    // Whichever camera is loaded is told the mode when it connects.
    private void SetInputMode(CameraManipulator.InputMode mode)
    {
        m_InputMode = mode;

        if (m_CameraManipulator != null)
            m_CameraManipulator.TouchMode = mode;

        UpdateInputModeVisual();
    }

    // Toggles the interaction mode between Drag and Explode.
    private void ToggleInputMode()
    {
        var isDrag = m_InputMode == CameraManipulator.InputMode.Drag;
        SetInputMode(isDrag ? CameraManipulator.InputMode.Explode : CameraManipulator.InputMode.Drag);
    }

    // The Interaction button is a single toggle: its text shows the mode a click will switch to.
    private void UpdateInputModeVisual()
    {
        var isDrag = m_InputMode == CameraManipulator.InputMode.Drag;
        m_InteractionButton.text = isDrag
            ? $"Explode [{ToolboxUtility.HighlightColor}I{ToolboxUtility.EndHighlightColor}]"
            : $"Drag [{ToolboxUtility.HighlightColor}I{ToolboxUtility.EndHighlightColor}]";
    }

    public void ResetSceneState()
    {
#if false
        // Disable any "TestBody".
        foreach (var testBody in FindObjectsByType<TestBody>(FindObjectsInactive.Include))
            testBody.enabled = false;

        // Disable any "TestWorld".
        foreach (var testWorld in FindObjectsByType<TestWorld>(FindObjectsInactive.Include))
            testWorld.enabled = false;
#endif
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

#if false
        // Enable any "TestWorld".
        foreach (var testWorld in FindObjectsByType<TestWorld>(FindObjectsInactive.Include))
            testWorld.enabled = true;

        // Enable all "TestBody" again.
        foreach (var testBody in FindObjectsByType<TestBody>(FindObjectsInactive.Include))
            testBody.enabled = true;
#endif
    }

    private void SetupOptions()
    {
        // The Toolbox (world/draw) controls now live in the merged BottomLeftMenu panel; query them
        // from its UIDocument root (it's a serialized reference, so no lookup is needed).
        var root = BottomLeftMenu.GetComponent<UIDocument>().rootVisualElement;

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
            // The camera belongs to the loaded example, so there is nothing to apply to between examples.
            m_ExplodeImpulseElement = root.Q<Slider>("explode-impulse");
            m_ExplodeImpulseElement.RegisterValueChangedCallback(evt =>
            {
                if (m_CameraManipulator != null)
                    m_CameraManipulator.ExplodeImpulse = evt.newValue;
            });
            m_ExplodeImpulseElement.value = m_MenuDefaults.ExplodeImpulse;

            // Camera Zoom.
            m_CameraZoomElement = root.Q<Slider>("camera-zoom");
            m_CameraZoomElement.value = m_MenuDefaults.CameraZoom;
            m_CameraZoomElement.RegisterValueChangedCallback(evt =>
            {
                if (m_CameraManipulator != null)
                    m_CameraManipulator.CameraZoom = evt.newValue;
            });

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
            // the Shortcuts "Interaction" button (the only UI for it now).
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
        m_SceneCategories.choices.AddRange(m_Manifest.GetCategories());

        // Register a category change.
        m_SceneCategories.RegisterValueChangedCallback(evt => SceneCategoryChanged(evt.newValue));

        // Resolve the configured start example.
        // An empty or unregistered name falls back to the first example in the manifest, so startup never throws when an example is renamed or removed.
        if (m_Manifest.examples.Count == 0)
        {
            Debug.LogWarning("[Toolbox] No examples are registered in the manifest; run Tools > 2D > Physics > Rebuild Toolbox Registry.");
        }
        else
        {
            if (!m_Manifest.TryGetExample(StartScene, out var startItem))
            {
                startItem = m_Manifest.examples[0];

                if (string.IsNullOrEmpty(StartScene))
                    Debug.LogWarning($"[Toolbox] No start example set; using the first registered example '{startItem.exampleName}'.");
                else
                    Debug.LogWarning($"[Toolbox] Start example '{StartScene}' is not registered; using the first registered example '{startItem.exampleName}'.");
            }

            // Select the resolved example, suppressing the category-change auto-selection so we land on exactly this one.
            m_IgnoreAutoSceneSelection = true;
            m_SceneCategories.value = startItem.category;
            m_IgnoreAutoSceneSelection = false;
            m_Scenes.value = startItem.exampleName;
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
        m_Scenes.choices.AddRange(m_Manifest.GetExampleNames(categoryName));

        // Register a scene change.
        m_SceneChangedCallback = evt => SceneChanged(evt.newValue);
        m_Scenes.RegisterValueChangedCallback(m_SceneChangedCallback);

        // Select the first scene (if not ignored).
        if (!m_IgnoreAutoSceneSelection)
            m_Scenes.index = 0;
    }

    private void SceneChanged(string sceneName)
    {
        // Ignore if invalid scene, the same as the currently loaded one, or the one already on its way in.
        if (string.IsNullOrEmpty(sceneName) ||
            sceneName == m_LoadedExampleName ||
            sceneName == m_PendingExampleName)
            return;

        // Unpause the world if paused.
        if (WorldPaused)
            TogglePausePlay();

        DebugView.ResetStats();

        // Collapse the bottom-left panel so it can't overlap the new scene's options.
        BottomLeftMenu.Collapse();

        LoadExample(sceneName);
    }

    private void ResetScene()
    {
        if (m_DisableUIRestarts)
            return;

        // Reloading the example scene restores it from what is saved on disk, which is the whole reset for a scene of components.
        if (!string.IsNullOrEmpty(m_LoadedExampleName))
            LoadExample(m_LoadedExampleName, reloading: true);
    }

    // Loads the specified example on top of the UI scene, replacing whatever example is loaded now.
    // The switch runs as a coroutine, because the unload, the load and the example filling itself each finish a frame or more after the one before.
    private void LoadExample(string exampleName, bool reloading = false)
    {
        if (!m_Manifest.TryGetExample(exampleName, out var item))
        {
            Debug.LogWarning($"[Toolbox] Example '{exampleName}' is not registered; run Tools > 2D > Physics > Rebuild Toolbox Registry.");
            return;
        }

        // A second request while a switch is still running would load two examples at once.
        if (m_Switching)
            return;

        m_Switching = true;
        m_PendingExampleName = item.exampleName;

        if (LoadingOverlay != null)
            LoadingOverlay.Show($"{(reloading ? "Reloading" : "Loading")} \"{item.exampleName}\" example ...");

        StartCoroutine(SwitchExample(item));
    }

    // Takes the loaded example down and brings the next one up, a step at a time.
    // Both the unload and the load finish a frame or more later, and an example builds its contents in Start, so each step waits for the one before it rather than assuming it has already happened.
    private IEnumerator SwitchExample(ToolboxManifest.ExampleItem item)
    {
        // Everything after this blocks the main thread in places, so the overlay is given a frame to lay out and draw before any of it starts.
        yield return null;
        yield return new WaitForEndOfFrame();

        var unloadOperation = UnloadCurrentExample();

        // The outgoing example's camera has just been switched off and the incoming one does not exist yet, so something has to hold the screen or Unity reports that nothing is rendering.
        // It is only up for the switch, so it never shares the screen with an example's own camera.
        if (FallbackCamera != null)
            FallbackCamera.SetActive(true);

        if (unloadOperation != null)
            yield return unloadOperation;

        // Reset the now empty default world, so every example starts from the same state and stays deterministic.
        // This runs before the load, so the arriving components create their physics objects into a clean world.
        ResetSceneState();

        // Apply what the example asked for before its components exist, so the first frame it runs already has the settings it expects.
        ApplyExampleState(item.state);

        yield return SceneManager.LoadSceneAsync(item.scenePath, LoadSceneMode.Additive);

        m_LoadedExampleScene = SceneManager.GetSceneByPath(item.scenePath);
        m_LoadedExampleName = item.exampleName;
        m_PendingExampleName = string.Empty;

        // Render settings and baked lighting come from the active scene, so the example supplies them rather than the UI.
        if (m_LoadedExampleScene.IsValid() && m_LoadedExampleScene.isLoaded)
            SceneManager.SetActiveScene(m_LoadedExampleScene);

        ConnectExample(item);

        // The example brings its own camera, so the stand-in is switched off again before it can render alongside it.
        if (FallbackCamera != null)
            FallbackCamera.SetActive(false);

        // An example fills itself in Start, which has not run yet, so the overlay stays up for the frame that does the building and comes down once something has actually been drawn with it in place.
        yield return null;
        yield return new WaitForEndOfFrame();

        m_Switching = false;

        if (LoadingOverlay != null)
            LoadingOverlay.Hide();
    }

    // Switches off and unloads the example currently loaded on top of the UI scene.
    // Deactivating the roots fires OnDisable immediately, so the camera stops rendering and the example's components destroy their physics objects before any further frame is drawn, whatever the unload itself does afterwards.
    // Returns the unload operation to wait on, or null when there was nothing loaded.
    private AsyncOperation UnloadCurrentExample()
    {
        ClearSceneOptions();
        ControlsMenu.ResetControls();
        RestoreExampleState();

        m_CameraManipulator = null;

        if (!m_LoadedExampleScene.IsValid() || !m_LoadedExampleScene.isLoaded)
        {
            m_LoadedExampleName = string.Empty;
            return null;
        }

        foreach (var rootObject in m_LoadedExampleScene.GetRootGameObjects())
            rootObject.SetActive(false);

        // Nothing visible or running is left in the scene by this point, so the unload is only reclaiming memory.
        var unloadOperation = SceneManager.UnloadSceneAsync(m_LoadedExampleScene);

        m_LoadedExampleScene = default;
        m_LoadedExampleName = string.Empty;

        return unloadOperation;
    }

    // Applies the global settings the specified example declared, remembering enough about each one to put it back.
    // These settings belong to the Toolbox rather than to the scene, so nothing in an example can restore them once that example has been unloaded.
    private void ApplyExampleState(ToolboxExampleState state)
    {
        m_AppliedExampleState = state;

        if (state.overridesDrawOptions)
            SetOverrideDrawOptions(state.overriddenDrawOptions, state.fixedDrawOptions);

        if (state.sleeping != ToolboxExampleState.Override.Default)
        {
            m_SavedWorldSleeping = WorldSleeping;
            WorldSleeping = state.sleeping == ToolboxExampleState.Override.On;
        }

        if (state.frameRateVisible != ToolboxExampleState.Override.Default)
        {
            if (state.frameRateVisible == ToolboxExampleState.Override.On)
                ShowFPS();
            else
                HideFPS();
        }

        // Holding a frame to one fixed step stops a slow frame running the simulation several times to catch up, which an example measuring the cost of a step would read as one very expensive step.
        if (state.catchUpSteps == ToolboxExampleState.Override.Off)
        {
            m_SavedMaximumDeltaTime = Time.maximumDeltaTime;
            Time.maximumDeltaTime = Time.fixedDeltaTime;
        }
    }

    // Puts back every global setting the outgoing example changed, so the next example starts from the menu's own values.
    // A control the example supplied may have moved any of these while it ran, which is why each one is restored from what was saved rather than from what the example asked for.
    private void RestoreExampleState()
    {
        var state = m_AppliedExampleState;
        m_AppliedExampleState = default;

        if (state.overridesDrawOptions)
            ResetOverrideDrawOptions();

        if (state.sleeping != ToolboxExampleState.Override.Default)
            WorldSleeping = m_SavedWorldSleeping;

        // The readout is visible unless an example hides it, so restoring it means showing it again.
        if (state.frameRateVisible != ToolboxExampleState.Override.Default)
            ShowFPS();

        if (state.catchUpSteps == ToolboxExampleState.Override.Off)
            Time.maximumDeltaTime = m_SavedMaximumDeltaTime;
    }

    // Connects a freshly loaded example to the UI: its camera, its description, and any controls it supplies.
    private void ConnectExample(ToolboxManifest.ExampleItem item)
    {
        // The camera belongs to the example scene, so it is resolved again on every load.
        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();

        if (m_CameraManipulator == null)
        {
            Debug.LogWarning($"[Toolbox] Example '{item.exampleName}' has no {nameof(CameraManipulator)}, so the camera controls do nothing.");
        }
        else
        {
            // Carry the UI's own settings onto the example's camera, so switching example does not quietly reset them.
            m_CameraManipulator.TouchMode = m_InputMode;
            m_CameraManipulator.ExplodeImpulse = m_ExplodeImpulseElement.value;
            m_CameraZoomElement.value = m_CameraManipulator.CameraZoom;
        }

        SetSceneDescription(item.description);

        // Most examples supply no controls at all, because their components are already tunable in the Inspector.
        var optionsProvider = FindAnyObjectByType<ToolboxOptionsProvider>();

        if (optionsProvider != null)
            optionsProvider.BuildOptions(this, SceneOptionsContent, ControlsMenu);

        RefreshSceneOptionsSection();
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

        if (m_CameraManipulator != null)
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


        // Input mode has no Options control; reset it directly and sync the Shortcuts button.
        SetInputMode(CameraManipulator.InputMode.Drag);

        DebugView.ResetStats();

        // Reload the example so it comes back exactly as it is saved on disk.
        if (!string.IsNullOrEmpty(m_LoadedExampleName))
            LoadExample(m_LoadedExampleName, reloading: true);

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
            ? $"Play [{ToolboxUtility.HighlightColor}P{ToolboxUtility.EndHighlightColor}]"
            : $"Pause [{ToolboxUtility.HighlightColor}P{ToolboxUtility.EndHighlightColor}]";

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

    public void ShowFPS() => DebugView.ShowFPS();

    public void HideFPS() => DebugView.HideFPS();
}
