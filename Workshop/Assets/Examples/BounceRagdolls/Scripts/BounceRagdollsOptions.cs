using UnityEngine;

/// <summary>
/// Adds the bounce ragdolls' controls to the Workshop menu: how often a ragdoll is dropped, how many are dropped, and how strong the swinging gravity is.
/// Changing the period or the count restarts the drop, while the gravity strength takes effect straight away.
/// </summary>
public sealed class BounceRagdollsOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Update Period", m_Contents.updatePeriod, 0.1f, 5f, value =>
        {
            m_Contents.updatePeriod = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Ragdoll Count", m_Contents.ragdollCount, 10, 250, value =>
        {
            m_Contents.ragdollCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Gravity Scale", m_Contents.gravityScale, 1f, 5f, value => m_Contents.gravityScale = value);
    }

    #region Internal

    [SerializeField] BounceRagdollsContents m_Contents;

    #endregion
}
