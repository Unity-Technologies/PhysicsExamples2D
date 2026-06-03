using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Benchmarks", "A vast world far from the origin with a drivable car and streamed debris.")]
public sealed class LargeWorld : SandboxExampleBehaviour
{
    private bool m_FollowCar;

    private float m_WavePeriod;
    private float m_GridSize;
    private float m_GridCount;
    private int m_CycleCount;
    private int m_CycleIndex;
    private float m_CameraPanSpeed;
    private float m_StartX;
    private int m_RagdollIndex;
    private RagdollFactory.Configuration m_RagDollConfiguration;

    private Vector2 m_CameraPosition;
    private PhysicsWheelJoint m_RearWheelJoint;
    private PhysicsWheelJoint m_FrontWheelJoint;

    private FloatField m_WorldPositionField;
    private FloatField m_WorldSizeField;

    private ControlsMenu.CustomButton m_ReverseButton;
    private ControlsMenu.CustomButton m_ForwardButton;
    private ControlsMenu.CustomButton m_BrakeButton;

    protected override float CameraSize => 20f;
    protected override Vector2 CameraPosition => new(0f, -5f);

    protected override void OnExampleEnable()
    {
        // Set controls.
        {
            m_ReverseButton = SandboxManager.ControlsMenu[2];
            m_ForwardButton = SandboxManager.ControlsMenu[1];
            m_BrakeButton = SandboxManager.ControlsMenu[0];

            m_ReverseButton.Set("Reverse [←]");
            m_ForwardButton.Set("Forward [→]");
            m_BrakeButton.Set("Brake [Spc]");
        }

        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.Off);
        CameraManipulator.DisableManipulators = true;

        m_WavePeriod = 80f;
        m_CycleCount = 600;
        m_GridSize = 1f;
        m_GridCount = (int)(m_CycleCount * m_WavePeriod / m_GridSize);
        m_CameraPanSpeed = 25f;

