using System;
using Unity.Scripting.LifecycleManagement;
using Unity.U2D.Physics;
using Unity.U2D.RadianceCascades;
using UnityEngine;

/// <summary>
/// One enemy in a wave.
/// Holds health, wave identity, and the shield: a larger circle shape that takes the bullet hits (and flashes briefly)
/// until the enemy is one hit from death, when it drops and hits land on the core shapes.
/// Movement is owned by the <see cref="WaveMovementSystem"/>; the only per-frame work here is the shield flash fade.
/// </summary>
[AutoStaticsCleanup]
[RequireComponent(typeof(PhysicsPose))]
public sealed partial class Enemy : MonoBehaviour
{
    /// <summary>
    /// Raised when this enemy is destroyed for any reason; the spawner uses it to track wave progress.
    /// </summary>
    public event Action<Enemy> Destroyed;

    /// <summary>
    /// The wave this enemy belongs to.
    /// </summary>
    public WaveDefinition wave => m_Wave;

    /// <summary>
    /// The tint this enemy shows: its wave color, or the per-enemy override a boss-fired berserker carries.
    /// </summary>
    public Color color => m_Color;

    /// <summary>
    /// Whether the shield is still up, taking the hits at its rim.
    /// </summary>
    public bool isShielded => m_ShieldArea != null && m_ShieldArea.enabled;

    /// <summary>
    /// A uniform multiplier on the body sprite's authored scale, purely visual; the berserker pulses it.
    /// </summary>
    public float visualScale
    {
        get => m_VisualScale;
        set
        {
            m_VisualScale = value;

            if (m_BodyInstance != null)
                m_BodyInstance.scale = m_BodyScale * value;
        }
    }

    /// <summary>
    /// Registers a hit on the shield: it pops visible, fades out quickly, and scores a tenth of the kill value.
    /// </summary>
    public void RegisterShieldHit()
    {
        m_ShieldFlashUntil = Time.time + ShieldFlashSettings.duration;

        if (GameManager.instance != null)
            GameManager.instance.AddScore(m_Score / 10);
    }

    /// <summary>
    /// Applies wave identity, the score awarded when the player's fire kills this enemy, and registers with the movement system.
    /// Call immediately after instantiating the enemy at its spawn position.
    /// An optional color replaces the wave color everywhere it shows: body tint, shield flash, and death burst.
    /// </summary>
    public void Initialize(WaveDefinition wave, Vector2 spawnPosition, WaveMovementSystem movement, int score, Color? colorOverride = null)
    {
        Setup(wave, score, colorOverride ?? wave.color);
        m_Health = wave.health;

        m_Movement = movement;
        m_MovementIndex = movement.Register(this, GetComponent<PhysicsPose>().body, spawnPosition, wave);
    }

    /// <summary>
    /// Initializes this enemy as one element of the boss chain: wave identity for its look, an overridden health so the shield lasts, and an orbit path.
    /// Call immediately after instantiating the element at its starting orbit position.
    /// </summary>
    public void InitializeBossElement(WaveDefinition wave, WaveMovementSystem movement, in WaveMovementSystem.OrbitPath path, int score, float health)
    {
        Setup(wave, score, wave.color);
        m_Health = health;

        m_Movement = movement;
        m_MovementIndex = movement.RegisterOrbit(this, GetComponent<PhysicsPose>().body, path);
    }

    // The setup shared by wave enemies and boss elements: identity, live color, and the body and shield sprites.
    void Setup(WaveDefinition wave, int score, Color color)
    {
        m_Wave = wave;
        m_Score = score;
        m_Color = color;

        if (m_Shield != null)
            m_ShieldArea = m_Shield.GetComponent<PhysicsAreaCircle>();

        if (SpriteRenderSystem.instance != null)
        {
            // The body is solid, so it blocks light and casts a shadow through the lit background; the shield ring does not.
            m_BodyInstance = SpriteRenderSystem.instance.Add(transform, m_BodySprite, m_BodyScale, color);

            // The shield is a generated ring sprite scaled so its rim sits exactly on the physics circle; it rests invisible.
            if (m_Shield != null && m_ShieldArea != null)
            {
                m_ShieldBaseScale = m_ShieldArea.activeGeometry.radius / RingSpriteRadius;

                var shieldColor = color;
                shieldColor.a = 0f;
                m_ShieldInstance = SpriteRenderSystem.instance.Add(m_Shield.transform, ringSprite, Vector2.one * m_ShieldBaseScale, shieldColor);
            }
        }
    }

    /// <summary>
    /// Applies damage; the enemy dies when its health is exhausted, bursting into particles in its wave color.
    /// The shield drops once the next hit of this size would kill, so the final hit lands on the core shapes.
    /// </summary>
    public void TakeDamage(float damage)
    {
        m_Health -= damage;

        if (m_Health <= 0f)
        {
            Die(awardScore: true);
            return;
        }

        // One hit from death the shield stops detecting, so the kill lands on the core shapes.
        // Only the physics area is disabled; the sprite stays active so this hit's flash still fades out.
        if (isShielded && m_Health <= damage)
            m_ShieldArea.enabled = false;
    }

    /// <summary>
    /// Kills the enemy outright with its full death effects but no score; used when it reaches the player and detonates.
    /// </summary>
    public void Detonate() => Die(awardScore: false);

