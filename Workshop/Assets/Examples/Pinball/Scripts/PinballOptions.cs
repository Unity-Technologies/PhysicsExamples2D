using UnityEngine;

/// <summary>
/// Adds the pinball's flipper button to the Workshop controls menu.
/// The button raises both flippers while it is held, as the space key does.
/// </summary>
public sealed class PinballOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        var flipperButton = controlsMenu[0];
        flipperButton.Set("Flippers [Spc]");

        m_Contents.SetButton(flipperButton);
    }

    #region Internal

    [SerializeField] PinballContents m_Contents;

    #endregion
}
