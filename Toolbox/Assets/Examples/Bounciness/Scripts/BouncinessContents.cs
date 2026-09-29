using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Drops a row of identical shapes onto a flat ground, with each shape's bounciness a little higher than the one to its left, from none at all to fully elastic.
/// Every shape is one Physics Pose with a Physics Area, instantiated from the prefab for the chosen shape type and given its own bounciness.
/// </summary>
/// <remarks>
/// The shapes cannot rotate, so each one bounces straight up and down and the row stays easy to compare.
/// Gravity can be scaled up to drop them harder, and the world's own gravity is put back when the example unloads.
/// </remarks>
public sealed class BouncinessContents : MonoBehaviour
{
    /// <summary>
    /// The shape every body in the row uses.
    /// </summary>
    public enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Box = 2
    }

    /// <summary>
    /// Destroys the current row and drops a new one of the current shape type.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        var prefab = m_ObjectType switch
        {
            ObjectType.Circle => m_CirclePrefab,
            ObjectType.Capsule => m_CapsulePrefab,
            _ => m_BoxPrefab
        };

        if (prefab == null)
            return;

        // The row is centered on the middle, with the bounciness rising by an equal step from the first shape to the last.
        var bounciness = 0f;
        var bouncinessStep = 1f / (ShapeCount > 1 ? ShapeCount - 1 : 1);
        var x = -1f * (ShapeCount - 1);

        for (var i = 0; i < ShapeCount; ++i)
        {
            var spawned = Instantiate(prefab, new Vector3(x, DropHeight, 0f), Quaternion.identity);

            var area = spawned.GetComponent<PhysicsArea>();
            var definition = area.definition;
            var surfaceMaterial = definition.surfaceMaterial;
            surfaceMaterial.bounciness = bounciness;
            definition.surfaceMaterial = surfaceMaterial;
            area.definition = definition;

            spawned.SetActive(true);
            m_Spawned.Add(spawned);

            bounciness += bouncinessStep;
            x += ShapeSpacing;
        }
    }

    /// <summary>
    /// Destroys every shape in the row, leaving the ground alone.
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
    /// The shape every body in the row uses.
    /// Changing this does not rebuild the row until <see cref="Rebuild"/> is called.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    /// <summary>
    /// How much harder than normal gravity pulls on the row.
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
        world.gravity = m_WorldGravity * m_GravityScale;
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay scaled into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;
    }

    private void Start() => Rebuild();

    #region Internal

    // How many shapes are in the row, how far apart they are, and how high they are dropped from.
    const int ShapeCount = 40;
    const float ShapeSpacing = 2f;
    const float DropHeight = 44f;

    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_BoxPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Capsule;
    [SerializeField, Range(1f, 10f)] float m_GravityScale = 1f;

    readonly List<GameObject> m_Spawned = new();
    Vector2 m_WorldGravity;

    #endregion
}
