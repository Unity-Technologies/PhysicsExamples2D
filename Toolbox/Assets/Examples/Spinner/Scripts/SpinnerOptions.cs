using UnityEngine;

/// <summary>
/// Adds the spinner's controls to the Toolbox menu: the paddle's motor speed and torque, whether it is kinematic, the debris settings and the gravity scale.
/// The motor speed, motor torque and gravity scale act on the running scene, while the paddle type and debris settings rebuild it.
/// </summary>
public sealed class SpinnerOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Motor Speed", m_Contents.motorSpeed, -360f, 360f, value => m_Contents.motorSpeed = value);

        // A kinematic paddle has no hinge, so its motor torque does nothing and is switched off.
        var motorTorqueSlider = AddSlider("Motor Torque", m_Contents.maxMotorTorque, 0f, 100000f, value => m_Contents.maxMotorTorque = value);
        motorTorqueSlider.enabledSelf = !m_Contents.kinematicSpinner;

        AddToggle("Kinematic Spinner", m_Contents.kinematicSpinner, value =>
        {
            m_Contents.kinematicSpinner = value;
            motorTorqueSlider.enabledSelf = !value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Debris Count", m_Contents.debrisCount, 1000, 5000, value =>
        {
            m_Contents.debrisCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Debris Friction", m_Contents.debrisFriction, 0f, 1f, value =>
        {
            m_Contents.debrisFriction = value;
            m_Contents.Rebuild();
        });

        AddSlider("Debris Bounciness", m_Contents.debrisBounciness, 0f, 1f, value =>
        {
            m_Contents.debrisBounciness = value;
            m_Contents.Rebuild();
        });

        AddSlider("Gravity Scale", m_Contents.gravityScale, 0f, 2f, value => m_Contents.gravityScale = value);
    }

    #region Internal

    [SerializeField] SpinnerContents m_Contents;

    #endregion
}
