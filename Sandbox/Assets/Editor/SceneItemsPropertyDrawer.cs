using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(SceneManifest.SceneItem))]
    internal sealed class SceneItemsPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var foldout = new Foldout { text = property.displayName, value = false };
            root.Add(foldout);

            const string nameTypeName = nameof(SceneManifest.SceneItem.Name);
            const string categoryTypeName = nameof(SceneManifest.SceneItem.Category);
            const string descriptionTypeName = nameof(SceneManifest.SceneItem.Description);
            const string typeNameField = nameof(SceneManifest.SceneItem.TypeName);
            const string dataField = nameof(SceneManifest.SceneItem.Data);

            var nameProperty = property.FindPropertyRelative(nameTypeName);
            var categoryProperty = property.FindPropertyRelative(categoryTypeName);
            var descriptionProperty = property.FindPropertyRelative(descriptionTypeName);
            var typeNameProperty = property.FindPropertyRelative(typeNameField);
            var dataProperty = property.FindPropertyRelative(dataField);

            // Name (read-only — set by ExampleRegistryBuilder).
            foldout.Add(new TextField { label = nameTypeName, bindingPath = nameProperty.propertyPath, enabledSelf = false });

            // Category.
            foldout.Add(new PropertyField(categoryProperty, categoryTypeName));

            // Description.
            foldout.Add(new PropertyField(descriptionProperty, descriptionTypeName));

            // TypeName (read-only — set by ExampleRegistryBuilder).
            foldout.Add(new TextField { label = typeNameField, bindingPath = typeNameProperty.propertyPath, enabledSelf = false });

            // Data (optional companion asset — auto-discovered by ExampleRegistryBuilder).
            foldout.Add(new PropertyField(dataProperty, dataField));

            return root;
        }
    }
}
