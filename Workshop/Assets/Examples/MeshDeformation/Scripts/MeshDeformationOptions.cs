using UnityEngine;

/// <summary>
/// Adds the mesh deformation's controls to the Workshop menu: which outline the mesh is made from, and how it is deformed.
/// The outline and Delaunay controls make a new mesh, and every other control acts on the running mesh straight away.
/// </summary>
public sealed class MeshDeformationOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Seed", m_Contents.seed, 1, 1000, value =>
        {
            m_Contents.seed = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Outline Vertices", m_Contents.outlineVertexCount, 6, 64, value =>
        {
            m_Contents.outlineVertexCount = value;
            m_Contents.Rebuild();
        });

        AddToggle("Delaunay", m_Contents.useDelaunay, value =>
        {
            m_Contents.useDelaunay = value;
            m_Contents.Rebuild();
        });

        AddSlider("Min Area", m_Contents.mesh.minArea, 0f, 0.5f, value => m_Contents.mesh.minArea = value);
        AddEnum("Deform", m_Contents.deformMode, value => m_Contents.deformMode = value);
        AddSlider("Amount", m_Contents.amount, -1f, 1f, value => m_Contents.amount = value);
        AddToggle("Animate", m_Contents.animate, value => m_Contents.animate = value);
    }

    #region Internal

    [SerializeField] MeshDeformationContents m_Contents;

    #endregion
}
