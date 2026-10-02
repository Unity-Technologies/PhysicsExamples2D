using UnityEngine;

/// <summary>
/// Adds the conveyor belt's controls to the Workshop: a Spawn button that drops more debris, and sliders for the belt's surface speed and tilt.
/// Either slider clears whatever debris is on the belt and drops a fresh batch.
/// </summary>
public sealed class ConveyorBeltOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        m_SpawnButton = controlsMenu[0];
        m_SpawnButton.Set("Spawn");
        m_SpawnButton.button.clickable.clicked += m_Contents.Spawn;

        AddSlider("Conveyor Speed", m_Contents.conveyorSpeed, -30f, 30f, value =>
        {
            m_Contents.conveyorSpeed = value;
            m_Contents.Rebuild();
        });

        AddSlider("Conveyor Angle", m_Contents.conveyorAngle, -25f, 25f, value =>
        {
            m_Contents.conveyorAngle = value;
            m_Contents.Rebuild();
        });
    }

    // The controls menu outlives this example, so the button's click is handed back when the example unloads.
    private void OnDisable()
    {
        if (m_SpawnButton != null && m_Contents != null)
            m_SpawnButton.button.clickable.clicked -= m_Contents.Spawn;

        m_SpawnButton = null;
    }

    #region Internal

    [SerializeField] ConveyorBeltContents m_Contents;

    ControlsMenu.CustomButton m_SpawnButton;

    #endregion
}
