using UnityEngine;

/// <summary>
/// Adds the walking machine's controls to the Workshop: the left, right and brake buttons, and sliders for how fast the motor turns and how much torque it has.
/// Every change acts on the running machine straight away.
/// </summary>
public sealed class TheoJansenOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        var leftButton = controlsMenu[2];
        var rightButton = controlsMenu[1];
        var brakeButton = controlsMenu[0];

        leftButton.Set("Left [←]");
        rightButton.Set("Right [→]");
        brakeButton.Set("Brake [Spc]");

        m_Contents.SetButtons(leftButton, rightButton, brakeButton);

        AddSlider("Speed", m_Contents.motorSpeed, 0f, 10f, value => m_Contents.motorSpeed = value);
        AddSlider("Torque", m_Contents.motorTorque, 0f, 2000f, value => m_Contents.motorTorque = value);
    }

    #region Internal

    [SerializeField] TheoJansenContents m_Contents;

    #endregion
}
