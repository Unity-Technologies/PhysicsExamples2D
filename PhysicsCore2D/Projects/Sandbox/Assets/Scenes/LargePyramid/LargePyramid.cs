using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

[ExampleScene("Benchmarks", "A large pyramid of boxes stress-testing stacking stability.")]
public sealed class LargePyramid : SandboxExampleBehaviour
{
    private int m_BaseCount;
    private Vector2 m_OldGravity;
    private float m_GravityScale;

    protected override float CameraSize => 80f;
    protected override Vector2 CameraPosition => new(0f, 79f);

    protected override void OnExampleEnable()
    {
        m_BaseCount = 60;
        m_OldGravity = World.gravity;
        m_GravityScale = 1f;
    }

    protected override void OnExampleDisable()
    {
        // Get the default world.
        var world = World;
        world.gravity = m_OldGravity;
    }

    protected override void SetupOptions()
    {
        // Base Count.
        AddSliderInt("Base Count", m_BaseCount, 2, 150, v => m_BaseCount = v, rebuild: true);

        // Gravity Scale.
        AddSlider("Gravity Scale", m_GravityScale, 0.1f, 5f, v =>
        {
            m_GravityScale = v;

            // Get the default world.
            var world = World;
            world.gravity = m_OldGravity * m_GravityScale;
        }, rebuild: false);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(0f, -1f) });

            const float groundLength = 1000f;
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(groundLength, 2f)), PhysicsShapeDefinition.defaultDefinition);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(groundLength, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, groundLength), PhysicsRotate.identity)), PhysicsShapeDefinition.defaultDefinition);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, groundLength), radius: 0f, new PhysicsTransform(new Vector2(groundLength * -0.5f, groundLength * 0.5f), PhysicsRotate.identity)), PhysicsShapeDefinition.defaultDefinition);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, groundLength), radius: 0f, new PhysicsTransform(new Vector2(groundLength * 0.5f, groundLength * 0.5f), PhysicsRotate.identity)), PhysicsShapeDefinition.defaultDefinition);
        }

        // Pyramid.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            const float halfHeight = 0.5f;
            const float radius = 0.05f;
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(halfHeight - radius, halfHeight - radius) * 2f, radius);

            const float shift = 1.0f * halfHeight;

            for (var i = 0; i < m_BaseCount; ++i)
            {
                var y = (2.0f * i + 1.0f) * shift;

                for (var j = i; j < m_BaseCount; ++j)
                {
                    var x = (i + 1.0f) * shift + 2.0f * (j - i) * shift - halfHeight * m_BaseCount;

                    bodyDef.position = new Vector2(x, y);
                    var body = world.CreateBody(bodyDef);

                    shapeDef.surfaceMaterial.customColor = ShapeColor;
                    body.CreateShape(boxGeometry, shapeDef);
                }
            }
        }
    }
}
