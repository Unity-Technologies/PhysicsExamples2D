using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [EditorTool("Edit Scene Chain", typeof(SceneChain))]
    internal sealed partial class SceneChainTool : EditorTool, IGeometryToolSettings
    {
        public override void OnActivated()
        {
            // Create overlay.
            m_Overlay = new SceneGeometryToolOverlay(this) { visible = true };
            SceneView.AddOverlayToActiveView(m_Overlay);
            
            // Create the appropriate geometry tools.
            m_GeometryTools = new List<SceneChainGeometryEditorTool>(capacity: 1);
            foreach (var toolTarget in targets)
            {
                if (toolTarget is SceneChain sceneChainShape)
                {
                    if (sceneChainShape.isActiveAndEnabled)
                        m_GeometryTools.Add(new ChainSceneChainGeometryEditorTool(sceneChainShape, this));
                }
            }
        }
        
        public override void OnWillBeDeactivated()
        {
            // Remove overlay.
            SceneView.RemoveOverlayFromActiveView(m_Overlay);
            m_Overlay.visible = false;
            m_Overlay = null;
            
            // Clear the tools.
            m_GeometryTools.Clear();
        }
        
        public override void OnToolGUI(EditorWindow window)
        {
            // Finish if there are no tools available.
            if (m_GeometryTools == null)
                return;

            // Iterate the available tools.
            foreach (var geometryTool in m_GeometryTools)
            {
                // Update the tool in-case the target has been recreated. 
                geometryTool.UpdateTool();
                
                // Let the tool handle the callback if it's valid.
                if (geometryTool.isValid)
                    geometryTool.OnToolGUI(window);
            }
            
            // If we're not playing then we should queue a player update.
            // NOTE: We do this, so we'll render the gizmos.
            if (!EditorApplication.isPlaying)
                EditorApplication.QueuePlayerLoopUpdate();
        }
        
        public IGeometryToolSettings.ShowLabelMode ShowLabels { get; set; }
        public Color LabelColor { get; set; }
        public Color GrabHandleVertexColor { get; set; }
        public Color GrabHandleMoveAllColor { get; set; }
        public Color GrabHandleAddColor { get; set; }
        public Color GrabHandleDeleteColor { get; set; }
        
        public override GUIContent toolbarIcon => m_ToolIcon ??= new GUIContent(EditorGUIUtility.IconContent(IconUtility.IconPath + "SceneShapeTool.png").image, EditorGUIUtility.TrTextContent("Edit Geometry.").text);

        private static GUIContent m_ToolIcon;
        private List<SceneChainGeometryEditorTool> m_GeometryTools;
        private SceneGeometryToolOverlay m_Overlay;

        /// <summary>
        /// Chain Geometry Tool.
        /// </summary>
        private sealed class ChainSceneChainGeometryEditorTool : SceneChainGeometryEditorTool
        {
            public ChainSceneChainGeometryEditorTool(SceneChain sceneChain, IGeometryToolSettings geometryToolSettings) : base(sceneChain, geometryToolSettings)
            { }

            public override void OnToolGUI(EditorWindow window)
            {
                // Get the chain points. 
                var chainPoints = Target.Points;

                // Fetch the point count.
                var pointCount = chainPoints.Length;
                if (pointCount < 4)
                    return;
                
                var currentEvent = Event.current;
                var currentEventType = currentEvent.type;
                
                // Are we in add/remove mode as opposed to move-only mode?
                var addMode = (currentEvent.command || currentEvent.shift);
                var removeMode = pointCount == 4 ? false : addMode;
                
                // Always consume the mouse-drag if we're adding/removing to suppress other function.
                if ((addMode || removeMode) && currentEventType == EventType.MouseDrag)
                    currentEvent.Use();
                
                // Set-up handles.
                var snap = GetSnapSettings();
                var handleDirection = PhysicsMath.GetTranslationIgnoredAxes(TransformPlane);

                // Fetch the show labels option.
                var showLabels = geometryToolSettings.ShowLabels; 
                
                TargetShapeChanged = false;
                
                // Points.
                for (int i = 0, j = pointCount - 1; i < pointCount; j = i++)
                {
                    // Calculate the axis/handle directions.
                    var axisUp = (Vector3)(chainPoints[i] - chainPoints[j]).normalized;
                    var axisRight = new Vector3(axisUp.y, -axisUp.x, 0f);
                    var handleRight = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisRight, TransformPlane)).normalized;
                    var handleUp = Body.rotation.GetMatrix(TransformPlane).MultiplyVector(PhysicsMath.Swizzle(axisUp, TransformPlane)).normalized;

                    {
                        // Fetch the point.
                        var point = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(chainPoints[i]), Target.transform.position, TransformPlane);
                        var handleSize = GetHandleSize(point);

                        // Draw the point handle.
                        using (new Handles.DrawingScope(Matrix4x4.TRS(point, Quaternion.identity, Vector3.one)))
                        {
                            // Set the handle color.
                            Handles.color = removeMode ? geometryToolSettings.GrabHandleDeleteColor : geometryToolSettings.GrabHandleVertexColor;
                            
                            EditorGUI.BeginChangeCheck();

                            var controlId = GUIUtility.GetControlID(FocusType.Passive);
                            var newCenterValue = Handles.Slider2D(controlId, Vector3.zero, handleDirection, handleRight, handleUp, handleSize, Handles.CubeHandleCap, snap);

                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(Target, "Change ChainGeometry Point");
                                chainPoints[i] += Body.rotation.InverseRotateVector(PhysicsMath.ToPosition2D(newCenterValue, TransformPlane));
                                TargetShapeChanged = true;
                            }

                            // Draw vertex label.
                            if (showLabels != IGeometryToolSettings.ShowLabelMode.Off)
                            {
                                Handles.color = geometryToolSettings.LabelColor;
                                var labelVertices = showLabels == IGeometryToolSettings.ShowLabelMode.LocalSpace ? chainPoints[i] : Body.transform.TransformPoint(chainPoints[i]);
                                Handles.Label(handleUp * handleSize * 2f, $"{labelVertices.ToString(LabelFloatFormat)}");
                            }
                            
                            // Did we click to delete the point?
                            if (removeMode && currentEventType == EventType.MouseDown && GUIUtility.hotControl == controlId)
                            {
                                Undo.RecordObject(Target, "Delete ChainGeometry Point");

                                var newPoints = new List<Vector2>(chainPoints);
                                newPoints.RemoveAt(i);
                                chainPoints = newPoints.ToArray();
                                Target.UpdateShape(chainPoints);
                                return;
                            }
                        }
                    }

                    if (addMode)
                    {
                        // Calculate the add point.
                        var midPoint = (chainPoints[i] + chainPoints[j]) * 0.5f;
                        var point = PhysicsMath.ToPosition3D(Body.transform.TransformPoint(midPoint), Target.transform.position, TransformPlane);
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
                                Undo.RecordObject(Target, "Add ChainGeometry Point");

                                var newPoints = new List<Vector2>(chainPoints);
                                newPoints.Insert(i, midPoint);
                                chainPoints = newPoints.ToArray();
                                Target.UpdateShape(chainPoints);
                                return;
                            }
                        }
                    }
                }

                // Update the chain if it changed.
                if (TargetShapeChanged)
                    Target.UpdateShape(chainPoints);
            }
        }
    }
}