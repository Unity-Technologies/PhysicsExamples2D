using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneHingeJoint))]
    [CanEditMultipleObjects]
    public class SceneHingeJointEditor : SceneJointBaseEditor
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