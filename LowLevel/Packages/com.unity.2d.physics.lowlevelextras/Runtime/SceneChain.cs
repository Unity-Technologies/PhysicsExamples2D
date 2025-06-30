using System.Linq;
using UnityEngine.LowLevelPhysics2D;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(PhysicsLowLevelExtrasExecutionOrder.SceneShape)]
    [AddComponentMenu("Physics 2D (LowLevel)/Scene Chain", 1)]
    [Icon(IconUtility.IconPath + "SceneChain.png")]
    public sealed class SceneChain : MonoBehaviour
    {
        public Vector2[] Points = { Vector2.left + Vector2.down, Vector2.right + Vector2.down, Vector2.right + Vector2.up, Vector2.left + Vector2.up };
        public bool ReverseChain;
        public PhysicsChainDefinition ChainDefinition = new() { isLoop = true };
        public SceneBody SceneBody;

        public PhysicsChain ChainShape => m_ChainShape;
        private PhysicsChain m_ChainShape;
        private int m_OwnerKey;

        public void UpdateShape(Vector2[] points)
        {
            Points = points;
            CreateShape();
        }
        
        private void Reset()
        {
            if (SceneBody == null)
                SceneBody = SceneBody.FindSceneBody(gameObject);
        }
       
        private void OnEnable()
        {
            Reset();
        
            if (SceneBody != null)
            {
                SceneBody.CreateBodyEvent += OnCreateBody;
                SceneBody.DestroyBodyEvent += OnDestroyBody;
            }
            
            CreateShape();
        }

        private void OnDisable()
        {
            DestroyShape();
            
            if (SceneBody != null)
            {
                SceneBody.CreateBodyEvent -= OnCreateBody;
                SceneBody.DestroyBodyEvent -= OnDestroyBody;
            }
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            DestroyShape();
            CreateShape();
        }

        private void Update()
        {
            if (Application.isPlaying ||
                !transform.hasChanged ||
                !m_ChainShape.isValid)
                return;
            
            // Reset transform changed flag.
            transform.hasChanged = false;

            // Is the body the same transform or deeper in the hierarchy?
            if (SceneBody.transform == transform.root || SceneBody.transform.IsChildOf(transform))
            {
                // Fetch the body.
                var body = SceneBody.Body;
                
                // Fetch the transform plane.
                var transformPlane = body.world.transformPlane;
                
                // Update the body.
                body.transform = new PhysicsTransform(
                    PhysicsMath.TransformPosition2D(transform.position, transformPlane),
                    new PhysicsRotate(PhysicsMath.TransformRotation2D(transform.rotation, transformPlane)));            
            }
            
            // Create the shape.
            CreateShape();
        }
        
        private void CreateShape()
        {
            // Destroy any existing shape.
            DestroyShape();
            
            if (!SceneBody)
                return;

            var body = SceneBody.Body;
            if (!body.isValid)
                return;

            // Create the chain geometry.
            var chainGeometry = new ChainGeometry(ReverseChain ? Points.Reverse().ToArray() : Points);
            
            // Create the chain shape.
            m_ChainShape = body.CreateChain(chainGeometry, ChainDefinition);

            if (m_ChainShape.isValid)
                m_OwnerKey = m_ChainShape.SetOwner(this);
        }

        private void DestroyShape()
        {
            if (m_ChainShape.isValid)
            {
                m_ChainShape.Destroy(m_OwnerKey);
                m_ChainShape = default;
                m_OwnerKey = 0;
            }
        }
        
        private void OnCreateBody(SceneBody sceneBody)
        {
            CreateShape();
        }

        private void OnDestroyBody(SceneBody sceneBody)
        {
            DestroyShape();
        }        
        
        public override string ToString() => m_ChainShape.ToString();
    }
}