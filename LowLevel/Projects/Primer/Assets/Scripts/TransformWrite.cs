using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates how to control if and what a body will write to a Transform.
/// Press "Play" to see the bodies, each of which has a different transform write mode.
/// See the comments for more information.
/// </summary>
public class TransformWrite : MonoBehaviour
{
    public PhysicsBody.TransformWriteMode TransformWriteMode = PhysicsBody.TransformWriteMode.Off;
    
    private PhysicsWorld m_PhysicsWorld;

    private void OnEnable()
    {
        // Create a world.
        // NOTE: For the world to perform transform writes, it needs to use "TransformWriteMode.Fast2D" or "TransformWriteModeSlow3D".
        // If the world is set to "TransformWriteMode.Off", no transform writes will occur, irrelevant of what each body requests.
        // YOu can change this dynamically with "PhysicsWorld.transformWriteMode" or preferably by setting the default in the physics low-level settings as is used in this project (it uses "Fast2D").
        m_PhysicsWorld = PhysicsWorld.Create();

       // Create a static area for the shapes to move around in.
        CreateArea();
        
        // Create two bodies at different positions.
        var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
        {
            // Set the body transform write mode to whatever is selected in the script.
            transformWriteMode = TransformWriteMode,
            
            // We want a dynamic bodi so we move and have collision responses.
            bodyType = RigidbodyType2D.Dynamic,
            
            // Set the start position to be the Transform position.
            position = transform.position
        });
        
        // To perform transform writes, the body must specify which Transform object to write to!
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
        var groundBody = m_PhysicsWorld.CreateBody();

        var extents = new Vector2(8f, 5f);
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