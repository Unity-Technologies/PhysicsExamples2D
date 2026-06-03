using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Makes the Sandbox the scene a new user starts from. Two parts:
///   1. The Play-mode start scene is (re)assigned the instant Play is pressed, so hitting Play
///      always boots the Sandbox regardless of which scene is open.
///   2. On the first editor load of a session (e.g. a fresh checkout that opened an empty Untitled
///      scene), the Sandbox scene is opened automatically so the user just sees it and can hit Play.
/// </summary>
[InitializeOnLoad]
public static class StartupScene
{
    private const string StartupScenePath = "Assets/Sandbox.unity";
    private const string AutoOpenedSessionKey = "StartupScene.AutoOpenedThisSession";

    static StartupScene()
    {
        // Re-assert the start scene the instant Play is pressed. At ExitingEditMode the
        // AssetDatabase is fully loaded, so the SceneAsset always resolves — unlike during the
        // initial import on a fresh checkout, when LoadAssetAtPath can still return null (the
        // original cause of "hitting Play doesn't start the Sandbox" on a new clone).
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        // Defer load-time work until the AssetDatabase has settled after the (re)compile/import.
        EditorApplication.delayCall += OnEditorLoaded;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            AssignStartScene();
    }

    private static void OnEditorLoaded()
    {
        // Keep the Play-mode start scene pointing at the Sandbox (so it also shows correctly in
        // Build Settings / the toolbar dropdown).
        AssignStartScene();

        // Only auto-open once per editor session, so this never stomps a scene the user opens
        // deliberately after the initial load.
        if (SessionState.GetBool(AutoOpenedSessionKey, false))
            return;
        SessionState.SetBool(AutoOpenedSessionKey, true);

        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        // Only step in when nothing real is open (an unsaved/Untitled scene has an empty path),
        // e.g. a fresh checkout with no remembered last scene. Don't override a scene the user
        // intentionally has open.
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
}
