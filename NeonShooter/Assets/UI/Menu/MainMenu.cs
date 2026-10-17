using System;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// The main menu and pause menu, drawn with UI Toolkit over the arena.
/// The menu is up when the scene loads, and its Start Game button begins the run.
/// During a run, Escape or the gamepad's Start button pauses the game and brings the menu back with Resume.
/// The options screen is generated from GameSettings: one tab per settings tab and one row per setting, each showing its default with a reset.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public sealed class MainMenu : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        m_Root = root.Q<VisualElement>("root");
        m_MenuPanel = root.Q<VisualElement>("menuPanel");
        m_OptionsPanel = root.Q<VisualElement>("optionsPanel");
        m_TabBar = root.Q<VisualElement>("tabBar");
        m_OptionsBody = root.Q<ScrollView>("optionsBody");
        m_Title = root.Q<Label>("menuTitle");
        m_Subtitle = root.Q<Label>("menuSubtitle");
        m_ResumeButton = root.Q<Button>("resumeButton");
        m_StartButton = root.Q<Button>("startButton");
        m_RestartButton = root.Q<Button>("restartButton");
        m_QuitButton = root.Q<Button>("quitButton");
        m_BackButton = root.Q<Button>("backButton");

        // The buttons die with the document's elements when this is disabled, so nothing here needs unsubscribing.
        m_ResumeButton.clicked += Resume;
        m_StartButton.clicked += StartRun;
        m_RestartButton.clicked += Restart;
        m_QuitButton.clicked += QuitButton.Quit;
        root.Q<Button>("optionsButton").clicked += ShowOptions;
        m_BackButton.clicked += ShowMenu;
        root.Q<Button>("resetTabButton").clicked += ResetTab;
        root.Q<Button>("resetAllButton").clicked += GameSettings.ResetAll;

        root.Query<Button>(className: MenuButtonClass).ForEach(AddHoverSound);

#if UNITY_WEBGL && !UNITY_EDITOR
        // A browser page cannot quit itself.
        SetVisible(m_QuitButton, false);
