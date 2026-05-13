using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Factory pattern for a deformable "donut" soft body. `SoftBodyFactory.Create(...)` lays N capsule bodies in a
/// ring and joins each adjacent pair with a `PhysicsFixedJoint` whose `angularFrequency`/`angularDamping` give
/// the joint a spring-like compliance — the result wobbles and squashes like a soft body. Returns a `SoftBody`
/// struct exposing all bodies and joints with a `Destroy()` method.
///
/// Negative `groupIndex` in the contact filter is the standard "members of this group never collide with each
/// other" trick — prevents the ring's own segments from collapsing inward.
/// </summary>
public static class SoftBodyFactory
{
    public struct SoftBody
    {
        public NativeArray<PhysicsBody> bodies;
        public NativeArray<PhysicsJoint> joints;

        public void Destroy()
        {
            if (joints.IsCreated)
            {
                for (var i = 0; i < joints.Length; ++i)
                    if (joints[i].isValid) joints[i].Destroy();
                joints.Dispose();
            }
            if (bodies.IsCreated)
            {
                for (var i = 0; i < bodies.Length; ++i)
                    if (bodies[i].isValid) bodies[i].Destroy();
                bodies.Dispose();
            }
        }
    }

    public static SoftBody Create(
        PhysicsWorld world,
        Vector2 position,
        int sides = 10,
        float scale = 2f,
        float jointFrequency = 7f,
        float jointDamping = 0f,
        bool triggerEvents = false)
    {
        var radius = 1.0f * scale;
        var deltaAngle = PhysicsMath.TAU / sides;
        var length = PhysicsMath.TAU * radius / sides;

        var capsuleGeometry = new CapsuleGeometry
        {
            center1 = new Vector2(0f, -0.5f * length),
            center2 = new Vector2(0f,  0.5f * length),
            radius = 0.25f * scale
        };

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = new PhysicsShapeDefinition
        {
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.3f },
            // Negative groupIndex: members of group -1 never collide with each other.
            contactFilter = new PhysicsShape.ContactFilter { categories = 1, contacts = PhysicsMask.All, groupIndex = -1 },
            triggerEvents = triggerEvents
        };

        var bodies = new NativeArray<PhysicsBody>(sides, Allocator.Persistent);
        var angle = 35f;
        for (var i = 0; i < sides; ++i)
        {
            bodyDef.position = new Vector2(radius * math.cos(angle) + position.x, radius * math.sin(angle) + position.y);
            bodyDef.rotation = PhysicsRotate.FromRadians(angle);

            var body = world.CreateBody(bodyDef);
            body.CreateShape(capsuleGeometry, shapeDef);
            bodies[i] = body;

            angle += deltaAngle;
        }

        // Soft joints between consecutive bodies in the ring.
        var joints = new NativeArray<PhysicsJoint>(sides, Allocator.Persistent);
        var fixedDefinition = new PhysicsFixedJointDefinition
        {
            angularFrequency = jointFrequency,
            angularDamping = jointDamping,
            localAnchorA = new Vector2(0.0f,  0.5f * length),
            localAnchorB = new Vector2(0.0f, -0.5f * length)
        };

        var prevBody = bodies[sides - 1];
        for (var i = 0; i < sides; ++i)
        {
            var currentBody = bodies[i];
            fixedDefinition.bodyA = prevBody;
            fixedDefinition.bodyB = currentBody;
            joints[i] = world.CreateJoint(fixedDefinition);
            prevBody = currentBody;
        }

        return new SoftBody { bodies = bodies, joints = joints };
    }
}

/// <summary>
/// Demo MonoBehaviour: builds one soft donut in OnEnable inside a small chain-bounded chamber, destroys in OnDisable.
/// </summary>
public class SoftBodyFactoryDemo : MonoBehaviour
{
    public Vector2 SpawnPosition = Vector2.zero;
    public int Sides = 10;
    public float Scale = 2f;
    public float JointFrequency = 7f;
    public float JointDamping = 0f;

    private SoftBodyFactory.SoftBody m_SoftBody;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;

        // Bounding chamber so the soft body has something to deform against.
        {
            var groundBody = world.CreateBody();
            using var vertices = new NativeList<Vector2>(4, Allocator.Temp)
            {
                new(-5.5f,  4.5f),
                new( 5.5f,  4.5f),
                new( 5.5f, -4.5f),
                new(-5.5f, -4.5f)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        m_SoftBody = SoftBodyFactory.Create(world, SpawnPosition, Sides, Scale, JointFrequency, JointDamping);
    }

    private void OnDisable()
    {
        m_SoftBody.Destroy();
    }
}
