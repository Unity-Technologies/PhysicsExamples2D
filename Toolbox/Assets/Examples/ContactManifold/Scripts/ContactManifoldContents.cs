using Unity.Mathematics;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Draws the contact manifold between many pairs of shapes, every frame, with the second shape of each pair following a shared drag and rotate.
/// Nothing in this example is a physics body: each pair is tested with the shape queries and drawn straight away, so it needs no Physics Pose or Physics Area.
/// </summary>
/// <remarks>
/// Drag with the left mouse button to move the second shape of every pair, or hold the left Control key while dragging to rotate them.
/// A pair whose second shape turns green is touching, and each contact point is drawn as a blue dot with a white line along the contact normal.
/// The camera cannot be moved in this example, so dragging never pans the view.
/// </remarks>
public sealed class ContactManifoldContents : MonoBehaviour
{
    private void OnEnable()
    {
        m_ManipulatorState = ManipulatorState.None;
        m_Transform = new PhysicsTransform { position = new Vector2(0f, 0.5f), rotation = PhysicsRotate.identity };
        m_Angle = 0f;

        if (m_CameraManipulator != null)
            m_CameraManipulator.DisableManipulators = true;
    }

    // The camera belongs outside this scene, so it would otherwise stay locked into whichever example is loaded next.
    private void OnDisable()
    {
        if (m_CameraManipulator != null)
            m_CameraManipulator.DisableManipulators = false;
    }

    private void Update()
    {
        HandleInput();

        var world = PhysicsWorld.defaultWorld;

        DrawFirstRow(world);
        DrawSecondRow(world);
        DrawThirdRow(world);
    }

