using UnityEngine.LowLevelPhysics2D;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(PhysicsLowLevelExtrasExecutionOrder.SceneJoint)]    
    public abstract class SceneJointBase : MonoBehaviour
    {
        public SceneWorld SceneWorld;
        public SceneBody BodyA;
        public SceneBody BodyB;
        
        protected int m_OwnerKey;
        
        private void Reset()
        {
            if (SceneWorld == null)
                SceneWorld = SceneWorld.FindSceneWorld(gameObject);

            if (BodyA == null)
                BodyA = SceneBody.FindSceneBody(gameObject);
        }
        
        private void OnEnable()
        {
            Reset();
        
            if (BodyA != null)
            {
                BodyA.CreateBodyEvent += OnCreateBody;
                BodyA.DestroyBodyEvent += OnDestroyBody;
            }
            if (BodyB != null)
            {
                BodyB.CreateBodyEvent += OnCreateBody;
                BodyB.DestroyBodyEvent += OnDestroyBody;
            }
            
            CreateJoint();
        }

        private void OnDisable()
        {
            DestroyJoint();
            
            if (BodyA != null)
            {
                BodyA.CreateBodyEvent -= OnCreateBody;
                BodyA.DestroyBodyEvent -= OnDestroyBody;
            }
            if (BodyB != null)
            {
                BodyB.CreateBodyEvent -= OnCreateBody;
                BodyB.DestroyBodyEvent -= OnDestroyBody;
            }
        }

        private void OnValidate()
        {
            DestroyJoint();
            CreateJoint();
        }

        protected abstract void CreateJoint();
        protected abstract void DestroyJoint();
        
        private void OnCreateBody(SceneBody sceneBody)
        {
            CreateJoint();
        }

        private void OnDestroyBody(SceneBody sceneBody)
        {
            DestroyJoint();
        }        

    }
}