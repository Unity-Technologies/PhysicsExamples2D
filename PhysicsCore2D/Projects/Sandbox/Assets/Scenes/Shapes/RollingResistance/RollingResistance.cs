using System;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class RollingResistance : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private SlopeType m_SlopeType;

    private enum SlopeType
    {
        Uphill,
        Flat,
        Downhill
    }

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 35f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 20f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_SlopeType = SlopeType.Flat;

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

            // Slope Type.
            var slopeType = root.Q<EnumField>("slope-type");
            slopeType.value = m_SlopeType;
            slopeType.RegisterValueChangedCallback(evt =>
            {
                m_SlopeType = (SlopeType)slopeType.value;
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

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Slopes.
        {
            var slopeAngle = m_SlopeType switch
            {
                SlopeType.Uphill => 5f,
                SlopeType.Flat => 0f,
                SlopeType.Downhill => -5f,
                _ => throw new ArgumentOutOfRangeException()
            };

            var circle = new CircleGeometry { radius = 0.5f };
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, linearVelocity = new Vector2(5f, 0f), angularVelocity = -10f };
            var slopeShapeDef = PhysicsShapeDefinition.defaultDefinition;
            var ballShapeDef = PhysicsShapeDefinition.defaultDefinition;

            // Add slopes.
            for (var n = 0; n < 20; ++n)
            {
                // Create Slope.
                var groundBody = world.CreateBody();
                groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-40f, 2f * n), point2 = new Vector2(-40f, 2f * n + 1.5f) }, slopeShapeDef);
                groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-40f, 2f * n), point2 = new Vector2(40f, 2f * n + slopeAngle) }, slopeShapeDef);
                groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(40f, 2f * n + slopeAngle), point2 = new Vector2(40f, 2f * n + slopeAngle + 1.5f) }, slopeShapeDef);

                // Create Ball.
                bodyDef.position = new Vector2(-39.5f, 2f * n + 0.75f);
                var ballBody = world.CreateBody(bodyDef);
                ballShapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                ballShapeDef.surfaceMaterial.rollingResistance = 0.02f * n;
                ballBody.CreateShape(circle, ballShapeDef);
            }
        }
    }
}