using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

public class Shooter : MonoBehaviour
{
	public PhysicsShape.ContactFilter BatchFilter = PhysicsShape.ContactFilter.defaultFilter;
	
	private SandboxManager m_SandboxManager;
	private SceneManifest m_SceneManifest;
	private UIDocument m_UIDocument;
	private CameraManipulator m_CameraManipulator;
	
	private int m_BatchCount = 100;
	private float m_BatchDelay = 0.1f;
	private float m_GravityScale = 5f;
	private float m_BatchSpread = 10.0f;
	private Vector2 m_BatchSpeed = new(10f, 50f);
	private Vector2 m_BatchOffset = new (0.8f, 2f);
	private Vector2 m_BatchLength = new(0.01f, 0.1f); 
	private Vector2 m_BatchRadius = new (0.01f, 0.1f);
	
	private float m_Time;
	private float m_BatchDelayTime;
	private Vector2 m_FireDirection;
	private Vector2 m_OldGravity;
		
	private void OnEnable()
	{
		m_SandboxManager = FindFirstObjectByType<SandboxManager>();
		m_SceneManifest = FindFirstObjectByType<SceneManifest>();
		m_UIDocument = GetComponent<UIDocument>();

		m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
		m_CameraManipulator.CameraSize = 12f;
		m_CameraManipulator.CameraStartPosition = Vector2.zero;

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
	}

