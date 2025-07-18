using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneOutlineShape))]
    [CanEditMultipleObjects]
    public class SceneOutlineShapeEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneOutlineShape.Points))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneOutlineShape.ShapeDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneOutlineShape.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneOutlineShape.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneOutlineShape.SceneBody))));

            return root;
        }
    }
}