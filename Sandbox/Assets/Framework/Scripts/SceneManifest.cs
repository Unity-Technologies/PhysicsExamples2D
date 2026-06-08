using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneManifest", menuName = "2D Physics/Scene Manifest")]
public class SceneManifest : ScriptableObject
{
    [Serializable]
    public struct SceneItem
    {
        public string Name;
        public string Category;
        public string Description;
        public string TypeName;
        public ExampleSceneData Data;
    }

    public List<SceneItem> SceneItems = new();

    public string LoadedSceneName { get; private set; }
    public string LoadedSceneDescription { get; private set; }

    private GameObject m_CurrentExampleGO;

    // Called by SandboxManager at play-session start to clear any stale runtime state.
    public void Initialize()
    {
        LoadedSceneName = string.Empty;
        LoadedSceneDescription = string.Empty;
        m_CurrentExampleGO = null;
    }

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

    public bool TryGetSceneItem(string sceneName, out SceneItem item)
    {
        var index = SceneItems.FindIndex(existing => existing.Name == sceneName);
        item = index >= 0 ? SceneItems[index] : default;
        return index >= 0;
    }

    public void LoadScene(string sceneName, Action action)
    {
        if (sceneName == LoadedSceneName)
            return;

        // Disable the departing example — OnDisable fires immediately on SetActive(false).
        if (m_CurrentExampleGO != null)
        {
            m_CurrentExampleGO.SetActive(false);
            UnityEngine.Object.Destroy(m_CurrentExampleGO);
            m_CurrentExampleGO = null;
        }

        // Physics reset.
        action?.Invoke();

        // Resolve target item and set loaded state before OnEnable fires.
        var index = FindItemIndex(sceneName);
        var item = SceneItems[index];
        LoadedSceneName = item.Name;
        LoadedSceneDescription = item.Description;

        // Create the arriving example inactive so ExampleData can be set before OnEnable fires.
        m_CurrentExampleGO = new GameObject(item.Name);
        m_CurrentExampleGO.SetActive(false);
        var example = (SandboxExampleBehaviour)m_CurrentExampleGO.AddComponent(Type.GetType(item.TypeName));
        if (example != null)
            example.ExampleData = item.Data;
        m_CurrentExampleGO.SetActive(true);
    }

    public void ReloadCurrentScene(Action action)
    {
        if (string.IsNullOrEmpty(LoadedSceneName))
            return;

        var current = LoadedSceneName;
        LoadedSceneName = string.Empty;   // bypass same-name guard in LoadScene
        LoadScene(current, action);
    }

    private int FindItemIndex(string sceneName)
    {
        var index = SceneItems.FindIndex(item => item.Name == sceneName);
        if (index >= 0)
            return index;

        Debug.LogWarning($"[SceneManifest] Example '{sceneName}' not found — falling back to the first example.");
        return 0;
    }
}
