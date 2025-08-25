using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates how to control transfo and what a body will write to a Transform.
/// Press "Play" to see the bodies, each of which has a different transform write mode.
/// See the comments for more information.
/// </summary>
public class TransformPlaneWrite : MonoBehaviour
{
    public PhysicsWorld.TransformPlane TransformPlane = PhysicsWorld.TransformPlane.XY;
    public Vector2 LinearVelocity = Vector2.one * 5f;
    public float AngularVelocity = 90f;
    
    private PhysicsWorld m_PhysicsWorld;

    private void OnEnable()
    {
        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create(
            new PhysicsWorldDefinition
            {
                // Set the Transform Plane.
                transformPlane = TransformPlane,
                
                // We don't want gravity.
                gravity = Vector2.zero,
                
                // Increase the line-thickness for gizmos (better visuals).
                drawThickness = 5f
            });

       // Create a static area for the shapes to move around in.
        CreateArea();

        // Create two bodies at different positions.
        var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
        {
            // Set the body transform write mode to whatever is selected in the script.
            transformWriteMode = PhysicsBody.TransformWriteMode.Current,
            
            // We want a dynamic bodi so we move and have collision responses.
            bodyType = RigidbodyType2D.Dynamic,
            
            // Set the start position as the transform position.
            position = transform.position,
            
            // Start the body moving.
            linearVelocity = LinearVelocity,
            angularVelocity = AngularVelocity
        });
        
        // To perform transform writes, the body must specify which Transform object to write to!
        body.transformObject = transform;
        
        // Create a shape definition that has high bounciness.
        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 1f, friction = 0f } };
        
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
        var groundBody = m_PhysicsWorld.CreateBody();

        var extents = new Vector2(4f, 4f);
        using var extentPoints = new NativeList<Vector2>(Allocator.Temp)
        {
            new(-extents.x, extents.y),
            new(extents.x, extents.y),
            new(extents.x, -extents.y),
            new(-extents.x, -extents.y)
        };

        // Create a chain of line segments.
        groundBody.CreateChain(
            geometry: new ChainGeometry(extentPoints.AsArray()),
            definition: PhysicsChainDefinition.defaultDefinition);
    }
}