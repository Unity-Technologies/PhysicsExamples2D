using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.TestJoint)]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace)]
    public abstract class TestJointBase : MonoBehaviour
    {
        public TestBody BodyA;
        public TestBody BodyB;
        public bool AutoAnchorA;
        public bool AutoAnchorB;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;

        protected int m_OwnerKey;

        protected struct AutoAnchorTransform
        {
            public PhysicsTransform bodyTransformA;
            public PhysicsTransform bodyTransformB;
            public PhysicsTransform localAnchorA;
            public PhysicsTransform localAnchorB;
        }

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

        protected void CalculateAutoAnchors(ref AutoAnchorTransform autoAnchorDefinition)
        {
            // Calculate auto anchors.
            if (AutoAnchorA)
                autoAnchorDefinition.localAnchorA = autoAnchorDefinition.bodyTransformA.InverseMultiplyTransform(autoAnchorDefinition.bodyTransformB).MultiplyTransform(autoAnchorDefinition.localAnchorA);
            if (AutoAnchorB)
                autoAnchorDefinition.localAnchorB = autoAnchorDefinition.bodyTransformB.InverseMultiplyTransform(autoAnchorDefinition.bodyTransformA).MultiplyTransform(autoAnchorDefinition.localAnchorB);
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