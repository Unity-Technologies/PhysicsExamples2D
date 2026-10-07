using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Turns a grid of small kinematic boxes, each one open in the middle, while tiny capsules are dropped into every one of them a few times a second, to stress-test how the simulation copes with a great many bodies being churned at once.
/// Every box is one kinematic Physics Pose with four Physics Area Polygon walls, and every capsule is one Physics Pose with a Physics Area Capsule, both instantiated from prefabs.
/// </summary>
/// <remarks>
/// A round of capsules is dropped into every tumbler every tenth of a second until the spawn count is reached, and no rounds are dropped while the world is paused.
/// Every setting rebuilds the scene, which removes the tumblers and capsules and starts the rounds again from the first.
/// </remarks>
public sealed class ManyTumblersContents : MonoBehaviour
{
    /// <summary>
    /// Removes every tumbler and capsule, lays the grid of tumblers out again and starts the rounds of capsules again from the first.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        m_CurrentSpawnCounter = 0;
        m_SpawnTime = 0f;

        SpawnTumblers();
    }

    /// <summary>
    /// Removes every tumbler and capsule.
    /// </summary>
    public void Clear()
    {
        // Each tumbler and capsule is a root object of its own, so each one is removed individually.
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
    }

    /// <summary>
    /// How many rows of tumblers the grid has.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int rowCount
    {
        get => m_RowCount;
        set => m_RowCount = value;
    }

    /// <summary>
    /// How many columns of tumblers the grid has.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int columnCount
    {
        get => m_ColumnCount;
        set => m_ColumnCount = value;
    }

    /// <summary>
    /// How fast every tumbler turns, in degrees per second.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float angularVelocity
    {
        get => m_AngularVelocity;
        set => m_AngularVelocity = value;
    }

    /// <summary>
    /// How many rounds of capsules are dropped into the tumblers before the dropping stops.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int spawnCount
    {
        get => m_SpawnCount;
        set => m_SpawnCount = value;
    }

    private void Start() => Rebuild();

    private void Update()
    {
        if (m_Spawned.Count == 0 || PhysicsWorld.defaultWorld.paused)
            return;

        if (m_CurrentSpawnCounter >= m_SpawnCount)
            return;

        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0f)
            return;

        m_SpawnTime = SpawnPeriod;
        m_CurrentSpawnCounter++;

        SpawnDebris();
    }

    // Lays out the grid of tumblers, each one left inactive until its turning speed is set so its body is built with it.
    private void SpawnTumblers()
    {
        if (m_TumblerPrefab == null)
            return;

        var bodyDefinition = m_TumblerPrefab.GetComponent<PhysicsPose>().definition;
        bodyDefinition.angularVelocity = m_AngularVelocity;

        var x = -4f * m_ColumnCount;

        for (var i = 0; i < m_ColumnCount; ++i, x += 8f)
        {
            var y = -4f * m_RowCount;

            for (var j = 0; j < m_RowCount; ++j, y += 8f)
            {
                var spawned = Instantiate(m_TumblerPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                spawned.GetComponent<PhysicsPose>().definition = bodyDefinition;
                spawned.SetActive(true);
                m_Spawned.Add(spawned);
            }
        }
    }

    // Drops one capsule into the middle of every tumbler.
    private void SpawnDebris()
    {
        if (m_DebrisPrefab == null)
            return;

        var x = -4f * m_ColumnCount;

        for (var i = 0; i < m_ColumnCount; ++i, x += 8f)
        {
            var y = -4f * m_RowCount;

            for (var j = 0; j < m_RowCount; ++j, y += 8f)
                m_Spawned.Add(Instantiate(m_DebrisPrefab, new Vector3(x, y, 0f), Quaternion.identity));
        }
    }

    #region Internal

    // How long to wait between one round of capsules and the next, in seconds.
    const float SpawnPeriod = 0.1f;

    [SerializeField] GameObject m_TumblerPrefab;
    [SerializeField] GameObject m_DebrisPrefab;
    [SerializeField, Range(1, 50)] int m_RowCount = 15;
    [SerializeField, Range(1, 50)] int m_ColumnCount = 15;
    [SerializeField, Range(-90f, 90f)] float m_AngularVelocity = 45f;
    [SerializeField, Range(1, 10)] int m_SpawnCount = 10;

    readonly List<GameObject> m_Spawned = new();
    float m_SpawnTime;
    int m_CurrentSpawnCounter;

    #endregion
}
