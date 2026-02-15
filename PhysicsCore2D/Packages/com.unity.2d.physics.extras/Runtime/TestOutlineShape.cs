using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.TestShape)]
    [AddComponentMenu("Physics 2D/CoreExamples/Test Outline Shape", 21)]
    [Icon(IconUtility.IconPath + "TestShape.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, "SceneOutlineShape")]
    public sealed class TestOutlineShape : MonoBehaviour, PhysicsCallbacks.ITransformChangedCallback, IWorldDrawable
    {
        public Vector2[] Points = { Vector2.left + Vector2.down, Vector2.right + Vector2.down, Vector2.right + Vector2.up, Vector2.left + Vector2.up };
        public PhysicsShapeDefinition ShapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;
        public TestBody testBody;

        private struct OwnedShapes
        {
            public PhysicsShape Shape;
            public int OwnerKey;
        }

        private NativeList<OwnedShapes> m_OwnedShapes;

        public void UpdateShape(Vector2[] points)
        {
            Points = points;
            CreateShapes();
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

            m_OwnedShapes = new NativeList<OwnedShapes>(Allocator.Persistent);

            CreateShapes();

            // Register for transform changes.
            PhysicsWorld.RegisterTransformChange(transform, this);
        }

        private void OnDisable()
        {
            DestroyShapes();

            if (m_OwnedShapes.IsCreated)
                m_OwnedShapes.Dispose();

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

            CreateShapes();
        }

        private void CreateShapes()
        {
            // Destroy any existing shape.
            DestroyShapes();

            if (!m_OwnedShapes.IsCreated)
                return;

            if (!testBody)
                return;

            var body = testBody.body;
            if (!body.isValid)
                return;

            if (Points.Length < 3)
                return;
            
            // Calculate the polygons from the points.
            using var polygons = PolygonGeometry.CreatePolygons(Points, PhysicsTransform.identity, transform.lossyScale);

            // Calculate the relative transform from the scene body to this scene shape.
            var relativeTransform = PhysicsMath.GetRelativeMatrix(testBody.transform, transform, testBody.body.world.transformPlane, useScale: false);

            // Iterate the polygons.
            foreach (var geometry in polygons)
            {
                if (!geometry.isValid)
                    continue;

                var shapeGeometry = geometry.Transform(relativeTransform, false);
                if (!shapeGeometry.isValid)
                    continue;

                var shape = body.CreateShape(shapeGeometry, ShapeDefinition);
                if (!shape.isValid)
                    continue;

                // Set the user data.
                shape.userData = UserData;

                // Set the callback target.
                shape.callbackTarget = CallbackTarget;

                // Set the owner.
                var ownerKey = shape.SetOwner(this);

                // Add to owned shapes.
                m_OwnedShapes.Add(new OwnedShapes
                {
                    Shape = shape,
                    OwnerKey = ownerKey
                });
            }
        }

        private void DestroyShapes()
        {
            if (!m_OwnedShapes.IsCreated)
                return;

            foreach (var ownedShape in m_OwnedShapes)
            {
                if (ownedShape.Shape.isValid)
                    ownedShape.Shape.Destroy(updateBodyMass: false, ownerKey: ownedShape.OwnerKey);
            }

            m_OwnedShapes.Clear();

            if (testBody.body.isValid)
                testBody.body.ApplyMassFromShapes();
        }

        private void OnCreateBody(TestBody testBody)
        {
            CreateShapes();
        }

        private void OnDestroyBody(TestBody testBody)
        {
            DestroyShapes();
        }

        void PhysicsCallbacks.ITransformChangedCallback.OnTransformChanged(PhysicsEvents.TransformChangeEvent transformChangeEvent)
        {
            CreateShapes();
        }

        void IWorldDrawable.Draw()
        {
            // Finish if we've nothing to draw.
            if (!m_OwnedShapes.IsCreated || m_OwnedShapes.Length == 0)
                return;

            // Finish if we're not drawing selections.
            if (!m_OwnedShapes[0].Shape.world.drawOptions.HasFlag(PhysicsWorld.DrawOptions.SelectedShapes))
                return;

            // Draw selections.
            foreach (var ownedShape in m_OwnedShapes)
                ownedShape.Shape.Draw();
        }
    }
}