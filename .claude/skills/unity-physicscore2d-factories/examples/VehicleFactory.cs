using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Factory pattern for a 2-wheel vehicle. `VehicleFactory.Create(...)` builds a polygon chassis + 2 circle wheels +
/// 2 wheel joints (with motor + spring suspension), returns a `Vehicle` struct holding all four. The struct
/// exposes `Destroy()` to tear everything down via the canonical destroy-with-isValid-guard pattern.
///
/// Bodies the factory creates: chassis (Dynamic), rear wheel (Dynamic), front wheel (Dynamic).
/// Joints: rearWheelJoint (PhysicsWheelJoint), frontWheelJoint (PhysicsWheelJoint).
/// </summary>
public static class VehicleFactory
{
    public struct Vehicle
    {
        public PhysicsBody chassis;
        public PhysicsBody rearWheel;
        public PhysicsBody frontWheel;
        public PhysicsWheelJoint rearWheelJoint;
        public PhysicsWheelJoint frontWheelJoint;

        public void Destroy()
        {
            // Joints first, then bodies. Each guarded with isValid since joints can be auto-destroyed by body destroy.
            if (rearWheelJoint.isValid) rearWheelJoint.Destroy();
            if (frontWheelJoint.isValid) frontWheelJoint.Destroy();
            if (rearWheel.isValid) rearWheel.Destroy();
            if (frontWheel.isValid) frontWheel.Destroy();
            if (chassis.isValid) chassis.Destroy();
        }
    }

    public static Vehicle Create(
        PhysicsWorld world,
        Vector2 position,
        float carScale = 1f,
        float suspensionFrequency = 5f,
        float suspensionDamping = 0.7f,
        float maxMotorTorque = 10f,
        float gravityScale = 1f)
    {
        var result = default(Vehicle);

        // Chassis polygon vertices (rounded via radius arg).
        var chassisScaling = carScale * 0.85f;
        using var vertices = new NativeList<Vector2>(Allocator.Temp)
        {
            new Vector2(-1.5f, -0.5f) * chassisScaling,
            new Vector2( 1.5f, -0.5f) * chassisScaling,
            new Vector2( 1.5f,  0.0f) * chassisScaling,
            new Vector2( 0.0f,  0.9f) * chassisScaling,
            new Vector2(-1.15f, 0.9f) * chassisScaling,
            new Vector2(-1.5f,  0.2f) * chassisScaling
        };

        // Chassis body.
        {
            var geometry = PolygonGeometry.Create(vertices.AsArray(), carScale * 0.15f);
            var shapeDef = new PhysicsShapeDefinition
            {
                density = 1f / carScale,
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.2f }
            };
            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                position = position + Vector2.up * carScale,
                gravityScale = gravityScale
            };
            result.chassis = world.CreateBody(bodyDef);
            result.chassis.CreateShape(geometry, shapeDef);
        }

        // Wheels.
        {
            var geometry = new CircleGeometry { radius = carScale * 0.4f };
            var shapeDef = new PhysicsShapeDefinition
            {
                density = 2f / carScale,
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 1.5f, rollingResistance = 0.1f }
            };
            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                position = position + new Vector2(-1f * carScale, 0.35f * carScale),
                gravityScale = gravityScale,
                fastRotationAllowed = true
            };

            result.rearWheel = world.CreateBody(bodyDef);
            result.rearWheel.CreateShape(geometry, shapeDef);

            bodyDef.position = position + new Vector2(1f * carScale, 0.4f * carScale);
            result.frontWheel = world.CreateBody(bodyDef);
            result.frontWheel.CreateShape(geometry, shapeDef);
        }

        // Wheel joints — vertical-axis joint with motor + spring suspension + travel limit.
        var axleRotation = PhysicsRotate.FromRadians(PhysicsMath.PI * 0.5f);

        {
            var pivot = result.rearWheel.position;
            result.rearWheelJoint = world.CreateJoint(new PhysicsWheelJointDefinition
            {
                bodyA = result.chassis,
                bodyB = result.rearWheel,
                localAnchorA = new PhysicsTransform(result.chassis.GetLocalPoint(pivot), axleRotation),
                localAnchorB = result.rearWheel.GetLocalPoint(pivot),
                enableMotor = true,
                motorSpeed = 0f,
                maxMotorTorque = maxMotorTorque,
                enableSpring = true,
                springFrequency = suspensionFrequency,
                springDamping = suspensionDamping,
                enableLimit = true,
                lowerTranslationLimit = -0.25f * carScale,
                upperTranslationLimit = 0.25f * carScale
            });
        }

        {
            var pivot = result.frontWheel.position;
            result.frontWheelJoint = world.CreateJoint(new PhysicsWheelJointDefinition
            {
                bodyA = result.chassis,
                bodyB = result.frontWheel,
                localAnchorA = new PhysicsTransform(result.chassis.GetLocalPoint(pivot), axleRotation),
                localAnchorB = result.frontWheel.GetLocalPoint(pivot),
                enableMotor = true,
                motorSpeed = 0f,
                maxMotorTorque = maxMotorTorque,
                enableSpring = true,
                springFrequency = suspensionFrequency,
                springDamping = suspensionDamping,
                enableLimit = true,
                lowerTranslationLimit = -0.25f * carScale,
                upperTranslationLimit = 0.25f * carScale
            });
        }

        return result;
    }
}

/// <summary>
/// Demo MonoBehaviour: builds one vehicle in OnEnable, destroys it in OnDisable, drives it via arrow keys.
/// </summary>
public class VehicleFactoryDemo : MonoBehaviour
{
    public Vector2 SpawnPosition = Vector2.zero;
    public float MotorSpeed = 2000f;

    private VehicleFactory.Vehicle m_Vehicle;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;
        m_Vehicle = VehicleFactory.Create(world, SpawnPosition);
    }

    private void OnDisable()
    {
        m_Vehicle.Destroy();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))  SetSpeed(MotorSpeed);
        else if (Input.GetKey(KeyCode.RightArrow)) SetSpeed(-MotorSpeed);
        else if (Input.GetKey(KeyCode.Space))      SetSpeed(0f);
    }

    private void SetSpeed(float speed)
    {
        if (!m_Vehicle.rearWheelJoint.isValid) return;
        m_Vehicle.rearWheelJoint.motorSpeed = speed;
        m_Vehicle.frontWheelJoint.motorSpeed = speed;
        m_Vehicle.rearWheelJoint.WakeBodies();
    }
}
