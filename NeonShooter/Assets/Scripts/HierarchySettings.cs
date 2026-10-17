using Unity.Scripting.LifecycleManagement;
using UnityEngine;

/// <summary>
/// Scene-wide control over whether spawned enemies and bullets show up in the Hierarchy window.
/// One instance lives in the scene so every spawning system reads it through the static accessor,
/// which falls back to the default when no instance is active.
/// </summary>
[AutoStaticsCleanup]
public sealed partial class HierarchySettings : MonoBehaviour
{
    /// <summary>
    /// Whether spawned enemies and bullets are hidden from the Hierarchy window.
    /// True by default, since a large wave's per-spawn hierarchy refresh costs more editor frame time
    /// than the enemies themselves; turn it off to show the physics components a demo is meant to showcase.
    /// </summary>
    public static bool hideSpawned => s_Instance == null || s_Instance.m_HideSpawned;

    void OnEnable() => s_Instance = this;

    void OnDisable()
    {
        if (s_Instance == this)
            s_Instance = null;
    }

    #region Internal

    [SerializeField]
    [Tooltip("Hide spawned enemies and bullets from the Hierarchy window. Turn off to show the physics components live.")]
    bool m_HideSpawned = true;

    static HierarchySettings s_Instance;

    #endregion
}
