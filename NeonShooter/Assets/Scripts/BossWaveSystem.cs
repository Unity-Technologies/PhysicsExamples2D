using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;
using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Runs the end-of-cycle boss: a chain of identical enemies orbiting the player at a fixed radius, undulating like a snake.
/// Elements carry long-lived shields that grow with the cycle, and living elements fire berserkers at the player.
/// The boss ends when every element is dead, awarding a large bonus; the spawner resumes the next cycle afterward.
/// </summary>
[AutoStaticsCleanup]
public sealed partial class BossWaveSystem : MonoBehaviour
{
    /// <summary>
    /// The scene's boss system, or null when none is active.
    /// </summary>
    public static BossWaveSystem instance { get; private set; }

    /// <summary>
    /// Raised when the last boss element dies and the boss is over.
    /// </summary>
    public event Action Completed;

    /// <summary>
    /// Whether a boss is currently running; the panic bomb and its HUD pips are suppressed while it is.
    /// </summary>
    public bool isActive => m_Elements.Count > 0;

    /// <summary>
    /// Starts the boss for the cycle that just completed: the chain spawns from the given wave's enemy, and the cycle scales the shields.
    /// The boss presentation (screen distortion and drone) runs until the boss ends.
    /// </summary>
    public void StartBoss(WaveDefinition elementWave, int cycle)
    {
        // The whole chain shares one orbit direction, head angle, undulation phase step, and undulation rate.
        // They come from a generator seeded by the cycle, so a given cycle's boss moves identically every run while still varying between cycles.
        // The phase step and rate span a slow whole-body wave up to a tight fast snake.
        var random = new System.Random(cycle);
        var direction = random.NextDouble() < 0.5 ? 1f : -1f;
        var headAngle = (float)random.NextDouble() * Mathf.PI * 2f;
        var phaseStep = Mathf.Lerp(PhaseStepMinDegrees, PhaseStepMaxDegrees, (float)random.NextDouble()) * Mathf.Deg2Rad;
        var frequency = Mathf.Lerp(UndulationFrequencyMin, UndulationFrequencyMax, (float)random.NextDouble());

        var angularSpeed = direction * m_OrbitDegreesPerSecond * Mathf.Deg2Rad;
        var spacing = m_SpacingDegrees * Mathf.Deg2Rad;
        var health = m_ShieldHitsBase + m_ShieldHitsPerCycle * (cycle - 1) + 1;

        for (var i = 0; i < m_ElementCount; ++i)
        {
            // Each element trails the head against the orbit direction, with its undulation phase stepped so the ripple travels down the chain.
            var path = new WaveMovementSystem.OrbitPath
            {
                startAngle = headAngle - direction * spacing * i,
                radius = m_OrbitRadius,
                angularSpeed = angularSpeed,
                amplitude = m_UndulationAmplitude,
                frequency = frequency,
                phase = phaseStep * i,
                spinRadians = elementWave.spinDegreesPerSecond * Mathf.Deg2Rad
            };

            var spawnDirection = new Vector2(Mathf.Cos(path.startAngle), Mathf.Sin(path.startAngle));
            var spawnPosition = spawnDirection * (m_OrbitRadius + m_UndulationAmplitude * Mathf.Sin(path.phase));

            var element = Instantiate(elementWave.enemyPrefab, spawnPosition, Quaternion.identity);

            if (HierarchySettings.hideSpawned)
                element.gameObject.hideFlags = HideFlags.HideInHierarchy;

            element.InitializeBossElement(elementWave, m_Movement, path, m_ElementScore, health);
            element.Destroyed += OnElementDestroyed;

            m_Elements.Add(element);
        }

        m_BerserkerTimer = m_BerserkerInterval;

        ScreenFxSettings.SetBossMode(true);

        if (GameAudio.instance != null)
            GameAudio.instance.StartBossDrone();
    }

    // Every value is read when a boss starts, so a change shows on the next boss.
    void Awake()
    {
        GameSettings.RegisterWholeSlider(this, GameSettings.BossTab, "Elements", 1, 100, () => m_ElementCount, v => m_ElementCount = v);
        GameSettings.RegisterSlider(this, GameSettings.BossTab, "Orbit Radius", 2f, 12f, () => m_OrbitRadius, v => m_OrbitRadius = v);
        GameSettings.RegisterSlider(this, GameSettings.BossTab, "Orbit Speed", 0f, 360f, () => m_OrbitDegreesPerSecond, v => m_OrbitDegreesPerSecond = v);
        GameSettings.RegisterSlider(this, GameSettings.BossTab, "Spacing", 1f, 45f, () => m_SpacingDegrees, v => m_SpacingDegrees = v);
        GameSettings.RegisterSlider(this, GameSettings.BossTab, "Undulation", 0f, 4f, () => m_UndulationAmplitude, v => m_UndulationAmplitude = v);
        GameSettings.RegisterWholeSlider(this, GameSettings.BossTab, "Shield Hits", 0, 100, () => m_ShieldHitsBase, v => m_ShieldHitsBase = v);
        GameSettings.RegisterWholeSlider(this, GameSettings.BossTab, "Shield Hits Per Cycle", 0, 50, () => m_ShieldHitsPerCycle, v => m_ShieldHitsPerCycle = v);
        GameSettings.RegisterSlider(this, GameSettings.BossTab, "Berserker Interval", 0.1f, 5f, () => m_BerserkerInterval, v => m_BerserkerInterval = v);
        GameSettings.RegisterSlider(this, GameSettings.BossTab, "Berserker Chance", 0f, 1f, () => m_BerserkerChance, v => m_BerserkerChance = v);
        GameSettings.RegisterWholeSlider(this, GameSettings.BossTab, "Max Berserkers", 1, 10, () => m_MaxBerserkers, v => m_MaxBerserkers = v);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable() => instance = this;

    void OnDisable()
    {
        if (instance == this)
            instance = null;

        m_Elements.Clear();
    }

    // Rolls the berserker dice on a fixed interval while elements live, and shuts the presentation down if the game ends mid-boss.
    void Update()
    {
        if (!isActive)
            return;

        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            StopPresentation();
            return;
        }

        m_BerserkerTimer -= Time.deltaTime;

        if (m_BerserkerTimer > 0f)
            return;

        m_BerserkerTimer += m_BerserkerInterval;

        if (m_BerserkersAlive >= m_MaxBerserkers || UnityEngine.Random.value >= m_BerserkerChance)
            return;

        FireBerserker();
    }

