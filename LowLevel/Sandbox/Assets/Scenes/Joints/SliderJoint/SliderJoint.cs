using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class SliderJoint : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsSliderJoint m_Joint;

    private float m_SliderAngle;
    private bool m_EnableSpring;
    private float m_SpringTargetTranslation;
    private float m_SpringFrequency;
    private float m_SpringDamping;
    private bool m_EnableMotor;
    private float m_MotorSpeed;
    private float m_MaxMotorForce;
    private bool m_EnableLimit;
    private float m_LowerTranslationLimit;
    private float m_UpperTranslationLimit;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 12f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 9f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        m_SandboxManager.SetOverrideColorShapeState(true);

        m_SliderAngle = 45f;
        m_EnableSpring = false;
        m_SpringTargetTranslation = 0f;
        m_SpringFrequency = 1f;
        m_SpringDamping = 0.5f;
        m_EnableMotor = false;
        m_MotorSpeed = 2f;
        m_MaxMotorForce = 50f;
        m_EnableLimit = true;
        m_LowerTranslationLimit = -10f;
        m_UpperTranslationLimit = 10f;

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
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );

            // Slider Angle.
            var sliderAngle = root.Q<Slider>("slider-angle");
            sliderAngle.value = m_SliderAngle;
            sliderAngle.RegisterValueChangedCallback(evt =>
            {
                m_SliderAngle = evt.newValue;
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

            // Spring Target Translation.
            var springTargetTranslation = root.Q<Slider>("spring-target-translation");
            springTargetTranslation.value = m_SpringTargetTranslation;
            springTargetTranslation.RegisterValueChangedCallback(evt =>
            {
                m_SpringTargetTranslation = evt.newValue;
                m_Joint.springTargetTranslation = m_SpringTargetTranslation;
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
            
            // Max Motor Force.
            var maxMotorForce = root.Q<Slider>("max-motor-force");
            maxMotorForce.value = m_MaxMotorForce;
            maxMotorForce.RegisterValueChangedCallback(evt =>
            {
                m_MaxMotorForce = evt.newValue;
                m_Joint.maxMotorForce = m_MaxMotorForce;
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
            
            // Reset Scene.
            var resetScene = root.Q<Button>("reset-scene");
            resetScene.clicked += SetupScene;
            
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
        var bodies = m_SandboxManager.Bodies;

        // Ground Body.
        PhysicsBody groundBody;
        {
            groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            bodies.Add(groundBody);
        }

        {
            var bodeDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = Vector2.up * 9f };
            var body = world.CreateBody(bodeDef);
            bodies.Add(body);

            var geometry = new CapsuleGeometry { center1 = Vector2.down * 2f, center2 = Vector2.up * 2f, radius = 1f };
            body.CreateShape(geometry);

            var slideRotation = new PhysicsRotate(angle: PhysicsMath.ToRadians(m_SliderAngle));
            
            var jointPivot = new Vector2(0f, 9f);
            var jointDef = new PhysicsSliderJointDefinition
            {
                bodyA = groundBody,
                bodyB = body,
                localAnchorA = new PhysicsTransform(groundBody.GetLocalPoint(jointPivot), slideRotation),
                localAnchorB = new PhysicsTransform(body.GetLocalPoint(jointPivot), slideRotation),
                drawScale = 2f,
                enableSpring = m_EnableSpring,
                springTargetTranslation = m_SpringTargetTranslation,
                springFrequency = m_SpringFrequency,
                springDamping = m_SpringDamping,
                enableMotor = m_EnableMotor,
                motorSpeed = m_MotorSpeed,
                maxMotorForce = m_MaxMotorForce,
                enableLimit = m_EnableLimit,
                lowerTranslationLimit = m_LowerTranslationLimit,
                upperTranslationLimit = m_UpperTranslationLimit
            };

            m_Joint = world.CreateJoint(jointDef);
        }
    }
}
