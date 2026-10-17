using Unity.Scripting.LifecycleManagement;
using Unity.U2D.RadianceCascades;

using UnityEngine;

/// <summary>
/// A sprite that takes part in the radiance cascade lighting: it is drawn on screen and it is also what the lighting gathers from.
/// Add it to any GameObject to place a light or a solid body in a test scene, and move, resize and recolor it while playing to
/// watch the lighting follow.
/// </summary>
/// <remarks>
/// The lighting reads the sprites registered with <see cref="SpriteRenderSystem"/> rather than the ones Unity's own renderer draws,
/// so a plain SpriteRenderer placed in the scene is invisible to it. That is what this component is for.
/// A color above one is a light, since the color is the light the sprite gives off. A dark color is a solid body: it blocks what is
/// behind it without adding anything of its own, which is what casts a shadow.
/// </remarks>
[AutoStaticsCleanup]
public sealed partial class GISprite : MonoBehaviour
{
    /// <summary>
    /// The light this sprite gives off. Values above one make it a light; a dark value makes it a body that only blocks light.
    /// </summary>
    public Color color
    {
        get => m_Color;
        set => m_Color = value;
    }

    /// <summary>
    /// How wide and tall the sprite is drawn, in world units.
    /// </summary>
    public Vector2 size
    {
        get => m_Size;
        set => m_Size = value;
    }

    private void OnEnable() => Register();

    // Registering is tried again here because the order two objects are woken in is not fixed, so the system may not have been
    // ready when this was enabled. By the time this runs every object in the scene has been woken, which makes a system still
    // missing a real fault worth reporting rather than a matter of ordering.
    private void Start()
    {
        if (Register())
            return;

        Debug.LogError($"[GISprite] No {nameof(SpriteRenderSystem)} in the scene, so \"{name}\" cannot be registered for lighting.", this);
    }

    // Registers this sprite with the system, and returns whether it is registered once done.
    private bool Register()
    {
        if (m_Instance != null)
            return true;

        var spriteRenderSystem = SpriteRenderSystem.instance;

        if (spriteRenderSystem == null)
            return false;

        m_Instance = spriteRenderSystem.Add(transform, m_Sprite != null ? m_Sprite : GetDiscSprite(), m_Size, m_Color);

        return true;
    }

    private void OnDisable()
    {
        if (m_Instance == null)
            return;

        if (SpriteRenderSystem.instance != null)
            SpriteRenderSystem.instance.Remove(m_Instance);

        m_Instance = null;
    }

    // Pushes the color and size across every frame, so both can be changed in the Inspector while playing and take effect at once.
    // The position is read from the transform by the system itself, so it needs nothing here.
    private void LateUpdate()
    {
        if (m_Instance == null)
            return;

        m_Instance.color = m_Color;
        m_Instance.scale = m_Size;
    }

    // A filled white disc, built once and shared, so a test scene needs no art to start with.
    // The edge is hard rather than faded, because the lighting treats a point as either a surface or not, and a faded edge leaves a
    // ring of texels that are neither.
    private static Sprite GetDiscSprite()
    {
        if (s_Disc != null)
            return s_Disc;

        const int size = 64;

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "GISprite Disc",
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp
        };

        var pixels = new Color32[size * size];
        var centre = (size - 1) * 0.5f;

        for (var y = 0; y < size; ++y)
        {
            for (var x = 0; x < size; ++x)
            {
                var distance = Mathf.Sqrt((x - centre) * (x - centre) + (y - centre) * (y - centre));

                pixels[y * size + x] = new Color32(255, 255, 255, distance <= centre ? (byte)255 : (byte)0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        s_Disc = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        s_Disc.name = "GISprite Disc";
        s_Disc.hideFlags = HideFlags.HideAndDontSave;

        return s_Disc;
    }

    #region Internal

    // What to draw. Left empty, a plain white disc is used, so a new object lights the scene without any art being assigned first.
    [SerializeField]
    private Sprite m_Sprite;

    [SerializeField]
    private Vector2 m_Size = Vector2.one;

    [SerializeField]
    [ColorUsage(true, true)]
    private Color m_Color = new(6f, 5.5f, 5f, 1f);

    private SpriteRenderSystem.SpriteInstance m_Instance;

    private static Sprite s_Disc;

    #endregion
}
