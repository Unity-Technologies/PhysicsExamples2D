using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Drops a grid of randomly shaped polygons, each rounded off by its own radius, into a tall box.
/// Every shape is one Physics Pose with a Physics Area Polygon instantiated from a prefab, and each one is given its own random outline and the current friction and bounciness before it is built.
/// </summary>
/// <remarks>
/// A polygon's radius rounds every edge outward, so the grid shows a mix of sharp and softly rounded shapes side by side.
/// </remarks>
public sealed class RoundedPolygonsContents : MonoBehaviour
{
    /// <summary>
    /// Destroys every shape and drops a new grid with the current size, friction and bounciness.
    /// </summary>
    public void Rebuild()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();

        if (m_PolygonPrefab == null)
            return;

        m_Random = new Random(RandomSeed);

        var y = 2f;

        for (var row = 0; row < m_RowCount; ++row, y += 1f)
        {
            var x = m_ColumnCount * -0.5f + 0.5f;

            for (var column = 0; column < m_ColumnCount; ++column, x += 1f)
            {
                var spawned = Instantiate(m_PolygonPrefab, new Vector3(x, y, 0f), Quaternion.identity);

                var radius = m_Random.NextFloat(0.05f, 0.25f);
                var area = spawned.GetComponent<PhysicsAreaPolygon>();
                area.geometry = ToolboxUtility.CreateRandomPolygon(extent: 0.5f, radius: radius, ref m_Random);

                var definition = area.definition;
                var surfaceMaterial = definition.surfaceMaterial;
                surfaceMaterial.friction = m_Friction;
                surfaceMaterial.bounciness = m_Bounciness;
                definition.surfaceMaterial = surfaceMaterial;
                area.definition = definition;

                spawned.SetActive(true);
                m_Spawned.Add(spawned);
            }
        }
    }

    /// <summary>
    /// How many shapes there are across each row of the grid.
    /// Changing this does not affect the grid until <see cref="Rebuild"/> is called.
    /// </summary>
    public int columnCount
    {
        get => m_ColumnCount;
        set => m_ColumnCount = value;
    }

    /// <summary>
    /// How many rows the grid has.
    /// Changing this does not affect the grid until <see cref="Rebuild"/> is called.
    /// </summary>
    public int rowCount
    {
        get => m_RowCount;
        set => m_RowCount = value;
    }

    /// <summary>
    /// The friction every shape is given.
    /// Changing this does not affect the grid until <see cref="Rebuild"/> is called.
    /// </summary>
    public float friction
    {
        get => m_Friction;
        set => m_Friction = value;
    }

    /// <summary>
    /// The bounciness every shape is given, from none at all to fully elastic.
    /// Changing this does not affect the grid until <see cref="Rebuild"/> is called.
    /// </summary>
    public float bounciness
    {
        get => m_Bounciness;
        set => m_Bounciness = value;
    }

    private void Start() => Rebuild();

    #region Internal

    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField, Range(1, 35)] int m_ColumnCount = 20;
    [SerializeField, Range(1, 20)] int m_RowCount = 20;
    [SerializeField, Range(0f, 1f)] float m_Friction = 0.6f;
    [SerializeField, Range(0f, 1f)] float m_Bounciness;

    readonly List<GameObject> m_Spawned = new();
    Random m_Random;

    #endregion
}
