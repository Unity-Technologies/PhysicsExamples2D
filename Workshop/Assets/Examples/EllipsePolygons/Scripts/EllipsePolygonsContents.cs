using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Drops a grid of rounded shapes that look like ellipses into a tall box, to show how close a polygon with a radius can get to a curved outline.
/// Every shape is one Physics Pose with a Physics Area Polygon instantiated from a prefab, and each one is given the current friction and bounciness before it is built.
/// </summary>
/// <remarks>
/// The polygon is a thin six-sided outline, and its radius rounds every edge outward so the result is a smooth oval rather than a hexagon.
/// There is no ellipse shape, so this is how one is approximated, and a little rolling resistance slows the rounded shapes as they roll.
/// </remarks>
public sealed class EllipsePolygonsContents : MonoBehaviour
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

        if (m_EllipsePrefab == null)
            return;

        var y = 2f;

        for (var row = 0; row < m_RowCount; ++row, y += 1f)
        {
            var x = m_ColumnCount * -0.5f + 0.5f;

            for (var column = 0; column < m_ColumnCount; ++column, x += 1f)
            {
                var spawned = Instantiate(m_EllipsePrefab, new Vector3(x, y, 0f), Quaternion.identity);

                var area = spawned.GetComponent<PhysicsArea>();
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

    [SerializeField] GameObject m_EllipsePrefab;
    [SerializeField, Range(1, 35)] int m_ColumnCount = 35;
    [SerializeField, Range(1, 20)] int m_RowCount = 20;
    [SerializeField, Range(0f, 1f)] float m_Friction = 0.6f;
    [SerializeField, Range(0f, 1f)] float m_Bounciness;

    readonly List<GameObject> m_Spawned = new();

    #endregion
}
