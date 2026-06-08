using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Joints", "Demonstrating the features of the Distance Joint.")]
public sealed class DistanceJoint : SandboxExampleBehaviour
{
    private NativeList<PhysicsDistanceJoint> m_Joints;

    private int m_JointCount;
    private float m_JointDistance;
    private bool m_EnableSpring;
    private float m_SpringFrequency;
    private float m_SpringDamping;
    private float m_SpringTension;
    private float m_SpringCompression;
    private bool m_EnableLimit;
    private float m_MinDistanceLimit;
    private float m_MaxDistanceLimit;
    private bool m_EnableMotor;
    private float m_MotorSpeed;
    private float m_MaxMotorForce;

    protected override float CameraSize => 8f;
    protected override Vector2 CameraPosition => new(0f, 15f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        SandboxManager.SetOverrideColorShapeState(true);

        m_JointCount = 3;
        m_JointDistance = 1f;
        m_EnableSpring = false;
        m_SpringFrequency = 5f;
        m_SpringDamping = 0.5f;
        m_SpringTension = 2000f;
        m_SpringCompression = 100f;
        m_EnableLimit = false;
        m_MinDistanceLimit = m_JointDistance;
        m_MaxDistanceLimit = m_JointDistance;
        m_EnableMotor = false;
        m_MotorSpeed = 2f;
        m_MaxMotorForce = 50f;

        m_Joints = new NativeList<PhysicsDistanceJoint>(m_JointCount, Allocator.Persistent);
    }

    protected override void OnExampleDisable()
    {
        if (m_Joints.IsCreated)
            m_Joints.Dispose();
    }

    protected override void SetupOptions()
    {
        // Joint Count.
        AddSliderInt("Joint Count", m_JointCount, 1, 10, v => m_JointCount = v, rebuild: true);

        // Joint Distance.
        AddSlider("Distance", m_JointDistance, 0.1f, 4f, v =>
        {
            m_JointDistance = v;
            UpdateJoints();
        });

        // Enable Spring.
        AddToggle("Enable Spring", m_EnableSpring, v =>
        {
            m_EnableSpring = v;
            UpdateJoints();
        });

        // Spring Frequency.
        AddSlider("Spring Frequency", m_SpringFrequency, 0f, 60f, v =>
        {
            m_SpringFrequency = v;
            UpdateJoints();
        });

        // Spring Damping.
        AddSlider("Spring Damping", m_SpringDamping, 0f, 4f, v =>
        {
            m_SpringDamping = v;
            UpdateJoints();
        });

        // Spring Tension.
        AddSlider("Spring Tension", m_SpringTension, 0f, 4000f, v =>
        {
            m_SpringTension = v;
            UpdateJoints();
        });

        // Spring Compression.
        AddSlider("Spring Compression", m_SpringCompression, 0f, 200f, v =>
        {
            m_SpringCompression = v;
            UpdateJoints();
        });

        // Enable Limit.
        AddToggle("Enable Limit", m_EnableLimit, v =>
        {
            m_EnableLimit = v;
            UpdateJoints();
        });

        // Min Distance Limit.
        Slider minDistanceLimit = null;
        minDistanceLimit = AddSlider("Min Distance Limit", m_MinDistanceLimit, 0.1f, 4f, v =>
        {
            m_MinDistanceLimit = v;
            if (m_MinDistanceLimit > m_MaxDistanceLimit)
            {
                m_MinDistanceLimit = m_MaxDistanceLimit;
                minDistanceLimit.value = m_MinDistanceLimit;
                return;
            }

            RebuildScene();
        });

        // Max Distance Limit.
        Slider maxDistanceLimit = null;
        maxDistanceLimit = AddSlider("Max Distance Limit", m_MaxDistanceLimit, 0.4f, 4f, v =>
        {
            m_MaxDistanceLimit = v;
            if (m_MaxDistanceLimit < m_MinDistanceLimit)
            {
                m_MaxDistanceLimit = m_MinDistanceLimit;
                maxDistanceLimit.value = m_MaxDistanceLimit;
                return;
            }

            RebuildScene();
        });

        // Enable Motor.
        AddToggle("Enable Motor", m_EnableMotor, v =>
        {
            m_EnableMotor = v;
            UpdateJoints();
        });

        // Motor Speed.
        AddSlider("Motor Speed", m_MotorSpeed, -50f, 50f, v =>
        {
            m_MotorSpeed = v;
            UpdateJoints();
        });

        // Max Motor Force.
        AddSlider("Max Motor Force", m_MaxMotorForce, 0f, 500f, v =>
        {
            m_MaxMotorForce = v;
            UpdateJoints();
        });
    }

    protected override void SetupScene()
    {
        m_Joints.Clear();

        // Get the default world.
        var world = World;

        // Ground Body.
        var groundBody = world.CreateBody();

        const float radius = 0.25f;
        var geometry = new CircleGeometry { radius = radius };
        var shapeDef = new PhysicsShapeDefinition { density = 20f };
        var jointDef = new PhysicsDistanceJointDefinition
        {
            distance = m_JointDistance,
            enableSpring = m_EnableSpring,
            springFrequency = m_SpringFrequency,
            springDamping = m_SpringDamping,
            springLowerForce = -m_SpringTension,
            springUpperForce = m_SpringCompression,
            enableLimit = m_EnableLimit,
            minDistanceLimit = m_MinDistanceLimit,
            maxDistanceLimit = m_MaxDistanceLimit,
            enableMotor = m_EnableMotor,
            motorSpeed = m_MotorSpeed,
            maxMotorForce = m_MaxMotorForce
        };

        const float offsetY = 20f;
        var prevBody = groundBody;

        // Joints.
        {
            // Create the joints.
            for (var n = 0; n < m_JointCount; ++n)
            {
                var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, angularDamping = 1f, position = new Vector2(m_JointDistance * (n + 1f), offsetY) };
                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = ShapeColor;
                body.CreateShape(geometry, shapeDef);

                var pivotA = new Vector2(m_JointDistance * n, offsetY);
                var pivotB = new Vector2(m_JointDistance * (n + 1f), offsetY);
                jointDef.bodyA = prevBody;
                jointDef.bodyB = body;
                jointDef.localAnchorA = new PhysicsTransform { position = prevBody.GetLocalPoint(pivotA) };
                jointDef.localAnchorB = new PhysicsTransform { position = body.GetLocalPoint(pivotB) };
                m_Joints.Add(world.CreateJoint(jointDef));

                prevBody = body;
            }
        }
    }

    private void UpdateJoints()
    {
        // Update the max motor torque.
        foreach (var joint in m_Joints)
        {
            joint.distance = m_JointDistance;
            joint.enableSpring = m_EnableSpring;
            joint.springFrequency = m_SpringFrequency;
            joint.springDamping = m_SpringDamping;
            joint.springLowerForce = -m_SpringTension;
            joint.springUpperForce = m_SpringCompression;
            joint.enableLimit = m_EnableLimit;
            joint.minDistanceLimit = m_MinDistanceLimit;
            joint.maxDistanceLimit = m_MaxDistanceLimit;
            joint.enableMotor = m_EnableMotor;
            joint.motorSpeed = m_MotorSpeed;
            joint.maxMotorForce = m_MaxMotorForce;

            joint.WakeBodies();
        }
    }
}
