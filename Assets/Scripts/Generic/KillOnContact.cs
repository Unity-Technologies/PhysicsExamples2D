using UnityEngine;

/// <summary>
/// Kill any object coming into contact with this object.
/// </summary>
public class KillOnContact : MonoBehaviour
{
    public bool m_KillCollision = true;
    public bool m_KillTrigger = true;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_KillCollision)
            Destroy (collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_KillTrigger)
            Destroy (collision.gameObject);
    }
}
