using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

using Random = Unity.Mathematics.Random;

/// <summary>
/// Fills a walled table seen from above with a grid of shapes that have no gravity, each held to the ground by a relative joint that only limits how much force and torque it can resist.
/// That limit acts like friction against the table surface, so a shape slides to a stop and stops spinning rather than drifting forever.
/// </summary>
/// <remarks>
/// Every shape is one Physics Pose with a Physics Area, instantiated from a prefab for its type and held by a Physics Constraint Relative added when it is created.
/// The force and torque limits act on the shapes where they are, without rebuilding them.
/// </remarks>
public sealed class TopDownFrictionContents : MonoBehaviour
{
    /// <summary>
    /// Pushes every shape near the middle of the table outward with an explosion, and draws where it happened.
    /// </summary>
    public void Explode()
    {
        var world = PhysicsWorld.defaultWorld;

        world.Explode(new PhysicsWorld.ExplosionDefinition
        {
            position = ExplosionPosition,
            radius = ExplosionRadius,
            falloff = ExplosionFalloff,
            impulsePerLength = ExplosionImpulsePerLength
        });

        world.DrawCircle(ExplosionPosition, ExplosionRadius, Color.softRed, 2f / 60f);
    }

    /// <summary>
    /// The most force each joint can resist a shape sliding with, in newtons.
    /// </summary>
    public float maxForce
    {
        get => m_MaxForce;
        set
        {
            m_MaxForce = value;
            UpdateJoints();
        }
    }

    /// <summary>
    /// The most torque each joint can resist a shape spinning with.
    /// </summary>
    public float maxTorque
    {
        get => m_MaxTorque;
        set
        {
            m_MaxTorque = value;
            UpdateJoints();
        }
    }

    private void Start() => SpawnGrid();

    // Creates the grid row by row from the top, with each shape the next of the four types in turn.
    private void SpawnGrid()
    {
        if (m_Ground == null)
            return;

        var random = new Random(RandomSeed);
        var offset = GridStart;

        for (var i = 0; i < GridOrder; ++i)
        {
            for (var j = 0; j < GridOrder; ++j)
            {
                var shapeIndex = (GridOrder * i + j) % 4;
                var prefab = shapeIndex switch
                {
                    0 => m_CapsulePrefab,
                    1 => m_CirclePrefab,
                    2 => m_BoxPrefab,
                    _ => m_PolygonPrefab
                };

                if (prefab != null)
                {
                    var spawned = Instantiate(prefab, new Vector3(offset.x, offset.y, 0f), Quaternion.identity);

                    // The polygons are each a different random outline; the other types keep the geometry authored on their prefab.
                    if (shapeIndex == 3)
                        spawned.GetComponent<PhysicsAreaPolygon>().geometry = WorkshopUtility.CreateRandomPolygon(extent: 0.75f, radius: 0.1f, ref random);

                    AddJoint(spawned);
                    spawned.SetActive(true);
                }

                offset.x += 1f;
            }

            offset += new Vector2(-GridOrder, -1f);
        }
    }

    // Holds a shape to the ground with a relative joint that has no spring, so it never pulls the shape anywhere and only resists it moving and turning, up to its force and torque limits.
    private void AddJoint(GameObject spawned)
    {
        var joint = spawned.AddComponent<PhysicsConstraintRelative>();
        joint.source = PhysicsConstraint.PoseSource.Custom;
        joint.poseA = m_Ground;
        joint.poseB = spawned.GetComponent<PhysicsPose>();

        var definition = joint.definition;
        definition.autoAnchorA = false;
        definition.autoAnchorB = false;
        definition.collideConnected = true;
        definition.maxForce = m_MaxForce;
        definition.maxTorque = m_MaxTorque;
        definition.worldDrawing = false;
        joint.definition = definition;

        m_Joints.Add(joint);
    }

    // Writes the current force and torque limits onto every joint, which wakes the shapes so they respond straight away.
    private void UpdateJoints()
    {
        foreach (var joint in m_Joints)
        {
            if (joint == null)
                continue;

            var definition = joint.definition;
            definition.maxForce = m_MaxForce;
            definition.maxTorque = m_MaxTorque;
            joint.definition = definition;

            joint.ApplyDefinition();
        }
    }

    #region Internal

    // The grid: how many shapes across and down, and where its top-left shape sits.
    const int GridOrder = 10;
    static readonly Vector2 GridStart = new(-5f, 15f);

    // The explosion: where it is centered, how far it reaches, how quickly it weakens toward its edge, and how hard it pushes.
    static readonly Vector2 ExplosionPosition = Vector2.up * 10f;
    const float ExplosionRadius = 10f;
    const float ExplosionFalloff = 5f;
    const float ExplosionImpulsePerLength = 10f;

    const uint RandomSeed = 0x32628473;

    [SerializeField] PhysicsPose m_Ground;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_BoxPrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField, Range(0f, 10f)] float m_MaxForce = 10f;
    [SerializeField, Range(0f, 10f)] float m_MaxTorque = 10f;

    readonly List<PhysicsConstraintRelative> m_Joints = new();

    #endregion
}
