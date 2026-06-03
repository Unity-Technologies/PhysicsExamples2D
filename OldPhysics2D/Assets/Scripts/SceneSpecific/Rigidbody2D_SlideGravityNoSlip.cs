using UnityEngine;

public class Rigidbody2D_SlideGravityNoSlip : MonoBehaviour
{
    public Vector2 Gravity;

    private Rigidbody2D m_Rigidbody;

    private Rigidbody2D.SlideMovement m_SlideMovement = new Rigidbody2D.SlideMovement() { surfaceAnchor = Vector2.zero };

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        m_SlideMovement.gravity = Gravity;

        m_Rigidbody.Slide(Vector2.zero, Time.deltaTime, m_SlideMovement);
    }
}
