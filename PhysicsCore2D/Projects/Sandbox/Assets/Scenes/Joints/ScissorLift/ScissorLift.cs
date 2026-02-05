using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class ScissorLift : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsDistanceJoint m_Joint;

    private bool m_EnableMotor;
    private float m_MotorSpeed;

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 10f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 8f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        m_SandboxManager.SetOverrideColorShapeState(true);

        m_EnableMotor = false;
        m_MotorSpeed = 0.25f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Reset overrides.
        m_SandboxManager.ResetOverrideDrawOptions();
        m_SandboxManager.ResetOverrideColorShapeState();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Enable Motor.
            var enableMotor = root.Q<Toggle>("enable-motor");
            enableMotor.value = m_EnableMotor;
            enableMotor.RegisterValueChangedCallback(evt =>
            {
                m_EnableMotor = evt.newValue;
                m_Joint.enableMotor = m_EnableMotor;
                m_Joint.WakeBodies();
            });

            // Motor Speed.
            var motorSpeed = root.Q<Slider>("motor-speed");
            motorSpeed.value = m_MotorSpeed;
            motorSpeed.RegisterValueChangedCallback(evt =>
            {
                m_MotorSpeed = evt.newValue;
                m_Joint.motorSpeed = m_MotorSpeed;
                m_Joint.WakeBodies();
            });

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        var groundBody = world.CreateBody();

        // Ground.
        {
            groundBody.CreateShape(new SegmentGeometry { point1 = Vector2.left * 20f, point2 = Vector2.right * 20f });
        }

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, sleepThreshold = 0.01f };
        var capsule = new CapsuleGeometry { center1 = Vector2.left * 2.5f, center2 = Vector2.right * 2.5f, radius = 0.15f };

        var base1 = groundBody;
        var base2 = groundBody;
        var baseAnchor1 = new Vector2(-2.5f, 0.2f);
        var baseAnchor2 = new Vector2(2.5f, 0.2f);

        var y = 0.5f;

        PhysicsBody linkBody1 = default;
        const int levels = 3;

        for (var n = 0; n < levels; ++n)
        {
            bodyDef.position = new Vector2(0f, y);
            bodyDef.rotation = PhysicsRotate.CreateRadians(0.15f);
            var body1 = world.CreateBody(bodyDef);
            body1.CreateShape(capsule);

            bodyDef.rotation = PhysicsRotate.CreateRadians(0.15f);
            var body2 = world.CreateBody(bodyDef);
            body2.CreateShape(capsule);

            if (n == 1)
                linkBody1 = body2;

            // Left Pin.
            world.CreateJoint(new PhysicsHingeJointDefinition
            {
                bodyA = base1,
                bodyB = body1,
                localAnchorA = baseAnchor1,
                localAnchorB = new PhysicsTransform(new Vector2(-2.5f, 0.0f)),
                collideConnected = (n == 0)
            });

            // Right Pin.
            if (n == 0)
            {
                world.CreateJoint(new PhysicsWheelJointDefinition
                {
                    bodyA = base2,
                    bodyB = body2,
                    localAnchorA = baseAnchor2,
                    localAnchorB = new PhysicsTransform(new Vector2(2.5f, 0.0f)),
                    enableSpring = false,
                    collideConnected = true
                });
            }
            else
            {
                world.CreateJoint(new PhysicsHingeJointDefinition
                {
                    bodyA = base2,
                    bodyB = body2,
                    localAnchorA = baseAnchor2,
                    localAnchorB = new PhysicsTransform(new Vector2(2.5f, 0.0f)),
                    collideConnected = false
                });
            }

            // Middle Pin.
            world.CreateJoint(new PhysicsHingeJointDefinition
            {
                bodyA = body1,
                bodyB = body2,
                collideConnected = false
            });

            base1 = body2;
            base2 = body1;
            baseAnchor1 = new Vector2(-2.5f, 0.0f);
            baseAnchor2 = new Vector2(2.5f, 0.0f);
            y += 1.0f;
        }

        bodyDef.position = new Vector2(0.0f, y);
        bodyDef.rotation = PhysicsRotate.identity;

        var platform = world.CreateBody(bodyDef);

        var box = PolygonGeometry.CreateBox(new Vector2(6.0f, 0.4f));
        platform.CreateShape(box);

        // Left Pin.
        world.CreateJoint(new PhysicsHingeJointDefinition
        {
            bodyA = platform,
            bodyB = base1,
            localAnchorA = new PhysicsTransform(new Vector2(-2.5f, -0.4f)),
            localAnchorB = baseAnchor1,
            collideConnected = true
        });

        // Right Pin.
        world.CreateJoint(new PhysicsWheelJointDefinition
        {
            bodyA = platform,
            bodyB = base2,
            localAnchorA = new PhysicsTransform(new Vector2(2.5f, -0.4f)),
            localAnchorB = baseAnchor2,
            enableSpring = false,
            collideConnected = true
        });

        m_Joint = world.CreateJoint(new PhysicsDistanceJointDefinition
        {
            bodyA = groundBody,
            bodyB = linkBody1,
            localAnchorA = new PhysicsTransform(new Vector2(-2.5f, 0.2f)),
            localAnchorB = new PhysicsTransform(new Vector2(0.5f, 0.0f)),
            enableSpring = true,
            enableLimit = true,
            minDistanceLimit = 0.2f,
            maxDistanceLimit = 5.5f,
            collideConnected = true,
            enableMotor = m_EnableMotor,
            motorSpeed = m_MotorSpeed,
            maxMotorForce = 10000f
        });

        // Car.
        {
            const float springFrequency = 3f;
            const float springDamping = 0.7f;
            const float maxMotorTorque = 0f;
            using var car = CarFactory.Spawn(world, new Vector2(0f, y + 2f), 1f, springFrequency, springDamping, maxMotorTorque, 1f, out var rearWheelJoint, out var frontWheelJoint);
        }
    }
}