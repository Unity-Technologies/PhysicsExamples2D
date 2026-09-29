using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Adds the large world's controls to the Toolbox menu: the reverse, forward and brake buttons, whether the camera follows the car, how fast it pans, and two read-only fields for where the camera is and how wide the world is.
/// Every change acts on the running scene.
/// </summary>
public sealed class LargeWorldOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_World == null)
            return;

        var reverseButton = controlsMenu[2];
        var forwardButton = controlsMenu[1];
        var brakeButton = controlsMenu[0];

        reverseButton.Set("Reverse [←]");
        forwardButton.Set("Forward [→]");
        brakeButton.Set("Brake [Spc]");

        m_World.SetButtons(reverseButton, forwardButton, brakeButton);

        AddToggle("Follow Car", m_World.followCar, value => m_World.followCar = value);
        AddSlider("Camera Pan Speed", m_World.cameraPanSpeed, -400f, 400f, value => m_World.cameraPanSpeed = value);

        m_WorldPositionField = AddElement(new FloatField("World Position (Km)") { isReadOnly = true, focusable = false });

        var worldSizeField = AddElement(new FloatField("World Size (Km)") { isReadOnly = true, focusable = false });
        worldSizeField.value = m_World.worldSize;
    }

    // Keeps the world position field showing where the camera is.
    private void Update()
    {
        if (m_World != null && m_WorldPositionField != null)
            m_WorldPositionField.value = m_World.worldPosition;
    }

    #region Internal

    [SerializeField] LargeWorldContents m_World;

    FloatField m_WorldPositionField;

    #endregion
}
