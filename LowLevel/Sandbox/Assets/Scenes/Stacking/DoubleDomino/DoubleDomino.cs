using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class DoubleDomino : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private const int DominoCount = 20;
    
    private Vector2 m_OldGravity;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 6f;
        m_CameraManipulator.CameraStartPosition = new Vector2(-0.5f, 0f);

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
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );

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
        
        // Create the domino shelves.
        for (var n = 0; n < 5; ++n)
            CreateDominoShelf((n % 2) == 0, 2.5f - n * 2f);
    }

    private void CreateDominoShelf(bool tipRight, float positionY)
    {
        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;

        {
            var bodyDef = new PhysicsBodyDefinition { position = new Vector2(0f, positionY) };
            var body = world.CreateBody(bodyDef);
            bodies.Add(body);
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.dimGray } };
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(200f, 0.5f)), shapeDef);
        }

        {
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.6f } };
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(0.25f, 1f));

            var x = -0.5f * DominoCount;
            for (var i = 0; i < DominoCount; ++i, x += 1f)
            {
                bodyDef.position = new Vector2(x, positionY + 0.75f);
                var dominoBody = world.CreateBody(bodyDef);
                bodies.Add(dominoBody);
                
                // Fetch the appropriate shape color.
                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                
                dominoBody.CreateShape(boxGeometry, shapeDef);

                if (tipRight && i == 0)
                    dominoBody.ApplyLinearImpulse(new Vector2(0.2f, 0f), new Vector2(x, positionY + 1f));
                else if (!tipRight && i == (DominoCount-1))
                    dominoBody.ApplyLinearImpulse(new Vector2(-0.2f, 0f), new Vector2(x, positionY + 1f));
            }
        }        
    }
}
