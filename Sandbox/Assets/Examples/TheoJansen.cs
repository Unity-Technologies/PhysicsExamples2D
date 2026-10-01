using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Joints", "Theo Jansen's walking machine, where a motor turns a wheel that steps six linkage legs across a field of balls.")]
public sealed class TheoJansen : SandboxExampleBehaviour
{
    private static readonly Vector2 Offset = new(0f, 8f);
    private static readonly Vector2 Pivot = new(0f, 0.8f);

    private const float CameraHeight = 6f;
    private const float GroundHalfWidth = 50f;
    private const float GroundWallHeight = 20f;
    private const float BallFieldHalfWidth = 40f;
    private const float BallRadius = 0.25f;
    private const float BallSpacing = 2f;
    private const int BallCount = 50;
    private const float WheelRadius = 1.6f;
    private const float LegSpringFrequency = 10f;
    private const float LegSpringDamping = 0.5f;

    // The legs, chassis and wheel share a negative group so they never touch each other, only the ground and the balls.
    private const int WalkerGroup = -1;

    private float m_MotorSpeed;
    private float m_MotorTorque;

    // Which way the motor is turning: left, braked or right.
    private int m_Direction;

    private ControlsMenu.CustomButton m_LeftButton;
    private ControlsMenu.CustomButton m_RightButton;
    private ControlsMenu.CustomButton m_BrakeButton;

    private PhysicsBody m_ChassisBody;
    private PhysicsBody m_WheelBody;
    private PhysicsHingeJoint m_MotorJoint;

    protected override float CameraSize => 12.5f;
    protected override Vector2 CameraPosition => new(0f, CameraHeight);

    protected override void OnExampleEnable()
    {
        // Set controls.
        {
            m_LeftButton = SandboxManager.ControlsMenu[2];
            m_RightButton = SandboxManager.ControlsMenu[1];
            m_BrakeButton = SandboxManager.ControlsMenu[0];

            m_LeftButton.Set("Left [←]");
            m_RightButton.Set("Right [→]");
            m_BrakeButton.Set("Brake [Spc]");
        }

        // Draw all the joints, and don't let them be turned off, since the joints are what show how the legs are linked.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);

        m_MotorSpeed = 3f;
        m_MotorTorque = 1000f;
    }

