using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Joints", "Demonstrating the features of the Wheel Joint.",
    Purpose = "Demonstrates the wheel joint, which lets a body slide along one axis against a spring while it rotates freely, the way a car wheel moves on its suspension. It has the spring, limits on the slide, and a motor that spins the wheel.\nA wheel is held on a wheel joint to a fixed point so every setting can be tried.",
    Controls = "Wheel Angle: the direction of the slide axis. The wheel is rebuilt when it changes.\nEnable Spring with Spring Frequency and Spring Damping: the suspension spring.\nEnable Motor with Motor Speed and Max Motor Torque: spins the wheel.\nEnable Limit with Min Distance Limit and Max Distance Limit: limits how far it can slide.")]
public sealed class WheelJoint : SandboxExampleBehaviour
{
    private PhysicsWheelJoint m_Joint;

    private float m_WheelAngle;
    private bool m_EnableSpring;
    private float m_SpringFrequency;
    private float m_SpringDamping;
    private bool m_EnableMotor;
    private float m_MotorSpeed;
    private float m_MaxMotorTorque;
    private bool m_EnableLimit;
    private float m_LowerTranslationLimit;
    private float m_UpperTranslationLimit;

    protected override float CameraSize => 2f;
    protected override Vector2 CameraPosition => new(0f, 10f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        SandboxManager.SetOverrideColorShapeState(true);

        m_WheelAngle = 90f;
        m_EnableSpring = true;
        m_SpringFrequency = 1.5f;
        m_SpringDamping = 0.7f;
        m_EnableMotor = true;
        m_MotorSpeed = 120f;
        m_MaxMotorTorque = 5f;
        m_EnableLimit = true;
        m_LowerTranslationLimit = -1f;
        m_UpperTranslationLimit = 1f;
    }

    protected override void SetupOptions()
    {
        // Wheel Angle.
        AddSlider("Wheel Angle", m_WheelAngle, -180f, 180f, v => m_WheelAngle = v, rebuild: true);

        // Enable Spring.
        AddToggle("Enable Spring", m_EnableSpring, v =>
        {
            m_EnableSpring = v;
            m_Joint.enableSpring = m_EnableSpring;
            m_Joint.WakeBodies();
        });

        // Spring Frequency.
        AddSlider("Spring Frequency", m_SpringFrequency, 0f, 60f, v =>
        {
            m_SpringFrequency = v;
            m_Joint.springFrequency = m_SpringFrequency;
            m_Joint.WakeBodies();
        });

        // Spring Damping.
        AddSlider("Spring Damping", m_SpringDamping, 0f, 4f, v =>
        {
            m_SpringDamping = v;
            m_Joint.springDamping = m_SpringDamping;
            m_Joint.WakeBodies();
        });

        // Enable Motor.
        AddToggle("Enable Motor", m_EnableMotor, v =>
        {
            m_EnableMotor = v;
            m_Joint.enableMotor = m_EnableMotor;
            m_Joint.WakeBodies();
        });

        // Motor Speed.
        AddSlider("Motor Speed", m_MotorSpeed, -3000f, 3000f, v =>
        {
            m_MotorSpeed = v;
            m_Joint.motorSpeed = m_MotorSpeed;
            m_Joint.WakeBodies();
        });

        // Max Motor Torque.
        AddSlider("Max Motor Torque", m_MaxMotorTorque, 0f, 20f, v =>
        {
            m_MaxMotorTorque = v;
            m_Joint.maxMotorTorque = m_MaxMotorTorque;
            m_Joint.WakeBodies();
        });

        // Enable Limit.
        AddToggle("Enable Limit", m_EnableLimit, v =>
        {
            m_EnableLimit = v;
            m_Joint.enableLimit = m_EnableLimit;
            m_Joint.WakeBodies();
        });

        // Lower Translation Limit.
        Slider lowerTranslationLimit = null;
        lowerTranslationLimit = AddSlider("Min Distance Limit", m_LowerTranslationLimit, -4f, 0f, v =>
        {
            m_LowerTranslationLimit = v;
            if (m_LowerTranslationLimit > m_UpperTranslationLimit)
            {
                m_LowerTranslationLimit = m_UpperTranslationLimit;
                lowerTranslationLimit.value = m_LowerTranslationLimit;
                return;
            }

            m_Joint.lowerTranslationLimit = m_LowerTranslationLimit;
            m_Joint.WakeBodies();
        });

        // Upper Translation Limit.
        Slider upperTranslationLimit = null;
        upperTranslationLimit = AddSlider("Max Distance Limit", m_UpperTranslationLimit, 0f, 4f, v =>
        {
            m_UpperTranslationLimit = v;
            if (m_UpperTranslationLimit < m_LowerTranslationLimit)
            {
                m_UpperTranslationLimit = m_LowerTranslationLimit;
                upperTranslationLimit.value = m_UpperTranslationLimit;
                return;
            }

            m_Joint.upperTranslationLimit = m_UpperTranslationLimit;
            m_Joint.WakeBodies();
        });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground Body.
        var groundBody = world.CreateBody();

        {
            var bodeDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = Vector2.up * 10.25f, fastRotationAllowed = true };
            var body = world.CreateBody(bodeDef);

            //var geometry = new CapsuleGeometry { center1 = Vector2.down * 0.5f, center2 = Vector2.up * 0.5f, radius = 0.5f };
            var geometry = new CircleGeometry { radius = 0.5f };
            body.CreateShape(geometry);

            var wheelRotation = PhysicsRotate.FromDegrees(m_WheelAngle);

            var jointPivot = new Vector2(0f, 10f);
            var jointDef = new PhysicsWheelJointDefinition
            {
                bodyA = groundBody,
                bodyB = body,
                autoAxis = false,
                localAnchorA = new PhysicsTransform(groundBody.GetLocalPoint(jointPivot), wheelRotation),
                localAnchorB = PhysicsTransform.identity,
                drawScale = 2f,
                enableSpring = m_EnableSpring,
                springFrequency = m_SpringFrequency,
                springDamping = m_SpringDamping,
                enableMotor = m_EnableMotor,
                motorSpeed = m_MotorSpeed,
                maxMotorTorque = m_MaxMotorTorque,
                enableLimit = m_EnableLimit,
                lowerTranslationLimit = m_LowerTranslationLimit,
                upperTranslationLimit = m_UpperTranslationLimit
            };

            m_Joint = world.CreateJoint(jointDef);
        }
    }
}
