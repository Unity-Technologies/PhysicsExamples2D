using Unity.Mathematics;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Benchmarks", "A rotating washer-drum with paddles agitating thousands of debris circles.")]
public sealed class Washer : SandboxExampleBehaviour
{
    private Vector2 m_OldGravity;
    private float m_GravityScale;
    private float m_MotorSpeed;
    private int m_DebrisCount;
    private float m_DebrisFriction;
    private int m_PaddleSpacing;
    private float m_PaddleScale;

    private PhysicsBody m_WasherBody;
    private PhysicsHingeJoint m_WasherHinge;

    protected override float CameraSize => 28f;
    protected override Vector2 CameraPosition => new(0f, 12f);

    protected override void OnExampleEnable()
    {
        m_OldGravity = World.gravity;
        m_GravityScale = 1f;

        m_MotorSpeed = -30f;
        m_DebrisCount = 2500;
        m_DebrisFriction = 0.6f;
        m_PaddleSpacing = 4;
        m_PaddleScale = 0.4f;
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

        // Motor Speed.
        AddSlider("Motor Speed", m_MotorSpeed, -90f, 90f, v =>
        {
            m_MotorSpeed = v;
            m_WasherBody.angularVelocity = m_MotorSpeed;
        });

        // Debris Count.
        AddSliderInt("Debris Count", m_DebrisCount, 1000, 3000, v => m_DebrisCount = v, rebuild: true);

        // Debris Friction.
        AddSlider("Debris Friction", m_DebrisFriction, 0f, 1f, v => m_DebrisFriction = v, rebuild: true);

        // Gravity Scale.
        AddSlider("Gravity Scale", m_GravityScale, 0f, 2f, v =>
        {
            m_GravityScale = v;
            world.gravity = m_OldGravity * m_GravityScale;
        });

        // Paddle Spacing
        AddSliderInt("Paddle Spacing", m_PaddleSpacing, 2, 18, v => m_PaddleSpacing = v, rebuild: true);

        // Paddle Scale.
        AddSlider("Paddle Scale", m_PaddleScale, 0f, 1f, v => m_PaddleScale = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Washer.
        {
            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Kinematic,
                position = Vector2.up * 10f,
                angularVelocity = m_MotorSpeed,
                linearVelocity = new Vector2(0.001f, -0.002f)
            };

            m_WasherBody = world.CreateBody(bodyDef);

            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            var r0 = Mathf.Lerp(14.0f, 10.0f, m_PaddleScale);
            const float r1 = 16.0f;
            const float r2 = 22.0f;

            var angle = PhysicsMath.PI / 18.0f;
            var q = new PhysicsRotate(angle);
            var qo = new PhysicsRotate(angle * 0.1f);
            var u1 = Vector2.right;

            for (var n = 0; n < 36; ++n )
            {
                var u2 = n == 35 ? Vector2.right : q.RotateVector(u1);

                {
                    var a1 = qo.InverseRotateVector(u1);
                    var a2 = qo.RotateVector(u2);

                    var p1 = a1 * r1;
                    var p2 = a1 * r2;
                    var p3 = a2 * r1;
                    var p4 = a2 * r2;
                    m_WasherBody.CreateShape(PolygonGeometry.Create(vertices: new[] { p1, p2, p3, p4 }), shapeDef);
                }

                if ( n % m_PaddleSpacing == 0 )
                {
                    var p1 = u1 * r0;
                    var p2 = u1 * r1;
                    var p3 = u2 * r0;
                    var p4 = u2 * r1;

                    m_WasherBody.CreateShape(PolygonGeometry.Create(vertices: new[] { p1, p2, p3, p4 }), shapeDef);
                }

                u1 = u2;
            }
        }

        // Debris.
        {
            var gridCount = Mathf.Sqrt(m_DebrisCount);
            const float scale = 0.15f;

            var geometry = new CircleGeometry { radius = scale };
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_DebrisFriction, bounciness = 0f } };

            var y = -1.1f * scale * gridCount + 10.0f;
            for (var i = 0; i < gridCount; ++i)
            {
                var x = -1.1f * scale * gridCount;

                for ( var j = 0; j < gridCount; ++j )
                {
                    bodyDef.position = new Vector2(x, y);

                    // Update color state.
                    shapeDef.surfaceMaterial.customColor = ShapeColor;

                    // Create shape.
                    world.CreateBody(bodyDef).CreateShape(geometry, shapeDef);

                    x += 2.1f * scale;
                }

                y += 2.1f * scale;
            }
        }
    }
}
