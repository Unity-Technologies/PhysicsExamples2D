using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The panic button: press the gamepad's right shoulder button, or press or tap the player, to spin up a second gun for a few seconds.
/// It fires at the player's own rate but sweeps around and around, spraying distinctly colored bullets in every direction.
/// Charges are limited per run.
/// </summary>
public sealed class PanicBomb : MonoBehaviour
{
    /// <summary>
    /// How many panic charges remain this run.
    /// </summary>
    public int charges => m_Charges;

    /// <summary>
    /// Whether the spinning gun is currently running.
    /// </summary>
    public bool isActive => Time.time < m_ActiveUntil;

    void Awake()
    {
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Panic Duration", 1f, 20f, () => m_Duration, v => m_Duration = v);
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Panic Spin Speed", 30f, 720f, () => m_SpinDegreesPerSecond, v => m_SpinDegreesPerSecond = v);
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Panic Muzzle Radius", 0f, 2f, () => m_MuzzleRadius, v => m_MuzzleRadius = v);
        GameSettings.RegisterWholeSlider(this, GameSettings.WeaponsTab, "Panic Charges", 0, 10, () => m_Charges, v => m_Charges = v);
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Panic Trigger Radius", 0.2f, 3f, () => m_TriggerRadius, v => m_TriggerRadius = v);
        GameSettings.RegisterSlider(this, GameSettings.WeaponsTab, "Panic Exposure Drop", -6f, 0f, () => m_ExposureDrop, v => m_ExposureDrop = v);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable() => m_Camera = Camera.main;

    void Update()
    {
        // The set transition cuts a running spinner short and blocks new activations, keeping the warp clean of bullets.
        // The boss suppresses the bomb the same way for its whole run; charges are untouched and usable again afterward.
        var suppressed = (m_Spawner != null && m_Spawner.inTransition) || (BossWaveSystem.instance != null && BossWaveSystem.instance.isActive);

        if (suppressed)
            m_ActiveUntil = 0f;

        // The drone cuts off the moment the spinner expires or game over interrupts it.
        var gameOver = m_Game != null && m_Game.isGameOver;

        if (m_DronePlaying && (!isActive || gameOver))
        {
            m_DronePlaying = false;

            if (GameAudio.instance != null)
                GameAudio.instance.StopPanicDrone();

            ScreenFxSettings.HoldAberration(0f);
            ScreenFxSettings.HoldExposure(0f);
        }

        if (gameOver)
            return;

        // Nothing while the menu is up, and a short grace after the start so the starting press cannot double as the bomb tap.
        if (m_Game != null && (m_Game.isAwaitingStart || m_Game.isPaused || Time.time < m_Game.startedTime + 0.3f))
            return;

        if (isActive)
            UpdateSpinFire();
        else if (!suppressed && m_Charges > 0 && PressedOnPlayer())
            Activate();
    }

    // The gamepad's right shoulder button, or a press or tap that starts within the player region, triggers the bomb.
    bool PressedOnPlayer()
    {
        if (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
            return true;

        Vector2 screenPoint;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            screenPoint = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            screenPoint = Mouse.current.position.ReadValue();
        else
            return false;

        var worldPoint = (Vector2)m_Camera.ScreenToWorldPoint(screenPoint);

        return worldPoint.magnitude <= m_TriggerRadius;
    }

    void Activate()
    {
        --m_Charges;
        m_ActiveUntil = Time.time + m_Duration;
        m_Cooldown = 0f;

        if (GameAudio.instance != null)
        {
            GameAudio.instance.PlayPanic();
            GameAudio.instance.StartPanicDrone();
            m_DronePlaying = true;
        }

        ScreenFxSettings.Pulse(1f);
        ScreenFxSettings.HoldAberration(0.6f);
        ScreenFxSettings.HoldExposure(m_ExposureDrop);
    }

    // Fires at the player's rate while the aim sweeps steadily around.
    // The angle advances only per shot, by exactly the sweep covered in one firing interval, so consecutive
    // bullets sit a fixed few degrees apart and the stream reads as one rotating beam.
    void UpdateSpinFire()
    {
        // Match the player's gun for rate, speed, spread, and jitter, falling back to defaults when no gun is wired.
        var fireRate = 30f;
        var bulletSpeed = 20f;
        var spread = 0f;
        var speedJitter = 0f;

        if (m_Gun != null)
        {
            fireRate = m_Gun.effectiveFireRate;
            bulletSpeed = m_Gun.bulletSpeed;
            spread = m_Gun.spreadDegrees;
            speedJitter = m_Gun.speedJitter;
        }

        m_Cooldown -= Time.deltaTime;

        while (m_Cooldown <= 0f)
        {
            var interval = 1f / fireRate;
            m_Cooldown += interval;
            m_SpinAngle += m_SpinDegreesPerSecond * interval;

            // The same per-shot jitter as the player's gun, on top of the sweep.
            var shotAngle = m_SpinAngle + Random.Range(-spread, spread);
            var radians = shotAngle * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

            var rotation = Quaternion.Euler(0f, 0f, shotAngle);
            var bullet = Instantiate(m_BulletPrefab, direction * m_MuzzleRadius, rotation);

            if (HierarchySettings.hideSpawned)
                bullet.gameObject.hideFlags = HideFlags.HideInHierarchy;

            // The same per-shot speed jitter as the player's gun, staggering same-frame shots along the path.
            var speed = bulletSpeed * (1f + Random.Range(-speedJitter, speedJitter));

            bullet.Launch(direction * speed);

            bullet.color = m_BulletColor;
        }
    }

    #region Internal

    [SerializeField] GameManager m_Game;

    // The player's gun, matched for fire rate and bullet speed.
    [SerializeField] GunController m_Gun;

    // The spawner whose set transition cuts the spinner off.
    [SerializeField] WaveSpawner m_Spawner;

    // The projectile the spinner fires; the query-cast bullet prefab.
    [SerializeField] Bullet m_BulletPrefab;

    // How long the spinner runs, how fast it sweeps, where bullets appear, and the color marking them apart from gun fire.
    [SerializeField] float m_Duration = 5f;
    [SerializeField] float m_SpinDegreesPerSecond = 150f;
    [SerializeField] float m_MuzzleRadius = 0.7f;
    [SerializeField]
    [ColorUsage(true, true)]
    Color m_BulletColor = new(3.2f, 0.5f, 2.8f, 1f);

    // How far the arena's lighting is held down while the spinner runs, in stops.
    // The spinner puts hundreds of lit bullets on screen at once, which together carry far more light than the run ever
    // normally does, so without this the whole arena washes out for its duration.
    [SerializeField]
    [Range(-6f, 0f)]
    float m_ExposureDrop = -2f;

    // Charges per run and how far a press from the player center still counts.
    [SerializeField] int m_Charges = 3;
    [SerializeField] float m_TriggerRadius = 1.2f;

    Camera m_Camera;
    float m_ActiveUntil;
    float m_SpinAngle;
    float m_Cooldown;
    bool m_DronePlaying;

    #endregion
}
