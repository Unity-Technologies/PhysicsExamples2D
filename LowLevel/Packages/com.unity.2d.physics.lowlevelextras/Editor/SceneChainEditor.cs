using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneChain))]
    [CanEditMultipleObjects]
    public class SceneChainEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.ReverseChain))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.ChainDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.Points))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneChain.SceneBody))));
            
            return root;
        }
    }
}