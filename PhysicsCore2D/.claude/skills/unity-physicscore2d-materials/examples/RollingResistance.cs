using System;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Rolling-resistance sweep across 20 stacked slopes. Each slope launches a circle with the same initial linear
/// and angular velocity but a higher `surfaceMaterial.rollingResistance` (0, 0.02, 0.04, …, 0.38). Higher
/// resistance → ball stops sooner. Slope can be flat, uphill, or downhill.
/// </summary>
public class RollingResistance : MonoBehaviour
{
    public enum SlopeType { Uphill, Flat, Downhill }

    public SlopeType Slope = SlopeType.Flat;

    private void OnEnable()
    {
        SetupScene();
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

        var slopeAngle = Slope switch
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

        for (var n = 0; n < 20; ++n)
        {
            // Slope: left wall + ramp + right wall, built from segment geometries on a single static body.
            var groundBody = world.CreateBody();
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-40f, 2f * n), point2 = new Vector2(-40f, 2f * n + 1.5f) }, slopeShapeDef);
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-40f, 2f * n), point2 = new Vector2(40f, 2f * n + slopeAngle) }, slopeShapeDef);
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(40f, 2f * n + slopeAngle), point2 = new Vector2(40f, 2f * n + slopeAngle + 1.5f) }, slopeShapeDef);

            bodyDef.position = new Vector2(-39.5f, 2f * n + 0.75f);
            var ballBody = world.CreateBody(bodyDef);
            ballShapeDef.surfaceMaterial.rollingResistance = 0.02f * n;
            ballBody.CreateShape(circle, ballShapeDef);
        }
    }
}
