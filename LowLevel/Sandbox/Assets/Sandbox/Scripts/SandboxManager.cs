using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

public class SandboxManager : MonoBehaviour, IShapeColorProvider
{
    public ref Random Random => ref m_Random;
    public bool WorldPaused { get; private set; }
    public bool WorldSleeping { get => m_SleepingElement.value; set => m_SleepingElement.value = value; }

    public float CameraZoom
    {
        get => m_CameraZoomElement.value;
        set => m_CameraZoomElement.value = value;
    }
    
    public UIDocument SceneOptionsUI { get; set; }
    
    private bool ColorShapeState { get; set; }
    private ControlsMenu.CustomButton m_PausePlayButton;    
    private ControlsMenu.CustomButton m_SingleStepButton;    
    private ControlsMenu.CustomButton m_DebugButton;    
    private ControlsMenu.CustomButton m_UIButton;
    private ControlsMenu.CustomButton m_ResetButton;
    private ControlsMenu.CustomButton m_QuitButton;

    public string StartScene = string.Empty;
    public Action SceneResetAction;
    public DebugView DebugView;
    public ShortcutsView ShortcutsView;
    public ControlsMenu ControlsMenu;
    public float UpdatePeriodFPS = 0.1f;

    // Override state.
    private PhysicsWorld.DrawOptions m_OverrideDrawOptions;
    private PhysicsWorld.DrawOptions m_OverridePreviousDrawOptions;
    private bool m_OverrideColorShapeState;
    private bool m_OverridePreviousColorShapeState;

    private struct MenuDefaults
    {
        // PhysicsWorld.
        public int Workers;
        public int SubSteps;
        public string Frequency;
        public bool WarmStarting;
        public bool Sleeping;
        public bool Continuous;

        // Draw.
        public bool ShowDebugView;
        public string InputDrag;
        public float ExplodeImpulse;
        public float CameraZoom;
        public bool ColorShapeState;
        public float DrawThickness;
        public float DrawPointScale;
        public float DrawNormalScale;
        public float DrawImpulseScale;
        public PhysicsWorld.DrawOptions DrawOptions;
    }

    private CameraManipulator m_CameraManipulator;
    private SceneManifest m_SceneManifest;
    private UIDocument m_MainMenuDocument;
    private TreeView m_ScenesView;
    private VisualElement m_CountersElement;
    private ProgressBar m_BarFPS;
    private float m_MaxFPS;
    private float m_UpdateTimeFPS;
    private MenuDefaults m_MenuDefaults;
    private bool m_DisableUIRestarts;
    private bool m_ShowUI;
    private Dictionary<PhysicsWorld.DrawOptions, Toggle> m_DrawFlagElements;

    // PhysicsWorld Elements.
    private SliderInt m_WorkersElement;
    private SliderInt m_SubStepsElement;
    private DropdownField m_FrequencyElement;
    private Toggle m_WarmStartingElement;
    private Toggle m_SleepingElement;
    private Toggle m_ContinuousElement;
    private Button m_SingleStepElement;
    private Button m_PausePlayElement;

    // Draw Elements.
    private Toggle m_ShowDebugElement;
    private DropdownField m_InputModeElement;
    private Slider m_ExplodeImpulseElement;
    private Slider m_CameraZoomElement;
    private Slider m_DrawThicknessElement;
    private Slider m_DrawPointScaleElement;
    private Slider m_DrawNormalScaleElement;
    private Slider m_DrawImpulseScaleElement;
    private Toggle m_ColorShapeStateElement;
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

    public Color32 ShapeColorState => ColorShapeState ? new Color32() : new Color32((byte)m_Random.NextUInt(0, 256), (byte)m_Random.NextUInt(0, 256), (byte)m_Random.NextUInt(0, 256), 255);
    bool IShapeColorProvider.IsShapeColorActive => ColorShapeState;

