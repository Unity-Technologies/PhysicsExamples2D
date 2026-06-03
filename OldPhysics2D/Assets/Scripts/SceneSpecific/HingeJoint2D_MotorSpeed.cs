using UnityEngine;

/// <summary>
/// Show the HingeJoint2D motor speed and resultant Rigidbody2D angular velocity.
/// </summary>
public class HingeJoint2D_MotorSpeed : MonoBehaviour
{
	public GameObject m_Source;

	private TextMesh m_Text;
	private Rigidbody2D m_Body;
	private HingeJoint2D m_HingeJoint;

	void Start ()
	{
		if (!m_Source)
			return;

		m_Text = GetComponent<TextMesh> ();
		m_Body = m_Source.GetComponent<Rigidbody2D> ();
		m_HingeJoint = m_Source.GetComponent<HingeJoint2D> ();
	}
	
	void FixedUpdate()
	{
		if (!m_Text || !m_Body || !m_HingeJoint)
			return;		

		// Set the text to show current values.
		// Note we show the negative motor speed due the fact that the motor drives in a specific rotation resulting in the body going in the opposite.
		m_Text.text = "Motor Speed = " + m_HingeJoint.motor.motorSpeed.ToString ("n1") + "\n" + "Body Speed = " + (-m_Body.angularVelocity).ToString ("n1");
	} 
}
