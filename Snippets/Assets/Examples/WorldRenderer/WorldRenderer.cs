using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

using Unity.U2D.Physics;

public class WorldRenderer : MonoBehaviour
{
    public Material CircleMaterial;
    public Material CapsuleMaterial;
    public Material PolygonMaterial;
    public Material LineMaterial;
    public Material PointMaterial;

    private DrawerGroup m_DrawerGroup = null;
    private static Mesh m_RenderMesh;

    private static int ElementBufferShaderProperty = Shader.PropertyToID("element_buffer");
    private static int TransformPlaneShaderProperty = Shader.PropertyToID("transform_plane");
    private static int TransformPlaneMatrixShaderProperty = Shader.PropertyToID("transform_plane_matrix");
    private static int ThicknessShaderProperty = Shader.PropertyToID("thickness");
    private static int FillAlphaShaderProperty = Shader.PropertyToID("fillAlpha");
    
    private const string RenderCommandBufferName = "Snippets.WorldRenderer";

    private void OnEnable()
    {
        // Register draw results.
        PhysicsEvents.WorldDrawResults += Render;
    }

    private void OnDisable()
    {
        // Unregister from draw results.
        PhysicsEvents.WorldDrawResults -= Render;
        
        // Dispose of the drawer groups.
        m_DrawerGroup?.Dispose();
        m_DrawerGroup = null;

        if (m_RenderMesh != null)
            Destroy(m_RenderMesh);
    }

    private void Render(PhysicsWorld physicsWorld, ref PhysicsWorld.DrawResults drawResults)
    {
        // Only render the default world.
        if (physicsWorld != PhysicsWorld.defaultWorld)
            return;
        
        // Early-out if no draw results.
        if (!drawResults.isValid)
            return;
        
        Profiler.BeginSample(RenderCommandBufferName);

        // Create the mesh.
        if (m_RenderMesh == null)
        {
            m_RenderMesh = new Mesh
            {
                vertices = new Vector3[]
                {
                    new(-1.1f, -1.1f, 0f),
                    new(-1.1f, 1.1f, 0f),
                    new(1.1f, 1.1f, 0f),
                    new(1.1f, -1.1f, 0f)
                },
                normals = new[] { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward },
                uv = new[] { Vector2.zero, new Vector2(0f, 1f), Vector2.one, new Vector2(1f, 0f) },
                triangles = new[] { 0, 1, 2, 2, 3, 0 }
            };
        }

        // Create the drawer groups.
        m_DrawerGroup ??= new DrawerGroup(this);
        
        // Draw the drawer group.
        m_DrawerGroup.Draw(physicsWorld, ref drawResults);

        Profiler.EndSample();
    }
    
    private sealed class DrawerGroup : IDisposable
    {
        private BaseDrawer[] m_Drawers;
        private CommandBuffer m_RendererCommandBuffer = new CommandBuffer { name = RenderCommandBufferName };
        
        public DrawerGroup(WorldRenderer renderer)
        {
            m_Drawers = new BaseDrawer[]
            {
                new PolygonGeometryDrawer(renderer.PolygonMaterial),
                new CircleGeometryDrawer(renderer.CircleMaterial),
                new CapsuleGeometryDrawer(renderer.CapsuleMaterial),
                new LineDrawer(renderer.LineMaterial),
                new PointDrawer(renderer.PointMaterial)
            };
        }            
            
        public void Draw(PhysicsWorld physicsWorld, ref PhysicsWorld.DrawResults drawResults)
        {
            // Fetch global properties.
            var thickness = physicsWorld.drawThickness;
            var fillAlpha = physicsWorld.drawFillAlpha;
            var transformPlane = physicsWorld.transformPlane;
            var transformPlaneCustomMatrix = physicsWorld.transformPlaneCustom.fromCustom;
            
            // Draw all the drawers.
            foreach (var drawer in m_Drawers)
                drawer.Draw(rendererCommandBuffer: m_RendererCommandBuffer, renderMesh: m_RenderMesh, drawResults: ref drawResults, thickness: thickness, fillAlpha: fillAlpha, transformPlane: transformPlane, transformPlaneCustomMatrix: ref transformPlaneCustomMatrix);
            
            // Render the final command buffer.
            Graphics.ExecuteCommandBuffer(m_RendererCommandBuffer);

            // Clear the render command buffer.
            m_RendererCommandBuffer.Clear();
        }
        
        private bool isValid => m_Drawers != null;
        
