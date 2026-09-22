using UnityEngine;

/// <summary>
/// Adds the spinning grid's controls to the Toolbox menu: how many boxes it is made of, how far apart they sit, and how fast it turns.
/// The first two rebuild the body; the speed is applied to the body where it stands.
/// </summary>
public sealed class LargeKinematicOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Grid Size", m_Contents.gridSize, 10, 150, value =>
        {
            m_Contents.gridSize = value;
            m_Contents.Rebuild();
        });

        AddSlider("Grid Spacing", m_Contents.gridSpacing, 0f, 1f, value =>
        {
            m_Contents.gridSpacing = value;
            m_Contents.Rebuild();
        });

        AddSlider("Angular Velocity", m_Contents.angularVelocity, -360f, 360f, value => m_Contents.angularVelocity = value);
    }

    #region Internal

    [SerializeField] LargeKinematicContents m_Contents;

    #endregion
}
