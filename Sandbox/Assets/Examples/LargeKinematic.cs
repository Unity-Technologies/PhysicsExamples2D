using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Benchmarks", "A single large rotating kinematic body composed of many shapes.")]
public sealed class LargeKinematic : SandboxExampleBehaviour
{
    private int m_GridSize;
    private float m_GridSpacing;
    private float m_AngularVelocity;

    protected override float CameraSize => 80f;

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        m_GridSize = 100;
        m_GridSpacing = 0f;
        m_AngularVelocity = 90f;
    }

    protected override void SetupOptions()
    {
        // Grid Size.
        AddSliderInt("Grid Size", m_GridSize, 10, 150, v => m_GridSize = v, rebuild: true);

        // Grid Spacing.
        AddSlider("Grid Spacing", m_GridSpacing, 0f, 1f, v => m_GridSpacing = v, rebuild: true);

        // Angular Velocity.
        AddSlider("Angular Velocity", m_AngularVelocity, -360f, 360f, v => m_AngularVelocity = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Rotating Kinematic.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic, angularVelocity = m_AngularVelocity };
            var shapeDef = new PhysicsShapeDefinition { startMassUpdate = false };

            var body = world.CreateBody(bodyDef);

            const float grid = 1f;
            var gridBox = new Vector2(grid, grid);
            var span = m_GridSize / 2;

            for (var i = -span; i < span; ++i)
            {
                var y = i * (grid + m_GridSpacing);
                for (var j = -span; j < span; ++j)
                {
                    var x = j * (grid + m_GridSpacing);

                    shapeDef.surfaceMaterial.customColor = ShapeColor;
                    var boxTransform = new PhysicsTransform(new Vector2(x, y), PhysicsRotate.identity);
                    var offsetBox = PolygonGeometry.CreateBox(gridBox, radius: 0f, boxTransform);
                    body.CreateShape(offsetBox, shapeDef);
                }
            }

            // All shapes have been added, so we can efficiently compute the mass properties.
            body.ApplyMassFromShapes();
        }
    }
}
