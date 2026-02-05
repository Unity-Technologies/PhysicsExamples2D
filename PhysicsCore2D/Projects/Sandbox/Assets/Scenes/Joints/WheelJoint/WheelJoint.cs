using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class WheelJoint : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsWheelJoint m_Joint;

    private float m_WheelAngle;
    private bool m_EnableSpring;
    private float m_SpringFrequency;
    private float m_SpringDamping;
    private bool m_EnableMotor;
    private float m_MotorSpeed;
    private float m_MaxMotorTorque;
    private bool m_EnableLimit;
    private float m_LowerTranslationLimit;
    private float m_UpperTranslationLimit;

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 4f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 10f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        m_SandboxManager.SetOverrideColorShapeState(true);

        m_WheelAngle = 90f;
        m_EnableSpring = true;
        m_SpringFrequency = 1.5f;
        m_SpringDamping = 0.7f;
        m_EnableMotor = true;
        m_MotorSpeed = 2f;
        m_MaxMotorTorque = 5f;
        m_EnableLimit = true;
        m_LowerTranslationLimit = -1f;
        m_UpperTranslationLimit = 1f;

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

            // Wheel Angle.
            var wheelAngle = root.Q<Slider>("wheel-angle");
            wheelAngle.value = m_WheelAngle;
            wheelAngle.RegisterValueChangedCallback(evt =>
            {
                m_WheelAngle = evt.newValue;
                SetupScene();
            });

            // Enable Spring.
            var enableSpring = root.Q<Toggle>("enable-spring");
            enableSpring.value = m_EnableSpring;
            enableSpring.RegisterValueChangedCallback(evt =>
            {
                m_EnableSpring = evt.newValue;
                m_Joint.enableSpring = m_EnableSpring;
                m_Joint.WakeBodies();
            });

            // Spring Frequency.
            var springFrequency = root.Q<Slider>("spring-frequency");
            springFrequency.value = m_SpringFrequency;
            springFrequency.RegisterValueChangedCallback(evt =>
            {
                m_SpringFrequency = evt.newValue;
                m_Joint.springFrequency = m_SpringFrequency;
                m_Joint.WakeBodies();
            });

            // Spring Damping.
            var springDamping = root.Q<Slider>("spring-damping");
            springDamping.value = m_SpringDamping;
            springDamping.RegisterValueChangedCallback(evt =>
            {
                m_SpringDamping = evt.newValue;
                m_Joint.springDamping = m_SpringDamping;
                m_Joint.WakeBodies();
            });

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

            // Max Motor Torque.
            var maxMotorTorque = root.Q<Slider>("max-motor-torque");
            maxMotorTorque.value = m_MaxMotorTorque;
            maxMotorTorque.RegisterValueChangedCallback(evt =>
            {
                m_MaxMotorTorque = evt.newValue;
                m_Joint.maxMotorTorque = m_MaxMotorTorque;
                m_Joint.WakeBodies();
            });

            // Enable Limit.
            var enableLimit = root.Q<Toggle>("enable-limit");
            enableLimit.value = m_EnableLimit;
            enableLimit.RegisterValueChangedCallback(evt =>
            {
                m_EnableLimit = evt.newValue;
                m_Joint.enableLimit = m_EnableLimit;
                m_Joint.WakeBodies();
            });

            // Lower Translation Limit.
            var lowerTranslationLimit = root.Q<Slider>("min-translation-limit");
            lowerTranslationLimit.value = m_LowerTranslationLimit;
            lowerTranslationLimit.RegisterValueChangedCallback(evt =>
            {
                m_LowerTranslationLimit = evt.newValue;
                if (m_LowerTranslationLimit > m_UpperTranslationLimit)
                {
                    m_LowerTranslationLimit = m_UpperTranslationLimit;
                    lowerTranslationLimit.value = m_LowerTranslationLimit;
                    return;
                }

                m_Joint.lowerTranslationLimit = m_LowerTranslationLimit;
                m_Joint.WakeBodies();
            });

            // Upper Translation Limit.
            var upperTranslationLimit = root.Q<Slider>("max-translation-limit");
            upperTranslationLimit.value = m_UpperTranslationLimit;
            upperTranslationLimit.RegisterValueChangedCallback(evt =>
            {
                m_UpperTranslationLimit = evt.newValue;
                if (m_UpperTranslationLimit < m_LowerTranslationLimit)
                {
                    m_UpperTranslationLimit = m_LowerTranslationLimit;
                    upperTranslationLimit.value = m_UpperTranslationLimit;
                    return;
                }

                m_Joint.upperTranslationLimit = m_UpperTranslationLimit;
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

        // Ground Body.
        var groundBody = world.CreateBody();

        {
            var bodeDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = Vector2.up * 10.25f, fastRotationAllowed = true };
            var body = world.CreateBody(bodeDef);

            //var geometry = new CapsuleGeometry { center1 = Vector2.down * 0.5f, center2 = Vector2.up * 0.5f, radius = 0.5f };
            var geometry = new CircleGeometry { radius = 0.5f };
            body.CreateShape(geometry);

            var wheelRotation = PhysicsRotate.CreateDegrees(m_WheelAngle);

            var jointPivot = new Vector2(0f, 10f);
            var jointDef = new PhysicsWheelJointDefinition
            {
                bodyA = groundBody,
                bodyB = body,
                localAnchorA = new PhysicsTransform(groundBody.GetLocalPoint(jointPivot), wheelRotation),
                localAnchorB = PhysicsTransform.identity,
                drawScale = 2f,
                enableSpring = m_EnableSpring,
                springFrequency = m_SpringFrequency,
                springDamping = m_SpringDamping,
                enableMotor = m_EnableMotor,
                motorSpeed = m_MotorSpeed,
                maxMotorTorque = m_MaxMotorTorque,
                enableLimit = m_EnableLimit,
                lowerTranslationLimit = m_LowerTranslationLimit,
                upperTranslationLimit = m_UpperTranslationLimit
            };

            m_Joint = world.CreateJoint(jointDef);
        }
    }
}