using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(SceneSpriteShape))]
    [CanEditMultipleObjects]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.EditorSourceNamespace)]
    public class SceneSpriteShapeEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.Sprite))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.UseDelaunay))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.ShapeDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneSpriteShape.SceneBody))));

            // Button to Read Sprite.
            root.Add(new Button
            {
                text = "Update From Sprite",
                clickable = new Clickable(_ =>
                {
                    foreach (var shapeTarget in targets)
                    {
                        (shapeTarget as SceneSpriteShape)?.UpdateShape();
                    }
                })
            });

            return root;
        }
    }
}