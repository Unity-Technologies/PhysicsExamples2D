using System.Collections.Generic;

using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Clicking anywhere on one of two buildings cuts a hole out of it, and the pieces that come free fall and can be blown apart.
/// Every piece is one Physics Pose with a Physics Area Polygon per polygon, and each one is drawn with a runtime sprite cut from the same source texture so the picture stays consistent across a break.
/// </summary>
/// <remarks>
/// A piece still crossing the ground line drawn below the buildings stays static, while one cut free of it becomes dynamic and falls, so a building only collapses once a hole is cut all the way through it.
/// The fragments broken out of a hole always fall as debris, and an optional explosion can scatter them outward from the click.
/// Every piece is destroyed once it reaches the surrounding wall, and its sprite is returned to a small pool rather than being destroyed, so steady clicking never keeps creating new textures.
/// </remarks>
public sealed class SpriteDestructionContents : MonoBehaviour, PhysicsAreaCallbacks.IContactCallback
{
    /// <summary>
    /// The radius of the hole a click breaks out of a building, in meters.
    /// </summary>
    public float fragmentRadius
    {
        get => m_FragmentRadius;
        set
        {
            m_FragmentRadius = value;
            UpdateFragmentMask();
        }
    }

    /// <summary>
    /// Whether the pieces broken out of a hole fall as debris, rather than simply vanishing.
    /// </summary>
    public bool fragmentCreate
    {
        get => m_FragmentCreate;
        set => m_FragmentCreate = value;
    }

    /// <summary>
    /// How many pieces each hole is split into.
    /// </summary>
    public int fragmentCount
    {
        get => m_FragmentCount;
        set => m_FragmentCount = value;
    }

    /// <summary>
    /// The friction every new piece is given.
    /// </summary>
    public float fragmentFriction
    {
        get => m_FragmentFriction;
        set => m_FragmentFriction = value;
    }

    /// <summary>
    /// The bounciness every new piece is given.
    /// </summary>
    public float fragmentBounciness
    {
        get => m_FragmentBounciness;
        set => m_FragmentBounciness = value;
    }

    /// <summary>
    /// How hard the debris from a hole is blown outward, or zero to leave it falling under gravity alone.
    /// </summary>
    public float fragmentForce
    {
        get => m_FragmentForce;
        set => m_FragmentForce = value;
    }

    /// <summary>
    /// How much harder than normal gravity pulls on every piece.
    /// </summary>
    public float gravityScale
    {
        get => m_GravityScale;
        set
        {
            m_GravityScale = value;

            var world = PhysicsWorld.defaultWorld;
            world.gravity = m_WorldGravity * m_GravityScale;
        }
    }

    // Destroys whatever hit the surrounding wall.
    void PhysicsAreaCallbacks.IContactCallback.OnContactBegin2D(PhysicsAreaCallbacks.ContactBeginEvent beginEvent)
    {
        var shapeA = beginEvent.beginEvent.shapeA;
        var shapeB = beginEvent.beginEvent.shapeB;

        if (!shapeA.isValid || !shapeB.isValid)
            return;

        var categoryA = shapeA.contactFilter.categories;
        var categoryB = shapeB.contactFilter.categories;

        if (categoryA != GroundMask && categoryB != GroundMask)
            return;

        Remove(categoryA == GroundMask ? beginEvent.areaB : beginEvent.areaA);
    }

