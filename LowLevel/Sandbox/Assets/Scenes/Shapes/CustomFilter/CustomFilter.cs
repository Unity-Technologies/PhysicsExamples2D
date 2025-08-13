using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class CustomFilter : MonoBehaviour, PhysicsCallbacks.IContactFilterCallback
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsWorld m_PhysicsWorld;
    private bool m_OldContactFilterCallbacks;
    
    private Button m_SpawnButton;
    private PhysicsBody m_ConveyorBeltBody;
    private PhysicsShape m_ConveyorBeltShape;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 12;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 7f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);

        m_PhysicsWorld = PhysicsWorld.defaultWorld;
        
        // Ensure the contact filter callbacks are on.
        m_OldContactFilterCallbacks = m_PhysicsWorld.contactFilterCallbacks;
        m_PhysicsWorld.contactFilterCallbacks = true;
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Restore global default.
        m_PhysicsWorld.contactFilterCallbacks = m_OldContactFilterCallbacks;
        
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
            var body = m_PhysicsWorld.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
	            new(-20f, 0f),
	            new(-20f, 10f),
	            new(20f, 10f),
	            new(20f, 0f)
            };
            body.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        // Create even/odd bodies.
        {
            var geometry = PolygonGeometry.CreateBox(Vector2.one * 2f, 0.2f);
            var shapeDef = new PhysicsShapeDefinition { contactFilterCallbacks = true };
            
            for (var n = 0; n < 15; ++n)
            {
                var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, gravityScale = 4f, position = new Vector2(-17.5f + n * 2.5f, 4f) });

                var colorGroup = n % 3;

                // Set the color appropriately.
                shapeDef.surfaceMaterial.customColor = colorGroup switch
                {
                    0 => Color.red,
                    1 => Color.green,
                    2 => Color.blue,
                    _ => default
                };

                var shape = body.CreateShape(geometry, shapeDef);
                
                // Set the custom data.
                shape.userData = new PhysicsUserData { intValue = colorGroup, boolValue = true };
                
                // Set the callback target.
                shape.callbackTarget = this;
            }
        }
    }

    // Called when any shapes that are asking for a contact filter potentially come into contact.
    // This must be thread-safe and should NOT perform any write operations to the physics engine!
    public bool OnContactFilter2D(PhysicsEvents.ContactFilterEvent contactFilterEvent)
    {
        // Fetch the user-data from the shapes.
        // NOTE: This is a (thread) safe way to pass custom-data.
        var userData1 = contactFilterEvent.shapeA.userData;
        var userData2 = contactFilterEvent.shapeB.userData;

        // Allow contact if this isn't a filtered shape pair.
        if (!userData1.boolValue || !userData2.boolValue)
            return true;
        
        // Allow contact if in the same "color group".
        return userData1.intValue == userData2.intValue;
    }
}
