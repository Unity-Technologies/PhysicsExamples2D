using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.U2D.Physics;
using Random = Unity.Mathematics.Random;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Experiments", "Builds a deformable 2D mesh and maps Polygon geometry directly to its triangles.")]
public sealed class MeshDeformPolygon : SandboxExampleBehaviour
{
    private enum DeformMode
    {
        None,
        Shear,
        Taper,
        Twist
    }

    private int m_Seed = 1;
    private int m_OutlineVertexCount = 25;
    private DeformMode m_DeformMode = DeformMode.Shear;
    private float m_Amount = 0.5f;
    private bool m_Animate = true;
    private float m_MinArea = 0.01f;
    private bool m_ShowMesh = false;
    private bool m_ShowGuard = true;

    private readonly Color m_MeshColor = Color.lightSeaGreen;
    private readonly Color m_GuardColor = Color.yellowNice;
    private readonly Color m_PolygonColor = Color.dodgerBlue;
    private readonly Color m_InvalidColor = Color.orangeRed;

    // Rest holds the undeformed vertex positions; the topology is fixed at build, only positions move.
    // Everything is laid out as NativeArrays of blittable types so the per-frame work runs as
    // Burst-compiled jobs and the result draws in one batched call.
    private NativeArray<float2> m_RestVertices;   // undeformed positions, built once
    private NativeArray<float2> m_LiveVertices;   // deformed positions, written each frame
    private NativeArray<int> m_TriIndices;        // triangle vertex indices, three per triangle
    private NativeArray<PolygonGeometry> m_Polygons; // one polygon per triangle, written by the build job
    private NativeArray<byte> m_Valid;            // per-triangle validity (0 = degenerate), written by the build job
    private NativeArray<PolygonGeometry> m_DrawScratch; // valid polygons compacted for the batched draw

    // Largest rest vertex distance from the origin, used to normalize the deformers.
    private float m_RefExtent;

    // Read-only display of the triangle count and how many are currently degenerate.
    private Label m_CountLabel;

    protected override float CameraSize => 12f;
    protected override Vector2 CameraPosition => Vector2.zero;

    protected override void OnExampleDisable()
    {
        DisposeNative();
    }

    protected override void SetupOptions()
    {
        AddSliderInt("Seed", m_Seed, 1, 1000, v => m_Seed = v, rebuild: true);
        AddSliderInt("Outline Vertices", m_OutlineVertexCount, 6, 64, v => m_OutlineVertexCount = v, rebuild: true);

        // Minimum triangle area below which a triangle is treated as degenerate (drawn red, not built).
        AddSlider("Min Area", m_MinArea, 0f, 0.5f, v => m_MinArea = v);

        // Deformation is applied per frame, so these do not rebuild the mesh.
        AddEnum("Deform", m_DeformMode, v => m_DeformMode = v);
        AddSlider("Amount", m_Amount, -1f, 1f, v => m_Amount = v);
        AddToggle("Animate", m_Animate, v => m_Animate = v);
        AddToggle("Show Mesh", m_ShowMesh, v => m_ShowMesh = v);
        AddToggle("Show Guard", m_ShowGuard, v => m_ShowGuard = v);

        m_CountLabel = AddElement(new Label());
        m_CountLabel.AddToClassList("hash-label");
    }

    protected override void SetupScene()
    {
        // Rebuild from scratch: drop any prior allocation first.
        DisposeNative();

        // Build the rest mesh: a concave outline triangulated into an indexed mesh (managed, one-time).
        var restVertices = BuildOutline(m_Seed, m_OutlineVertexCount);
        var triangles = Triangulate(restVertices);
        var triangleCount = triangles.Length / 3;

        // Cache the reference extent so deformers scale sensibly regardless of seed/size.
        m_RefExtent = 1f;
        foreach (var v in restVertices)
            m_RefExtent = math.max(m_RefExtent, v.magnitude);

        // Mirror topology into native storage and seed the live positions at the rest pose.
        m_RestVertices = new NativeArray<float2>(restVertices.Length, Allocator.Persistent);
        m_LiveVertices = new NativeArray<float2>(restVertices.Length, Allocator.Persistent);
        for (var i = 0; i < restVertices.Length; i++)
        {
            m_RestVertices[i] = restVertices[i];
            m_LiveVertices[i] = restVertices[i];
        }

        m_TriIndices = new NativeArray<int>(triangles, Allocator.Persistent);
        m_Polygons = new NativeArray<PolygonGeometry>(triangleCount, Allocator.Persistent);
        m_Valid = new NativeArray<byte>(triangleCount, Allocator.Persistent);
        m_DrawScratch = new NativeArray<PolygonGeometry>(triangleCount, Allocator.Persistent);
    }

