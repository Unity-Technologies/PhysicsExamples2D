using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

// Rebuilds the generated Toolbox example registry from the ToolboxExampleInfo assets in the project.
// Discovery is by asset type, so an example is registered purely by having its info asset in its folder, with no code to edit and no scene list to maintain by hand.
internal static class ToolboxRegistryBuilder
{
    // Project relative path of the always loaded UI scene, which must always be first in the build settings scene list.
    private const string UIScenePath = "Assets/Toolbox.unity";

    // Where the generated manifest is created if the project does not already have one.
    private const string DefaultManifestPath = "Assets/Framework/ToolboxManifest.asset";

    [MenuItem("Tools/2D/Physics/Rebuild Toolbox Registry")]
    internal static void RebuildRegistry()
    {
        var manifest = FindOrCreateManifest();
        var examples = CollectExamples();

        manifest.SetExamples(examples);
        UpdateBuildSettingsScenes(examples);

        AssetDatabase.SaveAssets();

        Debug.Log($"[Toolbox] Registry rebuilt with {examples.Count} example(s).");
    }

    // Gathers every example info asset in the project and flattens it into the manifest's runtime form.
    // An info asset with no scene assigned is skipped with a warning, since it would produce a menu entry that cannot load anything.
    private static List<ToolboxManifest.ExampleItem> CollectExamples()
    {
        var examples = new List<ToolboxManifest.ExampleItem>();

        foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(ToolboxExampleInfo)}"))
        {
            var infoPath = AssetDatabase.GUIDToAssetPath(guid);
            var info = AssetDatabase.LoadAssetAtPath<ToolboxExampleInfo>(infoPath);

            if (info == null)
                continue;

            info.SyncScenePath();

            if (string.IsNullOrEmpty(info.scenePath))
            {
                Debug.LogWarning($"[Toolbox] '{infoPath}' has no scene assigned, so it was left out of the registry.", info);
                continue;
            }

            if (string.IsNullOrEmpty(info.exampleName))
            {
                Debug.LogWarning($"[Toolbox] '{infoPath}' has no name, so it was left out of the registry.", info);
                continue;
            }

            examples.Add(new ToolboxManifest.ExampleItem
            {
                exampleName = info.exampleName,
                category = string.IsNullOrEmpty(info.category) ? "Uncategorized" : info.category,
                description = info.description,
                scenePath = info.scenePath
            });
        }

        examples.Sort((left, right) =>
        {
            var byCategory = string.Compare(left.category, right.category, System.StringComparison.OrdinalIgnoreCase);

            return byCategory != 0 ? byCategory : string.Compare(left.exampleName, right.exampleName, System.StringComparison.OrdinalIgnoreCase);
        });

        return examples;
    }

    // Rewrites the build settings scene list as the UI scene followed by every example scene.
    // Rewriting rather than merging means a deleted example drops out of the list without anyone having to remember to remove it.
    private static void UpdateBuildSettingsScenes(List<ToolboxManifest.ExampleItem> examples)
    {
        var scenes = new List<EditorBuildSettingsScene>();

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(UIScenePath) != null)
            scenes.Add(new EditorBuildSettingsScene(UIScenePath, true));
        else
            Debug.LogWarning($"[Toolbox] UI scene '{UIScenePath}' was not found, so the build settings scene list starts with the examples instead.");

        foreach (var example in examples.Where(example => example.scenePath != UIScenePath))
            scenes.Add(new EditorBuildSettingsScene(example.scenePath, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // Returns the project's manifest asset, creating one at the default path when the project has none yet.
    // More than one manifest is ambiguous, so the first found is used and the rest are reported.
    private static ToolboxManifest FindOrCreateManifest()
    {
        var guids = AssetDatabase.FindAssets($"t:{nameof(ToolboxManifest)}");

        if (guids.Length > 1)
            Debug.LogWarning($"[Toolbox] The project has {guids.Length} manifest assets; using '{AssetDatabase.GUIDToAssetPath(guids[0])}'.");

        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<ToolboxManifest>(AssetDatabase.GUIDToAssetPath(guids[0]));

        var manifest = ScriptableObject.CreateInstance<ToolboxManifest>();
        AssetDatabase.CreateAsset(manifest, DefaultManifestPath);

        return manifest;
    }
}
