using Unity.U2D.RadianceCascades;
using UnityEngine;

/// <summary>
/// A controlled scene for judging the radiance cascade lighting: one bright emitter and a few solid occluders, nothing moving.
/// With a single dominant light a shadow wedge is either clearly present or clearly absent, which the shooter's dozens of
/// simultaneous emitters make impossible to tell.
/// </summary>
/// <remarks>
/// Everything is registered with the same <see cref="SpriteRenderSystem"/> the game uses, so this exercises the real lighting path.
/// The created objects are ordinary children, so they can be dragged around in the scene view while playing to watch the shadows follow.
/// </remarks>
public sealed class GILab : MonoBehaviour
{
    private void Start()
    {
        if (SpriteRenderSystem.instance == null)
        {
            Debug.LogError("[GILab] No SpriteRenderSystem in the scene, so nothing can be registered for lighting.", this);

            return;
        }

        m_Sprite = CreateDiscSprite();

        CreateEmitter();
        CreateOccluders();
    }

    // The lights. Anything covering a point blocks what is behind it, so these cast shadows of their own as well as giving off light.
    // Several of them in a line stands in for a stream of projectiles, which is the case a single lamp does not represent.
    private void CreateEmitter()
    {
        for (var i = 0; i < m_EmitterCount; ++i)
        {
            var emitter = new GameObject(i == 0 ? "Emitter" : $"Emitter{i + 1}");
            emitter.transform.SetParent(transform, false);
            emitter.transform.position = m_EmitterPosition + (Vector3)(m_EmitterSpacing * i);

            SpriteRenderSystem.instance.Add(emitter.transform, m_Sprite, Vector2.one * m_EmitterSize, GetEmitterColor(i));
        }
    }

    // The solid bodies. These block light without emitting any, which is what makes the shadow unambiguous.
    private void CreateOccluders()
    {
        for (var i = 0; i < m_OccluderCount; ++i)
        {
            var occluder = new GameObject($"Occluder{i + 1}");
            occluder.transform.SetParent(transform, false);
            occluder.transform.position = m_FirstOccluderPosition + (Vector3)(m_OccluderSpacing * i);

            SpriteRenderSystem.instance.Add(occluder.transform, m_Sprite, Vector2.one * m_OccluderSize, m_OccluderColor);
        }
    }

    // Returns the color of one light. With the hue spread at zero every light is the same color, and raising it walks them around
    // the color wheel, which is what makes color bleeding onto the surface visible rather than leaving one white wash.
    private Color GetEmitterColor(int index)
    {
        if (m_EmitterHueSpread <= 0f || m_EmitterCount < 2)
            return m_EmitterColor;

        Color.RGBToHSV(m_EmitterColor, out var hue, out var saturation, out _);

        var shifted = Color.HSVToRGB(Mathf.Repeat(hue + m_EmitterHueSpread * index / (m_EmitterCount - 1f), 1f), saturation, 1f);

        // The brightness is well above one and the conversion clamps it, so it is reapplied to the color channels alone.
        // Scaling the alpha along with them would drive it past one, which turns the sprite's blend inside out and draws it black.
        var brightness = m_EmitterColor.maxColorComponent;

        return new Color(shifted.r * brightness, shifted.g * brightness, shifted.b * brightness, m_EmitterColor.a);
    }

    // How wide each light is, in world units. The size is carried on the sprite rather than the transform, so a measurement has to read it from here.
    internal float emitterSize => m_EmitterSize;

    // A filled white disc, generated so the scene needs no art. The edge is hard rather than faded, because the lighting treats a
    // point as either a surface or not, and a faded edge leaves a ring of texels that are neither.
    private static Sprite CreateDiscSprite()
    {
        const int size = 64;

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "GILab Disc",
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

        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    #region Internal

    // The light: where it sits, how big it is, and how bright. The color is deliberately far above one, since it is the radiance itself.
    [SerializeField]
    private Vector3 m_EmitterPosition = new(0f, 3.5f, 0f);

    [SerializeField]
    [Min(0.05f)]
    private float m_EmitterSize = 0.6f;

    [SerializeField]
    [ColorUsage(true, true)]
    private Color m_EmitterColor = new(8f, 7.4f, 6f, 1f);

    // How many lights there are and how far apart, so a stream of projectiles can be reproduced rather than a single lamp.
    [SerializeField]
    [Range(1, 60)]
    private int m_EmitterCount = 1;

    [SerializeField]
    private Vector2 m_EmitterSpacing = new(0.4f, 0f);

    // How far around the color wheel the lights are spread. Zero makes them all the same color.
    [SerializeField]
    [Range(0f, 1f)]
    private float m_EmitterHueSpread;

    // The occluders: a row of solid discs below the light, so their shadows fall across empty background.
    [SerializeField]
    [Range(1, 6)]
    private int m_OccluderCount = 2;

    [SerializeField]
    private Vector3 m_FirstOccluderPosition = new(-2f, 0f, 0f);

    [SerializeField]
    private Vector2 m_OccluderSpacing = new(4f, 0f);

    [SerializeField]
    [Min(0.05f)]
    private float m_OccluderSize = 1.2f;

    // Dark enough to give off almost nothing, so what these contribute is the shadow behind them rather than any light of their own.
    [SerializeField]
    [ColorUsage(false)]
    private Color m_OccluderColor = new(0.25f, 0.27f, 0.32f, 1f);

    private Sprite m_Sprite;

    #endregion
}
