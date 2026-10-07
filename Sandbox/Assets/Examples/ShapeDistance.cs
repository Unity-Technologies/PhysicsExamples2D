using System;
using UnityEngine;
using Unity.U2D.Physics;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrates the distance and closest points between two shapes as they orbit the origin and spin.",
    Purpose = "Demonstrates the shape distance query, which finds the distance between two shapes and the closest point on each, for any combination of shape types.\nOne inner shape is measured against four outer shapes, one of each type, as they orbit the origin and spin.",
    Controls = "Inner Shape: the type of the inner shape.\nOrbit Speed and Spin Speed: how fast the shapes orbit and spin.\nShape Scale: how big the shapes are.")]
public sealed class ShapeDistance : SandboxExampleBehaviour
{
    private enum ShapeType
    {
        Circle,
        Capsule,
        Polygon,
        Segment
    }

    private const float InnerOrbitRadius = 1.25f;
    private const float OuterOrbitRadius = 10f;
    private const float ShapeRadius = 1.5f;
    private const float InnerShapeScale = 1.5f;
    private const float InnerSpinSpeed = -20f;
    private const float RoundedPolygonRadius = 0.3f;
    private const float PointSize = 10f;
    private const float NormalLength = 0.5f;
    private const int PolygonVertexCount = 8;
    private const int OuterShapeCount = 4;

    private static readonly Color ColorInner = Color.limeGreen;
    private static readonly Color ColorOuter = Color.gold;
    private static readonly Color ColorNormal = Color.cornflowerBlue;

    private ShapeType m_InnerShapeType;
    private float m_OrbitSpeed;
    private float m_SpinSpeed;
    private float m_ShapeScale;
    private float m_OrbitAngle;
    private float m_SpinAngle;
    private float m_InnerSpinAngle;
    private PolygonGeometry m_InnerPolygon;
    private PolygonGeometry m_OuterPolygon;

    protected override float CameraSize => 16f;

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        m_InnerShapeType = ShapeType.Polygon;
        m_OrbitSpeed = 30f;
        m_SpinSpeed = 20f;
        m_ShapeScale = 1f;

