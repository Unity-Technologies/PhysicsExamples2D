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
    [AddComponentMenu("Physics 2D/LowLevel/Test Shape", 20)]
    [Icon(IconUtility.IconPath + "TestShape.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, sourceClassName: "TestShape")]
    public sealed class TestShape : MonoBehaviour, IWorldTransformChanged, IWorldDrawable
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
            // Finish if the test body already set.
            if (testBody != null)
                return;

            // Set the test body.
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
            WorldTransformMonitor.AddMonitor(this);
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
            WorldTransformMonitor.RemoveMonitor(this);
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

            // Calculate the relative transform from the test body to this test shape.
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

        void IWorldTransformChanged.TransformChanged()
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