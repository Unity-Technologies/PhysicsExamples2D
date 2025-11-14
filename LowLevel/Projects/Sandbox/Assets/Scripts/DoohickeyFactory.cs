using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public static class DoohickeyFactory
{
    public static NativeList<PhysicsBody> SpawnDumbbell(PhysicsWorld world, IShapeColorProvider colorProvider, Vector2 position, float scale = 1f, Allocator allocator = Allocator.Temp)
    {
        NativeList<PhysicsBody> bodies = new(allocator);

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { rollingResistance = 0.1f } };

        var circle = new CircleGeometry { radius = 1f * scale };
        var capsule = new CapsuleGeometry { center1 = Vector2.left * 3.5f * scale, center2 = Vector2.right * 3.5f * scale, radius = 0.15f * scale };

        bodyDef.position = position + new Vector2(-5f, 3.0f) * scale;
        var wheelBody1 = world.CreateBody(bodyDef);
        bodies.Add(wheelBody1);
        shapeDef.surfaceMaterial.customColor = colorProvider.ShapeColorState;
        wheelBody1.CreateShape(circle, shapeDef);

        bodyDef.position = position + new Vector2(5f, 3.0f) * scale;
        var wheelBody2 = world.CreateBody(bodyDef);
        bodies.Add(wheelBody2);
        shapeDef.surfaceMaterial.customColor = colorProvider.ShapeColorState;
        wheelBody2.CreateShape(circle, shapeDef);

        bodyDef.position = position + new Vector2(-1.5f, 3.0f) * scale;
        var barBody1 = world.CreateBody(bodyDef);
        bodies.Add(barBody1);
        shapeDef.surfaceMaterial.customColor = colorProvider.ShapeColorState;
        barBody1.CreateShape(capsule, shapeDef);

        bodyDef.position = position + new Vector2(1.5f, 3.0f) * scale;
        var barBody2 = world.CreateBody(bodyDef);
        bodies.Add(barBody2);
        shapeDef.surfaceMaterial.customColor = colorProvider.ShapeColorState;
        barBody2.CreateShape(capsule, shapeDef);

        world.CreateJoint(new PhysicsHingeJointDefinition
        {
            bodyA = wheelBody1,
            bodyB = barBody1,
            localAnchorA = Vector2.zero,
            localAnchorB = Vector2.left * 3.5f * scale,
            enableMotor = true,
            maxMotorTorque = 2f * scale
        });

        world.CreateJoint(new PhysicsHingeJointDefinition
        {
            bodyA = wheelBody2,
            bodyB = barBody2,
            localAnchorA = Vector2.zero,
            localAnchorB = Vector2.right * 3.5f * scale,
            enableMotor = true,
            maxMotorTorque = 2f * scale
        });

        world.CreateJoint(new PhysicsSliderJointDefinition
        {
            bodyA = barBody1,
            bodyB = barBody2,
            localAnchorA = Vector2.right * 2f * scale,
            localAnchorB = Vector2.left * 2f * scale,
            enableLimit = true,
            lowerTranslationLimit = -2f * scale,
            upperTranslationLimit = 2f * scale,
            enableMotor = true,
            maxMotorForce = 2f * scale,
            enableSpring = true,
            springFrequency = 1f,
            springDamping = 0.5f
        });

        return bodies;
    }
}