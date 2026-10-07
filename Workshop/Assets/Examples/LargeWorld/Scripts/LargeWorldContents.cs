using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Builds a rolling terrain forty-eight kilometers wide, far out from the origin, with a car to drive along it and a batch of debris dropped ahead of the camera every two seconds, to check that the simulation stays stable a very long way from the origin.
/// The car is a Physics Pose chassis with two wheel Physics Poses held by Physics Constraint Wheels, the walls at each end are Physics Area Segments, and the debris comes from prefabs, but the terrain is far too large to be objects, so this script creates its shapes with the physics API.
/// </summary>
/// <remarks>
/// The terrain is a column of small boxes for every meter of its width, with a new static body every ten columns so that every shape stays close to its body's origin, which keeps contacts stable a long way from the world's origin.
/// The debris is a batch of boxes, then a batch of ragdolls, then a batch of soft rings, repeating along the terrain until the whole world has been dropped on.
/// The camera follows the car, or pans along the terrain at a chosen speed, and the terrain takes a moment to build when the example loads.
/// </remarks>
public sealed class LargeWorldContents : MonoBehaviour
{
    /// <summary>
    /// Gives the car the buttons that steer it, in addition to the keyboard.
    /// </summary>
    /// <param name="reverseButton">The button held to reverse.</param>
    /// <param name="forwardButton">The button held to drive forward.</param>
    /// <param name="brakeButton">The button held to brake.</param>
    public void SetButtons(ControlsMenu.CustomButton reverseButton, ControlsMenu.CustomButton forwardButton, ControlsMenu.CustomButton brakeButton)
    {
        m_ReverseButton = reverseButton;
        m_ForwardButton = forwardButton;
        m_BrakeButton = brakeButton;
    }

    /// <summary>
    /// Whether the camera follows the car along the terrain.
    /// Turning this off leaves the camera where the car was, to be moved by the camera pan speed.
    /// </summary>
    public bool followCar
    {
        get => m_FollowCar;
        set
        {
            m_FollowCar = value;

            if (!m_FollowCar && m_Chassis != null && m_Chassis.body.isValid)
                m_CameraPosition = new Vector2(m_Chassis.body.position.x, m_CameraPosition.y);
        }
    }

    /// <summary>
    /// How fast the camera pans along the terrain, in meters per second, where a negative speed pans it the other way.
    /// </summary>
    public float cameraPanSpeed
    {
        get => m_CameraPanSpeed;
        set => m_CameraPanSpeed = value;
    }

    /// <summary>
    /// Where the camera is along the terrain, in kilometers from the origin.
    /// </summary>
    public float worldPosition => m_CameraManipulator != null ? m_CameraManipulator.CameraPosition.x / 1000f : 0f;

    /// <summary>
    /// How wide the terrain is, in kilometers.
    /// </summary>
    public float worldSize => GridSize * GridCount / 1000f;

    private void OnEnable()
    {
        if (m_CameraManipulator != null)
            m_CameraManipulator.DisableManipulators = true;
    }

    // The camera belongs outside this scene, so it would otherwise stay locked into whichever example is loaded next.
    // The terrain's bodies belong to the world rather than to a component, so they are removed here or they would stay in whichever example is loaded next.
    private void OnDisable()
    {
        CancelInvoke(nameof(SpawnBatch));
        DestroyTerrain();

        if (m_CameraManipulator != null)
            m_CameraManipulator.DisableManipulators = false;
    }

    private void Start()
    {
        m_StartX = -0.5f * CycleCount * WavePeriod;

        m_CameraPosition = new Vector2(m_StartX, 15f);
        if (m_CameraManipulator != null)
            m_CameraManipulator.CameraPosition = m_CameraPosition;

        BuildTerrain();

        InvokeRepeating(nameof(SpawnBatch), 0f, 2f);
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;
        if (world.paused)
            return;

        var currentKeyboard = Keyboard.current;

        if (IsPressed(m_ReverseButton) || (currentKeyboard != null && currentKeyboard.leftArrowKey.isPressed))
            SetCarSpeed(ReverseSpeed);

        if (IsPressed(m_ForwardButton) || (currentKeyboard != null && currentKeyboard.rightArrowKey.isPressed))
            SetCarSpeed(ForwardSpeed);

        if (IsPressed(m_BrakeButton) || (currentKeyboard != null && currentKeyboard.spaceKey.isPressed))
            SetCarSpeed(0f);

        if (m_CameraManipulator == null)
            return;

        var cameraBound = 0.5f * WavePeriod * CycleCount;
        m_CameraPosition.x = Mathf.Clamp(m_CameraPosition.x + Time.deltaTime * m_CameraPanSpeed, -cameraBound, cameraBound);

        if (m_CameraPanSpeed != 0f)
            m_CameraManipulator.CameraPosition = m_CameraPosition;

        if (m_FollowCar && m_Chassis != null && m_Chassis.body.isValid)
            m_CameraManipulator.CameraPosition = new Vector2(m_Chassis.body.position.x, m_CameraManipulator.CameraPosition.y);
    }

