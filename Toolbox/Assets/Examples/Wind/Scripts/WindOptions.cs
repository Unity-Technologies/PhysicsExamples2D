using UnityEngine;

/// <summary>
/// Adds the wind example's controls to the Toolbox menu: the chain's shape and length, which rebuild it, and the wind's direction, speed, drag and lift, which act on it live.
/// </summary>
public sealed class WindOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddEnum("Geometry Type", m_Contents.geometryType, value =>
        {
            m_Contents.geometryType = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Count", m_Contents.geometryCount, 1, 50, value =>
        {
            m_Contents.geometryCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Wind Direction", m_Contents.windDirection, 0f, 359f, value => m_Contents.windDirection = value);
        AddSlider("Wind Speed", m_Contents.windSpeed, 0f, 10f, value => m_Contents.windSpeed = value);
        AddSlider("Drag", m_Contents.drag, 0f, 1f, value => m_Contents.drag = value);
        AddSlider("Lift", m_Contents.lift, 0f, 4f, value => m_Contents.lift = value);
    }

    #region Internal

    [SerializeField] WindContents m_Contents;

    #endregion
}
