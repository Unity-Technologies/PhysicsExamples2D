using UnityEngine;

public class Miscellaneous_QuadraticDrag : MonoBehaviour
{
    public float DragCoefficient = 0.5f;

    private Rigidbody2D Body;

    void Start()
    {
        Body = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Fetch the body velocity.
        var velocity = Body.velocity;

        // Calculate the body speed.
        var speed = velocity.magnitude;
        if (speed > 0f)
        {
            // Calculate and apply the quadratic drag force (speed squared).
            var dragForce = (DragCoefficient * speed * speed) * -velocity.normalized;
            Body.AddForce(dragForce, ForceMode2D.Force);
        }
    }
}
