using System;

using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Draws thousands of random primitives with the world's debug drawing calls, each one staying on screen for its own lifetime, to stress-test the drawing.
/// Nothing in this example is a physics body: every shape is drawn straight into the world, so it needs no Physics Pose or Physics Area.
/// </summary>
/// <remarks>
/// Every change to a setting clears the drawing and draws it all again, from the same starting seed, so the same layout appears each time.
/// Each shape gets a random position, rotation and color, and a lifetime that is either shared or spread at random between one second and the chosen lifetime.
/// The outline and interior settings apply to every type that draws a filled shape, which is all of them except the segment, point, line and line strip types.
/// </remarks>
public sealed class DrawingContents : MonoBehaviour
{
    /// <summary>
    /// The drawing call used for every shape.
    /// </summary>
    public enum DrawingType
    {
        CircleGeometry,
        CapsuleGeometry,
        PolygonGeometry,
        SegmentGeometry,
        Box,
        Circle,
        Capsule,
        Point,
        Line,
        LineStrip
    }

    /// <summary>
    /// Clears the current drawing and draws a new one with the current settings.
    /// </summary>
    public void Rebuild()
    {
        var world = PhysicsWorld.defaultWorld;
        world.ClearDraw();

        m_Random = new Random(RandomSeed);

        var drawOptions = default(PhysicsWorld.DrawFillOptions);
        if (m_DrawOutline)
            drawOptions |= PhysicsWorld.DrawFillOptions.Outline;
        if (m_DrawInterior)
            drawOptions |= PhysicsWorld.DrawFillOptions.Interior;

        if (m_DrawingType == DrawingType.LineStrip)
        {
            DrawLineStrip(world);
            return;
        }

        for (var n = 0; n < m_DrawingCount; ++n)
            DrawShape(world, drawOptions);
    }

    /// <summary>
    /// The drawing call used for every shape.
    /// Changing this does not redraw until <see cref="Rebuild"/> is called.
    /// </summary>
    public DrawingType drawingType
    {
        get => m_DrawingType;
        set => m_DrawingType = value;
    }

    /// <summary>
    /// How many shapes are drawn, or for the line strip type roughly how many vertices are drawn in total, as strips of ten vertices.
    /// Changing this does not redraw until <see cref="Rebuild"/> is called.
    /// </summary>
    public int drawingCount
    {
        get => m_DrawingCount;
        set => m_DrawingCount = value;
    }

    /// <summary>
    /// How long each shape stays on screen, in seconds, or the longest it stays when the lifetime is spread.
    /// Changing this does not redraw until <see cref="Rebuild"/> is called.
    /// </summary>
    public float drawingLifetime
    {
        get => m_DrawingLifetime;
        set => m_DrawingLifetime = value;
    }

    /// <summary>
    /// Whether each shape gets a random lifetime between one second and the chosen lifetime, instead of all sharing the chosen lifetime.
    /// Changing this does not redraw until <see cref="Rebuild"/> is called.
    /// </summary>
    public bool spreadLifetime
    {
        get => m_SpreadLifetime;
        set => m_SpreadLifetime = value;
    }

    /// <summary>
    /// Whether the types that draw a filled shape draw its outline.
    /// Changing this does not redraw until <see cref="Rebuild"/> is called.
    /// </summary>
    public bool drawOutline
    {
        get => m_DrawOutline;
        set => m_DrawOutline = value;
    }

    /// <summary>
    /// Whether the types that draw a filled shape draw its filled interior.
    /// Changing this does not redraw until <see cref="Rebuild"/> is called.
    /// </summary>
    public bool drawInterior
    {
        get => m_DrawInterior;
        set => m_DrawInterior = value;
    }

    private void Start() => Rebuild();

    // Draws many short closed strips of lines through random vertices, so that each one gets its own color and lifetime.
    // The count is the approximate number of vertices in total, so the number of strips is the count divided by the vertices in each strip.
    private void DrawLineStrip(PhysicsWorld world)
    {
        var stripCount = Mathf.Max(1, m_DrawingCount / LineStripVertexCount);
        var vertices = new NativeArray<Vector2>(LineStripVertexCount, Allocator.Temp);

        for (var n = 0; n < stripCount; ++n)
        {
            for (var i = 0; i < LineStripVertexCount; ++i)
                vertices[i] = RandomPoint();

            var lifetime = m_SpreadLifetime ? m_Random.NextFloat(1f, m_DrawingLifetime) : m_DrawingLifetime;
            world.DrawLineStrip(PhysicsTransform.identity, vertices, true, RandomColor(), lifetime);
        }

        vertices.Dispose();
    }

