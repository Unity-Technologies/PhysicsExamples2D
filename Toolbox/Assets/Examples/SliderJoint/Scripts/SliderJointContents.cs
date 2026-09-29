using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Holds a capsule on a slider joint to a fixed point, so it can only move along one axis, and exposes every slider setting to try on it.
/// The capsule is one Physics Pose with a Physics Area Capsule, instantiated from a prefab and held to the fixed point by a Physics Constraint Slider.
/// </summary>
/// <remarks>
/// Changing the slide angle rebuilds the capsule and its joint, since the axis is part of the joint's anchors; every other setting acts on the joint where it is.
/// </remarks>
public sealed class SliderJointContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current capsule and creates a new one on a slider joint along the current angle.
    /// </summary>
    public void Rebuild()
    {
        if (m_Spawned != null)
            Destroy(m_Spawned);

        m_Joint = null;

        if (m_CapsulePrefab == null || m_Ground == null)
            return;

        m_Spawned = Instantiate(m_CapsulePrefab, new Vector3(Pivot.x, Pivot.y, 0f), Quaternion.identity);
        var capsule = m_Spawned.GetComponent<PhysicsPose>();

        // Both anchors sit at the pivot and share the slide angle, which sets the axis the capsule moves along.
        var slideRotation = PhysicsRotate.FromDegrees(m_SliderAngle);

        m_Joint = m_Spawned.AddComponent<PhysicsConstraintSlider>();
        m_Joint.source = PhysicsConstraint.PoseSource.Custom;
        m_Joint.poseA = m_Ground;
        m_Joint.poseB = capsule;

        var definition = m_Joint.definition;
        definition.autoAnchorA = false;
        definition.autoAnchorB = false;
        definition.autoAxis = false;
        definition.localAnchorA = new PhysicsTransform(Pivot, slideRotation);
        definition.localAnchorB = new PhysicsTransform(Vector2.zero, slideRotation);
        definition.drawScale = DrawScale;
        m_Joint.definition = WriteSettings(definition);

        m_Spawned.SetActive(true);
    }

    /// <summary>
    /// The angle of the axis the capsule slides along, in degrees counter-clockwise from horizontal.
    /// Changing this does not rebuild the capsule until <see cref="Rebuild"/> is called.
    /// </summary>
    public float sliderAngle
    {
        get => m_SliderAngle;
        set => m_SliderAngle = value;
    }

    /// <summary>
    /// Whether a spring pulls the capsule toward the spring target.
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
    /// How far along the axis from the pivot the spring pulls the capsule to, in meters.
    /// </summary>
    public float springTargetTranslation
    {
        get => m_SpringTargetTranslation;
        set
        {
            m_SpringTargetTranslation = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// How stiff the spring is, in cycles per second.
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
    /// How quickly the spring stops oscillating, where zero never settles and one settles without overshooting.
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
    /// Whether a motor drives the capsule along the axis.
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
    /// The speed the motor drives the capsule at, in meters per second along the axis.
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
    /// The most force the motor can apply, in newtons.
    /// </summary>
    public float maxMotorForce
    {
        get => m_MaxMotorForce;
        set
        {
            m_MaxMotorForce = value;
            UpdateJoint();
        }
    }

    /// <summary>
    /// Whether the capsule is kept between the lower and upper translation limits.
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
    /// The furthest the capsule can move along the axis in the negative direction from the pivot, in meters.
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
    /// The furthest the capsule can move along the axis in the positive direction from the pivot, in meters.
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

    // Writes the current settings onto the running joint, which wakes the capsule so it responds straight away.
    private void UpdateJoint()
    {
        if (m_Joint == null)
            return;

        m_Joint.definition = WriteSettings(m_Joint.definition);
        m_Joint.ApplyDefinition();
    }

    // Returns the specified definition with every setting the controls expose written onto it.
    private PhysicsSliderJointDefinition WriteSettings(PhysicsSliderJointDefinition definition)
    {
        definition.enableSpring = m_EnableSpring;
        definition.springTargetTranslation = m_SpringTargetTranslation;
        definition.springFrequency = m_SpringFrequency;
        definition.springDamping = m_SpringDamping;
        definition.enableMotor = m_EnableMotor;
        definition.motorSpeed = m_MotorSpeed;
        definition.maxMotorForce = m_MaxMotorForce;
        definition.enableLimit = m_EnableLimit;
        definition.lowerTranslationLimit = m_LowerTranslationLimit;
        definition.upperTranslationLimit = m_UpperTranslationLimit;

        return definition;
    }

    #region Internal

    // Where the capsule starts and the joint's pivot sits, and how large the joint is drawn.
    static readonly Vector2 Pivot = new(0f, 9f);
    const float DrawScale = 2f;

    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] PhysicsPose m_Ground;
    [SerializeField, Range(-180f, 180f)] float m_SliderAngle = 45f;
    [SerializeField] bool m_EnableSpring;
    [SerializeField, Range(-10f, 10f)] float m_SpringTargetTranslation;
    [SerializeField, Range(0f, 60f)] float m_SpringFrequency = 1f;
    [SerializeField, Range(0f, 4f)] float m_SpringDamping = 0.5f;
    [SerializeField] bool m_EnableMotor;
    [SerializeField, Range(-50f, 50f)] float m_MotorSpeed = 2f;
    [SerializeField, Range(0f, 500f)] float m_MaxMotorForce = 50f;
    [SerializeField] bool m_EnableLimit = true;
    [SerializeField, Range(-10f, 0f)] float m_LowerTranslationLimit = -10f;
    [SerializeField, Range(0f, 10f)] float m_UpperTranslationLimit = 10f;

    GameObject m_Spawned;
    PhysicsConstraintSlider m_Joint;

    #endregion
}
