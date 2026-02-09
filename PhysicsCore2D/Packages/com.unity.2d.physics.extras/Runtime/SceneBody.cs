using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.SceneBody)]
    [AddComponentMenu("Physics 2D/LowLevel/Scene Body", 10)]
    [Icon(IconUtility.IconPath + "SceneBody.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace)]
    public sealed class SceneBody : MonoBehaviour, IWorldSceneTransformChanged, IWorldSceneDrawable
    {
        public PhysicsBodyDefinition BodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;
        public bool UseDefaultWorld = true;
        public SceneWorld SceneWorld;

        public PhysicsBody Body => m_Body;
        private int m_OwnerKey;
        private PhysicsBody m_Body;

        public delegate void SceneBodyCreateEventHandler(SceneBody sceneBody);

        public delegate void SceneBodyDestroyEventHandler(SceneBody sceneBody);

        public event SceneBodyCreateEventHandler CreateBodyEvent;
        public event SceneBodyDestroyEventHandler DestroyBodyEvent;

        /// <summary>
        /// Find a SceneBody.
        /// </summary>
        /// <param name="obj">The GameObject used to find the most likely SceneBody.</param>
        /// <returns>The found SceneBody or NULL if not found at all.</returns>
        public static SceneBody FindSceneBody(GameObject obj)
        {
            // Find a SceneBody up this hierarchy.
            return obj.GetComponentInParent<SceneBody>();
        }

        private void Reset()
        {
            if (UseDefaultWorld || SceneWorld != null)
                return;

            // This is super slow, hopefully we don't need to do this.
            SceneWorld = SceneWorld.FindSceneWorld(gameObject);
        }

        private void OnEnable()
        {
            Reset();

            // Register to body recreation.
            if (SceneWorld != null)
            {
                SceneWorld.CreateWorldEvent += OnCreateWorld;
                SceneWorld.DestroyWorldEvent += OnDestroyWorld;
            }

            // Create the body.
            CreateBody();

            // Fix any SceneShapes here that are not assigned a SceneBody.
            FixUnassignedSceneShapes();

#if UNITY_EDITOR
            WorldSceneTransformMonitor.AddMonitor(this);
#endif
        }

        private void OnDisable()
        {
            DestroyBody();

            if (SceneWorld != null)
            {
                SceneWorld.CreateWorldEvent -= OnCreateWorld;
                SceneWorld.DestroyWorldEvent -= OnDestroyWorld;
            }

#if UNITY_EDITOR
            WorldSceneTransformMonitor.RemoveMonitor(this);
#endif
        }

        private void CreateBody(bool transformChanged = false)
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

        private void OnCreateWorld(SceneWorld sceneWorld)
        {
            if (!UseDefaultWorld)
                CreateBody();
        }

        private void OnDestroyWorld(SceneWorld sceneWorld)
        {
            if (!UseDefaultWorld)
                DestroyBody();
        }

        private PhysicsWorld GetWorld() => UseDefaultWorld || SceneWorld == null ? PhysicsWorld.defaultWorld : SceneWorld.World;
        
        void IWorldSceneTransformChanged.TransformChanged()
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

        private void FixUnassignedSceneShapes()
        {
            var componentCount = gameObject.GetComponentCount();
            for (var i = 0; i < componentCount; ++i)
            {
                var sceneShape = gameObject.GetComponentAtIndex(i) as SceneShape;
                if (sceneShape && sceneShape.SceneBody == null)
                {
                    sceneShape.SceneBody = this;

                    // If the scene shape is active then we need to toggle its enabled state for these changes to take effect.
                    if (sceneShape.isActiveAndEnabled)
                    {
                        sceneShape.enabled = false;
                        sceneShape.enabled = true;
                    }
                }
            }
        }
        
        void IWorldSceneDrawable.Draw()
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