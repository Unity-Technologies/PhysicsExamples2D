using UnityEngine;

/// <summary>
/// Adds the Strandbeest's controls to the Workshop: the walk left, walk right and stop buttons, and sliders for how fast the motor turns and how much torque it has.
/// Every change acts on the running machine straight away.
/// </summary>
public sealed class StrandbeestOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        var leftButton = controlsMenu[2];
        var rightButton = controlsMenu[1];
        var brakeButton = controlsMenu[0];

        leftButton.Set("Walk Left [←]");
        rightButton.Set("Walk Right [→]");
        brakeButton.Set("Stop [Spc]");

        m_Contents.SetButtons(leftButton, rightButton, brakeButton);

        AddSlider("Motor Speed", m_Contents.motorSpeed, 0f, 10f, value => m_Contents.motorSpeed = value);
        AddSlider("Motor Torque", m_Contents.motorTorque, 0f, 2000f, value => m_Contents.motorTorque = value);
    }

    #region Internal

    [SerializeField] StrandbeestContents m_Contents;

    #endregion
}
