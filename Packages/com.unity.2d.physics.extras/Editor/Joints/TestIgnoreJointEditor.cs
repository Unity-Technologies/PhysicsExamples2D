using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(TestIgnoreJoint))]
    [CanEditMultipleObjects]
    internal class TestIgnoreJointEditor : TestJointBaseEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.BodyA))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.BodyB))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.AutoAnchorA))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.AutoAnchorB))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestIgnoreJoint.JointDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.CallbackTarget))));

            return root;
        }
    }
}