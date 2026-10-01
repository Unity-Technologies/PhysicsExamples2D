using System;
using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Shows the distance and closest points between shapes as they orbit the origin and spin, one inner shape measured against four outer shapes, one of each type.
/// Every shape is a kinematic Physics Pose with a Physics Area, so the physics renderer draws them, and this script only moves them and measures the distance between them.
/// </summary>
/// <remarks>
/// The inner shape is a Physics Area Primitive, so its type can be changed while the example runs.
/// The four outer shapes are a Physics Area Circle, Capsule, Polygon and Segment, a quarter turn apart at a larger distance from the origin.
/// Every frame the distance between the inner shape and each outer shape is found, and the line between the closest points, the points themselves and the direction from the inner shape to the outer shape are drawn.
/// A shape that overlaps another has a distance of zero, which has no direction, so only a red point is drawn for it.
/// The shapes only move while the world is simulating, so they stop when it is paused.
/// </remarks>
public sealed class ShapeDistanceContents : MonoBehaviour
{
    /// <summary>
    /// The kinds of shape the inner shape can be.
    /// </summary>
    public enum ShapeType
    {
        Circle,
        Capsule,
        Polygon,
        Segment
    }

    /// <summary>
    /// The type of the inner shape.
    /// </summary>
    public ShapeType innerShapeType
    {
        get => m_InnerShapeType;
        set
        {
            m_InnerShapeType = value;
            ApplyInnerShapeType();
        }
    }

    /// <summary>
    /// How fast the shapes orbit the origin, in degrees per second.
    /// </summary>
    public float orbitSpeed
    {
        get => m_OrbitSpeed;
        set => m_OrbitSpeed = value;
    }

    /// <summary>
    /// How fast each outer shape spins about its own center, in degrees per second.
    /// </summary>
    public float spinSpeed
    {
        get => m_SpinSpeed;
        set => m_SpinSpeed = value;
    }

    /// <summary>
    /// How much every shape is scaled by.
    /// </summary>
    public float shapeScale
    {
        get => m_ShapeScale;
        set
        {
            m_ShapeScale = value;
            ApplyShapes();
        }
    }

    // Shapes are applied here because Unity calls this again after a script reload while playing, and Awake it does not.
    private void OnEnable()
    {
        ApplyShapes();

        PhysicsEvents.PreSimulate += OnPreSimulate;
    }

    private void OnDisable()
    {
        PhysicsEvents.PreSimulate -= OnPreSimulate;
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        if (!m_InnerPose.body.isValid)
            return;

        var innerTransform = m_InnerPose.body.transform;
        var innerProxy = m_InnerShapes.CreateProxy(m_InnerShapeType);

        // Each outer shape is measured against the inner shape.
        for (var i = 0; i < OuterShapeCount; ++i)
        {
            var outerBody = m_OuterPoses[i].body;
            if (!outerBody.isValid)
                continue;

            DrawDistance(world, innerProxy, innerTransform, m_OuterShapes.CreateProxy((ShapeType)i), outerBody.transform);
        }
    }

    // Moves the shapes before each step: the inner shape orbits close to the origin and spins at a fixed rate, and the outer shapes sit a quarter turn apart at the same distance from the origin and spin at the chosen rate.
    private void OnPreSimulate(PhysicsWorld world, float timeStep)
    {
        if (!world.isDefaultWorld)
            return;

        var fullTurn = 2f * PhysicsMath.PI;
        m_OrbitAngle = Mathf.Repeat(m_OrbitAngle + m_OrbitSpeed * Mathf.Deg2Rad * timeStep, fullTurn);
        m_SpinAngle = Mathf.Repeat(m_SpinAngle + m_SpinSpeed * Mathf.Deg2Rad * timeStep, fullTurn);
        m_InnerSpinAngle = Mathf.Repeat(m_InnerSpinAngle + InnerSpinSpeed * Mathf.Deg2Rad * timeStep, fullTurn);

        var innerDirection = PhysicsRotate.FromRadians(m_OrbitAngle).direction;
        m_InnerPose.body.SetTransformTarget(new PhysicsTransform(innerDirection * InnerOrbitRadius, PhysicsRotate.FromRadians(m_InnerSpinAngle)), timeStep);

        for (var i = 0; i < OuterShapeCount; ++i)
        {
            var outerDirection = PhysicsRotate.FromRadians(m_OrbitAngle + i * 0.5f * PhysicsMath.PI).direction;
            m_OuterPoses[i].body.SetTransformTarget(new PhysicsTransform(outerDirection * OuterOrbitRadius, PhysicsRotate.FromRadians(-m_SpinAngle)), timeStep);
        }
    }

    // Creates every shape's geometry at the current scale and gives it to the Physics Areas, which update their live shapes when they have them.
    private void ApplyShapes()
    {
        var innerScale = m_ShapeScale * InnerShapeScale;
        m_InnerShapes = ShapeSet.Create(innerScale, RoundedPolygonRadius * innerScale);
        m_OuterShapes = ShapeSet.Create(m_ShapeScale, RoundedPolygonRadius * m_ShapeScale);

        m_InnerArea.circleGeometry = m_InnerShapes.circle;
        m_InnerArea.capsuleGeometry = m_InnerShapes.capsule;
        m_InnerArea.polygonGeometry = m_InnerShapes.polygon;
        m_InnerArea.segmentGeometry = m_InnerShapes.segment;
        m_InnerArea.shapeType = ToPhysicsShapeType(m_InnerShapeType);

        m_OuterCircleArea.geometry = m_OuterShapes.circle;
        m_OuterCapsuleArea.geometry = m_OuterShapes.capsule;
        m_OuterPolygonArea.geometry = m_OuterShapes.polygon;
        m_OuterSegmentArea.geometry = m_OuterShapes.segment;

        // The shapes are only there to update once the poses have created their bodies, which is not yet the case when this runs as the example loads.
        if (!m_InnerPose.body.isValid)
            return;

        m_InnerArea.ApplyGeometry();
        m_OuterCircleArea.ApplyGeometry();
        m_OuterCapsuleArea.ApplyGeometry();
        m_OuterPolygonArea.ApplyGeometry();
        m_OuterSegmentArea.ApplyGeometry();
    }

