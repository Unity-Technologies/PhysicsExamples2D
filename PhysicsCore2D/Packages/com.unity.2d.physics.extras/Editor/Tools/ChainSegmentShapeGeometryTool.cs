using UnityEngine;
using Unity.U2D.Physics.Extras;
using UnityEditor;

namespace Unity.U2D.Physics.Editor.Extras
{
    internal sealed partial class TestShapeEditorTool
    {
        /// <summary>
        /// Chain Segment Geometry Tool.
        /// </summary>
        private sealed class ChainSegmentShapeGeometryTool : TestShapeGeometryTool
        {
            public ChainSegmentShapeGeometryTool(TestShape testShape, IGeometryToolSettings geometryToolSettings) : base(testShape, geometryToolSettings)
            {
            }

            public override void OnToolGUI(EditorWindow window)
            {
                // Get the shape geometry. 
                var geometry = Shape.chainSegmentGeometry;
                var geometrySegment = geometry.segment;
                var localGeometry = ShapeTarget.ChainSegmentGeometry;

                // Calculate the relative transform from the scene body to this scene shape.
                var relativeTransform = PhysicsMath.GetRelativeMatrix(ShapeTarget.testBody.transform, ShapeTarget.transform, ShapeTarget.testBody.body.world.transformPlane);

                // Set-up handles.
                var snap = GetSnapSettings();
                var handleDirection = PhysicsMath.GetTranslationIgnoredAxes(TransformPlane);

                var axisUp = (Vector3)(geometrySegment.point1 - geometrySegment.point2).normalized;
                var axisRight = new Vector3(axisUp.y, -axisUp.x, 0f);
                var handleRight = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisRight, TransformPlane)).normalized;
                var handleUp = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisUp, TransformPlane)).normalized;

                // Fetch the show labels option.
                var showLabels = geometryToolSettings.ShowLabels;

                // Segment Mid.
                var centerOriginMid = PhysicsMath.ToPosition3D(Body.transform.TransformPoint((geometrySegment.point1 + geometrySegment.point2) * 0.5f), ShapeTarget.transform.position, TransformPlane);
                var handleSize = GetHandleSize(centerOriginMid);
                using (new Handles.DrawingScope(Matrix4x4.TRS(centerOriginMid, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleMoveAllColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change ChainSegmentGeometry Center1&2");
                        var centerOffset = Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        geometrySegment.point1 += centerOffset;
                        geometrySegment.point2 += centerOffset;
                        geometry.ghost1 += centerOffset;
                        geometry.ghost2 += centerOffset;
                        geometry.segment = geometrySegment;
                        localGeometry = geometry.InverseTransform(relativeTransform);
                        ShapeTarget.ChainSegmentGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }
                }

                // Segment Point #1.
                var pointOrigin1 = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometrySegment.point1), ShapeTarget.transform.position, TransformPlane);
                handleSize = GetHandleSize(pointOrigin1);
                using (new Handles.DrawingScope(Matrix4x4.TRS(pointOrigin1, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change ChainSegmentGeometry Point1");
                        var offset = Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        geometrySegment.point1 += offset;
                        geometry.ghost1 += offset;
                        geometry.segment = geometrySegment;
                        localGeometry = geometry.InverseTransform(relativeTransform);
                        ShapeTarget.ChainSegmentGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }

                    // Draw center label.
                    if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                    {
                        Handles.color = geometryToolSettings.LabelColor;
                        var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry.Transform(Body.transform);
                        Handles.Label(handleUp * handleSize * 2f, $"Point1 = {labelGeometry.segment.point1.ToString(LabelFloatFormat)}");
                    }
                }

                // Segment Point #2.
                var pointOrigin2 = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometrySegment.point2), ShapeTarget.transform.position, TransformPlane);
                handleSize = GetHandleSize(pointOrigin2);
                using (new Handles.DrawingScope(Matrix4x4.TRS(pointOrigin2, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change ChainSegmentGeometry Point2");
                        var offset = Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        geometrySegment.point2 += offset;
                        geometry.ghost2 += offset;
                        geometry.segment = geometrySegment;
                        localGeometry = geometry.InverseTransform(relativeTransform);
                        ShapeTarget.ChainSegmentGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }

                    // Draw center label.
                    if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                    {
                        Handles.color = geometryToolSettings.LabelColor;
                        var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry.Transform(Body.transform);
                        Handles.Label(handleUp * handleSize * 2f, $"Point2 = {labelGeometry.segment.point2.ToString(LabelFloatFormat)}");
                    }
                }

                // Ghost Point #1.
                var ghostPointOrigin1 = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometry.ghost1), ShapeTarget.transform.position, TransformPlane);
                handleSize = GetHandleSize(ghostPointOrigin1);
                using (new Handles.DrawingScope(Matrix4x4.TRS(ghostPointOrigin1, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change ChainSegmentGeometry GhostPoint1");
                        geometry.ghost1 += Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        localGeometry = geometry.InverseTransform(relativeTransform);
                        ShapeTarget.ChainSegmentGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }

                    // Draw center label.
                    if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                    {
                        Handles.color = geometryToolSettings.LabelColor;
                        var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry.Transform(Body.transform);
                        Handles.Label(handleUp * handleSize * 2f, $"Ghost1 = {labelGeometry.ghost1.ToString(LabelFloatFormat)}");
                    }
                }

                // Ghost Point #2.
                var ghostPointOrigin2 = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometry.ghost2), ShapeTarget.transform.position, TransformPlane);
                handleSize = GetHandleSize(ghostPointOrigin2);
                using (new Handles.DrawingScope(Matrix4x4.TRS(ghostPointOrigin2, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change ChainSegmentGeometry GhostPoint2");
                        geometry.ghost2 += Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        localGeometry = geometry.InverseTransform(relativeTransform);
                        ShapeTarget.ChainSegmentGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }

                    // Draw center label.
                    if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                    {
                        Handles.color = geometryToolSettings.LabelColor;
                        var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry.Transform(Body.transform);
                        Handles.Label(handleUp * handleSize * 2f, $"Ghost1 = {labelGeometry.ghost2.ToString(LabelFloatFormat)}");
                    }
                }

                // Update shape if changed.
                if (TargetShapeChanged)
                    ShapeTarget.UpdateShape();

                // Draw the geometry.
                World.SetElementDepth3D(centerOriginMid);
                World.DrawLine(Body.transform.TransformPoint(geometrySegment.point1), Body.transform.TransformPoint(geometrySegment.point2), Color.green);
            }
        }
    }
}