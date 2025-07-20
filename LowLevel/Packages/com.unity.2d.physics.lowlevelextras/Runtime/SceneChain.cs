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
        public PhysicsChainDefinition ChainDefinition = PhysicsChainDefinition.defaultDefinition;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;
        public SceneBody SceneBody;

        public PhysicsChain ChainShape => m_ChainShape;
        private PhysicsChain m_ChainShape;
        private int m_OwnerKey;

        private bool m_TransformChangeReset;
        
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
            
            m_TransformChangeReset = false;
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
            
            // Flag as transform needing reset.
            m_TransformChangeReset = true;

            // Create the shape.
            CreateShape();
        }
        
        private void LateUpdate()
        {
            if (!m_TransformChangeReset)
                return;

            transform.hasChanged = m_TransformChangeReset = false;
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
            {
                // Set the user data.
                m_ChainShape.userData = UserData;
             
                // Set the callback target.
                m_ChainShape.callbackTarget = CallbackTarget;
                
                // Set the owner.
                m_OwnerKey = m_ChainShape.SetOwner(this);
            }
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