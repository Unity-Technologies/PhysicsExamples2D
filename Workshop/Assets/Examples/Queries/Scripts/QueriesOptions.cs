using UnityEngine;

/// <summary>
/// Adds the queries' controls to the Workshop menu: how many rays are cast, how wide and far they reach, how hard they push, and what is drawn for each one.
/// Every change acts on the next frame's rays.
/// </summary>
public sealed class QueriesOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Batch Count", m_Contents.batchCount, 1, 1000, value => m_Contents.batchCount = value);
        AddSlider("Batch Spread ", m_Contents.batchSpread, 1f, 360f, value => m_Contents.batchSpread = value);
        AddSlider("Batch Distance ", m_Contents.batchDistance, 1f, 50f, value => m_Contents.batchDistance = value);
        AddSlider("Batch Force", m_Contents.batchForce, 0f, 10f, value => m_Contents.batchForce = value);
        AddToggle("Draw Rays", m_Contents.drawRays, value => m_Contents.drawRays = value);
        AddToggle("Draw Points", m_Contents.drawPoints, value => m_Contents.drawPoints = value);
        AddToggle("Draw Normals", m_Contents.drawNormals, value => m_Contents.drawNormals = value);
    }

    #region Internal

    [SerializeField] QueriesContents m_Contents;

    #endregion
}
