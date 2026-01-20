using UnityEngine;

public class Physics2D_SimulationLayers : MonoBehaviour
{
    public LayerMask SimulationLayers = Physics2D.AllLayers;
    public SimulationMode2D SimulationMode = SimulationMode2D.FixedUpdate;

    private SimulationMode2D m_OldSimulationMode;

    private void OnEnable()
    {
        m_OldSimulationMode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode;

        Physics2D.simulationLayers = SimulationLayers;
    }

    private void OnDisable()
    {
        Physics2D.simulationMode = m_OldSimulationMode;
        Physics2D.simulationLayers = Physics2D.AllLayers;
    }

    private void Update()
    {
        if (SimulationMode == SimulationMode2D.Script)
            Physics2D.Simulate(Time.deltaTime, SimulationLayers);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            Physics2D.simulationLayers = SimulationLayers;
            Physics2D.simulationMode = SimulationMode;
        }
    }
}
