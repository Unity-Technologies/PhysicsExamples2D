using System;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrates modifying the geometry of existing shapes.")]
public sealed class ModifyGeometry : SandboxExampleBehaviour
{
    private PhysicsBody m_ChangerBody;
    private PhysicsShape m_ChangerShape;

    private enum GeometryType
    {
        Circle,
        Capsule,
        Segment,
        Polygon
    }

    private PhysicsBody.BodyType m_BodyType;
    private GeometryType m_GeometryType;
    private float m_GeometryScale;

    protected override float CameraSize => 6f;
    protected override Vector2 CameraPosition => new(0f, 5f);

    protected override void OnExampleEnable()
    {
        SandboxManager.SetOverrideColorShapeState(false);

        m_BodyType = PhysicsBody.BodyType.Kinematic;
        m_GeometryType = GeometryType.Circle;
        m_GeometryScale = 1f;
    }

    protected override void SetupOptions()
    {
        // Body Type.
        AddEnum("Body Type", m_BodyType, v =>
        {
            m_BodyType = v;
            m_ChangerBody.type = m_BodyType;
        });

        // Geometry Type.
        AddEnum("Geometry Type", m_GeometryType, v =>
        {
            m_GeometryType = v;
            UpdateShape();
        });

        // Geometry Scale.
        AddSlider("Geometry Scale", m_GeometryScale, 0.5f, 10f, v =>
        {
            m_GeometryScale = v;
            UpdateShape();
        });
    }

    protected override void SetupScene()
    {
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 50f), 0f, Vector2.down * 25f));
        }

        // Interact Shape.
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = Vector2.up * 4f });
            body.CreateShape(PolygonGeometry.CreateBox(Vector2.one * 2f));
        }

        // Shape Changer.
        {
            m_ChangerBody = world.CreateBody(new PhysicsBodyDefinition { type = m_BodyType, position = Vector2.up });
            m_ChangerShape = m_ChangerBody.CreateShape(circleGeometry);

            // Update the geometry if this isn't the current default.
            if (m_GeometryType != GeometryType.Circle)
                UpdateShape();
        }
    }

    private void UpdateShape()
    {
        // Finish if the changer shape isn't valid.
        if (!m_ChangerShape.isValid)
            return;

        switch (m_GeometryType)
        {
            case GeometryType.Circle:
                m_ChangerShape.circleGeometry = circleGeometry;
                return;

            case GeometryType.Capsule:
                m_ChangerShape.capsuleGeometry = capsuleGeometry;
                return;

            case GeometryType.Segment:
                m_ChangerShape.segmentGeometry = segmentGeometry;
                return;

            case GeometryType.Polygon:
                m_ChangerShape.polygonGeometry = polygonGeometry;
                return;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private CircleGeometry circleGeometry => new CircleGeometry { radius = 0.5f * m_GeometryScale };
    private CapsuleGeometry capsuleGeometry => new CapsuleGeometry { center1 = Vector2.left * 0.5f * m_GeometryScale, center2 = Vector2.right * 0.5f * m_GeometryScale, radius = 0.5f * m_GeometryScale };
    private SegmentGeometry segmentGeometry => new SegmentGeometry { point1 = Vector2.left * 0.5f * m_GeometryScale, point2 = Vector2.right * 0.5f * m_GeometryScale };
    private PolygonGeometry polygonGeometry => PolygonGeometry.CreateBox(new Vector2(m_GeometryScale, 1.5f * m_GeometryScale));
}
