using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.TestJoint)]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, "SceneJointBase")]
    public abstract class TestJointBase : MonoBehaviour
    {
        public TestBody BodyA;
        public TestBody BodyB;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;

        protected int m_OwnerKey;

        private void Reset()
        {
            if (BodyA == null)
                BodyA = TestBody.FindTestBody(gameObject);
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

        private void OnCreateBody(TestBody testBody)
        {
            CreateJoint();
        }

        private void OnDestroyBody(TestBody testBody)
        {
            DestroyJoint();
        }
    }
}