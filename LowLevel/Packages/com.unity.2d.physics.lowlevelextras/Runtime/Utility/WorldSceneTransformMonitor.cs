#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    public static class WorldSceneTransformMonitor
    {
        private static readonly Dictionary<Component, IWorldSceneTransformChanged> Monitors = new();
        private static readonly List<Transform> ChangedTransforms = new();
        
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
                
                // Call the monitor.
                monitor.Value.TransformChanged();
            }

            // Reset changed transforms.
            foreach (var transform in ChangedTransforms)
                transform.hasChanged = false;
            
            // Reset the list.
            ChangedTransforms.Clear();
        }

        public static void AddMonitor(Component component)
        {
            if (component is not IWorldSceneTransformChanged worldSceneTransformChanged)
                throw new ArgumentException(nameof(component), $"Component must implement {nameof(IWorldSceneTransformChanged)}.");
            
            Monitors.Add(component, worldSceneTransformChanged);
        }

        public static void RemoveMonitor(Component component)
        {
            if (component is not IWorldSceneTransformChanged worldSceneTransformChanged)
                throw new ArgumentException(nameof(component), $"Component must implement {nameof(IWorldSceneTransformChanged)}.");
            
            Monitors.Remove(component);
        }
    }
}

#endif