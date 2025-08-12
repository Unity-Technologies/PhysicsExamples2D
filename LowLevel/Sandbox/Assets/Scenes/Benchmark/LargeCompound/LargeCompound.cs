using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class LargeCompound : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private int m_CompoundCount = 5;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 100f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 99f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

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

            // Compound Count.
            var compoundCount = root.Q<SliderInt>("compound-count");
            compoundCount.value = m_CompoundCount;
            compoundCount.RegisterValueChangedCallback(evt =>
            {
                m_CompoundCount = evt.newValue;
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

        const int height = 300;

        const float gridSize = 1.0f;
        var gridBoxSize = new Vector2(gridSize, gridSize);

        // Ground.
        {
            var body = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            for (var i = 0; i < height; ++i)
            {
                var y = gridSize * i;
                for (var j = i - 4; j < i; ++j)
                {
                    body.CreateShape(PolygonGeometry.CreateBox(gridBoxSize, radius: 0f, new PhysicsTransform(new Vector2(gridSize * j, y), PhysicsRotate.identity)), shapeDef);
                    body.CreateShape(PolygonGeometry.CreateBox(gridBoxSize, radius: 0f, new PhysicsTransform(new Vector2(-gridSize * j, y), PhysicsRotate.identity)), shapeDef);
                }
            }
        }

        // Large Dynamic Compounds. 
        {
            var span = 50 / m_CompoundCount;

            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { startMassUpdate = false };

            for (var m = 0; m < m_CompoundCount; ++m)
            {
                var bodyY = (200.0f + m * span) * gridSize;

                for (var n = 0; n < m_CompoundCount; ++n)
                {
                    var bodyX = -0.5f * gridSize * m_CompoundCount * span + n * span * gridSize;
                    bodyDef.position = new Vector2(bodyX, bodyY);
                    var body = world.CreateBody(bodyDef);

                    for (var i = 0; i < span; ++i)
                    {
                        var y = i * gridSize;
                        for (var j = 0; j < span; ++j)
                        {
                            var x = gridSize * j;
                            shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                            body.CreateShape(PolygonGeometry.CreateBox(gridBoxSize, radius: 0f, new PhysicsTransform(new Vector2(x, y), PhysicsRotate.identity)), shapeDef);
                        }
                    }

                    // All shapes have been added, so we can efficiently compute the mass properties.
                    body.ApplyMassFromShapes();
                }
            }
        }
    }
}