    // Builds the terrain as a column of small boxes for every meter of its width, using the physics API since a Physics Pose and Physics Area for every one would be half a million objects.
    // Static shapes start no contacts of their own, which greatly reduces the cost of creating this many of them.
    private void BuildTerrain()
    {
        var world = PhysicsWorld.defaultWorld;

        var omega = PhysicsMath.TAU / WavePeriod;

        var bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        bodyDefinition.type = PhysicsBody.BodyType.Static;

        var shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        shapeDefinition.startStaticContacts = false;

        var bodyX = m_StartX;
        var shapeX = m_StartX;
        var groundBody = default(PhysicsBody);

        for (var i = 0; i < GridCount; ++i)
        {
            // A new body is created regularly so that its shapes are never far from its origin, as most of the physics works in coordinates relative to it.
            if (i % 10 == 0)
            {
                bodyDefinition.position = new Vector2(bodyX, bodyDefinition.position.y);
                groundBody = world.CreateBody(bodyDefinition);
                m_TerrainBodies.Add(groundBody);
                shapeX = 0f;
            }

            var y = 0f;
            var countY = Mathf.RoundToInt(TerrainHeight * Mathf.Cos(omega * bodyX)) + 12;

            for (var j = 0; j < countY; ++j)
            {
                var squareGeometry = PolygonGeometry.CreateBox(Vector2.one * 0.8f * GridSize, 0.1f, new Vector2(shapeX, y));
                groundBody.CreateShape(squareGeometry, shapeDefinition);

                y += GridSize;
            }

            bodyX += GridSize;
            shapeX += GridSize;
        }
    }

    // Removes every body of the terrain.
    private void DestroyTerrain()
    {
        foreach (var body in m_TerrainBodies)
        {
            if (body.isValid)
                body.Destroy();
        }

        m_TerrainBodies.Clear();
    }

    // Drops the next batch of debris ahead of the camera: boxes, then ragdolls, then soft rings, and stops once every cycle has been dropped.
    private void SpawnBatch()
    {
        if (m_CycleIndex >= CycleCount)
        {
            CancelInvoke(nameof(SpawnBatch));
            return;
        }

        var baseX = (0.5f + m_CycleIndex) * WavePeriod + m_StartX;

        switch (m_CycleIndex % 3)
        {
            case 0:
                SpawnBoxes(baseX);
                break;

            case 1:
                SpawnRagdolls(baseX);
                break;

            default:
                SpawnDonuts(baseX);
                break;
        }

        m_CycleIndex++;
    }

    // Drops ten stacks of five small boxes side by side.
    private void SpawnBoxes(float baseX)
    {
        if (m_BoxPrefab == null)
            return;

        var x = baseX - 3f;

        for (var i = 0; i < 10; ++i)
        {
            var y = 10f;

            for (var j = 0; j < 5; ++j)
            {
                Instantiate(m_BoxPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                y += 0.5f;
            }

            x += 0.6f;
        }
    }

    // Drops five ragdolls in a row.
    private void SpawnRagdolls(float baseX)
    {
        if (m_RagdollPrefab == null)
            return;

        var x = baseX - 2f;

        for (var i = 0; i < 5; ++i)
        {
            var spawned = Instantiate(m_RagdollPrefab, new Vector3(x, 10f, 0f), Quaternion.identity);
            SetRagdoll(spawned, m_RagdollIndex++);
            spawned.SetActive(true);

            x += 1f;
        }
    }

    // Scales a ragdoll to the size used here, loosens its joints so they have no spring and only a little friction, and gives it a group of its own so its limbs never push each other apart.
    // Scaling the root carries the bone placements and shapes, but a joint anchor is a body-local value the transform never reaches, so every anchor is scaled here to match.
    private static void SetRagdoll(GameObject spawned, int dollIndex)
    {
        spawned.transform.localScale = new Vector3(RagdollScale, RagdollScale, 1f);

        foreach (var hinge in spawned.GetComponentsInChildren<PhysicsConstraintHinge>(true))
        {
            var definition = hinge.definition;

            var anchorA = definition.localAnchorA;
            var anchorB = definition.localAnchorB;
            anchorA.position *= RagdollScale;
            anchorB.position *= RagdollScale;

            definition.localAnchorA = anchorA;
            definition.localAnchorB = anchorB;
            definition.springFrequency = 0f;
            definition.springDamping = 0f;

            // The prefab carries each joint's own share of a joint friction of 0.03, so this scales it up to the friction used here.
            definition.maxMotorTorque *= RagdollScale * RagdollJointFriction / PrefabJointFriction;

            hinge.definition = definition;
        }

        // The group is negative, which means shapes sharing it never touch, and unique per doll so dolls still collide with one another.
        var groupIndex = -(dollIndex + 1);

        foreach (var area in spawned.GetComponentsInChildren<PhysicsArea>(true))
        {
            var definition = area.definition;
            var contactFilter = definition.contactFilter;
            contactFilter.groupIndex = groupIndex;
            definition.contactFilter = contactFilter;
            area.definition = definition;
        }
    }

    // Drops five soft rings in a row.
    private void SpawnDonuts(float baseX)
    {
        if (m_DonutSegmentPrefab == null)
            return;

        var x = baseX - 4f;

        for (var i = 0; i < 5; ++i)
        {
            SpawnDonut(new Vector2(x, 12f));
            x += 2f;
        }
    }

    // Builds one soft ring as a ring of capsule segments, each held to its neighbors by a fixed constraint with a soft angular spring.
    private void SpawnDonut(Vector2 center)
    {
        // The ring is built inactive and switched on once, so every segment and every joint between them is created together.
        var ring = new GameObject("Donut");
        ring.SetActive(false);

        var radius = DonutScale;
        var deltaAngle = PhysicsMath.TAU / DonutSides;
        var length = PhysicsMath.TAU * radius / DonutSides;
        var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.5f * length), center2 = new Vector2(0f, 0.5f * length), radius = 0.25f * DonutScale };

