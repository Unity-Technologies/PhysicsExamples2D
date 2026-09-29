using System;

using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Moves a capsule character through a course of steps, a hanging bridge, loose debris and a lift, using the world's mover cast rather than a physics body.
/// The character is script only: each step it is moved by a shape cast that slides it along whatever it hits, held off the ground by a spring-damper driven from a downward cast, or pogo.
/// </summary>
/// <remarks>
/// The course is made of components: two Physics Area Path grounds, a bridge of hinged planks, debris boxes and a moving lift, each one a Physics Pose with a Physics Area.
/// The planks and debris are spawned from prefabs when the example loads, and the lift is a kinematic body that this script moves along a sine wave every step.
/// The character shares no contact category with other movers, so it slides along the ground, the planks, the debris and the lift, and pushes the dynamic ones.
/// </remarks>
public sealed class CharacterMoverContents : MonoBehaviour
{
    /// <summary>
    /// The shape of the cast that finds the ground under the character.
    /// </summary>
    public enum PogoType
    {
        Point = 0,
        Circle = 1,
        Segment = 2
    }

    /// <summary>
    /// Gives the character the buttons that steer it, in addition to the keyboard.
    /// </summary>
    /// <param name="leftButton">The button held to move left.</param>
    /// <param name="rightButton">The button held to move right.</param>
    /// <param name="jumpButton">The button held to jump.</param>
    public void SetButtons(ControlsMenu.CustomButton leftButton, ControlsMenu.CustomButton rightButton, ControlsMenu.CustomButton jumpButton)
    {
        m_LeftButton = leftButton;
        m_RightButton = rightButton;
        m_JumpButton = jumpButton;
    }

    /// <summary>
    /// How fast the character leaves the ground when it jumps, in meters per second.
    /// </summary>
    public float jumpSpeed
    {
        get => m_JumpSpeed;
        set => m_JumpSpeed = value;
    }

    /// <summary>
    /// The speed below which the character stops dead, in meters per second.
    /// </summary>
    public float minSpeed
    {
        get => m_MinSpeed;
        set => m_MinSpeed = value;
    }

    /// <summary>
    /// The fastest the character walks, in meters per second.
    /// </summary>
    public float maxSpeed
    {
        get => m_MaxSpeed;
        set => m_MaxSpeed = value;
    }

    /// <summary>
    /// The speed below which ground friction slows the character by a fixed amount rather than in proportion to its speed, in meters per second.
    /// </summary>
    public float stopSpeed
    {
        get => m_StopSpeed;
        set => m_StopSpeed = value;
    }

    /// <summary>
    /// How quickly the character reaches its top speed when it steers.
    /// </summary>
    public float accelerate
    {
        get => m_Accelerate;
        set => m_Accelerate = value;
    }

    /// <summary>
    /// How much of its steering the character keeps in the air, from zero for none to one for all of it.
    /// </summary>
    public float airSteer
    {
        get => m_AirSteer;
        set => m_AirSteer = value;
    }

    /// <summary>
    /// How quickly the ground slows the character when it is not steering, in units of one over seconds.
    /// </summary>
    public float friction
    {
        get => m_Friction;
        set => m_Friction = value;
    }

    /// <summary>
    /// How hard gravity pulls the character down, in meters per second squared.
    /// </summary>
    public float gravity
    {
        get => m_Gravity;
        set => m_Gravity = value;
    }

    /// <summary>
    /// How long the pogo is at rest, as a multiple of the character's radius.
    /// </summary>
    public float pogoScale
    {
        get => m_PogoScale;
        set => m_PogoScale = value;
    }

    /// <summary>
    /// How stiff the pogo spring is, in cycles per second.
    /// </summary>
    public float pogoFrequency
    {
        get => m_PogoFrequency;
        set => m_PogoFrequency = value;
    }

    /// <summary>
    /// How quickly the pogo spring stops bouncing, where zero never settles.
    /// </summary>
    public float pogoDamping
    {
        get => m_PogoDamping;
        set => m_PogoDamping = value;
    }

    /// <summary>
    /// The shape of the cast that finds the ground under the character.
    /// </summary>
    public PogoType pogoType
    {
        get => m_PogoType;
        set => m_PogoType = value;
    }

