using System;

using Unity.Scripting.LifecycleManagement;
using Unity.U2D.RadianceCascades;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Scene-wide full-screen effects on top of the bloom: a constant vignette framing the arena, an impact pulse of
/// chromatic aberration plus lens distortion (fired by the panic bomb and player hits), and a color drain at game over.
/// The overrides are added to a runtime instance of the volume profile, so the profile asset is never modified.
/// </summary>
[AutoStaticsCleanup]
public sealed partial class ScreenFxSettings : MonoBehaviour
{
    /// <summary>
    /// Fires the impact pulse at the given strength (0 to 1), scaling the aberration and distortion peaks.
    /// The pulse opens instantly and eases away over the pulse duration; with no instance active this does nothing.
    /// </summary>
    public static void Pulse(float strength)
    {
        if (s_Instance == null)
            return;

        s_Instance.m_PulseStrength = Mathf.Clamp01(strength);
        s_Instance.m_PulseUntil = Time.time + s_Instance.m_PulseDuration;
    }

    /// <summary>
    /// Holds the chromatic aberration at the given strength (0 to 1) until released, for effects with a duration
    /// like the panic bomb's firing run; the transient pulse still applies on top while it is stronger.
    /// Pass 0 to release; the hold glides in and out rather than snapping.
    /// </summary>
    public static void HoldAberration(float strength)
    {
        if (s_Instance != null)
            s_Instance.m_HoldTarget = Mathf.Clamp01(strength);
    }

    /// <summary>
    /// Holds the arena's lighting down by the given number of stops until released, for effects that put far more light on screen
    /// than the run normally carries, like the panic bomb's firing run. Pass 0 to release.
    /// The hold glides in and out rather than snapping, so the arena dims into the effect instead of flicking dark.
    /// </summary>
    public static void HoldExposure(float stops)
    {
        if (s_Instance != null)
            s_Instance.m_ExposureTarget = stops;
    }

    /// <summary>
    /// Turns the boss unease on or off: a slow lens-distortion wobble with a raised aberration floor, held for as long as the boss lives.
    /// It sits under the transient pulses, blips, and warp, which all still read on top; the level glides in and out rather than snapping.
    /// </summary>
    public static void SetBossMode(bool active)
    {
        if (s_Instance != null)
            s_Instance.m_BossActive = active;
    }

    /// <summary>
    /// Fires the catapult-warp screen effect at the start of a set transition.
    /// The screen is drawn inward over a short wind-up, snaps out through an outward overshoot at the release, then settles flat.
    /// The chromatic aberration rises with the tension and fades with the settle; with no instance active this does nothing.
    /// </summary>
    public static void Warp()
    {
        if (s_Instance != null)
            s_Instance.m_WarpUntil = Time.time + WarpFxDuration;
    }

    /// <summary>
    /// The kill blip: snaps the aberration to the kill value for an instant, decaying straight back to rest.
    /// Ignored while a hold (the panic bomb) is active, so kills during the run do not fight its sustained level.
    /// </summary>
    public static void KillBlip()
    {
        if (s_Instance != null && s_Instance.m_HoldTarget <= 0f)
            s_Instance.m_BlipUntil = Time.time + s_Instance.m_KillBlipDuration;
    }

