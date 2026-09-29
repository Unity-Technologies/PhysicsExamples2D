using UnityEngine;

/// <summary>
/// Adds the slicing example's controls to the Toolbox: buttons to orbit the player and fire, and how many times a shot can reflect and how full the arena can get.
/// The controls take effect from the next shot, leaving whatever is already cut where it is.
/// </summary>
public sealed class SlicingOptions : ToolboxOptionsProvider
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

        AddSliderInt("Reflection Count", m_Contents.reflectionCount, 0, 10, value => m_Contents.reflectionCount = value);
        AddSliderInt("Maximum Fragments", m_Contents.maximumFragments, 1000, 5000, value => m_Contents.maximumFragments = value);
    }

    // The controls menu outlives this example, so the fire button's click is handed back when the example unloads.
    private void OnDisable()
    {
        if (m_FireButton != null && m_Contents != null)
            m_FireButton.button.clickable.clicked -= m_Contents.Fire;

        m_FireButton = null;
    }

    #region Internal

    [SerializeField] SlicingContents m_Contents;

    ControlsMenu.CustomButton m_FireButton;

    #endregion
}
