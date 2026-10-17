using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Scripting.LifecycleManagement;
using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Moves every query-mode bullet and finds its hits in one parallel job per frame.
/// Each bullet sweeps its circle along this frame's displacement with a geometry cast, so nothing can tunnel at any speed.
/// Positions land in a single SetBatchTransform call that also writes the Transforms.
/// </summary>
/// <remarks>
/// Bullets here are static bodies: no velocity, no solver work, no contact events.
/// The cast result carries the surface normal, which the bullet uses to orient its hit sparks.
/// </remarks>
[AutoStaticsCleanup]
public sealed partial class BulletMovementSystem : MonoBehaviour
{
    /// <summary>
    /// The scene's bullet movement system, or null when none is active.
    /// </summary>
    public static BulletMovementSystem instance { get; private set; }

    /// <summary>
    /// The number of bullets currently in flight.
    /// </summary>
    public int activeCount => m_States.IsCreated ? m_States.Length : 0;

    /// <summary>
    /// Registers a bullet for movement and returns its state index.
    /// The bullet must report index changes it is told about and unregister when it dies.
    /// </summary>
    public int Register(Bullet bullet, PhysicsBody body, Vector2 position, Vector2 velocity, float radius)
    {
        PhysicsRotate rotation = default;
        rotation.direction = velocity.normalized;

        m_States.Add(new BulletState
        {
            body = body,
            position = position,
            velocity = velocity,
            rotation = rotation,
            radius = radius
        });
        m_Bullets.Add(bullet);

        return m_States.Length - 1;
    }

    /// <summary>
    /// Removes the bullet at the given state index.
    /// The last entry is swapped into its place and that bullet is told its new index.
    /// </summary>
    public void Unregister(int index)
    {
        if (!m_States.IsCreated || index < 0 || index >= m_States.Length)
            return;

        var lastIndex = m_States.Length - 1;
        m_States.RemoveAtSwapBack(index);
        m_Bullets[index] = m_Bullets[lastIndex];
        m_Bullets.RemoveAt(lastIndex);

        if (index < m_Bullets.Count)
            m_Bullets[index].UpdateMovementIndex(index);
    }

    void OnEnable()
    {
        m_States = new NativeList<BulletState>(512, Allocator.Persistent);
        m_Bullets = new List<Bullet>(512);

        m_Filter = new PhysicsQuery.QueryFilter
        {
            categories = PhysicsLayers.GetLayerMask("Bullet"),

            // Shield circles sit in front of the enemy shapes; the closest cast hit is the shield while it is up, the core once it drops.
            hitCategories = PhysicsLayers.GetLayerMask("Enemy", "EnemyShield")
        };

        m_Camera = Camera.main;

        PhysicsEvents.PreSimulate += OnPreSimulate;
        instance = this;
    }

    void OnDisable()
    {
        if (instance == this)
            instance = null;

        PhysicsEvents.PreSimulate -= OnPreSimulate;

        m_States.Dispose();
        m_Bullets.Clear();
    }

    // Sweeps every bullet, applies all transforms in one batched call, then resolves the hits on the main thread.
    void OnPreSimulate(PhysicsWorld world, float deltaTime)
    {
        if (!world.isDefaultWorld || m_States.Length == 0 || deltaTime <= 0f)
            return;

        // Cull bounds track the live view so resolution and fullscreen changes are always respected.
        m_CullHalfHeight = m_Camera.orthographicSize + m_CullMargin;
        m_CullHalfWidth = m_Camera.orthographicSize * m_Camera.aspect + m_CullMargin;

        var count = m_States.Length;
        var transforms = new NativeArray<PhysicsBody.BatchTransform>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var outcomes = new NativeArray<byte>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var hits = new NativeArray<PhysicsQuery.WorldCastResult>(count, Allocator.TempJob);

        new SweepJob
        {
            world = world,
            deltaTime = deltaTime,
            filter = m_Filter,
            cullHalfWidth = m_CullHalfWidth,
            cullHalfHeight = m_CullHalfHeight,
            states = m_States.AsArray(),
            transforms = transforms,
            outcomes = outcomes,
            hits = hits
            
        }.Schedule(count, 32).Complete();

        // writePoses also writes each body's Transform, so the static bullets render at their new positions.
        PhysicsBody.SetBatchTransform(transforms, writePoses: true);
        transforms.Dispose();

        // Resolve deaths back to front so swap-back removals never disturb an index not yet visited.
        for (var i = count - 1; i >= 0; --i)
        {
            if (outcomes[i] == OutcomeFlying)
                continue;

            var bullet = m_Bullets[i];

            if (outcomes[i] == OutcomeHit)
                bullet.OnQueryHit(Enemy.FromShape(hits[i].shape), hits[i].point, hits[i].normal);
            else
                Destroy(bullet.gameObject);
        }

        outcomes.Dispose();
        hits.Dispose();
    }

    // Everything the job needs to sweep one bullet.
    struct BulletState
    {
        public PhysicsBody body;
        public Vector2 position;
        public Vector2 velocity;
        public PhysicsRotate rotation;
        public float radius;
    }

    // Casts each bullet's circle along its frame displacement and reports where it ends up.
    [BurstCompile]
    struct SweepJob : IJobParallelFor
    {
        [ReadOnly] public PhysicsWorld world;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public PhysicsQuery.QueryFilter filter;
        [ReadOnly] public float cullHalfWidth;
        [ReadOnly] public float cullHalfHeight;
        public NativeArray<BulletState> states;
        [WriteOnly] public NativeArray<PhysicsBody.BatchTransform> transforms;
        [WriteOnly] public NativeArray<byte> outcomes;
        [WriteOnly] public NativeArray<PhysicsQuery.WorldCastResult> hits;

        public void Execute(int index)
        {
            var state = states[index];
            var displacement = state.velocity * deltaTime;

            var geometry = new CircleGeometry { center = state.position, radius = state.radius };
            using var results = world.CastGeometry(geometry, displacement, filter, PhysicsQuery.WorldCastMode.Closest, Allocator.TempJob);

            if (results.Length > 0)
            {
                var hit = results[0];
                state.position = hit.point;
                outcomes[index] = OutcomeHit;
                hits[index] = hit;
            }
            else
            {
                state.position += displacement;
                outcomes[index] = Mathf.Abs(state.position.x) > cullHalfWidth || Mathf.Abs(state.position.y) > cullHalfHeight
                    ? OutcomeOutOfBounds
                    : OutcomeFlying;
            }

            states[index] = state;

            transforms[index] = new PhysicsBody.BatchTransform
            {
                physicsBody = state.body,
                position = state.position,
                rotation = state.rotation
            };
        }
    }

    #region Internal

    // Sweep outcomes per bullet per frame.
    const byte OutcomeFlying = 0;
    const byte OutcomeHit = 1;
    const byte OutcomeOutOfBounds = 2;

    // Distance beyond the view edge at which stray bullets are culled.
    [SerializeField] float m_CullMargin = 2.5f;

    // Per-bullet states and the matching Bullet components, kept index-aligned via swap-back removal.
    NativeList<BulletState> m_States;
    List<Bullet> m_Bullets;
    PhysicsQuery.QueryFilter m_Filter;
    Camera m_Camera;
    float m_CullHalfWidth;
    float m_CullHalfHeight;

    #endregion
}