    void Awake()
    {
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Vignette", 0f, 1f, () => m_Vignette, v => m_Vignette = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Aberration Rest", 0f, 0.5f, () => m_AberrationRest, v => m_AberrationRest = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Kill Aberration", 0f, 1f, () => m_KillAberration, v => m_KillAberration = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Kill Blip Duration", 0.02f, 1f, () => m_KillBlipDuration, v => m_KillBlipDuration = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Aberration Peak", 0f, 1f, () => m_AberrationPeak, v => m_AberrationPeak = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Distortion Peak", 0f, 1f, () => m_DistortionPeak, v => m_DistortionPeak = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Pulse Duration", 0.05f, 2f, () => m_PulseDuration, v => m_PulseDuration = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Warp Pinch", 0f, 1f, () => m_WarpPinch, v => m_WarpPinch = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Warp Aberration", 0f, 1f, () => m_WarpAberration, v => m_WarpAberration = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Boss Wobble", 0f, 0.5f, () => m_BossWobble, v => m_BossWobble = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Boss Wobble Frequency", 0.05f, 5f, () => m_BossWobbleFrequency, v => m_BossWobbleFrequency = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Boss Aberration Floor", 0f, 1f, () => m_BossAberrationFloor, v => m_BossAberrationFloor = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Desaturate Duration", 0.05f, 5f, () => m_DesaturateDuration, v => m_DesaturateDuration = v);
        GameSettings.RegisterSlider(this, GameSettings.ScreenFxTab, "Grain", 0f, 1f, () => m_Grain, v => m_Grain = v);
        GameSettings.RegisterChoice(this, GameSettings.ScreenFxTab, "Grain Type", Enum.GetNames(typeof(FilmGrainLookup)), () => (int)m_GrainType, v => m_GrainType = (FilmGrainLookup)v);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable()
    {
        if (m_Volume == null)
            m_Volume = FindAnyObjectByType<Volume>();

        if (m_Volume == null)
            return;

        // volume.profile is a runtime clone of the shared asset, so these overrides live only in play mode.
        var profile = m_Volume.profile;

        m_VignetteOverride = GetOrAdd<Vignette>(profile);
        m_VignetteOverride.intensity.overrideState = true;
        m_VignetteOverride.smoothness.overrideState = true;
        m_VignetteOverride.smoothness.value = 0.4f;

        m_AberrationOverride = GetOrAdd<ChromaticAberration>(profile);
        m_AberrationOverride.intensity.overrideState = true;

        m_DistortionOverride = GetOrAdd<LensDistortion>(profile);
        m_DistortionOverride.intensity.overrideState = true;

        m_ColorOverride = GetOrAdd<ColorAdjustments>(profile);
        m_ColorOverride.saturation.overrideState = true;

        m_GrainOverride = GetOrAdd<FilmGrain>(profile);
        m_GrainOverride.intensity.overrideState = true;
        m_GrainOverride.type.overrideState = true;

        s_Instance = this;
    }

    void OnDisable()
    {
        if (s_Instance == this)
        {
            s_Instance = null;

            RadianceCascades2DGIFeature.exposureOffset = 0f;
        }
    }

    void Update()
    {
        // Ahead of the volume overrides, since the lighting reads this whether or not a volume was ever found.
        m_Exposure = Mathf.MoveTowards(m_Exposure, m_ExposureTarget, Time.deltaTime * m_ExposureGlideRate);

        RadianceCascades2DGIFeature.exposureOffset = m_Exposure;

        if (m_VignetteOverride == null)
            return;

        m_VignetteOverride.intensity.value = m_Vignette;

        m_GrainOverride.intensity.value = m_Grain;
        m_GrainOverride.type.value = m_GrainType;

        // The impact pulse: full strength at the hit instant, easing away quadratically for a snap-then-tail feel.
        var pulse = Mathf.Clamp01((m_PulseUntil - Time.time) / m_PulseDuration);
        pulse *= pulse * m_PulseStrength;

        // The sustained hold glides toward its target; the aberration takes whichever of pulse and hold is stronger.
        m_Hold = Mathf.MoveTowards(m_Hold, m_HoldTarget, Time.deltaTime * 3f);

        // The kill blip snaps to its own value and decays back within its short window.
        var blip = m_KillAberration * Mathf.Clamp01((m_BlipUntil - Time.time) / m_KillBlipDuration);

        // The catapult warp: a quadratic pinch inward during the wind-up, a hard snap out through the overshoot, then a quadratic settle back to flat.
        var warpDistortion = 0f;
        var warpAberration = 0f;

        if (m_WarpUntil > Time.time)
        {
            var warpTime = Time.time - (m_WarpUntil - WarpFxDuration);

            if (warpTime < WarpWindupDuration)
            {
                var windup = warpTime / WarpWindupDuration;
                warpDistortion = -m_WarpPinch * windup * windup;
                warpAberration = m_WarpAberration * windup;
            }
            else if (warpTime < WarpWindupDuration + WarpSnapDuration)
            {
                // The release leaves fast and decelerates into the overshoot, like the tension letting go.
                var snap = (warpTime - WarpWindupDuration) / WarpSnapDuration;
                var ease = 1f - (1f - snap) * (1f - snap);
                warpDistortion = Mathf.Lerp(-m_WarpPinch, m_WarpPinch * WarpOvershootRatio, ease);
                warpAberration = m_WarpAberration;
            }
            else
            {
                var settle = (warpTime - WarpWindupDuration - WarpSnapDuration) / WarpSettleDuration;
                var ease = (1f - settle) * (1f - settle);
                warpDistortion = m_WarpPinch * WarpOvershootRatio * ease;
                warpAberration = m_WarpAberration * ease;
            }
        }

        // The boss unease glides in and out; the wobble is a slow breathing distortion the transient effects ride on top of.
        m_BossBlend = Mathf.MoveTowards(m_BossBlend, m_BossActive ? 1f : 0f, Time.deltaTime * 3f);

        var bossDistortion = m_BossBlend * m_BossWobble * Mathf.Sin(Mathf.PI * 2f * m_BossWobbleFrequency * Time.time);
        var bossAberration = m_BossBlend * m_BossAberrationFloor;

        // The aberration takes whichever contributor is strongest: the resting floor, the kill blip, the pulse-or-hold peak, the warp, and the boss floor.
        var aberration = Mathf.Max(m_AberrationRest, blip);
        aberration = Mathf.Max(aberration, m_AberrationPeak * Mathf.Max(pulse, m_Hold));
        aberration = Mathf.Max(aberration, warpAberration);
        aberration = Mathf.Max(aberration, bossAberration);

        m_AberrationOverride.intensity.value = aberration;
        m_DistortionOverride.intensity.value = Mathf.Clamp(-m_DistortionPeak * pulse + warpDistortion + bossDistortion, -1f, 1f);

        // Game over drains the color, leaving its text on a grey arena; the menu keeps the arena in color so the lit grid shows behind it.
        var game = GameManager.instance;
        var drained = game != null && game.isGameOver;

        m_Desaturation = Mathf.MoveTowards(m_Desaturation, drained ? 100f : 0f, Time.deltaTime * 100f / m_DesaturateDuration);
        m_ColorOverride.saturation.value = -m_Desaturation;
    }

    static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (!profile.TryGet(out T component))
            component = profile.Add<T>();

        component.active = true;

        return component;
    }

