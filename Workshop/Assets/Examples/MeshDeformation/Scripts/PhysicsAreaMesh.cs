using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Adds a mesh of triangle shapes to the physics body of the Physics Pose on this GameObject or a parent, made from a closed outline that can then be deformed while it runs.
/// The outline is split into triangles once, using a Physics Composer limited to three vertices per polygon, and each triangle becomes one polygon shape.
/// </summary>
/// <remarks>
/// Deforming moves the corners of the triangles and writes the result straight onto the existing shapes, so the triangles stay connected and are never rebuilt or split again.
/// A triangle that collapses to less than the minimum area cannot be a valid polygon, so it is left as it was and counted in <see cref="invalidCount"/>.
/// Use this instead of a Physics Area Polygon when one outline should become many shapes that follow a changing mesh.
/// </remarks>
public sealed class PhysicsAreaMesh : PhysicsArea
{
    /// <summary>
    /// How <see cref="Deform"/> moves each corner of the mesh.
    /// </summary>
    public enum DeformMode
    {
        /// <summary>
        /// Leaves the mesh as it was authored.
        /// </summary>
        None,

        /// <summary>
        /// Slides each corner sideways in proportion to its height, leaning the mesh over.
        /// </summary>
        Shear,

        /// <summary>
        /// Widens the top of the mesh and pinches the bottom, or the other way around for a negative amount.
        /// </summary>
        Taper,

        /// <summary>
        /// Rotates each corner around the origin by an angle that grows with its distance from it.
        /// </summary>
        Twist
    }

    /// <summary>
    /// Creates a closed outline of vertices placed around the origin, each at a random distance from it, which never crosses itself.
    /// The same seed always gives the same outline.
    /// </summary>
    /// <param name="seed">Chooses the outline. Values below one are treated as one.</param>
    /// <param name="vertexCount">How many vertices the outline has. Fewer than three gives an empty array.</param>
    /// <param name="radius">The average distance of the vertices from the origin, in meters.</param>
    /// <param name="jitter">How far each vertex can be nearer or further than the radius, in meters.</param>
    /// <returns>The vertices in anti-clockwise order, in the space of the area.</returns>
    public static Vector2[] CreateRandomOutline(int seed, int vertexCount, float radius, float jitter)
    {
        if (vertexCount < 3)
            return new Vector2[0];

        var random = new Unity.Mathematics.Random((uint)math.max(1, seed));
        var outline = new Vector2[vertexCount];

        for (var i = 0; i < vertexCount; ++i)
        {
            var angle = 2f * math.PI * i / vertexCount;
            var distance = radius + random.NextFloat(-jitter, jitter);
            outline[i] = new Vector2(math.cos(angle) * distance, math.sin(angle) * distance);
        }

        return outline;
    }

    /// <summary>
    /// Moves the corners of every triangle to their deformed positions, and writes the result onto the shapes.
    /// Does nothing until the area has built its shapes, or if the shapes no longer match the triangles.
    /// </summary>
    /// <remarks>
    /// The deformation is always worked out from the mesh as it was built, never from the last deformation, so calling this each frame with a changing amount does not build up error.
    /// An amount of zero puts the mesh back to how it was built.
    /// </remarks>
    /// <param name="mode">How the corners are moved.</param>
    /// <param name="amount">How strongly they are moved. The useful range is minus one to one, and the sign reverses the direction.</param>
    public void Deform(DeformMode mode, float amount)
    {
        if (!m_RestVertices.IsCreated || shapeCount != m_TriangleCount)
            return;

        var job = new DeformJob
        {
            restVertices = m_RestVertices,
            mode = mode,
            amount = amount,
            extent = m_Extent,
            minArea = m_MinArea,
            relative = m_Relative,
            template = PolygonGeometry.defaultGeometry,
            polygons = m_Polygons,
            valid = m_Valid
        };
        job.Schedule(m_TriangleCount, 64).Complete();

        var invalid = 0;

        for (var i = 0; i < m_TriangleCount; ++i)
        {
            if (m_Valid[i] == 0)
            {
                ++invalid;
                continue;
            }

            var shape = this[i];
            shape.polygonGeometry = m_Polygons[i];
        }

        m_InvalidCount = invalid;
    }

    /// <summary>
    /// The closed outline the mesh is made from, in the space of this area.
    /// </summary>
    /// <remarks>
    /// The outline must not cross itself, and its vertices run anti-clockwise.
    /// Changing this does not change the running shapes until <see cref="PhysicsArea.ApplyGeometry"/> is called, which splits the new outline into triangles again.
    /// An outline with fewer than three vertices produces no shapes.
    /// </remarks>
    public Vector2[] outline
    {
        get => m_Outline;
        set => m_Outline = value;
    }

