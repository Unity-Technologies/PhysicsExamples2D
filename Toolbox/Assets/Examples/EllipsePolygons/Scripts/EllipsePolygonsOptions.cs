using UnityEngine;

/// <summary>
/// Adds the ellipse grid's controls to the Toolbox menu: how many columns and rows it has, and the friction and bounciness of every shape.
/// Every one of them drops the whole grid again so all the shapes share the same settings.
/// </summary>
public sealed class EllipsePolygonsOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Column Count", m_Contents.columnCount, 1, 35, value =>
        {
            m_Contents.columnCount = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Row Count", m_Contents.rowCount, 1, 20, value =>
        {
            m_Contents.rowCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Friction", m_Contents.friction, 0f, 1f, value =>
        {
            m_Contents.friction = value;
            m_Contents.Rebuild();
        });

        AddSlider("Restitution", m_Contents.bounciness, 0f, 1f, value =>
        {
            m_Contents.bounciness = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] EllipsePolygonsContents m_Contents;

    #endregion
}