	private void SetupOptions()
	{
		var root = m_UIDocument.rootVisualElement;

		{
			// Menu Region (for camera manipulator).
			var menuRegion = root.Q<VisualElement>("menu-region");
			menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
			menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

			// Batch Count.
			var batchCount = root.Q<SliderInt>("batch-count");
			batchCount.value = m_BatchCount;
			batchCount.RegisterValueChangedCallback(evt =>
			{
				m_BatchCount = evt.newValue;
			});

			// Batch Delay.
			var batchDelay = root.Q<Slider>("batch-delay");
			batchDelay.value = m_BatchDelay;
			batchDelay.RegisterValueChangedCallback(evt => {
				m_BatchDelay = evt.newValue;
			});
			
			// Batch Spread.
			var batchSpread = root.Q<Slider>("batch-spread");
			batchSpread.value = m_BatchSpread;
			batchSpread.RegisterValueChangedCallback(evt => { m_BatchSpread = evt.newValue; });
			
			// Batch Speed.
			var batchSpeed = root.Q<MinMaxSlider>("batch-speed");
			batchSpeed.value = m_BatchSpeed;
			batchSpeed.RegisterValueChangedCallback(evt => { m_BatchSpeed = evt.newValue; });

			// Batch Offset.
			var batchOffset = root.Q<MinMaxSlider>("batch-offset");
			batchOffset.value = m_BatchOffset;
			batchOffset.RegisterValueChangedCallback(evt => { m_BatchOffset = evt.newValue; });

			// Batch Length.
			var batchLength = root.Q<MinMaxSlider>("batch-length");
			batchLength.value = m_BatchLength;
			batchLength.RegisterValueChangedCallback(evt => { m_BatchLength = evt.newValue; });

			// Batch Radius.
			var batchRadius = root.Q<MinMaxSlider>("batch-radius");
			batchRadius.value = m_BatchRadius;
			batchRadius.RegisterValueChangedCallback(evt => { m_BatchRadius = evt.newValue; });
			
			// Gravity Scale.
			var gravityScale = root.Q<Slider>("gravity-scale");
			gravityScale.value = m_GravityScale;
			gravityScale.RegisterValueChangedCallback(evt => { m_GravityScale = evt.newValue; });
			
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
	}

	private void Update()
	{
		// Draw Fire Direction.
		var segment = new SegmentGeometry { point1 = Vector2.zero, point2 = m_FireDirection };
		if (segment.isValid)
		{
			var world = PhysicsWorld.defaultWorld;
			world.DrawGeometry(segment, PhysicsTransform.identity, Color.azure);
		}
	}
	
	private void OnPreSimulation(PhysicsWorld world, float timeStep)
	{
		// Destroy batch contacts.
		DestroyBatch();
		
		// Batch Creation.
		{
			m_Time += Time.deltaTime;
			var rotation1 = new PhysicsRotate(m_Time * 0.5f);
			var rotation2 = new PhysicsRotate(m_Time);
			m_FireDirection = new Vector2(rotation1.direction.x, rotation2.direction.y);
			var fireAngle = PhysicsMath.Atan2(m_FireDirection.y, m_FireDirection.x);
			
			// Are we ready to create a batch?
			m_BatchDelayTime += Time.deltaTime;
			if (m_BatchDelayTime > m_BatchDelay)
			{
				// Yes, so reset batch delay.
				m_BatchDelayTime = 0f;

				// Create the Batch.
				{
					var worldBodies = m_SandboxManager.Bodies;
					ref var random = ref m_SandboxManager.Random;
					
					var capsuleRadius = random.NextFloat(m_BatchRadius.x, m_BatchRadius.y);
					var capsuleLength = capsuleRadius + random.NextFloat(m_BatchLength.x, m_BatchLength.y) * 0.5f;
					var capsuleGeometry = new CapsuleGeometry
					{
						center1 = Vector2.left * capsuleLength,
						center2 = Vector2.right * capsuleLength,
						radius = capsuleRadius
					};
					
					var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, gravityScale = m_GravityScale, fastCollisions = true };
					var shapeDef = new PhysicsShapeDefinition { contactFilter = BatchFilter, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0.2f } };
					
					// Fire all the projectiles.
					var definitions = new NativeArray<PhysicsBodyDefinition>(m_BatchCount, Allocator.Temp);
					for (var i = 0; i < m_BatchCount; ++i)
					{
						// Calculate the fire spread.
						var halfSpread = m_BatchSpread * 0.5f;
						var fireDirection = new PhysicsRotate(math.radians(random.NextFloat(-halfSpread, halfSpread)) + fireAngle).direction;
						var fireOffset = random.NextFloat(m_BatchOffset.x, m_BatchOffset.y);
						var fireSpeed = random.NextFloat(m_BatchSpeed.x, m_BatchSpeed.y);

						// Create the projectile body.
						bodyDef.position = fireDirection * fireOffset;
						bodyDef.rotation = new PhysicsRotate(random.NextFloat(-3f, 3f));
						bodyDef.linearVelocity = fireDirection * fireSpeed;
            
						definitions[i] = bodyDef;
					}

					// Create the bodies.
					using var bodies = world.CreateBodyBatch(definitions);

					// Create the capsules.
					for (var i = 0; i < m_BatchCount; ++i)
					{
						// Create the projectile shape.
						shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
						var body = bodies[i];
						body.CreateShape(capsuleGeometry, shapeDef);
						
						worldBodies.Add(body);
					}
					
					// Dispose.
					definitions.Dispose();
				}
			}
		}		
	}

	private void DestroyBatch()
	{
		// Fetch hit events and destroy any dynamic bodies in the event.
		var world = PhysicsWorld.defaultWorld;
		var beginEvents = world.contactBeginEvents;

		// Finish if no events.
		if (beginEvents.Length == 0)
			return;
		
		// Create a list for the batch to destroy.
		var bodyBatch = new NativeList<PhysicsBody>(initialCapacity: beginEvents.Length, Allocator.Temp);

		var targetCategory = BatchFilter.categories;
		
		// Iterate all the begin events looking for the dynamic bodies.
		foreach (var beginEvent in beginEvents)
		{
			var shapeA = beginEvent.shapeA;
			if (shapeA.isValid && shapeA.contactFilter.categories == targetCategory)
			{
				bodyBatch.Add(shapeA.body);
				continue;
			}
			
			var shapeB = beginEvent.shapeB;
			if (shapeB.isValid && shapeB.contactFilter.categories == targetCategory)
				bodyBatch.Add(shapeB.body);
		}

		// Destroy the bodies as a batch.
		if (bodyBatch.Length > 0)
			PhysicsBody.DestroyBatch(bodyBatch.AsArray());
		
		// Dispose.
		bodyBatch.Dispose();
	}
}
