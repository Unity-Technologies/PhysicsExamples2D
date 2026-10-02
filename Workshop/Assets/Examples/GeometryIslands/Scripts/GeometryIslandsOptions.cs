using UnityEngine;

/// <summary>
/// Adds the geometry islands example's controls to the Workshop: buttons to move the player and fire, and the size and explosion of each hole broken out of a slab.
/// The controls take effect from the next hit, leaving the slabs and any debris as they are.
/// </summary>
public sealed class GeometryIslandsOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        var leftButton = controlsMenu[2];
        var rightButton = controlsMenu[1];
        var fireButton = controlsMenu[0];

        leftButton.Set("Left [←]");
        rightButton.Set("Right [→]");
        fireButton.Set("Fire [Spc]");

        m_Contents.SetButtons(leftButton, rightButton, fireButton);

        AddSlider("Fragment Radius", m_Contents.fragmentRadius, 0.5f, 3f, value => m_Contents.fragmentRadius = value);
        AddToggle("Fragment Explode", m_Contents.fragmentExplode, value => m_Contents.fragmentExplode = value);
    }

    #region Internal

    [SerializeField] GeometryIslandsContents m_Contents;

    #endregion
}
