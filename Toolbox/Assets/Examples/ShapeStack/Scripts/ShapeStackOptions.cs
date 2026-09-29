using UnityEngine;

/// <summary>
/// Adds the shape stack's controls to the Toolbox menu: the tower's shape and height, which rebuild it, and the world's contact and gravity settings, which act on it live.
/// </summary>
public sealed class ShapeStackOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddEnum("Object Type", m_Contents.objectType, value =>
        {
            m_Contents.objectType = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Stack Height", m_Contents.stackHeight, 2, 20, value =>
        {
            m_Contents.stackHeight = value;
            m_Contents.Rebuild();
        });

        AddSlider("Contact Frequency", m_Contents.contactFrequency, 0f, 120f, value => m_Contents.contactFrequency = value);
        AddSlider("Contact Damping", m_Contents.contactDamping, 0f, 100f, value => m_Contents.contactDamping = value);
        AddSlider("Contact Speed", m_Contents.contactSpeed, 0f, 10f, value => m_Contents.contactSpeed = value);
        AddSlider("Gravity Scale", m_Contents.gravityScale, 0f, 20f, value => m_Contents.gravityScale = value);
    }

    #region Internal

    [SerializeField] ShapeStackContents m_Contents;

    #endregion
}
