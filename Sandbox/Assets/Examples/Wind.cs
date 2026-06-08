using System;
using Unity.Collections;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrating the application of Wind with drag and lift.")]
public sealed class Wind : SandboxExampleBehaviour
{
    private enum GeometryType
    {
        Circle,
        Capsule,
        Polygon
    }

    private GeometryType m_GeometryType;
    private int m_GeometryCount;
    private float m_WindDirection;
    private float m_WindSpeed;
    private float m_Drag;
    private float m_Lift;

    private CircleGeometry m_CircleGeometry;
    private CapsuleGeometry m_CapsuleGeometry;
    private PolygonGeometry m_PolygonGeometry;

    private Vector2 m_WindNoise;
    private Vector2 m_CurrentWind;
    private NativeList<PhysicsShape> m_Shapes;

    private const float GeometryRadius = 0.1f;

    protected override float CameraSize => 4f;
    protected override Vector2 CameraPosition => new(0f, 1f);

    protected override void OnExampleEnable()
    {
        // Register to update wind.
        PhysicsEvents.PreSimulate += ApplyWind;

        m_GeometryType = GeometryType.Capsule;
        m_GeometryCount = 20;
        m_WindDirection = 0f;
        m_WindSpeed = 6f;
        m_Drag = 1f;
        m_Lift = 0.75f;

        m_WindNoise = Vector2.zero;
        m_CurrentWind = Vector2.zero;
        m_Shapes = new NativeList<PhysicsShape>(Allocator.Persistent);

        m_CircleGeometry = new CircleGeometry { radius = GeometryRadius };
        m_CapsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -GeometryRadius), center2 = new Vector2(0f, GeometryRadius), radius = GeometryRadius * 0.25f };
        m_PolygonGeometry = PolygonGeometry.CreateBox(new Vector2(GeometryRadius * 0.5f, GeometryRadius * 2.5f));
    }

    protected override void OnExampleDisable()
    {
        // Unregister to update wind.
        PhysicsEvents.PreSimulate -= ApplyWind;

        if (m_Shapes.IsCreated)
            m_Shapes.Dispose();
    }

    protected override void SetupOptions()
    {
        // Geometry Type.
        AddEnum("Geometry Type", m_GeometryType, v => m_GeometryType = v, rebuild: true);

        // Geometry Count.
        AddSliderInt("Count", m_GeometryCount, 1, 50, v => m_GeometryCount = v, rebuild: true);

        // Wind Direction.
        AddSlider("Wind Direction", m_WindDirection, 0f, 359f, v => m_WindDirection = v);

        // Wind Speed.
        AddSlider("Wind Speed", m_WindSpeed, 0f, 10f, v => m_WindSpeed = v);

        // Drag.
        AddSlider("Drag", m_Drag, 0f, 1f, v => m_Drag = v);

        // Lift
        AddSlider("Lift", m_Lift, 0f, 4f, v => m_Lift = v);
    }

    protected override void SetupScene()
    {
        var physicsWorld = World;

        var groundBody = physicsWorld.CreateBody();

        var jointDef = new PhysicsHingeJointDefinition
        {
            bodyA = groundBody,
            localAnchorA = new Vector2(0f, 2f + GeometryRadius),
            springFrequency = 0.1f,
            springDamping = 0f,
            enableSpring = true
        };

        var shapeDef = new PhysicsShapeDefinition { density = 20f };
        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, gravityScale = 0.5f, sleepingAllowed = false };

        m_Shapes.Clear();

        for (var n = 0; n < m_GeometryCount; ++n)
        {
            // Create a body.
            bodyDef.position = new Vector2(0.0f, 2.0f - 2.0f * GeometryRadius * n);
            var body = physicsWorld.CreateBody(bodyDef);

            // Set the shape definition custom color.
            shapeDef.surfaceMaterial.customColor = ShapeColor;

            // Create the appropriate shape type.
            switch (m_GeometryType)
            {
                case GeometryType.Circle:
                {
                    m_Shapes.Add(body.CreateShape(m_CircleGeometry, shapeDef));
                    break;
                }
                case GeometryType.Capsule:
                {
                    m_Shapes.Add(body.CreateShape(m_CapsuleGeometry, shapeDef));
                    break;
                }
                case GeometryType.Polygon:
                {
                    m_Shapes.Add(body.CreateShape(m_PolygonGeometry, shapeDef));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Joint to next body.
            jointDef.bodyB =  body;
            jointDef.localAnchorB = new Vector2(0f, GeometryRadius);
            physicsWorld.CreateJoint(jointDef);

            // Set as current body.
            jointDef.bodyA = body;
            jointDef.localAnchorA = new Vector2(0f, -GeometryRadius);
        }
    }

    private void Update()
    {
        var physicsWorld = World;

        physicsWorld.DrawLine(Vector2.zero, Vector2.up * (2f + GeometryRadius), Color.gray);
        physicsWorld.DrawLine(Vector2.zero, m_CurrentWind * 0.2f, Color.goldenRod);
    }

    private void ApplyWind(PhysicsWorld world, float deltaTime)
    {
        if (!m_Shapes.IsCreated || world != PhysicsWorld.defaultWorld)
            return;

        // Calculate the wind.
        var direction = new PhysicsRotate(PhysicsMath.ToRadians(m_WindDirection));
        m_CurrentWind = (direction + m_WindNoise) * m_WindSpeed;

        // Apply the wind.
        foreach (var physicsShape in m_Shapes)
            physicsShape.ApplyWind(m_CurrentWind, m_Drag, m_Lift);

        // Calculate new wind noise.
        ref var random = ref Random;
        var noise = new Vector2(random.NextFloat(-0.3f, 0.3f), random.NextFloat(-0.3f, 0.3f));
        m_WindNoise = Vector2.Lerp(m_WindNoise, noise, 0.05f);
    }
}
