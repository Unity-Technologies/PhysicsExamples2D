using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;

public class ContactFiltering : MonoBehaviour
{
    public enum FilterFunctionMode
    {
        NormalAngle,
        NormalImpulse,
        NormalAngleAndImpulse
    }
    
    public FilterFunctionMode FilterMode = FilterFunctionMode.NormalAngle;
    
    private PhysicsShape m_PhysicsShape;
    
    private void Start()
    {
        // Set the shape context we're interested in.
        m_PhysicsShape = GetComponent<SceneShape>().Shape;
    }

    private void OnEnable()
    {
        // Show the contacts after the simulation has finished.
        PhysicsEvents.PostSimulate += ShowContacts;
    }

    private void OnDisable()
    {
        // We should always unregister physics events.
        PhysicsEvents.PostSimulate -= ShowContacts;
    }

    // Show the filter contacts for the shape.
    private void ShowContacts(PhysicsWorld physicsWorld, float deltaTime)
    {
        // Not strictly needed here but let's be safe if we experiment.
        if (!physicsWorld.isDefaultWorld || !m_PhysicsShape.isValid)
            return;

        // Fetch the contacts for the shape, early-out if there are none.
        using var contacts = m_PhysicsShape.GetContacts();
        if (contacts.Length == 0)
            return;

        // Select the filter function we selected in the script.
        ContactExtensions.ContactFilterFunction filterFunction = FilterMode switch
        {
            FilterFunctionMode.NormalAngle => NormalAngleFilter,
            FilterFunctionMode.NormalImpulse => NormalImpulseFilter,
            FilterFunctionMode.NormalAngleAndImpulse => NormalAngleAndImpulseFilter,
            _ => throw new NotImplementedException()
        };

        // We can filter contacts by enumerating them or creating a copy as a new list.
        // We'd likely create a new list if we intend to iterate many times or if we wanted the list lifetime to last beyond the original contacts array we have.
        //
        // NOTES:
        // - The filter function can be a simple lambda expression too.
        // - The shape context "m_PhysicsShape" is optional when filtering. Filtering by normal might require it.
#if true
        var filteredContacts = contacts.Filter(filterFunction, m_PhysicsShape);
#else
        // Create a list of filtered contacts with a filter function.
        // NOTE: This is a new list as a copy of the original contacts therefore needs disposing.
        using var filteredContacts = contacts.ToFilteredList(filterFunction, m_PhysicsShape);
#endif
        
        // Show the filtered contacts by changing the shape color.
        foreach (var contact in filteredContacts)
        {
            var otherShape = contact.shapeB == m_PhysicsShape ? contact.shapeA : contact.shapeB;
            otherShape.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.darkOrange };
        }
    }

    // Example filter that checks if the normal angle is within a set range.
    private static bool NormalAngleFilter(ref PhysicsShape.Contact contact, PhysicsShape shapeContext)
    {
        // Fetch the normal.
        // NOTE: Normal is always in the direction of shape A to shape B so always ensure we're referring to it in context.
        var manifold = contact.manifold;
        var normal = shapeContext == contact.shapeB ? manifold.normal : -manifold.normal;
        
        // Filter the normal.
        var normalAngle = PhysicsMath.ToDegrees(new PhysicsRotate(normal).angle);
        return normalAngle is > 85f and < 95f;
    }

    // Example filter that checks if the normal impulse is above a threshold.
    private static bool NormalImpulseFilter(ref PhysicsShape.Contact contact, PhysicsShape shapeContext)
    {
        var manifold = contact.manifold;
        foreach (var point in manifold)
        {
            if (point.totalNormalImpulse > 4.0f)
                return true;
        }

        return false;
    }

    // Example filter that checks both the normal angle and impulse threshold.
    private static bool NormalAngleAndImpulseFilter(ref PhysicsShape.Contact contact, PhysicsShape shapeContext)
    {
        return NormalAngleFilter(ref contact, shapeContext) && NormalImpulseFilter(ref contact, shapeContext);
    }
}
