using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class BallAndChain : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private const int JointCount = 30;
    private NativeList<PhysicsHingeJoint> m_Joints;

    private float m_MaxMotorTorque;
    private float m_SpringFrequency;
    private float m_SpringDamping;
    private bool m_FixChainLength;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 28f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, -8f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);

        m_Joints = new NativeList<PhysicsHingeJoint>(JointCount, Allocator.Persistent);

        m_SpringFrequency = 40f;
        m_SpringDamping = 1f;
        m_MaxMotorTorque = 100f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        if (m_Joints.IsCreated)
            m_Joints.Dispose();

        // Reset overrides.
        m_SandboxManager.ResetOverrideDrawOptions();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

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

            // Joint Frequency.
            var maxMotorTorque = root.Q<Slider>("max-motor-torque");
            maxMotorTorque.value = m_MaxMotorTorque;
            maxMotorTorque.RegisterValueChangedCallback(evt =>
            {
                m_MaxMotorTorque = evt.newValue;
                UpdateJoints();
            });

            // Fix Chain Length.
            var fixChainLength = root.Q<Toggle>("fix-chain-length");
            fixChainLength.value = m_FixChainLength;
            fixChainLength.RegisterValueChangedCallback(evt =>
            {
                m_FixChainLength = evt.newValue;
                SetupScene();
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

        const float scale = 0.5f;

        // Ground Body.
        var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
        bodies.Add(groundBody);

        var prevBody = groundBody;

        // Chain.
        {
            // Create the chain links.
            for (var n = 0; n < JointCount; ++n)
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
                    springDamping = m_SpringDamping
                };
                m_Joints.Add(world.CreateJoint(jointDef));

                prevBody = body;
            }
        }

        // Ball.
        {
            var geometry = new CircleGeometry { radius = 4f };

            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
                position = new Vector2((1f + 2f * JointCount) * scale + geometry.radius - scale, JointCount * scale),
                gravityScale = 3f
            };

            var body = world.CreateBody(bodyDef);
            bodies.Add(body);

            var shapeDef = new PhysicsShapeDefinition
            {
                density = 20f,
                contactFilter = new PhysicsShape.ContactFilter { categories = 0x2, contacts = 0x1 },
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState }
            };
            body.CreateShape(geometry, shapeDef);

            var pivot = new Vector2(2f * JointCount * scale, JointCount * scale);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = prevBody,
                bodyB = body,
                localAnchorA = prevBody.GetLocalPoint(pivot),
                localAnchorB = body.GetLocalPoint(pivot),
                enableMotor = true,
                maxMotorTorque = m_MaxMotorTorque,
                enableSpring = true,
                springFrequency = m_SpringFrequency,
                springDamping = m_SpringDamping
            };
            m_Joints.Add(world.CreateJoint(jointDef));

            if (m_FixChainLength)
            {
                // Constraint the length of the chain.
                var distance = JointCount;
                world.CreateJoint(new PhysicsDistanceJointDefinition
                {
                    bodyA = groundBody,
                    bodyB = body,
                    localAnchorA = groundBody.GetLocalPoint(Vector2.up * JointCount * scale),
                    localAnchorB = body.GetLocalPoint(pivot),
                    distance = distance,
                    enableSpring = false,
                    springFrequency = 30f,
                    springDamping = 1f,
                    enableLimit = false,
                    minDistanceLimit = -distance,
                    maxDistanceLimit = distance
                });
            }
        }
    }

    private void UpdateJoints()
    {
        // Update the max motor torque.
        foreach (var joint in m_Joints)
        {
            joint.springFrequency = m_SpringFrequency;
            joint.springDamping = m_SpringDamping;
            joint.maxMotorTorque = m_MaxMotorTorque;

            joint.WakeBodies();
        }
    }
}