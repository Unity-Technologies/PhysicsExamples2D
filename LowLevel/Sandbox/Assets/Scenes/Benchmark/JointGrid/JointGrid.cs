using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class JointGrid : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private Vector2 m_OldGravity;
    private int m_GridSize;
    private float m_GravityScale;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraPosition = new Vector2(-10f, -10f);
        m_CameraManipulator.CameraSize = 100f;
        
        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;
        
        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.Off);
        
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GridSize = 64;
        m_GravityScale = 1f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
	    world.gravity = m_OldGravity;
        
        // Reset overrides.
        m_SandboxManager.ResetOverrideDrawOptions();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;
        var world = PhysicsWorld.defaultWorld;
        
        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );

            // Grid Size.
            var gridSize = root.Q<SliderInt>("grid-size");
            gridSize.value = m_GridSize;
            gridSize.RegisterValueChangedCallback(evt => { m_GridSize = evt.newValue; SetupScene(); });
            
            
            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.value = m_GravityScale;
            gravityScale.RegisterValueChangedCallback(evt => { m_GravityScale = evt.newValue; world.gravity = m_OldGravity * m_GravityScale; });

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
        
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        var hingeJointDef = PhysicsHingeJointDefinition.defaultDefinition;
        var gridShapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        
        var gridScale = 150.0f / m_GridSize;
        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = 0.4f * gridScale };
        
        var offset = new Vector2(m_GridSize * -0.5f, m_GridSize * 0.5f);
        var index = 0;
        var bodyArray = new NativeArray<PhysicsBody>(m_GridSize * m_GridSize, Allocator.Temp);
        for (var k = 0; k < m_GridSize; ++k )
        {
            for (var i = 0; i < m_GridSize; ++i )
            {
                var fk = (float)k;
                var fi = (float)i;

                if (k >= m_GridSize / 2 - 3 && k <= m_GridSize / 2 + 3 && i == 0)
                {
                    bodyDef.bodyType = RigidbodyType2D.Static;
                }
                else
                {
                    bodyDef.bodyType = RigidbodyType2D.Dynamic;
                }

                bodyDef.position = (new Vector2(fk, -fi) + offset) * gridScale;
                var body = world.CreateBody(bodyDef);
                
                gridShapeDefinition.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                body.CreateShape(circleGeometry, gridShapeDefinition);
                
                if (i > 0)
                {
                    hingeJointDef.bodyA = bodyArray[index - 1];
                    hingeJointDef.bodyB = body;
                    hingeJointDef.localAnchorA = new Vector2(0f, -0.5f) * gridScale;
                    hingeJointDef.localAnchorB = new Vector2(0f, 0.5f) * gridScale;
                    world.CreateJoint(hingeJointDef);
                }

                if ( k > 0 )
                {
                    hingeJointDef.bodyA = bodyArray[index - m_GridSize];
                    hingeJointDef.bodyB = body;
                    hingeJointDef.localAnchorA = new Vector2(0.5f, 0f) * gridScale;
                    hingeJointDef.localAnchorB = new Vector2(-0.5f, 0f) * gridScale;
                    world.CreateJoint(hingeJointDef);
                }

                bodyArray[index++] = body;
            }
        }

        foreach (var body in bodyArray)
        {
            bodies.Add(body);
        }
        bodyArray.Dispose();
    }
}
