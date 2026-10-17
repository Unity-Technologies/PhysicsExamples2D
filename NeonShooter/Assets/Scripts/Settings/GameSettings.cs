using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

/// <summary>
/// The registry of tunable settings behind the options menu, and the saved values that persist between runs.
/// A component registers its settings in Awake and removes them in OnDestroy, so they stay listed while the component's object is switched off.
/// Registration records the component's authored value as the default, then applies the saved value if there is one.
/// Saved values are kept in PlayerPrefs as JSON, so they survive scene reloads and player restarts.
/// </summary>
[AutoStaticsCleanup]
public static partial class GameSettings
{
    /// <summary>
    /// Raised after any value changes through the registry, including resets, so open controls can refresh.
    /// </summary>
    public static event Action changed;

    /// <summary>
    /// Registers a float setting shown as a slider.
    /// </summary>
    public static SettingDefinition RegisterSlider(object owner, string tab, string label, float min, float max, Func<float> getter, Action<float> setter, bool nextRun = false)
    {
        var setting = new SettingDefinition(owner, tab, label, SettingDefinition.Kind.Slider, getter, setter) { min = min, max = max, nextRun = nextRun };

        return Add(setting);
    }

    /// <summary>
    /// Registers an integer setting shown as a slider that moves in whole steps.
    /// </summary>
    public static SettingDefinition RegisterWholeSlider(object owner, string tab, string label, int min, int max, Func<int> getter, Action<int> setter, bool nextRun = false)
    {
        var setting = new SettingDefinition(owner, tab, label, SettingDefinition.Kind.WholeSlider, () => getter(), v => setter(Mathf.RoundToInt(v)))
        {
            min = min,
            max = max,
            nextRun = nextRun
        };

        return Add(setting);
    }

    /// <summary>
    /// Registers a bool setting shown as a toggle.
    /// </summary>
    public static SettingDefinition RegisterToggle(object owner, string tab, string label, Func<bool> getter, Action<bool> setter, bool nextRun = false)
    {
        var setting = new SettingDefinition(owner, tab, label, SettingDefinition.Kind.Toggle, () => getter() ? 1f : 0f, v => setter(v >= 0.5f)) { nextRun = nextRun };

        return Add(setting);
    }

    /// <summary>
    /// Registers a setting that picks one of a list of names, shown as a dropdown.
    /// </summary>
    public static SettingDefinition RegisterChoice(object owner, string tab, string label, string[] choices, Func<int> getter, Action<int> setter, bool nextRun = false)
    {
        var setting = new SettingDefinition(owner, tab, label, SettingDefinition.Kind.Choice, () => getter(), v => setter(Mathf.RoundToInt(v)))
        {
            max = choices.Length - 1,
            choices = choices,
            nextRun = nextRun
        };

        return Add(setting);
    }

    /// <summary>
    /// Removes every setting the owner registered.
    /// </summary>
    public static void Unregister(object owner)
    {
        if (s_Settings == null)
            return;

        s_Settings.RemoveAll(setting => setting.owner == owner);
    }

    /// <summary>
    /// Writes a value to a setting's component and records it for saving.
    /// A slider drag calls this many times a second, so the write to disk waits for Flush.
    /// </summary>
    public static void Set(SettingDefinition setting, float newValue)
    {
        var clamped = setting.Clamp(newValue);

        setting.Apply(clamped);
        saved[setting.key] = clamped;
        s_Dirty = true;

        changed?.Invoke();
    }

    /// <summary>
    /// Writes any recorded values to disk.
    /// The menu calls this as it closes, so a run of slider changes costs one write.
    /// </summary>
    public static void Flush()
    {
        if (s_Dirty)
            Save();
    }

    /// <summary>
    /// Restores one setting to its authored value and forgets its saved value.
    /// </summary>
    public static void Reset(SettingDefinition setting)
    {
        setting.Apply(setting.defaultValue);
        saved.Remove(setting.key);

        Save();
        changed?.Invoke();
    }

    /// <summary>
    /// Restores every setting on one tab to its authored value.
    /// </summary>
    public static void ResetTab(string tab)
    {
        foreach (var setting in settings)
        {
            if (setting.tab != tab)
                continue;

            setting.Apply(setting.defaultValue);
            saved.Remove(setting.key);
        }

        Save();
        changed?.Invoke();
    }

