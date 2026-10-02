using UnityEngine;

/// <summary>
/// Adds the rolling resistance example's control to the Workshop menu: the tilt of every ramp, which rebuilds the whole stack.
/// </summary>
public sealed class RollingResistanceOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddEnum("Slope Type", m_Contents.slopeType, value =>
        {
            m_Contents.slopeType = value;
            m_Contents.Rebuild();
        });
    }

    #region Internal

    [SerializeField] RollingResistanceContents m_Contents;

    #endregion
}
