using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class Doohickey : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private int m_DoohickeyCount;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 9f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 5f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        m_SandboxManager.SetOverrideColorShapeState(true);

        m_DoohickeyCount = 5;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Reset overrides.
        m_SandboxManager.ResetOverrideDrawOptions();
        m_SandboxManager.ResetOverrideColorShapeState();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Doohickey Count.
            var doohickeyCount = root.Q<SliderInt>("doohickey-count");
            doohickeyCount.value = m_DoohickeyCount;
            doohickeyCount.RegisterValueChangedCallback(evt =>
            {
                m_DoohickeyCount = evt.newValue;
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
            var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);

            groundBody.CreateShape(new SegmentGeometry { point1 = Vector2.left * 20f, point2 = Vector2.right * 20f });
            groundBody.CreateShape(PolygonGeometry.CreateBox(Vector2.one * 2f, 0.1f, new PhysicsTransform(Vector2.up), true));
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 50f), 0f, new PhysicsTransform(Vector2.left * 8f + Vector2.up * 25f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 50f), 0f, new PhysicsTransform(Vector2.right * 8f + Vector2.up * 25f)));
        }

        // Doohickey.
        {
            var y = 4f;
            for (var n = 0; n < m_DoohickeyCount; ++n, y += 1.2f)
            {
                using var dumbbell = DoohickeyFactory.SpawnDumbbell(world, m_SandboxManager, new Vector2(0f, y), 0.5f);
            }
        }
    }
}