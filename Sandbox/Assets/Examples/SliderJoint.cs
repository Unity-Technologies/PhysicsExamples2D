using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Joints", "Demonstrating the features of the Slider Joint.")]
public sealed class SliderJoint : SandboxExampleBehaviour
{
    private PhysicsSliderJoint m_Joint;

    private float m_SliderAngle;
    private bool m_EnableSpring;
    private float m_SpringTargetTranslation;
    private float m_SpringFrequency;
    private float m_SpringDamping;
    private bool m_EnableMotor;
    private float m_MotorSpeed;
    private float m_MaxMotorForce;
    private bool m_EnableLimit;
    private float m_LowerTranslationLimit;
    private float m_UpperTranslationLimit;

    protected override float CameraSize => 12f;
    protected override Vector2 CameraPosition => new(0f, 9f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        SandboxManager.SetOverrideColorShapeState(true);

        m_SliderAngle = 45f;
        m_EnableSpring = false;
        m_SpringTargetTranslation = 0f;
        m_SpringFrequency = 1f;
        m_SpringDamping = 0.5f;
        m_EnableMotor = false;
        m_MotorSpeed = 2f;
        m_MaxMotorForce = 50f;
        m_EnableLimit = true;
        m_LowerTranslationLimit = -10f;
        m_UpperTranslationLimit = 10f;
    }

    protected override void SetupOptions()
    {
        // Slider Angle.
        AddSlider("Slider Angle", m_SliderAngle, -180f, 180f, v => m_SliderAngle = v, rebuild: true);

        // Enable Spring.
        AddToggle("Enable Spring", m_EnableSpring, v =>
        {
            m_EnableSpring = v;
            m_Joint.enableSpring = m_EnableSpring;
            m_Joint.WakeBodies();
        });

        // Spring Target Translation.
        AddSlider("Spring Target Translation", m_SpringTargetTranslation, -10f, 10f, v =>
        {
            m_SpringTargetTranslation = v;
            m_Joint.springTargetTranslation = m_SpringTargetTranslation;
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
        AddSlider("Motor Speed", m_MotorSpeed, -50f, 50f, v =>
        {
            m_MotorSpeed = v;
            m_Joint.motorSpeed = m_MotorSpeed;
            m_Joint.WakeBodies();
        });

        // Max Motor Force.
        AddSlider("Max Motor Force", m_MaxMotorForce, 0f, 500f, v =>
        {
            m_MaxMotorForce = v;
            m_Joint.maxMotorForce = m_MaxMotorForce;
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
        lowerTranslationLimit = AddSlider("Min Distance Limit", m_LowerTranslationLimit, -10f, 0f, v =>
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
        upperTranslationLimit = AddSlider("Max Distance Limit", m_UpperTranslationLimit, 0f, 10f, v =>
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
            var bodeDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = Vector2.up * 9f };
            var body = world.CreateBody(bodeDef);

            var geometry = new CapsuleGeometry { center1 = Vector2.down * 2f, center2 = Vector2.up * 2f, radius = 1f };
            body.CreateShape(geometry);

            var slideRotation = PhysicsRotate.FromDegrees(m_SliderAngle);

            var jointPivot = new Vector2(0f, 9f);
            var jointDef = new PhysicsSliderJointDefinition
            {
                bodyA = groundBody,
                bodyB = body,
                localAnchorA = new PhysicsTransform(groundBody.GetLocalPoint(jointPivot), slideRotation),
                localAnchorB = new PhysicsTransform(body.GetLocalPoint(jointPivot), slideRotation),
                drawScale = 2f,
                enableSpring = m_EnableSpring,
                springTargetTranslation = m_SpringTargetTranslation,
                springFrequency = m_SpringFrequency,
                springDamping = m_SpringDamping,
                enableMotor = m_EnableMotor,
                motorSpeed = m_MotorSpeed,
                maxMotorForce = m_MaxMotorForce,
                enableLimit = m_EnableLimit,
                lowerTranslationLimit = m_LowerTranslationLimit,
                upperTranslationLimit = m_UpperTranslationLimit
            };

            m_Joint = world.CreateJoint(jointDef);
        }
    }
}
