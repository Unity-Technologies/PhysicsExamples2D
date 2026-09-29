using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Stacks a tower of shapes on the ground, so how stable it stays can be checked against the world's contact and gravity settings.
/// Every shape in the tower is one Physics Pose with a Physics Area, instantiated from the prefab for the chosen shape type.
/// </summary>
/// <remarks>
/// Contact frequency, damping and speed all belong to the world rather than to this scene, so their sliders change the world directly, and every one of the three is put back to what it was when the example unloads, alongside gravity.
/// </remarks>
public sealed class ShapeStackContents : MonoBehaviour
{
    /// <summary>
    /// The shapes a tower can be built from.
    /// </summary>
    public enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Box = 2,
        Mix = 3
    }

    /// <summary>
    /// Destroys the tower and builds it again with the current shape type and height.
    /// </summary>
    public void Rebuild()
    {
        for (var i = 0; i < m_Spawned.Count; ++i)
        {
            if (m_Spawned[i] != null)
                Destroy(m_Spawned[i]);
        }

        m_Spawned.Clear();

        for (var i = 0; i < m_StackHeight; ++i)
        {
            var prefab = m_ObjectType == ObjectType.Mix ? PrefabForMix(i) : PrefabForType(m_ObjectType);

            if (prefab == null)
                continue;

            var spawned = Instantiate(prefab, new Vector3(0f, StackBaseHeight + i * StackSpacing, 0f), Quaternion.identity);
            spawned.SetActive(true);
            m_Spawned.Add(spawned);
        }
    }

    /// <summary>
    /// The shape the tower is built from.
    /// Changing this does not rebuild the tower until <see cref="Rebuild"/> is called.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    /// <summary>
    /// How many shapes are stacked.
    /// Changing this does not rebuild the tower until <see cref="Rebuild"/> is called.
    /// </summary>
    public int stackHeight
    {
        get => m_StackHeight;
        set => m_StackHeight = value;
    }

    /// <summary>
    /// The world's contact stiffness, in cycles per second.
    /// </summary>
    public float contactFrequency
    {
        get => PhysicsWorld.defaultWorld.contactFrequency;
        set
        {
            var world = PhysicsWorld.defaultWorld;
            world.contactFrequency = value;
        }
    }

    /// <summary>
    /// The world's contact bounciness, with one being critical damping.
    /// </summary>
    public float contactDamping
    {
        get => PhysicsWorld.defaultWorld.contactDamping;
        set
        {
            var world = PhysicsWorld.defaultWorld;
            world.contactDamping = value;
        }
    }

    /// <summary>
    /// The speed the world uses to solve overlaps, in meters per second.
    /// </summary>
    public float contactSpeed
    {
        get => PhysicsWorld.defaultWorld.contactSpeed;
        set
        {
            var world = PhysicsWorld.defaultWorld;
            world.contactSpeed = value;
        }
    }

    /// <summary>
    /// How much harder than normal gravity pulls on the tower.
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
        m_WorldContactFrequency = world.contactFrequency;
        m_WorldContactDamping = world.contactDamping;
        m_WorldContactSpeed = world.contactSpeed;
        m_WorldGravity = world.gravity;
    }

    // The contact and gravity settings belong to the world rather than to this scene, so they would otherwise stay changed into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.contactFrequency = m_WorldContactFrequency;
        world.contactDamping = m_WorldContactDamping;
        world.contactSpeed = m_WorldContactSpeed;
        world.gravity = m_WorldGravity;
    }

    private void Start() => Rebuild();

    // Returns the prefab for one of the fixed shape types.
    private GameObject PrefabForType(ObjectType objectType) => objectType switch
    {
        ObjectType.Circle => m_CirclePrefab,
        ObjectType.Capsule => m_CapsulePrefab,
        _ => m_BoxPrefab
    };

    // Returns the shape for one level of a mixed tower: circle, capsule, box, then a gap, repeating.
    private GameObject PrefabForMix(int level) => (level % 4) switch
    {
        0 => m_CirclePrefab,
        1 => m_CapsulePrefab,
        2 => m_BoxPrefab,
        _ => null
    };

    #region Internal

    // Where the bottom shape sits and how far apart each level of the tower is.
    const float StackBaseHeight = 0.55f;
    const float StackSpacing = 1.2f;

    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_BoxPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Circle;
    [SerializeField, Range(2, 20)] int m_StackHeight = 8;
    [SerializeField, Range(0f, 20f)] float m_GravityScale = 1f;

    readonly List<GameObject> m_Spawned = new();
    float m_WorldContactFrequency;
    float m_WorldContactDamping;
    float m_WorldContactSpeed;
    Vector2 m_WorldGravity;

    #endregion
}
