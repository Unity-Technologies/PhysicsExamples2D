using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    public abstract class SceneJointBaseEditor : Editor
    {
        protected void AddBaseInspectorGUI(VisualElement root)
        {
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneJointBase.BodyA))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneJointBase.BodyB))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneJointBase.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneJointBase.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneJointBase.SceneWorld))));
        }
    }
}