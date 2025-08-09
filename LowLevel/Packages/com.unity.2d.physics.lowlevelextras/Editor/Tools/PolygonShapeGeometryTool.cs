using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    internal sealed partial class SceneShapeEditorTool
    {
        /// <summary>
        /// Polygon Geometry Tool.
        /// </summary>
        private sealed class PolygonShapeGeometryTool : SceneShapeGeometryTool
        {
            private Vector2 m_HotVertex1;
            private Vector2 m_HotVertex2;
            private int m_HotVertexCount;

            public PolygonShapeGeometryTool(SceneShape sceneShape, IGeometryToolSettings geometryToolSettings) : base(sceneShape, geometryToolSettings)
            {
            }

            public override void OnToolGUI(EditorWindow window)
            {
                // Get the shape geometry. 
                var geometry = Shape.polygonGeometry;
                var localGeometry = ShapeTarget.PolygonGeometry;

                // Calculate the relative transform from the scene body to this scene shape.
                var relativeTransform = PhysicsMath.GetRelativeMatrix(ShapeTarget.SceneBody.transform, ShapeTarget.transform, ShapeTarget.SceneBody.Body.world.transformPlane);

                TargetShapeChanged = false;

                // Fetch the point count.
                var pointCount = geometry.count;
                if (pointCount < 3 || pointCount > PhysicsConstants.MaxPolygonVertices)
                    return;

                var vertices = geometry.vertices;
                var normals = geometry.normals;

                var currentEvent = Event.current;
                var currentEventType = currentEvent.type;

                // Are we in add/remove mode as opposed to move-only mode?
                var addMode = (currentEvent.command || currentEvent.shift) && pointCount < PhysicsConstants.MaxPolygonVertices;
                var removeMode = (currentEvent.command || currentEvent.shift) && pointCount > 3;

                // Always consume the mouse-drag if we're adding/removing to suppress other function.
                if ((addMode || removeMode) && currentEventType == EventType.MouseDrag)
                    currentEvent.Use();

                // Reset any hot vertex if the mouse button is released.
                if (currentEventType == EventType.MouseUp)
                    m_HotVertexCount = 0;

                // Set-up handles.
                var snap = GetSnapSettings();
                var handleDirection = PhysicsMath.GetTranslationIgnoredAxes(TransformPlane);

                // Fetch the show labels option.
                var showLabels = geometryToolSettings.ShowLabels;

                // Points.
                for (int i = 0, j = pointCount - 1; i < pointCount; j = i++)
                {
                    // Calculate the axis/handle directions.
                    var axisUp = (Vector3)(vertices[i] - vertices[j]).normalized;
                    var axisRight = new Vector3(axisUp.y, -axisUp.x, 0f);
                    var handleRight = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisRight, TransformPlane)).normalized;
                    var handleUp = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisUp, TransformPlane)).normalized;

                    // Vertices.
                    {
                        // Fetch the point.
                        var point = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(vertices[i]), ShapeTarget.transform.position, TransformPlane);
                        var handleSize = GetHandleSize(point);

                        // Draw the point handle.
                        using (new Handles.DrawingScope(Matrix4x4.TRS(point, Quaternion.identity, Vector3.one)))
                        {
                            // Set the handle color.
                            Handles.color = removeMode ? geometryToolSettings.GrabHandleDeleteColor : geometryToolSettings.GrabHandleVertexColor;

                            EditorGUI.BeginChangeCheck();

                            var controlId = GUIUtility.GetControlID(FocusType.Passive);

                            // If this is the current hot vertex then set this as the hot-control.
                            if (m_HotVertexCount == 1)
                            {
                                if (vertices[i] == m_HotVertex1)
                                    GUIUtility.hotControl = controlId;
                            }

                            var newCenterValue = Handles.Slider2D(controlId, Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);

                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(ShapeTarget, "Change PolygonGeometry Point");
                                vertices[i] += Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                                localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);

                                m_HotVertexCount = 1;
                                m_HotVertex1 = vertices[i];
                                TargetShapeChanged = true;
                            }

                            // Draw vertex label.
                            if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                            {
                                Handles.color = geometryToolSettings.LabelColor;
                                var labelVertices = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry.vertices[i] : Body.transform.TransformPoint(geometry.vertices[i]);
                                Handles.Label(handleUp * handleSize * 2f, $"{labelVertices.ToString(LabelFloatFormat)}");
                            }

                            // Did we click to delete the point?
                            if (removeMode && currentEventType == EventType.MouseDown && GUIUtility.hotControl == controlId)
                            {
                                Undo.RecordObject(ShapeTarget, "Delete PolygonGeometry Point");

                                geometry = PolygonGeometry.DeleteVertex(geometry, i).Validate();
                                localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);

                                ShapeTarget.PolygonGeometry = localGeometry;
                                ShapeTarget.UpdateShape();
                                return;
                            }
                        }
                    }

                    // Add.
                    if (addMode)
                    {
                        // Calculate the add point.
                        // NOTE: We must offset the point along the normal, so we're not collinear.
                        var collinearOffset = 0.5f;
                        var midPoint = (vertices[i] + vertices[j]) * 0.5f + (normals[j] * collinearOffset);
                        var point = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(midPoint), ShapeTarget.transform.position, TransformPlane);
                        var handleSize = GetHandleSize(point);

                        // Draw the add point handle.
                        using (new Handles.DrawingScope(Matrix4x4.TRS(point, Quaternion.identity, Vector3.one)))
                        {
                            // Set the handle color.
                            Handles.color = geometryToolSettings.GrabHandleAddColor;

                            var controlId = GUIUtility.GetControlID(FocusType.Passive);
                            Handles.Slider2D(controlId, Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);

                            // Did we click to delete the point?
                            if (currentEventType == EventType.MouseDown && GUIUtility.hotControl == controlId)
                            {
                                Undo.RecordObject(ShapeTarget, "Add PolygonGeometry Point");

                                geometry = PolygonGeometry.InsertVertex(geometry, i, midPoint).Validate();
                                localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);

                                ShapeTarget.PolygonGeometry = localGeometry;
                                ShapeTarget.UpdateShape();
                                return;
                            }
                        }
                    }

                    // Radius.
                    if (!addMode && !removeMode)
                    {
                        if (i == 0)
                        {
                            // Calculate the add point.
                            var radiusPoint = Vector2.Lerp(vertices[i], vertices[j], 0.75f);
                            var point = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(radiusPoint), ShapeTarget.transform.position, TransformPlane);
                            var handleSize = GetHandleSize(point);

                            // Draw the radius point handle.
                            using (new Handles.DrawingScope(Matrix4x4.TRS(point, Quaternion.identity, Vector3.one)))
                            {
                                EditorGUI.BeginChangeCheck();
                                var radiusValue = handleRight * geometry.radius;
                                var newRadiusValue = Handles.Slider2D(radiusValue, handleDirection, handleRight, handleUp, handleSize, Handles.SphereHandleCap, snap);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(ShapeTarget, "Change PolygonGeometry Radius");
                                    geometry.radius = Vector3.Dot(handleRight, newRadiusValue) > 0f ? newRadiusValue.magnitude : 0f;
                                    localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);

                                    ShapeTarget.PolygonGeometry = localGeometry;
                                    ShapeTarget.UpdateShape();
                                    return;
                                }

                                // Draw radius label.
                                if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                                {
                                    Handles.color = geometryToolSettings.LabelColor;
                                    var labelGeometry = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? localGeometry : geometry;
                                    Handles.Label((geometry.radius + handleSize) * handleRight, $"Radius = {labelGeometry.radius.ToString(LabelFloatFormat)}");
                                }
                            }

                            // Centroid.
                            var centroid = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometry.centroid), ShapeTarget.transform.position, TransformPlane);
                            handleSize = GetHandleSize(centroid);
                            using (new Handles.DrawingScope(Matrix4x4.TRS(centroid, Quaternion.identity, Vector3.one)))
                            {
                                // Set handle color.
                                Handles.color = geometryToolSettings.GrabHandleMoveAllColor;

                                EditorGUI.BeginChangeCheck();
                                var newCenterValue = Handles.Slider2D(Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(ShapeTarget, "Change PolygonGeometry Centroid");
                                    var centerOffset = Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                                    geometry = geometry.Transform(new PhysicsTransform(centerOffset, PhysicsRotate.identity));
                                    localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);
                                    ShapeTarget.PolygonGeometry = localGeometry;
                                    ShapeTarget.UpdateShape();
                                    return;
                                }
                            }
                        }

                        // Edge move.
                        {
                            // Calculate the add point.
                            var edgePoint = Vector2.Lerp(vertices[i], vertices[j], 0.5f) + normals[j] * geometry.radius;
                            var point = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(edgePoint), ShapeTarget.transform.position, TransformPlane);
                            var handleSize = GetHandleSize(point);

                            // Draw the radius point handle.
                            using (new Handles.DrawingScope(Matrix4x4.TRS(point, Quaternion.identity, Vector3.one)))
                            {
                                // Set handle color.
                                Handles.color = geometryToolSettings.GrabHandleMoveAllColor;

                                EditorGUI.BeginChangeCheck();

                                var controlId = GUIUtility.GetControlID(FocusType.Passive);

                                // If this is the current hot vertex then set this as the hot-control.
                                if (m_HotVertexCount == 2)
                                {
                                    if (vertices[i] == m_HotVertex1 && vertices[j] == m_HotVertex2)
                                        GUIUtility.hotControl = controlId;
                                }

                                var newEdgeValue = Handles.Slider2D(controlId, Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(ShapeTarget, "Change PolygonGeometry Edge");
                                    var projectedOffset = Vector3.Dot(handleRight, newEdgeValue) * handleRight;
                                    var vertexOffset = Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(projectedOffset, TransformPlane));
                                    vertices[i] += vertexOffset;
                                    vertices[j] += vertexOffset;

                                    localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);

                                    m_HotVertexCount = 2;
                                    m_HotVertex1 = vertices[i];
                                    m_HotVertex2 = vertices[j];
                                    TargetShapeChanged = true;
                                }
                            }
                        }
                    }
                }

                var drawGeometryColor = Color.green;
                var drawGeometryFilled = false;

                // Update shape if changed.
                if (TargetShapeChanged)
                {
                    // Update the vertices.
                    geometry.vertices = vertices;
                    var newGeometry = geometry.Validate();
                    if (newGeometry.isValid && newGeometry.count == geometry.count)
                    {
                        localGeometry = geometry.InverseTransform(relativeTransform, ShapeTarget.ScaleRadius);

                        ShapeTarget.PolygonGeometry = localGeometry;
                        ShapeTarget.UpdateShape();
                    }
                    else
                    {
                        drawGeometryColor = Color.red;
                        drawGeometryFilled = true;
                    }
                }

                // Draw the geometry.
                World.SetElementDepth3D(PhysicsMath.ToPosition3D(Body.transform.TransformPoint(geometry.centroid), ShapeTarget.transform.position, TransformPlane));
                World.DrawGeometry(geometry, Body.transform, drawGeometryColor, 0.0f, drawGeometryFilled ? PhysicsWorld.DrawFillOptions.All : PhysicsWorld.DrawFillOptions.Outline);
            }
        }
    }
}