        var segments = new PhysicsPose[DonutSides];
        var angle = DonutStartAngle;

        for (var i = 0; i < DonutSides; ++i)
        {
            var position = new Vector3(radius * Mathf.Cos(angle) + center.x, radius * Mathf.Sin(angle) + center.y, 0f);
            var segment = Instantiate(m_DonutSegmentPrefab, position, Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg), ring.transform);
            segment.SetActive(true);

            segment.GetComponent<PhysicsAreaCapsule>().geometry = capsuleGeometry;
            segments[i] = segment.GetComponent<PhysicsPose>();

            angle += deltaAngle;
        }

        // Each segment's top end is fixed to the next segment's bottom end, all the way round and back to the first.
        var previous = segments[DonutSides - 1];

        foreach (var segment in segments)
        {
            var joint = segment.gameObject.AddComponent<PhysicsConstraintFixed>();
            joint.source = PhysicsConstraint.PoseSource.Custom;
            joint.poseA = previous;
            joint.poseB = segment;

            var definition = joint.definition;
            definition.autoAnchorA = false;
            definition.autoAnchorB = false;
            definition.localAnchorA = new PhysicsTransform(new Vector2(0f, 0.5f * length), PhysicsRotate.identity);
            definition.localAnchorB = new PhysicsTransform(new Vector2(0f, -0.5f * length), PhysicsRotate.identity);
            definition.angularFrequency = DonutJointFrequency;
            definition.angularDamping = 0f;
            joint.definition = definition;

            previous = segment;
        }

        ring.SetActive(true);
    }

    // Drives both wheels at the specified speed, which wakes the car so it responds straight away.
    private void SetCarSpeed(float speed)
    {
        foreach (var wheel in new[] { m_RearWheel, m_FrontWheel })
        {
            if (wheel == null)
                continue;

            var definition = wheel.definition;
            definition.motorSpeed = speed;
            wheel.definition = definition;

            wheel.ApplyDefinition();
        }
    }

    // Returns whether a controls menu button is assigned and currently held down.
    private static bool IsPressed(ControlsMenu.CustomButton button) => button != null && button.isPressed;

    #region Internal

    // How the terrain is laid out: a wave of the given period repeated for a number of cycles, made of columns of boxes one meter apart.
    const float WavePeriod = 80f;
    const int CycleCount = 600;
    const float GridSize = 1f;
    const int GridCount = (int)(CycleCount * WavePeriod / GridSize);
    const float TerrainHeight = 4f;

    // How fast the wheels are driven in each direction, in degrees per second.
    const float ReverseSpeed = 1200f;
    const float ForwardSpeed = -300f;

    // The size a ragdoll is dropped at, the joint friction it is given, and the joint friction the ragdoll prefab was authored with.
    const float RagdollScale = 1.5f;
    const float RagdollJointFriction = 0.05f;
    const float PrefabJointFriction = 0.03f;

    // The shape of a soft ring: how many segments it has, its radius, the angle its first segment is placed at and how stiffly its joints spring.
    const int DonutSides = 7;
    const float DonutScale = 0.75f;
    const float DonutStartAngle = 35f;
    const float DonutJointFrequency = 5f;

    [SerializeField] CameraManipulator m_CameraManipulator;
    [SerializeField] PhysicsPose m_Chassis;
    [SerializeField] PhysicsConstraintWheel m_RearWheel;
    [SerializeField] PhysicsConstraintWheel m_FrontWheel;
    [SerializeField] GameObject m_BoxPrefab;
    [SerializeField] GameObject m_RagdollPrefab;
    [SerializeField] GameObject m_DonutSegmentPrefab;
    [SerializeField] bool m_FollowCar = true;
    [SerializeField, Range(-400f, 400f)] float m_CameraPanSpeed = 25f;

    readonly List<PhysicsBody> m_TerrainBodies = new();
    Vector2 m_CameraPosition;
    float m_StartX;
    int m_CycleIndex;
    int m_RagdollIndex;

    ControlsMenu.CustomButton m_ReverseButton;
    ControlsMenu.CustomButton m_ForwardButton;
    ControlsMenu.CustomButton m_BrakeButton;

    #endregion
}
