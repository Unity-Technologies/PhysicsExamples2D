using UnityEngine;
using UnityEngine.UIElements;

// Bottom-left panel hosting two mutually-exclusive sections — the "Sandbox" world/draw controls
// (queried + wired by SandboxManager) and the "Shortcuts" global-control buttons. Two footer
// buttons act as an accordion: opening one collapses the other; both can be collapsed, but both
// cannot be open at once. This component owns the panel chrome (camera-overlap guard + accordion);
// SandboxManager wires the actual control behaviour.
public class BottomLeftMenu : MonoBehaviour, IFoldable
{
    // Which section is expanded. None = both collapsed (just the two footer buttons showing).
    private enum AccordionState { None, Sandbox, Shortcuts }

    // When true the panel starts with both sections collapsed.
    public bool StartFolded = true;

    // The global-control buttons (wired by SandboxManager).
    public Button InteractionButton { get; private set; }
    public Button PausePlayButton { get; private set; }
    public Button SingleStepButton { get; private set; }
    public Button ColorsButton { get; private set; }
    public Button ResetButton { get; private set; }
    public Button RestartButton { get; private set; }
    public Button FoldAllButton { get; private set; }
    public Button QuitButton { get; private set; }

    private UIDocument m_UIDocument;

    private VisualElement m_SandboxDetails;
    private VisualElement m_ShortcutsDetails;
    private Button m_SandboxFooter;
    private Button m_ShortcutsFooter;
    private AccordionState m_State;

    // IFoldable: "Fold All" collapses both sections. "Unfold All" leaves the panel collapsed — the
    // sections are large / used on demand, so expanding stays a deliberate click on a footer button.
    public void SetFolded(bool folded)
    {
        if (folded)
            Collapse();
    }

    // Collapse both sections (e.g. on scene change, so the panel can't overlap the scene's options).
    public void Collapse() => SetState(AccordionState.None);

    // Shows the section for the given state (or neither for None) and refreshes both footer carets.
    private void SetState(AccordionState state)
    {
        m_State = state;

        m_SandboxDetails.style.display = state == AccordionState.Sandbox ? DisplayStyle.Flex : DisplayStyle.None;
        m_ShortcutsDetails.style.display = state == AccordionState.Shortcuts ? DisplayStyle.Flex : DisplayStyle.None;

        m_SandboxFooter.text = FooterText(state == AccordionState.Sandbox, "Sandbox");
        m_ShortcutsFooter.text = FooterText(state == AccordionState.Shortcuts, "Shortcuts");
    }

    // ▼ = expanded (click to collapse), ▲ = collapsed (click to expand); caret left of the title
    // with the same half-character spacing as the other panels.
    private static string FooterText(bool expanded, string label)
    {
        var caret = expanded ? "▼" : "▲";
        return $"{SandboxUtility.HighlightColor}{caret}{SandboxUtility.EndHighlightColor}<size=50%> </size>{label}";
    }

    // Footer clicks: toggle this section, collapsing the other (accordion — never both open).
    private void ToggleSandbox() => SetState(m_State == AccordionState.Sandbox ? AccordionState.None : AccordionState.Sandbox);
    private void ToggleShortcuts() => SetState(m_State == AccordionState.Shortcuts ? AccordionState.None : AccordionState.Shortcuts);

    private void OnEnable()
    {
        m_UIDocument = GetComponent<UIDocument>();
        var root = m_UIDocument.rootVisualElement;

        // The two accordion sections.
        m_SandboxDetails = root.Q<VisualElement>("sandbox-details");
        m_ShortcutsDetails = root.Q<VisualElement>("shortcuts-details");

        // Footer toggles.
        m_SandboxFooter = root.Q<Button>("sandbox-footer");
        m_ShortcutsFooter = root.Q<Button>("shortcuts-footer");
        m_SandboxFooter.clicked += ToggleSandbox;
        m_ShortcutsFooter.clicked += ToggleShortcuts;

        // Global-control buttons (wired by SandboxManager).
        InteractionButton = root.Q<Button>("sc-interaction");
        PausePlayButton = root.Q<Button>("sc-pause-play");
        SingleStepButton = root.Q<Button>("sc-single-step");
        ColorsButton = root.Q<Button>("sc-colors");
        ResetButton = root.Q<Button>("sc-reset");
        RestartButton = root.Q<Button>("sc-restart");
        FoldAllButton = root.Q<Button>("sc-fold-all");
        QuitButton = root.Q<Button>("sc-quit");

        // Apply the initial state.
        SetState(StartFolded ? AccordionState.None : AccordionState.Shortcuts);
    }
}
