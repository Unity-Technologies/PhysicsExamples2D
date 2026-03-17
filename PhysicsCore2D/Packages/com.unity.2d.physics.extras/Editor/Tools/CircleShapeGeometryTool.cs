using UnityEngine;
using Unity.U2D.Physics.Extras;
using UnityEditor;

namespace Unity.U2D.Physics.Editor.Extras
{
    internal sealed partial class TestShapeEditorTool
    {
        /// <summary>
        /// Circle Geometry Tool.
        /// </summary>
        private sealed class CircleShapeGeometryTool : TestShapeGeometryTool
        {
            public CircleShapeGeometryTool(TestShape testShape, IGeometryToolSettings geometryToolSettings) : base(testShape, geometryToolSettings)
            {
            }

            public override void OnToolGUI(EditorWindow window)
            {
                // Get the shape geometry. 
                var geometry = Shape.circleGeometry;
                var localGeometry = ShapeTarget.CircleGeometry;

                // Calculate the relative transform from the scene body to this scene shape.
                var relativeTransform = GetRelativeTransform();

                // Set-up handles.
                var snap = GetSnapSettings();
                var handleDirection = GetHandleDirection();
                var handleRight = GetTransformedVector(Vector3.right);
                var handleUp = GetTransformedVector(Vector3.up);

                // Fetch the show labels option.
                var showLabels = geometryToolSettings.ShowLabels;

                var shapeOrigin = ToBodyPosition3D(geometry.center);
                var handleSize = GetHandleSize(shapeOrigin);
                using (new Handles.DrawingScope(Matrix4x4.TRS(shapeOrigin, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    // Radius.
                    {
                        EditorGUI.BeginChangeCheck();
                        var radiusValue = handleRight * geometry.radius;
                        var newValue = Handles.Slider2D(radiusValue, handleDirection, handleRight, handleUp, handleSize, Handles.SphereHandleCap, snap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(ShapeTarget, "Change CircleGeometry Radius");

                            geometry.radius = newValue.magnitude;
                            localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);
                            ShapeTarget.CircleGeometry = localGeometry;
                            TargetShapeChanged = true;
                        }

                        // Draw label.
                        if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                        {
                            Handles.color = geometryToolSettings.LabelColor;
                            var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry;
                            Handles.Label((geometry.radius + handleSize) * handleRight, $"Radius = {labelGeometry.radius.ToString(LabelFloatFormat)}");
                        }
                    }

                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleMoveAllColor;

                    // Center.
                    {
                        EditorGUI.BeginChangeCheck();
                        var newValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(ShapeTarget, "Change CircleGeometry Center");

                            geometry.center += Body.rotation.InverseRotateVector(ToPosition2D(newValue));
                            localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);
                            ShapeTarget.CircleGeometry = localGeometry;
                            TargetShapeChanged = true;
                        }

                        // Draw label.
                        if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                        {
                            Handles.color = geometryToolSettings.LabelColor;
                            var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry.Transform(Body.transform);
                            Handles.Label(handleUp * handleSize * 2f, $"Center = {labelGeometry.center.ToString(LabelFloatFormat)}");
                        }
                    }
                }

                // Update shape if changed.
                if (TargetShapeChanged)
                    ShapeTarget.UpdateShape();

                // Draw the geometry.
                World.SetElementDepth3D(shapeOrigin);
                World.DrawCircle(Body.transform.TransformPoint(geometry.center), geometry.radius, Color.green);
            }
        }
    }
}