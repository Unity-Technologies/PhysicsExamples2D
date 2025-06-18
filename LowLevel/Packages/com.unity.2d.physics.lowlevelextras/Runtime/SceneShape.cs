using UnityEngine.LowLevelPhysics2D;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(PhysicsLowLevelExtrasExecutionOrder.SceneShape)]
    [AddComponentMenu("Physics 2D (LowLevel)/Scene Shape", 1)]
    [Icon(IconUtility.IconPath + "SceneShape.png")]
    public sealed class SceneShape : MonoBehaviour, IWorldSceneDrawable
    {
        public PhysicsShape.ShapeType ShapeType = PhysicsShape.ShapeType.Circle;
        public CircleGeometry CircleGeometry = new();
        public CapsuleGeometry CapsuleGeometry = new();
        public SegmentGeometry SegmentGeometry = new();
        public ChainSegmentGeometry ChainSegmentGeometry = new();
        public PolygonGeometry PolygonGeometry = new();
        public PhysicsShapeDefinition ShapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        public bool ScaleRadius = true;
        public SceneBody SceneBody;

        public PhysicsShape Shape => m_Shape;
        private PhysicsShape m_Shape;
        private int m_OwnerKey;

        public void UpdateShape() => CreateShape();
        
        private void Reset()
        {
            // Finish if the scene body already set.
            if (SceneBody != null)
                return;

            // Set the scene body.
            SceneBody = SceneBody.FindSceneBody(gameObject);
#if true            
            // Set the contact filter categories to match the GameObject layer.
            // NOTE: We do this here for convenience as the component has just been added.
            ShapeDefinition.contactFilter = new PhysicsShape.ContactFilter { categories = new PhysicsMask(gameObject.layer), contacts = ShapeDefinition.contactFilter.contacts };
#endif
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
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;
            
            CreateShape();
        }

        private void Update()
        {
            if (Application.isPlaying ||
                !transform.hasChanged ||
                !m_Shape.isValid)
                return;
            
            // Reset transform changed flag.
            transform.hasChanged = false;

            // Is the body the same transform?
            if (SceneBody.transform == transform)
            {
                // Fetch the body.
                var body = SceneBody.Body;
                
                // Fetch the transform plane.
                var transformPlane = body.world.transformPlane;
                
                // Update the body.
                body.transform = new PhysicsTransform(
                    PhysicsMath.TransformPosition2D(transform.position, transformPlane),
                    new PhysicsRotate(PhysicsMath.TransformRotation2D(transform.rotation, transformPlane)));            
            }
            
            // Create the shape.
            CreateShape();
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
            
            // Calculate the relative transform from the scene body to this scene shape.
            var relativeTransform = PhysicsMath.GetRelativeMatrix(SceneBody.transform, transform, SceneBody.Body.world.transformPlane);
            
            // Create the appropriate shape.
            switch(ShapeType)
            {
                case PhysicsShape.ShapeType.Circle:
                {
                    var geometry = CircleGeometry.Transform(relativeTransform, ScaleRadius);
                    if (geometry.isValid)
                    {
                        m_Shape = body.CreateShape(geometry, ShapeDefinition);
                    }

                    break;
                }

                case PhysicsShape.ShapeType.Capsule:
                {
                    var geometry = CapsuleGeometry.Transform(relativeTransform, ScaleRadius);
                    if (geometry.isValid)
                    {
                        m_Shape = body.CreateShape(geometry, ShapeDefinition);
                    }

                    break;
                }

                case PhysicsShape.ShapeType.Segment:
                {
                    var geometry = SegmentGeometry.Transform(relativeTransform);
                    if (geometry.isValid)
                    {
                        m_Shape = body.CreateShape(geometry, ShapeDefinition);
                    }

                    break;
                }
                
                case PhysicsShape.ShapeType.ChainSegment:
                {
                    var geometry = ChainSegmentGeometry.Transform(relativeTransform);
                    if (geometry.isValid)
                    {
                        m_Shape = body.CreateShape(geometry, ShapeDefinition);
                    }

                    break;
                }                

                case PhysicsShape.ShapeType.Polygon:
                {
                    var geometry = PolygonGeometry.Transform(relativeTransform, ScaleRadius);
                    if (geometry.isValid)
                    {
                        m_Shape = body.CreateShape(geometry, ShapeDefinition);
                    }

                    break;
                }

                default:
                    return;
            }

            // Set owner.
            if (m_Shape.isValid)
                m_OwnerKey = m_Shape.SetOwner(this);
        }

        private void DestroyShape()
        {
            // Destroy the shape.
            if (m_Shape.isValid)
            {
                m_Shape.Destroy(true, m_OwnerKey);
                m_Shape = default;
                m_OwnerKey = 0;
            }
        }
        
        private void OnCreateBody(SceneBody sceneBody)
        {
            if (sceneBody == SceneBody)
                CreateShape();
        }

        private void OnDestroyBody(SceneBody sceneBody)
        {
            if (sceneBody == SceneBody)
                DestroyShape();
        }
        
        public void Draw()
        {
            if (!m_Shape.isValid)
                return;
            
            // Draw if we're drawing selections.
            if (m_Shape.world.drawOptions.HasFlag(PhysicsWorld.DrawOptions.SelectedShapes))
                m_Shape.Draw();
        }

        public override string ToString() => m_Shape.ToString();
        
#if false        
        [DrawGizmo(GizmoType.InSelectionHierarchy, typeof(SceneShape))]
        private static void DrawGizmos(SceneShape sceneShape, GizmoType gizmoType)
        {
            // Draw if we're drawing selections.
            var shape = sceneShape.Shape;
            if (shape.isValid && sceneShape.Shape.world.drawOptions.HasFlag(PhysicsWorld.DrawOptions.SelectedShapes))
                shape.Draw();
        }
#endif
    }
}