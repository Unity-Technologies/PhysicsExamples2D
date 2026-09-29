using UnityEngine;

/// <summary>
/// Adds the fragmenting example's controls to the Toolbox: buttons to move the player and fire, and the size, piece count and explosion of each hole broken out of the slab.
/// The controls take effect from the next hit, leaving the slab and any debris as they are.
/// </summary>
public sealed class FragmentingOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        var leftButton = controlsMenu[2];
        var rightButton = controlsMenu[1];
        m_FireButton = controlsMenu[0];

        leftButton.Set("Left [←]");
        rightButton.Set("Right [→]");
        m_FireButton.Set("Fire [Spc]");
        m_FireButton.button.clickable.clicked += m_Contents.Fire;

        m_Contents.SetButtons(leftButton, rightButton);

        AddSlider("Fragment Radius", m_Contents.fragmentRadius, 0.5f, 5f, value => m_Contents.fragmentRadius = value);
        AddSliderInt("Fragment Count", m_Contents.fragmentCount, 1, 300, value => m_Contents.fragmentCount = value);
        AddToggle("Fragment Explode", m_Contents.fragmentExplode, value => m_Contents.fragmentExplode = value);
    }

    // The controls menu outlives this example, so the fire button's click is handed back when the example unloads.
    private void OnDisable()
    {
        if (m_FireButton != null && m_Contents != null)
            m_FireButton.button.clickable.clicked -= m_Contents.Fire;

        m_FireButton = null;
    }

    #region Internal

    [SerializeField] FragmentingContents m_Contents;

    ControlsMenu.CustomButton m_FireButton;

    #endregion
}
