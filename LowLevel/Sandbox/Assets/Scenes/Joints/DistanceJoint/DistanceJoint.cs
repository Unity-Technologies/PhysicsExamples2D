using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class DistanceJoint : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private NativeList<PhysicsHingeJoint> m_Joints;
    
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
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 8f;
        m_CameraManipulator.CameraStartPosition = new Vector2(0f, 12f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_JointCount = 1;
        m_JointDistance = 1f;
        m_EnableSpring = false;
        m_SpringFrequency = 5f;
        m_SpringDamping = 0.5f;
        m_SpringTension = 2000f;
        m_SpringCompression = 100f;
        m_EnableLimit = false;
        m_MinDistanceLimit = m_JointDistance;
        m_MaxDistanceLimit = m_JointDistance;
            
        m_Joints = new NativeList<PhysicsHingeJoint>(m_JointCount, Allocator.Persistent);
    
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        if (m_Joints.IsCreated)
            m_Joints.Dispose();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;
        
        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );

            // Joint Count.
            var jointCount = root.Q<SliderInt>("joint-count");
            jointCount.value = m_JointCount;
            jointCount.RegisterValueChangedCallback(evt =>
            {
                m_JointCount = evt.newValue;
                UpdateJoints();
            });
            
            // Spring Frequency.
            var jointDidtance = root.Q<Slider>("joint-distance");
            jointDidtance.value = m_JointDistance;
            jointDidtance.RegisterValueChangedCallback(evt =>
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
                UpdateJoints();
            });
            
            // Max Distance Limit.
            var maxDistanceLimit = root.Q<Slider>("max-distance-limit");
            maxDistanceLimit.value = m_MaxDistanceLimit;
            maxDistanceLimit.RegisterValueChangedCallback(evt =>
            {
                m_MaxDistanceLimit = evt.newValue;
                UpdateJoints();
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

        m_Joints.Clear();

        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;

        //const float scale = 0.5f;

        // Ground Body.
        var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
        bodies.Add(groundBody);

#if false        
        const float radius = 0.25f;
        var geometry = new CircleGeometry { radius = radius };
        var shapeDef = new PhysicsShapeDefinition { density = 20f };
        var jointDef = new PhysicsDistanceJointDefinition
        {
            distance = m_JointDistance,
            enableSpring = m_EnableSpring,
            springFrequency = m_SpringFrequency,
            springDampingRatio = m_SpringDamping,
            springLowerForce = -m_SpringTension,
        };

        var offsetY = 20f;

        

        var prevBody = groundBody;

        // Joints.
        {
            // Create the joints.
            for (var n = 0; n < m_JointCount; ++n)
            {
                var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = new Vector2((1f + 2f * n) * scale, JointCount * scale) };
                var body = world.CreateBody(bodyDef);
                bodies.Add(body);

                var geometry = new CapsuleGeometry { center1 = Vector2.left * scale, center2 = Vector2.right * scale, radius = 0.125f };
                var shapeDef = new PhysicsShapeDefinition
                {
                    density = 20f,
                    contactFilter = new PhysicsShape.ContactFilter { categories = 0x1, contacts = 0x2 },
                    surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState }
                };
                body.CreateShape(geometry, shapeDef);

                var pivot = new Vector2(2f * n * scale, JointCount * scale);
                var jointDef = new PhysicsHingeJointDefinition
                {
                    bodyA = prevBody,
                    bodyB = body,
                    localAnchorA = prevBody.GetLocalPoint(pivot),
                    localAnchorB = body.GetLocalPoint(pivot),
                    enableMotor = true,
                    maxMotorTorque = m_MaxMotorTorque,
                    enableSpring = n > 0,
                    springFrequency = m_SpringFrequency,
                    springDampingRatio = m_SpringDamping
                };
                m_Joints.Add(world.CreateJoint(jointDef));

                prevBody = body;
            }
        }
#endif
    }

    private void UpdateJoints()
    {
#if false        
        // Update the max motor torque.
        foreach (var joint in m_Joints)
        {
            //joint.springFrequency = m_SpringFrequency;
            //joint.springDamping = m_SpringDamping;
        }
#endif
    }
}