    /// <summary>
    /// Whether the outline is split into triangles with Delaunay tessellation, which prefers triangles that are close to equilateral over long thin ones.
    /// </summary>
    /// <remarks>
    /// Changing this does not change the running shapes until <see cref="PhysicsArea.ApplyGeometry"/> is called, which splits the outline into triangles again.
    /// </remarks>
    public bool useDelaunay
    {
        get => m_UseDelaunay;
        set => m_UseDelaunay = value;
    }

    /// <summary>
    /// The area below which a deformed triangle counts as collapsed, in square meters.
    /// </summary>
    /// <remarks>
    /// A collapsed triangle keeps the shape it had before it collapsed, and is counted in <see cref="invalidCount"/>.
    /// </remarks>
    public float minArea
    {
        get => m_MinArea;
        set => m_MinArea = value;
    }

    /// <summary>
    /// How many triangles the outline was split into, which is also how many shapes the area has.
    /// </summary>
    public int triangleCount => m_TriangleCount;

    /// <summary>
    /// How many triangles were collapsed the last time <see cref="Deform"/> ran.
    /// </summary>
    public int invalidCount => m_InvalidCount;

    /// <summary>
    /// Whether this area always produces exactly one shape.
    /// </summary>
    /// <remarks>
    /// Returns false: the outline becomes one shape for every triangle.
    /// </remarks>
    protected override bool producesSingleShape => false;

    /// <summary>
    /// Whether this area's geometry has a radius that the Transform scale can affect.
    /// </summary>
    /// <remarks>
    /// Returns false: the triangles have no radius, so the inspector's Scale Radius option does not apply.
    /// </remarks>
    protected override bool usesScaleRadius => false;

    /// <summary>
    /// Whether this area is eligible to be referenced by a Physics Area Composite layer.
    /// </summary>
    /// <remarks>
    /// Returns false: the shapes are rebuilt from the outline, so a composite cannot use this area as input.
    /// </remarks>
    protected override bool canContributeToComposer => false;

    /// <summary>
    /// Splits the outline into triangles and creates one polygon shape for each.
    /// </summary>
    /// <param name="body">Body to create the shapes on.</param>
    /// <param name="relative">Matrix mapping the outline in the space of this area into the local space of the body.</param>
    /// <param name="applyScaleRadius">Unused: the triangles have no radius.</param>
    /// <param name="shapeDefinition">Shape definition to create every shape with.</param>
    /// <param name="shapes">List to append the created shapes to, in the order of the triangles.</param>
    protected override void GenerateShapes(PhysicsBody body, Matrix4x4 relative, bool applyScaleRadius, PhysicsShapeDefinition shapeDefinition, NativeList<PhysicsShape> shapes)
    {
        DisposeNative();

        m_Relative = relative;
        m_InvalidCount = 0;

        if (m_Outline == null || m_Outline.Length < 3)
            return;

        // Limiting the composer to three vertices per polygon makes it return the outline as triangles.
        var composer = PhysicsComposer.Create(Allocator.Temp);
        composer.maxPolygonVertices = 3;
        composer.useDelaunay = m_UseDelaunay;
        composer.AddLayer(m_Outline, PhysicsTransform.identity, PhysicsComposer.Operation.OR, 0, false);

        var triangles = composer.CreatePolygonGeometry(Allocator.Temp);
        composer.Destroy();

        m_TriangleCount = triangles.Length;

        if (m_TriangleCount == 0)
            return;

        m_RestVertices = new NativeArray<float2>(m_TriangleCount * 3, Allocator.Persistent);
        m_Polygons = new NativeArray<PolygonGeometry>(m_TriangleCount, Allocator.Persistent);
        m_Valid = new NativeArray<byte>(m_TriangleCount, Allocator.Persistent);

        m_Extent = 1f;
        var corners = new NativeArray<Vector2>(3, Allocator.Temp);

        for (var i = 0; i < m_TriangleCount; ++i)
        {
            var triangle = triangles[i];

            for (var corner = 0; corner < 3; ++corner)
            {
                var vertex = triangle.vertices[corner];
                m_RestVertices[i * 3 + corner] = vertex;
                m_Extent = math.max(m_Extent, math.length(vertex));
                corners[corner] = relative.MultiplyPoint3x4(vertex);
            }

            var polygon = PolygonGeometry.Create(corners.AsReadOnlySpan(), 0f);
            shapes.Add(body.CreateShape(polygon, shapeDefinition));
        }

        corners.Dispose();
        triangles.Dispose();
    }

