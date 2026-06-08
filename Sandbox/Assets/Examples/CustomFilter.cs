using Unity.Collections;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrates the use of a custom shape filter.")]
public sealed class CustomFilter : SandboxExampleBehaviour, PhysicsCallbacks.IContactFilterCallback
{
    private bool m_OldContactFilterCallbacks;

    protected override float CameraSize => 17f;
    protected override Vector2 CameraPosition => new(0f, 7f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        // Ensure the contact filter callbacks are on.
        var world = World;
        m_OldContactFilterCallbacks = world.contactFilterCallbacks;
        world.contactFilterCallbacks = true;
    }

    protected override void OnExampleDisable()
    {
        // Restore global default.
        var world = World;
        world.contactFilterCallbacks = m_OldContactFilterCallbacks;
    }

    protected override void SetupScene()
    {
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();
            using var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-20f, 0f),
                new(-20f, 10f),
                new(20f, 10f),
                new(20f, 0f)
            };
            groundBody.CreateChain(new ChainGeometry(vertices.AsArray()), PhysicsChainDefinition.defaultDefinition);
        }

        // Create even/odd bodies.
        {
            var geometry = PolygonGeometry.CreateBox(Vector2.one * 2f, 0.2f);
            var shapeDef = new PhysicsShapeDefinition { contactFilterCallbacks = true };

            for (var n = 0; n < 15; ++n)
            {
                var body = world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, gravityScale = 4f, position = new Vector2(-17.5f + n * 2.5f, 4f) });

                var colorGroup = n % 3;

                // Set the color appropriately.
                shapeDef.surfaceMaterial.customColor = colorGroup switch
                {
                    0 => Color.red,
                    1 => Color.green,
                    2 => Color.blue,
                    _ => default
                };

                var shape = body.CreateShape(geometry, shapeDef);

                // Set the custom data.
                shape.userData = new PhysicsUserData { intValue = colorGroup, boolValue = true };

                // Set the callback target.
                shape.callbackTarget = this;
            }
        }
    }

    // Called when any shapes that are asking for a contact filter potentially come into contact.
    // This must be thread-safe and should NOT perform any write operations to the physics engine!
    public bool OnContactFilter2D(PhysicsEvents.ContactFilterEvent contactFilterEvent)
    {
        // Fetch the user-data from the shapes.
        // NOTE: This is a (thread) safe way to pass custom-data.
        var userData1 = contactFilterEvent.shapeA.userData;
        var userData2 = contactFilterEvent.shapeB.userData;

        // Allow contact if this isn't a filtered shape pair.
        if (!userData1.boolValue || !userData2.boolValue)
            return true;

        // Allow contact if in the same "color group".
        return userData1.intValue == userData2.intValue;
    }
}
