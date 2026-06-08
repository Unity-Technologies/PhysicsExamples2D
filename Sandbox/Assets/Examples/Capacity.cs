using System;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

/// <summary>
/// Benchmark a large quantity of bodies, shapes and contacts, spawning until the simulation limit has been reached.
/// This provides an approximate limit for a worst-case scenario with bodies and shapes all in contact with each other on the current device.
/// The available threads has a huge impact on this limit. The debug rendering defaults to being turned off as the FPS is irrelevant and may result in the Sandbox UI becoming sluggish on some devices.
/// </summary>
[ExampleScene("Benchmarks", "Spawns bodies until the per-step simulation time reaches a configurable limit.")]
public sealed class Capacity : SandboxExampleBehaviour
{
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

    private const int LimitReachedCount = 60;
    private const int SimulationSpawnPeriod = 0x1F;
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

    protected override float CameraSize => 300f;
    protected override Vector2 CameraPosition => new(10f, 234f);

    protected override void OnExampleEnable()
    {
        // Showing FPS is not important here.
        SandboxManager.HideFPS();

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
        m_OldWorldSleeping = SandboxManager.WorldSleeping;
        SandboxManager.WorldSleeping = false;

        m_SimulationCounter = 0;
        m_LimitReachedCount = 0;
        m_Finished = false;

        m_ShapeType = ShapeType.Circle;
        m_SimulationLimit = 20;
        m_RenderingOn = true;

        // Set Overrides.
        if (!m_RenderingOn)
            SandboxManager.SetOverrideDrawOptions(overridenOptions: ~PhysicsWorld.DrawOptions.Off, fixedOptions: PhysicsWorld.DrawOptions.Off);

        PhysicsEvents.PostSimulate += OnPostSimulation;
    }

    protected override void OnExampleDisable()
    {
        // Restore global state.
        Time.maximumDeltaTime = m_OldMaximumDeltaTime;
        SandboxManager.WorldSleeping = m_OldWorldSleeping;

        PhysicsEvents.PostSimulate -= OnPostSimulation;

        // Restore the FPS again.
        SandboxManager.ShowFPS();
    }

    protected override void SetupOptions()
    {
        // Simulation Limit.
        AddSliderInt("Simulation Limit (ms)", m_SimulationLimit, 1, 50, v => m_SimulationLimit = v, rebuild: true);

        // Shape Type.
        AddEnum("Shape Type", m_ShapeType, v => m_ShapeType = v, rebuild: true);

        // Rendering On.
        AddToggle("Rendering On", m_RenderingOn, v =>
        {
            m_RenderingOn = v;

            if (m_RenderingOn)
            {
                // Reset overrides.
                SandboxManager.ResetOverrideDrawOptions();
            }
            else
            {
                // Set Overrides.
                SandboxManager.SetOverrideDrawOptions(overridenOptions: ~PhysicsWorld.DrawOptions.Off, fixedOptions: PhysicsWorld.DrawOptions.Off);
            }
        });

        // Count Displays (read-only).
        m_DisplayBodyCount = AddElement(new FloatField("Body Count") { isReadOnly = true, focusable = false });
        m_DisplayShapeCount = AddElement(new FloatField("Shape Count") { isReadOnly = true, focusable = false });
        m_DisplayContactCount = AddElement(new FloatField("Contact Count") { isReadOnly = true, focusable = false });

        // Test indicator.
        m_TestIndicator = AddElement(new ProgressBar { title = "", lowValue = 0f, highValue = 1f, value = 0f });
        m_TestIndicatorProgress = m_TestIndicator.Q(className: "unity-progress-bar__progress");
    }

    protected override void SetupScene()
    {
        m_SimulationCounter = 0;
        m_LimitReachedCount = 0;
        m_Finished = false;
        m_TestIndicator.highValue = m_SimulationLimit;
        m_TestIndicator.title = $"Waiting for {m_TestIndicator.highValue:F0} ms ...";
        m_TestIndicatorProgress.style.backgroundColor = Color.clear;

        // Ground.
        {
            // Get the default world.
            var world = World;

            var groundBody = world.CreateBody(new PhysicsBodyDefinition { position = Vector2.down * 5f });
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(1600f, 10f)));
        }
    }

    private void OnPostSimulation(PhysicsWorld world, float timeStep)
    {
        // If we've finished then nothing more to do.
        if (m_Finished || SandboxManager.WorldPaused)
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
            if (++m_LimitReachedCount > LimitReachedCount)
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
            // Reset the limit reached count.
            m_LimitReachedCount = 0;
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
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = ShapeColor } };

            const int count = 200;
            var position = new Vector2(-count + m_SpawnOffset, 200f);
            for (var n = 0; n < count; ++n)
            {
                position.y += 0.5f;
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
