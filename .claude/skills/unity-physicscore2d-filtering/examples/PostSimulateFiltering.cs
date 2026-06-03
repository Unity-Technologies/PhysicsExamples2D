using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Post-simulate contact filtering: instead of suppressing contacts during the step,
/// inspect the contacts produced by the step and react to those that match a custom predicate
/// (here: contacts with near-vertical normals, or impulses above a threshold, or both).
/// Matching contacts are visualised by tinting the *other* shape orange.
/// </summary>
public class PostSimulateFiltering : MonoBehaviour
{
    public enum FilterFunctionMode
    {
        NormalAngle,
        NormalImpulse,
        NormalAngleAndImpulse
    }

    public FilterFunctionMode FilterMode = FilterFunctionMode.NormalAngle;

    private PhysicsBody m_Body;
    private PhysicsShape m_PhysicsShape;

    private void OnEnable()
    {
        // Create a body+shape we'll inspect for post-step contacts.
        var world = PhysicsWorld.defaultWorld;
        m_Body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(0f, 5f) });
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.contactEvents = true; // Required for GetContacts() to return anything.
        m_PhysicsShape = m_Body.CreateShape(PolygonGeometry.CreateBox(Vector2.one), shapeDef);

        PhysicsEvents.PostSimulate += ShowContacts;
    }

    private void OnDisable()
    {
        PhysicsEvents.PostSimulate -= ShowContacts;

        if (m_Body.isValid)
            m_Body.Destroy();
    }

    private void ShowContacts(PhysicsWorld physicsWorld, float deltaTime)
    {
        if (!physicsWorld.isDefaultWorld || !m_PhysicsShape.isValid)
            return;

        using var contacts = m_PhysicsShape.GetContacts();
        if (contacts.Length == 0)
            return;

        ContactExtensions.ContactFilterFunction filterFunction = FilterMode switch
        {
            FilterFunctionMode.NormalAngle => NormalAngleFilter,
            FilterFunctionMode.NormalImpulse => NormalImpulseFilter,
            FilterFunctionMode.NormalAngleAndImpulse => NormalAngleAndImpulseFilter,
            _ => throw new NotImplementedException()
        };

        // Lazy enumeration without allocating a copy. ToFilteredList is the alternative when
        // the result must outlive the source contacts buffer or be iterated multiple times.
        var filteredContacts = contacts.Filter(filterFunction, m_PhysicsShape);

        foreach (var contact in filteredContacts)
        {
            var otherShape = contact.shapeB == m_PhysicsShape ? contact.shapeA : contact.shapeB;
            otherShape.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.darkOrange };
        }
    }

    // Normal is always shape A → shape B; flip if our context shape is shape B so the angle is consistent.
    private static bool NormalAngleFilter(ref PhysicsShape.Contact contact, PhysicsShape shapeContext)
    {
        var manifold = contact.manifold;
        var normal = shapeContext == contact.shapeB ? manifold.normal : -manifold.normal;

        var normalAngle = PhysicsMath.ToDegrees(new PhysicsRotate(normal).radians);
        return normalAngle is > 85f and < 95f;
    }

    private static bool NormalImpulseFilter(ref PhysicsShape.Contact contact, PhysicsShape shapeContext)
    {
        foreach (var point in contact.manifold)
        {
            if (point.totalNormalImpulse > 4.0f)
                return true;
        }

        return false;
    }

    private static bool NormalAngleAndImpulseFilter(ref PhysicsShape.Contact contact, PhysicsShape shapeContext)
    {
        return NormalAngleFilter(ref contact, shapeContext) && NormalImpulseFilter(ref contact, shapeContext);
    }
}

public static class ContactExtensions
{
    /// <summary>
    /// Filter delegate. The shape context is passed so normal-direction filters can compare against
    /// either shape A or shape B's perspective (the manifold's normal always points A → B).
    /// </summary>
    public delegate bool ContactFilterFunction(ref PhysicsShape.Contact contact, PhysicsShape shapeContext);

    public static IEnumerable<PhysicsShape.Contact> Filter(this NativeArray<PhysicsShape.Contact> contacts, ContactFilterFunction filterFunction, PhysicsShape shapeContext = default)
        => new FilteredContacts(contacts, filterFunction, shapeContext);

    public static NativeList<PhysicsShape.Contact> ToFilteredList(this NativeArray<PhysicsShape.Contact> contacts, ContactFilterFunction filterFunction, PhysicsShape shapeContext = default, Allocator allocator = Allocator.Temp)
    {
        var filterList = new NativeList<PhysicsShape.Contact>(initialCapacity: contacts.Length, allocator);

        foreach (var contact in new FilteredContacts(contacts, filterFunction, shapeContext))
            filterList.Add(contact);

        return filterList;
    }

    private struct FilteredContacts : IEnumerable<PhysicsShape.Contact>, IEnumerator<PhysicsShape.Contact>
    {
        public FilteredContacts(NativeArray<PhysicsShape.Contact> contacts, ContactFilterFunction filterFunction, PhysicsShape shapeContext)
        {
            m_Contacts = contacts;
            m_ShapeContext = shapeContext;
            m_FilterFunction = filterFunction;
            m_Index = -1;
        }

        private readonly NativeArray<PhysicsShape.Contact> m_Contacts;
        private readonly PhysicsShape m_ShapeContext;
        private readonly ContactFilterFunction m_FilterFunction;
        private int m_Index;

        object IEnumerator.Current => m_Contacts[m_Index];
        PhysicsShape.Contact IEnumerator<PhysicsShape.Contact>.Current => m_Contacts[m_Index];

        bool IEnumerator.MoveNext()
        {
            while (++m_Index < m_Contacts.Length)
            {
                var contact = m_Contacts[m_Index];
                if (m_FilterFunction(ref contact, m_ShapeContext))
                    return true;
            }
            return false;
        }

        void IEnumerator.Reset() => m_Index = -1;

        public IEnumerator<PhysicsShape.Contact> GetEnumerator() => new FilteredContacts(m_Contacts, m_FilterFunction, m_ShapeContext);
        IEnumerator IEnumerable.GetEnumerator() => new FilteredContacts(m_Contacts, m_FilterFunction, m_ShapeContext);
        public readonly void Dispose() { }
    }
}
