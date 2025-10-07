using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class StartupScene
{
    static StartupScene()
    {
        const string sceneStartUpName = "Assets/Sandbox.unity";
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneStartUpName);
    }
}
