using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class Spinner : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private Vector2 m_OldGravity;
    private float m_GravityScale;
    private float m_MotorSpeed;
    private float m_MaxMotorTorque;
    private bool m_KinematicSpinner;
    private int m_DebrisCount;
    private float m_DebrisFriction;
    private float m_DebrisBounciness;

    private Slider m_MotorTorqueElement;
    private PhysicsBody m_SpinnerBody;
    private PhysicsHingeJoint m_SpinnerHinge;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 42f;
        m_CameraManipulator.CameraStartPosition = new Vector2(0f, 0f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;
        
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GravityScale = 1f;
        m_MotorSpeed = 5f;
        m_MaxMotorTorque = 1000000f;
        m_KinematicSpinner = false;
        m_DebrisCount = 5000;
        m_DebrisFriction = 0.1f;
        m_DebrisBounciness = 0.1f;

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
        var world = PhysicsWorld.defaultWorld;
        
        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );

            // Motor Speed.
            var motorSpeed = root.Q<Slider>("motor-speed");
            motorSpeed.value = m_MotorSpeed;
            motorSpeed.RegisterValueChangedCallback(evt =>
            {
                m_MotorSpeed = evt.newValue;

                if (m_KinematicSpinner)
                    m_SpinnerBody.angularVelocity = m_MotorSpeed;
                else
                    m_SpinnerHinge.motorSpeed = m_MotorSpeed;
            });
            
            // Max Motor Torque.
            m_MotorTorqueElement = root.Q<Slider>("motor-torque");
            m_MotorTorqueElement.value = m_MaxMotorTorque;
            m_MotorTorqueElement.RegisterValueChangedCallback(evt => m_SpinnerHinge.maxMotorTorque = m_MaxMotorTorque = evt.newValue);

            // Kinematic Spinner.
            var kinematicSpinner = root.Q<Toggle>("kinematic-spinner");
            kinematicSpinner.RegisterValueChangedCallback(evt =>
            {
                m_KinematicSpinner = evt.newValue;
                m_MotorTorqueElement.enabledSelf = !m_KinematicSpinner;
                SetupScene();
            });
            kinematicSpinner.value = m_KinematicSpinner;
            
            // Debris Count.
            var debrisCount = root.Q<SliderInt>("debris-count");
            debrisCount.value = m_DebrisCount;
            debrisCount.RegisterValueChangedCallback(evt =>
            {
                m_DebrisCount = evt.newValue;
                SetupScene();
            });
            
            // Debris Friction.
            var debrisFriction = root.Q<Slider>("debris-friction");
            debrisFriction.value = m_DebrisFriction;
            debrisFriction.RegisterValueChangedCallback(evt =>
            {
                m_DebrisFriction = evt.newValue;
                SetupScene();
            });

            // Debris Bounciness.
            var debrisBounciness = root.Q<Slider>("debris-bounciness");
            debrisBounciness.value = m_DebrisBounciness;
            debrisBounciness.RegisterValueChangedCallback(evt =>
            {
                m_DebrisBounciness = evt.newValue;
                SetupScene();
            });
            
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

        PhysicsBody groundBody;
        
        // Chain Surround.
        {
            groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            bodies.Add(groundBody);

            const int pointCount = 360;

            var chainPoints = new NativeArray<Vector2>(pointCount, Allocator.Temp);

            var tau = PhysicsMath.TAU;
            var rotate = new PhysicsRotate(-tau / pointCount);
            var offset = new Vector2(40.0f, 0f);
            for (var i = 0; i < pointCount; ++i)
            {
                chainPoints[i] = new Vector2(offset.x, offset.y);
                offset = rotate.RotateVector(offset);
            }
            groundBody.CreateChain(new ChainGeometry(chainPoints), new PhysicsChainDefinition {surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0f }, isLoop = true });
            chainPoints.Dispose();
        }

        // Spinner Paddle.
        {
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = m_KinematicSpinner ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic,
                angularVelocity = m_KinematicSpinner ? m_MotorSpeed : 0f,
                position = new Vector2(0f, -20f),
                sleepingAllowed = false
            };
            
            m_SpinnerBody = world.CreateBody(bodyDef);
            bodies.Add(m_SpinnerBody);

            var box = PolygonGeometry.CreateBox(size: new Vector2(0.8f, 39f), radius: 0.2f);
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f } };
            m_SpinnerBody.CreateShape(box, shapeDef);

            // Add hinge if the spinner isn't Kinematic.
            if (!m_KinematicSpinner)
            {
                var hingeDefinition = new PhysicsHingeJointDefinition
                {
                    bodyA = groundBody,
                    bodyB = m_SpinnerBody,
                    localAnchorA = bodyDef.position,
                    enableMotor = true,
                    motorSpeed = m_MotorSpeed,
                    maxMotorTorque = m_MaxMotorTorque
                };
                m_SpinnerHinge = world.CreateJoint(hingeDefinition);
            }
        }

        // Spinner Debris.
        {
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.25f, 0f), center2 = new Vector2(0.25f, 0f), radius = 0.25f };
            var circle = new CircleGeometry { center = Vector2.zero, radius = 0.35f };
            var square = PolygonGeometry.CreateBox(new Vector2(0.35f, 0.35f));

            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { density = 0.25f, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_DebrisFriction, bounciness = m_DebrisBounciness } };

            var x = -23f;
            var y = -30f;
            
            for (var i = 0; i < m_DebrisCount; ++i)
            {
                bodyDef.position = new Vector2(x, y);
                var body = world.CreateBody(bodyDef);
                bodies.Add(body);

                shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
                
                var remainder = i % 3;
                if (remainder == 0)
                {
                    body.CreateShape(capsule, shapeDef);
                }
                else if ( remainder == 1 )
                {
                    body.CreateShape(circle, shapeDef);
                }
                else if ( remainder == 2 )
                {
                    body.CreateShape(square, shapeDef);
                }
                
                x += 0.5f;

                if ( x >= 23.0f )
                {
                    x = -23.0f;
                    y += 0.5f;
                }
            }            
        }
    }
}
