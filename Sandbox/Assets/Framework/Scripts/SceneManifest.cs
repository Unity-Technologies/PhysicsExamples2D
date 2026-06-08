using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManifest : MonoBehaviour
{
    [Serializable]
    public struct SceneItem
    {
        public string Name;
        public string Category;
        public string Description;
        public string ScenePath;
    }

    public List<SceneItem> SceneItems = new();

    public string LoadedSceneName { get; private set; }
    public string LoadedSceneDescription { get; private set; }
    private int LoadedSceneIndex { get; set; }

    private Coroutine m_LoadSceneRoutine;

    public List<string> GetCategories()
    {
        var seen = new HashSet<string>();
        var result = new List<string>();
        foreach (var item in SceneItems)
        {
            if (seen.Add(item.Category))
                result.Add(item.Category);
        }
        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    public List<string> GetScenes(string category)
    {
        var result = new List<string>();
        foreach (var item in SceneItems)
        {
            if (item.Category == category)
                result.Add(item.Name);
        }
        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }
    // Look up a scene item by Name. Returns false (and a default item) if no match — callers
    // should handle the missing case rather than assume the name is always valid.
    public bool TryGetSceneItem(string sceneName, out SceneItem item)
    {
        var index = SceneItems.FindIndex(existing => existing.Name == sceneName);
        item = index >= 0 ? SceneItems[index] : default;
        return index >= 0;
    }

    private void OnEnable()
    {
        LoadedSceneIndex = -1;
        LoadedSceneName = string.Empty;
        LoadedSceneDescription = string.Empty;
    }

    public void LoadScene(string sceneName, Action action)
    {
        if (m_LoadSceneRoutine != null)
            return;

        m_LoadSceneRoutine = StartCoroutine(LoadSceneCoroutine(sceneName, action));
    }

    public void ReloadCurrentScene(Action action)
    {
        if (LoadedSceneName == string.Empty)
            return;

        LoadScene(LoadedSceneName, action);
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, Action action)
    {
        var sceneIndex = FindSceneIndex(sceneName);

        // Unload any previous scene.
        if (LoadedSceneIndex != -1)
        {
            var operation = UnloadSceneIndex(LoadedSceneIndex);
            while (!operation.isDone)
                yield return null;
        }

        // Call the action.
        action();

        // Load the new scene.
        {
            LoadedSceneIndex = sceneIndex;
            LoadedSceneName = sceneName;
            LoadedSceneDescription = SceneItems[sceneIndex].Description;
            var operation = LoadSceneIndex(LoadedSceneIndex, LoadSceneMode.Additive);
            while (!operation.isDone)
                yield return null;
        }

        // Remove any Camera present in the example scene — cameras are managed globally
        // by CameraManipulator in Sandbox.unity and must not exist in example scenes.
        var loadedScene = SceneManager.GetSceneByPath(SceneItems[LoadedSceneIndex].ScenePath);
        foreach (var camera in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (camera.gameObject.scene == loadedScene)
                Destroy(camera.gameObject);
        }

        // Indicate we're done.
        m_LoadSceneRoutine = null;
    }

    private int FindSceneIndex(string sceneName)
    {
        var index = SceneItems.FindIndex(item => item.Name == sceneName);
        if (index >= 0)
            return index;

        Debug.LogWarning($"[SceneManifest] Scene '{sceneName}' not found — falling back to the first scene.");
        return 0;
    }

    private AsyncOperation LoadSceneIndex(int sceneIndex, LoadSceneMode loadSceneMode)
    {
        var buildIndex = SceneUtility.GetBuildIndexByScenePath(SceneItems[sceneIndex].ScenePath);
        if (buildIndex != -1)
            return SceneManager.LoadSceneAsync(buildIndex, loadSceneMode);

        throw new ArgumentException("Invalid Scene Index", nameof(sceneIndex));
    }

    private AsyncOperation UnloadSceneIndex(int sceneIndex)
    {
        var buildIndex = SceneUtility.GetBuildIndexByScenePath(SceneItems[sceneIndex].ScenePath);
        if (buildIndex != -1)
            return SceneManager.UnloadSceneAsync(buildIndex);

        throw new ArgumentException("Invalid Scene Index", nameof(sceneIndex));
    }
}