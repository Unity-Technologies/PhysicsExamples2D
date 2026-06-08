using Unity.Collections;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrating creating pseudo-ellipse shapes as Polygons.")]
public sealed class EllipsePolygons : SandboxExampleBehaviour
{
    private int m_ColumnCount;
    private int m_RowCount;
    private float m_Friction;
    private float m_Bounciness;

    protected override float CameraSize => 17f;
    protected override Vector2 CameraPosition => new(0f, 11f);

    protected override void OnExampleEnable()
    {
        m_ColumnCount = 30;
        m_RowCount = 20;
        m_Friction = 0.6f;
        m_Bounciness = 0f;
    }

    protected override void SetupOptions()
    {
        AddSliderInt("Column Count", m_ColumnCount, 1, 35, v => m_ColumnCount = v, rebuild: true);

        AddSliderInt("Row Count", m_RowCount, 1, 20, v => m_RowCount = v, rebuild: true);

        // Friction.
        AddSlider("Friction", m_Friction, 0f, 1f, v => m_Friction = v, rebuild: true);

        // Bounciness.
        AddSlider("Restitution", m_Bounciness, 0f, 1f, v => m_Bounciness = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        var world = World;

        // Ground.
        {
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            var groundBody = world.CreateBody();

            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(40f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, -1f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(40f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, 101f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 100f), radius: 0f, new PhysicsTransform(new Vector2(19f, 50f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 100f), radius: 0f, new PhysicsTransform(new Vector2(-19f, 50f), PhysicsRotate.identity)), shapeDef);
        }

        // Rounded Polygons.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_Friction, bounciness = m_Bounciness, rollingResistance = 0.2f } };

            var y = 2.0f;
            for (var i = 0; i < m_RowCount; ++i, y += 1.0f)
            {
                var x = m_ColumnCount * -0.5f + 0.5f;
                for (var j = 0; j < m_ColumnCount; ++j, x += 1.0f)
                {
                    bodyDef.position = new Vector2(x, y);
                    var body = world.CreateBody(bodyDef);

                    shapeDef.surfaceMaterial.customColor = ShapeColor;

                    // Create Ellipse Polygon.
                    using var vertices = new NativeList<Vector2>(Allocator.Temp)
                    {
                        new(0.0f, -0.25f),
                        new(0.0f, 0.25f),
                        new(0.05f, 0.075f),
                        new(-0.05f, 0.075f),
                        new(0.05f, -0.075f),
                        new(-0.05f, -0.075f)
                    };
                    body.CreateShape(PolygonGeometry.Create(vertices: vertices.AsArray(), radius: 0.2f), shapeDef);
                }
            }
        }
    }
}
