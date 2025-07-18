using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.LowLevelPhysics2D;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [ExecuteAlways]
    [DefaultExecutionOrder(PhysicsLowLevelExtrasExecutionOrder.SceneShape)]
    [AddComponentMenu("Physics 2D (LowLevel)/Scene Outline Shape", 1)]
    [Icon(IconUtility.IconPath + "SceneShape.png")]
    public sealed class SceneOutlineShape : MonoBehaviour
    {
        public Vector2[] Points = { Vector2.left + Vector2.down, Vector2.right + Vector2.down, Vector2.right + Vector2.up, Vector2.left + Vector2.up };
        public PhysicsShapeDefinition ShapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        public PhysicsUserData UserData;
        public Object CallbackTarget;
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
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            DestroyShapes();
            CreateShapes();
        }

        private void Update()
        {
            if (Application.isPlaying ||
                !transform.hasChanged ||
                m_OwnedShapes.Length == 0)
                return;
            
            // Reset transform changed flag.
            transform.hasChanged = false;

            // Is the body the same transform or deeper in the hierarchy?
            if (SceneBody.transform == transform.root || SceneBody.transform.IsChildOf(transform))
            {
                // Fetch the body.
                var body = SceneBody.Body;
                
                // Fetch the transform plane.
                var transformPlane = body.world.transformPlane;
                
                // Update the body.
                body.transform = new PhysicsTransform(
                    PhysicsMath.ToPosition2D(transform.position, transformPlane),
                    new PhysicsRotate(PhysicsMath.ToRotation2D(transform.rotation, transformPlane)));            
            }
            
            // Create the shape.
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
            
            // Calculate the polygons from the points.
            var composer = PhysicsComposer.Create();
            composer.AddLayer(Points, PhysicsTransform.identity);
            using var polygons = composer.CreatePolygonGeometry();
            composer.Destroy();
            
            // Calculate the relative transform from the scene body to this scene shape.
            var relativeTransform = PhysicsMath.GetRelativeMatrix(SceneBody.transform, transform, SceneBody.Body.world.transformPlane);

            // Iterate the polygons.
            foreach (var geometry in polygons)
            {
                if (!geometry.isValid)
                    continue;
                
                var shapeGeometry = geometry.Transform(relativeTransform, false);
                if (!shapeGeometry.isValid)
                    continue;
                
                var shape = body.CreateShape(geometry, ShapeDefinition);
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
    }
}