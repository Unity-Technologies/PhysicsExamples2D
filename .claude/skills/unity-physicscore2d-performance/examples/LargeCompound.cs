using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Large-compound-body stress test. `(CompoundSplits+1)^2` dynamic bodies, each composed of `span × span` boxes
/// where `span = CompoundSize / (CompoundSplits+1)`. Defaults: 6×6 = 36 bodies × 12×12 = 144 boxes each ≈ 5184
/// shapes. Demonstrates `startMassUpdate = false` + a single `body.ApplyMassFromShapes()` at the end — the
/// recommended pattern when adding many shapes to one body.
/// </summary>
public class LargeCompound : MonoBehaviour
{
    public int CompoundSize = 75;
    public int CompoundSplits = 5;

    private void OnEnable()
    {
        SetupScene();
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

        // Tall U-shaped chain ground.
        {
            var groundBody = world.CreateBody();
            var span = new Vector2(100f, 500f);

            var vertices = new NativeList<Vector2>(Allocator.Temp);
            vertices.Add(Vector2.right * span.x + Vector2.up * (span.y + 1f));
            vertices.Add(Vector2.right * span.x + Vector2.up * span.y);
            vertices.Add(Vector2.right * span.x);
            vertices.Add(Vector2.left * span.x);
            vertices.Add(Vector2.left * span.x + Vector2.up * span.y);
            vertices.Add(Vector2.left * span.x + Vector2.up * (span.y + 1f));

            var geometry = new ChainGeometry(vertices.AsArray());
            groundBody.CreateChain(geometry, new PhysicsChainDefinition { isLoop = false });
            vertices.Dispose();
        }

        // Large compound bodies.
        const float gridSize = 1.0f;
        var gridBoxSize = new Vector2(gridSize, gridSize);

        var splits = CompoundSplits + 1;
        var span2 = CompoundSize / splits;

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };

        // KEY PERFORMANCE PATTERN: defer mass calc per-shape; compute once at the end via ApplyMassFromShapes.
        var shapeDef = new PhysicsShapeDefinition { startMassUpdate = false };

        for (var m = 0; m < splits; ++m)
        {
            var bodyY = (CompoundSize + 1 + m * span2) * gridSize;

            for (var n = 0; n < splits; ++n)
            {
                var bodyX = -0.5f * gridSize * splits * span2 + n * span2 * gridSize;
                bodyDef.position = new Vector2(bodyX, bodyY);
                var body = world.CreateBody(bodyDef);

                for (var i = 0; i < span2; ++i)
                {
                    var y = i * gridSize;
                    for (var j = 0; j < span2; ++j)
                    {
                        var x = gridSize * j;
                        body.CreateShape(PolygonGeometry.CreateBox(gridBoxSize, radius: 0f, new PhysicsTransform(new Vector2(x, y), PhysicsRotate.identity)), shapeDef);
                    }
                }

                body.ApplyMassFromShapes();
            }
        }
    }
}
