using UnityEngine;

/// <summary>
/// Show Density of a Collider2D.
/// </summary>
public class ShowDensity : MonoBehaviour
{
    public Collider2D m_Collider;
	public TextMesh m_Text;

	void Start ()
	{
		if (!m_Collider)
			m_Collider = GetComponentInParent<Collider2D> ();

		if (!m_Text)
			m_Text = GetComponent<TextMesh> ();
	}
	
	void FixedUpdate()
	{
		if (!m_Text || !m_Collider)
			return;		

		// Set the text to show current values.
		m_Text.text = "Density = " + m_Collider.density.ToString("n2");
	} 
}
