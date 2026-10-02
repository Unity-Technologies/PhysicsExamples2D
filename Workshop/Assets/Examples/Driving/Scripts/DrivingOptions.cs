using UnityEngine;

/// <summary>
/// Adds the car's controls to the Workshop: the reverse, forward and brake buttons, and sliders for the suspension and the wheel motors.
/// Every change acts on the car where it is, without rebuilding the course.
/// </summary>
public sealed class DrivingOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Car == null)
            return;

        var reverseButton = controlsMenu[2];
        var forwardButton = controlsMenu[1];
        var brakeButton = controlsMenu[0];

        reverseButton.Set("Reverse [←]");
        forwardButton.Set("Forward [→]");
        brakeButton.Set("Brake [Spc]");

        m_Car.SetButtons(reverseButton, forwardButton, brakeButton);

        AddSlider("Spring Frequency", m_Car.springFrequency, 0f, 60f, value => m_Car.springFrequency = value);
        AddSlider("Spring Damping", m_Car.springDamping, 0f, 4f, value => m_Car.springDamping = value);
        AddSlider("Motor Speed", m_Car.motorSpeed, -3000f, 3000f, value => m_Car.motorSpeed = value);
        AddSlider("Max Motor Torque", m_Car.maxMotorTorque, 0f, 20f, value => m_Car.maxMotorTorque = value);
    }

    #region Internal

    [SerializeField] DrivingCar m_Car;

    #endregion
}
