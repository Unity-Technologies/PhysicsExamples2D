using UnityEngine;

/// <summary>
/// Adds the stuck inside example's Intrude button to the Workshop, which drops a body inside each of the tables and ships.
/// </summary>
public sealed class StuckInsideOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        m_IntrudeButton = controlsMenu[0];
        m_IntrudeButton.Set("Intrude");
        m_IntrudeButton.button.clickable.clicked += m_Contents.Intrude;
    }

    // The controls menu outlives this example, so the button's click is handed back when the example unloads.
    private void OnDisable()
    {
        if (m_IntrudeButton != null && m_Contents != null)
            m_IntrudeButton.button.clickable.clicked -= m_Contents.Intrude;

        m_IntrudeButton = null;
    }

    #region Internal

    [SerializeField] StuckInsideContents m_Contents;

    ControlsMenu.CustomButton m_IntrudeButton;

    #endregion
}
