using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Conveyor belt via `surfaceMaterial.tangentSpeed` — a static box whose surface velocity drags resting bodies
/// horizontally without the box itself moving. Press space to spawn debris that gets carried along the belt.
/// Adjust `ConveyorSpeed` (m/s) and `ConveyorAngle` (degrees) at runtime to see the effect.
/// </summary>
public class ConveyorBelt : MonoBehaviour
{
    public float ConveyorSpeed = 3f;
    public float ConveyorAngle = 0f;

    private const int SpawnCount = 10;
    public uint RandomSeed = 1234;

    private PhysicsWorld m_PhysicsWorld;
    private PhysicsBody m_ConveyorBeltBody;
    private PhysicsShape m_ConveyorBeltShape;
    private Random m_Random;

    private void OnEnable()
    {
        m_PhysicsWorld = PhysicsWorld.defaultWorld;
        m_Random = new Random(RandomSeed);
        SetupScene();
    }

    private void Update()
    {
        // Live-update tangent speed and platform angle so the change is visible during play.
        UpdateConveyorSpeed();
        UpdateConveyorAngle();

        if (Input.GetKeyDown(KeyCode.Space))
            SpawnDebris();
    }

    private void SetupScene()
    {
        // Bounded chain area.
        {
            var groundBody = m_PhysicsWorld.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-20f, 0f),
                new(-20f, 23f),
                new(20f, 23f),
                new(20f, 0f)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        // Belt platform — Static body whose surface drags via tangentSpeed (no movement of the body itself).
        {
            m_ConveyorBeltBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
            {
                position = Vector2.up * 8f,
                rotation = PhysicsRotate.FromRadians(PhysicsMath.ToRadians(ConveyorAngle))
            });

            var geometry = PolygonGeometry.CreateBox(new Vector2(20f, 0.5f), 0.25f);
            var shapeDef = new PhysicsShapeDefinition
            {
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.8f, tangentSpeed = ConveyorSpeed }
            };
            m_ConveyorBeltShape = m_ConveyorBeltBody.CreateShape(geometry, shapeDef);
        }

        SpawnDebris();
    }

    private void SpawnDebris()
    {
        for (var n = 0; n < SpawnCount; ++n)
        {
            var spawnPosition = new Vector2(m_Random.NextFloat(-5f, 5f), m_Random.NextFloat(9f, 20f));
            var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                position = spawnPosition,
                rotation = PhysicsRotate.FromRadians(m_Random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI))
            });

            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            if (m_Random.NextBool())
            {
                var scaleX = m_Random.NextFloat(0.05f, 1.0f);
                var scaleY = m_Random.NextFloat(0.05f, 1.0f);
                var radius = m_Random.NextFloat(0f, 0.2f);
                body.CreateShape(PolygonGeometry.CreateBox(new Vector2(scaleX, scaleY), radius), shapeDef);
            }
            else
            {
                var scale = m_Random.NextFloat(0.2f, 0.6f);
                var radius = m_Random.NextFloat(0.2f, 0.4f);
                body.CreateShape(new CapsuleGeometry { center1 = Vector2.left * scale, center2 = Vector2.right * scale, radius = radius }, shapeDef);
            }
        }
    }

    private void UpdateConveyorAngle()
    {
        if (m_ConveyorBeltBody.isValid)
            m_ConveyorBeltBody.rotation = PhysicsRotate.FromRadians(PhysicsMath.ToRadians(ConveyorAngle));
    }

    private void UpdateConveyorSpeed()
    {
        if (!m_ConveyorBeltShape.isValid)
            return;

        // Read-modify-write because surfaceMaterial is a struct returned by-value.
        var surfaceMaterial = m_ConveyorBeltShape.surfaceMaterial;
        surfaceMaterial.tangentSpeed = ConveyorSpeed;
        m_ConveyorBeltShape.surfaceMaterial = surfaceMaterial;
    }
}
