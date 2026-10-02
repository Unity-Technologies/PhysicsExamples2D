using UnityEngine;

/// <summary>
/// Adds the confined example's control to the Workshop menu: how many circles are packed along each side of the grid, which refills the box.
/// </summary>
public sealed class ConfinedOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Grid Count", m_Contents.gridCount, 20, 50, value =>
        {
            m_Contents.gridCount = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] ConfinedContents m_Contents;

    #endregion
}
