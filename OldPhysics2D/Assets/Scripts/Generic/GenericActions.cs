using UnityEngine;

/// <summary>
/// Perform generic actions.
/// </summary>
public class GenericActions : MonoBehaviour
{
	/// <summary>
	/// Play the particle system.
	/// </summary>
	public void PlayParticleSystem()
	{
		var particleSystem = GetComponentInChildren<ParticleSystem> ();
		if (particleSystem)
			particleSystem.Play ();
	}

	/// <summary>
	/// Stop the particle system.
	/// </summary>
	public void StopParticleSystem()
	{
		var particleSystem = GetComponentInChildren<ParticleSystem> ();
		if (particleSystem)
			particleSystem.Stop (true, ParticleSystemStopBehavior.StopEmittingAndClear);
	}
}
