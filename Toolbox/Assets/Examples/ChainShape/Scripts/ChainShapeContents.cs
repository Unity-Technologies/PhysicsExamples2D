using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Slides a stream of objects down a long uneven ground made from one closed contour of segments, which join without any bump where one segment meets the next.
/// Every object is one Physics Pose with a Physics Area, instantiated from the prefab for the chosen shape type.
/// </summary>
/// <remarks>
/// A separate segment per edge would catch a sliding shape on the corner where two edges meet, a "ghost" collision, which the contour's joined segments never produce.
/// Objects are spawned one at a time at the top left until the chosen count is reached, and the interval shortens as gravity is scaled up so the stream stays as dense.
/// </remarks>
public sealed class ChainShapeContents : MonoBehaviour
{
    /// <summary>
    /// The shape every spawned body uses.
    /// </summary>
    public enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Box = 2
    }

    /// <summary>
    /// Destroys every spawned object and starts the stream again from the first one.
    /// </summary>
    public void Rebuild()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
        m_SpawnTime = 0f;
    }

    /// <summary>
    /// The shape every spawned body uses.
    /// Changing this does not affect objects already spawned until <see cref="Rebuild"/> is called.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    /// <summary>
    /// How many objects are spawned in total.
    /// </summary>
    public int objectCount
    {
        get => m_ObjectCount;
        set => m_ObjectCount = value;
    }

    /// <summary>
    /// How much harder than normal gravity pulls on each spawned body.
    /// Changing this does not affect objects already spawned until <see cref="Rebuild"/> is called.
    /// </summary>
    public float gravityScale
    {
        get => m_GravityScale;
        set => m_GravityScale = value;
    }

    /// <summary>
    /// How far a spawned body has to move in one step, relative to its own shape size, before continuous collision detection kicks in to stop it passing through something.
    /// Changing this does not affect objects already spawned until <see cref="Rebuild"/> is called.
    /// </summary>
    public float collisionThreshold
    {
        get => m_CollisionThreshold;
        set => m_CollisionThreshold = value;
    }

    private void Update()
    {
        if (PhysicsWorld.defaultWorld.paused)
            return;

        if (m_Spawned.Count >= m_ObjectCount)
            return;

        m_SpawnTime -= Time.deltaTime;

        if (m_SpawnTime > 0f)
            return;

        // Stronger gravity gets an object down the slope sooner, so the wait shortens with it and the stream stays as dense as it was.
        m_SpawnTime = SpawnPeriod / Mathf.Sqrt(m_GravityScale);

        Spawn();
    }

    // Creates one object at the top left, already moving down the first slope.
    private void Spawn()
    {
        var prefab = m_ObjectType switch
        {
            ObjectType.Circle => m_CirclePrefab,
            ObjectType.Capsule => m_CapsulePrefab,
            _ => m_BoxPrefab
        };

        if (prefab == null)
            return;

        var spawned = Instantiate(prefab, StartPosition, Quaternion.identity);

        var pose = spawned.GetComponent<PhysicsPose>();
        var definition = pose.definition;
        definition.gravityScale = m_GravityScale;
        definition.collisionThreshold = m_CollisionThreshold;
        definition.linearVelocity = StartLinearVelocity;
        pose.definition = definition;

        spawned.SetActive(true);
        m_Spawned.Add(spawned);
    }

    #region Internal

    // Where each object starts and how fast it is already moving, and how long to wait between spawns before the gravity scale is taken into account.
    static readonly Vector3 StartPosition = new(-55f, 13.5f, 0f);
    static readonly Vector2 StartLinearVelocity = new(2f, -1f);
    const float SpawnPeriod = 1.75f;

    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_BoxPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Box;
    [SerializeField, Range(1, 100)] int m_ObjectCount = 100;
    [SerializeField, Range(1f, 20f)] float m_GravityScale = 10f;
    [SerializeField, Range(0f, 1f)] float m_CollisionThreshold;

    readonly List<GameObject> m_Spawned = new();
    float m_SpawnTime;

    #endregion
}
