using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A Strandbeest, Theo Jansen's walking machine, where a motor turns a wheel that steps six linkage legs across a field of Strandpebbles.
/// The ground, Strandpebbles, chassis, wheel and legs are authored from Physics Poses and Physics Areas, and the motor, the leg pivots and the soft leg springs are Physics Constraints, so this script only drives the motor and moves the camera.
/// </summary>
/// <remarks>
/// The wheel, chassis and legs share a negative contact group so they never touch each other, only the ground and the Strandpebbles.
/// Each leg is two triangles joined by four soft distance constraints, which reduce jitter and act like a suspension, and the lower triangle pivots on the chassis.
/// The walk left and walk right buttons or arrow keys turn the motor each way, the stop button or space key stops it, and the motor keeps turning the way it was last told to.
/// The camera follows the machine along the ground, and any attempt to pan it by hand is undone, so the machine can still be dragged.
/// </remarks>
public sealed class StrandbeestContents : MonoBehaviour
{
    /// <summary>
    /// Sets the buttons that turn the motor each way and stop it.
    /// </summary>
    /// <param name="leftButton">The button that turns the motor to the left.</param>
    /// <param name="rightButton">The button that turns the motor to the right.</param>
    /// <param name="brakeButton">The button that stops the motor.</param>
    public void SetButtons(ControlsMenu.CustomButton leftButton, ControlsMenu.CustomButton rightButton, ControlsMenu.CustomButton brakeButton)
    {
        m_LeftButton = leftButton;
        m_RightButton = rightButton;
        m_BrakeButton = brakeButton;
    }

    /// <summary>
    /// How fast the motor turns the wheel, in radians per second.
    /// </summary>
    public float motorSpeed
    {
        get => m_MotorSpeed;
        set
        {
            m_MotorSpeed = value;
            ApplyMotorSpeed();
        }
    }

    /// <summary>
    /// The most torque the motor can use to turn the wheel.
    /// </summary>
    public float motorTorque
    {
        get => m_MotorTorque;
        set
        {
            m_MotorTorque = value;

            var joint = m_MotorHinge != null ? m_MotorHinge.GetJoint() : default;
            if (!joint.isValid)
                return;

            joint.maxMotorTorque = m_MotorTorque;
            joint.WakeBodies();
        }
    }

    // The machine starts turning to the right, and its constraints are created as the scene loads, so the motor is given its settings once they exist.
    private void Start()
    {
        m_Direction = 1;

        var joint = m_MotorHinge != null ? m_MotorHinge.GetJoint() : default;
        if (joint.isValid)
            joint.maxMotorTorque = m_MotorTorque;

        ApplyMotorSpeed();
    }

    private void Update()
    {
        // The camera follows the machine along the ground, whether or not the world is paused, which also puts the camera back if it is panned by hand.
        if (m_CameraManipulator != null && m_Chassis != null && m_Chassis.body.isValid)
            m_CameraManipulator.CameraPosition = new Vector2(m_Chassis.body.position.x, CameraHeight);

        // Finish if the world is paused.
        if (PhysicsWorld.defaultWorld.paused)
            return;

        var currentKeyboard = Keyboard.current;
        var leftPressed = IsPressed(m_LeftButton) || (currentKeyboard != null && currentKeyboard.leftArrowKey.isPressed);
        var rightPressed = IsPressed(m_RightButton) || (currentKeyboard != null && currentKeyboard.rightArrowKey.isPressed);
        var brakePressed = IsPressed(m_BrakeButton) || (currentKeyboard != null && currentKeyboard.spaceKey.isPressed);

        if (leftPressed)
            SetDirection(-1);

        if (rightPressed)
            SetDirection(1);

        if (brakePressed)
            SetDirection(0);
    }

    // Sets which way the motor turns, with zero meaning the brake.
    private void SetDirection(int direction)
    {
        m_Direction = direction;
        ApplyMotorSpeed();
    }

    // Gives the motor the chosen speed in the current direction, converting the speed in radians per second to the degrees per second the joint uses.
    private void ApplyMotorSpeed()
    {
        var joint = m_MotorHinge != null ? m_MotorHinge.GetJoint() : default;
        if (!joint.isValid)
            return;

        joint.motorSpeed = m_Direction * PhysicsMath.ToDegrees(m_MotorSpeed);
        joint.WakeBodies();
    }

    private static bool IsPressed(ControlsMenu.CustomButton button) => button != null && button.isPressed;

    #region Internal

    // The height the camera stays at while it follows the machine.
    const float CameraHeight = 6f;

    [SerializeField] PhysicsConstraintHinge m_MotorHinge;
    [SerializeField] PhysicsPose m_Chassis;
    [SerializeField] CameraManipulator m_CameraManipulator;
    [SerializeField, Range(0f, 10f)] float m_MotorSpeed = 3f;
    [SerializeField, Range(0f, 2000f)] float m_MotorTorque = 1000f;

    ControlsMenu.CustomButton m_LeftButton;
    ControlsMenu.CustomButton m_RightButton;
    ControlsMenu.CustomButton m_BrakeButton;
    int m_Direction = 1;

    #endregion
}