#endif

        GameSettings.changed += RefreshRows;

        // The glow pulse: a class toggled on a timer, with the stylesheet's transitions gliding the colors between the two states.
        // The scheduler runs on real time, so the pulse keeps breathing while the game is paused.
        m_Root.schedule.Execute(() => m_Root.ToggleInClassList(PulseClass)).Every(PulseMilliseconds);

        m_Spawner = FindAnyObjectByType<WaveSpawner>();

        ShowMenu();
    }

    void OnDisable()
    {
        GameSettings.changed -= RefreshRows;

        GameSettings.Flush();
    }

    // The pause control: during a run it opens the menu, and with the menu up it steps back out of options or resumes.
    // Game over brings the menu back by itself once the grace after the death has passed.
    void Update()
    {
        UpdateFocus();

        var game = GameManager.instance;

        if (game == null)
            return;

        if (game.isGameOverSettled && !m_GameOverShown)
        {
            m_GameOverShown = true;
            ShowMenu();

            return;
        }

        if (!pausePressed || game.isAwaitingStart || game.isGameOver)
            return;

        if (!game.isPaused)
        {
            game.Pause();
            ShowMenu();
        }
        else if (m_OptionsOpen)
            ShowMenu();
        else
            Resume();
    }

    // Focus exists only for gamepad and keyboard navigation, which has nothing else to show where it is.
    // It appears on the first stick, d-pad, or arrow press, and clears the moment the pointer moves or clicks, so mouse and touch never see it.
    void UpdateFocus()
    {
        if (m_Root.ClassListContains(HiddenClass) || m_Root.panel == null)
            return;

        var focused = m_Root.panel.focusController.focusedElement;

        if (pointerUsed)
        {
            focused?.Blur();

            return;
        }

        if (focused == null && navigationPressed)
            firstButton.Focus();
    }

    // Drops any focus, so a button hidden by a panel change cannot keep it.
    void ClearFocus()
    {
        if (m_Root.panel != null)
            m_Root.panel.focusController.focusedElement?.Blur();
    }

    // Shows the menu panel in its start, pause, or game-over form.
    // Game over turns the panel red, swaps the subtitle for the run's result, and leads with Restart.
    void ShowMenu()
    {
        ClearFocus();

        var game = GameManager.instance;
        var paused = game != null && game.isPaused;
        var gameOver = game != null && game.isGameOver;

        m_Title.text = gameOver ? "GAME OVER" : paused ? "PAUSED" : "NEON SHOOTER";
        m_Subtitle.text = gameOver ? RunResult(game) : Tagline;

        m_MenuPanel.EnableInClassList(DangerClass, gameOver);
        m_RestartButton.EnableInClassList(PrimaryButtonClass, gameOver);

        SetVisible(m_ResumeButton, paused);
        SetVisible(m_RestartButton, paused || gameOver);
        SetVisible(m_StartButton, !paused && !gameOver);
        HidePanel(m_OptionsPanel);
        ShowPanel(m_MenuPanel);
        SetVisible(m_Root, true);

        m_OptionsOpen = false;

        GameSettings.Flush();
    }

    // A panel starts transparent and slightly small, and the shown class is added a frame later so the transition has a start state to run from.
    static void ShowPanel(VisualElement panel)
    {
        SetVisible(panel, true);
        panel.schedule.Execute(() => panel.AddToClassList(PanelShownClass));
    }

    static void HidePanel(VisualElement panel)
    {
        panel.RemoveFromClassList(PanelShownClass);
        SetVisible(panel, false);
    }

    void Hide()
    {
        ClearFocus();
        HidePanel(m_MenuPanel);
        HidePanel(m_OptionsPanel);
        SetVisible(m_Root, false);

        m_OptionsOpen = false;

        GameSettings.Flush();
    }

    void StartRun()
    {
        if (GameManager.instance != null)
            GameManager.instance.StartRun();

        Hide();
    }

    void Resume()
    {
        if (GameManager.instance != null)
            GameManager.instance.Resume();

        Hide();
    }

    // The reload destroys this menu along with the scene, so nothing needs hiding first.
    static void Restart()
    {
        if (GameManager.instance != null)
            GameManager.instance.Restart();
    }

    // The tabs are rebuilt on every opening, so a component registered since the last look is included.
    void ShowOptions()
    {
        ClearFocus();
        BuildTabs();

        HidePanel(m_MenuPanel);
        ShowPanel(m_OptionsPanel);

        m_OptionsOpen = true;
    }

    void BuildTabs()
    {
        m_TabBar.Clear();
        m_TabButtons.Clear();

        var tabs = GameSettings.tabs;

        if (!tabs.Contains(m_SelectedTab))
            m_SelectedTab = tabs.Count > 0 ? tabs[0] : null;

        foreach (var tab in tabs)
        {
            var button = new Button(() => ShowTab(tab)) { text = tab };

            button.AddToClassList(TabButtonClass);
            AddHoverSound(button);
            m_TabBar.Add(button);
            m_TabButtons.Add(button);
        }

        ShowTab(m_SelectedTab);
    }

    // Fills the body with the selected tab's rows and marks its tab button.
    void ShowTab(string tab)
    {
        m_SelectedTab = tab;

        foreach (var button in m_TabButtons)
            button.EnableInClassList(TabSelectedClass, button.text == tab);

        m_OptionsBody.Clear();
        m_Rows.Clear();

        if (tab == null)
            return;

        foreach (var setting in GameSettings.SettingsIn(tab))
            m_OptionsBody.Add(CreateRow(setting));

        m_OptionsBody.scrollOffset = Vector2.zero;

        // The rows rise into place a frame after they are added, for the same reason the panels do.
        m_OptionsBody.schedule.Execute(() => m_OptionsBody.Query(className: RowClass).ForEach(row => row.AddToClassList(RowShownClass)));
    }

    void ResetTab()
    {
        if (m_SelectedTab != null)
            GameSettings.ResetTab(m_SelectedTab);
    }

    // One row: the label, the control for the setting's kind, the live value, the authored default, and a reset.
    VisualElement CreateRow(SettingDefinition setting)
    {
        var row = new VisualElement();
        row.AddToClassList(RowClass);
        AddHoverSound(row);

        var label = new Label(setting.nextRun ? setting.label + " *" : setting.label);
        label.AddToClassList(RowLabelClass);
        row.Add(label);

        var control = CreateControl(setting);
        control.AddToClassList(RowControlClass);
        row.Add(control);

        var valueLabel = new Label();
        valueLabel.AddToClassList(RowValueClass);
        row.Add(valueLabel);

        var defaultLabel = new Label($"default {Format(setting, setting.defaultValue)}");
        defaultLabel.AddToClassList(RowDefaultClass);
        row.Add(defaultLabel);

        var resetButton = new Button(() => GameSettings.Reset(setting)) { text = "Reset" };
        resetButton.AddToClassList(RowResetClass);
        row.Add(resetButton);

        var entry = new Row(setting, control, valueLabel, resetButton);

        m_Rows.Add(entry);
        RefreshRow(entry);

        return row;
    }

    // The control for a setting's kind, wired to write through the registry so every change is clamped and recorded.
    static VisualElement CreateControl(SettingDefinition setting)
    {
        switch (setting.kind)
        {
            case SettingDefinition.Kind.WholeSlider:
            {
                var slider = new SliderInt(Mathf.RoundToInt(setting.min), Mathf.RoundToInt(setting.max));
                slider.RegisterValueChangedCallback(e => GameSettings.Set(setting, e.newValue));

                return slider;
            }

            case SettingDefinition.Kind.Toggle:
            {
                var toggle = new Toggle();
                toggle.RegisterValueChangedCallback(e => GameSettings.Set(setting, e.newValue ? 1f : 0f));

                return toggle;
            }

            case SettingDefinition.Kind.Choice:
            {
                var dropdown = new DropdownField(new List<string>(setting.choices), Mathf.RoundToInt(setting.value));
                dropdown.RegisterValueChangedCallback(e => GameSettings.Set(setting, Array.IndexOf(setting.choices, e.newValue)));

                return dropdown;
            }

            default:
            {
                var slider = new Slider(setting.min, setting.max);
                slider.RegisterValueChangedCallback(e => GameSettings.Set(setting, e.newValue));

                return slider;
            }
        }
    }

    void RefreshRows()
    {
        foreach (var row in m_Rows)
            RefreshRow(row);
    }

    // Copies the live value into the row without raising the control's change event, which would write it straight back.
    static void RefreshRow(Row row)
    {
        var setting = row.setting;
        var current = setting.value;

        switch (row.control)
        {
            case Slider slider:
                slider.SetValueWithoutNotify(current);
                break;

            case SliderInt sliderInt:
                sliderInt.SetValueWithoutNotify(Mathf.RoundToInt(current));
                break;

            case Toggle toggle:
                toggle.SetValueWithoutNotify(current >= 0.5f);
                break;

            case DropdownField dropdown:
                dropdown.SetValueWithoutNotify(Format(setting, current));
                break;
        }

        row.valueLabel.text = Format(setting, current);
        row.resetButton.SetEnabled(!setting.isDefault);
    }

    // The value as the row shows it: on or off, a choice's name, a whole number, or a decimal sized to the range.
    static string Format(SettingDefinition setting, float settingValue)
    {
        switch (setting.kind)
        {
            case SettingDefinition.Kind.Toggle:
                return settingValue >= 0.5f ? "On" : "Off";

            case SettingDefinition.Kind.Choice:
                return setting.choices[Mathf.Clamp(Mathf.RoundToInt(settingValue), 0, setting.choices.Length - 1)];

            case SettingDefinition.Kind.WholeSlider:
                return Mathf.RoundToInt(settingValue).ToString(CultureInfo.InvariantCulture);

            default:
                return settingValue.ToString(setting.max - setting.min > 20f ? "0.#" : "0.00", CultureInfo.InvariantCulture);
        }
    }

    static void SetVisible(VisualElement element, bool visible) => element.EnableInClassList(HiddenClass, !visible);

    // The pointer entering a control ticks once; moving within it does not, since the element stays entered.
    static void AddHoverSound(VisualElement element) => element.RegisterCallback<PointerEnterEvent>(_ => PlayTick());

    static void PlayTick()
    {
        if (GameAudio.instance != null)
            GameAudio.instance.PlayUiTick();
    }

    // The run's result for the game-over panel: the score and how far the waves got.
    string RunResult(GameManager game)
    {
        var score = game.score.ToString("N0", CultureInfo.InvariantCulture);

        if (m_Spawner == null)
            return $"Score {score}";

        return $"Score {score}     Cycle {m_Spawner.cycle}     Set {m_Spawner.set} of {m_Spawner.setsPerCycle}";
    }

    // Where navigation lands when nothing is focused: the first tab in options, otherwise the menu's first button.
    Button firstButton
    {
        get
        {
            if (m_OptionsOpen)
                return m_TabButtons.Count > 0 ? m_TabButtons[0] : m_BackButton;

            var game = GameManager.instance;

            if (game != null && game.isGameOver)
                return m_RestartButton;

            return game != null && game.isPaused ? m_ResumeButton : m_StartButton;
        }
    }

    // A mouse move or click; either means the pointer is in charge and focus should not show.
    static bool pointerUsed =>
        Mouse.current != null && (Mouse.current.delta.ReadValue() != Vector2.zero || Mouse.current.leftButton.wasPressedThisFrame);

    // A gamepad d-pad or stick push, or a keyboard arrow or Tab, which is navigation asking for somewhere to start.
    static bool navigationPressed
    {
        get
        {
            var gamepad = Gamepad.current;

            if (gamepad != null)
            {
                if (gamepad.dpad.up.wasPressedThisFrame || gamepad.dpad.down.wasPressedThisFrame || gamepad.dpad.left.wasPressedThisFrame || gamepad.dpad.right.wasPressedThisFrame)
                    return true;

                if (gamepad.leftStick.ReadValue().sqrMagnitude > 0.25f)
                    return true;
            }

            var keyboard = Keyboard.current;

            if (keyboard == null)
                return false;

            return keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame ||
                   keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame ||
                   keyboard.tabKey.wasPressedThisFrame;
        }
    }

    // Escape or the gamepad's Start button.
    static bool pausePressed =>
        (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
        (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

    #region Internal

    // The style classes the generated elements use; the rest of the look lives in the stylesheet.
    const string HiddenClass = "hidden";
    const string PulseClass = "menu-root--pulse";
    const string DangerClass = "panel-halo--danger";
    const string PrimaryButtonClass = "menu-button--primary";
    const string PanelShownClass = "panel-halo--shown";
    const string RowShownClass = "option-row--shown";
    const string MenuButtonClass = "menu-button";
    const string TabButtonClass = "tab-button";
    const string TabSelectedClass = "tab-button--selected";
    const string RowClass = "option-row";
    const string RowLabelClass = "option-label";
    const string RowControlClass = "option-control";
    const string RowValueClass = "option-value";
    const string RowDefaultClass = "option-default";
    const string RowResetClass = "option-reset";

    // Half a breath of the glow pulse; the stylesheet's transition takes the same time, so each state is reached as the next begins.
    const long PulseMilliseconds = 1600;

    // The line under the title whenever the panel is not reporting a finished run.
    const string Tagline = "a 2D physics package experiment";

    // The parts of one generated options row that refresh when a value changes.
    sealed class Row
    {
        public Row(SettingDefinition setting, VisualElement control, Label valueLabel, Button resetButton)
        {
            this.setting = setting;
            this.control = control;
            this.valueLabel = valueLabel;
            this.resetButton = resetButton;
        }

        public SettingDefinition setting { get; }
        public VisualElement control { get; }
        public Label valueLabel { get; }
        public Button resetButton { get; }
    }

    readonly List<Button> m_TabButtons = new();
    readonly List<Row> m_Rows = new();

    VisualElement m_Root;
    VisualElement m_MenuPanel;
    VisualElement m_OptionsPanel;
    VisualElement m_TabBar;
    ScrollView m_OptionsBody;
    Label m_Title;
    Label m_Subtitle;
    WaveSpawner m_Spawner;
    Button m_ResumeButton;
    Button m_StartButton;
    Button m_RestartButton;
    Button m_QuitButton;
    Button m_BackButton;
    string m_SelectedTab;
    bool m_OptionsOpen;
    bool m_GameOverShown;

    #endregion
}
