using System;
using UnityEngine;
using Unity.U2D.Physics;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Joints", "An Anglepoise style desk lamp whose springs balance the arm and shade at any pose. Drag the shade to move it.",
    Purpose = "Demonstrates joints working together as a mechanism, with springs, joint friction and limits. The lamp's arm is a parallelogram of links, so the shade keeps its angle as the arm moves, and two springs balance the arm so it holds any pose it is left in.\nThe joint friction is a hinge motor with a target speed of zero, which resists movement up to its maximum torque.",
    Controls = "Drag the shade, or any part of the lamp, to pose it. Let go and the springs hold it there.\nArm Spring and Head Spring: how stiff the two springs are.\nSpring Damping: how much the springs are damped.\nPost Friction and Elbow Friction: how much friction the joints have.\nHead: the angle of the shade.")]
public sealed class DeskLamp : SandboxExampleBehaviour
{
    // The dimensions of an Original 1227 in meters, scaled up because at life size the silver link is two centimeters, which is far too close to the linear slop.
    private const float Scale = 5f;
    private const float BaseHalfWidth = 0.070f * Scale;
    private const float BaseHalfHeight = 0.018f * Scale;
    private const float PostHalfWidth = 0.008f * Scale;
    private const float PivotHeight = 0.185f * Scale;
    private const float SpringAnchorHeight = 0.095f * Scale;
    private const float TopLinkLength = 0.296f * Scale;
    private const float SilverFoot = 0.024f * Scale;
    private const float TopLeverLength = 0.019f * Scale;
    private const float LowerLeverLength = 0.028f * Scale;
    private const float UpperArmLength = 0.324f * Scale;
    private const float TubeRadius = 0.0036f * Scale;
    private const float ShadeLength = 0.161f * Scale;
    private const float ShadeNeckRadius = 0.020f * Scale;
    private const float ShadeMouthRadius = 0.061f * Scale;
    private const float ShadeRoundedRadius = 0.01f * Scale;

    // The pose the lamp starts in, in radians.
    private const float TopLinkAngle = -0.878f;
    private const float UpperArmAngle = 1.192f;
    private const float ShadeTilt = -0.41f;

    // The head can turn this far either way from its target angle, in degrees.
    private const float HeadLimit = 68.75f;

    // The ground is flat between these two points, then slopes up and out past the edges of the view.
    private const float GroundFlatHalfWidth = 2.5f;
    private const float GroundSlopeRun = 4.2f;
    private const float GroundSlopeRise = 6f;

    // How far along the ground the lamp's base is.
    private const float LampX = 0.9f;

    // The child's ball sits to the left of the lamp, about as wide as the lamp's base.
    private const float BallRadius = 0.35f;
    private const float BallX = -0.15f;

    private static readonly Vector2 SilverSpan = new Vector2(0.0212f, 0.0109f) * Scale;
    private static readonly Color SilverColor = Color.silver;

    private float m_ArmSpring;
    private float m_HeadSpring;
    private float m_SpringDamping;
    private float m_PostFriction;
    private float m_ElbowFriction;
    private float m_HeadAngle;

    private readonly PhysicsDistanceJoint[] m_SpringJoints = new PhysicsDistanceJoint[2];
    private PhysicsHingeJoint m_PostJoint;
    private PhysicsHingeJoint m_ElbowJoint;
    private PhysicsHingeJoint m_HeadJoint;

    protected override float CameraSize => 1.6f;
    protected override Vector2 CameraPosition => new(0f, 1.42f);

    protected override void OnExampleEnable()
    {
        // The arm spring balances the elevation, and the head spring balances the upper arm through the parallelogram, which is what the real lamp uses two springs for.
        m_ArmSpring = 5f;
        m_HeadSpring = 5f;
        m_SpringDamping = 4f;
        m_PostFriction = 100f;
        m_ElbowFriction = 100f;
        m_HeadAngle = 0f;
    }

