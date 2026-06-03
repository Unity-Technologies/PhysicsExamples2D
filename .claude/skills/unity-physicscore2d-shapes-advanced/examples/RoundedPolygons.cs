using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Rounded polygons. Spawns a grid of dynamic random-polygon bodies in a chamber, each polygon built with a
/// non-zero `radius` argument so its edges are rounded (a Minkowski-sum offset). Surface friction and
/// bounciness apply uniformly. Adjust `ColumnCount`/`RowCount` and run to see how density affects stability.
/// </summary>
public class RoundedPolygons : MonoBehaviour
{
    public int ColumnCount = 20;
    public int RowCount = 20;
    public float Friction = 0.6f;
    public float Bounciness = 0f;
    public uint RandomSeed = 1234;

    private void OnEnable()
    {
        SetupScene();
    }

    private void SetupScene()
    {
        var random = new Random(RandomSeed);
        var world = PhysicsWorld.defaultWorld;

        // 4-walled chamber.
        {
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            var groundBody = world.CreateBody();

            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(40f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, -1f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(40f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, 101f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 100f), radius: 0f, new PhysicsTransform(new Vector2(19f, 50f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 100f), radius: 0f, new PhysicsTransform(new Vector2(-19f, 50f), PhysicsRotate.identity)), shapeDef);
        }

        // Grid of rounded polygons.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = Friction, bounciness = Bounciness } };

            var y = 2.0f;
            for (var i = 0; i < RowCount; ++i, y += 1.0f)
            {
                var x = ColumnCount * -0.5f + 0.5f;
                for (var j = 0; j < ColumnCount; ++j, x += 1.0f)
                {
                    bodyDef.position = new Vector2(x, y);
                    var body = world.CreateBody(bodyDef);

                    var radius = random.NextFloat(0.05f, 0.25f);
                    body.CreateShape(CreateRandomPolygon(extent: 0.5f, radius: radius, ref random), shapeDef);
                }
            }
        }
    }

    /// <summary>
    /// Build a small random convex polygon constrained to a square extent, with rounded corners via the radius arg.
    /// 3-8 vertices, random angles around a circle, random radii within bounds.
    /// </summary>
    private static PolygonGeometry CreateRandomPolygon(float extent, float radius, ref Random random)
    {
        var vertexCount = random.NextInt(3, 9);
        using var vertices = new NativeList<Vector2>(vertexCount, Allocator.Temp);

        // Sorted angles around a circle so the resulting polygon is convex.
        using var angles = new NativeArray<float>(vertexCount, Allocator.Temp);
        for (var i = 0; i < vertexCount; ++i)
            angles[i] = random.NextFloat(0f, PhysicsMath.TAU);

        // Manual sort (NativeArray.Sort is fine here too; manual to avoid extra usings).
        for (var i = 1; i < vertexCount; ++i)
        {
            var key = angles[i];
            var j = i - 1;
            while (j >= 0 && angles[j] > key)
            {
                angles[j + 1] = angles[j];
                --j;
            }
            angles[j + 1] = key;
        }

        for (var i = 0; i < vertexCount; ++i)
        {
            var r = random.NextFloat(extent * 0.5f, extent);
            PhysicsMath.CosSin(angles[i], out var c, out var s);
            vertices.Add(new Vector2(c * r, s * r));
        }

        return PolygonGeometry.Create(vertices.AsArray(), radius: radius);
    }
}
