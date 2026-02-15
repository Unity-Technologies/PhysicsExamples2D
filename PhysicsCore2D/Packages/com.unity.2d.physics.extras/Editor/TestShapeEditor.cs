using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(TestShape))]
    [CanEditMultipleObjects]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.EditorSourceNamespace)]
    public class TestShapeEditor : UnityEditor.Editor
    {
        private SerializedProperty m_ShapeTypeProperty;

        private PropertyField m_ScaleRadiusPropertyField;
        private PropertyField m_CircleGeometryPropertyField;
        private PropertyField m_CapsuleGeometryPropertyField;
        private PropertyField m_SegmentGeometryPropertyField;
        private PropertyField m_ChainSegmentGeometryPropertyField;
        private PropertyField m_PolygonGeometryPropertyField;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Geometry.
            m_ScaleRadiusPropertyField = new PropertyField(serializedObject.FindProperty(nameof(TestShape.ScaleRadius)));
            m_CircleGeometryPropertyField = new PropertyField(serializedObject.FindProperty(nameof(TestShape.CircleGeometry)));
            m_CapsuleGeometryPropertyField = new PropertyField(serializedObject.FindProperty(nameof(TestShape.CapsuleGeometry)));
            m_SegmentGeometryPropertyField = new PropertyField(serializedObject.FindProperty(nameof(TestShape.SegmentGeometry)));
            m_ChainSegmentGeometryPropertyField = new PropertyField(serializedObject.FindProperty(nameof(TestShape.ChainSegmentGeometry)));
            m_PolygonGeometryPropertyField = new PropertyField(serializedObject.FindProperty(nameof(TestShape.PolygonGeometry)));

            // Shape Type.
            m_ShapeTypeProperty = serializedObject.FindProperty(nameof(TestShape.ShapeType));
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
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestShape.ShapeDefinition))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestShape.UserData))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestShape.CallbackTarget))));
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(TestShape.testBody))));

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