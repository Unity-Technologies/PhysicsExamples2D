using UnityEngine;

public class Rigidbody2D_SlideSurfaceAnchor : MonoBehaviour
{
    public Vector2 SurfaceAnchor;

    private Rigidbody2D m_Rigidbody;

    private Rigidbody2D.SlideMovement m_SlideMovement = new Rigidbody2D.SlideMovement() { gravity = Vector2.zero };

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        m_SlideMovement.surfaceAnchor = SurfaceAnchor;

        m_Rigidbody.Slide(Vector2.right, Time.deltaTime, m_SlideMovement);
    }
}
