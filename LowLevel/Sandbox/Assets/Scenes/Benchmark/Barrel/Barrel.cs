using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

public class Barrel : MonoBehaviour
{
	private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private const int MaxRows = 150;
    private const int MaxColumns = 26;

    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Polygon = 2,
        PrimitiveMix = 3,
        Compound = 4,
        Ragdoll = 5
    }

    private ObjectType m_ObjectType;
    private bool m_FastCollisions;

    private void OnEnable()
    {
	    m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 60f;
        m_CameraManipulator.CameraStartPosition = new Vector2(0f, 58f);

        m_ObjectType = ObjectType.Polygon;
        
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

            // Object Type.
            var objectType = root.Q<DropdownField>("object-type");
            objectType.index = (int)m_ObjectType;
            objectType.RegisterValueChangedCallback(evt =>
            {
	            m_ObjectType = Enum.Parse<ObjectType>(evt.newValue);
	            SetupScene();
            });
            
            // Fast Collisions.
            var fastCollisions = root.Q<Toggle>("fast-collisions");
            fastCollisions.value = m_FastCollisions;
            fastCollisions.RegisterValueChangedCallback(evt =>
            {
	            m_FastCollisions = evt.newValue;
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
        
        CreateBarrel();

        var columnCount = MaxColumns;
        var rowCount = MaxRows;
        
        if (m_ObjectType == ObjectType.Compound)
        {
	        columnCount = 20;
        }
        else if (m_ObjectType == ObjectType.Ragdoll)
        {
	        rowCount = 30;
        }            
        
        var shift = 1.15f;
        var centerX = shift * columnCount / 2.0f;
        var centerY = shift / 2.0f;
        var side = -0.1f;
        var extray = 0.5f;
        
        if (m_ObjectType == ObjectType.Compound)
        {
            extray = 0.25f;
            side = 0.25f;
            shift = 2.0f;
            centerX = shift * columnCount / 2.0f - 1.0f;
        }
        else if (m_ObjectType == ObjectType.Ragdoll)
        {
            extray = 0.5f;
            side = 0.55f;
            shift = 2.5f;
            centerX = shift * columnCount / 2.0f;
        }

        var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, fastCollisions = m_FastCollisions };
        if (m_ObjectType == ObjectType.PrimitiveMix)
            bodyDef.angularDamping = 0.3f;

        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.5f } };
        
        var ragDollConfiguration = new SpawnFactory.Ragdoll.Configuration
        {
	        ScaleRange = new Vector2(3.5f, 3.5f),
	        JointHertz = 5f,
	        JointDamping = 0.5f,
	        JointFriction = 0.05f,
	        GravityScale = 1f,
	        ContactBodyLayer = 2,
	        ContactFeetLayer = 1,
	        ContactGroupIndex = 0,
	        FastCollisions = m_FastCollisions,
	        TriggerEvents = false,
	        EnableLimits = true,
	        EnableMotor = true,
	        ColorBodyState = m_SandboxManager.ColorShapeState
        };
        
        var leftGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(-1.0f, 0f), new(0.5f, 1f), new(0f, 2f) }.AsSpan());
        var rightGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(1.0f, 0f), new(-0.5f, 1f), new(0f, 2f) }.AsSpan());

        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;
        
        ref var random = ref m_SandboxManager.Random;
        
		var startY = m_ObjectType == ObjectType.Ragdoll ? 25.0f : 120.0f;

		for (var i = 0; i < columnCount; ++i)
		{
			var x = i * shift - centerX;

			for (var j = 0; j < rowCount; ++j)
			{
				var y = j * ( shift + extray ) + centerY + startY;

				bodyDef.position = new Vector2(x + side, y);
				side = -side;

				// Fetch the appropriate shape color.
				shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
				
				switch (m_ObjectType)
				{
					case ObjectType.Circle:
					{
						var body = world.CreateBody(bodyDef);
						bodies.Add(body);

						CreateCircle(body, shapeDef, ref random);
						continue;
					}
            
					case ObjectType.Capsule:
					{
						var body = world.CreateBody(bodyDef);
						bodies.Add(body);
						
						CreateCapsule(body, shapeDef, ref random);
						continue;
					}
            
					case ObjectType.Polygon:
					{
						var body = world.CreateBody(bodyDef);
						bodies.Add(body);

						CreatePolygon(body, shapeDef, ref random);
						continue;
					}
            
					case ObjectType.PrimitiveMix:
					{
						var body = world.CreateBody(bodyDef);
						bodies.Add(body);

						switch (bodies.Count % 3)
						{
							case 0:
							{
								CreateCircle(body, shapeDef, ref random);
								continue;
							}
							
							case 1:
							{
								CreateCapsule(body, shapeDef, ref random);
								continue;
							}
							
							case 2:
							{
								CreatePolygon(body, shapeDef, ref random);
								continue;
							}

							default:
								continue;
						}
					}
            
					case ObjectType.Compound:
					{
						var body = world.CreateBody(bodyDef);
						bodies.Add(body);
						
						body.CreateShape(leftGeometry, shapeDef);
						body.CreateShape(rightGeometry, shapeDef);
						continue;
					}
            
					case ObjectType.Ragdoll:
					{
						var spawnedItem = SpawnFactory.Ragdoll.SpawnRagdoll(world, bodyDef.position, ragDollConfiguration, true, ref random);
						foreach (var body in spawnedItem.Bodies)
						{
							bodies.Add(body);
						}
						spawnedItem.Dispose();
						continue;
					}
            
					default:
						throw new ArgumentOutOfRangeException();
				}				
			}
		}
    }

    private static void CreateCircle(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
	    var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = random.NextFloat(0.25f, 0.75f) };
	    body.CreateShape(circleGeometry, shapeDef);
    }
    
    private static void CreateCapsule(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
	    var capsuleLength = random.NextFloat(0.25f, 1.0f);
	    var capsuleGeometry = new CapsuleGeometry
	    {
		    center1 = new Vector2(0f, -0.5f * capsuleLength),
		    center2 = new Vector2(0f, 0.5f * capsuleLength),
		    radius = random.NextFloat(0.25f, 0.5f)
	    };
	    body.CreateShape(capsuleGeometry, shapeDef);
    }
    
    private static void CreatePolygon(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
	    var radius = 0.25f * random.NextFloat(0f, 1.0f);
	    var polygonGeometry = SandboxUtility.CreateRandomPolygon(extent: 0.75f, radius: radius, ref random);
	    body.CreateShape(polygonGeometry, shapeDef);
    }
    
    private void CreateBarrel()
    {
	    var world = PhysicsWorld.defaultWorld;

	    var shapeDef = PhysicsShapeDefinition.defaultDefinition;
	    
		var body = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);

		var bodies = m_SandboxManager.Bodies;
		bodies.Add(body);

		{
			var boxTransform = new PhysicsTransform(new Vector2(0f, 4f), PhysicsRotate.identity);
			var boxGeometry = PolygonGeometry.CreateBox(new Vector2(94f, 10f), radius: 0f, transform: boxTransform);
			body.CreateShape(boxGeometry, shapeDef);
		}

		{
			var boxTransform = new PhysicsTransform(new Vector2(-42f, 199f), PhysicsRotate.identity);
			var boxGeometry = PolygonGeometry.CreateBox(new Vector2(10f, 400f), radius: 0f, transform: boxTransform);
			body.CreateShape(boxGeometry, shapeDef);
		}
		
		{
			var boxTransform = new PhysicsTransform(new Vector2(42f, 199f), PhysicsRotate.identity);
			var boxGeometry = PolygonGeometry.CreateBox(new Vector2(10f, 400f), radius: 0f, transform: boxTransform);
			body.CreateShape(boxGeometry, shapeDef);
		}
		
	    var segmentGeometry = new SegmentGeometry { point1 = new Vector2(-800f, -80f), point2 = new Vector2(800f, -80f) };
	    body.CreateShape(segmentGeometry, shapeDef);
    }
}
