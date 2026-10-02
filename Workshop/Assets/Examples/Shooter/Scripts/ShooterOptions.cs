using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Adds the shooter's controls to the Workshop menu: how many capsules are fired, how often and how widely, their speed and size ranges, and the gravity scale.
/// Every change acts on the next batch, apart from the gravity scale which acts on the capsules already in flight.
/// </summary>
public sealed class ShooterOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSliderInt("Batch Count", m_Contents.batchCount, 10, 250, value => m_Contents.batchCount = value);
        AddSlider("Batch Delay ", m_Contents.batchDelay, 0.1f, 5f, value => m_Contents.batchDelay = value);
        AddSlider("Batch Spread ", m_Contents.batchSpread, 0f, 360f, value => m_Contents.batchSpread = value);

        var batchSpeed = AddElement(new MinMaxSlider("Batch Speed", m_Contents.batchSpeed.x, m_Contents.batchSpeed.y, 10f, 50f) { focusable = false });
        batchSpeed.RegisterValueChangedCallback(evt => m_Contents.batchSpeed = evt.newValue);

        var batchSize = AddElement(new MinMaxSlider("Batch Size", m_Contents.batchSize.x, m_Contents.batchSize.y, 0.01f, 0.5f) { focusable = false });
        batchSize.RegisterValueChangedCallback(evt => m_Contents.batchSize = evt.newValue);

        AddSlider("Gravity Scale", m_Contents.gravityScale, 1f, 5f, value => m_Contents.gravityScale = value);
    }

    #region Internal

    [SerializeField] ShooterContents m_Contents;

    #endregion
}
