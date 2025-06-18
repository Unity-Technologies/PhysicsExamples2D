using UnityEditor.UIElements;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneShape))]
    [CanEditMultipleObjects]
    public class SceneShapeEditor : Editor
    {
        private SerializedProperty m_ShapeTypeProperty;
        private SerializedProperty m_CircleGeometryProperty;
        private SerializedProperty m_CapsuleGeometryProperty;
        private SerializedProperty m_SegmentGeometryProperty;
        private SerializedProperty m_ChainSegmentGeometryProperty;
        private SerializedProperty m_PolygonGeometryProperty;
        private SerializedProperty m_ShapeDefinitionProperty;
        private SerializedProperty m_ScaleRadiusProperty;
        private SerializedProperty m_SceneBodyProperty;
        
        private PropertyField m_CircleGeometryPropertyField;
        private PropertyField m_CapsuleGeometryPropertyField;
        private PropertyField m_SegmentGeometryPropertyField;
        private PropertyField m_ChainSegmentGeometryPropertyField;
        private PropertyField m_PolygonGeometryPropertyField;
        private PropertyField m_ScaleRadiusPropertyField;
        
        private void OnEnable()
        {
            m_SceneBodyProperty = serializedObject.FindProperty(nameof(SceneShape.SceneBody));
            m_ShapeDefinitionProperty = serializedObject.FindProperty(nameof(SceneShape.ShapeDefinition));
            m_ScaleRadiusProperty = serializedObject.FindProperty(nameof(SceneShape.ScaleRadius));
            m_ShapeTypeProperty = serializedObject.FindProperty(nameof(SceneShape.ShapeType));
            m_CircleGeometryProperty = serializedObject.FindProperty(nameof(SceneShape.CircleGeometry));
            m_CapsuleGeometryProperty = serializedObject.FindProperty(nameof(SceneShape.CapsuleGeometry));
            m_SegmentGeometryProperty = serializedObject.FindProperty(nameof(SceneShape.SegmentGeometry));
            m_ChainSegmentGeometryProperty = serializedObject.FindProperty(nameof(SceneShape.ChainSegmentGeometry));
            m_PolygonGeometryProperty = serializedObject.FindProperty(nameof(SceneShape.PolygonGeometry));
        }
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Geometry.
            m_ScaleRadiusPropertyField = new PropertyField(m_ScaleRadiusProperty);
            m_CircleGeometryPropertyField = new PropertyField(m_CircleGeometryProperty);
            m_CapsuleGeometryPropertyField = new PropertyField(m_CapsuleGeometryProperty);
            m_SegmentGeometryPropertyField = new PropertyField(m_SegmentGeometryProperty);
            m_ChainSegmentGeometryPropertyField = new PropertyField(m_ChainSegmentGeometryProperty);
            m_PolygonGeometryPropertyField = new PropertyField(m_PolygonGeometryProperty);
            
            // Shape Type.
            var shapeTypePropertyField = new PropertyField(m_ShapeTypeProperty);
            shapeTypePropertyField.RegisterValueChangeCallback(_ => { UpdateGeometries(); });
            root.Add(shapeTypePropertyField);
            root.Add(m_ScaleRadiusPropertyField);
            
            root.Add(m_CircleGeometryPropertyField);
            root.Add(m_CapsuleGeometryPropertyField);
            root.Add(m_SegmentGeometryPropertyField);
            root.Add(m_ChainSegmentGeometryPropertyField);
            root.Add(m_PolygonGeometryPropertyField);
            UpdateGeometries();

            // Base Properties.
            root.Add(new PropertyField(m_ShapeDefinitionProperty));
            root.Add(new PropertyField(m_SceneBodyProperty));
            
            return root;
        }

        private void UpdateGeometries()
        {
            var shapeType = (PhysicsShape.ShapeType)m_ShapeTypeProperty.enumValueIndex;

            m_ScaleRadiusPropertyField.style.display = (shapeType != PhysicsShape.ShapeType.Segment && shapeType != PhysicsShape.ShapeType.ChainSegment) ? DisplayStyle.Flex : DisplayStyle.None;
            
            m_CircleGeometryPropertyField.style.display = shapeType == PhysicsShape.ShapeType.Circle ? DisplayStyle.Flex : DisplayStyle.None;
            m_CapsuleGeometryPropertyField.style.display = shapeType == PhysicsShape.ShapeType.Capsule ? DisplayStyle.Flex : DisplayStyle.None;
            m_SegmentGeometryPropertyField.style.display = shapeType == PhysicsShape.ShapeType.Segment ? DisplayStyle.Flex : DisplayStyle.None;
            m_ChainSegmentGeometryPropertyField.style.display = shapeType == PhysicsShape.ShapeType.ChainSegment ? DisplayStyle.Flex : DisplayStyle.None;
            m_PolygonGeometryPropertyField.style.display = shapeType == PhysicsShape.ShapeType.Polygon ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}