using System;
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates how to control if and what a body will write to a Transform.
/// Press "Play" to see the bodies, each of which has a different transform write mode.
/// See the comments for more information.
/// </summary>
public class TransformWriteCustom : MonoBehaviour, PhysicsCallbacks.ITransformWriteCallback
{
    public PhysicsWorld.TransformWriteMode WorldWriteMode = PhysicsWorld.TransformWriteMode.Custom;
    public PhysicsWorld.TransformTweenMode WorldTweenMode = PhysicsWorld.TransformTweenMode.Custom;
    public PhysicsBody.TransformWriteMode TransformWriteMode = PhysicsBody.TransformWriteMode.Current;
    
    private PhysicsWorld m_PhysicsWorld;

    private void OnEnable()
    {
        // Create a world.
        // NOTE: For the world to perform transform writes, it needs to use "TransformWriteMode.Fast2D" or "TransformWriteModeSlow3D".
        // If the world is set to "TransformWriteMode.Off", no transform writes will occur, irrelevant of what each body requests.
        // You can change this dynamically with "PhysicsWorld.transformWriteMode" or preferably by setting the default in the physics low-level settings as is used in this project (it uses "Fast2D").
        m_PhysicsWorld = PhysicsWorld.Create(
            new PhysicsWorldDefinition
            {
                transformWriteMode = WorldWriteMode,
                transformTweenMode = WorldTweenMode
            });

        // Set the custom callback target.
        m_PhysicsWorld.transformWriteCallbackTarget = this;
        
        // Create a static area for the shapes to move around in.
        CreateArea();
        
        // Create two bodies at different positions.
        var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
        {
            // Set the body transform write mode to whatever is selected in the script.
            transformWriteMode = TransformWriteMode,
            
            // We want a dynamic bodi so we move and have collision responses.
            type = PhysicsBody.BodyType.Dynamic,

            // Not needed for the demo but ensure we use CCD against the ground.
            collisionThreshold = 0f,
            
            // Set the start position to be the Transform position.
            position = transform.position,
        });
        
        // To perform transform writes, the body must specify which Transform object is being used to convert 2D to 3D custom writes.
        body.transformObject = transform;
        
        // Create a shape definition that has high bounciness.
        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 1f} };
        
        // Create a shape.
        body.CreateShape(new CircleGeometry { radius = 1f }, shapeDef);
    }

    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        m_PhysicsWorld.Destroy();
    }

    private void CreateArea()
    {
        // Ground Body. 
        var groundBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { collisionThreshold = 0f, fastCollisionsAllowed = false });
        var extents = new Vector2(8f, -5f);
        groundBody.CreateShape(new SegmentGeometry { point1 = extents, point2 = new Vector2(-extents.x, extents.y) });
    }

    public void OnTransformWrite(PhysicsEvents.TransformWriteEvent transformWriteEvent)
    {
        if (transformWriteEvent.physicsWorld != m_PhysicsWorld)
            return;

        foreach (var writeTween in transformWriteEvent.tweens)
        {
            var transformPlane = transformWriteEvent.transformPlane;
            var transformPlaneCustom = transformWriteEvent.transformPlaneCustom;
            writeTween.GetPose(transformPlane, ref transformPlaneCustom, true, out var position, out var rotation);

            PhysicsWorld.SetTransform(writeTween.transform, ref position, ref rotation);
        }
        
        Debug.Log("OnTransformWrite");
    }

    public void OnTransformTweenWrite(PhysicsEvents.TransformTweenWriteEvent transformTweenWriteEvent)
    {
        if (transformTweenWriteEvent.physicsWorld != m_PhysicsWorld)
            return;

        // Fetch some data.
        var transformPlane = transformTweenWriteEvent.transformPlane;
        var transformPlaneCustom = transformTweenWriteEvent.transformPlaneCustom;
        var interpolatedTime = transformTweenWriteEvent.interpolationTime;
        var extrapolationTime = transformTweenWriteEvent.extrapolationTime;

        // Iterate the tweens.
        foreach (var writeTween in transformTweenWriteEvent.tweens)
        {
            switch (writeTween.transformWriteMode)
            {
                case PhysicsBody.TransformWriteMode.Interpolate:
                {
                    writeTween.GetInterpolatedPose(transformPlane, ref transformPlaneCustom, true, interpolatedTime, out var position, out var rotation);
                    PhysicsWorld.SetTransform(writeTween.transform, ref position, ref rotation);
                    continue;
                }
                case PhysicsBody.TransformWriteMode.Extrapolate:
                {
                    writeTween.GetExtrapolatedPose(transformPlane, ref transformPlaneCustom, extrapolationTime, out var position, out var rotation);
                    PhysicsWorld.SetTransform(writeTween.transform, ref position, ref rotation);
                    continue;
                }

                case PhysicsBody.TransformWriteMode.Off:
                case PhysicsBody.TransformWriteMode.Current:
                default:
                    continue;
            }
        }
        
        Debug.Log("OnTransformTweenWrite");
    }

    private void OnValidate()
    {
        if (m_PhysicsWorld.isValid)
        {
            m_PhysicsWorld.transformWriteMode = WorldWriteMode;
            m_PhysicsWorld.transformTweenMode = WorldTweenMode;
        }
    }
}