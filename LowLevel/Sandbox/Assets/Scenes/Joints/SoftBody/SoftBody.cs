using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class SoftBody : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private int m_BodySides;
    private float m_BodyScale;
    private float m_JointFrequency;
    private float m_JointDamping;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 5f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 0f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_BodySides = 10;
        m_BodyScale = 2f;
        m_JointFrequency = 7f;
        m_JointDamping = 0f;

        SetupOptions();

        SetupScene();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Body Sides.
            var bodySides = root.Q<SliderInt>("body-sides");
            bodySides.value = m_BodySides;
            bodySides.RegisterValueChangedCallback(evt =>
            {
                m_BodySides = evt.newValue;
                SetupScene();
            });

            // Body Scale.
            var bodyScale = root.Q<Slider>("body-scale");
            bodyScale.value = m_BodyScale;
            bodyScale.RegisterValueChangedCallback(evt =>
            {
                m_BodyScale = evt.newValue;
                SetupScene();
            });

            // Joint Frequency.
            var jointFrequency = root.Q<Slider>("joint-frequency");
            jointFrequency.value = m_JointFrequency;
            jointFrequency.RegisterValueChangedCallback(evt =>
            {
                m_JointFrequency = evt.newValue;
                SetupScene();
            });

            // Joint Damping.
            var jointDamping = root.Q<Slider>("joint-damping");
            jointDamping.value = m_JointDamping;
            jointDamping.RegisterValueChangedCallback(evt =>
            {
                m_JointDamping = evt.newValue;
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

        var world = PhysicsWorld.defaultWorld;

        var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);

        // Ground.
        {
            var bodyDef = PhysicsBodyDefinition.defaultDefinition;
            var body = world.CreateBody(bodyDef);

            using var vertices = new NativeList<Vector2>(4, Allocator.Temp)
            {
                new(-5.5f, 4.5f),
                new(5.5f, 4.5f),
                new(5.5f, -4.5f),
                new(-5.5f, -4.5f)
            };
            body.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        // Soft Body.
        {
            using var donut = SoftbodyFactory.SpawnDonut(world, m_SandboxManager, Vector2.zero, m_BodySides, m_BodyScale, triggerEvents: false, jointFrequency: m_JointFrequency, jointDamping: m_JointDamping);
        }
    }
}