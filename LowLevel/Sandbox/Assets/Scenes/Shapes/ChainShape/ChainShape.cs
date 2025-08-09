using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class ChainShape : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Box = 2
    }

    private ObjectType m_ObjectType;
    private int m_ObjectCount;
    private const float Friction = 0.1f;
    private float m_GravityScale;
    private bool m_FastCollisionsAllowed;

    private Vector2 m_OldGravity;
    private int m_ItemsSpawned;
    private const float SpawnPeriod = 1.75f;
    private float m_SpawnTime;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 35f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 0f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_ObjectType = ObjectType.Box;
        m_ObjectCount = 100;

        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GravityScale = 10f;
        m_FastCollisionsAllowed = false;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_OldGravity;
    }

    private void Update()
    {
        // Finish if the world is paused.
        if (m_SandboxManager.WorldPaused)
            return;

        if (m_ItemsSpawned >= m_ObjectCount)
            return;

        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0)
            return;

        m_SpawnTime = SpawnPeriod / math.sqrt(m_GravityScale);
        ++m_ItemsSpawned;

        // Sliding Object.
        {
            var world = PhysicsWorld.defaultWorld;
            var bodies = m_SandboxManager.Bodies;

            var startPosition = new Vector2(-55f, 13.5f);
            var startLinearVelocity = new Vector2(2f, -1f);

            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, fastCollisionsAllowed = m_FastCollisionsAllowed, position = startPosition, linearVelocity = startLinearVelocity };
            var body = world.CreateBody(bodyDef);
            bodies.Add(body);

            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = Friction, bounciness = 0f, customColor = m_SandboxManager.ShapeColorState } };

            switch (m_ObjectType)
            {
                case ObjectType.Circle:
                {
                    body.CreateShape(new CircleGeometry { radius = 1f }, shapeDef);
                    break;
                }

                case ObjectType.Capsule:
                {
                    body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-0.75f, 0f), center2 = new Vector2(0.75f, 0f), radius = 0.75f }, shapeDef);
                    break;
                }

                case ObjectType.Box:
                {
                    body.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 2f)), shapeDef);
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
        var world = PhysicsWorld.defaultWorld;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Object Type.
            var objectType = root.Q<DropdownField>("object-type");
            objectType.index = (int)m_ObjectType;
            objectType.RegisterValueChangedCallback(evt =>
            {
                m_ObjectType = Enum.Parse<ObjectType>(evt.newValue);
                SetupScene();
            });

            // Object Count.
            var objectCount = root.Q<SliderInt>("object-count");
            objectCount.value = m_ObjectCount;
            objectCount.RegisterValueChangedCallback(evt =>
            {
                m_ObjectCount = evt.newValue;
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

            // Fast Collisions.
            var fastCollisions = root.Q<Toggle>("fast-collisions");
            fastCollisions.value = m_FastCollisionsAllowed;
            fastCollisions.RegisterValueChangedCallback(evt =>
            {
                m_FastCollisionsAllowed = evt.newValue;
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

        m_ItemsSpawned = 0;
        m_SpawnTime = 0f;

        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;

        // Ground.
        {
            var body = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            bodies.Add(body);

            using var points = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-60.885498f, 12.8985004f), new(-60.885498f, 16.2057495f), new(60.885498f, 16.2057495f), new(60.885498f, -30.2057514f),
                new(51.5935059f, -30.2057514f), new(43.6559982f, -10.9139996f), new(35.7184982f, -10.9139996f), new(27.7809982f, -10.9139996f),
                new(21.1664963f, -14.2212505f), new(11.9059982f, -16.2057514f), new(0f, -16.2057514f), new(-10.5835037f, -14.8827496f),
                new(-17.1980019f, -13.5597477f), new(-21.1665001f, -12.2370014f), new(-25.1355019f, -9.5909977f), new(-31.75f, -3.63799858f),
                new(-38.3644981f, 5.0840004f), new(-42.3334999f, 8.59125137f), new(-47.625f, 10.5755005f), new(-60.885498f, 12.8985004f),
            };

            var chainGeometry = new ChainGeometry(points.AsArray());
            var chainDef = new PhysicsChainDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0f } };
            body.CreateChain(chainGeometry, chainDef);
        }
    }
}