using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(SceneBody))]
    [CanEditMultipleObjects]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.EditorSourceNamespace)]
    public class SceneBodyEditor : UnityEditor.Editor
    {
        private VisualElement m_ShowHideWorldDefinition;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneBody.BodyDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneBody.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneBody.CallbackTarget))));

            // Default World.
            var defaultWorldProperty = serializedObject.FindProperty(nameof(SceneBody.UseDefaultWorld));
            var defaultWorldPropertyField = new PropertyField(defaultWorldProperty);
            root.Add(defaultWorldPropertyField);

            m_ShowHideWorldDefinition = new VisualElement();
            defaultWorldPropertyField.RegisterValueChangeCallback(_ => { m_ShowHideWorldDefinition.style.display = defaultWorldProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex; });
            m_ShowHideWorldDefinition.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneBody.SceneWorld))));
            root.Add(m_ShowHideWorldDefinition);

            return root;
        }
    }
}