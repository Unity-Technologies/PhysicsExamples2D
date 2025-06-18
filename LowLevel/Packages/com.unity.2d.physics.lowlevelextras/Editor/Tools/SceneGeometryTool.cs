using UnityEngine;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    internal interface IGeometryToolSettings
    {
        public enum ShowLabelMode
        {
            Off = 0,
            LocalSpace = 1,
            WorldSpace = 2
        }        
        
        ShowLabelMode ShowLabels { get; set; }
        Color LabelColor { get; set; }
        Color GrabHandleVertexColor { get; set; }
        Color GrabHandleMoveAllColor { get; set; }
        Color GrabHandleAddColor { get; set; }
        Color GrabHandleDeleteColor { get; set; }
    }
    
    internal abstract class SceneGeometryTool
    {
        protected SceneGeometryTool(IGeometryToolSettings geometryToolSettings)
        {
            this.geometryToolSettings = geometryToolSettings;
        }
            
        public abstract void OnToolGUI(EditorWindow window);
        public abstract bool UpdateTool();
        public abstract bool isValid { get; }
        
        protected IGeometryToolSettings geometryToolSettings { get; set; } 
        
        protected static float GetHandleSize(Vector3 position) => HandleUtility.GetHandleSize(position) * 0.075f;
        protected static Vector3 GetSnapSettings() => EditorSnapSettings.move;
        protected static string LabelFloatFormat => "F4";
        protected const float MinimumRadii = 0.001f;
    }
}