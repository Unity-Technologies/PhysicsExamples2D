using System.Collections.Generic;
using UnityEngine;

public class SpawnPyramid : MonoBehaviour
{
    [Range(1, 10000)]
    public int m_SpawnCount = 20;

    [Range(0, 10)]
    public float m_SpacingX = 1;

    [Range(0, 10)]
    public float m_SpacingY = 1;

    [Range(0, 10)]
	public float m_StepOffset = 1.5f;

	[Range (0.01f, 10.0f)]
	public float m_MinScale = 1.0f;

	[Range (0.01f, 10.0f)]
	public float m_MaxScale = 1.0f;

	[Range (0f, 100f)]
	public float m_GravityScale = 1.0f;

	[Range (0, 360)]
	public float m_RandomRotation = 0.0f;

	public RigidbodySleepMode2D m_SleepMode = RigidbodySleepMode2D.NeverSleep;
	public CollisionDetectionMode2D m_CollisionMode = CollisionDetectionMode2D.Discrete;

	public LayerMask ContactCaptureLayers = Physics2D.AllLayers;

	public PhysicsMaterial2D m_PhysicsMaterial;

	public Transform m_SpawnParent;
    public GameObject m_SpawnItem;

	// Use this for initialization
	void Start ()
    {
        Spawn();
	}

    void Spawn()
    {
        if ( m_SpawnItem == null)
            return;

        var y = 0.0f;

		for (var i = 0; i < m_SpawnCount; ++i)
		{
            var x = y * m_StepOffset;

            for (var j = i; j < m_SpawnCount; ++j)
			{
				var position = transform.TransformPoint (new Vector3 (x, y, transform.position.z));
				var rotation = Random.Range (0.0f, m_RandomRotation);

				// Create the spawn object at the random position & rotation.
				var spawnObj = Instantiate (m_SpawnItem, position, Quaternion.Euler (0f, 0f, rotation), m_SpawnParent ? m_SpawnParent : transform);

				// Set its random scale.
				var randomScale = Random.Range (m_MinScale, m_MaxScale);
				spawnObj.transform.localScale = new Vector3 (randomScale, randomScale);

                var body = spawnObj.GetComponent<Rigidbody2D> ();
				if (body)
				{
					body.sleepMode = m_SleepMode;
					body.gravityScale = m_GravityScale;
					body.sharedMaterial = m_PhysicsMaterial;
                    body.collisionDetectionMode = m_CollisionMode;
				}

				var collider = spawnObj.GetComponent<Collider2D>();
				if (collider)
				{
					collider.contactCaptureLayers = ContactCaptureLayers;
				}

				x += m_SpacingX;
			}

			y += m_SpacingY;
		}
    }
}
