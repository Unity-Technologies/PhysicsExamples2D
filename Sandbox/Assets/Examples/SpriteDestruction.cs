using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrates the use of Sprite fragmenting mapping.")]
public sealed class SpriteDestruction : SandboxExampleBehaviour, PhysicsCallbacks.IContactCallback
{
    private Sprite m_Sprite;
    private Material m_SpriteMaterial;

    private Camera m_Camera;

    private Vector2 m_OldGravity;
    private bool m_OldAutoContactCallbacks;

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

    private readonly List<Vector2> m_PhysicsShapeVertex = new();

    private float m_FragmentRadius;
    private bool m_FragmentCreate;
    private int m_FragmentCount;
    private float m_FragmentFriction;
    private float m_FragmentBounciness;
    private float m_FragmentForce;
    private float m_GravityScale;

    // --- Sprite rendering -------------------------------------------------------------------------
    // Each destructible/fragment body is drawn as a runtime Sprite whose mesh is the body's polygon
    // geometry (set GC-free via SpriteDataAccessExtensions) and rendered with Graphics.RenderSprite.
    // Sprites are pooled and reused so steady-state destruction allocates no managed memory, and the
    // scratch geometry buffers are reused NativeArrays (native memory, no GC).
    //
    // The map stays unmanaged by storing the sprite's EntityId (a managed Sprite ref can't live in a
    // NativeHashMap); resolve it back with Resources.EntityIdToObject.
    private NativeHashMap<PhysicsBody, EntityId> m_DrawItems;
    private readonly Stack<Sprite> m_FreeSprites = new(128);
    private readonly List<Sprite> m_AllSprites = new(128);
    private NativeArray<Vector3> m_ScratchPositions;
    private NativeArray<Vector2> m_ScratchUVs;
    private NativeArray<ushort> m_ScratchIndices;
    private RenderParams m_RenderParams;

    protected override float CameraSize => 12f;
    protected override Vector2 CameraPosition => Vector2.down * 2f;

    protected override void OnExampleEnable()
    {
        var data = (SpriteDestructionData)ExampleData;
        m_Sprite = data.Sprite;
        m_SpriteMaterial = data.SpriteMaterial;

        CameraManipulator.DisableManipulators = true;
        m_Camera = CameraManipulator.Camera;

        // Get the default world.
        var world = World;

        // Store old gravity.
        m_OldGravity = world.gravity;

        // Turn-on auto contact callbacks.
        m_OldAutoContactCallbacks = world.autoContactCallbacks;
        world.autoContactCallbacks = true;

        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        m_FragmentRadius = 2f;
        m_FragmentCreate = true;
        m_FragmentCount = 20;
        m_FragmentFriction = 0.5f;
        m_FragmentBounciness = 0f;
        m_FragmentForce = 10f;
        m_GravityScale = 5f;

        // Set up the sprite render state.
        m_DrawItems = new NativeHashMap<PhysicsBody, EntityId>(100, Allocator.Persistent);
        m_FreeSprites.Clear();
        m_AllSprites.Clear();
        m_RenderParams = new RenderParams(m_SpriteMaterial)
        {
            // Leave worldBounds at its default (AABB.zero). RenderSprite only derives the renderer
            // bounds from the sprite (sprite->GetBounds() transformed by objectToWorld) when
            // worldBounds is zero; supplying a non-zero worldBounds makes the engine's
            // CreateSpriteIntermediateRenderer skip that path and pass uninitialised bounds (→ a
            // per-frame "bounds contain NaN" warning). RenderSprite draws for all cameras (camera == null).
            camera = null
        };

        m_DestructibleColor = new Color(0.1f, 0f, 0f, 0f);
        m_DestructibleContactFilter = new PhysicsShape.ContactFilter { categories = m_DestructibleMask, contacts = m_GroundMask | m_DebrisMask | m_DestructibleMask };

        m_VirtualGroundGeometry = new SegmentGeometry { point1 = new Vector2(-100f, 0f), point2 = new Vector2(100f, 0f) };
        m_VirtualGroundTransform = new PhysicsTransform(Vector2.down * 7.25f);

        CreateFragmentMaskGeometry();
    }

