using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates how shapes control what they can come into contact with.
/// Press "Play" to see the two bodies/shapes contact each other, bounce and roll, eventually slowing down and stopping.
/// See the comments for more information.
/// </summary>
public class PhysicsShapeContact : MonoBehaviour
{
    private PhysicsWorld m_PhysicsWorld;

    private void OnEnable()
    {
        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create();

        // Create a static area for the shapes to move around in.
        CreateArea();

        // Create a shape definition to set bounciness and the contact filter, controlling what the shape can contact.
        // NOTE: The contact filter we set here is actually the default.
        var shapeDef = new PhysicsShapeDefinition
        {
            // This is actually: PhysicsShape.ContactFilter.defaultFilter;            
            // "categories" is a mask of which categories this shape belongs to (a 64-bit mask i.e. 64 layers).
            // "contacts" is a mask of which categories this shape can contact with (a 64-bit mask i.e. 64 layers).
            contactFilter = new PhysicsShape.ContactFilter { categories = PhysicsMask.One, contacts = PhysicsMask.All },
            
            // The surface material controls many behavioural properties related to when the surface of a shape contacts another shape surface.
            // NOTE: This also includes "customColor" which is for visualisation purposes only.
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0.8f, friction = 0.3f, rollingResistance = 0.1f}
        };

        // Create two bodies at different positions.
        var body1 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-0.25f, 0f) });
        var body2 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(0.25f, 4f) });
        
        // Create a shape on each body.
        body1.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
        body2.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
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