    private void Update()
    {
        if (!m_TriIndices.IsCreated)
            return;

        // Core: deform the mesh, then build one Polygon per triangle (two chained Burst jobs).
        DeformAndBuild();

        // Core: draw the built polygons (and any degenerate triangles in red).
        DrawPolygons();

        if (m_ShowMesh)
            DrawMesh();
        if (m_ShowGuard)
            DrawExteriorGuard();
    }

    // ==========================================================================================
    // Core: mesh triangle -> PolygonGeometry mapping (the idea being demonstrated)
    //
    // Each mesh triangle becomes a 3-vertex PolygonGeometry built directly from its deformed corners.
    // We never call PolygonGeometry.Create (which runs a convex-hull pass) or PolygonGeometry.isValid
    // (relatively expensive across thousands of polygons). Instead we do the small amount of work the
    // validity actually needs, knowing we always have exactly three vertices:
    //   - one cross product gives twice the signed area: its sign is the winding, its magnitude the
    //     degeneracy test,
    //   - negative winding is fixed up to counter-clockwise by swapping two vertices,
    //   - only a near-zero area is genuinely invalid (a winding fix-up cannot rescue a collinear
    //     triangle); those are drawn red and not built,
    //   - the three outward edge normals are computed directly.
    //
    // The build runs as a Burst IJobParallelFor over triangles so it scales to high triangle counts.
    // ==========================================================================================

    // Per frame: deform the vertices, then build a Polygon per triangle. Both jobs are Burst-compiled;
    // the build depends on the deform and we complete immediately because the results draw this frame.
    private void DeformAndBuild()
    {
        // Animate oscillates the amount through zero so the rest pose is part of the cycle.
        var amount = m_Animate ? m_Amount * math.sin(Time.time) : m_Amount;

        var deformJob = new DeformJob
        {
            RestVertices = m_RestVertices,
            Mode = m_DeformMode,
            Amount = amount,
            RefExtent = m_RefExtent,
            LiveVertices = m_LiveVertices
        };
        var deformHandle = deformJob.Schedule(m_RestVertices.Length, 64);

        var buildJob = new BuildJob
        {
            LiveVertices = m_LiveVertices,
            TriIndices = m_TriIndices,
            MinArea = m_MinArea,
            Polygons = m_Polygons,
            Valid = m_Valid
        };
        buildJob.Schedule(m_Polygons.Length, 64, deformHandle).Complete();
    }

    // Burst kernel: build one counter-clockwise 3-vertex PolygonGeometry per triangle, computing
    // winding, degeneracy, and normals directly. Degenerate triangles set Valid = 0 and are left for
    // the main thread to draw red.
    [BurstCompile]
    private struct BuildJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> LiveVertices;
        [ReadOnly] public NativeArray<int> TriIndices;
        public float MinArea;

        [WriteOnly] public NativeArray<PolygonGeometry> Polygons;
        [WriteOnly] public NativeArray<byte> Valid;

        public void Execute(int t)
        {
            var t3 = t * 3;
            var a = LiveVertices[TriIndices[t3]];
            var b = LiveVertices[TriIndices[t3 + 1]];
            var c = LiveVertices[TriIndices[t3 + 2]];

            // Twice the signed area: sign is the winding, magnitude is the degeneracy test.
            var doubleArea = Cross(b - a, c - a);

            // Collinear / near-zero area: a winding fix-up cannot rescue this.
            if (math.abs(doubleArea) < MinArea * 2f)
            {
                Valid[t] = 0;
                Polygons[t] = default;
                return;
            }

            // Ensure counter-clockwise winding so the outward normals are correct.
            if (doubleArea < 0f)
                (b, c) = (c, b);

            var polygon = PolygonGeometry.defaultGeometry;
            polygon.count = 3;
            polygon.radius = 0f;

            polygon.vertices[0] = (Vector2)a;
            polygon.vertices[1] = (Vector2)b;
            polygon.vertices[2] = (Vector2)c;

            // Outward normal of a CCW edge p->q is perpendicular to the right: (d.y, -d.x).
            polygon.normals[0] = (Vector2)EdgeNormal(a, b);
            polygon.normals[1] = (Vector2)EdgeNormal(b, c);
            polygon.normals[2] = (Vector2)EdgeNormal(c, a);

            polygon.centroid = (Vector2)((a + b + c) / 3f);

            Polygons[t] = polygon;
            Valid[t] = 1;
        }