    private void Start()
    {
        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_SceneManifest = GetComponent<SceneManifest>();
        m_MainMenuDocument = GetComponent<UIDocument>();

        ColorShapeState = true;
        m_ShowUI = true;

        // Show the Shortcut view by default.
        ShortcutsView.gameObject.SetActive(true);
        
        // Reset the controls.
        ControlsMenu.ResetControls();
        m_PausePlayButton = ControlsMenu.pausePlayButton;
        m_SingleStepButton = ControlsMenu.singleStepButton;
        m_DebugButton = ControlsMenu.debugButton;
        m_UIButton = ControlsMenu.uiButton;
        m_ResetButton = ControlsMenu.resetButton;
        m_QuitButton = ControlsMenu.quitButton;
        
        m_PausePlayButton.button.clickable.clicked += TogglePausePlay;
        m_SingleStepButton.button.clickable.clicked += SingleStep;
        m_DebugButton.button.clickable.clicked += ToggleDebugView; 
        m_UIButton.button.clickable.clicked += ToggleUI;
        m_ResetButton.button.clickable.clicked += ResetScene;
        
        m_PausePlayButton.button.text = WorldPaused ? "Play" : "Pause";
        m_SingleStepButton.button.enabledSelf = WorldPaused;

            
        var defaultWorld = PhysicsWorld.defaultWorld;
        m_MenuDefaults = new MenuDefaults
        {
            // PhysicsWorld.
            Workers = defaultWorld.simulationWorkers,
            SubSteps = defaultWorld.simulationSubSteps,
            Frequency = "60",
            WarmStarting = defaultWorld.warmStartingAllowed,
            Sleeping = defaultWorld.sleepingAllowed,
            Continuous = defaultWorld.continuousAllowed,

            // Drawing.
            ShowDebugView = true,
            InputDrag = Enum.GetName(typeof(CameraManipulator.InputMode), CameraManipulator.InputMode.Drag),
            ExplodeImpulse = 30f,
            CameraZoom = 1f,
            ColorShapeState = ColorShapeState,
            DrawThickness = defaultWorld.drawThickness,
            DrawPointScale = defaultWorld.drawPointScale,
            DrawNormalScale = defaultWorld.drawNormalScale,
            DrawImpulseScale = defaultWorld.drawImpulseScale,
            DrawOptions = defaultWorld.drawOptions
        };

        SetupSceneTree();
        SetupOptions();

        m_UpdateTimeFPS = UpdatePeriodFPS;
    }

    private void OnEnable()
    {
        // We don't want this appearing all the time.
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;

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

            // Quit.
            if (m_QuitButton.isPressed || currentKeyboard.escapeKey.wasPressedThisFrame)
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }

            // Toggle UI.
            if (currentKeyboard.tabKey.wasPressedThisFrame)
            {
                ToggleUI();
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

            // Debug View.
            if (currentKeyboard.dKey.wasPressedThisFrame)
            {
                ToggleDebugView();
            }

            // Toggle Color PhysicsShape State.
            if (currentKeyboard.cKey.wasPressedThisFrame)
            {
                if (!m_OverrideColorShapeState)
                    m_ColorShapeStateElement.value = !m_ColorShapeStateElement.value;
            }

            // Touch State.
            {
                // Drag Mode.
                if (currentKeyboard.digit1Key.wasPressedThisFrame)
                    m_InputModeElement.value = Enum.GetName(typeof(CameraManipulator.InputMode), CameraManipulator.InputMode.Drag);
                else if (currentKeyboard.digit2Key.wasPressedThisFrame)
                    m_InputModeElement.value = Enum.GetName(typeof(CameraManipulator.InputMode), CameraManipulator.InputMode.Explode);
            }
        }

