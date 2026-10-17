using System;

using UnityEngine;

namespace Unity.U2D.RadianceCascades
{
    /// <summary>
    /// The look of the background surface that receives the global illumination.
    /// A flat dark surface with a faint regular grid, chosen so the lighting itself is what reads on screen
    /// and so artifacts in it, which straight lines show up clearly, are easy to spot.
    /// </summary>
    [Serializable]
    public sealed class BackgroundSettings
    {
        /// <summary>
        /// How much of the light landing between the grid lines the surface sends back.
        /// A surface reflecting everything disappears into the light, so with emitters spread across the screen this is what keeps
        /// the lights reading as lights rather than as a fog.
        /// </summary>
        [ColorUsage(false)]
        public Color baseColor = new(0.16f, 0.17f, 0.19f, 1f);

        /// <summary>
        /// How much of that light a grid line sends back. Brighter than the surface around it is what makes a line a line.
        /// </summary>
        [ColorUsage(false)]
        public Color lineColor = new(0.30f, 0.36f, 0.46f, 1f);

        /// <summary>
        /// How many grid cells fit across the height of the view. Cells stay square regardless of the window's shape.
        /// </summary>
        [Range(2f, 60f)]
        public float cellsAcross = 16f;

        /// <summary>
        /// How thick a grid line is, as a fraction of a cell. Zero leaves the surface completely flat,
        /// which is the clearest way to judge where light is landing, since then anything visible is the lighting.
        /// </summary>
        [Range(0f, 0.2f)]
        public float lineThickness = 0.02f;

        /// <summary>
        /// Shifts the grid, so it can be offset or drifted.
        /// </summary>
        public Vector2 offset;

        /// <summary>
        /// How far the number of cells can drift either side of the count above, in cells.
        /// It drifts in step with the lean, so the grid settles as one rather than the two moving against each other.
        /// Zero holds the count fixed.
        /// </summary>
        [Range(0f, 30f)]
        public float cellsVariation = 5f;

        /// <summary>
        /// How far the grid's plane can lean away from facing the camera, in degrees about each axis.
        /// Leaning it is what gives the grid a vanishing point, so the larger these are the deeper the view into it reads.
        /// </summary>
        public Vector3 tiltAmplitude = new(14f, 14f, 5f);

        /// <summary>
        /// How long the grid takes to lean from one angle to the next, in seconds. The angles themselves are picked at random
        /// within the amplitude, so a longer time is a slower and less noticeable drift rather than a smaller movement.
        /// Zero holds the grid square to the camera.
        /// </summary>
        [Min(0f)]
        public float tiltInterval = 9f;

        /// <summary>
        /// How near the camera sits to the grid's plane. Small values put it close, so the cells spread hard toward the viewer and
        /// crowd toward the vanishing point; large values flatten the whole thing back toward a plain grid.
        /// </summary>
        [Range(0.3f, 6f)]
        public float perspective = 1.1f;
    }
}
