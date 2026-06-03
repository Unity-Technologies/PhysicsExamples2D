using System.Collections;
using UnityEngine;

/// <summary>
/// Enable/Disable a component periodically.
/// </summary>
public class BehaviourEnableDisable : MonoBehaviour
{
    public Behaviour m_BehaviourComponent;
    public float m_TogglePeriod = 1f;

	IEnumerator Start ()
	{
		yield return new WaitForSeconds (1f);

        if (m_BehaviourComponent)
        {
            while (true)
            {
                // Flip the enabled state.
                m_BehaviourComponent.enabled = !m_BehaviourComponent.enabled;

                // Wait for specified period before toggling again.
                yield return new WaitForSeconds (m_TogglePeriod);
            }
        }		
	}
	
}