        m_FollowCar = true;
    }

    protected override void OnExampleDisable()
    {
        CameraManipulator.DisableManipulators = false;
    }

    protected override void SetupOptions()
    {
        // Follow Car.
        AddToggle("Follow Car", m_FollowCar, v =>
        {
            m_FollowCar = v;

            // If we're no longer following the car then set the current camera position to the car position.
            if (!m_FollowCar)
                m_CameraPosition = new Vector2(m_FrontWheelJoint.bodyA.position.x, m_CameraPosition.y);
        });

        // Camera Pan Speed.
        AddSlider("Camera Pan Speed", m_CameraPanSpeed, -400f, 400f, v => m_CameraPanSpeed = v);

        // World Position (read-only).
        m_WorldPositionField = AddElement(new FloatField("World Position (Km)") { isReadOnly = true, focusable = false });

        // World Size (read-only).
        m_WorldSizeField = AddElement(new FloatField("World Size (Km)") { isReadOnly = true, focusable = false });
        m_WorldSizeField.value = m_GridSize * m_GridCount / 1000.0f;
    }

    protected override void SetupScene()
    {
        // Cancel spawning batches.
        CancelInvoke(nameof(SpawnBatch));

        m_CycleIndex = 0;
        m_RagdollIndex = 0;
        m_RagDollConfiguration = new RagdollFactory.Configuration
        {
            ScaleRange = new Vector2(1.5f, 1.5f),
            JointFrequency = 0f,
            JointDamping = 0f,
            JointFriction = 0.05f,
            GravityScale = 1f,
            ContactBodyLayer = 2,
            ContactFeetLayer = 1,
            ContactGroupIndex = 1,
            ColorProvider = SandboxManager,
            FastCollisionsAllowed = false,
            TriggerEvents = false,
            EnableLimits = true,
            EnableMotor = true
        };

        // Get the default world.
        var world = World;

        var omega = PhysicsMath.TAU / m_WavePeriod;
        m_StartX = -0.5f * m_CycleCount * m_WavePeriod;

        // Set the camera position.
        m_CameraPosition = new Vector2(m_StartX, 15f);
        CameraManipulator.CameraPosition = m_CameraPosition;

        {
            var bodyDef = PhysicsBodyDefinition.defaultDefinition;
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            // Setting this to false significantly reduces the cost of creating static bodies and shapes.
            shapeDef.startStaticContacts = false;

            const float height = 4f;
            var bodyX = m_StartX;
            var shapeX = m_StartX;

            PhysicsBody groundBody = default;

            // Limits.
            {
                var body = world.CreateBody();
                body.CreateShape(new SegmentGeometry { point1 = new Vector2(m_StartX - m_GridSize, 0f), point2 = new Vector2(m_StartX - m_GridSize, 100f) });
                body.CreateShape(new SegmentGeometry { point1 = new Vector2(m_StartX + m_GridCount * m_GridSize, 0f), point2 = new Vector2(m_StartX + m_GridCount * m_GridSize, 100f) });
            }

            for (var i = 0; i < m_GridCount; ++i)
            {
                // Create a new body regularly so that shapes are not too far from the body origin.
                // Most algorithms in physics work in local coordinates, but contact points are computed relative to the body origin.
                // This makes a noticeable improvement in stability far from the origin.
                if (i % 10 == 0)
                {
                    bodyDef.position = new Vector2(bodyX, bodyDef.position.y);
                    groundBody = world.CreateBody(bodyDef);
                    shapeX = 0.0f;
                }

                var y = 0.0f;
                var countY = Mathf.RoundToInt(height * Mathf.Cos(omega * bodyX)) + 12;

                for (var j = 0; j < countY; ++j)
                {
                    var squareGeometry = PolygonGeometry.CreateBox(Vector2.one * 0.8f * m_GridSize, 0.1f, new Vector2(shapeX, y));
                    groundBody.CreateShape(squareGeometry, shapeDef);

                    y += m_GridSize;
                }

                bodyX += m_GridSize;
                shapeX += m_GridSize;
            }
        }

        // Car.
        {
            const float springFrequency = 10f;
            const float springDamping = 0.7f;
            const float maxMotorTorque = 2000f;
            using var car = CarFactory.Spawn(world, new Vector2(m_StartX + 20f, 40f), 10f, springFrequency, springDamping, maxMotorTorque, 1f, out m_RearWheelJoint, out m_FrontWheelJoint);
        }

        // Start spawning batches.
        InvokeRepeating(nameof(SpawnBatch), 0f, 2f);
    }

    private void SpawnBatch()
    {
        // Finish spawning batches if needed.
        if (m_CycleIndex >= m_CycleCount)
        {
            // Cancel spawning batches.
            CancelInvoke(nameof(SpawnBatch));

            return;
        }

        // Get the default world.
        var world = World;

        ref var random = ref Random;

        var baseX = (0.5f + m_CycleIndex) * m_WavePeriod + m_StartX;

        var remainder = m_CycleIndex % 3;
        if (remainder == 0)
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(baseX - 3f, 10f) };

            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(0.6f, 0.4f));
            for (var i = 0; i < 10; ++i)
            {
                bodyDef.position = new Vector2(bodyDef.position.x, 10f);
                for (var j = 0; j < 5; ++j)
                {
                    var body = world.CreateBody(bodyDef);
                    body.CreateShape(boxGeometry);

                    bodyDef.position = new Vector2(bodyDef.position.x, bodyDef.position.y + 0.5f);
                }

                bodyDef.position = new Vector2(bodyDef.position.x + 0.6f, bodyDef.position.y);
            }
        }
        else if (remainder == 1)
        {
            var position = new Vector2(baseX - 2f, 10f);
            for (var i = 0; i < 5; ++i)
            {
                m_RagDollConfiguration.ContactGroupIndex = m_RagdollIndex++;
                using var ragdoll = RagdollFactory.Spawn(world, position, m_RagDollConfiguration, true, ref random);

                position.x += 1f;
            }
        }
        else
        {
            var position = new Vector2(baseX - 4f, 12f);
            for (var i = 0; i < 5; ++i)
            {
                using var donut = SoftbodyFactory.SpawnDonut(world, SandboxManager, position, 7, 0.75f);

                position.x += 2f;
            }
        }

        // Next cycle.
        m_CycleIndex++;
    }

    private void Update()
    {
        // Finish if the world is paused.
        if (SandboxManager.WorldPaused)
            return;

        // Fetch keyboard input.
        var currentKeyboard = Keyboard.current;

        // Reverse.
        if (m_ReverseButton.isPressed || currentKeyboard.leftArrowKey.isPressed)
            SetCarSpeed(1200f);

        // Forward.
        if (m_ForwardButton.isPressed || currentKeyboard.rightArrowKey.isPressed)
            SetCarSpeed(-300f);

        // Brake.
        if (m_BrakeButton.isPressed || currentKeyboard.spaceKey.isPressed)
            SetCarSpeed(0f);

        // Camera Pan.
        {
            var cameraBound = 0.5f * m_WavePeriod * m_CycleCount;
            m_CameraPosition.x = Mathf.Clamp(m_CameraPosition.x + Time.deltaTime * m_CameraPanSpeed, -cameraBound, cameraBound);

            if (m_CameraPanSpeed != 0.0f)
                CameraManipulator.CameraPosition = m_CameraPosition;

            if (m_FollowCar)
                CameraManipulator.CameraPosition = new Vector2(m_FrontWheelJoint.bodyA.position.x, CameraManipulator.CameraPosition.y);
        }

        // Show world position.
        m_WorldPositionField.value = CameraManipulator.CameraPosition.x / 1000.0f;
    }

    private void SetCarSpeed(float speed)
    {
        m_RearWheelJoint.motorSpeed = speed;
        m_FrontWheelJoint.motorSpeed = speed;
        m_RearWheelJoint.WakeBodies();
    }
}
