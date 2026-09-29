using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Implements a joint of its own, rather than using one of the built-in joints: two springs from a fixed point at the origin to two anchors on a box, solved by hand after every simulation step.
/// The box is authored in the scene as a Physics Pose with a Physics Area Polygon; only the joint itself is script, because it is the part a component cannot provide.
/// </summary>
/// <remarks>
/// Each spring only pulls, never pushes, and only once it is stretched past its rest length, so the box hangs from the two springs and swings freely when they are slack.
/// Every setting acts on the next step, without rebuilding anything.
/// </remarks>
public sealed class UserJointContents : MonoBehaviour
{
    /// <summary>
    /// How stiff each spring is, in cycles per second.
    /// </summary>
    public float jointFrequency
    {
        get => m_JointFrequency;
        set => m_JointFrequency = value;
    }

    /// <summary>
    /// How quickly each spring stops oscillating, where zero never settles and one settles without overshooting.
    /// </summary>
    public float jointDamping
    {
        get => m_JointDamping;
        set => m_JointDamping = value;
    }

    /// <summary>
    /// The most force each spring can pull with, in newtons.
    /// </summary>
    public float jointMaxForce
    {
        get => m_JointMaxForce;
        set => m_JointMaxForce = value;
    }

    /// <summary>
    /// How far either side of the box's center line the two anchors sit, in meters.
    /// </summary>
    public float anchorOffsetX
    {
        get => m_AnchorOffsetX;
        set => m_AnchorOffsetX = value;
    }

    /// <summary>
    /// How far along the box from its center the two anchors sit, in meters.
    /// </summary>
    public float anchorOffsetY
    {
        get => m_AnchorOffsetY;
        set => m_AnchorOffsetY = value;
    }

    /// <summary>
    /// The impulse the first spring applied on the last step, which is zero when it is slack.
    /// </summary>
    public float impulse0 => m_Impulses[0];

    /// <summary>
    /// The impulse the second spring applied on the last step, which is zero when it is slack.
    /// </summary>
    public float impulse1 => m_Impulses[1];

    private void OnEnable() => PhysicsEvents.PostSimulate += OnUpdateJoint;

    private void OnDisable() => PhysicsEvents.PostSimulate -= OnUpdateJoint;

    // Solves both springs once after the simulation step, applying their impulses to the box's velocity directly.
    private void OnUpdateJoint(PhysicsWorld world, float deltaTime)
    {
        if (deltaTime == 0f || m_Box == null)
            return;

        var body = m_Box.body;

        if (!body.isValid)
            return;

        // The soft-constraint coefficients, which turn the spring's frequency and damping into how much of the error to correct this step.
        var omega = 2f * PhysicsMath.PI * m_JointFrequency;
        var sigma = 2f * m_JointDamping + deltaTime * omega;
        var s = deltaTime * omega * sigma;
        var impulseCoefficient = 1f / (1f + s);
        var massCoefficient = s * impulseCoefficient;
        var biasCoefficient = omega / sigma;

        var mass = body.mass;
        var invMass = mass < 0.0001f ? 0f : 1f / mass;
        var inertiaTensor = body.rotationalInertia;
        var invI = inertiaTensor < 0.0001f ? 0f : 1f / inertiaTensor;

        var bodyLinearVelocity = body.linearVelocity;
        var bodyAngularVelocity = PhysicsMath.ToRadians(body.angularVelocity);
        var bodyWorldCenterOfMass = body.worldCenterOfMass;

        var localAnchors = new[] { new Vector2(m_AnchorOffsetY, -m_AnchorOffsetX), new Vector2(m_AnchorOffsetY, m_AnchorOffsetX) };

        var anchorA = Vector2.zero;
        world.DrawPoint(anchorA, 8f, Color.azure, deltaTime);

        for (var i = 0; i < 2; ++i)
        {
            var anchorB = body.GetWorldPoint(localAnchors[i]);
            world.DrawPoint(anchorB, 8f, Color.greenYellow, deltaTime);
            var deltaAnchor = anchorB - anchorA;

            // A spring shorter than its rest length is slack, so it applies nothing and is drawn in a lighter color.
            var length = deltaAnchor.magnitude;
            var compression = length - SpringLength;
            if (compression < 0f || length < 0.001f)
            {
                world.DrawLine(anchorA, anchorB, Color.lightCyan, deltaTime);
                m_Impulses[i] = 0f;
                continue;
            }

            world.DrawLine(anchorA, anchorB, Color.yellow, deltaTime);

            // The effective mass along the spring, from the box's mass and how far the anchor is from its center of mass.
            var axis = deltaAnchor.normalized;
            var rB = anchorB - bodyWorldCenterOfMass;
            var Jb = rB.x * axis.y - rB.y * axis.x;
            var K = invMass + Jb * invI * Jb;
            var invK = K < 0.0001f ? 0f : 1f / K;

            // The impulse only ever pulls, and never more than the maximum force allows in one step.
            var dotVelocity = Vector2.Dot(bodyLinearVelocity, axis) + Jb * bodyAngularVelocity;
            var impulse = -massCoefficient * invK * (dotVelocity + biasCoefficient * compression);
            var appliedImpulse = Mathf.Clamp(impulse, -m_JointMaxForce * deltaTime, 0f);

            bodyLinearVelocity += invMass * appliedImpulse * axis;
            bodyAngularVelocity += appliedImpulse * invI * Jb;

            m_Impulses[i] = appliedImpulse;
        }

        body.linearVelocity = bodyLinearVelocity;
        body.angularVelocity = PhysicsMath.ToDegrees(bodyAngularVelocity);
    }

    #region Internal

    // The length each spring rests at, beyond which it starts to pull.
    const float SpringLength = 1f;

    [SerializeField] PhysicsPose m_Box;
    [SerializeField, Range(0.1f, 240f)] float m_JointFrequency = 3f;
    [SerializeField, Range(0f, 4f)] float m_JointDamping = 0.7f;
    [SerializeField, Range(0f, 1000f)] float m_JointMaxForce = 1000f;
    [SerializeField, Range(0f, 0.7f)] float m_AnchorOffsetX = 0.5f;
    [SerializeField, Range(-1f, 1f)] float m_AnchorOffsetY = 1f;

    readonly float[] m_Impulses = new float[2];

    #endregion
}
