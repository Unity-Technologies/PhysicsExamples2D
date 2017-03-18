using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Exposes 2D physics 'OnCollisionXXX2D' and `OnTriggerXXX2D' callbacks as events.
/// </summary>
public class PhysicsEvents : MonoBehaviour
{
    [Serializable]
    public class CollisionEnterEvent : UnityEvent<Collision2D> { }

    [Serializable]
    public class CollisionStayEvent : UnityEvent<Collision2D> { }

    [Serializable]
    public class CollisionExitEvent : UnityEvent<Collision2D> { }

    [Serializable]
    public class TriggerEnterEvent : UnityEvent<Collider2D> { }

    [Serializable]
    public class TriggerStayEvent : UnityEvent<Collider2D> { }

    [Serializable]
    public class TriggerExitEvent : UnityEvent<Collider2D> { }

    [SerializeField]
    private CollisionEnterEvent m_OnCollisionEnter = new CollisionEnterEvent ();

    [SerializeField]
    private CollisionStayEvent m_OnCollisionStay = new CollisionStayEvent ();

    [SerializeField]
    private CollisionExitEvent m_OnCollisionExit = new CollisionExitEvent ();

    [SerializeField]
    private TriggerEnterEvent m_OnTriggerEnter = new TriggerEnterEvent ();

    [SerializeField]
    private TriggerStayEvent m_OnTriggerStay = new TriggerStayEvent ();

    [SerializeField]
    private TriggerExitEvent m_OnTriggerExit = new TriggerExitEvent ();


    public CollisionEnterEvent onCollisionEnterEvent
    {
        get { return m_OnCollisionEnter; }
        set { m_OnCollisionEnter = value; }
    }

    public CollisionStayEvent onCollisionStayEvent
    {
        get { return m_OnCollisionStay; }
        set { m_OnCollisionStay = value; }
    }

    public CollisionExitEvent onCollisionExitEvent
    {
        get { return m_OnCollisionExit; }
        set { m_OnCollisionExit = value; }
    }

    public TriggerEnterEvent onTriggerEnterEvent
    {
        get { return m_OnTriggerEnter; }
        set { m_OnTriggerEnter = value; }
    }

    public TriggerStayEvent onTriggerStayEvent
    {
        get { return m_OnTriggerStay; }
        set { m_OnTriggerStay = value; }
    }

    public TriggerExitEvent onTriggerExitEvent
    {
        get { return m_OnTriggerExit; }
        set { m_OnTriggerExit = value; }
    }

	void OnCollisionEnter2D (Collision2D col)
	{
		m_OnCollisionEnter.Invoke (col);
	}

	void OnCollisionStay2D (Collision2D col)
	{
		m_OnCollisionStay.Invoke (col);
	}

	void OnCollisionExit2D (Collision2D col)
	{
		m_OnCollisionExit.Invoke (col);
	}

	void OnTriggerEnter2D (Collider2D col)
	{
		m_OnTriggerEnter.Invoke (col);
	}

	void OnTriggerStay2D (Collider2D col)
	{	
		m_OnTriggerStay.Invoke (col);
	}

	void OnTriggerExit2D (Collider2D col)
	{
		m_OnTriggerExit.Invoke (col);
	}
}
