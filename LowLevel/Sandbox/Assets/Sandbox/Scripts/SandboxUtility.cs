using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Random = Unity.Mathematics.Random;

public static class SandboxUtility
{
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
