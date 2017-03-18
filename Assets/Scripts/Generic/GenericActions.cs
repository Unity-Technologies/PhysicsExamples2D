using UnityEngine;

/// <summary>
/// Perform generic actions.
/// </summary>
public class GenericActions : MonoBehaviour
{
	/// <summary>
	/// Start the particle system.
	/// </summary>
	public void PlayParticleSystem()
	{
		var particleSystem = GetComponentInChildren<ParticleSystem> ();
		if (particleSystem)
			particleSystem.Play ();
	}
}
