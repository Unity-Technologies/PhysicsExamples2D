using System.Collections.Generic;
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
    private Vector2[] m_RestVertices;
    private Vector2[] m_LiveVertices;
    private int[] m_TriIndices;

    // Largest rest vertex distance from the origin, used to normalize the deformers.
    private float m_RefExtent;

    // Read-only display of the triangle count and how many are currently degenerate.
    private Label m_CountLabel;

    protected override float CameraSize => 12f;
    protected override Vector2 CameraPosition => Vector2.zero;

    protected override void SetupOptions()
    {
        AddSliderInt("Seed", m_Seed, 1, 1000, v => m_Seed = v, rebuild: true);
        AddSliderInt("Outline Vertices", m_OutlineVertexCount, 6, 64, v => m_OutlineVertexCount = v, rebuild: true);

        // Minimum triangle area below which a triangle is treated as degenerate (drawn red, not set).
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
        // Build the rest mesh: a concave outline triangulated into an indexed mesh.
        m_RestVertices = BuildOutline(m_Seed, m_OutlineVertexCount);
        m_TriIndices = Triangulate(m_RestVertices);
        m_LiveVertices = (Vector2[])m_RestVertices.Clone();

        // Cache the reference extent so deformers scale sensibly regardless of seed/size.
        m_RefExtent = 1f;
        foreach (var v in m_RestVertices)
            m_RefExtent = math.max(m_RefExtent, v.magnitude);
    }

    private void Update()
    {
        if (m_TriIndices == null)
            return;

        // Testbed: produce the deformed mesh vertices.
        DeformVertices();

        // Core: map each deformed triangle to a Polygon geometry and draw it.
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
    //     triangle); those are drawn red and not set,
    //   - the three outward edge normals are computed directly.
    // ==========================================================================================

    private void DrawPolygons()
    {
        var world = World;
        var triangleCount = m_TriIndices.Length / 3;
        var invalidCount = 0;

        for (var t = 0; t < triangleCount; t++)
        {
            var a = m_LiveVertices[m_TriIndices[t * 3]];
            var b = m_LiveVertices[m_TriIndices[t * 3 + 1]];
            var c = m_LiveVertices[m_TriIndices[t * 3 + 2]];

            if (TryBuildTriangle(a, b, c, out var polygon))
            {
                world.DrawGeometry(polygon, PhysicsTransform.identity, m_PolygonColor);
            }
            else
            {
                // Degenerate: draw the triangle outline in red and leave the shape unset.
                world.DrawLine(a, b, m_InvalidColor);
                world.DrawLine(b, c, m_InvalidColor);
                world.DrawLine(c, a, m_InvalidColor);
                invalidCount++;
            }
        }

        if (m_CountLabel != null)
            m_CountLabel.text = $"Triangles: {triangleCount}   Invalid: {invalidCount}";
    }

    // Builds a counter-clockwise 3-vertex PolygonGeometry from the three corners, computing winding,
    // degeneracy, and normals ourselves so we never touch PolygonGeometry.Create / isValid.
    // Returns false when the triangle is degenerate (area below Min Area), which cannot be fixed up.
    private bool TryBuildTriangle(Vector2 a, Vector2 b, Vector2 c, out PolygonGeometry polygon)
    {
        polygon = default;

        // Twice the signed area: sign is the winding, magnitude is the degeneracy test.
        var doubleArea = Cross(b - a, c - a);

        // Collinear / near-zero area: a winding fix-up cannot rescue this.
        if (math.abs(doubleArea) < m_MinArea * 2f)
            return false;

        // Ensure counter-clockwise winding so the outward normals are correct.
        if (doubleArea < 0f)
            (b, c) = (c, b);

        polygon = PolygonGeometry.defaultGeometry;
        polygon.count = 3;
        polygon.radius = 0f;

        polygon.vertices[0] = a;
        polygon.vertices[1] = b;
        polygon.vertices[2] = c;

        // Outward normal of a CCW edge p->q is perpendicular to the right: (d.y, -d.x).
        polygon.normals[0] = EdgeNormal(a, b);
        polygon.normals[1] = EdgeNormal(b, c);
        polygon.normals[2] = EdgeNormal(c, a);

        polygon.centroid = (a + b + c) / 3f;

        return true;
    }

    private static Vector2 EdgeNormal(Vector2 p, Vector2 q)
    {
        var d = q - p;
        return new Vector2(d.y, -d.x).normalized;
    }

    // Imitates the exterior guard: a closed strip through the deformed outline vertices. This stands
    // in for a PhysicsChain on the boundary (as in ChainShapeDeform) without creating shapes yet.
    private void DrawExteriorGuard()
    {
        World.DrawLineStrip(PhysicsTransform.identity, m_LiveVertices, true, m_GuardColor, 0f);
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
    private Vector2 Deform(Vector2 p, float amount)
    {
        switch (m_DeformMode)
        {
            case DeformMode.Shear:
            {
                // Slide x in proportion to height.
                return new Vector2(p.x + amount * 1.5f * p.y, p.y);
            }

            case DeformMode.Taper:
            {
                // Widen one end and pinch the other, producing slivers at the narrow end.
                var scaleX = 1f + amount * (p.y / m_RefExtent);
                return new Vector2(p.x * scaleX, p.y);
            }

            case DeformMode.Twist:
            {
                // Rotate each point by an angle that grows with distance from the origin.
                var angle = amount * math.PI * (p.magnitude / m_RefExtent);
                var s = math.sin(angle);
                var co = math.cos(angle);
                return new Vector2(p.x * co - p.y * s, p.x * s + p.y * co);
            }

            case DeformMode.None:
            default:
                return p;
        }
    }

    // Draws the triangle edges of the live mesh as a wireframe.
    private void DrawMesh()
    {
        var world = World;

        for (var t = 0; t < m_TriIndices.Length; t += 3)
        {
            var a = m_LiveVertices[m_TriIndices[t]];
            var b = m_LiveVertices[m_TriIndices[t + 1]];
            var c = m_LiveVertices[m_TriIndices[t + 2]];

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
}
