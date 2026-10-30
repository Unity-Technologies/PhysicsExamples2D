using UnityEngine;

/// <summary>
/// Adds the junkyard's controls to the Workshop menu: the dropped shape type, the column and row counts, the pusher's height and speed, and the gravity scale.
/// The shape type, counts and pusher height rebuild the scene, while the pusher speed and gravity scale act on the running scene.
/// </summary>
public sealed class JunkyardOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddEnum("Object Type", m_Contents.objectType, value =>
        {
            m_Contents.objectType = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Column Count", m_Contents.columnCount, 10, 200, value =>
        {
            m_Contents.columnCount = value;
            m_Contents.Rebuild();
        });

        AddSliderInt("Row Count", m_Contents.rowCount, 1, 40, value =>
        {
            m_Contents.rowCount = value;
            m_Contents.Rebuild();
        });

        AddSlider("Pusher Height", m_Contents.pusherHeight, 8f, 40f, value =>
        {
            m_Contents.pusherHeight = value;
            m_Contents.Rebuild();
        });

        AddSlider("Pusher Speed", m_Contents.pusherSpeed, 0f, 5f, value => m_Contents.pusherSpeed = value);
        AddSlider("Gravity Scale", m_Contents.gravityScale, 0f, 5f, value => m_Contents.gravityScale = value);
    }

    #region Internal

    [SerializeField] JunkyardContents m_Contents;

    #endregion
}
