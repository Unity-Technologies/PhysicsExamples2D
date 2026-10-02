using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Slides a stream of shapes down a zigzag of ramps, with each shape's friction a little higher than the one before it, from none at all to full friction.
/// Every shape is one Physics Pose with a Physics Area, instantiated from the prefab for the chosen shape type and given its own friction.
/// </summary>
/// <remarks>
/// The first shapes slide all the way down, while the later ones slow and stop on the ramps sooner the higher their friction is.
/// Shapes are spawned one at a time at the top until all of them are out, and the interval shortens as gravity is scaled up so they stay as far apart.
/// </remarks>
public sealed class FrictionContents : MonoBehaviour
{
    /// <summary>
    /// The shape every spawned body uses.
    /// </summary>
    public enum ObjectType
    {
        Capsule = 0,
        Box = 1
    }

    /// <summary>
    /// Destroys every spawned shape and starts the stream again from the first one.
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
    /// Changing this does not affect shapes already spawned until <see cref="Rebuild"/> is called.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    /// <summary>
    /// How much harder than normal gravity pulls on each spawned body.
    /// Changing this does not affect shapes already spawned until <see cref="Rebuild"/> is called.
    /// </summary>
    public float gravityScale
    {
        get => m_GravityScale;
        set => m_GravityScale = value;
    }

    private void Update()
    {
        if (PhysicsWorld.defaultWorld.paused)
            return;

        if (m_Spawned.Count >= ObjectCount)
            return;

        m_SpawnTime -= Time.deltaTime;

        if (m_SpawnTime > 0f)
            return;

        // Stronger gravity gets a shape down the ramps sooner, so the wait shortens with it and the shapes stay as far apart as they were.
        m_SpawnTime = SpawnPeriod / Mathf.Sqrt(m_GravityScale);

        Spawn();
    }

    // Creates the next shape at the top, already moving down the first ramp, with its friction a step higher than the last one's.
    private void Spawn()
    {
        var prefab = m_ObjectType == ObjectType.Capsule ? m_CapsulePrefab : m_BoxPrefab;

        if (prefab == null)
            return;

        var spawned = Instantiate(prefab, StartPosition, Quaternion.identity);

        var pose = spawned.GetComponent<PhysicsPose>();
        var bodyDefinition = pose.definition;
        bodyDefinition.gravityScale = m_GravityScale;
        bodyDefinition.linearVelocity = StartLinearVelocity;
        pose.definition = bodyDefinition;

        var frictionStep = 1f / (ObjectCount > 1 ? ObjectCount - 1 : 1);

        var area = spawned.GetComponent<PhysicsArea>();
        var shapeDefinition = area.definition;
        var surfaceMaterial = shapeDefinition.surfaceMaterial;
        surfaceMaterial.friction = frictionStep * m_Spawned.Count;
        shapeDefinition.surfaceMaterial = surfaceMaterial;
        area.definition = shapeDefinition;

        spawned.SetActive(true);
        m_Spawned.Add(spawned);
    }

    #region Internal

    // How many shapes are spawned, where each one starts and how fast it is already moving, and how long to wait between them before the gravity scale is taken into account.
    const int ObjectCount = 10;
    static readonly Vector3 StartPosition = new(15f, 40f, 0f);
    static readonly Vector2 StartLinearVelocity = new(-1.5f, -0.5f);
    const float SpawnPeriod = 2f;

    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_BoxPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Capsule;
    [SerializeField, Range(1f, 10f)] float m_GravityScale = 5f;

    readonly List<GameObject> m_Spawned = new();
    float m_SpawnTime;

    #endregion
}
