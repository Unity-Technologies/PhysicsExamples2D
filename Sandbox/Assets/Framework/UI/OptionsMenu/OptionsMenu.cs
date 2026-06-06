using UnityEngine;
using UnityEngine.UIElements;

// Bottom-left panel holding the merged World + Options controls. SandboxManager wires the actual
// controls (it queries them from this UIDocument's root); this component only owns the panel chrome:
// the camera-overlap guard and the roll-down fold (footer caret), mirroring ShortcutsView.
public class OptionsMenu : MonoBehaviour, IFoldable
{
    // When folded the panel shows only the "Options" footer; the control list is hidden. Defaults
    // to folded since the merged control list is long.
    public bool StartFolded = true;

    // IFoldable: "Fold All" collapses this panel to its footer, but "Unfold All" leaves it folded —
    // the merged control list is long and used less often, so expanding it stays a deliberate click
    // on the footer.
    public void SetFolded(bool folded)
    {
        if (folded)
            SetRolledDown(true);
    }

    private CameraManipulator m_CameraManipulator;
    private UIDocument m_UIDocument;

    private VisualElement m_DetailsRegion;
    private Button m_Footer;
    private bool m_RolledDown;

    // Rolls the panel down to show only the footer (rolledDown) or expands the control list.
    public void SetRolledDown(bool rolledDown)
    {
        m_RolledDown = rolledDown;

        m_DetailsRegion.style.display = rolledDown ? DisplayStyle.None : DisplayStyle.Flex;

        // ▾ = roll down to collapse, ▴ = roll up to expand; caret left of the title with the same
        // half-character spacing as the other panels.
        var caret = rolledDown ? "▴" : "▾";
        m_Footer.text = $"{SandboxUtility.HighlightColor}{caret}{SandboxUtility.EndHighlightColor}<size=50%> </size>Sandbox Options";
    }

    private void ToggleRollDown() => SetRolledDown(!m_RolledDown);

    private void OnEnable()
    {
        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_UIDocument = GetComponent<UIDocument>();
        var root = m_UIDocument.rootVisualElement;

        // Suppress camera input while the pointer is over the menu.
        var menuRegion = root.Q<VisualElement>("menu-region");
        menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
        menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

        // Roll-down (footer) toggle.
        m_DetailsRegion = root.Q<VisualElement>("details-region");
        m_Footer = root.Q<Button>("options-footer");
        m_Footer.clicked += ToggleRollDown;

        SetRolledDown(StartFolded);
    }
}
