using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class ManyTumblers : MonoBehaviour
{
   
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private int m_RowCount;
    private int m_ColumnCount;
    private int m_SpawnCount;
    private float m_AngularVelocity;

    private int m_CurrentSpawnCounter;
    private const float m_SpawnPeriod = 0.1f;
    private float m_SpawnTime;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 90f;
        m_CameraManipulator.CameraStartPosition = new Vector4(-4f, -4f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;
        
        m_RowCount = 15;
        m_ColumnCount = 15;
        m_SpawnCount = 10;
        m_AngularVelocity = 1f;

        SetupOptions();

        SetupScene();
    }

    private void Update()
    {
        // Finish if the world is paused.
        if (m_SandboxManager.WorldPaused)
            return;
        
        // Limit spawn count.
        if (m_CurrentSpawnCounter >= m_SpawnCount)
            return;
        
        // Limit spawn period.
        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0f)
            return;
        
        m_SpawnTime = m_SpawnPeriod;
        m_CurrentSpawnCounter++;

        // Spawn debris.
        SpawnDebris();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;
        
        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );

            // Row Count.
            var rowCount = root.Q<SliderInt>("row-count");
            rowCount.value = m_RowCount;
            rowCount.RegisterValueChangedCallback(evt =>
            {
                m_RowCount = evt.newValue;
                SetupScene();
            });
            
            // Row Count.
            var columnCount = root.Q<SliderInt>("column-count");
            columnCount.value = m_ColumnCount;
            columnCount.RegisterValueChangedCallback(evt =>
            {
                m_ColumnCount = evt.newValue;
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
            
            // Row Count.
            var spawnCount = root.Q<SliderInt>("spawn-count");
            spawnCount.value = m_SpawnCount;
            spawnCount.RegisterValueChangedCallback(evt =>
            {
                m_SpawnCount = evt.newValue;
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

        m_CurrentSpawnCounter = 0;
        
        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;
        
        // Tumblers.
        {
            var x = -4.0f * m_ColumnCount;
            for (var i = 0; i < m_ColumnCount; ++i, x += 8f)
            {
                var y = -4.0f * m_RowCount;
                for (var j = 0; j < m_RowCount; ++j, y += 8f)
                {
                    var position = new Vector2(x, y);
                    {
                        var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Kinematic, position = position, angularVelocity = m_AngularVelocity };
                        var body = world.CreateBody(bodyDef);
                        bodies.Add(body);

                        var shapeDef = new PhysicsShapeDefinition { density = 50f, surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState } };

                        body.CreateShape(
                            PolygonGeometry.CreateBox(new Vector2(0.5f, 4f), 0f, new PhysicsTransform(new Vector2(2f, 0f), PhysicsRotate.identity)),
                            shapeDef);
                        body.CreateShape(
                            PolygonGeometry.CreateBox(new Vector2(0.5f, 4f), 0f, new PhysicsTransform(new Vector2(-2f, 0f), PhysicsRotate.identity)),
                            shapeDef);
                        body.CreateShape(
                            PolygonGeometry.CreateBox(new Vector2(4f, 0.5f), 0f, new PhysicsTransform(new Vector2(0f, 2f), PhysicsRotate.identity)),
                            shapeDef);
                        body.CreateShape(
                            PolygonGeometry.CreateBox(new Vector2(4f, 0.5f), 0f, new PhysicsTransform(new Vector2(0f, -2f), PhysicsRotate.identity)),
                            shapeDef);
                    }
                }
            }
        }
    }

    private void SpawnDebris()
    {
        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;
        
        var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(-0.1f, 0.0f), center2 = new Vector2(0.1f, 0.0f), radius = 0.075f };
        
        var x = -4.0f * m_ColumnCount;
        for (var i = 0; i < m_ColumnCount; ++i, x += 8f)
        {
            var y = -4.0f * m_RowCount;
            for (var j = 0; j < m_RowCount; ++j, y += 8f)
            {
                bodyDef.position = new Vector2(x, y);
                var body = world.CreateBody(bodyDef);
                bodies.Add(body);

                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                body.CreateShape(capsuleGeometry, shapeDef);
            }
        }
    }
}
