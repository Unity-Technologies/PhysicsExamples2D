using UnityEngine.LowLevelPhysics2D;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [DefaultExecutionOrder(PhysicsLowLevelExtrasExecutionOrder.SceneBody)]
    [AddComponentMenu("Physics 2D (LowLevel)/Scene Body")]
    [Icon(IconUtility.IconPath + "SceneBody.png")]
    public sealed class SceneBody : MonoBehaviour, IWorldSceneDrawable
    {
        public PhysicsBodyDefinition BodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        public SceneWorld SceneWorld;
        public Object CallbackTarget;

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
            if (SceneWorld != null)
                return;
            
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
        }

        private void OnDisable()
        {
            DestroyBody();

            if (SceneWorld != null)
            {
                SceneWorld.CreateWorldEvent -= OnCreateWorld;
                SceneWorld.DestroyWorldEvent -= OnDestroyWorld;
            }
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;
            
            // Create a new body.
            DestroyBody();
            CreateBody();
        }

        private void CreateBody()
        {
            var world = SceneWorld == null ? PhysicsWorld.defaultWorld : SceneWorld.World;
            
            // Fetch the transform plane.
            var transformPlane = world.transformPlane;
            
            // Create the body at the transform position.
            BodyDefinition.position = PhysicsMath.ToPosition2D(transform.position, transformPlane);
            BodyDefinition.rotation = new PhysicsRotate(PhysicsMath.ToRotation2D(transform.rotation, transformPlane));
            m_Body = PhysicsBody.Create(world: world, definition: BodyDefinition);
            if (m_Body.isValid)
            {
                // Set the transform object.
                m_Body.transformObject = transform;

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
            CreateBody();
        }

        private void OnDestroyWorld(SceneWorld sceneWorld)
        {
            DestroyBody();
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

        public void Draw()
        {
            if (!m_Body.isValid)
                return;
            
            // Draw if we're drawing selections.
            var world = SceneWorld == null ? PhysicsWorld.defaultWorld : SceneWorld.World;
            if (world.drawOptions.HasFlag(PhysicsWorld.DrawOptions.SelectedBodies))
                m_Body.Draw();
        }
        
        public override string ToString() => m_Body.ToString();
    }
}