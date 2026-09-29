using UnityEngine;

/// <summary>
/// Adds the washer's controls to the Toolbox menu: how fast the drum turns, the debris count and friction, the gravity scale, and how many paddles the drum has and how far they reach.
/// The turning speed and gravity act on the running scene, while the debris and paddle settings rebuild it.
/// </summary>
public sealed class WasherOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Motor Speed", m_Contents.motorSpeed, -90f, 90f, value => m_Contents.motorSpeed = value);

        AddSliderInt("Debris Count", m_Contents.debrisCount, 1000, 3000, value =>
        {
            m_Contents.debrisCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Debris Friction", m_Contents.debrisFriction, 0f, 1f, value =>
        {
            m_Contents.debrisFriction = value;
            m_Contents.Rebuild();
        });

        AddSlider("Gravity Scale", m_Contents.gravityScale, 0f, 2f, value => m_Contents.gravityScale = value);

        AddSliderInt("Paddle Spacing", m_Contents.paddleSpacing, 2, 18, value =>
        {
            m_Contents.paddleSpacing = value;
            m_Contents.Rebuild();
        });

        AddSlider("Paddle Scale", m_Contents.paddleScale, 0f, 1f, value =>
        {
            m_Contents.paddleScale = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] WasherContents m_Contents;

    #endregion
}