    protected override void SetupOptions()
    {
        // Speed.
        AddSlider("Speed", m_MotorSpeed, 0f, 10f, v =>
        {
            m_MotorSpeed = v;
            ApplyMotorSpeed();
        });

        // Torque.
        AddSlider("Torque", m_MotorTorque, 0f, 2000f, v =>
        {
            m_MotorTorque = v;

            if (!m_MotorJoint.isValid)
                return;

            m_MotorJoint.maxMotorTorque = v;
            m_MotorJoint.WakeBodies();
        });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // The walker starts turning to the right.
        m_Direction = 1;

        // Ground, with a wall at each end.
        {
            var groundBody = world.CreateBody();
            var shapeDef = new PhysicsShapeDefinition();

            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-GroundHalfWidth, 0f), point2 = new Vector2(GroundHalfWidth, 0f) }, shapeDef);
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-GroundHalfWidth, 0f), point2 = new Vector2(-GroundHalfWidth, GroundWallHeight) }, shapeDef);
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(GroundHalfWidth, 0f), point2 = new Vector2(GroundHalfWidth, GroundWallHeight) }, shapeDef);
        }

        // Balls, laid out in rows along the ground, with every other row shifted half a step and any more stacked above.
        {
            var radius = BallRadius;
            var spacing = BallSpacing;
            var perRow = Mathf.Max(1, Mathf.FloorToInt(2f * BallFieldHalfWidth / spacing) + 1);

            var circle = new CircleGeometry { radius = radius };
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { density = 1f };
            shapeDef.surfaceMaterial.customColor = ShapeColor;

            for (var i = 0; i < BallCount; ++i)
            {
                var row = i / perRow;
                var column = i % perRow;
                var shift = (row & 1) == 0 ? 0f : 0.5f * spacing;

                bodyDef.position = new Vector2(-BallFieldHalfWidth + column * spacing + shift, radius + row * (2f * radius + 0.02f));
                world.CreateBody(bodyDef).CreateShape(circle, shapeDef);
            }
        }

        var walkerShapeDef = new PhysicsShapeDefinition { density = 1f };
        var contactFilter = walkerShapeDef.contactFilter;
        contactFilter.groupIndex = WalkerGroup;
        walkerShapeDef.contactFilter = contactFilter;

        var dynamicBodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = Pivot + Offset };

        // Chassis.
        m_ChassisBody = world.CreateBody(dynamicBodyDef);
        m_ChassisBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(5f, 2f)), walkerShapeDef);

        // Wheel.
        m_WheelBody = world.CreateBody(dynamicBodyDef);
        m_WheelBody.CreateShape(new CircleGeometry { radius = WheelRadius }, walkerShapeDef);

        // Motor.
        {
            var motorPivot = Pivot + Offset;

            var hingeDef = new PhysicsHingeJointDefinition
            {
                bodyA = m_WheelBody,
                bodyB = m_ChassisBody,
                localAnchorA = m_WheelBody.GetLocalPoint(motorPivot),
                localAnchorB = m_ChassisBody.GetLocalPoint(motorPivot),
                enableMotor = true,
                motorSpeed = PhysicsMath.ToDegrees(m_MotorSpeed),
                maxMotorTorque = m_MotorTorque
            };

            m_MotorJoint = world.CreateJoint(hingeDef);
        }

        var wheelAnchor = Pivot + new Vector2(0f, -0.8f);

        CreateLeg(world, walkerShapeDef, -1f, wheelAnchor);
        CreateLeg(world, walkerShapeDef, 1f, wheelAnchor);

        // Stagger three leg pairs around the wheel so some feet always touch the ground.
        m_WheelBody.transform = new PhysicsTransform(m_WheelBody.position, PhysicsRotate.FromDegrees(120f));
        CreateLeg(world, walkerShapeDef, -1f, wheelAnchor);
        CreateLeg(world, walkerShapeDef, 1f, wheelAnchor);

        m_WheelBody.transform = new PhysicsTransform(m_WheelBody.position, PhysicsRotate.FromDegrees(-120f));
        CreateLeg(world, walkerShapeDef, -1f, wheelAnchor);
        CreateLeg(world, walkerShapeDef, 1f, wheelAnchor);
    }

    private void Update()
    {
        // The camera follows the walker along the ground, whether or not the world is paused, which also puts the camera back if it is panned by hand.
        if (m_ChassisBody.isValid)
            CameraManipulator.CameraPosition = new Vector2(m_ChassisBody.position.x, CameraHeight);

        // Finish if the world is paused.
        if (SandboxManager.WorldPaused)
            return;

        // Fetch keyboard input.
        var currentKeyboard = Keyboard.current;
        var leftPressed = m_LeftButton.isPressed || (currentKeyboard != null && currentKeyboard.leftArrowKey.isPressed);
        var rightPressed = m_RightButton.isPressed || (currentKeyboard != null && currentKeyboard.rightArrowKey.isPressed);
        var brakePressed = m_BrakeButton.isPressed || (currentKeyboard != null && currentKeyboard.spaceKey.isPressed);

        if (leftPressed)
            SetDirection(-1);

        if (rightPressed)
            SetDirection(1);

        if (brakePressed)
            SetDirection(0);
    }

    // Sets which way the motor turns, with zero meaning the brake.
    private void SetDirection(int direction)
    {
        m_Direction = direction;
        ApplyMotorSpeed();
    }

    // Gives the motor the chosen speed in the current direction, converting the speed in radians per second to the degrees per second the joint uses.
    private void ApplyMotorSpeed()
    {
        if (!m_MotorJoint.isValid)
            return;

        m_MotorJoint.motorSpeed = m_Direction * PhysicsMath.ToDegrees(m_MotorSpeed);
        m_MotorJoint.WakeBodies();
    }

    // Creates one leg of a pair, mirrored for a negative side, from two triangles joined by four soft distance joints.
    private void CreateLeg(PhysicsWorld world, PhysicsShapeDefinition shapeDef, float side, Vector2 wheelAnchor)
    {
        var p1 = new Vector2(5.4f * side, -6.1f);
        var p2 = new Vector2(7.2f * side, -1.2f);
        var p3 = new Vector2(4.3f * side, -1.9f);
        var p4 = new Vector2(3.1f * side, 0.8f);
        var p5 = new Vector2(6.0f * side, 1.5f);
        var p6 = new Vector2(2.5f * side, 3.7f);

        // The mirrored side winds the other way, which creating a polygon from vertices handles.
        Span<Vector2> vertices1 = stackalloc Vector2[3] { p1, p2, p3 };
        Span<Vector2> vertices2 = stackalloc Vector2[3] { Vector2.zero, p5 - p4, p6 - p4 };

        var bodyDef1 = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = Offset, angularDamping = 10f };
        var body1 = world.CreateBody(bodyDef1);
        body1.CreateShape(PolygonGeometry.Create(vertices1), shapeDef);

        var bodyDef2 = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = p4 + Offset, angularDamping = 10f };
        var body2 = world.CreateBody(bodyDef2);
        body2.CreateShape(PolygonGeometry.Create(vertices2), shapeDef);

        // Soft distance joints reduce jitter and act like a suspension system.
        {
            var springDef = new PhysicsDistanceJointDefinition
            {
                enableSpring = true,
                springFrequency = LegSpringFrequency,
                springDamping = LegSpringDamping,
                autoDistance = false
            };

            springDef.bodyA = body1;
            springDef.bodyB = body2;

            var anchorA = p2 + Offset;
            var anchorB = p5 + Offset;
            springDef.localAnchorA = body1.GetLocalPoint(anchorA);
            springDef.localAnchorB = body2.GetLocalPoint(anchorB);
            springDef.distance = (anchorA - anchorB).magnitude;
            world.CreateJoint(springDef);

            anchorA = p3 + Offset;
            anchorB = p4 + Offset;
            springDef.localAnchorA = body1.GetLocalPoint(anchorA);
            springDef.localAnchorB = body2.GetLocalPoint(anchorB);
            springDef.distance = (anchorA - anchorB).magnitude;
            world.CreateJoint(springDef);

            springDef.bodyB = m_WheelBody;
            anchorB = wheelAnchor + Offset;
            springDef.localAnchorB = m_WheelBody.GetLocalPoint(anchorB);
            springDef.distance = (anchorA - anchorB).magnitude;
            world.CreateJoint(springDef);

            springDef.bodyA = body2;
            anchorA = p6 + Offset;
            springDef.localAnchorA = body2.GetLocalPoint(anchorA);
            springDef.distance = (anchorA - anchorB).magnitude;
            world.CreateJoint(springDef);
        }

        // The lower triangle pivots on the chassis.
        {
            var legPivot = p4 + Offset;

            var hingeDef = new PhysicsHingeJointDefinition
            {
                bodyA = body2,
                bodyB = m_ChassisBody,
                localAnchorA = body2.GetLocalPoint(legPivot),
                localAnchorB = m_ChassisBody.GetLocalPoint(legPivot)
            };

            world.CreateJoint(hingeDef);
        }
    }
}
