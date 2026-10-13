using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;
using Random = Unity.Mathematics.Random;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Experiments", "Builds a deformable 2D mesh and maps Polygon geometry directly to its triangles.",
    Purpose = "An experiment in mapping a deforming mesh onto physics geometry.\nA random outline is split into triangles by a Physics Composer limited to three vertices per polygon, and every triangle becomes a three vertex Polygon Geometry built directly from its deformed corners. This skips the convex hull step of creating a polygon, because a triangle is always convex, and only does the checks a triangle needs: its winding, and whether it has collapsed. The deforming and the building both run as Burst jobs, so it scales to many triangles.\nOnly the geometry is built and drawn. No bodies or shapes are created.",
    Controls = "Seed: chooses the random outline. The mesh is rebuilt when it changes.\nOutline Vertices: how many vertices the outline has, and so how many triangles the mesh has. The mesh is rebuilt when it changes.\nDelaunay: how the outline is split into triangles. When on, the composer prefers triangles that are close to equilateral over long thin ones. The mesh is rebuilt when it changes.\nMin Area: a triangle smaller than this is treated as collapsed. It is drawn in red and its polygon is not built.\nDeform: how the mesh is deformed. Shear slides it sideways in proportion to its height, Taper widens one end and pinches the other, and Twist rotates it more the further it is from the center.\nAmount: how strongly the mesh is deformed.\nAnimate: swings the amount back and forth through zero, so the undeformed mesh is part of the cycle.")]
public sealed class MeshDeformation : SandboxExampleBehaviour
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
    private bool m_UseDelaunay = true;
    private DeformMode m_DeformMode = DeformMode.Shear;
    private float m_Amount = 0.5f;
    private bool m_Animate = true;
    private float m_MinArea = 0.01f;

    private readonly Color m_PolygonColor = Color.dodgerBlue;
    private readonly Color m_InvalidColor = Color.orangeRed;

    // Rest holds the undeformed corner positions, three per triangle; the triangles are fixed at build, only positions move.
    // Everything is laid out as NativeArrays of blittable types so the per-frame work runs as
    // Burst-compiled jobs and the result draws in one batched call.
    private NativeArray<float2> m_RestVertices;   // undeformed positions, built once
    private NativeArray<float2> m_LiveVertices;   // deformed positions, written each frame
    private NativeArray<PolygonGeometry> m_Polygons; // one polygon per triangle, written by the build job
    private NativeArray<byte> m_Valid;            // per-triangle validity (0 = degenerate), written by the build job
    private NativeArray<PolygonGeometry> m_DrawScratch; // valid polygons compacted for the batched draw

    // Largest rest vertex distance from the origin, used to normalize the deformers.
    private float m_RefExtent;

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

        AddToggle("Delaunay", m_UseDelaunay, v => m_UseDelaunay = v, rebuild: true);

        // Minimum triangle area below which a triangle is treated as degenerate (drawn red, not built).
        AddSlider("Min Area", m_MinArea, 0f, 0.5f, v => m_MinArea = v);

        // Deformation is applied per frame, so these do not rebuild the mesh.
        AddEnum("Deform", m_DeformMode, v => m_DeformMode = v);
        AddSlider("Amount", m_Amount, -1f, 1f, v => m_Amount = v);
        AddToggle("Animate", m_Animate, v => m_Animate = v);
    }

    protected override void SetupScene()
    {
        // Rebuild from scratch: drop any prior allocation first.
        DisposeNative();

        // Split a concave outline into triangles with a Physics Composer. Limiting it to three vertices per polygon makes it return triangles.
        var outline = BuildOutline(m_Seed, m_OutlineVertexCount);
        var composer = PhysicsComposer.Create(Allocator.Temp);
        composer.maxPolygonVertices = 3;
        composer.useDelaunay = m_UseDelaunay;
        composer.AddLayer(outline, PhysicsTransform.identity, PhysicsComposer.Operation.OR, 0, false);

        var triangles = composer.CreatePolygonGeometry(Allocator.Temp);
        composer.Destroy();

        var triangleCount = triangles.Length;
        m_RestVertices = new NativeArray<float2>(triangleCount * 3, Allocator.Persistent);
        m_LiveVertices = new NativeArray<float2>(triangleCount * 3, Allocator.Persistent);

        // Cache the reference extent so deformers scale sensibly regardless of seed/size.
        m_RefExtent = 1f;

        // Copy the triangle corners into native storage and seed the live positions at the rest pose.
        for (var t = 0; t < triangleCount; t++)
        {
            for (var corner = 0; corner < 3; corner++)
            {
                var vertex = (float2)triangles[t].vertices[corner];
                m_RestVertices[t * 3 + corner] = vertex;
                m_LiveVertices[t * 3 + corner] = vertex;
                m_RefExtent = math.max(m_RefExtent, math.length(vertex));
            }
        }

        triangles.Dispose();

        m_Polygons = new NativeArray<PolygonGeometry>(triangleCount, Allocator.Persistent);
        m_Valid = new NativeArray<byte>(triangleCount, Allocator.Persistent);
        m_DrawScratch = new NativeArray<PolygonGeometry>(triangleCount, Allocator.Persistent);
    }

    private void Update()
    {
        if (!m_RestVertices.IsCreated)
            return;

        // Core: deform the mesh, then build one Polygon per triangle (two chained Burst jobs).
        DeformAndBuild();

        // Core: draw the built polygons (and any degenerate triangles in red).
        DrawPolygons();
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
            MinArea = m_MinArea,
            Template = PolygonGeometry.defaultGeometry,
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
        public float MinArea;

        // The default geometry each polygon starts from. It is created on the main thread because reading PolygonGeometry.defaultGeometry runs a static constructor Burst cannot compile.
        public PolygonGeometry Template;

        [WriteOnly] public NativeArray<PolygonGeometry> Polygons;
        [WriteOnly] public NativeArray<byte> Valid;

        public void Execute(int t)
        {
            var t3 = t * 3;
            var a = LiveVertices[t3];
            var b = LiveVertices[t3 + 1];
            var c = LiveVertices[t3 + 2];

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

            var polygon = Template;
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

        for (var t = 0; t < triangleCount; t++)
        {
            if (m_Valid[t] != 0)
            {
                m_DrawScratch[validCount++] = m_Polygons[t];
                continue;
            }

            // Degenerate: draw the triangle outline in red and leave the shape unbuilt.
            var t3 = t * 3;
            var a = (Vector2)m_LiveVertices[t3];
            var b = (Vector2)m_LiveVertices[t3 + 1];
            var c = (Vector2)m_LiveVertices[t3 + 2];
            world.DrawLine(a, b, m_InvalidColor);
            world.DrawLine(b, c, m_InvalidColor);
            world.DrawLine(c, a, m_InvalidColor);
        }

        if (validCount > 0)
            world.DrawGeometry(m_DrawScratch.AsReadOnlySpan().Slice(0, validCount), PhysicsTransform.identity, m_PolygonColor);
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

    private void DisposeNative()
    {
        if (m_RestVertices.IsCreated) m_RestVertices.Dispose();
        if (m_LiveVertices.IsCreated) m_LiveVertices.Dispose();
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

}
