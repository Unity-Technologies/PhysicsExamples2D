using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

// Starter template for a new Sandbox example. Copy this file to Examples/, rename it and
// the class inside, update [ExampleScene] with your category and description, then run
// Tools > 2D > Physics > Rebuild Sandbox Registry. See AUTHORING_EXAMPLES.md for the full guide.
// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Template", "An empty scene for you to experiment with!")]
public sealed class Example : SandboxExampleBehaviour
{
    private int m_CustomCount;
    private float m_CustomAngle;

    // Override to reframe the camera for your example (defaults: size 6, centered on origin).
    protected override float CameraSize => 12f;

    // Per-example setup: initialise state, save world properties, subscribe to PhysicsEvents,
    // allocate native collections, and call SetOverride* here. The base class has already
    // resolved the shared infrastructure and framed the camera before this runs.
    protected override void OnExampleEnable()
    {
        m_CustomCount = 200;
        m_CustomAngle = 199f;
    }

    // Per-example teardown: unsubscribe events, dispose native collections, restore world
    // properties. The base class clears any SetOverride* state for you after this runs.
    protected override void OnExampleDisable()
    {
    }

    // Build your option controls in code with the AddSlider/AddSliderInt/AddToggle/AddEnum helpers
    // (use OptionsContent + AddElement for anything they don't cover). Pass rebuild: true on a
    // control whose change should rebuild the scene.
    protected override void SetupOptions()
    {
        // Add option controls with the AddX helpers; they go into the shared chrome's panel.
        // Pass rebuild: true on a control whose change should rebuild the scene.
        AddSliderInt("Count", m_CustomCount, 1, 500, v => m_CustomCount = v);
        AddSlider("Angle", m_CustomAngle, 1f, 359f, v => m_CustomAngle = v);
    }

    // Build the example. The world has already been reset, so start directly with PhysicsCore2D
    // code. This is also re-run on Reset (R) and whenever you call RebuildScene().
    protected override void SetupScene()
    {
        // Here you can create the initial physics objects you might need...
    }

    private void Update()
    {
        // Get the default world.
        var world = World;

        // Draw a custom pattern.
        const float radius = 10f;
        var lastPosition = Vector2.right * radius;
        for (var n = 1; n < m_CustomCount; ++n)
        {
            var angle = PhysicsMath.ToRadians(n * m_CustomAngle);
            PhysicsMath.CosSin(angle, out var cosine, out var sine);
            var newPosition = new Vector2(cosine, sine) * radius;

            world.DrawLine(lastPosition, newPosition, Color.chartreuse);

            lastPosition = newPosition;
        }
    }
}
