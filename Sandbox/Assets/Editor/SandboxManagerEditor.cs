using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEditor
{
    [CustomEditor(typeof(SandboxManager), true)]
    public class SandboxManagerEditor : Editor
    {
        private SerializedProperty m_StartSceneNameProperty;

        private void OnEnable()
        {
            m_StartSceneNameProperty = serializedObject.FindProperty(nameof(SandboxManager.StartScene));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Populate the Start Scene dropdown from the SceneManifest asset reference.
            var manifestProp = serializedObject.FindProperty("m_SceneManifest");
            var manifest = manifestProp?.objectReferenceValue as SceneManifest;
            var sceneNames = manifest?.SceneItems.Select(item => item.Name).ToList() ?? new System.Collections.Generic.List<string>();
            sceneNames.Sort();
            root.Add(new DropdownField { label = m_StartSceneNameProperty.displayName, choices = sceneNames, bindingPath = m_StartSceneNameProperty.propertyPath });

            InspectorElement.FillDefaultInspector(root, serializedObject, this, m_StartSceneNameProperty.propertyPath);

            return root;
        }
    }
}
