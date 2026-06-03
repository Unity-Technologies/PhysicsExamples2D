using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

[ExampleScene("Benchmarks", "A grid of rotating kinematic tumblers churning spawned debris.")]
public sealed class ManyTumblers : SandboxExampleBehaviour
{
    private int m_RowCount;
    private int m_ColumnCount;
    private int m_SpawnCount;
    private float m_AngularVelocity;

    private int m_CurrentSpawnCounter;
    private const float m_SpawnPeriod = 0.1f;
    private float m_SpawnTime;

    protected override float CameraSize => 90f;
    protected override Vector2 CameraPosition => new(-4f, -4f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        m_RowCount = 15;
        m_ColumnCount = 15;
        m_SpawnCount = 10;
        m_AngularVelocity = 45f;
    }

    private void Update()
    {
        // Finish if the world is paused.
        if (SandboxManager.WorldPaused)
            return;

        // Limit spawn count.
        if (m_CurrentSpawnCounter >= m_SpawnCount)
            return;

        // Limit spawn period.
        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0f)
            return;

        m_SpawnTime = m_SpawnPeriod;
        m_CurrentSpawnCounter++;

        // Spawn debris.
        SpawnDebris();
    }

    protected override void SetupOptions()
    {
        // Row Count.
        AddSliderInt("Row Count", m_RowCount, 1, 50, v => m_RowCount = v, rebuild: true);

        // Column Count.
        AddSliderInt("Column Count", m_ColumnCount, 1, 50, v => m_ColumnCount = v, rebuild: true);

        // Angular Velocity.
        AddSlider("Angular Velocity", m_AngularVelocity, -90f, 90f, v => m_AngularVelocity = v, rebuild: true);

        // Spawn Count.
        AddSliderInt("Spawn Count", m_SpawnCount, 1, 10, v => m_SpawnCount = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        m_CurrentSpawnCounter = 0;

        // Get the default world.
        var world = World;

        // Tumblers.
        {
            var x = -4.0f * m_ColumnCount;
            for (var i = 0; i < m_ColumnCount; ++i, x += 8f)
            {
                var y = -4.0f * m_RowCount;
                for (var j = 0; j < m_RowCount; ++j, y += 8f)
                {
                    var position = new Vector2(x, y);
                    {
                        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic, position = position, angularVelocity = m_AngularVelocity };
                        var body = world.CreateBody(bodyDef);

                        var shapeDef = new PhysicsShapeDefinition { density = 50f, surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = ShapeColor } };

                        body.CreateShape(
                            PolygonGeometry.CreateBox(new Vector2(0.5f, 4f), 0f, new PhysicsTransform(new Vector2(2f, 0f), PhysicsRotate.identity)),
                            shapeDef);
                        body.CreateShape(
                            PolygonGeometry.CreateBox(new Vector2(0.5f, 4f), 0f, new PhysicsTransform(new Vector2(-2f, 0f), PhysicsRotate.identity)),
                            shapeDef);
                        body.CreateShape(
                            PolygonGeometry.CreateBox(new Vector2(4f, 0.5f), 0f, new PhysicsTransform(new Vector2(0f, 2f), PhysicsRotate.identity)),
                            shapeDef);
                        body.CreateShape(
                            PolygonGeometry.CreateBox(new Vector2(4f, 0.5f), 0f, new PhysicsTransform(new Vector2(0f, -2f), PhysicsRotate.identity)),
                            shapeDef);
                    }
                }
            }
        }
    }

    private void SpawnDebris()
    {
        // Get the default world.
        var world = World;

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(-0.1f, 0.0f), center2 = new Vector2(0.1f, 0.0f), radius = 0.075f };

        var x = -4.0f * m_ColumnCount;
        for (var i = 0; i < m_ColumnCount; ++i, x += 8f)
        {
            var y = -4.0f * m_RowCount;
            for (var j = 0; j < m_RowCount; ++j, y += 8f)
            {
                bodyDef.position = new Vector2(x, y);
                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = ShapeColor;
                body.CreateShape(capsuleGeometry, shapeDef);
            }
        }
    }
}
