using UnityEngine;

/// <summary>
/// Animate the angular offset of any RelativeJoint2D on the current GameObject.
/// </summary>
public class RelativeJoint2D_AnimateAngularOffset : MonoBehaviour
{
    public float m_RotationSpeed = 90.0f;

    private RelativeJoint2D relativeJoint;

	void Start ()
	{
        relativeJoint = GetComponent<RelativeJoint2D> ();	
	}
	
	void FixedUpdate ()
	{
        // Finish if we have no joint.
        if (relativeJoint == null)
            return;

        // Animate the angular offset.
        relativeJoint.angularOffset = relativeJoint.angularOffset + (m_RotationSpeed * Time.deltaTime);
    }
}
