using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class TopDownFriction : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private NativeList<PhysicsRelativeJoint> m_Joints;
    
    private float m_MaxForce;
    private float m_MaxTorque;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 10f;
        m_CameraManipulator.CameraStartPosition = new Vector2(0f, 10f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        m_SandboxManager.SetOverrideColorShapeState(true);

        m_MaxForce = 10f;
        m_MaxTorque = 10f;

        m_Joints = new NativeList<PhysicsRelativeJoint>(initialCapacity: 100, Allocator.Persistent);
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        if (m_Joints.IsCreated)
            m_Joints.Dispose();
        
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
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );
            
            // Max Force.
            var maxForce = root.Q<Slider>("max-force");
            maxForce.value = m_MaxForce;
            maxForce.RegisterValueChangedCallback(evt =>
            {
                m_MaxForce = evt.newValue;
                UpdateJoints();
            });           

            // Max Torque.
            var maxTorque = root.Q<Slider>("max-torque");
            maxTorque.value = m_MaxTorque;
            maxTorque.RegisterValueChangedCallback(evt =>
            {
                m_MaxTorque = evt.newValue;
                UpdateJoints();
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

        m_Joints.Clear();
        
        ref var random = ref m_SandboxManager.Random;
        
        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;

        // Ground Body.
        PhysicsBody groundBody;
        {
            groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            bodies.Add(groundBody);
            
            var vertices = new NativeList<Vector2>(Allocator.Temp);
            vertices.Add(Vector2.right * 10f + Vector2.up * 20f);
            vertices.Add(Vector2.right * 10f);
            vertices.Add(Vector2.left * 10f);
            vertices.Add(Vector2.left * 10f + Vector2.up * 20f);

            var geometry = new ChainGeometry(vertices.AsArray());
            groundBody.CreateChain(geometry, PhysicsChainDefinition.defaultDefinition);
        }

        {
            var jointDef = new PhysicsRelativeJointDefinition
            {
                bodyA = groundBody,
                collideConnected = true,
                maxForce = 10f,
                maxTorque = 10f
            };

            var capsule = new CapsuleGeometry { center1 = Vector2.left * 0.25f, center2 = Vector2.right * 0.25f, radius = 0.25f };
            var circle = new CircleGeometry { radius = 0.35f };
            var box = PolygonGeometry.CreateBox(new Vector2(0.7f, 0.7f));

            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, gravityScale = 0f };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0.8f } };
            
            const int order = 10;
            var offset = new Vector2(-5f, 15f);
            for (var i = 0; i < order; ++i )
            {
                for (var j = 0; j < order; ++j )
                {
                    bodyDef.position = offset;
                    var body = world.CreateBody(bodyDef);
                    bodies.Add(body);

                    shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                    
                    // Create a shape.
                    var shapeIndex = (order * i + j) % 4;
                    if (shapeIndex == 0)
                        body.CreateShape(capsule, shapeDef);
                    else if (shapeIndex == 1)
                        body.CreateShape(circle, shapeDef);
                    else if (shapeIndex == 2)
                        body.CreateShape(box, shapeDef);
                    else
                        body.CreateShape(SandboxUtility.CreateRandomPolygon(extent: 0.75f, radius: 0.1f, ref random), shapeDef);

                    // Create the joint.
                    jointDef.bodyB = body;
                    m_Joints.Add(world.CreateJoint(jointDef));

                    // Offset.
                    offset.x += 1f;
                }

                // Offset.
                offset += new Vector2(-10f, -1f);
            }
        }
    }
    
    private void UpdateJoints()
    {
        // Update the max motor torque.
        foreach (var joint in m_Joints)
        {
            joint.maxForce = m_MaxForce;
            joint.maxTorque = m_MaxTorque;
            
            joint.WakeBodies();
        }
    }
}
