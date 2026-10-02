using UnityEngine;
using UnityEngine.UIElements;

// Bottom-left panel hosting two mutually-exclusive sections — the "Workshop" world/draw controls
// (queried + wired by WorkshopManager) and the "Shortcuts" global-control buttons. Two footer
// buttons act as an accordion: opening one collapses the other; both can be collapsed, but both
// cannot be open at once. This component owns the panel layout (camera-overlap guard + accordion);
// WorkshopManager wires the actual control behaviour.
public class BottomLeftMenu : MonoBehaviour, IFoldable
{
    // Which section is expanded. None = both collapsed (just the two footer buttons showing).
    private enum AccordionState { None, Workshop, Shortcuts }

    // When true the panel starts with both sections collapsed.
    private const bool k_StartFolded = true;

    // The global-control buttons (wired by WorkshopManager).
    public Button InteractionButton { get; private set; }
    public Button PausePlayButton { get; private set; }
    public Button SingleStepButton { get; private set; }
    public Button ResetButton { get; private set; }
    public Button FoldAllButton { get; private set; }
    public Button QuitButton { get; private set; }

    private UIDocument m_UIDocument;

    private VisualElement m_WorkshopDetails;
    private VisualElement m_ShortcutsDetails;
    private Button m_WorkshopFooter;
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

        m_WorkshopDetails.style.display = state == AccordionState.Workshop ? DisplayStyle.Flex : DisplayStyle.None;
        m_ShortcutsDetails.style.display = state == AccordionState.Shortcuts ? DisplayStyle.Flex : DisplayStyle.None;

        m_WorkshopFooter.text = FooterText(state == AccordionState.Workshop, "Workshop");
        m_ShortcutsFooter.text = FooterText(state == AccordionState.Shortcuts, "Shortcuts");
    }

    // ▼ = expanded (click to collapse), ▲ = collapsed (click to expand); caret left of the title
    // with the same half-character spacing as the other panels.
    private static string FooterText(bool expanded, string label)
    {
        var caret = expanded ? "▼" : "▲";
        return $"{WorkshopUtility.HighlightColor}{caret}{WorkshopUtility.EndHighlightColor}<size=50%> </size>{label}";
    }

    // Footer clicks: toggle this section, collapsing the other (accordion — never both open).
    private void ToggleWorkshop() => SetState(m_State == AccordionState.Workshop ? AccordionState.None : AccordionState.Workshop);
    private void ToggleShortcuts() => SetState(m_State == AccordionState.Shortcuts ? AccordionState.None : AccordionState.Shortcuts);

    private void OnEnable()
    {
        m_UIDocument = GetComponent<UIDocument>();
        var root = m_UIDocument.rootVisualElement;

        // The two accordion sections.
        m_WorkshopDetails = root.Q<VisualElement>("workshop-details");
        m_ShortcutsDetails = root.Q<VisualElement>("shortcuts-details");

        // Footer toggles.
        m_WorkshopFooter = root.Q<Button>("workshop-footer");
        m_ShortcutsFooter = root.Q<Button>("shortcuts-footer");
        m_WorkshopFooter.clicked += ToggleWorkshop;
        m_ShortcutsFooter.clicked += ToggleShortcuts;

        // Global-control buttons (wired by WorkshopManager).
        InteractionButton = root.Q<Button>("sc-interaction");
        PausePlayButton = root.Q<Button>("sc-pause-play");
        SingleStepButton = root.Q<Button>("sc-single-step");
        ResetButton = root.Q<Button>("sc-reset");
        FoldAllButton = root.Q<Button>("sc-fold-all");
        QuitButton = root.Q<Button>("sc-quit");

        // Apply the initial state.
        SetState(k_StartFolded ? AccordionState.None : AccordionState.Shortcuts);
    }
}