    /// <summary>
    /// Finds the enemy behind a physics shape via the component ownership chain, or null when the shape is not an enemy's.
    /// </summary>
    public static Enemy FromShape(PhysicsShape shape)
    {
        if (!shape.isValid)
            return null;

        if (shape.owner is Component shapeOwner && shapeOwner.TryGetComponent(out Enemy enemy))
            return enemy;

        var body = shape.body;

        if (body.isValid && body.owner is Component bodyOwner && bodyOwner.TryGetComponent(out Enemy bodyEnemy))
            return bodyEnemy;

        return null;
    }

    // The shared shield ring sprite, generated once on first use: a white ring with a hard inner edge and a soft outer
    // glow, tinted per enemy through the sprite color. White pixels let the HDR wave color drive the bloom.
    static Sprite ringSprite
    {
        get
        {
            if (s_RingSprite != null)
                return s_RingSprite;

            const int size = RingTextureSize;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            var center = (size - 1) * 0.5f;

            for (var y = 0; y < size; ++y)
            {
                for (var x = 0; x < size; ++x)
                {
                    var distance = new Vector2(x - center, y - center).magnitude;
                    var falloff = distance < RingRadiusPixels
                        ? 1f - (RingRadiusPixels - distance) / RingInnerWidthPixels
                        : 1f - (distance - RingRadiusPixels) / RingOuterWidthPixels;

                    var alpha = Mathf.Clamp01(falloff);
                    alpha *= alpha;

                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            s_RingSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);

            return s_RingSprite;
        }
    }

    void Die(bool awardScore)
    {
        if (awardScore && GameManager.instance != null)
            GameManager.instance.AddScore(m_Score);

        if (awardScore)
            ScreenFxSettings.KillBlip();

        if (m_DeathBurstPrefab != null)
        {
            var burst = Instantiate(m_DeathBurstPrefab, transform.position, Quaternion.identity);
            burst.gameObject.hideFlags = HideFlags.HideInHierarchy;

            var main = burst.main;
            main.startColor = m_Color;

            ParticleSettings.Apply(burst);
        }

        if (GameAudio.instance != null)
            GameAudio.instance.PlayKill();

        Destroy(gameObject);
    }

    /// <summary>
    /// Called by the movement system when a swap-back removal moves this enemy's state to a new index.
    /// </summary>
    public void UpdateMovementIndex(int index) => m_MovementIndex = index;

    // Fade the shield flash back to invisible; idle once faded, so unhit enemies do no per-frame work here.
    // The flash opens as an HDR-boosted, slightly oversized ring, then eases back: the alpha falls quadratically
    // for a bright snap with a fast tail while the ring settles onto the physics rim.
    void Update()
    {
        if (m_ShieldFlashUntil <= 0f || m_ShieldInstance == null)
            return;

        var remaining = m_ShieldFlashUntil - Time.time;
        var strength = Mathf.Clamp01(remaining / ShieldFlashSettings.duration);

        var color = m_Color * Mathf.Lerp(1f, ShieldFlashSettings.glow, strength);
        color.a = strength * strength * ShieldFlashSettings.peakAlpha;
        m_ShieldInstance.color = color;

        m_ShieldInstance.scale = Vector2.one * (m_ShieldBaseScale * (1f + ShieldFlashSettings.punch * strength));

        if (remaining <= 0f)
            m_ShieldFlashUntil = 0f;
    }

    void OnDisable()
    {
        if (m_Movement != null && m_MovementIndex >= 0)
        {
            m_Movement.Unregister(m_MovementIndex);
            m_MovementIndex = -1;
        }

        if (SpriteRenderSystem.instance != null)
        {
            if (m_BodyInstance != null)
                SpriteRenderSystem.instance.Remove(m_BodyInstance);

            if (m_ShieldInstance != null)
                SpriteRenderSystem.instance.Remove(m_ShieldInstance);
        }

        m_BodyInstance = null;
        m_ShieldInstance = null;

        Destroyed?.Invoke(this);
    }

    #region Internal

    // Ring texture layout in pixels, and the ring radius in world units at sprite scale 1 (pixels-per-unit is the texture size).
    const int RingTextureSize = 128;
    const float RingRadiusPixels = 50f;
    const float RingInnerWidthPixels = 5f;
    const float RingOuterWidthPixels = 12f;
    const float RingSpriteRadius = RingRadiusPixels / RingTextureSize;

    // The particle burst spawned at the death position, tinted with the wave color.
    [SerializeField] ParticleSystem m_DeathBurstPrefab;

    // The shield child: its root and its physics area (disabled when one hit from death).
    [SerializeField] GameObject m_Shield;

    // The body sprite drawn through the instanced sprite system, and its world scale.
    [SerializeField] Sprite m_BodySprite;
    [SerializeField] Vector2 m_BodyScale = Vector2.one;

    SpriteRenderSystem.SpriteInstance m_BodyInstance;
    SpriteRenderSystem.SpriteInstance m_ShieldInstance;
    PhysicsAreaCircle m_ShieldArea;

    // The one ring sprite all enemies share, built lazily.
    static Sprite s_RingSprite;

    // The live tint: the wave color, or the per-enemy override a boss-fired berserker carries.
    Color m_Color = Color.white;

    WaveDefinition m_Wave;
    WaveMovementSystem m_Movement;
    int m_MovementIndex = -1;
    int m_Score;
    float m_Health;
    float m_VisualScale = 1f;
    float m_ShieldBaseScale = 1f;
    float m_ShieldFlashUntil;

    #endregion
}
