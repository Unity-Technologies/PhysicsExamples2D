using System;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

[ExampleScene("Benchmarks", "A rotating kinematic tumbler churning many debris pieces.")]
public sealed class Tumbler : SandboxExampleBehaviour
{
    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Polygon = 2,
        Compound = 3,
        Random = 4
    }

    private ObjectType m_ObjectType;
    private float m_AngularVelocity;
    private int m_DebrisCount;
    private float m_GravityScale;

    private Vector2 m_OldGravity;
    private PhysicsBody m_TumblerBody;

    protected override float CameraSize => 34f;
    protected override Vector2 CameraPosition => Vector2.down;

    protected override void OnExampleEnable()
    {
        m_OldGravity = World.gravity;
        m_GravityScale = 2f;
        m_AngularVelocity = 15f;
        m_DebrisCount = 1000;
        m_ObjectType = ObjectType.Polygon;
    }

    protected override void OnExampleDisable()
    {
        // Get the default world.
        var world = World;
        world.gravity = m_OldGravity;
    }

    protected override void SetupOptions()
    {
        // Get the default world.
        var world = World;

        // Object Type.
        AddEnum("Object Type", m_ObjectType, v =>
        {
            m_ObjectType = v;
        }, rebuild: true);

        // Angular Velocity.
        AddSlider("Angular Velocity", m_AngularVelocity, -90f, 90f, v => m_TumblerBody.angularVelocity = m_AngularVelocity = v);

        // Debris Count.
        AddSliderInt("Debris Count", m_DebrisCount, 1, 2000, v => m_DebrisCount = v, rebuild: true);

        // Gravity Scale.
        AddSlider("Gravity Scale", m_GravityScale, 0f, 2f, v =>
        {
            m_GravityScale = v;
            world.gravity = m_OldGravity * m_GravityScale;
        });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;
        world.gravity = m_OldGravity * m_GravityScale;

        // Tumbler.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic, angularVelocity = m_AngularVelocity };
            m_TumblerBody = world.CreateBody(bodyDef);

            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            m_TumblerBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 42f), radius: 0f, new PhysicsTransform(new Vector2(20f, 0f), PhysicsRotate.identity)), shapeDef);
            m_TumblerBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 42f), radius: 0f, new PhysicsTransform(new Vector2(-20f, 0f), PhysicsRotate.identity)), shapeDef);
            m_TumblerBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(42f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, 20f), PhysicsRotate.identity)), shapeDef);
            m_TumblerBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(42f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, -20f), PhysicsRotate.identity)), shapeDef);
        }

        // Tumbler Debris.
        {
            var leftGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(-0.35f, -0.35f), new(0.17f, 0f), new(0f, 0.35f) }.AsSpan());
            var rightGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(0.35f, -0.35f), new(-0.17f, 0f), new(0f, 0.35f) }.AsSpan());

            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            ref var random = ref Random;

            for (var i = 0; i < m_DebrisCount; ++i)
            {
                bodyDef.position = new Vector2(random.NextFloat(-18f, 18f), random.NextFloat(-18f, 18f));
                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = ShapeColor;

                switch (m_ObjectType)
                {
                    case ObjectType.Circle:
                    {
                        CreateCircle(body, shapeDef, ref random);
                        continue;
                    }

                    case ObjectType.Capsule:
                    {
                        CreateCapsule(body, shapeDef, ref random);
                        continue;
                    }

                    case ObjectType.Polygon:
                    {
                        CreatePolygon(body, shapeDef, ref random);
                        continue;
                    }

                    case ObjectType.Compound:
                    {
                        CreateCompound(body, shapeDef, leftGeometry, rightGeometry);
                        continue;
                    }

                    case ObjectType.Random:
                    {
                        switch (i % 4)
                        {
                            case 0:
                            {
                                CreateCircle(body, shapeDef, ref random);
                                continue;
                            }

                            case 1:
                            {
                                CreateCapsule(body, shapeDef, ref random);
                                continue;
                            }

                            case 2:
                            {
                                CreatePolygon(body, shapeDef, ref random);
                                continue;
                            }

                            case 3:
                            {
                                CreateCompound(body, shapeDef, leftGeometry, rightGeometry);
                                continue;
                            }

                            default:
                                continue;
                        }
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private static void CreateCircle(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
        var radius = random.NextFloat(0.25f, 0.45f);
        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = radius };
        body.CreateShape(circleGeometry, shapeDef);
    }

    private static void CreateCapsule(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
        var capsuleLength = random.NextFloat(0.25f, 1.0f);
        var capsuleGeometry = new CapsuleGeometry
        {
            center1 = new Vector2(0f, -0.3f * capsuleLength),
            center2 = new Vector2(0f, 0.3f * capsuleLength),
            radius = random.NextFloat(0.25f, 0.3f)
        };
        body.CreateShape(capsuleGeometry, shapeDef);
    }

    private static void CreatePolygon(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
        var radius = 0.25f * random.NextFloat(0f, 1.0f);
        var polygonGeometry = SandboxUtility.CreateRandomPolygon(extent: 0.35f, radius: radius, ref random);
        body.CreateShape(polygonGeometry, shapeDef);
    }

    private static void CreateCompound(PhysicsBody body, PhysicsShapeDefinition shapeDef, PolygonGeometry leftGeometry, PolygonGeometry rightGeometry)
    {
        body.CreateShape(leftGeometry, shapeDef);
        body.CreateShape(rightGeometry, shapeDef);
    }
}
