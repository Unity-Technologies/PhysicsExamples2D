using System;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Runtime geometry mutation. A single PhysicsShape's geometry is swapped between Circle/Capsule/Segment/Polygon
/// at runtime — same shape handle, different geometry data. Press 1/2/3/4 to switch geometry; +/- to scale.
/// Uses the type-specific setter properties (`shape.circleGeometry = ...`, `shape.polygonGeometry = ...`) which
/// only succeed when the shape's current `shapeType` matches.
/// </summary>
public class ModifyGeometry : MonoBehaviour
{
    public enum GeometryType { Circle, Capsule, Segment, Polygon }

    public PhysicsBody.BodyType BodyType = PhysicsBody.BodyType.Kinematic;
    public GeometryType InitialGeometry = GeometryType.Circle;
    public float GeometryScale = 1f;

    private PhysicsWorld m_PhysicsWorld;
    private PhysicsBody m_ChangerBody;
    private PhysicsShape m_ChangerShape;
    private GeometryType m_CurrentType;

    private void OnEnable()
    {
        m_PhysicsWorld = PhysicsWorld.defaultWorld;
        SetupScene();
    }

    private void OnDisable()
    {
        if (m_ChangerBody.isValid)
            m_ChangerBody.Destroy();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchGeometry(GeometryType.Circle);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchGeometry(GeometryType.Capsule);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchGeometry(GeometryType.Segment);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchGeometry(GeometryType.Polygon);
        if (Input.GetKey(KeyCode.Equals)) { GeometryScale = Mathf.Min(GeometryScale + Time.deltaTime, 4f); UpdateShape(); }
        if (Input.GetKey(KeyCode.Minus))  { GeometryScale = Mathf.Max(GeometryScale - Time.deltaTime, 0.1f); UpdateShape(); }
    }

    private void SetupScene()
    {
        // Ground plate.
        var groundBody = m_PhysicsWorld.CreateBody();
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 50f), 0f, Vector2.down * 25f));

        // Falling interactor body so collisions are visible.
        var interact = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = Vector2.up * 4f });
        interact.CreateShape(PolygonGeometry.CreateBox(Vector2.one * 2f));

        // The changer body holds the mutable shape.
        m_ChangerBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = BodyType, position = Vector2.up });
        m_ChangerShape = m_ChangerBody.CreateShape(BuildCircleGeometry());
        m_CurrentType = GeometryType.Circle;

        if (InitialGeometry != GeometryType.Circle)
            SwitchGeometry(InitialGeometry);
    }

    private void SwitchGeometry(GeometryType newType)
    {
        if (!m_ChangerShape.isValid)
            return;

        // To switch the underlying shape *type*, we destroy and recreate the shape on the same body.
        // Same-type setters (e.g. shape.circleGeometry = ...) are used to mutate within a type.
        if (newType != m_CurrentType)
        {
            m_ChangerShape.Destroy();
            m_ChangerShape = newType switch
            {
                GeometryType.Circle => m_ChangerBody.CreateShape(BuildCircleGeometry()),
                GeometryType.Capsule => m_ChangerBody.CreateShape(BuildCapsuleGeometry()),
                GeometryType.Segment => m_ChangerBody.CreateShape(BuildSegmentGeometry()),
                GeometryType.Polygon => m_ChangerBody.CreateShape(BuildPolygonGeometry()),
                _ => throw new ArgumentOutOfRangeException()
            };
            m_CurrentType = newType;
        }
        else
        {
            UpdateShape();
        }
    }

    private void UpdateShape()
    {
        if (!m_ChangerShape.isValid)
            return;

        switch (m_CurrentType)
        {
            case GeometryType.Circle:  m_ChangerShape.circleGeometry  = BuildCircleGeometry();  return;
            case GeometryType.Capsule: m_ChangerShape.capsuleGeometry = BuildCapsuleGeometry(); return;
            case GeometryType.Segment: m_ChangerShape.segmentGeometry = BuildSegmentGeometry(); return;
            case GeometryType.Polygon: m_ChangerShape.polygonGeometry = BuildPolygonGeometry(); return;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private CircleGeometry  BuildCircleGeometry()  => new CircleGeometry  { radius = 0.5f * GeometryScale };
    private CapsuleGeometry BuildCapsuleGeometry() => new CapsuleGeometry { center1 = Vector2.left * 0.5f * GeometryScale, center2 = Vector2.right * 0.5f * GeometryScale, radius = 0.5f * GeometryScale };
    private SegmentGeometry BuildSegmentGeometry() => new SegmentGeometry { point1 = Vector2.left * 0.5f * GeometryScale, point2 = Vector2.right * 0.5f * GeometryScale };
    private PolygonGeometry BuildPolygonGeometry() => PolygonGeometry.CreateBox(new Vector2(GeometryScale, 1.5f * GeometryScale));
}
