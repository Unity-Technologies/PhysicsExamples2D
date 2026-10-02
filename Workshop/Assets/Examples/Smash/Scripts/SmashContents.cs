using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Fires one large dense box across a room with no gravity into a field of thousands of small sleeping boxes, to stress-test how the simulation copes with a huge number of bodies waking at once.
/// Every box is one Physics Pose with a Physics Area Polygon, instantiated from a prefab, and the room's four walls are Physics Area Polygons on a single static Physics Pose.
/// </summary>
/// <remarks>
/// The small boxes start asleep, so nothing moves until the large box reaches them.
/// Every setting rebuilds the scene, so the large box always starts again from the left wall and the field is laid out again from scratch.
/// Gravity is turned off while this example is loaded and the world's own gravity is put back when it unloads.
/// </remarks>
public sealed class SmashContents : MonoBehaviour
{
    /// <summary>
    /// Destroys every box and lays the field out again, with the large box fired from its starting point at the current speed.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        SpawnLargeBox();
        SpawnField();
    }

    /// <summary>
    /// Destroys the large box and every small box, leaving the walls alone.
    /// </summary>
    public void Clear()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
    }

    /// <summary>
    /// How fast the large box is moving when it starts, in meters per second.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float speed
    {
        get => m_Speed;
        set => m_Speed = value;
    }

    /// <summary>
    /// The density of the large box, which sets how heavy it is for its size.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float density
    {
        get => m_Density;
        set => m_Density = value;
    }

    /// <summary>
    /// How bouncy the large box and every small box are, from zero for none to one for fully elastic.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float bounciness
    {
        get => m_Bounciness;
        set => m_Bounciness = value;
    }

    /// <summary>
    /// The gap between neighboring small boxes in the field, in meters.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float spacing
    {
        get => m_Spacing;
        set => m_Spacing = value;
    }

    /// <summary>
    /// How fast two objects must approach before the contact counts as a collision, in meters per second, for the large box and every small box.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float collisionThreshold
    {
        get => m_CollisionThreshold;
        set => m_CollisionThreshold = value;
    }

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;
        m_WorldGravity = world.gravity;
        world.gravity = Vector2.zero;
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay off in whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;
    }

    private void Start() => Rebuild();

    // Fires the large box from the left of the room, spinning slowly, with the current density and bounciness, left inactive until those are set so its body is built with them.
    private void SpawnLargeBox()
    {
        if (m_LargeBoxPrefab == null)
            return;

        var spawned = Instantiate(m_LargeBoxPrefab, LargeBoxPosition, Quaternion.identity);

        var pose = spawned.GetComponent<PhysicsPose>();
        var bodyDefinition = pose.definition;
        bodyDefinition.linearVelocity = new Vector2(m_Speed, 0f);
        bodyDefinition.collisionThreshold = m_CollisionThreshold;
        pose.definition = bodyDefinition;

        var area = spawned.GetComponent<PhysicsArea>();
        var shapeDefinition = area.definition;
        shapeDefinition.density = m_Density;
        SetBounciness(ref shapeDefinition);
        area.definition = shapeDefinition;

        spawned.SetActive(true);
        m_Spawned.Add(spawned);
    }

    // Lays out the grid of small boxes, centered on the middle of the room, each one asleep until something hits it, and left inactive until its settings are written so its body is built with them.
    private void SpawnField()
    {
        if (m_SmallBoxPrefab == null)
            return;

        var poseDefinition = m_SmallBoxPrefab.GetComponent<PhysicsPose>().definition;
        poseDefinition.collisionThreshold = m_CollisionThreshold;

        var shapeDefinition = m_SmallBoxPrefab.GetComponent<PhysicsArea>().definition;
        SetBounciness(ref shapeDefinition);

        var boxSpacing = BoxDimension + m_Spacing;

        for (var i = 0; i < Columns; ++i)
        {
            for (var j = 0; j < Rows; ++j)
            {
                var position = new Vector3((i - (Columns - 1) / 2f) * boxSpacing, (j - (Rows - 1) / 2f) * boxSpacing, 0f);
                var spawned = Instantiate(m_SmallBoxPrefab, position, Quaternion.identity);

                spawned.GetComponent<PhysicsPose>().definition = poseDefinition;
                spawned.GetComponent<PhysicsArea>().definition = shapeDefinition;

                spawned.SetActive(true);
                m_Spawned.Add(spawned);
            }
        }
    }

    // Writes the current bounciness onto a shape definition.
    private void SetBounciness(ref PhysicsShapeDefinition shapeDefinition)
    {
        var surfaceMaterial = shapeDefinition.surfaceMaterial;
        surfaceMaterial.bounciness = m_Bounciness;
        shapeDefinition.surfaceMaterial = surfaceMaterial;
    }

    #region Internal

    // How many columns and rows of small boxes the field has, and the width of each box.
    const int Columns = 100;
    const int Rows = 60;
    const float BoxDimension = 0.4f;

    // Where the large box starts.
    static readonly Vector3 LargeBoxPosition = new(-90f, 0f, 0f);

    [SerializeField] GameObject m_LargeBoxPrefab;
    [SerializeField] GameObject m_SmallBoxPrefab;
    [SerializeField, Range(0f, 100f)] float m_Speed = 60f;
    [SerializeField, Range(1f, 100f)] float m_Density = 5f;
    [SerializeField, Range(0f, 1f)] float m_Bounciness;
    [SerializeField, Range(0f, 0.5f)] float m_Spacing;
    [SerializeField, Range(0f, 1f)] float m_CollisionThreshold = 0.5f;

    readonly List<GameObject> m_Spawned = new();
    Vector2 m_WorldGravity;

    #endregion
}
