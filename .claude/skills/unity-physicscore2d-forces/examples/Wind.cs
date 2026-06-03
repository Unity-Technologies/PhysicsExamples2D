using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Wind force applied per-shape via `PhysicsShape.ApplyWind(wind, drag, lift)`. A vertical chain of dynamic
/// bodies hangs from a static anchor via spring hinge joints; on each `PreSimulate`, wind (with low-pass-filtered
/// noise) is applied to every shape. Drag opposes motion; lift acts perpendicular to the wind direction.
/// </summary>
public class Wind : MonoBehaviour
{
    public enum GeometryType { Circle, Capsule, Polygon }

    public GeometryType Type = GeometryType.Capsule;
    public int GeometryCount = 20;
    public float WindDirectionDegrees = 0f;
    public float WindSpeed = 6f;
    public float Drag = 1f;
    public float Lift = 0.75f;
    public uint RandomSeed = 1234;

    private const float GeometryRadius = 0.1f;

    private PhysicsWorld m_PhysicsWorld;
    private NativeList<PhysicsShape> m_Shapes;
    private CircleGeometry m_CircleGeometry;
    private CapsuleGeometry m_CapsuleGeometry;
    private PolygonGeometry m_PolygonGeometry;
    private Vector2 m_WindNoise;
    private Vector2 m_CurrentWind;
    private Random m_Random;

    private void OnEnable()
    {
        m_PhysicsWorld = PhysicsWorld.defaultWorld;
        m_Random = new Random(RandomSeed);

        m_Shapes = new NativeList<PhysicsShape>(Allocator.Persistent);

        m_CircleGeometry = new CircleGeometry { radius = GeometryRadius };
        m_CapsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -GeometryRadius), center2 = new Vector2(0f, GeometryRadius), radius = GeometryRadius * 0.25f };
        m_PolygonGeometry = PolygonGeometry.CreateBox(new Vector2(GeometryRadius * 0.5f, GeometryRadius * 2.5f));

        PhysicsEvents.PreSimulate += ApplyWind;
        SetupScene();
    }

    private void OnDisable()
    {
        PhysicsEvents.PreSimulate -= ApplyWind;
        if (m_Shapes.IsCreated)
            m_Shapes.Dispose();
    }

    private void SetupScene()
    {
        var groundBody = m_PhysicsWorld.CreateBody();

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

        for (var n = 0; n < GeometryCount; ++n)
        {
            bodyDef.position = new Vector2(0.0f, 2.0f - 2.0f * GeometryRadius * n);
            var body = m_PhysicsWorld.CreateBody(bodyDef);

            switch (Type)
            {
                case GeometryType.Circle:  m_Shapes.Add(body.CreateShape(m_CircleGeometry, shapeDef)); break;
                case GeometryType.Capsule: m_Shapes.Add(body.CreateShape(m_CapsuleGeometry, shapeDef)); break;
                case GeometryType.Polygon: m_Shapes.Add(body.CreateShape(m_PolygonGeometry, shapeDef)); break;
                default: throw new ArgumentOutOfRangeException();
            }

            // Spring-hinge joint to the previous body in the chain.
            jointDef.bodyB = body;
            jointDef.localAnchorB = new Vector2(0f, GeometryRadius);
            m_PhysicsWorld.CreateJoint(jointDef);

            // Walk the anchor down to the bottom of this body for the next iteration.
            jointDef.bodyA = body;
            jointDef.localAnchorA = new Vector2(0f, -GeometryRadius);
        }
    }

    private void Update()
    {
        m_PhysicsWorld.DrawLine(Vector2.zero, Vector2.up * (2f + GeometryRadius), Color.gray);
        m_PhysicsWorld.DrawLine(Vector2.zero, m_CurrentWind * 0.2f, Color.goldenRod);
    }

    private void ApplyWind(PhysicsWorld world, float deltaTime)
    {
        if (!m_Shapes.IsCreated || world != PhysicsWorld.defaultWorld)
            return;

        var direction = PhysicsRotate.FromRadians(PhysicsMath.ToRadians(WindDirectionDegrees));
        m_CurrentWind = (direction.direction + m_WindNoise) * WindSpeed;

        // ApplyWind dispatches lift+drag based on shape geometry; per-shape so each body's surface area matters.
        foreach (var physicsShape in m_Shapes)
            physicsShape.ApplyWind(m_CurrentWind, Drag, Lift);

        var noise = new Vector2(m_Random.NextFloat(-0.3f, 0.3f), m_Random.NextFloat(-0.3f, 0.3f));
        m_WindNoise = Vector2.Lerp(m_WindNoise, noise, 0.05f);
    }
}
