using UnityEngine;

public class Rigidbody2D_AddForceXY : MonoBehaviour
{
    public float forceX;
    public float forceY;

    private Rigidbody2D m_Rigidbody;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        m_Rigidbody.AddForceX(forceX);
        m_Rigidbody.AddForceY(forceY);
    }
}
