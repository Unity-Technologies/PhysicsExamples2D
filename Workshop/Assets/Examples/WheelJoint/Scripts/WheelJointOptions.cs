using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Adds the wheel joint's controls to the Workshop menu: the wheel angle and every spring, motor and limit setting.
/// The wheel angle rebuilds the wheel; everything else acts on the joint where it is.
/// </summary>
public sealed class WheelJointOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Wheel Angle", m_Contents.wheelAngle, -180f, 180f, value =>
        {
            m_Contents.wheelAngle = value;
            m_Contents.Rebuild();
        });

        AddToggle("Enable Spring", m_Contents.enableSpring, value => m_Contents.enableSpring = value);
        AddSlider("Spring Frequency", m_Contents.springFrequency, 0f, 60f, value => m_Contents.springFrequency = value);
        AddSlider("Spring Damping", m_Contents.springDamping, 0f, 4f, value => m_Contents.springDamping = value);
        AddToggle("Enable Motor", m_Contents.enableMotor, value => m_Contents.enableMotor = value);
        AddSlider("Motor Speed", m_Contents.motorSpeed, -3000f, 3000f, value => m_Contents.motorSpeed = value);
        AddSlider("Max Motor Torque", m_Contents.maxMotorTorque, 0f, 20f, value => m_Contents.maxMotorTorque = value);
        AddToggle("Enable Limit", m_Contents.enableLimit, value => m_Contents.enableLimit = value);

        // The lower limit can never pass the upper, so a value beyond it is pulled back to it rather than applied.
        Slider lowerTranslationLimit = null;
        lowerTranslationLimit = AddSlider("Min Distance Limit", m_Contents.lowerTranslationLimit, -4f, 0f, value =>
        {
            if (value > m_Contents.upperTranslationLimit)
            {
                lowerTranslationLimit.value = m_Contents.upperTranslationLimit;
                return;
            }

            m_Contents.lowerTranslationLimit = value;
        });

        // The upper limit can never fall below the lower, so a value beneath it is pulled back to it rather than applied.
        Slider upperTranslationLimit = null;
        upperTranslationLimit = AddSlider("Max Distance Limit", m_Contents.upperTranslationLimit, 0f, 4f, value =>
        {
            if (value < m_Contents.lowerTranslationLimit)
            {
                upperTranslationLimit.value = m_Contents.lowerTranslationLimit;
                return;
            }

            m_Contents.upperTranslationLimit = value;
        });
    }

    #region Internal

    [SerializeField] WheelJointContents m_Contents;

    #endregion
}
