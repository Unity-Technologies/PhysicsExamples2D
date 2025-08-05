using UnityEngine;
using UnityEngine.UIElements;

public class ShortcutMenu : MonoBehaviour
{
    private CameraManipulator m_CameraManipulator;
    private UIDocument m_UIDocument;
    
    private void OnEnable()
    {
        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_UIDocument = GetComponent<UIDocument>();
        var root = m_UIDocument.rootVisualElement;

        // Menu Region.
        {
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );
        }

        // Shortcut Description.
        {
            const string color = "<color=#7FFFD4>";
            const string endColor = "</color>";
            
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text =
                $"[{color}P{endColor}]\t\t\tPause/Continue\n" +
                $"[{color}S{endColor}]\t\t\tSingle Step\n" +
                $"[{color}C{endColor}]\t\t\tToggle Color State\n" +
                $"[{color}1{endColor}]\t\t\tLeft Mouse Drag\n" +
                $"[{color}2{endColor}]\t\t\tLeft Mouse Explode\n" +
                $"[{color}D{endColor}]\t\t\tToggle Debugging\n" +
                $"[{color}TAB{endColor}]\t\t\tToggle Overlays\n" +
                $"[{color}LMB+LCtrl{endColor}]\t\tPan\n" +
                $"[{color}LMB+Move{endColor}]\tDrag/Explode\n" +
                $"[{color}Mouse Wheel{endColor}]\tZoom\n" +
                $"[{color}Arrows+Space{endColor}]\tMisc Interaction\n" +
                $"[{color}ESC{endColor}]\t\t\tQuit (Player)\n";
        }
    }
}
