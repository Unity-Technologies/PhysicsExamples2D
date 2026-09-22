using System;

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Lets an example add its own controls to the Toolbox menu while it is loaded.
/// Add one of these to an example scene only when that example needs controls a player can change while it runs; most examples need none, because their components are already tunable in the Inspector.
/// </summary>
/// <remarks>
/// The UI finds the provider in the example scene after it loads, calls <see cref="SetupOptions"/> once, and clears the controls again when the example unloads.
/// Controls are built with the <c>AddSlider</c>, <c>AddSliderInt</c>, <c>AddToggle</c> and <c>AddEnum</c> helpers, or with <see cref="AddElement{T}"/> for anything those do not cover.
/// </remarks>
public abstract class ToolboxOptionsProvider : MonoBehaviour
{
    /// <summary>
    /// Builds this example's controls into the menu's options panel.
    /// Called once after the example scene has loaded, with the panel already emptied of the previous example's controls.
    /// </summary>
    protected abstract void SetupOptions();

    /// <summary>
    /// Adds a float slider to the options panel and returns it, so a caller can update it later.
    /// The value passed in is the slider's starting value, so the field it writes to stays the single place the default lives.
    /// </summary>
    /// <remarks>
    /// The callback runs when the drag ends rather than as the handle moves, so a control that rebuilds an example does that once per drag.
    /// Typing in the slider's field or nudging it with the arrow keys still takes effect straight away.
    /// </remarks>
    protected Slider AddSlider(string label, float value, float low, float high, Action<float> onChanged)
    {
        var slider = new Slider(label, low, high) { value = value, showInputField = true, fill = true, focusable = false };
        DeferUntilSettled<Slider, float>(slider, onChanged);

        return AddElement(slider);
    }

    /// <summary>
    /// Adds a whole number slider to the options panel and returns it, so a caller can update it later.
    /// </summary>
    /// <remarks>
    /// The callback runs when the drag ends rather than as the handle moves, so a control that rebuilds an example does that once per drag.
    /// Typing in the slider's field or nudging it with the arrow keys still takes effect straight away.
    /// </remarks>
    protected SliderInt AddSliderInt(string label, int value, int low, int high, Action<int> onChanged)
    {
        var slider = new SliderInt(label, low, high) { value = value, showInputField = true, fill = true, focusable = false };
        DeferUntilSettled<SliderInt, int>(slider, onChanged);

        return AddElement(slider);
    }

    /// <summary>
    /// Adds a checkbox to the options panel and returns it, so a caller can update it later.
    /// </summary>
    protected Toggle AddToggle(string label, bool value, Action<bool> onChanged)
    {
        var toggle = new Toggle(label) { value = value, focusable = false };
        toggle.RegisterValueChangedCallback(evt => onChanged?.Invoke(evt.newValue));

        return AddElement(toggle);
    }

    /// <summary>
    /// Adds a dropdown for the specified enumeration to the options panel and returns it, so a caller can update it later.
    /// </summary>
    protected EnumField AddEnum<TEnum>(string label, TEnum value, Action<TEnum> onChanged) where TEnum : Enum
    {
        var field = new EnumField(label, value) { focusable = false };
        field.RegisterValueChangedCallback(evt => onChanged?.Invoke((TEnum)evt.newValue));

        return AddElement(field);
    }

    /// <summary>
    /// Adds an already built control to the options panel and returns it.
    /// Use this for anything the typed helpers do not cover, such as a progress bar or a read only label.
    /// </summary>
    protected T AddElement<T>(T element) where T : VisualElement
    {
        m_OptionsContent?.Add(element);

        return element;
    }

    /// <summary>
    /// The touch button bar shared by every example, for the two or three hold down controls an example may need.
    /// Returns null when the UI has not connected the provider yet, which is the case before <see cref="SetupOptions"/> runs.
    /// </summary>
    protected ControlsMenu controlsMenu => m_ControlsMenu;

    /// <summary>
    /// The Toolbox itself, for the few controls that change something the Toolbox owns rather than something in the example scene.
    /// Anything an example wants set for its whole lifetime should be declared on its <see cref="ToolboxExampleInfo"/> instead, since that is applied before the scene loads and put back when it unloads.
    /// </summary>
    protected ToolboxManager toolbox => m_Toolbox;

    // Reports a slider's value once it stops moving, rather than on every step of a drag, so a control that rebuilds an example does that once instead of on every frame of the drag.
    // Each change pushes the report back, so a drag reports only when the handle comes to rest, and typing a value or nudging it with the arrow keys reports a moment later.
    private static void DeferUntilSettled<TSlider, TValue>(TSlider slider, Action<TValue> onChanged) where TSlider : BaseSlider<TValue> where TValue : IComparable<TValue>
    {
        var pendingReport = slider.schedule.Execute(() => onChanged?.Invoke(slider.value));
        pendingReport.Pause();

        slider.RegisterValueChangedCallback(_ => pendingReport.ExecuteLater(SettleMilliseconds));
    }

    // Connects the provider to the menu's panel and control bar, then lets the example build its controls.
    // Only the UI calls this, once, after the example scene has loaded.
    internal void BuildOptions(ToolboxManager toolbox, VisualElement optionsContent, ControlsMenu controlsMenu)
    {
        m_Toolbox = toolbox;
        m_OptionsContent = optionsContent;
        m_ControlsMenu = controlsMenu;

        SetupOptions();
    }

    #region Internal

    // How long a slider must hold still before its value is reported, in milliseconds.
    // Long enough that a drag reports once, short enough that typing a value feels immediate.
    const long SettleMilliseconds = 150;

    ToolboxManager m_Toolbox;
    VisualElement m_OptionsContent;
    ControlsMenu m_ControlsMenu;

    #endregion
}
