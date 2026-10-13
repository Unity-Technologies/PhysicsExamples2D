using UnityEngine;

/// <summary>
/// Drives a Physics Area Mesh, deforming its mesh of triangle shapes every frame.
/// The mesh is split into triangles by the area itself, so this script only chooses the outline, and the mode and strength of the deformation.
/// </summary>
/// <remarks>
/// With animation on, the strength of the deformation swings back and forth through zero, so the undeformed mesh is part of the cycle.
/// Changing the seed or the outline vertex count makes a new outline, which the area splits into triangles again.
/// </remarks>
public sealed class MeshDeformationContents : MonoBehaviour
{
    /// <summary>
    /// Replaces the mesh's outline with one made from the current seed and vertex count, and builds its triangles again.
    /// </summary>
    public void Rebuild()
    {
        if (m_Mesh == null)
            return;

        m_Mesh.outline = PhysicsAreaMesh.CreateRandomOutline(m_Seed, m_OutlineVertexCount, OutlineRadius, OutlineJitter);
        m_Mesh.useDelaunay = m_UseDelaunay;
        m_Mesh.ApplyGeometry();
    }

    /// <summary>
    /// The mesh being deformed.
    /// </summary>
    public PhysicsAreaMesh mesh => m_Mesh;

    /// <summary>
    /// Chooses the random outline.
    /// Changing this does not make a new outline until <see cref="Rebuild"/> is called.
    /// </summary>
    public int seed
    {
        get => m_Seed;
        set => m_Seed = value;
    }

    /// <summary>
    /// How many vertices the outline has, and so how many triangles the mesh has.
    /// Changing this does not make a new outline until <see cref="Rebuild"/> is called.
    /// </summary>
    public int outlineVertexCount
    {
        get => m_OutlineVertexCount;
        set => m_OutlineVertexCount = value;
    }

    /// <summary>
    /// Whether the outline is split into triangles with Delaunay tessellation.
    /// Changing this does not split the outline again until <see cref="Rebuild"/> is called.
    /// </summary>
    public bool useDelaunay
    {
        get => m_UseDelaunay;
        set => m_UseDelaunay = value;
    }

    /// <summary>
    /// How the mesh is deformed.
    /// </summary>
    public PhysicsAreaMesh.DeformMode deformMode
    {
        get => m_DeformMode;
        set => m_DeformMode = value;
    }

    /// <summary>
    /// How strongly the mesh is deformed, and the most it is deformed when animation is on.
    /// </summary>
    public float amount
    {
        get => m_Amount;
        set => m_Amount = value;
    }

    /// <summary>
    /// Whether the strength of the deformation swings back and forth through zero over time.
    /// </summary>
    public bool animate
    {
        get => m_Animate;
        set => m_Animate = value;
    }

    private void Start() => Rebuild();

    private void Update()
    {
        if (m_Mesh == null)
            return;

        var currentAmount = m_Animate ? m_Amount * Mathf.Sin(Time.time) : m_Amount;
        m_Mesh.Deform(m_DeformMode, currentAmount);
    }

    #region Internal

    // The size of the random outline, in meters.
    const float OutlineRadius = 6f;
    const float OutlineJitter = 2.5f;

    [SerializeField] PhysicsAreaMesh m_Mesh;
    [SerializeField, Range(1, 1000)] int m_Seed = 1;
    [SerializeField, Range(6, 64)] int m_OutlineVertexCount = 25;
    [SerializeField] bool m_UseDelaunay = true;
    [SerializeField] PhysicsAreaMesh.DeformMode m_DeformMode = PhysicsAreaMesh.DeformMode.Shear;
    [SerializeField, Range(-1f, 1f)] float m_Amount = 0.5f;
    [SerializeField] bool m_Animate = true;

    #endregion
}
