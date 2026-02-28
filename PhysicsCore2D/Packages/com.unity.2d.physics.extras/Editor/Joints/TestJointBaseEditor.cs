using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    internal abstract class TestJointBaseEditor : UnityEditor.Editor
    {
        protected void AddBaseInspectorGUI(VisualElement root)
        {
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.BodyA))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.BodyB))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestJointBase.CallbackTarget))));
        }
    }
}