    protected override void OnExampleDisable()
    {
        // Destroy the runtime sprites (pooled + active) and dispose the render state.
        foreach (var sprite in m_AllSprites)
        {
            if (sprite != null)
                Destroy(sprite);
        }
        m_AllSprites.Clear();
        m_FreeSprites.Clear();

        if (m_DrawItems.IsCreated)
            m_DrawItems.Dispose();

        if (m_ScratchPositions.IsCreated)
            m_ScratchPositions.Dispose();
        if (m_ScratchUVs.IsCreated)
            m_ScratchUVs.Dispose();
        if (m_ScratchIndices.IsCreated)
            m_ScratchIndices.Dispose();

        // Enable manipulators.
        CameraManipulator.DisableManipulators = false;

        // Get the default world.
        var world = World;

        // Reset the callbacks.
        world.autoContactCallbacks = m_OldAutoContactCallbacks;

        // Reset the old gravity.
        world.gravity = m_OldGravity;

        // Dispose.
        if (m_FragmentGeometryMask.IsCreated)
            m_FragmentGeometryMask.Dispose();
    }

    protected override void OnBeforeResetScene()
    {
        // Reset the draw items before the world is reset (they are keyed by body).
        ResetDrawItems();
    }

    protected override void SetupOptions()
    {
        // Get the default world.
        var world = World;

        // Fragment Radius.
        AddSlider("Fragment  Radius", m_FragmentRadius, 0.5f, 3f, v =>
        {
            m_FragmentRadius = v;
            CreateFragmentMaskGeometry();
        });

        // Fragment Create.
        AddToggle("Create Fragments", m_FragmentCreate, v => m_FragmentCreate = v);

        // Fragment Count.
        AddSliderInt("Fragment Count", m_FragmentCount, 1, 50, v => m_FragmentCount = v);

        // Fragment Friction.
        AddSlider("Fragment  Friction", m_FragmentFriction, 0f, 1f, v => m_FragmentFriction = v);

        // Fragment Bounciness.
        AddSlider("Fragment  Bounciness", m_FragmentBounciness, 0f, 0.75f, v => m_FragmentBounciness = v);

        // Fragment Force.
        AddSlider("Fragment  Force", m_FragmentForce, 0f, 50f, v => m_FragmentForce = v);

        // Gravity Scale.
        AddSlider("Gravity Scale", m_GravityScale, 0.1f, 10f, v =>
        {
            m_GravityScale = v;
            world.gravity = m_OldGravity * m_GravityScale;
        });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;
        world.gravity = m_OldGravity * m_GravityScale;

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
        if (SandboxManager.WorldPaused)
            return;

        // Get the default world.
        var world = World;

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

    // Render every fragment sprite for the frame. Runs unconditionally (even while the world is
    // paused) so the scene keeps drawing. Graphics.RenderSprite is batched by the SRP Batcher and
    // works in every render pipeline, so there is no per-camera callback or immediate-mode drawing.
    private void LateUpdate()
    {
        if (!m_DrawItems.IsCreated || m_DrawItems.IsEmpty)
            return;

        foreach (var drawItem in m_DrawItems)
        {
            var sprite = Resources.EntityIdToObject(drawItem.Value) as Sprite;
            if (sprite == null)
                continue;

            var body = drawItem.Key;
            var world = body.world;
            var transformPlane = world.transformPlane;
            var bodyTransform = body.transform;

            var position = PhysicsMath.ToPosition3D(bodyTransform.position, Vector3.zero, transformPlane);
            var rotation = PhysicsMath.ToRotationFast3D(body.rotation.radians, transformPlane);
            
            // SpriteParams is an immutable struct (stack-allocated, no GC); RenderParams/SpriteParams
            // are `in` parameters so they're passed without the `ref` keyword.
            var spriteParams = new SpriteParams(sprite);
            Graphics.RenderSprite(m_RenderParams, spriteParams, 0, Matrix4x4.TRS(position, rotation, Vector3.one));
        }
    }

    private void DestructAtPosition(Vector2 hitPosition)
    {
        // Get the default world.
        var world = World;

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
            ref var random = ref Random;
            var fragmentPoints = new NativeArray<Vector2>(m_FragmentCount, Allocator.Temp);
            fragmentPoints[0] = hitPosition;
            for (var i = 1; i < m_FragmentCount; ++i)
            {
                var rotate = PhysicsRotate.FromRadians(random.NextFloat(0f, PhysicsMath.PI));
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
            DestroySpriteDrawItem(destructibleBody);

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
                        surfaceMaterial = m_DestructibleSurfaceMaterial,
                        worldDrawing = false
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
                        surfaceMaterial = m_DestructibleSurfaceMaterial,
                        worldDrawing = false
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

        ref var random = ref Random;

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
        if (m_Sprite == null)
        {
            Debug.LogError("[SpriteDestruction] Sprite asset is missing — reimport Building.png in the Project window.");
            return;
        }

        if (m_Sprite.packed)
        {
            Debug.LogWarning("Sprite is packed in an Atlas which isn't supported.");
            return;
        }

        // Finish if no physics outlines are available.
        var physicsOutlines = m_Sprite.GetPhysicsShapeCount();
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
            if (m_Sprite.GetPhysicsShape(i, m_PhysicsShapeVertex) > 0)
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
        using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one);

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
        var world = World;

        // Create the static body.
        var physicsBody = world.CreateBody(new PhysicsBodyDefinition { position = worldPosition });

        // Create an appropriate shape definition for the island geometry.
        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = m_DestructibleContactFilter,
            contactEvents = true,
            surfaceMaterial = m_DestructibleSurfaceMaterial,
            worldDrawing = false
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

    // Build the body's renderable geometry from its polygons and upload it to a pooled runtime Sprite.
    // Vertex positions are the polygon vertices (body-local space); UVs map each vertex onto the source
    // sprite's texture so the texture "stays put" across breaks. All geometry is written into reused
    // scratch NativeArrays and pushed via SpriteDataAccessExtensions, so there is no managed allocation.
    private void CreateDrawItem(PhysicsBody physicsBody, ReadOnlySpan<PolygonGeometry> polygons)
    {
        // Fetch the sprite details.
        var spriteExtents = m_Sprite.bounds.extents * 2f;
        var worldToTex = new Vector2(1f / spriteExtents.x, 1f / spriteExtents.y);

        // This needs to be calculated!
        var textureOffset = new Vector2(0.5f, 0.5f);

        // Count the vertices/indices so the scratch buffers can be sized once.
        var vertexCount = 0;
        var indexCount = 0;
        foreach (var polygonGeometry in polygons)
        {
            var count = polygonGeometry.count;
            if (count < 3)
                continue;
            vertexCount += count;
            indexCount += (count - 2) * 3;
        }

        if (vertexCount == 0)
            return;

        EnsureScratchCapacity(vertexCount, indexCount);

        // Create the rendering triangles from the polygon geometry.
        var vertexIndex = 0;
        var triangleIndex = 0;
        foreach (var polygonGeometry in polygons)
        {
            var polygonVertexCount = polygonGeometry.count;
            if (polygonVertexCount < 3)
                continue;

            var polygonVertices = polygonGeometry.vertices;
            var rootVertex = vertexIndex;

            // Add the triangle vertices (position + texture UV).
            for (var i = 0; i < polygonVertexCount; ++i)
            {
                ref var vertex = ref polygonVertices[i];
                // The Sprite Position channel is Vector3 (z = 0 for 2D).
                m_ScratchPositions[vertexIndex] = new Vector3(vertex.x, vertex.y, 0f);
                m_ScratchUVs[vertexIndex] = (vertex * worldToTex) + textureOffset;
                ++vertexIndex;
            }

            // Add the triangle indices (fan triangulation).
            for (var i = 2; i < polygonVertexCount; ++i)
            {
                m_ScratchIndices[triangleIndex++] = (ushort)rootVertex;
                m_ScratchIndices[triangleIndex++] = (ushort)(rootVertex + i);
                m_ScratchIndices[triangleIndex++] = (ushort)(rootVertex + i - 1);
            }
        }

        // Rent a pooled sprite and upload the geometry GC-free via SpriteDataAccessExtensions.
        var sprite = RentSprite();
        sprite.SetVertexCount(vertexCount);
        sprite.SetVertexAttribute(VertexAttribute.Position, m_ScratchPositions.GetSubArray(0, vertexCount));
        sprite.SetVertexAttribute(VertexAttribute.TexCoord0, m_ScratchUVs.GetSubArray(0, vertexCount));
        sprite.SetIndices(m_ScratchIndices.GetSubArray(0, indexCount));

        // Add the draw item (store the sprite by EntityId so the map stays unmanaged).
        m_DrawItems.Add(physicsBody, sprite.GetEntityId());
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

        // Destroy whatever hit the ground (and its draw item).
        var destroyShape = categoryA == m_GroundMask ? shapeB : shapeA;
        DestroySpriteDrawItem(destroyShape.body);
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent) { }

    // --- Sprite pool + draw-item management -------------------------------------------------------

    // Rent a reusable runtime sprite from the pool, creating a new one only when the pool is empty.
    private Sprite RentSprite()
    {
        if (m_FreeSprites.Count > 0)
            return m_FreeSprites.Pop();

        // Create a runtime sprite over the whole source texture; its geometry is overwritten per use
        // (so rect/pivot/pixelsPerUnit don't affect the rendered fragment, only the sampled texture).
        var texture = m_Sprite.texture;
        var sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            m_Sprite.pixelsPerUnit,
            extrude: 0,
            SpriteMeshType.FullRect);
        sprite.hideFlags = HideFlags.HideAndDontSave;

        m_AllSprites.Add(sprite);
        return sprite;
    }

    // Grow the reusable scratch geometry buffers if a body needs more than the current capacity.
    // NativeArrays are native memory, so this never produces managed GC.
    private void EnsureScratchCapacity(int vertexCount, int indexCount)
    {
        if (!m_ScratchPositions.IsCreated || m_ScratchPositions.Length < vertexCount)
        {
            if (m_ScratchPositions.IsCreated) m_ScratchPositions.Dispose();
            if (m_ScratchUVs.IsCreated) m_ScratchUVs.Dispose();
            m_ScratchPositions = new NativeArray<Vector3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            m_ScratchUVs = new NativeArray<Vector2>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        if (!m_ScratchIndices.IsCreated || m_ScratchIndices.Length < indexCount)
        {
            if (m_ScratchIndices.IsCreated) m_ScratchIndices.Dispose();
            m_ScratchIndices = new NativeArray<ushort>(indexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
    }

    // Remove a body's draw item, return its sprite to the pool, and destroy the body.
    private void DestroySpriteDrawItem(PhysicsBody physicsBody)
    {
        if (!m_DrawItems.TryGetValue(physicsBody, out var spriteId))
            throw new ArgumentException("Could not find draw item.", nameof(physicsBody));

        // Return the sprite to the pool for reuse (do not destroy it).
        if (Resources.EntityIdToObject(spriteId) is Sprite sprite)
            m_FreeSprites.Push(sprite);

        // Remove the draw item.
        m_DrawItems.Remove(physicsBody);

        // Destroy the body.
        physicsBody.Destroy();
    }

    // Return all active sprites to the pool and destroy their bodies (used on scene reset).
    private void ResetDrawItems()
    {
        if (!m_DrawItems.IsCreated)
            return;

        foreach (var drawItem in m_DrawItems)
        {
            if (Resources.EntityIdToObject(drawItem.Value) is Sprite sprite)
                m_FreeSprites.Push(sprite);

            // Destroy the body.
            drawItem.Key.Destroy();
        }

        m_DrawItems.Clear();
    }
}
