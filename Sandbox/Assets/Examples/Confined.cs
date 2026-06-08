using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

[ExampleScene("Shapes", "Showing the solver dealing with tight/overlapping confinement.")]
public sealed class Confined : SandboxExampleBehaviour
{
    private int m_GridCount;
    private Vector2 m_OldGravity;

    protected override float CameraSize => 12f;
    protected override Vector2 CameraPosition => new(0f, 10f);

    protected override void OnExampleEnable()
    {
        m_GridCount = 25;
        m_OldGravity = World.gravity;
    }

    protected override void OnExampleDisable()
    {
        var world = World;
        world.gravity = m_OldGravity;
    }

    protected override void SetupOptions()
    {
        AddSliderInt("Grid Count", m_GridCount, 20, 50, v => m_GridCount = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        var world = World;

        // Reset the gravity.
        world.gravity = Vector2.zero;

        // Confining Border.
        {
            var body = world.CreateBody();

            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-10.5f, 0f), center2 = new Vector2(10.5f, 0f), radius = 0.5f }, shapeDef);
            body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-10.5f, 0f), center2 = new Vector2(-10.5f, 20.5f), radius = 0.5f }, shapeDef);
            body.CreateShape(new CapsuleGeometry { center1 = new Vector2(10.5f, 0f), center2 = new Vector2(10.5f, 20.5f), radius = 0.5f }, shapeDef);
            body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-10.5f, 20.5f), center2 = new Vector2(10.5f, 20.5f), radius = 0.5f }, shapeDef);
        }

        // Confined Objects.
        {
            var column = 0;
            var count = 0;

            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                gravityScale = 0f
            };

            var maxCount = m_GridCount * m_GridCount;
            var circleGeometry = new CircleGeometry { center = new Vector2(0f, 0f), radius = 0.5f };
            while (count < maxCount)
            {
                var row = 0;
                for (var i = 0; i < m_GridCount; ++i)
                {
                    bodyDef.position = new Vector2(-8.75f + column * 18.0f / m_GridCount, 1.5f + row * 18.0f / m_GridCount);
                    var body = world.CreateBody(bodyDef);

                    var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = ShapeColor } };
                    body.CreateShape(circleGeometry, shapeDef);

                    ++count;
                    ++row;
                }

                ++column;
            }
        }
    }
}
