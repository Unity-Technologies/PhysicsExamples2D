using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(SceneOutlineShape))]
    [CanEditMultipleObjects]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.EditorSourceNamespace)]
    public class SceneOutlineShapeEditor : UnityEditor.Editor
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