using UnityEngine;

/// <summary>
/// Adds the buoyancy controls to the Workshop menu: the dropped shapes' type, count, scale and density, and the liquid's surface level, density, flow and damping.
/// The shape type, count and scale rebuild the shapes; everything else acts where it is.
/// </summary>
public sealed class BuoyancyOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddEnum("Object Type", m_Contents.objectType, value =>
        {
            m_Contents.objectType = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Spawn Count", m_Contents.spawnCount, 1, 1000, value =>
        {
            m_Contents.spawnCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Shape Scale", m_Contents.shapeScale, 0.1f, 2f, value =>
        {
            m_Contents.shapeScale = value;
            m_Contents.Rebuild();
        });

        AddSlider("Shape Density", m_Contents.shapeDensity, 0.1f, 5f, value => m_Contents.shapeDensity = value);
        AddSlider("Surface Level", m_Contents.surfaceLevel, 0.1f, 75f, value => m_Contents.surfaceLevel = value);
        AddSlider("Liquid Density", m_Contents.liquidDensity, 0.1f, 10f, value => m_Contents.liquidDensity = value);
        AddSlider("Flow Direction", m_Contents.flowDirection, 0f, 359f, value => m_Contents.flowDirection = value);
        AddSlider("Flow Speed", m_Contents.flowSpeed, -20f, 20f, value => m_Contents.flowSpeed = value);
        AddSlider("Linear Damping", m_Contents.linearDamping, 0f, 10f, value => m_Contents.linearDamping = value);
        AddSlider("Angular Damping", m_Contents.angularDamping, 0f, 10f, value => m_Contents.angularDamping = value);
    }

    #region Internal

    [SerializeField] BuoyancyContents m_Contents;

    #endregion
}
