using UnityEngine;
using Unity.U2D.Physics.Extras;
using UnityEditor;

namespace Unity.U2D.Physics.Editor.Extras
{
    internal sealed partial class SceneShapeEditorTool
    {
        /// <summary>
        /// Circle Geometry Tool.
        /// </summary>
        private sealed class CircleShapeGeometryTool : SceneShapeGeometryTool
        {
            public CircleShapeGeometryTool(SceneShape sceneShape, IGeometryToolSettings geometryToolSettings) : base(sceneShape, geometryToolSettings)
            {
            }

            public override void OnToolGUI(EditorWindow window)
            {
                // Get the shape geometry. 
                var geometry = Shape.circleGeometry;
                var localGeometry = ShapeTarget.CircleGeometry;

                // Calculate the relative transform from the scene body to this scene shape.
                var relativeTransform = PhysicsMath.GetRelativeMatrix(ShapeTarget.SceneBody.transform, ShapeTarget.transform, ShapeTarget.SceneBody.Body.world.transformPlane);

                // Set-up handles.
                var snap = GetSnapSettings();
                var handleDirection = PhysicsMath.GetTranslationIgnoredAxes(TransformPlane);
                var handleRight = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(Vector3.right, TransformPlane)).normalized;
                var handleUp = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(Vector3.up, TransformPlane)).normalized;

                // Fetch the show labels option.
                var showLabels = geometryToolSettings.ShowLabels;

                var shapeOrigin = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometry.center), ShapeTarget.transform.position, TransformPlane);
                var handleSize = GetHandleSize(shapeOrigin);
                using (new Handles.DrawingScope(Matrix4x4.TRS(shapeOrigin, Quaternion.identity, Vector3.one)))
                {
                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleVertexColor;

                    // Radius.
                    {
                        EditorGUI.BeginChangeCheck();
                        var radiusValue = handleRight * geometry.radius;
                        var newRadiusValue = Handles.Slider2D(radiusValue, handleDirection, handleRight, handleUp, handleSize, Handles.SphereHandleCap, snap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(ShapeTarget, "Change CircleGeometry Radius");

                            geometry.radius = newRadiusValue.magnitude;
                            localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);
                            ShapeTarget.CircleGeometry = localGeometry;
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

                    // Set handle color.
                    Handles.color = geometryToolSettings.GrabHandleMoveAllColor;

                    // Center.
                    {
                        EditorGUI.BeginChangeCheck();
                        var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(ShapeTarget, "Change CircleGeometry Center");

                            geometry.center += Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                            localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);
                            ShapeTarget.CircleGeometry = localGeometry;
                            TargetShapeChanged = true;
                        }

                        // Draw center label.
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
                World.DrawCircle(PhysicsMath.ToPosition2D(shapeOrigin, TransformPlane), geometry.radius, Color.green);
            }
        }
    }
}