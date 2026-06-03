using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(TestOutlineShape))]
    [CanEditMultipleObjects]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.EditorSourceNamespace)]
    public class TestOutlineShapeEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestOutlineShape.Points))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestOutlineShape.ShapeDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestOutlineShape.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestOutlineShape.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestOutlineShape.testBody))));

            return root;
        }
    }
}