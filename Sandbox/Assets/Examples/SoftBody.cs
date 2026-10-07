using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Joints", "Demonstrating using joints to create a soft body.",
    Purpose = "Demonstrates building a soft body from rigid bodies and joints. A ring of capsule segments is held together by fixed joints whose angular spring is soft, so the ring flexes and wobbles instead of staying rigid.\nThe segments share a negative contact group, so neighbors that overlap at the joins never push each other apart.",
    Controls = "Body Sides and Body Scale: how many segments the ring has and how big it is.\nJoint Frequency and Joint Damping: how soft the joints are.\nEvery change rebuilds the ring.")]
public sealed class SoftBody : SandboxExampleBehaviour
{
    private int m_BodySides;
    private float m_BodyScale;
    private float m_JointFrequency;
    private float m_JointDamping;

    protected override float CameraSize => 5f;
    protected override Vector2 CameraPosition => new(0f, 0f);

    protected override void OnExampleEnable()
    {
        m_BodySides = 10;
        m_BodyScale = 2f;
        m_JointFrequency = 7f;
        m_JointDamping = 0f;
    }

    protected override void SetupOptions()
    {
        // Body Sides.
        AddSliderInt("Body Sides", m_BodySides, 3, 32, v => m_BodySides = v, rebuild: true);

        // Body Scale.
        AddSlider("Body Scale", m_BodyScale, 1f, 3f, v => m_BodyScale = v, rebuild: true);

        // Joint Frequency.
        AddSlider("Joint Frequency", m_JointFrequency, 0f, 60f, v => m_JointFrequency = v, rebuild: true);

        // Joint Damping.
        AddSlider("Joint Damping", m_JointDamping, 0f, 1f, v => m_JointDamping = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground.
        {
            var bodyDef = PhysicsBodyDefinition.defaultDefinition;
            var groundBody = world.CreateBody(bodyDef);

            using var vertices = new NativeList<Vector2>(4, Allocator.Temp)
            {
                new(-5.5f, 4.5f),
                new(5.5f, 4.5f),
                new(5.5f, -4.5f),
                new(-5.5f, -4.5f)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        // Soft Body.
        {
            using var donut = SoftbodyFactory.SpawnDonut(world, SandboxManager, Vector2.zero, m_BodySides, m_BodyScale, triggerEvents: false, jointFrequency: m_JointFrequency, jointDamping: m_JointDamping);
        }
    }
}
