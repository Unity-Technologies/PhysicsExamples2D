using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneSpriteShape))]
    [CanEditMultipleObjects]
    public class SceneSpriteShapeEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.Sprite))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.ShapeDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.SceneBody))));

            return root;
        }
    }
}