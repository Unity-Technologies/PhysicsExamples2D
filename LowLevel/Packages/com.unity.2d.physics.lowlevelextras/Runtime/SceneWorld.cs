using System.Collections.Generic;
using UnityEditor;
using UnityEngine.LowLevelPhysics2D;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    /// <summary>
    /// A wrapper around a World that also provides world drawing.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [DefaultExecutionOrder(PhysicsLowLevelExtrasExecutionOrder.SceneWorld)]
    [AddComponentMenu("Physics 2D (LowLevel)/Scene World", 2)]
    [Icon(IconUtility.IconPath + "SceneWorld.png")]
    public sealed class SceneWorld : MonoBehaviour
    {
        public bool UseDefaultWorld = true;
        public PhysicsWorldDefinition WorldDefinition = PhysicsWorldDefinition.defaultDefinition;
        public PhysicsUserData UserData;

        public delegate void SceneWorldCreateEventHandler(SceneWorld sceneWorld);

        public delegate void SceneWorldDestroyEventHandler(SceneWorld sceneWorld);

        public event SceneWorldCreateEventHandler CreateWorldEvent;
        public event SceneWorldDestroyEventHandler DestroyWorldEvent;

        public PhysicsWorld World => m_World;
        private PhysicsWorld m_World;
        private int m_OwnerKey;

#if UNITY_EDITOR
        // Editor selection.
        private static readonly List<IWorldSceneDrawable> SelectedDrawables = new();
        static SceneWorld() => EditorApplication.update += DrawEditorSelections;
        private static void DrawEditorSelections()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms == null || selectedTransforms.Length == 0)
                return;

            foreach (var xf in selectedTransforms)
            {
                xf.GetComponentsInChildren(SelectedDrawables);
                foreach (var drawable in SelectedDrawables)
                    drawable.Draw();
            }
        }
#endif
        /// <summary>
        /// Find a SceneWorld.
        /// </summary>
        /// <param name="obj">The GameObject used to find the most likely SceneWorld.</param>
        /// <returns>The found SceneWorld or NULL if not found at all.</returns>
        public static SceneWorld FindSceneWorld(GameObject obj)
        {
            // Find a SceneWorld up this hierarchy.
            var sceneWorld = obj.GetComponentInParent<SceneWorld>();

            // If we didn't find one then find the first in the world.
            if (sceneWorld == null)
                sceneWorld = FindAnyObjectByType<SceneWorld>();

            return sceneWorld;
        }

        private void Reset()
        {
            // Use the default world definition.
            WorldDefinition = new PhysicsWorldDefinition();
        }

        /// <summary>
        /// Create the world and register the debug drawers.
        /// </summary>
        private void OnEnable() => CreateWorld();

        /// <summary>
        /// Destroy the world.
        /// </summary>
        private void OnDisable() => DestroyWorld();

        private void CreateWorld()
        {
            if (UseDefaultWorld)
            {
                m_World = PhysicsWorld.defaultWorld;
            }
            else
            {
                m_World = PhysicsWorld.Create(definition: WorldDefinition);
                if (m_World.isValid)
                {
                    // Set the user data.
                    m_World.userData = UserData;

                    // Set the owner.
                    m_OwnerKey = m_World.SetOwner(this);
                }
            }

            // Notify.
            if (m_World.isValid)
                CreateWorldEvent?.Invoke(this);
        }

        private void DestroyWorld()
        {
            // Nothing to destroy if invalid.
            if (!m_World.isValid)
                return;

            // Notify.
            DestroyWorldEvent?.Invoke(this);

            // Destroy the world if not the default world.
            if (!m_World.isDefaultWorld)
            {
                m_World.Destroy(m_OwnerKey);
                m_World = default;
                m_OwnerKey = 0;
            }
        }

        private void OnValidate()
        {
            // Write the definition if the world is valid and not the default world.
            if (m_World.isValid && !m_World.isDefaultWorld)
                m_World.definition = WorldDefinition;
        }

        public override string ToString() => m_World.ToString();
    }
}