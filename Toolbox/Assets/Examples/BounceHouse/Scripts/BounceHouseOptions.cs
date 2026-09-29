using UnityEngine;

/// <summary>
/// Adds the bounce house's control to the Toolbox menu: which shape is fired across the room.
/// </summary>
public sealed class BounceHouseOptions : ToolboxOptionsProvider
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
