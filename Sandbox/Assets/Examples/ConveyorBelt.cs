using Unity.Collections;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

[ExampleScene("Shapes", "Demonstrates the use of the Tangent Speed surface material property.")]
public sealed class ConveyorBelt : SandboxExampleBehaviour
{
    private const int SpawnCount = 10;
    private ControlsMenu.CustomButton m_SpawnButton;
    private PhysicsBody m_ConveyorBeltBody;
    private PhysicsShape m_ConveyorBeltShape;

    private float m_ConveyorSpeed;
    private float m_ConveyorAngle;

    protected override float CameraSize => 17f;
    protected override Vector2 CameraPosition => new(1f, 12f);

    protected override void OnExampleEnable()
    {
        // Set controls.
        m_SpawnButton = SandboxManager.ControlsMenu[0];
        m_SpawnButton.Set("Spawn");
        m_SpawnButton.button.clickable.clicked += SpawnDebris;

        m_ConveyorSpeed = 3f;
        m_ConveyorAngle = 0f;
    }

    protected override void OnExampleDisable()
    {
        m_SpawnButton.button.clickable.clicked -= SpawnDebris;
    }

    protected override void SetupOptions()
    {
        // Conveyor Speed.
        AddSlider("Conveyor Speed", m_ConveyorSpeed, -30f, 30f, v =>
        {
            m_ConveyorSpeed = v;
            UpdateConveyorSpeed();
        });

        // Conveyor Angle.
        AddSlider("Conveyor Angle", m_ConveyorAngle, -25f, 25f, v =>
        {
            m_ConveyorAngle = v;
            UpdateConveyorAngle();
        });
    }

    protected override void SetupScene()
    {
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-20f, 0f),
                new(-20f, 23f),
                new(20f, 23f),
                new(20f, 0f)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        // Platform.
        {
            m_ConveyorBeltBody = world.CreateBody(new PhysicsBodyDefinition { position = Vector2.up * 8f, rotation = new PhysicsRotate(PhysicsMath.ToRadians(m_ConveyorAngle)) });

            var geometry = PolygonGeometry.CreateBox(new Vector2(20f, 0.5f), 0.25f);
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.8f, tangentSpeed = m_ConveyorSpeed } };
            m_ConveyorBeltShape = m_ConveyorBeltBody.CreateShape(geometry, shapeDef);
        }

        // Spawn Debris.
        {
            SpawnDebris();
        }
    }

    private void SpawnDebris()
    {
        var world = World;
        ref var random = ref Random;

        for (var n = 0; n < SpawnCount; ++n)
        {
            var spawnPosition = new Vector2(random.NextFloat(-5f, 5f), random.NextFloat(9f, 20f));
            var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = spawnPosition, rotation = new PhysicsRotate(random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI)) });

            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = ShapeColor } };

            if (random.NextBool())
            {
                var scaleX = random.NextFloat(0.05f, 1.0f);
                var scaleY = random.NextFloat(0.05f, 1.0f);
                var radius = random.NextFloat(0f, 0.2f);
                body.CreateShape(PolygonGeometry.CreateBox(new Vector2(scaleX, scaleY), radius), shapeDef);
            }
            else
            {
                var scale = random.NextFloat(0.2f, 0.6f);
                var radius = random.NextFloat(0.2f, 0.4f);
                body.CreateShape(new CapsuleGeometry { center1 = Vector2.left * scale, center2 = Vector2.right * scale, radius = radius }, shapeDef);
            }
        }
    }

    private void UpdateConveyorAngle()
    {
        // Update the conveyor angle.
        m_ConveyorBeltBody.rotation = new PhysicsRotate(PhysicsMath.ToRadians(m_ConveyorAngle));
    }

    private void UpdateConveyorSpeed()
    {
        // Update the tangent speed.
        var surfaceMaterial = m_ConveyorBeltShape.surfaceMaterial;
        surfaceMaterial.tangentSpeed = m_ConveyorSpeed;
        m_ConveyorBeltShape.surfaceMaterial = surfaceMaterial;
    }
}
