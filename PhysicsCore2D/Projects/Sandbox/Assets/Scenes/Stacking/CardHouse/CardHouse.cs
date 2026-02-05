using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class CardHouse : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 1f;
        m_CameraManipulator.CameraPosition = new Vector2(0.7f, 0.9f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(true);
        
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

        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.7f } };

        // Ground.
        {
            var groundBody = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(0f, -2f) });
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(80f, 4f)), shapeDef);
        }

        // Cards.
        {
            var cardHeight = 0.2f;
            var cardThickness = 0.001f;

            var angle0 = math.radians(25.0f);
            var angle1 = math.radians(-25.0f);
            var angle2 = 0.5f * PhysicsMath.PI;

            var cardBox = PolygonGeometry.CreateBox(new Vector2(cardThickness, cardHeight) * 2f);
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };

            var nb = 5;
            var z0 = 0.0f;
            var y = cardHeight - 0.02f;
            while (nb > 0)
            {
                var z = z0;
                for (var i = 0; i < nb; i++)
                {
                    if (i != nb - 1)
                    {
                        bodyDef.position = new Vector2(z + 0.25f, y + cardHeight - 0.015f);
                        bodyDef.rotation = PhysicsRotate.FromRadians(angle2);
                        var body = world.CreateBody(bodyDef);

                        shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                        body.CreateShape(cardBox, shapeDef);
                    }

                    {
                        bodyDef.position = new Vector2(z, y);
                        bodyDef.rotation = PhysicsRotate.FromRadians(angle1);
                        var body = world.CreateBody(bodyDef);

                        shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                        body.CreateShape(cardBox, shapeDef);
                    }

                    z += 0.175f;

                    {
                        bodyDef.position = new Vector2(z, y);
                        bodyDef.rotation = PhysicsRotate.FromRadians(angle0);
                        var body = world.CreateBody(bodyDef);

                        shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                        body.CreateShape(cardBox, shapeDef);
                    }

                    z += 0.175f;
                }

                y += cardHeight * 2.0f - 0.03f;
                z0 += 0.175f;
                nb--;
            }
        }
    }
}