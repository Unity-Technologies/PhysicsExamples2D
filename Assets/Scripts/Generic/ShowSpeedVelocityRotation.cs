using UnityEngine;

/// <summary>
/// Optionally show the Speed, Velocity & Rotation of a Rigidbody2D.
/// </summary>
public class ShowSpeedVelocityRotation : MonoBehaviour
{
	public bool m_ShowSpeed;
	public bool m_ShowVelocity;
	public bool m_ShowRotation;

	public Rigidbody2D m_Body;
	public TextMesh m_Text;

	void Start ()
	{
		if (!m_Body)
			m_Body = GetComponentInParent<Rigidbody2D> ();

		if (!m_Text)
			m_Text = GetComponent<TextMesh> ();
	}
	
	void FixedUpdate()
	{
		if (!m_Text || !m_Body)
			return;		


		string text = string.Empty;
		if (m_ShowSpeed)
			text += "Speed = " + m_Body.velocity.magnitude.ToString ("n2") + "\n";

		if (m_ShowVelocity)
			text += "LinVel = " + m_Body.velocity.ToString ("n2") + "\n";

		if (m_ShowRotation)
			text += "AngVel = " + m_Body.angularVelocity.ToString ("n2");

		// Set the text to show current values.
		m_Text.text = text;
	} 
}
