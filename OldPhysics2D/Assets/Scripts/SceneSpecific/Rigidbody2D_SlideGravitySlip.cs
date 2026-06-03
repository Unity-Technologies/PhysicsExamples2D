using UnityEngine;

public class Rigidbody2D_SlideGravitySlip : MonoBehaviour
{
    public Vector2 Gravity;
    public float SlipAngle = 30f;

    private Rigidbody2D m_Rigidbody;

    private Rigidbody2D.SlideMovement m_SlideMovement = new Rigidbody2D.SlideMovement() { surfaceAnchor = Vector2.zero };

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        m_SlideMovement.gravity = Gravity;
        m_SlideMovement.gravitySlipAngle = SlipAngle;

        m_Rigidbody.Slide(Vector2.zero, Time.deltaTime, m_SlideMovement);
    }
}
