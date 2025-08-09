using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneRelativeJoint))]
    [CanEditMultipleObjects]
    public class SceneRelativeJointEditor : SceneJointBaseEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneRelativeJoint.JointDefinition))));
            AddBaseInspectorGUI(root);

            return root;
        }
    }
}