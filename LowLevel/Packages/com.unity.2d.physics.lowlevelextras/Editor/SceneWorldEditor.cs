using UnityEditor.UIElements;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Physics.LowLevelExtras
{
    [CustomEditor(typeof(SceneWorld))]
    [CanEditMultipleObjects]
    public class SceneWorldEditor : Editor
    {
        private VisualElement m_ShowHideWorldDefinition;

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
            if (!m_InfoFoldout.value ||
                ((SceneWorld)target).World != world)
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
            var root = new VisualElement();
            m_ShowHideWorldDefinition = new VisualElement();

            // Default World.
            var defaultWorldProperty = serializedObject.FindProperty(nameof(SceneWorld.UseDefaultWorld));            
            var defaultWorldPropertyField = new PropertyField(defaultWorldProperty);
            root.Add(defaultWorldPropertyField);
            defaultWorldPropertyField.RegisterValueChangeCallback(_ => { m_ShowHideWorldDefinition.style.display = defaultWorldProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex; });

            // World Definition.
            m_ShowHideWorldDefinition.Add(new PropertyField(serializedObject.FindProperty(nameof(SceneWorld.WorldDefinition))));
            root.Add(m_ShowHideWorldDefinition);

            // Info.
            m_InfoFoldout = new Foldout { text = "Info", value = false, style = { marginTop = 4 }, viewDataKey = $"{typeof(SceneWorldEditor)}_Info" };
            var profilePropertyField = new PropertyField(serializedObject.FindProperty(nameof(WorldProfile)));
            var countersPropertyField = new PropertyField(serializedObject.FindProperty(nameof(WorldCounters)));
            profilePropertyField.Bind(serializedObject);
            countersPropertyField.Bind(serializedObject);
            m_InfoFoldout.Add(profilePropertyField);
            m_InfoFoldout.Add(countersPropertyField);
            root.Add(m_InfoFoldout);
            
            return root;
        }
    }
}