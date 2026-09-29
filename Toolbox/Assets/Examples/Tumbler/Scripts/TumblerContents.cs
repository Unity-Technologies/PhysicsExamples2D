using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Turns a square box slowly around its middle with a kinematic body while hundreds of debris pieces tumble around inside it, to stress-test how the simulation copes with a great many bodies being churned at once.
/// The box is one kinematic Physics Pose with four Physics Area Polygon walls, and every debris piece is one Physics Pose with a Physics Area, instantiated from a prefab and given its own random size.
/// </summary>
/// <remarks>
/// The debris is one shape type, or a mix of all four, and every piece starts at a random place inside the box.
/// Changing the shape type or the debris count rebuilds the scene and starts the box from upright again, while the box's turning speed and gravity act on the running scene.
/// The world's own gravity is put back when the example unloads.
/// </remarks>
public sealed class TumblerContents : MonoBehaviour
{
    /// <summary>
    /// The shape every debris piece uses.
    /// </summary>
    public enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Polygon = 2,
        Compound = 3,
        Random = 4
    }

    /// <summary>
    /// Destroys every debris piece, stands the box upright again and drops a new set of debris into it.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity * m_GravityScale;

        m_Random = new Random(RandomSeed);

        ResetTumbler();
        SpawnDebris();
    }

    /// <summary>
    /// Destroys every debris piece, leaving the box alone.
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
    /// The shape every debris piece uses, or a mix of all four.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    /// <summary>
    /// How fast the box turns, in degrees per second.
    /// Changing this turns the running box at the new speed without rebuilding the scene.
    /// </summary>
    public float angularVelocity
    {
        get => m_AngularVelocity;
        set
        {
            m_AngularVelocity = value;

            if (m_Tumbler != null && m_Tumbler.body.isValid)
            {
                var body = m_Tumbler.body;
                body.angularVelocity = m_AngularVelocity;
            }
        }
    }

    /// <summary>
    /// How many debris pieces are dropped into the box.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int debrisCount
    {
        get => m_DebrisCount;
        set => m_DebrisCount = value;
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
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay scaled into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;
    }

    private void Start() => Rebuild();

    // Stands the box upright and sets it turning at the current speed.
    private void ResetTumbler()
    {
        if (m_Tumbler == null || !m_Tumbler.body.isValid)
            return;

        var body = m_Tumbler.body;
        body.transform = new PhysicsTransform(Vector2.zero, PhysicsRotate.identity);
        body.angularVelocity = m_AngularVelocity;
    }

    // Drops the debris at random places inside the box, each one left inactive until its size is set so its shape is built at that size.
    private void SpawnDebris()
    {
        for (var i = 0; i < m_DebrisCount; ++i)
        {
            var position = new Vector3(m_Random.NextFloat(-18f, 18f), m_Random.NextFloat(-18f, 18f), 0f);

            // A mixed set cycles through the four shape types, one after the other.
            var resolvedType = m_ObjectType == ObjectType.Random ? (ObjectType)(i % 4) : m_ObjectType;

            var prefab = resolvedType switch
            {
                ObjectType.Circle => m_CirclePrefab,
                ObjectType.Capsule => m_CapsulePrefab,
                ObjectType.Polygon => m_PolygonPrefab,
                _ => m_CompoundPrefab
            };

            if (prefab == null)
                continue;

            var spawned = Instantiate(prefab, position, Quaternion.identity);
            SetGeometry(spawned, resolvedType);

            spawned.SetActive(true);
            m_Spawned.Add(spawned);
        }
    }

    // Sizes the debris piece, since every circle, capsule and polygon is a different size.
    // A compound piece keeps the two triangles authored on its prefab.
    private void SetGeometry(GameObject spawned, ObjectType resolvedType)
    {
        switch (resolvedType)
        {
            case ObjectType.Circle:
            {
                var area = spawned.GetComponent<PhysicsAreaCircle>();
                area.geometry = new CircleGeometry { center = Vector2.zero, radius = m_Random.NextFloat(0.25f, 0.45f) };

                return;
            }

            case ObjectType.Capsule:
            {
                var length = m_Random.NextFloat(0.25f, 1.0f);
                var area = spawned.GetComponent<PhysicsAreaCapsule>();
                area.geometry = new CapsuleGeometry
                {
                    center1 = new Vector2(0f, -0.3f * length),
                    center2 = new Vector2(0f, 0.3f * length),
                    radius = m_Random.NextFloat(0.25f, 0.3f)
                };

                return;
            }

            case ObjectType.Polygon:
            {
                var radius = 0.25f * m_Random.NextFloat(0f, 1.0f);
                var area = spawned.GetComponent<PhysicsAreaPolygon>();
                area.geometry = ToolboxUtility.CreateRandomPolygon(extent: 0.35f, radius: radius, ref m_Random);

                return;
            }
        }
    }

    #region Internal

    // The seed the debris positions and sizes start from, so every rebuild lays out the same debris.
    const uint RandomSeed = 0x32628473;

    [SerializeField] PhysicsPose m_Tumbler;
    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField] GameObject m_CompoundPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Polygon;
    [SerializeField, Range(-90f, 90f)] float m_AngularVelocity = 15f;
    [SerializeField, Range(1, 2000)] int m_DebrisCount = 1000;
    [SerializeField, Range(0f, 2f)] float m_GravityScale = 2f;

    readonly List<GameObject> m_Spawned = new();
    Random m_Random;
    Vector2 m_WorldGravity;

    #endregion
}
