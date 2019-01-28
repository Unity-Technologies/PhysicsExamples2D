using UnityEngine;

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
        var clockwise = Input.GetKeyDown(KeyCode.LeftArrow);
        var anticlockwise = Input.GetKeyDown(KeyCode.RightArrow);

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