    void PhysicsAreaCallbacks.IContactCallback.OnContactEnd2D(PhysicsAreaCallbacks.ContactEndEvent endEvent)
    {
    }

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);

        if (m_CameraManipulator != null)
            m_CameraManipulator.DisableManipulators = true;

        var world = PhysicsWorld.defaultWorld;
        m_WorldGravity = world.gravity;
        world.gravity = m_WorldGravity * m_GravityScale;

        m_RenderParams = new RenderParams(m_SpriteMaterial) { camera = null };
        UpdateFragmentMask();
    }

    // Every changed world and camera setting belongs outside this scene, so it would otherwise stay changed into whichever example is loaded next.
    private void OnDisable()
    {
        foreach (var sprite in m_AllSprites)
        {
            if (sprite != null)
                Destroy(sprite);
        }

        m_AllSprites.Clear();
        m_FreeSprites.Clear();
        m_DrawItems.Clear();

        if (m_CameraManipulator != null)
            m_CameraManipulator.DisableManipulators = false;

        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;

        if (m_FragmentMask.IsCreated)
            m_FragmentMask.Dispose();
    }

    private void Start()
    {
        CreateBuilding(Vector2.left * 7f);
        CreateBuilding(Vector2.right * 6f);
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        if (world.paused)
            return;

        world.DrawGeometry(GroundLineGeometry, GroundLineTransform, Color.saddleBrown);

        var currentMouse = Mouse.current;

        if (currentMouse == null || m_CameraManipulator == null)
            return;

        var worldPosition = (Vector2)m_CameraManipulator.Camera.ScreenToWorldPoint(currentMouse.position.value);

        if (currentMouse.leftButton.wasPressedThisFrame)
            DestructAtPosition(worldPosition);

        world.DrawGeometry(m_FragmentMask, worldPosition, Color.dodgerBlue, 0f, PhysicsWorld.DrawFillOptions.Outline);
    }

    // Draws every piece's runtime sprite for the frame, using the physics world's own transform so the picture always matches where its body actually is.
    private void LateUpdate()
    {
        if (m_DrawItems.Count == 0)
            return;

        foreach (var drawItem in m_DrawItems)
        {
            var body = drawItem.Key;

            if (!body.isValid)
                continue;

            var bodyTransform = body.transform;
            var position = new Vector3(bodyTransform.position.x, bodyTransform.position.y, 0f);
            var rotation = Quaternion.Euler(0f, 0f, bodyTransform.rotation.degrees);

            var spriteParams = new SpriteParams(drawItem.Value);
            Graphics.RenderSprite(m_RenderParams, spriteParams, 0, Matrix4x4.TRS(position, rotation, Vector3.one));
        }
    }

    // Builds the physics outline of the source sprite into one static building, with its own runtime sprite drawn from the same polygons.
    private void CreateBuilding(Vector2 worldPosition)
    {
        if (m_Sprite == null || m_PiecePrefab == null)
            return;

        var physicsOutlineCount = m_Sprite.GetPhysicsOutlineCount();

        if (physicsOutlineCount == 0)
            return;

        var composer = PhysicsComposer.Create();
        composer.useDelaunay = true;

        var vertexPath = new NativeList<Vector2>(Allocator.Temp);
        var outlineVertices = new List<Vector2>();

        for (var i = 0; i < physicsOutlineCount; ++i)
        {
            if (m_Sprite.GetPhysicsOutline(i, outlineVertices) > 0)
            {
                foreach (var vertex in outlineVertices)
                    vertexPath.Add(vertex);

                composer.AddLayer(vertexPath.AsArray(), PhysicsTransform.identity);
            }

            vertexPath.Clear();
        }

        using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one);
        vertexPath.Dispose();
        composer.Destroy();

        if (polygons.Length == 0)
            return;

        BuildPiece(polygons, worldPosition, Quaternion.identity, isDynamic: false, Vector2.zero);
    }

    // Cuts a hole out of whichever destructible building the position overlaps, replacing it with one piece per surviving island plus a piece of debris per fragment broken out.
    private void DestructAtPosition(Vector2 hitPosition)
    {
        var world = PhysicsWorld.defaultWorld;

        using var hits = world.OverlapPoint(hitPosition, new PhysicsQuery.QueryFilter { categories = DestructibleMask, hitCategories = DestructibleMask });

        if (hits.Length == 0)
            return;

        var destructibleBody = hits[0].shape.body;
        var destructibleWasStatic = destructibleBody.type == PhysicsBody.BodyType.Static;
        var destructibleVelocity = destructibleBody.linearVelocity;

        using var destructibleShapes = destructibleBody.GetShapes();
        var targetPolygons = new NativeList<PolygonGeometry>(destructibleShapes.Length, Allocator.Temp);

        foreach (var shape in destructibleShapes)
        {
            if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
                targetPolygons.Add(shape.polygonGeometry);
        }

        var targetGeometry = new PhysicsDestructor.FragmentGeometry(destructibleBody.transform, targetPolygons.AsReadOnly());
        targetPolygons.Dispose();

        var fragmentPoints = new NativeArray<Vector2>(m_FragmentCount, Allocator.Temp);
        fragmentPoints[0] = hitPosition;

        for (var i = 1; i < m_FragmentCount; ++i)
        {
            var rotate = PhysicsRotate.FromRadians(m_Random.NextFloat(0f, PhysicsMath.PI));
            var radius = m_Random.NextFloat(0.05f, m_FragmentRadius);
            fragmentPoints[i] = hitPosition + rotate.direction * radius;
        }

        var maskGeometry = new PhysicsDestructor.FragmentGeometry(hitPosition, m_FragmentMask);
        using var fragmentResults = PhysicsDestructor.Fragment(targetGeometry, maskGeometry, fragmentPoints, Allocator.Temp);
        fragmentPoints.Dispose();

        Remove(destructibleBody);

        var fragmentTransform = fragmentResults.transform;
        var fragmentPosition = new Vector3(fragmentTransform.position.x, fragmentTransform.position.y, 0f);
        var fragmentRotation = Quaternion.Euler(0f, 0f, fragmentTransform.rotation.degrees);

        var unbrokenGeometry = fragmentResults.unbrokenGeometry;

        foreach (var islandRange in fragmentResults.unbrokenGeometryIslands)
        {
            var islandGeometry = unbrokenGeometry.GetSubArray(islandRange.start, islandRange.length);
            var falls = !destructibleWasStatic || !CrossesGroundLine(islandGeometry, fragmentTransform);

            BuildPiece(islandGeometry, fragmentPosition, fragmentRotation, falls, falls ? destructibleVelocity : Vector2.zero);
        }

        if (m_FragmentCreate)
        {
            foreach (var geometry in fragmentResults.brokenGeometry)
            {
                var singlePolygon = new NativeArray<PolygonGeometry>(1, Allocator.Temp) { [0] = geometry };
                BuildPiece(singlePolygon, fragmentPosition, fragmentRotation, isDynamic: true, destructibleVelocity, isDebris: true);
                singlePolygon.Dispose();
            }
        }

        if (m_FragmentCreate && m_FragmentForce > 0f)
        {
            world.Explode(new PhysicsWorld.ExplosionDefinition
            {
                position = hitPosition,
                hitCategories = DebrisMask,
                impulsePerLength = m_FragmentForce,
                radius = m_FragmentRadius * 2f
            });
        }
    }

    // Returns whether any polygon of an island crosses the ground line, which is what keeps a static building's island standing.
    private static bool CrossesGroundLine(NativeArray<PolygonGeometry> islandGeometry, PhysicsTransform islandTransform)
    {
        foreach (var geometry in islandGeometry)
        {
            if (geometry.Intersect(islandTransform, GroundLineGeometry, GroundLineTransform).pointCount > 0)
                return true;
        }

        return false;
    }

    // Instantiates one piece from the specified polygons: one Physics Area Polygon per polygon, its own runtime sprite, and (for a falling piece) contact callbacks.
    private void BuildPiece(NativeArray<PolygonGeometry> polygons, Vector3 position, Quaternion rotation, bool isDynamic, Vector2 velocity, bool isDebris = false)
    {
        var piece = Instantiate(m_PiecePrefab, position, rotation);

        var pose = piece.GetComponent<PhysicsPose>();
        var bodyDefinition = pose.definition;

        if (isDynamic)
        {
            bodyDefinition.type = PhysicsBody.BodyType.Dynamic;
            bodyDefinition.fastCollisionsAllowed = true;
            bodyDefinition.linearVelocity = velocity;
        }

        pose.definition = bodyDefinition;

        var shapeDefinition = new PhysicsShapeDefinition
        {
            contactFilter = isDebris
                ? new PhysicsShape.ContactFilter { categories = DebrisMask, contacts = GroundMask | DestructibleMask | ObstacleMask | DebrisMask }
                : new PhysicsShape.ContactFilter { categories = DestructibleMask, contacts = GroundMask | DebrisMask | DestructibleMask },
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_FragmentFriction, bounciness = m_FragmentBounciness }
        };

        for (var i = 0; i < polygons.Length; ++i)
        {
            var area = i == 0 ? piece.GetComponent<PhysicsAreaPolygon>() : piece.AddComponent<PhysicsAreaPolygon>();
            area.definition = shapeDefinition;
            area.geometry = polygons[i];

            if (isDynamic)
            {
                area.callbackSource = PhysicsArea.CallbackSourceType.Area;
                area.callbackTarget = this;
            }
        }

        piece.SetActive(true);

        CreateDrawItem(pose.body, polygons);
    }

    // Builds one piece's renderable geometry from its polygons and uploads it to a pooled runtime sprite.
    // Vertex positions are the polygon vertices in body-local space, and the UVs map each vertex directly onto the source sprite's own image so the texture stays consistent across a break.
    private void CreateDrawItem(PhysicsBody physicsBody, NativeArray<PolygonGeometry> polygons)
    {
        var spriteExtents = m_Sprite.bounds.extents * 2f;
        var worldToTexture = new Vector2(1f / spriteExtents.x, 1f / spriteExtents.y);
        var textureOffset = new Vector2(0.5f, 0.5f);

        var vertexCount = 0;
        var indexCount = 0;

        foreach (var polygon in polygons)
        {
            var count = polygon.count;

            if (count < 3)
                continue;

            vertexCount += count;
            indexCount += (count - 2) * 3;
        }

        if (vertexCount == 0)
            return;

        var positions = new NativeArray<Vector3>(vertexCount, Allocator.Temp);
        var uvs = new NativeArray<Vector2>(vertexCount, Allocator.Temp);
        var indices = new NativeArray<ushort>(indexCount, Allocator.Temp);

        var vertexIndex = 0;
        var triangleIndex = 0;

        foreach (var polygon in polygons)
        {
            var polygonVertexCount = polygon.count;

            if (polygonVertexCount < 3)
                continue;

            var polygonVertices = polygon.vertices;
            var rootVertex = vertexIndex;

            for (var i = 0; i < polygonVertexCount; ++i)
            {
                var vertex = polygonVertices[i];
                positions[vertexIndex] = new Vector3(vertex.x, vertex.y, 0f);
                uvs[vertexIndex] = vertex * worldToTexture + textureOffset;
                ++vertexIndex;
            }

            for (var i = 2; i < polygonVertexCount; ++i)
            {
                indices[triangleIndex++] = (ushort)rootVertex;
                indices[triangleIndex++] = (ushort)(rootVertex + i);
                indices[triangleIndex++] = (ushort)(rootVertex + i - 1);
            }
        }

        var sprite = RentSprite();
        sprite.SetVertexCount(vertexCount);
        sprite.SetVertexAttribute(VertexAttribute.Position, positions);
        sprite.SetVertexAttribute(VertexAttribute.TexCoord0, uvs);
        sprite.SetIndices(indices);

        positions.Dispose();
        uvs.Dispose();
        indices.Dispose();

        m_DrawItems.Add(physicsBody, sprite);
    }

    // Rebuilds the circle cut out of a building at every click, as polygons centered on the origin, with a jittered radius so the hole reads as a rough break rather than a perfect circle.
    private void UpdateFragmentMask()
    {
        if (m_FragmentMask.IsCreated)
            m_FragmentMask.Dispose();

        var vertices = new NativeList<Vector2>(MaskVertexCount, Allocator.Temp);
        var stride = PhysicsMath.TAU / MaskVertexCount;

        for (var i = 0; i < MaskVertexCount; ++i)
        {
            var angle = i * stride;
            PhysicsMath.CosSin(angle, out var cosine, out var sine);
            var radius = m_Random.NextFloat(m_FragmentRadius * 0.9f, m_FragmentRadius * 1.1f);
            vertices.Add(new Vector2(cosine * radius, sine * radius));
        }

        m_FragmentMask = PolygonGeometry.CreatePolygons(vertices.AsArray(), PhysicsTransform.identity, Vector2.one, Allocator.Persistent);
        vertices.Dispose();
    }

    // Rents a reusable runtime sprite from the pool, creating a new one over the whole source texture only when the pool is empty.
    private Sprite RentSprite()
    {
        if (m_FreeSprites.Count > 0)
            return m_FreeSprites.Pop();

        var texture = m_Sprite.texture;
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), m_Sprite.pixelsPerUnit, extrude: 0, SpriteMeshType.FullRect);
        sprite.hideFlags = HideFlags.HideAndDontSave;

        m_AllSprites.Add(sprite);
        return sprite;
    }

    // Removes a piece: returns its sprite to the pool and destroys its body.
    private void Remove(PhysicsArea area)
    {
        if (area == null)
            return;

        Remove(area.pose.body);
    }

    // The body is owned by its Physics Pose component, so destroying the body directly is refused; destroying the GameObject releases it instead.
    private void Remove(PhysicsBody body)
    {
        if (m_DrawItems.TryGetValue(body, out var sprite))
        {
            m_FreeSprites.Push(sprite);
            m_DrawItems.Remove(body);
        }

        if (body.owner is not PhysicsPose pose)
            return;

        var target = pose.gameObject;
        target.SetActive(false);
        Destroy(target);
    }

    #region Internal

    // The contact categories every piece is authored with.
    static readonly PhysicsMask ObstacleMask = new(1);
    static readonly PhysicsMask GroundMask = new(2);
    static readonly PhysicsMask DestructibleMask = new(3);
    static readonly PhysicsMask DebrisMask = new(4);

    // The line below the buildings that a static piece has to cross to stay standing, and how many vertices the jittered hole outline uses.
    static readonly SegmentGeometry GroundLineGeometry = new() { point1 = new Vector2(-100f, 0f), point2 = new Vector2(100f, 0f) };
    static readonly PhysicsTransform GroundLineTransform = new(Vector2.down * 7.25f);
    const int MaskVertexCount = 36;

    const uint RandomSeed = 0x32628473;

    [SerializeField] Sprite m_Sprite;
    [SerializeField] Material m_SpriteMaterial;
    [SerializeField] GameObject m_PiecePrefab;
    [SerializeField] CameraManipulator m_CameraManipulator;
    [SerializeField, Range(0.5f, 3f)] float m_FragmentRadius = 2f;
    [SerializeField] bool m_FragmentCreate = true;
    [SerializeField, Range(1, 50)] int m_FragmentCount = 20;
    [SerializeField, Range(0f, 1f)] float m_FragmentFriction = 0.5f;
    [SerializeField, Range(0f, 0.75f)] float m_FragmentBounciness;
    [SerializeField, Range(0f, 50f)] float m_FragmentForce = 10f;
    [SerializeField, Range(0.1f, 10f)] float m_GravityScale = 5f;

    readonly Dictionary<PhysicsBody, Sprite> m_DrawItems = new();
    readonly Stack<Sprite> m_FreeSprites = new();
    readonly List<Sprite> m_AllSprites = new();
    RenderParams m_RenderParams;
    NativeArray<PolygonGeometry> m_FragmentMask;
    Vector2 m_WorldGravity;
    Random m_Random;

    #endregion
}
