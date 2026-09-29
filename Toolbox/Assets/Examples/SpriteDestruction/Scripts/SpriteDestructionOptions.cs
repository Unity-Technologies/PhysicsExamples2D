using UnityEngine;

/// <summary>
/// Adds the sprite destruction example's controls to the Toolbox menu: the size of each break, whether it leaves debris, how many pieces it makes, their friction, bounciness and blast force, and how hard gravity pulls.
/// Every one of them takes effect on the next click, leaving whatever is already broken as it is.
/// </summary>
public sealed class SpriteDestructionOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Fragment Radius", m_Contents.fragmentRadius, 0.5f, 3f, value => m_Contents.fragmentRadius = value);
        AddToggle("Create Fragments", m_Contents.fragmentCreate, value => m_Contents.fragmentCreate = value);
        AddSliderInt("Fragment Count", m_Contents.fragmentCount, 1, 50, value => m_Contents.fragmentCount = value);
        AddSlider("Fragment Friction", m_Contents.fragmentFriction, 0f, 1f, value => m_Contents.fragmentFriction = value);
        AddSlider("Fragment Bounciness", m_Contents.fragmentBounciness, 0f, 0.75f, value => m_Contents.fragmentBounciness = value);
        AddSlider("Fragment Force", m_Contents.fragmentForce, 0f, 50f, value => m_Contents.fragmentForce = value);
        AddSlider("Gravity Scale", m_Contents.gravityScale, 0.1f, 10f, value => m_Contents.gravityScale = value);
    }

    #region Internal

    [SerializeField] SpriteDestructionContents m_Contents;

    #endregion
}
