using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads twin-stick controller input from the active gamepad.
/// </summary>
static class GamepadHelper
{
    /// <summary>
    /// The left-stick aim direction, normalized, or zero when the stick rests inside the deadzone.
    /// </summary>
    public static Vector2 aim
    {
        get
        {
            var gamepad = Gamepad.current;

            if (gamepad == null)
                return Vector2.zero;

            var stick = gamepad.leftStick.ReadValue();

            return stick.sqrMagnitude > Deadzone * Deadzone ? stick.normalized : Vector2.zero;
        }
    }

    /// <summary>
    /// Whether the right trigger is held.
    /// </summary>
    public static bool fireHeld => Gamepad.current?.rightTrigger.isPressed ?? false;

    // How far the left stick must deflect before it registers as aim.
    const float Deadzone = 0.3f;
}
