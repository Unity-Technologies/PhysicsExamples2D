using UnityEngine;

/// <summary>
/// Adds the scissor lift's controls to the Workshop menu: whether its motor runs, and how fast.
/// Both act on the lift where it stands.
/// </summary>
public sealed class ScissorLiftOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddToggle("Enable Motor", m_Contents.enableMotor, value => m_Contents.enableMotor = value);
        AddSlider("Motor Speed", m_Contents.motorSpeed, -2f, 2f, value => m_Contents.motorSpeed = value);
    }

    #region Internal

    [SerializeField] ScissorLiftContents m_Contents;

    #endregion
}
