using System;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Friction sweep: spawns objects sliding down a zig-zag of slopes; each successive object has a higher
/// `surfaceMaterial.friction` so you can see the effect along the slope chain.
/// </summary>
public class Friction : MonoBehaviour
{
    public enum ObjectType { Capsule = 0, Box = 1 }

    public ObjectType Type = ObjectType.Capsule;
    public float GravityScale = 5f;

    private Vector2 m_OldGravity;

    private const int ObjectCount = 10;
    private int m_ItemsSpawned;
    private const float SpawnPeriod = 2f;
    private float m_SpawnTime;

    private void OnEnable()
    {
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        PhysicsWorld.defaultWorld.gravity = m_OldGravity * GravityScale;

        SetupScene();
    }

    private void OnDisable()
    {
        PhysicsWorld.defaultWorld.gravity = m_OldGravity;
    }

    private void Update()
    {
        if (m_ItemsSpawned >= ObjectCount)
            return;

        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0)
            return;

        m_SpawnTime = SpawnPeriod / math.sqrt(GravityScale);
        ++m_ItemsSpawned;

        var world = PhysicsWorld.defaultWorld;

        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            linearVelocity = new Vector2(-1.5f, -0.5f),
            position = new Vector2(15f, 40f)
        };

        var body = world.CreateBody(bodyDef);

        // Each successive spawn has a higher friction value, sweeping 0..1.
        const float frictionDelta = 1.0f / (ObjectCount > 1 ? ObjectCount - 1 : 1);
        var friction = frictionDelta * (m_ItemsSpawned - 1);
        var shapeDef = new PhysicsShapeDefinition
        {
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = friction, bounciness = 0f }
        };

        switch (Type)
        {
            case ObjectType.Capsule:
            {
                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0f), center2 = new Vector2(0.5f, 0f), radius = 0.5f };
                body.CreateShape(capsuleGeometry, shapeDef);
                break;
            }

            case ObjectType.Box:
            {
                var boxGeometry = PolygonGeometry.CreateBox(new Vector2(1f, 1f));
                body.CreateShape(boxGeometry, shapeDef);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetupScene()
    {
        m_ItemsSpawned = 0;
        m_SpawnTime = 0f;

        var world = PhysicsWorld.defaultWorld;

        // Ground + zig-zag slope chain. Friction here is fixed at 0.1 so the test variable is the moving body.
        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f } };

        var groundBody = world.CreateBody();

        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 1f)), shapeDef);
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 1f), radius: 0f, new PhysicsTransform(new Vector2(-40f, 1f), PhysicsRotate.identity)), shapeDef);
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 1f), radius: 0f, new PhysicsTransform(new Vector2(40f, 1f), PhysicsRotate.identity)), shapeDef);

        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(4f, 31f), PhysicsRotate.FromRadians(0.25f))), shapeDef);
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(-11f, 28f), PhysicsRotate.identity)), shapeDef);
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(-4f, 22f), PhysicsRotate.FromRadians(-0.25f))), shapeDef);
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(11.5f, 19f), PhysicsRotate.identity)), shapeDef);
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(4f, 14f), PhysicsRotate.FromRadians(0.25f))), shapeDef);
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(-11f, 11f), PhysicsRotate.identity)), shapeDef);
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(-4f, 6f), PhysicsRotate.FromRadians(-0.25f))), shapeDef);
    }
}
