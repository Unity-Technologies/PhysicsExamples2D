using UnityEngine;

/// <summary>
/// Adds the ignore joint's control to the Workshop menu: whether the joint is stopping contacts between the two boxes.
/// </summary>
public sealed class IgnoreJointOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddToggle("Enable Joint", m_Contents.enableJoint, value => m_Contents.enableJoint = value);
    }

    #region Internal

    [SerializeField] IgnoreJointContents m_Contents;

    #endregion
}
