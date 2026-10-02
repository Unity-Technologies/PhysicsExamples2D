using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Builds a short chain of small circles joined end to end by distance joints, hanging from a fixed point, so every setting of the distance joint can be tried on it.
/// Every circle is one Physics Pose with a Physics Area Circle, held to the one before it by a Physics Constraint Distance.
/// </summary>
/// <remarks>
/// The spring, limit toggle and motor settings act on the chain where it hangs, while changing the joint count, the distance or the distance limits rebuilds it.
/// </remarks>
public sealed class DistanceJointContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current chain and builds a new one with the current settings.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        if (m_BallPrefab == null || m_Ground == null)
            return;

        var previous = m_Ground;

        // Each circle starts one joint distance further along, and its joint pulls on the center of the circle before it, which for the first circle is the fixed point.
        for (var n = 0; n < m_JointCount; ++n)
        {
            var spawned = Instantiate(m_BallPrefab, new Vector3(m_JointDistance * (n + 1f), OffsetY, 0f), Quaternion.identity);
            m_Spawned.Add(spawned);

            var ball = spawned.GetComponent<PhysicsPose>();
            var anchorOnPrevious = n == 0 ? new Vector2(0f, OffsetY) : Vector2.zero;

            AddJoint(ball, previous, anchorOnPrevious);

            spawned.SetActive(true);
            previous = ball;
        }
    }

    /// <summary>
    /// Destroys every circle in the chain, leaving the fixed point alone.
    /// </summary>
    public void Clear()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
        m_Joints.Clear();
    }

    /// <summary>
    /// How many circles, and so how many joints, make up the chain.
    /// Changing this does not rebuild the chain until <see cref="Rebuild"/> is called.
    /// </summary>
    public int jointCount
    {
        get => m_JointCount;
        set => m_JointCount = value;
    }

    /// <summary>
    /// The rest distance each joint holds between the two things it joins, in meters, which is also how far apart the circles start.
    /// Changing this does not rebuild the chain until <see cref="Rebuild"/> is called.
    /// </summary>
    public float jointDistance
    {
        get => m_JointDistance;
        set => m_JointDistance = value;
    }

    /// <summary>
    /// Whether each joint acts as a spring around its rest distance rather than holding it rigidly.
    /// </summary>
    public bool enableSpring
    {
        get => m_EnableSpring;
        set
        {
            m_EnableSpring = value;
            UpdateJoints();
        }
    }

    /// <summary>
    /// How stiff each spring is, in cycles per second.
    /// </summary>
    public float springFrequency
    {
        get => m_SpringFrequency;
        set
        {
            m_SpringFrequency = value;
            UpdateJoints();
        }
    }

    /// <summary>
    /// How quickly each spring stops oscillating, where zero never settles and one settles without overshooting.
    /// </summary>
    public float springDamping
    {
        get => m_SpringDamping;
        set
        {
            m_SpringDamping = value;
            UpdateJoints();
        }
    }

    /// <summary>
    /// The most force each spring can apply to pull a stretched joint back in, in newtons.
    /// </summary>
    public float springTension
    {
        get => m_SpringTension;
        set
        {
            m_SpringTension = value;
            UpdateJoints();
        }
    }

    /// <summary>
    /// The most force each spring can apply to push a compressed joint back out, in newtons.
    /// </summary>
    public float springCompression
    {
        get => m_SpringCompression;
        set
        {
            m_SpringCompression = value;
            UpdateJoints();
        }
    }

    /// <summary>
    /// Whether each joint is kept between the minimum and maximum distance limits.
    /// </summary>
    public bool enableLimit
    {
        get => m_EnableLimit;
        set
        {
            m_EnableLimit = value;
            UpdateJoints();
        }
    }

    /// <summary>
    /// The shortest each joint can become when the limit is enabled, in meters.
    /// Changing this does not rebuild the chain until <see cref="Rebuild"/> is called.
    /// </summary>
    public float minDistanceLimit
    {
        get => m_MinDistanceLimit;
        set => m_MinDistanceLimit = value;
    }

    /// <summary>
    /// The longest each joint can become when the limit is enabled, in meters.
    /// Changing this does not rebuild the chain until <see cref="Rebuild"/> is called.
    /// </summary>
    public float maxDistanceLimit
    {
        get => m_MaxDistanceLimit;
        set => m_MaxDistanceLimit = value;
    }

    /// <summary>
    /// Whether each joint drives its length toward the motor speed.
    /// </summary>
    public bool enableMotor
    {
        get => m_EnableMotor;
        set
        {
            m_EnableMotor = value;
            UpdateJoints();
        }
    }

    /// <summary>
    /// The speed each motor drives its joint at, in meters per second, where a positive speed lengthens it.
    /// </summary>
    public float motorSpeed
    {
        get => m_MotorSpeed;
        set
        {
            m_MotorSpeed = value;
            UpdateJoints();
        }
    }

    /// <summary>
    /// The most force each motor can apply, in newtons.
    /// </summary>
    public float maxMotorForce
    {
        get => m_MaxMotorForce;
        set
        {
            m_MaxMotorForce = value;
            UpdateJoints();
        }
    }

    private void Start() => Rebuild();

    // Joins a circle to the one before it in the chain, pulling on the circle's center.
    private void AddJoint(PhysicsPose physicsPose, PhysicsPose previous, Vector2 anchorOnPrevious)
    {
        var constraint = physicsPose.gameObject.AddComponent<PhysicsConstraintDistance>();
        constraint.source = PhysicsConstraint.PoseSource.Custom;
        constraint.poseA = previous;
        constraint.poseB = physicsPose;

        var definition = constraint.definition;
        definition.autoAnchorA = false;
        definition.autoAnchorB = false;
        definition.autoDistance = false;
        definition.localAnchorA = new PhysicsTransform(anchorOnPrevious, PhysicsRotate.identity);
        definition.localAnchorB = PhysicsTransform.identity;
        constraint.definition = WriteSettings(definition);

        m_Joints.Add(constraint);
    }

    // Writes the current settings onto every joint in the chain, which wakes the chain so it responds straight away.
    private void UpdateJoints()
    {
        foreach (var constraint in m_Joints)
        {
            if (constraint == null)
                continue;

            constraint.definition = WriteSettings(constraint.definition);
            constraint.ApplyDefinition();
        }
    }

    // Returns the specified definition with every setting the controls expose written onto it.
    private PhysicsDistanceJointDefinition WriteSettings(PhysicsDistanceJointDefinition definition)
    {
        definition.distance = m_JointDistance;
        definition.enableSpring = m_EnableSpring;
        definition.springFrequency = m_SpringFrequency;
        definition.springDamping = m_SpringDamping;
        definition.springLowerForce = -m_SpringTension;
        definition.springUpperForce = m_SpringCompression;
        definition.enableLimit = m_EnableLimit;
        definition.minDistanceLimit = m_MinDistanceLimit;
        definition.maxDistanceLimit = m_MaxDistanceLimit;
        definition.enableMotor = m_EnableMotor;
        definition.motorSpeed = m_MotorSpeed;
        definition.maxMotorForce = m_MaxMotorForce;

        return definition;
    }

    #region Internal

    // The height the chain is laid out at.
    const float OffsetY = 20f;

    [SerializeField] GameObject m_BallPrefab;
    [SerializeField] PhysicsPose m_Ground;
    [SerializeField, Range(1, 20)] int m_JointCount = 3;
    [SerializeField, Range(0.5f, 4f)] float m_JointDistance = 1f;
    [SerializeField] bool m_EnableSpring;
    [SerializeField, Range(0f, 60f)] float m_SpringFrequency = 5f;
    [SerializeField, Range(0f, 4f)] float m_SpringDamping = 0.5f;
    [SerializeField, Range(0f, 4000f)] float m_SpringTension = 2000f;
    [SerializeField, Range(0f, 200f)] float m_SpringCompression = 100f;
    [SerializeField] bool m_EnableLimit;
    [SerializeField, Range(0.1f, 4f)] float m_MinDistanceLimit = 1f;
    [SerializeField, Range(0.4f, 4f)] float m_MaxDistanceLimit = 1f;
    [SerializeField] bool m_EnableMotor;
    [SerializeField, Range(-50f, 50f)] float m_MotorSpeed = 2f;
    [SerializeField, Range(0f, 500f)] float m_MaxMotorForce = 50f;

    readonly List<GameObject> m_Spawned = new();
    readonly List<PhysicsConstraintDistance> m_Joints = new();

    #endregion
}
