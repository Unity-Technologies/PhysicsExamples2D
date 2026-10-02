using UnityEngine;

/// <summary>
/// Adds the explosion's controls to the Workshop: the Explode button, how many shapes are dropped, and the radius, falloff and impulse of the explosion.
/// Changing the shape count rebuilds the scene, and the other sliders act on the next explosion.
/// </summary>
public sealed class ExplodeOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        var explodeButton = controlsMenu[0];
        explodeButton.Set("Explode [Spc]");

        m_Contents.SetButton(explodeButton);

        AddSliderInt("Shape Count", m_Contents.shapeCount, 50, 1000, value =>
        {
            m_Contents.shapeCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Radius", m_Contents.radius, 1f, 20f, value => m_Contents.radius = value);
        AddSlider("Falloff", m_Contents.falloff, 0f, 10f, value => m_Contents.falloff = value);
        AddSlider("Impulse", m_Contents.impulse, -60f, 60f, value => m_Contents.impulse = value);
    }

    #region Internal

    [SerializeField] ExplodeContents m_Contents;

    #endregion
}
