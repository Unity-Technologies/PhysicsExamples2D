using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Adds the changing shape's controls to the Workshop menu: its body type, its shape type and its scale.
/// Every one of them changes the existing shape while it runs.
/// </summary>
public sealed class ModifyGeometryOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddEnum<PhysicsBody.BodyType>("Body Type", m_Contents.bodyType, value => m_Contents.bodyType = value);
        AddEnum("Geometry Type", m_Contents.geometryType, value => m_Contents.geometryType = value);
        AddSlider("Geometry Scale", m_Contents.geometryScale, 0.5f, 10f, value => m_Contents.geometryScale = value);
    }

    #region Internal

    [SerializeField] ModifyGeometryContents m_Contents;

    #endregion
}
