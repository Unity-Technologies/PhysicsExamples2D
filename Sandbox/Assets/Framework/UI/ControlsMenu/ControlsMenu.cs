using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlsMenu : MonoBehaviour
{
    public sealed class CustomButton
    {
        private readonly Action m_OnShown;

        public CustomButton(Button button, Action onShown = null)
        {
            this.button = button;
            m_OnShown = onShown;
            isPressed = false;

            button.RegisterCallback<PointerDownEvent>(ButtonPointerDown, TrickleDown.TrickleDown);
            button.RegisterCallback<PointerUpEvent>(ButtonPointerUp, TrickleDown.TrickleDown);
        }

        public void Set(string label)
        {
            button.text = label;
            button.style.display = DisplayStyle.Flex;

            // Reveal the (otherwise hidden) controls bar now that a scene has claimed a slot.
            m_OnShown?.Invoke();
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

    private UIDocument m_UIDocument;
    private VisualElement m_Controls;
    private readonly CustomButton[] m_CustomButtons = new CustomButton[3];

    private void OnEnable()
    {
        m_UIDocument = GetComponent<UIDocument>();
        var root = m_UIDocument.rootVisualElement;

        // Buttons.
        {
            // The bar now holds only per-scene custom buttons; it stays hidden until a scene
            // claims a slot (see ShowControls / ResetControls).
            m_Controls = root.Q<VisualElement>("controls");

            this[0] = new CustomButton(root.Q<Button>("button0"), ShowControls);
            this[1] = new CustomButton(root.Q<Button>("button1"), ShowControls);
            this[2] = new CustomButton(root.Q<Button>("button2"), ShowControls);

            ResetControls();
        }
    }

    private void OnDisable()
    {
        foreach (var button in m_CustomButtons)
            button.Reset();
    }

    private void ShowControls() => m_Controls.style.display = DisplayStyle.Flex;

    public void ResetControls()
    {
        // Hide the whole bar; a scene reveals it again by claiming a slot (CustomButton.Set).
        if (m_Controls != null)
            m_Controls.style.display = DisplayStyle.None;

        foreach (var customButton in m_CustomButtons)
        {
            // Reset the button.
            customButton.button.text = "...";
            customButton.button.style.display = DisplayStyle.None;
            customButton.isPressed = false;
        }
    }
}
