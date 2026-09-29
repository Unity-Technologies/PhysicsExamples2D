using UnityEngine;

/// <summary>
/// Adds the character's controls to the Toolbox: the left, right and jump buttons, and sliders for how it walks, jumps and pogos.
/// Every change acts on the character where it is, without rebuilding the course.
/// </summary>
public sealed class CharacterMoverOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Character == null)
            return;

        var leftButton = controlsMenu[2];
        var rightButton = controlsMenu[1];
        var jumpButton = controlsMenu[0];

        leftButton.Set("Left [←]");
        rightButton.Set("Right [→]");
        jumpButton.Set("Jump [Spc]");

        m_Character.SetButtons(leftButton, rightButton, jumpButton);

        AddSlider("Jump Speed", m_Character.jumpSpeed, 0f, 40f, value => m_Character.jumpSpeed = value);
        AddSlider("Min Speed", m_Character.minSpeed, 0f, 1f, value => m_Character.minSpeed = value);
        AddSlider("Max Speed", m_Character.maxSpeed, 0f, 20f, value => m_Character.maxSpeed = value);
        AddSlider("Stop Speed", m_Character.stopSpeed, 0f, 10f, value => m_Character.stopSpeed = value);
        AddSlider("Accelerate", m_Character.accelerate, 0f, 100f, value => m_Character.accelerate = value);
        AddSlider("Air Steer", m_Character.airSteer, 0f, 1f, value => m_Character.airSteer = value);
        AddSlider("Friction", m_Character.friction, 0f, 10f, value => m_Character.friction = value);
        AddSlider("Gravity", m_Character.gravity, 0f, 100f, value => m_Character.gravity = value);
        AddSlider("Pogo Scale", m_Character.pogoScale, 1f, 10f, value => m_Character.pogoScale = value);
        AddSlider("Pogo Frequency", m_Character.pogoFrequency, 0f, 30f, value => m_Character.pogoFrequency = value);
        AddSlider("Pogo Damping", m_Character.pogoDamping, 0f, 30f, value => m_Character.pogoDamping = value);
        AddEnum("Pogo Type", m_Character.pogoType, value => m_Character.pogoType = value);
    }

    #region Internal

    [SerializeField] CharacterMoverContents m_Character;

    #endregion
}
