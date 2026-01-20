using UnityEngine;
using Unity.U2D.Physics.Extras;
using UnityEditor;

namespace Unity.U2D.Physics.Editor.Extras
{
    internal sealed partial class SceneShapeEditorTool
    {
        /// <summary>
        /// Segment Geometry Tool.
        /// </summary>
        private sealed class SegmentShapeGeometryTool : SceneShapeGeometryTool
        {
            public SegmentShapeGeometryTool(SceneShape sceneShape, IGeometryToolSettings geometryToolSettings) : base(sceneShape, geometryToolSettings)
            {
            }

            public override void OnToolGUI(EditorWindow window)
            {
                // Get the shape geometry. 
                var geometry = Shape.segmentGeometry;
                var localGeometry = ShapeTarget.SegmentGeometry;

                // Calculate the relative transform from the scene body to this scene shape.
                var relativeTransform = PhysicsMath.GetRelativeMatrix(ShapeTarget.SceneBody.transform, ShapeTarget.transform, ShapeTarget.SceneBody.Body.world.transformPlane);

                // Set-up handles.
                var snap = GetSnapSettings();
                var handleDirection = PhysicsMath.GetTranslationIgnoredAxes(TransformPlane);

                var axisUp = (Vector3)(geometry.point1 - geometry.point2).normalized;
                var axisRight = new Vector3(axisUp.y, -axisUp.x, 0f);
                var handleRight = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisRight, TransformPlane)).normalized;
                var handleUp = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisUp, TransformPlane)).normalized;

                // Fetch the show labels option.
                var showLabels = geometryToolSettings.ShowLabels;

                // Segment Mid.
                var centerOriginMid = PhysicsMath.ToPosition3D(Body.transform.TransformPoint((geometry.point1 + geometry.point2) * 0.5f), ShapeTarget.transform.position, TransformPlane);
                var handleSize = GetHandleSize(centerOriginMid);
                using (new Handles.DrawingScope(Matrix4x4.TRS(centerOriginMid, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleMoveAllColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change SegmentGeometry Point1&2");
                        var centerOffset = Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        geometry.point1 += centerOffset;
                        geometry.point2 += centerOffset;
                        localGeometry = geometry.InverseTransform(relativeTransform);
                        ShapeTarget.SegmentGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }
                }

                // Point #1.
                var pointOrigin1 = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometry.point1), ShapeTarget.transform.position, TransformPlane);
                handleSize = GetHandleSize(pointOrigin1);
                using (new Handles.DrawingScope(Matrix4x4.TRS(pointOrigin1, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change SegmentGeometry Point1");
                        geometry.point1 += Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        localGeometry = geometry.InverseTransform(relativeTransform);
                        ShapeTarget.SegmentGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }

                    // Draw center label.
                    if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                    {
                        Handles.color = geometryToolSettings.LabelColor;
                        var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry.Transform(Body.transform);
                        Handles.Label(handleUp * handleSize * 2f, $"Point1 = {labelGeometry.point1.ToString(LabelFloatFormat)}");
                    }
                }

                // Point2 #2.
                var pointOrigin2 = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometry.point2), ShapeTarget.transform.position, TransformPlane);
                handleSize = GetHandleSize(pointOrigin2);
                using (new Handles.DrawingScope(Matrix4x4.TRS(pointOrigin2, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change SegmentGeometry Point2");
                        geometry.point2 += Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        localGeometry = geometry.InverseTransform(relativeTransform);
                        ShapeTarget.SegmentGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }

                    // Draw center label.
                    if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                    {
                        Handles.color = geometryToolSettings.LabelColor;
                        var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry.Transform(Body.transform);
                        Handles.Label(handleUp * handleSize * 2f, $"Point2 = {labelGeometry.point2.ToString(LabelFloatFormat)}");
                    }
                }

                // Update shape if changed.
                if (TargetShapeChanged)
                    ShapeTarget.UpdateShape();

                // Draw the geometry.
                World.SetElementDepth3D(centerOriginMid);
                World.DrawLine(Body.transform.TransformPoint(geometry.point1), Body.transform.TransformPoint(geometry.point2), Color.green);
            }
        }
    }
}