    protected override void SetupOptions()
    {
        // Arm Spring.
        AddSlider("Arm Spring", m_ArmSpring, 0f, 20f, v =>
        {
            m_ArmSpring = v;
            SetSpringFrequency(0, v);
        });

        // Head Spring.
        AddSlider("Head Spring", m_HeadSpring, 0f, 20f, v =>
        {
            m_HeadSpring = v;
            SetSpringFrequency(1, v);
        });

        // Spring Damping.
        AddSlider("Spring Damping", m_SpringDamping, 0f, 5f, v =>
        {
            m_SpringDamping = v;

            for (var i = 0; i < m_SpringJoints.Length; ++i)
            {
                if (!m_SpringJoints[i].isValid)
                    continue;

                m_SpringJoints[i].springDamping = v;
                m_SpringJoints[i].WakeBodies();
            }
        });

        // Post Friction.
        AddSlider("Post Friction", m_PostFriction, 0f, 200f, v =>
        {
            m_PostFriction = v;

            if (!m_PostJoint.isValid)
                return;

            m_PostJoint.maxMotorTorque = v;
            m_PostJoint.WakeBodies();
        });

        // Elbow Friction.
        AddSlider("Elbow Friction", m_ElbowFriction, 0f, 200f, v =>
        {
            m_ElbowFriction = v;

            if (!m_ElbowJoint.isValid)
                return;

            m_ElbowJoint.maxMotorTorque = v;
            m_ElbowJoint.WakeBodies();
        });

        // Head.
        AddSlider("Head", m_HeadAngle, -HeadLimit, HeadLimit, v =>
        {
            m_HeadAngle = v;

            if (!m_HeadJoint.isValid)
                return;

            m_HeadJoint.springTargetAngle = v;
            m_HeadJoint.WakeBodies();
        });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();
            var groundShapeDef = new PhysicsShapeDefinition();

            var outerX = GroundFlatHalfWidth + GroundSlopeRun;
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-GroundFlatHalfWidth, 0f), point2 = new Vector2(GroundFlatHalfWidth, 0f) }, groundShapeDef);
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-GroundFlatHalfWidth, 0f), point2 = new Vector2(-outerX, GroundSlopeRise) }, groundShapeDef);
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(GroundFlatHalfWidth, 0f), point2 = new Vector2(outerX, GroundSlopeRise) }, groundShapeDef);
        }

        // Ball.
        {
            var ballBody = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(BallX, BallRadius), collisionThreshold = 0f });

            var ballShapeDef = new PhysicsShapeDefinition();
            ballShapeDef.surfaceMaterial.customColor = Color.yellow;
            ballShapeDef.surfaceMaterial.bounciness = 0.6f;
            ballBody.CreateShape(new CircleGeometry { radius = BallRadius }, ballShapeDef);
        }

        // The silver link is measured in the top link's frame, and the same span separates the two pins on the upper arm, which is what closes the parallelogram.
        var silverLength = SilverSpan.magnitude;

        var topRotation = PhysicsRotate.FromRadians(TopLinkAngle);
        var upperRotation = PhysicsRotate.FromRadians(UpperArmAngle);
        var shadeRotation = PhysicsRotate.FromRadians(ShadeTilt);

        var topAxis = topRotation.RotateVector(Vector2.up);
        var silverWorld = topRotation.RotateVector(SilverSpan);

        var postPivot = new Vector2(LampX, PivotHeight);
        var upperPin = postPivot + TopLinkLength * topAxis;
        var silverBase = postPivot + SilverFoot * topAxis;
        var lowerFoot = silverBase + silverWorld;
        var lowerPin = upperPin + silverWorld;

        // The lower link is parallel to the top link and shorter by the silver link's foot.
        var lowerLinkLength = TopLinkLength - SilverFoot;
        var silverRotation = new PhysicsRotate(new Vector2(silverWorld.y, -silverWorld.x));

        var headPivot = upperPin + UpperArmLength * upperRotation.RotateVector(Vector2.up);

        // The base is static so the linkage is all that moves.
        var baseBody = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Static, position = new Vector2(LampX, 0f) });
        {
            var shapeDef = new PhysicsShapeDefinition();

            var plinth = PolygonGeometry.CreateBox(new Vector2(BaseHalfWidth * 2f, BaseHalfHeight * 2f), 0f, new PhysicsTransform(new Vector2(0f, BaseHalfHeight), PhysicsRotate.identity));
            var postHalfHeight = 0.5f * (PivotHeight - 2f * BaseHalfHeight);
            var post = PolygonGeometry.CreateBox(new Vector2(PostHalfWidth * 2f, postHalfHeight * 2f), 0f, new PhysicsTransform(new Vector2(0f, PivotHeight - postHalfHeight), PhysicsRotate.identity));

            baseBody.CreateShape(plinth, shapeDef);
            baseBody.CreateShape(post, shapeDef);
        }

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, sleepingAllowed = false, collisionThreshold = 0f };
        var linkShapeDef = new PhysicsShapeDefinition { density = 20f };

        // Both long links run past their feet into the levers the springs pull on.
        bodyDef.position = postPivot;
        bodyDef.rotation = topRotation;
        var topLinkBody = world.CreateBody(bodyDef);
        topLinkBody.CreateShape(new CapsuleGeometry { center1 = new Vector2(0f, -TopLeverLength), center2 = new Vector2(0f, TopLinkLength), radius = TubeRadius }, linkShapeDef);

        bodyDef.position = lowerFoot;
        bodyDef.rotation = topRotation;
        var lowerLinkBody = world.CreateBody(bodyDef);
        lowerLinkBody.CreateShape(new CapsuleGeometry { center1 = new Vector2(0f, -LowerLeverLength), center2 = new Vector2(0f, lowerLinkLength), radius = TubeRadius }, linkShapeDef);

        bodyDef.position = silverBase;
        bodyDef.rotation = silverRotation;
        var silverBody = world.CreateBody(bodyDef);
        {
            var silverShapeDef = linkShapeDef;
            silverShapeDef.surfaceMaterial.customColor = SilverColor;
            silverBody.CreateShape(new CapsuleGeometry { center1 = Vector2.zero, center2 = new Vector2(0f, silverLength), radius = 1.4f * TubeRadius }, silverShapeDef);
        }

        bodyDef.position = upperPin;
        bodyDef.rotation = upperRotation;
        var upperArmBody = world.CreateBody(bodyDef);
        upperArmBody.CreateShape(new CapsuleGeometry { center1 = Vector2.zero, center2 = new Vector2(0f, UpperArmLength), radius = 0.8f * TubeRadius }, linkShapeDef);

        // The coupler span reaches back to the lower link's pin.
        var lowerPinLocal = upperArmBody.GetLocalPoint(lowerPin);
        upperArmBody.CreateShape(new CapsuleGeometry { center1 = Vector2.zero, center2 = lowerPinLocal, radius = TubeRadius }, linkShapeDef);

        // Pins.
        var pinDef = new PhysicsHingeJointDefinition
        {
            drawScale = 0.03f * Scale,
            tuningFrequency = 240f,
            tuningDamping = 2f,
            enableMotor = false,
            motorSpeed = 0f
        };

        // Only the top link reaches the post. The springs carry it, so it needs little friction.
        pinDef.bodyA = baseBody;
        pinDef.bodyB = topLinkBody;
        pinDef.localAnchorA = new PhysicsTransform(baseBody.GetLocalPoint(postPivot), PhysicsRotate.identity);
        pinDef.localAnchorB = PhysicsTransform.identity;
        pinDef.enableMotor = true;
        pinDef.maxMotorTorque = m_PostFriction;
        m_PostJoint = world.CreateJoint(pinDef);

        pinDef.enableMotor = false;

        // The silver link hangs off the top link and carries the foot of the lower link.
        pinDef.bodyA = topLinkBody;
        pinDef.bodyB = silverBody;
        pinDef.localAnchorA = new PhysicsTransform(new Vector2(0f, SilverFoot), PhysicsRotate.identity);
        pinDef.localAnchorB = PhysicsTransform.identity;
        world.CreateJoint(pinDef);

        pinDef.bodyA = silverBody;
        pinDef.bodyB = lowerLinkBody;
        pinDef.localAnchorA = new PhysicsTransform(new Vector2(0f, silverLength), PhysicsRotate.identity);
        pinDef.localAnchorB = PhysicsTransform.identity;
        world.CreateJoint(pinDef);

        // Both long links reach the upper arm, at points a silver link apart.
        pinDef.bodyA = lowerLinkBody;
        pinDef.bodyB = upperArmBody;
        pinDef.localAnchorA = new PhysicsTransform(new Vector2(0f, lowerLinkLength), PhysicsRotate.identity);
        pinDef.localAnchorB = new PhysicsTransform(lowerPinLocal, PhysicsRotate.identity);
        world.CreateJoint(pinDef);

        // No spring reaches the elbow, so it is a friction pivot like the real one.
        pinDef.bodyA = topLinkBody;
        pinDef.bodyB = upperArmBody;
        pinDef.localAnchorA = new PhysicsTransform(new Vector2(0f, TopLinkLength), PhysicsRotate.identity);
        pinDef.localAnchorB = PhysicsTransform.identity;
        pinDef.enableMotor = true;
        pinDef.maxMotorTorque = m_ElbowFriction;
        m_ElbowJoint = world.CreateJoint(pinDef);

        // Springs.
        // With almost no free length, the spring's torque about the pivot follows the gravity torque as the arm swings, so the arm balances over its whole range instead of at one angle.
        var springDef = new PhysicsDistanceJointDefinition
        {
            drawScale = 0.03f * Scale,
            enableSpring = true,
            springDamping = m_SpringDamping,
            autoDistance = false,
            distance = 0.01f
        };

        var springBase = new Vector2(LampX, SpringAnchorHeight);
        var leverBodies = new[] { topLinkBody, lowerLinkBody };
        var leverLengths = new[] { TopLeverLength, LowerLeverLength };
        var springFrequencies = new[] { m_ArmSpring, m_HeadSpring };

        for (var i = 0; i < m_SpringJoints.Length; ++i)
        {
            springDef.bodyA = baseBody;
            springDef.bodyB = leverBodies[i];
            springDef.localAnchorA = new PhysicsTransform(baseBody.GetLocalPoint(springBase), PhysicsRotate.identity);
            springDef.localAnchorB = new PhysicsTransform(new Vector2(0f, -leverLengths[i]), PhysicsRotate.identity);
            springDef.springFrequency = springFrequencies[i];
            m_SpringJoints[i] = world.CreateJoint(springDef);
        }

        // Head.
        {
            bodyDef.position = headPivot;
            bodyDef.rotation = shadeRotation;
            var headBody = world.CreateBody(bodyDef);

            // The pivot is on the shade's center line, where the arm capsule ends, and the shade opens along the local negative y.
            var neck = 0.45f * ShadeLength;
            var mouth = neck - ShadeLength;
            Span<Vector2> vertices = stackalloc Vector2[4]
            {
                new Vector2(-ShadeNeckRadius, neck),
                new Vector2(ShadeNeckRadius, neck),
                new Vector2(ShadeMouthRadius, mouth),
                new Vector2(-ShadeMouthRadius, mouth)
            };

            // The shade is a thin shell.
            var shadeDef = new PhysicsShapeDefinition { density = 1f };
            headBody.CreateShape(PolygonGeometry.Create(vertices, ShadeRoundedRadius), shadeDef);

            // Offset the reference frame so the head reads zero at the pose above.
            pinDef.bodyA = upperArmBody;
            pinDef.bodyB = headBody;
            pinDef.localAnchorA = new PhysicsTransform(new Vector2(0f, UpperArmLength), PhysicsRotate.FromRadians(ShadeTilt - UpperArmAngle));
            pinDef.localAnchorB = PhysicsTransform.identity;
            pinDef.enableMotor = false;
            pinDef.maxMotorTorque = 10f;
            pinDef.enableSpring = true;
            pinDef.springFrequency = 8f;
            pinDef.springDamping = 2f;
            pinDef.springTargetAngle = m_HeadAngle;
            pinDef.enableLimit = true;
            pinDef.lowerAngleLimit = -HeadLimit;
            pinDef.upperAngleLimit = HeadLimit;
            m_HeadJoint = world.CreateJoint(pinDef);
        }
    }

    // Sets the frequency of one of the springs and wakes the lamp so the change is seen.
    private void SetSpringFrequency(int index, float frequency)
    {
        if (!m_SpringJoints[index].isValid)
            return;

        m_SpringJoints[index].springFrequency = frequency;
        m_SpringJoints[index].WakeBodies();
    }
}
