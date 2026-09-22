using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

// Draws the manager's inspector with the start example as a dropdown of the registered examples, rather than a name typed by hand.
[CustomEditor(typeof(ToolboxManager))]
internal sealed class ToolboxManagerEditor : Editor
{
    // The label shown when no start example is chosen, which means the first registered example is used.
    private const string NoneLabel = "<First registered>";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var startScene = serializedObject.FindProperty(nameof(ToolboxManager.StartScene));
        var manifest = serializedObject.FindProperty("m_Manifest").objectReferenceValue as ToolboxManifest;

        DrawStartExample(startScene, manifest);

        // Everything else keeps the default inspector, minus the two properties drawn above.
        DrawPropertiesExcluding(serializedObject, "m_Script", nameof(ToolboxManager.StartScene));

        serializedObject.ApplyModifiedProperties();
    }

    // Draws the start example as a dropdown built from the manifest, falling back to a plain text field when there is no manifest to list.
    private static void DrawStartExample(SerializedProperty startScene, ToolboxManifest manifest)
    {
        if (manifest == null || manifest.examples.Count == 0)
        {
            EditorGUILayout.PropertyField(startScene, new GUIContent("Start Example"));
            EditorGUILayout.HelpBox("Assign a manifest and rebuild the registry to pick a start example from a list.", MessageType.None);

            return;
        }

        var names = new List<string> { NoneLabel };
        names.AddRange(manifest.examples.Select(example => example.exampleName));

        var currentIndex = names.IndexOf(startScene.stringValue);

        // A name that is no longer registered would otherwise vanish silently, so it is kept in the list and marked.
        if (currentIndex < 0)
        {
            if (string.IsNullOrEmpty(startScene.stringValue))
            {
                currentIndex = 0;
            }
            else
            {
                names.Add($"{startScene.stringValue} (missing)");
                currentIndex = names.Count - 1;
            }
        }

        var newIndex = EditorGUILayout.Popup(new GUIContent("Start Example", "The example loaded when Play starts."), currentIndex, names.ToArray());

        if (newIndex == currentIndex)
            return;

        startScene.stringValue = newIndex == 0 ? string.Empty : names[newIndex];
    }
}
