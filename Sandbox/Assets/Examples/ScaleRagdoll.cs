using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Joints", "Checking the stability of solving forced overlap due to scaling a relatively complex setup.")]
public sealed class ScaleRagdoll : SandboxExampleBehaviour
{
    private RagdollFactory.Ragdoll m_Ragdoll;
    private float m_RagdollScale;

    protected override float CameraSize => 5f;
    protected override Vector2 CameraPosition => new(0f, 4f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        m_RagdollScale = 1f;
    }

    protected override void OnExampleDisable()
    {
        if (m_Ragdoll.IsCreated)
            m_Ragdoll.Dispose();
    }

    protected override void OnBeforeResetScene()
    {
        // Dispose the ragdoll before the world is reset (while its handles are still valid).
        if (m_Ragdoll.IsCreated)
            m_Ragdoll.Dispose();
    }

    protected override void SetupOptions()
    {
        // Ragdoll Scale.
        AddSlider("Ragdoll Scale", m_RagdollScale, 0.5f, 10f, v =>
        {
            m_RagdollScale = v;
            m_Ragdoll.Rescale(m_RagdollScale);
        });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        ref var random = ref Random;

        // Ground.
        {
            var groundBody = world.CreateBody();

            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(40f, 20f), radius: 0, transform: new PhysicsTransform(Vector2.down * 10f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(20f, 100f), radius: 0, transform: new PhysicsTransform(Vector2.left * 20f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(20f, 100f), radius: 0, transform: new PhysicsTransform(Vector2.right * 20f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(40f, 20f), radius: 0, transform: new PhysicsTransform(Vector2.up * 20f)));
        }

        // Ragdoll.
        {
            var ragDollConfiguration = new RagdollFactory.Configuration
            {
                ScaleRange = new Vector2(m_RagdollScale, m_RagdollScale),
                AngularImpulseRange = new Vector2(m_RagdollScale * 10f, m_RagdollScale * 10f),
                JointFrequency = 1f,
                JointDamping = 0.5f,
                JointFriction = 0.03f,
                GravityScale = 1f,
                ContactBodyLayer = 2,
                ContactFeetLayer = 1,
                ContactGroupIndex = 1,
                ColorProvider = SandboxManager,
                FastCollisionsAllowed = true,
                TriggerEvents = false,
                EnableLimits = true,
                EnableMotor = true
            };

            m_Ragdoll = RagdollFactory.Spawn(world, Vector2.up * 5f, ragDollConfiguration, true, ref random, Allocator.Persistent);
        }
    }
}
