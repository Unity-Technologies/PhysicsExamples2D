using UnityEngine;

/// <summary>
/// Adds the arch's control to the Workshop menu: how much friction holds the stones together.
/// Changing the friction rebuilds the arch, so it always starts standing.
/// </summary>
public sealed class ArchOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Friction", m_Contents.friction, 0.5f, 1f, value =>
        {
            m_Contents.friction = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] ArchContents m_Contents;

    #endregion
}
