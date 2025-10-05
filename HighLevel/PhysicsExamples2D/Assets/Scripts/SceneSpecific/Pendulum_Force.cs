using UnityEngine;
using UnityEngine.InputSystem;

public class Pendulum_Force : MonoBehaviour
{
    public float TangentForce = 1.0f;
    public Rigidbody2D PivotBody;
    private Rigidbody2D BallBody;

    void Start()
    {
        BallBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        var currentKeyboard = Keyboard.current;

        var clockwise = currentKeyboard.leftArrowKey.wasPressedThisFrame;
        var anticlockwise = currentKeyboard.rightArrowKey.wasPressedThisFrame;

        var constraint = (PivotBody.position - BallBody.position).normalized;

        if (clockwise)
        {
            var force = new Vector2(-constraint.y, constraint.x) * TangentForce;
            BallBody.AddForce(force, ForceMode2D.Impulse);
        }

        if (anticlockwise)
        {
            var force = new Vector2(constraint.y, -constraint.x) * TangentForce;
            BallBody.AddForce(force, ForceMode2D.Impulse);
        }
    }
}
