using UnityEngine;

/// <summary>
/// Adds the friction example's controls to the Workshop menu: which shape slides down the ramps, and how hard gravity pulls on it.
/// Both restart the stream so every shape shares the same settings.
/// </summary>
public sealed class FrictionOptions : WorkshopOptionsProvider
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

        AddSlider("Gravity Scale", m_Contents.gravityScale, 1f, 10f, value =>
        {
            m_Contents.gravityScale = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] FrictionContents m_Contents;

    #endregion
}
