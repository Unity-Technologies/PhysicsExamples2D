using UnityEngine;

/// <summary>
/// Adds the bounce house's control to the Workshop menu: which shape is fired across the room.
/// </summary>
public sealed class BounceHouseOptions : WorkshopOptionsProvider
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
    }

    #region Internal

    [SerializeField] BounceHouseContents m_Contents;

    #endregion
}
