using System.Collections.Generic;
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
        private SerializedObject m_SceneWorldEditorSerializedObject;
        
        private SerializedProperty m_DefaultWorldProperty;
        private SerializedProperty m_WorldDefinitionProperty;
        private SerializedProperty m_WorldProfileProperty;
        private SerializedProperty m_WorldCountersProperty;
        
        private VisualElement m_ShowHideWorldDefinition;

        private Foldout m_InfoFoldout;
        private const float InfoUpdatePeriod = 0.25f;
        private float m_InfoUpdateTime;
        
        public PhysicsWorld.WorldProfile WorldProfile;
        public PhysicsWorld.WorldCounters WorldCounters;

        private void OnEnable()
        {
            m_DefaultWorldProperty = serializedObject.FindProperty(nameof(SceneWorld.UseDefaultWorld));
            m_WorldDefinitionProperty = serializedObject.FindProperty(nameof(SceneWorld.WorldDefinition));
            
            m_SceneWorldEditorSerializedObject = new SerializedObject(this);
            m_WorldProfileProperty = m_SceneWorldEditorSerializedObject.FindProperty(nameof(WorldProfile));
            m_WorldCountersProperty = m_SceneWorldEditorSerializedObject.FindProperty(nameof(WorldCounters));

            // Hook into post-simulate.
            PhysicsEvents.PostSimulate += PostSimulateHandler;
        }

        private void OnDisable()
        {
            m_SceneWorldEditorSerializedObject.Dispose();
            
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
            var defaultWorldProperty = new PropertyField(m_DefaultWorldProperty);
            root.Add(defaultWorldProperty);
            defaultWorldProperty.RegisterValueChangeCallback(_ => { m_ShowHideWorldDefinition.style.display = m_DefaultWorldProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex; });

            // World Definition.
            m_ShowHideWorldDefinition.Add(new PropertyField(m_WorldDefinitionProperty));
            root.Add(m_ShowHideWorldDefinition);

            // Info.
            m_InfoFoldout = new Foldout() { text = "Info", value = false, style = { marginTop = 4 }, viewDataKey = $"{typeof(SceneWorldEditor)}_Info" };
            var profilePropertyField = new PropertyField(m_WorldProfileProperty);
            var countersPropertyField = new PropertyField(m_WorldCountersProperty);
            profilePropertyField.Bind(m_SceneWorldEditorSerializedObject);
            countersPropertyField.Bind(m_SceneWorldEditorSerializedObject);
            m_InfoFoldout.Add(profilePropertyField);
            m_InfoFoldout.Add(countersPropertyField);
            root.Add(m_InfoFoldout);
            
            return root;
        }
    }
}