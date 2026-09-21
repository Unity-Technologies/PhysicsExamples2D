using Unity.U2D.Physics;
using UnityEngine;

public static class GearFactory
{
    // The tooth dimensions, as fractions of the gear radius, so a gear scales as a whole.
    // They are public because a caller placing geometry that meshes with the teeth needs to know how far they reach.
    public const float ToothHalfWidthScale = 0.09f;
    public const float ToothHalfHeightScale = 0.06f;
    public const float ToothRadiusScale = 0.03f;

    // How many teeth ring the gear.
    public const int ToothCount = 16;

    // Creates a toothed gear at gearPosition and hinges it to groundBody, returning the gear's body.
    // The gear is a circle of gearRadius ringed by ToothCount rounded boxes, each sized from the scales above.
    // The hinge's dynamics belong to the caller: only bodyA, bodyB and the two anchors are filled in here, and everything
    // else in definition is used as given, which is what lets one gear run a motor while another is held between limits.
    public static PhysicsBody Spawn(PhysicsWorld world, PhysicsBody groundBody, IShapeColorProvider colorProvider, PhysicsShape.ContactFilter contactFilter, Vector2 gearPosition, float gearRadius, PhysicsHingeJointDefinition definition, out PhysicsHingeJoint hingeJoint)
    {
        var toothHalfWidth = ToothHalfWidthScale * gearRadius;
        var toothHalfHeight = ToothHalfHeightScale * gearRadius;
        var toothRadius = ToothRadiusScale * gearRadius;

        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = gearPosition
        };

        var gearBody = world.CreateBody(bodyDef);

        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = contactFilter,
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f }
        };

        // Gear.
        {
            if (colorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = Color.saddleBrown;

            var circle = new CircleGeometry { radius = gearRadius };
            gearBody.CreateShape(circle, shapeDef);
        }

        // Teeth.
        {
            shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f };

            if (colorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = Color.gray;

            var dq = PhysicsRotate.FromRadians(PhysicsMath.TAU / ToothCount);
            var center = new Vector2(gearRadius + toothHalfHeight, 0f);
            var rotation = PhysicsRotate.identity;

            for (var i = 0; i < ToothCount; ++i)
            {
                var tooth = PolygonGeometry.CreateBox(
                    size: new Vector2(toothHalfWidth, toothHalfHeight) * 2f,
                    radius: toothRadius,
                    transform: new PhysicsTransform(center, rotation));

                gearBody.CreateShape(tooth, shapeDef);

                rotation = dq.MultiplyRotation(rotation);
                center = rotation.RotateVector(new Vector2(gearRadius + toothHalfHeight, 0f));
            }
        }

        // Hinge the gear to the ground, leaving the caller's motor and limit settings alone.
        definition.bodyA = groundBody;
        definition.bodyB = gearBody;
        definition.localAnchorA = groundBody.GetLocalPoint(gearPosition);
        definition.localAnchorB = Vector2.zero;

        hingeJoint = world.CreateJoint(definition);

        return gearBody;
    }
}
