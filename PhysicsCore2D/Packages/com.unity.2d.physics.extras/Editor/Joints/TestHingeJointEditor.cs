using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(TestHingeJoint))]
    [CanEditMultipleObjects]
    internal class TestHingeJointEditor : TestJointBaseEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestHingeJoint.JointDefinition))));
            AddBaseInspectorGUI(root);

            return root;
        }
    }
}