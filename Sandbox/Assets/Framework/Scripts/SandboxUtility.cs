using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using Random = Unity.Mathematics.Random;

public static class SandboxUtility
{
    public const string HighlightColor = "<color=#7FFFD4>";
    public const string EndHighlightColor = "</color>";
    
    public static PolygonGeometry CreateRandomPolygon(float extent, float radius, ref Random random)
    {
        PolygonGeometry geometry = default;
        geometry.radius = radius;
        ref var vertices = ref geometry.vertices;

        var count = 3 + random.NextInt() % 6;
        geometry.count = count;
        for (var n = 0; n < count; ++n)
        {
            vertices[n] = new Vector2(random.NextFloat(-extent, extent), random.NextFloat(-extent, extent));
        }

        // Validate the geometry.
        geometry = geometry.Validate();

        return geometry.count > 0 ? geometry : PolygonGeometry.CreateBox(new Vector2(extent, extent), radius: radius);
    }
}