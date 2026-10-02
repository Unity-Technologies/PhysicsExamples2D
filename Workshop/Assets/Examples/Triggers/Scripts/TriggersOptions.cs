using UnityEngine;

/// <summary>
/// Adds the triggers' control to the Workshop menu: how many columns of triggers the field has, which is also how many circles are dropped through it in each row.
/// Changing the column count rebuilds the scene.
/// </summary>
public sealed class TriggersOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Column Count", m_Contents.columnCount, 10, 500, value =>
        {
            m_Contents.columnCount = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] TriggersContents m_Contents;

    #endregion
}
