using UnityEngine;

/// <summary>
/// Adds the table's controls to the Workshop: an Explode button, and sliders for how much force and torque the friction joints can resist.
/// The sliders act on the shapes where they are.
/// </summary>
public sealed class TopDownFrictionOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        m_ExplodeButton = controlsMenu[0];
        m_ExplodeButton.Set("Explode");
        m_ExplodeButton.button.clickable.clicked += m_Contents.Explode;

        AddSlider("Max Force", m_Contents.maxForce, 0f, 10f, value => m_Contents.maxForce = value);
        AddSlider("Max Torque", m_Contents.maxTorque, 0f, 10f, value => m_Contents.maxTorque = value);
    }

    // The controls menu outlives this example, so the button's click is handed back when the example unloads.
    private void OnDisable()
    {
        if (m_ExplodeButton != null && m_Contents != null)
            m_ExplodeButton.button.clickable.clicked -= m_Contents.Explode;

        m_ExplodeButton = null;
    }

    #region Internal

    [SerializeField] TopDownFrictionContents m_Contents;

    ControlsMenu.CustomButton m_ExplodeButton;

    #endregion
}
