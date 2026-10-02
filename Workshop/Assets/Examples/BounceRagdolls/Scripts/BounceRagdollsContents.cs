using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Drops ragdolls one at a time into a closed room of very bouncy walls and bumpers while gravity swings around the room, to check that very fast continuous collision stays stable.
/// Every ragdoll is the shared ragdoll prefab of Physics Pose, Physics Area and Physics Constraint Hinge components, and the room is a Physics Area Contour outline with five Physics Area Circle bumpers.
/// </summary>
/// <remarks>
/// Gravity is rotated a little every simulation step, so it sweeps in a slow loop and keeps flinging the ragdolls between the walls.
/// The world's own gravity is put back when the example unloads, and an orange line from the middle of the room shows where gravity currently points.
/// </remarks>
public sealed class BounceRagdollsContents : MonoBehaviour
{
    /// <summary>
    /// Destroys every ragdoll and starts dropping them again from the first one.
    /// </summary>
    public void Rebuild()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
        m_UpdateTime = m_UpdatePeriod;
        m_Time = 0f;
    }

    /// <summary>
    /// How long to wait between dropping one ragdoll and the next, in seconds.
    /// Changing this does not restart the drop until <see cref="Rebuild"/> is called.
    /// </summary>
    public float updatePeriod
    {
        get => m_UpdatePeriod;
        set => m_UpdatePeriod = value;
    }

    /// <summary>
    /// How many ragdolls are dropped in total before the drop stops.
    /// Changing this does not restart the drop until <see cref="Rebuild"/> is called.
    /// </summary>
    public int ragdollCount
    {
        get => m_MaxRagdollCount;
        set => m_MaxRagdollCount = value;
    }

    /// <summary>
    /// How strong the swinging gravity is, as a multiple of one meter per second squared.
    /// </summary>
    public float gravityScale
    {
        get => m_GravityScale;
        set
        {
            m_GravityScale = value;
            m_UpdateTime = m_UpdatePeriod;
        }
    }

    private void OnEnable()
    {
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_Random = new Random(RandomSeed);
        m_UpdateTime = m_UpdatePeriod;

        PhysicsEvents.PreSimulate += OnPreSimulate;
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay swinging in whichever example is loaded next.
    private void OnDisable()
    {
        PhysicsEvents.PreSimulate -= OnPreSimulate;

        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_OldGravity;
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        var segment = new SegmentGeometry { point1 = Vector2.zero, point2 = world.gravity * 3f };
        if (segment.isValid)
            world.DrawGeometry(segment, PhysicsTransform.identity, Color.orangeRed);
    }

    // Swings gravity around the room, then drops the next ragdoll once the update period has passed.
    private void OnPreSimulate(PhysicsWorld world, float timeStep)
    {
        if (!world.isDefaultWorld)
            return;

        m_Time += timeStep;

        var rotation1 = PhysicsRotate.FromRadians(m_Time * 0.5f);
        var rotation2 = PhysicsRotate.FromRadians(m_Time);
        world.gravity = new Vector2(rotation1.direction.x, rotation2.direction.y) * m_GravityScale;

        m_UpdateTime += timeStep;
        if (m_UpdateTime < m_UpdatePeriod)
            return;

        m_UpdateTime = 0f;

        if (m_Spawned.Count >= m_MaxRagdollCount || m_RagdollPrefab == null)
            return;

        SpawnRagdoll();
    }

    // Drops one ragdoll near the middle of the room, built at the ragdoll scale with its joints tuned for the bounce.
    private void SpawnRagdoll()
    {
        var position = new Vector3(m_Random.NextFloat(-2f, 2f), m_Random.NextFloat(-2f, 0f), 0f);
        var spawned = Instantiate(m_RagdollPrefab, position, Quaternion.identity);

        // A doll's limbs rest against each other, so its shapes share a negative group, which stops them touching and fighting the hinges.
        foreach (var area in spawned.GetComponentsInChildren<PhysicsArea>(true))
        {
            var definition = area.definition;
            var contactFilter = definition.contactFilter;
            contactFilter.groupIndex = GroupIndex;
            definition.contactFilter = contactFilter;
            area.definition = definition;

            var transformation = area.transformation;
            transformation.scale = RagdollScale;
            area.transformation = transformation;
        }

        // Every bone is placed and every joint anchored at the ragdoll scale while the doll is still inactive, so it is created at that size rather than resized afterwards.
        foreach (var pose in spawned.GetComponentsInChildren<PhysicsPose>(true))
        {
            pose.transform.localPosition *= RagdollScale;

            var hinge = pose.GetComponent<PhysicsConstraintHinge>();
            if (hinge == null)
                continue;

            var definition = hinge.definition;
            definition.localAnchorA = new PhysicsTransform(definition.localAnchorA.position * RagdollScale, definition.localAnchorA.rotation);
            definition.localAnchorB = new PhysicsTransform(definition.localAnchorB.position * RagdollScale, definition.localAnchorB.rotation);
            definition.springFrequency = JointFrequency;
            definition.springDamping = JointDamping;
            definition.maxMotorTorque = JointFriction * RagdollScale;
            hinge.definition = definition;
        }

        spawned.SetActive(true);
        m_Spawned.Add(spawned);
    }

    #region Internal

    // Every ragdoll's size, how stiff and damped its joints are, how much friction its joints have, and the group its shapes share.
    const float RagdollScale = 1.75f;
    const float JointFrequency = 1f;
    const float JointDamping = 0.1f;
    const float JointFriction = 0f;
    const int GroupIndex = -1;

    // The seed the random drop positions start from.
    const uint RandomSeed = 0x9E3779B9;

    [SerializeField] GameObject m_RagdollPrefab;
    [SerializeField, Range(0.1f, 5f)] float m_UpdatePeriod = 0.1f;
    [SerializeField, Range(10, 250)] int m_MaxRagdollCount = 50;
    [SerializeField, Range(1f, 5f)] float m_GravityScale = 5f;

    readonly List<GameObject> m_Spawned = new();
    Random m_Random;
    Vector2 m_OldGravity;
    float m_Time;
    float m_UpdateTime;

    #endregion
}
