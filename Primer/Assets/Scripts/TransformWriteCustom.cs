using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates how to control if and what a body will write to a Transform in custom mode.
/// See the comments for more information.
/// </summary>
public class TransformWriteCustom : MonoBehaviour, PhysicsCallbacks.ITransformWriteCallback
{
    public PhysicsBody.TransformWriteMode BodyWriteMode = PhysicsBody.TransformWriteMode.Interpolate;
    public bool DrawShape = true;
    public bool ConsoleLog = false;

    private const PhysicsWorld.TransformWriteMode WorldWriteMode = PhysicsWorld.TransformWriteMode.Custom;
    private const PhysicsWorld.TransformTweenMode WorldTweenMode = PhysicsWorld.TransformTweenMode.Custom;
    private PhysicsWorld m_PhysicsWorld;
    private PhysicsBody m_PhysicsBody;
    private PhysicsShape m_PhysicsShape;

    private void OnEnable()
    {
        // Create a world.
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
        
        // Create a body.
        m_PhysicsBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
        {
            // Set the body transform write mode to whatever is selected in the script.
            transformWriteMode = BodyWriteMode,
            
            // We want a dynamic bodi so we move and have collision responses.
            type = PhysicsBody.BodyType.Dynamic,

            // Not needed for the demo but ensure we use CCD against the ground.
            collisionThreshold = 0f,
            
            // Set the start position to be the Transform position.
            position = transform.position,
        });
        
        // To perform transform writes, the body must specify which Transform object is being used to convert 2D to 3D custom writes.
        m_PhysicsBody.transformObject = transform;

        // Create a shape definition that has high bounciness.
        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 1f}, worldDrawing = DrawShape };
        
        // Create a shape.
        m_PhysicsShape = m_PhysicsBody.CreateShape(new CircleGeometry { radius = 1f }, shapeDef);
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
        
        if (ConsoleLog)
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
        
        if (ConsoleLog)
            Debug.Log("OnTransformTweenWrite");
    }

    private void OnValidate()
    {
        if (!m_PhysicsWorld.isValid ||
            !m_PhysicsBody.isValid ||
            !m_PhysicsShape.isValid)
            return;

        // Update.
        m_PhysicsWorld.transformWriteMode = WorldWriteMode;
        m_PhysicsWorld.transformTweenMode = WorldTweenMode;
        m_PhysicsBody.transformWriteMode = BodyWriteMode;
        m_PhysicsShape.worldDrawing = DrawShape;
    }
}