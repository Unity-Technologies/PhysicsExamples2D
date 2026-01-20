using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public static class CarFactory
{
    public static NativeList<PhysicsBody> Spawn(PhysicsWorld world, Vector2 position, float carScale, float suspensionFrequency, float suspensionDamping, float maxMotorTorque, float gravityScale, out PhysicsWheelJoint rearWheelJoint, out PhysicsWheelJoint frontWheelJoint, Allocator allocator = Allocator.Temp)
    {
        NativeList<PhysicsBody> bodies = new(allocator);

        var chassisScaling = carScale * 0.85f;
        var vertices = new NativeList<Vector2>(Allocator.Temp)
        {
            new Vector2(-1.5f, -0.5f) * chassisScaling,
            new Vector2(1.5f, -0.5f) * chassisScaling,
            new Vector2(1.5f, 0.0f) * chassisScaling,
            new Vector2(0.0f, 0.9f) * chassisScaling,
            new Vector2(-1.15f, 0.9f) * chassisScaling,
            new Vector2(-1.5f, 0.2f) * chassisScaling
        };

        // Chassis.
        PhysicsBody chassisBody;
        {
            var geometry = PolygonGeometry.Create(vertices.AsArray(), carScale * 0.15f);
            var shapeDef = new PhysicsShapeDefinition { density = 1f / carScale, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.2f } };

            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = position + Vector2.up * carScale, gravityScale = gravityScale };
            chassisBody = world.CreateBody(bodyDef);
            bodies.Add(chassisBody);
            chassisBody.CreateShape(geometry, shapeDef);
        }

        // Wheels.
        PhysicsBody rearWheelBody;
        PhysicsBody frontWheelBody;
        {
            var geometry = new CircleGeometry { radius = carScale * 0.4f };
            var shapeDef = new PhysicsShapeDefinition { density = 2f / carScale, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 1.5f, rollingResistance = 0.1f } };
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = position + new Vector2(-1f * carScale, 0.35f * carScale), gravityScale = gravityScale, fastRotationAllowed = true };

            rearWheelBody = world.CreateBody(bodyDef);
            bodies.Add(rearWheelBody);
            rearWheelBody.CreateShape(geometry, shapeDef);

            bodyDef.position = position + new Vector2(1f * carScale, 0.4f * carScale);
            frontWheelBody = world.CreateBody(bodyDef);
            bodies.Add(frontWheelBody);
            frontWheelBody.CreateShape(geometry, shapeDef);
        }

        var axleRotation = new PhysicsRotate(PhysicsMath.PI * 0.5f);

        // Rear Wheel.
        {
            var pivot = rearWheelBody.position;
            var jointDef = new PhysicsWheelJointDefinition
            {
                bodyA = chassisBody,
                bodyB = rearWheelBody,
                localAnchorA = new PhysicsTransform(chassisBody.GetLocalPoint(pivot), axleRotation),
                localAnchorB = rearWheelBody.GetLocalPoint(pivot),
                enableMotor = true,
                motorSpeed = 0f,
                maxMotorTorque = maxMotorTorque,
                enableSpring = true,
                springFrequency = suspensionFrequency,
                springDamping = suspensionDamping,
                enableLimit = true,
                lowerTranslationLimit = -0.25f * carScale,
                upperTranslationLimit = 0.25f * carScale
            };
            rearWheelJoint = world.CreateJoint(jointDef);
        }

        // Front Wheel.
        {
            var pivot = frontWheelBody.position;
            var jointDef = new PhysicsWheelJointDefinition
            {
                bodyA = chassisBody,
                bodyB = frontWheelBody,
                localAnchorA = new PhysicsTransform(chassisBody.GetLocalPoint(pivot), axleRotation),
                localAnchorB = frontWheelBody.GetLocalPoint(pivot),
                enableMotor = true,
                motorSpeed = 0f,
                maxMotorTorque = maxMotorTorque,
                enableSpring = true,
                springFrequency = suspensionFrequency,
                springDamping = suspensionDamping,
                enableLimit = true,
                lowerTranslationLimit = -0.25f * carScale,
                upperTranslationLimit = 0.25f * carScale
            };
            frontWheelJoint = world.CreateJoint(jointDef);
        }

        return bodies;
    }
}