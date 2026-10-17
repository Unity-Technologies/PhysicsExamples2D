using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rotates the gun to aim at the gamepad's left stick, a touch, or the mouse, and fires bullets while the fire control is held.
/// Bullets are instantiated fresh per shot, deliberately unpooled: component spawn cost is part of what this project measures.
/// </summary>
public sealed class GunController : MonoBehaviour
{
    /// <summary>
    /// Shots per second while the fire control is held.
    /// </summary>
    public float fireRate
    {
        get => m_FireRate;
        set => m_FireRate = Mathf.Clamp(value, 10f, 500f);
    }

    /// <summary>
    /// Bullet launch speed, in units per second.
    /// </summary>
    public float bulletSpeed
    {
        get => m_BulletSpeed;
        set => m_BulletSpeed = Mathf.Max(0.1f, value);
    }

    /// <summary>
    /// Random per-shot deviation either side of the aim, in degrees.
    /// A held stream fans into a cone of twice this angle.
    /// </summary>
    public float spreadDegrees
    {
        get => m_SpreadDegrees;
        set => m_SpreadDegrees = Mathf.Clamp(value, 0f, 45f);
    }

    /// <summary>
    /// Distance from the gun pivot at which bullets appear.
    /// </summary>
    public float muzzleOffset
    {
        get => m_MuzzleOffset;
        set => m_MuzzleOffset = Mathf.Max(0f, value);
    }

    /// <summary>
    /// Random per-shot speed variation as a fraction of the bullet speed, either side of it.
    /// Staggers bullets fired on the same frame along the flight path, so fewer sit wastefully overlapped.
    /// </summary>
    public float speedJitter
    {
        get => m_SpeedJitter;
        set => m_SpeedJitter = Mathf.Clamp(value, 0f, 0.5f);
    }

    /// <summary>
    /// The firing rate actually in effect: the base rate scaled by the spawner's cycle.
    /// Each completed pass through the wave pool raises the cycle, so the second pass fires at twice the base, the third at three times, and so on.
    /// Firepower implicitly grows with the enemy pressure so the escalation stays winnable.
    /// </summary>
    public float effectiveFireRate => m_FireRate * (m_Spawner != null ? m_Spawner.cycle : 1);

    /// <summary>
    /// The current normalized aim direction.
    /// </summary>
    public Vector2 aimDirection => m_AimDirection;

    /// <summary>
    /// Whether the fire control is currently held.
    /// </summary>
    public bool isFiring { get; private set; }

