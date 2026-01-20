using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(SceneDistanceJoint))]
    [CanEditMultipleObjects]
    internal class SceneDistanceJointEditor : SceneJointBaseEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneDistanceJoint.JointDefinition))));
            AddBaseInspectorGUI(root);

            return root;
        }
    }
}