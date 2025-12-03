using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    internal abstract class SceneJointBaseEditor : UnityEditor.Editor
    {
        protected void AddBaseInspectorGUI(VisualElement root)
        {
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneJointBase.BodyA))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneJointBase.BodyB))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneJointBase.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneJointBase.CallbackTarget))));
        }
    }
}