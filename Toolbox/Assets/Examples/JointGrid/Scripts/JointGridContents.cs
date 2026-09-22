using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Builds a square grid of circles, each hinged to the neighbor above it and the neighbor to its left, hanging from a short static run along the top of its middle column.
/// The grid is made of Physics Pose, Physics Area and Physics Constraint Hinge components, so the whole cloth is authored the same way a handful of jointed bodies would be.
/// </summary>
/// <remarks>
/// The grid always covers the same area, so raising the count makes every circle and every link smaller rather than making the sheet larger.
/// </remarks>
public sealed class JointGridContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current grid and builds a new one at the current size.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        if (m_NodePrefab == null)
            return;

        // The grid covers a fixed span, so the spacing and everything measured against it shrink as the count rises.
        var gridScale = GridSpan / m_GridSize;
        var radius = 0.4f * gridScale;
        var offset = new Vector2(m_GridSize * -0.5f, m_GridSize * 0.5f);

        var nodes = new PhysicsPose[m_GridSize * m_GridSize];
        var index = 0;

        m_Spawned.Capacity = Mathf.Max(m_Spawned.Capacity, nodes.Length);

        for (var column = 0; column < m_GridSize; ++column)
        {
            for (var row = 0; row < m_GridSize; ++row)
            {
                var position = (new Vector2(column, -row) + offset) * gridScale;

                // A few columns of the top row are static, so the sheet hangs from them rather than falling away.
                var anchored = row == 0 && column >= m_GridSize / 2 - AnchoredColumns && column <= m_GridSize / 2 + AnchoredColumns;

                var node = Spawn(position, radius, anchored);

                // The link upward and the link to the left are what hold a circle in the sheet; the first row and the first column each have one of them missing.
                if (row > 0)
                    AddHinge(node, nodes[index - 1], new Vector2(0f, -0.5f) * gridScale, new Vector2(0f, 0.5f) * gridScale);

                if (column > 0)
                    AddHinge(node, nodes[index - m_GridSize], new Vector2(0.5f, 0f) * gridScale, new Vector2(-0.5f, 0f) * gridScale);

                node.gameObject.SetActive(true);

                nodes[index++] = node;
            }
        }
    }

    /// <summary>
    /// Destroys every circle in the grid.
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
    /// How many circles the grid is across and down.
    /// Changing this does not rebuild the grid until <see cref="Rebuild"/> is called.
    /// </summary>
    public int gridSize
    {
        get => m_GridSize;
        set => m_GridSize = value;
    }

    private void Start() => Rebuild();

    // Creates one circle, left inactive so its links can be added before its body and shape are built.
    private PhysicsPose Spawn(Vector2 position, float radius, bool anchored)
    {
        var spawned = Instantiate(m_NodePrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity);

        m_Spawned.Add(spawned);

        var pose = spawned.GetComponent<PhysicsPose>();
        var definition = pose.definition;
        definition.type = anchored ? PhysicsBody.BodyType.Static : PhysicsBody.BodyType.Dynamic;
        pose.definition = definition;

        spawned.GetComponent<PhysicsAreaCircle>().geometry = new CircleGeometry { center = Vector2.zero, radius = radius };

        return pose;
    }

    // Hinges a circle to a neighbor already in the grid.
    // The anchors are body-local points on the facing sides of the two circles, so the pair pivots where they meet rather than about either center.
    private static void AddHinge(PhysicsPose node, PhysicsPose neighbor, Vector2 anchorOnNeighbor, Vector2 anchorOnNode)
    {
        var hinge = node.gameObject.AddComponent<PhysicsConstraintHinge>();
        hinge.source = PhysicsConstraint.PoseSource.Custom;
        hinge.poseA = neighbor;
        hinge.poseB = node;

        var definition = hinge.definition;
        definition.autoAnchorA = false;
        definition.autoAnchorB = false;
        definition.localAnchorA = new PhysicsTransform(anchorOnNeighbor, PhysicsRotate.identity);
        definition.localAnchorB = new PhysicsTransform(anchorOnNode, PhysicsRotate.identity);
        hinge.definition = definition;
    }

    #region Internal

    // How wide the grid is in meters, whatever it is divided into.
    const float GridSpan = 150f;

    // How many columns either side of the middle are held static along the top row.
    const int AnchoredColumns = 3;

    [SerializeField] GameObject m_NodePrefab;
    [SerializeField, Range(10, 100)] int m_GridSize = 32;

    readonly List<GameObject> m_Spawned = new();

    #endregion
}
