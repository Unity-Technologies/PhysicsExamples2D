using UnityEngine;

public class Rigidbody2D_SlideAllBodyTypes : MonoBehaviour
{
    public RigidbodyType2D BodyType = RigidbodyType2D.Dynamic;

    private const float Radius = 2f;

    private Rigidbody2D m_Rigidbody;
    private float m_Time;
    private Rigidbody2D.SlideMovement m_SlideMovement;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();

        m_SlideMovement = new Rigidbody2D.SlideMovement()
        {
            gravity = Vector2.zero,
            surfaceAnchor = Vector2.zero
        };
    }

    void Update()
    {
        m_Time += Time.deltaTime;

        var velocity = new Vector2(Mathf.Cos(m_Time), Mathf.Sin(m_Time)) * Radius;

        m_Rigidbody.Slide(velocity, Time.deltaTime, m_SlideMovement);
    }

    private void OnValidate()
    {
        if (Application.isPlaying && m_Rigidbody)
            m_Rigidbody.bodyType = BodyType;
    }
}
