using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneTargetJoint))]
    [CanEditMultipleObjects]
    public class SceneTargetJointEditor : SceneJointBaseEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneTargetJoint.JointDefinition))));
            AddBaseInspectorGUI(root);
            
            return root;
        }
    }
}