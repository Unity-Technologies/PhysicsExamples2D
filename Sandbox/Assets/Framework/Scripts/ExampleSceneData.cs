using UnityEngine;

/// <summary>
/// Base class for optional per-example data assets. Derive from this when an example needs
/// inspector-assigned assets (sprites, materials, etc.) that cannot be created procedurally.
/// Create a <c>MyExampleData.asset</c> in <c>ExampleAssets/</c> and assign your fields there.
/// <see cref="SceneManifest"/> auto-discovers the companion asset and injects it into
/// <see cref="SandboxExampleBehaviour.ExampleData"/> before <c>OnEnable</c> fires.
/// </summary>
public abstract class ExampleSceneData : ScriptableObject { }