        public void Dispose()
        {
            if (!isValid)
                return;

            foreach(var drawer in m_Drawers)
            {
                drawer.Dispose();
            }
            m_Drawers = null;

            // Destroy the render mesh.
            if (m_RenderMesh != null)
            {
                UnityEngine.Object.DestroyImmediate(m_RenderMesh);
                m_RenderMesh = null;
            }
            
            // Dispose command buffer.
            if (m_RendererCommandBuffer != null)
            {
                m_RendererCommandBuffer.Dispose();
                m_RendererCommandBuffer = null;
            }            
        }
        
        private abstract class BaseDrawer : IDisposable
        {
            private bool m_Disposed;

            protected GraphicsBuffer m_GraphicsBuffer = new(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            protected GraphicsBuffer.IndirectDrawIndexedArgs[] m_CommandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
            protected ComputeBuffer m_ElementBuffer;
            protected Material m_ShaderMaterial;
            protected MaterialPropertyBlock m_ShaderMaterialPropertyBlock;

            public void Dispose()
            {
                // Finish if disposed.
                if (m_Disposed)
                    return;

                m_GraphicsBuffer?.Dispose();
                m_GraphicsBuffer = null;
                m_CommandData = null;

                m_ElementBuffer?.Dispose();
                m_ElementBuffer = null;

                m_ShaderMaterialPropertyBlock = null;
                m_ShaderMaterial = null;

                // Flag as disposed.
                m_Disposed = true;
            }

            public abstract void Draw(CommandBuffer rendererCommandBuffer, Mesh renderMesh, ref PhysicsWorld.DrawResults drawResults, float thickness, float fillAlpha, PhysicsWorld.TransformPlane transformPlane, ref Matrix4x4 transformPlaneCustomMatrix);
        }

        private sealed class PolygonGeometryDrawer : BaseDrawer
        {
            public PolygonGeometryDrawer(Material shaderMaterial)
            {
                m_ShaderMaterial = shaderMaterial;
                m_ShaderMaterialPropertyBlock = new MaterialPropertyBlock();
            }

            public override void Draw(CommandBuffer rendererCommandBuffer, Mesh renderMesh, ref PhysicsWorld.DrawResults drawResults, float thickness, float fillAlpha, PhysicsWorld.TransformPlane transformPlane, ref Matrix4x4 transformPlaneCustomMatrix)
            {
                var polygonGeometryElements = drawResults.polygonGeometryArray;
                var count = polygonGeometryElements.Length;
                if (count == 0)
                    return;

                Profiler.BeginSample("PhysicsCore2D.DrawWorlds.PolygonCommand");

                // Set-up command buffer.
                m_CommandData[0].indexCountPerInstance = renderMesh.GetIndexCount(0);
                m_CommandData[0].instanceCount = (uint)count;
                m_GraphicsBuffer.SetData(m_CommandData);

                // Set-up compute buffer.
                if (m_ElementBuffer == null)
                {
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.PolygonGeometryElement.Size());
                }
                else if (m_ElementBuffer.count < count)
                {
                    m_ElementBuffer.Release();
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.PolygonGeometryElement.Size());
                }
                m_ElementBuffer.SetData(polygonGeometryElements);

                // Set up the material property block.
                m_ShaderMaterialPropertyBlock.SetBuffer(ElementBufferShaderProperty, m_ElementBuffer);
                m_ShaderMaterialPropertyBlock.SetInteger(TransformPlaneShaderProperty, (int)transformPlane);
                m_ShaderMaterialPropertyBlock.SetMatrix(TransformPlaneMatrixShaderProperty, transformPlaneCustomMatrix);
                m_ShaderMaterialPropertyBlock.SetFloat(ThicknessShaderProperty, thickness);
                m_ShaderMaterialPropertyBlock.SetFloat(FillAlphaShaderProperty, fillAlpha);

                // Draw to the renderer command buffer.
                rendererCommandBuffer.DrawMeshInstancedIndirect(renderMesh, 0, m_ShaderMaterial, 0, m_GraphicsBuffer, 0, m_ShaderMaterialPropertyBlock);

                Profiler.EndSample();
            }
        }

        private sealed class CircleGeometryDrawer : BaseDrawer
        {
            /// <undoc/>
            public CircleGeometryDrawer(Material shaderMaterial)
            {
                m_ShaderMaterial = shaderMaterial;
                m_ShaderMaterialPropertyBlock = new MaterialPropertyBlock();
            }

