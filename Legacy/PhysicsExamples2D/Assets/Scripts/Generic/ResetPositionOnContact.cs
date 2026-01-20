using UnityEngine;

/// <summary>
/// Reset the position of any object coming into contact with this object.
/// </summary>
public class ResetPositionOnContact : MonoBehaviour
{
    public bool m_ResetOnCollision = true;
    public bool m_ResetOnTrigger = true;
	public GameObject m_Target;

	private Vector3 m_Position;
	private Quaternion m_Rotation;

	void Start ()
	{
		var target = m_Target ? m_Target : gameObject;
		m_Position = target.transform.position;
		m_Rotation = target.transform.rotation;				
	}

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_ResetOnCollision)
		{
			var target = m_Target ? m_Target : gameObject;
			target.transform.SetPositionAndRotation (m_Position, m_Rotation);
		}
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_ResetOnTrigger)
		{
			var target = m_Target ? m_Target : gameObject;
			target.transform.SetPositionAndRotation (m_Position, m_Rotation);
		}
    }
}
