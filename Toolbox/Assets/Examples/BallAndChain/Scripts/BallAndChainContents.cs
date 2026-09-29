using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Builds a chain of capsule links hinged end to end, hanging from a fixed point with a heavy ball on its far end.
/// Every link and the ball is one Physics Pose with a Physics Area, and every link is held to the one before it by a Physics Constraint Hinge.
/// </summary>
/// <remarks>
/// The links never touch each other, only the ball, so the chain can fold through itself while the ball still swings into it.
/// The hinge spring and motor settings act on the chain where it hangs, while fixing the chain length rebuilds it with a distance limit from the fixed point to the ball.
/// </remarks>
public sealed class BallAndChainContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current chain and builds a new one with the current settings.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        if (m_LinkPrefab == null || m_BallPrefab == null || m_Ground == null)
            return;

        var previous = m_Ground;

        // Each link spans two halves of a unit, and the hinge sits at the joint between it and the one before, which on the ground is the fixed point itself.
        for (var n = 0; n < LinkCount; ++n)
        {
            var link = Spawn(m_LinkPrefab, new Vector2((1f + 2f * n) * Scale, ChainHeight));
            var anchorOnPrevious = n == 0 ? new Vector2(0f, ChainHeight) : new Vector2(Scale, 0f);

            AddHinge(link, previous, anchorOnPrevious, new Vector2(-Scale, 0f), n > 0);

            link.gameObject.SetActive(true);
            previous = link;
        }

        // The ball hangs off the far end of the last link, touching it at the ball's edge.
        var ball = Spawn(m_BallPrefab, new Vector2((1f + 2f * LinkCount) * Scale + BallRadius - Scale, ChainHeight));

        AddHinge(ball, previous, new Vector2(Scale, 0f), new Vector2(-BallRadius, 0f), true);

        if (m_FixChainLength)
            AddDistance(ball, m_Ground, new Vector2(0f, ChainHeight), new Vector2(-BallRadius, 0f));

        ball.gameObject.SetActive(true);
    }

    /// <summary>
    /// Destroys every link and the ball, leaving the fixed point alone.
    /// </summary>
    public void Clear()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
        m_Hinges.Clear();
    }

    /// <summary>
    /// How stiffly each hinge springs back to straight, in cycles per second.
    /// </summary>
    public float springFrequency
    {
        get => m_SpringFrequency;
        set
        {
            m_SpringFrequency = value;
            UpdateHinges();
        }
    }

    /// <summary>
    /// How quickly each hinge spring stops oscillating, where zero never settles and one settles without overshooting.
    /// </summary>
    public float springDamping
    {
        get => m_SpringDamping;
        set
        {
            m_SpringDamping = value;
            UpdateHinges();
        }
    }

    /// <summary>
    /// The most torque each hinge motor can apply to hold its links still against one another.
    /// </summary>
    public float maxMotorTorque
    {
        get => m_MaxMotorTorque;
        set
        {
            m_MaxMotorTorque = value;
            UpdateHinges();
        }
    }

    /// <summary>
    /// Whether a distance limit from the fixed point to the ball stops the chain from stretching.
    /// Changing this does not rebuild the chain until <see cref="Rebuild"/> is called.
    /// </summary>
    public bool fixChainLength
    {
        get => m_FixChainLength;
        set => m_FixChainLength = value;
    }

    private void Start() => Rebuild();

    // Creates one link or the ball, left inactive so its constraints can be added before its body is built.
    private PhysicsPose Spawn(GameObject prefab, Vector2 position)
    {
        var spawned = Instantiate(prefab, new Vector3(position.x, position.y, 0f), Quaternion.identity);

        m_Spawned.Add(spawned);

        return spawned.GetComponent<PhysicsPose>();
    }

    // Hinges a link or the ball to the body before it in the chain.
    // The anchors are body-local points at the place the two meet, so the pair pivots there rather than about either center.
    private void AddHinge(PhysicsPose physicsPose, PhysicsPose previous, Vector2 anchorOnPrevious, Vector2 anchorOnPose, bool enableSpring)
    {
        var hinge = physicsPose.gameObject.AddComponent<PhysicsConstraintHinge>();
        hinge.source = PhysicsConstraint.PoseSource.Custom;
        hinge.poseA = previous;
        hinge.poseB = physicsPose;

        var definition = hinge.definition;
        definition.autoAnchorA = false;
        definition.autoAnchorB = false;
        definition.localAnchorA = new PhysicsTransform(anchorOnPrevious, PhysicsRotate.identity);
        definition.localAnchorB = new PhysicsTransform(anchorOnPose, PhysicsRotate.identity);
        definition.enableMotor = true;
        definition.maxMotorTorque = m_MaxMotorTorque;
        definition.enableSpring = enableSpring;
        definition.springFrequency = m_SpringFrequency;
        definition.springDamping = m_SpringDamping;

        hinge.definition = definition;
        m_Hinges.Add(hinge);
    }

    // Holds the ball at a fixed distance from the fixed point, measured to the place it meets the chain.
    private static void AddDistance(PhysicsPose physicsPose, PhysicsPose ground, Vector2 anchorOnGround, Vector2 anchorOnPose)
    {
        var constraint = physicsPose.gameObject.AddComponent<PhysicsConstraintDistance>();
        constraint.source = PhysicsConstraint.PoseSource.Custom;
        constraint.poseA = ground;
        constraint.poseB = physicsPose;

        var definition = constraint.definition;
        definition.autoAnchorA = false;
        definition.autoAnchorB = false;
        definition.autoDistance = false;
        definition.localAnchorA = new PhysicsTransform(anchorOnGround, PhysicsRotate.identity);
        definition.localAnchorB = new PhysicsTransform(anchorOnPose, PhysicsRotate.identity);
        definition.distance = FixedDistance;
        definition.enableSpring = false;
        definition.springFrequency = 30f;
        definition.springDamping = 1f;
        definition.enableLimit = false;
        definition.minDistanceLimit = -FixedDistance;
        definition.maxDistanceLimit = FixedDistance;

        constraint.definition = definition;
    }

    // Writes the current spring and motor settings onto every hinge in the chain, which wakes the chain so it responds straight away.
    private void UpdateHinges()
    {
        foreach (var hinge in m_Hinges)
        {
            if (hinge == null)
                continue;

            var definition = hinge.definition;
            definition.springFrequency = m_SpringFrequency;
            definition.springDamping = m_SpringDamping;
            definition.maxMotorTorque = m_MaxMotorTorque;
            hinge.definition = definition;

            hinge.ApplyDefinition();
        }
    }

    #region Internal

    // How many links make up the chain, and the half-length of each one, which sets the spacing everything else is measured from.
    const int LinkCount = 30;
    const float Scale = 0.5f;

    // The height the chain is laid out at, the radius of the ball on its end, and the distance the ball is held at when the chain length is fixed.
    const float ChainHeight = LinkCount * Scale;
    const float BallRadius = 4f;
    const float FixedDistance = LinkCount;

    [SerializeField] GameObject m_LinkPrefab;
    [SerializeField] GameObject m_BallPrefab;
    [SerializeField] PhysicsPose m_Ground;
    [SerializeField, Range(0f, 120f)] float m_SpringFrequency = 40f;
    [SerializeField, Range(0f, 10f)] float m_SpringDamping = 1f;
    [SerializeField, Range(0f, 1000f)] float m_MaxMotorTorque = 100f;
    [SerializeField] bool m_FixChainLength;

    readonly List<GameObject> m_Spawned = new();
    readonly List<PhysicsConstraintHinge> m_Hinges = new();

    #endregion
}
