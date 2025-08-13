using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlsMenu : MonoBehaviour
{
    public sealed class CustomButton
    {
        public CustomButton(Button button)
        {
            this.button = button;
            isPressed = false;
            
            button.RegisterCallback<PointerDownEvent>(ButtonPointerDown, TrickleDown.TrickleDown);
            button.RegisterCallback<PointerUpEvent>(ButtonPointerUp, TrickleDown.TrickleDown);
        }
        
        public void Set(string label)
        {
            button.text = label;
            button.style.display = DisplayStyle.Flex;
        }

        public void Reset()
        {
            button.UnregisterCallback<PointerDownEvent>(ButtonPointerDown, TrickleDown.TrickleDown);
            button.UnregisterCallback<PointerUpEvent>(ButtonPointerUp, TrickleDown.TrickleDown);
        }
        
        private void ButtonPointerDown(PointerDownEvent evt) => isPressed = true;
        private void ButtonPointerUp(PointerUpEvent evt) => isPressed = false;
        
        public Button button { get; }
        public bool isPressed { get; set; }
    }

    public CustomButton quitButton { get; private set; }
    
    public CustomButton this[int index]
    {
        get
        {
            if (index < 0 || index >= m_CustomButtons.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            return m_CustomButtons[index];
        }
        private set
        {
            if (index < 0 || index >= m_CustomButtons.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            m_CustomButtons[index] = value;
        }
    }
    
    private CameraManipulator m_CameraManipulator;
    private UIDocument m_UIDocument;
    private readonly CustomButton[] m_CustomButtons = new CustomButton[3];

    private void OnEnable()
    {
        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_UIDocument = GetComponent<UIDocument>();
        var root = m_UIDocument.rootVisualElement;

        // Menu Region.
        {
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);
        }

        // Buttons.
        {
            this[0] = new CustomButton(root.Q<Button>("button0"));
            this[1] = new CustomButton(root.Q<Button>("button1"));
            this[2] = new CustomButton(root.Q<Button>("button2"));
            
            quitButton = new CustomButton(root.Q<Button>("quit"));

            ResetControls();
        }
    }

    private void OnDisable()
    {
        quitButton.Reset();
        
        foreach (var button in m_CustomButtons)
            button.Reset();
    }

    public void ResetControls()
    {
        foreach (var customButton in m_CustomButtons)
        {
            // Reset the button.
            customButton.button.text = "...";
            customButton.button.style.display = DisplayStyle.None;
            customButton.isPressed = false;
        }
    }
}