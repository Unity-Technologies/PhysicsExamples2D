using UnityEngine;

/// <summary>
/// Adds the shape distance controls to the Toolbox menu: the type of the inner shape, how fast the shapes orbit and spin, and how big they are.
/// Every change acts on the next frame.
/// </summary>
public sealed class ShapeDistanceOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddEnum("Inner Shape", m_Contents.innerShapeType, value => m_Contents.innerShapeType = value);
        AddSlider("Orbit Speed", m_Contents.orbitSpeed, 1f, 180f, value => m_Contents.orbitSpeed = value);
        AddSlider("Spin Speed", m_Contents.spinSpeed, 1f, 180f, value => m_Contents.spinSpeed = value);
        AddSlider("Shape Scale", m_Contents.shapeScale, 0.5f, 2f, value => m_Contents.shapeScale = value);
    }

    #region Internal

    [SerializeField] ShapeDistanceContents m_Contents;

    #endregion
}
