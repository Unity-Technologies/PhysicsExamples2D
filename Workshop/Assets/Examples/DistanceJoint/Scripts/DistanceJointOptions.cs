using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Adds the distance joint's controls to the Workshop menu: the joint count and every distance, spring, limit and motor setting.
/// The joint count, the distance and the distance limits rebuild the chain; everything else acts on the chain where it hangs.
/// </summary>
public sealed class DistanceJointOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Joint Count", m_Contents.jointCount, 1, 20, value =>
        {
            m_Contents.jointCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Distance", m_Contents.jointDistance, 0.5f, 4f, value =>
        {
            m_Contents.jointDistance = value;
            m_Contents.Rebuild();
        });
        AddToggle("Enable Spring", m_Contents.enableSpring, value => m_Contents.enableSpring = value);
        AddSlider("Spring Frequency", m_Contents.springFrequency, 0f, 60f, value => m_Contents.springFrequency = value);
        AddSlider("Spring Damping", m_Contents.springDamping, 0f, 4f, value => m_Contents.springDamping = value);
        AddSlider("Spring Tension", m_Contents.springTension, 0f, 4000f, value => m_Contents.springTension = value);
        AddSlider("Spring Compression", m_Contents.springCompression, 0f, 200f, value => m_Contents.springCompression = value);
        AddToggle("Enable Limit", m_Contents.enableLimit, value => m_Contents.enableLimit = value);

        // The minimum limit can never pass the maximum, so a value beyond it is pulled back to it rather than applied.
        Slider minDistanceLimit = null;
        minDistanceLimit = AddSlider("Min Distance Limit", m_Contents.minDistanceLimit, 0.1f, 4f, value =>
        {
            if (value > m_Contents.maxDistanceLimit)
            {
                m_Contents.minDistanceLimit = m_Contents.maxDistanceLimit;
                minDistanceLimit.value = m_Contents.minDistanceLimit;
                return;
            }

            m_Contents.minDistanceLimit = value;
            m_Contents.Rebuild();
        });

        // The maximum limit can never fall below the minimum, so a value beneath it is pulled back to it rather than applied.
        Slider maxDistanceLimit = null;
        maxDistanceLimit = AddSlider("Max Distance Limit", m_Contents.maxDistanceLimit, 0.4f, 4f, value =>
        {
            if (value < m_Contents.minDistanceLimit)
            {
                m_Contents.maxDistanceLimit = m_Contents.minDistanceLimit;
                maxDistanceLimit.value = m_Contents.maxDistanceLimit;
                return;
            }

            m_Contents.maxDistanceLimit = value;
            m_Contents.Rebuild();
        });

        AddToggle("Enable Motor", m_Contents.enableMotor, value => m_Contents.enableMotor = value);
        AddSlider("Motor Speed", m_Contents.motorSpeed, -50f, 50f, value => m_Contents.motorSpeed = value);
        AddSlider("Max Motor Force", m_Contents.maxMotorForce, 0f, 500f, value => m_Contents.maxMotorForce = value);
    }

    #region Internal

    [SerializeField] DistanceJointContents m_Contents;

    #endregion
}
