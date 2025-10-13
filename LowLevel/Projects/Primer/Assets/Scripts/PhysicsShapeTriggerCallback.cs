using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates how shapes can request callbacks for when they overlap as a trigger with other shapes.
/// Press "Play" to see the two bodies/shapes overlap each other.
/// When a shape overlaps anything, it will change color.
/// See the comments for more information.
/// </summary>
public class PhysicsShapeTriggerCallback : MonoBehaviour, PhysicsCallbacks.ITriggerCallback
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

        // Create a body and a non-trigger shape.
        // NOTE: The term visitor here matches the terminology used in trigger events where there's a trigger and visitor shape pair involved in the event.
        var visitorBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic, position = new Vector2(0f, 4f) });
        var visitorShape = visitorBody.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
        
        // Create a body and a trigger shape.
        var triggerBody = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Static, position = new Vector2(0f, -4f) });
        // Change the shape definition to specific a trigger. 
        shapeDef.isTrigger = true;
        var triggerShape = triggerBody.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
        
        // For callbacks to be produced, trigger events must be enabled on BOTH of the shapes.
        visitorShape.triggerEvents = true;
        triggerShape.triggerEvents = true;
        
        // To receive callbacks, an object must state which object is the callback target.
        // In this case, it's this script for both shapes.
        // NOTE: "AutoTriggerCallbacks" is normally off by default, but we have changed the "Physics LowLevel Settings > Physics World Definition" to default this to being enabled.
        // You can also control this dynamically with "PhysicsWorld.autoTriggerCallbacks".
        visitorShape.callbackTarget = triggerShape.callbackTarget = this;
    }

    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        m_PhysicsWorld.Destroy();
    }

    // Called when a pair of shapes begin to overlap.
    public void OnTriggerBegin2D(PhysicsEvents.TriggerBeginEvent beginEvent)
    {
        var visitorShape = beginEvent.visitorShape;

        // Fetch the surface material and change the custom color.
        var surfaceMaterial = visitorShape.surfaceMaterial;
        surfaceMaterial.customColor = Color.hotPink;
        visitorShape.surfaceMaterial = surfaceMaterial;
    }

    // Called when a pair of shapes have ended overlapping.
    public void OnTriggerEnd2D(PhysicsEvents.TriggerEndEvent endEvent)
    {
        var visitorShape = endEvent.visitorShape;
        
        // Fetch the surface material and change the custom color.
        // A custom color of (0,0,0,0) i.e. "Color.Clear" will remove the custom color and restore default color rendering which uses the body/shape state to select colors.
        var surfaceMaterial = visitorShape.surfaceMaterial;
        surfaceMaterial.customColor = Color.clear;
        visitorShape.surfaceMaterial = surfaceMaterial;
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