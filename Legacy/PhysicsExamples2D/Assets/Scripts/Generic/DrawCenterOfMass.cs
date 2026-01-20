using UnityEngine;

/// <summary>
/// Draw the center-of-mass for an attached Rigidbody2D as a coloured cross.
/// </summary>
[ExecuteInEditMode]
public class DrawCenterOfMass : MonoBehaviour
{
	public Color m_Color = Color.cyan;
	public float m_Radius = 0.25f;

	private Rigidbody2D m_Body;

	void Start ()
	{
		m_Body = GetComponent<Rigidbody2D> ();
	}
	
	void Update ()
	{
		if (m_Body)
		{
			var centerOfMass = (Vector3)m_Body.worldCenterOfMass;

			// Draw the center of mass as a cross.
			Debug.DrawLine (centerOfMass + (Vector3.left * m_Radius), centerOfMass + (Vector3.right * m_Radius), m_Color);
			Debug.DrawLine (centerOfMass + (Vector3.up * m_Radius), centerOfMass + (Vector3.down * m_Radius), m_Color);
		}
	}}
