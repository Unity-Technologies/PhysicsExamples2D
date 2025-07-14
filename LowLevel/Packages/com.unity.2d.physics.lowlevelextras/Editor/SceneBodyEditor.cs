using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneBody))]
    [CanEditMultipleObjects]
    public class SceneBodyEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneBody.BodyDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneBody.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneBody.SceneWorld))));
            
            return root;
        }
    }
}