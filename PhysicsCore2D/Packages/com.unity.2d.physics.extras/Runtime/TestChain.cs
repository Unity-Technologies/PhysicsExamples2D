using System.Linq;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.TestShape)]
    [AddComponentMenu("Physics 2D/LowLevel/Test Chain", 30)]
    [Icon(IconUtility.IconPath + "TestChain.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, sourceClassName: "SceneChain")]
    public sealed class TestChain : MonoBehaviour, ITestWorldTransformChanged
    {
        public Vector2[] Points = { Vector2.left + Vector2.down, Vector2.right + Vector2.down, Vector2.right + Vector2.up, Vector2.left + Vector2.up };
        public bool ReverseChain;
        public PhysicsChainDefinition ChainDefinition = PhysicsChainDefinition.defaultDefinition;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;
        public TestBody testBody;
        public PhysicsChain chain => m_Chain;
        
        private PhysicsChain m_Chain;
        private int m_OwnerKey;

        public void UpdateShape(Vector2[] points)
        {
            Points = points;
            CreateShape();
        }

        private void Reset()
        {
            if (testBody == null)
                testBody = TestBody.FindTestBody(gameObject);
        }

        private void OnEnable()
        {
            Reset();

            if (testBody != null)
            {
                testBody.CreateBodyEvent += OnCreateBody;
                testBody.DestroyBodyEvent += OnDestroyBody;
            }

            CreateShape();

#if UNITY_EDITOR
            TestWorldTransformMonitor.AddMonitor(this);
#endif
        }

        private void OnDisable()
        {
            DestroyShape();

            if (testBody != null)
            {
                testBody.CreateBodyEvent -= OnCreateBody;
                testBody.DestroyBodyEvent -= OnDestroyBody;
            }

#if UNITY_EDITOR
            TestWorldTransformMonitor.RemoveMonitor(this);
#endif
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            CreateShape();
        }

        private void CreateShape()
        {
            // Destroy any existing shape.
            DestroyShape();

            if (!testBody)
                return;

            var body = testBody.body;
            if (!body.isValid)
                return;

            // Create the chain geometry.
            var chainGeometry = new ChainGeometry(ReverseChain ? Points.Reverse().ToArray() : Points);

            // Create the chain shape.
            m_Chain = body.CreateChain(chainGeometry, ChainDefinition);

            if (m_Chain.isValid)
            {
                // Set the user data.
                m_Chain.userData = UserData;

                // Set the callback target.
                m_Chain.callbackTarget = CallbackTarget;

                // Set the owner.
                m_OwnerKey = m_Chain.SetOwner(this);
            }
        }

        private void DestroyShape()
        {
            if (m_Chain.isValid)
            {
                m_Chain.Destroy(m_OwnerKey);
                m_Chain = default;
                m_OwnerKey = 0;
            }
        }

        private void OnCreateBody(TestBody testBody)
        {
            CreateShape();
        }

        private void OnDestroyBody(TestBody testBody)
        {
            DestroyShape();
        }

        void ITestWorldTransformChanged.TransformChanged()
        {
            if (m_Chain.isValid)
                CreateShape();
        }

        public override string ToString() => m_Chain.ToString();
    }
}