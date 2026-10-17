using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Unity.U2D.RadianceCascades
{
    // Gathers the light the sprites give off and draws it as the background underneath them.
    //
    // It runs just before the sprite batch for its sorting layer, and draws the sprites itself into a buffer of its own: every
    // sprite's color weighted by how much of a point it covers, over a transparent clear, so the alpha says how much that is and a
    // point with no alpha is empty space a ray travels through. The sprites it draws come from the same instance data the batch is
    // about to submit, so the light matches what is on screen this frame rather than lagging a frame behind it. Owning the buffer is
    // also what lets the background be laid down opaque and the sprites drawn over it, leaving the camera's own target with nothing
    // to carry for the lighting's sake.
    sealed class GILightPass : ScriptableRenderPass2D
    {
        public GILightPass()
        {
            renderPassEvent2D = RenderPassEvent2D.BeforeRenderingSprites;
        }

        // The sorting layer the pass injects around. The owning feature sets this whenever its own settings change, which is why the
        // pass is kept alive across those changes rather than rebuilt: rebuilding it would throw away the meshes built for the
        // sprites, and the settings change arrives during a callback in which those cannot be destroyed.
        public string sortingLayerName
        {
            set => renderPassSortingLayerID = SortingLayer.NameToID(value);
        }

        // Records the draw of the sprites, the cascade chain, and then the background composite.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (backgroundMaterial == null || settings == null)
                return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var colorTarget = resourceData.activeColorTexture;

            // The target is only ever written, never read back, so it does not matter whether the renderer gave us an intermediate
            // texture or the window itself.
            if (!colorTarget.IsValid())
                return;

            var cameraData = frameData.Get<UniversalCameraData>();
            var targetDescriptor = cameraData.cameraTargetDescriptor;

            var screen = new Vector2(Mathf.Max(1, targetDescriptor.width), Mathf.Max(1, targetDescriptor.height));

            var irradiance = TextureHandle.nullHandle;
            var color = TextureHandle.nullHandle;
            var bufferScale = Vector2.one;

            if (cascadeMaterial != null && sceneCaptureMaterial != null && distanceFieldMaterial != null)
            {
                // Everything the lighting works on shares one size, so a cascade texel is exactly one texel of the buffer the rays
                // read and the probe spacing of the finest level is exactly one. Running the scene capture at another size instead
                // leaves the two grids slightly out of step, which beats against itself and stripes the result.
                var texelsPerPixel = Mathf.Clamp(renderScale, MinRenderScale, 1f);
                var resolution = GetCascadeResolution(targetDescriptor, cascadeCount, texelsPerPixel);

                // How much of that buffer the screen actually covers. Everything that reads it scales into that corner rather
                // than stretching across the whole of it.
                bufferScale = new Vector2(screen.x * texelsPerPixel / resolution.x, screen.y * texelsPerPixel / resolution.y);

                color = RecordSceneCapture(renderGraph, cameraData, resolution, texelsPerPixel);

                if (color.IsValid())
                {
                    var distanceField = RecordDistanceField(renderGraph, color, resolution);

                    irradiance = RecordCascades(renderGraph, color, distanceField, resolution);

                    RecordCapture(renderGraph, spriteCapture, color);
                    RecordCapture(renderGraph, irradianceCapture, irradiance);
                }
            }

            RecordBackground(renderGraph, colorTarget, irradiance, color, bufferScale, screen.x / screen.y);
        }

        // Draws the frame's sprites into a buffer the lighting owns, giving it both what every point gives off and how much of that
        // point is covered, without the camera's own target having to carry either.
        //
        // The camera's frustum is drawn into the bottom left corner of the buffer at the given number of texels per screen pixel, and
        // every later stage maps the two onto each other from that corner.
        private TextureHandle RecordSceneCapture(RenderGraph renderGraph, UniversalCameraData cameraData, Vector2Int resolution, float texelsPerPixel)
        {
            var spriteRenderSystem = SpriteRenderSystem.instance;

            if (spriteRenderSystem == null)
                return TextureHandle.nullHandle;

            m_Batches ??= new List<SpriteRenderSystem.Batch>();
            m_BatchMeshes ??= new List<Mesh>();

            spriteRenderSystem.GetBatches(m_Batches);

            m_BatchMeshes.Clear();

            for (var i = 0; i < m_Batches.Count; ++i)
                m_BatchMeshes.Add(GetSpriteMesh(m_Batches[i].sprite));

            var desc = new TextureDesc(resolution.x, resolution.y)
            {
                name = SpriteTextureName,
                format = GraphicsFormat.R16G16B16A16_SFloat,

                // Cleared to nothing at all, so every point the sprites do not reach reads as empty space rather than as a black surface.
                clearBuffer = true,
                clearColor = Color.clear,

                // The rays step through this in fixed strides that do not land on texel centers, so it is read as a continuous field.
                // Reading the nearest texel instead makes a ray's answer jump with which column or row its steps happen to land on,
                // which neighboring rays do differently and shows as fine streaks along the ray direction.
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var sprites = renderGraph.CreateTexture(desc);

            m_CaptureTransforms ??= new Matrix4x4[MaxInstancesPerCaptureDraw];
            m_CaptureColors ??= new Vector4[MaxInstancesPerCaptureDraw];

            using (var builder = renderGraph.AddRasterRenderPass<SceneCapturePassData>(SceneCapturePassName, out var passData))
            {
                passData.material = sceneCaptureMaterial;
                passData.properties = m_CaptureProperties;
                passData.batches = m_Batches;
                passData.meshes = m_BatchMeshes;
                passData.transforms = m_CaptureTransforms;
                passData.colors = m_CaptureColors;
                // The buffer is square and larger than the camera's image, and the camera occupies its bottom left corner. Building
                // the projection from that directly, rather than remapping the camera's own afterwards, keeps it the same way up as
                // the coordinates everything downstream reads it with.
                var camera = cameraData.camera;
                var unitsPerTexel = camera.orthographicSize * 2f / camera.pixelHeight / texelsPerPixel;
                var origin = camera.transform.position;
                var left = origin.x - camera.orthographicSize * camera.aspect;
                var bottom = origin.y - camera.orthographicSize;
                var span = resolution.x * unitsPerTexel;

                var ortho = Matrix4x4.Ortho(left, left + span, bottom, bottom + span, camera.nearClipPlane, camera.farClipPlane);

                passData.viewProjection = GL.GetGPUProjectionMatrix(ortho, true) * cameraData.GetViewMatrix();

                builder.SetRenderAttachment(sprites, 0, AccessFlags.Write);

                builder.SetRenderFunc(static (SceneCapturePassData data, RasterGraphContext context) =>
                {
                    for (var i = 0; i < data.batches.Count; ++i)
                    {
                        var batch = data.batches[i];
                        var mesh = data.meshes[i];

                        if (mesh == null || batch.sprite == null || batch.sprite.texture == null)
                            continue;

                        // Split into runs the instancing buffer can hold, which means copying each run to the front of a scratch array
                        // because an instanced draw always reads from the start of the arrays it is given.
                        for (var offset = 0; offset < batch.count; offset += MaxInstancesPerCaptureDraw)
                        {
                            var length = Mathf.Min(MaxInstancesPerCaptureDraw, batch.count - offset);

                            Array.Copy(batch.transforms, offset, data.transforms, 0, length);
                            Array.Copy(batch.colors, offset, data.colors, 0, length);

                            data.properties.Clear();
                            data.properties.SetMatrix(CaptureViewProjectionId, data.viewProjection);
                            data.properties.SetTexture(SpriteTexId, batch.sprite.texture);
                            data.properties.SetVectorArray(InstanceColorId, data.colors);

                            context.cmd.DrawMeshInstanced(mesh, 0, data.material, 0, data.transforms, length, data.properties);
                        }
                    }
                });
            }

            return sprites;
        }

        // Returns a mesh matching one sprite, built once and kept.
        //
        // The sprite's own vertices are used rather than a plain quad, so the pivot, the pixels per unit and any tight packing all come
        // out the same as the draw this is reproducing, and the sprite's coordinates already cover its region of whatever texture it
        // was packed into.
        private Mesh GetSpriteMesh(Sprite sprite)
        {
            if (sprite == null)
                return null;

            m_SpriteMeshes ??= new Dictionary<Sprite, Mesh>();

            if (m_SpriteMeshes.TryGetValue(sprite, out var mesh) && mesh != null)
                return mesh;

            var spriteVertices = sprite.vertices;
            var spriteTriangles = sprite.triangles;

            var positions = new Vector3[spriteVertices.Length];

            for (var i = 0; i < spriteVertices.Length; ++i)
                positions[i] = spriteVertices[i];

            var indices = new int[spriteTriangles.Length];

            for (var i = 0; i < spriteTriangles.Length; ++i)
                indices[i] = spriteTriangles[i];

            mesh = new Mesh
            {
                name = $"{SpriteMeshNamePrefix}{sprite.name}",
                hideFlags = HideFlags.HideAndDontSave
            };

            mesh.vertices = positions;
            mesh.uv = sprite.uv;
            mesh.triangles = indices;

            m_SpriteMeshes[sprite] = mesh;

            return mesh;
        }

        // Destroys the meshes built for the sprites, which the owning feature calls when it is torn down.
        public void Cleanup()
        {
            if (m_SpriteMeshes == null)
                return;

            foreach (var pair in m_SpriteMeshes)
                CoreUtils.Destroy(pair.Value);

            m_SpriteMeshes.Clear();
        }

        // The material the background draw uses, the one that builds the distance field, and the one that draws a cascade level.
        // The owning feature builds all three and keeps them alive.
        public Material backgroundMaterial { get; set; }

        public Material distanceFieldMaterial { get; set; }

        public Material cascadeMaterial { get; set; }

        // The material the sprites are drawn with into the lighting's own buffer. It has to have instancing enabled, since every batch
        // is submitted as one instanced draw.
        public Material sceneCaptureMaterial { get; set; }

        // How many levels the cascade chain runs. Each level costs about as much as the last while reaching four times as far,
        // so this sets how far light carries across the screen.
        public int cascadeCount { get; set; } = 6;

        // How many texels of the lighting's buffers one screen pixel covers. The gathered light is low frequency, so a fraction of
        // the screen's size costs little in the picture while the cost of the march falls with the square of it.
        public float renderScale { get; set; } = 0.25f;

        // How far, in texels, a ray of the finest level moves between two reads of the scene. Every level above doubles it, which
        // keeps the number of reads per ray the same at every level while each level's ray is four times as long as the last.
        public float stepStride { get; set; } = 0.125f;

        // The one brightness control, in stops. The gathered light is scaled by two to this power before it is tonemapped for display.
        public float exposure { get; set; } = -1.8f;

        // The look of the background surface the gathered light lands on.
        public BackgroundSettings settings { get; set; }

        // Where the finest cascade is copied so it can be read back and measured. Leaving it null skips the copy entirely.
        // The cascade itself cannot be read any other way: it is a render graph texture, and the graph hands the same pooled
        // texture to several levels within one frame, so reading one by name afterwards reads whichever level wrote it last.
        public RTHandle irradianceCapture { get; set; }

        // Where the copy of the sprites is copied again so it can be read back and measured, on the same terms as the cascade above.
        public RTHandle spriteCapture { get; set; }

        // Which of the lighting's working buffers is drawn in place of the lit surface.
        public DebugView debugView { get; set; }

        // Builds how far the nearest surface is from every texel, which is how far a ray may move between two reads of the scene.
        // Without it a ray crossing the screen reads every texel along its whole length whether anything is there or not, which is
        // almost all of the march's cost on a screen that is mostly empty.
        private TextureHandle RecordDistanceField(RenderGraph renderGraph, TextureHandle color, Vector2Int resolution)
        {
            var extent = new Vector4(resolution.x, resolution.y, 0f, 0f);

            var floodDesc = new TextureDesc(resolution.x, resolution.y)
            {
                name = NearestSurfaceTextureName,

                // Full precision, because the flood carries a texture coordinate and half precision cannot resolve one to better than
                // about a texel, which would round the nearest surface onto the wrong one.
                format = GraphicsFormat.R32G32_SFloat,
                clearBuffer = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var front = renderGraph.CreateTexture(floodDesc);

            floodDesc.name = NearestSurfaceAlternateTextureName;

            var back = renderGraph.CreateTexture(floodDesc);

            m_SeedProperties.SetVector(RenderExtentId, extent);

            RecordDistancePass(renderGraph, SeedPassName, SeedPass, m_SeedProperties, ColorTexId, color, front);

            // Each round halves the jump, starting wide enough to cross the whole image. The halving sequence alone leaves a scatter
            // of texels holding a surface that is not their nearest, because a texel can only ever learn about a surface one of its
            // eight sampled neighbors already knows. Repeating the last two rounds lets those texels pick up what their immediate
            // neighbors settled on, which is where the correct answer has by then reached.
            m_Jumps ??= new List<int>();
            m_Jumps.Clear();

            for (var jump = Mathf.NextPowerOfTwo(Mathf.Max(resolution.x, resolution.y)) / 2; jump >= 1; jump /= 2)
                m_Jumps.Add(jump);

            m_Jumps.Add(2);
            m_Jumps.Add(1);

            for (var i = 0; i < m_Jumps.Count; ++i)
            {
                var properties = GetFloodProperties(i);

                properties.SetVector(RenderExtentId, extent);
                properties.SetFloat(JumpDistanceId, m_Jumps[i]);

                RecordDistancePass(renderGraph, FloodPassName, FloodPass, properties, SourceTexId, front, back);

                (front, back) = (back, front);
            }

            var distanceDesc = floodDesc;

            distanceDesc.name = DistanceFieldTextureName;
            distanceDesc.format = GraphicsFormat.R16_SFloat;

            // Point sampled, because a ray reads this to decide how far it may move and an average of two texels' distances is not a
            // distance anything is actually clear by.
            distanceDesc.filterMode = FilterMode.Point;

            var distanceField = renderGraph.CreateTexture(distanceDesc);

            m_DistanceProperties.SetVector(RenderExtentId, extent);

            RecordDistancePass(renderGraph, DistancePassName, DistancePass, m_DistanceProperties, SourceTexId, front, distanceField);

            return distanceField;
        }

        // Records one full-screen draw of the distance field shader, reading one texture and writing another.
        private void RecordDistancePass(RenderGraph renderGraph, string passName, int shaderPass, MaterialPropertyBlock properties, int sourceId, TextureHandle source,
            TextureHandle target)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DistanceFieldPassData>(passName, out var passData))
            {
                passData.material = distanceFieldMaterial;
                passData.properties = properties;
                passData.source = source;
                passData.sourceId = sourceId;
                passData.shaderPass = shaderPass;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(target, 0, AccessFlags.Write);

                builder.SetRenderFunc(static (DistanceFieldPassData data, RasterGraphContext context) =>
                {
                    data.properties.SetTexture(data.sourceId, data.source);

                    context.cmd.DrawProcedural(Matrix4x4.identity, data.material, data.shaderPass, MeshTopology.Triangles, 3, 1, data.properties);
                });
            }
        }

        // Returns a property block for one round of the flood, creating it on first use.
        // Each round needs its own, because a block is read when the pass executes rather than when it is recorded.
        private MaterialPropertyBlock GetFloodProperties(int round)
        {
            m_FloodProperties ??= new Dictionary<int, MaterialPropertyBlock>();

            if (!m_FloodProperties.TryGetValue(round, out var properties))
            {
                properties = new MaterialPropertyBlock();
                m_FloodProperties.Add(round, properties);
            }

            return properties;
        }

        // Runs the cascade chain from the coarsest level down to the finest and returns the finest, which holds the light arriving
        // at every point. Both buffers are the same size at every level; what changes is how that size is divided between probes
        // and directions. Working downward lets each level lay itself over the one already finished above it in the same pass.
        private TextureHandle RecordCascades(RenderGraph renderGraph, TextureHandle color, TextureHandle distanceField, Vector2Int resolution)
        {
            var levels = Mathf.Clamp(cascadeCount, 1, MaxCascadeCount);

            // The finest level puts one probe on every texel, and those texels are the pixels the rays travel through, so the spacing
            // is one. Its ray reaches one spacing, and every level above doubles the first and quadruples the second.
            var probeSpacing = 1f;

            // The reference's own setup: the diagonal of the square of the probe spacing, halved, then rounded up to a multiple of two.
            var baseInterval = Mathf.Ceil(new Vector2(probeSpacing, probeSpacing).magnitude * 0.5f / 2f) * 2f;
            var screen = new Vector2(resolution.x, resolution.y);

            var desc = new TextureDesc(resolution.x, resolution.y)
            {
                name = CascadeTextureName,
                format = GraphicsFormat.R16G16B16A16_SFloat,
                clearBuffer = false,

                // The smoothing is what blends the four coarse probes around each finer probe together during the merge.
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var target = renderGraph.CreateTexture(desc);

            desc.name = CascadeAlternateTextureName;

            var parent = renderGraph.CreateTexture(desc);

            for (var level = levels - 1; level >= 0; --level)
            {
                var properties = GetCascadeProperties(level);

                properties.SetVector(RenderExtentId, new Vector4(screen.x, screen.y, 0f, 0f));
                properties.SetVector(CascadeExtentId, new Vector4(resolution.x, resolution.y, 0f, 0f));
                properties.SetFloat(CascadeIndexId, level);
                properties.SetFloat(CascadeCountId, levels);
                properties.SetFloat(CascadeLinearId, probeSpacing);
                properties.SetFloat(CascadeIntervalId, baseInterval);
                properties.SetFloat(StepStrideId, Mathf.Max(MinStepStride, stepStride));

                var isTopLevel = level == levels - 1;

                using (var builder = renderGraph.AddRasterRenderPass<CascadePassData>(CascadePassName, out var passData))
                {
                    passData.material = cascadeMaterial;
                    passData.properties = properties;
                    passData.color = color;
                    passData.distanceField = distanceField;
                    passData.parent = parent;
                    passData.hasParent = !isTopLevel;

                    builder.UseTexture(color, AccessFlags.Read);
                    builder.UseTexture(distanceField, AccessFlags.Read);

                    // The top level has nothing above it, and its buffer holds whatever the graph last left there.
                    if (!isTopLevel)
                        builder.UseTexture(parent, AccessFlags.Read);

                    builder.SetRenderAttachment(target, 0, AccessFlags.Write);

                    builder.SetRenderFunc(static (CascadePassData data, RasterGraphContext context) =>
                    {
                        data.properties.SetTexture(ColorTexId, data.color);
                        data.properties.SetTexture(DistanceTexId, data.distanceField);

                        if (data.hasParent)
                            data.properties.SetTexture(ParentTexId, data.parent);

                        context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1, data.properties);
                    });
                }

                (target, parent) = (parent, target);
            }

            // The swap after the last level leaves the finest cascade in the buffer the loop was reading from.
            return parent;
        }

        // Returns a property block for one cascade level, creating it on first use.
        // Each level needs its own, because a block is read when the pass executes rather than when it is recorded.
        private MaterialPropertyBlock GetCascadeProperties(int level)
        {
            m_CascadeProperties ??= new MaterialPropertyBlock[MaxCascadeCount];

            return m_CascadeProperties[level] ??= new MaterialPropertyBlock();
        }

        // Copies one of the frame's working buffers into a buffer that outlives it, so a measurement can read what the lighting did.
        private void RecordCapture(RenderGraph renderGraph, RTHandle capture, TextureHandle source)
        {
            if (capture == null || !source.IsValid())
                return;

            var target = renderGraph.ImportTexture(capture);

            using (var builder = renderGraph.AddRasterRenderPass<CapturePassData>(CapturePassName, out var passData))
            {
                passData.irradiance = source;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(target, 0, AccessFlags.Write);

                builder.SetRenderFunc(static (CapturePassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.irradiance, FullTarget, 0f, false);
                });
            }
        }

        // Draws the gathered light, exposed and tonemapped, into the camera color as the background.
        // A debug view replaces that with one of the buffers the lighting was built from, which needs that buffer bound as well.
        private void RecordBackground(RenderGraph renderGraph, TextureHandle colorTarget, TextureHandle irradiance, TextureHandle color, Vector2 bufferScale, float aspectRatio)
        {
            var hasIrradiance = irradiance.IsValid();
            var showsWorkingBuffers = debugView != DebugView.Off && color.IsValid();

            m_BackgroundProperties.SetInteger(DebugViewId, showsWorkingBuffers ? (int)debugView : 0);
            // Effects that flood the arena with light hold the exposure down for as long as they run, so one of them cannot wash the
            // whole screen out.
            m_BackgroundProperties.SetFloat(ExposureId, exposure + RadianceCascades2DGIFeature.exposureOffset);
            m_BackgroundProperties.SetColor(BaseColorId, settings.baseColor);
            m_BackgroundProperties.SetColor(LineColorId, settings.lineColor);
            GetGridDrift(out var gridRotation, out var gridCellsAcross);

            m_BackgroundProperties.SetFloat(CellsAcrossId, gridCellsAcross);
            m_BackgroundProperties.SetFloat(LineThicknessId, settings.lineThickness);
            m_BackgroundProperties.SetVector(OffsetId, settings.offset);
            m_BackgroundProperties.SetVector(AspectScaleId, new Vector4(aspectRatio, 1f, 0f, 0f));
            m_BackgroundProperties.SetMatrix(GridRotationId, gridRotation);
            m_BackgroundProperties.SetFloat(GridPerspectiveId, settings.perspective);
            m_BackgroundProperties.SetVector(BufferScaleId, bufferScale);

            using (var builder = renderGraph.AddRasterRenderPass<BackgroundPassData>(BackgroundPassName, out var passData))
            {
                passData.material = backgroundMaterial;
                passData.properties = m_BackgroundProperties;
                passData.irradiance = irradiance;
                passData.hasIrradiance = hasIrradiance;
                passData.color = color;
                passData.showsWorkingBuffers = showsWorkingBuffers;

                if (hasIrradiance)
                    builder.UseTexture(irradiance, AccessFlags.Read);

                if (showsWorkingBuffers)
                    builder.UseTexture(color, AccessFlags.Read);

                builder.SetRenderAttachment(colorTarget, 0, AccessFlags.Write);

                builder.SetRenderFunc(static (BackgroundPassData data, RasterGraphContext context) =>
                {
                    // Black rather than nothing when the lighting did not run, since an unbound texture reads white and would draw
                    // the whole screen white instead of the unlit black it should be.
                    data.properties.SetTexture(IrradianceTexId, data.hasIrradiance ? data.irradiance : Texture2D.blackTexture);

                    if (data.showsWorkingBuffers)
                        data.properties.SetTexture(ColorTexId, data.color);
                    else
                        data.properties.SetTexture(ColorTexId, Texture2D.blackTexture);

                    context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1, data.properties);
                });
            }
        }

        // Returns which way the grid's plane is leaning right now and how many cells it is divided into.
        //
        // Time is cut into equal stretches, each with its own lean and cell count picked at random within their ranges, and the grid
        // eases from one stretch's to the next. Both come from the same stretch, so the grid settles as one rather than the lean and
        // the count moving against each other. Deriving them from the stretch's number rather than remembering the last one keeps the
        // movement identical however the frames fall, so it neither drifts over a long session nor jumps when the editor pauses.
        private void GetGridDrift(out Matrix4x4 rotation, out float cellsAcross)
        {
            if (settings.tiltInterval <= 0f)
            {
                rotation = Matrix4x4.identity;
                cellsAcross = settings.cellsAcross;

                return;
            }

            var position = Time.time / settings.tiltInterval;
            var stretch = Mathf.FloorToInt(position);
            var through = position - stretch;

            // Eased at both ends, so the grid leaves one arrangement and arrives at the next without a visible change of direction.
            var blend = through * through * (3f - 2f * through);

            rotation = Matrix4x4.Rotate(Quaternion.Euler(Vector3.Lerp(GetTiltAngles(stretch), GetTiltAngles(stretch + 1), blend)));

            // Never fewer than a couple of cells, since a count at or below zero turns the grid inside out.
            var drift = Mathf.Lerp(Spread(stretch * 4 + 3), Spread((stretch + 1) * 4 + 3), blend);

            cellsAcross = Mathf.Max(2f, settings.cellsAcross + settings.cellsVariation * drift);
        }

        // Returns the angle the grid's plane leans to for one stretch of time, the same answer every time for a given stretch.
        private Vector3 GetTiltAngles(int stretch)
        {
            return new Vector3(
                settings.tiltAmplitude.x * Spread(stretch * 4),
                settings.tiltAmplitude.y * Spread(stretch * 4 + 1),
                settings.tiltAmplitude.z * Spread(stretch * 4 + 2));
        }

        // Turns a whole number into one between minus one and one, spread evenly, so consecutive numbers give unrelated answers.
        private static float Spread(int value)
        {
            value = (value << 13) ^ value;

            var scrambled = (value * (value * value * 15731 + 789221) + 1376312589) & 0x7fffffff;

            return 1f - scrambled / 1073741824f;
        }

        // Returns the size both cascade buffers are allocated at, rounded up so every level divides it into whole blocks.
        internal static Vector2Int GetCascadeResolution(RenderTextureDescriptor targetDescriptor, int cascadeCount, float texelsPerPixel)
        {
            // Rounded up rather than down, and never below the camera's image at the given scale. Everything the lighting works on
            // is this size, so a smaller one would mean shrinking that image to reach it, and a fractional shrink undersamples: the
            // scene picks up a repeating pattern of dropped columns, and every ray then reads it.
            var granularity = 1 << Mathf.Clamp(cascadeCount, 1, MaxCascadeCount);

            // Square, because a probe's four rays are spaced by angle and a buffer whose two axes differ gives those rays different
            // reach across than up. The camera then occupies the corner of it.
            var longest = Mathf.Max(targetDescriptor.width, targetDescriptor.height) * Mathf.Clamp(texelsPerPixel, MinRenderScale, 1f);
            var extent = Mathf.CeilToInt(longest / granularity) * granularity;

            extent = Mathf.Max(granularity, extent);

            return new Vector2Int(extent, extent);
        }

        // One of the lighting's working buffers, drawn full screen in place of the lit surface.
        // Each stage can then be judged on its own, rather than only by the final image, where a fault anywhere upstream looks the same.
        public enum DebugView
        {
            // Draws the lit surface normally.
            Off,

            // Where the lighting thinks a surface is. This has to match the sprites on screen exactly, and nothing else can be white.
            Coverage,

            // The light gathered at every point, as the cascades produced it, before the exposure and the tonemap are applied.
            GatheredLight
        }

        // What one round of the distance field build needs at execute time.
        private class DistanceFieldPassData
        {
            public Material material;
            public MaterialPropertyBlock properties;
            public TextureHandle source;
            public int sourceId;
            public int shaderPass;
        }

        // What one cascade level needs at execute time.
        private class CascadePassData
        {
            public Material material;
            public MaterialPropertyBlock properties;
            public TextureHandle color;
            public TextureHandle distanceField;
            public TextureHandle parent;
            public bool hasParent;
        }

        // What the draw of the sprites needs at execute time.
        private class SceneCapturePassData
        {
            public Material material;
            public MaterialPropertyBlock properties;
            public List<SpriteRenderSystem.Batch> batches;
            public List<Mesh> meshes;
            public Matrix4x4[] transforms;
            public Vector4[] colors;
            public Matrix4x4 viewProjection;
        }

        // What the measurement copy needs at execute time.
        private class CapturePassData
        {
            public TextureHandle irradiance;
        }

        // What the background draw needs at execute time.
        private class BackgroundPassData
        {
            public Material material;
            public MaterialPropertyBlock properties;
            public TextureHandle irradiance;
            public TextureHandle color;
            public bool hasIrradiance;
            public bool showsWorkingBuffers;
        }

        #region Internal

        // The most levels the chain can run. Every level quadruples the reach of the one below, so six already covers any screen.
        internal const int MaxCascadeCount = 6;

        // The smallest fraction of the screen the lighting's buffers can be. Below this the finest probes sit further apart than
        // the sprites are wide, and small emitters fall between them.
        internal const float MinRenderScale = 0.25f;

        // The shortest stride a ray can take, in texels. Below this a ray reads the same texel many times over for nothing.
        private const float MinStepStride = 0.05f;

        // How many sprites one instanced draw carries. This is the size of the per-instance array the shader declares, so a larger
        // run has to be split rather than submitted whole.
        private const int MaxInstancesPerCaptureDraw = 500;

        private const string SpriteMeshNamePrefix = "RC2DGI Sprite ";

        private const int SeedPass = 0;
        private const int FloodPass = 1;
        private const int DistancePass = 2;

        private const string SeedPassName = "RC2DGI Distance Field Seed";
        private const string FloodPassName = "RC2DGI Distance Field Flood";
        private const string DistancePassName = "RC2DGI Distance Field";
        private const string SceneCapturePassName = "RC2DGI Scene Capture";
        private const string CascadePassName = "RC2DGI Cascade";
        private const string CapturePassName = "RC2DGI Irradiance Capture";
        private const string BackgroundPassName = "RC2DGI Background";

        private const string SpriteTextureName = "_RC2DGISprites";
        private const string NearestSurfaceTextureName = "_RC2DGINearestSurface";
        private const string NearestSurfaceAlternateTextureName = "_RC2DGINearestSurfaceAlternate";
        private const string DistanceFieldTextureName = "_RC2DGIDistanceField";
        private const string CascadeTextureName = "_RC2DGICascade";
        private const string CascadeAlternateTextureName = "_RC2DGICascadeAlternate";

        private static readonly int SpriteTexId = Shader.PropertyToID("_SpriteTex");
        private static readonly int InstanceColorId = Shader.PropertyToID("_InstanceColor");
        private static readonly int CaptureViewProjectionId = Shader.PropertyToID("_CaptureViewProjection");
        private static readonly int ColorTexId = Shader.PropertyToID("_ColorTex");
        private static readonly int SourceTexId = Shader.PropertyToID("_SourceTex");
        private static readonly int DistanceTexId = Shader.PropertyToID("_DistanceTex");
        private static readonly int JumpDistanceId = Shader.PropertyToID("_JumpDistance");
        private static readonly int BufferScaleId = Shader.PropertyToID("_BufferScale");
        private static readonly int ParentTexId = Shader.PropertyToID("_ParentTex");
        private static readonly int IrradianceTexId = Shader.PropertyToID("_IrradianceTex");
        private static readonly int RenderExtentId = Shader.PropertyToID("_RenderExtent");
        private static readonly int CascadeExtentId = Shader.PropertyToID("_CascadeExtent");
        private static readonly int CascadeIndexId = Shader.PropertyToID("_CascadeIndex");
        private static readonly int CascadeLinearId = Shader.PropertyToID("_CascadeLinear");
        private static readonly int CascadeIntervalId = Shader.PropertyToID("_CascadeInterval");
        private static readonly int CascadeCountId = Shader.PropertyToID("_CascadeCount");
        private static readonly int StepStrideId = Shader.PropertyToID("_StepStride");
        private static readonly int ExposureId = Shader.PropertyToID("_Exposure");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LineColorId = Shader.PropertyToID("_LineColor");
        private static readonly int CellsAcrossId = Shader.PropertyToID("_CellsAcross");
        private static readonly int LineThicknessId = Shader.PropertyToID("_LineThickness");
        private static readonly int OffsetId = Shader.PropertyToID("_Offset");
        private static readonly int AspectScaleId = Shader.PropertyToID("_AspectScale");
        private static readonly int GridRotationId = Shader.PropertyToID("_GridRotation");
        private static readonly int GridPerspectiveId = Shader.PropertyToID("_GridPerspective");
        private static readonly int DebugViewId = Shader.PropertyToID("_DebugView");

        // The whole source mapped onto the whole destination, with no scale or offset.
        private static readonly Vector4 FullTarget = new(1f, 1f, 0f, 0f);

        private readonly MaterialPropertyBlock m_SeedProperties = new();
        private readonly MaterialPropertyBlock m_DistanceProperties = new();
        private readonly MaterialPropertyBlock m_BackgroundProperties = new();
        private readonly MaterialPropertyBlock m_CaptureProperties = new();
        private Dictionary<int, MaterialPropertyBlock> m_FloodProperties;
        private MaterialPropertyBlock[] m_CascadeProperties;
        private List<int> m_Jumps;

        // What the sprites are drawn from: the frame's batches, a mesh for each of their sprites, and the scratch arrays one
        // instanced draw is handed.
        private List<SpriteRenderSystem.Batch> m_Batches;
        private List<Mesh> m_BatchMeshes;
        private Dictionary<Sprite, Mesh> m_SpriteMeshes;
        private Matrix4x4[] m_CaptureTransforms;
        private Vector4[] m_CaptureColors;

        #endregion
    }
}
