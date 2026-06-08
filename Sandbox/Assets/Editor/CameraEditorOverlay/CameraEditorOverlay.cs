//#define SHOW_CAMERA_INFO

#if UNITY_EDITOR && SHOW_CAMERA_INFO

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraEditorOverlay : MonoBehaviour
{
    private struct PendingCapture
    {
        public float size;
        public Vector2 position;
        public string typeName;
    }

    private static readonly Dictionary<string, PendingCapture> s_Pending = new();
    private static bool s_HookRegistered;

    private CameraManipulator m_CameraManipulator;
    private Label m_PositionLabel;
    private Label m_SizeLabel;
    private Button m_SetDefaultButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inject()
    {
        var go = new GameObject("[CameraEditorOverlay]");
        go.AddComponent<CameraEditorOverlay>();
        Object.DontDestroyOnLoad(go);

        if (!s_HookRegistered)
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            s_HookRegistered = true;
        }
    }

    private void Start()
    {
#if UNITY_6000_5_OR_NEWER
        m_CameraManipulator = Object.FindAnyObjectByType<CameraManipulator>();
#else
        m_CameraManipulator = Object.FindFirstObjectByType<CameraManipulator>();
#endif

        var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/Framework/UI/PanelSettings.asset");
        var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/CameraEditorOverlay/CameraEditorOverlay.uxml");

        var doc = gameObject.AddComponent<UIDocument>();
        doc.panelSettings = ps;
        doc.visualTreeAsset = vta;

        var root = doc.rootVisualElement;
        m_PositionLabel = root.Q<Label>("position-label");
        m_SizeLabel = root.Q<Label>("size-label");
        m_SetDefaultButton = root.Q<Button>("set-default-button");
        m_SetDefaultButton.clicked += OnSetAsDefault;
        RefreshButtonText();
    }

    private void Update()
    {
        if (m_PositionLabel == null || Camera.main == null)
            return;

        var cam = Camera.main;
        var pos = (Vector2)cam.transform.position;
        var size = cam.orthographicSize;

        const string h = SandboxUtility.HighlightColor;
        const string e = SandboxUtility.EndHighlightColor;
        m_PositionLabel.text = $"{h}({FormatFloat(pos.x)}, {FormatFloat(pos.y)}){e}";
        m_SizeLabel.text = $"{h}{FormatFloat(size)}{e}";

        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            SnapToIntegers();
    }

    private void SnapToIntegers()
    {
        var cam = Camera.main;
        if (cam == null) return;

        var pos = cam.transform.position;
        cam.transform.position = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), pos.z);

        if (m_CameraManipulator != null)
        {
            var roundedSize = Mathf.Max(1f, Mathf.Round(cam.orthographicSize));
            m_CameraManipulator.CameraZoom = m_CameraManipulator.CameraSize / roundedSize;
        }
    }

    private void OnSetAsDefault()
    {
        if (Camera.main == null)
            return;

#if UNITY_6000_5_OR_NEWER
        var example = Object.FindAnyObjectByType<SandboxExampleBehaviour>();
#else
        var example = Object.FindFirstObjectByType<SandboxExampleBehaviour>();
#endif
        if (example == null)
            return;

        var monoScript = MonoScript.FromMonoBehaviour(example);
        var assetPath = AssetDatabase.GetAssetPath(monoScript);
        if (string.IsNullOrEmpty(assetPath))
            return;

        s_Pending[assetPath] = new PendingCapture
        {
            size = Camera.main.orthographicSize,
            position = (Vector2)Camera.main.transform.position,
            typeName = example.GetType().Name
        };
        RefreshButtonText();
    }

    private void RefreshButtonText()
    {
        if (m_SetDefaultButton == null)
            return;
        m_SetDefaultButton.text = s_Pending.Count > 0
            ? $"Set as Default ({s_Pending.Count} pending)"
            : "Set as Default";
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingPlayMode || s_Pending.Count == 0)
            return;

        foreach (var kvp in s_Pending)
        {
            var assetPath = kvp.Key;
            var capture = kvp.Value;
            var fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
            if (!File.Exists(fullPath))
                continue;

            var original = File.ReadAllText(fullPath);
            var patched = PatchSource(original, capture);
            if (patched != original)
                File.WriteAllText(fullPath, patched);
        }

        s_Pending.Clear();
        AssetDatabase.Refresh();
    }

    private static string PatchSource(string src, PendingCapture cap)
    {
        var sizeStr = $"{FormatFloat(cap.size)}f";
        var posStr = $"new({FormatFloat(cap.position.x)}f, {FormatFloat(cap.position.y)}f)";

        const string sizeRx = @"protected override float CameraSize\s*=>\s*[^;]+;";
        const string posRx = @"protected override Vector2 CameraPosition\s*=>\s*[^;]+;";

        bool hadSize = Regex.IsMatch(src, sizeRx);
        bool hadPos = Regex.IsMatch(src, posRx);

        var result = src;
        if (hadSize)
            result = Regex.Replace(result, sizeRx, $"protected override float CameraSize => {sizeStr};");
        if (hadPos)
            result = Regex.Replace(result, posRx, $"protected override Vector2 CameraPosition => {posStr};");

        if (!hadSize || !hadPos)
        {
            var insertText = "";
            if (!hadSize)
                insertText += $"\n    protected override float CameraSize => {sizeStr};";
            if (!hadPos)
                insertText += $"\n    protected override Vector2 CameraPosition => {posStr};";

            var classRx = @"(class\s+" + Regex.Escape(cap.typeName) + @"\b[^{]*\{)";
            result = Regex.Replace(result, classRx, $"$1{insertText}");
        }

        return result;
    }

    private static string FormatFloat(float v) => v.ToString("0.##");
}

#endif
