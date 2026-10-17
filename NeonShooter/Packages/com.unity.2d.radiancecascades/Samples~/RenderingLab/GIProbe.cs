using System.Collections;
using System.Text;

using Unity.U2D.RadianceCascades;
using UnityEngine;

/// <summary>
/// Reports what the lighting actually produced, so it can be judged on numbers rather than on how it looks.
/// </summary>
/// <remarks>
/// Add this next to <see cref="GILab"/> and enter play mode. It switches on the feature's copy of the finest cascade, reads it back,
/// and logs what the lighting had to work from, where that light sits, and how the gathered light falls off with distance from the emitter.
/// The fraction of texels carrying light is the useful column: a sum or an average hides a buffer that has lost almost everything.
/// </remarks>
public sealed class GIProbe : MonoBehaviour
{
    private void Start()
    {
        m_Feature = FindFeature();

        if (m_Feature == null)
        {
            Debug.LogError("[GIProbe] No radiance cascade feature is loaded, so there is nothing to measure.", this);

            return;
        }

        StartCoroutine(Measure());
    }

    private void OnDisable()
    {
        // The copy costs a full cascade of memory, so it is never left switched on.
        if (m_Feature != null)
            m_Feature.irradianceCaptureEnabled = false;
    }

    // Measures the scene copy the lighting reads and the light it produced from it, and logs the result as one report.
    private IEnumerator Measure()
    {
        m_Feature.irradianceCaptureEnabled = true;

        // The buffer is allocated while the next frame's passes are set up and written while that frame renders, so several frames
        // pass before it holds anything.
        yield return null;
        yield return null;
        yield return null;

        var report = new StringBuilder();

        report.AppendLine("[GIProbe] Fraction of texels carrying light, and the range of what they carry.");

        var color = m_Feature.colorSource;

        if (color?.rt != null)
        {
            report.AppendLine($"scene copy      {Describe(color.rt)}");
            report.AppendLine(DescribeEmitterPlacement(color.rt));
        }
        else
        {
            report.AppendLine("scene copy      nothing captured, so the lighting has nothing to gather from");
        }

        var irradiance = m_Feature.irradiance;

        if (irradiance?.rt != null)
        {
            report.AppendLine($"gathered light  {Describe(irradiance.rt)}");
            report.AppendLine($"  exposure currently applied on top, in stops: {m_Feature.exposure:F1}");

            ReportFalloff(report, irradiance.rt, color?.rt);
        }
        else
        {
            report.AppendLine("gathered light  nothing captured");
        }

        m_Feature.irradianceCaptureEnabled = false;

        Debug.Log(report.ToString());
    }

    // Compares the light actually gathered at a series of distances from the emitter against what a disc of that brightness and size should deliver.
    // A shortfall that stays the same at every distance points at a constant scale somewhere in the merge.
    // A shortfall that grows with distance points instead at the cascades failing to carry light that far.
    private void ReportFalloff(StringBuilder report, RenderTexture irradiance, RenderTexture color)
    {
        var camera = Camera.main;
        var emitter = GameObject.Find("Emitter");

        if (camera == null || emitter == null || color == null)
        {
            report.AppendLine("falloff: nothing to measure against");

            return;
        }

        var pixels = ReadBack(irradiance, out var width, out var height);

        // The emitter's own size and brightness set how much light it can deliver, so both come from the scene rather than being assumed.
        // The size lives on the sprite rather than the transform, so it is read from the lab that placed it.
        var lab = FindAnyObjectByType<GILab>();
        var radius = (lab != null ? lab.emitterSize : 1f) * 0.5f;
        var brightness = BrightestLuminance(color);

        report.AppendLine($"falloff from a disc of radius {radius:F2} at luminance {brightness:F2}, measured across the surface against what it should deliver:");

        for (var distance = 1f; distance <= 6f; ++distance)
        {
            var world = emitter.transform.position + new Vector3(distance, 0f, 0f);
            var viewport = camera.WorldToViewportPoint(world);

            if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
                continue;

            var x = Mathf.Clamp((int)(viewport.x * width), 0, width - 1);
            var y = Mathf.Clamp((int)(viewport.y * height), 0, height - 1);
            var pixel = pixels[y * width + x];

            var measured = pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f;

            // A disc of radius r seen from distance d covers an angle of about 2r/d, and averaging its brightness over the full circle of
            // directions gives what a point on the surface should receive.
            var expected = brightness * (2f * radius / distance) / (2f * Mathf.PI);

            report.AppendLine($"  {distance:F0} units: measured {measured:F5}  expected {expected:F5}  short by {(measured > 0f ? expected / measured : 0f):F1}x");
        }
    }

