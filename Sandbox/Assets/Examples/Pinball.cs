using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Collision", "A pinball table showing small fast balls using continuous collision against moving flippers.")]
public sealed class Pinball : SandboxExampleBehaviour
{
    private const float FlipperHalfLength = 1.55f;
    private const float FlipperThickness = 0.4f;
    private const float FlipperRadius = 0.1f;
    private const float FlipperPivotX = 3.6f;
    private const float FlipperPivotY = 1.6f;
    private const float TableBottom = -2f;
    private const float FlipperMaxTorque = 1000f;

    private const float BallRadius = 0.4f;
    private const float BallInterval = 3f;
    private const int BallLimit = 10;
    private const float BallSpawnX = 7.5f;
    private const float BallSpawnY = 18f;
    private const float BallSpawnSpeed = 2f;
    private const float DrainHeight = TableBottom + BallRadius * 2f;

    private static readonly float FlipperRaiseSpeed = PhysicsMath.ToDegrees(20f);
    private static readonly float FlipperLowerSpeed = PhysicsMath.ToDegrees(10f);

    private float m_SpawnTime;
    private bool m_SpawnLeft;

    private ControlsMenu.CustomButton m_FlipperButton;
    private PhysicsHingeJoint m_LeftFlipperHinge;
    private PhysicsHingeJoint m_RightFlipperHinge;
    private readonly List<PhysicsBody> m_Balls = new();

    protected override float CameraSize => 13f;
    protected override Vector2 CameraPosition => new(0f, 9f);

    protected override void OnExampleEnable()
    {
        // Set controls.
        m_FlipperButton = SandboxManager.ControlsMenu[0];
        m_FlipperButton.Set("Flippers [Spc]");
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // The first ball is emitted straight away.
        m_Balls.Clear();
        m_SpawnTime = BallInterval;
        m_SpawnLeft = true;

        var groundBody = world.CreateBody();

        // Table.
        {
            // The diagonal walls run down to the flipper pivots, then drop straight down to a rectangular pocket that collects the balls that fall between the flippers.
            var vertices = new NativeArray<Vector2>(8, Allocator.Temp);
            vertices[0] = new Vector2(-8f, 6f);
            vertices[1] = new Vector2(-8f, 20f);
            vertices[2] = new Vector2(8f, 20f);
            vertices[3] = new Vector2(8f, 6f);
            vertices[4] = new Vector2(FlipperPivotX, FlipperPivotY);
            vertices[5] = new Vector2(FlipperPivotX, TableBottom);
            vertices[6] = new Vector2(-FlipperPivotX, TableBottom);
            vertices[7] = new Vector2(-FlipperPivotX, FlipperPivotY);

            var chainDef = new PhysicsChainDefinition { isLoop = true };
            groundBody.CreateChain(new ChainGeometry(vertices), chainDef);
            vertices.Dispose();
        }

        // Flippers.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, sleepingAllowed = false, collisionThreshold = 0f };
            var shapeDef = new PhysicsShapeDefinition();
            var size = new Vector2(FlipperHalfLength * 2f, FlipperThickness);

            // Each flipper pivots about its outer end, so the box is offset along the flipper from the body origin, and the rounding is kept inside the box size.
            var leftPosition = new Vector2(-FlipperPivotX, FlipperPivotY);
            bodyDef.position = leftPosition;
            var leftFlipperBody = world.CreateBody(bodyDef);
            leftFlipperBody.CreateShape(PolygonGeometry.CreateBox(size, FlipperRadius, new PhysicsTransform(new Vector2(FlipperHalfLength, 0f), PhysicsRotate.identity), inscribe: true), shapeDef);

            var rightPosition = new Vector2(FlipperPivotX, FlipperPivotY);
            bodyDef.position = rightPosition;
            var rightFlipperBody = world.CreateBody(bodyDef);
            rightFlipperBody.CreateShape(PolygonGeometry.CreateBox(size, FlipperRadius, new PhysicsTransform(new Vector2(-FlipperHalfLength, 0f), PhysicsRotate.identity), inscribe: true), shapeDef);

            var hingeDef = new PhysicsHingeJointDefinition
            {
                bodyA = groundBody,
                enableMotor = true,
                maxMotorTorque = FlipperMaxTorque,
                enableLimit = true
            };

            hingeDef.bodyB = leftFlipperBody;
            hingeDef.localAnchorA = leftPosition;
            hingeDef.localAnchorB = Vector2.zero;
            hingeDef.motorSpeed = -FlipperLowerSpeed;
            hingeDef.lowerAngleLimit = -30f;
            hingeDef.upperAngleLimit = 5f;
            m_LeftFlipperHinge = world.CreateJoint(hingeDef);

            hingeDef.bodyB = rightFlipperBody;
            hingeDef.localAnchorA = rightPosition;
            hingeDef.motorSpeed = FlipperLowerSpeed;
            hingeDef.lowerAngleLimit = -5f;
            hingeDef.upperAngleLimit = 30f;
            m_RightFlipperHinge = world.CreateJoint(hingeDef);
        }

