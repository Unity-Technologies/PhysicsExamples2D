using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Covers the screen with a message naming the example being loaded, so a switch that takes a while does not look like the application has stopped responding.
/// </summary>
/// <remarks>
/// Building an example's contents blocks the main thread, so nothing can animate while this is up; it is shown and given a frame to draw before the work starts, and taken down once the example is running.
/// It also swallows clicks while it is up, which keeps a second example from being asked for in the middle of loading one.
/// </remarks>
[RequireComponent(typeof(UIDocument))]
public sealed class LoadingOverlay : MonoBehaviour
{
    /// <summary>
    /// Shows the overlay with the specified message, for example "Loading Barrel example ...".
    /// The caller supplies the whole message, so the same overlay covers loading, reloading and anything else that holds the main thread.
    /// </summary>
    public void Show(string message)
    {
        if (m_Message == null)
            return;

        m_Message.text = message;
        m_Root.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Hides the overlay.
    /// </summary>
    public void Hide()
    {
        if (m_Root != null)
            m_Root.style.display = DisplayStyle.None;
    }

    private void Awake()
    {
        var document = GetComponent<UIDocument>();

        m_Root = document.rootVisualElement;
        m_Root.style.flexGrow = 1f;
        m_Root.style.alignItems = Align.Center;
        m_Root.style.justifyContent = Justify.Center;
        m_Root.style.backgroundColor = BackdropColor;
        m_Root.style.display = DisplayStyle.None;
        m_Root.pickingMode = PickingMode.Position;

        var panel = new VisualElement
        {
            style =
            {
                paddingLeft = PanelPadding,
                paddingRight = PanelPadding,
                paddingTop = PanelPadding,
                paddingBottom = PanelPadding,
                backgroundColor = PanelColor,
                borderTopWidth = BorderWidth,
                borderBottomWidth = BorderWidth,
                borderLeftWidth = BorderWidth,
                borderRightWidth = BorderWidth,
                borderTopColor = BorderColor,
                borderBottomColor = BorderColor,
                borderLeftColor = BorderColor,
                borderRightColor = BorderColor,
                alignItems = Align.Center
            }
        };

        m_Message = new Label
        {
            style =
            {
                fontSize = MessageFontSize,
                color = MessageColor,
                unityFontStyleAndWeight = FontStyle.Bold
            }
        };

        panel.Add(m_Message);
        m_Root.Add(panel);
    }

    #region Internal

    // The dimmed backdrop, the box on top of it, and the message inside the box.
    static readonly Color BackdropColor = new(0f, 0f, 0f, 0.6f);
    static readonly Color PanelColor = new(0.16f, 0.16f, 0.16f, 0.98f);
    static readonly Color BorderColor = new(0.35f, 0.35f, 0.35f, 1f);
    static readonly Color MessageColor = new(0.9f, 0.9f, 0.9f, 1f);

    const float PanelPadding = 24f;
    const float BorderWidth = 1f;
    const int MessageFontSize = 22;

    VisualElement m_Root;
    Label m_Message;

    #endregion
}
