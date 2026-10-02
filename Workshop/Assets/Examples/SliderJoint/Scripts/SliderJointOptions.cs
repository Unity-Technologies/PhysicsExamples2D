using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Adds the slider joint's controls to the Workshop menu: the slide angle and every spring, motor and limit setting.
/// The slide angle rebuilds the capsule; everything else acts on the joint where it is.
/// </summary>
public sealed class SliderJointOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Slider Angle", m_Contents.sliderAngle, -180f, 180f, value =>
        {
            m_Contents.sliderAngle = value;
            m_Contents.Rebuild();
        });

        AddToggle("Enable Spring", m_Contents.enableSpring, value => m_Contents.enableSpring = value);
        AddSlider("Spring Target Translation", m_Contents.springTargetTranslation, -10f, 10f, value => m_Contents.springTargetTranslation = value);
        AddSlider("Spring Frequency", m_Contents.springFrequency, 0f, 60f, value => m_Contents.springFrequency = value);
        AddSlider("Spring Damping", m_Contents.springDamping, 0f, 4f, value => m_Contents.springDamping = value);
        AddToggle("Enable Motor", m_Contents.enableMotor, value => m_Contents.enableMotor = value);
        AddSlider("Motor Speed", m_Contents.motorSpeed, -50f, 50f, value => m_Contents.motorSpeed = value);
        AddSlider("Max Motor Force", m_Contents.maxMotorForce, 0f, 500f, value => m_Contents.maxMotorForce = value);
        AddToggle("Enable Limit", m_Contents.enableLimit, value => m_Contents.enableLimit = value);

        // The lower limit can never pass the upper, so a value beyond it is pulled back to it rather than applied.
        Slider lowerTranslationLimit = null;
        lowerTranslationLimit = AddSlider("Min Distance Limit", m_Contents.lowerTranslationLimit, -10f, 0f, value =>
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
        upperTranslationLimit = AddSlider("Max Distance Limit", m_Contents.upperTranslationLimit, 0f, 10f, value =>
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

    [SerializeField] SliderJointContents m_Contents;

    #endregion
}
