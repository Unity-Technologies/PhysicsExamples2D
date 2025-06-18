using UnityEngine;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.U2D.Physics.LowLevelExtras;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [Overlay(typeof(SceneView),
        OverlayId,
        DefaultName,
        defaultDisplay = DefaultVisibility,
        defaultDockZone = DockZone.RightColumn,
        defaultDockPosition = DockPosition.Bottom,
        defaultLayout = Layout.Panel,
        defaultWidth = DefaultWidth + WidthPadding,
        defaultHeight = DefaultHeight + HeightPadding)]
    internal class SceneGeometryToolOverlay : Overlay, ITransientOverlay
    {
        private const string OverlayId = "Scene View/SceneShape Tool";
        private const string DefaultName = "SceneShapeTool";
        private const float DefaultWidth = 250.0f;
        private const float DefaultHeight = 200.0f;
        private const float WidthPadding = 6.0f;
        private const float HeightPadding = 19.0f;
        private const bool DefaultVisibility = false;

        private readonly IGeometryToolSettings m_GeometryToolSettings;

        private Label m_LabelTitle;
        private EnumField m_ShowLabels;
        private ColorField m_LabelColor;
        private ColorField m_GrabHandleVertexColor;
        private ColorField m_GrabHandleMoveAllColor;
        private ColorField m_GrabHandleAddColor;
        private ColorField m_GrabHandleDeleteColor;
        private Button m_ResetButton;
        
        public SceneGeometryToolOverlay()
        {
        }

        public SceneGeometryToolOverlay(IGeometryToolSettings geometryToolSettings)
        {
            m_GeometryToolSettings = geometryToolSettings;

            ReadToolSettings();
        }
        
        public override VisualElement CreatePanelContent()
        {
            // Set-up fields.
            m_LabelTitle = new Label { text = "Geometry Tool Options:", style = { paddingBottom = 8f, left = 2f } };
            m_ShowLabels = new EnumField("Show Labels", m_GeometryToolSettings.ShowLabels);
            m_LabelColor = new ColorField("Label Color") { value = m_GeometryToolSettings.LabelColor };
            m_GrabHandleVertexColor = new ColorField("Vertex Color") { value = m_GeometryToolSettings.GrabHandleVertexColor };
            m_GrabHandleMoveAllColor = new ColorField("Move Color") { value = m_GeometryToolSettings.GrabHandleMoveAllColor };
            m_GrabHandleAddColor = new ColorField("Add Color") { value = m_GeometryToolSettings.GrabHandleAddColor };
            m_GrabHandleDeleteColor = new ColorField("Delete Color") { value = m_GeometryToolSettings.GrabHandleDeleteColor };
            m_ResetButton = new Button() { text = "Reset" };

            // Register value changes.
            m_ShowLabels.RegisterValueChangedCallback(valueChangeEvent => { m_GeometryToolSettings.ShowLabels = (IGeometryToolSettings.ShowLabelMode)valueChangeEvent.newValue; WriteToolSettings(); });
            m_LabelColor.RegisterValueChangedCallback(valueChangeEvent => { m_GeometryToolSettings.LabelColor = valueChangeEvent.newValue; WriteToolSettings(); });
            m_GrabHandleVertexColor.RegisterValueChangedCallback(valueChangeEvent => { m_GeometryToolSettings.GrabHandleVertexColor = valueChangeEvent.newValue; WriteToolSettings(); });
            m_GrabHandleMoveAllColor.RegisterValueChangedCallback(valueChangeEvent => { m_GeometryToolSettings.GrabHandleMoveAllColor = valueChangeEvent.newValue; WriteToolSettings(); });
            m_GrabHandleAddColor.RegisterValueChangedCallback(valueChangeEvent => { m_GeometryToolSettings.GrabHandleAddColor = valueChangeEvent.newValue; WriteToolSettings(); });
            m_GrabHandleDeleteColor.RegisterValueChangedCallback(valueChangeEvent => { m_GeometryToolSettings.GrabHandleDeleteColor = valueChangeEvent.newValue; WriteToolSettings(); });
            m_ResetButton.clicked += ResetToolSettings;
            
            // Add elements.
            var root = new VisualElement { style = { width = new StyleLength(DefaultWidth) } };
            root.Add(m_LabelTitle);
            root.Add(m_ShowLabels);
            root.Add(m_LabelColor);
            root.Add(m_GrabHandleVertexColor);
            root.Add(m_GrabHandleMoveAllColor);
            root.Add(m_GrabHandleAddColor);
            root.Add(m_GrabHandleDeleteColor);
            root.Add(m_ResetButton);
            return root;
        }
        
        private void ReadToolSettings()
        {
            // Read the tool settings.
            m_GeometryToolSettings.ShowLabels = (IGeometryToolSettings.ShowLabelMode)EditorPrefs.GetInt(nameof(m_GeometryToolSettings.ShowLabels), 0);
            m_GeometryToolSettings.LabelColor = GetColor(nameof(m_GeometryToolSettings.LabelColor), Color.cornsilk);
            m_GeometryToolSettings.GrabHandleVertexColor = GetColor(nameof(m_GeometryToolSettings.GrabHandleVertexColor), Color.whiteSmoke);
            m_GeometryToolSettings.GrabHandleMoveAllColor = GetColor(nameof(m_GeometryToolSettings.GrabHandleMoveAllColor), Color.cornflowerBlue);
            m_GeometryToolSettings.GrabHandleAddColor = GetColor(nameof(m_GeometryToolSettings.GrabHandleAddColor), new Color(1f, 1f, 1f, 0.25f));
            m_GeometryToolSettings.GrabHandleDeleteColor = GetColor(nameof(m_GeometryToolSettings.GrabHandleDeleteColor), Color.softRed);
        }
        
        private void WriteToolSettings()
        {
            // Read the tool settings.
            EditorPrefs.SetInt(nameof(m_GeometryToolSettings.ShowLabels), (int)m_GeometryToolSettings.ShowLabels);
            SetColor(nameof(m_GeometryToolSettings.LabelColor), m_GeometryToolSettings.LabelColor);
            SetColor(nameof(m_GeometryToolSettings.GrabHandleVertexColor), m_GeometryToolSettings.GrabHandleVertexColor);
            SetColor(nameof(m_GeometryToolSettings.GrabHandleMoveAllColor), m_GeometryToolSettings.GrabHandleMoveAllColor);
            SetColor(nameof(m_GeometryToolSettings.GrabHandleAddColor), m_GeometryToolSettings.GrabHandleAddColor);
            SetColor(nameof(m_GeometryToolSettings.GrabHandleDeleteColor), m_GeometryToolSettings.GrabHandleDeleteColor);
        }

        private void ResetToolSettings()
        {
            m_ShowLabels.value = IGeometryToolSettings.ShowLabelMode.LocalSpace;
            m_LabelColor.value = Color.cornsilk;
            m_GrabHandleVertexColor.value = Color.whiteSmoke;
            m_GrabHandleMoveAllColor.value = Color.cornflowerBlue;
            m_GrabHandleAddColor.value = new Color(1f, 1f, 1f, 0.25f);
            m_GrabHandleDeleteColor.value = Color.softRed;
        }
        
        private static Color GetColor(string key, Color defaultColor)
        {
            return new Color(
                EditorPrefs.GetFloat(key + "_r", defaultColor.r),
                EditorPrefs.GetFloat(key + "_g", defaultColor.g),
                EditorPrefs.GetFloat(key + "_b", defaultColor.b),
                EditorPrefs.GetFloat(key + "_a", defaultColor.a));
        }

        private static void SetColor(string key, Color color)
        {
            EditorPrefs.SetFloat(key + "_r", color.r);
            EditorPrefs.SetFloat(key + "_g", color.g);
            EditorPrefs.SetFloat(key + "_b", color.b);
            EditorPrefs.SetFloat(key + "_a", color.a);
        }        

        public bool visible { get; set; }
    }
}