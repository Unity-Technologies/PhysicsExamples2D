using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates how shapes control what they can come into contact with and how to intercept contact processing
/// to stop contacts being created. Two dynamic circle bodies fall into a chain-bounded area; their contacts with
/// the ground proceed normally but the contact between the two circles themselves is filtered out.
/// </summary>
public class BasicContactFiltering : MonoBehaviour, PhysicsCallbacks.IContactFilterCallback
{
    private PhysicsWorld m_PhysicsWorld;

    private void OnEnable()
    {
        // Create our own world for this isolated demo.
        m_PhysicsWorld = PhysicsWorld.Create();

        CreateArea();

        // Shape definition: contactFilter controls *who* this shape can touch (by category bits);
        // surfaceMaterial controls *how* the touch behaves (bounce/friction/etc).
        var shapeDef = new PhysicsShapeDefinition
        {
            // Default filter: belongs to category 1, contacts every category.
            contactFilter = new PhysicsShape.ContactFilter { categories = PhysicsMask.One, contacts = PhysicsMask.All },
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0.8f, friction = 0.3f, rollingResistance = 0.1f }
        };

        var body1 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(-0.25f, 0f) });
        var body2 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, position = new Vector2(0.25f, 4f) });

        var shape1 = body1.CreateShape(CircleGeometry.defaultGeometry, shapeDef);
        var shape2 = body2.CreateShape(CircleGeometry.defaultGeometry, shapeDef);

        // Per-shape opt-in for the filter callback. Both shapes opt in here, but enabling on one is sufficient.
        shape1.contactFilterCallbacks = true;
        shape2.contactFilterCallbacks = true;

        // Direct callbacks at this MonoBehaviour. The world-level contactFilterCallbacks flag must also be on
        // (configured via Physics LowLevel Settings or PhysicsWorld.contactFilterCallbacks at runtime).
        shape1.callbackTarget = shape2.callbackTarget = this;
    }

    // IMPORTANT: This callback can fire off the main thread and in parallel.
    // - Read-only access to physics state is safe; do NOT perform writes.
    // - Keep logic simple; for identity, prefer PhysicsUserData over reflection.
    public bool OnContactFilter2D(PhysicsEvents.ContactFilterEvent contactFilterEvent)
    {
        var circlesContacting =
            contactFilterEvent.shapeA.shapeType == PhysicsShape.ShapeType.Circle &&
            contactFilterEvent.shapeB.shapeType == PhysicsShape.ShapeType.Circle;

        // Returning false suppresses the contact entirely (no collision, no events).
        return !circlesContacting;
    }

    private void OnDisable()
    {
        // Destroying a world destroys all its contents.
        if (m_PhysicsWorld.isValid)
            m_PhysicsWorld.Destroy();
    }

    private void CreateArea()
    {
        var groundBody = m_PhysicsWorld.CreateBody();

        var extents = new Vector2(8f, 5f);
        using var extentPoints = new NativeList<Vector2>(Allocator.Temp)
        {
            new(-extents.x, extents.y),
            new(extents.x, extents.y),
            new(extents.x, -extents.y),
            new(-extents.x, -extents.y)
        };

        groundBody.CreateChain(
            geometry: new ChainGeometry(extentPoints.AsArray()),
            definition: PhysicsChainDefinition.defaultDefinition);
    }
}
