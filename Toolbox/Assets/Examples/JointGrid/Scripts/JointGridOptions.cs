using UnityEngine;

/// <summary>
/// Adds the grid's one control to the Toolbox menu: how many circles it is across and down.
/// Changing it rebuilds the grid, since every circle's size and every link's anchors are measured from the count.
/// </summary>
public sealed class JointGridOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Grid Size", m_Contents.gridSize, 10, 100, value =>
        {
            m_Contents.gridSize = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] JointGridContents m_Contents;

    #endregion
}
