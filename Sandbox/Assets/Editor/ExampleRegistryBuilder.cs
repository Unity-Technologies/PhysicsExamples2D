using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// <see cref="ExampleSceneAttribute"/> and deriving from <see cref="SandboxExampleBehaviour"/>
    /// is upserted into the <see cref="SceneManifest"/> asset. No scene files or build settings
    /// entries are required.
    /// </summary>
    public static class ExampleRegistryBuilder
    {
        private const string SandboxScenePath = "Assets/Sandbox.unity";

        [MenuItem("Tools/2D/Physics/Rebuild Sandbox Registry", false, 1)]
        public static void Rebuild()
        {
            // Discover every attributed example type.
            var discovered = new List<SceneManifest.SceneItem>();
            foreach (var type in TypeCache.GetTypesWithAttribute<ExampleSceneAttribute>())
            {
                if (!typeof(SandboxExampleBehaviour).IsAssignableFrom(type))
                {
                    Debug.LogWarning($"[ExampleRegistry] '{type.Name}' has [ExampleScene] but does not derive from SandboxExampleBehaviour. Skipping.");
                    continue;
                }

                var attribute = (ExampleSceneAttribute)Attribute.GetCustomAttribute(type, typeof(ExampleSceneAttribute));
                discovered.Add(new SceneManifest.SceneItem
                {
                    Name = ToDisplayName(type.Name),
                    Category = attribute.Category,
                    Description = attribute.Description,
                    TypeName = type.AssemblyQualifiedName,
                    Data = FindCompanionData(type)
                });
            }

            UpsertManifest(discovered, out var added, out var updated, out var removed);
            EnsureSandboxSceneInBuild(out var buildCleaned);

            Debug.Log($"[ExampleRegistry] Discovered {discovered.Count} example(s): {added} added, {updated} updated, {removed} removed in the manifest.{(buildCleaned ? " Cleaned stale Assets/Scenes/ entries from build settings." : "")}");
        }

        // Add/update manifest entries for the discovered examples, matching by TypeName.
        // Prunes entries whose TypeName is no longer claimed by any [ExampleScene] attribute.
        private static void UpsertManifest(List<SceneManifest.SceneItem> discovered, out int added, out int updated, out int removed)
        {
            added = 0;
            updated = 0;
            removed = 0;

            var manifest = FindManifestAsset();
            if (manifest == null)
            {
                Debug.LogError("[ExampleRegistry] No SceneManifest asset found in the project. Create one via Assets > Create > 2D Physics > Scene Manifest.");
                return;
            }

            var items = manifest.SceneItems ?? new List<SceneManifest.SceneItem>();
            foreach (var entry in discovered)
            {
                var index = items.FindIndex(existing => existing.TypeName == entry.TypeName);
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

            // Prune entries no longer claimed by any [ExampleScene] attribute.
            var discoveredTypeNames = new HashSet<string>(discovered.Select(d => d.TypeName));
            removed = items.RemoveAll(item =>
                !string.IsNullOrEmpty(item.TypeName) &&
                !discoveredTypeNames.Contains(item.TypeName));

            manifest.SceneItems = items;
            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssetIfDirty(manifest);

            // If SandboxManager.StartScene points at a name that no longer exists, reset it.
            var sandboxScene = SceneManager.GetSceneByPath(SandboxScenePath);
            var openedAdditively = false;
            if (!sandboxScene.IsValid() || !sandboxScene.isLoaded)
            {
                sandboxScene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Additive);
                openedAdditively = true;
            }

            var sandboxManager = FindComponent<SandboxManager>(sandboxScene);
            if (sandboxManager != null &&
                !string.IsNullOrEmpty(sandboxManager.StartScene) &&
                items.All(item => item.Name != sandboxManager.StartScene))
            {
                Debug.LogWarning($"[ExampleRegistry] Start scene '{sandboxManager.StartScene}' is no longer registered; resetting it.");
                sandboxManager.StartScene = string.Empty;
                EditorUtility.SetDirty(sandboxManager);
                EditorSceneManager.MarkSceneDirty(sandboxScene);
                EditorSceneManager.SaveScene(sandboxScene);
            }

            if (openedAdditively)
                EditorSceneManager.CloseScene(sandboxScene, removeScene: true);
        }

        // Ensure Sandbox.unity is in build settings and prune any stale Assets/Scenes/ entries.
        private static void EnsureSandboxSceneInBuild(out bool cleaned)
        {
            cleaned = false;
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            var staleCount = scenes.RemoveAll(s =>
                !string.IsNullOrEmpty(s.path) &&
                s.path.StartsWith("Assets/Scenes/"));
            cleaned = staleCount > 0;

            if (scenes.All(s => s.path != SandboxScenePath))
                scenes.Insert(0, new EditorBuildSettingsScene(SandboxScenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // Find the companion ExampleSceneData asset for a type by convention: looks for a
        // ScriptableObject whose name matches "{TypeName}Data" anywhere in the project.
        private static ExampleSceneData FindCompanionData(Type type)
        {
            var dataTypeName = $"{type.Name}Data";
            foreach (var guid in AssetDatabase.FindAssets($"{dataTypeName} t:ScriptableObject"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ExampleSceneData>(path);
                if (asset != null && asset.GetType().Name == dataTypeName)
                    return asset;
            }
            return null;
        }

        private static SceneManifest FindManifestAsset()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:SceneManifest"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SceneManifest>(path);
                if (asset != null)
                    return asset;
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

        // "CharacterMover" -> "Character Mover"
        private static string ToDisplayName(string typeName) =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Regex.Replace(typeName, "(\\B[A-Z])", " $1"));
    }
}
