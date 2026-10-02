using UnityEngine;

/// <summary>
/// Adds the bounciness row's controls to the Workshop menu: which shape the row uses, and how hard gravity pulls on it.
/// Both rebuild the row so every shape is dropped again from the same height.
/// </summary>
public sealed class BouncinessOptions : WorkshopOptionsProvider
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

        AddSlider("Gravity Scale", m_Contents.gravityScale, 1f, 10f, value =>
        {
            m_Contents.gravityScale = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] BouncinessContents m_Contents;

    #endregion
}