        private static float2 EdgeNormal(float2 p, float2 q)
        {
            var d = q - p;
            return math.normalize(new float2(d.y, -d.x));
        }

        private static float Cross(float2 u, float2 v) => u.x * v.y - u.y * v.x;
    }

    // Draws the built polygons: valid ones compacted into one batched call, degenerate ones in red.
    // The compaction is a cheap main-thread pass (drawing can't happen inside a job), and degenerate
    // triangles are rare so the per-line red draw does not matter.
    private void DrawPolygons()
    {
        var world = World;
        var triangleCount = m_Polygons.Length;
        var validCount = 0;
        var invalidCount = 0;

        for (var t = 0; t < triangleCount; t++)
        {
            if (m_Valid[t] != 0)
            {
                m_DrawScratch[validCount++] = m_Polygons[t];
                continue;
            }

            // Degenerate: draw the triangle outline in red and leave the shape unbuilt.
            var t3 = t * 3;
            var a = (Vector2)m_LiveVertices[m_TriIndices[t3]];
            var b = (Vector2)m_LiveVertices[m_TriIndices[t3 + 1]];
            var c = (Vector2)m_LiveVertices[m_TriIndices[t3 + 2]];
            world.DrawLine(a, b, m_InvalidColor);
            world.DrawLine(b, c, m_InvalidColor);
            world.DrawLine(c, a, m_InvalidColor);
            invalidCount++;
        }

        if (validCount > 0)
            world.DrawGeometry(m_DrawScratch.AsReadOnlySpan().Slice(0, validCount), PhysicsTransform.identity, m_PolygonColor);

        if (m_CountLabel != null)
            m_CountLabel.text = $"Triangles: {triangleCount}   Invalid: {invalidCount}";
    }

    // Imitates the exterior guard: a closed strip through the deformed outline vertices. This stands
    // in for a PhysicsChain on the boundary (as in ChainShapeDeform) without creating shapes yet.
    private void DrawExteriorGuard()
    {
        var outline = m_LiveVertices.Reinterpret<Vector2>();
        World.DrawLineStrip(PhysicsTransform.identity, outline, true, m_GuardColor, 0f);
    }

    // ==========================================================================================
    // Testbed scaffolding: building and deforming a stand-in mesh. Not part of the idea above.
    // ==========================================================================================

    // Burst kernel: map each rest vertex to its deformed position. Each op is a pure function of
    // position, so vertices are fully independent.
    [BurstCompile]
    private struct DeformJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> RestVertices;
        public DeformMode Mode;
        public float Amount;
        public float RefExtent;

        [WriteOnly] public NativeArray<float2> LiveVertices;

        public void Execute(int i)
        {
            LiveVertices[i] = Deform(RestVertices[i], Amount, Mode, RefExtent);
        }

        private static float2 Deform(float2 p, float amount, DeformMode mode, float refExtent)
        {
            switch (mode)
            {
                case DeformMode.Shear:
                {
                    // Slide x in proportion to height.
                    return new float2(p.x + amount * 1.5f * p.y, p.y);
                }

                case DeformMode.Taper:
                {
                    // Widen one end and pinch the other, producing slivers at the narrow end.
                    var scaleX = 1f + amount * (p.y / refExtent);
                    return new float2(p.x * scaleX, p.y);
                }

                case DeformMode.Twist:
                {
                    // Rotate each point by an angle that grows with distance from the origin.
                    var angle = amount * math.PI * (math.length(p) / refExtent);
                    var s = math.sin(angle);
                    var co = math.cos(angle);
                    return new float2(p.x * co - p.y * s, p.x * s + p.y * co);
                }

                case DeformMode.None:
                default:
                    return p;
            }
        }
    }

    // Draws the triangle edges of the live mesh as a wireframe.
    private void DrawMesh()
    {
        var world = World;

        for (var t = 0; t < m_TriIndices.Length; t += 3)
        {
            var a = (Vector2)m_LiveVertices[m_TriIndices[t]];
            var b = (Vector2)m_LiveVertices[m_TriIndices[t + 1]];
            var c = (Vector2)m_LiveVertices[m_TriIndices[t + 2]];

            world.DrawLine(a, b, m_MeshColor);
            world.DrawLine(b, c, m_MeshColor);
            world.DrawLine(c, a, m_MeshColor);
        }
    }

    private void DisposeNative()
    {
        if (m_RestVertices.IsCreated) m_RestVertices.Dispose();
        if (m_LiveVertices.IsCreated) m_LiveVertices.Dispose();
        if (m_TriIndices.IsCreated) m_TriIndices.Dispose();
        if (m_Polygons.IsCreated) m_Polygons.Dispose();
        if (m_Valid.IsCreated) m_Valid.Dispose();
        if (m_DrawScratch.IsCreated) m_DrawScratch.Dispose();
    }

    // Generates a simple concave outline: points around a circle with a seeded, per-point radius
    // variation. Monotonic angle keeps the polygon star-shaped, so it never self-intersects.
    private static Vector2[] BuildOutline(int seed, int vertexCount)
    {
        const float baseRadius = 6f;
        const float radiusJitter = 2.5f;

        var rng = new Random((uint)math.max(1, seed));
        var outline = new Vector2[vertexCount];

        for (var i = 0; i < vertexCount; i++)
        {
            var angle = 2f * math.PI * i / vertexCount;
            var radius = baseRadius + rng.NextFloat(-radiusJitter, radiusJitter);
            outline[i] = new Vector2(math.cos(angle) * radius, math.sin(angle) * radius);
        }

        return outline;
    }

    // Ear-clipping triangulation of a simple polygon. Returns triangle indices (three per triangle)
    // referencing the input vertices. Assumes a non-self-intersecting outline.
    private static int[] Triangulate(Vector2[] polygon)
    {
        var triangles = new List<int>();
        var n = polygon.Length;
        if (n < 3)
            return triangles.ToArray();

        // Build the working list of vertex indices, forced to counter-clockwise winding.
        var remaining = new List<int>(n);
        if (SignedArea(polygon) < 0f)
            for (var i = n - 1; i >= 0; i--) remaining.Add(i);
        else
            for (var i = 0; i < n; i++) remaining.Add(i);

        // Clip ears until a single triangle remains. The guard prevents an infinite loop on
        // degenerate input.
        var guard = n * n;
        while (remaining.Count > 3 && guard-- > 0)
        {
            var clipped = false;
            var count = remaining.Count;

            for (var i = 0; i < count; i++)
            {
                var i0 = remaining[(i - 1 + count) % count];
                var i1 = remaining[i];
                var i2 = remaining[(i + 1) % count];

                var a = polygon[i0];
                var b = polygon[i1];
                var c = polygon[i2];

                // Skip reflex or collinear corners (not convex in a CCW polygon).
                if (Cross(b - a, c - b) <= 0f)
                    continue;

                // Skip if any other vertex falls inside the candidate ear triangle.
                var contains = false;
                for (var j = 0; j < count; j++)
                {
                    var p = remaining[j];
                    if (p == i0 || p == i1 || p == i2)
                        continue;

                    if (PointInTriangle(polygon[p], a, b, c))
                    {
                        contains = true;
                        break;
                    }
                }

                if (contains)
                    continue;

                triangles.Add(i0);
                triangles.Add(i1);
                triangles.Add(i2);
                remaining.RemoveAt(i);
                clipped = true;
                break;
            }

            // No ear found this pass — bail to avoid spinning on bad geometry.
            if (!clipped)
                break;
        }

        if (remaining.Count == 3)
        {
            triangles.Add(remaining[0]);
            triangles.Add(remaining[1]);
            triangles.Add(remaining[2]);
        }

        return triangles.ToArray();
    }

    private static float SignedArea(Vector2[] polygon)
    {
        var area = 0f;
        for (var i = 0; i < polygon.Length; i++)
        {
            var p0 = polygon[i];
            var p1 = polygon[(i + 1) % polygon.Length];
            area += p0.x * p1.y - p1.x * p0.y;
        }

        return area * 0.5f;
    }

    private static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // Inside when p is on the same side (left) of all three CCW edges.
        var d1 = Cross(b - a, p - a);
        var d2 = Cross(c - b, p - b);
        var d3 = Cross(a - c, p - c);
        return d1 >= 0f && d2 >= 0f && d3 >= 0f;
    }
}
