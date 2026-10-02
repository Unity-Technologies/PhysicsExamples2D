using UnityEngine;

/// <summary>
/// Adds the chain shape stream's controls to the Workshop menu: which shape slides, how many, how hard gravity pulls, and the collision threshold each one spawns with.
/// Every one of them restarts the stream so all the objects share the same settings.
/// </summary>
public sealed class ChainShapeOptions : WorkshopOptionsProvider
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

        AddSliderInt("Object Count", m_Contents.objectCount, 1, 100, value =>
        {
            m_Contents.objectCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Gravity Scale", m_Contents.gravityScale, 1f, 20f, value =>
        {
            m_Contents.gravityScale = value;
            m_Contents.Rebuild();
        });

        AddSlider("Collision Threshold", m_Contents.collisionThreshold, 0f, 1f, value =>
        {
            m_Contents.collisionThreshold = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] ChainShapeContents m_Contents;

    #endregion
}