    /// <summary>
    /// Releases the memory the deformation uses when the area is disabled.
    /// </summary>
    protected override void OnAreaDisable() => DisposeNative();

    // Moves each triangle's corners to their deformed positions and builds the polygon for it, one triangle per job step.
    // The deformation is a function of position alone, so corners shared by neighbouring triangles land in the same place.
    [BurstCompile]
    private struct DeformJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> restVertices;
        public DeformMode mode;
        public float amount;
        public float extent;
        public float minArea;
        public float4x4 relative;

        // The polygon each triangle starts from, made on the main thread because reading its default runs a static constructor Burst cannot compile.
        public PolygonGeometry template;

        [WriteOnly] public NativeArray<PolygonGeometry> polygons;
        [WriteOnly] public NativeArray<byte> valid;

        public void Execute(int t)
        {
            var corner = t * 3;
            var a = DeformPoint(restVertices[corner]);
            var b = DeformPoint(restVertices[corner + 1]);
            var c = DeformPoint(restVertices[corner + 2]);

            // Twice the signed area of the triangle, where a value near zero means it has collapsed to a line or a point.
            if (math.abs(Cross(b - a, c - a)) < minArea * 2f)
            {
                valid[t] = 0;
                polygons[t] = default;
                return;
            }

            // Bring the corners into the space of the body, which can flip the winding if the area is mirrored by its scale.
            a = ToBody(a);
            b = ToBody(b);
            c = ToBody(c);

            // A polygon needs its vertices anti-clockwise so its edge normals point outward.
            if (Cross(b - a, c - a) < 0f)
                (b, c) = (c, b);

            var polygon = template;
            polygon.count = 3;
            polygon.radius = 0f;

            polygon.vertices[0] = (Vector2)a;
            polygon.vertices[1] = (Vector2)b;
            polygon.vertices[2] = (Vector2)c;

            polygon.normals[0] = (Vector2)EdgeNormal(a, b);
            polygon.normals[1] = (Vector2)EdgeNormal(b, c);
            polygon.normals[2] = (Vector2)EdgeNormal(c, a);

            polygon.centroid = (Vector2)((a + b + c) / 3f);

            polygons[t] = polygon;
            valid[t] = 1;
        }

        // Moves one corner according to the mode and amount.
        private float2 DeformPoint(float2 p)
        {
            switch (mode)
            {
                case DeformMode.Shear:
                    return new float2(p.x + amount * 1.5f * p.y, p.y);

                case DeformMode.Taper:
                    return new float2(p.x * (1f + amount * (p.y / extent)), p.y);

                case DeformMode.Twist:
                {
                    var angle = amount * math.PI * (math.length(p) / extent);
                    math.sincos(angle, out var s, out var c);
                    return new float2(p.x * c - p.y * s, p.x * s + p.y * c);
                }

                default:
                    return p;
            }
        }

        private float2 ToBody(float2 p) => math.transform(relative, new float3(p, 0f)).xy;

        // The outward normal of an anti-clockwise edge from p to q points to the right of its direction.
        private static float2 EdgeNormal(float2 p, float2 q)
        {
            var d = q - p;
            return math.normalize(new float2(d.y, -d.x));
        }

        private static float Cross(float2 u, float2 v) => u.x * v.y - u.y * v.x;
    }

    // Releases the native arrays, which are recreated every time the shapes are built.
    private void DisposeNative()
    {
        if (m_RestVertices.IsCreated)
            m_RestVertices.Dispose();

        if (m_Polygons.IsCreated)
            m_Polygons.Dispose();

        if (m_Valid.IsCreated)
            m_Valid.Dispose();

        m_TriangleCount = 0;
    }

    #region Internal

    [SerializeField] Vector2[] m_Outline = CreateRandomOutline(1, 25, 6f, 2.5f);
    [SerializeField] bool m_UseDelaunay = true;
    [SerializeField, Min(0f)] float m_MinArea = 0.01f;

    // The corners of every triangle as built, three per triangle, in the space of the area.
    NativeArray<float2> m_RestVertices;

    // The polygon built for each triangle by the last deformation, and whether that triangle was valid.
    NativeArray<PolygonGeometry> m_Polygons;
    NativeArray<byte> m_Valid;

    // Maps the area's space into the body's space, kept from the last time the shapes were built.
    Matrix4x4 m_Relative = Matrix4x4.identity;

    // The furthest any corner is from the origin, which scales the taper and the twist so they behave the same for any outline size.
    float m_Extent = 1f;

    int m_TriangleCount;
    int m_InvalidCount;

    #endregion
}
