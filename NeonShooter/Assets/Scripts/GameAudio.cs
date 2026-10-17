using Unity.Scripting.LifecycleManagement;
using UnityEngine;

/// <summary>
/// Synthesizes and plays the game's sounds: fire zap, hit blip, and kill burst.
/// Every effect is generated in code at startup; the shipped audio assets are the ambience under the main menu and the music under the run.
/// </summary>
[AutoStaticsCleanup]
public sealed partial class GameAudio : MonoBehaviour
{
    /// <summary>
    /// The scene's audio instance, or null when none is active.
    /// </summary>
    public static GameAudio instance { get; private set; }

    /// <summary>
    /// Plays the firing zap with slight pitch jitter.
    /// Rapid fire is throttled so high fire rates thicken the sound instead of stacking one clip per shot.
    /// </summary>
    public void PlayFire()
    {
        if (Time.time < m_NextFireSound)
            return;

        m_NextFireSound = Time.time + 0.05f;

        m_Source.pitch = Random.Range(0.92f, 1.08f);
        m_Source.PlayOneShot(m_FireClip, m_FireVolume);
    }

    /// <summary>
    /// Plays the short blip for a bullet striking an enemy.
    /// </summary>
    public void PlayHit()
    {
        if (Time.time < m_NextHitSound)
            return;

        m_NextHitSound = Time.time + 0.04f;

        m_Source.pitch = Random.Range(0.95f, 1.15f);
        m_Source.PlayOneShot(m_HitClip, m_HitVolume);
    }

    /// <summary>
    /// Plays the short tick for the pointer moving onto a menu control.
    /// It runs on unscaled time and ignores the listener pause, since the menu is mostly used while the game is paused.
    /// </summary>
    public void PlayUiTick()
    {
        if (Time.unscaledTime < m_NextUiSound)
            return;

        m_NextUiSound = Time.unscaledTime + 0.03f;

        m_UiSource.pitch = Random.Range(0.97f, 1.03f);
        m_UiSource.PlayOneShot(m_UiClip, m_UiVolume);
    }

    /// <summary>
    /// Plays the burst for an enemy dying.
    /// Rate-limited so mass wipes (the panic bomb) thicken instead of stacking dozens of clips.
    /// </summary>
    public void PlayKill()
    {
        if (Time.time < m_NextKillSound)
            return;

        m_NextKillSound = Time.time + 0.06f;

        m_Source.pitch = Random.Range(0.9f, 1.1f);
        m_Source.PlayOneShot(m_KillClip, m_KillVolume);
    }

    /// <summary>
    /// Plays the panic bomb's deep sweep.
    /// </summary>
    public void PlayPanic()
    {
        m_Source.pitch = 1f;
        m_Source.PlayOneShot(m_PanicClip, m_PanicVolume);
    }

    /// <summary>
    /// Plays the glassy ping for a bullet striking an enemy's shield.
    /// </summary>
    public void PlayShieldHit()
    {
        if (Time.time < m_NextShieldSound)
            return;

        m_NextShieldSound = Time.time + 0.04f;

        m_Source.pitch = Random.Range(0.95f, 1.1f);
        m_Source.PlayOneShot(m_ShieldClip, m_ShieldVolume);
    }

    /// <summary>
    /// Plays the berserker's spawn alert: an urgent two-tone klaxon stab.
    /// </summary>
    public void PlayBerserker()
    {
        m_Source.pitch = 1f;
        m_Source.PlayOneShot(m_BerserkerClip, m_BerserkerVolume);
    }

    /// <summary>
    /// Plays the set-transition warp whoosh, pitch-stretched so it resolves over the given duration.
    /// It runs on its own source: the shared one-shot source's pitch jitters with every fire and hit, which would warble a clip this long.
    /// </summary>
    public void PlayWarp(float duration)
    {
        m_WarpSource.pitch = Mathf.Clamp(m_WarpClip.length / Mathf.Max(duration, 0.1f), 0.5f, 2f);
        m_WarpSource.PlayOneShot(m_WarpClip, m_WarpVolume);
    }

