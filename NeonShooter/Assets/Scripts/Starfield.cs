using Unity.Scripting.LifecycleManagement;
using UnityEngine;

/// <summary>
/// A parallax starfield behind the arena.
/// The particle layers drift opposite the gun's aim, harder while firing, so shooting feels like thrust.
/// Stars are placed once and wrapped at the view edges, making the field infinite with a fixed particle count.
/// </summary>
[AutoStaticsCleanup]
public sealed partial class Starfield : MonoBehaviour
{
    /// <summary>
    /// The scene's starfield, or null when none is active.
    /// </summary>
    public static Starfield instance { get; private set; }

    /// <summary>
    /// Kicks the drift into a warp burst that eases back down to normal speed over the duration.
    /// The launch waits out the catapult screen effect's wind-up, holds full boost, and eases out so the field lands back at cruise exactly as the duration expires.
    /// </summary>
    public void Warp(float duration)
    {
        m_WarpDuration = Mathf.Max(duration, 0.01f);
        m_WarpTimer = m_WarpDuration;
    }

    void Awake()
    {
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Star Drift Speed", 0f, 10f, () => m_DriftSpeed, v => m_DriftSpeed = v);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Star Firing Boost", 1f, 20f, () => m_FiringBoost, v => m_FiringBoost = v);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Star Streak", 0f, 1f, () => m_StreakScale, v => m_StreakScale = v);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Star Warp Boost", 1f, 60f, () => m_WarpBoost, v => m_WarpBoost = v);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable() => instance = this;

    void OnDisable()
    {
        if (instance == this)
            instance = null;
    }

    void Start()
    {
        m_Camera = Camera.main;
        m_HalfHeight = m_Camera.orthographicSize + WrapMargin;
        m_HalfWidth = m_Camera.orthographicSize * m_Camera.aspect + WrapMargin;

        // Depth layers: nearer stars are fewer, bigger, brighter, and drift faster.
        // The hero layer is HDR-bright so its handful of stars pick up bloom like the gameplay objects.
        m_Layers = new[]
        {
            CreateLayer("Far", 320, 0.02f, 0.04f, 0.45f, 0.2f),
            CreateLayer("Mid", 200, 0.035f, 0.06f, 0.75f, 0.5f),
            CreateLayer("Near", 120, 0.05f, 0.09f, 1.2f, 1f),
            CreateLayer("Hero", 40, 0.08f, 0.14f, 2.2f, 1.4f)
        };
    }

    void Update()
    {
        // Wrap bounds track the live view so resolution and fullscreen changes are always covered.
        m_HalfHeight = m_Camera.orthographicSize + WrapMargin;
        m_HalfWidth = m_Camera.orthographicSize * m_Camera.aspect + WrapMargin;

        // The warp holds full boost from its launch and eases out over its final third, settling at cruise exactly as the duration expires.
        // The launch itself waits out the catapult's wind-up, so the slam starts on the frame the screen pinch releases.
        var warp = 0f;

        if (m_WarpTimer > 0f)
        {
            m_WarpTimer -= Time.deltaTime;

            if (m_WarpDuration - m_WarpTimer >= ScreenFxSettings.WarpWindupDuration)
            {
                var remaining = Mathf.Clamp01(m_WarpTimer / m_WarpDuration);
                var release = Mathf.Clamp01(remaining / 0.3f);
                warp = release * release;
            }
        }

        // Drift opposite the aim, boosted while firing, smoothed so direction changes glide.
        // A disabled gun (game over) counts as not firing; its isFiring freezes at its last value, so a death mid-burst would otherwise keep the boost forever.
        // On the main menu there is no aim yet, since the player is inactive, so the field cruises left the same way the warp streams.
        var target = Vector2.zero;
        var game = GameManager.instance;

        if (game != null && game.isAwaitingStart)
            target = Vector2.left * m_DriftSpeed;
        else if (m_Gun != null)
        {
            var firing = m_Gun.enabled && m_Gun.isFiring;
            target = -m_Gun.aimDirection * (m_DriftSpeed * (firing ? m_FiringBoost : 1f));
        }

        // The warp takes over the drift entirely, regardless of aim and firing, and blends back to the aim drift as it releases.
        // The stars stream left, which reads as the ship warping to the right.
        target = Vector2.Lerp(target, Vector2.left * (m_DriftSpeed * m_WarpBoost), warp);

        // The smoothing stiffens while the warp is live, so the burst snaps up instead of being flattened by the cruise-tuned glide.
        var smoothing = 3f + 9f * warp;
        m_Drift = Vector2.Lerp(m_Drift, target, 1f - Mathf.Exp(-smoothing * Time.deltaTime));

        // Streaks only appear above the base drift: 0 at cruise, ramping to full as the firing boost takes hold.
        var boostedSpeed = m_DriftSpeed * m_FiringBoost;
        var streak = Mathf.Clamp01((m_Drift.magnitude - m_DriftSpeed) / (boostedSpeed - m_DriftSpeed));

        foreach (var layer in m_Layers)
        {
            layer.renderer.velocityScale = m_StreakScale;
            MoveLayer(layer, m_Drift, streak);
        }
    }

    Layer CreateLayer(string layerName, int count, float minSize, float maxSize, float brightness, float parallax)
    {
        var go = new GameObject(layerName);
        go.transform.SetParent(transform, false);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.maxParticles = count;
        main.startLifetime = float.MaxValue;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = false;

        // Stretched billboards elongate each star along its velocity, so streak length tracks the drift speed.
        // The velocity is written purely as render data; the simulation stays paused and movement remains manual.
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = m_StarMaterial;
        renderer.sortingOrder = -10;
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 1f;
        renderer.velocityScale = m_StreakScale;

        // Place the stars once, scattered over the whole wrap area.
        var particles = new ParticleSystem.Particle[count];
        ps.Emit(count);
        ps.GetParticles(particles);

        for (var i = 0; i < count; ++i)
        {
            particles[i].position = new Vector3(Random.Range(-m_HalfWidth, m_HalfWidth), Random.Range(-m_HalfHeight, m_HalfHeight), 0f);
            particles[i].startSize = Random.Range(minSize, maxSize);
            particles[i].remainingLifetime = float.MaxValue;

            // Mostly cool white-blues with the occasional warm star for variety.
            var tint = Random.Range(0.7f, 1f);
            var color = Random.value < 0.15f
                ? new Color(brightness, brightness * tint, brightness * tint * 0.7f, 1f)
                : new Color(brightness * tint, brightness * tint, brightness, 1f);
            color.a = Random.Range(0.55f, 1f);
            particles[i].startColor = color;
        }

        ps.SetParticles(particles, count);
        ps.Pause();

        return new Layer { system = ps, renderer = renderer, buffer = particles, parallax = parallax };
    }

    void MoveLayer(Layer layer, Vector2 drift, float streak)
    {
        var count = layer.system.GetParticles(layer.buffer);
        var layerDrift = drift * layer.parallax;
        var step = (Vector3)(layerDrift * Time.deltaTime);

        // A stretched billboard with zero velocity collapses to nothing, so resting stars keep a tiny velocity: enough to orient the quad, far too small to read as a streak.
        var direction = layerDrift.sqrMagnitude > 1e-6f ? layerDrift.normalized : Vector2.right;
        var streakVelocity = (Vector3)(direction * Mathf.Max(MinStreakSpeed, layerDrift.magnitude * streak));

        for (var i = 0; i < count; ++i)
        {
            var position = layer.buffer[i].position + step;

            if (position.x > m_HalfWidth) position.x -= m_HalfWidth * 2f;
            if (position.x < -m_HalfWidth) position.x += m_HalfWidth * 2f;
            if (position.y > m_HalfHeight) position.y -= m_HalfHeight * 2f;
            if (position.y < -m_HalfHeight) position.y += m_HalfHeight * 2f;

            layer.buffer[i].position = position;
            layer.buffer[i].velocity = streakVelocity;
        }

        layer.system.SetParticles(layer.buffer, count);
    }

    sealed class Layer
    {
        public ParticleSystem system;
        public ParticleSystemRenderer renderer;
        public ParticleSystem.Particle[] buffer;
        public float parallax;
    }

    #region Internal

    // Extra wrap distance beyond the view so stars never pop at the edges.
    const float WrapMargin = 1f;

    // The resting star velocity that keeps a stretched billboard oriented without visible stretch.
    const float MinStreakSpeed = 0.01f;

    [SerializeField] GunController m_Gun;

    // The soft-dot particle material shared with the game's other effects.
    [SerializeField] Material m_StarMaterial;

    // Base drift speed in units per second, and its multiplier while firing.
    [SerializeField] float m_DriftSpeed = 1.6f;
    [SerializeField] float m_FiringBoost = 3.5f;

    // How much drift speed stretches each star into a streak; 0 keeps them round dots.
    [SerializeField]
    [Range(0f, 1f)]
    float m_StreakScale = 0.2f;

    // Peak drift multiplier of the set-transition warp burst; streak length scales with speed, so this also sets how hard the stars stretch.
    // It must comfortably exceed the firing boost, or the burst disappears into the drift that firing already produces.
    [SerializeField] float m_WarpBoost = 30f;

    Layer[] m_Layers;
    Camera m_Camera;
    Vector2 m_Drift;
    float m_HalfWidth;
    float m_HalfHeight;
    float m_WarpTimer;
    float m_WarpDuration;

    #endregion
}
