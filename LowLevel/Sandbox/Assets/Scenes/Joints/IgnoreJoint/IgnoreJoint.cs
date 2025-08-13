using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class IgnoreJoint : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsJoint m_Joint;
    private PhysicsBody m_BodyA;
    private PhysicsBody m_BodyB;

    private bool m_EnableJoint;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 10f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 7f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);

        m_EnableJoint = true;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Reset overrides.
        m_SandboxManager.ResetOverrideDrawOptions();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Enable Joint.
            var enableJoint = root.Q<Toggle>("enable-joint");
            enableJoint.value = m_EnableJoint;
            enableJoint.RegisterValueChangedCallback(evt =>
            {
                m_EnableJoint = evt.newValue;
                UpdateJoint();
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

        // Ground Body.
        {
            var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);

            var vertices = new NativeList<Vector2>(Allocator.Temp);
            vertices.Add(Vector2.right * 17f + Vector2.up * 17f);
            vertices.Add(Vector2.right * 17f);
            vertices.Add(Vector2.left * 17f);
            vertices.Add(Vector2.left * 17f + Vector2.up * 17f);

            var geometry = new ChainGeometry(vertices.AsArray());
            groundBody.CreateChain(geometry, PhysicsChainDefinition.defaultDefinition);
        }

        // Obstacle Body.
        {
            var geometry = PolygonGeometry.CreateBox(size: new Vector2(2f, 6f));

            var body = world.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = new Vector2(0f, 3f) });
            body.CreateShape(geometry);
        }

        // Ignored Bodies.
        {
            var geometry = PolygonGeometry.CreateBox(size: new Vector2(4f, 4f));

            m_BodyA = world.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = new Vector2(-4f, 2f) });
            m_BodyA.CreateShape(geometry);

            m_BodyB = world.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = new Vector2(4f, 2f) });
            m_BodyB.CreateShape(geometry);

            UpdateJoint();
        }
    }

    private void UpdateJoint()
    {
        // Destroy the joint if it's valid.
        if (m_Joint.isValid)
        {
            // NOTE: There seems to be a (reported) bug when deleting a joint not colliding both bodies is deleted. Both bodies won't collide again until moved significantly.
            // A workaround is to disable/enable one of the bodies. This will suffice until a fix is available.
            var body = m_Joint.bodyA;
            body.enabled = false;
            body.enabled = true;

            m_Joint.Destroy();
        }

        // Finish if the joint is not enabled.
        if (!m_EnableJoint)
            return;

        // Create the joint.
        var world = PhysicsWorld.defaultWorld;
        m_Joint = world.CreateJoint(new PhysicsIgnoreJointDefinition { bodyA = m_BodyA, bodyB = m_BodyB });
    }
}