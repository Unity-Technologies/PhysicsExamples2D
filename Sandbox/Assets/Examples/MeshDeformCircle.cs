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
[ExampleScene("Experiments", "Builds a deformable 2D mesh and approximates its interior area with Circle geometry.")]
public sealed class MeshDeformCircle : SandboxExampleBehaviour
{
    private enum DeformMode
    {
        None,
        Shear,
        Taper,
        Twist
    }

    private enum Packing
    {
        Square,
        Hex
    }

    private enum Placement
    {
        Bands,       // interior grid + dense contour band
        MaximalDisk  // greedy maximal inscribed disks (fewest circles)
    }

    private int m_Seed = 1;
    private int m_OutlineVertexCount = 25;
    private DeformMode m_DeformMode = DeformMode.Shear;
    private float m_Amount = 0.5f;
    private bool m_Animate = true;
    private float m_InteriorSpacing = 0.5f;
    private float m_ContourSpacing = 0.15f;
    private float m_ContourBand = 0.35f;
    private bool m_ShowMesh = false;
    private Placement m_Placement = Placement.Bands;
    private float m_MaxDiskRadius = 1f;
    private Packing m_Packing = Packing.Hex;
    private bool m_ClampToContour = true;
    private bool m_ShowGuard = true;

    private readonly Color m_MeshColor = Color.lightSeaGreen;
    private readonly Color m_GuardColor = Color.yellowNice;
    private readonly Color m_CircleColor = Color.dodgerBlue;

    // Rest holds the undeformed vertex positions (managed; only read on the main thread).
    // The topology is fixed at build: only positions change when deformed.
    private Vector2[] m_RestVertices;

    // Largest rest vertex distance from the origin, used to normalize the deformers.
    private float m_RefExtent;

    // --- Grid binding (the demonstrated idea) -------------------------------------------------
    // The interior is covered by a uniform grid of circles, one per kept grid point. Each circle is
    // bound to the triangle it lands in by barycentric weights, then carried by that triangle under
    // deformation, so coverage rides the mesh while the circle count stays constant.
    //
    // The data is laid out as NativeArrays of blittable types so the per-frame update runs as a
    // Burst-compiled IJobParallelFor. Topology and bindings are built once; only m_LiveVertices and
    // the two outputs change each frame.
    private NativeArray<int> m_TriIndices;        // triangle vertex indices, three per triangle
    private NativeArray<float2> m_LiveVertices;   // deformed vertex positions, written each frame
    private NativeArray<int> m_CircleTriangle;    // carrier triangle per circle
    private NativeArray<float3> m_CircleWeights;   // barycentric weights per circle
    private NativeArray<float> m_TriangleRestArea; // per-triangle rest area, for radius scaling
    private NativeArray<float> m_CircleClampRadius; // rest distance to the contour, for boundary clamping
    private NativeArray<float> m_CircleBaseRadius;  // rest radius per circle (interior or contour spacing)
    private NativeArray<CircleGeometry> m_CircleGeometry; // live circle (center + radius), written by the job

    // Split of the current circle count, for the read-only display.
    private int m_InteriorCircleCount;
    private int m_ContourCircleCount;

    // Read-only display of how many circles the current binding manipulates each frame.
    private Label m_CircleCountLabel;

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

        // Spacing, band, and packing re-place the circles, so they rebuild.
        AddSlider("Interior Spacing", m_InteriorSpacing, 0.1f, 3f, v => m_InteriorSpacing = v, rebuild: true);
        AddSlider("Contour Spacing", m_ContourSpacing, 0.05f, 1f, v => m_ContourSpacing = v, rebuild: true);
        AddSlider("Contour Band", m_ContourBand, 0f, 2f, v => m_ContourBand = v, rebuild: true);
        AddEnum("Placement", m_Placement, v => m_Placement = v, rebuild: true);
        AddSlider("Max Disk Radius", m_MaxDiskRadius, 0.1f, 5f, v => m_MaxDiskRadius = v, rebuild: true);
        AddEnum("Packing", m_Packing, v => m_Packing = v, rebuild: true);

