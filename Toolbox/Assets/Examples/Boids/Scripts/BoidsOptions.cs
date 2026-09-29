using UnityEngine;

/// <summary>
/// Adds the boids' controls to the Toolbox menu: the size of the flock, how the boids steer, how the world is bounded and what is drawn.
/// The boid count, size and groups rebuild the flock, while every other setting acts on the running flock.
/// </summary>
public sealed class BoidsOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        m_Contents.SetToolbox(toolbox);

        AddSliderInt("Boid Count", m_Contents.boidCount, 3, 5000, value =>
        {
            m_Contents.boidCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Boid Size", m_Contents.boidSize, 0.1f, 0.5f, value =>
        {
            m_Contents.boidSize = value;
            m_Contents.Rebuild();
        });

        AddSlider("Max Speed", m_Contents.maxSpeed, 1f, 20f, value => m_Contents.maxSpeed = value);
        AddSlider("Sight Radius", m_Contents.sightRadius, 0.1f, 3f, value => m_Contents.sightRadius = value);
        AddSlider("Separation Radius", m_Contents.separationRadius, 0f, 10f, value => m_Contents.separationRadius = value);
        AddSlider("Separation Strength", m_Contents.separationStrength, 0f, 1f, value => m_Contents.separationStrength = value);
        AddSlider("Cohesion Strength", m_Contents.cohesionStrength, 0f, 0.1f, value => m_Contents.cohesionStrength = value);
        AddSlider("Alignment Strength", m_Contents.alignmentStrength, 0f, 1f, value => m_Contents.alignmentStrength = value);
        AddSlider("Bounds Radius", m_Contents.boundsRadius, 5f, 30f, value => m_Contents.boundsRadius = value);
        AddToggle("Bounds Wrap", m_Contents.boundsWrap, value => m_Contents.boundsWrap = value);

        AddToggle("Boid Groups", m_Contents.boidGroups, value =>
        {
            m_Contents.boidGroups = value;
            m_Contents.Rebuild();
        });

        AddToggle("Draw Trails", m_Contents.drawTrails, value => m_Contents.drawTrails = value);
    }

    #region Internal

    [SerializeField] BoidsContents m_Contents;

    #endregion
}