    /// <summary>
    /// Starts the panic spinner's continuous mechanical drone, with a short pitch spin-up.
    /// Runs on its own looping source until <see cref="StopPanicDrone"/>.
    /// </summary>
    public void StartPanicDrone()
    {
        m_DroneStartTime = Time.time;
        m_DroneSource.pitch = DroneSpinUpPitch;
        m_DroneSource.Play();
    }

    /// <summary>
    /// Stops the panic spinner's drone.
    /// </summary>
    public void StopPanicDrone() => m_DroneSource.Stop();

    /// <summary>
    /// Starts the boss's ominous background drone, looping on its own source until stopped.
    /// </summary>
    public void StartBossDrone() => m_BossDroneSource.Play();

    /// <summary>
    /// Stops the boss drone.
    /// </summary>
    public void StopBossDrone() => m_BossDroneSource.Stop();

    /// <summary>
    /// Plays the alarm for an enemy detonating against the player.
    /// Rate-limited so several simultaneous detonations read as one hit.
    /// </summary>
    public void PlayPlayerHit()
    {
        if (Time.time < m_NextPlayerHitSound)
            return;

        m_NextPlayerHitSound = Time.time + 0.1f;

        m_Source.pitch = 1f;
        m_Source.PlayOneShot(m_PlayerHitClip, m_PlayerHitVolume);
    }

