using System.Collections.Generic;

using Unity.U2D.RadianceCascades;
using UnityEngine;

/// <summary>
/// Lights the arena behind the main menu.
/// Nothing emits while the menu is up, since the player is hidden and no enemies exist, so the lit grid would otherwise sit black.
/// A crowd of neon shapes drifts and spins across the view until the run starts, then fades out and is removed.
/// </summary>
public sealed class MenuLighting : MonoBehaviour
{
    // Runs in Start so the SpriteRenderSystem has registered itself, which it does in OnEnable.
    void Start()
    {
        if (SpriteRenderSystem.instance == null || m_Sprites.Length == 0 || m_Colors.Length == 0)
        {
            enabled = false;

            return;
        }

        m_Camera = Camera.main;
        m_Fade = 1f;

        for (var i = 0; i < m_Count; ++i)
        {
            var emitter = new GameObject($"MenuEmitter {i}");
            emitter.transform.SetParent(transform, false);

            var sprite = m_Sprites[Random.Range(0, m_Sprites.Length)];
            var scale = m_Scale * Random.Range(0.7f, 1.3f);
            var instance = SpriteRenderSystem.instance.Add(emitter.transform, sprite, Vector2.one * scale, Color.black);

            m_Emitters.Add(new Emitter(emitter.transform, instance, m_Colors[Random.Range(0, m_Colors.Length)]));
        }
    }

    void OnDisable() => RemoveAll();

    // Moves each emitter along its own slow figure within the view, spins it, and fades the set out once the run has begun.
    void Update()
    {
        var game = GameManager.instance;
        var menuUp = game == null || game.isAwaitingStart;

        m_Fade = Mathf.MoveTowards(m_Fade, menuUp ? 1f : 0f, Time.deltaTime / Mathf.Max(m_FadeDuration, 0.01f));

        if (!menuUp && m_Fade <= 0f)
        {
            RemoveAll();
            enabled = false;

            return;
        }

        var halfHeight = m_Camera.orthographicSize * DriftExtent;
        var halfWidth = halfHeight * m_Camera.aspect;
        var time = Time.time * m_DriftSpeed;

        foreach (var emitter in m_Emitters)
        {
            var x = Mathf.Sin(time * emitter.rateX + emitter.phaseX) * halfWidth;
            var y = Mathf.Sin(time * emitter.rateY + emitter.phaseY) * halfHeight;

            emitter.transform.SetPositionAndRotation(new Vector3(x, y, 0f), Quaternion.Euler(0f, 0f, Time.time * emitter.spin));

            var color = emitter.color * (m_Intensity * m_Fade);
            color.a = 1f;

            emitter.instance.color = color;
        }
    }

    void RemoveAll()
    {
        if (SpriteRenderSystem.instance != null)
        {
            foreach (var emitter in m_Emitters)
                SpriteRenderSystem.instance.Remove(emitter.instance);
        }

        foreach (var emitter in m_Emitters)
        {
            if (emitter.transform != null)
                Destroy(emitter.transform.gameObject);
        }

        m_Emitters.Clear();
    }

    #region Internal

    // How far across the view the drift reaches, as a fraction of the half extents.
    const float DriftExtent = 0.85f;

    // The spread of spin rates, in degrees per second; the sign is rolled too, so neighbors turn against each other.
    const float MinSpin = 8f;
    const float MaxSpin = 30f;

    // One drifting light: the object it moves, the sprite it draws, its color, and the figure it follows.
    // The figure is a Lissajous curve with its own rates and phases, so no two emitters move in step.
    sealed class Emitter
    {
        public Emitter(Transform transform, SpriteRenderSystem.SpriteInstance instance, Color color)
        {
            this.transform = transform;
            this.instance = instance;
            this.color = color;

            rateX = Random.Range(0.25f, 0.6f);
            rateY = Random.Range(0.25f, 0.6f);
            phaseX = Random.Range(0f, Mathf.PI * 2f);
            phaseY = Random.Range(0f, Mathf.PI * 2f);
            spin = Random.Range(MinSpin, MaxSpin) * (Random.value < 0.5f ? -1f : 1f);
        }

        public Transform transform { get; }
        public SpriteRenderSystem.SpriteInstance instance { get; }
        public Color color { get; }
        public float rateX { get; }
        public float rateY { get; }
        public float phaseX { get; }
        public float phaseY { get; }
        public float spin { get; }
    }

    // The shapes the emitters are drawn as, each drawn through the SpriteRenderSystem so the lighting sees it; one is picked per emitter at random.
    [SerializeField] Sprite[] m_Sprites;

    // The colors an emitter can take, picked at random.
    [SerializeField]
    [ColorUsage(false)]
    Color[] m_Colors =
    {
        new(0.31f, 0.86f, 1f),
        new(1f, 0.35f, 0.85f),
        new(1f, 0.77f, 0.31f)
    };

    // How many emitters drift across the view.
    [SerializeField]
    [Range(1, 60)]
    int m_Count = 22;

    // How bright each emitter is pushed past its color, which is what sets how much light lands on the grid.
    [SerializeField]
    [Min(0f)]
    float m_Intensity = 2.5f;

    // The base world size of an emitter; each one varies around it.
    [SerializeField]
    [Min(0.1f)]
    float m_Scale = 1.2f;

    // How fast the crowd drifts.
    [SerializeField]
    [Min(0f)]
    float m_DriftSpeed = 0.15f;

    // How long the emitters take to fade out once the run starts.
    [SerializeField]
    [Min(0f)]
    float m_FadeDuration = 0.6f;

    readonly List<Emitter> m_Emitters = new();
    Camera m_Camera;
    float m_Fade;

    #endregion
}
