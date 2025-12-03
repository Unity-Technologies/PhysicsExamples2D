using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(SceneFixedJoint))]
    [CanEditMultipleObjects]
    internal class SceneFixedJointEditor : SceneJointBaseEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneFixedJoint.JointDefinition))));
            AddBaseInspectorGUI(root);

            return root;
        }
    }
}