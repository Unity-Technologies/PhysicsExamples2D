using UnityEngine;

public class Rigidbody2D_SlideMoveSlideAngle : MonoBehaviour
{
    [Range(0f, 90f)]
    public float SlideAngle = 90f;

    [Range(0f, 50f)]
    public float Speed = 3f;

    private Rigidbody2D m_Rigidbody;

    private Rigidbody2D.SlideMovement m_SlideMovement = new Rigidbody2D.SlideMovement();

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        m_SlideMovement.surfaceSlideAngle = SlideAngle;
        m_Rigidbody.Slide(Vector2.right * Speed, Time.deltaTime, m_SlideMovement);
    }
}
