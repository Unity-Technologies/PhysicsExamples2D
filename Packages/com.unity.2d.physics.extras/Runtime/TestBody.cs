using UnityEditor;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.TestBody)]
    [AddComponentMenu("Physics 2D/LowLevel/Test Body", 10)]
    [Icon(IconUtility.IconPath + "TestBody.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, sourceClassName: "TestBody")]
    public sealed class TestBody : MonoBehaviour, IWorldTransformChanged, IWorldDrawable
    {
        public PhysicsBodyDefinition BodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;
        public bool UseDefaultWorld = true;
        public TestWorld testWorld;
        public PhysicsBody body => m_Body;
        
        private PhysicsBody m_Body;
        private int m_OwnerKey;

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

#if UNITY_EDITOR
            WorldTransformMonitor.AddMonitor(this);
#endif
        }

        private void OnDisable()
        {
            DestroyBody();

            if (testWorld != null)
            {
                testWorld.CreateWorldEvent -= OnCreateWorld;
                testWorld.DestroyWorldEvent -= OnDestroyWorld;
            }

#if UNITY_EDITOR
            WorldTransformMonitor.RemoveMonitor(this);
#endif
        }

        private void CreateBody()
        {
            // Destroy any existing body.
            DestroyBody();

            var world = GetWorld();

#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
#endif
                SyncDefinitionToTransform();
            
            // Create the body at the transform position.
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

        private PhysicsWorld GetWorld() => UseDefaultWorld || testWorld == null ? PhysicsWorld.defaultWorld : testWorld.world;
        
        void IWorldTransformChanged.TransformChanged()
        {
            SyncDefinitionToTransform(); 
            
            // Create a new body.
            CreateBody();
        }
        
        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            SyncTransformToDefinition();
            
            // Create a new body.
            CreateBody();
        }

        private void SyncDefinitionToTransform()
        {
            // Fetch the world.
            var world = GetWorld();

            // Fetch the transform plane.
            var transformPlane = world.transformPlane;
            
            // Update the body definition to match the transform.
            BodyDefinition.position = PhysicsMath.ToPosition2D(transform.position, transformPlane);
            BodyDefinition.rotation = new PhysicsRotate(PhysicsMath.ToRotation2D(transform.rotation, transformPlane));
        }

        private void SyncTransformToDefinition()
        {
#if UNITY_EDITOR
            // Synchronize the transform to the body definition.
            if (!EditorApplication.isPlaying)
            {
                // Fetch the world.
                var world = GetWorld();

                // Fetch the transform plane.
                var transformPlane = world.transformPlane;
            
                // Fetch the transform pose.
                var transformPosition = transform.position;
                var transformRotation = transform.rotation;
            
                // Calculate the body pose.
                var position = PhysicsMath.ToPosition2D(transformPosition, transformPlane);
                var rotation = new PhysicsRotate(PhysicsMath.ToRotation2D(transformRotation, transformPlane));
            
                // If ithe body definition and transform don't match then update the transform.
                if (BodyDefinition.position != position || !Mathf.Approximately(BodyDefinition.rotation.angle, rotation.angle))
                {
                    transform.SetPositionAndRotation(
                        PhysicsMath.ToPosition3D(BodyDefinition.position, transformPosition, transformPlane),
                        PhysicsMath.ToRotationSlow3D(BodyDefinition.rotation.angle, transformRotation, transformPlane));
                }
            }
#endif            
        }

        private void FixUnassignedTestShapes()
        {
            var componentCount = gameObject.GetComponentCount();
            for (var i = 0; i < componentCount; ++i)
            {
                var testShape = gameObject.GetComponentAtIndex(i) as TestShape;
                if (testShape && testShape.testBody == null)
                {
                    testShape.testBody = this;

                    // If the test shape is active then we need to toggle its enabled state for these changes to take effect.
                    if (testShape.isActiveAndEnabled)
                    {
                        testShape.enabled = false;
                        testShape.enabled = true;
                    }
                }
            }
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