    /// <summary>
    /// Restores every setting to its authored value and clears everything saved.
    /// </summary>
    public static void ResetAll()
    {
        foreach (var setting in settings)
            setting.Apply(setting.defaultValue);

        saved.Clear();

        Save();
        changed?.Invoke();
    }

    /// <summary>
    /// The settings registered on one tab, in registration order.
    /// </summary>
    public static List<SettingDefinition> SettingsIn(string tab)
    {
        var result = new List<SettingDefinition>();

        foreach (var setting in settings)
        {
            if (setting.tab == tab)
                result.Add(setting);
        }

        return result;
    }

    // Records the authored value as the default, applies any saved value, and lists the setting.
    // Two settings with the same key would share one saved value, so a duplicate is reported and dropped.
    // An entry whose owner has since been destroyed is a leak rather than a duplicate, so it is replaced.
    private static SettingDefinition Add(SettingDefinition setting)
    {
        for (var i = 0; i < settings.Count; ++i)
        {
            var existing = settings[i];

            if (existing.key != setting.key)
                continue;

            if (existing.owner is UnityEngine.Object unityOwner && unityOwner == null)
            {
                settings.RemoveAt(i);

                break;
            }

            Debug.LogError($"A setting named \"{setting.key}\" is already registered by {existing.owner}; the one from {setting.owner} is ignored.");

            return setting;
        }

        setting.defaultValue = setting.value;

        if (saved.TryGetValue(setting.key, out var savedValue))
            setting.Apply(savedValue);

        settings.Add(setting);

        return setting;
    }

    // Writes every saved value to PlayerPrefs as one JSON document.
    private static void Save()
    {
        var document = new SavedValues();

        foreach (var pair in saved)
        {
            document.keys.Add(pair.Key);
            document.values.Add(pair.Value);
        }

        PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(document));
        PlayerPrefs.Save();

        s_Dirty = false;
    }

    // Reads the saved values back from PlayerPrefs; an absent or unreadable document leaves nothing saved.
    private static Dictionary<string, float> Load()
    {
        var result = new Dictionary<string, float>();
        var json = PlayerPrefs.GetString(PrefsKey, "");

        if (string.IsNullOrEmpty(json))
            return result;

        var document = JsonUtility.FromJson<SavedValues>(json);

        if (document == null || document.keys == null || document.values == null)
            return result;

        var count = Mathf.Min(document.keys.Count, document.values.Count);

        for (var i = 0; i < count; ++i)
            result[document.keys[i]] = document.values[i];

        return result;
    }

    /// <summary>
    /// The tabs that currently have settings, in menu order.
    /// Tabs outside the known order are appended as they appear.
    /// </summary>
    public static List<string> tabs
    {
        get
        {
            var result = new List<string>();

            foreach (var tab in TabOrder)
            {
                if (SettingsIn(tab).Count > 0)
                    result.Add(tab);
            }

            foreach (var setting in settings)
            {
                if (!result.Contains(setting.tab))
                    result.Add(setting.tab);
            }

            return result;
        }
    }

    // The registry and the saved values, created on first use so a statics reset leaves them ready to rebuild.
    private static List<SettingDefinition> settings => s_Settings ??= new List<SettingDefinition>();
    private static Dictionary<string, float> saved => s_Saved ??= Load();

    #region Internal

    // The tab names every component registers under, in the order the menu shows them.
    public const string GameTab = "Game";
    public const string BossTab = "Boss";
    public const string WeaponsTab = "Weapons";
    public const string AudioTab = "Audio";
    public const string ScreenFxTab = "Screen FX";
    public const string EffectsTab = "Effects";
    public const string LightingTab = "Lighting";
    public const string VisualsTab = "Visuals";

    private const string PrefsKey = "NeonShooter.Settings";

    // Fixed data, so it opts out of the automatic statics reset that would otherwise clear it.
    [NoAutoStaticsCleanup]
    private static readonly string[] TabOrder = { GameTab, BossTab, WeaponsTab, AudioTab, ScreenFxTab, EffectsTab, LightingTab, VisualsTab };

    private static List<SettingDefinition> s_Settings;
    private static Dictionary<string, float> s_Saved;
    private static bool s_Dirty;

    // The shape the saved values take on disk; JsonUtility cannot write a dictionary, so the keys and values are parallel lists.
    [Serializable]
    private sealed class SavedValues
    {
        public List<string> keys = new();
        public List<float> values = new();
    }

    #endregion
}
