using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

using Random = Unity.Mathematics.Random;

public class TriggerFunnel : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Polygon = 2,
        Compound = 3,
        Ragdoll = 4,
        Softbody = 5,
        Random = 6,
    }

    private ObjectType m_ObjectType;
    private float m_ObjectScale;
    private float m_SpawnPeriod;
    private float m_GravityScale;
    private bool m_FastCollisions;

    private Vector2 m_OldGravity;
    private const float SpawnSide = -15f;
    private float m_SpawnTime;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 35f;
        m_CameraManipulator.CameraPosition = Vector2.zero;

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;
        
        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.Off);
        
        m_ObjectType = ObjectType.Ragdoll;

        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GravityScale = 2f;
        m_ObjectScale = 2f;
        m_SpawnPeriod = 0.75f;
        m_FastCollisions = false;

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

    private void Update()
    {
        // Finish if the world is paused.
        if (m_SandboxManager.WorldPaused)
            return;

        SpawnObject();

        DestroyTriggerDetections();
    }

    private void DestroyTriggerDetections()
    {
        var world = PhysicsWorld.defaultWorld;
        var triggerEvents = world.triggerBeginEvents;
        foreach (var triggerEvent in triggerEvents)
        {
            var visitorShape = triggerEvent.visitorShape;
            if (!visitorShape.isValid)
                continue;

            var body = visitorShape.body;
            if (!body.isValid)
                continue;

            // Ignore any static bodies.
            // NOTE: We do this because there's currently a bug in Box2D where sibling (same body) shapes
            // are detecting events with each other.
            if (body.bodyType == RigidbodyType2D.Static)
                return;

            // Destroy the body.
            body.Destroy();
        }
    }

    private void SpawnObject()
    {
        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0)
            return;

        m_SpawnTime = m_SpawnPeriod / math.sqrt(m_GravityScale);

        // Spawn Object.
        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;
        ref var random = ref m_SandboxManager.Random;

        var spawnPosition = new Vector2(random.NextFloat(-SpawnSide, SpawnSide), 35f);

        var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = spawnPosition };
        var shapeDef = new PhysicsShapeDefinition
        {
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.05f, customColor = m_SandboxManager.ShapeColorState },
            triggerEvents = true
        };

        switch (m_ObjectType)
        {
            case ObjectType.Circle:
            {
                var body = world.CreateBody(bodyDef);
                bodies.Add(body);
                CreateCircle(body, shapeDef, ref random);
                return;
            }

            case ObjectType.Capsule:
            {
                var body = world.CreateBody(bodyDef);
                bodies.Add(body);
                CreateCapsule(body, shapeDef, ref random);
                return;
            }

            case ObjectType.Polygon:
            {
                var body = world.CreateBody(bodyDef);
                bodies.Add(body);
                CreatePolygon(body, shapeDef, ref random);
                return;
            }

            case ObjectType.Compound:
            {
                var body = world.CreateBody(bodyDef);
                bodies.Add(body);
                CreateCompound(body, shapeDef);
                return;
            }

            case ObjectType.Ragdoll:
            {
                CreateRagdoll(world, spawnPosition, random, bodies);
                return;
            }

            case ObjectType.Softbody:
            {
                CreateDonut(world, spawnPosition, bodies);
                return;
            }

            case ObjectType.Random:
            {
                var body = world.CreateBody(bodyDef);
                bodies.Add(body);

                switch (random.NextInt(0, 7))
                {
                    case 0:
                    {
                        CreateCircle(body, shapeDef, ref random);
                        return;
                    }

                    case 1:
                    {
                        CreateCapsule(body, shapeDef, ref random);
                        return;
                    }

                    case 2:
                    {
                        CreatePolygon(body, shapeDef, ref random);
                        return;
                    }

                    case 4:
                    {
                        CreateCompound(body, shapeDef);
                        return;
                    }

                    case 5:
                    {
                        CreateRagdoll(world, spawnPosition, random, bodies);
                        return;
                    }

                    case 6:
                    {
                        CreateDonut(world, spawnPosition, bodies);
                        return;
                    }
                }

                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CreateCircle(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = m_ObjectScale * 0.5f };
        body.CreateShape(circleGeometry, shapeDef);
    }

    private void CreateCapsule(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
        var capsuleScale = m_ObjectScale * 0.5f;

        var capsuleGeometry = new CapsuleGeometry
        {
            center1 = new Vector2(0f, -0.5f * capsuleScale),
            center2 = new Vector2(0f, 0.5f * capsuleScale),
            radius = 0.5f * capsuleScale
        };
        body.CreateShape(capsuleGeometry, shapeDef);
    }

    private void CreatePolygon(PhysicsBody body, PhysicsShapeDefinition shapeDef, ref Random random)
    {
        var polygonScale = m_ObjectScale * 0.5f;
        var radius = m_ObjectScale * 0.25f;
        var polygonGeometry = SandboxUtility.CreateRandomPolygon(extent: polygonScale, radius: radius, ref random);
        body.CreateShape(polygonGeometry, shapeDef);
    }

    private void CreateRagdoll(PhysicsWorld world, Vector2 spawnPosition, Random random, NativeHashSet<PhysicsBody> bodies)
    {
        var ragDollConfiguration = new SpawnFactory.Ragdoll.Configuration
        {
            ScaleRange = new Vector2(m_ObjectScale * 1.25f, m_ObjectScale * 1.25f),
            JointFrequency = 1f,
            JointDamping = 0.1f,
            JointFriction = 0.0f,
            GravityScale = 1f,
            ContactBodyLayer = 0x2,
            ContactFeetLayer = 0x1,
            ContactGroupIndex = 0x1,
            ColorProvider = m_SandboxManager,
            TriggerEvents = true,
            FastCollisionsAllowed = m_FastCollisions,
            EnableLimits = true,
            EnableMotor = true
        };

        using var spawnedBodies = SpawnFactory.Ragdoll.SpawnRagdoll(world, spawnPosition, ragDollConfiguration, true, ref random);
        foreach (var body in spawnedBodies)
            bodies.Add(body);
    }

    private void CreateDonut(PhysicsWorld world, Vector2 spawnPosition, NativeHashSet<PhysicsBody> bodies)
    {
        using var spawnedBodies = SpawnFactory.Softbody.SpawnDonut(world, m_SandboxManager, spawnPosition, sides: 7, scale: m_ObjectScale * 0.5f, triggerEvents: true, jointFrequency: 20f);
        foreach (var body in spawnedBodies)
            bodies.Add(body);
    }

    private void CreateCompound(PhysicsBody body, PhysicsShapeDefinition shapeDef)
    {
        var compoundScale = new Vector3(m_ObjectScale, m_ObjectScale, m_ObjectScale) * 0.5f;
        var scale = Matrix4x4.Scale(compoundScale);
        var leftGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(-1.0f, 0f), new(0.5f, 1f), new(0f, 2f) }.AsSpan()).Transform(scale, false);
        var rightGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(1.0f, 0f), new(-0.5f, 1f), new(0f, 2f) }.AsSpan()).Transform(scale, false);
        body.CreateShape(leftGeometry, shapeDef);
        body.CreateShape(rightGeometry, shapeDef);
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

            // Object Type.
            var objectType = root.Q<DropdownField>("object-type");
            objectType.index = (int)m_ObjectType;
            objectType.RegisterValueChangedCallback(evt =>
            {
                m_ObjectType = Enum.Parse<ObjectType>(evt.newValue);
            });

            // Object Scale.
            var objectScale = root.Q<Slider>("object-scale");
            objectScale.value = m_ObjectScale;
            objectScale.RegisterValueChangedCallback(evt =>
            {
                m_ObjectScale = evt.newValue;
            });

            // Spawn Period.
            var spawnPeriod = root.Q<Slider>("spawn-period");
            spawnPeriod.value = m_SpawnPeriod;
            spawnPeriod.RegisterValueChangedCallback(evt =>
            {
                m_SpawnPeriod = evt.newValue;
            });

            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.RegisterValueChangedCallback(evt =>
            {
                m_GravityScale = evt.newValue;
                world.gravity = m_OldGravity * m_GravityScale;
            });
            gravityScale.value = m_GravityScale;

            // Fast Collisions.
            var fastCollisions = root.Q<Toggle>("fast-collisions");
            fastCollisions.value = m_FastCollisions;
            fastCollisions.RegisterValueChangedCallback(evt =>
            {
                m_FastCollisions = evt.newValue;
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

        m_SpawnTime = 0f;

        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;

        // Ground.
        {
            var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            bodies.Add(groundBody);

            using var points = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-26.8672504f, 41.088623f), new(26.8672485f, 41.088623f), new(16.8672485f, 17.1978741f),
                new(8.26824951f, 11.906374f), new(16.8672485f, 11.906374f), new(16.8672485f, -0.661376953f),
                new(8.26824951f, -5.953125f), new(16.8672485f, -5.953125f), new(16.8672485f, -13.229126f),
                new(3.63799858f, -23.151123f), new(3.63799858f, -31.088623f), new(-3.63800049f, -31.088623f),
                new(-3.63800049f, -23.151123f), new(-16.8672504f, -13.229126f), new(-16.8672504f, -5.953125f),
                new(-8.26825142f, -5.953125f), new(-16.8672504f, -0.661376953f), new(-16.8672504f, 11.906374f),
                new(-8.26825142f, 11.906374f), new(-16.8672504f, 17.1978741f)
            };

            var chainGeometry = new ChainGeometry(points.AsArray().AsSpan());
            var chainDef = new PhysicsChainDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.2f, bounciness = 0f } };
            groundBody.CreateChain(chainGeometry, chainDef);

            {
                var sign = 1.0f;
                var y = 28.0f;
                var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
                for (var i = 0; i < 4; ++i)
                {
                    bodyDef.position = new Vector2(0f, y);

                    var body = world.CreateBody(bodyDef);
                    bodies.Add(body);

                    var boxGeometry = PolygonGeometry.CreateBox(new Vector2(11f, 1f), radius: 0.5f);
                    var shapeDef = new PhysicsShapeDefinition { density = 1f, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 1f } };
                    body.CreateShape(boxGeometry, shapeDef);

                    var jointDef = new PhysicsHingeJointDefinition
                    {
                        bodyA = groundBody,
                        bodyB = body,
                        localAnchorA = bodyDef.position,
                        localAnchorB = Vector2.zero,
                        maxMotorTorque = 200.0f,
                        motorSpeed = 2.0f * sign,
                        enableMotor = true
                    };
                    world.CreateJoint(jointDef);

                    y -= 14.0f;
                    sign = -sign;
                }

                {
                    var boxGeometry = PolygonGeometry.CreateBox(new Vector2(10f, 4f), radius: 0f, new PhysicsTransform(new Vector2(0f, -32.5f), PhysicsRotate.identity));
                    var shapeDef = new PhysicsShapeDefinition { isTrigger = true, triggerEvents = true };
                    groundBody.CreateShape(boxGeometry, shapeDef);
                }
            }
        }
    }
}
