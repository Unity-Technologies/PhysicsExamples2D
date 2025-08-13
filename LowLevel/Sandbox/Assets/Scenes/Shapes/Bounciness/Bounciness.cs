using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class Bounciness : MonoBehaviour
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
    private Vector2 m_OldGravity;
    private float m_GravityScale;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 28f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 19f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_ObjectType = ObjectType.Circle;
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GravityScale = 1f;

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

            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.value = m_GravityScale;
            gravityScale.RegisterValueChangedCallback(evt =>
            {
                m_GravityScale = evt.newValue;
                world.gravity = m_OldGravity * m_GravityScale;
                SetupScene();
            });

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

        // Ground.
        {
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            var body = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);

            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 1f), radius: 0f, new PhysicsTransform(new Vector2(0f, 3f), PhysicsRotate.identity)), shapeDef);
        }

        // Bounciness Objects.
        {
            var circleGeometry = new CircleGeometry { radius = 0.5f };
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(1f, 1f));
            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, 0.5f), center2 = new Vector2(0f, -0.5f), radius = 0.5f };

            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, bodyConstraints = RigidbodyConstraints2D.FreezeRotation };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0f } };

            const int ShapeCount = 40;

            const float dr = 1.0f / (ShapeCount > 1 ? ShapeCount - 1 : 1);
            const float dx = 2.0f;
            var x = -1.0f * (ShapeCount - 1);

            for (var i = 0; i < ShapeCount; ++i)
            {
                bodyDef.position = new Vector2(x, 44.0f);

                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;

                switch (m_ObjectType)
                {
                    case ObjectType.Circle:
                    {
                        body.CreateShape(circleGeometry, shapeDef);
                        break;
                    }
                    case ObjectType.Capsule:
                    {
                        body.CreateShape(capsuleGeometry, shapeDef);
                        break;
                    }

                    case ObjectType.Box:
                    {
                        body.CreateShape(boxGeometry, shapeDef);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                shapeDef.surfaceMaterial.bounciness += dr;
                x += dx;
            }
        }
    }
}