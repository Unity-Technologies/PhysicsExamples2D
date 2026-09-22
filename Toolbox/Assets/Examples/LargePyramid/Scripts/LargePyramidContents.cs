using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Stacks a pyramid of rounded boxes, as many rows high as it is wide along the base, to show how well a tall stack of resting bodies holds its shape.
/// Every box is one Physics Pose with a Physics Area, instantiated from a single prefab.
/// </summary>
/// <remarks>
/// Gravity can be scaled up to lean on the stack harder without rebuilding it, and the world's own gravity is put back when the example unloads.
/// </remarks>
public sealed class LargePyramidContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current pyramid and stacks a new one at the current base count.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        if (m_BoxPrefab == null)
            return;

        m_Spawned.Capacity = Mathf.Max(m_Spawned.Capacity, m_BaseCount * (m_BaseCount + 1) / 2);

        for (var row = 0; row < m_BaseCount; ++row)
        {
            var y = (2f * row + 1f) * Shift;

            // Each row up is one box shorter and starts half a box further in, which is what leaves the sloped sides.
            for (var column = row; column < m_BaseCount; ++column)
            {
                var x = (row + 1f) * Shift + 2f * (column - row) * Shift - HalfExtent * m_BaseCount;

                m_Spawned.Add(Instantiate(m_BoxPrefab, new Vector3(x, y, 0f), Quaternion.identity));
            }
        }
    }

    /// <summary>
    /// Destroys every box in the pyramid, leaving the ground alone.
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
    /// How many boxes wide the bottom row is, which is also how many rows tall the pyramid stands.
    /// Changing this does not restack the pyramid until <see cref="Rebuild"/> is called.
    /// </summary>
    public int baseCount
    {
        get => m_BaseCount;
        set => m_BaseCount = value;
    }

    /// <summary>
    /// How much harder than normal gravity pulls on the stack.
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

    // Half a box, which is the spacing the whole pyramid is laid out from.
    const float HalfExtent = 0.5f;
    const float Shift = HalfExtent;

    [SerializeField] GameObject m_BoxPrefab;
    [SerializeField, Range(2, 150)] int m_BaseCount = 60;
    [SerializeField, Range(0.1f, 5f)] float m_GravityScale = 1f;

    readonly List<GameObject> m_Spawned = new();
    Vector2 m_WorldGravity;

    #endregion
}
