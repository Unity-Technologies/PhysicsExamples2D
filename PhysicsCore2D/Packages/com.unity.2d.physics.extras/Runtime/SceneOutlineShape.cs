using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrder.SceneShape)]
    [AddComponentMenu("Physics 2D/CoreExamples/Scene Outline Shape", 21)]
    [Icon(IconUtility.IconPath + "SceneShape.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace)]
    public sealed class SceneOutlineShape : MonoBehaviour, PhysicsCallbacks.ITransformChangedCallback, IWorldSceneDrawable
    {
        public Vector2[] Points = { Vector2.left + Vector2.down, Vector2.right + Vector2.down, Vector2.right + Vector2.up, Vector2.left + Vector2.up };
        public PhysicsShapeDefinition ShapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        public PhysicsUserData UserData;
        public MonoBehaviour CallbackTarget;
        public SceneBody SceneBody;

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
            if (SceneBody == null)
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

            if (SceneBody != null)
            {
                SceneBody.CreateBodyEvent -= OnCreateBody;
                SceneBody.DestroyBodyEvent -= OnDestroyBody;
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

            if (!SceneBody)
                return;

            var body = SceneBody.Body;
            if (!body.isValid)
                return;

            if (Points.Length < 3)
                return;
            
            // Calculate the polygons from the points.
            using var polygons = PolygonGeometry.CreatePolygons(Points, PhysicsTransform.identity, transform.lossyScale);

            // Calculate the relative transform from the scene body to this scene shape.
            var relativeTransform = PhysicsMath.GetRelativeMatrix(SceneBody.transform, transform, SceneBody.Body.world.transformPlane, useScale: false);

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

            if (SceneBody.Body.isValid)
                SceneBody.Body.ApplyMassFromShapes();
        }

        private void OnCreateBody(SceneBody sceneBody)
        {
            CreateShapes();
        }

        private void OnDestroyBody(SceneBody sceneBody)
        {
            DestroyShapes();
        }

        void PhysicsCallbacks.ITransformChangedCallback.OnTransformChanged(PhysicsEvents.TransformChangeEvent transformChangeEvent)
        {
            CreateShapes();
        }

        void IWorldSceneDrawable.Draw()
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