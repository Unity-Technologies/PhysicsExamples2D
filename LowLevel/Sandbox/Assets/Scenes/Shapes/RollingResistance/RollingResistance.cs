using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
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
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
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
        var world = PhysicsWorld.defaultWorld;
        
        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );

            // Slope Type.
            var slopeType = root.Q<EnumField>("slope-type");
            slopeType.value = m_SlopeType;
            slopeType.RegisterValueChangedCallback(evt =>
            {
                m_SlopeType = (SlopeType)slopeType.value;
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
        var bodies = m_SandboxManager.Bodies;
        
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
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, linearVelocity = new Vector2(5f, 0f), angularVelocity = -10f };
            var slopeShapeDef = PhysicsShapeDefinition.defaultDefinition;
            var ballShapeDef = PhysicsShapeDefinition.defaultDefinition;
            
            // Add slopes.
            for (var n = 0; n < 20; ++n)
            {
                // Create Slope.
                var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
                bodies.Add(groundBody);
                groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-40f, 2f * n), point2 = new Vector2(-40f, 2f * n + 1.5f) }, slopeShapeDef);
                groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-40f, 2f * n), point2 = new Vector2(40f, 2f * n + slopeAngle) }, slopeShapeDef);
                groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(40f, 2f * n + slopeAngle), point2 = new Vector2(40f, 2f * n + slopeAngle + 1.5f) }, slopeShapeDef);

                // Create Ball.
                bodyDef.position = new Vector2(-39.5f, 2f * n + 0.75f);
                var ballBody = world.CreateBody(bodyDef);
                bodies.Add(ballBody);
                ballShapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                ballShapeDef.surfaceMaterial.rollingResistance = 0.02f * n;
                ballBody.CreateShape(circle, ballShapeDef);
            }
        }
    }
}