    // Changes the type of the inner shape's Physics Area Primitive.
    private void ApplyInnerShapeType()
    {
        m_InnerArea.shapeType = ToPhysicsShapeType(m_InnerShapeType);

        if (m_InnerPose.body.isValid)
            m_InnerArea.ApplyGeometry();
    }

    private static PhysicsShape.ShapeType ToPhysicsShapeType(ShapeType shapeType)
    {
        switch (shapeType)
        {
            case ShapeType.Circle:
                return PhysicsShape.ShapeType.Circle;

            case ShapeType.Capsule:
                return PhysicsShape.ShapeType.Capsule;

            case ShapeType.Polygon:
                return PhysicsShape.ShapeType.Polygon;

            default:
                return PhysicsShape.ShapeType.Segment;
        }
    }

    // Finds the closest points between the two shapes and draws them, the line between them and the direction from the first shape to the second.
    // The closest points and the normal are in world space, and a distance of zero means the shapes overlap, which has no normal.
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

    // One geometry of each shape type at a given scale, used both to set the Physics Areas and to measure the distance between shapes.
    private struct ShapeSet
    {
        public CircleGeometry circle;
        public CapsuleGeometry capsule;
        public PolygonGeometry polygon;
        public SegmentGeometry segment;

        // Creates the geometries at the given scale, with the polygon rounded by the given radius so the proxy radius is also used.
        public static ShapeSet Create(float scale, float polygonRadius)
        {
            // A regular polygon with a corner on each step around the circle.
            Span<Vector2> vertices = stackalloc Vector2[PolygonVertexCount];
            for (var i = 0; i < PolygonVertexCount; ++i)
            {
                var angle = 2f * PhysicsMath.PI * i / PolygonVertexCount;
                vertices[i] = PhysicsRotate.FromRadians(angle).direction * ShapeRadius * scale;
            }

            return new ShapeSet
            {
                circle = new CircleGeometry { radius = ShapeRadius * scale },
                capsule = new CapsuleGeometry { center1 = new Vector2(-ShapeRadius, 0f) * scale, center2 = new Vector2(ShapeRadius, 0f) * scale, radius = ShapeRadius * 0.5f * scale },
                polygon = PolygonGeometry.Create(vertices, polygonRadius),
                segment = new SegmentGeometry { point1 = new Vector2(-ShapeRadius * 1.5f, 0f) * scale, point2 = new Vector2(ShapeRadius * 1.5f, 0f) * scale }
            };
        }

        // Returns the proxy of the geometry of the given type, which is the form of the shape the distance query takes.
        public readonly PhysicsShape.ShapeProxy CreateProxy(ShapeType shapeType)
        {
            switch (shapeType)
            {
                case ShapeType.Circle:
                    return circle.CreateShapeProxy();

                case ShapeType.Capsule:
                    return capsule.CreateShapeProxy();

                case ShapeType.Polygon:
                    return polygon.CreateShapeProxy();

                default:
                    return segment.CreateShapeProxy();
            }
        }
    }

    #region Internal

    // How far from the origin the inner shape and the outer shapes orbit, and the size of a shape before it is scaled.
    const float InnerOrbitRadius = 1.25f;
    const float OuterOrbitRadius = 10f;
    const float ShapeRadius = 1.5f;

    // How much bigger the inner shape is than the outer shapes, and how much the polygons are rounded by before they are scaled.
    const float InnerShapeScale = 1.5f;
    const float InnerSpinSpeed = -20f;
    const float RoundedPolygonRadius = 0.3f;

    // The size in pixels of a closest point, and how long the line showing the direction between two shapes is.
    const float PointSize = 10f;
    const float NormalLength = 0.5f;

    // How many vertices the polygons have, and how many outer shapes there are, one of each type.
    const int PolygonVertexCount = 8;
    const int OuterShapeCount = 4;

    static readonly Color ColorNormal = Color.cornflowerBlue;

    // The inner shape, and the outer shapes in the order of the shape types: circle, capsule, polygon, segment.
    [SerializeField] PhysicsPose m_InnerPose;
    [SerializeField] PhysicsAreaPrimitive m_InnerArea;
    [SerializeField] PhysicsPose[] m_OuterPoses;
    [SerializeField] PhysicsAreaCircle m_OuterCircleArea;
    [SerializeField] PhysicsAreaCapsule m_OuterCapsuleArea;
    [SerializeField] PhysicsAreaPolygon m_OuterPolygonArea;
    [SerializeField] PhysicsAreaSegment m_OuterSegmentArea;

    [SerializeField] ShapeType m_InnerShapeType = ShapeType.Polygon;
    [SerializeField, Range(1f, 180f)] float m_OrbitSpeed = 30f;
    [SerializeField, Range(1f, 180f)] float m_SpinSpeed = 20f;
    [SerializeField, Range(0.5f, 2f)] float m_ShapeScale = 1f;

    ShapeSet m_InnerShapes;
    ShapeSet m_OuterShapes;
    float m_OrbitAngle;
    float m_SpinAngle;
    float m_InnerSpinAngle;

    #endregion
}
