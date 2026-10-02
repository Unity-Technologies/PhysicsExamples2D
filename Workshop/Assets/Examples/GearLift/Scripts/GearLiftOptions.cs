using UnityEngine;

/// <summary>
/// Adds the gear lift's controls to the Workshop menu: whether the drive gear's motor runs, and its speed and torque.
/// Every change acts on the drive gear where it stands.
/// </summary>
public sealed class GearLiftOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddToggle("Use Motor", m_Contents.useMotor, value => m_Contents.useMotor = value);
        AddSlider("Motor Speed", m_Contents.motorSpeed, -100f, 100f, value => m_Contents.motorSpeed = value);
        AddSlider("Motor Max Torque", m_Contents.maxMotorTorque, 0f, 100f, value => m_Contents.maxMotorTorque = value);
    }

    #region Internal

    [SerializeField] GearLiftContents m_Contents;

    #endregion
}
