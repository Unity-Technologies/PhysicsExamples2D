using UnityEngine;

public class Collider2D_CreateMeshDelaunay : MonoBehaviour
{
    public bool UseDelaunayMesh;

    private Collider2D m_Collider;
    private MeshFilter m_MeshFilter;
    private Mesh m_Mesh;
    private bool m_LastUseDelaunayMesh;

    void Start()
    {
        m_Collider = GetComponent<Collider2D>();
        m_MeshFilter = GetComponentInChildren<MeshFilter>();
        m_LastUseDelaunayMesh = UseDelaunayMesh;

        CreateMesh();
    }

    private void Update()
    {
        if (UseDelaunayMesh != m_LastUseDelaunayMesh)
        {
            m_LastUseDelaunayMesh = UseDelaunayMesh;
            CreateMesh();
        }
    }

    void CreateMesh()
    {
        DestroyMesh();

        if (!m_Collider || !m_MeshFilter)
            return;

        m_Mesh = m_Collider.CreateMesh(false, false, UseDelaunayMesh);
        m_MeshFilter.mesh = m_Mesh;
    }

    void DestroyMesh()
    {
        if (m_Mesh)
        {
            Destroy(m_Mesh);
            m_Mesh = null;
        }
    }

    void OnDestroy()
    {
        DestroyMesh();
    }
}
