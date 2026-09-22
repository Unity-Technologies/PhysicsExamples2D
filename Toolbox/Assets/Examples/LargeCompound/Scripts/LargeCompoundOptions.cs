using UnityEngine;

/// <summary>
/// Adds the compound controls to the Toolbox menu: how much material is dropped, and how many bodies it is divided into.
/// Both drop a fresh set, since the whole block is laid out from them.
/// </summary>
public sealed class LargeCompoundOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Compound Splits", m_Contents.compoundSplits, 1, 20, value =>
        {
            m_Contents.compoundSplits = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Compound Size", m_Contents.compoundSize, 50, 100, value =>
        {
            m_Contents.compoundSize = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] LargeCompoundContents m_Contents;

    #endregion
}
