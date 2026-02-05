using System;
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class Wind : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsWorld m_PhysicsWorld;
    
    private enum GeometryType
    {
        Circle,
        Capsule,
        Polygon
    }
    
    private GeometryType m_GeometryType;
    private int m_GeometryCount;
    private float m_WindDirection;
    private float m_WindSpeed;
    private float m_Drag;
    private float m_Lift;

    private CircleGeometry m_CircleGeometry;
    private CapsuleGeometry m_CapsuleGeometry;
    private PolygonGeometry m_PolygonGeometry;
    
    private Vector2 m_WindNoise;
    private Vector2 m_CurrentWind;
    private NativeList<PhysicsShape> m_Shapes;

    private const float GeometryRadius = 0.1f;
    
    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 4f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 1f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Register to update wind.
        PhysicsEvents.PreSimulate += ApplyWind;

        m_GeometryType = GeometryType.Capsule;
        m_GeometryCount = 20;
        m_WindDirection = 0f;
        m_WindSpeed = 6f;
        m_Drag = 1f;
        m_Lift = 0.75f;

        m_WindNoise = Vector2.zero;
        m_CurrentWind = Vector2.zero;
        m_Shapes = new NativeList<PhysicsShape>(Allocator.Persistent);

        m_CircleGeometry = new CircleGeometry { radius = GeometryRadius };
        m_CapsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -GeometryRadius), center2 = new Vector2(0f, GeometryRadius), radius = GeometryRadius * 0.25f };
        m_PolygonGeometry = PolygonGeometry.CreateBox(new Vector2(GeometryRadius * 0.5f, GeometryRadius * 2.5f));
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Unregister to update wind.
        PhysicsEvents.PreSimulate -= ApplyWind;
        
        if (m_Shapes.IsCreated)
            m_Shapes.Dispose();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);
            
            // Geometry Type.
            var geometryType = root.Q<EnumField>("geometry-type");
            geometryType.value = m_GeometryType;
            geometryType.RegisterValueChangedCallback(evt =>
            {
                m_GeometryType = (GeometryType)evt.newValue;
                SetupScene();
            });

            // Geometry Scale.
            var geometryCount = root.Q<SliderInt>("geometry-count");
            geometryCount.value = m_GeometryCount;
            geometryCount.RegisterValueChangedCallback(evt =>
            {
                m_GeometryCount = evt.newValue;
                SetupScene();
            });
            
            // Wind Direction.
            var windDirection = root.Q<Slider>("wind-direction");
            windDirection.value = m_WindDirection;
            windDirection.RegisterValueChangedCallback(evt => { m_WindDirection = evt.newValue; });
            
            // Wind Speed.
            var windSpeed = root.Q<Slider>("wind-speed");
            windSpeed.value = m_WindSpeed;
            windSpeed.RegisterValueChangedCallback(evt => { m_WindSpeed = evt.newValue; });            
            
            // Drag.
            var drag = root.Q<Slider>("drag");
            drag.value = m_Drag;
            drag.RegisterValueChangedCallback(evt => { m_Drag = evt.newValue; });

            // Lift
            var lift = root.Q<Slider>("lift");
            lift.value = m_Lift;
            lift.RegisterValueChangedCallback(evt => { m_Lift = evt.newValue; });
            
            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        var physicsWorld = PhysicsWorld.defaultWorld;

        var groundBody = physicsWorld.CreateBody();
        
        var jointDef = new PhysicsHingeJointDefinition
        {
            bodyA = groundBody,
            localAnchorA = new Vector2(0f, 2f + GeometryRadius),
            springFrequency = 0.1f,
            springDamping = 0f,
            enableSpring = true
        };

        var shapeDef = new PhysicsShapeDefinition { density = 20f };
        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, gravityScale = 0.5f, sleepingAllowed = false };

        m_Shapes.Clear();
        
        for (var n = 0; n < m_GeometryCount; ++n)
        {
            // Create a body.
            bodyDef.position = new Vector2(0.0f, 2.0f - 2.0f * GeometryRadius * n);
            var body = physicsWorld.CreateBody(bodyDef);

            // Set the shape definition custom color.
            shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
            
            // Create the appropriate shape type.
            switch (m_GeometryType)
            {
                case GeometryType.Circle:
                {
                    m_Shapes.Add(body.CreateShape(m_CircleGeometry, shapeDef));
                    break;
                }
                case GeometryType.Capsule:
                {
                    m_Shapes.Add(body.CreateShape(m_CapsuleGeometry, shapeDef));
                    break;
                }
                case GeometryType.Polygon:
                {
                    m_Shapes.Add(body.CreateShape(m_PolygonGeometry, shapeDef));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Joint to next body.
            jointDef.bodyB =  body;
            jointDef.localAnchorB = new Vector2(0f, GeometryRadius);
            physicsWorld.CreateJoint(jointDef);

            // Set as current body.
            jointDef.bodyA = body;
            jointDef.localAnchorA = new Vector2(0f, -GeometryRadius);
        }
    }

    private void Update()
    {
        var physicsWorld = PhysicsWorld.defaultWorld;

        physicsWorld.DrawLine(Vector2.zero, Vector2.up * (2f + GeometryRadius), Color.gray);
        physicsWorld.DrawLine(Vector2.zero, m_CurrentWind * 0.2f, Color.goldenRod);
    }
    
    private void ApplyWind(PhysicsWorld world, float deltaTime)
    {
        if (!m_Shapes.IsCreated || world != PhysicsWorld.defaultWorld)
            return;

        // Calculate the wind.
        var direction = PhysicsRotate.FromRadians(PhysicsMath.ToRadians(m_WindDirection));
        m_CurrentWind = (direction + m_WindNoise) * m_WindSpeed;

        // Apply the wind.
        foreach (var physicsShape in m_Shapes)
            physicsShape.ApplyWind(m_CurrentWind, m_Drag, m_Lift);

        // Calculate new wind noise.
        ref var random = ref m_SandboxManager.Random;
        var noise = new Vector2(random.NextFloat(-0.3f, 0.3f), random.NextFloat(-0.3f, 0.3f));
        m_WindNoise = Vector2.Lerp(m_WindNoise, noise, 0.05f);
    }
}
