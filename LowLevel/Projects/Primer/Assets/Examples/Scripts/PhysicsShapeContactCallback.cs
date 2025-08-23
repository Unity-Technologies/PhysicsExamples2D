using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates how shapes can request callbacks for when they come into contact with other shapes.
/// Press "Play" to see the two bodies/shapes contact each other.
/// When a shape contacts anything, a point will be drawn at the contact point,
/// See the comments for more information.
/// </summary>
public class PhysicsShapeContactCallback : MonoBehaviour, PhysicsCallbacks.IContactCallback
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
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0.9f }
        };

        // Create two bodies at different positions.
        var body1 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = new Vector2(-0.25f, 2f) });
        var body2 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = new Vector2(0.25f, 4f) });
        
        // Create a shape on each body.
        var shape1 = body1.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
        var shape2 = body2.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
        
        // For callbacks to be produced, contact events must be enabled on one of the shapes.
        // NOTE: Here we're contact the ground so if we were to enable contact events for it, we'd still receive callbacks when hitting the ground, however shape/shape contact would not.
        shape1.contactEvents = true;
        shape2.contactEvents = true;
        
        // To receive callbacks, an object must state which object is the callback target.
        // In this case, it's this script for both shapes.
        // NOTE: "AutoContactCallbacks" is normally off by default but we have changed the "Physics LowLevel Settings > Physics World Definition" to default ot this being enabled.
        // You can also control this dynamically with "PhysicsWorld.autoContactCallbacks".
        shape1.callbackTarget = shape2.callbackTarget = this;
    }

    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        m_PhysicsWorld.Destroy();
    }

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        // Fetch the contact fom the begin event.
        var contact = beginEvent.contactId.contact;

        // Draw all the manifold points.
        foreach (var manifoldPoint in contact.manifold)
            m_PhysicsWorld.DrawPoint(position: manifoldPoint.point, radius: 25f, color: Color.softYellow, lifetime: 10f);
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent)
    {
        // We don't want to perform an action here.
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