using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Hangs a chain of small shapes from a spring-loaded hinge and sways it with a wind blowing from a chosen direction.
/// Every link is one Physics Pose with a Physics Area, joined to its neighbor by a Physics Constraint Hinge, and the wind itself is applied to each link's shape every simulation step.
/// </summary>
/// <remarks>
/// Drag pulls a shape along with the wind, and lift pushes it sideways across the flow, the same way a real flag or streamer catches a crosswind; a circle has no lift, since only a capsule or a polygon has an edge for lift to act across.
/// The wind direction wanders a little on its own, so the chain never settles into holding perfectly still even at a constant speed and direction.
/// </remarks>
public sealed class WindContents : MonoBehaviour
{
    /// <summary>
    /// The shape every link of the chain uses.
    /// </summary>
    public enum GeometryType
    {
        Circle,
        Capsule,
        Polygon
    }

    /// <summary>
    /// Destroys the chain and hangs a new one with the current shape and length.
    /// </summary>
    public void Rebuild()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
        m_Shapes.Clear();

        var linkPrefab = m_GeometryType switch
        {
            GeometryType.Circle => m_CirclePrefab,
            GeometryType.Capsule => m_CapsulePrefab,
            _ => m_PolygonPrefab
        };

        if (m_AnchorPrefab == null || linkPrefab == null)
            return;

        var anchor = Instantiate(m_AnchorPrefab, Vector3.zero, Quaternion.identity);
        anchor.SetActive(true);
        m_Spawned.Add(anchor);

        var previousPose = anchor.GetComponent<PhysicsPose>();

        for (var i = 0; i < m_GeometryCount; ++i)
        {
            var link = Instantiate(linkPrefab, new Vector3(0f, AnchorHeight - 2f * GeometryRadius * i, 0f), Quaternion.identity);
            var linkPose = link.GetComponent<PhysicsPose>();

            var hinge = link.GetComponent<PhysicsConstraintHinge>();
            hinge.source = PhysicsConstraint.PoseSource.Custom;
            hinge.poseA = previousPose;
            hinge.poseB = linkPose;

            var definition = hinge.definition;
            definition.autoAnchorA = false;
            definition.autoAnchorB = false;
            definition.localAnchorA = new PhysicsTransform(i == 0 ? new Vector2(0f, AnchorHeight) : new Vector2(0f, -GeometryRadius));
            definition.localAnchorB = new PhysicsTransform(new Vector2(0f, GeometryRadius));
            hinge.definition = definition;

            link.SetActive(true);
            m_Spawned.Add(link);
            m_Shapes.Add(link.GetComponent<PhysicsArea>().shape);

            previousPose = linkPose;
        }
    }

    /// <summary>
    /// The shape every link of the chain uses.
    /// Changing this does not rebuild the chain until <see cref="Rebuild"/> is called.
    /// </summary>
    public GeometryType geometryType
    {
        get => m_GeometryType;
        set => m_GeometryType = value;
    }

    /// <summary>
    /// How many links the chain has.
    /// Changing this does not rebuild the chain until <see cref="Rebuild"/> is called.
    /// </summary>
    public int geometryCount
    {
        get => m_GeometryCount;
        set => m_GeometryCount = value;
    }

    /// <summary>
    /// The direction the wind blows from, in degrees.
    /// </summary>
    public float windDirection
    {
        get => m_WindDirection;
        set => m_WindDirection = value;
    }

    /// <summary>
    /// How fast the wind blows, in meters per second.
    /// </summary>
    public float windSpeed
    {
        get => m_WindSpeed;
        set => m_WindSpeed = value;
    }

    /// <summary>
    /// How strongly the wind drags a link along with it.
    /// </summary>
    public float drag
    {
        get => m_Drag;
        set => m_Drag = value;
    }

    /// <summary>
    /// How strongly the wind pushes a capsule or polygon link sideways across the flow.
    /// </summary>
    public float lift
    {
        get => m_Lift;
        set => m_Lift = value;
    }

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);
        m_WindNoise = Vector2.zero;
        PhysicsEvents.PreSimulate += ApplyWind;
    }

    private void OnDisable() => PhysicsEvents.PreSimulate -= ApplyWind;

    private void Start() => Rebuild();

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;
        world.DrawLine(Vector2.zero, Vector2.up * (AnchorHeight + GeometryRadius), Color.gray);
        world.DrawLine(Vector2.zero, m_CurrentWind * 0.2f, Color.goldenRod);
    }

    // Applies the current wind to every link's shape, then drifts the wind's own small random wobble a little further.
    private void ApplyWind(PhysicsWorld world, float deltaTime)
    {
        if (world != PhysicsWorld.defaultWorld)
            return;

        var direction = PhysicsRotate.FromDegrees(m_WindDirection);
        m_CurrentWind = (direction + m_WindNoise) * m_WindSpeed;
        var windInput = new PhysicsBody.WindInput { drag = m_Drag, lift = m_Lift, force = m_CurrentWind };

        foreach (var shape in m_Shapes)
        {
            if (shape.isValid)
                shape.ApplyWind(windInput);
        }

        var noise = new Vector2(m_Random.NextFloat(-0.3f, 0.3f), m_Random.NextFloat(-0.3f, 0.3f));
        m_WindNoise = Vector2.Lerp(m_WindNoise, noise, 0.05f);
    }

    #region Internal

    // The radius every link is sized from, and how high above the origin the chain hangs from.
    const float GeometryRadius = 0.1f;
    const float AnchorHeight = 2f + GeometryRadius;
    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_AnchorPrefab;
    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField] GeometryType m_GeometryType = GeometryType.Capsule;
    [SerializeField, Range(1, 50)] int m_GeometryCount = 20;
    [SerializeField, Range(0f, 359f)] float m_WindDirection;
    [SerializeField, Range(0f, 10f)] float m_WindSpeed = 6f;
    [SerializeField, Range(0f, 1f)] float m_Drag = 1f;
    [SerializeField, Range(0f, 4f)] float m_Lift = 0.75f;

    readonly List<GameObject> m_Spawned = new();
    readonly List<PhysicsShape> m_Shapes = new();
    Vector2 m_WindNoise;
    Vector2 m_CurrentWind;
    Random m_Random;

    #endregion
}