        // Spinners.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition();
            var box1 = PolygonGeometry.CreateBox(new Vector2(3f, 0.25f));
            var box2 = PolygonGeometry.CreateBox(new Vector2(0.25f, 3f));

            // The motor torque is tiny and the speed zero, so it only acts as friction that slows the spinner.
            var hingeDef = new PhysicsHingeJointDefinition
            {
                bodyA = groundBody,
                localAnchorB = Vector2.zero,
                enableMotor = true,
                maxMotorTorque = 0.1f
            };

            bodyDef.position = new Vector2(-4f, 17f);
            var spinnerBody = world.CreateBody(bodyDef);
            spinnerBody.CreateShape(box1, shapeDef);
            spinnerBody.CreateShape(box2, shapeDef);
            hingeDef.bodyB = spinnerBody;
            hingeDef.localAnchorA = bodyDef.position;
            world.CreateJoint(hingeDef);

            bodyDef.position = new Vector2(4f, 8f);
            spinnerBody = world.CreateBody(bodyDef);
            spinnerBody.CreateShape(box1, shapeDef);
            spinnerBody.CreateShape(box2, shapeDef);
            hingeDef.bodyB = spinnerBody;
            hingeDef.localAnchorA = bodyDef.position;
            world.CreateJoint(hingeDef);
        }

        // Bumpers.
        {
            var shapeDef = new PhysicsShapeDefinition();
            shapeDef.surfaceMaterial.bounciness = 1.5f;

            var circle = new CircleGeometry { radius = 1f };

            var bumperBody = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(-4f, 8f) });
            bumperBody.CreateShape(circle, shapeDef);

            bumperBody = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(4f, 17f) });
            bumperBody.CreateShape(circle, shapeDef);
        }
    }

    private void Update()
    {
        // Finish if the world is paused.
        if (SandboxManager.WorldPaused)
            return;

        // Raise both flippers while the button or the space key is held, and lower them otherwise.
        var currentKeyboard = Keyboard.current;
        var flippersRaised = m_FlipperButton.isPressed || (currentKeyboard != null && currentKeyboard.spaceKey.isPressed);
        m_LeftFlipperHinge.motorSpeed = flippersRaised ? FlipperRaiseSpeed : -FlipperLowerSpeed;
        m_RightFlipperHinge.motorSpeed = flippersRaised ? -FlipperRaiseSpeed : FlipperLowerSpeed;

        // Remove any ball that reaches the kill zone along the bottom of the pocket, which is as high as a ball.
        for (var i = m_Balls.Count - 1; i >= 0; --i)
        {
            var ball = m_Balls[i];
            if (ball.isValid && ball.position.y >= DrainHeight)
                continue;

            if (ball.isValid)
                ball.Destroy();

            m_Balls.RemoveAt(i);

            // A ball that reaches the pocket is replaced straight away.
            m_SpawnTime = BallInterval;
        }

        // Emit a ball every interval, or straight away after one is removed, while there are fewer balls than the limit.
        m_SpawnTime += Time.deltaTime;
        if (m_SpawnTime >= BallInterval && m_Balls.Count < BallLimit)
        {
            m_SpawnTime = 0f;
            SpawnBall();
        }
    }

    // Emits a ball from alternating sides of the table, moving it a little toward the middle.
    private void SpawnBall()
    {
        var side = m_SpawnLeft ? -1f : 1f;
        m_SpawnLeft = !m_SpawnLeft;

        // The ball is small and the flippers are thin and fast, so a collision threshold of zero makes it use continuous collision detection all the time.
        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            position = new Vector2(side * BallSpawnX, BallSpawnY),
            linearVelocity = new Vector2(-side * BallSpawnSpeed, 0f),
            collisionThreshold = 0f
        };

        var shapeDef = new PhysicsShapeDefinition();
        shapeDef.surfaceMaterial.customColor = ShapeColor;

        var ballBody = World.CreateBody(bodyDef);
        ballBody.CreateShape(new CircleGeometry { radius = BallRadius }, shapeDef);
        m_Balls.Add(ballBody);
    }
}
