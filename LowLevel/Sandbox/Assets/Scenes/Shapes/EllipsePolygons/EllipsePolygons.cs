using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class EllipsePolygons : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private int m_ColumnCount;
    private int m_RowCount;
    private float m_Friction;
    private float m_Bounciness;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 15f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 10f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;
        
        m_ColumnCount = 10;
        m_RowCount = 10;
        m_Friction = 0.6f;
        m_Bounciness = 0f;
        
        SetupOptions();

        SetupScene();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;
        
        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );
            
            var columnCount = root.Q<SliderInt>("column-count");
            columnCount.value = m_ColumnCount;
            columnCount.RegisterValueChangedCallback(evt =>
            {
                m_ColumnCount = evt.newValue;
                SetupScene();
            });

            var rowCount = root.Q<SliderInt>("row-count");
            rowCount.value = m_RowCount;
            rowCount.RegisterValueChangedCallback(evt =>
            {
                m_RowCount = evt.newValue;
                SetupScene();
            });
            
            // Friction.
            var friction = root.Q<Slider>("friction");
            friction.value = m_Friction;
            friction.RegisterValueChangedCallback(evt =>
            {
                m_Friction = evt.newValue;
                SetupScene();
            });

            // Bounciness.
            var bounciness = root.Q<Slider>("bounciness");
            bounciness.value = m_Bounciness;
            bounciness.RegisterValueChangedCallback(evt =>
            {
                m_Bounciness = evt.newValue;
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
        
        ref var random = ref m_SandboxManager.Random;
        
        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;
        
        // Ground.
        {
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            
            var body = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            bodies.Add(body);

            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(40f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, -1f), PhysicsRotate.identity)), shapeDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(40f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, 101f), PhysicsRotate.identity)), shapeDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 100f), radius: 0f, new PhysicsTransform(new Vector2(19f, 50f), PhysicsRotate.identity)), shapeDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 100f), radius: 0f, new PhysicsTransform(new Vector2(-19f, 50f), PhysicsRotate.identity)), shapeDef);
        }

        // Rounded Polygons.
        {
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_Friction, bounciness = m_Bounciness, rollingResistance = 0.2f } };
            
            var y = 2.0f;
            for (var i = 0; i < m_RowCount; ++i, y += 1.0f)
            {
                var x = m_ColumnCount * -0.5f + 0.5f;
                for (var j = 0; j < m_ColumnCount; ++j, x += 1.0f)
                {
                    bodyDef.position = new Vector2(x, y);
                    var body = world.CreateBody(bodyDef);
                    bodies.Add(body);
                    
                    shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;

                    // Create Ellipse Polygon.
                    using var vertices = new NativeList<Vector2>(Allocator.Temp)
                    {
                        new(0.0f, -0.25f),
                        new(0.0f, 0.25f),
                        new(0.05f, 0.075f),
                        new(-0.05f, 0.075f),
                        new(0.05f, -0.075f),
                        new(-0.05f, -0.075f)
                    };
                    body.CreateShape(PolygonGeometry.Create(vertices: vertices.AsArray(), radius: 0.2f), shapeDef);
                }
            }            
        }
    }
}