    private void OnEnable()
    {
        if (m_CameraManipulator != null)
            m_CameraManipulator.DisableManipulators = true;

        m_Transform = new PhysicsTransform(new Vector2(2f, 8f));
        m_Geometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.5f), center2 = new Vector2(0f, 0.5f), radius = 0.3f };
        m_JumpReleased = true;

        PhysicsEvents.PreSimulate += CharacterMove;
    }

    // The camera belongs outside this scene, so it would otherwise stay locked into whichever example is loaded next.
    private void OnDisable()
    {
        PhysicsEvents.PreSimulate -= CharacterMove;

        if (m_CameraManipulator != null)
            m_CameraManipulator.DisableManipulators = false;
    }

    private void Start()
    {
        m_Random = new Random(RandomSeed);

        SpawnBridge();
        SpawnDebris();
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        world.DrawGeometry(m_Geometry, m_Transform, m_OnGround ? Color.orange : Color.aquamarine);
        world.DrawLine(m_Transform.position, m_Transform.position + m_Velocity.normalized * VelocityDrawScale, RandomColor());
        DrawPogo(world, m_PogoDraw);

        if (world.paused)
            return;

        var currentKeyboard = Keyboard.current;
        m_LeftPressed = IsPressed(m_LeftButton) || (currentKeyboard != null && currentKeyboard.leftArrowKey.isPressed);
        m_RightPressed = IsPressed(m_RightButton) || (currentKeyboard != null && currentKeyboard.rightArrowKey.isPressed);
        m_JumpPressed = IsPressed(m_JumpButton) || (currentKeyboard != null && currentKeyboard.spaceKey.isPressed);
    }

    // Hangs a bridge of hinged planks between the two grounds, each one held to the one before it by a spring and motor.
    private void SpawnBridge()
    {
        if (m_PlankPrefab == null || m_GroundLeft == null || m_GroundRight == null)
            return;

        var previous = m_GroundLeft;

        for (var n = 0; n < PlankCount; ++n)
        {
            var plank = Instantiate(m_PlankPrefab, new Vector3(BridgeOffset.x + 0.5f + n, BridgeOffset.y, 0f), Quaternion.identity);
            var pose = plank.GetComponent<PhysicsPose>();

            // The hinge sits half a plank behind the plank's center, which on the first plank is a point on the left ground.
            var pivot = new Vector2(BridgeOffset.x + n, BridgeOffset.y);
            AddHinge(plank, previous, pose, previous.transform.InverseTransformPoint(pivot), new Vector2(-0.5f, 0f));

            plank.SetActive(true);
            previous = pose;
        }

        // The last plank is hinged to the right ground, by a hinge on an object of its own that is left inactive until it is set up, since a constraint is created as soon as it is enabled.
        var lastPivot = new Vector2(BridgeOffset.x + PlankCount, BridgeOffset.y);
        var endHinge = new GameObject("Bridge End");
        endHinge.SetActive(false);
        endHinge.transform.SetParent(transform);

        AddHinge(endHinge, previous, m_GroundRight, new Vector2(0.5f, 0f), m_GroundRight.transform.InverseTransformPoint(lastPivot));
        endHinge.SetActive(true);
    }

    // Adds a spring-and-motor hinge between two poses, with the anchors given in each pose's own space.
    private static void AddHinge(GameObject owner, PhysicsPose poseA, PhysicsPose poseB, Vector2 anchorA, Vector2 anchorB)
    {
        var hinge = owner.AddComponent<PhysicsConstraintHinge>();
        hinge.source = PhysicsConstraint.PoseSource.Custom;
        hinge.poseA = poseA;
        hinge.poseB = poseB;

        var definition = hinge.definition;
        definition.autoAnchorA = false;
        definition.autoAnchorB = false;
        definition.localAnchorA = new PhysicsTransform(anchorA, PhysicsRotate.identity);
        definition.localAnchorB = new PhysicsTransform(anchorB, PhysicsRotate.identity);
        definition.enableMotor = true;
        definition.maxMotorTorque = PlankMotorTorque;
        definition.enableSpring = true;
        definition.springFrequency = PlankSpringFrequency;
        definition.springDamping = PlankSpringDamping;
        hinge.definition = definition;
    }

    // Scatters tall thin boxes of random size and spin across the course, for the character to push through.
    private void SpawnDebris()
    {
        if (m_DebrisPrefab == null)
            return;

        for (var n = -70f; n < 75f; n += 2f)
        {
            var position = new Vector3(75f + n, 12f + m_Random.NextFloat(0f, 6f), 0f);
            var rotation = Quaternion.Euler(0f, 0f, m_Random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI) * Mathf.Rad2Deg);
            var angularVelocity = m_Random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI);
            var size = new Vector2(m_Random.NextFloat(0.1f, 0.5f), m_Random.NextFloat(0.5f, 1.5f));

            var debris = Instantiate(m_DebrisPrefab, position, rotation);

            var pose = debris.GetComponent<PhysicsPose>();
            var bodyDefinition = pose.definition;
            bodyDefinition.angularVelocity = angularVelocity;
            pose.definition = bodyDefinition;

            debris.GetComponent<PhysicsAreaPolygon>().geometry = PolygonGeometry.CreateBox(size);
            debris.SetActive(true);
        }
    }

    // Moves the lift, then steers and moves the character, once every simulation step.
    private void CharacterMove(PhysicsWorld world, float deltaTime)
    {
        if (world.paused || !world.isDefaultWorld)
            return;

        if (m_Elevator != null && m_Elevator.body.isValid)
        {
            var target = new PhysicsTransform(new Vector2(ElevatorOffset.x, ElevatorAmplitude * Mathf.Cos(m_Time + PhysicsMath.PI) + ElevatorOffset.y));
            m_Elevator.body.SetTransformTarget(target, deltaTime);
        }

        m_Time += deltaTime;

        var throttle = 0f;

        if (m_LeftPressed)
            throttle -= 1f;

        if (m_RightPressed)
            throttle += 1f;

        if (m_JumpPressed)
        {
            if (m_OnGround && m_JumpReleased)
            {
                m_Velocity.y = m_JumpSpeed;
                m_OnGround = false;
                m_JumpReleased = false;
            }
        }
        else
        {
            m_JumpReleased = true;
        }

        SolveMove(world, deltaTime, throttle);

        if (m_CameraManipulator != null)
            m_CameraManipulator.CameraPosition = new Vector2(m_Transform.position.x, m_CameraManipulator.CameraPosition.y);
    }

    // Applies friction, steering, gravity and the pogo to the character's velocity, then moves it with the mover cast.
    // Reference: https://github.com/id-Software/Quake/blob/master/QW/client/pmove.c#L390
    private void SolveMove(PhysicsWorld world, float deltaTime, float throttle)
    {
        // Friction slows the character in proportion to its speed above the stop speed, and by a fixed amount below it.
        var speed = m_Velocity.magnitude;
        if (speed < m_MinSpeed)
        {
            m_Velocity = Vector2.zero;
        }
        else if (m_OnGround)
        {
            var control = speed < m_StopSpeed ? m_StopSpeed : speed;
            var drop = control * m_Friction * deltaTime;
            var newSpeed = Mathf.Max(0f, speed - drop);
            m_Velocity *= newSpeed / speed;
        }

        // On the ground there is no vertical velocity to keep.
        if (m_OnGround)
            m_Velocity.y = 0f;

        // Steering accelerates the character toward its top speed, and does so less in the air.
        var desiredVelocity = new Vector2(m_MaxSpeed * throttle, 0f);
        var desiredSpeed = Mathf.Min(desiredVelocity.magnitude, m_MaxSpeed);
        var desiredDirection = desiredVelocity.normalized;

        var currentSpeed = Vector2.Dot(m_Velocity, desiredDirection);
        var addSpeed = desiredSpeed - currentSpeed;
        if (addSpeed > 0f)
        {
            var steer = m_OnGround ? 1f : m_AirSteer;
            var accelSpeed = Mathf.Min(steer * m_Accelerate * m_MaxSpeed * deltaTime, addSpeed);

            m_Velocity += accelSpeed * desiredDirection;
        }

        m_Velocity.y -= m_Gravity * deltaTime;

        // The pogo is a cast straight down from the bottom of the capsule, in the chosen shape, that finds the ground and springs the character off it.
        var pogoRestLength = m_PogoScale * m_Geometry.radius;
        var rayLength = pogoRestLength + m_Geometry.radius;
        var origin = m_Transform.TransformPoint(m_Geometry.center1);
        var circle = new CircleGeometry { center = origin, radius = 0.5f * m_Geometry.radius };
        var segmentOffset = new Vector2(0.75f * m_Geometry.radius, 0f);
        var segment = new SegmentGeometry { point1 = origin - segmentOffset, point2 = origin + segmentOffset };

        var pogoFilter = new PhysicsQuery.QueryFilter
        {
            categories = MoverBit,
            hitCategories = StaticBit | DynamicBit
        };

        PhysicsShape.ShapeProxy shapeProxy;
        Vector2 translation;

        switch (m_PogoType)
        {
            case PogoType.Point:
            {
                shapeProxy = new PhysicsShape.ShapeProxy
                {
                    vertices = new PhysicsShape.ShapeArray { vertex0 = origin },
                    count = 1,
                    radius = 0
                };

                translation = new Vector2(0f, -rayLength);
                break;
            }

            case PogoType.Circle:
            {
                shapeProxy = new PhysicsShape.ShapeProxy(circle);
                translation = new Vector2(0f, -rayLength + circle.radius);
                break;
            }

            case PogoType.Segment:
            {
                shapeProxy = new PhysicsShape.ShapeProxy(segment);
                translation = new Vector2(0f, -rayLength);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }

        using var hit = world.CastShapeProxy(shapeProxy, translation, pogoFilter);
        var castResult = hit.Length > 0 ? hit[0] : default;

        // The character is grounded while the pogo finds ground, but does not snap back to it while it is still rising.
        if (m_OnGround)
            m_OnGround = castResult.isValid;
        else
            m_OnGround = castResult.isValid && m_Velocity.y <= 0.01f;

        if (castResult.isValid)
        {
            // The pogo spring pushes the character away from the ground by how far the pogo is from its rest length, and presses down on what it stands on.
            var pogoCurrentLength = castResult.fraction * rayLength;
            var offset = pogoCurrentLength - pogoRestLength;
            m_PogoVelocity = PhysicsMath.SpringDamper(frequency: m_PogoFrequency, damping: m_PogoDamping, translation: offset, speed: m_PogoVelocity, deltaTime: deltaTime);

            m_PogoDraw = new PogoDraw
            {
                origin = origin,
                deltaTranslation = castResult.fraction * translation,
                circle = circle,
                segment = segment,
                pogoColor = Color.plum,
                isValid = true
            };

            castResult.shape.body.ApplyForce(Vector2.down * 50f, castResult.point);
        }
        else
        {
            m_PogoVelocity = 0f;

            m_PogoDraw = new PogoDraw
            {
                origin = origin,
                deltaTranslation = translation,
                circle = circle,
                segment = segment,
                pogoColor = Color.gray,
                isValid = true
            };
        }

        // The mover cast slides the character to where it wants to go, and gives back where it ended up and what velocity it kept.
        var targetPosition = m_Transform.position + deltaTime * m_Velocity + deltaTime * m_PogoVelocity * Vector2.up;

        var worldMoverInput = new PhysicsQuery.WorldMoverInput
        {
            geometry = m_Geometry,
            maxIterations = MaxSolverIterations,
            overlapFilter = new PhysicsQuery.QueryFilter
            {
                categories = MoverBit,
                hitCategories = StaticBit | DynamicBit | MoverBit
            },

            // Movers do not sweep against other movers, which allows for soft collision.
            castFilter = new PhysicsQuery.QueryFilter
            {
                categories = MoverBit,
                hitCategories = StaticBit | DynamicBit
            },

            moveTolerance = 0.01f,
            targetPosition = targetPosition,
            transform = m_Transform,
            velocity = m_Velocity
        };

        var moverResult = world.CastMover(worldMoverInput);

        m_Transform = moverResult.transform;
        m_Velocity = moverResult.velocity;
    }

    // Draws the pogo's cast line and the shape it casts.
    private void DrawPogo(PhysicsWorld world, PogoDraw pogoDraw)
    {
        if (!pogoDraw.isValid)
            return;

        world.DrawLine(pogoDraw.origin, pogoDraw.origin + pogoDraw.deltaTranslation, Color.gray);

        switch (m_PogoType)
        {
            case PogoType.Point:
                world.DrawPoint(pogoDraw.origin + pogoDraw.deltaTranslation, 10f, pogoDraw.pogoColor);
                return;

            case PogoType.Circle:
                world.DrawCircle(pogoDraw.origin + pogoDraw.deltaTranslation, pogoDraw.circle.radius, pogoDraw.pogoColor);
                return;

            case PogoType.Segment:
                world.DrawLine(pogoDraw.segment.point1 + pogoDraw.deltaTranslation, pogoDraw.segment.point2 + pogoDraw.deltaTranslation, pogoDraw.pogoColor);
                return;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Returns a new random bright color, so the velocity line changes color every frame.
    private Color RandomColor() => Color.HSVToRGB(m_Random.NextFloat(0f, 1f), m_Random.NextFloat(0.7f, 1f) * SaturationScale, m_Random.NextFloat(0.5f, 1f));

    // Returns whether a button is held, treating a missing button as not held.
    private static bool IsPressed(ControlsMenu.CustomButton button) => button != null && button.isPressed;

    #region Internal

    // What the pogo drew this step, kept so it can be drawn again every frame.
    struct PogoDraw
    {
        public Vector2 origin;
        public Vector2 deltaTranslation;
        public CircleGeometry circle;
        public SegmentGeometry segment;
        public Color pogoColor;
        public bool isValid;
    }

    // The contact categories that tell the ground, the character and the loose bodies apart.
    static readonly PhysicsMask StaticBit = new(0);
    static readonly PhysicsMask MoverBit = new(1);
    static readonly PhysicsMask DynamicBit = new(2);

    // The most times the mover cast slides the character along what it hits in one step, and how long the drawn velocity line is.
    const int MaxSolverIterations = 5;
    const float VelocityDrawScale = 2f;

    // Where the bridge starts, how many planks it has, and the hinge spring and motor every plank uses.
    static readonly Vector2 BridgeOffset = new(48.7f, 9.2f);
    const int PlankCount = 50;
    const float PlankMotorTorque = 10f;
    const float PlankSpringFrequency = 3f;
    const float PlankSpringDamping = 0.8f;

    // The middle of the lift's travel and how far it rises and falls from it.
    static readonly Vector2 ElevatorOffset = new(112f, 10f);
    const float ElevatorAmplitude = 4f;

    // The seed the debris sizes, positions and spins start from, and how much the velocity line's random colors are washed out.
    const uint RandomSeed = 0x9E3779B9;
    const float SaturationScale = 0.65f;

    [SerializeField] CameraManipulator m_CameraManipulator;
    [SerializeField] PhysicsPose m_GroundLeft;
    [SerializeField] PhysicsPose m_GroundRight;
    [SerializeField] PhysicsPose m_Elevator;
    [SerializeField] GameObject m_PlankPrefab;
    [SerializeField] GameObject m_DebrisPrefab;

    float m_JumpSpeed = 15f;
    float m_MinSpeed = 0.1f;
    float m_MaxSpeed = 8f;
    float m_StopSpeed = 3f;
    float m_Accelerate = 20f;
    float m_AirSteer = 0.2f;
    float m_Friction = 8f;
    float m_Gravity = 50f;
    float m_PogoScale = 3f;
    float m_PogoFrequency = 5f;
    float m_PogoDamping = 0.8f;
    PogoType m_PogoType = PogoType.Circle;

    PhysicsTransform m_Transform;
    CapsuleGeometry m_Geometry;
    Vector2 m_Velocity;
    PogoDraw m_PogoDraw;
    Random m_Random;

    ControlsMenu.CustomButton m_LeftButton;
    ControlsMenu.CustomButton m_RightButton;
    ControlsMenu.CustomButton m_JumpButton;

    float m_Time;
    float m_PogoVelocity;
    bool m_OnGround;
    bool m_JumpReleased;
    bool m_LeftPressed;
    bool m_RightPressed;
    bool m_JumpPressed;

    #endregion
}
