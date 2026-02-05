using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SpriteDestruction : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    public Sprite Sprite;
    public Material SpriteMaterial;
    
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;
    private Camera m_Camera;
    
    private Vector2 m_OldGravity;
    private bool m_OldAutoContactCallbacks;
    private PhysicsWorld.DrawFillOptions m_OldDrawFillOptions;
    
    private readonly PhysicsMask m_ObstacleMask = new(1);
    private readonly PhysicsMask m_GroundMask = new(2);
    private readonly PhysicsMask m_DestructibleMask = new(3);
    private readonly PhysicsMask m_DebrisMask = new(4);

    private Color m_DestructibleColor;
    private PhysicsShape.ContactFilter m_DestructibleContactFilter;
    private PhysicsShape.SurfaceMaterial m_DestructibleSurfaceMaterial;
    
    private NativeArray<PolygonGeometry> m_FragmentGeometryMask;

    private SegmentGeometry m_VirtualGroundGeometry;
    private PhysicsTransform m_VirtualGroundTransform;

    private SpriteDestructionBatch m_SpriteDestructionBatch;
    private readonly List<Vector2> m_PhysicsShapeVertex = new();
    private  NativeArray<VertexAttributeDescriptor> m_VertexAttributes;
    
    private float m_FragmentRadius;
    private bool m_FragmentCreate;
    private int m_FragmentCount;
    private float m_FragmentFriction;
    private float m_FragmentBounciness;
    private float m_FragmentForce;
    private float m_GravityScale;

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 12f;
        m_CameraManipulator.CameraPosition = Vector2.down * 2f;
        m_CameraManipulator.DisableManipulators = true;
        m_Camera = m_CameraManipulator.Camera;

        // Get the default world.        
        var world = PhysicsWorld.defaultWorld;
        
        // Store old gravity.
        m_OldGravity = world.gravity;

        // Turn-on auto contact callbacks.
        m_OldAutoContactCallbacks = world.autoContactCallbacks;
        world.autoContactCallbacks = true;
        
        // Turn on interior drawing only.
        m_OldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;
        
        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);

        m_FragmentRadius = 2f;
        m_FragmentCreate = true;
        m_FragmentCount = 20;
        m_FragmentFriction = 0.5f;
        m_FragmentBounciness = 0f;
        m_FragmentForce = 10f;
        m_GravityScale = 5f;
        
        // Create the sprite batch.
        m_SpriteDestructionBatch = new SpriteDestructionBatch();
        m_SpriteDestructionBatch.Create(SpriteMaterial);

        m_DestructibleColor = new Color(0.1f, 0f, 0f, 0f);
        m_DestructibleContactFilter = new PhysicsShape.ContactFilter { categories = m_DestructibleMask, contacts = m_GroundMask | m_DebrisMask | m_DestructibleMask };
        
        m_VirtualGroundGeometry = new SegmentGeometry { point1 = new Vector2(-100f, 0f), point2 = new Vector2(100f, 0f) };
        m_VirtualGroundTransform = new PhysicsTransform(Vector2.down * 7.25f);
        
        // Vertex attributes.
        m_VertexAttributes = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Persistent);
        m_VertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
        m_VertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 2);
        m_VertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
            
        CreateFragmentMaskGeometry();
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Destroy the sprite batch.
        m_SpriteDestructionBatch.Destroy();

        if (m_VertexAttributes.IsCreated)
            m_VertexAttributes.Dispose();
        
        // Enable manipulators.
        m_CameraManipulator.DisableManipulators = false;
        
        // Reset overrides.
        m_SandboxManager.ResetOverrideColorShapeState();        

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Reset the callbacks.
        world.autoContactCallbacks = m_OldAutoContactCallbacks;
        
        // Reset the old gravity.
        world.gravity = m_OldGravity;
        
        // Reset the draw fill options.
        world.drawFillOptions = m_OldDrawFillOptions;
        
        // Dispose.
        if (m_FragmentGeometryMask.IsCreated)
            m_FragmentGeometryMask.Dispose();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
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
                CreateFragmentMaskGeometry();
            });
            
            // Fragment Create.
            var fragmentCreate = root.Q<Toggle>("fragment-create");
            fragmentCreate.value = m_FragmentCreate;
            fragmentCreate.RegisterValueChangedCallback(evt => m_FragmentCreate = evt.newValue);
            
            // Fragment Count.
            var fragmentCount = root.Q<SliderInt>("fragment-count");
            fragmentCount.value = m_FragmentCount;
            fragmentCount.RegisterValueChangedCallback(evt => { m_FragmentCount = evt.newValue; });
            
            // Fragment Friction.
            var fragmentFriction = root.Q<Slider>("fragment-friction");
            fragmentFriction.value = m_FragmentFriction;
            fragmentFriction.RegisterValueChangedCallback(evt => { m_FragmentFriction = evt.newValue; });
            
            // Fragment Bounciness.
            var fragmentBounciness = root.Q<Slider>("fragment-bounciness");
            fragmentBounciness.value = m_FragmentBounciness;
            fragmentBounciness.RegisterValueChangedCallback(evt => { m_FragmentBounciness = evt.newValue; });
            
            // Fragment Force.
            var fragmentForce = root.Q<Slider>("fragment-force");
            fragmentForce.value = m_FragmentForce;
            fragmentForce.RegisterValueChangedCallback(evt => { m_FragmentForce = evt.newValue; });
            
            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.value = m_GravityScale;
            gravityScale.RegisterValueChangedCallback(evt =>
            {
                m_GravityScale = evt.newValue;
                world.gravity = m_OldGravity * m_GravityScale;
            });
            
            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the draw batch.
        m_SpriteDestructionBatch.Reset();
        
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();
        
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Ground.
        {
            var groundBody = world.CreateBody();
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_GroundMask, contacts = m_DebrisMask | m_DestructibleMask },
            };
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(110f, 10f), radius: 0f, new PhysicsTransform(Vector2.up * 50f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(110f, 10f), radius: 0f, new PhysicsTransform(Vector2.down * 50f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, 100f), radius: 0f, new PhysicsTransform(Vector2.left * 50f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, 100f), radius: 0f, new PhysicsTransform(Vector2.right * 50f)), shapeDef);
        }
        
        // Create the initial sprites.
        CreateInitialSprite(Vector2.left * 7f);
        CreateInitialSprite(Vector2.right * 6f);
    }
    
    private void Update()
    {
        // Finish if the world is paused.
        if (m_SandboxManager.WorldPaused)
            return;

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Draw the virtual ground.
        world.DrawGeometry(m_VirtualGroundGeometry, m_VirtualGroundTransform, Color.saddleBrown);
        
        // Fetch the world mouse position.
        var currentMouse = Mouse.current;
        var worldPosition = (Vector2)m_Camera.ScreenToWorldPoint(currentMouse.position.value);
        
        // Destruct at the position if selected. 
        if (currentMouse.leftButton.wasPressedThisFrame)
            DestructAtPosition(worldPosition);

        world.DrawGeometry(m_FragmentGeometryMask, worldPosition, Color.dodgerBlue, 0f, PhysicsWorld.DrawFillOptions.Outline);
    }

    private void DestructAtPosition(Vector2 hitPosition)
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Create a new fragment mask.
        CreateFragmentMaskGeometry();            
        
        // Finish if position is not overlapped with a destructible.
        using var hits = world.OverlapPoint(hitPosition, new PhysicsQuery.QueryFilter { categories = m_DestructibleMask, hitCategories = m_DestructibleMask });
        if (hits.Length > 0)
        {
            // Simply use the first hit.
            var hit = hits[0];
            
            // Fetch the destructible shapes.
            var destructibleShape = hit.shape;
            var destructibleBody = destructibleShape.body;
            var destructibleBodyType = destructibleBody.type;
            var destructibleBodyLinearVelocity = destructibleBody.linearVelocity;
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
                var rotate = PhysicsRotate.CreateRadians(random.NextFloat(0f, PhysicsMath.PI));
                var radius = random.NextFloat(0.05f, m_FragmentRadius);
                fragmentPoints[i] = hitPosition + rotate.direction * radius;
            }

            // Fragment the target geometry with the mask and fragment the results with the fragment points.
            // NOTE: This is the most complex operation, you don't have to mask the target geometry.
            var maskGeometry = new PhysicsDestructor.FragmentGeometry(hitPosition, m_FragmentGeometryMask);
            using var fragmentResults = PhysicsDestructor.Fragment(targetGeometry, maskGeometry, fragmentPoints, Allocator.Temp);

            // Dispose of the fragment points.
            fragmentPoints.Dispose();

            // Destroy the draw item.
            m_SpriteDestructionBatch.DestroySpriteDrawItem(destructibleBody);

            // Set-up destructible surface material.
            m_DestructibleSurfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_FragmentFriction, bounciness = m_FragmentBounciness, tangentSpeed = 0f, customColor = m_DestructibleColor };
            
            // Fetch the fragment transform.
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
                linearVelocity = destructibleBodyLinearVelocity,
                gravityScale = 1.0f
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

                    // Create the draw item.
                    CreateDrawItem(body, islandGeometry);
                }
            }

            // Create fragments if selected (if we have any).
            if (m_FragmentCreate)
            {
                // Create the debris destructible geometry from the "broken" geometry.
                var brokenCount = fragmentResults.brokenGeometry.Length;
                if (brokenCount > 0)
                {
                    // Create a batch of bodies.
                    using var bodies = world.CreateBodyBatch(dynamicBodyDef, brokenCount);

                    // Create a shape definition for the broken geometry.
                    var shapeDef = new PhysicsShapeDefinition
                    {
                        contactFilter = new PhysicsShape.ContactFilter { categories = m_DebrisMask, contacts = m_GroundMask | m_DestructibleMask | m_ObstacleMask | m_DebrisMask },
                        contactEvents = true,
                        surfaceMaterial = m_DestructibleSurfaceMaterial
                    };

                    // Add a shape for each body.
                    for (var i = 0; i < brokenCount; ++i)
                    {
                        var body = bodies[i];
                        var geometry = fragmentResults.brokenGeometry[i];

                        var shape = body.CreateShape(geometry, shapeDef);
                        shape.callbackTarget = this;

                        // Create the draw item.
                        CreateDrawItem(body, ref geometry);
                    }
                }
            }
        }

        // Explode the fragments.
        if (m_FragmentCreate && m_FragmentForce > 0f)
        {
            world.Explode(new PhysicsWorld.ExplosionDefinition
            {
                position = hitPosition,
                hitCategories = m_DebrisMask,
                impulsePerLength = m_FragmentForce,
                radius = m_FragmentRadius * 2f
            });
        }
    }
    
    private void CreateFragmentMaskGeometry()
    {
        // Dispose of any existing fire geometry mask.
        if (m_FragmentGeometryMask.IsCreated)
            m_FragmentGeometryMask.Dispose();
        
        ref var random = ref m_SandboxManager.Random;
        
        var vertices = new NativeList<Vector2>(100, Allocator.Temp);
        var stride = PhysicsMath.TAU / 36f;
        for (var angle = 0f; angle < PhysicsMath.TAU; angle += stride)
        {
            PhysicsMath.CosSin(angle, out var cosine, out var sine);
            var radius = random.NextFloat(m_FragmentRadius * 0.9f, m_FragmentRadius * 1.1f);
            vertices.Add(new Vector2(cosine * radius, sine * radius));
        }

        // Create the fragment geometry.
        m_FragmentGeometryMask = PolygonGeometry.CreatePolygons(vertices.AsArray(), PhysicsTransform.identity, Vector2.one, Allocator.Persistent);
        vertices.Dispose();
    }

    private void CreateInitialSprite(Vector2 worldPosition)
    {
        if (Sprite.packed)
        {
            Debug.LogWarning("Sprite is packed in an Atlas which isn't supported.");
            return;
        }
        
        // Finish if no physics outlines are available.
        var physicsOutlines = Sprite.GetPhysicsShapeCount();
        if (physicsOutlines == 0)
        {
            Debug.LogWarning("No physics outlines were found for the initial sprite.");
            return;
        }

        // Create the composer.
        var composer = PhysicsComposer.Create();
        composer.useDelaunay = true;
        var vertexPath = new NativeList<Vector2>(Allocator.Temp);

        // Add all physic outlines.
        for (var i = 0; i < physicsOutlines; ++i)
        {
            // Get the physics shape.
            if (Sprite.GetPhysicsShape(i, m_PhysicsShapeVertex) > 0)
            {
                // Add to something we can use.
                foreach (var vertex in m_PhysicsShapeVertex)
                    vertexPath.Add(vertex);

                // Add the layer to the composer.
                composer.AddLayer(vertexPath.AsArray(), PhysicsTransform.identity);
            }

            vertexPath.Clear();
        }

        // Calculate the polygons from the points.
        using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Temp);
        
        // Dispose.
        vertexPath.Dispose();
        composer.Destroy();

        // Finish if no polygons.
        if (polygons.Length == 0)
        {
            Debug.LogWarning("No polygons were produced for the initial sprite.");
            return;
        }

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Create the static body.
        var physicsBody = world.CreateBody(new PhysicsBodyDefinition { position = worldPosition });

        // Create an appropriate shape definition for the island geometry.
        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = m_DestructibleContactFilter,
            contactEvents = true,
            surfaceMaterial = m_DestructibleSurfaceMaterial
        };

        // Create the island geometry as a shape batch.
        using var shapeBatch = physicsBody.CreateShapeBatch(polygons, shapeDef);
        
        // Create the draw item.
        CreateDrawItem(physicsBody, polygons);
    }

    private unsafe void CreateDrawItem(PhysicsBody physicsBody, ref PolygonGeometry polygon)
    {
        fixed (PolygonGeometry* pPolygonGeometry = &polygon)
        {
            CreateDrawItem(physicsBody, new ReadOnlySpan<PolygonGeometry>(pPolygonGeometry, 1));
        }
    }
    
    private void CreateDrawItem(PhysicsBody physicsBody, ReadOnlySpan<PolygonGeometry> polygons)
    {
        // Fetch the sprite details.
        var spriteExtents = Sprite.bounds.extents * 2f;
        var worldToTex = new Vector2(1f / spriteExtents.x, 1f / spriteExtents.y);

        // This needs to be calculated!
        var textureOffset = new Vector2(0.5f, 0.5f);
        
        // Create the vertices and indices data.
        var initialCapacity = polygons.Length * PhysicsConstants.MaxPolygonVertices * 3;
        var vertices = new NativeList<SpriteDestructionBatch.BatchVertex>(initialCapacity, Allocator.Temp);
        var indices = new NativeList<int>(initialCapacity, Allocator.Temp);
        
        // Create the rendering triangles from the polygon geometry.
        foreach (var polygonGeometry in polygons)
        {
            var polygonVertexCount = polygonGeometry.count;
            var polygonVertices = polygonGeometry.vertices;
            var rootVertex = vertices.Length;
            
            // Add the triangle vertices.
            for (var i = 0; i < polygonVertexCount; ++i) 
            {
                ref var vertex = ref polygonVertices[i];
                var uv = (vertex * worldToTex) + textureOffset;
                vertices.Add(new SpriteDestructionBatch.BatchVertex { position = vertex, uv = uv, normals = -Vector3.forward });
            }

            // Add the triangle indices.
            for (var i = 2; i < polygonVertexCount; ++i)
            {
                indices.Add(rootVertex);
                indices.Add(rootVertex + i);
                indices.Add(rootVertex + i - 1);
            }   
        }

        // Create the sprite draw item.
        m_SpriteDestructionBatch.CreateSpriteDrawItem(
            m_VertexAttributes,
            vertices.AsArray(),
            indices.AsArray(),
            physicsBody);
        
        // Dispose.
        vertices.Dispose();
        indices.Dispose();        
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

        // Finish if we didn't hit the ground.
        if (categoryA != m_GroundMask && categoryB != m_GroundMask)
            return;
        
        // Destroy whatever hit the ground.
        // Destroy the draw item.
        var destroyShape = categoryA == m_GroundMask ? shapeB : shapeA;
        m_SpriteDestructionBatch.DestroySpriteDrawItem(destroyShape.body);
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent) { }
}