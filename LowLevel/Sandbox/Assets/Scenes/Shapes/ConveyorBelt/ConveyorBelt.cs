using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class ConveyorBelt : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsWorld m_PhysicsWorld;

    private const int SpawnCount = 10;
    private ControlsMenu.CustomButton m_SpawnButton;
    private PhysicsBody m_ConveyorBeltBody;
    private PhysicsShape m_ConveyorBeltShape;
    
    private float m_ConveyorSpeed;
    private float m_ConveyorAngle;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 12;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 7f);

        // Set controls.
        {
	        var controlsMenu = m_SandboxManager.ControlsMenu;
	        controlsMenu.gameObject.SetActive(true);

	        m_SpawnButton = m_SandboxManager.ControlsMenu[0];
	        m_SpawnButton.Set("Spawn");
	        m_SpawnButton.button.clickable.clicked += SpawnDebris;
        }
        
        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_PhysicsWorld = PhysicsWorld.defaultWorld;

        m_ConveyorSpeed = 3f;
        m_ConveyorAngle = 0f;
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
	    m_SpawnButton.button.clickable.clicked -= SpawnDebris;
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Conveyor Speed.
            var conveyorSpeed = root.Q<Slider>("conveyor-speed");
            conveyorSpeed.value = m_ConveyorSpeed;
            conveyorSpeed.RegisterValueChangedCallback(evt =>
            {
	            m_ConveyorSpeed = evt.newValue;
	            UpdateConveyorSpeed();
            });

            // Conveyor Angle.
            var conveyorAngle = root.Q<Slider>("conveyor-angle");
            conveyorAngle.value = m_ConveyorAngle;
            conveyorAngle.RegisterValueChangedCallback(evt =>
            {
	            m_ConveyorAngle = evt.newValue;
	            UpdateConveyorAngle();
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

        // Ground.
        {
            var body = m_PhysicsWorld.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
	            new(-20f, 0f),
	            new(-20f, 23f),
	            new(20f, 23f),
	            new(20f, 0f)
            };
            body.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }
        
		// Platform.
		{
			m_ConveyorBeltBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { position = Vector2.up * 8f, rotation = new PhysicsRotate(PhysicsMath.ToRadians(m_ConveyorAngle)) });

			var geometry = PolygonGeometry.CreateBox(new Vector2(20f, 0.5f), 0.25f);
			var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.8f, tangentSpeed = m_ConveyorSpeed } };
			m_ConveyorBeltShape = m_ConveyorBeltBody.CreateShape(geometry, shapeDef);
		}
		
		// Spawn Debris.
		{
			SpawnDebris();
		}
    }

    private void SpawnDebris()
    {
	    ref var random = ref m_SandboxManager.Random;

	    for (var n = 0; n < SpawnCount; ++n)
	    {
		    var spawnPosition = new Vector2(random.NextFloat(-5f, 5f), random.NextFloat(9f, 20f));
		    var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = spawnPosition, rotation = new PhysicsRotate(random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI)) });

		    var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState } };

		    if (random.NextBool())
		    {
			    var scaleX = random.NextFloat(0.05f, 1.0f);
			    var scaleY = random.NextFloat(0.05f, 1.0f);
			    var radius = random.NextFloat(0f, 0.2f);
			    body.CreateShape(PolygonGeometry.CreateBox(new Vector2(scaleX, scaleY), radius), shapeDef);
		    }
		    else
		    {
			    var scale = random.NextFloat(0.2f, 0.6f);
			    var radius = random.NextFloat(0.2f, 0.4f);
			    body.CreateShape(new CapsuleGeometry { center1 = Vector2.left * scale, center2 = Vector2.right * scale, radius = radius }, shapeDef);
		    }
	    }
    }

    private void UpdateConveyorAngle()
    {
	    // Update the conveyor angle.
	    m_ConveyorBeltBody.rotation = new PhysicsRotate(PhysicsMath.ToRadians(m_ConveyorAngle));
    }
    
    private void UpdateConveyorSpeed()
    {
	    // Update the tangent speed.
	    var surfaceMaterial = m_ConveyorBeltShape.surfaceMaterial;
	    surfaceMaterial.tangentSpeed = m_ConveyorSpeed;
	    m_ConveyorBeltShape.surfaceMaterial = surfaceMaterial;
    }
}
