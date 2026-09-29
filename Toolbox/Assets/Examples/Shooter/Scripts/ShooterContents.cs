using Unity.Mathematics;
using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Fires a fan of small capsules from the middle of an arena at a steadily turning angle, and removes every capsule that touches the floor, to check how well creating and removing many objects every frame performs.
/// The arena is authored from Physics Poses and Physics Areas, with swinging arms held by Physics Constraint Hinges, and every capsule is one Physics Pose with a Physics Area Capsule, instantiated from an inactive prefab.
/// </summary>
/// <remarks>
/// Each capsule is instantiated inactive and given its position, rotation, velocity, size and color before it is switched on, so its body and shape are created once with everything already set.
/// The capsules are in a contact category of their own so the kill zone along the bottom of the arena can tell them from everything else, and they are removed the moment they touch it.
/// The firing angle turns a little every frame, and the azure line from the middle of the arena shows where the next batch will go.
/// Gravity is scaled while this example is loaded and the world's own gravity is put back when it unloads.
/// </remarks>
public sealed class ShooterContents : MonoBehaviour
{
    /// <summary>
    /// How many capsules are fired in every batch.
    /// </summary>
    public int batchCount
    {
        get => m_BatchCount;
        set => m_BatchCount = value;
    }

    /// <summary>
    /// How long to wait between one batch and the next, in seconds.
    /// </summary>
    public float batchDelay
    {
        get => m_BatchDelay;
        set => m_BatchDelay = value;
    }

    /// <summary>
    /// How wide the fan of capsules in a batch is, in degrees, centered on the firing direction.
    /// </summary>
    public float batchSpread
    {
        get => m_BatchSpread;
        set => m_BatchSpread = value;
    }

    /// <summary>
    /// The slowest and fastest a capsule is fired, in meters per second, as the x and y of the vector.
    /// </summary>
    public Vector2 batchSpeed
    {
        get => m_BatchSpeed;
        set => m_BatchSpeed = value;
    }

    /// <summary>
    /// The shortest and longest a capsule's straight middle section is, in meters, as the x and y of the vector.
    /// </summary>
    public Vector2 batchSize
    {
        get => m_BatchSize;
        set => m_BatchSize = value;
    }

    /// <summary>
    /// How strong gravity is, as a multiple of the world's own gravity, which acts on every capsule straight away.
    /// </summary>
    public float gravityScale
    {
        get => m_GravityScale;
        set
        {
            m_GravityScale = value;

            var world = PhysicsWorld.defaultWorld;
            world.gravity = m_WorldGravity * m_GravityScale;
        }
    }

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;
        m_WorldGravity = world.gravity;
        world.gravity = m_WorldGravity * m_GravityScale;

        m_Random = new Random(RandomSeed);

        m_ProjectileRoot = new GameObject("Projectiles").transform;
        m_ProjectileRoot.SetParent(transform);

        PhysicsEvents.PreSimulate += OnPreSimulate;
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay scaled into whichever example is loaded next.
    private void OnDisable()
    {
        PhysicsEvents.PreSimulate -= OnPreSimulate;

        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;

        if (m_ProjectileRoot != null)
            Destroy(m_ProjectileRoot.gameObject);
    }

    private void Update()
    {
        var segment = new SegmentGeometry { point1 = Vector2.zero, point2 = m_FireDirection };
        if (!segment.isValid)
            return;

        var world = PhysicsWorld.defaultWorld;
        world.DrawGeometry(segment, PhysicsTransform.identity, Color.azure);
    }

    // Removes the capsules that touched the kill zone, then turns the firing direction and fires a new batch once the delay has passed.
    private void OnPreSimulate(PhysicsWorld world, float timeStep)
    {
        if (!world.isDefaultWorld)
            return;

        DestroyBatch(world);

        m_Time += Time.deltaTime;
        var rotation1 = PhysicsRotate.FromRadians(m_Time * 0.5f);
        var rotation2 = PhysicsRotate.FromRadians(m_Time);
        m_FireDirection = new Vector2(rotation1.direction.x, rotation2.direction.y);

        m_BatchDelayTime += Time.deltaTime;
        if (m_BatchDelayTime <= m_BatchDelay)
            return;

        m_BatchDelayTime = 0f;
        FireBatch();
    }

