using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Adds the capacity test's controls to the Toolbox menu: what it spawns, how long a step may take, and whether anything is drawn while it runs.
/// The counts and the progress bar below them are read only, and follow the test as it runs.
/// </summary>
public sealed class CapacityOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Test == null)
            return;

        AddSliderInt("Simulation Limit (ms)", m_Test.simulationLimit, 1, 50, value =>
        {
            m_Test.simulationLimit = value;
            m_Test.Restart();
        });

        AddEnum("Shape Type", m_Test.shapeType, value =>
        {
            m_Test.shapeType = value;
            m_Test.Restart();
        });

        // Drawing tens of thousands of shapes costs far more than simulating them, and the test does not measure drawing, so turning it off only makes the menu easier to use.
        AddToggle("Rendering On", true, value =>
        {
            if (toolbox == null)
                return;

            if (value)
                toolbox.ResetOverrideDrawOptions();
            else
                toolbox.SetOverrideDrawOptions(overridenOptions: ~PhysicsWorld.DrawOptions.Off, fixedOptions: PhysicsWorld.DrawOptions.Off);
        });

        m_BodyCountField = AddElement(new FloatField("Body Count") { isReadOnly = true, focusable = false });
        m_ShapeCountField = AddElement(new FloatField("Shape Count") { isReadOnly = true, focusable = false });
        m_ContactCountField = AddElement(new FloatField("Contact Count") { isReadOnly = true, focusable = false });

        m_Progress = AddElement(new ProgressBar { title = string.Empty, lowValue = 0f, highValue = m_Test.simulationLimit, value = 0f });
        m_ProgressFill = m_Progress.Q(className: "unity-progress-bar__progress");

        UpdateDisplay();

        m_Test.sampled += UpdateDisplay;
    }

    private void OnDisable()
    {
        if (m_Test != null)
            m_Test.sampled -= UpdateDisplay;
    }

    // Brings the counts and the progress bar up to date with the test's latest step.
    // The bar fills toward the limit and changes color as it gets there, so how close the device is can be read without watching the number.
    private void UpdateDisplay()
    {
        if (m_Progress == null)
            return;

        m_BodyCountField.value = m_Test.bodyCount;
        m_ShapeCountField.value = m_Test.shapeCount;
        m_ContactCountField.value = m_Test.contactCount;

        m_Progress.highValue = m_Test.simulationLimit;
        m_Progress.value = m_Test.simulationStep;

        if (m_Test.finished)
        {
            m_Progress.title = $"Simulation limit of {m_Test.simulationLimit} ms reached.";
            m_ProgressFill.style.backgroundColor = LimitColor;

            return;
        }

        m_Progress.title = $"Waiting for {m_Test.simulationLimit} ms ...";

        var progress = m_Test.simulationStep / m_Test.simulationLimit;
        m_ProgressFill.style.backgroundColor = progress switch
        {
            < 0.33f => FarColor,
            < 0.50f => ClosingColor,
            < 0.80f => NearColor,
            _ => AtColor
        };
    }

    #region Internal

    // How the progress bar reads as the step time climbs toward the limit, and the color it settles on once the test has finished.
    static readonly Color FarColor = Color.softGreen;
    static readonly Color ClosingColor = Color.yellowNice;
    static readonly Color NearColor = Color.orange;
    static readonly Color AtColor = Color.indianRed;
    static readonly Color LimitColor = Color.softRed;

    [SerializeField] CapacityTest m_Test;

    FloatField m_BodyCountField;
    FloatField m_ShapeCountField;
    FloatField m_ContactCountField;
    ProgressBar m_Progress;
    VisualElement m_ProgressFill;

    #endregion
}
