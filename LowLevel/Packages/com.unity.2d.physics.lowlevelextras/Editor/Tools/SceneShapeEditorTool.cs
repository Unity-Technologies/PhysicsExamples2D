using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [EditorTool("Edit Scene Shape", typeof(SceneShape))]
    internal sealed partial class SceneShapeEditorTool : EditorTool, IGeometryToolSettings
    {
        public override void OnActivated()
        {
            // Create overlay.
            m_Overlay = new SceneGeometryToolOverlay(this) { visible = true };
            SceneView.AddOverlayToActiveView(m_Overlay);
            
            // Create the appropriate geometry tools.
            m_SceneShapeTargets = new List<SceneShape>(capacity: 1);
            m_GeometryTools = new List<SceneShapeGeometryTool>(capacity: 1);
            foreach (var toolTarget in targets)
            {
                if (toolTarget is SceneShape sceneShape)
                {
                    if (sceneShape.isActiveAndEnabled)
                    {
                        m_SceneShapeTargets.Add(sceneShape);
                        m_GeometryTools.Add(CreateTool(sceneShape));
                    }
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
            for(var i = 0; i < m_GeometryTools.Count; ++i)
            {
                // Fetch the geometry tool.
                var geometryTool = m_GeometryTools[i];
                
                // Update the tool in-case the target has been recreated or changed.
                if (!geometryTool.UpdateTool())
                {
                    // It seems the tool failed due to shape-type change so recreate it.
                    geometryTool = CreateTool(m_SceneShapeTargets[i]);
                    geometryTool.UpdateTool();
                }

                // Let the tool handle the callback if it's valid.
                if (geometryTool.isValid)
                    geometryTool.OnToolGUI(window);
            }

            // If we're not playing then we should queue a player update.
            // NOTE: We do this, so we'll render the gizmos.
            if (!EditorApplication.isPlaying)
                EditorApplication.QueuePlayerLoopUpdate();
        }

        private SceneShapeGeometryTool CreateTool(SceneShape sceneShape)
        {
            return sceneShape.Shape.shapeType switch
            {
                PhysicsShape.ShapeType.Circle => new CircleShapeGeometryTool(sceneShape, this),
                PhysicsShape.ShapeType.Capsule => new CapsuleShapeGeometryTool(sceneShape, this),
                PhysicsShape.ShapeType.Segment => new SegmentShapeGeometryTool(sceneShape, this),
                PhysicsShape.ShapeType.ChainSegment => new ChainSegmentShapeGeometryTool(sceneShape, this),
                PhysicsShape.ShapeType.Polygon => new PolygonShapeGeometryTool(sceneShape, this),
                _ => throw new ArgumentOutOfRangeException(nameof(sceneShape.Shape.shapeType), sceneShape.Shape.shapeType, null)
            };
        }
        
        public IGeometryToolSettings.ShowLabelMode ShowLabels { get; set; }
        public Color LabelColor { get; set; }
        public Color GrabHandleVertexColor { get; set; }
        public Color GrabHandleMoveAllColor { get; set; }
        public Color GrabHandleAddColor { get; set; }
        public Color GrabHandleDeleteColor { get; set; }

        public override GUIContent toolbarIcon => m_ToolIcon ??= new GUIContent(EditorGUIUtility.IconContent(IconUtility.IconPath + "SceneShapeTool.png").image, EditorGUIUtility.TrTextContent("Edit Geometry.").text);
        private static GUIContent m_ToolIcon;
        private List<SceneShape> m_SceneShapeTargets;
        private List<SceneShapeGeometryTool> m_GeometryTools;
        private SceneGeometryToolOverlay m_Overlay;
        
        #region Geometry Tools
        
        /// <summary>
        /// Abstract Geometry Tool.
        /// </summary>
        private abstract class SceneShapeGeometryTool : SceneGeometryTool
        {
            protected SceneShapeGeometryTool(SceneShape sceneShape, IGeometryToolSettings geometryToolSettings) : base(geometryToolSettings)
            {
                ShapeTarget = sceneShape;
                ShapeType = sceneShape.ShapeType;
            
                UpdateTool();
            }

            protected readonly SceneShape ShapeTarget;
            protected readonly PhysicsShape.ShapeType ShapeType;
            protected PhysicsShape Shape;
            protected PhysicsBody Body;
            protected PhysicsWorld World;
            protected PhysicsWorld.TransformPlane TransformPlane;
            protected bool TargetShapeChanged;

            public sealed override bool UpdateTool()
            {
                TargetShapeChanged = false;

                // Fail if the shape type changed.
                if (ShapeTarget.ShapeType != ShapeType)
                    return false;
                
                if (Shape.isValid)
                    return true;
            
                Shape = ShapeTarget.Shape;
                if (!Shape.isValid)
                    return true;
                
                Body = Shape.body;
                World = Shape.world;
                TransformPlane = World.transformPlane;
                return true;
            }
        
            /// <summary>
            /// Check the conditions of the target to ensure it's valid to edit or not.
            /// </summary>
            /// <returns>If the target is valid to edit or not.</returns>
            public override bool isValid => ShapeTarget != null && Shape.isValid && ShapeTarget.isActiveAndEnabled && !Mathf.Approximately(Vector3.Scale(PhysicsMath.GetTranslationAxes(World.transformPlane), ShapeTarget.transform.lossyScale).sqrMagnitude, 0.0f);
        }

        #endregion
    }
}