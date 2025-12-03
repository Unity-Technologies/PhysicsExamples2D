using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Unity.U2D.Physics.Extras;

[ExecuteAlways]
public class GearComponent : MonoBehaviour, IWorldSceneTransformChanged
{
    [Header("Gear")]
    [Range(0.5f, 5f)] public float GearRadius = 2f;
    [Range(0f, 1f)] public float GearFriction = 0.1f;
    
    [Header("Teeth")]
    [Range(1, 64)] public int ToothCount = 18;
    [Range(0.05f, 3f)]  public float ToothWidth = 0.15f;
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
    
    // Here we store the physics components we used.
    private PhysicsBody m_GearBody;
    private PhysicsBody m_GroundBody;
    private int m_GearBodyOwnerKey;
    private int m_GroundBodyOwnerKey;

    // Component is enabled.
    private void OnEnable()
    {
        // Create the gear.
        CreateGear();
        
#if UNITY_EDITOR
        // Monitor transform changes.
        WorldSceneTransformMonitor.AddMonitor(this);
#endif
    }

    // Component is disabled.
    private void OnDisable()
    {
        // Destroy the gear.
        DestroyGear();

#if UNITY_EDITOR
        // Stop monitoring transform changes.
        WorldSceneTransformMonitor.RemoveMonitor(this);
#endif
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying || !isActiveAndEnabled)
            return;

        // Recreate the gear.
        CreateGear();
    }
    
    private void CreateGear()
    {
        // Destroy any existing gear.
        DestroyGear();

        // Fetch the default world.
        var physicsWorld = PhysicsWorld.defaultWorld;

        // Calculate a physics-transform using the Unity Transform. 
        // NOTE: Whilst optional, we transform the position and rotation using the current world transform plane.
        // This is all we need to do to get transform plane support.
        var gearTransform = PhysicsMath.ToPhysicsTransform(transform, physicsWorld.transformPlane);
        
        // Create the gear body.
        m_GearBody = physicsWorld.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            gravityScale = 0f,
            position = gearTransform.position,
            rotation = gearTransform.rotation,
            transformWriteMode = TransformWrite
        });
        
        // Set the transform object to write to if we're writing.
        if (TransformWrite != PhysicsBody.TransformWriteMode.Off)
            m_GearBody.transformObject = transform;
        
        // We don't want others to be able to destroy this gear.
        // NOTE: Whilst this doesn't stop external parties modifying the body, it's critical we control its lifetime.
        m_GearBodyOwnerKey = m_GearBody.SetOwner(this);
        
        // Create the gear shape.
        var shapeDef = new PhysicsShapeDefinition { contactFilter = GearContactFilter, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = GearFriction, customColor = GearColor } };
        var circle = new CircleGeometry { radius = GearRadius };
        var gearShape = m_GearBody.CreateShape(circle, shapeDef);
        
        // We also want to own the shape.
        gearShape.SetOwner(this);
        
        // Create the gear teeth.
        var toothAngle = new PhysicsRotate(PhysicsMath.TAU / ToothCount);
        var center = new Vector2(GearRadius + ToothHeight * 0.5f, 0f);
        var toothRotation = PhysicsRotate.identity;
        shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = ToothColor };
        for (var i = 0; i < ToothCount; ++i)
        {
            var tooth = PolygonGeometry.CreateBox(
                size: new Vector2(ToothHeight, ToothWidth) ,
                radius: ToothRadius,
                transform: new PhysicsTransform(center, toothRotation));

            var toothShape = m_GearBody.CreateShape(tooth, shapeDef);
            toothShape.SetOwner(this);

            toothRotation = toothAngle.MultiplyRotation(toothRotation);
            center = toothRotation.RotateVector(new Vector2(GearRadius + ToothHeight * 0.5f, 0.0f));
        }
        
        // Create a ground body for the gear motor.
        m_GroundBody = physicsWorld.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Static,
            position = gearTransform.position,
            rotation = gearTransform.rotation
        });
        
        // We also want to own the gear ground body.
        m_GroundBodyOwnerKey = m_GroundBody.SetOwner(this);
        
        // Create the gear motor.
        var joint = physicsWorld.CreateJoint(new PhysicsHingeJointDefinition
        {
            bodyA = m_GroundBody,
            bodyB = m_GearBody,
            enableMotor = EnableGearMotor,
            motorSpeed = MotorSpeed,
            maxMotorTorque = MaxMotorTorque
        });
        
        // We also want to own the gear motor.
        joint.SetOwner(this);
    }

    private void DestroyGear()
    {
        // Destroy the gear body if it's valid.
        // NOTE: This can only be done with the owner key.
        if (m_GearBody.isValid)
            m_GearBody.Destroy(m_GearBodyOwnerKey);
        
        // Destroy the gear ground body if it's valid.
        // NOTE: This can only be done with the owner key.
        if (m_GroundBody.isValid)
            m_GroundBody.Destroy(m_GroundBodyOwnerKey);
    }

    // If there's a transform change then we recreate the gear.
    void IWorldSceneTransformChanged.TransformChanged()
    {
        // Recreate gear.
        if (!m_GearBody.isValid)
            return;
        
        // Fetch the default world.
        var physicsWorld = PhysicsWorld.defaultWorld;

        // Calculate the new gear transform.
        var gearTransform = PhysicsMath.ToPhysicsTransform(transform, physicsWorld.transformPlane);

        // Update the body transform.
        m_GearBody.transform = gearTransform;
        m_GroundBody.transform = gearTransform;
    }
}
