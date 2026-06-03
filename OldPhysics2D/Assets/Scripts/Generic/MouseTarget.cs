using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Set the TargetJoint2D to follow the mouse position.
/// </summary>
public class MouseTarget : MonoBehaviour
{
	TargetJoint2D targetJoint;

	void Start()
	{
		// Fetch the target joint.
		targetJoint = GetComponent<TargetJoint2D> ();

		// Finish if no joint detected.
		if (targetJoint == null)
			return;
	} 

	void FixedUpdate ()
	{
		// Finish if no joint detected.
		if (targetJoint == null)
			return;

		// Calculate the world position for the mouse.
		var worldPos = Camera.main.ScreenToWorldPoint (Mouse.current.position.value);

		// Set the joint target.
		targetJoint.target = worldPos;
	}
}
