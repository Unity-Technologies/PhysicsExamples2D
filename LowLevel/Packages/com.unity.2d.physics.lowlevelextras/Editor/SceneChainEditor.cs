using UnityEditor.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneChain))]
    [CanEditMultipleObjects]
    public class SceneChainEditor : Editor
    {
        private SerializedProperty m_PointsProperty;
        private SerializedProperty m_ReverseChainProperty;
        private SerializedProperty m_ChainDefinitionProperty;
        private SerializedProperty m_SurfaceMaterialProperty;
        private SerializedProperty m_SceneBodyProperty;
        
        private PropertyField m_CircleGeometryPropertyField;
        private PropertyField m_CapsuleGeometryPropertyField;
        private PropertyField m_SegmentGeometryPropertyField;
        private PropertyField m_PolygonGeometryPropertyField;
        private Label m_ChainShapeLabel;
        
        private void OnEnable()
        {
            m_ReverseChainProperty = serializedObject.FindProperty(nameof(SceneChain.ReverseChain));
            m_ChainDefinitionProperty = serializedObject.FindProperty(nameof(SceneChain.ChainDefinition));
            m_PointsProperty = serializedObject.FindProperty(nameof(SceneChain.Points));
            m_SceneBodyProperty = serializedObject.FindProperty(nameof(SceneChain.SceneBody));
        }
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            root.Add(new PropertyField(m_ReverseChainProperty));
            root.Add(new PropertyField(m_PointsProperty));
            root.Add(new PropertyField(m_ChainDefinitionProperty));
            root.Add(new PropertyField(m_SceneBodyProperty));
            
            return root;
        }
    }
}