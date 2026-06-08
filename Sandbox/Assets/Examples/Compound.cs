using Unity.Collections;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrates how multiple shapes on a single body produce a compound shape and how to mitigate overlaps.")]
public sealed class Compound : SandboxExampleBehaviour
{
    private ControlsMenu.CustomButton m_IntrudeButton;

    private PhysicsBody m_Table1;
    private PhysicsBody m_Table2;
    private PhysicsBody m_Ship1;
    private PhysicsBody m_Ship2;

    protected override float CameraSize => 21.43f;
    protected override Vector2 CameraPosition => new(1.01f, 11.82f);

    protected override void OnExampleEnable()
    {
        // Set controls.
        m_IntrudeButton = SandboxManager.ControlsMenu[0];
        m_IntrudeButton.Set("Intrude");
        m_IntrudeButton.button.clickable.clicked += IntrudeShape;

        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);
    }

    protected override void OnExampleDisable()
    {
        // Unregister.
        m_IntrudeButton.button.clickable.clicked -= IntrudeShape;
    }

    protected override void SetupScene()
    {
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-25f, 0f),
                new(-25f, 23f),
                new(25f, 23f),
                new(25f, 0f)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        // Table 1.
        {
            m_Table1 = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-15f, 1f) });

            m_Table1.CreateShape(PolygonGeometry.CreateBox(new Vector2(6f, 1f), 0f, new Vector2(0f, 3.5f)));
            m_Table1.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 3f), 0f, new Vector2(-2.5f, 1.5f)));
            m_Table1.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 3f), 0f, new Vector2(2.5f, 1.5f)));
        }

        // Table 2.
        {
            m_Table2 = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-5f, 1f) });

            m_Table2.CreateShape(PolygonGeometry.CreateBox(new Vector2(6f, 1f), 0f, new Vector2(0f, 3.5f)));
            m_Table2.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 4f), 0f, new Vector2(-2.5f, 2f)));
            m_Table2.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 4f), 0f, new Vector2(2.5f, 2f)));
        }

        // Spaceship 1.
        {
            m_Ship1 = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(5f, 1f) });

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

        // Spaceship 2.
        {
            m_Ship2 = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(15f, 1f) });

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
        var world = World;

        // Table 1 intrusion.
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = m_Table1.position, rotation = m_Table1.rotation });
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(8f, 0.2f), 0f, new Vector2(0f, 3.0f)));
        }

        // Table 2 intrusion.
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = m_Table2.position, rotation = m_Table2.rotation });
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(8f, 0.2f), 0f, new Vector2(0f, 3.0f)));
        }

        // Ship 1 intrusion.
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = m_Ship1.position, rotation = m_Ship1.rotation });
            body.CreateShape(new CircleGeometry { center = new Vector2(0f, 2f), radius = 0.5f });
        }

        // Ship 2 intrusion.
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = m_Ship2.position, rotation = m_Ship2.rotation });
            body.CreateShape(new CircleGeometry { center = new Vector2(0f, 2f), radius = 0.5f });
        }
    }

    private void Update()
    {
        DrawBodyBounds(m_Table1, Color.red);
        DrawBodyBounds(m_Table2, Color.cyan);
        DrawBodyBounds(m_Ship1, Color.red);
        DrawBodyBounds(m_Ship2, Color.cyan);
    }

    private void DrawBodyBounds(PhysicsBody body, Color color)
    {
        if (!body.isValid)
            return;

        // Calculate the body AABB.
        var aabb = body.GetAABB();

        // Draw the AABB.
        World.DrawBox(aabb.center, aabb.extents * 2f, 0f, color);
    }
}