    void Awake()
    {
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Fire", 0f, 1f, () => m_FireVolume, v => m_FireVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Hit", 0f, 1f, () => m_HitVolume, v => m_HitVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Kill", 0f, 1f, () => m_KillVolume, v => m_KillVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Shield", 0f, 1f, () => m_ShieldVolume, v => m_ShieldVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Panic", 0f, 1f, () => m_PanicVolume, v => m_PanicVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Player Hit", 0f, 1f, () => m_PlayerHitVolume, v => m_PlayerHitVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Drone", 0f, 1f, () => m_DroneVolume, v => m_DroneVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Boss Drone", 0f, 1f, () => m_BossDroneVolume, v => m_BossDroneVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Berserker", 0f, 1f, () => m_BerserkerVolume, v => m_BerserkerVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Warp", 0f, 1f, () => m_WarpVolume, v => m_WarpVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "UI", 0f, 1f, () => m_UiVolume, v => m_UiVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Menu Ambience", 0f, 1f, () => m_MenuVolume, v => m_MenuVolume = v);
        GameSettings.RegisterSlider(this, GameSettings.AudioTab, "Game Music", 0f, 1f, () => m_GameVolume, v => m_GameVolume = v);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable()
    {
        // The sources live on a hidden child, never on this authored object.
        // Adding them here would change this object's component list on every play transition and drag live audio
        // inspectors into the editor's repaint path whenever it is selected, forcing a full inspector rebuild per editor tick.
        m_SourceRoot = new GameObject("AudioSources");
        m_SourceRoot.hideFlags = HideFlags.HideInHierarchy;
        m_SourceRoot.transform.SetParent(transform, false);

        m_Source = m_SourceRoot.AddComponent<AudioSource>();
        m_Source.playOnAwake = false;
        m_Source.spatialBlend = 0f;

        // The drone gets its own source so it can loop underneath the one-shots.
        m_DroneSource = m_SourceRoot.AddComponent<AudioSource>();
        m_DroneSource.playOnAwake = false;
        m_DroneSource.spatialBlend = 0f;
        m_DroneSource.loop = true;
        m_DroneSource.volume = m_DroneVolume;
        m_DroneSource.clip = SynthesizePanicDrone();

        // The boss drone loops on its own source so it can run underneath everything, including the panic drone.
        m_BossDroneSource = m_SourceRoot.AddComponent<AudioSource>();
        m_BossDroneSource.playOnAwake = false;
        m_BossDroneSource.spatialBlend = 0f;
        m_BossDroneSource.loop = true;
        m_BossDroneSource.volume = m_BossDroneVolume;
        m_BossDroneSource.clip = SynthesizeBossDrone();

        // The warp whoosh gets its own source too, so its per-play pitch stretch is isolated from the jittered one-shots.
        m_WarpSource = m_SourceRoot.AddComponent<AudioSource>();
        m_WarpSource.playOnAwake = false;
        m_WarpSource.spatialBlend = 0f;

        // The menu tick keeps sounding while the pause menu has the listener paused.
        m_UiSource = m_SourceRoot.AddComponent<AudioSource>();
        m_UiSource.playOnAwake = false;
        m_UiSource.spatialBlend = 0f;
        m_UiSource.ignoreListenerPause = true;

        // The menu ambience loops on its own source; the clip is rebuilt with its ends crossfaded so the loop has no seam.
        if (m_MenuClip != null)
        {
            m_MenuSource = m_SourceRoot.AddComponent<AudioSource>();
            m_MenuSource.playOnAwake = false;
            m_MenuSource.spatialBlend = 0f;
            m_MenuSource.loop = true;
            m_MenuSource.volume = 0f;
            m_MenuSource.clip = BuildSeamlessLoop(m_MenuClip);
        }

        // The game music loops the same way under the run, and pauses with the listener when the pause menu is up.
        if (m_GameClip != null)
        {
            m_GameSource = m_SourceRoot.AddComponent<AudioSource>();
            m_GameSource.playOnAwake = false;
            m_GameSource.spatialBlend = 0f;
            m_GameSource.loop = true;
            m_GameSource.volume = 0f;
            m_GameSource.clip = BuildSeamlessLoop(m_GameClip);
        }

        m_FireClip = SynthesizeFire();
        m_HitClip = SynthesizeHit();
        m_KillClip = SynthesizeKill();
        m_PanicClip = SynthesizePanic();
        m_PlayerHitClip = SynthesizePlayerHit();
        m_ShieldClip = SynthesizeShieldHit();
        m_BerserkerClip = SynthesizeBerserkerAlert();
        m_WarpClip = SynthesizeWarp();
        m_UiClip = SynthesizeUiTick();

        instance = this;
    }

    void OnDisable()
    {
        if (instance == this)
            instance = null;

        Destroy(m_SourceRoot);

        m_Source = null;
        m_DroneSource = null;
        m_BossDroneSource = null;
        m_WarpSource = null;
        m_UiSource = null;
        m_MenuSource = null;
        m_GameSource = null;
    }

    // The mechanical spin-up: the drone opens at a lowered pitch and glides to full speed over the ramp time.
    // The looping sources' volumes are re-applied here so their sliders tune live; the one-shots read theirs per play.
    void Update()
    {
        UpdateMenuAmbience();

        if (m_DroneSource != null)
            m_DroneSource.volume = m_DroneVolume;

        if (m_BossDroneSource != null)
            m_BossDroneSource.volume = m_BossDroneVolume;

        if (m_DroneSource == null || !m_DroneSource.isPlaying || m_DroneSource.pitch >= 1f)
            return;

        var progress = Mathf.Clamp01((Time.time - m_DroneStartTime) / DroneSpinUpDuration);
        m_DroneSource.pitch = Mathf.Lerp(DroneSpinUpPitch, 1f, progress);
    }

    // A laser bolt: two detuned oscillators sweeping exponentially from 2200 Hz to 180 Hz with a fast decay.
    // The phase accumulates per sample so the sweep is a clean glide rather than a chirp artifact.
    static AudioClip SynthesizeFire()
    {
        const float duration = 0.12f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var phaseA = 0f;
        var phaseB = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var progress = i / (float)samples.Length;
            var frequency = 2200f * Mathf.Pow(180f / 2200f, progress);

            phaseA += frequency * Mathf.PI * 2f / sampleRate;
            phaseB += frequency * 1.01f * Mathf.PI * 2f / sampleRate;

            var attack = Mathf.Clamp01(i / (0.002f * sampleRate));
            var envelope = attack * Mathf.Pow(1f - progress, 1.3f);
            var tone = Mathf.Sin(phaseA) * 0.6f + Mathf.Sin(phaseB) * 0.35f + Mathf.Sin(phaseA * 2f) * 0.2f;

            samples[i] = Mathf.Clamp(tone * envelope, -1f, 1f);
        }

        return CreateClip("Fire", samples, sampleRate);
    }

