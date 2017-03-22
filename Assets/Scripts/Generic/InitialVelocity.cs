using UnityEngine;

/// <summary>
/// Set the initial velocity of a Rigidbody2D.
/// </summary>
public class InitialVelocity : MonoBehaviour
{
    public Vector2 m_Velocity;
    public float m_AngularVelocity;

	void Start ()
    {
        var rigidbody = GetComponent<Rigidbody2D> ();

        // Set the initial velocities if the rigidbody was found.
        if (rigidbody)
        {
            rigidbody.velocity = m_Velocity;
            rigidbody.angularVelocity = m_AngularVelocity;
        }
	}
}
