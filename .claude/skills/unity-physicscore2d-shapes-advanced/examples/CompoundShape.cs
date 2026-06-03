using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Compound (multi-shape) bodies. Two "tables" and two "spaceships" are each constructed from multiple
/// PhysicsShape primitives attached to a single dynamic PhysicsBody — the body's mass and AABB combine all
/// the shapes. Press Space to "intrude" an extra shape into each body at runtime; the new shape attaches to
/// the existing body without reconfiguring it. AABBs of each compound body are drawn as a debug overlay.
/// </summary>
public class CompoundShape : MonoBehaviour
{
    private PhysicsWorld m_PhysicsWorld;

    private PhysicsBody m_Table1;
    private PhysicsBody m_Table2;
    private PhysicsBody m_Ship1;
    private PhysicsBody m_Ship2;

    private void OnEnable()
    {
        m_PhysicsWorld = PhysicsWorld.defaultWorld;
        SetupScene();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            IntrudeShape();

        DrawBodyBounds(m_Table1, Color.red);
        DrawBodyBounds(m_Table2, Color.cyan);
        DrawBodyBounds(m_Ship1, Color.red);
        DrawBodyBounds(m_Ship2, Color.cyan);
    }

    private void SetupScene()
    {
        // Ground.
        {
            var groundBody = m_PhysicsWorld.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-25f, 0f),
                new(-25f, 23f),
                new(25f, 23f),
                new(25f, 0f)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        // Table 1: 3 boxes — top + 2 short legs — combined into one dynamic body.
        {
            m_Table1 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-15f, 1f) });
            m_Table1.CreateShape(PolygonGeometry.CreateBox(new Vector2(6f, 1f), 0f, new Vector2(0f, 3.5f)));
            m_Table1.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 3f), 0f, new Vector2(-2.5f, 1.5f)));
            m_Table1.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 3f), 0f, new Vector2(2.5f, 1.5f)));
        }

        // Table 2: same shape, taller legs — illustrates how mass distribution differs from Table 1.
        {
            m_Table2 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-5f, 1f) });
            m_Table2.CreateShape(PolygonGeometry.CreateBox(new Vector2(6f, 1f), 0f, new Vector2(0f, 3.5f)));
            m_Table2.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 4f), 0f, new Vector2(-2.5f, 2f)));
            m_Table2.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 4f), 0f, new Vector2(2.5f, 2f)));
        }

        // Spaceship 1: 2 triangles forming a non-convex outline (compound shapes can be concave overall).
        {
            m_Ship1 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(5f, 1f) });
            {
                using var vertices = new NativeList<Vector2>(Allocator.Temp)
                {
                    new(-2f, 0f),
                    new(0f, 4f / 3f),
                    new(0f, 4f)
                };
                m_Ship1.CreateShape(PolygonGeometry.Create(vertices.AsArray()));
            }
            {
                using var vertices = new NativeList<Vector2>(Allocator.Temp)
                {
                    new(2f, 0f),
                    new(0f, 4f / 3f),
                    new(0f, 4f)
                };
                m_Ship1.CreateShape(PolygonGeometry.Create(vertices.AsArray()));
            }
        }

        // Spaceship 2: alternative concave outline.
        {
            m_Ship2 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(15f, 1f) });
            {
                using var vertices = new NativeList<Vector2>(Allocator.Temp)
                {
                    new(-2f, 0f),
                    new(1f, 2f),
                    new(0f, 4f)
                };
                m_Ship2.CreateShape(PolygonGeometry.Create(vertices.AsArray()));
            }
            {
                using var vertices = new NativeList<Vector2>(Allocator.Temp)
                {
                    new(2f, 0f),
                    new(-1f, 2f),
                    new(0f, 4f)
                };
                m_Ship2.CreateShape(PolygonGeometry.Create(vertices.AsArray()));
            }
        }
    }

    private void IntrudeShape()
    {
        // Add a *new* shape to a separate body that overlaps the existing body's transform.
        // This demonstrates that you can stack independent bodies of compound shapes coincidentally.
        IntrudeAt(m_Table1, PolygonGeometry.CreateBox(new Vector2(8f, 0.2f), 0f, new Vector2(0f, 3.0f)));
        IntrudeAt(m_Table2, PolygonGeometry.CreateBox(new Vector2(8f, 0.2f), 0f, new Vector2(0f, 3.0f)));
        IntrudeAt(m_Ship1, new CircleGeometry { center = new Vector2(0f, 2f), radius = 0.5f });
        IntrudeAt(m_Ship2, new CircleGeometry { center = new Vector2(0f, 2f), radius = 0.5f });
    }

    private void IntrudeAt(PhysicsBody anchor, PolygonGeometry geometry)
    {
        var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = anchor.position, rotation = anchor.rotation });
        body.CreateShape(geometry);
    }

    private void IntrudeAt(PhysicsBody anchor, CircleGeometry geometry)
    {
        var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = anchor.position, rotation = anchor.rotation });
        body.CreateShape(geometry);
    }

    private void DrawBodyBounds(PhysicsBody body, Color color)
    {
        if (!body.isValid)
            return;

        // GetAABB returns the union of all attached shapes' AABBs.
        var aabb = body.GetAABB();
        m_PhysicsWorld.DrawBox(aabb.center, aabb.extents * 2f, 0f, color);
    }
}
