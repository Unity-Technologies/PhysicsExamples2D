using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class LargePyramid : MonoBehaviour
{
   
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;
    
    private int m_BaseCount;
    private Vector2 m_OldGravity;
    private float m_GravityScale;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 80f;
        m_CameraManipulator.CameraStartPosition = new Vector2(0f, 79f);

        m_BaseCount = 100;
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GravityScale = 1f;

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
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );

            // Base Count.
            var baseCount = root.Q<SliderInt>("base-count");
            baseCount.value = m_BaseCount;
            baseCount.RegisterValueChangedCallback(evt =>
            {
                m_BaseCount = evt.newValue;
                SetupScene();
            });
            
            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.value = m_GravityScale;
            gravityScale.RegisterValueChangedCallback(evt =>
            {
                m_GravityScale = evt.newValue;
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

        // Ground.
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(0f, -1f) });
            bodies.Add(body);

            const float groundLength = 1000f;
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(groundLength, 2f)), PhysicsShapeDefinition.defaultDefinition);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(groundLength, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, groundLength), PhysicsRotate.identity)), PhysicsShapeDefinition.defaultDefinition);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, groundLength), radius: 0f, new PhysicsTransform(new Vector2(groundLength * -0.5f, groundLength * 0.5f), PhysicsRotate.identity)), PhysicsShapeDefinition.defaultDefinition);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, groundLength), radius: 0f, new PhysicsTransform(new Vector2(groundLength * 0.5f, groundLength * 0.5f), PhysicsRotate.identity)), PhysicsShapeDefinition.defaultDefinition);
        }

        // Pyramid.
        {
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, gravityScale = m_GravityScale };
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            
            const float halfHeight = 0.5f;
            const float radius = 0.05f;
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(halfHeight - radius, halfHeight - radius) * 2f, radius);

            const float shift = 1.0f * halfHeight;

            for (var i = 0; i < m_BaseCount; ++i)
            {
                var y = (2.0f * i + 1.0f) * shift;

                for (var j = i; j < m_BaseCount; ++j)
                {
                    var x = ( i + 1.0f ) * shift + 2.0f * ( j - i ) * shift - halfHeight * m_BaseCount;

                    bodyDef.position = new Vector2(x, y);
                    var body = world.CreateBody(bodyDef);
                    bodies.Add(body);

                    shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                    body.CreateShape(boxGeometry, shapeDef);
                }
            }            
        }        
    }
}
