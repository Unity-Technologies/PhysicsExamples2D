using UnityEngine;
using Unity.U2D.Physics;

namespace Unity.U2D.Physics.Extras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.SceneJoint)]
    public abstract class SceneJointBase : MonoBehaviour
    {
        public SceneBody BodyA;
        public SceneBody BodyB;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;

        protected int m_OwnerKey;

        private void Reset()
        {
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
            if (!isActiveAndEnabled)
                return;

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