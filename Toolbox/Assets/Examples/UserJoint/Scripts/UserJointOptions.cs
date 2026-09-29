using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Adds the custom joint's controls to the Toolbox menu: the springs' frequency, damping and maximum force, and where their anchors sit on the box.
/// It also shows the impulse each spring applied on the last step, updated every frame.
/// </summary>
public sealed class UserJointOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Joint Frequency", m_Contents.jointFrequency, 0.1f, 240f, value => m_Contents.jointFrequency = value);
        AddSlider("Joint Damping", m_Contents.jointDamping, 0f, 4f, value => m_Contents.jointDamping = value);
        AddSlider("Joint Max Force", m_Contents.jointMaxForce, 0f, 1000f, value => m_Contents.jointMaxForce = value);
        AddSlider("Anchor Offset X", m_Contents.anchorOffsetX, 0f, 0.7f, value => m_Contents.anchorOffsetX = value);
        AddSlider("Anchor Offset Y", m_Contents.anchorOffsetY, -1f, 1f, value => m_Contents.anchorOffsetY = value);

        m_DisplayImpulse0 = AddElement(new FloatField("Impulse #0") { isReadOnly = true, focusable = false });
        m_DisplayImpulse1 = AddElement(new FloatField("Impulse #1") { isReadOnly = true, focusable = false });
    }

    private void Update()
    {
        if (m_Contents == null || m_DisplayImpulse0 == null)
            return;

        m_DisplayImpulse0.value = m_Contents.impulse0;
        m_DisplayImpulse1.value = m_Contents.impulse1;
    }

    #region Internal

    [SerializeField] UserJointContents m_Contents;

    FloatField m_DisplayImpulse0;
    FloatField m_DisplayImpulse1;

    #endregion
}
