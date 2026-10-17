using Unity.Scripting.LifecycleManagement;
using UnityEngine;

/// <summary>
/// Scene-wide tuning for the enemy shield hit flash.
/// One instance lives in the scene so the flash can be tuned live from a single place; every enemy reads it through
/// the static accessors, which fall back to the defaults when no instance is active.
/// </summary>
[AutoStaticsCleanup]
public sealed partial class ShieldFlashSettings : MonoBehaviour
{
    /// <summary>
    /// How long a flash takes to fade.
    /// </summary>
    public static float duration => s_Instance != null ? s_Instance.m_Duration : DefaultDuration;

    /// <summary>
    /// The flash opacity at the hit instant.
    /// </summary>
    public static float peakAlpha => s_Instance != null ? s_Instance.m_PeakAlpha : DefaultPeakAlpha;

    /// <summary>
    /// How far past 1 the HDR color is pushed at the peak, which drives the bloom strength.
    /// </summary>
    public static float glow => s_Instance != null ? s_Instance.m_Glow : DefaultGlow;

    /// <summary>
    /// How much larger than rest the ring opens before settling onto the physics rim.
    /// </summary>
    public static float punch => s_Instance != null ? s_Instance.m_Punch : DefaultPunch;

    void Awake()
    {
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Shield Flash Duration", 0.05f, 1f, () => m_Duration, v => m_Duration = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Shield Peak Alpha", 0f, 1f, () => m_PeakAlpha, v => m_PeakAlpha = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Shield Glow", 1f, 4f, () => m_Glow, v => m_Glow = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Shield Punch", 0f, 0.5f, () => m_Punch, v => m_Punch = v);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable() => s_Instance = this;

    void OnDisable()
    {
        if (s_Instance == this)
            s_Instance = null;
    }

    #region Internal

    // The values used when no instance is active, mirrored as the field defaults.
    const float DefaultDuration = 0.3f;
    const float DefaultPeakAlpha = 1f;
    const float DefaultGlow = 2f;
    const float DefaultPunch = 0.15f;

    [SerializeField]
    [Range(0.05f, 1f)]
    float m_Duration = DefaultDuration;

    [SerializeField]
    [Range(0f, 1f)]
    float m_PeakAlpha = DefaultPeakAlpha;

    [SerializeField]
    [Range(1f, 4f)]
    float m_Glow = DefaultGlow;

    [SerializeField]
    [Range(0f, 0.5f)]
    float m_Punch = DefaultPunch;

    static ShieldFlashSettings s_Instance;

    #endregion
}
