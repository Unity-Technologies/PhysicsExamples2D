using UnityEngine;

/// <summary>
/// Adds the deforming contour's controls to the Toolbox menu: how many vertices it has, and the size, count and speed of the ripple running around it.
/// Only the vertex count replaces the contour; every other control changes the ripple while it runs.
/// </summary>
public sealed class ChainShapeDeformOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Vertex Count", m_Contents.vertexCount, 8, 1024, value =>
        {
            m_Contents.vertexCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Base Radius", m_Contents.baseRadius, 8f, 20f, value => m_Contents.baseRadius = value);

        AddSlider("Modulation Radius", m_Contents.modulationRadius, 0f, 5f, value => m_Contents.modulationRadius = value);

        AddSliderInt("Modulation Frequency", m_Contents.modulationFrequency, 1, 32, value => m_Contents.modulationFrequency = value);

        AddSlider("Speed", m_Contents.speed, -50f, 50f, value => m_Contents.speed = value);
    }

    #region Internal

    [SerializeField] ChainShapeDeformContents m_Contents;

    #endregion
}
