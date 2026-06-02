using System;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

[ExampleScene("Shapes", "Demonstrating the effect of Friction on shapes.")]
public sealed class Friction : SandboxExampleBehaviour
{
    private enum ObjectType
    {
        Capsule = 0,
        Box = 1
    }

    private ObjectType m_ObjectType;
    private float m_GravityScale;

    private const int ObjectCount = 10;
    private int m_ItemsSpawned;
    private const float SpawnPeriod = 2f;
    private float m_SpawnTime;

    protected override float CameraSize => 24f;
    protected override Vector2 CameraPosition => new(0f, 15f);

    protected override void OnExampleEnable()
    {
        m_ObjectType = ObjectType.Capsule;
        m_GravityScale = 5f;
    }

    private void Update()
    {
        // Finish if the world is paused.
        if (SandboxManager.WorldPaused)
            return;

        if (m_ItemsSpawned >= ObjectCount)
            return;

        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0)
            return;

        m_SpawnTime = SpawnPeriod / math.sqrt(m_GravityScale);
        ++m_ItemsSpawned;

        // Sliding Object.
        {
            var world = World;

            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                gravityScale = m_GravityScale,
                linearVelocity = new Vector2(-1.5f, -0.5f),
                position = new Vector2(15f, 40f)
            };

            var body = world.CreateBody(bodyDef);

            const float frictionDelta = 1.0f / (ObjectCount > 1 ? ObjectCount - 1 : 1);
            var friction = frictionDelta * (m_ItemsSpawned - 1);
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = friction, bounciness = 0f, customColor = ShapeColor } };

            switch (m_ObjectType)
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
    }

    protected override void SetupOptions()
    {
        // Object Type.
        AddEnum("Object Type", m_ObjectType, v => m_ObjectType = v, rebuild: true);

        // Gravity Scale (applied per-body on spawn; rebuilds to respawn with the new scale).
        AddSlider("Gravity Scale", m_GravityScale, 1f, 10f, v => m_GravityScale = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        m_ItemsSpawned = 0;
        m_SpawnTime = 0f;

        var world = World;

        // Ground.
        {
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f } };

            var groundBody = world.CreateBody();

            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 1f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 1f), radius: 0f, new PhysicsTransform(new Vector2(-40f, 1f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 1f), radius: 0f, new PhysicsTransform(new Vector2(40f, 1f), PhysicsRotate.identity)), shapeDef);

            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(4f, 31f), new PhysicsRotate(0.25f))), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(-11f, 28f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(-4f, 22f), new PhysicsRotate(-0.25f))), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(11.5f, 19f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(4f, 14f), new PhysicsRotate(0.25f))), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(-11f, 11f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(-4f, 6f), new PhysicsRotate(-0.25f))), shapeDef);
        }
    }
}
