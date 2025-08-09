using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class BounceHouse : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private const float DrawLifetime = 2f;

    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Polygon = 2
    }

    private ObjectType m_ObjectType;
    private PhysicsEvents.ContactHitEvent m_LastHitEvent;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 11f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 0f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_ObjectType = ObjectType.Polygon;

        SetupOptions();

        SetupScene();
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        var hitEvents = world.contactHitEvents;
        if (hitEvents.Length > 0)
            m_LastHitEvent = hitEvents[0];

        // Draw the hit event.
        if (m_LastHitEvent.shapeA.isValid)
        {
            var hitPoint = m_LastHitEvent.point;
            world.DrawCircle(hitPoint, 0.25f, Color.orangeRed, DrawLifetime);
            world.DrawLine(hitPoint, hitPoint + m_LastHitEvent.normal, Color.cornsilk, DrawLifetime);
        }
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

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

        // Ground.
        {
            var bodyDef = PhysicsBodyDefinition.defaultDefinition;
            var body = world.CreateBody(bodyDef);
            bodies.Add(body);

            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            body.CreateShape(new SegmentGeometry { point1 = new Vector2(-10f, -10f), point2 = new Vector2(10f, -10f) }, shapeDef);
            body.CreateShape(new SegmentGeometry { point1 = new Vector2(10f, -10f), point2 = new Vector2(10f, 10f) }, shapeDef);
            body.CreateShape(new SegmentGeometry { point1 = new Vector2(10f, 10f), point2 = new Vector2(-10f, 10f) }, shapeDef);
            body.CreateShape(new SegmentGeometry { point1 = new Vector2(-10f, 10f), point2 = new Vector2(-10f, -10f) }, shapeDef);
        }

        // Bouncing PhysicsShape.
        {
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
                linearVelocity = new Vector2(20f, 30f),
                fastCollisionsAllowed = true,
                fastRotationAllowed = m_ObjectType == ObjectType.Circle,
                gravityScale = 0f,
                position = Vector2.zero
            };

            var body = world.CreateBody(bodyDef);
            bodies.Add(body);

            var shapeDef = new PhysicsShapeDefinition
            {
                density = 1f,
                hitEvents = true,
                surfaceMaterial = new PhysicsShape.SurfaceMaterial
                {
                    bounciness = 1.2f,
                    friction = 0.3f,
                    customColor = m_SandboxManager.ShapeColorState
                }
            };

            switch (m_ObjectType)
            {
                case ObjectType.Circle:
                {
                    body.CreateShape(new CircleGeometry { center = Vector2.zero, radius = 0.5f }, shapeDef);
                    return;
                }

                case ObjectType.Capsule:
                {
                    body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-0.5f, 0f), center2 = new Vector2(0.5f, 0f), radius = 0.25f }, shapeDef);
                    return;
                }
                case ObjectType.Polygon:
                {
                    const float h = 0.2f;
                    body.CreateShape(PolygonGeometry.CreateBox(new Vector2(40f * h, h)), shapeDef);
                    return;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}