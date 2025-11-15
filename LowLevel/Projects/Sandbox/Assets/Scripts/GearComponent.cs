using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;

[ExecuteAlways]
[DefaultExecutionOrder(PhysicsLowLevelExtrasExecutionOrder.SceneBody)]
public class GearComponent : MonoBehaviour
{
    [Range(0.5f, 100f)] public float GearRadius = 3;
    [Range(0f, 1f)] public float GearFriction = 0.1f;
    public Color GearColor = Color.saddleBrown;
    public Color ToothColor = Color.gray;
    [Range(2, 64)] public int ToothCount = 16;
    [Range(0.1f, 10f)] public float ToothWidth = 0.5f;
    [Range(0.1f, 10f)] public float ToothHeight = 0.5f;
    [Range(0f, 10f)] public float ToothRadius = 0.1f;
    public bool EnableGearMotor = false;
    public float MotorSpeed = 0f;
    [Min(0f)] public float MaxMotorTorque = 1000000f;
    public PhysicsShape.ContactFilter GearContactFilter = PhysicsShape.ContactFilter.defaultFilter;

    public SceneWorld SceneWorld;

    private PhysicsWorld m_PhysicsWorld;
    private PhysicsBody m_GearBody;
    private PhysicsBody m_GroundBody;
    private int m_GearBodyOwnerKey;
    private int m_GroundBodyOwnerKey;

    private void OnEnable() => CreateGear();
    private void OnDestroy() => DestroyGear();

    private void OnValidate()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        CreateGear();
    }

    private void Reset()
    {
        SceneWorld = SceneWorld.FindSceneWorld(gameObject);
    }

    private void Update()
    {
        if (!Application.isPlaying &&
            isActiveAndEnabled &&
            transform.hasChanged)
        {
            transform.hasChanged = false;
            CreateGear();
        }
    }

    private void CreateGear()
    {
        // Destroy any existing gear.
        DestroyGear();

        if (SceneWorld == null)
            Reset();

        m_PhysicsWorld = SceneWorld == null ? PhysicsWorld.defaultWorld : SceneWorld.World;

        // Finish if no valid world.
        if (!m_PhysicsWorld.isValid)
            return;

        // Calculate gear position.
        var gearPosition = PhysicsMath.ToPosition2D(transform.position, m_PhysicsWorld.transformPlane);

        // Create the gear body.
        m_GearBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = gearPosition
        });
        m_GearBodyOwnerKey = m_GearBody.SetOwner(this);

        // Create the gear shape.
        var shapeDef = new PhysicsShapeDefinition { contactFilter = GearContactFilter, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = GearFriction, customColor = GearColor } };
        var circle = new CircleGeometry { radius = GearRadius };
        var gearShape = m_GearBody.CreateShape(circle, shapeDef);
        gearShape.SetOwner(this);

        // Create the gear teeth.
        var toothAngle = new PhysicsRotate(PhysicsMath.TAU / ToothCount);
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
            center = toothRotation.RotateVector(new Vector2(GearRadius + ToothHeight * 0.5f, 0.0f));
        }

        // Create the gear motor.
        m_GroundBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Static, position = gearPosition });
        m_GroundBodyOwnerKey = m_GroundBody.SetOwner(this);
        var joint = m_PhysicsWorld.CreateJoint(new PhysicsHingeJointDefinition
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
        if (m_GearBody.isValid)
            m_GearBody.Destroy(m_GearBodyOwnerKey);

        if (m_GroundBody.isValid)
            m_GroundBody.Destroy(m_GroundBodyOwnerKey);
    }
}