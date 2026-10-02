using UnityEngine;

/// <summary>
/// Adds the tumbler's controls to the Workshop menu: the debris shape and count, how fast the box turns, and the gravity scale.
/// The shape and count rebuild the scene, while the turning speed and gravity act on the running scene.
/// </summary>
public sealed class TumblerOptions : WorkshopOptionsProvider
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

        AddSlider("Angular Velocity", m_Contents.angularVelocity, -90f, 90f, value => m_Contents.angularVelocity = value);

        AddSliderInt("Debris Count", m_Contents.debrisCount, 1, 2000, value =>
        {
            m_Contents.debrisCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Gravity Scale", m_Contents.gravityScale, 0f, 2f, value => m_Contents.gravityScale = value);
    }

    #region Internal

    [SerializeField] TumblerContents m_Contents;

    #endregion
}
