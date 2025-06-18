using System.Collections;
using UnityEngine;

/// <summary>
/// Spawn objects into the scene.
/// </summary>
public class Spawner : MonoBehaviour
{
	[Range (0.0f, 10.0f)]
	public float m_SpawnTime = 1f;

	[Range (1, 100)]
	public int m_SpawnCount = 1;

	[Range (0, 1000)]
	public int m_SpawnMaximum = 10;

    [Range (0, 100)]
    public float m_SpawnLifetime = 0.0f;

	public Transform m_SpawnParent;

	public Vector2 m_Area;
   	public Color m_AreaColor = Color.cyan;

	[Range (0.1f, 10.0f)]
	public float m_MinScale = 1.0f;
	[Range (0.1f, 10.0f)]
	public float m_MaxScale = 1.0f;

	[Range (0f, 100f)]
	public float m_GravityScale = 1.0f;

	[Range (0, 360)]
	public float m_RandomRotation = 0.0f;

	[Range (0, 31)]
	public int m_Layer;

    public bool m_UseAutoMass;

	public bool m_UseRandomColor;

	public PhysicsMaterial2D m_PhysicsMaterial;

	public GameObject[] m_SpawnItems;

	private int m_SpawnTotal;

	/// <summary>
	/// Infinitely spawn items until maximum allowed has been reached.
	/// </summary>
	/// <returns></returns>
	public IEnumerator Start ()
	{
		if (m_SpawnItems != null && m_SpawnItems.Length > 0)
		{
			while (true)
			{
				Spawn ();
				yield return new WaitForSeconds (m_SpawnTime);
			}
		}
	}

	/// <summary>
	/// Spawn the current items for this period.
	/// </summary>
	private void Spawn ()
	{
		for (int n = 0; n < m_SpawnCount; ++n)
		{
			// Finish if we've reached our spawn maximum.
			// NOTE: Setting a spawn maximum of zero turns the limit off.
			if (m_SpawnMaximum > 0 && m_SpawnTotal >= m_SpawnMaximum)
				return;

			// Choose a spawn item.
			var obj = m_SpawnItems[Random.Range (0, m_SpawnItems.Length)];
			if (obj)
			{
				var spawnRange = m_Area * 0.5f;
				var position = transform.TransformPoint (new Vector3 (Random.Range (-spawnRange.x, spawnRange.x), Random.Range (-spawnRange.y, spawnRange.y), transform.position.z));
				var rotation = Random.Range (0.0f, m_RandomRotation);

				// Create the spawn object at the random position & rotation.
				var spawnObj = Instantiate (obj, position, Quaternion.Euler (0f, 0f, rotation), m_SpawnParent ? m_SpawnParent : transform);

				// Set its random scale.
				var randomScale = Random.Range (m_MinScale, m_MaxScale);
				spawnObj.transform.localScale = new Vector3 (randomScale, randomScale);

				// Set its layer.
				spawnObj.layer = m_Layer;

				// Set a random sprite renderer color if required.
				if (m_UseRandomColor)
				{
					var spriteRenderer = spawnObj.GetComponentInChildren<SpriteRenderer> ();
					if (spriteRenderer)
						spriteRenderer.color = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f), 1.0f);
				}

				// Set the Rigidbody2D configuration if it's available.
                var body = spawnObj.GetComponent<Rigidbody2D> ();
				if (body)
				{
					body.gravityScale = m_GravityScale;
					body.sharedMaterial = m_PhysicsMaterial;
                    body.useAutoMass = m_UseAutoMass;
				}

				// Enable the object.
				spawnObj.SetActive(true);

				// Set the spawn lifetime if required.
                if (m_SpawnLifetime > 0.0f)
					StartCoroutine (HandleObjectLifetime (spawnObj, m_SpawnLifetime));

				// Increase our spawn count.
				++m_SpawnTotal;
			}
		}

        // Draw the spawn area.
        DrawSpawnArea ();
	}

    private void DrawSpawnArea()
    {
        // Calculate the spawn area.
        var spawnRange = m_Area * 0.5f;
        var position = transform.position;
        var vertex0 = transform.TransformPoint (new Vector3 (position.x - spawnRange.x, position.y - spawnRange.y));
        var vertex1 = transform.TransformPoint (new Vector3 (position.x + spawnRange.x, position.y - spawnRange.y));
        var vertex2 = transform.TransformPoint (new Vector3 (position.x + spawnRange.x, position.y + spawnRange.y));
        var vertex3 = transform.TransformPoint (new Vector3 (position.x - spawnRange.x, position.y + spawnRange.y));

        // Draw the spawn area.
        Debug.DrawLine (vertex0, vertex1, m_AreaColor, 0.25f);
        Debug.DrawLine (vertex1, vertex2, m_AreaColor, 0.25f);
        Debug.DrawLine (vertex2, vertex3, m_AreaColor, 0.25f);
        Debug.DrawLine (vertex3, vertex0, m_AreaColor, 0.25f);
    }

	// Destroy the specified object after the specified period of time.
	private IEnumerator HandleObjectLifetime (Object obj, float lifetime)
	{
		// Wait for the lifetime.
		yield return new WaitForSeconds (lifetime);

		// Destroy the object.
		Destroy (obj);

		// Decrease our spawn count.
		--m_SpawnTotal;
	}
}
