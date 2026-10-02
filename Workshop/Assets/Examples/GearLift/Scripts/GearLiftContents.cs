using Unity.U2D.Physics;
using UnityEngine;

using Random = Unity.Mathematics.Random;

/// <summary>
/// Fills the stepped ground with a heap of small random polygons, and drives the gear that turns the follower gear, which winds the chain that raises the door.
/// The gears, chain and door are authored in the scene as Physics Pose, Physics Area and Physics Constraint components; only the heap is created here, because every polygon is a different random outline.
/// </summary>
/// <remarks>
/// The motor settings act on the drive gear where it stands, without rebuilding anything.
/// </remarks>
public sealed class GearLiftContents : MonoBehaviour
{
    /// <summary>
    /// Whether the drive gear's motor is turning it.
    /// </summary>
    public bool useMotor
    {
        get => m_UseMotor;
        set
        {
            m_UseMotor = value;
            UpdateDriveGear();
        }
    }

    /// <summary>
    /// The speed the drive gear's motor turns it at, in degrees per second.
    /// </summary>
    public float motorSpeed
    {
        get => m_MotorSpeed;
        set
        {
            m_MotorSpeed = value;
            UpdateDriveGear();
        }
    }

    /// <summary>
    /// The most torque the drive gear's motor can apply.
    /// </summary>
    public float maxMotorTorque
    {
        get => m_MaxMotorTorque;
        set
        {
            m_MaxMotorTorque = value;
            UpdateDriveGear();
        }
    }

    private void Start()
    {
        UpdateDriveGear();
        SpawnHeap();
    }

    // Creates the heap as rows of polygons, each row a little higher than the one below and running right to left.
    private void SpawnHeap()
    {
        if (m_PolygonPrefab == null)
            return;

        var random = new Random(RandomSeed);
        var y = HeapBottom;

        for (var row = 0; row < HeapRows; ++row)
        {
            var x = HeapRight;

            for (var column = 0; column < HeapColumns; ++column)
            {
                var spawned = Instantiate(m_PolygonPrefab, new Vector3(x, y, 0f), Quaternion.identity);

                // The radius is drawn before the outline, which keeps the sequence of random outlines the same as the Sandbox's.
                var radius = random.NextFloat(0.02f, 0.03f);
                spawned.GetComponent<PhysicsAreaPolygon>().geometry = WorkshopUtility.CreateRandomPolygon(extent: PolygonExtent, radius: radius, ref random);
                spawned.SetActive(true);

                x -= HeapSpacing;
            }

            y += HeapSpacing;
        }
    }

    // Writes the current motor settings onto the drive gear's hinge, which wakes the gears so they respond straight away.
    private void UpdateDriveGear()
    {
        if (m_DriveGear == null)
            return;

        var definition = m_DriveGear.definition;
        definition.enableMotor = m_UseMotor;
        definition.motorSpeed = m_MotorSpeed;
        definition.maxMotorTorque = m_MaxMotorTorque;
        m_DriveGear.definition = definition;

        m_DriveGear.ApplyDefinition();
    }

    #region Internal

    // The heap: how many polygons across and up, where its bottom-right corner sits, how far apart the polygons start, and how large each one is.
    const int HeapColumns = 25;
    const int HeapRows = 20;
    const float HeapRight = -6.8f + 5.5f;
    const float HeapBottom = 2.5f;
    const float HeapSpacing = 0.2f;
    const float PolygonExtent = 0.14f;

    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField] PhysicsConstraintHinge m_DriveGear;
    [SerializeField] bool m_UseMotor;
    [SerializeField, Range(-100f, 100f)] float m_MotorSpeed = -100f;
    [SerializeField, Range(0f, 100f)] float m_MaxMotorTorque = 100f;

    #endregion
}
