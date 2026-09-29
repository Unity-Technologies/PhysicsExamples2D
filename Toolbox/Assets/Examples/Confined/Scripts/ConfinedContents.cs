using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Packs a square grid of circles into a box too small to hold them, so the solver has to push every overlapping pair apart at once.
/// Every circle is one Physics Pose with a Physics Area Circle, instantiated from a prefab, and the box is four capsules on one static body.
/// </summary>
/// <remarks>
/// Gravity is switched off so nothing settles to the floor and the circles spread to fill the whole box.
/// The world's own gravity is put back when the example unloads.
/// </remarks>
public sealed class ConfinedContents : MonoBehaviour
{
    /// <summary>
    /// Destroys every circle and fills the box again with a grid of the current size.
    /// </summary>
    public void Rebuild()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();

        if (m_CirclePrefab == null)
            return;

        // The grid always spans the same area, so a larger count packs the circles closer and overlaps them further.
        var spacing = GridExtent / m_GridCount;

        for (var column = 0; column < m_GridCount; ++column)
        {
            for (var row = 0; row < m_GridCount; ++row)
            {
                var position = new Vector3(GridOrigin.x + column * spacing, GridOrigin.y + row * spacing, 0f);
                var spawned = Instantiate(m_CirclePrefab, position, Quaternion.identity);
                spawned.SetActive(true);
                m_Spawned.Add(spawned);
            }
        }
    }

    /// <summary>
    /// How many circles there are along each side of the grid.
    /// Changing this does not refill the box until <see cref="Rebuild"/> is called.
    /// </summary>
    public int gridCount
    {
        get => m_GridCount;
        set => m_GridCount = value;
    }

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;
        m_WorldGravity = world.gravity;
        world.gravity = Vector2.zero;
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay switched off into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;
    }

    private void Start() => Rebuild();

    #region Internal

    // Where the first circle of the grid is placed, and how far the whole grid spans along each side.
    static readonly Vector2 GridOrigin = new(-8.75f, 1.5f);
    const float GridExtent = 18f;

    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField, Range(20, 50)] int m_GridCount = 25;

    readonly List<GameObject> m_Spawned = new();
    Vector2 m_WorldGravity;

    #endregion
}
