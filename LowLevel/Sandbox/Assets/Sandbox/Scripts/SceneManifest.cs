using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManifest : MonoBehaviour
{
    [Serializable]
    public struct SceneItem
    {
        public string ScenePath;
        public string Name;
        public string Category;
        public string Description;
    }
    
    public List<SceneItem> SceneItems = new();
    
    public int LoadedSceneIndex { get; private set; }
    public string LoadedSceneName { get; private set; }
    public string LoadedSceneDescription { get; private set; }

    private Coroutine m_LoadSceneRoutine;
    
    public List<string> GetCategories() => SceneItems.Select(item => item.Category).Distinct().ToList();
    public List<string> GetScenes(string category) => SceneItems.Where(item => item.Category == category).Select(item => item.Name).ToList();
    public SceneItem GetSceneItem(string sceneName) => SceneItems.Select((item) => new { Item = item }).First(x => x.Item.Name == sceneName).Item;

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

        // Deactivate the sample camera.
        var sampleCamera = GameObject.Find("SampleCamera");
        if (sampleCamera)
            sampleCamera.SetActive(false);
        
        // Indicate we're done.
        m_LoadSceneRoutine = null;
    }

    private int FindSceneIndex(string sceneName) => SceneItems.Select((item, i) => new { Item = item, Index = i }).First(x => x.Item.Name == sceneName).Index;
    
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