    // Instantiates every capsule of a batch inactive, gives each its own spread, speed, offset, spin and color, then switches it on.
    private void FireBatch()
    {
        if (m_ProjectilePrefab == null)
            return;

        var fireAngle = PhysicsMath.Atan2(m_FireDirection.y, m_FireDirection.x);

        var capsuleRadius = m_Random.NextFloat(BatchRadius.x, BatchRadius.y);
        var capsuleLength = capsuleRadius + m_Random.NextFloat(m_BatchSize.x, m_BatchSize.y) * 0.5f;
        var capsuleGeometry = new CapsuleGeometry
        {
            center1 = Vector2.left * capsuleLength,
            center2 = Vector2.right * capsuleLength,
            radius = capsuleRadius
        };

        var bodyDefinition = m_ProjectilePrefab.GetComponent<PhysicsPose>().definition;
        var shapeDefinition = m_ProjectilePrefab.GetComponent<PhysicsArea>().definition;
        var surfaceMaterial = shapeDefinition.surfaceMaterial;
        var halfSpread = m_BatchSpread * 0.5f;

        for (var i = 0; i < m_BatchCount; ++i)
        {
            var fireDirection = PhysicsRotate.FromRadians(math.radians(m_Random.NextFloat(-halfSpread, halfSpread)) + fireAngle).direction;
            var fireOffset = m_Random.NextFloat(BatchOffset.x, BatchOffset.y);
            var fireSpeed = m_Random.NextFloat(m_BatchSpeed.x, m_BatchSpeed.y);
            var spin = m_Random.NextFloat(-3f, 3f);

            var position = fireDirection * fireOffset;
            var projectile = Instantiate(m_ProjectilePrefab, new Vector3(position.x, position.y, 0f), Quaternion.Euler(0f, 0f, spin * Mathf.Rad2Deg), m_ProjectileRoot);

            bodyDefinition.linearVelocity = fireDirection * fireSpeed;
            projectile.GetComponent<PhysicsPose>().definition = bodyDefinition;

            surfaceMaterial.customColor = RandomColor();
            shapeDefinition.surfaceMaterial = surfaceMaterial;

            var area = projectile.GetComponent<PhysicsAreaCapsule>();
            area.geometry = capsuleGeometry;
            area.definition = shapeDefinition;

            projectile.SetActive(true);
        }
    }

    // Returns a new random bright color.
    private Color RandomColor() => Color.HSVToRGB(m_Random.NextFloat(0f, 1f), m_Random.NextFloat(0.7f, 1f) * SaturationScale, m_Random.NextFloat(0.5f, 1f));

    // Removes every capsule involved in a new contact this step.
    private static void DestroyBatch(PhysicsWorld world)
    {
        foreach (var beginEvent in world.contactBeginEvents)
        {
            var shapeA = beginEvent.shapeA;
            if (shapeA.isValid && shapeA.contactFilter.categories == ProjectileMask)
            {
                RemoveProjectile(shapeA);
                continue;
            }

            var shapeB = beginEvent.shapeB;
            if (shapeB.isValid && shapeB.contactFilter.categories == ProjectileMask)
                RemoveProjectile(shapeB);
        }
    }

    // Removes a capsule by its Physics Pose, since a body a component created can only be removed by removing the component.
    private static void RemoveProjectile(PhysicsShape shape)
    {
        if (shape.body.owner is not PhysicsPose pose)
            return;

        var target = pose.gameObject;
        target.SetActive(false);
        Destroy(target);
    }

    #region Internal

    // The contact category the capsules are in, which the kill zone tells them apart by.
    const ulong ProjectileCategory = 0x2;
    static readonly PhysicsMask ProjectileMask = new() { bitMask = ProjectileCategory };

    // The range of radii a capsule has, and how far from the middle of the arena a capsule is fired from.
    static readonly Vector2 BatchRadius = new(0.01f, 0.1f);
    static readonly Vector2 BatchOffset = new(0.8f, 2f);

    // How washed out the random colors are, and the seed the fired capsules' spread, speed, spin and colors start from.
    const float SaturationScale = 0.65f;
    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_ProjectilePrefab;
    [SerializeField, Range(10, 250)] int m_BatchCount = 100;
    [SerializeField, Range(0.1f, 5f)] float m_BatchDelay = 0.1f;
    [SerializeField, Range(0f, 360f)] float m_BatchSpread = 10f;
    [SerializeField] Vector2 m_BatchSpeed = new(13f, 32.5f);
    [SerializeField] Vector2 m_BatchSize = new(0.01f, 0.1f);
    [SerializeField, Range(1f, 5f)] float m_GravityScale = 2f;

    Random m_Random;
    Transform m_ProjectileRoot;
    Vector2 m_WorldGravity;
    Vector2 m_FireDirection;
    float m_Time;
    float m_BatchDelayTime;

    #endregion
}
