using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public sealed class SpriteDestructionBatch
{
    public struct BatchVertex
    {
        public Vector3 position;
        public Vector2 normals;
        public Vector2 uv;
    }

    private struct DrawItem
    {
        public PhysicsBody physicsBody;
        public EntityId meshId;
    }

    private NativeHashMap<PhysicsBody, DrawItem> m_DrawItems;
    private bool m_UsingBIRP;
    
    private Material m_Material;
    
    public void Create(Material material)
    {
        // Set the material.
        m_Material = material;
        
        // Create the draw items.
        m_DrawItems = new NativeHashMap<PhysicsBody, DrawItem>(100, Allocator.Persistent);
        
        // Flag if using the built-in render pipeline or not.
        m_UsingBIRP = GraphicsSettings.currentRenderPipeline == null;
        
        // Register render callback.
        if (m_UsingBIRP)
            Camera.onPostRender += BIRP_RenderAllWorlds;
        else
            RenderPipelineManager.endCameraRendering += SRP_RenderAllWorlds;
    }

    public void Destroy()
    {
        // Un-register render callback.
        if (m_UsingBIRP)
            Camera.onPostRender -= BIRP_RenderAllWorlds;
        else
            RenderPipelineManager.endCameraRendering -= SRP_RenderAllWorlds;

        // Destroy draw items.
        if (m_DrawItems.IsCreated)
        {
            foreach (var drawItem in m_DrawItems)
            {
                var mesh = Resources.EntityIdToObject(drawItem.Value.meshId) as Mesh;
                if (mesh != null)
                    Object.Destroy(mesh);
            }
            
            m_DrawItems.Dispose();
        }
    }

    public void Reset()
    {
        // Finish if no draw items.
        if (!m_DrawItems.IsCreated)
            return;
        
        foreach (var drawItemPair in m_DrawItems)
        {
            var drawItem = drawItemPair.Value;
            
            var mesh = Resources.EntityIdToObject(drawItem.meshId) as Mesh;
            if (mesh != null)
                Object.Destroy(mesh);
            
            // Destroy the body.
            drawItem.physicsBody.Destroy();            
        }

        m_DrawItems.Clear();
    }
    
    public void CreateSpriteDrawItem(
        NativeArray<VertexAttributeDescriptor> vertexAttributes,
        NativeArray<BatchVertex> vertices,
        NativeArray<int> indices,
        PhysicsBody physicsBody)
    {
        // Create default mesh.
        var mesh = new Mesh();
        
        mesh.SetVertexBufferParams(vertices.Length, vertexAttributes);
        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
        mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
        mesh.SetIndexBufferData(indices, 0, 0, indices.Length);
        
        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length, MeshTopology.Triangles));
        
        // Add draw item.
        m_DrawItems.Add(physicsBody, new DrawItem
        {
            physicsBody = physicsBody,
            meshId = mesh.GetEntityId()
        });
    }

    public void DestroySpriteDrawItem(PhysicsBody physicsBody)
    {
        if (!m_DrawItems.TryGetValue(physicsBody, out var drawItem))
            throw new ArgumentException("Could not find draw item.", nameof(physicsBody));

        // Destroy the mesh.
        var mesh = Resources.EntityIdToObject(drawItem.meshId) as Mesh;
        if (mesh != null)
            Object.Destroy(mesh);
        
        // Remove the draw item.
        m_DrawItems.Remove(physicsBody);
        
        // Destroy the body.
        physicsBody.Destroy();
    }
    
    private void BIRP_RenderAllWorlds(Camera camera)
    {
        // Ensure the camera type is valid.
        if (!IsCameraTypeValid(camera))
            return;

        // Draw meshes.
        DrawMeshes(camera);
    }

    private void SRP_RenderAllWorlds(ScriptableRenderContext context, Camera camera)
    {
        // Ensure the camera type is valid.
        if (!IsCameraTypeValid(camera))
            return;

        // Draw meshes.
        DrawMeshes(camera);
    }

    private void DrawMeshes(Camera camera)
    {
        m_Material.SetPass(0);
        
        foreach (var drawItemPair in m_DrawItems)
        {
            var drawItem = drawItemPair.Value;
            
            var mesh = Resources.EntityIdToObject(drawItem.meshId) as Mesh;
            if (mesh != null)
            {
                var body = drawItem.physicsBody;
                var world = body.world;
                var transformPlane = world.transformPlane;
                
                var bodyTransform = body.transform;
                
                var position = PhysicsMath.ToPosition3D(bodyTransform.position, Vector3.zero, transformPlane);
                var rotation = PhysicsMath.ToRotationFast3D(body.rotation.radians, transformPlane);

                Graphics.DrawMeshNow(mesh, position, rotation);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsCameraTypeValid(Camera camera)
    {
        var cameraType = camera.cameraType;
        return cameraType == CameraType.Game || cameraType == CameraType.SceneView;
    }
}
