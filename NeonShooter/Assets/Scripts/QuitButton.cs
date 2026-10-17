using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A small quit control in the top-right corner, usable by mouse and touch on desktop and mobile.
/// The visual is drawn with immediate-mode UI, but the press is detected through the Input System,
/// since IMGUI receives no input in a player built with Input System-only handling.
/// In the editor it stops play mode instead of quitting.
/// In a WebGL player the control does not appear at all, since a browser page cannot quit itself.
/// </summary>
public sealed class QuitButton : MonoBehaviour
{
    void OnEnable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        enabled = false;
#endif
    }

    void Update()
    {
        if (menuShown)
            return;

        Vector2 screenPoint;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            screenPoint = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            screenPoint = Mouse.current.position.ReadValue();
        else
            return;

        // Input coordinates are bottom-left origin; the GUI rect is top-left origin, so flip Y before testing.
        var guiPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);

        if (buttonRect.Contains(guiPoint))
            Quit();
    }

    void OnGUI()
    {
        if (menuShown)
            return;

        m_Style ??= new GUIStyle(GUI.skin.box)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.6f, 0.9f, 1f, 0.9f) }
        };

        GUI.Box(buttonRect, "QUIT", m_Style);
    }

    // Leaves the game; in the editor that means stopping play mode. The menu's Quit button uses this too.
    internal static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    // The main, pause, and game-over menus carry their own quit control, so this one stays out of the way while any is up.
    static bool menuShown => GameManager.instance != null && (GameManager.instance.isAwaitingStart || GameManager.instance.isPaused || GameManager.instance.isGameOver);

    // The button rectangle in GUI (top-left origin) coordinates, kept generous so it is an easy touch target.
    Rect buttonRect => new(Screen.width - m_Size.x - m_Margin, m_Margin, m_Size.x, m_Size.y);

    #region Internal

    // On-screen size and edge margin, in pixels.
    [SerializeField] Vector2 m_Size = new(88f, 44f);
    [SerializeField] float m_Margin = 12f;

    GUIStyle m_Style;

    #endregion
}
