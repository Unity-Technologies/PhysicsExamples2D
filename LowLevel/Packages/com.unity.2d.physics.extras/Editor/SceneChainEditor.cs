using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(SceneChain))]
    [CanEditMultipleObjects]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.EditorSourceNamespace)]
    public class SceneChainEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.ChainDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneShape.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.Points))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.ReverseChain))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.SceneBody))));

            return root;
        }
    }
}