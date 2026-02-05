using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
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
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 6f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 4.5f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);
        
        m_RagdollScale = 1f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        if (m_Ragdoll.IsCreated)
            m_Ragdoll.Dispose();
        
        // Reset overrides.
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

            // Ragdoll Scale.
            var ragdollScale = root.Q<Slider>("ragdoll-scale");
            ragdollScale.value = m_RagdollScale;
            ragdollScale.RegisterValueChangedCallback(evt =>
            {
                m_RagdollScale = evt.newValue;
                m_Ragdoll.Rescale(m_RagdollScale);
            });

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

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        ref var random = ref m_SandboxManager.Random;

        // Ground.
        {
            var groundBody = world.CreateBody();

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
        }
    }
}