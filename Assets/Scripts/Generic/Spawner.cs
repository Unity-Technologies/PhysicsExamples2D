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

	[Range (1, 1000)]
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
			if (m_SpawnTotal >= m_SpawnMaximum)
				return;

			var obj = m_SpawnItems[Random.Range (0, m_SpawnItems.Length)];
			if (obj)
			{
				var spawnRange = m_Area * 0.5f;
				var position = transform.TransformPoint (new Vector3 (Random.Range (-spawnRange.x, spawnRange.x), Random.Range (-spawnRange.y, spawnRange.y), transform.position.z));
				var rotation = Random.Range (0.0f, m_RandomRotation);
				var spawnObj = Instantiate (obj, position, Quaternion.Euler (0f, 0f, rotation), m_SpawnParent ? m_SpawnParent : transform);
				var randomScale = Random.Range (m_MinScale, m_MaxScale);
				spawnObj.transform.localScale = new Vector3 (randomScale, randomScale);
				spawnObj.layer = m_Layer;

                var body = spawnObj.GetComponent<Rigidbody2D> ();
				if (body)
					body.gravityScale = m_GravityScale;

                if (m_SpawnLifetime > 0.0f)
                    Destroy (spawnObj, m_SpawnLifetime);

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
}
