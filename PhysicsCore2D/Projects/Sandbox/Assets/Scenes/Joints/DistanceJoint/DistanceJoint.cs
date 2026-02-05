using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class DistanceJoint : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private NativeList<PhysicsDistanceJoint> m_Joints;

    private int m_JointCount;
    private float m_JointDistance;
    private bool m_EnableSpring;
    private float m_SpringFrequency;
    private float m_SpringDamping;
    private float m_SpringTension;
    private float m_SpringCompression;
    private bool m_EnableLimit;
    private float m_MinDistanceLimit;
    private float m_MaxDistanceLimit;

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 8f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 14f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        m_SandboxManager.SetOverrideColorShapeState(true);

        m_JointCount = 3;
        m_JointDistance = 1f;
        m_EnableSpring = false;
        m_SpringFrequency = 5f;
        m_SpringDamping = 0.5f;
        m_SpringTension = 2000f;
        m_SpringCompression = 100f;
        m_EnableLimit = false;
        m_MinDistanceLimit = m_JointDistance;
        m_MaxDistanceLimit = m_JointDistance;

        m_Joints = new NativeList<PhysicsDistanceJoint>(m_JointCount, Allocator.Persistent);

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        if (m_Joints.IsCreated)
            m_Joints.Dispose();

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

            // Joint Count.
            var jointCount = root.Q<SliderInt>("joint-count");
            jointCount.value = m_JointCount;
            jointCount.RegisterValueChangedCallback(evt =>
            {
                m_JointCount = evt.newValue;
                SetupScene();
            });

            // Joint Distance.
            var jointDistance = root.Q<Slider>("joint-distance");
            jointDistance.value = m_JointDistance;
            jointDistance.RegisterValueChangedCallback(evt =>
            {
                m_JointDistance = evt.newValue;
                UpdateJoints();
            });

            // Enable Spring.
            var enableSpring = root.Q<Toggle>("enable-spring");
            enableSpring.value = m_EnableSpring;
            enableSpring.RegisterValueChangedCallback(evt =>
            {
                m_EnableSpring = evt.newValue;
                UpdateJoints();
            });

            // Spring Frequency.
            var springFrequency = root.Q<Slider>("spring-frequency");
            springFrequency.value = m_SpringFrequency;
            springFrequency.RegisterValueChangedCallback(evt =>
            {
                m_SpringFrequency = evt.newValue;
                UpdateJoints();
            });

            // Spring Damping.
            var springDamping = root.Q<Slider>("spring-damping");
            springDamping.value = m_SpringDamping;
            springDamping.RegisterValueChangedCallback(evt =>
            {
                m_SpringDamping = evt.newValue;
                UpdateJoints();
            });

            // Spring Tension.
            var springTension = root.Q<Slider>("spring-tension");
            springTension.value = m_SpringTension;
            springTension.RegisterValueChangedCallback(evt =>
            {
                m_SpringTension = evt.newValue;
                UpdateJoints();
            });

            // Spring Compression.
            var springCompression = root.Q<Slider>("spring-compression");
            springCompression.value = m_SpringCompression;
            springCompression.RegisterValueChangedCallback(evt =>
            {
                m_SpringCompression = evt.newValue;
                UpdateJoints();
            });

            // Enable Limit.
            var enableLimit = root.Q<Toggle>("enable-limit");
            enableLimit.value = m_EnableLimit;
            enableLimit.RegisterValueChangedCallback(evt =>
            {
                m_EnableLimit = evt.newValue;
                UpdateJoints();
            });

            // Min Distance Limit.
            var minDistanceLimit = root.Q<Slider>("min-distance-limit");
            minDistanceLimit.value = m_MinDistanceLimit;
            minDistanceLimit.RegisterValueChangedCallback(evt =>
            {
                m_MinDistanceLimit = evt.newValue;
                if (m_MinDistanceLimit > m_MaxDistanceLimit)
                {
                    m_MinDistanceLimit = m_MaxDistanceLimit;
                    minDistanceLimit.value = m_MinDistanceLimit;
                    return;
                }

                SetupScene();
            });

            // Max Distance Limit.
            var maxDistanceLimit = root.Q<Slider>("max-distance-limit");
            maxDistanceLimit.value = m_MaxDistanceLimit;
            maxDistanceLimit.RegisterValueChangedCallback(evt =>
            {
                m_MaxDistanceLimit = evt.newValue;
                if (m_MaxDistanceLimit < m_MinDistanceLimit)
                {
                    m_MaxDistanceLimit = m_MinDistanceLimit;
                    maxDistanceLimit.value = m_MaxDistanceLimit;
                    return;
                }

                SetupScene();
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

        m_Joints.Clear();

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Ground Body.
        var groundBody = world.CreateBody();

        const float radius = 0.25f;
        var geometry = new CircleGeometry { radius = radius };
        var shapeDef = new PhysicsShapeDefinition { density = 20f };
        var jointDef = new PhysicsDistanceJointDefinition
        {
            distance = m_JointDistance,
            enableSpring = m_EnableSpring,
            springFrequency = m_SpringFrequency,
            springDamping = m_SpringDamping,
            springLowerForce = -m_SpringTension,
            springUpperForce = m_SpringCompression,
            enableLimit = m_EnableLimit,
            minDistanceLimit = m_MinDistanceLimit,
            maxDistanceLimit = m_MaxDistanceLimit,
        };

        const float offsetY = 20f;
        var prevBody = groundBody;

        // Joints.
        {
            // Create the joints.
            for (var n = 0; n < m_JointCount; ++n)
            {
                var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, angularDamping = 1f, position = new Vector2(m_JointDistance * (n + 1f), offsetY) };
                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                body.CreateShape(geometry, shapeDef);

                var pivotA = new Vector2(m_JointDistance * n, offsetY);
                var pivotB = new Vector2(m_JointDistance * (n + 1f), offsetY);
                jointDef.bodyA = prevBody;
                jointDef.bodyB = body;
                jointDef.localAnchorA = new PhysicsTransform { position = prevBody.GetLocalPoint(pivotA) };
                jointDef.localAnchorB = new PhysicsTransform { position = body.GetLocalPoint(pivotB) };
                m_Joints.Add(world.CreateJoint(jointDef));

                prevBody = body;
            }
        }
    }

    private void UpdateJoints()
    {
        // Update the max motor torque.
        foreach (var joint in m_Joints)
        {
            joint.distance = m_JointDistance;
            joint.enableSpring = m_EnableSpring;
            joint.springFrequency = m_SpringFrequency;
            joint.springDamping = m_SpringDamping;
            joint.springLowerForce = -m_SpringTension;
            joint.springUpperForce = m_SpringCompression;
            joint.enableLimit = m_EnableLimit;
            joint.minDistanceLimit = m_MinDistanceLimit;
            joint.maxDistanceLimit = m_MaxDistanceLimit;

            joint.WakeBodies();
        }
    }
}