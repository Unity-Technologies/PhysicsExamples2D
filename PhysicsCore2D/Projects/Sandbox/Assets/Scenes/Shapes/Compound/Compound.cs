using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class Compound : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsWorld m_PhysicsWorld;
    private ControlsMenu.CustomButton m_IntrudeButton;
    
    private PhysicsBody m_Table1;
    private PhysicsBody m_Table2;
    private PhysicsBody m_Ship1;
    private PhysicsBody m_Ship2;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 15;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 9f);

        // Set controls.
        {
	        m_IntrudeButton = m_SandboxManager.ControlsMenu[0];
	        m_IntrudeButton.Set("Intrude");
	        m_IntrudeButton.button.clickable.clicked += IntrudeShape;
        }
        
        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);

        m_PhysicsWorld = PhysicsWorld.defaultWorld;
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
	    // Unregister.
	    m_IntrudeButton.button.clickable.clicked -= IntrudeShape;

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

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        // Ground.
        {
            var groundBody = m_PhysicsWorld.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
	            new(-25f, 0f),
	            new(-25f, 23f),
	            new(25f, 23f),
	            new(25f, 0f)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }
        
		// Table 1.
		{
			m_Table1 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-15f, 1f) });

			m_Table1.CreateShape(PolygonGeometry.CreateBox(new Vector2(6f, 1f), 0f, new Vector2(0f, 3.5f)));
			m_Table1.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 3f), 0f, new Vector2(-2.5f, 1.5f)));
			m_Table1.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 3f), 0f, new Vector2(2.5f, 1.5f)));
		}

		// Table 2.
		{
			m_Table2 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-5f, 1f) });

			m_Table2.CreateShape(PolygonGeometry.CreateBox(new Vector2(6f, 1f), 0f, new Vector2(0f, 3.5f)));
			m_Table2.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 4f), 0f, new Vector2(-2.5f, 2f)));
			m_Table2.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 4f), 0f, new Vector2(2.5f, 2f)));
		}

		// Spaceship 1.
		{
			m_Ship1 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(5f, 1f) });

			{
				using var vertices = new NativeList<Vector2>(Allocator.Temp)
				{
					new(-2f, 0f),
					new(0f, 4f / 3f),
					new(0f, 4f)
				};
				m_Ship1.CreateShape(PolygonGeometry.Create(vertices.AsArray()));
			}

			{
				using var vertices = new NativeList<Vector2>(Allocator.Temp)
				{
					new(2f, 0f),
					new(0f, 4f / 3f),
					new(0f, 4f)
				};
				m_Ship1.CreateShape(PolygonGeometry.Create(vertices.AsArray()));
			}
		}

		// Spaceship 2.
		{
			m_Ship2 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(15f, 1f) });

			{
				using var vertices = new NativeList<Vector2>(Allocator.Temp)
				{
					new(-2f, 0f),
					new(1f, 2f),
					new(0f, 4f)
				};
				m_Ship2.CreateShape(PolygonGeometry.Create(vertices.AsArray()));
			}

			{
				using var vertices = new NativeList<Vector2>(Allocator.Temp)
				{
					new(2f, 0f),
					new(-1f, 2f),
					new(0f, 4f)
				};
				m_Ship2.CreateShape(PolygonGeometry.Create(vertices.AsArray()));
			}
		}		
    }

    private void IntrudeShape()
    {
	    // Table 1 intrusion.
	    {
		    var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = m_Table1.position, rotation = m_Table1.rotation });
		    body.CreateShape(PolygonGeometry.CreateBox(new Vector2(8f, 0.2f), 0f, new Vector2(0f, 3.0f)));
	    }

	    // Table 2 intrusion.
	    {
		    var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = m_Table2.position, rotation = m_Table2.rotation });
		    body.CreateShape(PolygonGeometry.CreateBox(new Vector2(8f, 0.2f), 0f, new Vector2(0f, 3.0f)));
	    }

	    // Ship 1 intrusion.
	    {
		    var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = m_Ship1.position, rotation = m_Ship1.rotation });
		    body.CreateShape(new CircleGeometry { center = new Vector2(0f, 2f), radius = 0.5f });
	    }

	    // Ship 2 intrusion.
	    {
		    var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = m_Ship2.position, rotation = m_Ship2.rotation });
		    body.CreateShape(new CircleGeometry { center = new Vector2(0f, 2f), radius = 0.5f });
	    }	    
    }

    private void Update()
    {
	    DrawBodyBounds(m_Table1, Color.red);
	    DrawBodyBounds(m_Table2, Color.cyan);
	    DrawBodyBounds(m_Ship1, Color.red);
	    DrawBodyBounds(m_Ship2, Color.cyan);
    }

    private void DrawBodyBounds(PhysicsBody body, Color color)
    {
	    if (!body.isValid)
		    return;

	    // Calculate the body AABB.
	    var aabb =  body.GetAABB();
	    
	    // Draw the AABB.
	    m_PhysicsWorld.DrawBox(aabb.center, aabb.extents * 2f, 0f, color);
    }
}
