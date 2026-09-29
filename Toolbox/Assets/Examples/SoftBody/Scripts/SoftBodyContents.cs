using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Builds a soft body as a ring of capsule segments, each held to its neighbors by a fixed joint with a soft angular spring, so the ring flexes and wobbles instead of staying rigid.
/// Every segment is one Physics Pose with a Physics Area Capsule, instantiated from a single prefab and joined to the one before it by a Physics Constraint Fixed.
/// </summary>
/// <remarks>
/// The segments all share a negative group, so neighboring segments that overlap at the joins never push each other apart.
/// Every setting rebuilds the ring.
/// </remarks>
public sealed class SoftBodyContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current ring and builds a new one with the current settings.
    /// </summary>
    public void Rebuild()
    {
        if (m_Ring != null)
            Destroy(m_Ring);

        if (m_SegmentPrefab == null)
            return;

        // The ring is built inactive and switched on once, so every segment and every joint between them is created together.
        m_Ring = new GameObject("SoftBody");
        m_Ring.SetActive(false);

        var radius = m_BodyScale;
        var deltaAngle = PhysicsMath.TAU / m_BodySides;
        var length = PhysicsMath.TAU * radius / m_BodySides;
        var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.5f * length), center2 = new Vector2(0f, 0.5f * length), radius = 0.25f * m_BodyScale };

        var segments = new PhysicsPose[m_BodySides];

        // Each segment lies along the ring's edge, standing at a right angle to the line from the center, starting from the same angle the Sandbox does.
        var angle = StartAngle;
        for (var i = 0; i < m_BodySides; ++i)
        {
            var position = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0f);
            var segment = Instantiate(m_SegmentPrefab, position, Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg), m_Ring.transform);
            segment.SetActive(true);

            segment.GetComponent<PhysicsAreaCapsule>().geometry = capsuleGeometry;
            segments[i] = segment.GetComponent<PhysicsPose>();

            angle += deltaAngle;
        }

        // Each segment's top end is fixed to the next segment's bottom end, all the way round and back to the first.
        var previous = segments[m_BodySides - 1];
        foreach (var segment in segments)
        {
            AddJoint(segment, previous, length);
            previous = segment;
        }

        m_Ring.SetActive(true);
    }

    /// <summary>
    /// How many segments make up the ring.
    /// Changing this does not rebuild the ring until <see cref="Rebuild"/> is called.
    /// </summary>
    public int bodySides
    {
        get => m_BodySides;
        set => m_BodySides = value;
    }

    /// <summary>
    /// The radius of the ring in meters, which also sets how thick each segment is.
    /// Changing this does not rebuild the ring until <see cref="Rebuild"/> is called.
    /// </summary>
    public float bodyScale
    {
        get => m_BodyScale;
        set => m_BodyScale = value;
    }

    /// <summary>
    /// How stiffly each joint springs back to straight, in cycles per second, where zero holds it rigidly.
    /// Changing this does not rebuild the ring until <see cref="Rebuild"/> is called.
    /// </summary>
    public float jointFrequency
    {
        get => m_JointFrequency;
        set => m_JointFrequency = value;
    }

    /// <summary>
    /// How quickly each joint's spring stops oscillating, where zero never settles and one settles without overshooting.
    /// Changing this does not rebuild the ring until <see cref="Rebuild"/> is called.
    /// </summary>
    public float jointDamping
    {
        get => m_JointDamping;
        set => m_JointDamping = value;
    }

    private void Start() => Rebuild();

    // Fixes a segment's bottom end to the previous segment's top end, with a soft angular spring so the join can bend.
    private void AddJoint(PhysicsPose segment, PhysicsPose previous, float length)
    {
        var joint = segment.gameObject.AddComponent<PhysicsConstraintFixed>();
        joint.source = PhysicsConstraint.PoseSource.Custom;
        joint.poseA = previous;
        joint.poseB = segment;

        var definition = joint.definition;
        definition.autoAnchorA = false;
        definition.autoAnchorB = false;
        definition.localAnchorA = new PhysicsTransform(new Vector2(0f, 0.5f * length), PhysicsRotate.identity);
        definition.localAnchorB = new PhysicsTransform(new Vector2(0f, -0.5f * length), PhysicsRotate.identity);
        definition.angularFrequency = m_JointFrequency;
        definition.angularDamping = m_JointDamping;
        joint.definition = definition;
    }

    #region Internal

    // The angle the first segment is placed at, in radians, matching the Sandbox's layout.
    const float StartAngle = 35f;

    [SerializeField] GameObject m_SegmentPrefab;
    [SerializeField, Range(3, 32)] int m_BodySides = 10;
    [SerializeField, Range(1f, 3f)] float m_BodyScale = 2f;
    [SerializeField, Range(0f, 60f)] float m_JointFrequency = 7f;
    [SerializeField, Range(0f, 1f)] float m_JointDamping;

    GameObject m_Ring;

    #endregion
}
