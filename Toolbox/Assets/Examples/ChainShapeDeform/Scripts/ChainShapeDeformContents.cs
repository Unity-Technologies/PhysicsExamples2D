using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Reshapes a closed contour of segments every frame, rippling its radius around the loop so the whole outline appears to flow.
/// The contour is one Physics Area Contour whose vertices are moved in place and then applied, which rebuilds its segments to match.
/// </summary>
/// <remarks>
/// Each vertex sits at a fixed angle around the middle, at a radius that is the base radius plus a sine wave of the modulation radius, with the wave's phase advancing over time.
/// Changing the vertex count replaces the contour with one of the new size, where every other control only changes where the existing vertices are placed.
/// </remarks>
public sealed class ChainShapeDeformContents : MonoBehaviour
{
    /// <summary>
    /// Replaces the contour with one holding the current vertex count, placed for the current phase.
    /// </summary>
    public void Rebuild()
    {
        if (m_Area == null)
            return;

        var contour = new ContourGeometry(m_VertexCount);

        for (var i = 0; i < m_VertexCount; ++i)
            contour.Add(Vector2.zero);

        m_Contour = contour;
        FillVertices();

        m_Area.geometry = new ContourGroupGeometry(new[] { m_Contour });
        m_Area.ApplyGeometry();
    }

    /// <summary>
    /// How many vertices the contour has.
    /// Changing this does not replace the contour until <see cref="Rebuild"/> is called.
    /// </summary>
    public int vertexCount
    {
        get => m_VertexCount;
        set => m_VertexCount = value;
    }

    /// <summary>
    /// The radius the contour ripples around, in meters.
    /// </summary>
    public float baseRadius
    {
        get => m_BaseRadius;
        set => m_BaseRadius = value;
    }

    /// <summary>
    /// How far the ripple moves each vertex in or out from the base radius, in meters.
    /// </summary>
    public float modulationRadius
    {
        get => m_ModulationRadius;
        set => m_ModulationRadius = value;
    }

    /// <summary>
    /// How many ripples there are around the loop.
    /// </summary>
    public int modulationFrequency
    {
        get => m_ModulationFrequency;
        set => m_ModulationFrequency = value;
    }

    /// <summary>
    /// How fast the ripple travels around the loop, in radians of phase per second, where a negative value runs it the other way.
    /// </summary>
    public float speed
    {
        get => m_Speed;
        set => m_Speed = value;
    }

    private void Start() => Rebuild();

    private void Update()
    {
        if (m_Contour == null)
            return;

        m_Phase += Time.deltaTime * m_Speed;
        FillVertices();

        m_Area.ApplyGeometry();
    }

    // Places every vertex of the contour at its angle around the middle, at the rippled radius for the current phase.
    private void FillVertices()
    {
        var count = m_Contour.vertexCount;

        for (var i = 0; i < count; ++i)
        {
            var theta = 2f * Mathf.PI * i / count;
            var radius = m_BaseRadius + m_ModulationRadius * Mathf.Sin(m_ModulationFrequency * theta + m_Phase);
            m_Contour[i] = new Vector2(Mathf.Cos(theta) * radius, Mathf.Sin(theta) * radius);
        }
    }

    #region Internal

    [SerializeField] PhysicsAreaContour m_Area;
    [SerializeField, Range(8, 1024)] int m_VertexCount = 64;
    [SerializeField, Range(8f, 20f)] float m_BaseRadius = 8f;
    [SerializeField, Range(0f, 5f)] float m_ModulationRadius = 2f;
    [SerializeField, Range(1, 32)] int m_ModulationFrequency = 8;
    [SerializeField, Range(-50f, 50f)] float m_Speed = 4f;

    ContourGeometry m_Contour;
    float m_Phase;

    #endregion
}
