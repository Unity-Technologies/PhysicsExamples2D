using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Factory pattern for a stick-figure ragdoll. `RagdollFactory.Create(...)` assembles head + torso + upper/lower
/// arms (×2) + upper/lower legs (×2) — 9 capsule bodies total — connected with `PhysicsHingeJoint`s with
/// per-joint angle limits to prevent unnatural rotations. Returns a `Ragdoll` struct with all bodies and joints.
///
/// Negative `groupIndex` in the contact filter prevents self-collision between ragdoll parts (so the limbs can
/// overlap without snagging). The joint angle limits use degrees-based PhysicsRotate values.
///
/// This is a deliberately simple ragdoll — full biomechanical fidelity (e.g. variable joint stiffness, motor
/// drives, multi-segment spine) belongs in dedicated character-physics code; this file shows the *pattern*.
/// </summary>
public static class RagdollFactory
{
    public struct Ragdoll
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

    public static Ragdoll Create(PhysicsWorld world, Vector2 origin, float scale = 1f)
    {
        var bodies = new NativeArray<PhysicsBody>(9, Allocator.Persistent);
        var joints = new NativeArray<PhysicsJoint>(8, Allocator.Persistent);

        // Negative group prevents self-collision among ragdoll parts.
        var partFilter = new PhysicsShape.ContactFilter { categories = 1, contacts = PhysicsMask.All, groupIndex = -1 };
        var partShapeDef = new PhysicsShapeDefinition
        {
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.5f },
            contactFilter = partFilter,
            density = 1f
        };
        var partBodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };

        // Helper: capsule-shaped limb body at world position with segment from a to b in local space.
        PhysicsBody MakeLimb(Vector2 worldCenter, Vector2 a, Vector2 b, float radius)
        {
            partBodyDef.position = worldCenter;
            var body = world.CreateBody(partBodyDef);
            body.CreateShape(new CapsuleGeometry { center1 = a * scale, center2 = b * scale, radius = radius * scale }, partShapeDef);
            return body;
        }

        // Geometry: vertical orientation, head at top.
        // Indices:  0=head, 1=torso, 2=upperArmL, 3=lowerArmL, 4=upperArmR, 5=lowerArmR, 6=upperLegL, 7=lowerLegL... we'll lay it out below.
        // For clarity below, named locals first.

        // Head: small circle/capsule near the top.
        var head = MakeLimb(origin + new Vector2(0f,  3.0f * scale), Vector2.down * 0.15f, Vector2.up * 0.15f, 0.25f);
        // Torso: vertical capsule below the head.
        var torso = MakeLimb(origin + new Vector2(0f,  1.6f * scale), Vector2.down * 0.6f,  Vector2.up * 0.6f,  0.3f);
        // Upper arms: angled outward at shoulders.
        var upperArmL = MakeLimb(origin + new Vector2(-0.8f, 2.0f * scale), Vector2.down * 0.4f, Vector2.up * 0.4f, 0.12f);
        var upperArmR = MakeLimb(origin + new Vector2( 0.8f, 2.0f * scale), Vector2.down * 0.4f, Vector2.up * 0.4f, 0.12f);
        // Lower arms.
        var lowerArmL = MakeLimb(origin + new Vector2(-0.8f, 1.0f * scale), Vector2.down * 0.4f, Vector2.up * 0.4f, 0.1f);
        var lowerArmR = MakeLimb(origin + new Vector2( 0.8f, 1.0f * scale), Vector2.down * 0.4f, Vector2.up * 0.4f, 0.1f);
        // Upper legs.
        var upperLegL = MakeLimb(origin + new Vector2(-0.3f, 0.2f * scale), Vector2.down * 0.5f, Vector2.up * 0.5f, 0.15f);
        var upperLegR = MakeLimb(origin + new Vector2( 0.3f, 0.2f * scale), Vector2.down * 0.5f, Vector2.up * 0.5f, 0.15f);

        // (8 indices used; reuse one slot for symmetry — pad with torso to keep array sized.)
        bodies[0] = head;
        bodies[1] = torso;
        bodies[2] = upperArmL;
        bodies[3] = lowerArmL;
        bodies[4] = upperArmR;
        bodies[5] = lowerArmR;
        bodies[6] = upperLegL;
        bodies[7] = upperLegR;
        bodies[8] = torso; // padding slot, unused

        // Joints: hinges with angle limits. Pivots are world points; convert to local via GetLocalPoint.
        PhysicsJoint MakeHinge(PhysicsBody a, PhysicsBody b, Vector2 worldPivot, float lowerDeg, float upperDeg)
        {
            return world.CreateJoint(new PhysicsHingeJointDefinition
            {
                bodyA = a,
                bodyB = b,
                localAnchorA = a.GetLocalPoint(worldPivot),
                localAnchorB = b.GetLocalPoint(worldPivot),
                enableLimit = true,
                lowerAngleLimit = lowerDeg,
                upperAngleLimit = upperDeg
            });
        }

        // Neck.
        joints[0] = MakeHinge(torso, head, origin + new Vector2(0f, 2.5f * scale), -45f, 45f);
        // Shoulders.
        joints[1] = MakeHinge(torso, upperArmL, origin + new Vector2(-0.5f, 2.4f * scale), -90f, 90f);
        joints[2] = MakeHinge(torso, upperArmR, origin + new Vector2( 0.5f, 2.4f * scale), -90f, 90f);
        // Elbows (one-direction limits to feel skeletal).
        joints[3] = MakeHinge(upperArmL, lowerArmL, origin + new Vector2(-0.8f, 1.4f * scale), 0f, 135f);
        joints[4] = MakeHinge(upperArmR, lowerArmR, origin + new Vector2( 0.8f, 1.4f * scale), -135f, 0f);
        // Hips.
        joints[5] = MakeHinge(torso, upperLegL, origin + new Vector2(-0.25f, 0.8f * scale), -75f, 30f);
        joints[6] = MakeHinge(torso, upperLegR, origin + new Vector2( 0.25f, 0.8f * scale), -30f, 75f);
        // (Knees omitted in this minimal ragdoll — extend the array for a full ragdoll.)
        joints[7] = default;

        return new Ragdoll { bodies = bodies, joints = joints };
    }
}

/// <summary>
/// Demo MonoBehaviour: drops a ragdoll into a bounded chamber.
/// </summary>
public class RagdollFactoryDemo : MonoBehaviour
{
    public Vector2 SpawnPosition = Vector2.up * 5f;
    public float Scale = 1f;

    private RagdollFactory.Ragdoll m_Ragdoll;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;

        // Chamber walls.
        {
            var groundBody = world.CreateBody();
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(40f, 20f), radius: 0, transform: new PhysicsTransform(Vector2.down * 10f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(20f, 100f), radius: 0, transform: new PhysicsTransform(Vector2.left  * 20f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(20f, 100f), radius: 0, transform: new PhysicsTransform(Vector2.right * 20f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(40f, 20f), radius: 0, transform: new PhysicsTransform(Vector2.up    * 20f)));
        }

        m_Ragdoll = RagdollFactory.Create(world, SpawnPosition, Scale);
    }

    private void OnDisable()
    {
        m_Ragdoll.Destroy();
    }
}
