using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// Draws the manager's inspector with the start example as a dropdown of the registered example names, sorted alphabetically.
[CustomEditor(typeof(WorkshopManager), true)]
internal sealed class WorkshopManagerEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        var manifest = serializedObject.FindProperty("m_Manifest")?.objectReferenceValue as WorkshopManifest;
        var exampleNames = manifest?.examples.Select(example => example.exampleName).ToList() ?? new List<string>();
        exampleNames.Sort();
        root.Add(new DropdownField { label = m_StartSceneProperty.displayName, choices = exampleNames, bindingPath = m_StartSceneProperty.propertyPath });

        InspectorElement.FillDefaultInspector(root, serializedObject, this, m_StartSceneProperty.propertyPath);

        return root;
    }

    private void OnEnable()
    {
        m_StartSceneProperty = serializedObject.FindProperty(nameof(WorkshopManager.StartScene));
    }

    #region Internal

    private SerializedProperty m_StartSceneProperty;

    #endregion
}
