using UnityEngine;

/// <summary>
/// Show the SliderJoint2D motor speed and resultant Rigidbody2D linear velocity.
/// </summary>
public class SliderJoint2D_MotorSpeed : MonoBehaviour
{
	public GameObject m_Source;

	private TextMesh m_Text;
	private Rigidbody2D m_Body;
	private SliderJoint2D m_SliderJoint;

	void Start ()
	{
		if (!m_Source)
			return;

		m_Text = GetComponent<TextMesh> ();
		m_Body = m_Source.GetComponent<Rigidbody2D> ();
		m_SliderJoint = m_Source.GetComponent<SliderJoint2D> ();
	}
	
	void FixedUpdate()
	{
		if (!m_Text || !m_Body || !m_SliderJoint)
			return;		

		// Set the text to show current values.
		m_Text.text = "Motor Speed = " + Mathf.Abs (m_SliderJoint.motor.motorSpeed).ToString ("n1") + "\n" + "Body Speed = " + (m_Body.velocity.magnitude).ToString ("n1");
	} 
}
