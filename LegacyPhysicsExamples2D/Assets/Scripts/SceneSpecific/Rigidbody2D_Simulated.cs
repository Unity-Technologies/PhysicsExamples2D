using UnityEngine;

/// <summary>
/// Show a toggle button that controls the 'Rigidbody2D.simulated' state.
/// </summary>
public class Rigidbody2D_Simulated : MonoBehaviour
{
	private Rigidbody2D m_Body;

	void Start ()
	{
		m_Body = GetComponent<Rigidbody2D> ();
	}

	/// <summary>
	/// Show a toggle button that controls the 'Rigidbody2D.simulated' state.
	/// </summary>
	void OnGUI ()
	{
		// Finish if no body.
		if (m_Body == null)
			return;

		var simulated = m_Body.simulated;

		// Show simulated toggle.
		var buttonRect = new Rect (20f, 50f, 310f, 30f);
        simulated = GUI.Toggle(buttonRect, simulated, "Rigidbody2D.simulated is " + (simulated ? "ON" : "OFF"), "button");

		// Set the simulated state if it has changed.
		if (m_Body.simulated != simulated)
			m_Body.simulated = simulated;
    }
}
