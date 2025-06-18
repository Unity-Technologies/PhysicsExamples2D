using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(SceneManifest), true)]
    public class ScenePickerEditor : UnityEditor.Editor
    {
        private SerializedProperty m_SceneItemsProperty;
        
        private void OnEnable()
        {
            m_SceneItemsProperty = serializedObject.FindProperty("SceneItems");
        }

        public override void OnInspectorGUI()
        {
            var picker = target as SceneManifest;
            if (picker == null || picker.SceneItems == null)
                return;

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_SceneItemsProperty);
            
            for (var i = 0; i < picker.SceneItems.Count; ++i)
            {
                var sceneItemProperty = m_SceneItemsProperty.GetArrayElementAtIndex(i);
                var scenePathProperty = sceneItemProperty.FindPropertyRelative("ScenePath");
                var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProperty.stringValue);

                EditorGUI.BeginChangeCheck();
                var newScene = EditorGUILayout.ObjectField("Scene", oldScene, typeof(SceneAsset), false) as SceneAsset;
                if (EditorGUI.EndChangeCheck())
                    scenePathProperty.stringValue = AssetDatabase.GetAssetPath(newScene);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}