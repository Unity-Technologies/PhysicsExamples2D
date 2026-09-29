using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Carries a pile of boxes and capsules along a static platform whose surface moves without the platform itself moving, the way a conveyor belt does.
/// The belt is one Physics Area Polygon with a tangent speed on its surface material, and the debris is instantiated from a prefab per shape type.
/// </summary>
/// <remarks>
/// Tangent speed is the speed the surface slides along itself, so anything resting on the belt is dragged by friction as if the surface were running, where a negative speed runs it the other way.
/// The belt can also be tilted, and changing either control clears whatever debris is on the belt and drops a fresh batch.
/// </remarks>
public sealed class ConveyorBeltContents : MonoBehaviour
{
    /// <summary>
    /// Destroys every piece of debris on the belt and drops a fresh batch.
    /// </summary>
    public void Rebuild()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
        Spawn();
    }

    /// <summary>
    /// Drops a handful of random boxes and capsules above the belt.
    /// </summary>
    public void Spawn()
    {
        if (m_BoxPrefab == null || m_CapsulePrefab == null)
            return;

        for (var n = 0; n < SpawnCount; ++n)
        {
            var position = new Vector3(m_Random.NextFloat(-5f, 5f), m_Random.NextFloat(9f, 20f), 0f);
            var rotation = Quaternion.Euler(0f, 0f, m_Random.NextFloat(-Mathf.PI, Mathf.PI) * Mathf.Rad2Deg);

            if (m_Random.NextBool())
            {
                var size = new Vector2(m_Random.NextFloat(0.05f, 1f), m_Random.NextFloat(0.05f, 1f));
                var radius = m_Random.NextFloat(0f, 0.2f);

                var spawned = Instantiate(m_BoxPrefab, position, rotation);
                spawned.GetComponent<PhysicsAreaPolygon>().geometry = PolygonGeometry.CreateBox(size, radius);
                spawned.SetActive(true);
                m_Spawned.Add(spawned);
            }
            else
            {
                var scale = m_Random.NextFloat(0.2f, 0.6f);
                var radius = m_Random.NextFloat(0.2f, 0.4f);

                var spawned = Instantiate(m_CapsulePrefab, position, rotation);
                spawned.GetComponent<PhysicsAreaCapsule>().geometry = new CapsuleGeometry { center1 = Vector2.left * scale, center2 = Vector2.right * scale, radius = radius };
                spawned.SetActive(true);
                m_Spawned.Add(spawned);
            }
        }
    }

    /// <summary>
    /// How fast the belt's surface runs, in meters per second, where a negative speed runs it the other way.
    /// </summary>
    public float conveyorSpeed
    {
        get => m_ConveyorSpeed;
        set
        {
            m_ConveyorSpeed = value;

            if (m_Belt == null)
                return;

            var definition = m_Belt.definition;
            var surfaceMaterial = definition.surfaceMaterial;
            surfaceMaterial.tangentSpeed = m_ConveyorSpeed;
            definition.surfaceMaterial = surfaceMaterial;
            m_Belt.definition = definition;

            m_Belt.ApplyDefinition();
        }
    }

    /// <summary>
    /// How far the belt is tilted, in degrees, where a positive angle raises its right end.
    /// </summary>
    public float conveyorAngle
    {
        get => m_ConveyorAngle;
        set
        {
            m_ConveyorAngle = value;

            if (m_Belt == null)
                return;

            var body = m_Belt.pose.body;

            if (body.isValid)
                body.rotation = PhysicsRotate.FromDegrees(m_ConveyorAngle);
        }
    }

    private void Awake() => m_Random = new Random(RandomSeed);

    private void Start() => Spawn();

    #region Internal

    const int SpawnCount = 10;
    const uint RandomSeed = 0x32628473;

    [SerializeField] PhysicsAreaPolygon m_Belt;
    [SerializeField] GameObject m_BoxPrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField, Range(-30f, 30f)] float m_ConveyorSpeed = 3f;
    [SerializeField, Range(-25f, 25f)] float m_ConveyorAngle;

    readonly List<GameObject> m_Spawned = new();
    Random m_Random;

    #endregion
}
