using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Rebuilds the generated Workshop example registry from the WorkshopExampleInfo assets in the project.
// Discovery is by asset type, so an example is registered purely by having its info asset in its folder, with no code to edit and no scene list to maintain by hand.
internal static class WorkshopRegistryBuilder
{
    // Project relative path of the always loaded UI scene, which must always be first in the build settings scene list.
    private const string UIScenePath = "Assets/Workshop.unity";

    // Where the generated manifest is created if the project does not already have one.
    private const string DefaultManifestPath = "Assets/Framework/WorkshopManifest.asset";

    [MenuItem("Tools/2D/Physics/Rebuild Workshop Registry")]
    internal static void RebuildRegistry()
    {
        var manifest = FindOrCreateManifest();
        var examples = CollectExamples();

        manifest.SetExamples(examples);
        ResetUnregisteredStartScene(examples);
        UpdateBuildSettingsScenes(examples);

        AssetDatabase.SaveAssets();

        Debug.Log($"[Workshop] Registry rebuilt with {examples.Count} example(s).");
    }

    // Gathers every example info asset in the project and flattens it into the manifest's runtime form.
    // An info asset with no scene assigned is skipped with a warning, since it would produce a menu entry that cannot load anything.
    private static List<WorkshopManifest.ExampleItem> CollectExamples()
    {
        var examples = new List<WorkshopManifest.ExampleItem>();

        foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(WorkshopExampleInfo)}"))
        {
            var infoPath = AssetDatabase.GUIDToAssetPath(guid);
            var info = AssetDatabase.LoadAssetAtPath<WorkshopExampleInfo>(infoPath);

            if (info == null)
                continue;

            info.SyncScenePath();

            if (string.IsNullOrEmpty(info.scenePath))
            {
                Debug.LogWarning($"[Workshop] '{infoPath}' has no scene assigned, so it was left out of the registry.", info);
                continue;
            }

            if (string.IsNullOrEmpty(info.exampleName))
            {
                Debug.LogWarning($"[Workshop] '{infoPath}' has no name, so it was left out of the registry.", info);
                continue;
            }

            examples.Add(new WorkshopManifest.ExampleItem
            {
                exampleName = info.exampleName,
                category = string.IsNullOrEmpty(info.category) ? "Uncategorized" : info.category,
                description = info.description,
                scenePath = info.scenePath,
                state = info.state
            });
        }

        examples.Sort((left, right) =>
        {
            var byCategory = string.Compare(left.category, right.category, System.StringComparison.OrdinalIgnoreCase);

            return byCategory != 0 ? byCategory : string.Compare(left.exampleName, right.exampleName, System.StringComparison.OrdinalIgnoreCase);
        });

        return examples;
    }

    // Clears the manager's start example when it names an example that is no longer registered, so the manager falls back to the first registered example.
    // A start example that is empty or still registered is left exactly as it is.
    private static void ResetUnregisteredStartScene(List<WorkshopManifest.ExampleItem> examples)
    {
        var uiScene = SceneManager.GetSceneByPath(UIScenePath);
        var openedAdditively = false;

        if (!uiScene.IsValid() || !uiScene.isLoaded)
        {
            uiScene = EditorSceneManager.OpenScene(UIScenePath, OpenSceneMode.Additive);
            openedAdditively = true;
        }

        var manager = FindComponent<WorkshopManager>(uiScene);

        if (manager != null && !string.IsNullOrEmpty(manager.StartScene) && examples.All(example => example.exampleName != manager.StartScene))
        {
            Debug.LogWarning($"[Workshop] Start example '{manager.StartScene}' is no longer registered; resetting it.");
            manager.StartScene = string.Empty;

            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(uiScene);
            EditorSceneManager.SaveScene(uiScene);
        }

        if (openedAdditively)
            EditorSceneManager.CloseScene(uiScene, removeScene: true);
    }

    // Returns the first component of the specified type anywhere in the scene, including on inactive objects, or null when there is none.
    private static T FindComponent<T>(Scene scene) where T : Component
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var component = root.GetComponentInChildren<T>(includeInactive: true);

            if (component != null)
                return component;
        }

        return null;
    }

    // Rewrites the build settings scene list as the UI scene followed by every example scene.
    // Rewriting rather than merging means a deleted example drops out of the list without anyone having to remember to remove it.
    private static void UpdateBuildSettingsScenes(List<WorkshopManifest.ExampleItem> examples)
    {
        var scenes = new List<EditorBuildSettingsScene>();

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(UIScenePath) != null)
            scenes.Add(new EditorBuildSettingsScene(UIScenePath, true));
        else
            Debug.LogWarning($"[Workshop] UI scene '{UIScenePath}' was not found, so the build settings scene list starts with the examples instead.");

        foreach (var example in examples.Where(example => example.scenePath != UIScenePath))
            scenes.Add(new EditorBuildSettingsScene(example.scenePath, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // Returns the project's manifest asset, creating one at the default path when the project has none yet.
    // More than one manifest is ambiguous, so the first found is used and the rest are reported.
    private static WorkshopManifest FindOrCreateManifest()
    {
        var guids = AssetDatabase.FindAssets($"t:{nameof(WorkshopManifest)}");

        if (guids.Length > 1)
            Debug.LogWarning($"[Workshop] The project has {guids.Length} manifest assets; using '{AssetDatabase.GUIDToAssetPath(guids[0])}'.");

        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<WorkshopManifest>(AssetDatabase.GUIDToAssetPath(guids[0]));

        var manifest = ScriptableObject.CreateInstance<WorkshopManifest>();
        AssetDatabase.CreateAsset(manifest, DefaultManifestPath);

        return manifest;
    }
}