            /// <undoc/>
            public override void Draw(CommandBuffer rendererCommandBuffer, Mesh renderMesh, ref PhysicsWorld.DrawResults drawResults, float thickness, float fillAlpha, PhysicsWorld.TransformPlane transformPlane, ref Matrix4x4 transformPlaneCustomMatrix)
            {
                var circleGeometryElements = drawResults.circleGeometryArray;
                var count = circleGeometryElements.Length;
                if (count == 0)
                    return;

                Profiler.BeginSample("PhysicsCore2D.DrawWorlds.CircleCommand");

                // Set-up command buffer.
                m_CommandData[0].indexCountPerInstance = renderMesh.GetIndexCount(0);
                m_CommandData[0].instanceCount = (uint)count;
                m_GraphicsBuffer.SetData(m_CommandData);

                // Set-up compute buffer.
                if (m_ElementBuffer == null)
                {
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.CircleGeometryElement.Size());
                }
                else if (m_ElementBuffer.count < count)
                {
                    m_ElementBuffer.Release();
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.CircleGeometryElement.Size());
                }
                m_ElementBuffer.SetData(circleGeometryElements);

                // Set up the material property block.
                m_ShaderMaterialPropertyBlock.SetBuffer(ElementBufferShaderProperty, m_ElementBuffer);
                m_ShaderMaterialPropertyBlock.SetInteger(TransformPlaneShaderProperty, (int)transformPlane);
                m_ShaderMaterialPropertyBlock.SetMatrix(TransformPlaneMatrixShaderProperty, transformPlaneCustomMatrix);
                m_ShaderMaterialPropertyBlock.SetFloat(ThicknessShaderProperty, thickness);
                m_ShaderMaterialPropertyBlock.SetFloat(FillAlphaShaderProperty, fillAlpha);

                // Draw to the renderer command buffer.
                rendererCommandBuffer.DrawMeshInstancedIndirect(renderMesh, 0, m_ShaderMaterial, 0, m_GraphicsBuffer, 0, m_ShaderMaterialPropertyBlock);

