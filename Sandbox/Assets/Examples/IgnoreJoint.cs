using Unity.Collections;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Joints", "Demonstrating the IgnoreJoint to permanently ignore collisions between two bodies.")]
public sealed class IgnoreJoint : SandboxExampleBehaviour
{
    private PhysicsJoint m_Joint;
    private PhysicsBody m_BodyA;
    private PhysicsBody m_BodyB;

    private bool m_EnableJoint;

    protected override float CameraSize => 14f;
    protected override Vector2 CameraPosition => new(0f, 9f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);

        m_EnableJoint = true;
    }

    protected override void SetupOptions()
    {
        // Enable Joint.
        AddToggle("Enable Joint", m_EnableJoint, v =>
        {
            m_EnableJoint = v;
            UpdateJoint();
        }, rebuild: false);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground Body.
        {
            var groundBody = world.CreateBody();

            var vertices = new NativeList<Vector2>(Allocator.Temp);
            vertices.Add(Vector2.right * 17f + Vector2.up * 17f);
            vertices.Add(Vector2.right * 17f);
            vertices.Add(Vector2.left * 17f);
            vertices.Add(Vector2.left * 17f + Vector2.up * 17f);

            var geometry = new ChainGeometry(vertices.AsArray());
            groundBody.CreateChain(geometry, PhysicsChainDefinition.defaultDefinition);
        }

        // Obstacle Body.
        {
            var geometry = PolygonGeometry.CreateBox(size: new Vector2(2f, 6f));

            var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(0f, 3f) });
            body.CreateShape(geometry);
        }

        // Ignored Bodies.
        {
            var geometry = PolygonGeometry.CreateBox(size: new Vector2(4f, 4f));

            m_BodyA = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-4f, 2f) });
            m_BodyA.CreateShape(geometry);

            m_BodyB = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(4f, 2f) });
            m_BodyB.CreateShape(geometry);

            UpdateJoint();
        }
    }

    private void UpdateJoint()
    {
        // Destroy the joint if it's valid.
        if (m_Joint.isValid)
        {
            // NOTE: There seems to be a (reported) bug when deleting a joint not colliding both bodies is deleted. Both bodies won't collide again until moved significantly.
            // A workaround is to disable/enable one of the bodies. This will suffice until a fix is available.
            var body = m_Joint.bodyA;
            body.enabled = false;
            body.enabled = true;

            m_Joint.Destroy();
        }

        // Finish if the joint is not enabled.
        if (!m_EnableJoint)
            return;

        // Get the default world.
        var world = World;

        // Create the joint.
        m_Joint = world.CreateJoint(new PhysicsIgnoreJointDefinition { bodyA = m_BodyA, bodyB = m_BodyB });
    }
}
