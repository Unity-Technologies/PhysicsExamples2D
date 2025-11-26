#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Pool;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    public static class WorldSceneTransformMonitor
    {
        private static readonly Dictionary<Transform, HashSet<IWorldSceneTransformChanged>> Monitors = new();
        private static readonly List<Transform> ChangedTransforms = new();
        private static readonly List<IWorldSceneTransformChanged> BufferedCallbacks = new();

        [InitializeOnLoadMethod]
        public static void InitializeAllWatchers()
        {
            Monitors.Clear();
            ChangedTransforms.Clear();
            EditorApplication.update += MonitorUpdate;
        }

        private static void MonitorUpdate()
        {
            // We only do this in edit mode.
            if (EditorApplication.isPlaying)
                return;

            // Check for transform changes.
            foreach (var monitor in Monitors)
            {
                // Skip if the transform hasn't changed.
                var transform = monitor.Key.transform;
                if (!transform.hasChanged)
                    continue;

                // Add to transforms that have changed.
                // We may get duplicates here, but it doesn't matter.
                ChangedTransforms.Add(transform);

                // Fetch the buffered callbacks.
                // NOTE: We do this because the callbacks can have side effects such as changing the callback enumeration.
                BufferedCallbacks.Clear();
                BufferedCallbacks.AddRange(monitor.Value);
                
                // Call the monitors.
                foreach (var callback in BufferedCallbacks)
                    callback.TransformChanged();
            }

            // Reset changed transforms.
            foreach (var transform in ChangedTransforms)
                transform.hasChanged = false;

            // Reset the list.
            ChangedTransforms.Clear();
        }

        public static void AddMonitor(Component component) => AddMonitor(component.transform, component as IWorldSceneTransformChanged);
        
        public static void AddMonitor(Transform transform, IWorldSceneTransformChanged callback)
        {
            if (transform == null)
                throw new NullReferenceException(nameof(transform));
            
            if (callback == null)
                throw new NullReferenceException(nameof(callback));

            if (Monitors.TryGetValue(transform, out var callbacks))
            {
                callbacks.Add(callback);
                return;
            }

            // Add a new callback.
            var newCallbacks = HashSetPool<IWorldSceneTransformChanged>.Get();
            newCallbacks.Add(callback);
            Monitors.Add(transform, newCallbacks);
        }

        public static void RemoveMonitor(Transform transform, IWorldSceneTransformChanged callback)
        {
            if (transform == null)
                throw new NullReferenceException(nameof(transform));
            
            if (callback == null)
                throw new NullReferenceException(nameof(callback));

            // Finish if there's no monitors found.
            if (!Monitors.TryGetValue(transform, out var callbacks))
                return;
            
            // Remove the callback.
            callbacks.Remove(callback);

            // Finish if callbacks still exist.
            if (callbacks.Count > 0)
                return;
            
            // Release the callbacks.
            HashSetPool<IWorldSceneTransformChanged>.Release(callbacks);
            
            // Remove from the monitors.
            Monitors.Remove(transform);
        }
        
        public static void RemoveMonitor(Component component) => RemoveMonitor(component.transform, component as IWorldSceneTransformChanged);
    }
}

#endif