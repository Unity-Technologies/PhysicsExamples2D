using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public static class GearFactory
{
    public static NativeList<PhysicsBody> Spawn(PhysicsWorld world, IShapeColorProvider colorProvider, PhysicsShape.ContactFilter contactFilter, Vector2 gearPosition, float gearRadius, bool useMotor = false, float motorSpeed = 0f, Allocator allocator = Allocator.Temp)
    {
        NativeList<PhysicsBody> bodies = new(allocator);

        var toothHalfWidth = 0.09f * gearRadius;
        var toothHalfHeight = 0.06f * gearRadius;
        var toothRadius = 0.03f * gearRadius;

        // Ground Body.
        var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
        bodies.Add(groundBody);

        var bodyDef = new PhysicsBodyDefinition
        {
            bodyType = RigidbodyType2D.Dynamic,
            position = gearPosition
        };

        var gearBody = world.CreateBody(bodyDef);
        bodies.Add(gearBody);

        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = contactFilter,
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f }
        };

        if (colorProvider is not { IsShapeColorActive: true })
            shapeDef.surfaceMaterial.customColor = Color.saddleBrown;

        var circle = new CircleGeometry { radius = gearRadius };
        gearBody.CreateShape(circle, shapeDef);

        const int count = 16;
        var deltaAngle = PhysicsMath.TAU / 16f;
        var dq = new PhysicsRotate(deltaAngle);
        var center = new Vector2(gearRadius + toothHalfHeight, 0f);
        var rotation = PhysicsRotate.identity;

        shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f };
        if (colorProvider is not { IsShapeColorActive: true })
            shapeDef.surfaceMaterial.customColor = Color.gray;

        for (var i = 0; i < count; ++i)
        {
            var tooth = PolygonGeometry.CreateBox(
                size: new Vector2(toothHalfWidth, toothHalfHeight) * 2f,
                radius: toothRadius,
                transform: new PhysicsTransform(center, rotation));

            gearBody.CreateShape(tooth, shapeDef);

            rotation = dq.MultiplyRotation(rotation);
            center = rotation.RotateVector(new Vector2(gearRadius + toothHalfHeight, 0.0f));
        }

        var jointDef = new PhysicsHingeJointDefinition
        {
            bodyA = groundBody,
            bodyB = gearBody,
            localAnchorA = groundBody.GetLocalPoint(gearPosition),
            localAnchorB = Vector2.zero,
            enableMotor = useMotor,
            maxMotorTorque = 100000f,
            motorSpeed = motorSpeed
        };

        // Create the gear hinge.
        world.CreateJoint(jointDef);

        return bodies;
    }
}