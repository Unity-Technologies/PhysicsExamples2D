using System;
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Collision", "Checking the stability of very fast continuous collision.")]
public sealed class BounceHouse : SandboxExampleBehaviour
{
    private const float DrawLifetime = 2f;

    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Polygon = 2
    }

    private ObjectType m_ObjectType;
    private PhysicsEvents.ContactHitEvent m_LastHitEvent;

    protected override float CameraSize => 11f;
    protected override Vector2 CameraPosition => new(0f, -1f);

    protected override void OnExampleEnable()
    {
        SandboxManager.SetOverrideColorShapeState(false);
        m_ObjectType = ObjectType.Polygon;
    }

    private void Update()
    {
        // Get the default world.
        var world = World;

        var hitEvents = world.contactHitEvents;
        if (hitEvents.Length > 0)
            m_LastHitEvent = hitEvents[0];

        // Draw the hit event.
        if (m_LastHitEvent.shapeA.isValid)
        {
            var hitPoint = m_LastHitEvent.point;
            world.DrawCircle(hitPoint, 0.25f, Color.orangeRed, DrawLifetime);
            world.DrawLine(hitPoint, hitPoint + m_LastHitEvent.normal, Color.cornsilk, DrawLifetime);
        }
    }

    protected override void SetupOptions()
    {
        // Object Type.
        AddEnum("Object Type", m_ObjectType, v => m_ObjectType = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();

            groundBody.CreateShape(new CircleGeometry { radius = 2.5f });
            groundBody.CreateShape(new CircleGeometry { center = new Vector2(-10f, -9f), radius = 1f });
            groundBody.CreateShape(new CircleGeometry { center = new Vector2(10f, -9f), radius = 1f });
            groundBody.CreateShape(new CircleGeometry { center = new Vector2(10f, 9f), radius = 1f });
            groundBody.CreateShape(new CircleGeometry { center = new Vector2(-10f, 9f), radius = 1f });

            var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
                new (-10f, 9f),
                new (10f, 9f),
                new (10f, -9f),
                new (-10f, -9f)
            };

            var geometry = new ChainGeometry(vertices.AsArray());
            groundBody.CreateChain(geometry, PhysicsChainDefinition.defaultDefinition);

            vertices.Dispose();
        }

        // Bouncing PhysicsShape.
        {
            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                linearVelocity = new Vector2(50f, 40f),
                fastCollisionsAllowed = true,
                fastRotationAllowed = m_ObjectType == ObjectType.Circle,
                gravityScale = 0f,
                position = Vector2.up * 5f
            };

            var body = world.CreateBody(bodyDef);

            var shapeDef = new PhysicsShapeDefinition
            {
                density = 1f,
                hitEvents = true,
                surfaceMaterial = new PhysicsShape.SurfaceMaterial
                {
                    bounciness = 1.2f,
                    friction = 0f,
                    customColor = ShapeColor
                }
            };

            switch (m_ObjectType)
            {
                case ObjectType.Circle:
                {
                    body.CreateShape(new CircleGeometry { center = Vector2.zero, radius = 1f }, shapeDef);
                    return;
                }

                case ObjectType.Capsule:
                {
                    body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-2.5f, 0f), center2 = new Vector2(2.5f, 0f), radius = 0.5f }, shapeDef);
                    return;
                }
                case ObjectType.Polygon:
                {
                    const float h = 0.5f;
                    body.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f * h, h)), shapeDef);
                    return;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
