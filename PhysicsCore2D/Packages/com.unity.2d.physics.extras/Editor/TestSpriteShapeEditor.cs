using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(TestSpriteShape))]
    [CanEditMultipleObjects]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.EditorSourceNamespace)]
    public class TestSpriteShapeEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestSpriteShape.Sprite))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestSpriteShape.UseDelaunay))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestSpriteShape.ShapeDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestSpriteShape.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestSpriteShape.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestSpriteShape.testBody))));

            // Button to Read Sprite.
            root.Add(new Button
            {
                text = "Update From Sprite",
                clickable = new Clickable(_ =>
                {
                    foreach (var shapeTarget in targets)
                    {
                        (shapeTarget as TestSpriteShape)?.UpdateShape();
                    }
                })
            });

            return root;
        }
    }
}