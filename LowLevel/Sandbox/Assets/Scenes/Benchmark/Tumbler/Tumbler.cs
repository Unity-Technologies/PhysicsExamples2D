using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

public class Tumbler : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Polygon = 2,
        Compound = 3,
        Random = 4
    }
    
    private ObjectType m_ObjectType;
    private float m_AngularVelocity;
    private int m_DebrisCount;
    private float m_GravityScale;

    private Vector2 m_OldGravity;
    private PhysicsBody m_TumblerBody;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 30f;
        m_CameraManipulator.CameraStartPosition = new Vector2(0f, 0f);
        
        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;
        
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GravityScale = 2f;
        m_AngularVelocity = 0.25f;
        m_DebrisCount = 1000;
        m_ObjectType = ObjectType.Polygon;
        
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
        var world = PhysicsWorld.defaultWorld;
        
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
            
            // Angular Velocity.
            var angularVelocity = root.Q<Slider>("angular-velocity");
            angularVelocity.value = m_AngularVelocity;
            angularVelocity.RegisterValueChangedCallback(evt => { m_TumblerBody.angularVelocity = m_AngularVelocity = evt.newValue; });
            
            // Debris Count.
            var debrisCount = root.Q<SliderInt>("debris-count");
            debrisCount.value = m_DebrisCount;
            debrisCount.RegisterValueChangedCallback(evt =>
            {
                m_DebrisCount = evt.newValue;
                SetupScene();
            });
            
            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.RegisterValueChangedCallback(evt => { m_GravityScale = evt.newValue; world.gravity = m_OldGravity * m_GravityScale; });
            gravityScale.value = m_GravityScale;
            
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

        var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
        bodies.Add(groundBody);
        
        // Tumbler.
        {
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Kinematic, angularVelocity = m_AngularVelocity };
            m_TumblerBody = world.CreateBody(bodyDef);
            bodies.Add(m_TumblerBody);

            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            m_TumblerBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 42f), radius: 0f, new PhysicsTransform(new Vector2(20f, 0f), PhysicsRotate.identity)), shapeDef);
            m_TumblerBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 42f), radius: 0f, new PhysicsTransform(new Vector2(-20f, 0f), PhysicsRotate.identity)), shapeDef);
            m_TumblerBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(42f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, 20f), PhysicsRotate.identity)), shapeDef);
            m_TumblerBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(42f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, -20f), PhysicsRotate.identity)), shapeDef);
        }
        
        // Tumbler Debris.
        {
            var leftGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(-0.35f, -0.35f), new(0.17f, 0f), new(0f, 0.35f) }.AsSpan());
            var rightGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(0.35f, -0.35f), new(-0.17f, 0f), new(0f, 0.35f) }.AsSpan());

            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            ref var random = ref m_SandboxManager.Random;
            
            for (var i = 0; i < m_DebrisCount; ++i)
            {
                bodyDef.position = new Vector2(random.NextFloat(-18f, 18f), random.NextFloat(-18f, 18f));
                var body = world.CreateBody(bodyDef);
                bodies.Add(body);

                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;

				switch (m_ObjectType)
				{
					case ObjectType.Circle:
					{
						CreateCircle(body, shapeDef, ref random);
						continue;
					}
            
					case ObjectType.Capsule:
					{
						CreateCapsule(body, shapeDef, ref random);
						continue;
					}
            
					case ObjectType.Polygon:
					{
						CreatePolygon(body, shapeDef, ref random);
						continue;
					}

					case ObjectType.Compound:
					{
						CreateCompound(body, shapeDef, leftGeometry, rightGeometry);
						continue;
					}
				
					case ObjectType.Random:
					{
						switch (bodies.Count % 4)
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

							case 3:
							{
								CreateCompound(body, shapeDef, leftGeometry, rightGeometry);
								continue;
							}

							default:
								continue;
						}
					}
            
					default:
						throw new ArgumentOutOfRangeException();
				}				
            }            
        }
    }

    private static void CreateCircle(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
	    var radius = random.NextFloat(0.25f, 0.45f);
	    var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = radius };
	    body.CreateShape(circleGeometry, shapeDef);
    }

    private static void CreateCapsule(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
	    var capsuleLength = random.NextFloat(0.25f, 1.0f);
	    var capsuleGeometry = new CapsuleGeometry
	    {
		    center1 = new Vector2(0f, -0.3f * capsuleLength),
		    center2 = new Vector2(0f, 0.3f * capsuleLength),
		    radius = random.NextFloat(0.25f, 0.3f)
	    };
	    body.CreateShape(capsuleGeometry, shapeDef);
    }
    
    private static void CreatePolygon(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
	    var radius = 0.25f * random.NextFloat(0f, 1.0f);
	    var polygonGeometry = SandboxUtility.CreateRandomPolygon(extent: 0.35f, radius: radius, ref random);
	    body.CreateShape(polygonGeometry, shapeDef);
    }
    
    private static void CreateCompound(PhysicsBody body, PhysicsShapeDefinition shapeDef, PolygonGeometry leftGeometry, PolygonGeometry rightGeometry)
    {
	    body.CreateShape(leftGeometry, shapeDef);
	    body.CreateShape(rightGeometry, shapeDef);
    }
}
