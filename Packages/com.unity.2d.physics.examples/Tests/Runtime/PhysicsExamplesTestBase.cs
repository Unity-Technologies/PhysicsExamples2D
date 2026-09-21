using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Unity.U2D.Physics.Examples.Tests
{
    // Shared base for the example component tests.
    // It tracks every object a test creates and destroys them all in TearDown, so each test starts from a clean scene.
    public abstract class PhysicsExamplesTestBase
    {
        private readonly List<Object> m_Created = new();

        // Create a tracked GameObject.
        protected GameObject NewObject(string name, bool active = true)
        {
            var go = new GameObject(name);
            go.SetActive(active);
            m_Created.Add(go);
            return go;
        }

        // Register any already-created Object (e.g. a Sprite or Texture2D built directly) so it is destroyed in TearDown.
        // Returns the object so it can be tracked inline at its creation site.
        protected T Track<T>(T obj) where T : Object
        {
            m_Created.Add(obj);
            return obj;
        }

        [TearDown]
        public void DestroyCreatedObjects()
        {
            // DestroyImmediate runs OnDisable/OnDestroy synchronously in both edit and play mode.
            foreach (var created in m_Created)
            {
                if (created != null)
                    Object.DestroyImmediate(created);
            }

            m_Created.Clear();
        }
    }
}
