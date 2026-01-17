using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates how shapes control what they can come into contact with and how to intercept contact processing to stop contacts being created.
/// Press "Play" to see the two bodies/shapes that would normally come into contact with each other, however due to contact filtering, no contact is created.
/// The shapes do continue to contact the surroundings however.
/// See the comments for more information.
/// </summary>
public class PhysicsShapeContactFiltering : MonoBehaviour, PhysicsCallbacks.IContactFilterCallback
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
        var shape1 = body1.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
        var shape2 = body2.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
        
        // For callbacks to be produced, we must enable on one of the shapes.
        // Here we do it for both although for the purposes of this example, this isn't necessary.
        shape1.contactFilterCallbacks = true;
        shape2.contactFilterCallbacks = true;
        
        // To receive callbacks, an object must state which object is the callback target.
        // In this case, it's this script for both shapes.
        // NOTE: "ContactFilterCallbacks" is normally off by default, but we have changed the "Physics LowLevel Settings > Physics World Definition" to default this to being enabled.
        // You can also control this dynamically with "PhysicsWorld.contactFilterCallbacks".
        shape1.callbackTarget = shape2.callbackTarget = this;
    }

    // Called when a pair of shapes are about to come into contact or overlap.
    // Returning false will stop the contact from being created so no further processing will take place.
    // Returning true will allow the contact to be created.
    // NOTE: It is really important to understand that this can be called off the main-thread so it needs to be thread-safe.
    // You should not perform write-operations on the physics system here as it would likely cause corruption or a crash.
    // Also, multiple calls to this method may be called in parallel.
    // For this reason, it's desirable to have simple logic here. For identity, you should consider using the PhysicsUserData (example in this project).
    public bool OnContactFilter2D(PhysicsEvents.ContactFilterEvent contactFilterEvent)
    {
        // Is this circles contacting each other?
        // NOTE: Reading the "shapeType" is a read operation so with these we are good.
        // A write operation such as changing a shape-type (etc.) would be bad.
        var circlesContacting = contactFilterEvent.shapeA.shapeType == PhysicsShape.ShapeType.Circle && contactFilterEvent.shapeB.shapeType == PhysicsShape.ShapeType.Circle;

        // If both the shapes are circles then we ignore it else it's a contact with the ground which we want to allow.
        return !circlesContacting;
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