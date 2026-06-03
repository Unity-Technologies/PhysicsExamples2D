using UnityEngine;

/// <summary>
/// Draw a line between the anchor points of the selected anchored joint.
/// </summary>
public class DrawJointAnchorLine : MonoBehaviour
{
	public AnchoredJoint2D m_Joint;
	public Color m_Color = Color.cyan;

	void Update ()
	{
		if (m_Joint)
		{
			// Fetch the joint anchor in world-space.
			var anchor = m_Joint.transform.TransformPoint (m_Joint.anchor);

			// Fetch the connected joint anchor in world-space.
			var connectedAnchor = m_Joint.anchor;
			if (m_Joint.connectedBody)
				connectedAnchor = m_Joint.connectedBody.transform.TransformPoint (connectedAnchor);

			// Draw the line between the anchor and connected anchor positions.
			Debug.DrawLine (anchor, connectedAnchor, m_Color);
		}
	}
}
