using System;

/// <summary>
/// Marks a <see cref="SandboxExampleBehaviour"/> as a Sandbox example and supplies the
/// metadata the example menu needs. The registry tool (Tools/2D/Physics/Rebuild Sandbox Registry)
/// scans for this attribute to regenerate the SceneManifest list and the build settings, so an
/// example no longer needs to be registered by hand.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ExampleSceneAttribute : Attribute
{
    /// <summary>The category the example is grouped under in the menu (e.g. "Shapes").</summary>
    public string Category { get; }

    /// <summary>A short description shown in the example's options panel.</summary>
    public string Description { get; }

    public ExampleSceneAttribute(string category, string description)
    {
        Category = category;
        Description = description;
    }
}
