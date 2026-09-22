using UnityEngine;

/// <summary>
/// Adds the pyramid's controls to the Toolbox menu: how wide its base is, and how hard gravity leans on it.
/// Changing the base count restacks the pyramid; changing gravity acts on the stack where it stands.
/// </summary>
public sealed class LargePyramidOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Base Count", m_Contents.baseCount, 2, 150, value =>
        {
            m_Contents.baseCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Gravity Scale", m_Contents.gravityScale, 0.1f, 5f, value => m_Contents.gravityScale = value);
    }

    #region Internal

    [SerializeField] LargePyramidContents m_Contents;

    #endregion
}
