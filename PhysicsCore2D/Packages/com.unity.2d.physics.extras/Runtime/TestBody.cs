using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.TestBody)]
    [AddComponentMenu("Physics 2D/CoreExamples/Test Body", 10)]
    [Icon(IconUtility.IconPath + "TestBody.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, "SceneBody")]
    public sealed class TestBody : MonoBehaviour, PhysicsCallbacks.ITransformChangedCallback, IWorldDrawable
    {
        public PhysicsBodyDefinition BodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;
        public bool UseDefaultWorld = true;
        public TestWorld testWorld;

        public PhysicsBody body => m_Body;
        private int m_OwnerKey;
        private PhysicsBody m_Body;

        public delegate void TestBodyCreateEventHandler(TestBody testBody);

        public delegate void TestBodyDestroyEventHandler(TestBody testBody);

        public event TestBodyCreateEventHandler CreateBodyEvent;
        public event TestBodyDestroyEventHandler DestroyBodyEvent;

        /// <summary>
        /// Find a TestBody.
        /// </summary>
        /// <param name="obj">The GameObject used to find the most likely TestBody.</param>
        /// <returns>The found TestBody or NULL if not found at all.</returns>
        public static TestBody FindTestBody(GameObject obj)
        {
            // Find a TestBody up this hierarchy.
            return obj.GetComponentInParent<TestBody>();
        }

        private void Reset()
        {
            if (UseDefaultWorld || testWorld != null)
                return;

            // This is super slow, hopefully we don't need to do this.
            testWorld = TestWorld.FindTestWorld(gameObject);
        }

        private void OnEnable()
        {
            Reset();

            // Register to body recreation.
            if (testWorld != null)
            {
                testWorld.CreateWorldEvent += OnCreateWorld;
                testWorld.DestroyWorldEvent += OnDestroyWorld;
            }

            // Create the body.
            CreateBody();

            // Fix any TestShapes here that are not assigned a TestBody.
            FixUnassignedTestShapes();

            // Register for transform changes.
            PhysicsWorld.RegisterTransformChange(transform, this);
        }

        private void OnDisable()
        {
            DestroyBody();

            if (testWorld != null)
            {
                testWorld.CreateWorldEvent -= OnCreateWorld;
                testWorld.DestroyWorldEvent -= OnDestroyWorld;
            }

            // Unregister from transform changes.
            PhysicsWorld.UnregisterTransformChange(transform, this);
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            // Create a new body.
            CreateBody();
        }

        private void CreateBody()
        {
            // Destroy any existing body.
            DestroyBody();

            var world = UseDefaultWorld || testWorld == null ? PhysicsWorld.defaultWorld : testWorld.world;

            // Fetch the transform plane.
            var transformPlane = world.transformPlane;

            // Create the body at the transform position.
            BodyDefinition.position = PhysicsMath.ToPosition2D(transform.position, transformPlane);
            BodyDefinition.rotation = PhysicsRotate.FromRadians(PhysicsMath.ToRotation2D(transform.rotation, transformPlane));
            m_Body = PhysicsBody.Create(world: world, definition: BodyDefinition);
            if (m_Body.isValid)
            {
                // Set the transform object.
                m_Body.transformObject = transform;

                // Set the user data.
                m_Body.userData = UserData;

                // Set the callback target.
                m_Body.callbackTarget = CallbackTarget;

                // Set Owner.
                m_OwnerKey = m_Body.SetOwner(this);
            }

            // Notify.
            CreateBodyEvent?.Invoke(this);
        }

        private void DestroyBody()
        {
            // Destroy the body.
            if (m_Body.isValid)
            {
                DestroyBodyEvent?.Invoke(this);

                m_Body.Destroy(m_OwnerKey);
                m_Body = default;
                m_OwnerKey = 0;
            }
        }

        private void OnCreateWorld(TestWorld testWorld)
        {
            if (!UseDefaultWorld)
                CreateBody();
        }

        private void OnDestroyWorld(TestWorld testWorld)
        {
            if (!UseDefaultWorld)
                DestroyBody();
        }

        private void FixUnassignedTestShapes()
        {
            var componentCount = gameObject.GetComponentCount();
            for (var i = 0; i < componentCount; ++i)
            {
                var sceneShape = gameObject.GetComponentAtIndex(i) as TestShape;
                if (sceneShape && sceneShape.testBody == null)
                {
                    sceneShape.testBody = this;

                    // If the scene shape is active then we need to toggle its enabled state for these changes to take effect.
                    if (sceneShape.isActiveAndEnabled)
                    {
                        sceneShape.enabled = false;
                        sceneShape.enabled = true;
                    }
                }
            }
        }
        
        void PhysicsCallbacks.ITransformChangedCallback.OnTransformChanged(PhysicsEvents.TransformChangeEvent transformChangeEvent)
        {
            if (m_Body.isValid)
                CreateBody();
        }

        void IWorldDrawable.Draw()
        {
            if (!m_Body.isValid)
                return;

            // Draw if we're drawing selections.
            if (m_Body.world.drawOptions.HasFlag(PhysicsWorld.DrawOptions.SelectedBodies))
                m_Body.Draw();
        }

        public override string ToString() => m_Body.ToString();
    }
}