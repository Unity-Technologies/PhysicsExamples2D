using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class Capacity : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;
    
    private FloatField m_DisplayBodyCount;
    private FloatField m_DisplayShapeCount;
    private FloatField m_DisplayContactCount;
    private ProgressBar m_TestIndicator;
    private VisualElement m_TestIndicatorProgress;
    
    private PolygonGeometry m_PolygonBox;
    private float m_OldMaximumDeltaTime;
    private bool m_OldWorldSleeping;
    
    private const int SimulationSpawnPeriod = 30;
    private int m_SimulationCounter;
    private int m_LimitReachedCount;
    private bool m_Finished;

    private int m_SimulationLimit;
    private bool m_RenderingOn;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;
        
        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 280f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 50f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_PolygonBox = PolygonGeometry.CreateBox(Vector2.one);

        // Attempt to stop multiple fixed-updates.
        // We do not care about keeping game-time here.
        m_OldMaximumDeltaTime = Time.maximumDeltaTime;
        Time.maximumDeltaTime = Time.fixedDeltaTime;

        // Turn-off world-sleeping here.
        m_OldWorldSleeping = m_SandboxManager.WorldSleeping;
        m_SandboxManager.WorldSleeping = false;
        
        m_SimulationCounter = 0;
        m_LimitReachedCount = 0;
        m_Finished = false;
        
        m_SimulationLimit = 20;
        m_RenderingOn = false;        
        
        // Set Overrides (if not rendering).
        if (!m_RenderingOn)
            m_SandboxManager.SetOverrideDrawOptions(overridenOptions: ~PhysicsWorld.DrawOptions.Off, fixedOptions: PhysicsWorld.DrawOptions.Off);

        SetupOptions();

        SetupScene();
        
        PhysicsEvents.PostSimulate += OnPostSimulation;
    }

    private void OnDisable()
    {
        // Restore global state.
        Time.maximumDeltaTime = m_OldMaximumDeltaTime;
        m_SandboxManager.WorldSleeping = m_OldWorldSleeping;
        
        PhysicsEvents.PostSimulate -= OnPostSimulation;

        // Reset overrides.
        if (!m_RenderingOn)
            m_SandboxManager.ResetOverrideDrawOptions();
    }
    
    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Simulation Limit.
            var simulationLimit = root.Q<SliderInt>("simulation-limit");
            simulationLimit.value = m_SimulationLimit;
            simulationLimit.RegisterValueChangedCallback(evt =>
            {
                m_SimulationLimit = evt.newValue;
                SetupScene();
            });
            
            // Rendering On.
            var renderingOn = root.Q<Toggle>("rendering-on");
            renderingOn.value = m_RenderingOn;
            renderingOn.RegisterValueChangedCallback(evt =>
            {
                m_RenderingOn = evt.newValue;

                if (m_RenderingOn)
                {
                    // Reset overrides.
                    m_SandboxManager.ResetOverrideDrawOptions();
                }
                else
                {
                    // Set Overrides.
                    m_SandboxManager.SetOverrideDrawOptions(overridenOptions: ~PhysicsWorld.DrawOptions.Off, fixedOptions: PhysicsWorld.DrawOptions.Off);
                }
            });
            
            // Count Displays.
            m_DisplayBodyCount = root.Q<FloatField>("body-count");
            m_DisplayShapeCount = root.Q<FloatField>("shape-count");
            m_DisplayContactCount = root.Q<FloatField>("contact-count");
            
            // Test indicator.
            m_TestIndicator = root.Q<ProgressBar>("test-indicator");
            m_TestIndicatorProgress = m_TestIndicator.Q(className: "unity-progress-bar__progress");

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        m_SimulationCounter = 0;
        m_LimitReachedCount = 0;
        m_Finished = false;
        m_TestIndicator.highValue = m_SimulationLimit;
        m_TestIndicator.title = $"Waiting for {m_TestIndicator.highValue:F0} ms ...";
        m_TestIndicatorProgress.style.backgroundColor = Color.clear;
        
        // Ground.
        {
            var world = PhysicsWorld.defaultWorld;
            var body = world.CreateBody(new PhysicsBodyDefinition { position = Vector2.down * 5f });
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(1600f, 10f)));
        }
    }

    private void OnPostSimulation(PhysicsWorld world, float timeStep)
    {
        // If we've finished then nothing more to do.
        if (m_Finished || m_SandboxManager.WorldPaused)
            return;
        
        // Finish if not the sandbox world.
        if (!world.isDefaultWorld)
            return;
        
        // Fetch the simulation step time,
        var simulationStep = world.profile.simulationStep;
        
        // Update the test indicator.
        m_TestIndicator.value = simulationStep;
        var progress = simulationStep / m_SimulationLimit;
        m_TestIndicatorProgress.style.backgroundColor = progress < 0.33f ? Color.forestGreen : progress > 0.66f ? Color.orange : Color.yellow;
        
        // Finish if we're above the simulation limit.
        if (simulationStep > m_SimulationLimit)
        {
            // If we've reached the limit over a period of time then flag as finished.
            if (++m_LimitReachedCount > 30)
            {
                // Update indicator.
                m_TestIndicator.title = $"Simulation limit of {m_TestIndicator.highValue:F0} ms reached.";
                m_TestIndicatorProgress.style.backgroundColor = Color.red;
                
                // Flag as finished.
                m_Finished = true;
            }
        }
        else
        {
            // Reduce the limit reached.
            m_LimitReachedCount = Mathf.Max(0, m_LimitReachedCount - 1);
        }

        // Finish if we're not ready to spawn.
        if (++m_SimulationCounter % SimulationSpawnPeriod != 0)
            return;

        // Spawn.
        {
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState } };

            const int count = 200;
            var position = new Vector2(-count, 200f);
            for (var n = 0; n < count; ++n)
            {
                position.y += 0.5f;
                bodyDef.position = position;
                
                world.CreateBody(bodyDef).CreateShape(m_PolygonBox, shapeDef);
                
                position.x += 2f;
            }
        }
        
        // Update Display Counts.
        var worldCounter = world.counters;
        m_DisplayBodyCount.value = worldCounter.bodyCount;
        m_DisplayShapeCount.value = worldCounter.shapeCount;
        m_DisplayContactCount.value = worldCounter.contactCount;
    }
}