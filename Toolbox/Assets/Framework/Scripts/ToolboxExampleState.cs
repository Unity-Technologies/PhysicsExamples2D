using System;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// The global settings an example asks the Toolbox to change for as long as it is loaded, such as forcing joints to be drawn or stopping bodies from sleeping.
/// These belong to the Toolbox rather than to any one scene, so an example declares what it wants here and the Toolbox applies it before the scene loads and puts it back when the scene unloads.
/// </summary>
/// <remarks>
/// Every setting has a "leave alone" value and starts there, so an example that declares nothing changes nothing.
/// A control the example supplies may still move any of these while it runs; the value is put back on unload either way.
/// See <see cref="ToolboxExampleInfo"/>, which carries one of these per example.
/// </remarks>
[Serializable]
public struct ToolboxExampleState
{
    /// <summary>
    /// Whether an example leaves a setting as the menu has it, or forces it on or off while the example is loaded.
    /// </summary>
    public enum Override
    {
        /// <summary>
        /// Leave the setting wherever the menu currently has it.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Turn the setting on for as long as this example is loaded.
        /// </summary>
        On = 1,

        /// <summary>
        /// Turn the setting off for as long as this example is loaded.
        /// </summary>
        Off = 2
    }

    /// <summary>
    /// Which parts of the debug drawing this example takes control of, greying out their menu toggles while it is loaded.
    /// Leave this empty, the default, for an example that wants the drawing the menu is already set to.
    /// </summary>
    public PhysicsWorld.DrawOptions overriddenDrawOptions;

    /// <summary>
    /// What the parts named by <see cref="overriddenDrawOptions"/> are forced to.
    /// Naming a part in the override and leaving it out here turns that part off, which is how an example hides drawing it does not want.
    /// </summary>
    public PhysicsWorld.DrawOptions fixedDrawOptions;

    /// <summary>
    /// Whether bodies in this example are allowed to fall asleep when they stop moving.
    /// Turn it off for an example that measures the simulation, where a sleeping body would flatter the result.
    /// </summary>
    public Override sleeping;

    /// <summary>
    /// Whether the frames per second readout stays visible while this example is loaded.
    /// Turn it off for an example whose own frame rate is beside the point and whose readout would only distract.
    /// </summary>
    public Override frameRateVisible;

    /// <summary>
    /// Whether a frame that overruns is allowed to run several simulation steps to catch up.
    /// Turn it off to hold the frame to a single step, so a heavy example reports the cost of one step rather than of however many a slow frame needed.
    /// </summary>
    public Override catchUpSteps;

    /// <summary>
    /// Whether this example takes control of any part of the debug drawing.
    /// </summary>
    public readonly bool overridesDrawOptions => overriddenDrawOptions != PhysicsWorld.DrawOptions.Off;
}
