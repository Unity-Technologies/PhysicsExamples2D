using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Runs the endless level as escalating cycles.
/// Each cycle runs one set per wave in the pool; a set spawns several consecutive waves simultaneously, and each new cycle
/// raises that concurrency by one until every wave runs together, which then repeats forever.
/// The rotation is deterministic: cycle 1 runs wave 1, 2, 3...; cycle 2 runs 1+2, 2+3... wrapping around the pool, and so on.
/// Each completed cycle hands the arena to the boss before the next cycle begins.
/// Enemies spawn on an ellipse just outside the camera view.
/// </summary>
public sealed class WaveSpawner : MonoBehaviour
{
    /// <summary>
    /// The number of sets in each cycle, which is the wave pool size so every wave leads exactly one set.
    /// </summary>
    public int setsPerCycle => m_Waves.Length;

    /// <summary>
    /// The 1-based cycle number, which is also how many waves each set runs at once.
    /// </summary>
    public int cycle => m_Cycle + 1;

    /// <summary>
    /// The 1-based set number within the current cycle.
    /// </summary>
    public int set => Mathf.Min(m_Set + 1, setsPerCycle);

    /// <summary>
    /// How many spawned enemies are still alive.
    /// </summary>
    public int aliveCount => m_Alive;

    /// <summary>
    /// Whether the game is in the pause between sets, while the warp transition plays.
    /// </summary>
    public bool inTransition => m_Runs.Count == 0 && m_SetDelayTimer > 0f;

    /// <summary>
    /// Whether the editor-only cheat is enabled; always false in a player build.
    /// While on, the K key clears the arena.
    /// </summary>
    public bool allowCheat
    {
        get
        {
#if UNITY_EDITOR
            return m_AllowCheat;
#else
            return false;
#endif
        }
    }

    // The settings register before Start reads the starting point, so a saved start applies to this run.
    void Awake()
    {
        GameSettings.RegisterWholeSlider(this, GameSettings.GameTab, "Starting Cycle", 1, 6, () => m_StartingCycle, v => m_StartingCycle = v, nextRun: true);
        GameSettings.RegisterWholeSlider(this, GameSettings.GameTab, "Starting Set", 1, 6, () => m_StartingSet, v => m_StartingSet = v, nextRun: true);
        GameSettings.RegisterWholeSlider(this, GameSettings.GameTab, "Enemy Increase", 0, 50, () => m_EnemyIncrease, v => m_EnemyIncrease = v);
        GameSettings.RegisterWholeSlider(this, GameSettings.GameTab, "Enemy Cycle Increase", 0, 50, () => m_EnemyCycleIncrease, v => m_EnemyCycleIncrease = v);
        GameSettings.RegisterSlider(this, GameSettings.GameTab, "Berserker Chance", 0f, 1f, () => m_BerserkerChance, v => m_BerserkerChance = v);
        GameSettings.RegisterWholeSlider(this, GameSettings.GameTab, "Max Berserkers", 1, 10, () => m_MaxBerserkers, v => m_MaxBerserkers = v);
        GameSettings.RegisterWholeSlider(this, GameSettings.GameTab, "Berserker Allowance", 0, 20, () => m_BerserkerAllowance, v => m_BerserkerAllowance = v);
        GameSettings.RegisterWholeSlider(this, GameSettings.GameTab, "Berserker Allowance Per Set", 0, 5, () => m_BerserkerAllowancePerSet, v => m_BerserkerAllowancePerSet = v);
        GameSettings.RegisterSlider(this, GameSettings.GameTab, "Spawn Margin", 0f, 10f, () => m_SpawnMargin, v => m_SpawnMargin = v);
        GameSettings.RegisterSlider(this, GameSettings.GameTab, "Spawn Depth", 0f, 10f, () => m_SpawnDepth, v => m_SpawnDepth = v);
        GameSettings.RegisterSlider(this, GameSettings.GameTab, "Set Delay", 0f, 10f, () => m_InterSetDelay, v => m_InterSetDelay = v);
    }

