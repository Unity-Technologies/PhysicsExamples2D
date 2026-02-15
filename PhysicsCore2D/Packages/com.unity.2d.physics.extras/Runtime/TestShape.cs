using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.TestShape)]
    [AddComponentMenu("Physics 2D/CoreExamples/Test Shape", 20)]
    [Icon(IconUtility.IconPath + "TestShape.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, "SceneShape")]
    public sealed class TestShape : MonoBehaviour, PhysicsCallbacks.ITransformChangedCallback, IWorldDrawable
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
        public TestBody testBody;

        public PhysicsShape shape => m_Shape;
        private PhysicsShape m_Shape;
        private int m_OwnerKey;

        public void UpdateShape() => CreateShape();

        private void Reset()
        {
            // Finish if the scene body already set.
            if (testBody != null)
                return;

            // Set the scene body.
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

            // Register for transform changes.
            PhysicsWorld.RegisterTransformChange(transform, this);
        }

        private void OnDisable()
        {
            DestroyShape();

            if (testBody != null)
            {
                testBody.CreateBodyEvent -= OnCreateBody;
                testBody.DestroyBodyEvent -= OnDestroyBody;
            }

            // Unregister from transform changes.
            PhysicsWorld.UnregisterTransformChange(transform, this);
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

            // Calculate the relative transform from the scene body to this scene shape.
            var relativeTransform = PhysicsMath.GetRelativeMatrix(testBody.transform, transform, testBody.body.world.transformPlane);

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

        private void OnCreateBody(TestBody testBody)
        {
            if (testBody == this.testBody)
                CreateShape();
        }

        private void OnDestroyBody(TestBody testBody)
        {
            if (testBody == this.testBody)
                DestroyShape();
        }

        void PhysicsCallbacks.ITransformChangedCallback.OnTransformChanged(PhysicsEvents.TransformChangeEvent transformChangeEvent)
        {
            if (m_Shape.isValid)
                CreateShape();
        }

        void IWorldDrawable.Draw()
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