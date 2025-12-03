using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Unity.U2D.Physics.Extras;

public static class LegacyConversion
{
    private const string BaseMenu = "GameObject/2D Object/Physics/";
    private const string FromRigidbody2D = BaseMenu + "Rigidbody2D -> SceneBody";
    //private const string FromCircleCollider2D = BaseMenu + "CircleCollider2D -> SceneShape";
    //private const string FromCapsuleCollider2D = BaseMenu + "CapsuleCollider2D -> SceneShape";
    //private const string FromEdgeCollider2D = BaseMenu + "EdgeCollider2D -> SceneChain";
    //private const string FromPolygonCollider2D = BaseMenu + "PolygonCollider2D -> SceneOutlineShape";
    //private const string FromSpriteRenderer2D = BaseMenu + "SpriteRenderer -> SceneSpriteShape";

    private const SelectionMode ContextSelectionMode = SelectionMode.Editable | SelectionMode.Deep;
    
    [MenuItem(FromRigidbody2D, true, 0)]
    public static bool CanConvertFromRigidbody2D() => Selection.GetFiltered(typeof(Rigidbody2D), ContextSelectionMode).Length > 0;
    
    [MenuItem(FromRigidbody2D, false, 0)]
    private static void ConvertFromRigidbody2D()
    {
        foreach (var obj in Selection.GetFiltered(typeof(Rigidbody2D), ContextSelectionMode))
        {
            var rb = obj as Rigidbody2D;
            if (rb == null)
                continue;

            // Fetch the existing or add a new SceneBody.
            if (!rb.gameObject.TryGetComponent<SceneBody>(out var sceneBody))
                sceneBody = rb.gameObject.AddComponent<SceneBody>();
            
            // Disable the component.
            sceneBody.enabled = false;

            // Convert the properties we can.
            var bodyDefinition = new PhysicsBodyDefinition
            {
                type = (PhysicsBody.BodyType)rb.bodyType,
                constraints = (PhysicsBody.BodyConstraints)rb.constraints,
                linearDamping = rb.linearDamping,
                angularDamping = rb.angularDamping,
                gravityScale = rb.gravityScale,
                fastCollisionsAllowed = rb.collisionDetectionMode == CollisionDetectionMode2D.Continuous,
            };

            // Sleeping / Awake.
            switch (rb.sleepMode)
            {
                case RigidbodySleepMode2D.NeverSleep:
                {
                    bodyDefinition.sleepingAllowed = false;
                    bodyDefinition.awake = true;
                    
                    break;
                }
                
                case RigidbodySleepMode2D.StartAwake:
                {
                    bodyDefinition.sleepingAllowed = true;
                    bodyDefinition.awake = true;
                    break;
                }
                
                case RigidbodySleepMode2D.StartAsleep:
                {
                    bodyDefinition.sleepingAllowed = true;
                    bodyDefinition.awake = false;
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Interpolate.
            bodyDefinition.transformWriteMode = rb.interpolation switch
            {
                RigidbodyInterpolation2D.None => PhysicsBody.TransformWriteMode.Current,
                RigidbodyInterpolation2D.Interpolate => PhysicsBody.TransformWriteMode.Interpolate,
                RigidbodyInterpolation2D.Extrapolate => PhysicsBody.TransformWriteMode.Extrapolate,
                _ => throw new ArgumentOutOfRangeException()
            };

            // Material.
            if (rb.sharedMaterial)
                Debug.LogWarning($"Rigidbody2D has a PhysicsMaterial2D, this is not supported.", rb);
            
            // Auto Mass.
            if (rb.useAutoMass == false)
                Debug.LogWarning($"Rigidbody2D has 'AutoMass' disabled. To support this, you must set any attached shapes to have a Density of zero.", rb);

            // Assign the body definition.
            sceneBody.BodyDefinition = bodyDefinition;
            
            // Enable the component.
            sceneBody.enabled = true;
        }
    }
}
