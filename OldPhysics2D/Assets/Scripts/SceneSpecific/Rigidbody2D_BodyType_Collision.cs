using UnityEngine;

/// <summary>
/// Show a toggle button that controls the 'useFullIinematicContact' state.
/// </summary>
public class Rigidbody2D_BodyType_Collision : MonoBehaviour
{
	public Rigidbody2D[] m_Bodies;
    public GameObject[] m_ExpectationTicks;

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
		var buttonRect = new Rect (20f, 360f, 310f, 30f);
        m_UseFullKinematicContacts = GUI.Toggle(buttonRect, m_UseFullKinematicContacts, "Use Full Kinematic Contacts is " + (m_UseFullKinematicContacts ? "ON" : "OFF"), "button");

		UpdatetFullKinematicContacts ();
    }

	/// <summary>
	/// Update all selected Rigidbody2D 'useFullKinematicContact' state.
	/// </summary>
	private void UpdatetFullKinematicContacts ()
	{
		// Finish if no bodies.
		if (m_Bodies == null || m_ExpectationTicks == null)
			return;

		foreach (var body in m_Bodies)
		{
			if (body)
				body.useFullKinematicContacts = m_UseFullKinematicContacts;
		}

        foreach(var tick in m_ExpectationTicks)
        {
            tick.SetActive(m_UseFullKinematicContacts);
        }
	}
}
