using UnityEngine;
using UnityEngine.UIElements;

public class ShortcutsView : MonoBehaviour, IFoldable
{
    // When rolled down the panel shows only the "Shortcuts" footer; the shortcut list is hidden.
    public bool StartRolledDown;

    // IFoldable: "Fold All" collapses this panel to its footer-only roll-down state.
    public void SetFolded(bool folded) => SetRolledDown(folded);

    // The migrated global-control buttons (wired by SandboxManager). Each is a single toggle/action
    // whose text reflects what a click will do.
    public Button InteractionButton { get; private set; }
    public Button PausePlayButton { get; private set; }
    public Button SingleStepButton { get; private set; }
    public Button ColorsButton { get; private set; }
    public Button ResetButton { get; private set; }
    public Button RestartButton { get; private set; }
    public Button FoldAllButton { get; private set; }
    public Button QuitButton { get; private set; }

    private CameraManipulator m_CameraManipulator;
    private UIDocument m_UIDocument;

    // Roll-down (footer) state.
    private VisualElement m_DetailsRegion;
    private Button m_Footer;
    private bool m_RolledDown;

    // Rolls the panel down to show only the footer (rolledDown) or expands the shortcut list.
    public void SetRolledDown(bool rolledDown)
    {
        m_RolledDown = rolledDown;

        // Show/hide the shortcut list that sits above the footer.
        m_DetailsRegion.style.display = rolledDown ? DisplayStyle.None : DisplayStyle.Flex;

        // Reflect the state in the footer caption (▾ = roll down to collapse, ▴ = roll up to expand).
        var caret = rolledDown ? "▴" : "▾";
        m_Footer.text = $"{SandboxUtility.HighlightColor}{caret}{SandboxUtility.EndHighlightColor}<size=50%> </size>Shortcuts";
    }

    private void ToggleRollDown() => SetRolledDown(!m_RolledDown);

    private void OnEnable()
    {
        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_UIDocument = GetComponent<UIDocument>();
        var root = m_UIDocument.rootVisualElement;

        // Menu Region.
        {
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);
        }

        // Global-control buttons (wired by SandboxManager).
        {
            InteractionButton = root.Q<Button>("sc-interaction");
            PausePlayButton = root.Q<Button>("sc-pause-play");
            SingleStepButton = root.Q<Button>("sc-single-step");
            ColorsButton = root.Q<Button>("sc-colors");
            ResetButton = root.Q<Button>("sc-reset");
            RestartButton = root.Q<Button>("sc-restart");
            FoldAllButton = root.Q<Button>("sc-fold-all");
            QuitButton = root.Q<Button>("sc-quit");
        }

        // Shortcuts roll-down (footer) toggle.
        {
            m_DetailsRegion = root.Q<VisualElement>("details-region");
            m_Footer = root.Q<Button>("shortcuts-footer");
            m_Footer.clicked += ToggleRollDown;

            // Apply the initial state.
            SetRolledDown(StartRolledDown);
        }
    }
}
