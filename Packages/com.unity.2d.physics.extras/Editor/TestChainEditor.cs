using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(TestChain))]
    [CanEditMultipleObjects]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.EditorSourceNamespace)]
    public class TestChainEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestChain.ChainDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestChain.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestShape.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestChain.Points))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestChain.ReverseChain))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestChain.testBody))));

            return root;
        }
    }
}