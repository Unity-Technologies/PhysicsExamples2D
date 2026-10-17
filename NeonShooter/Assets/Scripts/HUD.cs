using UnityEngine;

/// <summary>
/// The in-game display: the score, health bar, and panic pips stacked at the top of the view, and live stats in the corner.
/// Everything is built at runtime; the bar and pips are world-space sprites and the corner text is immediate-mode UI.
/// </summary>
public sealed class HUD : MonoBehaviour
{
    void Awake()
    {
        GameSettings.RegisterToggle(this, GameSettings.VisualsTab, "Show Stats", () => m_ShowStats, v => m_ShowStats = v);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "HUD Dim", 0.1f, 1f, () => m_Dim, v => m_Dim = v);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Score Glow", 0.5f, 3f, () => m_ScoreGlow, v => m_ScoreGlow = v, nextRun: true);
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void Start()
    {
        ResolveSortingLayer();

        // The healthy color mirrors the player's own sprite, so the bar and pips always match the player's neon.
        if (m_PlayerSprite == null)
        {
            var player = GameObject.Find("Player");
            if (player != null)
                m_PlayerSprite = player.GetComponentInChildren<SpriteRenderer>();
        }

        m_HealthyColor = m_PlayerSprite != null ? m_PlayerSprite.color : FallbackHealthyColor;

        // A solid white sprite for the bar and pips, generated here so no asset is needed.
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var pixels = new Color32[16];

        for (var i = 0; i < pixels.Length; ++i)
            pixels[i] = new Color32(255, 255, 255, 255);

        texture.SetPixels32(pixels);
        texture.Apply();
        var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0f, 0.5f), 4f);

        var unlit = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));

        CreateScoreText();

        // The bar and pips hang below the score at the top of the view, keeping them out of the play space.
        var barY = m_ScoreRoot.transform.position.y - m_BarBelowScore;

        CreateHealthBar(sprite, unlit, barY);
        CreatePanicPips(sprite, unlit, barY);
    }

    // Finds the sorting layer the HUD draws on, which keeps it above the gameplay sprites.
    // The HUD stays on its authored layer when that sorting layer does not exist, so it still draws, just interleaved with the gameplay sprites.
    private void ResolveSortingLayer()
    {
        var layerId = SortingLayer.NameToID(HudSortingLayerName);

        if (SortingLayer.IsValid(layerId))
        {
            m_SortingLayerId = layerId;

            return;
        }

        Debug.LogWarning($"[HUD] There is no \"{HudSortingLayerName}\" sorting layer, so the HUD draws interleaved with the gameplay sprites. Add one under Project Settings > Tags and Layers.", this);
    }

    // The score readout: a world-space text at the top of the view.
    // Text meshes clamp vertex colors, so the HDR tint that drives the bloom lives on a cloned font material instead.
    void CreateScoreText()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        font.RequestCharactersInTexture("0123456789", ScoreFontSize, FontStyle.Bold);

        var scoreRoot = new GameObject("Score");
        scoreRoot.transform.position = new Vector3(0f, Camera.main.orthographicSize - 0.25f, 0f);
        m_ScoreRoot = scoreRoot;

        m_ScoreText = scoreRoot.AddComponent<TextMesh>();
        m_ScoreText.font = font;
        m_ScoreText.fontSize = ScoreFontSize;
        m_ScoreText.fontStyle = FontStyle.Bold;
        m_ScoreText.characterSize = 0.09f;
        m_ScoreText.anchor = TextAnchor.UpperCenter;
        m_ScoreText.alignment = TextAlignment.Center;
        m_ScoreText.text = "0";

        var scoreColor = m_HealthyColor * m_ScoreGlow;
        scoreColor.a = 1f;

        var scoreRenderer = scoreRoot.GetComponent<MeshRenderer>();
        scoreRenderer.sharedMaterial = new Material(font.material) { color = scoreColor };
        scoreRenderer.sortingLayerID = m_SortingLayerId;
        scoreRenderer.sortingOrder = 13;
    }

    // The health bar: a dark background track with a fill anchored left, so the fill shrinks toward the left edge as health drops.
    void CreateHealthBar(Sprite sprite, Material unlit, float barY)
    {
        var barRoot = new GameObject("HealthBar");
        barRoot.transform.position = new Vector3(-BarWidth * 0.5f, barY, 0f);
        m_BarRoot = barRoot;

        var back = CreateBarPart(barRoot.transform, "Back", sprite, unlit, TrackColor, 1, m_SortingLayerId);
        back.transform.localScale = new Vector3(BarWidth, BarThickness, 1f);

        m_Fill = CreateBarPart(barRoot.transform, "Fill", sprite, unlit, Dimmed(m_HealthyColor), 2, m_SortingLayerId);
        m_Fill.transform.localScale = new Vector3(BarWidth, BarThickness, 1f);
    }

    // The panic charge pips: right-leaning slashes below the bar, glowing like the fill, one per charge.
    // Positions are laid out per charge count in Update, so the remaining pips stay centered as charges are spent.
    void CreatePanicPips(Sprite sprite, Material unlit, float barY)
    {
        var maxCharges = m_Panic != null ? m_Panic.charges : 0;
        m_PanicPips = new SpriteRenderer[maxCharges];

        var pipsRoot = new GameObject("PanicPips");
        pipsRoot.transform.position = new Vector3(0f, barY - 0.24f, 0f);
        m_PipsRoot = pipsRoot;

        for (var i = 0; i < maxCharges; ++i)
        {
            var pip = CreateBarPart(pipsRoot.transform, "Pip", sprite, unlit, Dimmed(m_HealthyColor), 3, m_SortingLayerId);
            pip.transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
            pip.transform.localScale = new Vector3(0.05f, 0.2f, 1f);
            m_PanicPips[i] = pip;
        }
    }

    void Update()
    {
        if (m_Game == null || m_Fill == null)
            return;

        // The in-world HUD only shows during play: the start screen has nothing to report yet, and on game over it disappears with the player.
        var showHud = !m_Game.isGameOver && !m_Game.isAwaitingStart;

        if (m_BarRoot.activeSelf != showHud)
        {
            m_BarRoot.SetActive(showHud);
            m_PipsRoot.SetActive(showHud);
            m_ScoreRoot.SetActive(showHud);
        }

        if (!showHud)
            return;

        // The score string only rebuilds when the value moves.
        if (m_ScoreText != null && m_Game.score != m_LastScore)
        {
            m_LastScore = m_Game.score;
            m_ScoreText.text = m_LastScore.ToString();
        }

        var fraction = Mathf.Clamp01(m_Game.health / m_Game.maxHealth);
        var scale = m_Fill.transform.localScale;
        scale.x = BarWidth * fraction;
        m_Fill.transform.localScale = scale;

        // The player's color when healthy, shifting to red as health runs out.
        m_Fill.color = Dimmed(Color.Lerp(DangerColor, m_HealthyColor, fraction));

        // The pips hide while the boss suppresses the panic bomb, returning with it when the boss falls.
        var pipsVisible = BossWaveSystem.instance == null || !BossWaveSystem.instance.isActive;

        if (m_PipsRoot.activeSelf != pipsVisible)
            m_PipsRoot.SetActive(pipsVisible);

        var dimChanged = !Mathf.Approximately(m_Dim, m_LastDim);
        m_LastDim = m_Dim;

        if (m_PanicPips != null && m_Panic != null && (m_Panic.charges != m_LastCharges || dimChanged))
        {
            m_LastCharges = m_Panic.charges;

            // The remaining pips stay centered as a group as charges are spent.
            for (var i = 0; i < m_PanicPips.Length; ++i)
            {
                m_PanicPips[i].enabled = i < m_LastCharges;
                m_PanicPips[i].color = Dimmed(m_HealthyColor);
                m_PanicPips[i].transform.localPosition = new Vector3((i - (m_LastCharges - 1) * 0.5f) * 0.2f, 0f, 0f);
            }
        }
    }

    // The HUD dimmer: alpha scales the sprite's whole blended contribution, core and bloom overshoot together.
    Color Dimmed(Color color) => new(color.r, color.g, color.b, m_Dim);

    void OnGUI()
    {
        if (m_ShowStats)
        {
            // GUI.skin is only valid inside OnGUI, so the style is built lazily once here and reused.
            m_StatsStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                normal = { textColor = new Color(0.6f, 0.9f, 1f, 0.9f) }
            };

            // The label text refreshes a few times per second rather than allocating fresh strings every repaint.
            if (Time.unscaledTime >= m_NextStatsRefresh)
            {
                m_NextStatsRefresh = Time.unscaledTime + 0.25f;

                var bullets = BulletMovementSystem.instance != null ? BulletMovementSystem.instance.activeCount : 0;
                var fps = 1f / Mathf.Max(Time.smoothDeltaTime, 1e-5f);

                m_SetLine = $"Cycle {m_Spawner.cycle}   Set {m_Spawner.set}/{m_Spawner.setsPerCycle}";
                m_StatsLine = $"Enemies {m_Spawner.aliveCount}   Bullets {bullets}   {fps:F0} fps";
            }

            GUI.Label(new Rect(12f, 8f, 400f, 24f), m_SetLine, m_StatsStyle);
            GUI.Label(new Rect(12f, 30f, 400f, 24f), m_StatsLine, m_StatsStyle);
        }
    }

    static SpriteRenderer CreateBarPart(Transform parent, string partName, Sprite sprite, Material material, Color color, int order, int sortingLayerId)
    {
        var go = new GameObject(partName);
        go.transform.SetParent(parent, false);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.color = color;
        renderer.sortingLayerID = sortingLayerId;
        renderer.sortingOrder = 10 + order;

        return renderer;
    }

    #region Internal

    const float BarWidth = 1.2f;
    const float BarThickness = 0.07f;
    const int ScoreFontSize = 48;

    // The sorting layer the HUD draws on, which keeps it above the gameplay sprites.
    const string HudSortingLayerName = "HUD";

    // HUD sprite palette; the healthy color is sampled from the player's sprite at startup, this is only the fallback.
    static readonly Color TrackColor = new(0.1f, 0.15f, 0.2f, 1f);
    static readonly Color FallbackHealthyColor = new(0.13f, 2f, 1.45f, 1f);
    static readonly Color DangerColor = new(2f, 0.33f, 0.27f, 1f);

    [SerializeField] GameManager m_Game;
    [SerializeField] WaveSpawner m_Spawner;
    [SerializeField] PanicBomb m_Panic;

    // The sprite whose color the bar and pips mirror; the player's hex is found by name when unassigned.
    [SerializeField] SpriteRenderer m_PlayerSprite;

    // How far the health bar hangs below the top of the score text.
    [SerializeField] float m_BarBelowScore = 0.6f;

    // Whether the corner stats text is drawn at all.
    [SerializeField] bool m_ShowStats = true;

    // How strongly the health bar fill and pips register on screen; alpha-based, so it dims the core and the bloom together.
    // Around 0.55 the scaled color crosses the bloom threshold, so values above it glow and values below sit flat.
    [SerializeField]
    [Range(0.1f, 1f)]
    float m_Dim = 0.65f;

    // Multiplies the player color for the score text; above 1 it pushes past the bloom threshold and glows harder.
    [SerializeField]
    [Range(0.5f, 3f)]
    float m_ScoreGlow = 1.5f;

    SpriteRenderer m_Fill;
    SpriteRenderer[] m_PanicPips;
    GameObject m_BarRoot;
    GameObject m_PipsRoot;
    GameObject m_ScoreRoot;
    TextMesh m_ScoreText;
    int m_SortingLayerId;
    int m_LastScore = -1;
    int m_LastCharges = -1;
    float m_LastDim = -1f;
    Color m_HealthyColor;

    // Cached immediate-mode styles and label strings, rebuilt only when stale.
    GUIStyle m_StatsStyle;
    string m_SetLine = "";
    string m_StatsLine = "";
    float m_NextStatsRefresh;

    #endregion
}