    // Fires a berserker from a random living element: it appears at the element and behaves exactly like a wave berserker, in a random hue.
    void FireBerserker()
    {
        if (m_BerserkerWave == null || m_BerserkerWave.enemyPrefab == null)
            return;

        var source = m_Elements[UnityEngine.Random.Range(0, m_Elements.Count)];
        var spawnPosition = (Vector2)source.transform.position;

        // A random hue at the authored berserker saturation and HDR intensity, so the glow stays right while the color marks it as boss-fired.
        Color.RGBToHSV(m_BerserkerWave.color, out _, out var saturation, out var value);
        var color = Color.HSVToRGB(UnityEngine.Random.value, saturation, value, hdr: true);
        color.a = 1f;

        var berserker = Instantiate(m_BerserkerWave.enemyPrefab, spawnPosition, Quaternion.identity);

        if (HierarchySettings.hideSpawned)
            berserker.gameObject.hideFlags = HideFlags.HideInHierarchy;

        berserker.Initialize(m_BerserkerWave, spawnPosition, m_Movement, m_BerserkerScore, color);
        berserker.Destroyed += OnBerserkerDestroyed;

        ++m_BerserkersAlive;

        if (GameAudio.instance != null)
            GameAudio.instance.PlayBerserker();
    }

    // Removes a dead element; the last one ending the boss awards the kill bonus and hands control back via Completed.
    // An element not in the list means the system already shut down (scene teardown cleared it first), so nothing completes.
    void OnElementDestroyed(Enemy element)
    {
        if (!m_Elements.Remove(element) || m_Elements.Count > 0)
            return;

        if (GameManager.instance != null && !GameManager.instance.isGameOver)
            GameManager.instance.AddScore(m_KillBonus);

        StopPresentation();
        Completed?.Invoke();
    }

    void OnBerserkerDestroyed(Enemy enemy) => --m_BerserkersAlive;

    // Ends the boss screen distortion and drone; safe to call more than once.
    void StopPresentation()
    {
        ScreenFxSettings.SetBossMode(false);

        if (GameAudio.instance != null)
            GameAudio.instance.StopBossDrone();
    }

    #region Internal

    // The ranges the per-boss undulation is rolled from: the phase step in degrees per element, and the rate in cycles per second.
    const float PhaseStepMinDegrees = 5f;
    const float PhaseStepMaxDegrees = 40f;
    const float UndulationFrequencyMin = 0.5f;
    const float UndulationFrequencyMax = 1f;

    // The movement system boss elements and fired berserkers register with.
    [SerializeField] WaveMovementSystem m_Movement;

    // The berserker wave definition fired from living elements.
    [SerializeField] WaveDefinition m_BerserkerWave;

    // The chain: how many elements, the orbit they follow, and how the snake undulates.
    [SerializeField]
    [Range(1, 100)]
    int m_ElementCount = 25;

    // Sized against the camera's 8.5 half-height: the chain rings the player mid-arena and its undulation peak stays on-screen vertically.
    [SerializeField] float m_OrbitRadius = 5f;
    [SerializeField] float m_OrbitDegreesPerSecond = 25f;
    [SerializeField] float m_SpacingDegrees = 10f;
    [SerializeField] float m_UndulationAmplitude = 0.8f;

    // How many shield hits an element absorbs on the first cycle, and how many more each later cycle adds.
    [SerializeField] int m_ShieldHitsBase = 10;
    [SerializeField] int m_ShieldHitsPerCycle = 5;

    // Berserker fire: the roll interval, the chance per roll, and how many boss-fired berserkers may be alive at once.
    [SerializeField] float m_BerserkerInterval = 0.5f;

    [SerializeField]
    [Range(0f, 1f)]
    float m_BerserkerChance = 0.15f;

    [SerializeField]
    [Range(1, 10)]
    int m_MaxBerserkers = 3;

    // Scores: per element killed, per boss-fired berserker, and the bonus for finishing the boss.
    [SerializeField] int m_ElementScore = 250;
    [SerializeField] int m_BerserkerScore = 500;
    [SerializeField] int m_KillBonus = 50000;

    readonly List<Enemy> m_Elements = new();
    int m_BerserkersAlive;
    float m_BerserkerTimer;

    #endregion
}