    // Circles and capsules against a range of other shapes.
    private void DrawFirstRow(PhysicsWorld world)
    {
        var offset = new Vector2(-12f, -5f);

        // Circle and circle.
        {
            var circle1 = new CircleGeometry { center = Vector2.zero, radius = 0.5f };
            var circle2 = new CircleGeometry { center = Vector2.zero, radius = 1f };

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.CircleAndCircle(circle1, transform1, circle2, transform2);

            world.DrawGeometry(circle1, transform1, BaseColor);
            world.DrawGeometry(circle2, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Capsule and circle.
        {
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0f), center2 = new Vector2(0.5f, 0f), radius = 0.25f };
            var circle = new CircleGeometry { center = Vector2.zero, radius = 0.5f };

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.CapsuleAndCircle(capsule, transform1, circle, transform2);

            world.DrawGeometry(capsule, transform1, BaseColor);
            world.DrawGeometry(circle, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Segment and circle.
        {
            var segment = new SegmentGeometry { point1 = new Vector2(-0.5f, 0f), point2 = new Vector2(0.5f, 0f) };
            var circle = new CircleGeometry { center = Vector2.zero, radius = 0.5f };

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.SegmentAndCircle(segment, transform1, circle, transform2);

            world.DrawGeometry(segment, transform1, BaseColor);
            world.DrawGeometry(circle, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Box and circle.
        {
            var box = PolygonGeometry.CreateBox(Vector2.one, radius: ShapeRadius);
            var circle = new CircleGeometry { center = Vector2.zero, radius = 0.5f };

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.PolygonAndCircle(box, transform1, circle, transform2);

            world.DrawGeometry(box, transform1, BaseColor);
            world.DrawGeometry(circle, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Capsule and capsule.
        {
            var capsule1 = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0f), center2 = new Vector2(0.5f, 0f), radius = 0.25f };
            var capsule2 = new CapsuleGeometry { center1 = new Vector2(-0.25f, 0f), center2 = new Vector2(0.25f, 0f), radius = 0.25f };

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.CapsuleAndCapsule(capsule1, transform1, capsule2, transform2);

            world.DrawGeometry(capsule1, transform1, BaseColor);
            world.DrawGeometry(capsule2, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Box and capsule.
        {
            var box = PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, 0f), PhysicsRotate.FromRadians(0.25f * PhysicsMath.PI)));
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.4f, 0f), center2 = new Vector2(-0.1f, 0f), radius = 0.25f };

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.PolygonAndCapsule(box, transform1, capsule, transform2);

            world.DrawGeometry(box, transform1, BaseColor);
            world.DrawGeometry(capsule, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Segment and capsule.
        {
            var segment = new SegmentGeometry { point1 = new Vector2(-1f, 0.3f), point2 = new Vector2(1f, 0.3f) };
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0f), center2 = new Vector2(0.5f, 0f), radius = 0.25f };

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.SegmentAndCapsule(segment, transform1, capsule, transform2);

            world.DrawGeometry(segment, transform1, BaseColor);
            world.DrawGeometry(capsule, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);
        }
    }

    // Polygons and rounded polygons against each other and against segments.
    private void DrawSecondRow(PhysicsWorld world)
    {
        var offset = new Vector2(-10f, 0f);

        // Box and box.
        {
            var box1 = PolygonGeometry.CreateBox(new Vector2(4f, 0.5f));
            var box2 = PolygonGeometry.CreateBox(new Vector2(0.5f, 0.75f));

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.PolygonAndPolygon(box1, transform1, box2, transform2);

            world.DrawGeometry(box1, transform1, BaseColor);
            world.DrawGeometry(box2, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Box and rounded box.
        {
            var box1 = PolygonGeometry.CreateBox(Vector2.one);
            var box2 = PolygonGeometry.CreateBox(Vector2.one, radius: ShapeRadius, inscribe: true);

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.PolygonAndPolygon(box1, transform1, box2, transform2);

            world.DrawGeometry(box1, transform1, BaseColor);
            world.DrawGeometry(box2, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Rounded box and rounded box.
        {
            var box = PolygonGeometry.CreateBox(Vector2.one, radius: ShapeRadius, inscribe: true);

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.PolygonAndPolygon(box, transform1, box, transform2);

            world.DrawGeometry(box, transform1, BaseColor);
            world.DrawGeometry(box, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Segment and rounded box.
        {
            var segment = new SegmentGeometry { point1 = new Vector2(-1f, 0f), point2 = new Vector2(1f, 0f) };
            var box = PolygonGeometry.CreateBox(Vector2.one, radius: ShapeRadius, inscribe: true);

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.SegmentAndPolygon(segment, transform1, box, transform2);

            world.DrawGeometry(segment, transform1, BaseColor);
            world.DrawGeometry(box, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Wedge and wedge, with the outline drawn again without its rounding to show the underlying polygon.
        {
            var wedge = PolygonGeometry.Create(new Vector2[] { new(-0.1f, -0.5f), new(0.1f, -0.5f), new(0f, 0.5f) }, radius: ShapeRadius);

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.PolygonAndPolygon(wedge, transform1, wedge, transform2);

            var touchColor = TouchColor(manifold);
            world.DrawGeometry(wedge, transform1, BaseColor);
            world.DrawGeometry(wedge, transform2, touchColor);

            wedge.radius = 0f;
            world.DrawGeometry(wedge, transform1, BaseColor, 0f, PhysicsWorld.DrawFillOptions.Outline);
            world.DrawGeometry(wedge, transform2, touchColor, 0f, PhysicsWorld.DrawFillOptions.Outline);

            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Rounded triangle and rounded triangle, with the outlines drawn again without their rounding.
        {
            var triangle1 = PolygonGeometry.Create(new Vector2[] { new(0.175740838f, 0.224936664f), new(-0.301293969f, 0.194021404f), new(-0.105151534f, -0.432157338f) }, radius: ShapeRadius);
            var triangle2 = PolygonGeometry.Create(new Vector2[] { new(-0.427884758f, -0.225028217f), new(0.0566576123f, -0.128772855f), new(0.176625848f, 0.338923335f) }, radius: ShapeRadius);

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.PolygonAndPolygon(triangle1, transform1, triangle2, transform2);

            var touchColor = TouchColor(manifold);
            world.DrawGeometry(triangle1, transform1, BaseColor);
            world.DrawGeometry(triangle2, transform2, touchColor);

            triangle1.radius = 0f;
            triangle2.radius = 0f;
            world.DrawGeometry(triangle1, transform1, BaseColor, 0f, PhysicsWorld.DrawFillOptions.Outline);
            world.DrawGeometry(triangle2, transform2, touchColor, 0f, PhysicsWorld.DrawFillOptions.Outline);

            DrawManifold(world, ref manifold);
        }
    }

    // Polygons against each other, and chain segments, which know their neighbors, against circles, rounded boxes and capsules.
    private void DrawThirdRow(PhysicsWorld world)
    {
        var offset = new Vector2(-10f, 5f);

        // Box and triangle.
        {
            var box = PolygonGeometry.CreateBox(new Vector2(2f, 2f));
            var triangle = PolygonGeometry.Create(new Vector2[] { new(-0.5f, 0.2f), new(0.5f, 0.2f), new(0f, 1.5f) });

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.PolygonAndPolygon(box, transform1, triangle, transform2);

            world.DrawGeometry(box, transform1, BaseColor);
            world.DrawGeometry(triangle, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Chain segment and circle.
        {
            var chainSegment = CreateFirstChainSegment();
            var circle = new CircleGeometry { center = new Vector2(0f, 0.25f), radius = 0.5f };

            GetTransforms(offset, out var transform1, out var transform2);
            var manifold = PhysicsQuery.ChainSegmentAndCircle(chainSegment, transform1, circle, transform2);

            DrawChainSegment(world, chainSegment, transform1, true, true, false);
            world.DrawGeometry(circle, transform2, TouchColor(manifold));
            DrawManifold(world, ref manifold);

            offset += NextColumn;
        }

        // Chain segments and rounded box.
        {
            offset += NextColumn;

            var chainSegment1 = CreateFirstChainSegment();
            var chainSegment2 = CreateSecondChainSegment();
            var box = PolygonGeometry.CreateBox(Vector2.one, radius: ShapeRadius, inscribe: true, transform: new PhysicsTransform(new Vector2(0f, 0.25f)));

            GetTransforms(offset, out var transform1, out var transform2);

            DrawChainSegment(world, chainSegment1, transform1, false, true, true);
            DrawChainSegment(world, chainSegment2, transform1, true, false, true);

            var manifold1 = PhysicsQuery.ChainSegmentAndPolygon(chainSegment1, transform1, box, transform2);
            var manifold2 = PhysicsQuery.ChainSegmentAndPolygon(chainSegment2, transform1, box, transform2);

            world.DrawGeometry(box, transform2, TouchColor(manifold1, manifold2));
            DrawManifold(world, ref manifold1);
            DrawManifold(world, ref manifold2);

            offset += NextColumn;
        }

        // Chain segments and capsule.
        {
            offset += NextColumn;

            var chainSegment1 = CreateFirstChainSegment();
            var chainSegment2 = CreateSecondChainSegment();
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0.25f), center2 = new Vector2(0.5f, 0.25f), radius = 0.25f };

            GetTransforms(offset, out var transform1, out var transform2);

            DrawChainSegment(world, chainSegment1, transform1, false, true, true);
            DrawChainSegment(world, chainSegment2, transform1, true, false, true);

            var manifold1 = PhysicsQuery.ChainSegmentAndCapsule(chainSegment1, transform1, capsule, transform2);
            var manifold2 = PhysicsQuery.ChainSegmentAndCapsule(chainSegment2, transform1, capsule, transform2);

            world.DrawGeometry(capsule, transform2, TouchColor(manifold1, manifold2));
            DrawManifold(world, ref manifold1);
            DrawManifold(world, ref manifold2);
        }
    }

    // Returns the fixed transform of the first shape of a pair, and the transform of the second shape, which follows the drag and rotation.
    private void GetTransforms(Vector2 offset, out PhysicsTransform transform1, out PhysicsTransform transform2)
    {
        transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
        transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };
    }

    // The two chain segments the chain segment pairs are tested against, where the second one continues on from the first.
    private static ChainSegmentGeometry CreateFirstChainSegment() => new()
    {
        ghost1 = new Vector2(2f, 1f),
        ghost2 = new Vector2(-2f, 0f),
        segment = new SegmentGeometry { point1 = new Vector2(1f, 1f), point2 = new Vector2(-1f, 0f) }
    };

    private static ChainSegmentGeometry CreateSecondChainSegment() => new()
    {
        ghost1 = new Vector2(3f, 1f),
        ghost2 = new Vector2(-1f, 0f),
        segment = new SegmentGeometry { point1 = new Vector2(2f, 1f), point2 = new Vector2(1f, 1f) }
    };

    // Draws a chain segment, and optionally the lines out to its two neighboring vertices and a dot on each of its own ends.
    private static void DrawChainSegment(PhysicsWorld world, ChainSegmentGeometry chainSegment, PhysicsTransform transform, bool drawGhost1, bool drawGhost2, bool drawPoints)
    {
        var point1 = transform.TransformPoint(chainSegment.segment.point1);
        var point2 = transform.TransformPoint(chainSegment.segment.point2);

        if (drawGhost1)
            world.DrawLine(transform.TransformPoint(chainSegment.ghost1), point1, Color.lightGray);

        world.DrawLine(point1, point2, BaseColor);

        if (drawGhost2)
            world.DrawLine(point2, transform.TransformPoint(chainSegment.ghost2), Color.lightGray);

        if (!drawPoints)
            return;

        world.DrawPoint(point1, 4f * PointScale, BaseColor);
        world.DrawPoint(point2, 4f * PointScale, BaseColor);
    }

    // Returns green when any of the manifolds has a contact point, otherwise salmon.
    private static Color TouchColor(PhysicsShape.ContactManifold manifold1, PhysicsShape.ContactManifold manifold2 = default)
        => manifold1.pointCount > 0 || manifold2.pointCount > 0 ? HitColor : NoHitColor;

    // Draws each contact point of a manifold as a dot with a line along the contact normal.
    private static void DrawManifold(PhysicsWorld world, ref PhysicsShape.ContactManifold manifold)
    {
        for (var i = 0; i < manifold.pointCount; ++i)
        {
            var point = manifold[i].pointA;

            world.DrawLine(point, point + manifold.normal * 0.5f, Color.white);
            world.DrawPoint(point, 5f * PointScale, Color.blue);
        }
    }

    // Drags or rotates the second shape of every pair with the left mouse button, rotating instead while the left Control key is held.
    private void HandleInput()
    {
        var currentKeyboard = Keyboard.current;
        var currentMouse = Mouse.current;

        if (currentMouse == null || m_CameraManipulator == null)
            return;

        if (currentMouse.leftButton.wasReleasedThisFrame)
        {
            m_ManipulatorState = ManipulatorState.None;
            return;
        }

        var worldPosition = (Vector2)m_CameraManipulator.Camera.ScreenToWorldPoint(currentMouse.position.ReadValue());

        switch (m_ManipulatorState)
        {
            case ManipulatorState.None:
            {
                if (!currentMouse.leftButton.wasPressedThisFrame)
                    return;

                m_ManipulatorStartPoint = worldPosition;

                if (currentKeyboard != null && currentKeyboard.leftCtrlKey.isPressed)
                {
                    m_ManipulatorState = ManipulatorState.Rotating;
                    m_ManipulatorBaseAngle = m_Angle;
                    return;
                }

                m_ManipulatorState = ManipulatorState.Dragging;
                m_ManipulatorBasePosition = m_Transform.position;

                return;
            }

            case ManipulatorState.Dragging:
            {
                var positionDelta = worldPosition - m_ManipulatorStartPoint;
                m_Transform.position = m_ManipulatorBasePosition + positionDelta * 0.5f;
                return;
            }

            case ManipulatorState.Rotating:
            {
                var rotationDelta = worldPosition.x - m_ManipulatorStartPoint.x;
                m_Angle = math.clamp(m_ManipulatorBaseAngle + rotationDelta, -PhysicsMath.PI, PhysicsMath.PI);
                m_Transform.rotation = PhysicsRotate.FromRadians(m_Angle);
                return;
            }
        }
    }

    #region Internal

    // What the left mouse button is currently doing to the second shape of every pair.
    enum ManipulatorState
    {
        None,
        Dragging,
        Rotating
    }

    // The gap between the columns of pairs, the radius given to the rounded shapes, and the size of a drawn point at a scale of one.
    static readonly Vector2 NextColumn = new(4f, 0f);
    const float ShapeRadius = 0.1f;
    const float PointScale = 0.01f;

    // The colors of the first shape of every pair, and of the second shape when it does and does not touch.
    static readonly Color BaseColor = Color.cornflowerBlue;
    static readonly Color NoHitColor = Color.lightSalmon;
    static readonly Color HitColor = Color.lightGreen;

    [SerializeField] CameraManipulator m_CameraManipulator;

    PhysicsTransform m_Transform;
    ManipulatorState m_ManipulatorState;
    Vector2 m_ManipulatorStartPoint;
    Vector2 m_ManipulatorBasePosition;
    float m_ManipulatorBaseAngle;
    float m_Angle;

    #endregion
}
