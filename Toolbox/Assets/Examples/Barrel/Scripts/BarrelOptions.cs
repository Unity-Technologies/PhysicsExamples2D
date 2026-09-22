using UnityEngine;

/// <summary>
/// Adds the barrel's own controls to the Toolbox menu: what the barrel is filled with, and how hard objects must hit to count as colliding.
/// Both refill the barrel, so the values they set survive the refill rather than being read back from the scene.
/// </summary>
public sealed class BarrelOptions : ToolboxOptionsProvider
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

        AddSlider("Collision Threshold", m_Contents.collisionThreshold, 0f, 1f, value =>
        {
            m_Contents.collisionThreshold = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] BarrelContents m_Contents;

    #endregion
}
