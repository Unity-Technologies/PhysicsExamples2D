using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class TopDownFriction : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private ControlsMenu.CustomButton m_ExplodeButton;    
    private NativeList<PhysicsRelativeJoint> m_Joints;

    private float m_MaxForce;
    private float m_MaxTorque;

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 10f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 10f);

        // Set controls.
        {
            m_ExplodeButton = m_SandboxManager.ControlsMenu[0];
            m_ExplodeButton.Set("Explode");
            m_ExplodeButton.button.clickable.clicked += Explode;
        }
        
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
        // Unregister.
        m_ExplodeButton.button.clickable.clicked -= Explode;
        
        // Reset overrides.
        m_SandboxManager.ResetOverrideDrawOptions();
        m_SandboxManager.ResetOverrideColorShapeState();
        
        // Dispose.
        if (m_Joints.IsCreated)
            m_Joints.Dispose();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

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

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Ground Body.
        PhysicsBody groundBody;
        {
            groundBody = world.CreateBody();

            var vertices = new NativeList<Vector2>(Allocator.Temp);
            vertices.Add(new Vector2(10f, 19f));
            vertices.Add(new Vector2(10f, 3f));
            vertices.Add(new Vector2(-10f, 3f));
            vertices.Add(new Vector2(-10f, 19f));

            var geometry = new ChainGeometry(vertices.AsArray());
            groundBody.CreateChain(geometry, PhysicsChainDefinition.defaultDefinition);
        }

        {
            var jointDef = new PhysicsRelativeJointDefinition
            {
                bodyA = groundBody,
                collideConnected = true,
                maxForce = m_MaxForce,
                maxTorque = m_MaxTorque
            };

            var capsule = new CapsuleGeometry { center1 = Vector2.left * 0.25f, center2 = Vector2.right * 0.25f, radius = 0.25f };
            var circle = new CircleGeometry { radius = 0.35f };
            var box = PolygonGeometry.CreateBox(new Vector2(0.7f, 0.7f));

            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, gravityScale = 0f };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0.8f } };

            const int order = 10;
            var offset = new Vector2(-5f, 15f);
            for (var i = 0; i < order; ++i)
            {
                for (var j = 0; j < order; ++j)
                {
                    bodyDef.position = offset;
                    var body = world.CreateBody(bodyDef);

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

    private static void Explode()
    {
        var explosionDef = new PhysicsWorld.ExplosionDefinition
        {
            position = Vector2.up * 10f,
            radius = 10f,
            falloff = 5f,
            impulsePerLength = 10f
        };
        PhysicsWorld.defaultWorld.Explode(explosionDef);

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        world.DrawCircle(explosionDef.position, 10f, Color.softRed, 2f / 60f);
    }
}