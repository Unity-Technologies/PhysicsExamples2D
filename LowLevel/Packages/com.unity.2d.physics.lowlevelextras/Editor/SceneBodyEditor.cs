using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneBody))]
    [CanEditMultipleObjects]
    public class SceneBodyEditor : Editor
    {
        private SerializedProperty m_BodyDefinitionProperty;
        private SerializedProperty m_SceneWorldProperty;
        
        private void OnEnable()
        {
            m_BodyDefinitionProperty = serializedObject.FindProperty(nameof(SceneBody.BodyDefinition));
            m_SceneWorldProperty = serializedObject.FindProperty(nameof(SceneBody.SceneWorld));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(m_BodyDefinitionProperty));
            root.Add(new PropertyField(m_SceneWorldProperty));
            
            return root;
        }
    }
}