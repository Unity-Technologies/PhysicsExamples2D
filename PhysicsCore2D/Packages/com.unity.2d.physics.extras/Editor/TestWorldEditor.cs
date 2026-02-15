using UnityEditor.UIElements;
using Unity.U2D.Physics.Extras;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Unity.U2D.Physics.Editor.Extras
{
    [CustomEditor(typeof(TestWorld))]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.EditorSourceNamespace)]
    public class TestWorldEditor : UnityEditor.Editor
    {
        private VisualElement m_ShowHideWorldDefinition;
        private SerializedObject m_TestWorldEditorSerializedObject;

        private Foldout m_InfoFoldout;
        private const float InfoUpdatePeriod = 0.25f;
        private float m_InfoUpdateTime;

        public PhysicsWorld.WorldProfile WorldProfile;
        public PhysicsWorld.WorldCounters WorldCounters;

        private void OnEnable()
        {
            // Hook into post-simulate.
            PhysicsEvents.PostSimulate += PostSimulateHandler;
        }

        private void OnDisable()
        {
            // unhook from post-simulate.
            PhysicsEvents.PostSimulate -= PostSimulateHandler;
        }

        private void PostSimulateHandler(PhysicsWorld world, float deltaTime)
        {
            // Check if the update is relevant.
            if (m_InfoFoldout == null ||
                !m_InfoFoldout.value ||
                ((TestWorld)target).world != world)
                return;

            // Only allow periodic updates (for performance reasons).
            // NOTE: We skip this if paused because we always want fresh info if single-stepping.
            m_InfoUpdateTime += deltaTime;
            if (!EditorApplication.isPaused && m_InfoUpdateTime < InfoUpdatePeriod)
                return;

            // Update the info.
            WorldProfile = world.profile;
            WorldCounters = world.counters;

            // Reset update time.
            m_InfoUpdateTime = 0.0f;
        }

        public override VisualElement CreateInspectorGUI()
        {
            m_TestWorldEditorSerializedObject = new SerializedObject(this);

            var root = new VisualElement();
            m_ShowHideWorldDefinition = new VisualElement();

            // Default World.
            var defaultWorldProperty = serializedObject.FindProperty(nameof(TestWorld.UseDefaultWorld));
            var defaultWorldPropertyField = new PropertyField(defaultWorldProperty);
            root.Add(defaultWorldPropertyField);
            defaultWorldPropertyField.RegisterValueChangeCallback(_ => { m_ShowHideWorldDefinition.style.display = defaultWorldProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex; });

            // World Definition.
            m_ShowHideWorldDefinition.Add(new PropertyField(serializedObject.FindProperty(nameof(TestWorld.WorldDefinition))));
            m_ShowHideWorldDefinition.Add(new PropertyField(serializedObject.FindProperty(nameof(TestWorld.UserData))));
            root.Add(m_ShowHideWorldDefinition);

            // Info.
            m_InfoFoldout = new Foldout { text = "Info", value = false, style = { marginTop = 4 }, viewDataKey = $"{typeof(TestWorldEditor)}_Info" };
            var profilePropertyField = new PropertyField(m_TestWorldEditorSerializedObject.FindProperty(nameof(WorldProfile)));
            var countersPropertyField = new PropertyField(m_TestWorldEditorSerializedObject.FindProperty(nameof(WorldCounters)));
            profilePropertyField.Bind(m_TestWorldEditorSerializedObject);
            countersPropertyField.Bind(m_TestWorldEditorSerializedObject);
            m_InfoFoldout.Add(profilePropertyField);
            m_InfoFoldout.Add(countersPropertyField);
            root.Add(m_InfoFoldout);

            return root;
        }
    }
}