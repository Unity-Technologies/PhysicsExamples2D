using Unity.Collections;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

[ExampleScene("Benchmarks", "Many large dynamic compound bodies dropped into a container.")]
public sealed class LargeCompound : SandboxExampleBehaviour
{
    private int m_CompoundSize;
    private int m_CompoundSplits;

    protected override float CameraSize => 100f;
    protected override Vector2 CameraPosition => new(0f, 80f);

    protected override void OnExampleEnable()
    {
        m_CompoundSize = 75;
        m_CompoundSplits = 5;
    }

    protected override void SetupOptions()
    {
        // Compound Splits.
        AddSliderInt("Compound Splits", m_CompoundSplits, 1, 20, v => m_CompoundSplits = v, rebuild: true);

        // Compound Size.
        AddSliderInt("Compound Size", m_CompoundSize, 50, 100, v => m_CompoundSize = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground.
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
            groundBody.CreateChain(geometry, new  PhysicsChainDefinition { isLoop = false });
        }

        // Large Dynamic Compounds.
        {
            const float gridSize = 1.0f;
            var gridBoxSize = new Vector2(gridSize, gridSize);

            var splits = m_CompoundSplits + 1;

            var span = m_CompoundSize / splits;

            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { startMassUpdate = false };

            for (var m = 0; m < splits; ++m)
            {
                var bodyY = (m_CompoundSize + 1 + m * span) * gridSize;

                for (var n = 0; n < splits; ++n)
                {
                    var bodyX = -0.5f * gridSize * splits * span + n * span * gridSize;
                    bodyDef.position = new Vector2(bodyX, bodyY);
                    var body = world.CreateBody(bodyDef);

                    for (var i = 0; i < span; ++i)
                    {
                        var y = i * gridSize;
                        for (var j = 0; j < span; ++j)
                        {
                            var x = gridSize * j;
                            shapeDef.surfaceMaterial.customColor = ShapeColor;
                            body.CreateShape(PolygonGeometry.CreateBox(gridBoxSize, radius: 0f, new PhysicsTransform(new Vector2(x, y), PhysicsRotate.identity)), shapeDef);
                        }
                    }

                    // All shapes have been added, so we can efficiently compute the mass properties.
                    body.ApplyMassFromShapes();
                }
            }
        }
    }
}
