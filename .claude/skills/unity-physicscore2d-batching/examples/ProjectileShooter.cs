using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Batched projectile spawner + batched cleanup. Every `BatchDelay` seconds, builds an array of
/// `PhysicsBodyDefinition`s and creates the projectile bodies via `world.CreateBodyBatch(definitions)` (one
/// batched call vs. N individual `CreateBody` calls). On every step's `PreSimulate`, scans
/// `world.contactBeginEvents` for projectile-mask hits and tears down the matched bodies via
/// `PhysicsBody.DestroyBatch`.
///
/// Pattern: build a batch up-front, create-batch in one call; collect destruction targets, destroy-batch
/// in one call. The CreateBatch / DestroyBatch APIs are the bulk-mutate counterparts to the per-call
/// world.CreateBody / body.Destroy.
/// </summary>
public class ProjectileShooter : MonoBehaviour
{
    public PhysicsShape.ContactFilter BatchFilter = PhysicsShape.ContactFilter.defaultFilter;
    public int BatchCount = 100;
    public float BatchDelay = 0.1f;
    public float GravityScale = 2f;
    public float BatchSpread = 10.0f;
    public Vector2 BatchSpeedRange = new Vector2(10f, 25f);
    public Vector2 BatchSizeRange = new Vector2(0.01f, 0.1f);
    public uint RandomSeed = 1234;

    private readonly Vector2 m_BatchRadiusRange = new Vector2(0.01f, 0.1f);
    private readonly Vector2 m_BatchOffsetRange = new Vector2(0.8f, 2f);

    private float m_Time;
    private float m_BatchDelayTime;
    private Vector2 m_FireDirection;
    private Random m_Random;

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);
        PhysicsEvents.PreSimulate += OnPreSimulation;
    }

    private void OnDisable()
    {
        PhysicsEvents.PreSimulate -= OnPreSimulation;
    }

    private void Update()
    {
        // Visualize current fire direction.
        var segment = new SegmentGeometry { point1 = Vector2.zero, point2 = m_FireDirection };
        if (segment.isValid)
            PhysicsWorld.defaultWorld.DrawGeometry(segment, PhysicsTransform.identity, Color.azure);
    }

    private void OnPreSimulation(PhysicsWorld world, float timeStep)
    {
        DestroyBatch(world);

        m_Time += Time.deltaTime;
        var rotation1 = PhysicsRotate.FromRadians(m_Time * 0.5f);
        var rotation2 = PhysicsRotate.FromRadians(m_Time);
        m_FireDirection = new Vector2(rotation1.direction.x, rotation2.direction.y);
        var fireAngle = PhysicsMath.Atan2(m_FireDirection.y, m_FireDirection.x);

        m_BatchDelayTime += Time.deltaTime;
        if (m_BatchDelayTime <= BatchDelay)
            return;

        m_BatchDelayTime = 0f;

        var capsuleRadius = m_Random.NextFloat(m_BatchRadiusRange.x, m_BatchRadiusRange.y);
        var capsuleLength = capsuleRadius + m_Random.NextFloat(BatchSizeRange.x, BatchSizeRange.y) * 0.5f;
        var capsuleGeometry = new CapsuleGeometry
        {
            center1 = Vector2.left * capsuleLength,
            center2 = Vector2.right * capsuleLength,
            radius = capsuleRadius
        };

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, gravityScale = GravityScale, fastCollisionsAllowed = true };
        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = BatchFilter,
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f, bounciness = 0.3f }
        };

        // Build all definitions, then create-batch in one call.
        var definitions = new NativeArray<PhysicsBodyDefinition>(BatchCount, Allocator.Temp);
        for (var i = 0; i < BatchCount; ++i)
        {
            var halfSpread = BatchSpread * 0.5f;
            var fireDirection = PhysicsRotate.FromRadians(math.radians(m_Random.NextFloat(-halfSpread, halfSpread)) + fireAngle).direction;
            var fireOffset = m_Random.NextFloat(m_BatchOffsetRange.x, m_BatchOffsetRange.y);
            var fireSpeed = m_Random.NextFloat(BatchSpeedRange.x, BatchSpeedRange.y);

            bodyDef.position = fireDirection * fireOffset;
            bodyDef.rotation = PhysicsRotate.FromRadians(m_Random.NextFloat(-3f, 3f));
            bodyDef.linearVelocity = fireDirection * fireSpeed;

            definitions[i] = bodyDef;
        }

        // One batched create call. The returned NativeArray is owned by the user — dispose when done.
        using var bodies = world.CreateBodyBatch(definitions);

        // Per-body shape attachment is still serial — only body creation has a batch API at present.
        for (var i = 0; i < BatchCount; ++i)
            bodies[i].CreateShape(capsuleGeometry, shapeDef);

        definitions.Dispose();
    }

    private void DestroyBatch(PhysicsWorld world)
    {
        var beginEvents = world.contactBeginEvents;
        if (beginEvents.Length == 0)
            return;

        var bodyBatch = new NativeList<PhysicsBody>(initialCapacity: beginEvents.Length, Allocator.Temp);
        var targetCategory = BatchFilter.categories;

        // Collect all projectile-mask bodies that contacted anything this step.
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

        // One batched destroy call vs. N individual body.Destroy() calls.
        if (bodyBatch.Length > 0)
            PhysicsBody.DestroyBatch(bodyBatch.AsArray());

        bodyBatch.Dispose();
    }
}
