using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Joints", "Demonstrating the implementation of a custom joint.")]
public sealed class UserJoint : SandboxExampleBehaviour
{
    private float m_JointFrequency;
    private float m_JointDamping;
    private float m_JointMaxForce;
    private float m_AnchorOffsetX;
    private float m_AnchorOffsetY;

    private readonly float[] m_Impulses = new float[2];
    private FloatField m_DisplayImpulse0;
    private FloatField m_DisplayImpulse1;
    private PhysicsBody m_Body;

    protected override float CameraSize => 3f;
    protected override Vector2 CameraPosition => new(0f, -1f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        m_JointFrequency = 3f;
        m_JointDamping = 0.7f;
        m_JointMaxForce = 1000f;
        m_AnchorOffsetX = 0.5f;
        m_AnchorOffsetY = 1f;

        PhysicsEvents.PostSimulate += OnUpdateJoint;
    }

    protected override void OnExampleDisable()
    {
        PhysicsEvents.PostSimulate -= OnUpdateJoint;
    }

    protected override void SetupOptions()
    {
        // Joint Frequency.
        AddSlider("Joint Frequency", m_JointFrequency, 0.1f, 240f, v => m_JointFrequency = v);

        // Joint Damping.
        AddSlider("Joint Damping", m_JointDamping, 0f, 4f, v => m_JointDamping = v);

        // Joint Max Force.
        AddSlider("Joint Max Force", m_JointMaxForce, 0f, 1000f, v => m_JointMaxForce = v);

        // Anchor Offset X.
        AddSlider("Anchor Offset X", m_AnchorOffsetX, 0f, 0.7f, v => m_AnchorOffsetX = v);

        // Anchor Offset Y.
        AddSlider("Anchor Offset Y", m_AnchorOffsetY, -1f, 1f, v => m_AnchorOffsetY = v);

        // Impulse Display (read-only, written from PostSimulate).
        m_DisplayImpulse0 = AddElement(new FloatField("Impulse #0") { isReadOnly = true, focusable = false });
        m_DisplayImpulse1 = AddElement(new FloatField("Impulse #1") { isReadOnly = true, focusable = false });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            gravityScale = 1f,
            angularDamping = 0.5f,
            linearDamping = 0.2f
        };

        m_Body = world.CreateBody(bodyDef);

        var geometry = PolygonGeometry.CreateBox(new Vector2(2f, 1f), 0.2f);
        var shapeDef = new PhysicsShapeDefinition { density = 20f };
        m_Body.CreateShape(geometry, shapeDef);

        // Reset impulse.
        m_Impulses[0] = 0.0f;
        m_Impulses[1] = 0.0f;
    }

    private void OnUpdateJoint(PhysicsWorld world, float deltaTime)
    {
        if (deltaTime == 0.0f)
            return;

        // Set-up.
        var omega = 2.0f * PhysicsMath.PI * m_JointFrequency;
        var sigma = 2.0f * m_JointDamping + deltaTime * omega;
        var s = deltaTime * omega * sigma;
        var impulseCoefficient = 1.0f / (1.0f + s);
        var massCoefficient = s * impulseCoefficient;
        var biasCoefficient = omega / sigma;

        var mass = m_Body.mass;
        var invMass = mass < 0.0001f ? 0.0f : 1.0f / mass;
        var inertiaTensor = m_Body.rotationalInertia;
        var invI = inertiaTensor < 0.0001f ? 0.0f : 1.0f / inertiaTensor;

        // Fetch the body state.
        var bodyLinearVelocity = m_Body.linearVelocity;
        var bodyAngularVelocity = PhysicsMath.ToRadians(m_Body.angularVelocity);
        var bodyWorldCenterOfMass = m_Body.worldCenterOfMass;

        var localAnchors = new[] { new(m_AnchorOffsetY, -m_AnchorOffsetX), new Vector2(m_AnchorOffsetY, m_AnchorOffsetX) };

        // Draw the ground anchor.
        var anchorA = Vector2.zero;
        world.DrawPoint(anchorA, 8f, Color.azure, deltaTime);

        // Iterate the two impulses.
        for (var i = 0; i < 2; ++i)
        {
            // Anchors.
            var anchorB = m_Body.GetWorldPoint(localAnchors[i]);
            world.DrawPoint(anchorB, 8f, Color.greenYellow, deltaTime);
            var deltaAnchor = anchorB - anchorA;

            // Spring.
            const float springLength = 1.0f;
            var length = deltaAnchor.magnitude;
            var compression = length - springLength;
            if (compression < 0.0f || length < 0.001f)
            {
                world.DrawLine(anchorA, anchorB, Color.lightCyan, deltaTime);
                m_Impulses[i] = 0.0f;
                continue;
            }

            // Draw constraint.
            world.DrawLine(anchorA, anchorB, Color.yellow, deltaTime);

            // Mass.
            var axis = deltaAnchor.normalized;
            var rB = anchorB - bodyWorldCenterOfMass;
            var Jb = rB.x * axis.y - rB.y * axis.x; // Cross product.
            var K = invMass + Jb * invI * Jb;
            var invK = K < 0.0001f ? 0.0f : 1.0f / K;

            // Impulse.
            var dotVelocity = Vector2.Dot(bodyLinearVelocity, axis) + Jb * bodyAngularVelocity;
            var impulse = -massCoefficient * invK * (dotVelocity + biasCoefficient * compression);
            var appliedImpulse = Mathf.Clamp(impulse, -m_JointMaxForce * deltaTime, 0.0f);

            // Velocity Sum.
            bodyLinearVelocity += invMass * appliedImpulse * axis;
            bodyAngularVelocity += appliedImpulse * invI * Jb;

            // Impulse.
            m_Impulses[i] = appliedImpulse;
        }

        // Update impulse display.
        m_DisplayImpulse0.value = m_Impulses[0];
        m_DisplayImpulse1.value = m_Impulses[1];

        // Update the body.
        m_Body.linearVelocity = bodyLinearVelocity;
        m_Body.angularVelocity = PhysicsMath.ToDegrees(bodyAngularVelocity);
    }
}
