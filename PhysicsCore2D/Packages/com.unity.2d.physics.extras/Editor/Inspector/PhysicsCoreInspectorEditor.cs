using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    public sealed class PhysicsCoreInspectorEditor : EditorWindow
    {
        private const string MenuPath = "Tools/PhysicsCore2D/Inspector";
        private const string AutoSelectGameObjectPrefKey = nameof(PhysicsCoreInspectorEditor) + ".AutoSelectGameObject";
        private const string AutoReadIntervalPrefKey = nameof(PhysicsCoreInspectorEditor) + ".AutoReadInterval";

        // Auto Read dropdown options. Label shown to the user, period in ms (0 = off).
        private static readonly (string label, int periodMs)[] s_AutoReadOptions =
        {
            ("Off",     0),
            ("100ms",   100),
            ("250ms",   250),
            ("500ms",   500),
            ("1000ms",  1000),
        };
        private const int DefaultAutoReadIntervalMs = 250;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            var window = GetWindow<PhysicsCoreInspectorEditor>("PhysicsCoreInspector2D");
            window.minSize = new Vector2(560f, 320f);
            window.titleContent = new GUIContent(
                "PhysicsCoreInspector2D",
                EditorGUIUtility.IconContent("console.infoicon").image);
        }

        private enum NodeKind
        {
            Root,
            World,
            Body,
            TransformInfo,
            Shape,
            Joint,
            GeometryInfo,
            WorldCounters,
            WorldProfile,
            GlobalCounters,
            GlobalProfile,
            ShapeContacts,        // "Contacts" container under a Shape (always present).
            ShapeTriggers,        // "Triggers" container under a Shape (only when shape.isTrigger).
            ShapeContactEntry,    // A single contact this shape participates in.
            ShapeTriggerVisitor,  // A single visitor shape currently overlapping this trigger.
        }

        private sealed class Node
        {
            public NodeKind kind;
            public string label;
            public PhysicsWorld world;
            public PhysicsBody body;
            public PhysicsShape shape;
            public PhysicsJoint joint;
            public Transform transformObject;
            public bool deleted;

            // Per-shape Contacts/Triggers extras.
            public PhysicsShape.ContactId contactId;  // ShapeContactEntry only.
            public PhysicsShape secondShape;          // ShapeContactEntry: cached "other" shape; ShapeTriggerVisitor: visitor shape.
        }

        private TreeView m_TreeView;
        private VisualElement m_Inspector;
        private Button m_RefreshButton;
        private Button m_SelectGameObjectButton;
        private Button m_FindSelectionButton;
        private Button m_BackButton;
        private Button m_ForwardButton;
        private Toggle m_AutoSelectToggle;
        private DropdownField m_AutoReadDropdown;
        private IVisualElementScheduledItem m_AutoReadScheduledItem;

        // Browser-style selection history. Entries describe a node by (kind + handles), so they
        // can survive a tree Reload — we re-resolve the actual tree id at navigation time.
        private struct HistoryEntry
        {
            public NodeKind kind;
            public PhysicsWorld world;
            public PhysicsBody body;
            public PhysicsShape shape;
            public PhysicsJoint joint;
            public PhysicsShape.ContactId contactId;
            public PhysicsShape secondShape;
            public bool isBeginEvent;
        }
        private const int HistoryCapacity = 64;
        private readonly List<HistoryEntry> m_History = new List<HistoryEntry>();
        private int m_HistoryIndex = -1;
        // Set true while navigating via Back/Forward to suppress the selectionChanged handler
        // pushing a duplicate entry onto history.
        private bool m_NavigatingHistory;
        // Tracks the most recent GameObject involved in an auto-sync — set whenever we mirror
        // a selection in either direction. The reverse handler short-circuits when the incoming
        // value matches, breaking the otherwise-infinite ping-pong.
        private GameObject m_LastSyncedGO;

        private List<TreeViewItemData<Node>> m_TreeData;
        private int m_NextItemId;

        // Selection persistence across Refresh().
        private NodeKind m_PendingKind;
        private PhysicsWorld m_PendingWorld;
        private PhysicsBody m_PendingBody;
        private PhysicsShape m_PendingShape;
        private PhysicsJoint m_PendingJoint;
        private PhysicsShape.ContactId m_PendingContactId;
        private PhysicsShape m_PendingSecondShape;
        private bool m_HasPendingSelection;

        // Single backing ScriptableObject for every PropertyField the inspector binds. PropertyField
        // only renders the bound property path, so unused fields are dormant. See PhysicsInspectorHolder.cs.
        private PhysicsInspectorHolder m_Holder;

        // Icons loaded once from the package's Editor/Inspector/Icons/ folder.
        private const string IconFolder = "Packages/com.unity.2d.physics.extras/Editor/Inspector/Icons/";
        private Texture2D m_IconWorld;
        private Texture2D m_IconBody;
        private Texture2D m_IconShape;
        private Texture2D m_IconChain;
        private Texture2D m_IconDistanceJoint;
        private Texture2D m_IconFixedJoint;
        private Texture2D m_IconHingeJoint;
        private Texture2D m_IconIgnoreJoint;
        private Texture2D m_IconRelativeJoint;
        private Texture2D m_IconSliderJoint;
        private Texture2D m_IconWheelJoint;
        private Texture2D m_IconTransform;
        private Texture2D m_IconError;
        private Texture2D m_IconShapeGeometry;
        private Texture2D m_IconRoot;
        private Texture2D m_IconInfo;

        private void CreateGUI()
        {
            m_Holder = CreateHolder<PhysicsInspectorHolder>();

            LoadIcons();

            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            var buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.paddingLeft = 4f;
            buttonRow.style.paddingRight = 4f;
            buttonRow.style.paddingTop = 4f;
            buttonRow.style.paddingBottom = 4f;
            buttonRow.style.borderBottomWidth = 1f;
            buttonRow.style.borderBottomColor = new Color(0f, 0f, 0f, 0.3f);
            root.Add(buttonRow);

            m_SelectGameObjectButton = new Button(OnSelectGameObject) { text = "Select GameObject" };
            m_SelectGameObjectButton.SetEnabled(false);
            buttonRow.Add(m_SelectGameObjectButton);

            m_FindSelectionButton = new Button(OnFindSelection) { text = "Find Selection" };
            buttonRow.Add(m_FindSelectionButton);

            // Auto Select sits with the two related selection buttons on the left.
            // Toggle.text renders the label adjacent to the checkbox; using the constructor with
            // a label string would route through BaseField's label which has a wide min-width.
            m_AutoSelectToggle = new Toggle
            {
                text = "Auto Select",
                value = EditorPrefs.GetBool(AutoSelectGameObjectPrefKey, defaultValue: true),
            };
            m_AutoSelectToggle.style.alignSelf = Align.Center;
            m_AutoSelectToggle.style.marginLeft = 8f;
            m_AutoSelectToggle.tooltip =
                "When enabled, selecting a tree node automatically performs 'Select GameObject' " +
                "if the node resolves to a GameObject (via owner or transformObject).";
            m_AutoSelectToggle.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetBool(AutoSelectGameObjectPrefKey, evt.newValue);
                // Toggling on disables the manual buttons (their job is now automatic);
                // toggling off re-enables them. Also kicks off an immediate sync if turning on.
                UpdateSelectionDependentUI();
                OnUnitySelectionChanged();
            });
            buttonRow.Add(m_AutoSelectToggle);

            // Flex-grow spacer on each side of the Auto Read group centres it between the left
            // selection cluster and the right-aligned Reload button.
            buttonRow.Add(new VisualElement { style = { flexGrow = 1f } });

            // "Auto Read" dropdown — centred. Tight prefix Label so the dropdown's BaseField
            // alignment doesn't push the chooser away.
            var autoReadGroup = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                },
            };
            autoReadGroup.Add(new Label("Auto Read") { style = { marginRight = 4f } });

            var autoReadLabels = new List<string>(s_AutoReadOptions.Length);
            foreach (var (label, _) in s_AutoReadOptions) autoReadLabels.Add(label);
            var savedIntervalMs = EditorPrefs.GetInt(AutoReadIntervalPrefKey, DefaultAutoReadIntervalMs);
            m_AutoReadDropdown = new DropdownField(autoReadLabels, AutoReadLabelForInterval(savedIntervalMs));
            m_AutoReadDropdown.style.minWidth = 70f;
            m_AutoReadDropdown.tooltip =
                "How often to re-read the selected definition (World/Body/Shape/Geometry). " +
                "Only active in Play mode — in Edit mode use the explicit Read buttons. " +
                "'Off' disables. Pick a longer period when editing values to avoid clobbering edits.";
            m_AutoReadDropdown.RegisterValueChangedCallback(evt =>
                ApplyAutoReadInterval(AutoReadIntervalForLabel(evt.newValue)));
            autoReadGroup.Add(m_AutoReadDropdown);
            // Disable the whole group (label + dropdown) so the prefix Label also greys out.
            autoReadGroup.SetEnabled(EditorApplication.isPlaying);
            buttonRow.Add(autoReadGroup);

            buttonRow.Add(new VisualElement { style = { flexGrow = 1f } });

            m_BackButton = MakeNavButton(LoadArrowIcon(left: true), "Go to previous selection", OnBackClicked);
            m_ForwardButton = MakeNavButton(LoadArrowIcon(left: false), "Go to next selection", OnForwardClicked);
            buttonRow.Add(m_BackButton);
            buttonRow.Add(m_ForwardButton);
            UpdateHistoryButtons();

            m_RefreshButton = new Button(Refresh) { text = "Reload" };
            buttonRow.Add(m_RefreshButton);

            var splitView = new TwoPaneSplitView(0, 320f, TwoPaneSplitViewOrientation.Horizontal);
            splitView.style.flexGrow = 1f;
            root.Add(splitView);

            var leftPane = new VisualElement();
            leftPane.style.minWidth = 220f;
            splitView.Add(leftPane);

            m_TreeView = new TreeView
            {
                fixedItemHeight = 20f,
                selectionType = SelectionType.Single,
                style = { flexGrow = 1f },
            };
            m_TreeView.makeItem = MakeTreeRow;
            m_TreeView.bindItem = BindTreeRow;
            m_TreeView.selectionChanged += OnTreeSelectionChanged;
            m_TreeView.itemsChosen += OnTreeItemsChosen;
            leftPane.Add(m_TreeView);

            var rightPane = new VisualElement();
            rightPane.style.minWidth = 240f;
            splitView.Add(rightPane);

            var inspectorScroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal)
            {
                style = { flexGrow = 1f },
            };
            rightPane.Add(inspectorScroll);

            m_Inspector = new VisualElement();
            m_Inspector.style.paddingLeft = 6f;
            m_Inspector.style.paddingRight = 6f;
            m_Inspector.style.paddingTop = 6f;
            m_Inspector.style.paddingBottom = 6f;
            inspectorScroll.Add(m_Inspector);

            // Periodic auto-refresh tick. Bound to m_Inspector so it dies with the window.
            // Counters/Profile panels (per-world and global) re-render on each tick to surface
            // live values without the user clicking Read. All other selections are a no-op.
            m_Inspector.schedule.Execute(AutoRefreshTick).Every(250);

            // Honour the saved Auto Read interval — sets up the scheduled item once m_Inspector
            // is alive. Subsequent dropdown changes route through ApplyAutoReadInterval too.
            ApplyAutoReadInterval(EditorPrefs.GetInt(AutoReadIntervalPrefKey, DefaultAutoReadIntervalMs));

            Selection.selectionChanged += OnUnitySelectionChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            OnUnitySelectionChanged();

            Refresh();
        }

        private void AutoRefreshTick()
        {
            // Skip while the editor is paused — counters/profile values don't change so a refresh
            // is wasted work and would also cause the inspector to flicker on every tick.
            if (EditorApplication.isPaused) return;

            var node = CurrentSelectedNode();
            if (node == null) return;

            // Counters/profile and per-shape Contacts/Triggers panels always tick at the
            // AutoRefreshTick cadence (250ms). The tick re-renders the right panel only — it
            // never rebuilds tree children (that would flicker the tree).
            switch (node.kind)
            {
                case NodeKind.WorldCounters:
                case NodeKind.WorldProfile:
                case NodeKind.GlobalCounters:
                case NodeKind.GlobalProfile:
                case NodeKind.ShapeContacts:
                case NodeKind.ShapeTriggers:
                case NodeKind.ShapeContactEntry:
                case NodeKind.ShapeTriggerVisitor:
                    DisplayInspectorForCurrentSelection();
                    break;
            }
        }

        // Independently-scheduled tick for definition Auto Read. Period is user-configurable
        // (100/250/500/1000ms) — see ApplyAutoReadInterval below for the schedule wiring.
        // Auto Read only operates in Play mode; in Edit mode users rely on explicit Read buttons.
        private void AutoReadTick()
        {
            if (!EditorApplication.isPlaying) return;
            if (EditorApplication.isPaused) return;

            var node = CurrentSelectedNode();
            if (node == null) return;

            switch (node.kind)
            {
                case NodeKind.World:
                case NodeKind.Body:
                case NodeKind.Shape:
                case NodeKind.GeometryInfo:
                    DisplayInspectorForCurrentSelection();
                    break;
            }
        }

        // Persists the chosen period and (re)configures the scheduled task accordingly. Called
        // once during CreateGUI to honour the saved pref, and on every dropdown change.
        private void ApplyAutoReadInterval(int intervalMs)
        {
            EditorPrefs.SetInt(AutoReadIntervalPrefKey, intervalMs);

            if (m_Inspector == null) return;
            if (m_AutoReadScheduledItem == null)
                m_AutoReadScheduledItem = m_Inspector.schedule.Execute(AutoReadTick);

            if (intervalMs <= 0)
            {
                m_AutoReadScheduledItem.Pause();
            }
            else
            {
                m_AutoReadScheduledItem.Every(intervalMs);
                m_AutoReadScheduledItem.Resume();
            }
        }

        private static int AutoReadIntervalForLabel(string label)
        {
            foreach (var (l, ms) in s_AutoReadOptions)
                if (l == label) return ms;
            return 0;
        }

        private static string AutoReadLabelForInterval(int intervalMs)
        {
            foreach (var (l, ms) in s_AutoReadOptions)
                if (ms == intervalMs) return l;
            return s_AutoReadOptions[0].label; // fallback to "Off"
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            // Worlds get torn down and recreated across play-mode transitions; the tree must be
            // rebuilt or it will keep referencing the old (now invalid) handles.
            if (change == PlayModeStateChange.EnteredEditMode || change == PlayModeStateChange.EnteredPlayMode)
            {
                // Toggle the whole "Auto Read: <dropdown>" group so the prefix Label greys out too.
                if (m_AutoReadDropdown?.parent != null)
                    m_AutoReadDropdown.parent.SetEnabled(change == PlayModeStateChange.EnteredPlayMode);
                if (m_TreeView != null)
                    Refresh();
            }
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnUnitySelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            DestroyImmediateIfNotNull(m_Holder);
        }

        private static void DestroyImmediateIfNotNull(Object obj)
        {
            if (obj != null)
                DestroyImmediate(obj);
        }

        // ScriptableObject instances created with CreateInstance are torn down by Unity across
        // play-mode transitions (and similar editor lifecycle events) unless they are tagged as
        // don't-save. Without this, the holder's underlying SO becomes destroyed while the C#
        // field still references it, causing "Object at index 0 is null" from
        // `new SerializedObject(holder)`.
        //
        // CRITICAL: do NOT use HideFlags.HideAndDontSave here — that combination includes
        // NotEditable, which causes every PropertyField bound to the holder to render as disabled.
        // HideFlags.DontSave gives us persistence (DontSaveInEditor | DontSaveInBuild |
        // DontUnloadUnusedAsset) without the NotEditable bit, so fields stay editable.
        private static T CreateHolder<T>() where T : ScriptableObject
        {
            var so = ScriptableObject.CreateInstance<T>();
            so.hideFlags = HideFlags.DontSave;
            return so;
        }

        private void LoadIcons()
        {
            m_IconWorld = LoadIcon("PhysicsWorld");
            m_IconBody = LoadIcon("PhysicsBody");
            m_IconShape = LoadIcon("PhysicsShape");
            m_IconChain = LoadIcon("PhysicsChain");
            m_IconDistanceJoint = LoadIcon("PhysicsDistanceJoint");
            m_IconFixedJoint = LoadIcon("PhysicsFixedJoint");
            m_IconHingeJoint = LoadIcon("PhysicsHingeJoint");
            m_IconIgnoreJoint = LoadIcon("PhysicsIgnoreJoint");
            m_IconRelativeJoint = LoadIcon("PhysicsRelativeJoint");
            m_IconSliderJoint = LoadIcon("PhysicsSliderJoint");
            m_IconWheelJoint = LoadIcon("PhysicsWheelJoint");
            m_IconShapeGeometry = LoadIcon("PhysicsShapeGeometry");
            m_IconRoot = EditorGUIUtility.IconContent("UnityLogo")?.image as Texture2D;
            m_IconInfo = EditorGUIUtility.IconContent("console.infoicon")?.image as Texture2D;
            m_IconTransform = EditorGUIUtility.IconContent("Transform Icon").image as Texture2D;
            m_IconError = EditorGUIUtility.IconContent("console.erroricon").image as Texture2D;
        }

        private static Texture2D LoadIcon(string name)
            => AssetDatabase.LoadAssetAtPath<Texture2D>(IconFolder + name + ".png");

        // Tries a list of Unity built-in icon names in order and returns the first one that
        // resolves to a non-null texture. Different Unity versions expose these under different
        // names; this picks whichever exists in the running editor.
        private static Texture2D LoadArrowIcon(bool left)
        {
            // ArrowNavigation* are the modern (Unity 2022+) browser-style arrows. tab_prev/_next
            // and Animation.Prev/NextKey are widely-available older fallbacks.
            string[] candidates = left
                ? new[] { "ArrowNavigationLeft", "tab_prev", "Animation.PrevKey", "back" }
                : new[] { "ArrowNavigationRight", "tab_next", "Animation.NextKey", "forward" };
            foreach (var name in candidates)
            {
                var img = EditorGUIUtility.IconContent(name)?.image as Texture2D;
                if (img != null) return img;
            }
            return null;
        }

        // Compact icon-only button used for the Back/Forward navigation pair. Falls back to a
        // Unicode arrow if the requested icon couldn't be resolved.
        private static Button MakeNavButton(Texture2D icon, string tooltip, System.Action onClick)
        {
            var btn = new Button(onClick);
            btn.tooltip = tooltip;
            // Compact: roughly square, no extra horizontal padding.
            btn.style.width = 24f;
            btn.style.paddingLeft = 0f;
            btn.style.paddingRight = 0f;
            if (icon != null)
            {
                btn.iconImage = Background.FromTexture2D(icon);
                var iconImg = btn.Q<Image>(className: Button.imageUSSClassName);
                if (iconImg != null)
                {
                    iconImg.style.width = 14f;
                    iconImg.style.height = 14f;
                }
            }
            else
            {
                // No matching built-in icon — fall back to a Unicode arrow glyph. Use the
                // tooltip's first char to disambiguate left vs right (cheap and self-contained).
                btn.text = tooltip.StartsWith("Go to previous") ? "◀" : "▶";
            }
            return btn;
        }

        private static VisualElement MakeTreeRow()
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                },
            };
            var icon = new Image
            {
                name = "icon",
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 16f,
                    height = 16f,
                    marginRight = 4f,
                    flexShrink = 0f,
                },
            };
            var label = new Label
            {
                name = "label",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    flexGrow = 1f,
                },
            };
            row.Add(icon);
            row.Add(label);
            return row;
        }

        private void BindTreeRow(VisualElement element, int index)
        {
            var node = m_TreeView.GetItemDataForIndex<Node>(index);
            var icon = element.Q<Image>("icon");
            var label = element.Q<Label>("label");

            element.tooltip = GetTooltipForNode(node);

            if (node != null && node.deleted)
            {
                label.text = (node.label ?? "<null>") + " (Deleted)";
                icon.image = m_IconError;
                icon.style.display = DisplayStyle.Flex;
                return;
            }

            label.text = node?.label ?? "<null>";

            var iconTexture = GetIconForNode(node);
            icon.image = iconTexture;
            icon.style.display = iconTexture != null ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // PhysicsCore2D handle structs all override ToString() with the underlying handle details
        // (slot index / generation / world id, etc). Surface that as the row's tooltip so the user
        // can hover any node to see exactly which handle it represents.
        private static string GetTooltipForNode(Node node)
        {
            if (node == null) return string.Empty;
            switch (node.kind)
            {
                case NodeKind.World: return node.world.isValid ? node.world.ToString() : string.Empty;
                case NodeKind.Body:  return node.body.isValid  ? node.body.ToString()  : string.Empty;
                case NodeKind.Shape: return node.shape.isValid ? node.shape.ToString() : string.Empty;
                case NodeKind.Joint: return node.joint.isValid ? node.joint.ToString() : string.Empty;
                case NodeKind.TransformInfo:
                    return node.transformObject != null
                        ? $"Entity ID: {node.transformObject.gameObject.GetEntityId()}"
                        : string.Empty;
                default: return string.Empty;
            }
        }

        private Texture2D GetIconForNode(Node node)
        {
            if (node == null)
                return null;

            switch (node.kind)
            {
                case NodeKind.Root: return m_IconRoot;
                case NodeKind.World: return m_IconWorld;
                case NodeKind.Body: return m_IconBody;
                case NodeKind.TransformInfo: return m_IconTransform;
                case NodeKind.Shape:
                    return node.shape.isValid && node.shape.shapeType == PhysicsShape.ShapeType.ChainSegment
                        ? m_IconChain
                        : m_IconShape;
                case NodeKind.Joint: return GetIconForJointType(node.joint.isValid ? node.joint.jointType : default);
                case NodeKind.GeometryInfo: return m_IconShapeGeometry;
                case NodeKind.WorldCounters: return m_IconInfo;
                case NodeKind.WorldProfile: return m_IconInfo;
                case NodeKind.GlobalCounters: return m_IconInfo;
                case NodeKind.GlobalProfile: return m_IconInfo;
                case NodeKind.ShapeContacts: return m_IconInfo;
                case NodeKind.ShapeTriggers: return m_IconInfo;
                case NodeKind.ShapeContactEntry: return m_IconInfo;
                case NodeKind.ShapeTriggerVisitor: return m_IconInfo;
                default: return null;
            }
        }

        private Texture2D GetIconForJointType(PhysicsJoint.JointType type)
        {
            switch (type)
            {
                case PhysicsJoint.JointType.DistanceJoint: return m_IconDistanceJoint;
                case PhysicsJoint.JointType.FixedJoint: return m_IconFixedJoint;
                case PhysicsJoint.JointType.HingeJoint: return m_IconHingeJoint;
                case PhysicsJoint.JointType.IgnoreJoint: return m_IconIgnoreJoint;
                case PhysicsJoint.JointType.RelativeJoint: return m_IconRelativeJoint;
                case PhysicsJoint.JointType.SliderJoint: return m_IconSliderJoint;
                case PhysicsJoint.JointType.WheelJoint: return m_IconWheelJoint;
                default: return null;
            }
        }

        private void OnUnitySelectionChanged()
        {
            // When Auto Select is on, the GameObject → tree sync runs on every selection change,
            // so the manual Find Selection button is redundant — disable it for clarity.
            var autoSelectOn = m_AutoSelectToggle != null && m_AutoSelectToggle.value;
            if (m_FindSelectionButton != null)
                m_FindSelectionButton.SetEnabled(!autoSelectOn && Selection.activeGameObject != null);

            // Auto-sync: GameObject → tree. Skip if the change is the echo of our own
            // tree → GameObject sync (m_LastSyncedGO already matches).
            if (m_AutoSelectToggle == null || !m_AutoSelectToggle.value) return;
            var currentGO = Selection.activeGameObject;
            if (ReferenceEquals(currentGO, m_LastSyncedGO)) return;
            m_LastSyncedGO = currentGO;
            if (currentGO != null)
                TrySelectForEditorSelection();
        }

        private void Refresh()
        {
            CapturePendingSelection();

            m_TreeData = new List<TreeViewItemData<Node>>();
            m_NextItemId = 1;

            var rootNode = new Node { kind = NodeKind.Root, label = string.Empty };
            var rootChildren = new List<TreeViewItemData<Node>>();

            BuildWorldChildren(rootChildren);

            // Global counters/profile (summed across all worlds) sit below the per-world subtrees.
            rootChildren.Add(new TreeViewItemData<Node>(NextId(),
                new Node { kind = NodeKind.GlobalCounters, label = "Counters (Global)" }));
            rootChildren.Add(new TreeViewItemData<Node>(NextId(),
                new Node { kind = NodeKind.GlobalProfile, label = "Profile (Global)" }));

            m_TreeData.Add(new TreeViewItemData<Node>(NextId(), rootNode, rootChildren));

            m_TreeView.SetRootItems(m_TreeData);
            m_TreeView.Rebuild();
            m_TreeView.ExpandItem(m_TreeData[0].id);

            // Prefer any node owned by the current Editor GameObject selection (if any);
            // otherwise fall back to restoring the previous browser selection.
            if (!TrySelectForEditorSelection())
                TryRestoreSelection();
            UpdateSelectionDependentUI();
        }

        // Searches the current tree for a node tied to Selection.activeGameObject and selects it.
        // Pass order: World > Body > Shape > Joint by GetOwner(); Body.transformObject is the
        // final fallback. Returns true on a hit.
        private bool TrySelectForEditorSelection()
        {
            var target = Selection.activeGameObject;
            if (target == null) return false;

            // Owner passes, ordered by node kind priority.
            foreach (var kind in s_OwnerSearchOrder)
            {
                var found = FindNodeIdMatching(n => n.kind == kind && OwnerGameObjectFor(n) == target);
                if (found.HasValue) { SelectNodeById(found.Value); return true; }
            }

            // Fallback: a Body that has no explicit owner but renders into this GameObject.
            var bodyFallback = FindNodeIdMatching(n =>
                n.kind == NodeKind.Body
                && n.body.isValid
                && n.body.transformObject != null
                && n.body.transformObject.gameObject == target);
            if (bodyFallback.HasValue) { SelectNodeById(bodyFallback.Value); return true; }

            return false;
        }

        private static readonly NodeKind[] s_OwnerSearchOrder =
        {
            NodeKind.World, NodeKind.Body, NodeKind.Shape, NodeKind.Joint,
        };

        private static GameObject OwnerGameObjectFor(Node node)
        {
            if (node == null) return null;
            Object owner = null;
            switch (node.kind)
            {
                case NodeKind.World: if (node.world.isValid) owner = node.world.GetOwner(); break;
                case NodeKind.Body:  if (node.body.isValid)  owner = node.body.GetOwner();  break;
                case NodeKind.Shape: if (node.shape.isValid) owner = node.shape.GetOwner(); break;
                case NodeKind.Joint: if (node.joint.isValid) owner = node.joint.GetOwner(); break;
                default: return null;
            }
            return OwnerAsGameObject(owner);
        }

        private int? FindNodeIdMatching(System.Func<Node, bool> predicate)
        {
            if (m_TreeData == null) return null;
            foreach (var rootItem in m_TreeData)
            {
                var found = FindItemId(rootItem, item => item.data != null && predicate(item.data));
                if (found.HasValue) return found;
            }
            return null;
        }

        private int NextId() => m_NextItemId++;

        private void BuildWorldChildren(List<TreeViewItemData<Node>> destination)
        {
            using var worlds = PhysicsWorld.GetWorlds(Allocator.Temp);

            // Move the default world to the front.
            var ordered = new List<PhysicsWorld>(worlds.Length);
            int defaultIndex = -1;
            for (int i = 0; i < worlds.Length; ++i)
            {
                if (worlds[i].isDefaultWorld)
                {
                    defaultIndex = i;
                    break;
                }
            }
            if (defaultIndex >= 0)
                ordered.Add(worlds[defaultIndex]);
            for (int i = 0; i < worlds.Length; ++i)
            {
                if (i == defaultIndex) continue;
                ordered.Add(worlds[i]);
            }

            foreach (var world in ordered)
            {
                var worldHandleIndex = ExtractHandleIndex(world.ToString());
                var label = world.isDefaultWorld
                    ? $"World #{worldHandleIndex} (default)"
                    : $"World #{worldHandleIndex}";
                label += OwnerNameSuffix(world.GetOwner());

                var worldNode = new Node { kind = NodeKind.World, label = label, world = world };
                var worldChildren = new List<TreeViewItemData<Node>>();

                var countersNode = new Node
                {
                    kind = NodeKind.WorldCounters,
                    label = "Counters",
                    world = world,
                };
                worldChildren.Add(new TreeViewItemData<Node>(NextId(), countersNode));

                var profileNode = new Node
                {
                    kind = NodeKind.WorldProfile,
                    label = "Profile",
                    world = world,
                };
                worldChildren.Add(new TreeViewItemData<Node>(NextId(), profileNode));

                BuildBodyChildren(world, worldChildren);
                destination.Add(new TreeViewItemData<Node>(NextId(), worldNode, worldChildren));
            }
        }

        private void BuildBodyChildren(PhysicsWorld world, List<TreeViewItemData<Node>> destination)
        {
            if (!world.isValid)
                return;

            using var bodies = world.GetBodies(Allocator.Temp);

            for (int i = 0; i < bodies.Length; ++i)
            {
                var body = bodies[i];
                if (!body.isValid)
                    continue;

                var bodyNode = new Node
                {
                    kind = NodeKind.Body,
                    label = $"Body #{ExtractHandleIndex(body.ToString())}" + OwnerNameSuffix(body.GetOwner()),
                    world = world,
                    body = body,
                };

                var bodyChildren = new List<TreeViewItemData<Node>>();

                var transformObject = body.transformObject;
                if (transformObject != null)
                {
                    var transformNode = new Node
                    {
                        kind = NodeKind.TransformInfo,
                        label = $"Transform: \"{transformObject.gameObject.name}\"",
                        world = world,
                        body = body,
                        transformObject = transformObject,
                    };
                    bodyChildren.Add(new TreeViewItemData<Node>(NextId(), transformNode));
                }

                BuildShapesChild(world, body, bodyChildren);
                BuildJointsChild(world, body, bodyChildren);

                destination.Add(new TreeViewItemData<Node>(NextId(), bodyNode, bodyChildren));
            }
        }

        private void BuildShapesChild(PhysicsWorld world, PhysicsBody body, List<TreeViewItemData<Node>> destination)
        {
            using var shapes = body.GetShapes(Allocator.Temp);
            for (int i = 0; i < shapes.Length; ++i)
            {
                var shape = shapes[i];
                if (!shape.isValid)
                    continue;

                var shapeNode = new Node
                {
                    kind = NodeKind.Shape,
                    label = $"PhysicsShape #{ExtractHandleIndex(shape.ToString())} ({shape.shapeType})" + OwnerNameSuffix(shape.GetOwner()),
                    world = world,
                    body = body,
                    shape = shape,
                };

                var geometryNode = new Node
                {
                    kind = NodeKind.GeometryInfo,
                    label = $"{shape.shapeType}Geometry",
                    world = world,
                    body = body,
                    shape = shape,
                };

                var shapeChildren = new List<TreeViewItemData<Node>>
                {
                    new TreeViewItemData<Node>(NextId(), geometryNode),
                };

                // Contacts container — always present under every shape.
                var contactsListNode = new Node
                {
                    kind = NodeKind.ShapeContacts,
                    label = "Contacts",
                    world = world,
                    body = body,
                    shape = shape,
                };
                var contactsListChildren = new List<TreeViewItemData<Node>>();
                BuildContactEntryChildren(shape, contactsListChildren);
                shapeChildren.Add(new TreeViewItemData<Node>(NextId(), contactsListNode, contactsListChildren));

                // Triggers container — only when this shape is configured as a trigger.
                if (shape.isTrigger)
                {
                    var triggersListNode = new Node
                    {
                        kind = NodeKind.ShapeTriggers,
                        label = "Triggers",
                        world = world,
                        body = body,
                        shape = shape,
                    };
                    var triggersListChildren = new List<TreeViewItemData<Node>>();
                    BuildTriggerVisitorChildren(shape, triggersListChildren);
                    shapeChildren.Add(new TreeViewItemData<Node>(NextId(), triggersListNode, triggersListChildren));
                }

                destination.Add(new TreeViewItemData<Node>(NextId(), shapeNode, shapeChildren));
            }
        }

        // Snapshots the shape's current participating contacts via PhysicsShape.GetContacts.
        private void BuildContactEntryChildren(PhysicsShape owning, List<TreeViewItemData<Node>> destination)
        {
            if (!owning.isValid) return;

            using var contacts = owning.GetContacts(Allocator.Temp);
            for (int i = 0; i < contacts.Length; ++i)
            {
                var c = contacts[i];
                // Determine which side of the contact is the "other" (non-owning) shape. Both
                // shapes here come from the same internal source path so default struct equality
                // is reliable in this direction.
                var other = EqualityComparer<PhysicsShape>.Default.Equals(c.shapeA, owning) ? c.shapeB : c.shapeA;

                var entry = new Node
                {
                    kind = NodeKind.ShapeContactEntry,
                    label = $"Contact #{ExtractHandleIndex(c.contactId.ToString())} ↔ shape #{ExtractHandleIndex(other.ToString())}",
                    world = owning.world,
                    shape = owning,
                    secondShape = other,
                    contactId = c.contactId,
                };
                destination.Add(new TreeViewItemData<Node>(NextId(), entry));
            }
        }

        // Snapshots the trigger shape's current visitors via PhysicsShape.GetTriggerVisitors.
        // Only meaningful when owning.isTrigger == true; the API returns an empty array otherwise.
        private void BuildTriggerVisitorChildren(PhysicsShape owning, List<TreeViewItemData<Node>> destination)
        {
            if (!owning.isValid) return;

            using var visitors = owning.GetTriggerVisitors(Allocator.Temp);
            for (int i = 0; i < visitors.Length; ++i)
            {
                var visitor = visitors[i];
                var entry = new Node
                {
                    kind = NodeKind.ShapeTriggerVisitor,
                    label = $"Visitor: PhysicsShape #{ExtractHandleIndex(visitor.ToString())}",
                    world = owning.world,
                    shape = owning,
                    secondShape = visitor,
                };
                destination.Add(new TreeViewItemData<Node>(NextId(), entry));
            }
        }

        private void BuildJointsChild(PhysicsWorld world, PhysicsBody body, List<TreeViewItemData<Node>> destination)
        {
            using var joints = body.GetJoints(Allocator.Temp);
            for (int i = 0; i < joints.Length; ++i)
            {
                var joint = joints[i];
                if (!joint.isValid)
                    continue;

                var other = AreEqual(joint.bodyA, body) ? joint.bodyB : joint.bodyA;
                string otherLabel;
                if (!other.isValid)
                {
                    otherLabel = "Body (invalid)";
                }
                else
                {
                    var otherIndex = ExtractHandleIndex(other.ToString());
                    var sameWorld = EqualityComparer<PhysicsWorld>.Default.Equals(other.world, world);
                    otherLabel = sameWorld
                        ? $"Body #{otherIndex}"
                        : $"Body #{otherIndex} (other world)";
                }

                var jointNode = new Node
                {
                    kind = NodeKind.Joint,
                    label = $"{joint.jointType} #{ExtractHandleIndex(joint.ToString())} → {otherLabel}" + OwnerNameSuffix(joint.GetOwner()),
                    world = world,
                    body = body,
                    joint = joint,
                };
                destination.Add(new TreeViewItemData<Node>(NextId(), jointNode));
            }
        }

        private static bool AreEqual(PhysicsBody a, PhysicsBody b)
            => EqualityComparer<PhysicsBody>.Default.Equals(a, b);

        private void OnTreeSelectionChanged(IEnumerable<object> _)
        {
            UpdateSelectionDependentUI();

            // Selection-driven re-snapshot of per-shape Contacts/Triggers child lists. We do this
            // on the selection event — NOT inside the Display* methods — so the periodic AutoRead /
            // AutoRefresh tick re-renders don't tear down and rebuild the tree subtree (which
            // assigns new ids to child rows and can lose in-flight clicks on jump buttons).
            PerformSelectionDrivenRebuilds();

            // Push the new selection onto the navigation history (unless the change is being made
            // by Back/Forward themselves — the m_NavigatingHistory flag suppresses self-pushing).
            if (!m_NavigatingHistory)
                PushHistory(CurrentSelectedNode());
            UpdateHistoryButtons();

            DisplayInspectorForCurrentSelection();

            // Auto-sync: tree → GameObject. Skip if the change is the echo of our own
            // GameObject → tree sync (m_LastSyncedGO already matches).
            if (m_AutoSelectToggle == null || !m_AutoSelectToggle.value) return;
            var go = TryResolveOwnerGameObject(CurrentSelectedNode());
            if (go == null) return;
            if (ReferenceEquals(go, m_LastSyncedGO)) return;
            m_LastSyncedGO = go;
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }

        private void OnTreeItemsChosen(IEnumerable<object> _)
        {
            // itemsChosen fires on double-click (or Enter). For a TransformInfo node, jump straight
            // to the GameObject in the Hierarchy; for everything else, toggle expansion.
            foreach (var id in m_TreeView.selectedIds)
            {
                var node = m_TreeView.GetItemDataForId<Node>(id);
                if (node != null && node.kind == NodeKind.TransformInfo && node.transformObject != null)
                {
                    Selection.activeGameObject = node.transformObject.gameObject;
                    EditorGUIUtility.PingObject(node.transformObject.gameObject);
                    continue;
                }
                if (m_TreeView.IsExpanded(id))
                    m_TreeView.CollapseItem(id);
                else
                    m_TreeView.ExpandItem(id);
            }
        }

        private Node CurrentSelectedNode()
        {
            var selectedIds = m_TreeView?.selectedIds;
            if (selectedIds == null)
                return null;
            foreach (var id in selectedIds)
                return m_TreeView.GetItemDataForId<Node>(id);
            return null;
        }

        private void UpdateSelectionDependentUI()
        {
            // When Auto Select is on, the tree → GameObject sync runs on every selection change,
            // so the manual button is redundant — disable it for clarity.
            var autoSelectOn = m_AutoSelectToggle != null && m_AutoSelectToggle.value;
            m_SelectGameObjectButton.SetEnabled(
                !autoSelectOn && TryResolveOwnerGameObject(CurrentSelectedNode()) != null);
        }

        // Resolves the GameObject the "Select GameObject" button should select for the given node,
        // or null if the button should be disabled. Owner takes precedence; for Body the
        // transformObject is a fallback when no Component-typed owner exists.
        private static GameObject TryResolveOwnerGameObject(Node node)
        {
            if (node == null) return null;

            Object owner = null;
            switch (node.kind)
            {
                case NodeKind.World: if (node.world.isValid) owner = node.world.GetOwner(); break;
                case NodeKind.Body:  if (node.body.isValid)  owner = node.body.GetOwner();  break;
                case NodeKind.Shape: if (node.shape.isValid) owner = node.shape.GetOwner(); break;
                case NodeKind.Joint: if (node.joint.isValid) owner = node.joint.GetOwner(); break;
                default: return null;
            }

            var ownerGO = OwnerAsGameObject(owner);
            if (ownerGO != null) return ownerGO;

            // Body-only fallback: no Component/GameObject owner, but transformObject is assigned.
            if (node.kind == NodeKind.Body && node.body.isValid && node.body.transformObject != null)
                return node.body.transformObject.gameObject;

            return null;
        }

        private static GameObject OwnerAsGameObject(Object owner)
        {
            if (owner is GameObject go) return go;
            if (owner is Component c) return c.gameObject;
            return null;
        }

        // Suffix used in tree-node labels when a handle's owner resolves to a GameObject or
        // Component, mirroring how the Transform node renders its target name (e.g. ': "Foo"').
        private static string OwnerNameSuffix(Object owner)
        {
            var go = OwnerAsGameObject(owner);
            return go != null ? $" : \"{go.name}\"" : string.Empty;
        }

        // Extracts the first contiguous run of digits from `handleString` (typically a handle's
        // ToString() output). Used as a stable per-handle index for tree-node labels until the
        // PhysicsCore2D API exposes a direct accessor.
        private static string ExtractHandleIndex(string handleString)
        {
            if (string.IsNullOrEmpty(handleString)) return "?";
            int i = 0;
            while (i < handleString.Length && !char.IsDigit(handleString[i])) i++;
            if (i >= handleString.Length) return "?";
            int start = i;
            while (i < handleString.Length && char.IsDigit(handleString[i])) i++;
            return handleString.Substring(start, i - start);
        }

        private void DisplayInspectorForCurrentSelection()
        {
            m_Inspector.Clear();

            var node = CurrentSelectedNode();
            if (node == null)
            {
                m_Inspector.Add(new Label("No selection."));
                return;
            }

            if (node.deleted)
            {
                m_Inspector.Add(new HelpBox(
                    "This object is no longer valid. Click Refresh to remove it from the tree.",
                    HelpBoxMessageType.Warning));
                return;
            }

            switch (node.kind)
            {
                case NodeKind.Root:
                    DisplayRoot();
                    break;
                case NodeKind.World:
                    DisplayWorld(node);
                    break;
                case NodeKind.Body:
                    DisplayBody(node);
                    break;
                case NodeKind.TransformInfo:
                    DisplayTransformInfo(node);
                    break;
                case NodeKind.Shape:
                    DisplayShape(node);
                    break;
                case NodeKind.GeometryInfo:
                    DisplayShapeGeometry(node);
                    break;
                case NodeKind.Joint:
                    DisplayJoint(node);
                    break;
                case NodeKind.WorldCounters:
                    DisplayWorldCounters(node);
                    break;
                case NodeKind.WorldProfile:
                    DisplayWorldProfile(node);
                    break;
                case NodeKind.GlobalCounters:
                    DisplayGlobalCounters(node);
                    break;
                case NodeKind.GlobalProfile:
                    DisplayGlobalProfile(node);
                    break;
                case NodeKind.ShapeContacts:
                    DisplayShapeContacts(node);
                    break;
                case NodeKind.ShapeTriggers:
                    DisplayShapeTriggers(node);
                    break;
                case NodeKind.ShapeContactEntry:
                    DisplayShapeContactEntry(node);
                    break;
                case NodeKind.ShapeTriggerVisitor:
                    DisplayShapeTriggerVisitor(node);
                    break;
            }
        }

        private void AddHeader(string text)
        {
            var label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 4f;
            m_Inspector.Add(label);
        }

        private void AddInfo(string label, string value)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 2f } };
            var lbl = new Label(label)
            {
                style = { width = 200f, marginRight = 8f, whiteSpace = WhiteSpace.Normal },
            };
            var val = new Label(value) { style = { flexGrow = 1f, whiteSpace = WhiteSpace.Normal } };
            row.Add(lbl);
            row.Add(val);
            m_Inspector.Add(row);
        }

        // Runs `body` with m_Inspector temporarily redirected into an indented sub-container so
        // any AddInfo/AddColumnHeaders calls inside land 12px from the left edge — matches the
        // indent that BindHolder applies to its PropertyField container. Returns the container so
        // callers can apply post-build effects (e.g. zebra striping).
        private VisualElement WithIndent(System.Action body)
        {
            var saved = m_Inspector;
            var indented = new VisualElement { style = { paddingLeft = 12f } };
            saved.Add(indented);
            m_Inspector = indented;
            try { body(); }
            finally { m_Inspector = saved; }
            return indented;
        }

        // Applies a faint alternate-row background to every other direct child of `container`,
        // skipping the first `skipFirstN` rows (e.g. the bold column header row). The colour is
        // a low-alpha white so it brightens dark themes slightly without harming the light theme.
        private static void ApplyZebraStripes(VisualElement container, int skipFirstN = 0)
        {
            int dataIndex = 0;
            int childIndex = 0;
            foreach (var child in container.Children())
            {
                if (childIndex++ < skipFirstN) continue;
                if ((dataIndex++ & 1) == 0)
                    child.style.backgroundColor = new Color(1f, 1f, 1f, 0.04f);
            }
        }

        // Bold column headers that line up with AddInfo's two-column layout.
        private void AddColumnHeaders(string nameHeader, string valueHeader)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 4f } };
            var lbl = new Label(nameHeader)
            {
                style =
                {
                    width = 200f,
                    marginRight = 8f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                },
            };
            var val = new Label(valueHeader)
            {
                style =
                {
                    flexGrow = 1f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                },
            };
            row.Add(lbl);
            row.Add(val);
            m_Inspector.Add(row);
        }

        private void DisplayRoot()
        {
            AddHeader("Physics Worlds (summary)");
            using var worlds = PhysicsWorld.GetWorlds(Allocator.Temp);

            int totalBodies = 0, totalShapes = 0, totalJoints = 0, totalAwake = 0;
            for (int i = 0; i < worlds.Length; ++i)
            {
                var w = worlds[i];
                if (!w.isValid) continue;
                var counters = w.counters;
                totalBodies += counters.bodyCount;
                totalShapes += counters.shapeCount;
                totalJoints += counters.jointCount;
                totalAwake += w.awakeBodyCount;
            }

            var container = WithIndent(() =>
            {
                AddInfo("World count", worlds.Length.ToString());
                AddInfo("Total bodies", totalBodies.ToString());
                AddInfo("Total shapes", totalShapes.ToString());
                AddInfo("Total joints", totalJoints.ToString());
                AddInfo("Awake bodies", totalAwake.ToString());
            });
            ApplyZebraStripes(container);
        }

        // Renders a Read/Write button region, then a "<Type>Definition" header, then performs the
        // initial read and binds the holder. Used by World/Body/Shape definition inspectors and
        // Shape geometry. Pass writeAction = null when the source is API read-only (e.g.
        // ChainSegmentGeometry). `propertyPath` is the field name on the shared holder to bind.
        private void DisplayDefinition(
            Node node,
            ScriptableObject holder,
            string headerText,
            System.Func<bool> isValid,
            System.Action readAction,
            System.Action writeAction,
            string propertyPath)
        {
            AddButtonRegion(buttonRow =>
            {
                buttonRow.Add(new Button(() => OnReadClicked(node, isValid, readAction)) { text = "Read" });
                if (writeAction != null)
                    buttonRow.Add(new Button(() => OnWriteClicked(node, isValid, writeAction)) { text = "Write" });
            });

            AddHeader(headerText);

            readAction();
            BindHolder(holder, propertyPath);
        }

        // A horizontal button row with a subtle bottom border, sitting above the section title.
        private void AddButtonRegion(System.Action<VisualElement> populate)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    paddingBottom = 4f,
                    marginBottom = 6f,
                    borderBottomWidth = 1f,
                    borderBottomColor = new Color(0f, 0f, 0f, 0.3f),
                },
            };
            populate(row);
            m_Inspector.Add(row);
        }

        private void OnReadClicked(Node node, System.Func<bool> isValid, System.Action readAction)
        {
            if (!isValid()) { MarkNodeDeleted(node); return; }
            readAction();
            DisplayInspectorForCurrentSelection();
        }

        private void OnWriteClicked(Node node, System.Func<bool> isValid, System.Action writeAction)
        {
            if (!isValid()) { MarkNodeDeleted(node); return; }
            writeAction();
        }

        private void MarkNodeDeleted(Node node)
        {
            if (node == null || node.deleted) return;
            node.deleted = true;

            // RefreshItem takes an index and we only have the node reference; rebuild visible rows
            // to repaint the affected one.
            m_TreeView?.Rebuild();
            DisplayInspectorForCurrentSelection();
        }

        private void DisplayWorld(Node node)
        {
            if (!node.world.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }
            DisplayDefinition(node, m_Holder, "PhysicsWorldDefinition",
                () => node.world.isValid,
                () => m_Holder.world = node.world.definition,
                () => node.world.definition = m_Holder.world,
                "world");
        }

        private void DisplayWorldCounters(Node node)
        {
            if (!node.world.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }
            RenderCountersPanel("WorldCounters",
                isValid: () => node.world.isValid,
                onInvalid: () => MarkNodeDeleted(node),
                getCounters: () => node.world.counters,
                getAwakeBodyCount: () => node.world.awakeBodyCount);
        }

        private void DisplayWorldProfile(Node node)
        {
            if (!node.world.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }
            RenderProfilePanel("WorldProfile",
                isValid: () => node.world.isValid,
                onInvalid: () => MarkNodeDeleted(node),
                getProfile: () => node.world.profile);
        }

        private void DisplayGlobalCounters(Node node)
        {
            // Globals are static aggregates — always valid as long as the physics module exists.
            RenderCountersPanel("GlobalCounters (all worlds)",
                isValid: () => true,
                onInvalid: null,
                getCounters: () => PhysicsWorld.globalCounters,
                getAwakeBodyCount: null);   // No PhysicsWorld.globalAwakeBodyCount — skip the row.
        }

        private void DisplayGlobalProfile(Node node)
        {
            RenderProfilePanel("GlobalProfile (all worlds)",
                isValid: () => true,
                onInvalid: null,
                getProfile: () => PhysicsWorld.globalProfile);
        }

        // Shared layout for any "counters" panel: Read button region, header, indented data table.
        private void RenderCountersPanel(
            string headerText,
            System.Func<bool> isValid,
            System.Action onInvalid,
            System.Func<PhysicsWorld.WorldCounters> getCounters,
            System.Func<int> getAwakeBodyCount)
        {
            AddButtonRegion(buttonRow =>
            {
                buttonRow.Add(new Button(() =>
                {
                    if (!isValid()) { onInvalid?.Invoke(); return; }
                    DisplayInspectorForCurrentSelection();
                }) { text = "Read" });
            });

            AddHeader(headerText);

            var container = WithIndent(() =>
            {
                AddColumnHeaders("Name", "Count");
                var c = getCounters();
                AddInfo("Body count", c.bodyCount.ToString());
                if (getAwakeBodyCount != null)
                    AddInfo("Awake body count", getAwakeBodyCount().ToString());
                AddInfo("Shape count", c.shapeCount.ToString());
                AddInfo("Joint count", c.jointCount.ToString());
                AddInfo("Contact count", c.contactCount.ToString());
                AddInfo("Island count", c.islandCount.ToString());
                AddInfo("Task count", c.taskCount.ToString());
                AddInfo("Broadphase height", c.broadphaseHeight.ToString());
                AddInfo("Static broadphase height", c.staticBroadphaseHeight.ToString());
                AddInfo("Memory used (bytes)", c.memoryUsed.ToString());
                AddInfo("Stack used (bytes)", c.stackUsed.ToString());
            });
            ApplyZebraStripes(container, skipFirstN: 1);   // skip the column header row
        }

        // Shared layout for any "profile" panel: Read button region, header, indented data table.
        private void RenderProfilePanel(
            string headerText,
            System.Func<bool> isValid,
            System.Action onInvalid,
            System.Func<PhysicsWorld.WorldProfile> getProfile)
        {
            AddButtonRegion(buttonRow =>
            {
                buttonRow.Add(new Button(() =>
                {
                    if (!isValid()) { onInvalid?.Invoke(); return; }
                    DisplayInspectorForCurrentSelection();
                }) { text = "Read" });
            });

            AddHeader(headerText);

            var container = WithIndent(() =>
            {
                AddColumnHeaders("Name", "Time (seconds)");
                var p = getProfile();
                AddTime("Simulation step",       p.simulationStep);
                AddTime("Prepare stages",        p.prepareStages);
                AddTime("Body transforms",       p.bodyTransforms);
                AddTime("Broadphase updates",    p.broadphaseUpdates);
                AddTime("Contact pairs",         p.contactPairs);
                AddTime("Contact updates",       p.contactUpdates);
                AddTime("Update triggers",       p.updateTriggers);
                AddTime("Fast triggers",         p.fastTriggers);
                AddTime("Hit events",            p.hitEvents);
                AddTime("Joint events",          p.jointEvents);
                AddTime("Prepare constraints",   p.prepareConstraints);
                AddTime("Warm starting",         p.warmStarting);
                AddTime("Solving",               p.solving);
                AddTime("Solve constraints",     p.solveConstraints);
                AddTime("Solve impulses",        p.solveImpulses);
                AddTime("Relax impulses",        p.relaxImpulses);
                AddTime("Apply bounciness",      p.applyBounciness);
                AddTime("Store impulses",        p.storeImpulses);
                AddTime("Integrate velocities",  p.integrateVelocities);
                AddTime("Integrate transforms",  p.integrateTransforms);
                AddTime("Solve continuous",      p.solveContinuous);
                AddTime("Split islands",         p.splitIslands);
                AddTime("Sleep islands",         p.sleepIslands);
                AddTime("Write transforms",      p.writeTransforms);
            });
            ApplyZebraStripes(container, skipFirstN: 1);   // skip the column header row
        }

        // Format a per-stage timing as seconds with microsecond precision (unit shown by header).
        private void AddTime(string label, float seconds) => AddInfo(label, seconds.ToString("F6"));

        // ---- Per-shape Contacts / Triggers panels ----

        private void DisplayShapeContacts(Node node)
        {
            if (!node.shape.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }

            // Note: re-snapshot of the contact entries happens on the selection event (see
            // OnTreeSelectionChanged → PerformSelectionDrivenRebuilds), not here.

            AddButtonRegion(buttonRow =>
            {
                buttonRow.Add(new Button(() =>
                {
                    if (!node.shape.isValid) { MarkNodeDeleted(node); return; }
                    RebuildContactEntries(node);
                }) { text = "Refresh" });
            });

            AddHeader("Contacts");

            int count = GetChildCountForNode(node);
            var container = WithIndent(() =>
            {
                AddColumnHeaders("Name", "Count");
                AddInfo("Total", count.ToString());
            });
            ApplyZebraStripes(container, skipFirstN: 1);
        }

        private void DisplayShapeTriggers(Node node)
        {
            if (!node.shape.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }

            // See note in DisplayShapeContacts — re-snapshot is selection-driven.

            AddButtonRegion(buttonRow =>
            {
                buttonRow.Add(new Button(() =>
                {
                    if (!node.shape.isValid) { MarkNodeDeleted(node); return; }
                    RebuildTriggerVisitors(node);
                }) { text = "Refresh" });
            });

            AddHeader("Triggers");

            int count = GetChildCountForNode(node);
            var container = WithIndent(() =>
            {
                AddColumnHeaders("Name", "Count");
                AddInfo("Visitors", count.ToString());
            });
            ApplyZebraStripes(container, skipFirstN: 1);
        }

        private void DisplayShapeContactEntry(Node node)
        {
            if (!node.contactId.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }

            AddHeader("Contact Entry");

            // Just the other shape's jump button — the owning shape is named by the tree path.
            RenderShapeJumpButton(node.secondShape, node.world, "Other Shape");

            AddSpacer();

            var contact = node.contactId.contact;
            RenderManifoldPanel(contact.manifold);
        }

        private void DisplayShapeTriggerVisitor(Node node)
        {
            if (!node.secondShape.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }

            AddHeader("Trigger Visitor");

            RenderShapeJumpButton(node.secondShape, node.world, "Visitor");

            var visitor = node.secondShape;
            var bodyIdx = visitor.body.isValid
                ? ExtractHandleIndex(visitor.body.ToString())
                : "?";
            var container = WithIndent(() =>
            {
                AddColumnHeaders("Name", "Value");
                AddInfo("Shape index", ExtractHandleIndex(visitor.ToString()));
                AddInfo("Shape type", visitor.shapeType.ToString());
                AddInfo("Body index", bodyIdx);
            });
            ApplyZebraStripes(container, skipFirstN: 1);
        }

        // Renders a single "<roleLabel>: <shape-label>" jump button styled to match the joint
        // jump button. Disabled when the shape is invalid OR no matching tree node exists.
        // Compares by ToString() within the owning world (struct equality is unreliable for
        // shapes obtained via the events / contact APIs).
        private void RenderShapeJumpButton(PhysicsShape shape, PhysicsWorld owningWorld, string roleLabel)
        {
            string buttonLabel;
            int? jumpTargetId = null;

            if (!shape.isValid)
            {
                buttonLabel = $"{roleLabel}: (Deleted)";
            }
            else
            {
                jumpTargetId = FindShapeNodeId(shape, owningWorld);
                var idx = ExtractHandleIndex(shape.ToString());
                buttonLabel = jumpTargetId.HasValue
                    ? $"{roleLabel}: PhysicsShape #{idx} ({shape.shapeType})"
                    : $"{roleLabel}: PhysicsShape #{idx} (not in tree)";
            }

            var jumpButton = new Button(() =>
            {
                if (jumpTargetId.HasValue)
                    SelectNodeById(jumpTargetId.Value);
            }) { text = buttonLabel };

            var iconTex = shape.isValid && shape.shapeType == PhysicsShape.ShapeType.ChainSegment
                ? m_IconChain
                : m_IconShape;
            if (iconTex != null)
            {
                jumpButton.iconImage = Background.FromTexture2D(iconTex);
                var iconImg = jumpButton.Q<Image>(className: Button.imageUSSClassName);
                if (iconImg != null)
                {
                    iconImg.style.width = 14f;
                    iconImg.style.height = 14f;
                    iconImg.style.marginRight = 4f;
                }
            }
            jumpButton.SetEnabled(jumpTargetId.HasValue);
            jumpButton.style.marginBottom = 4f;
            jumpButton.style.height = 30f;
            m_Inspector.Add(jumpButton);
        }

        private void RenderManifoldPanel(PhysicsShape.ContactManifold manifold)
        {
            AddHeader("Manifold");

            var manifoldContainer = WithIndent(() =>
            {
                AddColumnHeaders("Name", "Value");
                AddInfo("Normal", manifold.normal.ToString("F4"));
                AddInfo("Point count", manifold.pointCount.ToString());
                AddInfo("Speculative point count", manifold.speculativePointCount.ToString());
                AddInfo("Rolling impulse", manifold.rollingImpulse.ToString("F4"));
            });
            ApplyZebraStripes(manifoldContainer, skipFirstN: 1);

            for (int i = 0; i < manifold.pointCount; ++i)
            {
                AddSpacer();
                AddHeader($"Point #{i}");

                var p = manifold[i];
                var pointContainer = WithIndent(() =>
                {
                    AddColumnHeaders("Name", "Value");
                    AddInfo("Point", p.point.ToString("F4"));
                    AddInfo("Anchor A", p.anchorA.ToString("F4"));
                    AddInfo("Anchor B", p.anchorB.ToString("F4"));
                    AddInfo("Separation", p.separation.ToString("F4"));
                    AddInfo("Normal impulse", p.normalImpulse.ToString("F4"));
                    AddInfo("Total normal impulse", p.totalNormalImpulse.ToString("F4"));
                    AddInfo("Normal velocity", p.normalVelocity.ToString("F4"));
                    AddInfo("Tangent impulse", p.tangentImpulse.ToString("F4"));
                    AddInfo("Persisted", p.persisted.ToString());
                    AddInfo("Speculative", p.speculative.ToString());
                    AddInfo("Id", p.id.ToString());
                });
                ApplyZebraStripes(pointContainer, skipFirstN: 1);
            }
        }

        private int? FindShapeNodeId(PhysicsShape shape, PhysicsWorld owningWorld)
        {
            if (!shape.isValid) return null;
            var shapeStr = shape.ToString();
            return FindNodeIdMatching(n =>
                n.kind == NodeKind.Shape
                && n.shape.isValid
                && EqualityComparer<PhysicsWorld>.Default.Equals(n.world, owningWorld)
                && n.shape.ToString() == shapeStr);
        }

        // ---- Subtree rebuild helpers (used by Refresh buttons + Shape-selection auto-refresh) ----

        private void RebuildContactEntries(Node listNode)
        {
            ReplaceChildren(listNode, newChildren =>
                BuildContactEntryChildren(listNode.shape, newChildren));
        }

        private void RebuildTriggerVisitors(Node listNode)
        {
            ReplaceChildren(listNode, newChildren =>
                BuildTriggerVisitorChildren(listNode.shape, newChildren));
        }

        // Selection-driven re-snapshot. Called from OnTreeSelectionChanged exactly once per
        // selection change. Must NOT be called from Display* methods — those are invoked on every
        // periodic re-render and rebuilding the tree on every tick destroys in-flight clicks.
        private void PerformSelectionDrivenRebuilds()
        {
            var node = CurrentSelectedNode();
            if (node == null) return;
            switch (node.kind)
            {
                case NodeKind.Shape:
                    if (node.shape.isValid) RebuildShapeChildLists(node);
                    break;
                case NodeKind.ShapeContacts:
                    if (node.shape.isValid) RebuildContactEntries(node);
                    break;
                case NodeKind.ShapeTriggers:
                    if (node.shape.isValid) RebuildTriggerVisitors(node);
                    break;
            }
        }

        // Walks the Shape node's children and rebuilds every ShapeContacts / ShapeTriggers
        // subtree it finds. Invoked via PerformSelectionDrivenRebuilds when a Shape is selected.
        private void RebuildShapeChildLists(Node shapeNode)
        {
            int shapeId = FindNodeIdForNode(shapeNode) ?? -1;
            if (shapeId < 0) return;
            ForEachChild(shapeId, child =>
            {
                var n = child.data;
                if (n == null) return;
                if (n.kind == NodeKind.ShapeContacts) RebuildContactEntries(n);
                else if (n.kind == NodeKind.ShapeTriggers) RebuildTriggerVisitors(n);
            });
        }

        private int GetChildCountForNode(Node node)
        {
            int parentId = FindNodeIdForNode(node) ?? -1;
            if (parentId < 0) return 0;
            int count = 0;
            ForEachChild(parentId, _ => count++);
            return count;
        }

        private int? FindNodeIdForNode(Node target)
        {
            if (target == null || m_TreeData == null) return null;
            foreach (var rootItem in m_TreeData)
            {
                var found = FindItemId(rootItem, item => ReferenceEquals(item.data, target));
                if (found.HasValue) return found;
            }
            return null;
        }

        private void ForEachChild(int parentId, System.Action<TreeViewItemData<Node>> action)
        {
            if (m_TreeData == null) return;
            foreach (var rootItem in m_TreeData)
            {
                if (TryForEachChild(rootItem, parentId, action)) return;
            }
        }

        private static bool TryForEachChild(TreeViewItemData<Node> item, int parentId, System.Action<TreeViewItemData<Node>> action)
        {
            if (item.id == parentId)
            {
                if (item.children != null)
                    foreach (var child in item.children)
                        action(child);
                return true;
            }
            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    if (TryForEachChild(child, parentId, action))
                        return true;
                }
            }
            return false;
        }

        // Replaces the children of `parent` in m_TreeData with a freshly-built list. Preserves
        // expansion of all ancestors and best-effort restores selection by handle equality.
        // TreeViewItemData<T> is immutable (children list is set at construction), so the chain
        // from each tree root down to the parent must be rebuilt.
        //
        // We temporarily detach the selectionChanged handler around SetRootItems/Rebuild because
        // those calls fire selectionChanged synchronously even when the selected id is unchanged
        // — and the handler routes back through DisplayShape(Contacts/Triggers), which would
        // re-enter ReplaceChildren and end up double-rendering the right panel.
        private void ReplaceChildren(Node parent, System.Action<List<TreeViewItemData<Node>>> populate)
        {
            if (m_TreeData == null) return;

            int parentId = FindNodeIdForNode(parent) ?? -1;
            if (parentId < 0) return;

            CapturePendingSelection();
            // m_TreeView.SetRootItems + Rebuild() wipes the entire expansion state.
            // Capture every expanded id before the swap so we can restore them after.
            var expandedIds = new List<int>();
            foreach (var rootItem in m_TreeData)
                CollectExpandedIds(rootItem, expandedIds);

            var newChildren = new List<TreeViewItemData<Node>>();
            populate(newChildren);

            for (int rootIdx = 0; rootIdx < m_TreeData.Count; ++rootIdx)
            {
                if (TryReplaceChildren(m_TreeData[rootIdx], parentId, newChildren, out var replacedRoot))
                {
                    m_TreeData[rootIdx] = replacedRoot;
                    break;
                }
            }

            m_TreeView.selectionChanged -= OnTreeSelectionChanged;
            try
            {
                m_TreeView.SetRootItems(m_TreeData);
                m_TreeView.Rebuild();

                // Restore expansion. Ids are stable across our rebuild (TryReplaceChildren
                // preserves item ids on every ancestor along the path), so re-expanding by id
                // works for ancestors and siblings of the swapped subtree.
                foreach (var id in expandedIds)
                    m_TreeView.ExpandItem(id);

                TryRestoreSelection();
            }
            finally
            {
                m_TreeView.selectionChanged += OnTreeSelectionChanged;
            }
        }

        private void CollectExpandedIds(TreeViewItemData<Node> item, List<int> ids)
        {
            if (m_TreeView.IsExpanded(item.id))
                ids.Add(item.id);
            if (item.children != null)
                foreach (var child in item.children)
                    CollectExpandedIds(child, ids);
        }

        private static bool TryReplaceChildren(
            TreeViewItemData<Node> item,
            int parentId,
            List<TreeViewItemData<Node>> newChildren,
            out TreeViewItemData<Node> replaced)
        {
            if (item.id == parentId)
            {
                replaced = new TreeViewItemData<Node>(item.id, item.data, newChildren);
                return true;
            }
            if (item.children != null)
            {
                List<TreeViewItemData<Node>> rebuilt = null;
                bool found = false;
                int i = 0;
                foreach (var child in item.children)
                {
                    if (TryReplaceChildren(child, parentId, newChildren, out var rebuiltChild))
                    {
                        if (rebuilt == null)
                        {
                            rebuilt = new List<TreeViewItemData<Node>>();
                            int j = 0;
                            foreach (var c in item.children)
                            {
                                if (j == i) break;
                                rebuilt.Add(c);
                                ++j;
                            }
                        }
                        rebuilt.Add(rebuiltChild);
                        found = true;
                    }
                    else if (rebuilt != null)
                    {
                        rebuilt.Add(child);
                    }
                    ++i;
                }
                if (found)
                {
                    replaced = new TreeViewItemData<Node>(item.id, item.data, rebuilt);
                    return true;
                }
            }
            replaced = item;
            return false;
        }

        private void DisplayBody(Node node)
        {
            if (!node.body.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }
            DisplayDefinition(node, m_Holder, "PhysicsBodyDefinition",
                () => node.body.isValid,
                () => m_Holder.body = node.body.definition,
                () => node.body.definition = m_Holder.body,
                "body");
        }

        private void DisplayTransformInfo(Node node)
        {
            AddHeader("Transform");
            var t = node.transformObject;
            if (t == null)
            {
                m_Inspector.Add(new HelpBox("Transform is no longer assigned.", HelpBoxMessageType.Warning));
                return;
            }
            var go = t.gameObject;

            var container = WithIndent(() =>
            {
                AddInfo("GameObject", go.name);
                AddInfo("Entity ID", go.GetEntityId().ToString());
                AddInfo("Scene", go.scene.IsValid() ? go.scene.name : "<no scene>");
                AddInfo("Hierarchy path", GetHierarchyPath(t));
            });
            ApplyZebraStripes(container);
        }

        private static string GetHierarchyPath(Transform t)
        {
            var stack = new Stack<string>();
            for (var cur = t; cur != null; cur = cur.parent)
                stack.Push(cur.name);
            return string.Join("/", stack);
        }

        private void DisplayShape(Node node)
        {
            if (!node.shape.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }

            // Note: per-shape Contacts/Triggers child lists are re-snapshotted on the SELECTION
            // event (see OnTreeSelectionChanged → PerformSelectionDrivenRebuilds), not here. This
            // method runs on every periodic AutoRead/AutoRefresh tick too, and rebuilding the
            // tree subtree on each tick would destroy clicks on jump buttons in flight.

            DisplayDefinition(node, m_Holder, "PhysicsShapeDefinition",
                () => node.shape.isValid,
                () => m_Holder.shape = node.shape.definition,
                () => node.shape.definition = m_Holder.shape,
                "shape");
        }

        private void DisplayShapeGeometry(Node node)
        {
            if (!node.shape.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }

            switch (node.shape.shapeType)
            {
                case PhysicsShape.ShapeType.Circle:
                    DisplayDefinition(node, m_Holder, "CircleGeometry",
                        () => node.shape.isValid,
                        () => m_Holder.circle = node.shape.circleGeometry,
                        () => node.shape.circleGeometry = m_Holder.circle,
                        "circle");
                    break;
                case PhysicsShape.ShapeType.Capsule:
                    DisplayDefinition(node, m_Holder, "CapsuleGeometry",
                        () => node.shape.isValid,
                        () => m_Holder.capsule = node.shape.capsuleGeometry,
                        () => node.shape.capsuleGeometry = m_Holder.capsule,
                        "capsule");
                    break;
                case PhysicsShape.ShapeType.Polygon:
                    DisplayDefinition(node, m_Holder, "PolygonGeometry",
                        () => node.shape.isValid,
                        () => m_Holder.polygon = node.shape.polygonGeometry,
                        () =>
                        {
                            // Validate() returns a copy with refreshed centroid/normals/isValid.
                            // If the polygon is degenerate (collinear, too few vertices, etc.) the
                            // returned copy comes back with isValid == false; bail with a modal and
                            // leave both the shape and the holder untouched.
                            var validated = m_Holder.polygon.Validate();
                            if (!validated.isValid)
                            {
                                EditorUtility.DisplayDialog(
                                    "Invalid PolygonGeometry",
                                    "The PolygonGeometry failed PolygonGeometry.Validate() and was not written to the shape. " +
                                    "Check that there are at least 3 vertices forming a non-degenerate convex hull.",
                                    "OK");
                                return;
                            }
                            node.shape.polygonGeometry = validated;
                            // Reflect the validated form back into the holder so the user sees the
                            // cleaned-up centroid/normals after the write.
                            m_Holder.polygon = validated;
                            DisplayInspectorForCurrentSelection();
                        },
                        "polygon");
                    break;
                case PhysicsShape.ShapeType.Segment:
                    DisplayDefinition(node, m_Holder, "SegmentGeometry",
                        () => node.shape.isValid,
                        () => m_Holder.segment = node.shape.segmentGeometry,
                        () => node.shape.segmentGeometry = m_Holder.segment,
                        "segment");
                    break;
                case PhysicsShape.ShapeType.ChainSegment:
                    // ChainSegmentGeometry is read-only on PhysicsShape (no setter); writeAction = null.
                    DisplayDefinition(node, m_Holder, "ChainSegmentGeometry (read-only)",
                        () => node.shape.isValid,
                        () => m_Holder.chainSegment = node.shape.chainSegmentGeometry,
                        null,
                        "chainSegment");
                    break;
                default:
                    m_Inspector.Add(new HelpBox($"Unknown shape type: {node.shape.shapeType}", HelpBoxMessageType.Warning));
                    break;
            }
        }

        private void DisplayJoint(Node node)
        {
            if (!node.joint.isValid)
            {
                MarkNodeDeleted(node);
                return;
            }

            // "Jump to" button for the connected (other) body. node.body is the body this joint is
            // parented under in the tree; the other side is whichever of bodyA/bodyB isn't that.
            // We resolve two ids:
            //   - otherBodyId — used only for the button label (e.g. "Jump to Body #4").
            //   - jumpTargetId — the actual joint node under the other body, so the same joint
            //     stays selected after the jump. Falls back to the body if for some reason the
            //     mirrored joint child isn't there.
            var other = AreEqual(node.joint.bodyA, node.body) ? node.joint.bodyB : node.joint.bodyA;
            int? otherBodyId = other.isValid
                ? FindNodeIdMatching(n => n.kind == NodeKind.Body && AreEqual(n.body, other))
                : null;
            int? jumpTargetId = null;
            if (otherBodyId.HasValue)
            {
                jumpTargetId = FindNodeIdMatching(n =>
                    n.kind == NodeKind.Joint
                    && AreEqual(n.body, other)
                    && EqualityComparer<PhysicsJoint>.Default.Equals(n.joint, node.joint))
                    ?? otherBodyId;
            }

            string buttonLabel;
            if (otherBodyId.HasValue)
            {
                var otherNode = m_TreeView.GetItemDataForId<Node>(otherBodyId.Value);
                buttonLabel = $"Jump to {otherNode?.label ?? "connected body"}";
            }
            else if (other.isValid)
            {
                buttonLabel = "Connected body (other world)";
            }
            else
            {
                buttonLabel = "Connected body (invalid)";
            }

            var jumpButton = new Button(() =>
            {
                if (jumpTargetId.HasValue)
                    SelectNodeById(jumpTargetId.Value);
            }) { text = buttonLabel };
            if (m_IconBody != null)
            {
                jumpButton.iconImage = Background.FromTexture2D(m_IconBody);
                // The inner image renders at the texture's native size by default; clamp it so it
                // doesn't dwarf the button text.
                var iconImg = jumpButton.Q<Image>(className: Button.imageUSSClassName);
                if (iconImg != null)
                {
                    iconImg.style.width = 14f;
                    iconImg.style.height = 14f;
                    iconImg.style.marginRight = 4f;
                }
            }
            jumpButton.SetEnabled(jumpTargetId.HasValue);
            jumpButton.style.marginBottom = 4f;
            // Default UIToolkit button height is ~20px; bump to 1.5x so the jump action stands out.
            jumpButton.style.height = 30f;
            m_Inspector.Add(jumpButton);

            AddInfo("Joint type", node.joint.jointType.ToString());
            AddInfo("Body A valid", node.joint.bodyA.isValid.ToString());
            AddInfo("Body B valid", node.joint.bodyB.isValid.ToString());
            AddInfo("Collide connected", node.joint.collideConnected.ToString());
            AddInfo("Force threshold", node.joint.forceThreshold.ToString("F3"));
            AddInfo("Torque threshold", node.joint.torqueThreshold.ToString("F3"));
            AddInfo("Current force", node.joint.currentConstraintForce.ToString("F3"));
            AddInfo("Current torque", node.joint.currentConstraintTorque.ToString("F3"));
            AddInfo("Linear sep error", node.joint.currentLinearSeparationError.ToString("F4"));
            AddInfo("Angular sep error", node.joint.currentAngularSeparationError.ToString("F4"));

            AddSpacer();

            // Per the documented API, only certain joint types have a downcast constructor from PhysicsJoint.
            // Distance and Ignore joints expose only the base properties above.
            switch (node.joint.jointType)
            {
                case PhysicsJoint.JointType.HingeJoint:
                    DisplayHingeExtras(new PhysicsHingeJoint(node.joint));
                    break;
                case PhysicsJoint.JointType.FixedJoint:
                    DisplayFixedExtras(new PhysicsFixedJoint(node.joint));
                    break;
                case PhysicsJoint.JointType.RelativeJoint:
                    DisplayRelativeExtras(new PhysicsRelativeJoint(node.joint));
                    break;
                case PhysicsJoint.JointType.SliderJoint:
                    DisplaySliderExtras(new PhysicsSliderJoint(node.joint));
                    break;
                case PhysicsJoint.JointType.WheelJoint:
                    DisplayWheelExtras(new PhysicsWheelJoint(node.joint));
                    break;
                case PhysicsJoint.JointType.DistanceJoint:
                case PhysicsJoint.JointType.IgnoreJoint:
                    m_Inspector.Add(new HelpBox(
                        $"{node.joint.jointType}: PhysicsCore2D does not currently expose a downcast constructor or .definition for this joint type. Only base properties shown above.",
                        HelpBoxMessageType.Info));
                    break;
            }
        }

        private void DisplayHingeExtras(PhysicsHingeJoint joint)
        {
            AddHeader("PhysicsHingeJoint");
            AddInfo("Angle", joint.angle.ToString("F3"));
            AddInfo("Motor enabled", joint.enableMotor.ToString());
            AddInfo("Motor speed", joint.motorSpeed.ToString("F3"));
            AddInfo("Max motor torque", joint.maxMotorTorque.ToString("F3"));
            AddInfo("Limit enabled", joint.enableLimit.ToString());
            AddInfo("Lower angle", joint.lowerAngleLimit.ToString("F3"));
            AddInfo("Upper angle", joint.upperAngleLimit.ToString("F3"));
            AddInfo("Spring enabled", joint.enableSpring.ToString());
            AddInfo("Spring frequency", joint.springFrequency.ToString("F3"));
            AddInfo("Spring damping", joint.springDamping.ToString("F3"));
            AddInfo("Spring target angle", joint.springTargetAngle.ToString("F3"));
        }

        private void DisplayFixedExtras(PhysicsFixedJoint joint)
        {
            AddHeader("PhysicsFixedJoint");
            AddInfo("Linear frequency", joint.linearFrequency.ToString("F3"));
            AddInfo("Linear damping", joint.linearDamping.ToString("F3"));
            AddInfo("Angular frequency", joint.angularFrequency.ToString("F3"));
            AddInfo("Angular damping", joint.angularDamping.ToString("F3"));
        }

        private void DisplayRelativeExtras(PhysicsRelativeJoint joint)
        {
            AddHeader("PhysicsRelativeJoint");
            AddInfo("Linear velocity", joint.linearVelocity.ToString("F3"));
            AddInfo("Angular velocity", joint.angularVelocity.ToString("F3"));
            AddInfo("Max force", joint.maxForce.ToString("F3"));
            AddInfo("Max torque", joint.maxTorque.ToString("F3"));
            AddInfo("Spring linear freq", joint.springLinearFrequency.ToString("F3"));
            AddInfo("Spring angular freq", joint.springAngularFrequency.ToString("F3"));
        }

        private void DisplaySliderExtras(PhysicsSliderJoint joint)
        {
            AddHeader("PhysicsSliderJoint");
            AddInfo("Translation", joint.currentTranslation.ToString("F3"));
            AddInfo("Speed", joint.currentSpeed.ToString("F3"));
            AddInfo("Motor enabled", joint.enableMotor.ToString());
            AddInfo("Motor speed", joint.motorSpeed.ToString("F3"));
            AddInfo("Max motor force", joint.maxMotorForce.ToString("F3"));
            AddInfo("Limit enabled", joint.enableLimit.ToString());
            AddInfo("Lower limit", joint.lowerTranslationLimit.ToString("F3"));
            AddInfo("Upper limit", joint.upperTranslationLimit.ToString("F3"));
            AddInfo("Spring enabled", joint.enableSpring.ToString());
            AddInfo("Spring frequency", joint.springFrequency.ToString("F3"));
        }

        private void DisplayWheelExtras(PhysicsWheelJoint joint)
        {
            AddHeader("PhysicsWheelJoint");
            AddInfo("Motor enabled", joint.enableMotor.ToString());
            AddInfo("Motor speed", joint.motorSpeed.ToString("F3"));
            AddInfo("Max motor torque", joint.maxMotorTorque.ToString("F3"));
            AddInfo("Limit enabled", joint.enableLimit.ToString());
            AddInfo("Lower limit", joint.lowerTranslationLimit.ToString("F3"));
            AddInfo("Upper limit", joint.upperTranslationLimit.ToString("F3"));
            AddInfo("Spring enabled", joint.enableSpring.ToString());
            AddInfo("Spring frequency", joint.springFrequency.ToString("F3"));
            AddInfo("Spring damping", joint.springDamping.ToString("F3"));
        }

        private void AddSpacer()
        {
            var spacer = new VisualElement();
            spacer.style.height = 6f;
            m_Inspector.Add(spacer);
        }

        private void BindHolder(ScriptableObject holder, string propertyPath)
        {
            // Guard against a holder whose underlying SO was destroyed (Unity's "fake null").
            // SerializedObject's ctor throws ArgumentException("Object at index 0 is null") on a
            // destroyed SO; bail with a HelpBox so the user can click Refresh to rebuild.
            if (holder == null)
            {
                m_Inspector.Add(new HelpBox(
                    "Inspector holder is no longer valid. Click Refresh to rebuild.",
                    HelpBoxMessageType.Warning));
                return;
            }

            var serializedObject = new SerializedObject(holder);
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                m_Inspector.Add(new HelpBox($"Property '{propertyPath}' not found on holder.", HelpBoxMessageType.Error));
                return;
            }

            // Render every child of the wrapping struct as its own row, so the user does not have to
            // expand the synthetic "value" foldout to see anything.
            var container = new VisualElement();
            // PropertyField labels only align with each other when the alignment context (an
            // InspectorElement) is in their ancestry. Tagging the container with that class opts
            // standalone PropertyFields into the same inspector label-width sync.
            container.AddToClassList(InspectorElement.ussClassName);
            // Foldout chevrons render to the left of their row; without this padding they get clipped
            // by the window edge.
            container.style.paddingLeft = 12f;

            var iterator = property.Copy();
            var end = property.GetEndProperty();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(iterator, end))
                        break;
                    container.Add(new PropertyField(iterator));
                } while (iterator.NextVisible(false));
            }

            m_Inspector.Add(container);
            container.Bind(serializedObject);
        }

        // ---- Buttons ----

        private void OnSelectGameObject()
        {
            var go = TryResolveOwnerGameObject(CurrentSelectedNode());
            if (go == null) return;
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }

        private void OnFindSelection()
        {
            var target = Selection.activeGameObject;
            if (target == null) return;

            if (TrySelectForEditorSelection())
                return;

            m_Inspector.Clear();
            m_Inspector.Add(new HelpBox(
                $"No PhysicsWorld/Body/Shape/Joint in any world has '{target.name}' as its owner (and no Body has it as a transformObject).",
                HelpBoxMessageType.Info));
        }

        // Scrolls the TreeView so the row for `id` is centered vertically in the viewport.
        // Done in a deferred callback so any expand-on-select layout has already settled.
        private void CenterItemInTree(int id)
        {
            var scrollView = m_TreeView.Q<ScrollView>();
            if (scrollView == null) return;

            m_TreeView.schedule.Execute(() =>
            {
                if (!TryGetVisibleIndex(id, out var index))
                    return;

                var rowHeight = m_TreeView.fixedItemHeight;
                var rowCenterY = (index + 0.5f) * rowHeight;
                var viewportHeight = scrollView.contentViewport.resolvedStyle.height;
                var totalContent = TotalVisibleItemCount() * rowHeight;
                var maxOffset = Mathf.Max(0f, totalContent - viewportHeight);
                var targetOffset = Mathf.Clamp(rowCenterY - viewportHeight * 0.5f, 0f, maxOffset);
                scrollView.scrollOffset = new Vector2(scrollView.scrollOffset.x, targetOffset);
            });
        }

        private bool TryGetVisibleIndex(int targetId, out int index)
        {
            int counter = 0;
            foreach (var rootItem in m_TreeData)
            {
                if (TryGetVisibleIndexHelper(rootItem, targetId, ref counter, out index))
                    return true;
            }
            index = -1;
            return false;
        }

        private bool TryGetVisibleIndexHelper(TreeViewItemData<Node> item, int targetId, ref int counter, out int index)
        {
            if (item.id == targetId) { index = counter; return true; }
            counter++;
            if (m_TreeView.IsExpanded(item.id) && item.children != null)
            {
                foreach (var child in item.children)
                {
                    if (TryGetVisibleIndexHelper(child, targetId, ref counter, out index))
                        return true;
                }
            }
            index = -1;
            return false;
        }

        private int TotalVisibleItemCount()
        {
            int count = 0;
            foreach (var rootItem in m_TreeData)
                CountVisibleHelper(rootItem, ref count);
            return count;
        }

        private void CountVisibleHelper(TreeViewItemData<Node> item, ref int count)
        {
            count++;
            if (m_TreeView.IsExpanded(item.id) && item.children != null)
            {
                foreach (var child in item.children)
                    CountVisibleHelper(child, ref count);
            }
        }

        // Expands ancestors and the target itself, selects it, and scrolls to centre. Works for any
        // node kind. Expanding a leaf is a no-op so this is safe to call for Joint nodes too.
        private void SelectNodeById(int id)
        {
            if (m_TreeData == null) return;
            foreach (var rootItem in m_TreeData)
            {
                var path = new List<int>();
                if (BuildPath(rootItem, id, path))
                {
                    for (int i = 0; i < path.Count - 1; ++i)
                        m_TreeView.ExpandItem(path[i]);
                    m_TreeView.ExpandItem(id);
                    m_TreeView.SetSelectionById(id);
                    CenterItemInTree(id);
                    return;
                }
            }
        }

        // ---- Navigation history (Back / Forward buttons) ----

        // Pushes the current selection onto the history. If we're at the end of the history,
        // appends; if we're mid-history (after some Back), truncates the forward portion first.
        // De-dups against the current entry so re-rendering the same selection doesn't bloat
        // history. Caps total entries at HistoryCapacity by trimming the oldest.
        private void PushHistory(Node node)
        {
            if (node == null) return;
            var entry = MakeHistoryEntry(node);

            // De-dup against the current entry.
            if (m_HistoryIndex >= 0 && m_HistoryIndex < m_History.Count
                && HistoryEntriesEqual(m_History[m_HistoryIndex], entry))
                return;

            // Truncate any forward history (we're branching from the current entry).
            if (m_HistoryIndex < m_History.Count - 1)
                m_History.RemoveRange(m_HistoryIndex + 1, m_History.Count - m_HistoryIndex - 1);

            m_History.Add(entry);
            m_HistoryIndex = m_History.Count - 1;

            // Trim the oldest entries if we're over capacity.
            int overflow = m_History.Count - HistoryCapacity;
            if (overflow > 0)
            {
                m_History.RemoveRange(0, overflow);
                m_HistoryIndex -= overflow;
            }
        }

        private static HistoryEntry MakeHistoryEntry(Node node)
        {
            return new HistoryEntry
            {
                kind = node.kind,
                world = node.world,
                body = node.body,
                shape = node.shape,
                joint = node.joint,
                contactId = node.contactId,
                secondShape = node.secondShape,
            };
        }

        private static bool HistoryEntriesEqual(HistoryEntry a, HistoryEntry b)
        {
            if (a.kind != b.kind) return false;
            // Use the same per-kind matching logic as MatchesPending — collapse to a string-form
            // identity check on the relevant handles. ToString is stable per logical handle.
            return EqualityComparer<PhysicsWorld>.Default.Equals(a.world, b.world)
                && EqualityComparer<PhysicsBody>.Default.Equals(a.body, b.body)
                && EqualityComparer<PhysicsShape>.Default.Equals(a.shape, b.shape)
                && EqualityComparer<PhysicsJoint>.Default.Equals(a.joint, b.joint)
                && EqualityComparer<PhysicsShape.ContactId>.Default.Equals(a.contactId, b.contactId)
                && EqualityComparer<PhysicsShape>.Default.Equals(a.secondShape, b.secondShape);
        }

        private void OnBackClicked()
        {
            if (m_HistoryIndex <= 0) return;
            // Walk backward past any entries that no longer resolve (e.g. selected handle was
            // destroyed). If nothing in the prior history is resolvable, leave the index alone.
            for (int i = m_HistoryIndex - 1; i >= 0; --i)
            {
                if (TryNavigateToHistoryIndex(i)) return;
            }
        }

        private void OnForwardClicked()
        {
            if (m_HistoryIndex >= m_History.Count - 1) return;
            for (int i = m_HistoryIndex + 1; i < m_History.Count; ++i)
            {
                if (TryNavigateToHistoryIndex(i)) return;
            }
        }

        private bool TryNavigateToHistoryIndex(int index)
        {
            if (index < 0 || index >= m_History.Count) return false;
            var entry = m_History[index];
            int? targetId = FindNodeIdMatching(n => HistoryEntryMatchesNode(entry, n));
            if (!targetId.HasValue) return false;

            m_NavigatingHistory = true;
            try
            {
                SelectNodeById(targetId.Value);
            }
            finally
            {
                m_NavigatingHistory = false;
            }
            // SelectNodeById fired selectionChanged → OnTreeSelectionChanged ran with
            // m_NavigatingHistory true (so it didn't push), but it also called
            // UpdateHistoryButtons after potentially clobbering m_HistoryIndex. Set the index
            // explicitly here so Back/Forward enabled state reflects the navigation.
            m_HistoryIndex = index;
            UpdateHistoryButtons();
            return true;
        }

        private static bool HistoryEntryMatchesNode(HistoryEntry entry, Node node)
        {
            if (node == null || node.kind != entry.kind) return false;
            switch (entry.kind)
            {
                case NodeKind.Root:
                case NodeKind.GlobalCounters:
                case NodeKind.GlobalProfile:
                    return true;
                case NodeKind.World:
                case NodeKind.WorldCounters:
                case NodeKind.WorldProfile:
                    return node.world.isValid && EqualityComparer<PhysicsWorld>.Default.Equals(node.world, entry.world);
                case NodeKind.Body:
                case NodeKind.TransformInfo:
                    return node.body.isValid && EqualityComparer<PhysicsBody>.Default.Equals(node.body, entry.body);
                case NodeKind.Shape:
                case NodeKind.GeometryInfo:
                case NodeKind.ShapeContacts:
                case NodeKind.ShapeTriggers:
                    return node.shape.isValid && EqualityComparer<PhysicsShape>.Default.Equals(node.shape, entry.shape);
                case NodeKind.ShapeContactEntry:
                    return node.contactId.isValid
                        && EqualityComparer<PhysicsShape.ContactId>.Default.Equals(node.contactId, entry.contactId);
                case NodeKind.ShapeTriggerVisitor:
                    return node.shape.isValid
                        && EqualityComparer<PhysicsShape>.Default.Equals(node.shape, entry.shape)
                        && node.secondShape.isValid
                        && entry.secondShape.isValid
                        && node.secondShape.ToString() == entry.secondShape.ToString();
                case NodeKind.Joint:
                    return node.joint.isValid && EqualityComparer<PhysicsJoint>.Default.Equals(node.joint, entry.joint);
                default:
                    return false;
            }
        }

        private void UpdateHistoryButtons()
        {
            if (m_BackButton != null)
                m_BackButton.SetEnabled(m_HistoryIndex > 0);
            if (m_ForwardButton != null)
                m_ForwardButton.SetEnabled(m_HistoryIndex >= 0 && m_HistoryIndex < m_History.Count - 1);
        }

        // ---- Selection persistence ----

        private void CapturePendingSelection()
        {
            var node = CurrentSelectedNode();
            m_HasPendingSelection = node != null;
            if (!m_HasPendingSelection)
                return;
            m_PendingKind = node.kind;
            m_PendingWorld = node.world;
            m_PendingBody = node.body;
            m_PendingShape = node.shape;
            m_PendingJoint = node.joint;
            m_PendingContactId = node.contactId;
            m_PendingSecondShape = node.secondShape;
        }

        private void TryRestoreSelection()
        {
            if (!m_HasPendingSelection || m_TreeData == null)
                return;
            m_HasPendingSelection = false;

            foreach (var rootItem in m_TreeData)
            {
                var found = FindItemId(rootItem, item => MatchesPending(item.data));
                if (found.HasValue)
                {
                    ExpandToItem(rootItem, found.Value);
                    m_TreeView.SetSelectionByIdWithoutNotify(new[] { found.Value });
                    return;
                }
            }
        }

        private bool MatchesPending(Node node)
        {
            if (node == null || node.kind != m_PendingKind)
                return false;

            switch (m_PendingKind)
            {
                case NodeKind.Root:
                case NodeKind.GlobalCounters:
                case NodeKind.GlobalProfile:
                    return true;
                case NodeKind.World:
                case NodeKind.WorldCounters:
                case NodeKind.WorldProfile:
                    return node.world.isValid && EqualityComparer<PhysicsWorld>.Default.Equals(node.world, m_PendingWorld);
                case NodeKind.Body:
                case NodeKind.TransformInfo:
                    return node.body.isValid && AreEqual(node.body, m_PendingBody);
                case NodeKind.Shape:
                case NodeKind.GeometryInfo:
                case NodeKind.ShapeContacts:
                case NodeKind.ShapeTriggers:
                    return node.shape.isValid && EqualityComparer<PhysicsShape>.Default.Equals(node.shape, m_PendingShape);
                case NodeKind.ShapeContactEntry:
                    return node.contactId.isValid
                        && EqualityComparer<PhysicsShape.ContactId>.Default.Equals(node.contactId, m_PendingContactId);
                case NodeKind.ShapeTriggerVisitor:
                    return node.shape.isValid
                        && EqualityComparer<PhysicsShape>.Default.Equals(node.shape, m_PendingShape)
                        && node.secondShape.isValid
                        && m_PendingSecondShape.isValid
                        && node.secondShape.ToString() == m_PendingSecondShape.ToString();
                case NodeKind.Joint:
                    return node.joint.isValid && EqualityComparer<PhysicsJoint>.Default.Equals(node.joint, m_PendingJoint);
                default:
                    return false;
            }
        }

        // Recursively search a TreeViewItemData<Node> for the first item whose data satisfies the predicate.
        // Returns the item id when found.
        private static int? FindItemId(TreeViewItemData<Node> item, System.Func<TreeViewItemData<Node>, bool> match)
        {
            if (match(item))
                return item.id;
            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    var result = FindItemId(child, match);
                    if (result.HasValue)
                        return result;
                }
            }
            return null;
        }

        // Walk down from rootItem, expanding every ancestor on the path that ends at the target id.
        private void ExpandToItem(TreeViewItemData<Node> rootItem, int targetId)
        {
            var path = new List<int>();
            if (BuildPath(rootItem, targetId, path))
            {
                // Expand every ancestor (path includes target itself, so skip it).
                for (int i = 0; i < path.Count - 1; ++i)
                    m_TreeView.ExpandItem(path[i]);
            }
        }

        private static bool BuildPath(TreeViewItemData<Node> item, int targetId, List<int> path)
        {
            path.Add(item.id);
            if (item.id == targetId)
                return true;
            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    if (BuildPath(child, targetId, path))
                        return true;
                }
            }
            path.RemoveAt(path.Count - 1);
            return false;
        }
    }
}
