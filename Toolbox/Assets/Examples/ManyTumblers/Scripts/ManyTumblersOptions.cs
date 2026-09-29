using UnityEngine;

/// <summary>
/// Adds the tumblers' controls to the Toolbox menu: how many rows and columns of tumblers there are, how fast they turn, and how many rounds of capsules are dropped into them.
/// Every change rebuilds the scene.
/// </summary>
public sealed class ManyTumblersOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Row Count", m_Contents.rowCount, 1, 50, value =>
        {
            m_Contents.rowCount = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Column Count", m_Contents.columnCount, 1, 50, value =>
        {
            m_Contents.columnCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Angular Velocity", m_Contents.angularVelocity, -90f, 90f, value =>
        {
            m_Contents.angularVelocity = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Spawn Count", m_Contents.spawnCount, 1, 10, value =>
        {
            m_Contents.spawnCount = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] ManyTumblersContents m_Contents;

    #endregion
}
