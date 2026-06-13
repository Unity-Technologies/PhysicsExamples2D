using System;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Benchmarks", "A large barrel of various object types stress-testing the solver.")]
public sealed class Barrel : SandboxExampleBehaviour
{
    private const int MaxRows = 150;
    private const int MaxColumns = 26;

    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Polygon = 2,
        Mix = 3,
        Compound = 4,
        Ragdoll = 5
    }

    private ObjectType m_ObjectType;
    private float m_CollisionThreshold;

    protected override float CameraSize => 60f;
    protected override Vector2 CameraPosition => new(0f, 58f);

    protected override void OnExampleEnable()
    {
        m_ObjectType = ObjectType.Mix;
        m_CollisionThreshold = 0.5f;
    }

    protected override void SetupOptions()
    {
        // Object Type.
        AddEnum("Object Type", m_ObjectType, v => m_ObjectType = v, rebuild: true);

        AddSlider("Collision Threshold", m_CollisionThreshold, 0f, 1f, v => m_CollisionThreshold = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        SandboxManager.SetOverrideColorShapeState(true);

        CreateBarrel();

        var columnCount = MaxColumns;
        var rowCount = MaxRows;

        if (m_ObjectType == ObjectType.Compound)
        {
            columnCount = 20;
        }
        else if (m_ObjectType == ObjectType.Ragdoll)
        {
            rowCount = 15;
        }

        var shift = 1.15f;
        var centerX = shift * columnCount / 2.0f;
        var centerY = shift / 2.0f;
        var side = -0.1f;
        var extray = 0.5f;

        if (m_ObjectType == ObjectType.Compound)
        {
            extray = 0.25f;
            side = 0.25f;
            shift = 2.0f;
            centerX = shift * columnCount / 2.0f - 1.0f;
        }
        else if (m_ObjectType == ObjectType.Ragdoll)
        {
            columnCount = 11;
            extray = 0.5f;
            side = 1.0f;
            shift = 6.5f;
            centerX = shift * columnCount / 2.0f;
        }

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, collisionThreshold = m_CollisionThreshold };
        if (m_ObjectType == ObjectType.Mix)
            bodyDef.angularDamping = 0.3f;

        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.5f } };

        var ragDollConfiguration = new RagdollFactory.Configuration
        {
            ScaleRange = new Vector2(5f, 9f),
            JointFrequency = 1f,
            JointDamping = 0.1f,
            JointFriction = 0.0f,
            GravityScale = 1f,
            ContactBodyLayer = 2,
            ContactFeetLayer = 1,
            ContactGroupIndex = 0,
            ColorProvider = SandboxManager,
            TriggerEvents = false,
            EnableLimits = true,
            EnableMotor = true
        };

        var leftGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(-1.0f, 0f), new(0.5f, 1f), new(0f, 2f) }.AsSpan());
        var rightGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(1.0f, 0f), new(-0.5f, 1f), new(0f, 2f) }.AsSpan());

        // Get the default world.
        var world = World;

        ref var random = ref Random;

        var startY = m_ObjectType == ObjectType.Ragdoll ? 25.0f : 120.0f;

        for (var i = 0; i < columnCount; ++i)
        {
            var x = i * shift - centerX;

            for (var j = 0; j < rowCount; ++j)
            {
                var y = j * (shift + extray) + centerY + startY;

                bodyDef.position = new Vector2(x + side, y);
                side = -side;

                // Fetch the appropriate shape color.
                shapeDef.surfaceMaterial.customColor = ShapeColor;

                switch (m_ObjectType)
                {
                    case ObjectType.Circle:
                    {
                        var body = world.CreateBody(bodyDef);
                        CreateCircle(body, shapeDef, ref random);
                        continue;
                    }

                    case ObjectType.Capsule:
                    {
                        var body = world.CreateBody(bodyDef);
                        CreateCapsule(body, shapeDef, ref random);
                        continue;
                    }

                    case ObjectType.Polygon:
                    {
                        var body = world.CreateBody(bodyDef);
                        CreatePolygon(body, shapeDef, ref random);
                        continue;
                    }

                    case ObjectType.Mix:
                    {
                        var body = world.CreateBody(bodyDef);
                        switch (i % 3)
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

                            default:
                                continue;
                        }
                    }

                    case ObjectType.Compound:
                    {
                        var body = world.CreateBody(bodyDef);
                        body.CreateShape(leftGeometry, shapeDef);
                        body.CreateShape(rightGeometry, shapeDef);
                        continue;
                    }

                    case ObjectType.Ragdoll:
                    {
                        using var ragdoll = RagdollFactory.Spawn(world, bodyDef.position, ragDollConfiguration, true, ref random);
                        continue;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private static void CreateCircle(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = random.NextFloat(0.25f, 0.75f) };
        body.CreateShape(circleGeometry, shapeDef);
    }

    private static void CreateCapsule(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
        var capsuleLength = random.NextFloat(0.25f, 1.0f);
        var capsuleGeometry = new CapsuleGeometry
        {
            center1 = new Vector2(0f, -0.5f * capsuleLength),
            center2 = new Vector2(0f, 0.5f * capsuleLength),
            radius = random.NextFloat(0.25f, 0.5f)
        };
        body.CreateShape(capsuleGeometry, shapeDef);
    }

    private static void CreatePolygon(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
        var radius = 0.25f * random.NextFloat(0f, 1.0f);
        var polygonGeometry = SandboxUtility.CreateRandomPolygon(extent: 0.75f, radius: radius, ref random);
        body.CreateShape(polygonGeometry, shapeDef);
    }

    private static void CreateBarrel()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;

        var body = world.CreateBody();

        {
            var boxTransform = new PhysicsTransform(new Vector2(0f, 4f), PhysicsRotate.identity);
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(94f, 10f), radius: 0f, transform: boxTransform);
            body.CreateShape(boxGeometry, shapeDef);
        }

        {
            var boxTransform = new PhysicsTransform(new Vector2(-42f, 199f), PhysicsRotate.identity);
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(10f, 400f), radius: 0f, transform: boxTransform);
            body.CreateShape(boxGeometry, shapeDef);
        }

        {
            var boxTransform = new PhysicsTransform(new Vector2(42f, 199f), PhysicsRotate.identity);
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(10f, 400f), radius: 0f, transform: boxTransform);
            body.CreateShape(boxGeometry, shapeDef);
        }

        var segmentGeometry = new SegmentGeometry { point1 = new Vector2(-800f, -80f), point2 = new Vector2(800f, -80f) };
        body.CreateShape(segmentGeometry, shapeDef);
    }
}
