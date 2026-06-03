using UnityEngine;

/// <summary>
/// Show Angular Velocity of a Rigidbody2D.
/// </summary>
public class ShowAngularVelocity : MonoBehaviour
{
    public Rigidbody2D m_Rigidbody;
	public TextMesh m_Text;

	void Start ()
	{
        if (!m_Rigidbody)
            m_Rigidbody = GetComponent<Rigidbody2D>();

		if (!m_Text)
			m_Text = GetComponent<TextMesh> ();
	}
	
	void FixedUpdate()
	{
		if (!m_Rigidbody || !m_Text)
			return;		

		// Set the text to show current values.
		m_Text.text = "AngVel = " + m_Rigidbody.angularVelocity.ToString("n2");
	} 
}
