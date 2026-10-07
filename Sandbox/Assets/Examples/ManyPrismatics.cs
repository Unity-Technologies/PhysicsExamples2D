using UnityEngine;
using Unity.U2D.Physics;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Joints", "A chain of slider joints that stays stable when it is stretched. Drag a box sideways to distort the chain.",
    Purpose = "A chain of boxes joined by slider joints, stretched to the end of their limits.\nThe number of sub-steps the world takes in each step decides how well stretched joints hold. With too few, the chain bends away from its slide axis.",
    Controls = "World Sub-Steps: sets how many sub-steps the world takes in each step. Raise it and the chain holds straight, lower it and the chain bends.\nDrag a box sideways to distort the chain and see how quickly it recovers.")]
public sealed class ManyPrismatics : SandboxExampleBehaviour
{
    private const int BoxCount = 6;
    private const float BoxHalfSize = 0.5f;
    private const float JointOffset = 0.6f;
    private const float TranslationLimit = 6f;
    private const float JointFrequency = 240f;

    private int m_SubSteps;

    protected override float CameraSize => 12.5f;
    protected override Vector2 CameraPosition => new(0f, 3.6f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);

        // The number of sub-steps decides how well the joints hold when they are stretched, so the example takes control of it and puts back the menu's value when it unloads.
        m_SubSteps = 4;
        SandboxManager.SetOverrideSubSteps(m_SubSteps);
    }

    protected override void SetupOptions()
    {
        // World Sub-Steps.
        AddSliderInt("World Sub-Steps", m_SubSteps, 1, 64, v =>
        {
            m_SubSteps = v;
            SandboxManager.SetOverrideSubSteps(m_SubSteps);
        });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground Body.
        var groundBody = world.CreateBody();

        var box = PolygonGeometry.CreateBox(new Vector2(BoxHalfSize * 2f, BoxHalfSize * 2f));
        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = new PhysicsShapeDefinition();

        // Each box slides sideways along the one below it, and the first slides along the ground, all within the same limits.
        var jointDef = new PhysicsSliderJointDefinition
        {
            bodyA = groundBody,
            autoAxis = false,
            localAnchorA = new PhysicsTransform(Vector2.zero, PhysicsRotate.identity),
            localAnchorB = new PhysicsTransform(new Vector2(0f, -JointOffset), PhysicsRotate.identity),
            drawScale = 1f,
            tuningFrequency = JointFrequency,
            enableLimit = true,
            lowerTranslationLimit = -TranslationLimit,
            upperTranslationLimit = TranslationLimit
        };

        for (var i = 0; i < BoxCount; ++i)
        {
            bodyDef.position = new Vector2(0f, JointOffset + 2f * JointOffset * i);
            var body = world.CreateBody(bodyDef);
            body.CreateShape(box, shapeDef);

            jointDef.bodyB = body;
            world.CreateJoint(jointDef);

            // The next box slides along this one, from its top.
            jointDef.bodyA = body;
            jointDef.localAnchorA = new PhysicsTransform(new Vector2(0f, JointOffset), PhysicsRotate.identity);
        }
    }
}
