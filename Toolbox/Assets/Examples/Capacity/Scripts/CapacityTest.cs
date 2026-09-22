using System;
using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Finds roughly how many bodies this device can simulate, by spawning more of them until a simulation step costs longer than a set time.
/// The objects are ordinary Physics Pose and Physics Area components, instantiated from one prefab per shape, so the number reached is what the components sustain on this machine.
/// </summary>
/// <remarks>
/// The result is a worst case rather than a guide to a real scene, because everything spawned ends up piled in contact with everything around it.
/// How many worker threads are available moves the number more than anything else does.
/// </remarks>
public sealed class CapacityTest : MonoBehaviour
{
    /// <summary>
    /// The shape every spawned object is given.
    /// </summary>
    public enum ShapeType
    {
        /// <summary>
        /// A circle of half a meter radius.
        /// </summary>
        Circle,

        /// <summary>
        /// A one meter box.
        /// </summary>
        Polygon,

        /// <summary>
        /// A capsule one meter long and half a meter across.
        /// </summary>
        Capsule
    }

    /// <summary>
    /// Raised after each simulation step, once the counts and the step time have been brought up to date.
    /// </summary>
    public event Action sampled;

    /// <summary>
    /// Destroys everything spawned so far and starts the test again from an empty world.
    /// </summary>
    public void Restart()
    {
        Clear();

        m_SimulationCounter = 0;
        m_LimitReachedCount = 0;
        m_SpawnOffset = 0f;
        m_SimulationStep = 0f;

        finished = false;

        sampled?.Invoke();
    }

    /// <summary>
    /// The shape every object spawned from now on is given.
    /// Changing this does not affect what has already been spawned until <see cref="Restart"/> is called.
    /// </summary>
    public ShapeType shapeType
    {
        get => m_ShapeType;
        set => m_ShapeType = value;
    }

    /// <summary>
    /// How long a single simulation step may take, in milliseconds, before the test stops spawning.
    /// </summary>
    public int simulationLimit
    {
        get => m_SimulationLimit;
        set => m_SimulationLimit = value;
    }

    /// <summary>
    /// Whether the test has stopped because steps have been over the limit for long enough to count as settled there.
    /// A brief spike does not finish the test.
    /// </summary>
    public bool finished { get; private set; }

    /// <summary>
    /// How long the most recent simulation step took, in milliseconds.
    /// </summary>
    public float simulationStep => m_SimulationStep;

    /// <summary>
    /// How many bodies the world holds.
    /// </summary>
    public int bodyCount => m_BodyCount;

    /// <summary>
    /// How many shapes the world holds.
    /// </summary>
    public int shapeCount => m_ShapeCount;

    /// <summary>
    /// How many contacts the world is solving.
    /// </summary>
    public int contactCount => m_ContactCount;

    private void OnEnable() => PhysicsEvents.PostSimulate += OnPostSimulate;

    private void OnDisable()
    {
        PhysicsEvents.PostSimulate -= OnPostSimulate;

        Clear();
    }

    // Reads the cost of the step that just ran and, while that cost is still under the limit, adds another batch of objects.
    private void OnPostSimulate(PhysicsWorld physicsWorld, float timeStep)
    {
        // Only the world the example runs in says anything about this device's capacity, and a paused world reports a step it never took.
        if (finished || !physicsWorld.isDefaultWorld || physicsWorld.paused)
            return;

        m_SimulationStep = physicsWorld.profile.simulationStep;

        var counters = physicsWorld.counters;
        m_BodyCount = counters.bodyCount;
        m_ShapeCount = counters.shapeCount;
        m_ContactCount = counters.contactCount;

        // One slow step is noise, so the test only finishes once the steps have stayed over the limit long enough to be where the device has settled.
        if (m_SimulationStep > m_SimulationLimit)
        {
            if (++m_LimitReachedCount > LimitReachedCount)
            {
                finished = true;

                sampled?.Invoke();

                return;
            }
        }
        else
        {
            m_LimitReachedCount = 0;
        }

        // Spawning every step would pile objects on faster than the ones already falling have landed, so the measurement would lag well behind what is actually in the world.
        if (++m_SimulationCounter % SimulationSpawnPeriod == 0)
            Spawn();

        sampled?.Invoke();
    }

    // Drops a row of objects in from above, wide enough to cover the whole of the ground.
    // Each row starts half a meter along from the one before, so the rows do not stack into neat columns that would settle far more cheaply than a real pile.
    private void Spawn()
    {
        var prefab = m_ShapeType switch
        {
            ShapeType.Polygon => m_PolygonPrefab,
            ShapeType.Capsule => m_CapsulePrefab,
            _ => m_CirclePrefab
        };

        if (prefab == null)
            return;

        var position = new Vector3(-SpawnCount + m_SpawnOffset, SpawnHeight, 0f);

        m_Spawned.Capacity = Mathf.Max(m_Spawned.Capacity, m_Spawned.Count + SpawnCount);

        for (var n = 0; n < SpawnCount; ++n)
        {
            position.y += SpawnRise;

            m_Spawned.Add(Instantiate(prefab, position, Quaternion.identity));

            position.x += SpawnStride;
        }

        m_SpawnOffset = SpawnRise - m_SpawnOffset;
    }

    // Destroys every object the test has spawned, leaving the ground alone.
    private void Clear()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();

        m_BodyCount = m_ShapeCount = m_ContactCount = 0;
    }

    #region Internal

    // How many steps in a row must run over the limit before the test calls the device finished.
    const int LimitReachedCount = 60;

    // How many steps pass between one batch of objects and the next.
    const int SimulationSpawnPeriod = 0x1F;

    // The shape of a single batch: how many objects, how far above the ground they start, and how far apart they are placed across and up.
    const int SpawnCount = 200;
    const float SpawnHeight = 200f;
    const float SpawnStride = 2f;
    const float SpawnRise = 0.5f;

    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] ShapeType m_ShapeType = ShapeType.Circle;
    [SerializeField, Range(1, 50)] int m_SimulationLimit = 20;

    readonly List<GameObject> m_Spawned = new();
    float m_SpawnOffset;
    float m_SimulationStep;
    int m_SimulationCounter;
    int m_LimitReachedCount;
    int m_BodyCount;
    int m_ShapeCount;
    int m_ContactCount;

    #endregion
}