    void Awake()
    {
        GameSettings.RegisterToggle(this, GameSettings.WeaponsTab, "Query Bullets", () => m_UseQueryBullets, v => m_UseQueryBullets = v);
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Fire Rate", 10f, 500f, () => m_FireRate, v => m_FireRate = v);
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Bullet Speed", 5f, 40f, () => m_BulletSpeed, v => m_BulletSpeed = v);
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Spread", 0f, 45f, () => m_SpreadDegrees, v => m_SpreadDegrees = v);
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Speed Jitter", 0f, 0.5f, () => m_SpeedJitter, v => m_SpeedJitter = v);
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Muzzle Offset", 0f, 2f, () => m_MuzzleOffset, v => m_MuzzleOffset = v);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable()
    {
        m_Camera = Camera.main;
        m_AimDirection = Vector2.right;
    }

    void Update()
    {
        Aim();
        UpdateFiring();
    }

    // Points the gun along the gamepad's left stick when deflected, otherwise at the active touch or the mouse cursor.
    void Aim()
    {
        var stickAim = GamepadHelper.aim;

        if (stickAim != Vector2.zero)
        {
            m_AimDirection = stickAim;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(m_AimDirection.y, m_AimDirection.x) * Mathf.Rad2Deg);

            return;
        }

        if (m_Camera == null)
            return;

        Vector2 screenPoint;
        var touch = Touchscreen.current;

        if (touch != null && touch.primaryTouch.press.isPressed)
            screenPoint = touch.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            screenPoint = Mouse.current.position.ReadValue();
        else
            return;

        var worldPoint = (Vector2)m_Camera.ScreenToWorldPoint(screenPoint);
        var direction = worldPoint - (Vector2)transform.position;

        if (direction.sqrMagnitude < 1e-6f)
            return;

        m_AimDirection = direction.normalized;

        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(m_AimDirection.y, m_AimDirection.x) * Mathf.Rad2Deg);
    }

    // Fires at the configured rate while the gamepad's right trigger, a touch, or the mouse button is held.
    // High rates can exceed one shot per frame, so each frame fires as many times as the cooldown allows.
    void UpdateFiring()
    {
        m_Cooldown -= Time.deltaTime;

        var firing = GamepadHelper.fireHeld ||
                     (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) ||
                     (Mouse.current != null && Mouse.current.leftButton.isPressed);

        // The gun holds fire through the set transition, so the warp plays out without bullets streaking through it.
        // isFiring reads false too, dropping the starfield's firing boost while the warp owns the drift.
        if (m_Spawner != null && m_Spawner.inTransition)
            firing = false;

        isFiring = firing;

        if (!firing)
        {
            m_Cooldown = Mathf.Max(0f, m_Cooldown);
            return;
        }

        while (m_Cooldown <= 0f)
        {
            m_Cooldown += 1f / effectiveFireRate;

            Fire();
        }
    }

    void Fire()
    {
        // Jitter each shot inside the spread cone so a held stream fans out instead of stacking on one line.
        var spread = Random.Range(-m_SpreadDegrees, m_SpreadDegrees);
        var direction = (Vector2)(Quaternion.Euler(0f, 0f, spread) * m_AimDirection);

        var prefab = m_UseQueryBullets && m_QueryBulletPrefab != null ? m_QueryBulletPrefab : m_BulletPrefab;
        var muzzle = (Vector2)transform.position + direction * m_MuzzleOffset;
        var rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        var bullet = Instantiate(prefab, muzzle, rotation);

        if (HierarchySettings.hideSpawned)
            bullet.gameObject.hideFlags = HideFlags.HideInHierarchy;

        // Jitter the speed so simultaneous shots spread along the flight path instead of overlapping as one projectile.
        var speed = m_BulletSpeed * (1f + Random.Range(-m_SpeedJitter, m_SpeedJitter));

        bullet.Launch(direction * speed);

        bullet.color = m_BulletColor;

        if (GameAudio.instance != null)
            GameAudio.instance.PlayFire();
    }

    #region Internal

    // The bullet prefabs: the dynamic-body version and the query-cast version, chosen by the toggle.
    [SerializeField] Bullet m_BulletPrefab;
    [SerializeField] Bullet m_QueryBulletPrefab;
    [SerializeField] bool m_UseQueryBullets = true;

    // The spawner whose enemy multiplier scales the fire rate.
    [SerializeField] WaveSpawner m_Spawner;

    // The tint applied to every fired bullet.
    [SerializeField]
    [ColorUsage(true, true)]
    Color m_BulletColor = new(3.2f, 2.6f, 0.6f, 1f);

    // Firing tuning; also exposed as properties for runtime tweaking.
    [SerializeField]
    [Range(10f, 500f)]
    float m_FireRate = 30f;

    [SerializeField]
    [Range(5f, 40f)]
    float m_BulletSpeed = 20f;

    [SerializeField]
    [Range(0f, 45f)]
    float m_SpreadDegrees = 2.5f;

    [SerializeField]
    [Range(0f, 0.5f)]
    float m_SpeedJitter = 0.1f;

    [SerializeField]
    [Range(0f, 2f)]
    float m_MuzzleOffset = 0.7f;

    Camera m_Camera;
    Vector2 m_AimDirection;
    float m_Cooldown;

    #endregion
}
