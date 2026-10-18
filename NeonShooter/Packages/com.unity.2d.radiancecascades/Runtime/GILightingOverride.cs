using Unity.Scripting.LifecycleManagement;

using UnityEngine;

namespace Unity.U2D.RadianceCascades
{
    /// <summary>
    /// Lighting settings belonging to one scene, which replace the ones on the renderer feature while that scene is loaded.
    /// Add it to a scene to light that scene its own way without changing the renderer asset every other scene shares.
    /// </summary>
    /// <remarks>
    /// A scene with none of these is lit by the feature's own settings, so adding this to a test scene leaves the game untouched.
    /// Every value applies while playing, so they can be dragged and watched.
    /// </remarks>
    [AutoStaticsCleanup]
    public sealed partial class GILightingOverride : MonoBehaviour
    {
        /// <summary>
        /// The loaded scene's lighting settings, or null when no scene provides any.
        /// </summary>
        public static GILightingOverride instance { get; private set; }

        /// <summary>
        /// Whether the scene is lit by the radiance cascades at all.
        /// Turning it off skips the lighting and the background surface it draws, removing their cost, while sprites, particles and screen effects still draw.
        /// </summary>
        public bool lightingEnabled
        {
            get => m_LightingEnabled;
            set => m_LightingEnabled = value;
        }

        /// <summary>
        /// How many levels the cascade chain runs, which sets how far light carries across the screen.
        /// Setting it clamps to the range the lighting supports.
        /// </summary>
        public int cascadeCount
        {
            get => m_CascadeCount;
            set => m_CascadeCount = Mathf.Clamp(value, 1, GILightPass.MaxCascadeCount);
        }

        /// <summary>
        /// How many texels of the lighting's buffers one screen pixel covers.
        /// Setting it clamps to the range the lighting supports.
        /// </summary>
        public float renderScale
        {
            get => m_RenderScale;
            set => m_RenderScale = Mathf.Clamp(value, GILightPass.MinRenderScale, 1f);
        }

        /// <summary>
        /// How far, in texels, a ray of the finest level moves between two reads of the scene.
        /// Setting it clamps to the range the lighting supports.
        /// </summary>
        public float stepStride
        {
            get => m_StepStride;
            set => m_StepStride = Mathf.Clamp(value, 0.05f, 1f);
        }

        /// <summary>
        /// The brightness control, in stops.
        /// Setting it clamps to the range the lighting supports.
        /// </summary>
        public float exposure
        {
            get => m_Exposure;
            set => m_Exposure = Mathf.Clamp(value, -8f, 8f);
        }

        /// <summary>
        /// The look of the background surface the gathered light lands on.
        /// </summary>
        public BackgroundSettings background => m_Background;

        /// <summary>
        /// The most cascade levels the lighting supports; the upper limit of cascadeCount.
        /// </summary>
        public const int MaxCascadeCount = GILightPass.MaxCascadeCount;

        /// <summary>
        /// The smallest fraction of the screen the lighting can work at; the lower limit of renderScale.
        /// </summary>
        public const float MinRenderScale = GILightPass.MinRenderScale;

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this)
                instance = null;
        }

        #region Internal

        [SerializeField]
        private bool m_LightingEnabled = true;

        [SerializeField]
        [Range(1, GILightPass.MaxCascadeCount)]
        private int m_CascadeCount = 6;

        [SerializeField]
        [Range(GILightPass.MinRenderScale, 1f)]
        private float m_RenderScale = 0.25f;

        [SerializeField]
        [Range(0.05f, 1f)]
        private float m_StepStride = 0.125f;

        [SerializeField]
        [Range(-8f, 8f)]
        private float m_Exposure = -1.8f;

        [SerializeField]
        private BackgroundSettings m_Background = new();

        #endregion
    }
}
