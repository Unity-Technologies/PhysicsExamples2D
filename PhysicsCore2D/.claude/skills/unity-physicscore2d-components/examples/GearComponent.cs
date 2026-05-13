using UnityEngine;
using Unity.U2D.Physics;

public class GearComponent : MonoBehaviour, PhysicsCallbacks.ITransformChangedCallback
{
    [Header("Gear")]
    [Range(0.5f, 5f)] public float GearRadius = 2f;
    [Range(0f, 1f)] public float GearFriction = 0.1f;

    [Header("Teeth")]
    [Range(1, 64)] public int ToothCount = 18;
    [Range(0.05f, 3f)] public float ToothWidth = 0.15f;
    [Range(0.05f, 3f)] public float ToothHeight = 0.5f;
    [Range(0f, 0.5f)] public float ToothRadius = 0.1f;

    [Header("Dynamics")]
    public PhysicsBody.TransformWriteMode TransformWrite = PhysicsBody.TransformWriteMode.Off;
    public bool EnableGearMotor = true;
    public float MotorSpeed = 45f;
    [Min(0f)] public float MaxMotorTorque = 1000000f;
    public PhysicsShape.ContactFilter GearContactFilter = PhysicsShape.ContactFilter.defaultFilter;

    [Header("Visual")]
    public Color GearColor = Color.gray;
    public Color ToothColor = Color.saddleBrown;

    private PhysicsBody m_GearBody;
    private PhysicsBody m_GroundBody;
    private int m_GearBodyOwnerKey;
    private int m_GroundBodyOwnerKey;

    private void OnEnable()
    {
        CreateGear();

        // Register for transform changes so we can rebuild the gear when the GameObject's transform moves.
        PhysicsWorld.RegisterTransformChange(transform, this);
    }

    private void OnDisable()
    {
        DestroyGear();
        PhysicsWorld.UnregisterTransformChange(transform, this);
    }

    private void CreateGear()
    {
        DestroyGear();

        var physicsWorld = PhysicsWorld.defaultWorld;

        // Convert the Unity Transform to a physics transform respecting the world's transform plane.
        var gearTransform = PhysicsMath.ToPhysicsTransform(transform, physicsWorld.transformPlane);

        m_GearBody = physicsWorld.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            gravityScale = 0f,
            position = gearTransform.position,
            rotation = gearTransform.rotation,
            transformWriteMode = TransformWrite
        });

        if (TransformWrite != PhysicsBody.TransformWriteMode.Off)
            m_GearBody.transformObject = transform;

        // Owner key pattern: only this component can destroy the body.
        m_GearBodyOwnerKey = m_GearBody.SetOwner(this);

        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = GearContactFilter,
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = GearFriction, customColor = GearColor }
        };
        var circle = new CircleGeometry { radius = GearRadius };
        var gearShape = m_GearBody.CreateShape(circle, shapeDef);
        gearShape.SetOwner(this);

        // Create the gear teeth as a ring of rounded boxes around the gear body.
        var toothAngle = PhysicsRotate.FromRadians(PhysicsMath.TAU / ToothCount);
        var center = new Vector2(GearRadius + ToothHeight * 0.5f, 0f);
        var toothRotation = PhysicsRotate.identity;
        shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = ToothColor };
        for (var i = 0; i < ToothCount; ++i)
        {
            var tooth = PolygonGeometry.CreateBox(
                size: new Vector2(ToothHeight, ToothWidth),
                radius: ToothRadius,
                transform: new PhysicsTransform(center, toothRotation));

            var toothShape = m_GearBody.CreateShape(tooth, shapeDef);
            toothShape.SetOwner(this);

            toothRotation = toothAngle.MultiplyRotation(toothRotation);
            center = toothRotation.RotateVector(new Vector2(GearRadius + ToothHeight * 0.5f, 0f));
        }

        // Static body anchors the motor.
        m_GroundBody = physicsWorld.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Static,
            position = gearTransform.position,
            rotation = gearTransform.rotation
        });
        m_GroundBodyOwnerKey = m_GroundBody.SetOwner(this);

        var joint = physicsWorld.CreateJoint(new PhysicsHingeJointDefinition
        {
            bodyA = m_GroundBody,
            bodyB = m_GearBody,
            enableMotor = EnableGearMotor,
            motorSpeed = MotorSpeed,
            maxMotorTorque = MaxMotorTorque
        });
        joint.SetOwner(this);
    }

    private void DestroyGear()
    {
        // Destroy with owner key — only the owner can destroy.
        if (m_GearBody.isValid)
            m_GearBody.Destroy(m_GearBodyOwnerKey);

        if (m_GroundBody.isValid)
            m_GroundBody.Destroy(m_GroundBodyOwnerKey);
    }

    public void OnTransformChanged(PhysicsEvents.TransformChangeEvent transformChangeEvent)
    {
        if (!m_GearBody.isValid)
            return;

        var physicsWorld = PhysicsWorld.defaultWorld;
        var gearTransform = PhysicsMath.ToPhysicsTransform(transform, physicsWorld.transformPlane);

        m_GearBody.transform = gearTransform;
        m_GroundBody.transform = gearTransform;
    }
}
