using UnityEngine;

/// <summary>
/// Animate the linear offset of any RelativeJoint2D on the current GameObject.
/// </summary>
public class RelativeJoint2D_AnimateLinearOffset : MonoBehaviour
{
    public float m_AnimateSpeed = 1f;
    public float m_LinearAmplitude = 2f;
    public float m_Direction = 0.0f;

    private RelativeJoint2D relativeJoint;
    private float time = 0f;

	void Start ()
	{
        relativeJoint = GetComponent<RelativeJoint2D> ();	
	}
	
	void FixedUpdate ()
	{
        // Finish if we have no joint.
        if (relativeJoint == null)
            return;

        // Calculate linear offset.
        var direction = m_Direction * Mathf.Deg2Rad;
        var amplitude = Mathf.PingPong (Mathf.Sin (time) * m_LinearAmplitude, m_LinearAmplitude);

        // Animate the angular offset.
        relativeJoint.linearOffset = new Vector2 (Mathf.Cos (direction) * amplitude, Mathf.Sin (direction) * amplitude);

        // Increase time.
        time += (m_AnimateSpeed / m_LinearAmplitude) * Time.deltaTime;
    }

}
