using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class Smash : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private const int Columns = 100;
    private const int Rows = 60;

    private Vector2 m_OldGravity;
    private float m_Speed;
    private float m_Density;
    private float m_Bounciness;
    private float m_Spacing;
    private bool m_FastCollisionsAllowed;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 60f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 0f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_Speed = 40f;
        m_Density = 25f;
        m_Bounciness = 0f;
        m_Spacing = 0f;
        m_FastCollisionsAllowed = false;

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

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Reset Scene.
            var resetScene = root.Q<Button>("reset-scene");
            resetScene.clicked += SetupScene;

            // Speed.
            var speed = root.Q<Slider>("speed");
            speed.value = m_Speed;
            speed.RegisterValueChangedCallback(evt =>
            {
                m_Speed = evt.newValue;
                SetupScene();
            });

            // Density.
            var density = root.Q<Slider>("density");
            density.value = m_Density;
            density.RegisterValueChangedCallback(evt =>
            {
                m_Density = evt.newValue;
                SetupScene();
            });

            // Bounciness.
            var bounciness = root.Q<Slider>("bounciness");
            bounciness.value = m_Bounciness;
            bounciness.RegisterValueChangedCallback(evt =>
            {
                m_Bounciness = evt.newValue;
                SetupScene();
            });

            // Spacing.
            var spacing = root.Q<Slider>("spacing");
            spacing.value = m_Spacing;
            spacing.RegisterValueChangedCallback(evt =>
            {
                m_Spacing = evt.newValue;
                SetupScene();
            });

            // Fast Collisions.
            var fastCollisions = root.Q<Toggle>("fast-collisions");
            fastCollisions.value = m_FastCollisionsAllowed;
            fastCollisions.RegisterValueChangedCallback(evt =>
            {
                m_FastCollisionsAllowed = evt.newValue;
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
        var bodies = m_SandboxManager.Bodies;

        // Reset the gravity.
        world.gravity = Vector2.zero;

        // Border
        {
            var body = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            bodies.Add(body);

            var extents = new Vector2(110f, 63f);

            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, extents.y * 2f), radius: 0f, new PhysicsTransform(new Vector2(-extents.x, 0f), PhysicsRotate.identity)), shapeDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, extents.y * 2f), radius: 0f, new PhysicsTransform(new Vector2(extents.x, 0f), PhysicsRotate.identity)), shapeDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(extents.x * 2f, 10f), radius: 0f, new PhysicsTransform(new Vector2(0f, -extents.y), PhysicsRotate.identity)), shapeDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(extents.x * 2f, 10f), radius: 0f, new PhysicsTransform(new Vector2(0f, extents.y), PhysicsRotate.identity)), shapeDef);
        }

        // Large dense object.
        {
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
                position = new Vector2(-90f, 0f),
                linearVelocity = new Vector2(m_Speed, 0f),
                angularVelocity = PhysicsMath.PI * 0.1f,
                fastCollisionsAllowed = m_FastCollisionsAllowed
            };
            var body = world.CreateBody(bodyDef);
            bodies.Add(body);
            body.CreateShape(
                PolygonGeometry.CreateBox(new Vector2(8f, 8f)),
                new PhysicsShapeDefinition { density = m_Density, surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = m_Bounciness } });
        }

        // Small objects.
        {
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
                fastCollisionsAllowed = m_FastCollisionsAllowed,
                awake = false
            };
            var largeBody = world.CreateBody(bodyDef);
            bodies.Add(largeBody);

            const float boxDimension = 0.4f;
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(boxDimension, boxDimension));

            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = m_Bounciness } };

            var spacing = boxDimension + m_Spacing;
            for (var i = 0; i < Columns; ++i)
            {
                for (var j = 0; j < Rows; ++j)
                {
                    bodyDef.position = new Vector2(i * spacing - 60f, (j - Rows / 2.0f) * spacing);
                    var boxBody = world.CreateBody(bodyDef);
                    bodies.Add(boxBody);

                    // Fetch the appropriate shape color.
                    shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;

                    boxBody.CreateShape(boxGeometry, shapeDef);
                }
            }
        }
    }
}