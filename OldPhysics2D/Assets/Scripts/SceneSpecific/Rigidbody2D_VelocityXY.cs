using UnityEngine;

public class Rigidbody2D_VelocityXY : MonoBehaviour
{
    public float velocityX;
    public float velocityY;

    private Rigidbody2D m_Rigidbody;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        m_Rigidbody.linearVelocityX = velocityX;
        m_Rigidbody.linearVelocityY = velocityY;
    }
}
