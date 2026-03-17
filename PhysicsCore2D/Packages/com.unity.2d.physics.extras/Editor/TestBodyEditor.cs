using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(TestBody))]
    [CanEditMultipleObjects]
    public class TestBodyEditor : UnityEditor.Editor
    {
        private VisualElement m_ShowHideWorldDefinition;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestBody.BodyDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestBody.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestBody.CallbackTarget))));

            // Use Transform Pose.
            var useTransformPoseProperty = serializedObject.FindProperty(nameof(TestBody.UseTransformPose));
            var useTransformPosePropertyField = new PropertyField(useTransformPoseProperty);
            root.Add(useTransformPosePropertyField);
            
            // Default World.
            var defaultWorldProperty = serializedObject.FindProperty(nameof(TestBody.UseDefaultWorld));
            var defaultWorldPropertyField = new PropertyField(defaultWorldProperty);
            root.Add(defaultWorldPropertyField);
            
            // World definition.
            m_ShowHideWorldDefinition = new VisualElement();
            defaultWorldPropertyField.RegisterValueChangeCallback(_ => { m_ShowHideWorldDefinition.style.display = defaultWorldProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex; });
            m_ShowHideWorldDefinition.Add(new PropertyField(serializedObject.FindProperty(nameof(TestBody.testWorld))));
            root.Add(m_ShowHideWorldDefinition);

            return root;
        }
    }
}