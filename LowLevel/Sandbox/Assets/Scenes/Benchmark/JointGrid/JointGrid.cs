using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

using PhysicsJoint = UnityEngine.LowLevelPhysics2D.PhysicsJoint;

public class JointGrid : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private const int JointSize = 100;
    
    private Vector2 m_OldGravity;
    private float m_GravityScale;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 70f;
        m_CameraManipulator.CameraStartPosition = new Vector2(50, -60f);
        
        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(PhysicsWorld.DrawOptions.AllJoints);
        var world = PhysicsWorld.defaultWorld;
        world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll & ~PhysicsWorld.DrawOptions.AllJoints;
        
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
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
        
        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter(),
        };
        
        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = 0.4f };
        
        var bodyArray = new NativeArray<PhysicsBody>(JointSize * JointSize, Allocator.Temp);
        
        var index = 0;
        for (var k = 0; k < JointSize; ++k )
        {
            for (var i = 0; i < JointSize; ++i )
            {
                var fk = (float)k;
                var fi = (float)i;

                if (k >= JointSize / 2 - 3 && k <= JointSize / 2 + 3 && i == 0)
                {
                    bodyDef.bodyType = RigidbodyType2D.Static;
                }
                else
                {
                    bodyDef.bodyType = RigidbodyType2D.Dynamic;
                }

                bodyDef.position = new Vector2(fk, -fi);

                var body = world.CreateBody(bodyDef);
                
                // Fetch the appropriate shape color.
                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                
                body.CreateShape(circleGeometry, shapeDef);
                
                if (i > 0)
                {
                    hingeJointDef.bodyA = bodyArray[index - 1];
                    hingeJointDef.bodyB = body;
                    hingeJointDef.localAnchorA = new Vector2(0f, -0.5f);
                    hingeJointDef.localAnchorB = new Vector2(0f, 0.5f);
                    world.CreateJoint(hingeJointDef);
                }

                if ( k > 0 )
                {
                    hingeJointDef.bodyA = bodyArray[index - JointSize];
                    hingeJointDef.bodyB = body;
                    hingeJointDef.localAnchorA = new Vector2(0.5f, 0f);
                    hingeJointDef.localAnchorB = new Vector2(-0.5f, 0f);
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