    // An impact crack: a bright noise snap over a mid tone sweeping 1400 Hz down to 700 Hz, so it cuts through the fire and kill sounds.
    static AudioClip SynthesizeHit()
    {
        const float duration = 0.08f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var random = new System.Random(777);
        var phase = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var progress = i / (float)samples.Length;
            var frequency = Mathf.Lerp(1400f, 700f, progress);

            phase += frequency * Mathf.PI * 2f / sampleRate;

            var envelope = Mathf.Pow(1f - progress, 2f);
            var crack = (float)(random.NextDouble() * 2.0 - 1.0) * Mathf.Pow(1f - progress, 5f) * 0.9f;
            var tone = Mathf.Sin(phase) * 0.6f;

            samples[i] = Mathf.Clamp((crack + tone) * envelope, -1f, 1f);
        }

        return CreateClip("Hit", samples, sampleRate);
    }

    // A tiny tap for the menu: a bright sine falling a little over a few milliseconds, with a speck of noise at the very front.
    // The whole thing is over before it reads as a tone, so it lands as a tap rather than a beep.
    static AudioClip SynthesizeUiTick()
    {
        const float duration = 0.03f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var random = new System.Random(4242);
        var phase = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var progress = i / (float)samples.Length;
            var frequency = Mathf.Lerp(2200f, 1500f, progress);

            phase += frequency * Mathf.PI * 2f / sampleRate;

            var attack = Mathf.Clamp01(i / (0.0008f * sampleRate));
            var envelope = attack * Mathf.Pow(1f - progress, 3f);
            var speck = (float)(random.NextDouble() * 2.0 - 1.0) * Mathf.Pow(1f - progress, 12f) * 0.5f;
            var tone = Mathf.Sin(phase) * 0.7f;

            samples[i] = Mathf.Clamp((tone + speck) * envelope, -1f, 1f);
        }

        return CreateClip("UiTick", samples, sampleRate);
    }

    // An energy-discharge explosion: overdriven detuned saws diving from bright to low carry the sound, with a
    // modest sine underneath for weight. The sum runs through the drone's soft clip, keeping it tonal and synthetic.
    // A high-pass on the output cuts the sub thump, so rapid kills stay zappy instead of pounding.
    static AudioClip SynthesizeKill()
    {
        const float duration = 0.45f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var bodyPhase = 0f;
        var sawPhaseA = 0f;
        var sawPhaseB = 0f;
        var low = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var progress = i / (float)samples.Length;

            // The grit saws lead, diving from bright to low; the sine body stays higher and quieter, support not boom.
            var bodyFrequency = 320f * Mathf.Pow(140f / 320f, progress);
            var sawFrequency = 700f * Mathf.Pow(150f / 700f, progress);

            bodyPhase += bodyFrequency / sampleRate;
            sawPhaseA += sawFrequency / sampleRate;
            sawPhaseB += sawFrequency * 1.03f / sampleRate;

            var body = Mathf.Sin(bodyPhase * Mathf.PI * 2f) * 0.4f;

            var gritEnvelope = Mathf.Pow(1f - progress, 1.8f);
            var grit = (2f * (sawPhaseA % 1f) - 1f + 2f * (sawPhaseB % 1f) - 1f) * 0.55f * gritEnvelope;

            var attack = Mathf.Clamp01(i / (0.003f * sampleRate));
            var envelope = attack * Mathf.Pow(1f - progress, 1.5f);

            var driven = (body + grit) * envelope * 3.5f;
            var clipped = driven / (1f + Mathf.Abs(driven));

            // One-pole high-pass around 180 Hz: subtracting the running low-pass strips the sub weight.
            low += (clipped - low) * 0.025f;

            samples[i] = clipped - low;
        }

        return CreateClip("Kill", samples, sampleRate);
    }

    // The panic sweep: a powerful dive from 900 Hz to 70 Hz with a noise swell, longer and deeper than anything else in the mix.
    static AudioClip SynthesizePanic()
    {
        const float duration = 0.55f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var random = new System.Random(424242);
        var phase = 0f;
        var rumble = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var progress = i / (float)samples.Length;
            var frequency = 900f * Mathf.Pow(70f / 900f, progress);

            phase += frequency * Mathf.PI * 2f / sampleRate;

            var white = (float)(random.NextDouble() * 2.0 - 1.0);
            rumble += (white - rumble) * 0.2f;

            var attack = Mathf.Clamp01(i / (0.005f * sampleRate));
            var envelope = attack * Mathf.Pow(1f - progress, 1.4f);
            var body = Mathf.Sin(phase) * 0.7f + Mathf.Sin(phase * 0.5f) * 0.3f + rumble * 0.8f;

            samples[i] = Mathf.Clamp(body * envelope, -1f, 1f);
        }

        return CreateClip("Panic", samples, sampleRate);
    }

    // The player-hit alarm: a dissonant two-tone buzz (a tritone apart) diving 520 Hz to 150 Hz under a fast tremolo, with a sharp opening crack.
    // The dissonance and tremolo make it read as a warning, distinct from the low kill boom and the bright bullet-hit blip.
    static AudioClip SynthesizePlayerHit()
    {
        const float duration = 0.32f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var random = new System.Random(9001);
        var phaseA = 0f;
        var phaseB = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var progress = i / (float)samples.Length;
            var frequency = 520f * Mathf.Pow(150f / 520f, progress);

            phaseA += frequency * Mathf.PI * 2f / sampleRate;
            phaseB += frequency * 1.414f * Mathf.PI * 2f / sampleRate;

            var seconds = i / (float)sampleRate;
            var tremolo = 0.6f + 0.4f * Mathf.Sin(Mathf.PI * 2f * 28f * seconds);

            var attack = Mathf.Clamp01(i / (0.003f * sampleRate));
            var envelope = attack * Mathf.Pow(1f - progress, 1.2f);
            var crack = (float)(random.NextDouble() * 2.0 - 1.0) * Mathf.Pow(1f - progress, 7f) * 0.6f;
            var tone = (Mathf.Sin(phaseA) * 0.6f + Mathf.Sin(phaseB) * 0.35f) * tremolo;

            samples[i] = Mathf.Clamp((tone + crack) * envelope, -1f, 1f);
        }

        return CreateClip("PlayerHit", samples, sampleRate);
    }

    // The panic drone: a one-second seamless loop for the spinning gun, a heavy continuous-beam sound.
    // Two detuned sawtooth oscillators (beating at 2 Hz) over a sub square carry a dense, buzzy spectrum, a band of
    // filtered noise adds electrical spit, and the whole sum is overdriven through a soft clip so it snarls instead
    // of ringing like an organ. Oscillator frequencies are whole cycles per loop, so the wrap point has no seam;
    // the noise wraps freely since a one-sample jump in noise is just more noise.
    static AudioClip SynthesizePanicDrone()
    {
        const float duration = 1f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var random = new System.Random(31337);
        var band = 0f;
        var bandLow = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var seconds = i / (float)sampleRate;

            // Naive sawtooths; the slight aliasing at this pitch reads as extra grit, which suits the beam.
            var sawA = 2f * (110f * seconds % 1f) - 1f;
            var sawB = 2f * (112f * seconds % 1f) - 1f;

            // A softened sub-octave square for weight under the saws.
            var sub = Mathf.Clamp(Mathf.Sin(Mathf.PI * 2f * 55f * seconds) * 4f, -1f, 1f);

            // Band-passed noise (low-pass minus a slower low-pass) chopped at 120 Hz for the electrical spit.
            var white = (float)(random.NextDouble() * 2.0 - 1.0);
            band += (white - band) * 0.25f;
            bandLow += (band - bandLow) * 0.05f;
            var spit = (band - bandLow) * (0.55f + 0.45f * Mathf.Sin(Mathf.PI * 2f * 120f * seconds));

            var mix = (sawA + sawB) * 0.4f + sub * 0.3f + spit * 0.5f;

            // A slow 3 Hz flutter over the 24 Hz buzz keeps the field feeling alive.
            var buzz = (0.8f + 0.2f * Mathf.Sin(Mathf.PI * 2f * 24f * seconds)) *
                       (0.9f + 0.1f * Mathf.Sin(Mathf.PI * 2f * 3f * seconds));

            // Overdrive into a soft clip: the intermodulation is what makes it snarl.
            var driven = mix * buzz * 5f;

            samples[i] = driven / (1f + Mathf.Abs(driven));
        }

        return CreateClip("PanicDrone", samples, sampleRate);
    }

    // The boss drone: a two-second seamless loop of low menace under the whole boss phase.
    // Detuned saws half a hertz apart beat exactly once per loop, a tritone sine above them carries the unease, and a
    // softened sub square adds weight; slow low-passed noise rumbles underneath. Oscillator frequencies are whole
    // cycles per loop so the wrap point has no seam, and a half-hertz breathing swell rides the sum into the soft clip.
    static AudioClip SynthesizeBossDrone()
    {
        const float duration = 2f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var random = new System.Random(86420);
        var rumble = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var seconds = i / (float)sampleRate;

            var sawA = 2f * (55f * seconds % 1f) - 1f;
            var sawB = 2f * (55.5f * seconds % 1f) - 1f;
            var tritone = Mathf.Sin(Mathf.PI * 2f * 78f * seconds);
            var sub = Mathf.Clamp(Mathf.Sin(Mathf.PI * 2f * 27.5f * seconds) * 4f, -1f, 1f);

            var white = (float)(random.NextDouble() * 2.0 - 1.0);
            rumble += (white - rumble) * 0.02f;

            var mix = (sawA + sawB) * 0.3f + tritone * 0.2f + sub * 0.35f + rumble * 1.2f;
            var swell = 0.75f + 0.25f * Mathf.Sin(Mathf.PI * 2f * 0.5f * seconds);

            var driven = mix * swell * 3f;

            samples[i] = driven / (1f + Mathf.Abs(driven));
        }

        return CreateClip("BossDrone", samples, sampleRate);
    }

    // The berserker alert: an urgent klaxon alternating between two tones a fourth apart, overdriven through the
    // soft clip so it snarls, with a slight upward pitch creep that makes it feel incoming.
    static AudioClip SynthesizeBerserkerAlert()
    {
        const float duration = 0.5f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var phase = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var progress = i / (float)samples.Length;
            var seconds = i / (float)sampleRate;

            // The klaxon alternates 640 and 850 Hz eight times a second, both creeping upward as it plays.
            var high = Mathf.Repeat(8f * seconds, 1f) < 0.5f;
            var frequency = (high ? 850f : 640f) * (1f + 0.12f * progress);

            phase += frequency / sampleRate;

            var attack = Mathf.Clamp01(i / (0.004f * sampleRate));
            var envelope = attack * Mathf.Pow(1f - progress, 0.8f);

            // A saw with a touch of octave gives the horn its edge before the drive.
            var tone = (2f * (phase % 1f) - 1f) * 0.7f + Mathf.Sin(phase * Mathf.PI * 4f) * 0.3f;

            var driven = tone * envelope * 3f;

            samples[i] = driven / (1f + Mathf.Abs(driven));
        }

        return CreateClip("BerserkerAlert", samples, sampleRate);
    }

    // The shield ping: a bright bell pair (a high tone and its fifth) with a fast shimmer, glassy against the noisy body-hit crack.
    static AudioClip SynthesizeShieldHit()
    {
        const float duration = 0.09f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var phaseA = 0f;
        var phaseB = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var progress = i / (float)samples.Length;
            var frequency = 2300f * Mathf.Pow(1700f / 2300f, progress);

            phaseA += frequency * Mathf.PI * 2f / sampleRate;
            phaseB += frequency * 1.5f * Mathf.PI * 2f / sampleRate;

            var seconds = i / (float)sampleRate;
            var shimmer = 0.75f + 0.25f * Mathf.Sin(Mathf.PI * 2f * 90f * seconds);

            var attack = Mathf.Clamp01(i / (0.001f * sampleRate));
            var envelope = attack * Mathf.Pow(1f - progress, 2.2f);
            var tone = (Mathf.Sin(phaseA) * 0.6f + Mathf.Sin(phaseB) * 0.4f) * shimmer;

            samples[i] = Mathf.Clamp(tone * envelope, -1f, 1f);
        }

        return CreateClip("ShieldHit", samples, sampleRate);
    }

    // The set-transition whoosh, mirroring the starfield's warp burst: a band-passed noise sweep rises fast to its
    // bright peak as the stars hit full speed, then glides back down as the field eases out, over a detuned saw
    // swell for body, and a soft low thump lands in the final stretch as the next set arrives.
    // The clip is authored at the default inter-set delay; PlayWarp pitch-stretches it to the live delay.
    static AudioClip SynthesizeWarp()
    {
        const float duration = 2f;
        var samples = NewSampleBuffer(duration, out var sampleRate);
        var random = new System.Random(555);
        var band = 0f;
        var bandLow = 0f;
        var sawPhaseA = 0f;
        var sawPhaseB = 0f;
        var thumpPhase = 0f;

        for (var i = 0; i < samples.Length; ++i)
        {
            var progress = i / (float)samples.Length;

            // The sweep mirrors the warp's hold-then-release: a fast squared rise, a long bright hold, and a fall only over the final third.
            // The rise spans the first eighth, so at the authored duration its peak lands on the catapult release a quarter second in.
            var rise = Mathf.Clamp01(progress / 0.125f);
            var fall = Mathf.Clamp01((progress - 0.65f) / 0.35f);

            // Band-passed noise (a fast low-pass minus a slower one) whose center tracks the sweep from a low rumble up to 2.4 kHz and back.
            var center = Mathf.Lerp(Mathf.Lerp(180f, 2400f, rise * rise), 220f, fall * fall);
            var k = Mathf.Clamp01(center * Mathf.PI * 2f / sampleRate);
            var white = (float)(random.NextDouble() * 2.0 - 1.0);
            band += (white - band) * k;
            bandLow += (band - bandLow) * k * 0.35f;
            var whoosh = (band - bandLow) * 2.2f;

            // Detuned saws under the noise give the whoosh a tonal core, following the same rise-and-fall shape an octave-ish range lower.
            var sawFrequency = Mathf.Lerp(Mathf.Lerp(55f, 130f, rise), 65f, fall);
            sawPhaseA += sawFrequency / sampleRate;
            sawPhaseB += sawFrequency * 1.02f / sampleRate;
            var swell = (2f * (sawPhaseA % 1f) - 1f + 2f * (sawPhaseB % 1f) - 1f) * 0.3f;

            // The level holds through the warp with a gentle tilt, releasing with the fall so the sound resolves as the field settles.
            var attack = Mathf.Clamp01(progress / 0.08f);
            var envelope = attack * (1f - 0.25f * progress) * Mathf.Pow(1f - fall, 1.3f);

            // The arrival thump: a short low sine dropping 85 to 55 Hz in the last stretch, timed so it reads as the next set landing.
            var thump = 0f;

            if (progress > 0.82f)
            {
                var thumpProgress = (progress - 0.82f) / 0.18f;
                thumpPhase += Mathf.Lerp(85f, 55f, thumpProgress) / sampleRate;
                thump = Mathf.Sin(thumpPhase * Mathf.PI * 2f) * Mathf.Clamp01(thumpProgress / 0.15f) * Mathf.Pow(1f - thumpProgress, 1.5f) * 0.9f;
            }

            // The whoosh and swell run through the soft clip together; the thump stays clean on top so it lands round, not snarly.
            var driven = (whoosh + swell) * envelope * 2.5f;

            samples[i] = driven / (1f + Mathf.Abs(driven)) + thump;
        }

        return CreateClip("Warp", samples, sampleRate);
    }

    // The two beds hand over at the start of the run: the ambience plays under the menu only, the music under the run until game over.
    // Each fades toward its level while wanted and toward silence otherwise, stopping once silent.
    void UpdateMenuAmbience()
    {
        var game = GameManager.instance;
        var menuUp = game != null && game.isAwaitingStart;
        var running = game != null && !game.isAwaitingStart && !game.isGameOver;

        FadeBed(m_MenuSource, menuUp, m_MenuVolume);
        FadeBed(m_GameSource, running, m_GameVolume);
    }

    static void FadeBed(AudioSource source, bool wanted, float level)
    {
        if (source == null)
            return;

        if (wanted && !source.isPlaying)
            source.Play();

        source.volume = Mathf.MoveTowards(source.volume, wanted ? level : 0f, Time.deltaTime / MenuFadeDuration);

        if (!wanted && source.volume <= 0f && source.isPlaying)
            source.Stop();
    }

    // Rebuilds a clip so it loops without a click: its last stretch is crossfaded into its first and the duplicated tail dropped.
    // The result is a little shorter than the source by the crossfade length.
    static AudioClip BuildSeamlessLoop(AudioClip source)
    {
        var channels = source.channels;
        var frames = source.samples;
        var fadeFrames = Mathf.Min(Mathf.RoundToInt(LoopCrossfadeDuration * source.frequency), frames / 2);
        var samples = new float[frames * channels];

        source.GetData(samples, 0);

        var outputFrames = frames - fadeFrames;
        var output = new float[outputFrames * channels];

        for (var frame = 0; frame < outputFrames; ++frame)
        {
            // The opening frames blend from the tail into the head, so the loop point lands on the tail's own continuation.
            var blend = frame < fadeFrames ? frame / (float)fadeFrames : 1f;
            var tailFrame = frames - fadeFrames + frame;

            for (var channel = 0; channel < channels; ++channel)
            {
                var head = samples[frame * channels + channel];
                var tail = blend < 1f ? samples[tailFrame * channels + channel] : 0f;

                output[frame * channels + channel] = head * blend + tail * (1f - blend);
            }
        }

        var clip = AudioClip.Create(source.name + " Loop", outputFrames, channels, source.frequency, false);
        clip.SetData(output, 0);

        return clip;
    }

    static float[] NewSampleBuffer(float duration, out int sampleRate)
    {
        sampleRate = 44100;
        return new float[Mathf.CeilToInt(duration * sampleRate)];
    }

    static AudioClip CreateClip(string clipName, float[] samples, int sampleRate)
    {
        var clip = AudioClip.Create(clipName, samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    #region Internal

    // The drone's spin-up: where the pitch starts and how long it takes to reach full speed.
    const float DroneSpinUpPitch = 0.8f;
    const float DroneSpinUpDuration = 0.5f;

    // How long a bed takes to fade in or out, and how much of its end is crossfaded into its start for the loop.
    const float MenuFadeDuration = 1f;
    const float LoopCrossfadeDuration = 0.75f;

    // The ambience looped under the main menu and the music looped under the run, each with its level in the mix.
    [SerializeField] AudioClip m_MenuClip;

    [SerializeField]
    [Range(0f, 1f)]
    float m_MenuVolume = 0.5f;

    [SerializeField] AudioClip m_GameClip;

    [SerializeField]
    [Range(0f, 1f)]
    float m_GameVolume = 0.5f;

    // Per-sound levels in the mix.
    [SerializeField]
    [Range(0f, 1f)]
    float m_FireVolume = 0.35f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_HitVolume = 0.9f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_KillVolume = 0.8f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_ShieldVolume = 0.8f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_PanicVolume = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_PlayerHitVolume = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_DroneVolume = 0.7f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_BossDroneVolume = 0.5f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_BerserkerVolume = 0.9f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_WarpVolume = 0.8f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_UiVolume = 0.5f;

    // The hidden child all sources are created on.
    GameObject m_SourceRoot;

    AudioSource m_Source;
    AudioSource m_DroneSource;
    AudioSource m_BossDroneSource;
    AudioSource m_WarpSource;
    AudioSource m_UiSource;
    AudioSource m_MenuSource;
    AudioSource m_GameSource;
    float m_DroneStartTime;
    AudioClip m_FireClip;
    AudioClip m_HitClip;
    AudioClip m_KillClip;
    AudioClip m_PanicClip;
    AudioClip m_PlayerHitClip;
    AudioClip m_ShieldClip;
    AudioClip m_BerserkerClip;
    AudioClip m_WarpClip;
    AudioClip m_UiClip;
    float m_NextFireSound;
    float m_NextHitSound;
    float m_NextKillSound;
    float m_NextPlayerHitSound;
    float m_NextShieldSound;
    float m_NextUiSound;

    #endregion
}
