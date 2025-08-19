using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class ModifyGeometry : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsWorld m_PhysicsWorld;
    private PhysicsBody m_ChangerBody;
    private PhysicsShape m_ChangerShape;

    private enum GeometryType
    {
        Circle,
        Capsule,
        Segment,
        Polygon
    }

    private RigidbodyType2D m_BodyType;
    private GeometryType m_GeometryType;
    private float m_GeometryScale;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 6;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 5f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);

        m_PhysicsWorld = PhysicsWorld.defaultWorld;

        m_BodyType = RigidbodyType2D.Kinematic;
        m_GeometryType = GeometryType.Circle;
        m_GeometryScale = 1f;
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
	    // Reset overrides.
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

            // Body Type.
            var bodyType = root.Q<EnumField>("body-type");
            bodyType.value = m_BodyType;
            bodyType.RegisterValueChangedCallback(evt =>
            {
                m_BodyType = (RigidbodyType2D)evt.newValue;
                m_ChangerBody.bodyType = m_BodyType;
            });
            
            // Geometry Type.
            var geometryType = root.Q<EnumField>("geometry-type");
            geometryType.value = m_GeometryType;
            geometryType.RegisterValueChangedCallback(evt =>
            {
                m_GeometryType = (GeometryType)evt.newValue;
                UpdateShape();
            });
            
            // Geometry Scale.
            var geometryScale = root.Q<Slider>("geometry-scale");
            geometryScale.value = m_GeometryScale;
            geometryScale.RegisterValueChangedCallback(evt =>
            {
                m_GeometryScale = evt.newValue;
                UpdateShape();
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

        // Ground.
        {
            var groundBody = m_PhysicsWorld.CreateBody();
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 50f), 0f, Vector2.down * 25f));
        }

        // Interact Shape.
        {
            var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = Vector2.up * 4f });
            body.CreateShape(PolygonGeometry.CreateBox(Vector2.one * 2f));
        }

        // Shape Changer.
        {
            m_ChangerBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { bodyType = m_BodyType, position = Vector2.up });
            m_ChangerShape = m_ChangerBody.CreateShape(circleGeometry);
            
            // Update the geometry if this isn't the current default.
            if (m_GeometryType != GeometryType.Circle)
                UpdateShape();
        }
    }

    private void UpdateShape()
    {
        // Finish if the changer shape isn't valid.
        if (!m_ChangerShape.isValid)
            return;
        
        switch (m_GeometryType)
        {
            case GeometryType.Circle:
                m_ChangerShape.circleGeometry = circleGeometry;
                return;
            
            case GeometryType.Capsule:
                m_ChangerShape.capsuleGeometry = capsuleGeometry;
                return;
            
            case GeometryType.Segment:
                m_ChangerShape.segmentGeometry = segmentGeometry;
                return;
            
            case GeometryType.Polygon:
                m_ChangerShape.polygonGeometry = polygonGeometry;
                return;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private CircleGeometry circleGeometry => new CircleGeometry { radius = 0.5f * m_GeometryScale };
    private CapsuleGeometry capsuleGeometry => new CapsuleGeometry { center1 = Vector2.left * 0.5f * m_GeometryScale, center2 = Vector2.right * 0.5f * m_GeometryScale, radius = 0.5f * m_GeometryScale };
    private SegmentGeometry segmentGeometry => new SegmentGeometry { point1 = Vector2.left * 0.5f * m_GeometryScale, point2 = Vector2.right * 0.5f * m_GeometryScale };
    private PolygonGeometry polygonGeometry => PolygonGeometry.CreateBox(new Vector2(m_GeometryScale, 1.5f * m_GeometryScale));
}
