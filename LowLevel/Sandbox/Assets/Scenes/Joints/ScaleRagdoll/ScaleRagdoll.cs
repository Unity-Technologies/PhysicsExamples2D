using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class ScaleRagdoll : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private RagdollFactory.Ragdoll m_Ragdoll;
    private float m_RagdollScale;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 6f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 4.5f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_RagdollScale = 1f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        if (m_Ragdoll.IsCreated)
            m_Ragdoll.Dispose();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Ragdoll Scale.
            var ragdollScale = root.Q<Slider>("ragdoll-scale");
            ragdollScale.value = m_RagdollScale;
            ragdollScale.RegisterValueChangedCallback(evt =>
            {
                m_RagdollScale = evt.newValue;
                m_Ragdoll.Rescale(m_RagdollScale);
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
        if (m_Ragdoll.IsCreated)
            m_Ragdoll.Dispose();

        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;
        ref var random = ref m_SandboxManager.Random;

        // Ground.
        {
            var groundBody = world.CreateBody();
            bodies.Add(groundBody);

            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(40f, 20f), radius: 0, transform: new PhysicsTransform(Vector2.down * 10f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(20f, 100f), radius: 0, transform: new PhysicsTransform(Vector2.left * 20f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(20f, 100f), radius: 0, transform: new PhysicsTransform(Vector2.right * 20f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(size: new Vector2(40f, 20f), radius: 0, transform: new PhysicsTransform(Vector2.up * 20f)));
        }

        // Ragdoll.
        {
            var ragDollConfiguration = new RagdollFactory.Configuration
            {
                ScaleRange = new Vector2(m_RagdollScale, m_RagdollScale),
                AngularImpulseRange = new Vector2(m_RagdollScale * 10f, m_RagdollScale * 10f),
                JointFrequency = 1f,
                JointDamping = 0.5f,
                JointFriction = 0.03f,
                GravityScale = 1f,
                ContactBodyLayer = 2,
                ContactFeetLayer = 1,
                ContactGroupIndex = 1,
                ColorProvider = m_SandboxManager,
                FastCollisionsAllowed = true,
                TriggerEvents = false,
                EnableLimits = true,
                EnableMotor = true
            };

            m_Ragdoll = RagdollFactory.Spawn(world, Vector2.up * 5f, ragDollConfiguration, true, ref random, Allocator.Persistent);
            foreach (var body in m_Ragdoll)
                bodies.Add(body);
        }
    }
}