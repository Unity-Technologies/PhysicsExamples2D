using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class LargeKinematic : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private int m_GridSize;
    private float m_GridSpacing;
    private float m_AngularVelocity;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 80f;
        m_CameraManipulator.CameraPosition = Vector2.zero;

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);
        
        m_GridSize = 100;
        m_GridSpacing = 0f;
        m_AngularVelocity = 90f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
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

            // Grid Size.
            var gridSize = root.Q<SliderInt>("grid-size");
            gridSize.value = m_GridSize;
            gridSize.RegisterValueChangedCallback(evt =>
            {
                m_GridSize = evt.newValue;
                SetupScene();
            });

            // Grid Spacing.
            var gridSpacing = root.Q<Slider>("grid-spacing");
            gridSpacing.value = m_GridSpacing;
            gridSpacing.RegisterValueChangedCallback(evt =>
            {
                m_GridSpacing = evt.newValue;
                SetupScene();
            });

            // Angular Velocity.
            var angularVelocity = root.Q<Slider>("angular-velocity");
            angularVelocity.value = m_AngularVelocity;
            angularVelocity.RegisterValueChangedCallback(evt =>
            {
                m_AngularVelocity = evt.newValue;
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

        // Rotating Kinematic.
        {
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Kinematic, angularVelocity = m_AngularVelocity };
            var shapeDef = new PhysicsShapeDefinition { startMassUpdate = false };

            var body = world.CreateBody(bodyDef);

            const float grid = 1f;
            var gridBox = new Vector2(grid, grid);
            var span = m_GridSize / 2;

            for (var i = -span; i < span; ++i)
            {
                var y = i * (grid + m_GridSpacing);
                for (var j = -span; j < span; ++j)
                {
                    var x = j * (grid + m_GridSpacing);

                    shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                    var boxTransform = new PhysicsTransform(new Vector2(x, y), PhysicsRotate.identity);
                    var offsetBox = PolygonGeometry.CreateBox(gridBox, radius: 0f, boxTransform);
                    body.CreateShape(offsetBox, shapeDef);
                }
            }

            // All shapes have been added, so we can efficiently compute the mass properties.
            body.ApplyMassFromShapes();
        }
    }
}