using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

/// <summary>
/// Benchmark a large quantity of bodies, shapes and contacts, spawning until the simulation limit has been reached.
/// This provides an approximate limit for a worse-case scenario with bodies and shapes all in contact with each other on the current device.
/// The available threads has a huge impact on this limit. The debug rendering defaults to being turned off as the FPS is irrelevant and may result in the Sandbox UI becoming sluggish on some devices. 
/// </summary>
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
    
    private PolygonGeometry m_PolygonGeometry;
    private CircleGeometry m_CircleGeometry;
    private CapsuleGeometry m_CapsuleGeometry;
    private float m_SpawnOffset;
    
    private float m_OldMaximumDeltaTime;
    private bool m_OldWorldSleeping;

    private Color[] m_ProgressColors;
    
    private const int SimulationSpawnPeriod = 30;
    private int m_SimulationCounter;
    private int m_LimitReachedCount;
    private bool m_Finished;

    private ShapeType m_ShapeType;
    private int m_SimulationLimit;
    private bool m_RenderingOn;

    private enum ShapeType
    {
        Circle,
        Polygon,
        Capsule
    }
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;
        
        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 280f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 200f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_PolygonGeometry = PolygonGeometry.CreateBox(Vector2.one);
        m_CircleGeometry = new CircleGeometry { radius = 0.5f };
        m_CapsuleGeometry = new CapsuleGeometry { center1 = Vector2.left * 0.5f, center2 = Vector2.right * 0.5f, radius = 0.5f };
        m_SpawnOffset = 0f;

        m_ProgressColors = new []
        {
            Color.softGreen,
            Color.yellowNice,
            Color.orange,
            Color.indianRed,
            Color.softRed
        };
        
        // Attempt to stop multiple fixed-updates.
        // We do not care about keeping game-time here.
        m_OldMaximumDeltaTime = Time.maximumDeltaTime;
        Time.maximumDeltaTime = Time.fixedDeltaTime;

        // Don't allow sleeping during testing.
        m_OldWorldSleeping = m_SandboxManager.WorldSleeping;
        m_SandboxManager.WorldSleeping = false;
        
        m_SimulationCounter = 0;
        m_LimitReachedCount = 0;
        m_Finished = false;

        m_ShapeType = ShapeType.Circle;
        m_SimulationLimit = 20;
        m_RenderingOn = false;        
        
        // Set Overrides.
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

            // Shape Type.
            var shapeType = root.Q<EnumField>("shape-type");
            shapeType.value = m_ShapeType;
            shapeType.RegisterValueChangedCallback(evt =>
            {
                m_ShapeType = (ShapeType)evt.newValue;
                SetupScene();
            });
            
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
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(550f, 10f)));
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, 800f), 0f, Vector2.left * 270f + Vector2.up * 395f));
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, 800f), 0f, Vector2.right * 270f + Vector2.up * 395f));
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
        
        // Finish if we're above the simulation limit.
        if (simulationStep > m_SimulationLimit)
        {
            // If we've reached the limit over a period of time then flag as finished.
            if (++m_LimitReachedCount > 60)
            {
                // Update indicator.
                m_TestIndicator.title = $"Simulation limit of {m_TestIndicator.highValue:F0} ms reached.";
                m_TestIndicatorProgress.style.backgroundColor = m_ProgressColors[4];

                // Flag as finished.
                m_Finished = true;

                return;
            }
        }
        else
        {
            // Reduce the limit reached.
            m_LimitReachedCount = Mathf.Max(0, m_LimitReachedCount - 1);
        }

        // Update the test indicator.
        m_TestIndicator.value = simulationStep;
        var progress = simulationStep / m_SimulationLimit;
        m_TestIndicatorProgress.style.backgroundColor = progress switch
        {
            < 0.33f => m_ProgressColors[0],
            < 0.50f => m_ProgressColors[1],
            < 0.80f => m_ProgressColors[2],
            _ => m_ProgressColors[3]
        };
        
        // Finish if we're not ready to spawn.
        if (++m_SimulationCounter % SimulationSpawnPeriod != 0)
            return;

        // Spawn.
        {
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, bodyConstraints = RigidbodyConstraints2D.FreezeRotation };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState } };

            const int count = 200;
            var position = new Vector2(-count + m_SpawnOffset, 420f);
            for (var n = 0; n < count; ++n)
            {
                position.y += 0.25f;
                bodyDef.position = position;
                position.x += 2f;

                // Create the appropriate geometry.
                switch (m_ShapeType)
                {
                    case ShapeType.Circle:
                        world.CreateBody(bodyDef).CreateShape(m_CircleGeometry, shapeDef);
                        continue;
                    
                    case ShapeType.Polygon:
                        world.CreateBody(bodyDef).CreateShape(m_PolygonGeometry, shapeDef);
                        continue;
                    
                    case ShapeType.Capsule:
                        world.CreateBody(bodyDef).CreateShape(m_CapsuleGeometry, shapeDef);
                        continue;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            m_SpawnOffset = 0.5f - m_SpawnOffset;
        }
        
        // Update Display Counts.
        var worldCounter = world.counters;
        m_DisplayBodyCount.value = worldCounter.bodyCount;
        m_DisplayShapeCount.value = worldCounter.shapeCount;
        m_DisplayContactCount.value = worldCounter.contactCount;
    }
}