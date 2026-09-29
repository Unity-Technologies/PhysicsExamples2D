using UnityEngine;

/// <summary>
/// Adds the smash's controls to the Toolbox menu: how fast and heavy the large box is, how bouncy everything is, how far apart the small boxes are, and the collision threshold.
/// Every change rebuilds the scene so the large box is fired again into a freshly laid out field.
/// </summary>
public sealed class SmashOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Speed", m_Contents.speed, 0f, 100f, value =>
        {
            m_Contents.speed = value;
            m_Contents.Rebuild();
        });

        AddSlider("Density", m_Contents.density, 1f, 100f, value =>
        {
            m_Contents.density = value;
            m_Contents.Rebuild();
        });

        AddSlider("Bounciness", m_Contents.bounciness, 0f, 1f, value =>
        {
            m_Contents.bounciness = value;
            m_Contents.Rebuild();
        });

        AddSlider("Spacing", m_Contents.spacing, 0f, 0.5f, value =>
        {
            m_Contents.spacing = value;
            m_Contents.Rebuild();
        });

        AddSlider("Collision Threshold", m_Contents.collisionThreshold, 0f, 1f, value =>
        {
            m_Contents.collisionThreshold = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] SmashContents m_Contents;

    #endregion
}
