using System;
using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Turns a ring-shaped drum with paddles on its inside around a point with a kinematic body, churning a grid of thousands of small circles dropped into it, to stress-test how the simulation copes with a great many bodies being agitated at once.
/// The drum is one kinematic Physics Pose with a Physics Area Polygon for every wall segment and paddle, and every circle is one Physics Pose with a Physics Area Circle, instantiated from a prefab.
/// </summary>
/// <remarks>
/// The drum is thirty-six wall segments around a full turn, with a paddle on every few of them, so the paddle spacing and length decide how many polygons it has and are why it is built from a script.
/// Changing the debris, the paddles or the friction rebuilds the scene and starts the drum from upright again, while the turning speed and gravity act on the running scene.
/// The world's own gravity is put back when the example unloads.
/// </remarks>
public sealed class WasherContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the drum and every circle, then builds them again with the current settings.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        SpawnDrum();
        SpawnDebris();
    }

    /// <summary>
    /// Destroys the drum and every circle.
    /// </summary>
    public void Clear()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
        m_Drum = null;
    }

    /// <summary>
    /// How fast the drum turns, in degrees per second, where a negative speed turns it clockwise.
    /// Changing this turns the running drum at the new speed without rebuilding the scene.
    /// </summary>
    public float motorSpeed
    {
        get => m_MotorSpeed;
        set
        {
            m_MotorSpeed = value;

            if (m_Drum != null && m_Drum.body.isValid)
            {
                var body = m_Drum.body;
                body.angularVelocity = m_MotorSpeed;
            }
        }
    }

    /// <summary>
    /// How many circles are dropped into the drum, rounded up to fill a square grid.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int debrisCount
    {
        get => m_DebrisCount;
        set => m_DebrisCount = value;
    }

    /// <summary>
    /// The friction of every circle.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float debrisFriction
    {
        get => m_DebrisFriction;
        set => m_DebrisFriction = value;
    }

    /// <summary>
    /// How many wall segments apart the paddles are, so a smaller number gives more paddles.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int paddleSpacing
    {
        get => m_PaddleSpacing;
        set => m_PaddleSpacing = value;
    }

    /// <summary>
    /// How far the paddles reach in toward the middle of the drum, from zero for the shortest to one for the longest.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float paddleScale
    {
        get => m_PaddleScale;
        set => m_PaddleScale = value;
    }

    /// <summary>
    /// How strong gravity is, as a multiple of the world's own gravity.
    /// Changing this scales gravity straight away without rebuilding the scene.
    /// </summary>
    public float gravityScale
    {
        get => m_GravityScale;
        set
        {
            m_GravityScale = value;

            var world = PhysicsWorld.defaultWorld;
            world.gravity = m_WorldGravity * m_GravityScale;
        }
    }

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;
        m_WorldGravity = world.gravity;
        world.gravity = m_WorldGravity * m_GravityScale;
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay scaled into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;
    }

    private void Start() => Rebuild();

    // Builds the drum from a wall segment for every step around a full turn, with a paddle on every few of them.
    // The drum is left inactive until all its polygons are added, so its body is built once with all of them.
    private void SpawnDrum()
    {
        if (m_DrumPrefab == null)
            return;

        var spawned = Instantiate(m_DrumPrefab, DrumPosition, Quaternion.identity);
        m_Drum = spawned.GetComponent<PhysicsPose>();

        var bodyDefinition = m_Drum.definition;
        bodyDefinition.angularVelocity = m_MotorSpeed;
        m_Drum.definition = bodyDefinition;

        var paddleInnerRadius = Mathf.Lerp(14f, 10f, m_PaddleScale);

        var angle = PhysicsMath.PI / 18f;
        var step = PhysicsRotate.FromRadians(angle);
        var overlap = PhysicsRotate.FromRadians(angle * 0.1f);
        var direction1 = Vector2.right;

        for (var n = 0; n < SegmentCount; ++n)
        {
            var direction2 = n == SegmentCount - 1 ? Vector2.right : step.RotateVector(direction1);

            // Each wall segment is tilted a little either way so neighboring segments overlap and leave no gap.
            var wall1 = overlap.InverseRotateVector(direction1);
            var wall2 = overlap.RotateVector(direction2);
            AddPolygon(spawned, wall1 * InnerRadius, wall1 * OuterRadius, wall2 * InnerRadius, wall2 * OuterRadius);

            if (n % m_PaddleSpacing == 0)
                AddPolygon(spawned, direction1 * paddleInnerRadius, direction1 * InnerRadius, direction2 * paddleInnerRadius, direction2 * InnerRadius);

            direction1 = direction2;
        }

        spawned.SetActive(true);
        m_Spawned.Add(spawned);
    }

    // Adds one polygon to the drum that is the outline around the four specified vertices, whatever order they are given in.
    private static void AddPolygon(GameObject drum, Vector2 vertex1, Vector2 vertex2, Vector2 vertex3, Vector2 vertex4)
    {
        var vertices = new[] { vertex1, vertex2, vertex3, vertex4 };

        var area = drum.AddComponent<PhysicsAreaPolygon>();
        area.geometry = PolygonGeometry.Create(vertices: vertices.AsSpan());
    }

    // Drops a square grid of small circles into the middle of the drum.
    private void SpawnDebris()
    {
        if (m_DebrisPrefab == null)
            return;

        var gridCount = Mathf.Sqrt(m_DebrisCount);
        var y = -1.1f * DebrisRadius * gridCount + DrumPosition.y;

        for (var i = 0; i < gridCount; ++i)
        {
            var x = -1.1f * DebrisRadius * gridCount;

            for (var j = 0; j < gridCount; ++j)
            {
                var spawned = Instantiate(m_DebrisPrefab, new Vector3(x, y, 0f), Quaternion.identity);

                var area = spawned.GetComponent<PhysicsArea>();
                var shapeDefinition = area.definition;
                var surfaceMaterial = shapeDefinition.surfaceMaterial;
                surfaceMaterial.friction = m_DebrisFriction;
                surfaceMaterial.bounciness = 0f;
                shapeDefinition.surfaceMaterial = surfaceMaterial;
                area.definition = shapeDefinition;

                spawned.SetActive(true);
                m_Spawned.Add(spawned);

                x += 2.1f * DebrisRadius;
            }

            y += 2.1f * DebrisRadius;
        }
    }

    #region Internal

    // How many wall segments make up the drum, the radii of the inside and outside of its wall, and where its middle is.
    const int SegmentCount = 36;
    const float InnerRadius = 16f;
    const float OuterRadius = 22f;
    static readonly Vector3 DrumPosition = new(0f, 10f, 0f);

    // The radius of each circle the drum is filled with.
    const float DebrisRadius = 0.15f;

    [SerializeField] GameObject m_DrumPrefab;
    [SerializeField] GameObject m_DebrisPrefab;
    [SerializeField, Range(-90f, 90f)] float m_MotorSpeed = -30f;
    [SerializeField, Range(1000, 3000)] int m_DebrisCount = 2500;
    [SerializeField, Range(0f, 1f)] float m_DebrisFriction = 0.6f;
    [SerializeField, Range(2, 18)] int m_PaddleSpacing = 4;
    [SerializeField, Range(0f, 1f)] float m_PaddleScale = 0.4f;
    [SerializeField, Range(0f, 2f)] float m_GravityScale = 1f;

    readonly List<GameObject> m_Spawned = new();
    PhysicsPose m_Drum;
    Vector2 m_WorldGravity;

    #endregion
}
