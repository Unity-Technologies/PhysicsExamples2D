using UnityEngine;

/// <summary>
/// Adds the ragdoll's control to the Toolbox menu: its size, which resizes the ragdoll where it is.
/// </summary>
public sealed class ScaleRagdollOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Ragdoll Scale", m_Contents.ragdollScale, 0.5f, 10f, value => m_Contents.ragdollScale = value);
    }

    #region Internal

    [SerializeField] ScaleRagdollContents m_Contents;

    #endregion
}
