using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Describes a single Toolbox example: the scene it loads and how the menu presents it.
/// One of these assets lives inside each example's own folder under Assets/Examples, so an example is fully self-contained.
/// </summary>
/// <remarks>
/// The registry tool discovers every asset of this type, writes them into the runtime manifest, and keeps the build settings scene list in step.
/// See <see cref="ToolboxManifest"/> for the runtime side.
/// </remarks>
[CreateAssetMenu(fileName = "ExampleInfo", menuName = "2D Physics/Toolbox Example Info")]
public sealed class ToolboxExampleInfo : ScriptableObject
{
    /// <summary>
    /// The name the menu shows for this example, written here alongside its description.
    /// </summary>
    public string exampleName => m_ExampleName;

    /// <summary>
    /// The menu group this example is listed under, for example "Constraints" or "Areas".
    /// Examples sharing a category name appear together, whatever folder each one lives in.
    /// </summary>
    public string category => m_Category;

    /// <summary>
    /// A one sentence explanation of what the example demonstrates, shown in the menu's description panel.
    /// </summary>
    public string description => m_Description;

    /// <summary>
    /// Project relative path of the scene this example loads, for example "Assets/Examples/Hinge/Hinge.unity".
    /// This is the value the runtime uses, so it stays serialized in a player build where the editor only scene reference cannot.
    /// </summary>
    public string scenePath => m_ScenePath;

#if UNITY_EDITOR

    // Copies the assigned scene's path into the serialized string the runtime reads.
    // The scene reference itself is an editor only type, so the path is what survives into a build.
    internal void SyncScenePath()
    {
        var path = m_Scene != null ? AssetDatabase.GetAssetPath(m_Scene) : string.Empty;

        if (m_ScenePath == path)
            return;

        m_ScenePath = path;
        EditorUtility.SetDirty(this);
    }

    // Keeps the path in step while the asset is edited in the Inspector, so the registry tool is not the only way to fix it up.
    private void OnValidate() => SyncScenePath();

#endif

    #region Internal

#if UNITY_EDITOR
    [SerializeField] SceneAsset m_Scene;
#endif
    [SerializeField, HideInInspector] string m_ScenePath;
    [SerializeField] string m_ExampleName;
    [SerializeField] string m_Category = "Uncategorized";
    [SerializeField, TextArea] string m_Description;

    #endregion
}
