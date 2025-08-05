using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class BounceRagdolls : MonoBehaviour
{
	private SandboxManager m_SandboxManager;
	private SceneManifest m_SceneManifest;
	private UIDocument m_UIDocument;
	private CameraManipulator m_CameraManipulator;

	private float m_Time;
	private float m_UpdateTime;
	private int m_RagdollCount;

	private float m_UpdatePeriod = 0.5f;
	private int m_MaxRagdollCount = 25;
	private float m_GravityScale = 5f; 

	private Vector2 m_OldGravity;
		
	private const bool FastCollisions = true;
	
	private void OnEnable()
	{
		m_SandboxManager = FindFirstObjectByType<SandboxManager>();
		m_SceneManifest = FindFirstObjectByType<SceneManifest>();
		m_UIDocument = GetComponent<UIDocument>();

		m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
		m_CameraManipulator.CameraSize = 12f;
		m_CameraManipulator.CameraPosition = Vector2.zero;

		// Set up the scene reset action.
		m_SandboxManager.SceneResetAction = SetupScene;
		
		// Set Overrides.
		m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.Off);
		
		m_OldGravity = PhysicsWorld.defaultWorld.gravity;
		
		SetupOptions();

		SetupScene();

		PhysicsEvents.PreSimulate += OnPreSimulation;
	}

	private void OnDisable()
	{
		var world = PhysicsWorld.defaultWorld;
		world.gravity = m_OldGravity;
		
		PhysicsEvents.PreSimulate -= OnPreSimulation;

		m_SandboxManager.ResetOverrideDrawOptions();
	}

	private void SetupOptions()
	{
		var root = m_UIDocument.rootVisualElement;

		{
			// Menu Region (for camera manipulator).
			var menuRegion = root.Q<VisualElement>("menu-region");
			menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
			menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

			// Update Period.
			var updatePeriod = root.Q<Slider>("update-period");
			updatePeriod.value = m_UpdatePeriod;
			updatePeriod.RegisterValueChangedCallback(evt => {
				m_UpdatePeriod = evt.newValue;
				m_UpdateTime = m_UpdatePeriod;
				SetupScene();
			});
            
			// Spawn Count.
			var ragdollCount = root.Q<SliderInt>("ragdoll-count");
			ragdollCount.value = m_MaxRagdollCount;
			ragdollCount.RegisterValueChangedCallback(evt =>
			{
				m_MaxRagdollCount = evt.newValue;
				SetupScene();
			});

			// Gravity Scale.
			var gravityScale = root.Q<Slider>("gravity-scale");
			gravityScale.value = m_GravityScale;
			gravityScale.RegisterValueChangedCallback(evt => {
				m_GravityScale = evt.newValue;
				m_UpdateTime = m_UpdatePeriod;
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

		m_RagdollCount = 0;
		m_UpdateTime = m_UpdatePeriod;
		m_Time = 0f;
		
		// Ground Body. 
		var groundBody = world.CreateBody();
		bodies.Add(groundBody);

		var extent = 10f;
		using var extentPoints = new NativeList<Vector2>(Allocator.Temp)
		{
			new(-extent, extent),			
			new(extent, extent),
			new(extent, -extent),
			new(-extent, -extent)
		};

		// Ground Box.
		{
			groundBody.CreateChain(
				geometry: new ChainGeometry(extentPoints.AsArray()),
				definition: new PhysicsChainDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 1.3f } }
			);
		}

		// Circles.
		{
			var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 2.0f } };
			groundBody.CreateShape(new CircleGeometry { radius = 3f }, shapeDef);

			foreach (var point in extentPoints)
				groundBody.CreateShape(new CircleGeometry { center = point, radius = 2f }, shapeDef);
		}
	}

	private void Update()
	{
		var world = PhysicsWorld.defaultWorld;

		var segment = new SegmentGeometry { point1 = Vector2.zero, point2 = world.gravity * 3f };
		if (segment.isValid)
			world.DrawGeometry(segment, PhysicsTransform.identity, Color.orangeRed);
	}

	private void OnPreSimulation(PhysicsWorld world, float timeStep)
	{
		// Finish if not the sandbox world.
		if (!world.isDefaultWorld)
			return;

		// Update Gravity.
		{
			m_Time += timeStep;
			var rotation1 = new PhysicsRotate(m_Time * 0.5f);
			var rotation2 = new PhysicsRotate(m_Time);
			world.gravity = new Vector2(rotation1.direction.x, rotation2.direction.y) * m_GravityScale;
		}
		
		// Update Period.
		m_UpdateTime += timeStep;
		if (m_UpdateTime < m_UpdatePeriod)
			return;
		
		m_UpdateTime = 0f;
		
		// Spawn Ragdoll.
		if (m_RagdollCount >= m_MaxRagdollCount)
			return;
		
		m_RagdollCount++;
		
        var ragDollConfiguration = new RagdollFactory.Configuration
        {
	        ScaleRange = new Vector2(1.75f, 1.75f),
	        JointFrequency = 1f,
	        JointDamping = 0.1f,
	        JointFriction = 0.0f,
	        GravityScale = 1f,
	        ContactBodyLayer = 2,
	        ContactFeetLayer = 1,
	        ContactGroupIndex = 1,
	        ColorProvider = m_SandboxManager,
	        FastCollisionsAllowed = FastCollisions,
	        TriggerEvents = false,
	        EnableLimits = true,
	        EnableMotor = true
        };

        ref var random = ref m_SandboxManager.Random;
        var position = new Vector2(random.NextFloat(-2f, 2f), random.NextFloat(-2f, 0f));
        
        using var ragdoll = RagdollFactory.Spawn(world, position, ragDollConfiguration, true, ref random);
        var bodies = m_SandboxManager.Bodies;
        foreach (var body in ragdoll)
        {
	        bodies.Add(body);
        }
	}
}