    #region Internal

    // The catapult warp's timeline: the inward wind-up, the snap through the overshoot, the settle back to flat, and their total.
    // The wind-up is visible to the starfield, which holds its launch until the release so the catapult reads as flinging the field.
    internal const float WarpWindupDuration = 0.25f;
    const float WarpSnapDuration = 0.12f;
    const float WarpSettleDuration = 0.45f;
    const float WarpFxDuration = WarpWindupDuration + WarpSnapDuration + WarpSettleDuration;

    // How far the snap overshoots outward, as a fraction of the inward pinch.
    const float WarpOvershootRatio = 0.5f;

    // The global volume carrying the profile; found automatically when not assigned.
    [SerializeField] Volume m_Volume;

    // The constant vignette strength framing the arena.
    [SerializeField]
    [Range(0f, 1f)]
    float m_Vignette = 0.25f;

    // The subtle aberration always present at rest; pulses and holds rise above it, never below.
    [SerializeField]
    [Range(0f, 0.5f)]
    float m_AberrationRest = 0.08f;

    // The aberration value an enemy kill snaps to, and how quickly it falls back to rest.
    [SerializeField]
    [Range(0f, 1f)]
    float m_KillAberration = 0.25f;

    [SerializeField]
    [Min(0.02f)]
    float m_KillBlipDuration = 0.12f;

    // The impact pulse peaks at full strength, and how long a pulse takes to fade.
    [SerializeField]
    [Range(0f, 1f)]
    float m_AberrationPeak = 0.6f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_DistortionPeak = 0.4f;

    [SerializeField]
    [Min(0.05f)]
    float m_PulseDuration = 0.35f;

    // The catapult warp's strengths: how far the wind-up pinches the screen inward, and the aberration held through the tension.
    [SerializeField]
    [Range(0f, 1f)]
    float m_WarpPinch = 0.5f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_WarpAberration = 0.5f;

    // The boss unease: the distortion wobble's swing and rate, and the aberration floor held while the boss lives.
    // Deliberately heavy so the boss visibly warps the whole screen; the impact pulses sum on top and the warp and panic hold still exceed the floor.
    [SerializeField]
    [Range(0f, 0.5f)]
    float m_BossWobble = 0.3f;

    [SerializeField]
    [Min(0.05f)]
    float m_BossWobbleFrequency = 0.35f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_BossAberrationFloor = 0.45f;

    // How quickly the lighting hold glides toward whatever it has been asked for, in stops per second.
    [SerializeField]
    [Min(0.1f)]
    float m_ExposureGlideRate = 4f;

    // How long the game-over color drain takes.
    [SerializeField]
    [Min(0.05f)]
    float m_DesaturateDuration = 0.5f;

    // The film grain strength (0 is off) and its speckle coarseness, from Thin up to Large.
    [SerializeField]
    [Range(0f, 1f)]
    float m_Grain = 1f;

    [SerializeField]
    FilmGrainLookup m_GrainType = FilmGrainLookup.Medium1;

    Vignette m_VignetteOverride;
    ChromaticAberration m_AberrationOverride;
    LensDistortion m_DistortionOverride;
    ColorAdjustments m_ColorOverride;
    FilmGrain m_GrainOverride;
    float m_PulseStrength;
    float m_PulseUntil;
    float m_HoldTarget;
    float m_Hold;
    float m_BlipUntil;
    float m_WarpUntil;
    float m_Desaturation;
    float m_ExposureTarget;
    float m_Exposure;
    bool m_BossActive;
    float m_BossBlend;

    static ScreenFxSettings s_Instance;

    #endregion
}
