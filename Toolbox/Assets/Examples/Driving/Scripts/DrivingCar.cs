using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Drives the car along the course by setting the motor speed of both its wheel joints, and keeps the camera following it.
/// The car is authored in the scene as a chassis and two wheels, each wheel held to the chassis by a Physics Constraint Wheel that acts as its suspension and its motor.
/// </summary>
/// <remarks>
/// Reverse, forward and brake come from the arrow keys and space bar, or from the matching buttons on the controls menu once they have been assigned.
/// Camera panning and dragging are turned off, since the camera follows the car and would otherwise fight the pointer.
/// </remarks>
public sealed class DrivingCar : MonoBehaviour
{
    /// <summary>
    /// Assigns the controls menu buttons that drive the car alongside the keyboard.
    /// </summary>
    public void SetButtons(ControlsMenu.CustomButton reverseButton, ControlsMenu.CustomButton forwardButton, ControlsMenu.CustomButton brakeButton)
    {
        m_ReverseButton = reverseButton;
        m_ForwardButton = forwardButton;
        m_BrakeButton = brakeButton;
    }

    /// <summary>
    /// How stiff the suspension on both wheels is, in cycles per second.
    /// </summary>
    public float springFrequency
    {
        get => m_SpringFrequency;
        set
        {
            m_SpringFrequency = value;
            UpdateWheels();
        }
    }

    /// <summary>
    /// How quickly the suspension on both wheels stops bouncing, where zero never settles and one settles without overshooting.
    /// </summary>
    public float springDamping
    {
        get => m_SpringDamping;
        set
        {
            m_SpringDamping = value;
            UpdateWheels();
        }
    }

    /// <summary>
    /// How fast the wheels are driven while the car is moving, in degrees per second.
    /// Changing this while the car is moving applies the new speed straight away, in the direction it is already going.
    /// </summary>
    public float motorSpeed
    {
        get => m_MotorSpeed;
        set
        {
            m_MotorSpeed = value;
            SetCarSpeed(m_MotorSpeed * m_Throttle);
        }
    }

    /// <summary>
    /// The most torque each wheel motor can apply, which is also what holds the car still when braking.
    /// </summary>
    public float maxMotorTorque
    {
        get => m_MaxMotorTorque;
        set
        {
            m_MaxMotorTorque = value;
            UpdateWheels();
        }
    }

    private void Start()
    {
        if (m_CameraManipulator != null)
            m_CameraManipulator.DisableManipulators = true;

        UpdateWheels();
    }

    private void Update()
    {
        var currentKeyboard = Keyboard.current;

        // Reverse and forward drive the wheels in opposite directions, and the throttle remembers which so a change of motor speed keeps the car going the same way.
        if (IsPressed(m_ReverseButton) || currentKeyboard.leftArrowKey.isPressed)
        {
            m_Throttle = 1f;
            SetCarSpeed(m_MotorSpeed);
        }

        if (IsPressed(m_ForwardButton) || currentKeyboard.rightArrowKey.isPressed)
        {
            m_Throttle = -1f;
            SetCarSpeed(-m_MotorSpeed);
        }

        // Braking holds the wheels still with the motor rather than letting them roll.
        if (IsPressed(m_BrakeButton) || currentKeyboard.spaceKey.isPressed)
        {
            m_Throttle = 0f;
            SetCarSpeed(0f);
        }

        if (m_CameraManipulator != null && m_Chassis != null)
            m_CameraManipulator.CameraPosition = m_Chassis.body.position;
    }

    // Returns whether a controls menu button is assigned and currently held down.
    private static bool IsPressed(ControlsMenu.CustomButton button) => button != null && button.isPressed;

    // Drives both wheels at the specified speed, which wakes the car so it responds straight away.
    private void SetCarSpeed(float speed)
    {
        foreach (var wheel in Wheels)
        {
            if (wheel == null)
                continue;

            var definition = wheel.definition;
            definition.motorSpeed = speed;
            wheel.definition = definition;

            wheel.ApplyDefinition();
        }
    }

    // Writes the current suspension and motor torque settings onto both wheels, which wakes the car so it responds straight away.
    private void UpdateWheels()
    {
        foreach (var wheel in Wheels)
        {
            if (wheel == null)
                continue;

            var definition = wheel.definition;
            definition.springFrequency = m_SpringFrequency;
            definition.springDamping = m_SpringDamping;
            definition.maxMotorTorque = m_MaxMotorTorque;
            wheel.definition = definition;

            wheel.ApplyDefinition();
        }
    }

    // Both wheel joints, rear first.
    private PhysicsConstraintWheel[] Wheels => new[] { m_RearWheel, m_FrontWheel };

    #region Internal

    [SerializeField] PhysicsPose m_Chassis;
    [SerializeField] PhysicsConstraintWheel m_RearWheel;
    [SerializeField] PhysicsConstraintWheel m_FrontWheel;
    [SerializeField] CameraManipulator m_CameraManipulator;
    [SerializeField, Range(0f, 60f)] float m_SpringFrequency = 5f;
    [SerializeField, Range(0f, 4f)] float m_SpringDamping = 0.7f;
    [SerializeField, Range(-3000f, 3000f)] float m_MotorSpeed = 2000f;
    [SerializeField, Range(0f, 20f)] float m_MaxMotorTorque = 10f;

    ControlsMenu.CustomButton m_ReverseButton;
    ControlsMenu.CustomButton m_ForwardButton;
    ControlsMenu.CustomButton m_BrakeButton;
    float m_Throttle;

    #endregion
}
