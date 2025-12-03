using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.SceneShape)]
    [AddComponentMenu("Physics 2D/LowLevel/Scene Shape", 20)]
    [Icon(IconUtility.IconPath + "SceneShape.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace)]
    public sealed class SceneShape : MonoBehaviour, IWorldSceneTransformChanged, IWorldSceneDrawable
    {
        public PhysicsShape.ShapeType ShapeType = PhysicsShape.ShapeType.Circle;
        public CircleGeometry CircleGeometry = new();
        public CapsuleGeometry CapsuleGeometry = new();
        public SegmentGeometry SegmentGeometry = new();
        public ChainSegmentGeometry ChainSegmentGeometry = new();
        public PolygonGeometry PolygonGeometry = new();
        public PhysicsShapeDefinition ShapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;
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

#if UNITY_EDITOR
            WorldSceneTransformMonitor.AddMonitor(this);
#endif
        }

        private void OnDisable()
        {
            DestroyShape();

            if (SceneBody != null)
            {
                SceneBody.CreateBodyEvent -= OnCreateBody;
                SceneBody.DestroyBodyEvent -= OnDestroyBody;
            }

#if UNITY_EDITOR
            WorldSceneTransformMonitor.RemoveMonitor(this);
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

            if (!SceneBody)
                return;

            var body = SceneBody.Body;
            if (!body.isValid)
                return;

            // Calculate the relative transform from the scene body to this scene shape.
            var relativeTransform = PhysicsMath.GetRelativeMatrix(SceneBody.transform, transform, SceneBody.Body.world.transformPlane);

            // Create the appropriate shape.
            switch (ShapeType)
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

            // Set extra details.
            if (m_Shape.isValid)
            {
                // Set the user data.
                m_Shape.userData = UserData;

                // Set the callback target.
                m_Shape.callbackTarget = CallbackTarget;

                // Set the owner.
                m_OwnerKey = m_Shape.SetOwner(this);
            }
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

        void IWorldSceneTransformChanged.TransformChanged()
        {
            if (m_Shape.isValid)
                CreateShape();
        }

        void IWorldSceneDrawable.Draw()
        {
            if (!m_Shape.isValid)
                return;

            // Draw if we're drawing selections.
            if (m_Shape.world.drawOptions.HasFlag(PhysicsWorld.DrawOptions.SelectedShapes))
                m_Shape.Draw();
        }

        public override string ToString() => m_Shape.ToString();
    }
}