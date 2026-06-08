using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Batching", "Continuously fires batches of projectiles and destroys them on contact using batched body APIs.")]
public sealed class Shooter : SandboxExampleBehaviour
{
    public PhysicsShape.ContactFilter BatchFilter = new PhysicsShape.ContactFilter { categories = 0x2, contacts = ~(ulong)0x2 };

    private int m_BatchCount = 100;
    private float m_BatchDelay = 0.1f;
    private float m_GravityScale = 2f;
    private float m_BatchSpread = 10.0f;
    private Vector2 m_BatchSpeed = new(13f, 32.5f);
    private Vector2 m_BatchSize = new(0.01f, 0.1f);
    private readonly Vector2 m_BatchRadius = new(0.01f, 0.1f);
    private readonly Vector2 m_BatchOffset = new(0.8f, 2f);

    private float m_Time;
    private float m_BatchDelayTime;
    private Vector2 m_FireDirection;
    private Vector2 m_OldGravity;

    protected override float CameraSize => 14f;
    protected override Vector2 CameraPosition => new(0f, 1f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);

        m_OldGravity = World.gravity;

        PhysicsEvents.PreSimulate += OnPreSimulation;
    }

    protected override void OnExampleDisable()
    {
        // Get the default world.
        var world = World;
        world.gravity = m_OldGravity;

        PhysicsEvents.PreSimulate -= OnPreSimulation;
    }

    protected override void SetupOptions()
    {
        // Batch Count.
        AddSliderInt("Batch Count", m_BatchCount, 10, 250, v => m_BatchCount = v);

        // Batch Delay.
        AddSlider("Batch Delay ", m_BatchDelay, 0.1f, 5f, v => m_BatchDelay = v);

        // Batch Spread.
        AddSlider("Batch Spread ", m_BatchSpread, 0f, 360f, v => m_BatchSpread = v);

        // Batch Speed (min/max range).
        var batchSpeed = AddElement(new MinMaxSlider("Batch Speed", m_BatchSpeed.x, m_BatchSpeed.y, 10f, 50f) { focusable = false });
        batchSpeed.RegisterValueChangedCallback(evt => m_BatchSpeed = evt.newValue);

        // Batch Size (min/max range).
        var batchSize = AddElement(new MinMaxSlider("Batch Size", m_BatchSize.x, m_BatchSize.y, 0.01f, 0.5f) { focusable = false });
        batchSize.RegisterValueChangedCallback(evt => m_BatchSize = evt.newValue);

        // Gravity Scale (live; scales world gravity so it affects all bullets immediately).
        AddSlider("Gravity Scale", m_GravityScale, 1f, 5f, v =>
        {
            m_GravityScale = v;
            var world = World;
            world.gravity = m_OldGravity * m_GravityScale;
        });
    }

    protected override void SetupScene()
    {
        var world = World;
        world.gravity = m_OldGravity * m_GravityScale;
        CreateGeometry(world);
    }

    private static void CreateGeometry(PhysicsWorld world)
    {
        // Joint arm shapes use categories=0x100 ("Obstacles" layer).
        // DestroyBatch() targets defaultFilter.categories (=1), so arms are never accidentally destroyed.
        var armFilter = PhysicsShape.ContactFilter.defaultFilter;
        armFilter.categories = 0x100;
        armFilter.contacts   = 0x1 | 0x2;  // contact walls/diamonds and projectiles; not other arms
        var armShapeDef = new PhysicsShapeDefinition
        {
            contactFilter   = armFilter,
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0.6f },
        };
        var armBodyDef = new PhysicsBodyDefinition
        {
            type           = PhysicsBody.BodyType.Dynamic,
            angularDamping = 0.5f,
        };

        // ── Walls ──────────────────────────────────────────────────────────
        {
            var body = world.CreateBody();
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(29f, 1f),  0f, new PhysicsTransform(new Vector2( 0f,  10.4f), PhysicsRotate.identity)));
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f,  22f), 0f, new PhysicsTransform(new Vector2(-14f,  0f),  PhysicsRotate.identity)));
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f,  22f), 0f, new PhysicsTransform(new Vector2( 14f,  0f),  PhysicsRotate.identity)));
        }

        // Kill zone: contactEvents=true feeds DestroyBatch(); categories=0x100 prevents it being
        // treated as a projectile and destroyed itself.
        {
            var killShapeDef = new PhysicsShapeDefinition { contactEvents = true, contactFilter = armFilter };
            world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(0f, -11f) })
                 .CreateShape(PolygonGeometry.CreateBox(new Vector2(29f, 1f)), killShapeDef);
        }

        // ── Diamond obstacles (1×1 box at 45° = diamond) ──────────────────
        {
            var rot45     = new PhysicsRotate(Mathf.PI / 4f);
            var obstacleBody = world.CreateBody();  // single shared static body

            // Left-wall teeth
            foreach (var y in new[] { 8f, 5f, 2f, -1f, -4f, -8f })
                obstacleBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 1f), 0f, new PhysicsTransform(new Vector2(-13.5f, y), rot45)));

            // Right-wall teeth
            foreach (var y in new[] { 8f, 5f, 2f, -1f, -4f, -8f })
                obstacleBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 1f), 0f, new PhysicsTransform(new Vector2(13.5f, y), rot45)));

            // Top row
            foreach (var x in new[] { -11f, -7f, -3f, 0f, 3f, 7f, 11f })
                obstacleBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 1f), 0f, new PhysicsTransform(new Vector2(x, 9.9f), rot45)));

            // Bottom cluster
            foreach (var pos in new Vector2[]
            {
                new(-11f, -10.5f), new(-7f, -10.5f), new(7f, -10.5f), new(11f, -10.5f),
                new(-2.5f, -8f), new(2.5f, -8f), new(0f, -7f),
            })
                obstacleBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 1f), 0f, new PhysicsTransform(pos, rot45)));
        }

        // ── Hinge joints ──────────────────────────────────────────────────
        {
            // TM (0,6), Motor_MM (0,-4): free hinges, horizontal arm.
            var freeCapsule = new CapsuleGeometry { center1 = new Vector2(-2f, 0f), center2 = new Vector2(2f, 0f), radius = 0.1f };
            foreach (var pos in new[] { new Vector2(0f, 6f), new Vector2(0f, -4f) })
            {
                var anchor = world.CreateBody(new PhysicsBodyDefinition { position = pos });
                armBodyDef.position = pos;
                var arm = world.CreateBody(armBodyDef);
                arm.CreateShape(freeCapsule, armShapeDef);
                world.CreateJoint(new PhysicsHingeJointDefinition { bodyA = anchor, bodyB = arm });
            }

            // Spring_BR (8,-6) and Spring_BL (-8,-6): spring hinges, freq=2.
            // BR arm extends left from pivot; BL arm extends right.
            foreach (var (pos, c1, c2) in new (Vector2, Vector2, Vector2)[]
            {
                (new Vector2( 8f, -0.3f), new Vector2(-4f, 0f), Vector2.zero),
                (new Vector2(-8f, -0.3f), Vector2.zero,          new Vector2(4f, 0f)),
            })
            {
                var anchor = world.CreateBody(new PhysicsBodyDefinition { position = pos });
                armBodyDef.position = pos;
                var arm = world.CreateBody(armBodyDef);
                arm.CreateShape(new CapsuleGeometry { center1 = c1, center2 = c2, radius = 0.1f }, armShapeDef);
                world.CreateJoint(new PhysicsHingeJointDefinition { bodyA = anchor, bodyB = arm, enableSpring = true, springFrequency = 2f });
            }
        }

    }

    private void Update()
    {
        // Draw Fire Direction.
        var segment = new SegmentGeometry { point1 = Vector2.zero, point2 = m_FireDirection };
        if (segment.isValid)
        {
            // Get the default world.
            var world = World;
            world.DrawGeometry(segment, PhysicsTransform.identity, Color.azure);
        }
    }

    private void OnPreSimulation(PhysicsWorld world, float timeStep)
    {
        // Destroy batch contacts.
        DestroyBatch();

        // Batch Creation.
        {
            m_Time += Time.deltaTime;
            var rotation1 = new PhysicsRotate(m_Time * 0.5f);
            var rotation2 = new PhysicsRotate(m_Time);
            m_FireDirection = new Vector2(rotation1.direction.x, rotation2.direction.y);
            var fireAngle = PhysicsMath.Atan2(m_FireDirection.y, m_FireDirection.x);

            // Are we ready to create a batch?
            m_BatchDelayTime += Time.deltaTime;
            if (m_BatchDelayTime > m_BatchDelay)
            {
                // Yes, so reset batch delay.
                m_BatchDelayTime = 0f;

                // Create the Batch.
                {
                    ref var random = ref Random;

                    var capsuleRadius = random.NextFloat(m_BatchRadius.x, m_BatchRadius.y);
                    var capsuleLength = capsuleRadius + random.NextFloat(m_BatchSize.x, m_BatchSize.y) * 0.5f;
                    var capsuleGeometry = new CapsuleGeometry
                    {
                        center1 = Vector2.left * capsuleLength,
                        center2 = Vector2.right * capsuleLength,
                        radius = capsuleRadius
                    };

                    var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, fastCollisionsAllowed = true };
                    var shapeDef = new PhysicsShapeDefinition { contactFilter = BatchFilter, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.0f, bounciness = 0.3f } };

                    // Fire all the projectiles.
                    var definitions = new NativeArray<PhysicsBodyDefinition>(m_BatchCount, Allocator.Temp);
                    for (var i = 0; i < m_BatchCount; ++i)
                    {
                        // Calculate the fire spread.
                        var halfSpread = m_BatchSpread * 0.5f;
                        var fireDirection = new PhysicsRotate(math.radians(random.NextFloat(-halfSpread, halfSpread)) + fireAngle).direction;
                        var fireOffset = random.NextFloat(m_BatchOffset.x, m_BatchOffset.y);
                        var fireSpeed = random.NextFloat(m_BatchSpeed.x, m_BatchSpeed.y);

                        // Create the projectile body.
                        bodyDef.position = fireDirection * fireOffset;
                        bodyDef.rotation = new PhysicsRotate(random.NextFloat(-3f, 3f));
                        bodyDef.linearVelocity = fireDirection * fireSpeed;

                        definitions[i] = bodyDef;
                    }

                    // Create the bodies.
                    using var bodies = world.CreateBodyBatch(definitions);

                    // Create the capsules.
                    for (var i = 0; i < m_BatchCount; ++i)
                    {
                        // Create the projectile shape.
                        shapeDef.surfaceMaterial.customColor = ShapeColor;
                        var body = bodies[i];
                        body.CreateShape(capsuleGeometry, shapeDef);
                    }

                    // Dispose.
                    definitions.Dispose();
                }
            }
        }
    }

    private void DestroyBatch()
    {
        // Get the default world.
        var world = World;

        // Fetch hit events and destroy any dynamic bodies in the event.
        var beginEvents = world.contactBeginEvents;

        // Finish if no events.
        if (beginEvents.Length == 0)
            return;

        // Create a list for the batch to destroy.
        var bodyBatch = new NativeList<PhysicsBody>(initialCapacity: beginEvents.Length, Allocator.Temp);

        var targetCategory = BatchFilter.categories;

        // Iterate all the begin events looking for the dynamic bodies.
        foreach (var beginEvent in beginEvents)
        {
            var shapeA = beginEvent.shapeA;
            if (shapeA.isValid && shapeA.contactFilter.categories == targetCategory)
            {
                bodyBatch.Add(shapeA.body);
                continue;
            }

            var shapeB = beginEvent.shapeB;
            if (shapeB.isValid && shapeB.contactFilter.categories == targetCategory)
                bodyBatch.Add(shapeB.body);
        }

        // Destroy the bodies as a batch.
        if (bodyBatch.Length > 0)
            PhysicsBody.DestroyBatch(bodyBatch.AsArray());

        // Dispose.
        bodyBatch.Dispose();
    }
}