    // The brightest luminance a buffer holds, which for the scene copy is the light the cascades actually have to work from.
    private static float BrightestLuminance(RenderTexture renderTexture)
    {
        var pixels = ReadBack(renderTexture, out _, out _);
        var max = 0f;

        foreach (var pixel in pixels)
        {
            var luminance = pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f;

            if (luminance > max)
                max = luminance;
        }

        return max;
    }

    // Compares where the light actually sits in the scene copy against where the camera says it is on screen.
    // A ray looks for surfaces at the position the camera gives, so the two have to agree for it to find anything.
    private static string DescribeEmitterPlacement(RenderTexture renderTexture)
    {
        var pixels = ReadBack(renderTexture, out var width, out var height);

        var weightedX = 0.0;
        var weightedY = 0.0;
        var weight = 0.0;

        for (var j = 0; j < height; ++j)
        {
            for (var i = 0; i < width; ++i)
            {
                var pixel = pixels[j * width + i];

                // Only a covered texel counts, since that is the only kind a ray can land on.
                if (pixel.a <= 0.5f)
                    continue;

                var luminance = pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f;

                if (luminance <= RadianceEpsilon)
                    continue;

                weightedX += (i + 0.5) * luminance;
                weightedY += (j + 0.5) * luminance;
                weight += luminance;
            }
        }

        if (weight <= 0.0)
            return "  the scene copy holds no light on any surface at all";

        var camera = Camera.main;
        var emitter = GameObject.Find("Emitter");

        // Read back rows run bottom upwards, matching the shader's own texture coordinates, so the two can be compared directly.
        var foundX = weightedX / weight / width;
        var foundY = weightedY / weight / height;

        if (camera == null || emitter == null)
            return $"  light found in the copy at ({foundX:F4}, {foundY:F4}), with no camera or emitter to compare against";

        var expected = camera.WorldToViewportPoint(emitter.transform.position);

        return $"  light found in the copy at ({foundX:F4}, {foundY:F4}), camera puts it at ({expected.x:F4}, {expected.y:F4})";
    }

    // Reads one texture back and summarizes it: how many texels carry any light, how many are covered by a surface, and the range of what was found.
    private static string Describe(RenderTexture renderTexture)
    {
        var pixels = ReadBack(renderTexture, out _, out _);

        var lit = 0;
        var covered = 0;
        var sum = 0f;
        var max = 0f;

        foreach (var pixel in pixels)
        {
            var luminance = pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f;

            if (luminance > RadianceEpsilon)
                ++lit;

            if (pixel.a > 0.5f)
                ++covered;

            sum += luminance;

            if (luminance > max)
                max = luminance;
        }

        var count = Mathf.Max(1, pixels.Length);

        return $"{renderTexture.width}x{renderTexture.height}  lit {100f * lit / count:F3}%  covered {100f * covered / count:F1}%  mean {sum / count:F6}  max {max:F3}";
    }

    // Copies a render texture into managed memory so its contents can be inspected. Row zero is the bottom row, matching texture coordinates.
    private static Color[] ReadBack(RenderTexture renderTexture, out int width, out int height)
    {
        width = renderTexture.width;
        height = renderTexture.height;

        var readback = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        var previous = RenderTexture.active;

        RenderTexture.active = renderTexture;
        readback.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        readback.Apply(false);
        RenderTexture.active = previous;

        var pixels = readback.GetPixels(0);

        Destroy(readback);

        return pixels;
    }

    // Returns the loaded radiance cascade feature, or null when the render pipeline is not using one.
    private static RadianceCascades2DGIFeature FindFeature()
    {
        var features = Resources.FindObjectsOfTypeAll<RadianceCascades2DGIFeature>();

        return features.Length > 0 ? features[0] : null;
    }

    #region Internal

    // Below this a texel is treated as carrying nothing, which keeps the format's rounding from counting as light.
    private const float RadianceEpsilon = 0.0001f;

    private RadianceCascades2DGIFeature m_Feature;

    #endregion
}
