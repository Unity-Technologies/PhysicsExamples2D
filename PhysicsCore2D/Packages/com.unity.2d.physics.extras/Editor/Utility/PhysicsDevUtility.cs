using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class PhysicsDevUtility
{
    [MenuItem("Tools/2D/Physics/Resave All Scenes", false, 2)]
    public static void ResaveScenes()
    {
        var allFiles = Directory.GetFiles(
            Application.dataPath, "*.unity", SearchOption.AllDirectories
        );

        for (var i = 0; i < allFiles.Length; ++i)
        {
            allFiles[i] = "Assets" + allFiles[i].Replace(Application.dataPath, "");
        }

        foreach (var file in allFiles)
        {
            EditorSceneManager.OpenScene(file);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), file);
        }
    }
}