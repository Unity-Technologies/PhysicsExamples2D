using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Makes the shooter the scene a new user starts from.
/// Pressing Play always boots the shooter, whichever scene is open.
/// The first time the editor loads in a session with no saved scene open, such as a fresh checkout showing an empty Untitled scene, the shooter scene opens automatically.
/// </summary>
[InitializeOnLoad]
public static class StartupScene
{
    static StartupScene()
    {
        // The start scene is assigned again the moment Play is pressed.
        // The asset database is fully loaded by then, so the scene always resolves, unlike during the first import of a fresh checkout when the load can still return null.
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        // Load-time work waits until the asset database has settled after the compile and import.
        EditorApplication.delayCall += OnEditorLoaded;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            AssignStartScene();
    }

    private static void OnEditorLoaded()
    {
        // The Play mode start scene keeps pointing at the shooter, so it also shows correctly in the toolbar dropdown.
        AssignStartScene();

        // The scene opens only once per editor session, so it never replaces a scene the user opens deliberately after the initial load.
        if (SessionState.GetBool(AutoOpenedSessionKey, false))
            return;

        SessionState.SetBool(AutoOpenedSessionKey, true);

        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        // An unsaved scene has an empty path, so only that case is replaced.
        // A scene the user intentionally has open is left alone.
        var activeScene = SceneManager.GetActiveScene();

        if (!string.IsNullOrEmpty(activeScene.path))
            return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StartupScenePath) != null)
            EditorSceneManager.OpenScene(StartupScenePath, OpenSceneMode.Single);
    }

    private static void AssignStartScene()
    {
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartupScenePath);

        if (scene != null)
            EditorSceneManager.playModeStartScene = scene;
    }

    #region Internal

    private const string StartupScenePath = "Assets/Shooter.unity";
    private const string AutoOpenedSessionKey = "StartupScene.AutoOpenedThisSession";

    #endregion
}
