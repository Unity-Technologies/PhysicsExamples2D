using System;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrating Rolling Resistance Surface Property.")]
public sealed class RollingResistance : SandboxExampleBehaviour
{
    private SlopeType m_SlopeType;

    private enum SlopeType
    {
        Uphill,
        Flat,
        Downhill
    }

    protected override float CameraSize => 35f;
    protected override Vector2 CameraPosition => new(0f, 20f);

    protected override void OnExampleEnable()
    {
        m_SlopeType = SlopeType.Flat;
    }

    protected override void SetupOptions()
    {
        AddEnum("Slope Type", m_SlopeType, v => m_SlopeType = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        var world = World;

        // Slopes.
        {
            var slopeAngle = m_SlopeType switch
            {
                SlopeType.Uphill => 5f,
                SlopeType.Flat => 0f,
                SlopeType.Downhill => -5f,
                _ => throw new ArgumentOutOfRangeException()
            };

            var circle = new CircleGeometry { radius = 0.5f };
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, linearVelocity = new Vector2(5f, 0f), angularVelocity = -10f };
            var slopeShapeDef = PhysicsShapeDefinition.defaultDefinition;
            var ballShapeDef = PhysicsShapeDefinition.defaultDefinition;

            // Add slopes.
            for (var n = 0; n < 20; ++n)
            {
                // Create Slope.
                var groundBody = world.CreateBody();
                groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-40f, 2f * n), point2 = new Vector2(-40f, 2f * n + 1.5f) }, slopeShapeDef);
                groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-40f, 2f * n), point2 = new Vector2(40f, 2f * n + slopeAngle) }, slopeShapeDef);
                groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(40f, 2f * n + slopeAngle), point2 = new Vector2(40f, 2f * n + slopeAngle + 1.5f) }, slopeShapeDef);

                // Create Ball.
                bodyDef.position = new Vector2(-39.5f, 2f * n + 0.75f);
                var ballBody = world.CreateBody(bodyDef);
                ballShapeDef.surfaceMaterial.customColor = ShapeColor;
                ballShapeDef.surfaceMaterial.rollingResistance = 0.02f * n;
                ballBody.CreateShape(circle, ballShapeDef);
            }
        }
    }
}
