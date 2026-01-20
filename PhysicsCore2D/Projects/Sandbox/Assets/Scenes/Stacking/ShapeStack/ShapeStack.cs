using System;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class ShapeStack : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Box = 2,
        Mix = 3
    }

    private ObjectType m_ObjectType;
    private int m_StackHeight;
    private float m_ContactFrequency;
    private float m_ContactDampingRatio;
    private float m_ContactSpeed;
    private float m_GravityScale;

    private float m_OldContactFrequency;
    private float m_OldContactDamping;
    private float m_OldContactSpeed;
    private Vector2 m_OldGravity;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 6f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 5f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_ObjectType = ObjectType.Circle;
        m_StackHeight = 8;

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        m_OldContactFrequency = world.contactFrequency;
        m_OldContactDamping = world.contactDamping;
        m_OldContactSpeed = world.contactSpeed;
        m_OldGravity = world.gravity;

        m_ContactFrequency = m_OldContactFrequency;
        m_ContactDampingRatio = m_OldContactDamping;
        m_ContactSpeed = m_OldContactSpeed;
        m_GravityScale = 1f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        world.contactFrequency = m_OldContactFrequency;
        world.contactDamping = m_OldContactDamping;
        world.contactSpeed = m_OldContactSpeed;
        world.gravity = m_OldGravity;
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

            // Stack Height.
            var stackHeight = root.Q<SliderInt>("stack-height");
            stackHeight.value = m_StackHeight;
            stackHeight.RegisterValueChangedCallback(evt =>
            {
                m_StackHeight = evt.newValue;
                SetupScene();
            });

            // Contact Frequency.
            var contactFrequency = root.Q<Slider>("contact-frequency");
            contactFrequency.RegisterValueChangedCallback(evt =>
            {
                m_ContactFrequency = evt.newValue;
                world.contactFrequency = m_ContactFrequency;
            });
            contactFrequency.value = m_ContactFrequency;

            // Contact Damping.
            var contactDamping = root.Q<Slider>("contact-damping");
            contactDamping.RegisterValueChangedCallback(evt =>
            {
                m_ContactDampingRatio = evt.newValue;
                world.contactDamping = m_ContactDampingRatio;
            });
            contactDamping.value = m_ContactDampingRatio;

            // Contact Speed.
            var contactSpeed = root.Q<Slider>("contact-speed");
            contactSpeed.RegisterValueChangedCallback(evt =>
            {
                m_ContactSpeed = evt.newValue;
                world.contactSpeed = m_ContactSpeed;
            });
            contactSpeed.value = m_ContactSpeed;

            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.RegisterValueChangedCallback(evt =>
            {
                m_GravityScale = evt.newValue;
                world.gravity = m_OldGravity * m_GravityScale;
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

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Ground.
        {
            var groundBody = world.CreateBody();
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-100f, 0f), point2 = new Vector2(100f, 0f) }, PhysicsShapeDefinition.defaultDefinition);
        }

        // Stack.
        {
            CreateStack(new Vector2(0f, 0.55f));
        }
    }

    private void CreateStack(Vector2 position)
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.3f } };

        for (var i = 0; i < m_StackHeight; ++i)
        {
            bodyDef.position = position + new Vector2(0f, i * 1.2f);
            var body = world.CreateBody(bodyDef);

            shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;

            switch (m_ObjectType)
            {
                case ObjectType.Circle:
                {
                    CreateCircle(body, shapeDef);
                    continue;
                }

                case ObjectType.Capsule:
                {
                    CreateCapsule(body, shapeDef);
                    continue;
                }

                case ObjectType.Box:
                {
                    CreateBox(body, shapeDef);
                    continue;
                }

                case ObjectType.Mix:
                {
                    switch (i % 4)
                    {
                        case 0:
                        {
                            CreateCircle(body, shapeDef);
                            continue;
                        }

                        case 1:
                        {
                            CreateCapsule(body, shapeDef);
                            continue;
                        }

                        case 2:
                        {
                            CreateBox(body, shapeDef);
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

    private static void CreateCircle(PhysicsBody body, PhysicsShapeDefinition shapeDef)
    {
        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = 0.5f };
        body.CreateShape(circleGeometry, shapeDef);
    }

    private static void CreateCapsule(PhysicsBody body, PhysicsShapeDefinition shapeDef)
    {
        var capsuleGeometry = new CapsuleGeometry
        {
            center1 = new Vector2(0f, -0.25f),
            center2 = new Vector2(0f, 0.25f),
            radius = 0.25f
        };
        body.CreateShape(capsuleGeometry, shapeDef);
    }

    private static void CreateBox(PhysicsBody body, PhysicsShapeDefinition shapeDef)
    {
        var boxSize = new Vector2(1f, 1f);
        const float boxRadius = 0f;
        var polygonGeometry = PolygonGeometry.CreateBox(size: boxSize, radius: boxRadius);
        body.CreateShape(polygonGeometry, shapeDef);
    }
}