        // Clamp uses precomputed contour distances, so it toggles live without a rebuild.
        AddToggle("Clamp to Contour", m_ClampToContour, v => m_ClampToContour = v);

        // Deformation is applied per frame, so these do not rebuild the mesh.
        AddEnum("Deform", m_DeformMode, v => m_DeformMode = v);
        AddSlider("Amount", m_Amount, -1f, 1f, v => m_Amount = v);
        AddToggle("Animate", m_Animate, v => m_Animate = v);
        AddToggle("Show Mesh", m_ShowMesh, v => m_ShowMesh = v);
        AddToggle("Show Guard", m_ShowGuard, v => m_ShowGuard = v);

        // Read-only circle count, refreshed on each rebuild in SetupScene.
        m_CircleCountLabel = AddElement(new Label());
        m_CircleCountLabel.AddToClassList("hash-label");
    }

    protected override void SetupScene()
    {
        // Rebuild from scratch: drop any prior allocation first.
        DisposeNative();

        // Build the rest mesh: a concave outline triangulated into an indexed mesh.
        m_RestVertices = BuildOutline(m_Seed, m_OutlineVertexCount);
        var triangles = Triangulate(m_RestVertices);

        // Cache the reference extent so deformers scale sensibly regardless of seed/size.
        m_RefExtent = 1f;
        foreach (var v in m_RestVertices)
            m_RefExtent = math.max(m_RefExtent, v.magnitude);

        // Mirror topology into native storage and seed the live positions at the rest pose.
        m_TriIndices = new NativeArray<int>(triangles, Allocator.Persistent);
        m_LiveVertices = new NativeArray<float2>(m_RestVertices.Length, Allocator.Persistent);
        for (var i = 0; i < m_RestVertices.Length; i++)
            m_LiveVertices[i] = m_RestVertices[i];

        // Bind the coverage grid to the rest mesh.
        BuildGridBinding();

        // Refresh the read-only count display (label persists across rebuilds).
        if (m_CircleCountLabel != null)
            m_CircleCountLabel.text = $"Circles: {m_CircleGeometry.Length} ({m_InteriorCircleCount} interior + {m_ContourCircleCount} contour)";
    }

    private void Update()
    {
        if (!m_TriIndices.IsCreated)
            return;

        // Testbed: produce the deformed mesh vertices.
        DeformVertices();

        // Core: carry the grid circles along with the deformed mesh (Burst job).
        ApplyDeformationToGrid();

        if (m_ShowMesh)
            DrawMesh();
        DrawGrid();
        if (m_ShowGuard)
            DrawExteriorGuard();
    }

    // ==========================================================================================
    // Core: mesh -> grid mapping (the idea being demonstrated)
    //
    // The grid is stamped once over the rest pose; circles that land in a triangle are kept and
    // bound to it by barycentric weights. Under deformation each circle is rebuilt from its
    // triangle's moved corners, and its radius scales by that triangle's area change. None of this
    // depends on the mesh being well-shaped — the triangulation is only a deformation carrier.
    // ==========================================================================================

    // One-time: stamp a grid over the rest mesh and bind each kept point to its carrier triangle.
    private void BuildGridBinding()
    {
        // Bounds of the rest mesh.
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        foreach (var v in m_RestVertices)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }

        // Rest area of each triangle, used later to scale the radius under deformation.
        var triangleCount = m_TriIndices.Length / 3;
        m_TriangleRestArea = new NativeArray<float>(triangleCount, Allocator.Persistent);
        for (var t = 0; t < triangleCount; t++)
            m_TriangleRestArea[t] = TriangleArea(RestCorner(t, 0), RestCorner(t, 1), RestCorner(t, 2));

        // Square packing seals at ~0.75 * spacing, hex at ~0.62. Each set sizes from its own spacing.
        var packingFactor = m_Packing == Packing.Hex ? 0.62f : 0.75f;

        var triangles = new List<int>();
        var weights = new List<float3>();
        var clamp = new List<float>();
        var baseRadii = new List<float>();

        if (m_Placement == Placement.MaximalDisk)
        {
            // Single set of greedy maximal inscribed disks (no interior/contour split).
            StampMaximalDisks(min, max, triangleCount, triangles, weights, clamp, baseRadii);
            m_InteriorCircleCount = triangles.Count;
            m_ContourCircleCount = 0;
        }
        else
        {
            // Interior: points at least one band deep from the contour. Coarse spacing, large circles.
            StampGrid(min, max, triangleCount, m_InteriorSpacing, m_InteriorSpacing * packingFactor,
                m_ContourBand, float.MaxValue, triangles, weights, clamp, baseRadii);
            m_InteriorCircleCount = triangles.Count;

            // Contour: points within the band. Fine spacing, small circles that tile the edge densely.
            StampGrid(min, max, triangleCount, m_ContourSpacing, m_ContourSpacing * packingFactor,
                0f, m_ContourBand, triangles, weights, clamp, baseRadii);
            m_ContourCircleCount = triangles.Count - m_InteriorCircleCount;
        }

        m_CircleTriangle = new NativeArray<int>(triangles.ToArray(), Allocator.Persistent);
        m_CircleWeights = new NativeArray<float3>(weights.ToArray(), Allocator.Persistent);
        m_CircleClampRadius = new NativeArray<float>(clamp.ToArray(), Allocator.Persistent);
        m_CircleBaseRadius = new NativeArray<float>(baseRadii.ToArray(), Allocator.Persistent);
        m_CircleGeometry = new NativeArray<CircleGeometry>(triangles.Count, Allocator.Persistent);
    }

    // Stamps one grid pass: keeps points inside a triangle whose contour distance is in [minDist,
    // maxDist), binding each to its triangle with the given rest radius. Used for both interior and
    // contour bands, each with its own spacing and radius.
    private void StampGrid(Vector2 min, Vector2 max, int triangleCount, float spacing, float radius,
        float minDist, float maxDist, List<int> triangles, List<float3> weights, List<float> clamp, List<float> baseRadii)
    {
        var dx = spacing;
        var dy = m_Packing == Packing.Hex ? spacing * 0.8660254f : spacing;
        var row = 0;
        for (var y = min.y; y <= max.y; y += dy, row++)
        {
            // Hex packing offsets alternate rows by half a step.
            var xStart = min.x + (m_Packing == Packing.Hex && (row & 1) == 1 ? dx * 0.5f : 0f);
            for (var x = xStart; x <= max.x; x += dx)
            {
                var p = new Vector2(x, y);
                for (var t = 0; t < triangleCount; t++)
                {
                    var a = RestCorner(t, 0);
                    var b = RestCorner(t, 1);
                    var c = RestCorner(t, 2);
                    if (!PointInTriangle(p, a, b, c))
                        continue;

                    // Inside this triangle: keep only if its contour distance is in this pass's band.
                    var d = DistanceToOutline(p);
                    if (d >= minDist && d < maxDist)
                    {
                        triangles.Add(t);
                        weights.Add(Barycentric(p, a, b, c));
                        clamp.Add(d);
                        baseRadii.Add(radius);
                    }

                    break;
                }
            }
        }
    }

    // Alternative placement: cover the area with maximal inscribed disks. Candidates are sampled at
    // the contour spacing; each candidate's potential radius is its distance to the boundary (the
    // biggest disk that fits there). A greedy pass takes the largest disks first and skips any
    // candidate already covered, so a few big interior disks replace many small grid circles.
    private void StampMaximalDisks(Vector2 min, Vector2 max, int triangleCount,
        List<int> triangles, List<float3> weights, List<float> clamp, List<float> baseRadii)
    {
        // Gather candidates (position, carrier triangle, distance-to-boundary) on a fine grid.
        var candPos = new List<Vector2>();
        var candTri = new List<int>();
        var candDist = new List<float>();
        for (var y = min.y; y <= max.y; y += m_ContourSpacing)
        for (var x = min.x; x <= max.x; x += m_ContourSpacing)
        {
            var p = new Vector2(x, y);
            for (var t = 0; t < triangleCount; t++)
            {
                if (!PointInTriangle(p, RestCorner(t, 0), RestCorner(t, 1), RestCorner(t, 2)))
                    continue;

                candPos.Add(p);
                candTri.Add(t);
                candDist.Add(DistanceToOutline(p));
                break;
            }
        }

        // Order candidates by distance descending (largest disks first).
        var order = new List<int>(candPos.Count);
        for (var i = 0; i < candPos.Count; i++)
            order.Add(i);
        order.Sort((a, b) => candDist[b].CompareTo(candDist[a]));

        // Greedily accept disks; skip candidates already covered. The overlap margin (< 1) keeps
        // neighbouring disks overlapping so the union has no seams.
        const float coverOverlap = 0.8f;
        var accCenter = new List<Vector2>();
        var accRadius = new List<float>();
        foreach (var idx in order)
        {
            var p = candPos[idx];

            // Cap the disk so deep-interior points don't produce a few giant disks; the greedy
            // coverage then backfills with more medium disks, evening out the scaling.
            var r = math.min(candDist[idx], m_MaxDiskRadius);
            if (r <= 0f)
                continue;

            var covered = false;
            for (var k = 0; k < accCenter.Count; k++)
            {
                if (Vector2.Distance(p, accCenter[k]) <= accRadius[k] * coverOverlap)
                {
                    covered = true;
                    break;
                }
            }

            if (covered)
                continue;

            accCenter.Add(p);
            accRadius.Add(r);

            var t = candTri[idx];
            triangles.Add(t);
            weights.Add(Barycentric(p, RestCorner(t, 0), RestCorner(t, 1), RestCorner(t, 2)));
            clamp.Add(r);
            baseRadii.Add(r);
        }
    }

    // Per-frame: rebuild every circle's center and radius from the deformed carrier triangle.
    private void ApplyDeformationToGrid()
    {
        var job = new DeformGridJob
        {
            LiveVertices = m_LiveVertices,
            TriIndices = m_TriIndices,
            CircleTriangle = m_CircleTriangle,
            CircleWeights = m_CircleWeights,
            TriangleRestArea = m_TriangleRestArea,
            CircleClampRadius = m_CircleClampRadius,
            CircleBaseRadius = m_CircleBaseRadius,
            Clamp = m_ClampToContour,
            CircleGeometry = m_CircleGeometry
        };

        // Results are needed this frame (draw / shape update), so complete immediately.
        job.Schedule(m_CircleGeometry.Length, 64).Complete();
    }

    // Burst kernel: one circle per index, fully independent. Center is the barycentric blend on the
    // moved corners; radius scales with sqrt of the triangle's area change (length ~ sqrt(area)).
    [BurstCompile]
    private struct DeformGridJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> LiveVertices;
        [ReadOnly] public NativeArray<int> TriIndices;
        [ReadOnly] public NativeArray<int> CircleTriangle;
        [ReadOnly] public NativeArray<float3> CircleWeights;
        [ReadOnly] public NativeArray<float> TriangleRestArea;
        [ReadOnly] public NativeArray<float> CircleClampRadius;
        [ReadOnly] public NativeArray<float> CircleBaseRadius;
        public bool Clamp;

        [WriteOnly] public NativeArray<CircleGeometry> CircleGeometry;

        public void Execute(int i)
        {
            var t = CircleTriangle[i];
            var t3 = t * 3;
            var a = LiveVertices[TriIndices[t3]];
            var b = LiveVertices[TriIndices[t3 + 1]];
            var c = LiveVertices[TriIndices[t3 + 2]];

            var w = CircleWeights[i];
            var center = w.x * a + w.y * b + w.z * c;

            // Clamp caps the rest radius at the distance to the contour, so boundary circles sit
            // tangent to the outline instead of spilling past it. Interior circles are unaffected.
            var baseRadius = CircleBaseRadius[i];
            var restRadius = Clamp ? math.min(baseRadius, CircleClampRadius[i]) : baseRadius;

            var area = 0.5f * math.abs(Cross(b - a, c - a));
            var ratio = area / math.max(TriangleRestArea[t], 1e-6f);
            var radius = restRadius * math.sqrt(ratio);

            CircleGeometry[i] = new CircleGeometry { center = center, radius = radius };
        }

        private static float Cross(float2 u, float2 v) => u.x * v.y - u.y * v.x;
    }

    // Draws the coverage circles in a single batched call.
    private void DrawGrid()
    {
        World.DrawGeometry(m_CircleGeometry.AsReadOnlySpan(), PhysicsTransform.identity, m_CircleColor);
    }

    // Imitates the exterior guard: a closed strip through the deformed outline vertices. This stands
    // in for a PhysicsChain on the boundary (as in ChainShapeDeform) without creating shapes yet.
    private void DrawExteriorGuard()
    {
        var outline = m_LiveVertices.Reinterpret<Vector2>();
        World.DrawLineStrip(PhysicsTransform.identity, outline, true, m_GuardColor, 0f);
    }

    private Vector2 RestCorner(int triangle, int corner) => m_RestVertices[m_TriIndices[triangle * 3 + corner]];

    private void DisposeNative()
    {
        if (m_TriIndices.IsCreated) m_TriIndices.Dispose();
        if (m_LiveVertices.IsCreated) m_LiveVertices.Dispose();
        if (m_CircleTriangle.IsCreated) m_CircleTriangle.Dispose();
        if (m_CircleWeights.IsCreated) m_CircleWeights.Dispose();
        if (m_TriangleRestArea.IsCreated) m_TriangleRestArea.Dispose();
        if (m_CircleClampRadius.IsCreated) m_CircleClampRadius.Dispose();
        if (m_CircleBaseRadius.IsCreated) m_CircleBaseRadius.Dispose();
        if (m_CircleGeometry.IsCreated) m_CircleGeometry.Dispose();
    }

    // Shortest distance from p to the rest outline (the polygon boundary edges).
    private float DistanceToOutline(Vector2 p)
    {
        var best = float.MaxValue;
        var n = m_RestVertices.Length;
        for (var i = 0; i < n; i++)
        {
            var a = m_RestVertices[i];
            var b = m_RestVertices[(i + 1) % n];
            best = math.min(best, PointSegmentDistance(p, a, b));
        }

        return best;
    }

    private static float PointSegmentDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var t = math.clamp(Vector2.Dot(p - a, ab) / math.max(Vector2.Dot(ab, ab), 1e-6f), 0f, 1f);
        return Vector2.Distance(p, a + t * ab);
    }

    // ==========================================================================================
    // Testbed scaffolding: building and deforming a stand-in mesh. Not part of the idea above.
    // ==========================================================================================

    // Writes deformed positions into m_LiveVertices from m_RestVertices.
    private void DeformVertices()
    {
        // Animate oscillates the amount through zero so the rest pose is part of the cycle.
        var amount = m_Animate ? m_Amount * math.sin(Time.time) : m_Amount;

        for (var i = 0; i < m_RestVertices.Length; i++)
            m_LiveVertices[i] = Deform(m_RestVertices[i], amount);
    }

    // Maps a rest position to its deformed position. Each op is a pure function of position.
    private float2 Deform(Vector2 p, float amount)
    {
        switch (m_DeformMode)
        {
            case DeformMode.Shear:
            {
                // Slide x in proportion to height.
                return new float2(p.x + amount * 1.5f * p.y, p.y);
            }

            case DeformMode.Taper:
            {
                // Widen one end and pinch the other, producing slivers at the narrow end.
                var scaleX = 1f + amount * (p.y / m_RefExtent);
                return new float2(p.x * scaleX, p.y);
            }

            case DeformMode.Twist:
            {
                // Rotate each point by an angle that grows with distance from the origin.
                var angle = amount * math.PI * (p.magnitude / m_RefExtent);
                var s = math.sin(angle);
                var c = math.cos(angle);
                return new float2(p.x * c - p.y * s, p.x * s + p.y * c);
            }

            case DeformMode.None:
            default:
                return new float2(p.x, p.y);
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

    private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c) => 0.5f * math.abs(Cross(b - a, c - a));

    // Barycentric weights of p within triangle (a, b, c). The three weights sum to 1.
    private static float3 Barycentric(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        var den = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
        var wa = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / den;
        var wb = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / den;
        return new float3(wa, wb, 1f - wa - wb);
    }
}
