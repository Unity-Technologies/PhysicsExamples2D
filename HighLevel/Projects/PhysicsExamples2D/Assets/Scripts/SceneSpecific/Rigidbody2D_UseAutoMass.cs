using UnityEngine;

/// <summary>
/// Show the Rigidbody2D mass when using Rigidbody2D.useAutoMass.
/// </summary>
public class Rigidbody2D_UseAutoMass : MonoBehaviour
{
	private TextMesh m_Text;
	private Rigidbody2D m_Body;
	private Collider2D m_Collider;

	void Start ()
	{
		// Fetch the text mesh.
		m_Text = GetComponent<TextMesh> ();
		if (m_Text)
		{
			// Fetch the Collider2D.
			m_Collider = GetComponentInParent<Collider2D> ();
			if (m_Collider)
			{
				// Fetch the attached Rigidbody2D.
				m_Body = m_Collider.attachedRigidbody;
				if (m_Body)
				{
					// Ensure the Rigidbody2D is Dynamic.
					if (m_Body.bodyType == RigidbodyType2D.Dynamic)
					{
						// Ensure the Rigidbody2D.useAutoMass is true.
						if (m_Body.useAutoMass)
						{
							// Ensure only a single Collider is attached to the Rigidbody2D.
							if (m_Body.attachedColliderCount == 1)
							{
								CalculateExpectedMass ();
								return;
							}
							else
							{
								Debug.Log ("Rigidbody2D must have a single attached Collider.", this);
							}
						}
						else
						{
							Debug.Log ("Rigidbody2D.useAutoMass must be true!", this);
						}
					}
					else
					{
						Debug.Log ("Rigidbody2D.bodyType must be Dynamic!", this);
					}
				}
				else
				{
					Debug.Log ("No Rigidbody2D was found!", this);
				}
			}
			else
			{
				Debug.Log ("No Collider2D was found!", this);
			}
		}
		else
		{
				Debug.Log ("No TextMesh was found!", this);
		}

		// Reset references if we failed to find what we require.
		m_Body = null;
		m_Collider = null;
	}
	
	private void CalculateExpectedMass()
	{
		// Finish if no Rigidbody2D, Collider2D or TextMesh.
		if (!m_Body || !m_Collider || !m_Text)
			return;

		// Calculate the expected area for each collider type.
		float expectedColliderArea = 0f;
		if (m_Collider is BoxCollider2D)
		{
			var collider = m_Collider as BoxCollider2D;
			var colliderSize = collider.size;
			expectedColliderArea = colliderSize.x * colliderSize.y;
		}
		else if (m_Collider is PolygonCollider2D)
		{
			var collider = m_Collider as PolygonCollider2D;

			// Ensure we have a single path.
			if (collider.pathCount != 1)
			{
				Debug.Log ("PolygonCollider2D must have a single convex path!");
				return;
			}

			// Fetch the collider path.
			var colliderPath = collider.GetPath (0);

			// Ensure the path defines at least a triangle.
			// NOTE: We do not supported calculating the area of a convex path here but we do not check.
			if (colliderPath.Length < 3)
			{
				Debug.Log ("PolygonCollider2D path must define at least a triangle.  Note that non-convex shapes are not supported.");
				return;
			}

			// Calculate the centroid.
			var centroid = Vector2.zero;
			for (var n = 0; n < colliderPath.Length; ++n)
			{
				centroid += colliderPath[n];
			}
			centroid *= 1.0f / colliderPath.Length;
			
			// Calculate the collider area.
			for (var n = 0; n < colliderPath.Length; ++n)
			{
				var v1 = colliderPath[n] - centroid;
				var v2 = colliderPath[n + 1 < colliderPath.Length ? n + 1 : 0] - centroid;
				expectedColliderArea += 0.5f * (v1.x * v2.y - v1.y * v2.x);				
			}
			expectedColliderArea = Mathf.Abs (expectedColliderArea);
		}
		else if (m_Collider is CircleCollider2D)
		{
			var collider = m_Collider as CircleCollider2D;
			expectedColliderArea = Mathf.PI * collider.radius * collider.radius;
		}
		else if (m_Collider is CapsuleCollider2D)
		{
			var collider = m_Collider as CapsuleCollider2D;
			var colliderSize = collider.size;
			var radius = colliderSize.x * 0.5f;
			expectedColliderArea = ((colliderSize.y - colliderSize.x) * colliderSize.x) + (Mathf.PI * radius * radius);
		}
		else if (m_Collider is EdgeCollider2D)
		{
			var collider = m_Collider as EdgeCollider2D;
			expectedColliderArea = 0f;
		}
		else
		{
			Debug.Log ("Collider type " + m_Collider.GetType () + " not supported by script!", this);
			return;
		}

		// Calculate the expected mass.
		// NOTE: When an area of zero or auto-mass of zero is encountered then a mass of 1 is automatically used.
		// A non-zero mass is effectively an infinite mass and is only used internally on Static and Kinematic bodies, not Dynamic ones.
		var expectedMass = expectedColliderArea < Mathf.Epsilon ? 1.0f : expectedColliderArea * m_Collider.density;

		// Fetch the actual mass.
		var actualMass = m_Body.mass;

		// Set the text to show current values.
		m_Text.text = m_Collider.GetType ().Name + "\n\nCollider Density = " + m_Collider.density.ToString ("n4") + "\nCollider Area = " + expectedColliderArea.ToString ("n4") + "\n\nExpected Mass = " + expectedMass.ToString ("n4") + "\n" + "Actual Mass = " + actualMass.ToString ("n4");
	} 
}
