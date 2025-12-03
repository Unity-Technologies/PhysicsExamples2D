using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(SceneHingeJoint))]
    [CanEditMultipleObjects]
    internal class SceneHingeJointEditor : SceneJointBaseEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneHingeJoint.JointDefinition))));
            AddBaseInspectorGUI(root);

            return root;
        }
    }
}