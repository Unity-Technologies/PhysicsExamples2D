using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PhysicsDevUtility
{
    [MenuItem("Physics-Dev/Resave All Scenes")]
    public static void ResaveScenes()
    {
        String[] allFiles = Directory.GetFiles(
            Application.dataPath, "*.unity", SearchOption.AllDirectories
        );

        for (int i=0; i<allFiles.Length; ++i)
        {
            allFiles[i] = "Assets" + allFiles[i].Replace(Application.dataPath, "");
        }

        foreach(var file in allFiles)
        {
            EditorSceneManager.OpenScene(file);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), file);
        }
    }
}
