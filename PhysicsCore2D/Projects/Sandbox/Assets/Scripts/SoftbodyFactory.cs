using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public static class SoftbodyFactory
{
    public static NativeList<PhysicsBody> SpawnDonut(PhysicsWorld world, IShapeColorProvider colorProvider, Vector2 position, int sides = 7, float scale = 1f, bool triggerEvents = false, float jointFrequency = 5f, float jointDamping = 0.0f, Allocator allocator = Allocator.Temp)
    {
        NativeList<PhysicsBody> bodies = new(allocator);

        var radius = 1.0f * scale;
        var deltaAngle = PhysicsMath.TAU / sides;
        var length = PhysicsMath.TAU * radius / sides;

        var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.5f * length), center2 = new Vector2(0f, 0.5f * length), radius = 0.25f * scale };
        var center = position;

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = new PhysicsShapeDefinition
        {
            surfaceMaterial = new PhysicsShape.SurfaceMaterial
            {
                friction = 0.3f,
                customColor = colorProvider.ShapeColorState
            },
            contactFilter = new PhysicsShape.ContactFilter { categories = 1, contacts = PhysicsMask.All, groupIndex = -1 },
            triggerEvents = triggerEvents
        };

        // Create bodies.
        var bodyIndex = bodies.Length;
        var angle = 35f;
        for (var i = 0; i < sides; ++i)
        {
            bodyDef.position = new Vector2(radius * math.cos(angle) + center.x, radius * math.sin(angle) + center.y);
            bodyDef.rotation = new PhysicsRotate(angle);

            var body = world.CreateBody(bodyDef);
            bodies.Add(body);

            body.CreateShape(capsuleGeometry, shapeDef);

            shapeDef.surfaceMaterial.customColor = colorProvider.ShapeColorState;

            angle += deltaAngle;
        }

        // Create joints.
        var fixedDefinition = new PhysicsFixedJointDefinition
        {
            angularFrequency = jointFrequency,
            angularDamping = jointDamping,
            localAnchorA = new Vector2(0.0f, 0.5f * length),
            localAnchorB = new Vector2(0.0f, -0.5f * length)
        };

        var prevBody = bodies[bodies.Length - 1];
        for (var i = 0; i < sides; ++i)
        {
            var currentBody = bodies[bodyIndex + i];
            fixedDefinition.bodyA = prevBody;
            fixedDefinition.bodyB = currentBody;
            world.CreateJoint(fixedDefinition);
            prevBody = currentBody;
        }

        return bodies;
    }
}