using System;

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The window that explains the loaded example: a title, what the example is for, and the controls that show it off.
/// </summary>
/// <remarks>
/// It is built in code on top of an existing panel and covers that panel with a dimmed backdrop while it is open, so clicks never reach anything behind it.
/// A click on the backdrop or on the Close button closes it, and <see cref="closed"/> is raised every time it closes.
/// The text is plain, always in the same two sections: a purpose, then the controls when there are any.
/// </remarks>
public sealed class HelpDialog
{
    /// <summary>
    /// Builds the window, hidden, as the last child of the specified element so it is drawn above everything already there.
    /// </summary>
    /// <param name="parent">The root element of the panel the window covers while it is open.</param>
    public HelpDialog(VisualElement parent)
    {
        m_Backdrop = new VisualElement
        {
            pickingMode = PickingMode.Position,
            style =
            {
                position = Position.Absolute,
                left = 0f,
                top = 0f,
                right = 0f,
                bottom = 0f,
                alignItems = Align.Center,
                justifyContent = Justify.Center,
                backgroundColor = BackdropColor,
                display = DisplayStyle.None
            }
        };

        // A click on the dimmed area closes the window, but a click on the window itself must not.
        m_Backdrop.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.target == m_Backdrop)
                Hide();
        });

        var window = new VisualElement
        {
            style =
            {
                width = Length.Percent(WindowWidthPercent),
                height = Length.Percent(WindowHeightPercent),
                paddingLeft = WindowPadding,
                paddingRight = WindowPadding,
                paddingTop = WindowPadding,
                paddingBottom = WindowPadding,
                backgroundColor = WindowColor,
                borderTopWidth = BorderWidth,
                borderBottomWidth = BorderWidth,
                borderLeftWidth = BorderWidth,
                borderRightWidth = BorderWidth,
                borderTopColor = BorderColor,
                borderBottomColor = BorderColor,
                borderLeftColor = BorderColor,
                borderRightColor = BorderColor,
                borderTopLeftRadius = BorderRadius,
                borderTopRightRadius = BorderRadius,
                borderBottomLeftRadius = BorderRadius,
                borderBottomRightRadius = BorderRadius
            }
        };

        m_Title = new Label
        {
            style =
            {
                fontSize = TitleFontSize,
                color = TextColor,
                unityTextAlign = TextAnchor.MiddleCenter,
                unityFontStyleAndWeight = FontStyle.Bold,
                marginBottom = SectionSpacing
            }
        };

        m_Scroll = new ScrollView(ScrollViewMode.Vertical)
        {
            style =
            {
                flexGrow = 1f,
                paddingLeft = WindowPadding,
                paddingRight = WindowPadding,
                paddingTop = WindowPadding,
                paddingBottom = WindowPadding,
                borderTopWidth = BorderWidth,
                borderBottomWidth = BorderWidth,
                borderLeftWidth = BorderWidth,
                borderRightWidth = BorderWidth,
                borderTopColor = BorderColor,
                borderBottomColor = BorderColor,
                borderLeftColor = BorderColor,
                borderRightColor = BorderColor
            }
        };

        m_PurposeHeading = CreateHeading("Purpose");
        m_PurposeText = CreateText();
        m_ControlsHeading = CreateHeading("Controls");
        m_ControlsText = CreateText();

        m_Scroll.Add(m_PurposeHeading);
        m_Scroll.Add(m_PurposeText);
        m_Scroll.Add(m_ControlsHeading);
        m_Scroll.Add(m_ControlsText);

        var closeButton = new Button(Hide)
        {
            text = "Close",
            focusable = false,
            style =
            {
                alignSelf = Align.Stretch,
                height = CloseButtonHeight,
                marginTop = WindowPadding,
                marginLeft = 0f,
                marginRight = 0f,
                marginBottom = 0f
            }
        };

        window.Add(m_Title);
        window.Add(m_Scroll);
        window.Add(closeButton);
        m_Backdrop.Add(window);
        parent.Add(m_Backdrop);
    }

    /// <summary>
    /// Whether the window is currently showing.
    /// </summary>
    public bool isOpen => m_Backdrop.style.display == DisplayStyle.Flex;

    /// <summary>
    /// Raised each time the window closes, whether the user closed it or the caller did.
    /// </summary>
    public event Action closed;

    /// <summary>
    /// Opens the window with the specified title and text.
    /// The controls section is left out when the controls text is empty, and the text is scrolled back to the top.
    /// </summary>
    /// <param name="title">The heading shown across the top, for example the category and example name.</param>
    /// <param name="purpose">What the example is for.</param>
    /// <param name="controls">The controls that show the example off, or empty for none.</param>
    public void Show(string title, string purpose, string controls)
    {
        m_Title.text = title;
        m_PurposeText.text = purpose?.Replace("\n", "\n\n");

        var hasControls = !string.IsNullOrWhiteSpace(controls);
        m_ControlsText.text = hasControls ? FormatControls(controls) : string.Empty;
        m_ControlsHeading.style.display = hasControls ? DisplayStyle.Flex : DisplayStyle.None;
        m_ControlsText.style.display = hasControls ? DisplayStyle.Flex : DisplayStyle.None;

        m_Scroll.scrollOffset = Vector2.zero;
        m_Backdrop.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Closes the window, and does nothing when the window is already closed.
    /// </summary>
    /// <remarks>
    /// Raises <see cref="closed"/> when it closes the window.
    /// </remarks>
    public void Hide()
    {
        if (!isOpen)
            return;

        m_Backdrop.style.display = DisplayStyle.None;
        closed?.Invoke();
    }

    // Puts a blank line between the controls, one per line of the text, and makes the name at the start of each one bold.
    // A control's name is whatever comes before the first colon on its line, and a line without a colon is left as it is.
    private static string FormatControls(string controls)
    {
        var lines = controls.Split('\n');

        for (var i = 0; i < lines.Length; ++i)
        {
            var colon = lines[i].IndexOf(": ", StringComparison.Ordinal);

            if (colon > 0)
                lines[i] = $"<b>{lines[i][..colon]}</b>{lines[i][colon..]}";
        }

        return string.Join("\n\n", lines);
    }

    // Creates the heading that starts one of the two sections.
    private static Label CreateHeading(string text)
    {
        return new Label($"{WorkshopUtility.HighlightColor}{text}{WorkshopUtility.EndHighlightColor}")
        {
            style =
            {
                fontSize = HeadingFontSize,
                unityFontStyleAndWeight = FontStyle.Bold,
                marginTop = HeadingTopSpacing,
                marginBottom = SectionSpacing * 0.5f
            }
        };
    }

    // Creates a block of body text that wraps to the window's width.
    private static Label CreateText()
    {
        return new Label
        {
            style =
            {
                color = TextColor,
                whiteSpace = WhiteSpace.Normal
            }
        };
    }

    #region Internal

    // The dimmed backdrop, the window on top of it, and the text inside the window.
    static readonly Color BackdropColor = new(0f, 0f, 0f, 0.6f);
    static readonly Color WindowColor = new(0.16f, 0.16f, 0.16f, 1f);
    static readonly Color BorderColor = new(0.35f, 0.35f, 0.35f, 1f);
    static readonly Color TextColor = new(0.9f, 0.9f, 0.9f, 1f);

    // The window's size as a share of the panel it covers, and its spacing and text sizes.
    const float WindowWidthPercent = 57f;
    const float WindowHeightPercent = 80f;
    const float WindowPadding = 12f;
    const float SectionSpacing = 8f;
    const float HeadingTopSpacing = 24f;
    const float BorderWidth = 1f;
    const float BorderRadius = 6f;
    const float CloseButtonHeight = 44f;
    const int TitleFontSize = 20;
    const int HeadingFontSize = 16;

    readonly VisualElement m_Backdrop;
    readonly Label m_Title;
    readonly ScrollView m_Scroll;
    readonly Label m_PurposeHeading;
    readonly Label m_PurposeText;
    readonly Label m_ControlsHeading;
    readonly Label m_ControlsText;

    #endregion
}