    void Start()
    {
        m_Camera = Camera.main;

        // The starting point is clamped against the current pool size, so a stale inspector value cannot start out of range.
        // The total count keeps the unclamped start, so boss selection and scaling reflect the true cycle from the first boss.
        m_Cycle = Mathf.Clamp(m_StartingCycle - 1, 0, m_Waves.Length - 1);
        m_TotalCycle = Mathf.Max(0, m_StartingCycle - 1);
        m_Set = Mathf.Clamp(m_StartingSet - 1, 0, setsPerCycle - 1);

        if (m_Boss != null)
            m_Boss.Completed += OnBossCompleted;
    }

    void OnDestroy()
    {
        GameSettings.Unregister(this);

        if (m_Boss != null)
            m_Boss.Completed -= OnBossCompleted;
    }

    void Update()
    {
#if UNITY_EDITOR
        // The editor-only cheat: one key clears the arena, so sets and bosses can be skipped while testing.
        if (m_AllowCheat && Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            CheatClearArena();
#endif

        if (m_Runs.Count == 0)
        {
            // The boss holds the flow: nothing spawns until its chain is destroyed.
            if (m_Boss != null && m_Boss.isActive)
                return;

            m_SetDelayTimer -= Time.deltaTime;

            if (m_SetDelayTimer <= 0f)
            {
                if (m_BossPending)
                    StartBoss();
                else
                    StartSet();
            }

            return;
        }

        var spawningDone = TickSpawning();

        // Set complete: every wave fully spawned and the arena cleared. Advance, raising concurrency each full cycle.
        if (!spawningDone || m_Alive > 0)
            return;

        AdvanceAfterSetComplete();
    }

    // Advances every active run's spawn timer, spawning any due enemy, and returns whether every run has finished spawning.
    bool TickSpawning()
    {
        var spawningDone = true;

        foreach (var run in m_Runs)
        {
            if (run.spawned >= run.count)
                continue;

            spawningDone = false;
            run.timer -= Time.deltaTime;

            if (run.timer <= 0f)
            {
                run.timer += run.interval;
                Spawn(run.wave, run.score);
                ++run.spawned;

                TrySpawnBerserker(run);
            }
        }

        return spawningDone;
    }

    // Ends the completed set: clears its runs, plays the transition, and advances the set, queuing the boss and raising concurrency at each cycle's end.
    void AdvanceAfterSetComplete()
    {
        m_Runs.Clear();
        BeginTransition();

        ++m_Set;

        if (m_Set < setsPerCycle)
            return;

        // The completed cycle leads into its boss: the chain's wave wraps around the pool by the true cycle count, which never clamps.
        if (m_Boss != null)
        {
            m_BossPending = true;
            m_BossWave = m_Waves[m_TotalCycle % m_Waves.Length];
            m_BossCycle = m_TotalCycle + 1;
        }

        m_Set = 0;
        m_Cycle = Mathf.Min(m_Cycle + 1, m_Waves.Length - 1);
        ++m_TotalCycle;
    }

    // Opens the between-sets pause: the starfield jumps to warp, the whoosh resolves as the next arrival lands, and the catapult distortion opens it.
    // The same flourish covers a set ending and the boss falling.
    void BeginTransition()
    {
        m_SetDelayTimer = m_InterSetDelay;

        if (Starfield.instance != null)
            Starfield.instance.Warp(m_InterSetDelay);

        if (GameAudio.instance != null)
            GameAudio.instance.PlayWarp(m_InterSetDelay);

        ScreenFxSettings.Warp();
    }

    // Hands the arena to the boss for the cycle that just completed; sets resume when it reports completion.
    void StartBoss()
    {
        m_BossPending = false;
        m_Boss.StartBoss(m_BossWave, m_BossCycle);
    }

    // The boss is down: the transition flourish plays and the next cycle's first set arrives after the delay.
    // A chain cleared by the game-over detonation sweep gets no flourish; the game is already ending.
    void OnBossCompleted()
    {
        if (GameManager.instance == null || !GameManager.instance.isGameOver)
            BeginTransition();
    }

#if UNITY_EDITOR
    // Completes every pending spawn and detonates everything alive, so the flow advances as if the set (or boss) was cleared.
    void CheatClearArena()
    {
        foreach (var run in m_Runs)
            run.spawned = run.count;

        foreach (var enemy in FindObjectsByType<Enemy>())
            enemy.Detonate();
    }
#endif

    // Starts the current set: the cycle's number of consecutive waves, led by the wave matching the set number and wrapping around the pool.
    // Cycle 1 runs each wave alone in order; cycle 2 runs 1+2, 2+3, ... 5+1; and so on, so runs are deterministic and extend naturally as waves are added.
    void StartSet()
    {
        var concurrency = Mathf.Min(m_Cycle + 1, m_Waves.Length);

        for (var i = 0; i < concurrency; ++i)
        {
            var waveIndex = (m_Set + i) % m_Waves.Length;
            var wave = m_Waves[waveIndex];

            // The authored count grows by the flat increase, plus the per-cycle extras from the second cycle on.
            // The interval shrinks by the same ratio, so a fattened wave still arrives over its authored time span.
            var count = wave.count + m_EnemyIncrease + m_EnemyCycleIncrease * m_Cycle;

            m_Runs.Add(new WaveRun
            {
                wave = wave,
                score = (waveIndex + 1) * ScorePerWave,
                count = count,
                interval = wave.spawnInterval * wave.count / count
            });
        }
    }

    // Each regular spawn rolls the berserker dice: on success one appears immediately with its alert.
    // A wave run's berserker allowance starts at the base and grows by the per-set amount with every set played, and no more than the maximum can be alive at once.
    // The game's very first wave (cycle 1, set 1) is berserker-free so the opening stays gentle.
    void TrySpawnBerserker(WaveRun run)
    {
        if (m_Cycle == 0 && m_Set == 0)
            return;

        var allowance = m_BerserkerAllowance + m_BerserkerAllowancePerSet * (m_Cycle * setsPerCycle + m_Set);

        if (m_BerserkerWave == null || m_BerserkersAlive >= m_MaxBerserkers || run.berserkers >= allowance)
            return;

        if (Random.value >= m_BerserkerChance)
            return;

        var berserker = Spawn(m_BerserkerWave, BerserkerScore);
        berserker.Destroyed += OnBerserkerDestroyed;

        ++run.berserkers;
        ++m_BerserkersAlive;

        if (GameAudio.instance != null)
            GameAudio.instance.PlayBerserker();
    }

    void OnBerserkerDestroyed(Enemy enemy) => m_BerserkersAlive = Mathf.Max(0, m_BerserkersAlive - 1);

    Enemy Spawn(WaveDefinition wave, int score)
    {
        // Start from a point just off the live view edge, then push it a little further out by a random amount.
        // Every enemy appears fully off-screen with mild variation in how far away it starts.
        var halfHeight = m_Camera.orthographicSize + m_SpawnMargin;
        var halfWidth = m_Camera.orthographicSize * m_Camera.aspect + m_SpawnMargin;

        var angle = Random.value * Mathf.PI * 2f;
        var edgePoint = new Vector2(Mathf.Cos(angle) * halfWidth, Mathf.Sin(angle) * halfHeight);
        var spawnPosition = edgePoint + edgePoint.normalized * Random.Range(0f, m_SpawnDepth);

        var enemy = Instantiate(wave.enemyPrefab, spawnPosition, Quaternion.identity);

        if (HierarchySettings.hideSpawned)
            enemy.gameObject.hideFlags = HideFlags.HideInHierarchy;

        enemy.Initialize(wave, spawnPosition, m_Movement, score);
        enemy.Destroyed += OnEnemyDestroyed;

        ++m_Alive;

        return enemy;
    }

    void OnEnemyDestroyed(Enemy enemy) => --m_Alive;

    // One wave's spawning progress within the current set, with the multiplier already applied.
    // The score is derived from the wave's position in the pool, so later waves are worth more without any per-asset value.
    sealed class WaveRun
    {
        public WaveDefinition wave;
        public int score;
        public int count;
        public float interval;
        public int spawned;
        public float timer;
        public int berserkers;
    }

    #region Internal

    // Score for killing an enemy of the pool's first wave; each later wave is worth one step more.
    const int ScorePerWave = 100;

    // Score for killing a berserker.
    const int BerserkerScore = 500;

    // The wave pool sets draw from.
    [SerializeField] WaveDefinition[] m_Waves;

    // The movement system every spawned enemy registers with.
    [SerializeField] WaveMovementSystem m_Movement;

    // The boss run between cycles; unassigned means cycles roll straight into each other with no boss.
    [SerializeField] BossWaveSystem m_Boss;

    // Where play begins, both 1-based: the cycle (how many waves run together) and the set within it (which wave leads).
    // Clamped to the pool size at startup, so values outlive changes to the wave count.
    [SerializeField]
    [Min(1)]
    [Tooltip("The difficulty tier to start at. The cycle is how many waves run simultaneously, and it also multiplies the gun's fire rate.")]
    int m_StartingCycle = 1;

    [SerializeField]
    [Min(1)]
    [Tooltip("The batch within the starting cycle. The set picks which wave leads; set 3 of cycle 2 starts with waves 3+4 together.")]
    int m_StartingSet = 1;

    // Flat addition to every wave's authored enemy count.
    [SerializeField]
    [Range(0, 50)]
    int m_EnemyIncrease;

    // Extra enemies added to every wave per completed cycle; the first cycle plays the authored counts.
    [SerializeField]
    [Range(0, 50)]
    int m_EnemyCycleIncrease;

    // The berserker: its wave definition (outside the pool), the chance rolled at every regular enemy spawn, and how many may be alive at once.
    [SerializeField] WaveDefinition m_BerserkerWave;

    [SerializeField]
    [Range(0f, 1f)]
    float m_BerserkerChance = 0.02f;

    [SerializeField]
    [Range(1, 10)]
    int m_MaxBerserkers = 1;

    // How many berserkers a wave run may spawn in total: the base, plus this much more for every set played so far in the run.
    [SerializeField]
    [Range(0, 20)]
    int m_BerserkerAllowance = 2;

    [SerializeField]
    [Range(0, 5)]
    int m_BerserkerAllowancePerSet = 1;

    // Distance beyond the view edge where enemies appear, extra random depth on top, and the pause between sets.
    [SerializeField] float m_SpawnMargin = 1.5f;
    [SerializeField] float m_SpawnDepth = 2.5f;
    [SerializeField] float m_InterSetDelay = 2f;

    // The editor-only cheat; it does nothing in a player build regardless of this setting.
    [SerializeField]
    [Tooltip("Editor-only. When on, pressing K destroys everything alive and completes pending spawns, skipping to the next set or past the boss.")]
    bool m_AllowCheat;

    readonly List<WaveRun> m_Runs = new();
    Camera m_Camera;
    int m_Cycle;

    // The cycle count without the concurrency clamp; it keeps climbing forever so the boss keeps rotating and scaling past the pool size.
    int m_TotalCycle;

    int m_Set;
    int m_Alive;
    float m_SetDelayTimer;
    int m_BerserkersAlive;

    // The boss queued by a completed cycle: whether one is due, the wave its chain uses, and the 1-based cycle that scales its shields.
    bool m_BossPending;
    WaveDefinition m_BossWave;
    int m_BossCycle;

    #endregion
}
