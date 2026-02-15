using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    /// <summary>
    /// A wrapper around a World that also provides world drawing.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.TestWorld)]
    [AddComponentMenu("Physics 2D/CoreExamples/Test World", 0)]
    [Icon(IconUtility.IconPath + "TestWorld.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, "SceneWorld")]
    public sealed class TestWorld : MonoBehaviour
    {
        public bool UseDefaultWorld = true;
        public PhysicsWorldDefinition WorldDefinition = PhysicsWorldDefinition.defaultDefinition;
        public PhysicsUserData UserData;

        public delegate void TestWorldCreateEventHandler(TestWorld testWorld);

        public delegate void TestWorldDestroyEventHandler(TestWorld testWorld);

        public event TestWorldCreateEventHandler CreateWorldEvent;
        public event TestWorldDestroyEventHandler DestroyWorldEvent;

        public PhysicsWorld world => m_World;
        private PhysicsWorld m_World;
        private int m_OwnerKey;

#if UNITY_EDITOR
        // Editor selection.
        private static readonly List<IWorldDrawable> SelectedDrawables = new();
        static TestWorld() => EditorApplication.update += DrawEditorSelections;
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
        /// Find a TestWorld.
        /// </summary>
        /// <param name="obj">The GameObject used to find the most likely TestWorld.</param>
        /// <returns>The found TestWorld or NULL if not found at all.</returns>
        public static TestWorld FindTestWorld(GameObject obj)
        {
            // Find a TestWorld up this hierarchy.
            var sceneWorld = obj.GetComponentInParent<TestWorld>();

            // If we didn't find one then find the first in the world.
            if (sceneWorld == null)
                sceneWorld = FindAnyObjectByType<TestWorld>();

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