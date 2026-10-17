using UnityEngine;

/// <summary>
/// Emphasis when an enemy detonates against the player: a warning sound, a subtle camera shake, and a short
/// flicker of the player sprites between their own color and a hit color.
/// Lives on the player; the PlayerHitSystem triggers it.
/// </summary>
public sealed class PlayerHitFeedback : MonoBehaviour
{
    /// <summary>
    /// Plays the full hit feedback: sound, camera shake, and sprite flicker.
    /// </summary>
    public void OnHit()
    {
        if (GameAudio.instance != null)
            GameAudio.instance.PlayPlayerHit();

        if (m_Shake != null)
            m_Shake.Shake(m_ShakeAmplitude, m_ShakeDuration);

        ScreenFxSettings.Pulse(0.7f);

        m_FlickerUntil = Time.time + m_FlickerDuration;
    }

    void Awake()
    {
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Hit Flicker Duration", 0f, 2f, () => m_FlickerDuration, v => m_FlickerDuration = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Hit Flicker Rate", 1f, 60f, () => m_FlickerRate, v => m_FlickerRate = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Shake Amplitude", 0f, 1f, () => m_ShakeAmplitude, v => m_ShakeAmplitude = v);
        GameSettings.RegisterSlider(this, GameSettings.EffectsTab, "Shake Duration", 0f, 2f, () => m_ShakeDuration, v => m_ShakeDuration = v);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable()
    {
        // Capture each sprite's own color so the flicker can restore it exactly.
        m_Sprites = GetComponentsInChildren<SpriteRenderer>();
        m_BaseColors = new Color[m_Sprites.Length];

        for (var i = 0; i < m_Sprites.Length; ++i)
            m_BaseColors[i] = m_Sprites[i].color;

        if (m_Shake == null && Camera.main != null)
            m_Shake = Camera.main.GetComponent<CameraShake>();
    }

    void Update()
    {
        if (m_FlickerUntil <= 0f)
            return;

        // Alternate between the hit color and each sprite's own color; the final pass lands on the own colors and stops.
        var flickering = Time.time < m_FlickerUntil;
        var hit = flickering && Mathf.FloorToInt((m_FlickerUntil - Time.time) * m_FlickerRate) % 2 == 0;

        for (var i = 0; i < m_Sprites.Length; ++i)
            m_Sprites[i].color = hit ? m_HitColor : m_BaseColors[i];

        if (!flickering)
            m_FlickerUntil = 0f;
    }

    #region Internal

    // The camera shake target; found on the main camera when not assigned.
    [SerializeField] CameraShake m_Shake;

    // The flash color and how long/fast the flicker runs.
    [SerializeField]
    [ColorUsage(true, true)]
    Color m_HitColor = new(3.2f, 1.2f, 0.25f, 1f);

    [SerializeField] float m_FlickerDuration = 0.45f;
    [SerializeField] float m_FlickerRate = 18f;

    // Shake strength and length; deliberately subtle.
    [SerializeField] float m_ShakeAmplitude = 0.12f;
    [SerializeField] float m_ShakeDuration = 0.3f;

    SpriteRenderer[] m_Sprites;
    Color[] m_BaseColors;
    float m_FlickerUntil;

    #endregion
}
