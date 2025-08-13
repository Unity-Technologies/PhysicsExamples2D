using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class LargeWorld : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private bool m_FollowCar;

    private float m_WavePeriod;
    private float m_GridSize;
    private float m_GridCount;
    private int m_CycleCount;
    private float m_CameraPanSpeed;

    private Vector2 m_CameraPosition;
    private PhysicsWheelJoint m_RearWheelJoint;
    private PhysicsWheelJoint m_FrontWheelJoint;
    
    private FloatField m_WorldPositionField;
    private FloatField m_WorldSizeField;

    private ControlsMenu.CustomButton m_ReverseButton;
    private ControlsMenu.CustomButton m_ForwardButton;
    private ControlsMenu.CustomButton m_BrakeButton;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 20f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, -5f);

        // Set controls.
        {
            m_ReverseButton = m_SandboxManager.ControlsMenu[2];
            m_ForwardButton = m_SandboxManager.ControlsMenu[1];
            m_BrakeButton = m_SandboxManager.ControlsMenu[0];
            
            m_ReverseButton.Set("Reverse");
            m_ForwardButton.Set("Forward");
            m_BrakeButton.Set("Brake");
        }
        
        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.Off);
        m_CameraManipulator.DisableManipulators = true;

        m_WavePeriod = 80f;
        m_CycleCount = 600;
        m_GridSize = 1f;
        m_GridCount = (int)(m_CycleCount * m_WavePeriod / m_GridSize);
        m_CameraPanSpeed = 25f;

        m_FollowCar = true;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Reset overrides.
        m_SandboxManager.ResetOverrideColorShapeState();        
        m_SandboxManager.ResetOverrideDrawOptions();
        m_CameraManipulator.DisableManipulators = false;
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Follow Car.
            var followCar = root.Q<Toggle>("follow-car");
            followCar.value = m_FollowCar;
            followCar.RegisterValueChangedCallback(evt =>
            {
                m_FollowCar = evt.newValue;
                
                // If we're no longer following the car then set the current camera position to the car position.
                if (!m_FollowCar)
                    m_CameraPosition = new Vector2(m_FrontWheelJoint.bodyA.position.x, m_CameraPosition.y);
            });

            // Camera Pan Speed.
            var cameraPanSpeed = root.Q<Slider>("camera-pan-speed");
            cameraPanSpeed.value = m_CameraPanSpeed;
            cameraPanSpeed.RegisterValueChangedCallback(evt => { m_CameraPanSpeed = evt.newValue; });

            // World Position.
            m_WorldPositionField = root.Q<FloatField>("world-position");
            
            // World Size.
            m_WorldSizeField = root.Q<FloatField>("world-size");
            m_WorldSizeField.value = m_GridSize * m_GridCount / 1000.0f;
            
            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        var world = PhysicsWorld.defaultWorld;
        ref var random = ref m_SandboxManager.Random;

        var omega = PhysicsMath.TAU / m_WavePeriod;
        var startX = -0.5f * m_CycleCount * m_WavePeriod;

        // Set the camera position.
        m_CameraPosition = new Vector2(startX, 15f);
        m_CameraManipulator.CameraPosition = m_CameraPosition;

        {
            var bodyDef = PhysicsBodyDefinition.defaultDefinition;
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            // Setting this to false significantly reduces the cost of creating static bodies and shapes.
            shapeDef.startStaticContacts = false;

            const float height = 4f;
            var bodyX = startX;
            var shapeX = startX;

            PhysicsBody groundBody = default;

            // Limits.
            {
                var body = world.CreateBody();
                body.CreateShape(new SegmentGeometry { point1 = new Vector2(startX - m_GridSize, 0f), point2 = new Vector2(startX - m_GridSize, 100f) });
                body.CreateShape(new SegmentGeometry { point1 = new Vector2(startX + m_GridCount * m_GridSize, 0f), point2 = new Vector2(startX + m_GridCount * m_GridSize, 100f) });
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

        {
            var ragdollIndex = 0;

            var ragDollConfiguration = new RagdollFactory.Configuration
            {
                ScaleRange = new Vector2(1.5f, 1.5f),
                JointFrequency = 0f,
                JointDamping = 0f,
                JointFriction = 0.05f,
                GravityScale = 1f,
                ContactBodyLayer = 2,
                ContactFeetLayer = 1,
                ContactGroupIndex = 1,
                ColorProvider = m_SandboxManager,
                FastCollisionsAllowed = false,
                TriggerEvents = false,
                EnableLimits = true,
                EnableMotor = true
            };

            for (var cycleIndex = 0; cycleIndex < m_CycleCount; ++cycleIndex)
            {
                var baseX = (0.5f + cycleIndex) * m_WavePeriod + startX;

                var remainder = cycleIndex % 3;
                if (remainder == 0)
                {
                    var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = new Vector2(baseX - 3f, 10f) };

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
                        ragDollConfiguration.ContactGroupIndex = ragdollIndex++;
                        using var ragdoll = RagdollFactory.Spawn(world, position, ragDollConfiguration, true, ref random);

                        position.x += 1f;
                    }
                }
                else
                {
                    var position = new Vector2(baseX - 4f, 12f);
                    for (var i = 0; i < 5; ++i)
                    {
                        using var donut = SoftbodyFactory.SpawnDonut(world, m_SandboxManager, position, 7, 0.75f);

                        position.x += 2f;
                    }
                }
            }
        }

        // Car.
        {
            const float springFrequency = 10f;
            const float springDamping = 0.7f;
            const float maxMotorTorque = 2000f;
            using var car = CarFactory.Spawn(world, new Vector2(startX + 20f, 40f), 10f, springFrequency, springDamping, maxMotorTorque, 1f, out m_RearWheelJoint, out m_FrontWheelJoint);
        }
    }

    private void Update()
    {
        // Finish if the world is paused.
        if (m_SandboxManager.WorldPaused)
            return;
        
        // Fetch keyboard input.
        var currentKeyboard = Keyboard.current;

        // Reverse.
        if (m_ReverseButton.isPressed || currentKeyboard.leftArrowKey.isPressed)
            SetCarSpeed(20f);

        // Forward.
        if (m_ForwardButton.isPressed || currentKeyboard.rightArrowKey.isPressed)
            SetCarSpeed(-5);

        // Brake.
        if (m_BrakeButton.isPressed || currentKeyboard.spaceKey.isPressed)
            SetCarSpeed(0f);

        // Camera Pan.
        {
            var cameraBound = 0.5f * m_WavePeriod * m_CycleCount;
            m_CameraPosition.x = Mathf.Clamp(m_CameraPosition.x + Time.deltaTime * m_CameraPanSpeed, -cameraBound, cameraBound);

            if (m_CameraPanSpeed != 0.0f)
                m_CameraManipulator.CameraPosition = m_CameraPosition;

            if (m_FollowCar)
                m_CameraManipulator.CameraPosition = new Vector2(m_FrontWheelJoint.bodyA.position.x, m_CameraManipulator.CameraPosition.y);
        }
        
        // Show world position.
        m_WorldPositionField.value = m_CameraManipulator.CameraPosition.x / 1000.0f;
    }

    private void SetCarSpeed(float speed)
    {
        m_RearWheelJoint.motorSpeed = speed;
        m_FrontWheelJoint.motorSpeed = speed;
        m_RearWheelJoint.WakeBodies();
    }
}