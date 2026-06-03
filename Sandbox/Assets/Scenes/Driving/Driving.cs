using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Joints", "Demonstrating the use of a Wheel Joint for suspension when driving a car.")]
public sealed class Driving : SandboxExampleBehaviour
{
    private float m_SpringFrequency;
    private float m_SpringDamping;
    private float m_MotorSpeed;
    private float m_MaxMotorTorque;

    private PhysicsWheelJoint m_RearWheelJoint;
    private PhysicsWheelJoint m_FrontWheelJoint;
    private float m_Throttle;

    private ControlsMenu.CustomButton m_ReverseButton;
    private ControlsMenu.CustomButton m_ForwardButton;
    private ControlsMenu.CustomButton m_BrakeButton;

    protected override float CameraSize => 10f;
    protected override Vector2 CameraPosition => new(0f, 5f);

    protected override void OnExampleEnable()
    {
        CameraManipulator.DisableManipulators = true;

        // Set controls.
        {
            m_ReverseButton = SandboxManager.ControlsMenu[2];
            m_ForwardButton = SandboxManager.ControlsMenu[1];
            m_BrakeButton = SandboxManager.ControlsMenu[0];

            m_ReverseButton.Set("Reverse [←]");
            m_ForwardButton.Set("Forward [→]");
            m_BrakeButton.Set("Brake [Spc]");
        }

        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.Off);
        SandboxManager.SetOverrideColorShapeState(true);

