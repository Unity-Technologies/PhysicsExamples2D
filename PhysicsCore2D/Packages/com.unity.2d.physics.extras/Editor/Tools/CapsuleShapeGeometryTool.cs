using UnityEngine;
using Unity.U2D.Physics.Extras;
using UnityEditor;

namespace Unity.U2D.Physics.Editor.Extras
{
    internal sealed partial class SceneShapeEditorTool
    {
        /// <summary>
        /// Capsule Geometry Tool.
        /// </summary>
        private sealed class CapsuleShapeGeometryTool : SceneShapeGeometryTool
        {
            public CapsuleShapeGeometryTool(SceneShape sceneShape, IGeometryToolSettings geometryToolSettings) : base(sceneShape, geometryToolSettings)
            {
            }

            public override void OnToolGUI(EditorWindow window)
            {
                // Get the shape geometry. 
                var geometry = Shape.capsuleGeometry;
                var localGeometry = ShapeTarget.CapsuleGeometry;

                // Calculate the relative transform from the scene body to this scene shape.
                var relativeTransform = PhysicsMath.GetRelativeMatrix(ShapeTarget.SceneBody.transform, ShapeTarget.transform, ShapeTarget.SceneBody.Body.world.transformPlane);

                // Set-up handles.
                var snap = GetSnapSettings();
                var handleDirection = PhysicsMath.GetTranslationIgnoredAxes(TransformPlane);

                var axisUp = (Vector3)(geometry.center1 - geometry.center2).normalized;
                var axisRight = new Vector3(axisUp.y, -axisUp.x, 0f);
                var handleRight = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisRight, TransformPlane)).normalized;
                var handleUp = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisUp, TransformPlane)).normalized;

                // Fetch the show labels option.
                var showLabels = geometryToolSettings.ShowLabels;

                // Radius.
                var shapeOrigin = PhysicsMath.ToPosition3D(Body.transform.TransformPoint((geometry.center1 + geometry.center2) * 0.5f), ShapeTarget.transform.position, TransformPlane);
                var handleSize = GetHandleSize(shapeOrigin);
                using (new Handles.DrawingScope(Matrix4x4.TRS(shapeOrigin, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    EditorGUI.BeginChangeCheck();
                    var radiusValue = handleRight * geometry.radius;
                    var newRadiusValue = Handles.Slider2D(radiusValue, handleDirection, handleRight, handleUp, handleSize, Handles.SphereHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change CapsuleGeometry Radius");
                        geometry.radius = Vector3.Dot(handleRight, newRadiusValue) > 0f ? newRadiusValue.magnitude : MinimumRadii;
                        localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);
                        ShapeTarget.CapsuleGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }

                    // Draw radius label.
                    if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                    {
                        Handles.color = geometryToolSettings.LabelColor;
                        var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry;
                        Handles.Label((geometry.radius + handleSize) * handleRight, $"Radius = {labelGeometry.radius.ToString(LabelFloatFormat)}");
                    }
                }

                // Center Mid.
                var centerOriginMid = PhysicsMath.ToPosition3D(Body.transform.TransformPoint((geometry.center1 + geometry.center2) * 0.5f), ShapeTarget.transform.position, TransformPlane);
                handleSize = GetHandleSize(centerOriginMid);
                using (new Handles.DrawingScope(Matrix4x4.TRS(centerOriginMid, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleMoveAllColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change CapsuleGeometry Center1&2");
                        var centerOffset = Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        geometry.center1 += centerOffset;
                        geometry.center2 += centerOffset;
                        localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);
                        ShapeTarget.CapsuleGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }
                }

                // Center #1.
                var centerOrigin1 = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometry.center1), ShapeTarget.transform.position, TransformPlane);
                handleSize = GetHandleSize(centerOrigin1);
                using (new Handles.DrawingScope(Matrix4x4.TRS(centerOrigin1, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change CapsuleGeometry Center1");
                        geometry.center1 += Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);
                        ShapeTarget.CapsuleGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }

                    // Draw center label.
                    if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                    {
                        Handles.color = geometryToolSettings.LabelColor;
                        var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry.Transform(Body.transform);
                        Handles.Label(handleUp * handleSize * 2f, $"Center1 = {labelGeometry.center1.ToString(LabelFloatFormat)}");
                    }
                }

                // Center #2.
                var centerOrigin2 = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometry.center2), ShapeTarget.transform.position, TransformPlane);
                handleSize = GetHandleSize(centerOrigin2);
                using (new Handles.DrawingScope(Matrix4x4.TRS(centerOrigin2, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    EditorGUI.BeginChangeCheck();
                    var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ShapeTarget, "Change CapsuleGeometry Center1");
                        geometry.center2 += Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                        localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);
                        ShapeTarget.CapsuleGeometry = localGeometry;
                        TargetShapeChanged = true;
                    }

                    // Draw center label.
                    if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                    {
                        Handles.color = geometryToolSettings.LabelColor;
                        var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry.Transform(Body.transform);
                        Handles.Label(handleUp * handleSize * 2f, $"Center2 = {labelGeometry.center2.ToString(LabelFloatFormat)}");
                    }
                }

                // Update shape if changed.
                if (TargetShapeChanged)
                    ShapeTarget.UpdateShape();

                // Draw the geometry.
                World.SetElementDepth3D(shapeOrigin);
                World.DrawCapsule(Body.transform, geometry.center1, geometry.center2, geometry.radius, Color.green, 0.0f, PhysicsWorld.DrawFillOptions.Outline);
            }
        }
    }
}