using System.Linq;
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
            UIHelpers.FillDefaultInspector(root, serializedObject, this, m_StartSceneNameProperty.propertyPath);
            
            // Add Start Scene.
            var sandboxManager = (target as SandboxManager);
            var sceneItems = sandboxManager.GetComponent<SceneManifest>().SceneItems;
            var sceneNames = sceneItems.Select(item => item.Name).ToList();
            var sceneNamesField = new DropdownField { label = m_StartSceneNameProperty.displayName, choices = sceneNames, bindingPath = m_StartSceneNameProperty.propertyPath };
            root.Add(sceneNamesField);
            
            return root;
        }

    }
}