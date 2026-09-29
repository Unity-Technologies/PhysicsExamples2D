using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Turns the ignore joint between the two large boxes on and off, so they either pass straight through each other or collide.
/// The boxes, the obstacle between them and the ground are authored in the scene, and the joint is a Physics Constraint Ignore on the first box.
/// </summary>
public sealed class IgnoreJointContents : MonoBehaviour
{
    /// <summary>
    /// Whether the ignore joint is stopping every contact between the two boxes.
    /// </summary>
    public bool enableJoint
    {
        get => m_EnableJoint;
        set
        {
            m_EnableJoint = value;
            UpdateJoint();
        }
    }

    private void Start() => UpdateJoint();

    // Enables or disables the ignore joint to match the current setting; disabling the component destroys the joint and enabling it creates a new one.
    private void UpdateJoint()
    {
        if (m_IgnoreConstraint == null)
            return;

        // Removing an ignore joint can leave the two bodies not colliding until one of them moves a long way, so the first body is disabled and enabled again first to make them collide straight away.
        if (!m_EnableJoint && m_IgnoreConstraint.hasJoint)
        {
            var body = m_IgnoreConstraint.resolvedPoseA.body;
            body.enabled = false;
            body.enabled = true;
        }

        m_IgnoreConstraint.enabled = m_EnableJoint;
    }

    #region Internal

    [SerializeField] PhysicsConstraintIgnore m_IgnoreConstraint;
    [SerializeField] bool m_EnableJoint = true;

    #endregion
}
