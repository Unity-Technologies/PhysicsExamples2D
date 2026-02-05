using System;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class Friction : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private enum ObjectType
    {
        Capsule = 0,
        Box = 1
    }

    private ObjectType m_ObjectType;
    private Vector2 m_OldGravity;
    private float m_GravityScale;

    private const int ObjectCount = 10;
    private int m_ItemsSpawned;
    private const float SpawnPeriod = 2f;
    private float m_SpawnTime;

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 24f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 15f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_ObjectType = ObjectType.Capsule;
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GravityScale = 5f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        world.gravity = m_OldGravity;
    }

    private void Update()
    {
        // Finish if the world is paused.
        if (m_SandboxManager.WorldPaused)
            return;

        if (m_ItemsSpawned >= ObjectCount)
            return;

        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0)
            return;

        m_SpawnTime = SpawnPeriod / math.sqrt(m_GravityScale);
        ++m_ItemsSpawned;

        // Sliding Object.
        {
            // Get the default world.
            var world = PhysicsWorld.defaultWorld;

            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                linearVelocity = new Vector2(-1.5f, -0.5f),
                position = new Vector2(15f, 40f)
            };

            var body = world.CreateBody(bodyDef);

            const float frictionDelta = 1.0f / (ObjectCount > 1 ? ObjectCount - 1 : 1);
            var friction = frictionDelta * (m_ItemsSpawned - 1);
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = friction, bounciness = 0f, customColor = m_SandboxManager.ShapeColorState } };

            switch (m_ObjectType)
            {
                case ObjectType.Capsule:
                {
                    var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0f), center2 = new Vector2(0.5f, 0f), radius = 0.5f };
                    body.CreateShape(capsuleGeometry, shapeDef);
                    break;
                }

                case ObjectType.Box:
                {
                    var boxGeometry = PolygonGeometry.CreateBox(new Vector2(1f, 1f));
                    body.CreateShape(boxGeometry, shapeDef);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;
        
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Object Type.
            var objectType = root.Q<EnumField>("object-type");
            objectType.value = m_ObjectType;
            objectType.RegisterValueChangedCallback(evt =>
            {
                m_ObjectType = (ObjectType)evt.newValue;
                SetupScene();
            });

            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.RegisterValueChangedCallback(evt =>
            {
                m_GravityScale = evt.newValue;
                world.gravity = m_OldGravity * m_GravityScale;
                SetupScene();
            });
            gravityScale.value = m_GravityScale;

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        m_ItemsSpawned = 0;
        m_SpawnTime = 0f;

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Ground.
        {
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f } };

            var groundBody = world.CreateBody();

            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 1f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 1f), radius: 0f, new PhysicsTransform(new Vector2(-40f, 1f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 1f), radius: 0f, new PhysicsTransform(new Vector2(40f, 1f), PhysicsRotate.identity)), shapeDef);

            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(4f, 31f), PhysicsRotate.FromRadians(0.25f))), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(-11f, 28f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(-4f, 22f), PhysicsRotate.FromRadians(-0.25f))), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(11.5f, 19f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(4f, 14f), PhysicsRotate.FromRadians(0.25f))), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(-11f, 11f), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 0.5f), radius: 0f, new PhysicsTransform(new Vector2(-4f, 6f), PhysicsRotate.FromRadians(-0.25f))), shapeDef);
        }
    }
}