    // Draws one random shape of the chosen type, at a random position and rotation with a random color and lifetime.
    private void DrawShape(PhysicsWorld world, PhysicsWorld.DrawFillOptions drawOptions)
    {
        var physicsTransform = new PhysicsTransform
        {
            position = RandomPoint(),
            rotation = PhysicsRotate.FromRadians(m_Random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI))
        };

        var color = RandomColor();
        var lifetime = m_SpreadLifetime ? m_Random.NextFloat(1f, m_DrawingLifetime) : m_DrawingLifetime;

        switch (m_DrawingType)
        {
            case DrawingType.CircleGeometry:
            {
                var geometry = new CircleGeometry { radius = m_Random.NextFloat(0.05f, 0.5f) };
                if (geometry.isValid)
                    world.DrawGeometry(geometry, physicsTransform, color, lifetime, drawOptions);

                return;
            }

            case DrawingType.CapsuleGeometry:
            {
                var geometry = new CapsuleGeometry
                {
                    center1 = RandomOffset(),
                    center2 = RandomOffset(),
                    radius = m_Random.NextFloat(0.05f, 0.5f)
                };

                if (geometry.isValid)
                    world.DrawGeometry(geometry, physicsTransform, color, lifetime, drawOptions);

                return;
            }

            case DrawingType.PolygonGeometry:
            {
                var geometry = WorkshopUtility.CreateRandomPolygon(extent: 0.5f, radius: m_Random.NextFloat(0f, 0.25f), ref m_Random);
                if (geometry.isValid)
                    world.DrawGeometry(geometry, physicsTransform, color, lifetime, drawOptions);

                return;
            }

            case DrawingType.SegmentGeometry:
            {
                var geometry = new SegmentGeometry { point1 = RandomPoint(), point2 = RandomPoint() };
                if (geometry.isValid)
                    world.DrawGeometry(geometry, PhysicsTransform.identity, color, lifetime);

                return;
            }

            case DrawingType.Box:
            {
                var size = new Vector2(m_Random.NextFloat(0.1f, 1f), m_Random.NextFloat(0.1f, 1f));
                var radius = m_Random.NextFloat(0.1f, 0.5f);
                world.DrawBox(physicsTransform, size, radius, color, lifetime, drawOptions);

                return;
            }

            case DrawingType.Circle:
            {
                var radius = m_Random.NextFloat(0.1f, 0.5f);
                world.DrawCircle(physicsTransform.position, radius, color, lifetime, drawOptions);

                return;
            }

            case DrawingType.Capsule:
            {
                var center1 = RandomOffset();
                var center2 = RandomOffset();
                var radius = m_Random.NextFloat(0.05f, 0.5f);
                world.DrawCapsule(physicsTransform, center1, center2, radius, color, lifetime, drawOptions);

                return;
            }

            case DrawingType.Point:
            {
                var size = m_Random.NextFloat(1f, 10f);
                world.DrawPoint(physicsTransform.position, size, color, lifetime);

                return;
            }

            case DrawingType.Line:
            {
                var point1 = RandomPoint();
                var point2 = RandomPoint();
                if (point1 != point2)
                    world.DrawLine(point1, point2, color, lifetime);

                return;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Returns a random point within the area the drawing covers.
    private Vector2 RandomPoint() => new(m_Random.NextFloat(-Extents.x, Extents.x), m_Random.NextFloat(-Extents.y, Extents.y));

    // Returns a random point within half a meter of the origin, for the ends of a capsule.
    private Vector2 RandomOffset() => new(m_Random.NextFloat(-0.5f, 0.5f), m_Random.NextFloat(-0.5f, 0.5f));

    // Returns a new random bright color.
    private Color RandomColor() => Color.HSVToRGB(m_Random.NextFloat(0f, 1f), m_Random.NextFloat(0.7f, 1f) * SaturationScale, m_Random.NextFloat(0.5f, 1f));

    #region Internal

    // How many vertices each line strip has.
    const int LineStripVertexCount = 10;

    // Half the width and height of the area shapes are scattered over, how washed out the random colors are, and the seed every redraw starts from.
    static readonly Vector2 Extents = new(9f, 7f);
    const float SaturationScale = 0.65f;
    const uint RandomSeed = 0x9E3779B9;

    [SerializeField] DrawingType m_DrawingType = DrawingType.CircleGeometry;
    [SerializeField, Range(10, 10000)] int m_DrawingCount = 1000;
    [SerializeField, Range(1f, 60f)] float m_DrawingLifetime = 10f;
    [SerializeField] bool m_SpreadLifetime = true;
    [SerializeField] bool m_DrawOutline = true;
    [SerializeField] bool m_DrawInterior = true;

    Random m_Random;

    #endregion
}
