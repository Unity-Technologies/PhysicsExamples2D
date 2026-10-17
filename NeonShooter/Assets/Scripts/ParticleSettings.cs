using Unity.Scripting.LifecycleManagement;
using UnityEngine;

/// <summary>
/// Scene-wide tuning for the spawned particle effects: the bullet hit sparks and the enemy death bursts.
/// One instance lives in the scene so the effects can be tuned live from a single place; the multipliers are applied
/// to each effect as it spawns, on top of whatever its prefab authors.
/// </summary>
[AutoStaticsCleanup]
public sealed partial class ParticleSettings : MonoBehaviour
{
    /// <summary>
    /// Applies the live multipliers to a freshly spawned effect instance, after its start color has been tinted.
    /// Quantity scales the emission bursts and rate, lifetime and speed scale the particles' start values.
    /// Variation spreads each particle's intensity randomly below the tint, and glow pushes the material color
    /// past 1 so the whole effect blooms; vertex colors clamp at 1, so the HDR lift has to come from the material.
    /// With no instance in the scene this does nothing, leaving the prefab's authored values.
    /// </summary>
    public static void Apply(ParticleSystem system)
    {
        if (s_Instance == null)
            return;

        var main = system.main;
        main.startLifetimeMultiplier *= s_Instance.m_Lifetime;
        main.startSpeedMultiplier *= s_Instance.m_Speed;

        if (s_Instance.m_Variation > 0f)
        {
            var full = main.startColor.color;
            var dimmed = full * Mathf.Lerp(1f, 0.3f, s_Instance.m_Variation);
            dimmed.a = full.a;
            main.startColor = new ParticleSystem.MinMaxGradient(dimmed, full);
        }

        if (s_Instance.m_Glow > 1f)
        {
            s_GlowBlock ??= new MaterialPropertyBlock();
            s_GlowBlock.SetColor(s_BaseColorId, new Color(s_Instance.m_Glow, s_Instance.m_Glow, s_Instance.m_Glow, 1f));
            system.GetComponent<ParticleSystemRenderer>().SetPropertyBlock(s_GlowBlock);
        }

        var emission = system.emission;
        emission.rateOverTimeMultiplier *= s_Instance.m_Quantity;

        for (var i = 0; i < emission.burstCount; ++i)
        {
            var burst = emission.GetBurst(i);
            burst.count = burst.count.constant * s_Instance.m_Quantity;
            emission.SetBurst(i, burst);
        }
    }

    void Awake()
    {
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Particle Quantity", 0.25f, 10f, () => m_Quantity, v => m_Quantity = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Particle Lifetime", 0.25f, 10f, () => m_Lifetime, v => m_Lifetime = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Particle Speed", 0.25f, 10f, () => m_Speed, v => m_Speed = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Particle Variation", 0f, 1f, () => m_Variation, v => m_Variation = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Particle Glow", 1f, 8f, () => m_Glow, v => m_Glow = v);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable() => s_Instance = this;

    void OnDisable()
    {
        if (s_Instance == this)
            s_Instance = null;
    }

    #region Internal

    [SerializeField]
    [Range(0.25f, 10f)]
    float m_Quantity = 1f;

    [SerializeField]
    [Range(0.25f, 10f)]
    float m_Lifetime = 1f;

    [SerializeField]
    [Range(0.25f, 10f)]
    float m_Speed = 1f;

    // 0 keeps every particle at the tint color; 1 rolls each particle anywhere down to 30% of it.
    [SerializeField]
    [Range(0f, 1f)]
    float m_Variation = 0f;

    // Multiplies the particle material color; above 1 the effect crosses the bloom threshold and glows.
    [SerializeField]
    [Range(1f, 8f)]
    float m_Glow = 1f;

    // The shared property block for the glow tint; the same values go to every spawn, so one block serves all.
    // The property id is deterministic per session, so it opts out of the automatic statics reset.
    [NoAutoStaticsCleanup]
    static readonly int s_BaseColorId = Shader.PropertyToID("_BaseColor");
    static MaterialPropertyBlock s_GlowBlock;

    static ParticleSettings s_Instance;

    #endregion
}
