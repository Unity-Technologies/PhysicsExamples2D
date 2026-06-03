using UnityEngine;
using UnityEngine.UIElements;

public class ShortcutsView : MonoBehaviour
{
    private CameraManipulator m_CameraManipulator;
    private UIDocument m_UIDocument;

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
    }
}