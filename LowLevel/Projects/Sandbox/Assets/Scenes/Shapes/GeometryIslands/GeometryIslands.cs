using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class GeometryIslands : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private bool m_OldAutoContactCallbacks;
    private PhysicsWorld.DrawFillOptions m_OldDrawFillOptions;
    
    private ControlsMenu.CustomButton m_LeftButton;
    private ControlsMenu.CustomButton m_RightButton;
    private ControlsMenu.CustomButton m_FireButton;

    private readonly PhysicsMask m_ObstacleMask = new(1);
    private readonly PhysicsMask m_GroundMask = new(2);
    private readonly PhysicsMask m_DestructibleMask = new(3);
    private readonly PhysicsMask m_DebrisMask = new(4);
    private readonly PhysicsMask m_ProjectileMask = new(5);

    private Color m_DestructibleColor;
    private PhysicsShape.ContactFilter m_DestructibleContactFilter;
    private PhysicsShape.SurfaceMaterial m_DestructibleSurfaceMaterial;
    
    private const float ProjectileRadius = 0.2f;
    private const float PlayerSpeed = 60f;
    private const float ProjectileSpeed = 40f;
    private const float ProjectileDelay = 0.02f;
    private float m_ProjectileTime;
    private Vector2 m_PlayerPosition;
    private CapsuleGeometry m_PlayerGeometry;
    private CircleGeometry m_FireGeometry;
    private PolygonGeometry m_DestructGeometry;
    private NativeArray<PolygonGeometry> m_FragmentGeometryMask;

    private SegmentGeometry m_VirtualGroundGeometry;
    private PhysicsTransform m_VirtualGroundTransform;
    
    private enum FragmentColors
    {
        Off,
        White,
        Group,
        Individual
    };
    
    private float m_FragmentRadius;
    private int m_FragmentCount;
    private FragmentColors m_FragmentColors;
    private bool m_FragmentExplode;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 28f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, 0f);

        // Get the default world.        
        var world = PhysicsWorld.defaultWorld;
        
        // Turn-on auto contact callbacks.
        m_OldAutoContactCallbacks = world.autoContactCallbacks;
        world.autoContactCallbacks = true;

        // Turn on interior drawing only.
        m_OldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;
        
        // Set controls.
        {
            m_LeftButton = m_SandboxManager.ControlsMenu[2];
            m_RightButton = m_SandboxManager.ControlsMenu[1];
            m_FireButton = m_SandboxManager.ControlsMenu[0];
            
            m_LeftButton.Set("Left");
            m_RightButton.Set("Right");
            m_FireButton.Set("Fire");
            m_FireButton.button.clickable.clicked += FirePressed;
        }
        
        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);
        
        m_DestructibleColor = Color.seaGreen;
        m_DestructibleContactFilter = new PhysicsShape.ContactFilter { categories = m_DestructibleMask, contacts = m_GroundMask | m_ProjectileMask | m_DebrisMask | m_DestructibleMask };
        m_DestructibleSurfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f, bounciness = 0.4f, tangentSpeed = 0.1f, customColor = m_DestructibleColor };
        
        m_PlayerGeometry = new CapsuleGeometry { center1 = Vector2.zero, center2 = Vector2.up, radius = 0.5f };
        m_DestructGeometry = PolygonGeometry.Create(vertices: new Vector2[] { new(-0.5f, -0.5f), new(0.5f, -0.5f), new(10f, 0.5f), new (9f, 0.5f) }.AsSpan()).Transform(Matrix4x4.Scale(new Vector2(3f, 40f)), false);        
        m_PlayerPosition = new Vector2(0f, 24f);

        m_VirtualGroundGeometry = new SegmentGeometry { point1 = new Vector2(-30f, 0f), point2 = new Vector2(25f, 0f) };
        m_VirtualGroundTransform = new PhysicsTransform(Vector2.down * 21.9f);
        
        m_FragmentRadius = 1f;
        m_FragmentCount = 25;
        m_FragmentColors = FragmentColors.Individual;
        m_FragmentExplode = false;
        
        UpdateFragmentGeometry();
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Reset overrides.
        m_SandboxManager.ResetOverrideColorShapeState();        

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Reset the callbacks.
        world.autoContactCallbacks = m_OldAutoContactCallbacks;
        
        // Reset the draw fill options.
        world.drawFillOptions = m_OldDrawFillOptions;
        
        // Unregister.
        m_FireButton.button.clickable.clicked -= FirePressed;

        // Dispose.
        if (m_FragmentGeometryMask.IsCreated)
            m_FragmentGeometryMask.Dispose();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);
            
            // Fragment Radius.
            var fireRadius = root.Q<Slider>("fragment-radius");
            fireRadius.value = m_FragmentRadius;
            fireRadius.RegisterValueChangedCallback(evt =>
            {
                m_FragmentRadius = evt.newValue;
                UpdateFragmentGeometry();
            });

            // Fragment Colors.
            var fragmentColors = root.Q<EnumField>("fragment-colors");
            fragmentColors.value = m_FragmentColors;
            fragmentColors.RegisterValueChangedCallback(evt => m_FragmentColors = (FragmentColors)evt.newValue);

            // Fragment Explode.
            var fragmentExplode = root.Q<Toggle>("fragment-explode");
            fragmentExplode.value = m_FragmentExplode;
            fragmentExplode.RegisterValueChangedCallback(evt => m_FragmentExplode = evt.newValue);
            
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

        // Ground.
        {
            var groundBody = world.CreateBody();
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_GroundMask, contacts = m_ProjectileMask | m_DebrisMask | m_DestructibleMask },
            };
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(1000f, 50f), radius: 0f, new PhysicsTransform(Vector2.down * 150f)), shapeDef);
        }
        
        // Destructible Geometry.
        {
            for (var n = 0; n < 6; ++n)
            {
                var body = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(n * 5f - 27f, -2f) });
                var shapeDef = new PhysicsShapeDefinition
                {
                    contactFilter = m_DestructibleContactFilter,
                    contactEvents = true,
                    surfaceMaterial = m_DestructibleSurfaceMaterial
                };
                body.CreateShape(m_DestructGeometry, shapeDef);
            }
        }
    }
    
    private void Update()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Only allow player movement/fire if the world is not paused.
        if (!m_SandboxManager.WorldPaused)
        {
            // Fetch input.
            var currentKeyboard = Keyboard.current;
            var leftPressed = m_LeftButton.isPressed || currentKeyboard.leftArrowKey.isPressed;
            var rightPressed = m_RightButton.isPressed || currentKeyboard.rightArrowKey.isPressed;
            var firePressed = currentKeyboard.spaceKey.isPressed;

            // Movement.
            {
                var movement = PlayerSpeed * Time.deltaTime;

                if (leftPressed)
                    m_PlayerPosition.x -= movement;

                if (rightPressed)
                    m_PlayerPosition.x += movement;

                // Clamp movement.
                m_PlayerPosition.x = Mathf.Clamp(m_PlayerPosition.x, -28f, 28f);
            }
            
            m_ProjectileTime += Time.deltaTime;
            
            // Fire.
            if (firePressed && m_ProjectileTime >= ProjectileDelay)
            {
                m_ProjectileTime = 0f;
                FirePressed();
            }
        }

        // Draw the Player.
        world.DrawGeometry(m_PlayerGeometry, m_PlayerPosition, Color.azure);

        // Draw the virtual ground.
        world.DrawGeometry(m_VirtualGroundGeometry, m_VirtualGroundTransform, m_DestructibleColor);
    }

    private void FirePressed()
    {
        // Finish if the world is paused.
        if (m_SandboxManager.WorldPaused)
            return;
    
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            gravityScale = 10f,
            fastCollisionsAllowed = true,
            position = m_PlayerPosition + Vector2.down * (ProjectileRadius + 1.5f),
            linearVelocity = Vector2.down * ProjectileSpeed
        };
        var body = world.CreateBody(bodyDef);
        body.callbackTarget = this;
        
        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter { categories = m_ProjectileMask, contacts = m_DestructibleMask | m_GroundMask },
            contactEvents = true,
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.ghostWhite }
        };
        var shape = body.CreateShape(new CircleGeometry { radius = ProjectileRadius }, shapeDef);
        shape.callbackTarget = this;
    }
    
    private void UpdateFragmentGeometry()
    {
        // Create the fragment geometry mask using the composer.
        {
            m_FireGeometry = new CircleGeometry { radius = m_FragmentRadius };

            // Dispose of any existing fire geometry mask.
            if (m_FragmentGeometryMask.IsCreated)
                m_FragmentGeometryMask.Dispose();
            
            var composer = PhysicsComposer.Create();
            composer.AddLayer(m_FireGeometry, PhysicsTransform.identity);
            m_FragmentGeometryMask = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Persistent);
            composer.Destroy();
        }
    }
    
    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        var shapeA = beginEvent.shapeA;
        var shapeB = beginEvent.shapeB;
        
        // Finish if either shape is invalid.
        // NOTE: We might've destroyed one in a previous callback.
        if (!shapeA.isValid || !shapeB.isValid)
            return;
        
        var categoryA = shapeA.contactFilter.categories;
        var categoryB = shapeB.contactFilter.categories;

        // Did we hit the ground?
        if (categoryA == m_GroundMask || categoryB == m_GroundMask)
        {
            // Yes, so destroy whatever hit the ground.
            var destroyShape = categoryA == m_GroundMask ? shapeB : shapeA;
            destroyShape.body.Destroy();
            return;
        }
        
        // Did we hit a destructable?
        if (categoryA == m_DestructibleMask || categoryB == m_DestructibleMask)
        {
            // Yes, so finish if it wasn't the projectile.
            // NOTE: It should only be but always be defensive.
            if (categoryA != m_ProjectileMask && categoryB != m_ProjectileMask)
                return;

            // Fetch the contact.
            var contact = beginEvent.contactId.contact;
            var hitPosition = contact.manifold.points[0].point;
            
            // Get the default world.
            var world = PhysicsWorld.defaultWorld;

            // Draw the fragment radius.            
            world.DrawCircle(hitPosition, m_FragmentRadius, Color.ivory, 2f / 60f, PhysicsWorld.DrawFillOptions.Outline);
            
            // Fetch the destructible shapes.
            var destructibleShape = categoryA == m_DestructibleMask ? shapeA : shapeB;
            var destructibleBody = destructibleShape.body;
            var destructibleBodyType = destructibleBody.type;
            using var destructibleShapes = destructibleBody.GetShapes();
            
            // Get the polygon geometry from the shapes.
            // NOTE: We know it's always polygon geometry so we don't have to convert it. 
            var targetPolygons = new NativeList<PolygonGeometry>(initialCapacity: destructibleShapes.Length, Allocator.Temp);
            foreach (var shape in destructibleShapes)
            {
                if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
                    targetPolygons.Add(shape.polygonGeometry);
            }

            // Create the target fragment geometry. 
            var targetGeometry = new PhysicsDestructor.FragmentGeometry(destructibleBody.transform, targetPolygons.AsReadOnly());
            targetPolygons.Dispose();
            
            // Create the fragment points.
            ref var random = ref m_SandboxManager.Random;
            var fragmentPoints = new NativeArray<Vector2>(m_FragmentCount, Allocator.Temp);
            fragmentPoints[0] = hitPosition;
            for (var i = 1; i < m_FragmentCount; ++i)
            {
                var rotate = new PhysicsRotate(random.NextFloat(0f, PhysicsMath.PI));
                var radius = random.NextFloat(0.05f, m_FragmentRadius);
                fragmentPoints[i] = hitPosition + rotate.direction * radius;
            }
            
            // Fragment the target geometry with the mask and fragment the results with the fragment points.
            // NOTE: This is the most complex operation, you don't have to mask the target geometry.
            var maskGeometry = new PhysicsDestructor.FragmentGeometry(hitPosition, m_FragmentGeometryMask);
            using var fragmentResults = PhysicsDestructor.Fragment(targetGeometry, maskGeometry, fragmentPoints, Allocator.Temp);

            // Dispose of the fragment points.
            fragmentPoints.Dispose();
            
            // Destroy both the destructible and the projectile.
            shapeA.body.Destroy();
            shapeB.body.Destroy();

            var fragmentTransform = fragmentResults.transform;

            // Create a body definition for static non-falling geometry.
            var staticBodyDef = new PhysicsBodyDefinition
            {
                position = fragmentTransform.position,
                rotation = fragmentTransform.rotation
            };
            
            // Create a body definition for dynamic falling geometry.
            var dynamicBodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                fastCollisionsAllowed = true,
                position = fragmentTransform.position,
                rotation = fragmentTransform.rotation,
                gravityScale = 4f
            };
            
            // Create the remaining destructible geometry from the "unbroken" geometry (if we have any).
            if (fragmentResults.unbrokenGeometry.Length > 0)
            {
                // Fetch the unbroken geometry.
                var unbrokenGeometry = fragmentResults.unbrokenGeometry;
                
                // Fetch the geometry islands.
                var unbrokenGeometryIslands = fragmentResults.unbrokenGeometryIslands;

                // Iterate the geometry islands looking for any island that intersects the virtual ground.
                // If it intersects then it'll be static otherwise it'll be treated as dynamic and fall.
                foreach (var islandRange in unbrokenGeometryIslands)
                {
                    var unbrokenAsDynamic = true;

                    // Fetch the geometry for the island.
                    var islandGeometry = unbrokenGeometry.GetSubArray(islandRange.start, islandRange.length);
                
                    // Was the destructible static?
                    if (destructibleBodyType == PhysicsBody.BodyType.Static)
                    {
                        // Yes, so iterate all the island geometry.
                        foreach (var geometry in islandGeometry)
                        {
                            // Skip if the geometry does not intersect with the virtual ground.
                            if (geometry.Intersect(fragmentTransform, m_VirtualGroundGeometry, m_VirtualGroundTransform).pointCount == 0)
                                continue;

                            // Intersection found so it's static.
                            unbrokenAsDynamic = false;
                            break;
                        }                    
                    }
                    
                    // Create a body for the island geometry.
                    var body = world.CreateBody(unbrokenAsDynamic ? dynamicBodyDef : staticBodyDef);
                    
                    // Create an appropriate shape definition for the island geometry.
                    var shapeDef = new PhysicsShapeDefinition
                    {
                        contactFilter = m_DestructibleContactFilter,
                        contactEvents = true,
                        surfaceMaterial = m_DestructibleSurfaceMaterial
                    };

                    // Create the island geometry as a shape batch.
                    using var shapeBatch = body.CreateShapeBatch(islandGeometry, shapeDef);
                    
                    // We'll need to see callbacks for dynamic geometry as we want to destroy it when it falls out of the scene.
                    if (unbrokenAsDynamic)
                    {
                        foreach (var shape in shapeBatch)
                            shape.callbackTarget = this;
                    }
                }
            }
            
            // Create the debris destructible geometry from the "broken" geometry (if we have any).
            {
                var brokenCount = fragmentResults.brokenGeometry.Length;
                if (brokenCount > 0)
                {
                    // Create a batch of bodies.
                    using var bodies = world.CreateBodyBatch(dynamicBodyDef, brokenCount);

                    // Create a shape color for dynamic falling geometry.
                    var dynamicShapeColor = m_FragmentColors switch
                    {
                        FragmentColors.Off or FragmentColors.Individual => m_DestructibleColor,
                        FragmentColors.White => Color.ivory,
                        FragmentColors.Group => m_SandboxManager.ShapeColorState,
                        _ => throw new ArgumentOutOfRangeException()
                    };


                    // Create a shape definition for the broken geometry.
                    var shapeDef = new PhysicsShapeDefinition
                    {
                        contactFilter = new PhysicsShape.ContactFilter { categories = m_DebrisMask, contacts = m_GroundMask | m_DestructibleMask | m_ObstacleMask },
                        contactEvents = true,
                        surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = dynamicShapeColor }
                    };

                    // Add a shape for each body.
                    for (var i = 0; i < brokenCount; ++i)
                    {
                        var body = bodies[i];
                        var geometry = fragmentResults.brokenGeometry[i];

                        if (m_FragmentColors == FragmentColors.Individual)
                            shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState };

                        var shape = body.CreateShape(geometry, shapeDef);
                        shape.callbackTarget = this;
                    }
                }
            }

            // Explode the debris.
            if (m_FragmentExplode)
            {
                world.Explode(new PhysicsWorld.ExplosionDefinition
                {
                    position = hitPosition + Vector2.up * m_FragmentRadius,
                    hitCategories = m_DebrisMask,
                    impulsePerLength = 2f,
                    radius = m_FragmentRadius * 3f
                });
            }
        }
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent) { }
}