using System;
using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Builds a free-standing arch of wedge-shaped stones with a stack of blocks resting on its keystone, held up by nothing but the stones pressing on each other.
/// Every stone and block is one Physics Pose with a Physics Area Polygon, instantiated from a single prefab and given its own outline.
/// </summary>
/// <remarks>
/// Friction is set on the ground as well as on every stone, so lowering it weakens every contact the arch depends on at once.
/// </remarks>
public sealed class ArchContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current arch and builds a new one at the current friction.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        if (m_Ground != null)
        {
            SetFriction(m_Ground);
            m_Ground.ApplyDefinition();
        }

        if (m_StonePrefab == null)
            return;

        var inner = ScaleVertices(InnerVertices);
        var outer = ScaleVertices(OuterVertices);

        // The right-hand side of the arch, one stone between each neighboring pair of vertices on the inner and outer curves.
        for (var i = 0; i < SideStoneCount; ++i)
            Spawn(Vector2.zero, new[] { inner[i], outer[i], outer[i + 1], inner[i + 1] });

        // The left-hand side, mirrored across the middle, with the vertex order reversed so the outline still winds the same way.
        for (var i = 0; i < SideStoneCount; ++i)
            Spawn(Vector2.zero, new[] { Mirror(outer[i]), Mirror(inner[i]), Mirror(inner[i + 1]), Mirror(outer[i + 1]) });

        // The keystone, spanning the tops of both sides.
        Spawn(Vector2.zero, new[] { inner[SideStoneCount], outer[SideStoneCount], Mirror(outer[SideStoneCount]), Mirror(inner[SideStoneCount]) });

        // The blocks stacked on the keystone, each one resting on the one below.
        for (var i = 0; i < BlockCount; ++i)
            Spawn(new Vector2(0f, 0.5f + outer[SideStoneCount].y + i), PolygonGeometry.CreateBox(BlockSize));
    }

    /// <summary>
    /// Destroys every stone and block in the arch, leaving the ground alone.
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
    /// The friction on the ground and on every stone and block.
    /// Changing this does not rebuild the arch until <see cref="Rebuild"/> is called.
    /// </summary>
    public float friction
    {
        get => m_Friction;
        set => m_Friction = value;
    }

    private void Start() => Rebuild();

    // Creates one stone whose outline is the specified vertices, which are given in the space of a body at the origin.
    private void Spawn(Vector2 position, Vector2[] vertices) => Spawn(position, PolygonGeometry.Create(vertices: vertices.AsSpan()));

    // Creates one stone at the specified position with the specified outline.
    private void Spawn(Vector2 position, PolygonGeometry polygonGeometry)
    {
        var spawned = Instantiate(m_StonePrefab, position, Quaternion.identity);

        var area = spawned.GetComponent<PhysicsAreaPolygon>();
        area.geometry = polygonGeometry;
        SetFriction(area);

        spawned.SetActive(true);
        m_Spawned.Add(spawned);
    }

    // Writes the current friction onto an area's definition, which takes effect when the area next builds its shape or is told to apply it.
    private void SetFriction(PhysicsArea area)
    {
        var definition = area.definition;

        var surfaceMaterial = definition.surfaceMaterial;
        surfaceMaterial.friction = m_Friction;
        definition.surfaceMaterial = surfaceMaterial;

        area.definition = definition;
    }

    // Returns a copy of the specified vertices shrunk to the size the arch is built at.
    private static Vector2[] ScaleVertices(Vector2[] vertices)
    {
        var scaled = new Vector2[vertices.Length];

        for (var i = 0; i < vertices.Length; ++i)
            scaled[i] = vertices[i] * VertexScale;

        return scaled;
    }

    // Returns the specified vertex reflected across the middle of the arch.
    private static Vector2 Mirror(Vector2 vertex) => new(-vertex.x, vertex.y);

    #region Internal

    // How many stones make up each side of the arch, not counting the keystone, and how many blocks are stacked on top.
    const int SideStoneCount = 8;
    const int BlockCount = 4;

    // The size of each block stacked on the keystone, and how much the curves below are shrunk before the arch is built from them.
    static readonly Vector2 BlockSize = new(4f, 1f);
    const float VertexScale = 0.25f;

    // The inner and outer curves of the right-hand side of the arch, from the ground up to the keystone.
    static readonly Vector2[] InnerVertices =
    {
        new(16.0f, 0.0f),
        new(14.93803712795643f, 5.133601056842984f),
        new(13.79871746027416f, 10.24928069555078f),
        new(12.56252963284711f, 15.34107019122473f),
        new(11.20040987372525f, 20.39856541571217f),
        new(9.66521217819836f, 25.40369899225096f),
        new(7.87179930638133f, 30.3179337000085f),
        new(5.635199558196225f, 35.03820717801641f),
        new(2.405937953536585f, 39.09554102558315f)
    };

    static readonly Vector2[] OuterVertices =
    {
        new(24.0f, 0.0f),
        new(22.33619528222415f, 6.02299846205841f),
        new(20.54936888969905f, 12.00964361211476f),
        new(18.60854610798073f, 17.9470321677465f),
        new(16.46769273811807f, 23.81367936585418f),
        new(14.05325025774858f, 29.57079353071012f),
        new(11.23551045834022f, 35.13775818285372f),
        new(7.752568160730571f, 40.30450679009583f),
        new(3.016931552701656f, 44.28891593799322f)
    };

    [SerializeField] GameObject m_StonePrefab;
    [SerializeField] PhysicsAreaSegment m_Ground;
    [SerializeField, Range(0.5f, 1f)] float m_Friction = 1f;

    readonly List<GameObject> m_Spawned = new();

    #endregion
}
