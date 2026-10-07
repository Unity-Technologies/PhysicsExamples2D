using System.Collections.Generic;
using Unity.Mathematics;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

/// <summary>
/// A pinball table where a steady stream of small balls drops onto two flippers, with spinners and bumpers to bounce off on the way down.
/// The table, flippers, spinners and bumpers are all authored from Physics Poses, Physics Areas and Physics Constraint Hinges, so this script only works the flippers and manages the balls.
/// </summary>
/// <remarks>
/// Holding the space key or the Flippers button raises both flippers, and letting go lowers them.
/// The flippers are driven by the motors of their hinges, which this script sets every frame.
/// A ball is emitted every few seconds, alternating between the left and right edge, and a ball that reaches the kill zone along the bottom of the pocket is removed and replaced straight away.
/// The balls and flippers use a collision threshold of zero so continuous collision detection is always used and the fast flippers cannot pass through a ball.
/// Each ball is instantiated inactive from a prefab and given its position, velocity and color before it is switched on.
/// </remarks>
public sealed class PinballContents : MonoBehaviour
{
    /// <summary>
    /// Sets the button that raises the flippers while it is held.
    /// </summary>
    /// <param name="flipperButton">The button on the controls menu.</param>
    public void SetButton(ControlsMenu.CustomButton flipperButton) => m_FlipperButton = flipperButton;

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);

        // The first ball is emitted straight away.
        m_SpawnTime = BallInterval;
        m_SpawnLeft = true;
    }

    private void OnDisable()
    {
        // Each ball is a root object of its own, so every one that is left is removed individually.
        foreach (var ball in m_Balls)
        {
            if (ball != null)
                Destroy(ball.gameObject);
        }

        m_Balls.Clear();
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        // Finish if the world is paused.
        if (world.paused)
            return;

        // Raise both flippers while the button or the space key is held, and lower them otherwise.
        var currentKeyboard = Keyboard.current;
        var flippersRaised = (m_FlipperButton != null && m_FlipperButton.isPressed) || (currentKeyboard != null && currentKeyboard.spaceKey.isPressed);
        SetMotorSpeed(m_LeftFlipperHinge, flippersRaised ? FlipperRaiseSpeed : -FlipperLowerSpeed);
        SetMotorSpeed(m_RightFlipperHinge, flippersRaised ? -FlipperRaiseSpeed : FlipperLowerSpeed);

        // Remove any ball that reaches the kill zone along the bottom of the pocket, which is as high as a ball.
        for (var i = m_Balls.Count - 1; i >= 0; --i)
        {
            var ball = m_Balls[i];
            if (ball != null && ball.body.isValid && ball.body.position.y >= DrainHeight)
                continue;

            if (ball != null)
            {
                var target = ball.gameObject;
                target.SetActive(false);
                Destroy(target);
            }

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

    // Sets the speed of a hinge's motor, if the hinge has been created.
    private static void SetMotorSpeed(PhysicsConstraintHinge hinge, float motorSpeed)
    {
        if (hinge == null)
            return;

        var joint = hinge.GetJoint();
        if (joint.isValid)
            joint.motorSpeed = motorSpeed;
    }

    // Emits a ball from alternating sides of the table, moving it a little toward the middle.
    private void SpawnBall()
    {
        if (m_BallPrefab == null)
            return;

        var side = m_SpawnLeft ? -1f : 1f;
        m_SpawnLeft = !m_SpawnLeft;

        var ball = Instantiate(m_BallPrefab, new Vector3(side * BallSpawnX, BallSpawnY, 0f), Quaternion.identity);

        var pose = ball.GetComponent<PhysicsPose>();
        var bodyDefinition = pose.definition;
        bodyDefinition.linearVelocity = new Vector2(-side * BallSpawnSpeed, 0f);
        pose.definition = bodyDefinition;

        var area = ball.GetComponent<PhysicsAreaCircle>();
        var shapeDefinition = area.definition;
        var surfaceMaterial = shapeDefinition.surfaceMaterial;
        surfaceMaterial.customColor = RandomColor();
        shapeDefinition.surfaceMaterial = surfaceMaterial;
        area.definition = shapeDefinition;

        ball.SetActive(true);
        m_Balls.Add(pose);
    }

    // Returns a new random bright color.
    private Color RandomColor() => Color.HSVToRGB(m_Random.NextFloat(0f, 1f), m_Random.NextFloat(0.7f, 1f) * SaturationScale, m_Random.NextFloat(0.5f, 1f));

    #region Internal

    // The motor speeds that raise and lower a flipper, in degrees per second.
    static readonly float FlipperRaiseSpeed = PhysicsMath.ToDegrees(20f);
    static readonly float FlipperLowerSpeed = PhysicsMath.ToDegrees(10f);

    // How often a ball is emitted, how many there can be at once, and where and how fast one enters the table.
    const float BallInterval = 3f;
    const int BallLimit = 10;
    const float BallSpawnX = 7.5f;
    const float BallSpawnY = 18f;
    const float BallSpawnSpeed = 2f;

    // A ball is removed once its center is lower than this, which is the height of a ball above the floor of the pocket.
    const float DrainHeight = -2f + 0.4f * 2f;

    // How washed out the random colors are, and the seed they start from.
    const float SaturationScale = 0.65f;
    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_BallPrefab;
    [SerializeField] PhysicsConstraintHinge m_LeftFlipperHinge;
    [SerializeField] PhysicsConstraintHinge m_RightFlipperHinge;

    readonly List<PhysicsPose> m_Balls = new();
    ControlsMenu.CustomButton m_FlipperButton;
    Random m_Random;
    float m_SpawnTime;
    bool m_SpawnLeft;

    #endregion
}
