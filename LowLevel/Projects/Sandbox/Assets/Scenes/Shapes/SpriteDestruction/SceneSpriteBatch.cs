using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Rendering;
using UnityEngine.U2D.Physics.LowLevelExtras;

public sealed class SceneSpriteBatch : MonoBehaviour
{
    public Sprite Sprite;
    public Material Material;
    
    public struct DrawItem
    {
        public EntityId meshId;
        public PhysicsBody body;
    }
    
    public struct DrawItemVertex
    {
        public Vector3 position;
        public Vector2 normals;
        public Vector2 uv;
    }

    private NativeList<DrawItem> m_DrawItems;

    private void OnEnable()
    {
        // Create the draw items.
        m_DrawItems = new NativeList<DrawItem>(Allocator.Persistent);
        
        // Flag if using the built-in render pipeline or not.
        m_UsingBIRP = GraphicsSettings.currentRenderPipeline == null;
        
        // Register render callback.
        if (m_UsingBIRP)
            Camera.onPostRender += BIRP_RenderAllWorlds;
        else
            RenderPipelineManager.endCameraRendering += SRP_RenderAllWorlds;

        if (Sprite == null || Material == null)
        {
            Debug.LogWarning("No Sprite or Material assigned.");
            return;
        }

        if (Sprite.packed)
        {
            Debug.LogWarning("Cannot use a Sprite packed in an Atlas.");
            return;
        }

        var localScale = transform.lossyScale;
        var spriteExtents = Sprite.bounds.extents;// * new Vector2(localScale.x, localScale.y);
        //var spriteRect = Sprite.textureRect;
        //var ppu = 1f / Sprite.pixelsPerUnit;
        localScale.Scale(spriteExtents);
        
        // Create default mesh.
        var mesh = new Mesh();
        var vertexAttributes = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            
        };
        var vertexData = new NativeArray<DrawItemVertex>(4, Allocator.Temp);
        vertexData[0] = new DrawItemVertex { position = new Vector3(-0.5f * localScale.x, -0.5f * localScale.y, 0f), uv = new Vector2(0f, 0f), normals = -Vector3.forward};
        vertexData[1] = new DrawItemVertex { position = new Vector3(0.5f * localScale.x, -0.5f * localScale.y, 0f), uv = new Vector2(1f, 0f), normals = -Vector3.forward };
        vertexData[2] = new DrawItemVertex { position = new Vector3(0.5f * localScale.x, 0.5f * localScale.y, 0f), uv = new Vector2(1f, 1f), normals = -Vector3.forward };
        vertexData[3] = new DrawItemVertex { position = new Vector3(-0.5f * localScale.x, 0.5f * localScale.y, 0f), uv = new Vector2(0f, 1f), normals = -Vector3.forward };
        mesh.SetVertexBufferParams(vertexData.Length, vertexAttributes);
        mesh.SetVertexBufferData(vertexData, 0, 0, vertexData.Length);
        vertexData.Dispose();
        
        var indexData = new NativeArray<ushort>(6, Allocator.Temp);
        indexData[0] = 0;
        indexData[1] = 3;
        indexData[2] = 1;
        indexData[3] = 1;
        indexData[4] = 3;
        indexData[5] = 2;
        mesh.SetIndexBufferParams(indexData.Length, IndexFormat.UInt16);
        mesh.SetIndexBufferData(indexData, 0, 0, indexData.Length);
        indexData.Dispose();

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, 6, MeshTopology.Triangles));
        
        // Add draw item.
        m_DrawItems.Add(new DrawItem
        {
            meshId = mesh.GetEntityId(),
            body = GetComponent<SceneBody>().Body
        });
    }

    private void OnDisable()
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
                var mesh = Resources.EntityIdToObject(drawItem.meshId) as Mesh;
                if (mesh != null)
                    Destroy(mesh);
            }
            
            m_DrawItems.Dispose();
        }
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
        Material.SetPass(0);
        
        foreach (var drawItem in m_DrawItems)
        {
            var mesh = Resources.EntityIdToObject(drawItem.meshId) as Mesh;
            if (mesh != null)
            {
                var body = drawItem.body;
                var world = body.world;
                var transformPlane = world.transformPlane;
                
                var bodyTransform = body.transform;
                
                var position = PhysicsMath.ToPosition3D(bodyTransform.position, Vector3.zero, transformPlane);
                var rotation = PhysicsMath.ToRotationFast3D(body.rotation.angle, transformPlane);

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
    
    private bool m_UsingBIRP;
}