                Profiler.EndSample();
            }
        }

        /// <summary>
        /// Capsule Geometry Drawer.
        /// </summary>
        private sealed class CapsuleGeometryDrawer : BaseDrawer
        {
            public CapsuleGeometryDrawer(Material shaderMaterial)
            {
                m_ShaderMaterial = shaderMaterial;
                m_ShaderMaterialPropertyBlock = new MaterialPropertyBlock();
            }

            public override void Draw(CommandBuffer rendererCommandBuffer, Mesh renderMesh, ref PhysicsWorld.DrawResults drawResults, float thickness, float fillAlpha, PhysicsWorld.TransformPlane transformPlane, ref Matrix4x4 transformPlaneCustomMatrix)
            {
                var capsuleGeometryElements = drawResults.capsuleGeometryArray;
                var count = capsuleGeometryElements.Length;
                if (count == 0)
                    return;

                Profiler.BeginSample("PhysicsCore2D.DrawWorlds.CapsuleCommand");

                // Set-up command buffer.
                m_CommandData[0].indexCountPerInstance = renderMesh.GetIndexCount(0);
                m_CommandData[0].instanceCount = (uint)count;
                m_GraphicsBuffer.SetData(m_CommandData);

                // Set-up compute buffer.
                if (m_ElementBuffer == null)
                {
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.CapsuleGeometryElement.Size());
                }
                else if (m_ElementBuffer.count < count)
                {
                    m_ElementBuffer.Release();
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.CapsuleGeometryElement.Size());
                }
                m_ElementBuffer.SetData(capsuleGeometryElements);

                // Set up the material property block.
                m_ShaderMaterialPropertyBlock.SetBuffer(ElementBufferShaderProperty, m_ElementBuffer);
                m_ShaderMaterialPropertyBlock.SetInteger(TransformPlaneShaderProperty, (int)transformPlane);
                m_ShaderMaterialPropertyBlock.SetMatrix(TransformPlaneMatrixShaderProperty, transformPlaneCustomMatrix);
                m_ShaderMaterialPropertyBlock.SetFloat(ThicknessShaderProperty, thickness);
                m_ShaderMaterialPropertyBlock.SetFloat(FillAlphaShaderProperty, fillAlpha);

                // Draw to the renderer command buffer.
                rendererCommandBuffer.DrawMeshInstancedIndirect(renderMesh, 0, m_ShaderMaterial, 0, m_GraphicsBuffer, 0, m_ShaderMaterialPropertyBlock);

                Profiler.EndSample();
            }
        }

        /// <summary>
        /// Line Drawer.
        /// </summary>
        private sealed class LineDrawer : BaseDrawer
        {
            public LineDrawer(Material shaderMaterial)
            {
                m_ShaderMaterial = shaderMaterial;
                m_ShaderMaterialPropertyBlock = new MaterialPropertyBlock();
            }

            public override void Draw(CommandBuffer rendererCommandBuffer, Mesh renderMesh, ref PhysicsWorld.DrawResults drawResults, float thickness, float fillAlpha, PhysicsWorld.TransformPlane transformPlane, ref Matrix4x4 transformPlaneCustomMatrix)
            {
                var lineElements = drawResults.lineArray;
                var count = lineElements.Length;
                if (count == 0)
                    return;

                Profiler.BeginSample("PhysicsCore2D.DrawWorlds.LineCommand");

                // Set-up command buffer.
                m_CommandData[0].indexCountPerInstance = renderMesh.GetIndexCount(0);
                m_CommandData[0].instanceCount = (uint)count;
                m_GraphicsBuffer.SetData(m_CommandData);

                // Set-up compute buffer.
                if (m_ElementBuffer == null)
                {
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.LineElement.Size());
                }
                else if (m_ElementBuffer.count < count)
                {
                    m_ElementBuffer.Release();
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.LineElement.Size());
                }
                m_ElementBuffer.SetData(lineElements);

                // Set up the material property block.
                m_ShaderMaterialPropertyBlock.SetBuffer(ElementBufferShaderProperty, m_ElementBuffer);
                m_ShaderMaterialPropertyBlock.SetInteger(TransformPlaneShaderProperty, (int)transformPlane);
                m_ShaderMaterialPropertyBlock.SetMatrix(TransformPlaneMatrixShaderProperty, transformPlaneCustomMatrix);
                m_ShaderMaterialPropertyBlock.SetFloat(ThicknessShaderProperty, thickness);

                // Draw to the renderer command buffer.
                rendererCommandBuffer.DrawMeshInstancedIndirect(renderMesh, 0, m_ShaderMaterial, 0, m_GraphicsBuffer, 0, m_ShaderMaterialPropertyBlock);

                Profiler.EndSample();
            }
        }

        /// <summary>
        /// Point Drawer.
        /// </summary>
        private sealed class PointDrawer : BaseDrawer
        {
            public PointDrawer(Material shaderMaterial)
            {
                m_ShaderMaterial = shaderMaterial;
                m_ShaderMaterialPropertyBlock = new MaterialPropertyBlock();
            }

            public override void Draw(CommandBuffer rendererCommandBuffer, Mesh renderMesh, ref PhysicsWorld.DrawResults drawResults, float thickness, float fillAlpha, PhysicsWorld.TransformPlane transformPlane, ref Matrix4x4 transformPlaneCustomMatrix)
            {
                var pointElements = drawResults.pointArray;
                var count = pointElements.Length;
                if (count == 0)
                    return;

                Profiler.BeginSample("PhysicsCore2D.DrawWorlds.PointCommand");

                // Set-up command buffer.
                m_CommandData[0].indexCountPerInstance = renderMesh.GetIndexCount(0);
                m_CommandData[0].instanceCount = (uint)count;
                m_GraphicsBuffer.SetData(m_CommandData);

                // Set-up compute buffer.
                if (m_ElementBuffer == null)
                {
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.PointElement.Size());
                }
                else if (m_ElementBuffer.count < count)
                {
                    m_ElementBuffer.Release();
                    m_ElementBuffer = new ComputeBuffer(count, PhysicsWorld.DrawResults.PointElement.Size());
                }
                m_ElementBuffer.SetData(pointElements);

                // Set up the material property block.
                m_ShaderMaterialPropertyBlock.SetBuffer(ElementBufferShaderProperty, m_ElementBuffer);
                m_ShaderMaterialPropertyBlock.SetInteger(TransformPlaneShaderProperty, (int)transformPlane);
                m_ShaderMaterialPropertyBlock.SetMatrix(TransformPlaneMatrixShaderProperty, transformPlaneCustomMatrix);

                // Draw to the renderer command buffer.
                rendererCommandBuffer.DrawMeshInstancedIndirect(renderMesh, 0, m_ShaderMaterial, 0, m_GraphicsBuffer, 0, m_ShaderMaterialPropertyBlock);

                Profiler.EndSample();
            }
        }
    }
}
