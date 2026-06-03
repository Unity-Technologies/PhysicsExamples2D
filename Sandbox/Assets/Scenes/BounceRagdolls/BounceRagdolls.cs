using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Collision", "Checking the stability of very fast continuous collision.")]
public sealed class BounceRagdolls : SandboxExampleBehaviour
{
    private float m_Time;
    private float m_UpdateTime;
    private int m_RagdollCount;

    private float m_UpdatePeriod = 0.1f;
    private int m_MaxRagdollCount = 50;
    private float m_GravityScale = 5f;

    private Vector2 m_OldGravity;

    private const bool FastCollisions = true;

    protected override float CameraSize => 12f;
    protected override Vector2 CameraPosition => Vector2.zero;

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.Off);

        m_OldGravity = World.gravity;

        PhysicsEvents.PreSimulate += OnPreSimulation;
    }

    protected override void OnExampleDisable()
    {
        // Get the default world.
        var world = World;

        world.gravity = m_OldGravity;

        PhysicsEvents.PreSimulate -= OnPreSimulation;
    }

    protected override void SetupOptions()
    {
        // Update Period.
        AddSlider("Update Period", m_UpdatePeriod, 0.1f, 5f, v => { m_UpdatePeriod = v; m_UpdateTime = m_UpdatePeriod; }, rebuild: true);

        // Spawn Count.
        AddSliderInt("Ragdoll Count", m_MaxRagdollCount, 10, 250, v => m_MaxRagdollCount = v, rebuild: true);

        // Gravity Scale.
        AddSlider("Gravity Scale", m_GravityScale, 1f, 5f, v => { m_GravityScale = v; m_UpdateTime = m_UpdatePeriod; });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        m_RagdollCount = 0;
        m_UpdateTime = m_UpdatePeriod;
        m_Time = 0f;

        // Ground Body.
        var groundBody = world.CreateBody();

        var extent = 10f;
        using var extentPoints = new NativeList<Vector2>(Allocator.Temp)
        {
            new(-extent, extent),
            new(extent, extent),
            new(extent, -extent),
            new(-extent, -extent)
        };

        // Ground Box.
        {
            groundBody.CreateChain(
                geometry: new ChainGeometry(extentPoints.AsArray()),
                definition: new PhysicsChainDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 1.3f } }
            );
        }

        // Circles.
        {
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 2.0f } };
            groundBody.CreateShape(new CircleGeometry { radius = 3f }, shapeDef);

            foreach (var point in extentPoints)
                groundBody.CreateShape(new CircleGeometry { center = point, radius = 2f }, shapeDef);
        }
    }

    private void Update()
    {
        // Get the default world.
        var world = World;

        var segment = new SegmentGeometry { point1 = Vector2.zero, point2 = world.gravity * 3f };
        if (segment.isValid)
            world.DrawGeometry(segment, PhysicsTransform.identity, Color.orangeRed);
    }

    private void OnPreSimulation(PhysicsWorld world, float timeStep)
    {
        // Finish if not the sandbox world.
        if (!world.isDefaultWorld)
            return;

        // Update Gravity.
        {
            m_Time += timeStep;
            var rotation1 = PhysicsRotate.FromRadians(m_Time * 0.5f);
            var rotation2 = PhysicsRotate.FromRadians(m_Time);
            world.gravity = new Vector2(rotation1.direction.x, rotation2.direction.y) * m_GravityScale;
        }

        // Update Period.
        m_UpdateTime += timeStep;
        if (m_UpdateTime < m_UpdatePeriod)
            return;

        m_UpdateTime = 0f;

        // Spawn Ragdoll.
        if (m_RagdollCount >= m_MaxRagdollCount)
            return;

        m_RagdollCount++;

        var ragDollConfiguration = new RagdollFactory.Configuration
        {
            ScaleRange = new Vector2(1.75f, 1.75f),
            JointFrequency = 1f,
            JointDamping = 0.1f,
            JointFriction = 0.0f,
            GravityScale = 1f,
            ContactBodyLayer = 2,
            ContactFeetLayer = 1,
            ContactGroupIndex = 1,
            ColorProvider = SandboxManager,
            FastCollisionsAllowed = FastCollisions,
            TriggerEvents = false,
            EnableLimits = true,
            EnableMotor = true
        };

        ref var random = ref Random;
        var position = new Vector2(random.NextFloat(-2f, 2f), random.NextFloat(-2f, 0f));

        using var ragdoll = RagdollFactory.Spawn(world, position, ragDollConfiguration, true, ref random);
    }
}
