using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor
{
    /// <summary>
    /// Regenerates the example registry from code: every type marked with
    /// <see cref="ExampleSceneAttribute"/> is upserted into the <see cref="SceneManifest"/>
    /// list in Sandbox.unity and into the build settings. This removes the two manual
    /// registration steps that adding an example used to require.
    ///
    /// The operation is an upsert (match by scene path): it adds or updates entries for
    /// attributed examples and leaves any other entries alone, so it is safe to run while only
    /// some examples have been migrated to <see cref="SandboxExampleBehaviour"/>.
    /// </summary>
    public static class ExampleRegistryBuilder
    {
        private const string SandboxScenePath = "Assets/Sandbox.unity";

        [MenuItem("Tools/2D/Physics/Rebuild Sandbox Registry", false, 1)]
        public static void Rebuild()
        {
            // Discover every attributed example and resolve its scene.
            var discovered = new List<SceneManifest.SceneItem>();
            foreach (var type in TypeCache.GetTypesWithAttribute<ExampleSceneAttribute>())
            {
                if (!typeof(SandboxExampleBehaviour).IsAssignableFrom(type))
                {
                    Debug.LogWarning($"[ExampleRegistry] '{type.Name}' has [ExampleScene] but does not derive from SandboxExampleBehaviour. Skipping.");
                    continue;
                }

                var scriptPath = FindScriptPath(type);
                if (scriptPath == null)
                {
                    Debug.LogWarning($"[ExampleRegistry] Could not locate the script asset for '{type.Name}'. Skipping.");
                    continue;
                }

                var folder = Path.GetDirectoryName(scriptPath)?.Replace('\\', '/');
                var scenePath = $"{folder}/{type.Name}.unity";
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    Debug.LogWarning($"[ExampleRegistry] '{type.Name}' has no sibling scene at '{scenePath}'. Skipping.");
                    continue;
                }

                var attribute = (ExampleSceneAttribute)Attribute.GetCustomAttribute(type, typeof(ExampleSceneAttribute));
                discovered.Add(new SceneManifest.SceneItem
                {
                    Name = ToDisplayName(type.Name),
                    Category = attribute.Category,
                    Description = attribute.Description,
                    ScenePath = scenePath
                });
            }

            UpsertManifest(discovered, out var added, out var updated, out var removed);
            UpsertBuildSettings(discovered, out var buildAdded, out var buildRemoved);

            Debug.Log($"[ExampleRegistry] Discovered {discovered.Count} example(s): {added} added, {updated} updated, {removed} removed in the manifest; {buildAdded} added, {buildRemoved} removed in build settings.");
        }

        // Add/update manifest entries for the discovered examples, matching by scene path.
        // Also prunes any example entry (under Assets/Scenes/) no longer claimed by an
        // [ExampleScene] attribute, so commenting/removing the attribute disables the example and
        // moved/deleted scenes don't leave dead entries. If the pruned set includes the configured
        // start scene, that reference is reset so play-mode startup can't throw.
        private static void UpsertManifest(List<SceneManifest.SceneItem> discovered, out int added, out int updated, out int removed)
        {
            added = 0;
            updated = 0;
            removed = 0;

            var openedAdditively = false;
            var scene = SceneManager.GetSceneByPath(SandboxScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Additive);
                openedAdditively = true;
            }

            var manifest = FindComponent<SceneManifest>(scene);
            if (manifest == null)
            {
                Debug.LogError($"[ExampleRegistry] No SceneManifest found in {SandboxScenePath}.");
                if (openedAdditively)
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                return;
            }

            var items = manifest.SceneItems ?? new List<SceneManifest.SceneItem>();
            foreach (var entry in discovered)
            {
                var index = items.FindIndex(existing => existing.ScenePath == entry.ScenePath);
                if (index >= 0)
                {
                    items[index] = entry;
                    ++updated;
                }
                else
                {
                    items.Add(entry);
                    ++added;
                }
            }

            // Prune example entries (under Assets/Scenes/) no longer claimed by an [ExampleScene]
            // attribute — removing/commenting the attribute is the disable switch. A moved or
            // deleted scene is naturally absent from the discovered set too, so this also covers
            // folder moves/renames.
            var discoveredPaths = new HashSet<string>(discovered.Select(d => d.ScenePath));
            removed = items.RemoveAll(item =>
                !string.IsNullOrEmpty(item.ScenePath) &&
                item.ScenePath.StartsWith("Assets/Scenes/") &&
                !discoveredPaths.Contains(item.ScenePath));

            manifest.SceneItems = items;
            EditorUtility.SetDirty(manifest);

            // Repair a now-dangling start scene: SandboxManager.StartScene stores a scene Name and
            // is resolved at startup via a throwing First(...). If it points at a name that no
            // longer exists (e.g. the example was just disabled), reset it to empty so startup
            // falls back to the first scene instead of throwing.
            var sandboxManager = FindComponent<SandboxManager>(scene);
            if (sandboxManager != null &&
                !string.IsNullOrEmpty(sandboxManager.StartScene) &&
                items.All(item => item.Name != sandboxManager.StartScene))
            {
                Debug.LogWarning($"[ExampleRegistry] Start scene '{sandboxManager.StartScene}' is no longer registered; resetting it (startup will use the first scene). Set a new Start Scene on the SandboxManager if desired.");
                sandboxManager.StartScene = string.Empty;
                EditorUtility.SetDirty(sandboxManager);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            if (openedAdditively)
                EditorSceneManager.CloseScene(scene, removeScene: true);
        }

        // Ensure every discovered scene (and Sandbox.unity itself) is in the enabled build list.
        // Also prunes any example build entry (under Assets/Scenes/) no longer claimed by an
        // [ExampleScene] attribute, mirroring the manifest prune.
        private static void UpsertBuildSettings(List<SceneManifest.SceneItem> discovered, out int added, out int removed)
        {
            added = 0;
            removed = 0;

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var present = new HashSet<string>();
            foreach (var s in scenes)
                present.Add(s.path);

            // The startup scene must be present (kept wherever it already is, or added first).
            if (!present.Contains(SandboxScenePath))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(SandboxScenePath, true));
                present.Add(SandboxScenePath);
            }

            foreach (var entry in discovered)
            {
                if (present.Add(entry.ScenePath))
                {
                    scenes.Add(new EditorBuildSettingsScene(entry.ScenePath, true));
                    ++added;
                }
            }

            // Prune example build entries (under Assets/Scenes/) no longer claimed by an
            // [ExampleScene] attribute, mirroring the manifest prune.
            var discoveredPaths = new HashSet<string>(discovered.Select(d => d.ScenePath));
            removed = scenes.RemoveAll(s =>
                !string.IsNullOrEmpty(s.path) &&
                s.path.StartsWith("Assets/Scenes/") &&
                !discoveredPaths.Contains(s.path));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static string FindScriptPath(Type type)
        {
            foreach (var guid in AssetDatabase.FindAssets($"{type.Name} t:MonoScript"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript != null && monoScript.GetClass() == type)
                    return path;
            }

            return null;
        }

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

        // "CharacterMover" -> "Character Mover", matching SceneItemsPropertyDrawer's formatting.
        private static string ToDisplayName(string typeName) =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Regex.Replace(typeName, "(\\B[A-Z])", " $1"));
    }
}
