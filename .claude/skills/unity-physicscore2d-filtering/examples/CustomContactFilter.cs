using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Custom contact filtering by per-shape user data. 15 dynamic boxes spawn in three color groups (red/green/blue).
/// Each pair-wise contact is filtered: only same-group boxes collide; cross-group pairs ignore each other.
/// Demonstrates passing identity through PhysicsUserData (thread-safe) rather than reaching back to MonoBehaviours.
/// </summary>
public class CustomContactFilter : MonoBehaviour, PhysicsCallbacks.IContactFilterCallback
{
    private PhysicsWorld m_PhysicsWorld;
    private bool m_OldContactFilterCallbacks;

    private void OnEnable()
    {
        m_PhysicsWorld = PhysicsWorld.defaultWorld;

        // Ensure the world routes contact-filter callbacks. Save the previous value so we restore it on disable.
        m_OldContactFilterCallbacks = m_PhysicsWorld.contactFilterCallbacks;
        m_PhysicsWorld.contactFilterCallbacks = true;

        SetupScene();
    }

    private void OnDisable()
    {
        m_PhysicsWorld.contactFilterCallbacks = m_OldContactFilterCallbacks;
    }

    private void SetupScene()
    {
        // Ground.
        {
            var groundBody = m_PhysicsWorld.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-20f, 0f),
                new(-20f, 10f),
                new(20f, 10f),
                new(20f, 0f)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        // Color-grouped dynamic boxes. Group identity is carried in PhysicsUserData so the filter callback
        // can read it without touching managed state.
        {
            var geometry = PolygonGeometry.CreateBox(Vector2.one * 2f, 0.2f);
            var shapeDef = new PhysicsShapeDefinition { contactFilterCallbacks = true };

            for (var n = 0; n < 15; ++n)
            {
                var body = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition
                {
                    type = PhysicsBody.BodyType.Dynamic,
                    gravityScale = 4f,
                    position = new Vector2(-17.5f + n * 2.5f, 4f)
                });

                var colorGroup = n % 3;

                shapeDef.surfaceMaterial.customColor = colorGroup switch
                {
                    0 => Color.red,
                    1 => Color.green,
                    2 => Color.blue,
                    _ => default
                };

                var shape = body.CreateShape(geometry, shapeDef);

                // intValue carries the group; boolValue marks "participates in custom filter".
                shape.userData = new PhysicsUserData { intValue = colorGroup, boolValue = true };
                shape.callbackTarget = this;
            }
        }
    }

    // Thread-safety: this can run off the main thread and in parallel. No physics writes here.
    public bool OnContactFilter2D(PhysicsEvents.ContactFilterEvent contactFilterEvent)
    {
        var userData1 = contactFilterEvent.shapeA.userData;
        var userData2 = contactFilterEvent.shapeB.userData;

        // Allow contact for any non-participating shape.
        if (!userData1.boolValue || !userData2.boolValue)
            return true;

        // Same color group → contact allowed.
        return userData1.intValue == userData2.intValue;
    }
}
