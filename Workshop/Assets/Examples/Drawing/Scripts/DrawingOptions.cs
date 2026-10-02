using UnityEngine;

/// <summary>
/// Adds the drawing's controls to the Workshop menu: which drawing call is used, how many shapes are drawn, how long each stays, and whether outlines and interiors are drawn.
/// Every change clears the drawing and draws it again.
/// </summary>
public sealed class DrawingOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddEnum("Type", m_Contents.drawingType, value =>
        {
            m_Contents.drawingType = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Count", m_Contents.drawingCount, 10, 10000, value =>
        {
            m_Contents.drawingCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Lifetime", m_Contents.drawingLifetime, 1f, 60f, value =>
        {
            m_Contents.drawingLifetime = value;
            m_Contents.Rebuild();
        });

        AddToggle("Spread lifetime", m_Contents.spreadLifetime, value =>
        {
            m_Contents.spreadLifetime = value;
            m_Contents.Rebuild();
        });

        AddToggle("Draw Outline", m_Contents.drawOutline, value =>
        {
            m_Contents.drawOutline = value;
            m_Contents.Rebuild();
        });

        AddToggle("Draw Interior", m_Contents.drawInterior, value =>
        {
            m_Contents.drawInterior = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] DrawingContents m_Contents;

    #endregion
}
