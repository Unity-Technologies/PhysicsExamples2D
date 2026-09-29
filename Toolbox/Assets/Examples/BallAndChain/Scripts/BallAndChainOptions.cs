using UnityEngine;

/// <summary>
/// Adds the chain's controls to the Toolbox menu: the hinge spring and motor settings, and whether the chain length is fixed.
/// The spring and motor settings act on the chain where it hangs; fixing the chain length rebuilds it.
/// </summary>
public sealed class BallAndChainOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Spring Frequency", m_Contents.springFrequency, 0f, 120f, value => m_Contents.springFrequency = value);
        AddSlider("Spring Damping", m_Contents.springDamping, 0f, 10f, value => m_Contents.springDamping = value);
        AddSlider("Max Motor Torque", m_Contents.maxMotorTorque, 0f, 1000f, value => m_Contents.maxMotorTorque = value);

        AddToggle("Fix Chain Length", m_Contents.fixChainLength, value =>
        {
            m_Contents.fixChainLength = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] BallAndChainContents m_Contents;

    #endregion
}
