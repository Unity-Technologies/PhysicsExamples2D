using Unity.U2D.Physics;

/// <summary>
/// Adds the multiple prismatic example's one control to the Toolbox menu: how many sub-steps the world takes in every step.
/// The sub-steps decide how well the stretched slider joints hold, and the Toolbox puts back the menu's own value when the example unloads.
/// </summary>
public sealed class ManyPrismaticsOptions : ToolboxOptionsProvider
{
    protected override void SetupOptions()
    {
        AddSliderInt("World Sub-Steps", PhysicsWorld.defaultWorld.simulationSubSteps, 1, 64, value => toolbox.SetExampleSubSteps(value));
    }
}