        // Update the FPS.
        UpdateFPS();
    }

    private void UpdateFPS()
    {
        // Fps.
        var deltaTime = Time.smoothDeltaTime;
        if (m_BarFPS != null & deltaTime > 0f)
        {
            m_UpdateTimeFPS -= deltaTime;
            if (m_UpdateTimeFPS >= 0f)
                return;

            m_UpdateTimeFPS = UpdatePeriodFPS;

            const string color = "<color=#7FFFD4>";
            const string endColor = "</color>";

            var fps = 1f / deltaTime;
            m_BarFPS.highValue = m_MaxFPS = math.max(m_MaxFPS, fps);
            m_BarFPS.value = fps;
            m_BarFPS.title = $"{color}{fps:F1}{endColor} fps ({color}{1000f / fps:F1}{endColor} ms)";
        }
    }

    private void ToggleUI()
    {
        m_ShowUI = !m_ShowUI;

        // Debug View.
        if (m_ShowDebugElement.value)
            DebugView.gameObject.SetActive(m_ShowUI);

        // Toggle Shortcut View.
        ShortcutsView.gameObject.SetActive(!ShortcutsView.gameObject.activeInHierarchy);
                
        // Main Menu.
        m_MainMenuDocument.rootVisualElement.style.display = m_ShowUI ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ToggleDebugView()
    {
        m_ShowDebugElement.value = !m_ShowDebugElement.value;
    }
    
    public void ResetSceneState()
    {
        // Disable any "SceneBody".
        foreach (var sceneBody in FindObjectsByType<SceneBody>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            sceneBody.enabled = false;

        // Disable any "SceneWorlds".
        foreach (var sceneWorld in FindObjectsByType<SceneWorld>(FindObjectsSortMode.None))
            sceneWorld.enabled = false;

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
        m_Random.InitState(0x42424242);

        // Reset the default world.
        PhysicsWorld.defaultWorld.Reset();

        // Enable any "SceneWorlds".
        foreach (var sceneWorld in FindObjectsByType<SceneWorld>(FindObjectsSortMode.None))
            sceneWorld.enabled = true;

        // Enable all bodies again.
        foreach (var sceneBody in FindObjectsByType<SceneBody>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            sceneBody.enabled = true;
    }
    
    private void SetupOptions()
    {
        var root = m_MainMenuDocument.rootVisualElement;

        // Menu Region.
        {
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);
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
            m_FrequencyElement = root.Q<DropdownField>("frequency");
            m_FrequencyElement.value = m_MenuDefaults.Frequency;
            FrequencySelection(m_FrequencyElement.value);
            m_FrequencyElement.RegisterValueChangedCallback(evt => FrequencySelection(evt.newValue));

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

            // Restart.
            var restart = root.Q<Button>("restart");
            restart.clicked += Restart;

            // Single-step.
            m_SingleStepElement = root.Q<Button>("single-step");
            m_SingleStepElement.clicked += SingleStep;

            // Pause/Play.
            m_PausePlayElement = root.Q<Button>("pause-play");
            m_PausePlayElement.text = WorldPaused ? "Play" : "Pause";
            m_PausePlayElement.clicked += TogglePausePlay;

            // Quit.
            var quit = root.Q<Button>("quit-application");
            quit.clicked += Application.Quit;
        }

        // Options.
        {
            // Show Debug View.
            m_ShowDebugElement = root.Q<Toggle>("show-debug");
            m_ShowDebugElement.RegisterValueChangedCallback(evt => { DebugView.gameObject.SetActive(evt.newValue); });
            m_ShowDebugElement.value = m_MenuDefaults.ShowDebugView;

            // Input Mode.
            m_InputModeElement = root.Q<DropdownField>("input-mode");
            m_InputModeElement.RegisterValueChangedCallback(evt =>
            {
                Enum.TryParse(evt.newValue, true, out CameraManipulator.InputMode touchMode);
                m_CameraManipulator.TouchMode = touchMode;
            });
            m_InputModeElement.value = m_MenuDefaults.InputDrag;

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
                    world.drawImpulseScale = evt.newValue;
            });
            m_DrawImpulseScaleElement.value = m_MenuDefaults.DrawImpulseScale;

            // PhysicsShape State Color.
            m_ColorShapeStateElement = root.Q<Toggle>("color-shape-state");
            m_ColorShapeStateElement.value = m_MenuDefaults.ColorShapeState;
            m_ColorShapeStateElement.RegisterValueChangedCallback(evt =>
            {
                ColorShapeState = evt.newValue;

                ResetScene();
            });

            // Bodies.
            m_DrawBodiesElement = ConfigureDrawFlag(root, "draw-bodies", PhysicsWorld.DrawOptions.AllBodies);
            m_DrawShapesElement = ConfigureDrawFlag(root, "draw-shapes", PhysicsWorld.DrawOptions.AllShapes);
            m_DrawJointsElement = ConfigureDrawFlag(root, "draw-joints", PhysicsWorld.DrawOptions.AllJoints);
            m_DrawBoundsElement = ConfigureDrawFlag(root, "draw-shape-bounds", PhysicsWorld.DrawOptions.AllShapeBounds);
            m_DrawIslandsElement = ConfigureDrawFlag(root, "draw-solver-islands", PhysicsWorld.DrawOptions.AllSolverIslands);
            m_DrawContactPointsElement = ConfigureDrawFlag(root, "draw-contact-points", PhysicsWorld.DrawOptions.AllContactPoints);
            m_DrawContactNormalsElement = ConfigureDrawFlag(root, "draw-contact-normals", PhysicsWorld.DrawOptions.AllContactNormal);
            m_DrawContactTangentsElement = ConfigureDrawFlag(root, "draw-contact-tangents", PhysicsWorld.DrawOptions.AllContactFriction);
            m_DrawContactImpulsesElement = ConfigureDrawFlag(root, "draw-contact-impulses", PhysicsWorld.DrawOptions.AllContactImpulse);
        }

        // FPS.
        {
            m_BarFPS = root.Q<ProgressBar>("fps");
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

    private static void FrequencySelection(string frequency)
    {
        var fixedRate = frequency != "Variable";
        if (fixedRate)
            Time.fixedDeltaTime = 1.0f / float.Parse(frequency);

        // Update the worlds.
        using var worlds = PhysicsWorld.GetWorlds();
        foreach (var world in worlds)
            world.simulationMode = fixedRate ? SimulationMode2D.FixedUpdate : SimulationMode2D.Update;
    }

    private void SetupSceneTree()
    {
        var root = m_MainMenuDocument.rootVisualElement;

        // Fetch the tree.
        m_ScenesView = root.Q<TreeView>("scenes");

        // Populate categorised scenes.
        var listId = 1;
        foreach (var category in m_SceneManifest.GetCategories())
        {
            var sceneList = new List<TreeViewItemData<string>>();
            foreach (var sceneName in m_SceneManifest.GetScenes(category))
                sceneList.Add(new TreeViewItemData<string>(listId++, sceneName));

            m_ViewItems.Add(new TreeViewItemData<string>(-listId++, category, sceneList));
        }

        // Set-up tree.
        m_ScenesView.SetRootItems(m_ViewItems);
        m_ScenesView.makeItem = () => new Label();
        m_ScenesView.bindItem = (e, i) => { ((Label)e).text = m_ScenesView.GetItemDataForIndex<string>(i); };
        m_ScenesView.itemsChosen += _ => TreeSelection();
        m_ScenesView.selectionChanged += _ => TreeSelectionChanged();
        m_ScenesView.itemExpandedChanged += TreeExpandedChanged;
        m_ScenesView.Rebuild();

        // Load the start scene if specified.
        if (StartScene != string.Empty)
        {
            DebugView.ResetStats();
            m_SceneManifest.LoadScene(StartScene, ResetSceneState);
        }
    }

    private void TreeSelection()
    {
        foreach (var selectedId in m_ScenesView.selectedIds)
        {
            if (m_ScenesView.IsExpanded(selectedId))
                m_ScenesView.CollapseItem(selectedId);
            else
                m_ScenesView.ExpandItem(selectedId);
        }
    }

    private void TreeExpandedChanged(TreeViewExpansionChangedArgs arg)
    {
        // Ignore if not expanding.
        if (!arg.isExpanded)
            return;

        // Collapse all other expanded items.
        foreach (var rootId in m_ScenesView.GetRootIds())
        {
            if (arg.id != rootId && m_ScenesView.IsExpanded(rootId))
                m_ScenesView.CollapseItem(rootId);
        }
    }

    private void TreeSelectionChanged()
    {
        if (!m_ScenesView.selectedIds.Any(selectedId => selectedId > 0))
            return;

        var sceneName = (string)m_ScenesView.selectedItem;

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
        m_CameraManipulator.OverlapUI = 0;

        // Reset the controls.
        ControlsMenu.ResetControls();
        
        SceneOptionsUI = null;
        SceneResetAction = null;
        m_SceneManifest.LoadScene(sceneName, ResetSceneState);
    }

    private void ResetScene()
    {
        if (!m_DisableUIRestarts)
            SceneResetAction?.Invoke();
    }
    
    // Reset the settings and reload the current scene.
    private void Restart()
    {
        m_DisableUIRestarts = true;

        // Reset overrides.
        ResetOverrideDrawOptions();
        ResetOverrideColorShapeState();

        // Worlds.
        m_WorkersElement.value = m_MenuDefaults.Workers;
        m_SubStepsElement.value = m_MenuDefaults.SubSteps;
        m_FrequencyElement.value = m_MenuDefaults.Frequency;
        m_WarmStartingElement.value = m_MenuDefaults.WarmStarting;
        m_SleepingElement.value = m_MenuDefaults.Sleeping;
        m_ContinuousElement.value = m_MenuDefaults.Continuous;

        // Drawing.
        m_ShowDebugElement.value = m_MenuDefaults.ShowDebugView;
        m_InputModeElement.value = m_MenuDefaults.InputDrag;
        m_ExplodeImpulseElement.value = m_MenuDefaults.ExplodeImpulse;
        m_CameraManipulator.ResetPanZoom();
        m_CameraZoomElement.value = m_MenuDefaults.CameraZoom;
        m_DrawThicknessElement.value = m_MenuDefaults.DrawThickness;
        m_DrawPointScaleElement.value = m_MenuDefaults.DrawPointScale;
        m_ColorShapeStateElement.value = m_MenuDefaults.ColorShapeState;
        m_DrawBodiesElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllBodies);
        m_DrawShapesElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllShapes);
        m_DrawJointsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllJoints);
        m_DrawBoundsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllShapeBounds);
        m_DrawIslandsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllSolverIslands);
        m_DrawContactPointsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllContactPoints);
        m_DrawContactNormalsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllContactNormal);
        m_DrawContactTangentsElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllContactFriction);
        m_DrawContactImpulsesElement.value = m_MenuDefaults.DrawOptions.HasFlag(PhysicsWorld.DrawOptions.AllContactImpulse);

        DebugView.ResetStats();

        // Reload the scene.
        SceneResetAction = null;
        m_SceneManifest.ReloadCurrentScene(ResetSceneState);

        m_DisableUIRestarts = false;
    }

    private void TogglePausePlay()
    {
        WorldPaused = !WorldPaused;
        m_PausePlayElement.text = m_PausePlayButton.button.text = WorldPaused ? "Play" : "Pause";
        m_SingleStepElement.enabledSelf = WorldPaused;
        m_SingleStepButton.button.enabledSelf = WorldPaused;

        // Update the worlds.
        using var worlds = PhysicsWorld.GetWorlds();
        foreach (var world in worlds)
            world.paused = WorldPaused;
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
            var oldPaused = defaultWorld.paused;
            var oldSimulationMode = defaultWorld.simulationMode;

            {
                world.simulationMode = SimulationMode2D.Script;
                world.paused = false;
                world.Simulate(oldSimulationMode == SimulationMode2D.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime);
            }

            world.paused = oldPaused;
            world.simulationMode = oldSimulationMode;
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
        m_ColorShapeStateElement.enabledSelf = false;
        ColorShapeState = colorShapeState;
        m_OverrideColorShapeState = true;
    }

    public void ResetOverrideColorShapeState()
    {
        // Restore previous flag.
        m_ColorShapeStateElement.enabledSelf = true;
        ColorShapeState = m_OverridePreviousColorShapeState;
        m_OverrideColorShapeState = m_OverridePreviousColorShapeState = false;
    }    
}