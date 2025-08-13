using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class Confined : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private int m_GridCount;
    private Vector2 m_OldGravity;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 14f;
        m_CameraManipulator.CameraPosition = new Vector2(0, 10f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_GridCount = 25;
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_OldGravity;
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Grid Count.
            var gridCount = root.Q<SliderInt>("grid-count");
            gridCount.value = m_GridCount;
            gridCount.RegisterValueChangedCallback(evt =>
            {
                m_GridCount = evt.newValue;
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

        var world = PhysicsWorld.defaultWorld;

        // Reset the gravity.
        world.gravity = Vector2.zero;

        // Confining Border.
        {
            var body = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);

            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-10.5f, 0f), center2 = new Vector2(10.5f, 0f), radius = 0.5f }, shapeDef);
            body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-10.5f, 0f), center2 = new Vector2(-10.5f, 20.5f), radius = 0.5f }, shapeDef);
            body.CreateShape(new CapsuleGeometry { center1 = new Vector2(10.5f, 0f), center2 = new Vector2(10.5f, 20.5f), radius = 0.5f }, shapeDef);
            body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-10.5f, 20.5f), center2 = new Vector2(10.5f, 20.5f), radius = 0.5f }, shapeDef);
        }

        // Confined Objects.
        {
            var column = 0;
            var count = 0;

            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
                gravityScale = 0f
            };

            var maxCount = m_GridCount * m_GridCount;
            var circleGeometry = new CircleGeometry { center = new Vector2(0f, 0f), radius = 0.5f };
            while (count < maxCount)
            {
                var row = 0;
                for (var i = 0; i < m_GridCount; ++i)
                {
                    bodyDef.position = new Vector2(-8.75f + column * 18.0f / m_GridCount, 1.5f + row * 18.0f / m_GridCount);
                    var body = world.CreateBody(bodyDef);

                    var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState } };
                    body.CreateShape(circleGeometry, shapeDef);

                    ++count;
                    ++row;
                }

                ++column;
            }
        }
    }
}