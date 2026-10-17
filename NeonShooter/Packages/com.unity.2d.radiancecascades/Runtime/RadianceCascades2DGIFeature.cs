using Unity.Scripting.LifecycleManagement;

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.U2D.RadianceCascades
{
    /// <summary>
    /// Screen-space global illumination for the 2D renderer, built on radiance cascades.
    /// Projectiles, enemies, explosions and the starfield act as light emitters, and a procedurally generated dark background
    /// is the surface that receives the light, so the arena is revealed by whatever is currently firing rather than lit by placed lights.
    /// </summary>
    /// <remarks>
    /// Add this to the 2D Renderer asset. The passes inject per sorting layer, so the background lands under the sprite batch
    /// and the copy the lighting reads is taken after it.
    /// That copy is shared by every camera this renderer serves, so the feature assumes a single rendering camera.
    /// </remarks>
    [AutoStaticsCleanup]
    public sealed partial class RadianceCascades2DGIFeature : ScriptableRendererFeature2D
    {
        /// <summary>
        /// Builds the materials and the passes.
        /// Called by the pipeline whenever the feature is created or its serialized settings change.
        /// </summary>
        public override void Create()
        {
            m_BackgroundMaterial = EnsureMaterial(m_BackgroundMaterial, m_BackgroundShader, BackgroundShaderName);
            m_DistanceFieldMaterial = EnsureMaterial(m_DistanceFieldMaterial, m_DistanceFieldShader, DistanceFieldShaderName);
            m_CascadeMaterial = EnsureMaterial(m_CascadeMaterial, m_CascadeShader, CascadeShaderName);
            // Every batch of sprites is submitted as one instanced draw, so this is copied from a material asset that has instancing
            // switched on rather than built from the shader. A player build only keeps a shader's instanced form when some material
            // asset asks for it, so a material made at runtime leaves the build with a shader that cannot be drawn instanced at all,
            // and the sprites then reach the lighting as nothing.
            if (m_SceneCaptureMaterialSource != null)
            {
                CoreUtils.Destroy(m_SceneCaptureMaterial);

                m_SceneCaptureMaterial = new Material(m_SceneCaptureMaterialSource) { enableInstancing = true };
            }
            else
            {
                m_SceneCaptureMaterial = EnsureMaterial(m_SceneCaptureMaterial, m_SceneCaptureShader, SceneCaptureShaderName);

                if (m_SceneCaptureMaterial != null)
                    m_SceneCaptureMaterial.enableInstancing = true;
            }

            m_LightPass ??= new GILightPass();
            m_LightPass.sortingLayerName = m_SortingLayerName;
        }

        /// <summary>
        /// Enqueues the feature's passes for the camera being rendered.
        /// </summary>
        /// <param name="renderer">The renderer the passes are added to.</param>
        /// <param name="renderingData">The frame's rendering data.</param>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_LightPass == null || m_BackgroundMaterial == null)
                return;

            var overrides = GILightingOverride.instance;
            var cascadeCount = overrides != null ? overrides.cascadeCount : m_CascadeCount;
            var renderScale = overrides != null ? overrides.renderScale : m_RenderScale;

            EnsureIrradianceCaptureHandle(renderingData.cameraData.cameraTargetDescriptor, cascadeCount, renderScale);

            m_LightPass.irradianceCapture = m_IrradianceCaptureHandle;
            m_LightPass.spriteCapture = m_SpriteCaptureHandle;
            m_LightPass.debugView = m_DebugView;
            m_LightPass.backgroundMaterial = m_BackgroundMaterial;
            m_LightPass.distanceFieldMaterial = m_DistanceFieldMaterial;
            m_LightPass.cascadeMaterial = m_CascadeMaterial;
            m_LightPass.sceneCaptureMaterial = m_SceneCaptureMaterial;
            // A scene carrying its own settings is lit by those instead, so a test scene can be set up without changing the renderer
            // asset that every other scene shares.
            m_LightPass.cascadeCount = cascadeCount;
            m_LightPass.renderScale = renderScale;
            m_LightPass.stepStride = overrides != null ? overrides.stepStride : m_StepStride;
            m_LightPass.exposure = overrides != null ? overrides.exposure : m_Exposure;
            m_LightPass.settings = overrides != null ? overrides.background : m_Background;

            renderer.EnqueuePass(m_LightPass);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_BackgroundMaterial);
            CoreUtils.Destroy(m_DistanceFieldMaterial);
            CoreUtils.Destroy(m_CascadeMaterial);
            CoreUtils.Destroy(m_SceneCaptureMaterial);

            m_BackgroundMaterial = null;
            m_DistanceFieldMaterial = null;
            m_CascadeMaterial = null;
            m_SceneCaptureMaterial = null;

            m_IrradianceCaptureHandle?.Release();
            m_IrradianceCaptureHandle = null;

            m_SpriteCaptureHandle?.Release();
            m_SpriteCaptureHandle = null;

            m_LightPass?.Cleanup();
            m_LightPass = null;
        }

        /// <summary>
        /// How many stops the lighting is currently being held down by, applied on top of the feature's own exposure.
        /// Game code sets this to dim the lighting during effects that put far more light on screen than usual, and sets it back to zero afterward.
        /// </summary>
        public static float exposureOffset { get; set; }

        /// <summary>
        /// The light arriving at every point, as the lighting produced it.
        /// This is null while irradianceCaptureEnabled is false, because the copy is only made while something is measuring.
        /// </summary>
        public RTHandle irradiance => m_IrradianceCaptureHandle;

        /// <summary>
        /// The sprites the lighting gathered from, captured on the same terms as irradiance.
        /// This is null while irradianceCaptureEnabled is false.
        /// </summary>
        public RTHandle colorSource => m_SpriteCaptureHandle;

        /// <summary>
        /// The feature's own exposure setting, in stops, which the lighting applies before the tonemap so a measurement can report what the light is worth before it.
        /// It does not include exposureOffset or the exposure of a scene's lighting override.
        /// </summary>
        public float exposure => m_Exposure;

        /// <summary>
        /// Whether the finest cascade is also copied into a buffer that can be read back, which is what irradiance and colorSource expose.
        /// The copy costs the memory of a full cascade, so leave it off unless something is measuring.
        /// </summary>
        public bool irradianceCaptureEnabled
        {
            get => m_IrradianceCaptureEnabled;
            set => m_IrradianceCaptureEnabled = value;
        }

        // Returns a material for the shader, reusing the existing one when it already matches.
        // The shader is resolved by name when no explicit reference was assigned.
        private static Material EnsureMaterial(Material current, Shader assigned, string shaderName)
        {
            var shader = assigned != null ? assigned : Shader.Find(shaderName);

            if (shader == null)
            {
                Debug.LogWarning($"[RC2DGI] Could not find the shader \"{shaderName}\". The pass using it is disabled.");

                return current;
            }

            if (current != null && current.shader == shader)
                return current;

            CoreUtils.Destroy(current);

            return CoreUtils.CreateEngineMaterial(shader);
        }

        // Allocates or resizes the buffer the finest cascade is copied into, sized to match it.
        // Nothing is allocated while the copy is switched off, so the measurement costs nothing when nothing is measuring.
        private void EnsureIrradianceCaptureHandle(RenderTextureDescriptor cameraTargetDescriptor, int cascadeCount, float renderScale)
        {
            if (!m_IrradianceCaptureEnabled)
            {
                m_IrradianceCaptureHandle?.Release();
                m_IrradianceCaptureHandle = null;

                m_SpriteCaptureHandle?.Release();
                m_SpriteCaptureHandle = null;

                return;
            }

            var spriteDescriptor = cameraTargetDescriptor;

            spriteDescriptor.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
            spriteDescriptor.depthStencilFormat = GraphicsFormat.None;
            spriteDescriptor.msaaSamples = 1;
            spriteDescriptor.useMipMap = false;
            spriteDescriptor.autoGenerateMips = false;

            RenderingUtils.ReAllocateHandleIfNeeded(ref m_SpriteCaptureHandle, spriteDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: SpriteCaptureTextureName);

            var resolution = GILightPass.GetCascadeResolution(cameraTargetDescriptor, cascadeCount, renderScale);

            var descriptor = cameraTargetDescriptor;

            descriptor.width = resolution.x;
            descriptor.height = resolution.y;
            descriptor.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
            descriptor.depthStencilFormat = GraphicsFormat.None;
            descriptor.msaaSamples = 1;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;

            RenderingUtils.ReAllocateHandleIfNeeded(ref m_IrradianceCaptureHandle, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: IrradianceCaptureTextureName);
        }

        #region Internal

        private const string BackgroundShaderName = "Hidden/RC2DGI/Composite";
        private const string DistanceFieldShaderName = "Hidden/RC2DGI/DistanceField";
        private const string CascadeShaderName = "Hidden/RC2DGI/Cascade";
        private const string SceneCaptureShaderName = "Hidden/RC2DGI/SceneCapture";

        private const string IrradianceCaptureTextureName = "_RC2DGIIrradianceCapture";
        private const string SpriteCaptureTextureName = "_RC2DGISpriteCapture";

        // The shaders the passes draw with; all are found by name when left unassigned.
        [SerializeField]
        private Shader m_BackgroundShader;

        [SerializeField]
        private Shader m_DistanceFieldShader;

        [SerializeField]
        private Shader m_CascadeShader;

        [SerializeField]
        private Shader m_SceneCaptureShader;

        // The material the sprites are drawn with into the lighting's own buffer, copied at startup. It has to be a material asset
        // with instancing switched on, since that is what carries the shader's instanced form into a player build.
        [SerializeField]
        private Material m_SceneCaptureMaterialSource;

        // The sorting layer the passes inject around. The background draws before this layer's sprites and the copy is taken after them.
        // Note that the 2D renderer merges sorting layers that share the same lights into one batch, so a later sorting layer is not excluded by itself.
        [SerializeField]
        private string m_SortingLayerName = "Default";

        // How many levels the cascade chain runs. Each level costs about as much as the last while reaching four times as far,
        // so this sets how far light carries across the screen.
        [SerializeField]
        [Range(1, GILightPass.MaxCascadeCount)]
        private int m_CascadeCount = 6;

        // How many texels of the lighting's buffers one screen pixel covers. The gathered light is low frequency, so a fraction of
        // the screen's size costs nothing in the picture while the cost of the march falls with the square of it.
        //
        // A quarter is not a compromise: the whole chain runs on a sixteenth of the texels, and the picture comes out smoother as
        // well as faster, because the probe grid whose interpolation shows up in a shadow's edge is then coarse enough that the
        // cascade above resolves it. The one thing it costs is how finely a shape's outline is recorded, so an emitter smaller than
        // about four screen pixels starts to flicker as it moves between probes.
        [SerializeField]
        [Range(GILightPass.MinRenderScale, 1f)]
        private float m_RenderScale = 0.25f;

        // How far, in texels, a ray of the finest level moves between two reads of the scene. Every level above doubles it.
        // Smaller is more exact at the edges of small emitters and costs more reads per ray; the default matches the reference.
        [SerializeField]
        [Range(0.05f, 1f)]
        private float m_StepStride = 0.125f;

        // The one brightness control, in stops. The gathered light is scaled by two to this power before it is tonemapped for display.
        [SerializeField]
        [Range(-8f, 8f)]
        private float m_Exposure = -1.8f;

        // The look of the background surface the gathered light lands on.
        [SerializeField]
        private BackgroundSettings m_Background = new();

        // Whether the finest cascade is copied into a readable buffer, which the measurement harness switches on while it runs.
        [SerializeField]
        private bool m_IrradianceCaptureEnabled;

        // Draws one of the lighting's working buffers full screen instead of the lit surface, so each stage can be judged on its own.
        [SerializeField]
        private GILightPass.DebugView m_DebugView;

        private RTHandle m_IrradianceCaptureHandle;
        private RTHandle m_SpriteCaptureHandle;

        private Material m_BackgroundMaterial;
        private Material m_DistanceFieldMaterial;
        private Material m_CascadeMaterial;
        private Material m_SceneCaptureMaterial;
        private GILightPass m_LightPass;

        #endregion
    }
}
