using System;
using Unity.Collections;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Benchmarks", "Stress-tests the debug drawing API with many primitives and lifetimes.")]
public sealed class Drawing : SandboxExampleBehaviour
{
    private enum DrawingType
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

    private DrawingType m_DrawingType;
    private int m_DrawingCount;
    private float m_DrawingLifetime;
    private bool m_SpreadLifetime;
    private bool m_DrawOutline;
    private bool m_DrawInterior;

    protected override float CameraSize => 10f;

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        m_DrawingType = DrawingType.CircleGeometry;
        m_DrawingCount = 1000;
        m_DrawingLifetime = 10f;
        m_SpreadLifetime = true;
        m_DrawOutline = true;
        m_DrawInterior = true;
    }

    protected override void SetupOptions()
    {
        AddEnum("Type", m_DrawingType, v => m_DrawingType = v, rebuild: true);

        // Drawing Count.
        AddSliderInt("Count", m_DrawingCount, 10, 10000, v => m_DrawingCount = v, rebuild: true);

        // Drawing Lifetime.
        AddSlider("Lifetime", m_DrawingLifetime, 1f, 60f, v => m_DrawingLifetime = v, rebuild: true);

        // Spread Lifetime.
        AddToggle("Spread lifetime", m_SpreadLifetime, v => m_SpreadLifetime = v, rebuild: true);

        // Draw Outline.
        AddToggle("Draw Outline", m_DrawOutline, v => m_DrawOutline = v, rebuild: true);

        // Draw Interior.
        AddToggle("Draw Interior", m_DrawInterior, v => m_DrawInterior = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        ref var random = ref Random;
        var extents = new Vector2(9f, 7f);

        // Draw options.
        PhysicsWorld.DrawFillOptions drawOptions = default;
        if (m_DrawOutline)
            drawOptions |= PhysicsWorld.DrawFillOptions.Outline;
        if (m_DrawInterior)
            drawOptions |= PhysicsWorld.DrawFillOptions.Interior;

        // Line Strip.
        if (m_DrawingType == DrawingType.LineStrip)
        {
            var color = ShapeColor;
            var vertices = new NativeArray<Vector2>(m_DrawingCount, Allocator.Temp);
            for (var i = 0; i < m_DrawingCount; ++i)
            {
                vertices[i] = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y));
            }

            world.DrawLineStrip(PhysicsTransform.identity, vertices, true, color, m_DrawingLifetime);
            vertices.Dispose();

            return;
        }

        // All others.
        for (var n = 0; n < m_DrawingCount; ++n)
        {
            var physicsTransform = new PhysicsTransform()
            {
                position = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y)),
                rotation = new PhysicsRotate(random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI))
            };

            var color = ShapeColor;
            var lifetime = m_SpreadLifetime ? random.NextFloat(1f, m_DrawingLifetime) : m_DrawingLifetime;

            switch (m_DrawingType)
            {
                case DrawingType.CircleGeometry:
                {
                    CircleGeometry geometry = new() { radius = random.NextFloat(0.05f, 0.5f) };
                    if (geometry.isValid)
                        world.DrawGeometry(geometry, physicsTransform, color, lifetime);

                    continue;
                }

                case DrawingType.CapsuleGeometry:
                {
                    CapsuleGeometry geometry = new()
                    {
                        center1 = new Vector2(random.NextFloat(-0.5f, 0.5f), random.NextFloat(-0.5f, 0.5f)),
                        center2 = new Vector2(random.NextFloat(-0.5f, 0.5f), random.NextFloat(-0.5f, 0.5f)),
                        radius = random.NextFloat(0.05f, 0.5f)
                    };
                    if (geometry.isValid)
                        world.DrawGeometry(geometry, physicsTransform, color, lifetime);

                    continue;
                }
                case DrawingType.PolygonGeometry:
                {
                    var geometry = SandboxUtility.CreateRandomPolygon(0.5f, random.NextFloat(0f, 0.25f), ref random);
                    if (geometry.isValid)
                        world.DrawGeometry(geometry, physicsTransform, color, lifetime);

                    continue;
                }

                case DrawingType.SegmentGeometry:
                {
                    SegmentGeometry geometry = new()
                    {
                        point1 = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y)),
                        point2 = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y)),
                    };
                    if (geometry.isValid)
                        world.DrawGeometry(geometry, PhysicsTransform.identity, color, lifetime);

                    continue;
                }

                case DrawingType.Box:
                {
                    var size = new Vector2(random.NextFloat(0.1f, 1f), random.NextFloat(0.1f, 1f));
                    var radius = random.NextFloat(0.1f, 0.5f);
                    world.DrawBox(physicsTransform, size, radius, color, lifetime, drawOptions);

                    continue;
                }

                case DrawingType.Circle:
                {
                    var radius = random.NextFloat(0.1f, 0.5f);
                    world.DrawCircle(physicsTransform.position, radius, color, lifetime, drawOptions);

                    continue;
                }

                case DrawingType.Capsule:
                {
                    var center1 = new Vector2(random.NextFloat(-0.5f, 0.5f), random.NextFloat(-0.5f, 0.5f));
                    var center2 = new Vector2(random.NextFloat(-0.5f, 0.5f), random.NextFloat(-0.5f, 0.5f));
                    var radius = random.NextFloat(0.05f, 0.5f);
                    world.DrawCapsule(physicsTransform, center1, center2, radius, color, lifetime, drawOptions);

                    continue;
                }

                case DrawingType.Point:
                {
                    var radius = random.NextFloat(1f, 10f);
                    world.DrawPoint(physicsTransform.position, radius, color, lifetime);

                    continue;
                }

                case DrawingType.Line:
                {
                    var point1 = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y));
                    var point2 = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y));
                    if (point1 != point2)
                        world.DrawLine(point1, point2, color, lifetime);

                    continue;
                }

                case DrawingType.LineStrip:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
