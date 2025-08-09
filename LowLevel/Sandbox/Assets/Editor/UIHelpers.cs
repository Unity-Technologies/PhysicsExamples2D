using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public static class UIHelpers
{
    public static void FillDefaultInspector(VisualElement container, SerializedObject serializedObject, Editor editor, params string[] names)
    {
        // Fetch the full set of properties.
        InspectorElement.FillDefaultInspector(container, serializedObject, editor);

        // Finish if no removals specified.
        if (names == null || names.Length == 0)
            return;

        // Remove named properties.
        var removals = new List<VisualElement>();
        FindRemovedProperties(container, removals, names);

        // Remove Properties.
        foreach (var property in removals)
            property.RemoveFromHierarchy();
    }

    private static void FindRemovedProperties(VisualElement container, List<VisualElement> removals, params string[] names)
    {
        // Is this a named property?
        if (container is PropertyField field && names.Contains(field.bindingPath))
        {
            // Yes, so add to removals.
            removals.Add(container);

            return;
        }

        // Search children.
        foreach (var element in container.Children())
            FindRemovedProperties(element, removals, names);
    }
}