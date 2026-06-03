using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Joints", "Demonstrating a chain of joints.")]
public sealed class BallAndChain : SandboxExampleBehaviour
{
    private const int JointCount = 30;
    private NativeList<PhysicsHingeJoint> m_Joints;

    private float m_MaxMotorTorque;
    private float m_SpringFrequency;
    private float m_SpringDamping;
    private bool m_FixChainLength;

    protected override float CameraSize => 28f;
    protected override Vector2 CameraPosition => new(0f, -8f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);

        m_Joints = new NativeList<PhysicsHingeJoint>(JointCount, Allocator.Persistent);

        m_SpringFrequency = 40f;
        m_SpringDamping = 1f;
        m_MaxMotorTorque = 100f;
    }

    protected override void OnExampleDisable()
    {
        if (m_Joints.IsCreated)
            m_Joints.Dispose();
    }

    protected override void SetupOptions()
    {
        // Spring Frequency.
        AddSlider("Spring Frequency", m_SpringFrequency, 0f, 120f, v => { m_SpringFrequency = v; UpdateJoints(); });

        // Spring Damping.
        AddSlider("Spring Damping", m_SpringDamping, 0f, 10f, v => { m_SpringDamping = v; UpdateJoints(); });

        // Joint Frequency.
        AddSlider("Max Motor Torque ", m_MaxMotorTorque, 0f, 1000f, v => { m_MaxMotorTorque = v; UpdateJoints(); });

        // Fix Chain Length.
        AddToggle("Fix Chain Length", m_FixChainLength, v => m_FixChainLength = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        m_Joints.Clear();

        // Get the default world.
        var world = World;

        const float scale = 0.5f;

        // Ground Body.
        var groundBody = world.CreateBody();

        var prevBody = groundBody;

        // Chain.
        {
            // Create the chain links.
            for (var n = 0; n < JointCount; ++n)
            {
                var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2((1f + 2f * n) * scale, JointCount * scale) };
                var body = world.CreateBody(bodyDef);

                var geometry = new CapsuleGeometry { center1 = Vector2.left * scale, center2 = Vector2.right * scale, radius = 0.125f };
                var shapeDef = new PhysicsShapeDefinition
                {
                    density = 20f,
                    contactFilter = new PhysicsShape.ContactFilter { categories = 0x1, contacts = 0x2 },
                    surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = ShapeColor }
                };
                body.CreateShape(geometry, shapeDef);

                var pivot = new Vector2(2f * n * scale, JointCount * scale);
                var jointDef = new PhysicsHingeJointDefinition
                {
                    bodyA = prevBody,
                    bodyB = body,
                    localAnchorA = prevBody.GetLocalPoint(pivot),
                    localAnchorB = body.GetLocalPoint(pivot),
                    enableMotor = true,
                    maxMotorTorque = m_MaxMotorTorque,
                    enableSpring = n > 0,
                    springFrequency = m_SpringFrequency,
                    springDamping = m_SpringDamping
                };
                m_Joints.Add(world.CreateJoint(jointDef));

                prevBody = body;
            }
        }

        // Ball.
        {
            var geometry = new CircleGeometry { radius = 4f };

            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                position = new Vector2((1f + 2f * JointCount) * scale + geometry.radius - scale, JointCount * scale),
                gravityScale = 3f
            };

            var body = world.CreateBody(bodyDef);

            var shapeDef = new PhysicsShapeDefinition
            {
                density = 20f,
                contactFilter = new PhysicsShape.ContactFilter { categories = 0x2, contacts = 0x1 },
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = ShapeColor }
            };
            body.CreateShape(geometry, shapeDef);

            var pivot = new Vector2(2f * JointCount * scale, JointCount * scale);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = prevBody,
                bodyB = body,
                localAnchorA = prevBody.GetLocalPoint(pivot),
                localAnchorB = body.GetLocalPoint(pivot),
                enableMotor = true,
                maxMotorTorque = m_MaxMotorTorque,
                enableSpring = true,
                springFrequency = m_SpringFrequency,
                springDamping = m_SpringDamping
            };
            m_Joints.Add(world.CreateJoint(jointDef));

            if (m_FixChainLength)
            {
                // Constraint the length of the chain.
                var distance = JointCount;
                world.CreateJoint(new PhysicsDistanceJointDefinition
                {
                    bodyA = groundBody,
                    bodyB = body,
                    localAnchorA = groundBody.GetLocalPoint(Vector2.up * JointCount * scale),
                    localAnchorB = body.GetLocalPoint(pivot),
                    distance = distance,
                    enableSpring = false,
                    springFrequency = 30f,
                    springDamping = 1f,
                    enableLimit = false,
                    minDistanceLimit = -distance,
                    maxDistanceLimit = distance
                });
            }
        }
    }

    private void UpdateJoints()
    {
        // Update the max motor torque.
        foreach (var joint in m_Joints)
        {
            joint.springFrequency = m_SpringFrequency;
            joint.springDamping = m_SpringDamping;
            joint.maxMotorTorque = m_MaxMotorTorque;

            joint.WakeBodies();
        }
    }
}