        m_SpringFrequency = 5f;
        m_SpringDamping = 0.7f;
        m_MotorSpeed = 2000f;
        m_MaxMotorTorque = 10f;
        m_Throttle = 0.0f;
    }

    protected override void OnExampleDisable()
    {
        CameraManipulator.DisableManipulators = false;
    }

    protected override void SetupOptions()
    {
        // Spring Frequency.
        AddSlider("Spring Frequency", m_SpringFrequency, 0f, 60f, v =>
        {
            m_SpringFrequency = v;
            m_RearWheelJoint.springFrequency = m_SpringFrequency;
            m_FrontWheelJoint.springFrequency = m_SpringFrequency;
            m_RearWheelJoint.WakeBodies();
        });

        // Spring Damping.
        AddSlider("Spring Damping", m_SpringDamping, 0f, 4f, v =>
        {
            m_SpringDamping = v;
            m_RearWheelJoint.springDamping = m_SpringDamping;
            m_FrontWheelJoint.springDamping = m_SpringDamping;
            m_RearWheelJoint.WakeBodies();
        });

        // Motor Speed.
        AddSlider("Motor Speed", m_MotorSpeed, -50f, 50f, v =>
        {
            m_MotorSpeed = v;
            SetCarSpeed(m_MotorSpeed * m_Throttle);
        });

        // Max Motor Torque.
        AddSlider("Max Motor Torque", m_MaxMotorTorque, 0f, 20f, v =>
        {
            m_MaxMotorTorque = v;
            m_RearWheelJoint.maxMotorTorque = m_MaxMotorTorque;
            m_FrontWheelJoint.maxMotorTorque = m_MaxMotorTorque;
            m_RearWheelJoint.WakeBodies();
        });
    }

    protected override unsafe void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground Body.
        PhysicsBody groundBody;
        {
            groundBody = world.CreateBody();

            var pointIndex = 25;
            var points = new NativeArray<Vector2>(pointIndex + 1, Allocator.Temp);

            // Fill in reverse to match the chain convention.
            points[pointIndex--] = new Vector2(-20f, 40f);
            points[pointIndex--] = new Vector2(-20f, 20f);
            points[pointIndex--] = new Vector2(-20f, 0f);
            points[pointIndex--] = new Vector2(20f, 0f);

            var heightShift = stackalloc float[] { 0.25f, 1.0f, 4.0f, 0.0f, 0.0f, -1.0f, -2.0f, -2.0f, -1.25f, 0.0f };
            var x = 20f;
            var dx = 5f;
            for (var j = 0; j < 2; ++j)
            {
                for (var i = 0; i < 10; ++i)
                {
                    var y = heightShift[i];
                    points[pointIndex--] = new Vector2(x + dx, y);
                    x += dx;
                }
            }

            // Flat before bridge.
            points[pointIndex--] = new Vector2(x + 40f, 0f);
            points[pointIndex--] = new Vector2(x + 40f, -20f);

            if (pointIndex != -1)
                throw new InvalidOperationException("Invalid Point Index");

            // Create chain.
            groundBody.CreateChain(new ChainGeometry(points), new PhysicsChainDefinition { isLoop = false });
            points.Dispose();

            // Flat after bridge.
            x += 80.0f;
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(x, 0f), point2 = new Vector2(x + 40f, 0f) });

            // Jump Ramp.
            x += 40.0f;
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(x, 0f), point2 = new Vector2(x + 10f, 5f) });
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(x + 10f, 5f), point2 = new Vector2(x + 20f, 0f) });

            // Final Corner.
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(x, 0f), point2 = new Vector2(x + 40f, 0f) });

            x += 40.0f;
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(x, 0f), point2 = new Vector2(x, 20f) });
        }

        // Teeter
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(140f, 1f), angularVelocity = 60 };
            var body = world.CreateBody(bodyDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(20.0f, 0.5f)));

            var pivot = bodyDef.position;
            world.CreateJoint(new PhysicsHingeJointDefinition
            {
                bodyA = groundBody,
                bodyB = body,
                localAnchorA = groundBody.GetLocalPoint(pivot),
                localAnchorB = body.GetLocalPoint(pivot),
                enableLimit = true,
                lowerAngleLimit = -8f,
                upperAngleLimit = 8f
            });
        }

        // Bridge.
        {
            var geometry = new CapsuleGeometry { center1 = Vector2.left, center2 = Vector2.right, radius = 0.125f };

            var prevBody = groundBody;
            const int count = 20;
            for (var n = 0; n < count; ++n)
            {
                var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(161f + 2f * n, -0.125f) };
                var body = world.CreateBody(bodyDef);
                body.CreateShape(geometry);

                var pivot = new Vector2(160f + 2f * n, -0.125f);
                world.CreateJoint(new PhysicsHingeJointDefinition
                {
                    bodyA = prevBody,
                    bodyB = body,
                    localAnchorA = prevBody.GetLocalPoint(pivot),
                    localAnchorB = body.GetLocalPoint(pivot),
                });

                prevBody = body;
            }

            {
                var pivot = new Vector2(160f + 2f * count, -0.125f);
                world.CreateJoint(new PhysicsHingeJointDefinition
                {
                    bodyA = prevBody,
                    bodyB = groundBody,
                    localAnchorA = prevBody.GetLocalPoint(pivot),
                    localAnchorB = groundBody.GetLocalPoint(pivot),
                    enableMotor = true,
                    maxMotorTorque = 50f
                });
            }
        }

        // Boxes
        {
            var box = PolygonGeometry.CreateBox(Vector2.one);

            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition
            {
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.25f, bounciness = 0.25f },
                density = 0.25f
            };

            {
                bodyDef.position = new Vector2(230.0f, 0.5f);
                var body = world.CreateBody(bodyDef);
                body.CreateShape(box, shapeDef);
            }

            {
                bodyDef.position = new Vector2(230.0f, 1.5f);
                var body = world.CreateBody(bodyDef);
                body.CreateShape(box, shapeDef);
            }

            {
                bodyDef.position = new Vector2(230.0f, 0.5f);
                var body = world.CreateBody(bodyDef);
                body.CreateShape(box, shapeDef);
            }

            {
                bodyDef.position = new Vector2(230.0f, 2.5f);
                var body = world.CreateBody(bodyDef);
                body.CreateShape(box, shapeDef);
            }

            {
                bodyDef.position = new Vector2(230.0f, 4.5f);
                var body = world.CreateBody(bodyDef);
                body.CreateShape(box, shapeDef);
            }
        }

        // Car.
        {
            using var car = CarFactory.Spawn(world, Vector2.zero, 1f, m_SpringFrequency, m_SpringDamping, m_MaxMotorTorque, 1f, out m_RearWheelJoint, out m_FrontWheelJoint);
        }
    }

    private void Update()
    {
        // Fetch keyboard input.
        var currentKeyboard = Keyboard.current;

        // Reverse.
        if (m_ReverseButton.isPressed || currentKeyboard.leftArrowKey.isPressed)
        {
            m_Throttle = 1f;
            SetCarSpeed(m_MotorSpeed);
        }

        // Forward.
        if (m_ForwardButton.isPressed || currentKeyboard.rightArrowKey.isPressed)
        {
            m_Throttle = -1f;
            SetCarSpeed(-m_MotorSpeed);
        }

        // Brake.
        if (m_BrakeButton.isPressed || currentKeyboard.spaceKey.isPressed)
        {
            m_Throttle = 0f;
            SetCarSpeed(0f);
        }

        // The Camera should follow the car.
        CameraManipulator.CameraPosition = m_FrontWheelJoint.bodyA.position;
    }

    private void SetCarSpeed(float speed)
    {
        m_RearWheelJoint.motorSpeed = speed;
        m_FrontWheelJoint.motorSpeed = speed;
        m_RearWheelJoint.WakeBodies();
    }
}
