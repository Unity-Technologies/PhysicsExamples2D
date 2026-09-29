using UnityEngine;

/// <summary>
/// Adds the soft body's controls to the Toolbox menu: how many segments it has, its size, and how its joints spring.
/// Every change rebuilds the ring.
/// </summary>
public sealed class SoftBodyOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Body Sides", m_Contents.bodySides, 3, 32, value =>
        {
            m_Contents.bodySides = value;
            m_Contents.Rebuild();
        });

        AddSlider("Body Scale", m_Contents.bodyScale, 1f, 3f, value =>
        {
            m_Contents.bodyScale = value;
            m_Contents.Rebuild();
        });

        AddSlider("Joint Frequency", m_Contents.jointFrequency, 0f, 60f, value =>
        {
            m_Contents.jointFrequency = value;
            m_Contents.Rebuild();
        });

        AddSlider("Joint Damping", m_Contents.jointDamping, 0f, 1f, value =>
        {
            m_Contents.jointDamping = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] SoftBodyContents m_Contents;

    #endregion
}
