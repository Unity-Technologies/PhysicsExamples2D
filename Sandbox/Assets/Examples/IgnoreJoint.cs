using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Joints", "Demonstrating the IgnoreJoint to permanently ignore collisions between two bodies.",
    Purpose = "Demonstrates the ignore joint, which stops all contacts between one specific pair of bodies while both still touch everything else. It is useful when two particular bodies must never collide, without changing the contact filtering of either.\nTwo large boxes with an obstacle between them pass through each other while the joint is on, and collide when it is off.",
    Controls = "Enable Joint: turns the ignore joint on or off.")]
public sealed class IgnoreJoint : SandboxExampleBehaviour
{
    private PhysicsJoint m_Joint;
    private PhysicsBody m_BodyA;
    private PhysicsBody m_BodyB;

    private bool m_EnableJoint;

    protected override float CameraSize => 3.5f;
    protected override Vector2 CameraPosition => new(0f, 2.25f);

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
            vertices.Add(Vector2.right * 4.25f + Vector2.up * 4.25f);
            vertices.Add(Vector2.right * 4.25f);
            vertices.Add(Vector2.left * 4.25f);
            vertices.Add(Vector2.left * 4.25f + Vector2.up * 4.25f);

            var geometry = new ChainGeometry(vertices.AsArray());
            groundBody.CreateChain(geometry, PhysicsChainDefinition.defaultDefinition);
        }

        // Obstacle Body.
        {
            var geometry = PolygonGeometry.CreateBox(size: new Vector2(0.5f, 1.5f));

            var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(0f, 0.75f) });
            body.CreateShape(geometry);
        }

        // Ignored Bodies.
        {
            var geometry = PolygonGeometry.CreateBox(size: new Vector2(1f, 1f));

            m_BodyA = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-1f, 0.5f) });
            m_BodyA.CreateShape(geometry);

            m_BodyB = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(1f, 0.5f) });
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
