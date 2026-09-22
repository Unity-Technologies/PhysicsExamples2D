using UnityEngine;

/// <summary>
/// Adds the funnel's controls to the Toolbox menu: what falls through it, how big, how often, and how hard gravity pulls.
/// Every one of them takes effect on the next spawn, so nothing already falling is disturbed.
/// </summary>
public sealed class FunnelOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Spawner == null)
            return;

        AddEnum("Object Type", m_Spawner.objectType, value => m_Spawner.objectType = value);

        AddSlider("Object Scale", m_Spawner.objectScale, 1f, 3f, value => m_Spawner.objectScale = value);

        AddSlider("Spawn Period", m_Spawner.spawnPeriod, 0.1f, 1f, value => m_Spawner.spawnPeriod = value);

        AddSlider("Gravity Scale", m_Spawner.gravityScale, 1f, 10f, value => m_Spawner.gravityScale = value);
    }

    #region Internal

    [SerializeField] FunnelSpawner m_Spawner;

    #endregion
}
