using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// An Anglepoise style desk lamp whose springs balance the arm and shade at any pose, with a child's ball beside it.
/// The whole lamp is authored from Physics Poses, Physics Areas and Physics Constraints, so this script only holds the settings that can be changed while it runs.
/// </summary>
/// <remarks>
/// The arm is a parallelogram of two long links, a short silver link and an upper arm, so the shade keeps its angle as the arm moves.
/// Two springs from a point on the post to a lever on each long link have almost no free length, so the spring's torque follows the gravity torque as the arm swings and the arm balances over its whole range.
/// The post and the elbow have a motor that never turns and is limited in torque, which acts as friction, and the head is held by a spring with limits.
/// The settings change the running lamp straight away, and the lamp is posed by dragging the shade.
/// </remarks>
public sealed class DeskLampContents : MonoBehaviour
{
    /// <summary>
    /// The frequency of the spring that balances the arm, in hertz.
    /// </summary>
    public float armSpring
    {
        get => m_ArmSpring;
        set
        {
            m_ArmSpring = value;
            ApplySpring(m_ArmSpringConstraint, m_ArmSpring);
        }
    }

    /// <summary>
    /// The frequency of the spring that balances the shade through the parallelogram, in hertz.
    /// </summary>
    public float headSpring
    {
        get => m_HeadSpring;
        set
        {
            m_HeadSpring = value;
            ApplySpring(m_HeadSpringConstraint, m_HeadSpring);
        }
    }

    /// <summary>
    /// How quickly the motion of both springs dies away, where one is critical damping.
    /// </summary>
    public float springDamping
    {
        get => m_SpringDamping;
        set
        {
            m_SpringDamping = value;
            ApplyDamping(m_ArmSpringConstraint);
            ApplyDamping(m_HeadSpringConstraint);
        }
    }

    /// <summary>
    /// The most torque the friction at the post can carry before the arm turns.
    /// </summary>
    public float postFriction
    {
        get => m_PostFriction;
        set
        {
            m_PostFriction = value;
            ApplyFriction(m_PostHinge, m_PostFriction);
        }
    }

    /// <summary>
    /// The most torque the friction at the elbow can carry before the upper arm turns.
    /// </summary>
    public float elbowFriction
    {
        get => m_ElbowFriction;
        set
        {
            m_ElbowFriction = value;
            ApplyFriction(m_ElbowHinge, m_ElbowFriction);
        }
    }

    /// <summary>
    /// The angle the shade is held at relative to the upper arm, in degrees.
    /// </summary>
    public float headAngle
    {
        get => m_HeadAngle;
        set
        {
            m_HeadAngle = value;

            var joint = m_HeadHinge != null ? m_HeadHinge.GetJoint() : default;
            if (!joint.isValid)
                return;

            joint.springTargetAngle = m_HeadAngle;
            joint.WakeBodies();
        }
    }

    // The constraints are created as the scene loads, so the settings are given to them once they exist.
    private void Start()
    {
        ApplySpring(m_ArmSpringConstraint, m_ArmSpring);
        ApplySpring(m_HeadSpringConstraint, m_HeadSpring);
        ApplyDamping(m_ArmSpringConstraint);
        ApplyDamping(m_HeadSpringConstraint);
        ApplyFriction(m_PostHinge, m_PostFriction);
        ApplyFriction(m_ElbowHinge, m_ElbowFriction);

        var joint = m_HeadHinge != null ? m_HeadHinge.GetJoint() : default;
        if (joint.isValid)
            joint.springTargetAngle = m_HeadAngle;
    }

    // Sets the frequency of a spring and wakes the lamp so the change is seen.
    private static void ApplySpring(PhysicsConstraintDistance spring, float frequency)
    {
        var joint = spring != null ? spring.GetJoint() : default;
        if (!joint.isValid)
            return;

        joint.springFrequency = frequency;
        joint.WakeBodies();
    }

    // Sets the damping of a spring to the current value and wakes the lamp so the change is seen.
    private void ApplyDamping(PhysicsConstraintDistance spring)
    {
        var joint = spring != null ? spring.GetJoint() : default;
        if (!joint.isValid)
            return;

        joint.springDamping = m_SpringDamping;
        joint.WakeBodies();
    }

    // Sets the torque a friction pivot can carry and wakes the lamp so the change is seen.
    private static void ApplyFriction(PhysicsConstraintHinge hinge, float torque)
    {
        var joint = hinge != null ? hinge.GetJoint() : default;
        if (!joint.isValid)
            return;

        joint.maxMotorTorque = torque;
        joint.WakeBodies();
    }

    #region Internal

    [SerializeField] PhysicsConstraintDistance m_ArmSpringConstraint;
    [SerializeField] PhysicsConstraintDistance m_HeadSpringConstraint;
    [SerializeField] PhysicsConstraintHinge m_PostHinge;
    [SerializeField] PhysicsConstraintHinge m_ElbowHinge;
    [SerializeField] PhysicsConstraintHinge m_HeadHinge;
    [SerializeField, Range(0f, 20f)] float m_ArmSpring = 5f;
    [SerializeField, Range(0f, 20f)] float m_HeadSpring = 5f;
    [SerializeField, Range(0f, 5f)] float m_SpringDamping = 4f;
    [SerializeField, Range(0f, 200f)] float m_PostFriction = 100f;
    [SerializeField, Range(0f, 200f)] float m_ElbowFriction = 100f;
    [SerializeField, Range(-68.75f, 68.75f)] float m_HeadAngle;

    #endregion
}
