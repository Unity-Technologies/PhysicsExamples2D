using System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// The list of every Toolbox example available at runtime, grouped into menu categories.
/// The UI scene holds a reference to one of these and reads it to build the example menu.
/// </summary>
/// <remarks>
/// This asset is generated, not hand edited: the registry tool rewrites its contents from the <see cref="ToolboxExampleInfo"/> assets it finds.
/// </remarks>
[CreateAssetMenu(fileName = "ToolboxManifest", menuName = "2D Physics/Toolbox Manifest")]
public sealed class ToolboxManifest : ScriptableObject
{
    /// <summary>
    /// One example's menu entry, flattened from its <see cref="ToolboxExampleInfo"/> so the runtime needs no editor only types.
    /// </summary>
    [Serializable]
    public struct ExampleItem
    {
        /// <summary>
        /// The name the menu shows for this example.
        /// </summary>
        public string exampleName;

        /// <summary>
        /// The menu group this example is listed under.
        /// </summary>
        public string category;

        /// <summary>
        /// A one sentence explanation of what the example demonstrates.
        /// </summary>
        public string description;

        /// <summary>
        /// Project relative path of the scene to load, for example "Assets/Examples/Hinge/Hinge.unity".
        /// </summary>
        public string scenePath;
    }

    /// <summary>
    /// Returns every category name that has at least one example, sorted alphabetically.
    /// </summary>
    public List<string> GetCategories()
    {
        var seen = new HashSet<string>();
        var categories = new List<string>();

        foreach (var item in m_Examples)
        {
            if (seen.Add(item.category))
                categories.Add(item.category);
        }

        categories.Sort(StringComparer.OrdinalIgnoreCase);

        return categories;
    }

    /// <summary>
    /// Returns the names of every example in the specified category, sorted alphabetically.
    /// Returns an empty list when the category holds no examples.
    /// </summary>
    public List<string> GetExampleNames(string category)
    {
        var exampleNames = new List<string>();

        foreach (var item in m_Examples)
        {
            if (item.category == category)
                exampleNames.Add(item.exampleName);
        }

        exampleNames.Sort(StringComparer.OrdinalIgnoreCase);

        return exampleNames;
    }

    /// <summary>
    /// Finds the example with the specified name.
    /// Returns false when no example carries that name, leaving the result at its default value.
    /// </summary>
    public bool TryGetExample(string exampleName, out ExampleItem item)
    {
        var index = m_Examples.FindIndex(existing => existing.exampleName == exampleName);
        item = index >= 0 ? m_Examples[index] : default;

        return index >= 0;
    }

    /// <summary>
    /// Every example in the manifest, in the order the registry tool wrote them.
    /// </summary>
    public IReadOnlyList<ExampleItem> examples => m_Examples;

#if UNITY_EDITOR

    // Replaces the whole example list.
    // Only the registry tool calls this, since the manifest is generated rather than hand edited.
    internal void SetExamples(List<ExampleItem> examples)
    {
        m_Examples = examples;
        UnityEditor.EditorUtility.SetDirty(this);
    }

#endif

    #region Internal

    [SerializeField] List<ExampleItem> m_Examples = new();

    #endregion
}
