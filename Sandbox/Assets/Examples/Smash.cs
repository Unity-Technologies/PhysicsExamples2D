using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Benchmarks", "A large dense body smashing through a field of small boxes.")]
public sealed class Smash : SandboxExampleBehaviour
{
    private const int Columns = 100;
    private const int Rows = 60;

    private Vector2 m_OldGravity;
    private float m_Speed;
    private float m_Density;
    private float m_Bounciness;
    private float m_Spacing;
    private bool m_FastCollisionsAllowed;

    protected override float CameraSize => 60f;

    protected override void OnExampleEnable()
    {
        m_OldGravity = World.gravity;
        m_Speed = 60f;
        m_Density = 5;
        m_Bounciness = 0f;
        m_Spacing = 0f;
        m_FastCollisionsAllowed = false;
    }

    protected override void OnExampleDisable()
    {
        // Get the default world.
        var world = World;
        world.gravity = m_OldGravity;
    }

    protected override void SetupOptions()
    {
        // Speed.
        AddSlider("Speed", m_Speed, 0f, 100f, v => m_Speed = v, rebuild: true);

        // Density.
        AddSlider("Density", m_Density, 1f, 100f, v => m_Density = v, rebuild: true);

        // Bounciness.
        AddSlider("Bounciness", m_Bounciness, 0f, 1f, v => m_Bounciness = v, rebuild: true);

        // Spacing.
        AddSlider("Spacing", m_Spacing, 0f, 0.5f, v => m_Spacing = v, rebuild: true);

        // Fast Collisions.
        AddToggle("Fast Collisions", m_FastCollisionsAllowed, v => m_FastCollisionsAllowed = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Reset the gravity.
        world.gravity = Vector2.zero;

        // Border
        {
            var body = world.CreateBody();

            var extents = new Vector2(110f, 63f);

            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, extents.y * 2f), radius: 0f, new PhysicsTransform(new Vector2(-extents.x, 0f), PhysicsRotate.identity)), shapeDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, extents.y * 2f), radius: 0f, new PhysicsTransform(new Vector2(extents.x, 0f), PhysicsRotate.identity)), shapeDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(extents.x * 2f, 10f), radius: 0f, new PhysicsTransform(new Vector2(0f, -extents.y), PhysicsRotate.identity)), shapeDef);
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(extents.x * 2f, 10f), radius: 0f, new PhysicsTransform(new Vector2(0f, extents.y), PhysicsRotate.identity)), shapeDef);
        }

        // Large dense object.
        {
            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                position = new Vector2(-90f, 0f),
                linearVelocity = new Vector2(m_Speed, 0f),
                angularVelocity = PhysicsMath.PI * 0.1f,
                fastCollisionsAllowed = m_FastCollisionsAllowed
            };
            var body = world.CreateBody(bodyDef);
            body.CreateShape(
                PolygonGeometry.CreateBox(new Vector2(8f, 8f)),
                new PhysicsShapeDefinition { density = m_Density, surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = m_Bounciness } });
        }

        // Small objects.
        {
            var bodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                fastCollisionsAllowed = m_FastCollisionsAllowed,
                awake = false
            };
            var largeBody = world.CreateBody(bodyDef);

            const float boxDimension = 0.4f;
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(boxDimension, boxDimension));

            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = m_Bounciness } };

            var spacing = boxDimension + m_Spacing;
            for (var i = 0; i < Columns; ++i)
            {
                for (var j = 0; j < Rows; ++j)
                {
                    bodyDef.position = new Vector2(i * spacing - 60f, (j - Rows / 2.0f) * spacing);
                    var boxBody = world.CreateBody(bodyDef);

                    // Fetch the appropriate shape color.
                    shapeDef.surfaceMaterial.customColor = ShapeColor;

                    boxBody.CreateShape(boxGeometry, shapeDef);
                }
            }
        }
    }
}
