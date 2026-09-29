using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Drives the scissor lift by running the motor on the distance joint that spans from the ground to one of its lower arms, so the lift raises or lowers the car parked on its platform.
/// The lift, its platform and the car are authored in the scene as Physics Pose, Physics Area and Physics Constraint components.
/// </summary>
public sealed class ScissorLiftContents : MonoBehaviour
{
    /// <summary>
    /// Whether the lift's motor is driving it.
    /// </summary>
    public bool enableMotor
    {
        get => m_EnableMotor;
        set
        {
            m_EnableMotor = value;
            UpdateMotor();
        }
    }

    /// <summary>
    /// How fast the motor lengthens the lift's distance joint, in meters per second, where a positive speed raises the lift.
    /// </summary>
    public float motorSpeed
    {
        get => m_MotorSpeed;
        set
        {
            m_MotorSpeed = value;
            UpdateMotor();
        }
    }

    private void Start() => UpdateMotor();

    // Writes the current motor settings onto the lift's distance joint, which wakes the lift so it responds straight away.
    private void UpdateMotor()
    {
        if (m_LiftJoint == null)
            return;

        var definition = m_LiftJoint.definition;
        definition.enableMotor = m_EnableMotor;
        definition.motorSpeed = m_MotorSpeed;
        m_LiftJoint.definition = definition;

        m_LiftJoint.ApplyDefinition();
    }

    #region Internal

    [SerializeField] PhysicsConstraintDistance m_LiftJoint;
    [SerializeField] bool m_EnableMotor;
    [SerializeField, Range(-2f, 2f)] float m_MotorSpeed = 0.25f;

    #endregion
}
