using UnityEngine;

/// <summary>
/// Builds the player's ship look at startup: the hex sprite moves onto a slowly spinning shell child, a counter-rotating
/// dashed ring and a bright pulsing core fill the middle, and the gun's bar becomes a barrel poking past the rim.
/// Everything is generated or rearranged at runtime, so the scene keeps its plain single-sprite player.
/// </summary>
public sealed class PlayerShipVisuals : MonoBehaviour
{
    // Runs in Awake so PlayerHitFeedback's OnEnable capture sees the created sprites and includes them in the hit flicker.
    // The settings register first, so a saved size or boost is in place before the parts are built from it.
    void Awake()
    {
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Shell Spin", -360f, 360f, () => m_ShellSpin, v => m_ShellSpin = v);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Ring Spin", -360f, 360f, () => m_RingSpin, v => m_RingSpin = v);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Ring Radius", 0.1f, 0.9f, () => m_RingRadius, v => m_RingRadius = v, nextRun: true);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Core Radius", 0.05f, 0.5f, () => m_CoreRadius, v => m_CoreRadius = v, nextRun: true);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Core Boost", 1f, 3f, () => m_CoreBoost, v => m_CoreBoost = v, nextRun: true);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Core Pulse", 0f, 0.4f, () => m_CorePulse, v => m_CorePulse = v);
        GameSettings.RegisterSlider(this, GameSettings.VisualsTab, "Core Pulse Rate", 0f, 10f, () => m_CorePulseRate, v => m_CorePulseRate = v);

        var rootRenderer = GetComponent<SpriteRenderer>();

        if (rootRenderer == null)
            return;

        var color = rootRenderer.color;

        // The shell: the root's hex sprite moved onto a child that can spin without touching the physics transform.
        m_Shell = CreatePart("Shell", transform, rootRenderer.sprite, rootRenderer.sharedMaterial, color, rootRenderer.sortingOrder);
        rootRenderer.enabled = false;

        var shellRadius = m_Shell.bounds.extents.x;

        // The ring: a generated dashed circle, counter-rotating against the shell.
        m_Ring = CreatePart("Ring", transform, CreateRingSprite(), rootRenderer.sharedMaterial, color, rootRenderer.sortingOrder + 1);
        m_Ring.transform.localScale = Vector3.one * (shellRadius * m_RingRadius * 2f);

        // The core: a generated soft dot, boosted past the shell color so it blooms, pulsing in scale.
        var coreColor = color * m_CoreBoost;
        coreColor.a = 1f;
        m_Core = CreatePart("Core", transform, CreateCoreSprite(), rootRenderer.sharedMaterial, coreColor, rootRenderer.sortingOrder + 2);
        m_CoreScale = shellRadius * m_CoreRadius * 2f;
        m_Core.transform.localScale = Vector3.one * m_CoreScale;

        // The barrel: the gun's own bar sprite moves onto a child pushed outward, so it pokes past the rim toward the aim.
        var gun = GetComponentInChildren<GunController>();

        if (gun != null && gun.TryGetComponent(out SpriteRenderer gunRenderer))
        {
            var barrel = CreatePart("Barrel", gun.transform, gunRenderer.sprite, gunRenderer.sharedMaterial, gunRenderer.color, gunRenderer.sortingOrder);
            barrel.transform.localPosition = new Vector3(m_BarrelOffset, 0f, 0f);
            gunRenderer.enabled = false;
        }
    }

    void OnDestroy() => GameSettings.Unregister(this);

    void Update()
    {
        m_Shell.transform.Rotate(0f, 0f, m_ShellSpin * Time.deltaTime);
        m_Ring.transform.Rotate(0f, 0f, m_RingSpin * Time.deltaTime);

        var pulse = 1f + m_CorePulse * Mathf.Sin(Mathf.PI * 2f * m_CorePulseRate * Time.time);
        m_Core.transform.localScale = Vector3.one * (m_CoreScale * pulse);
    }

    static SpriteRenderer CreatePart(string partName, Transform parent, Sprite sprite, Material material, Color color, int order)
    {
        var go = new GameObject(partName);
        go.transform.SetParent(parent, false);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.color = color;
        renderer.sortingOrder = order;

        return renderer;
    }

    // A dashed ring: six arcs with gaps so the counter-rotation reads; white, tinted by the renderer color.
    static Sprite CreateRingSprite()
    {
        const int size = 128;
        const float radius = 50f;
        const float halfWidth = 6f;
        const float soft = 3f;

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        var center = (size - 1) * 0.5f;

        for (var y = 0; y < size; ++y)
        {
            for (var x = 0; x < size; ++x)
            {
                var dx = x - center;
                var dy = y - center;
                var band = Mathf.Abs(Mathf.Sqrt(dx * dx + dy * dy) - radius);

                // Six 42-degree dashes with 18-degree gaps; the gap edges get the same softness as the band edges.
                var angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg + 180f;
                var inDash = angle % 60f < 42f;

                var alpha = inDash ? Mathf.Clamp01(1f - (band - halfWidth) / soft) : 0f;

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // A soft dot: opaque center falling off toward the edge.
    static Sprite CreateCoreSprite()
    {
        const int size = 64;
        const float radius = 28f;

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        var center = (size - 1) * 0.5f;

        for (var y = 0; y < size; ++y)
        {
            for (var x = 0; x < size; ++x)
            {
                var distance = new Vector2(x - center, y - center).magnitude;
                var alpha = Mathf.Clamp01(1f - distance / radius);
                alpha *= alpha;

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    #region Internal

    // Spin rates in degrees per second; the ring runs opposite the shell.
    [SerializeField] float m_ShellSpin = 20f;
    [SerializeField] float m_RingSpin = -60f;

    // Ring and core sizes as fractions of the shell radius.
    [SerializeField]
    [Range(0.1f, 0.9f)]
    float m_RingRadius = 0.45f;

    [SerializeField]
    [Range(0.05f, 0.5f)]
    float m_CoreRadius = 0.17f;

    // How far past the shell color the core is pushed, and its idle scale pulse.
    [SerializeField]
    [Range(1f, 3f)]
    float m_CoreBoost = 1.4f;

    [SerializeField]
    [Range(0f, 0.4f)]
    float m_CorePulse = 0.12f;

    [SerializeField] float m_CorePulseRate = 2f;

    // How far along the aim the barrel sits, in the gun's local space (its scale maps this to world units).
    [SerializeField] float m_BarrelOffset = 0.85f;

    SpriteRenderer m_Shell;
    SpriteRenderer m_Ring;
    SpriteRenderer m_Core;
    float m_CoreScale;

    #endregion
}
