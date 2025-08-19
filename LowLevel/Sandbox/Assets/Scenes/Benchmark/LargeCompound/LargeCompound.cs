using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class LargeCompound : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private int m_CompoundSize;
    private int m_CompoundSplits;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 100f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 50f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_CompoundSize = 100;
        m_CompoundSplits = 5;
        
        SetupOptions();

        SetupScene();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Compound Splits.
            var compoundSplits = root.Q<SliderInt>("compound-splits");
            compoundSplits.value = m_CompoundSplits;
            compoundSplits.RegisterValueChangedCallback(evt =>
            {
                m_CompoundSplits = evt.newValue;
                SetupScene();
            });

            // Compound Size.
            var compoundSize = root.Q<SliderInt>("compound-size");
            compoundSize.value = m_CompoundSize;
            compoundSize.RegisterValueChangedCallback(evt =>
            {
                m_CompoundSize = evt.newValue;
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
            var groundBody = world.CreateBody();

            var span = new Vector2(100f, 500f);
            
            var vertices = new NativeList<Vector2>(Allocator.Temp);
            vertices.Add(Vector2.right * span.x + Vector2.up * (span.y + 1f));
            vertices.Add(Vector2.right * span.x + Vector2.up * span.y);
            vertices.Add(Vector2.right * span.x);
            vertices.Add(Vector2.left * span.x);
            vertices.Add(Vector2.left * span.x + Vector2.up * span.y);
            vertices.Add(Vector2.left * span.x + Vector2.up * (span.y + 1f));

            var geometry = new ChainGeometry(vertices.AsArray());
            groundBody.CreateChain(geometry, new  PhysicsChainDefinition { isLoop = false });
        }

        // Large Dynamic Compounds. 
        {
            const float gridSize = 1.0f;
            var gridBoxSize = new Vector2(gridSize, gridSize);

            var splits = m_CompoundSplits + 1;
            
            var span = m_CompoundSize / splits;

            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { startMassUpdate = false };

            for (var m = 0; m < splits; ++m)
            {
                var bodyY = (m_CompoundSize + 1 + m * span) * gridSize;

                for (var n = 0; n < splits; ++n)
                {
                    var bodyX = -0.5f * gridSize * splits * span + n * span * gridSize;
                    bodyDef.position = new Vector2(bodyX, bodyY);
                    var body = world.CreateBody(bodyDef);

                    for (var i = 0; i < span; ++i)
                    {
                        var y = i * gridSize;
                        for (var j = 0; j < span; ++j)
                        {
                            var x = gridSize * j;
                            shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                            body.CreateShape(PolygonGeometry.CreateBox(gridBoxSize, radius: 0f, new PhysicsTransform(new Vector2(x, y), PhysicsRotate.identity)), shapeDef);
                        }
                    }

                    // All shapes have been added, so we can efficiently compute the mass properties.
                    body.ApplyMassFromShapes();
                }
            }
        }
    }
}