        CreatePolygons();
    }

    // Creates the two polygons at the current scale, both rounded so the proxy radius is also used.
    private void CreatePolygons()
    {
        m_InnerPolygon = CreatePolygon(m_ShapeScale * InnerShapeScale, RoundedPolygonRadius * m_ShapeScale * InnerShapeScale);
        m_OuterPolygon = CreatePolygon(m_ShapeScale, RoundedPolygonRadius * m_ShapeScale);
    }

    // Creates a regular polygon with a corner on each step around the circle.
    private static PolygonGeometry CreatePolygon(float scale, float radius)
    {
        Span<Vector2> vertices = stackalloc Vector2[PolygonVertexCount];
        for (var i = 0; i < PolygonVertexCount; ++i)
        {
            var angle = 2f * PhysicsMath.PI * i / PolygonVertexCount;
            vertices[i] = PhysicsRotate.FromRadians(angle).direction * ShapeRadius * scale;
        }

        return PolygonGeometry.Create(vertices, radius);
    }

    protected override void SetupOptions()
    {
        // Inner Shape Type.
        AddEnum("Inner Shape", m_InnerShapeType, v => m_InnerShapeType = v);

        // Orbit Speed.
        AddSlider("Orbit Speed", m_OrbitSpeed, 1f, 180f, v => m_OrbitSpeed = v);

        // Spin Speed.
        AddSlider("Spin Speed", m_SpinSpeed, 1f, 180f, v => m_SpinSpeed = v);

        // Shape Scale.
        AddSlider("Shape Scale", m_ShapeScale, 0.5f, 2f, v =>
        {
            m_ShapeScale = v;
            CreatePolygons();
        });
    }

    protected override void SetupScene()
    {
        m_OrbitAngle = 0f;
        m_SpinAngle = 0f;
        m_InnerSpinAngle = 0f;
    }

    private void Update()
    {
        // Only move the shapes if the world isn't paused.
        if (!SandboxManager.WorldPaused)
        {
            var fullTurn = 2f * PhysicsMath.PI;
            m_OrbitAngle = Mathf.Repeat(m_OrbitAngle + PhysicsMath.ToRadians(m_OrbitSpeed) * Time.deltaTime, fullTurn);
            m_SpinAngle = Mathf.Repeat(m_SpinAngle + PhysicsMath.ToRadians(m_SpinSpeed) * Time.deltaTime, fullTurn);
            m_InnerSpinAngle = Mathf.Repeat(m_InnerSpinAngle + PhysicsMath.ToRadians(InnerSpinSpeed) * Time.deltaTime, fullTurn);
        }

        // Get the default world.
        var world = World;

        // The inner shape orbits close to the origin and spins at a fixed rate, while the outer shapes spin at the chosen rate.
        var innerDirection = PhysicsRotate.FromRadians(m_OrbitAngle).direction;
        var innerTransform = new PhysicsTransform(innerDirection * InnerOrbitRadius, PhysicsRotate.FromRadians(m_InnerSpinAngle));
        var innerProxy = DrawShape(world, m_InnerShapeType, m_InnerPolygon, innerTransform, m_ShapeScale * InnerShapeScale, ColorInner);

        // The outer shapes are one of each type, a quarter turn apart at the same distance from the origin, and each is measured against the inner shape.
        for (var i = 0; i < OuterShapeCount; ++i)
        {
            var outerDirection = PhysicsRotate.FromRadians(m_OrbitAngle + i * 0.5f * PhysicsMath.PI).direction;
            var outerTransform = new PhysicsTransform(outerDirection * OuterOrbitRadius, PhysicsRotate.FromRadians(-m_SpinAngle));
            var outerProxy = DrawShape(world, (ShapeType)i, m_OuterPolygon, outerTransform, m_ShapeScale, ColorOuter);

            DrawDistance(world, innerProxy, innerTransform, outerProxy, outerTransform);
        }
    }

    // Draws a shape of the given type and returns its proxy, which is the form of the shape the distance query takes.
    private static PhysicsShape.ShapeProxy DrawShape(PhysicsWorld world, ShapeType shapeType, PolygonGeometry polygon, PhysicsTransform transform, float scale, Color color)
    {
        switch (shapeType)
        {
            case ShapeType.Circle:
            {
                var geometry = new CircleGeometry { radius = ShapeRadius * scale };
                world.DrawGeometry(geometry, transform, color);

                return geometry.CreateShapeProxy();
            }

            case ShapeType.Capsule:
            {
                var geometry = new CapsuleGeometry { center1 = new Vector2(-ShapeRadius, 0f) * scale, center2 = new Vector2(ShapeRadius, 0f) * scale, radius = ShapeRadius * 0.5f * scale };
                world.DrawGeometry(geometry, transform, color);

                return geometry.CreateShapeProxy();
            }

            case ShapeType.Polygon:
            {
                world.DrawGeometry(polygon, transform, color);

                return polygon.CreateShapeProxy();
            }

            default:
            {
                var geometry = new SegmentGeometry { point1 = new Vector2(-ShapeRadius * 1.5f, 0f) * scale, point2 = new Vector2(ShapeRadius * 1.5f, 0f) * scale };
                world.DrawGeometry(geometry, transform, color);

                return geometry.CreateShapeProxy();
            }
        }
    }

    // Finds the closest points between the two shapes and draws them, the line between them and the direction from the first shape to the second.
    // The closest points and the normal come back in world space, and a distance of zero means the shapes overlap, which has no normal.
    private static void DrawDistance(PhysicsWorld world, PhysicsShape.ShapeProxy proxyA, PhysicsTransform transformA, PhysicsShape.ShapeProxy proxyB, PhysicsTransform transformB)
    {
        var result = PhysicsQuery.ShapeDistance(new PhysicsQuery.DistanceInput
        {
            shapeProxyA = proxyA,
            shapeProxyB = proxyB,
            transformA = transformA,
            transformB = transformB,
            useRadii = true
        });

        if (result.distance <= 0f)
        {
            world.DrawPoint(result.pointA, PointSize, Color.red);
            return;
        }

        world.DrawLine(result.pointA, result.pointB, Color.gray);
        world.DrawLine(result.pointA, result.pointA + result.normal * NormalLength, ColorNormal);
        world.DrawPoint(result.pointA, PointSize, Color.white);
        world.DrawPoint(result.pointB, PointSize, Color.white);
    }
}
