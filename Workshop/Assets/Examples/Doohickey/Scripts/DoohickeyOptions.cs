using UnityEngine;

/// <summary>
/// Adds the doohickey control to the Workshop menu: how many doohickeys are stacked, which restacks them when changed.
/// </summary>
public sealed class DoohickeyOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Doohickey Count", m_Contents.doohickeyCount, 1, 10, value =>
        {
            m_Contents.doohickeyCount = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] DoohickeyContents m_Contents;

    #endregion
}
