using System;
using UnityEngine;

/// <summary>
/// One tunable value the options menu exposes: the tab it appears on, the control it is shown as, the range it moves in, and how to read and write it.
/// The owning component registers it through GameSettings, which records the component's authored value as the default and applies any saved value.
/// Every kind is carried as a float: a toggle is zero or one, and a choice is the index into its names.
/// </summary>
public sealed class SettingDefinition
{
    /// <summary>
    /// The control a setting is shown as.
    /// </summary>
    public enum Kind
    {
        Slider,
        WholeSlider,
        Toggle,
        Choice
    }

    public SettingDefinition(object owner, string tab, string label, Kind kind, Func<float> getter, Action<float> setter)
    {
        this.owner = owner;
        this.tab = tab;
        this.label = label;
        this.kind = kind;
        m_Getter = getter;
        m_Setter = setter;
    }

    /// <summary>
    /// Writes a value to the owning component, clamped to the setting's range.
    /// </summary>
    public void Apply(float newValue) => m_Setter(Clamp(newValue));

    /// <summary>
    /// Clamps a value to the setting's range, and to a whole number for the kinds that need one.
    /// </summary>
    public float Clamp(float rawValue)
    {
        var clamped = Mathf.Clamp(rawValue, min, max);

        return kind == Kind.Slider ? clamped : Mathf.Round(clamped);
    }

    /// <summary>
    /// The component the setting belongs to; used to remove its settings together.
    /// </summary>
    public object owner { get; }

    /// <summary>
    /// The options tab the setting appears on.
    /// </summary>
    public string tab { get; }

    /// <summary>
    /// The text shown beside the control.
    /// </summary>
    public string label { get; }

    /// <summary>
    /// The control the setting is shown as.
    /// </summary>
    public Kind kind { get; }

    /// <summary>
    /// The saved-value key, unique across the registry.
    /// </summary>
    public string key => $"{tab}/{label}";

    /// <summary>
    /// The lowest value the control allows.
    /// </summary>
    public float min { get; internal set; }

    /// <summary>
    /// The highest value the control allows.
    /// </summary>
    public float max { get; internal set; } = 1f;

    /// <summary>
    /// The names a choice setting picks between, in index order; null for the other kinds.
    /// </summary>
    public string[] choices { get; internal set; }

    /// <summary>
    /// Whether the value is only read when a run begins, so a change shows on the next run rather than at once.
    /// </summary>
    public bool nextRun { get; internal set; }

    /// <summary>
    /// The value the owning component was authored with, restored by a reset.
    /// </summary>
    public float defaultValue { get; internal set; }

    /// <summary>
    /// The live value read from the owning component.
    /// </summary>
    public float value => m_Getter();

    /// <summary>
    /// Whether the live value matches the authored default.
    /// </summary>
    public bool isDefault => Mathf.Approximately(value, defaultValue);

    #region Internal

    private readonly Func<float> m_Getter;
    private readonly Action<float> m_Setter;

    #endregion
}
