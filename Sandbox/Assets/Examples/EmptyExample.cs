using UnityEngine;
using Unity.U2D.Physics;

// A blank canvas for your own physics.
// Everything below is empty on purpose, so add your own code at the signposts.
// See the README for how to use the physics API, and look at the other examples, such as "Barrel", to see how a finished one is put together.
// To load this example first when you press "Play", select the "MainMenu" GameObject in the "Sandbox" scene and pick it in the "Start Scene" drop-down of its Sandbox Manager.
// Do this while developing, so every "Play" starts here and you do not have to choose it from the menu each time.
// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Custom", "A blank example to build your own physics in.",
    Name = "Empty",
    Purpose = "A blank canvas for your own physics.\nOpen Sandbox/Assets/Examples/EmptyExample.cs and add your code where the signposts are. Nothing is created until you do.")]
public sealed class EmptyExample : SandboxExampleBehaviour
{
    // CAMERA: change these to frame your scene.
    protected override float CameraSize => 6f;
    protected override Vector2 CameraPosition => Vector2.zero;

    // ENABLE: add one-off setup here, such as default values for your controls or event subscriptions.
    protected override void OnExampleEnable()
    {
    }

    // DISABLE: undo anything you did in OnExampleEnable here, such as event subscriptions.
    protected override void OnExampleDisable()
    {
    }

    // CUSTOM CONTROLS: add your sliders, toggles and other controls here.
    // Look at the SetupOptions method in the other examples to see how, such as "Barrel".
    protected override void SetupOptions()
    {
    }

    // SCENE SETUP: create your bodies, shapes and joints here.
    // This runs when the example loads and again whenever it is reset, with the world already cleared.
    protected override void SetupScene()
    {
    }

    // PER-FRAME LOGIC: add your own logic here, such as reading input or changing bodies as the simulation runs.
    private void Update()
    {
    }
}
