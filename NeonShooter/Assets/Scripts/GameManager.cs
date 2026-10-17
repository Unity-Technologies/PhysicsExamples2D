using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks player health and the run's score, and ends the game when health runs out.
/// The scene opens on the main menu with the spawner and gun disabled; the menu's Start Game button begins the run.
/// A run in progress can be paused, which stops time and holds the gun off while the menu is up.
/// On game over the spawner and gun are disabled, the player is removed, and every remaining enemy detonates;
/// after a short grace the menu returns with the score and a Restart, which reloads the scene for another run.
/// </summary>
[AutoStaticsCleanup]
public sealed partial class GameManager : MonoBehaviour
{
    /// <summary>
    /// The scene's game manager, or null when none is active.
    /// </summary>
    public static GameManager instance { get; private set; }

    /// <summary>
    /// The player's remaining health.
    /// </summary>
    public float health => m_Health;

    /// <summary>
    /// The health the player starts with.
    /// </summary>
    public float maxHealth => m_MaxHealth;

    /// <summary>
    /// Whether the player's health is exhausted.
    /// </summary>
    public bool isGameOver => m_Health <= 0f;

    /// <summary>
    /// Whether the game is on the main menu, before Start Game begins the run.
    /// </summary>
    public bool isAwaitingStart => !m_Started;

    /// <summary>
    /// Whether the run is held at the pause menu, with time stopped and the gun held off.
    /// </summary>
    public bool isPaused { get; private set; }

    /// <summary>
    /// Whether the game is over and the short grace after the death has passed, so the menu may return.
    /// The grace keeps a shot or touch held at the moment of death from landing on the menu.
    /// </summary>
    public bool isGameOverSettled => isGameOver && Time.time >= m_GameOverTime + GameOverGrace;

    /// <summary>
    /// Whether the player takes no damage.
    /// An option that works in every build, unlike the editor-only cheat on the spawner.
    /// </summary>
    public bool invincible { get; set; }

    /// <summary>
    /// The time the run began; meaningful once the run has started.
    /// </summary>
    public float startedTime => m_StartedTime;

    /// <summary>
    /// The score accumulated this run.
    /// </summary>
    public int score => m_Score;

    /// <summary>
    /// Adds to the run's score; enemies report their value here when the player's fire kills them.
    /// </summary>
    public void AddScore(int amount) => m_Score += amount;

    /// <summary>
    /// Begins the run from the main menu: the spawner and gun wake and the player appears.
    /// Does nothing once the run has begun.
    /// </summary>
    public void StartRun()
    {
        if (m_Started)
            return;

        m_Started = true;
        m_StartedTime = Time.time;
        m_Spawner.enabled = true;
        m_Gun.enabled = true;

        if (m_Player != null)
            m_Player.SetActive(true);
    }

    /// <summary>
    /// Holds the run at the pause menu: time stops, audio pauses, and the gun is held off so menu clicks cannot fire it.
    /// Only a run in progress can pause.
    /// </summary>
    public void Pause()
    {
        if (!m_Started || isGameOver || isPaused)
            return;

        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        m_Gun.enabled = false;
    }

    /// <summary>
    /// Releases a pause and lets the run continue.
    /// </summary>
    public void Resume()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        m_Gun.enabled = true;
    }

    /// <summary>
    /// Reloads the scene for a fresh run.
    /// Time scale and audio pause survive a scene load, so a pause is released first.
    /// </summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(gameObject.scene.name);
    }

    /// <summary>
    /// Applies damage to the player and triggers game over when health runs out.
    /// </summary>
    public void Damage(float amount)
    {
        if (isGameOver || invincible)
            return;

        m_Health = Mathf.Max(0f, m_Health - amount);

        if (m_Health > 0f)
            return;

        m_GameOverTime = Time.time;
        m_Spawner.enabled = false;
        m_Gun.enabled = false;

        // Clear the arena: every surviving enemy detonates (with its death burst), and the player disappears.
        // This runs from the post-simulate callback, a safe point for destroying physics objects.
        foreach (var enemy in FindObjectsByType<Enemy>())
            enemy.Detonate();

        if (m_Player != null)
            m_Player.SetActive(false);
    }

    // The settings register before OnEnable copies the maximum into the live health, so a saved maximum applies to this run.
    void Awake()
    {
        GameSettings.RegisterToggle(this, GameSettings.GameTab, "Invincible", () => invincible, v => invincible = v);
        GameSettings.RegisterSlider(this, GameSettings.GameTab, "Max Health", 10f, 1000f, () => m_MaxHealth, v => m_MaxHealth = v, nextRun: true);
    }

    void OnEnable()
    {
        m_Health = m_MaxHealth;
        instance = this;
    }

    void Start()
    {
        // The scene opens on the main menu: nothing spawns, the gun stays cold, and the player is hidden until the run starts.
        m_Spawner.enabled = false;
        m_Gun.enabled = false;

        if (m_Player != null)
            m_Player.SetActive(false);
    }

    // A pause left in place would outlive this scene, so it is released here as well as by Restart.
    void OnDisable()
    {
        if (instance == this)
            instance = null;

        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void OnDestroy() => GameSettings.Unregister(this);

    #region Internal

    // How long after the death the menu waits before it returns.
    const float GameOverGrace = 0.75f;

    [SerializeField] float m_MaxHealth = 100f;

    // Systems stopped on game over.
    [SerializeField] WaveSpawner m_Spawner;
    [SerializeField] GunController m_Gun;

    // The player root, removed on game over so only the game-over screen remains.
    [SerializeField] GameObject m_Player;

    float m_Health;
    float m_GameOverTime;
    int m_Score;
    bool m_Started;
    float m_StartedTime;

    #endregion
}
