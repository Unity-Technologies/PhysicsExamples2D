using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Benchmarks", "A spinning paddle churning thousands of debris pieces.")]
public sealed class Spinner : SandboxExampleBehaviour
{
    private Vector2 m_OldGravity;
    private float m_GravityScale;
    private float m_MotorSpeed;
    private float m_MaxMotorTorque;
    private bool m_KinematicSpinner;
    private int m_DebrisCount;
    private float m_DebrisFriction;
    private float m_DebrisBounciness;

    private Slider m_MotorTorqueElement;
    private PhysicsBody m_SpinnerBody;
    private PhysicsHingeJoint m_SpinnerHinge;

    protected override float CameraSize => 48f;

    protected override void OnExampleEnable()
    {
        m_OldGravity = World.gravity;
        m_GravityScale = 1f;
        m_MotorSpeed = 200f;
        m_MaxMotorTorque = 1000000f;
        m_KinematicSpinner = true;
        m_DebrisCount = 3000;
        m_DebrisFriction = 0.1f;
        m_DebrisBounciness = 0.1f;
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
        AddSlider("Motor Speed", m_MotorSpeed, -360f, 360f, v =>
        {
            m_MotorSpeed = v;

            if (m_KinematicSpinner)
                m_SpinnerBody.angularVelocity = m_MotorSpeed;
            else
                m_SpinnerHinge.motorSpeed = m_MotorSpeed;
        });

        // Max Motor Torque.
        m_MotorTorqueElement = AddSlider("Motor Torque", m_MaxMotorTorque, 0f, 100000f, v => m_SpinnerHinge.maxMotorTorque = m_MaxMotorTorque = v);

        // Kinematic Spinner.
        AddToggle("Kinematic Spinner", m_KinematicSpinner, v =>
        {
            m_KinematicSpinner = v;
            m_MotorTorqueElement.enabledSelf = !m_KinematicSpinner;
        }, rebuild: true);

        // Debris Count.
        AddSliderInt("Debris Count", m_DebrisCount, 1000, 5000, v => m_DebrisCount = v, rebuild: true);

        // Debris Friction.
        AddSlider("Debris Friction", m_DebrisFriction, 0f, 1f, v => m_DebrisFriction = v, rebuild: true);

        // Debris Bounciness.
        AddSlider("Debris Bounciness", m_DebrisBounciness, 0f, 1f, v => m_DebrisBounciness = v, rebuild: true);

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

        PhysicsBody groundBody;

        // Chain Surround.
        {
            groundBody = world.CreateBody();

            const int pointCount = 360;

            var chainPoints = new NativeArray<Vector2>(pointCount, Allocator.Temp);

            var tau = PhysicsMath.TAU;
            var rotate = PhysicsRotate.FromRadians(-tau / pointCount);
            var offset = Vector2.right * 40f;
            for (var i = 0; i < pointCount; ++i)
            {
                chainPoints[i] = new Vector2(offset.x, offset.y);
                offset = rotate.RotateVector(offset);
            }

            groundBody.CreateChain(new ChainGeometry(chainPoints), new PhysicsChainDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0f } });
            chainPoints.Dispose();
        }

        // Spinner Paddle.
        {
            var bodyDef = new PhysicsBodyDefinition
            {
                type = m_KinematicSpinner ? PhysicsBody.BodyType.Kinematic : PhysicsBody.BodyType.Dynamic,
                angularVelocity = m_KinematicSpinner ? m_MotorSpeed : 0f,
                position = new Vector2(0f, -20f),
                sleepingAllowed = false
            };

            m_SpinnerBody = world.CreateBody(bodyDef);

            var box = PolygonGeometry.CreateBox(size: new Vector2(0.8f, 39f), radius: 0.2f);
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f } };
            m_SpinnerBody.CreateShape(box, shapeDef);

            // Add hinge if the spinner isn't Kinematic.
            if (!m_KinematicSpinner)
            {
                var hingeDefinition = new PhysicsHingeJointDefinition
                {
                    bodyA = groundBody,
                    bodyB = m_SpinnerBody,
                    localAnchorA = bodyDef.position,
                    enableMotor = true,
                    motorSpeed = m_MotorSpeed,
                    maxMotorTorque = m_MaxMotorTorque
                };
                m_SpinnerHinge = world.CreateJoint(hingeDefinition);
            }
        }

        // Spinner Debris.
        {
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.25f, 0f), center2 = new Vector2(0.25f, 0f), radius = 0.25f };
            var circle = new CircleGeometry { center = Vector2.zero, radius = 0.35f };
            var square = PolygonGeometry.CreateBox(new Vector2(0.35f, 0.35f));

            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { density = 0.25f, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_DebrisFriction, bounciness = m_DebrisBounciness } };

            var x = -23f;
            var y = -30f;

            for (var i = 0; i < m_DebrisCount; ++i)
            {
                bodyDef.position = new Vector2(x, y);
                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = ShapeColor;

                var remainder = i % 3;
                if (remainder == 0)
                {
                    body.CreateShape(capsule, shapeDef);
                }
                else if (remainder == 1)
                {
                    body.CreateShape(circle, shapeDef);
                }
                else if (remainder == 2)
                {
                    body.CreateShape(square, shapeDef);
                }

                x += 0.5f;

                if (x >= 23.0f)
                {
                    x = -23.0f;
                    y += 0.5f;
                }
            }
        }
    }
}
