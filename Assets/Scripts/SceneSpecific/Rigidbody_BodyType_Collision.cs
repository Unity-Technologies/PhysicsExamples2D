using UnityEngine;

/// <summary>
/// Show a toggle button that controls the 'useFullIinematicContact' state.
/// </summary>
public class Rigidbody_BodyType_Collision : MonoBehaviour
{
	public Rigidbody2D[] m_Bodies;

	private bool m_UseFullKinematicContacts;

	void Start ()
	{
		UpdatetFullKinematicContacts ();
	}

	/// <summary>
	/// Show a toggle button that controls the 'useFullIinematicContact' state.
	/// </summary>
	void OnGUI ()
	{
		var buttonRect = new Rect (20f, Screen.height * 0.5f, Screen.width * 0.2f, 30f);
        m_UseFullKinematicContacts = GUI.Toggle(buttonRect, m_UseFullKinematicContacts, "Use Full Kinematic Contacts is " + (m_UseFullKinematicContacts ? "ON" : "OFF"), "button");

		UpdatetFullKinematicContacts ();
    }

	/// <summary>
	/// Update all selected Rigidbody2D 'useFullKinematicContact' state.
	/// </summary>
	private void UpdatetFullKinematicContacts ()
	{
		if (m_Bodies == null)
			return;

		foreach (var body in m_Bodies)
		{
			if (body)
				body.useFullKinematicContacts = m_UseFullKinematicContacts;
		}
	}
}
