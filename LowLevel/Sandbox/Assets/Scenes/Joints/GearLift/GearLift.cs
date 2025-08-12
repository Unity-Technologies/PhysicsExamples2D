using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class GearLift : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private bool m_UseMotor;
    private float m_MotorSpeed;
    private float m_MaxMotorTorque;
    private Vector2 m_OldGravity;

    private PhysicsHingeJoint m_GearMotor;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 7f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 3.5f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_UseMotor = false;
        m_MotorSpeed = -0.5f;
        m_MaxMotorTorque = 80f;

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

            // Fast Collisions.
            var useMotor = root.Q<Toggle>("use-motor");
            useMotor.value = m_UseMotor;
            useMotor.RegisterValueChangedCallback(evt =>
            {
                m_UseMotor = evt.newValue;
                m_GearMotor.enableMotor = m_UseMotor;
                m_GearMotor.WakeBodies();
            });

            // Motor Speed.
            var motorSpeed = root.Q<Slider>("motor-speed");
            motorSpeed.value = m_MotorSpeed;
            motorSpeed.RegisterValueChangedCallback(evt =>
            {
                m_MotorSpeed = evt.newValue;
                m_GearMotor.motorSpeed = m_MotorSpeed;
                m_GearMotor.WakeBodies();
            });

            // Max Motor Torque.
            var maxMotorTorque = root.Q<Slider>("motor-torque");
            maxMotorTorque.value = m_MaxMotorTorque;
            maxMotorTorque.RegisterValueChangedCallback(evt =>
            {
                m_MaxMotorTorque = evt.newValue;
                m_GearMotor.maxMotorTorque = m_MaxMotorTorque;
                m_GearMotor.WakeBodies();
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

        ref var random = ref m_SandboxManager.Random;

        // Ground Body.
        var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);

        // Ground.
        {
            var points = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-11.2999992f, -0.216665655f),
                new(9.33750057f, -0.216665655f),
                new(9.33750343f, 7.19166565f),
                new(8.80833435f, 7.19166565f),
                new(8.80833435f, 0.312500000f),
                new(0.341668695f, 0.312500000f),
                new(0.341668695f, 0.841665685f),
                new(-0.187500000f, 0.841668725f),
                new(-0.187500000f, 1.37083435f),
                new(-0.716665685f, 1.37083435f),
                new(-0.716665685f, 1.90000308f),
                new(-1.24583435f, 1.90000308f),
                new(-1.24583435f, 2.42916870f),
                new(-1.77499998f, 2.42916870f),
                new(-1.77499998f, 2.95833445f),
                new(-2.30416560f, 2.95833445f),
                new(-2.30416560f, 3.48749995f),
                new(-2.83333135f, 3.48749995f),
                new(-2.83333135f, 4.01666880f),
                new(-3.36249995f, 4.01666594f),
                new(-3.36249995f, 4.54583120f),
                new(-3.89166570f, 4.54583454f),
                new(-3.89166570f, 5.07500029f),
                new(-4.42083311f, 5.07500029f),
                new(-4.42083311f, 5.60416889f),
#if false
                new(-4.95000029f, 5.60416555f),
                new(-4.95000029f, 6.13333464f),
                new(-5.47916555f, 6.13333464f),
                new(-5.47916555f, 6.66249990f),
                new(-6.00833273f, 6.66249990f),
                new(-6.00833273f, 7.19166565f),
#endif
                new(-11.3000002f - 2f, 7.19166565f + 1.5f),
                new(-11.3000002f - 2f, -0.216665655f)
            };

            {
                var offset = new Vector2(0.0f, -4f);
                const float scale = 0.75f;

                for (var i = 0; i < points.Length; ++i)
                {
                    points[i] = (points[i] + offset) * scale;
                }
            }

            var chainGeometry = new ChainGeometry(points.AsArray());
            var chainDef = new PhysicsChainDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, customColor = Color.darkSeaGreen } };
            groundBody.CreateChain(chainGeometry, chainDef);

            points.Dispose();
        }

        const float gearRadius = 1.0f;
        const float toothHalfWidth = 0.09f;
        const float toothHalfHeight = 0.06f;
        const float toothRadius = 0.03f;
        const float linkHalfLength = 0.07f;
        const float linkRadius = 0.05f;
        const int linkCount = 40;
        const float doorHalfHeight = 1.5f;

        var gearPosition1 = new Vector2(-4.25f + 0.15f, 10.25f - 1.6f);
        var gearPosition2 = gearPosition1 + new Vector2(2.0f + 0.15f, 1.0f - 1.6f);
        var linkAttachPosition = gearPosition2 + new Vector2(gearRadius + 2.0f * toothHalfWidth + toothRadius, 0.0f);
        var doorPosition = linkAttachPosition - new Vector2(0.0f, 2.0f * linkCount * linkHalfLength + doorHalfHeight);

        {
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
                position = gearPosition1
            };

            var gearBody = world.CreateBody(bodyDef);

            var shapeDef = new PhysicsShapeDefinition
            {
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, customColor = Color.saddleBrown }
            };

            var circle = new CircleGeometry { radius = gearRadius };
            gearBody.CreateShape(circle, shapeDef);

            const int count = 16;
            var deltaAngle = PhysicsMath.TAU / 16f;
            var dq = new PhysicsRotate(deltaAngle);
            var center = new Vector2(gearRadius + toothHalfHeight, 0f);
            var rotation = PhysicsRotate.identity;

            for (var i = 0; i < count; ++i)
            {
                var tooth = PolygonGeometry.CreateBox(
                    size: new Vector2(toothHalfWidth, toothHalfHeight) * 2f,
                    radius: toothRadius,
                    transform: new PhysicsTransform(center, rotation));

                shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, customColor = Color.gray };
                gearBody.CreateShape(tooth, shapeDef);

                rotation = dq.MultiplyRotation(rotation);
                center = rotation.RotateVector(new Vector2(gearRadius + toothHalfHeight, 0.0f));
            }

            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = groundBody,
                bodyB = gearBody,
                localAnchorA = groundBody.GetLocalPoint(gearPosition1),
                localAnchorB = Vector2.zero,
                enableMotor = m_UseMotor,
                maxMotorTorque = m_MaxMotorTorque,
                motorSpeed = m_MotorSpeed
            };

            m_GearMotor = world.CreateJoint(jointDef);
        }

        PhysicsBody followerBody;
        {
            var position = gearPosition2;
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
                position = position
            };

            followerBody = world.CreateBody(bodyDef);

            var shapeDef = new PhysicsShapeDefinition
            {
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, customColor = Color.saddleBrown }
            };

            var circle = new CircleGeometry { radius = gearRadius };
            followerBody.CreateShape(circle, shapeDef);

            const int count = 16;
            var deltaAngle = PhysicsMath.TAU / 16f;
            var dq = new PhysicsRotate(deltaAngle);
            var center = new Vector2(gearRadius + toothHalfWidth, 0f);
            var rotation = PhysicsRotate.identity;

            for (var i = 0; i < count; ++i)
            {
                var tooth = PolygonGeometry.CreateBox(
                    size: new Vector2(toothHalfWidth, toothHalfHeight) * 2f,
                    radius: toothRadius,
                    transform: new PhysicsTransform(center, rotation));

                shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, customColor = Color.gray };
                followerBody.CreateShape(tooth, shapeDef);

                rotation = dq.MultiplyRotation(rotation);
                center = rotation.RotateVector(new Vector2(gearRadius + toothHalfHeight, 0f));
            }

            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = groundBody,
                bodyB = followerBody,
                localAnchorA = groundBody.GetLocalPoint(position),
                localAnchorB = PhysicsTransform.identity,
                enableMotor = true,
                maxMotorTorque = 0.5f,
                enableLimit = true,
                lowerAngleLimit = PhysicsMath.ToDegrees(-0.3f * PhysicsMath.PI),
                upperAngleLimit = PhysicsMath.ToDegrees(0.8f * PhysicsMath.PI)
            };

            world.CreateJoint(jointDef);
        }

        PhysicsBody lastLinkBody;
        {
            var capsule = new CapsuleGeometry
            {
                center1 = new Vector2(0f, -linkHalfLength),
                center2 = new Vector2(0f, linkHalfLength),
                radius = linkRadius
            };

            var shapeDef = new PhysicsShapeDefinition
            {
                density = 2f,
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.lightSteelBlue }
            };

            var jointDef = new PhysicsHingeJointDefinition
            {
                maxMotorTorque = 0.05f,
                enableMotor = true
            };

            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
            };

            var position = linkAttachPosition + new Vector2(0f, -linkHalfLength);

            const int count = 40;
            var prevBody = followerBody;
            for (var i = 0; i < count; ++i)
            {
                bodyDef.position = position;

                var body = world.CreateBody(bodyDef);
                body.CreateShape(capsule, shapeDef);

                var pivot = new Vector2(position.x, position.y + linkHalfLength);
                jointDef.bodyA = prevBody;
                jointDef.bodyB = body;
                jointDef.localAnchorA = jointDef.bodyA.GetLocalPoint(pivot);
                jointDef.localAnchorB = jointDef.bodyB.GetLocalPoint(pivot);
                world.CreateJoint(jointDef);

                position.y -= 2.0f * linkHalfLength;
                prevBody = body;
            }

            lastLinkBody = prevBody;
        }

        {
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
                position = doorPosition
            };

            var body = world.CreateBody(bodyDef);

            var capsule = new CapsuleGeometry
            {
                center1 = new Vector2(0f, doorHalfHeight),
                center2 = new Vector2(0f, -doorHalfHeight),
                radius = 0.15f
            };

            var shapeDef = new PhysicsShapeDefinition
            {
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, customColor = Color.darkCyan }
            };
            body.CreateShape(capsule, shapeDef);

            {
                var pivot = doorPosition + new Vector2(0f, doorHalfHeight);
                var jointDef = new PhysicsHingeJointDefinition
                {
                    bodyA = lastLinkBody,
                    bodyB = body,
                    localAnchorA = lastLinkBody.GetLocalPoint(pivot),
                    localAnchorB = new Vector2(0f, doorHalfHeight),
                    enableMotor = true,
                    maxMotorTorque = 0.05f
                };
                world.CreateJoint(jointDef);
            }

            {
                var jointDef = new PhysicsSliderJointDefinition
                {
                    bodyA = groundBody,
                    bodyB = body,
                    localAnchorA = new PhysicsTransform(groundBody.GetLocalPoint(doorPosition), PhysicsRotate.up),
                    localAnchorB = new PhysicsTransform(Vector2.zero, PhysicsRotate.up),
                    maxMotorForce = 0.2f,
                    enableMotor = true,
                    collideConnected = true
                };
                world.CreateJoint(jointDef);
            }
        }

        {
            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };

            var y = 2.5f;
            const int xCount = 25;
            const int yCount = 20;
            for (var i = 0; i < yCount; ++i)
            {
                var x = -6.8f + 5.5f;
                for (var j = 0; j < xCount; ++j)
                {
                    bodyDef.position = new Vector2(x, y);
                    var body = world.CreateBody(bodyDef);

                    var poly = SandboxUtility.CreateRandomPolygon(extent: 0.14f, radius: random.NextFloat(0.02f, 0.03f), ref random);

                    var shapeDef = new PhysicsShapeDefinition
                    {
                        surfaceMaterial = new PhysicsShape.SurfaceMaterial { rollingResistance = 0.3f, customColor = m_SandboxManager.ShapeColorState }
                    };

                    body.CreateShape(poly, shapeDef);

                    x -= 0.2f;
                }

                y += 0.2f;
            }
        }
    }
}