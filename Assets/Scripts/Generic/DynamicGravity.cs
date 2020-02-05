using UnityEngine;

public class DynamicGravity : MonoBehaviour
{
    public float AgainstGravityScale = 1f;
    public float FallGravityScale = 2f;

    private Rigidbody2D m_Rigidbody;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Calculate our direction relative to the global gravity.
        var direction = Vector2.Dot(m_Rigidbody.velocity, Physics2D.gravity);
        
        // Set the gravity scale accordingly.
        m_Rigidbody.gravityScale = direction > 0f ? FallGravityScale : AgainstGravityScale;
    }
}
