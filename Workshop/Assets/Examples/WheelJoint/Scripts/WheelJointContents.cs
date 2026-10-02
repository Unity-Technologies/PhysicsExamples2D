using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Holds a wheel on a wheel joint to a fixed point, so it spins freely while sliding along one axis against a spring, and exposes every wheel joint setting to try on it.
/// The wheel is one Physics Pose with a Physics Area Circle, instantiated from a prefab and held to the fixed point by a Physics Constraint Wheel.
/// </summary>
/// <remarks>
/// Changing the wheel angle rebuilds the wheel and its joint, since the axis is part of the joint's anchor; every other setting acts on the joint where it is.
/// </remarks>
public sealed class WheelJointContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current wheel and creates a new one on a wheel joint along the current angle.
    /// </summary>
    public void Rebuild()
    {
        if (m_Spawned != null)
            Destroy(m_Spawned);

        m_Joint = null;

        if (m_WheelPrefab == null || m_Ground == null)
            return;

        m_Spawned = Instantiate(m_WheelPrefab, new Vector3(WheelStart.x, WheelStart.y, 0f), Quaternion.identity);
        var wheel = m_Spawned.GetComponent<PhysicsPose>();

        // The fixed point's anchor sits at the pivot, turned by the wheel angle, which sets the axis the wheel slides along; the wheel's own anchor is its center.
        m_Joint = m_Spawned.AddComponent<PhysicsConstraintWheel>();
        m_Joint.source = PhysicsConstraint.PoseSource.Custom;
        m_Joint.poseA = m_Ground;
        m_Joint.poseB = wheel;

        var definition = m_Joint.definition;
        definition.autoAnchorA = false;
        definition.autoAnchorB = false;
        definition.autoAxis = false;
        definition.localAnchorA = new PhysicsTransform(Pivot, PhysicsRotate.FromDegrees(m_WheelAngle));
        definition.localAnchorB = PhysicsTransform.identity;
        definition.drawScale = DrawScale;
        m_Joint.definition = WriteSettings(definition);

        m_Spawned.SetActive(true);
    }

    /// <summary>
    /// The angle of the axis the wheel slides along, in degrees counter-clockwise from horizontal.
    /// Changing this does not rebuild the wheel until <see cref="Rebuild"/> is called.
    /// </summary>
    public float wheelAngle
    {
        get => m_WheelAngle;
        set => m_WheelAngle = value;
    }

    /// <summary>
    /// Whether a spring along the axis acts as the wheel's suspension.
    /// </summary>
    public bool enableSpring
    {
        get => m_EnableSpring;
        set
        {
            m_EnableSpring = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// How stiff the suspension spring is, in cycles per second.
    /// </summary>
    public float springFrequency
    {
        get => m_SpringFrequency;
        set
        {
            m_SpringFrequency = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// How quickly the suspension spring stops oscillating, where zero never settles and one settles without overshooting.
    /// </summary>
    public float springDamping
    {
        get => m_SpringDamping;
        set
        {
            m_SpringDamping = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// Whether a motor spins the wheel.
    /// </summary>
    public bool enableMotor
    {
        get => m_EnableMotor;
        set
        {
            m_EnableMotor = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// The speed the motor spins the wheel at, in degrees per second.
    /// </summary>
    public float motorSpeed
    {
        get => m_MotorSpeed;
        set
        {
            m_MotorSpeed = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// The most torque the motor can apply.
    /// </summary>
    public float maxMotorTorque
    {
        get => m_MaxMotorTorque;
        set
        {
            m_MaxMotorTorque = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// Whether the wheel is kept between the lower and upper translation limits along the axis.
    /// </summary>
    public bool enableLimit
    {
        get => m_EnableLimit;
        set
        {
            m_EnableLimit = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// The furthest the wheel can move along the axis in the negative direction from the pivot, in meters.
    /// </summary>
    public float lowerTranslationLimit
    {
        get => m_LowerTranslationLimit;
        set
        {
            m_LowerTranslationLimit = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// The furthest the wheel can move along the axis in the positive direction from the pivot, in meters.
    /// </summary>
    public float upperTranslationLimit
    {
        get => m_UpperTranslationLimit;
        set
        {
            m_UpperTranslationLimit = value;
            UpdateJoint();
        }
    }

    private void Start() => Rebuild();

    // Writes the current settings onto the running joint, which wakes the wheel so it responds straight away.
    private void UpdateJoint()
    {
        if (m_Joint == null)
            return;

        m_Joint.definition = WriteSettings(m_Joint.definition);
        m_Joint.ApplyDefinition();
    }

    // Returns the specified definition with every setting the controls expose written onto it.
    private PhysicsWheelJointDefinition WriteSettings(PhysicsWheelJointDefinition definition)
    {
        definition.enableSpring = m_EnableSpring;
        definition.springFrequency = m_SpringFrequency;
        definition.springDamping = m_SpringDamping;
        definition.enableMotor = m_EnableMotor;
        definition.motorSpeed = m_MotorSpeed;
        definition.maxMotorTorque = m_MaxMotorTorque;
        definition.enableLimit = m_EnableLimit;
        definition.lowerTranslationLimit = m_LowerTranslationLimit;
        definition.upperTranslationLimit = m_UpperTranslationLimit;

        return definition;
    }

    #region Internal

    // Where the wheel starts, where the joint's pivot sits just below it, and how large the joint is drawn.
    static readonly Vector2 WheelStart = new(0f, 10.25f);
    static readonly Vector2 Pivot = new(0f, 10f);
    const float DrawScale = 2f;

    [SerializeField] GameObject m_WheelPrefab;
    [SerializeField] PhysicsPose m_Ground;
    [SerializeField, Range(-180f, 180f)] float m_WheelAngle = 90f;
    [SerializeField] bool m_EnableSpring = true;
    [SerializeField, Range(0f, 60f)] float m_SpringFrequency = 1.5f;
    [SerializeField, Range(0f, 4f)] float m_SpringDamping = 0.7f;
    [SerializeField] bool m_EnableMotor = true;
    [SerializeField, Range(-3000f, 3000f)] float m_MotorSpeed = 120f;
    [SerializeField, Range(0f, 20f)] float m_MaxMotorTorque = 5f;
    [SerializeField] bool m_EnableLimit = true;
    [SerializeField, Range(-4f, 0f)] float m_LowerTranslationLimit = -1f;
    [SerializeField, Range(0f, 4f)] float m_UpperTranslationLimit = 1f;

    GameObject m_Spawned;
    PhysicsConstraintWheel m_Joint;

    #endregion
}
