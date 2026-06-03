using System.Globalization;
using System.Text.RegularExpressions;
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
            const string scenePathTypeName = nameof(SceneManifest.SceneItem.ScenePath);

            var nameProperty = property.FindPropertyRelative(nameTypeName);
            var categoryProperty = property.FindPropertyRelative(categoryTypeName);
            var descriptionProperty = property.FindPropertyRelative(descriptionTypeName);
            var scenePathProperty = property.FindPropertyRelative(scenePathTypeName);

            // Name.
            var namePropertyField = new TextField { label = nameTypeName, bindingPath = nameProperty.propertyPath, enabledSelf = false }; 
            foldout.Add(namePropertyField);
            
            // Category.
            foldout.Add(new PropertyField(categoryProperty, categoryTypeName));
            
            // Description.
            foldout.Add(new PropertyField(descriptionProperty, descriptionTypeName));

            // Scene Path.
            var scenePathPropertyField = new TextField { label = scenePathTypeName, bindingPath = scenePathProperty.propertyPath, enabledSelf = false };
            foldout.Add(scenePathPropertyField);

            // Provide the ability to drag a Unity Scene here.
            var unitySceneField = new ObjectField("Unity Scene") { value = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProperty.stringValue) };
            unitySceneField.RegisterValueChangedCallback(evt =>
            {
                namePropertyField.value = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Regex.Replace(evt.newValue.name, "(\\B[A-Z])", " $1"));
                scenePathPropertyField.value = AssetDatabase.GetAssetPath(evt.newValue);
            });
            foldout.Add(unitySceneField);
            
            return root;
        }
    }
}