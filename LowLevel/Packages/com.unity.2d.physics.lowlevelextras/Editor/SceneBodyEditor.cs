using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneBody))]
    [CanEditMultipleObjects]
    public class SceneBodyEditor : Editor
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