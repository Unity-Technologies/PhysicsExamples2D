using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Moves every registered enemy along its analytic path in one parallel job per frame.
/// The job runs in the pre-simulation callback and the results land in a single <see cref="PhysicsBody.SetBatchTransform"/> call.
/// </summary>
/// <remarks>
/// Enemy bodies are static; this system is the only thing that moves them, and the batch call also writes their Transforms.
/// Wave paths are spawn point + constant inward progress + a lateral oscillation on the tangent; boss orbit paths circle the player with a radial undulation.
/// Both are pure functions of time, and the per-enemy MonoBehaviours do no per-frame work at all.
/// </remarks>
public sealed class WaveMovementSystem : MonoBehaviour
{
    /// <summary>
    /// The number of enemies currently being moved.
    /// </summary>
    public int activeCount => m_States.IsCreated ? m_States.Length : 0;

    /// <summary>
    /// Global multiplier on enemy movement, captured by each enemy as it registers.
    /// It scales the inward speed and the oscillation and spin rates together, so paths keep their shape and just play out slower or faster.
    /// </summary>
    public float speedScale
    {
        get => m_SpeedScale;
        set => m_SpeedScale = Mathf.Clamp(value, 0.1f, 2f);
    }

    /// <summary>
    /// Registers an enemy for movement and returns its state index.
    /// The enemy must report index changes it is told about and unregister when it dies.
    /// </summary>
    public int Register(Enemy enemy, PhysicsBody body, Vector2 spawnPosition, WaveDefinition wave)
    {
        var inward = -spawnPosition.normalized;

        // The global speed scale multiplies every time-based rate, which is equivalent to uniformly compressing this enemy's timeline.
        m_States.Add(new MovementState
        {
            body = body,
            spawnPosition = spawnPosition,
            inwardDirection = inward,
            speed = wave.speed * m_SpeedScale,
            amplitude = wave.amplitude,
            frequency = wave.frequency * m_SpeedScale,
            spinRadians = wave.spinDegreesPerSecond * Mathf.Deg2Rad * m_SpeedScale,
            phase = UnityEngine.Random.value,
            pattern = (int)wave.pattern,
            startTime = Time.time,
            maxDistance = Mathf.Max(0f, spawnPosition.magnitude - StopRadius)
        });
        m_Enemies.Add(enemy);

        return m_States.Length - 1;
    }

    /// <summary>
    /// One boss element's orbit around the player: where on the circle it starts, how it circles, and how its radius undulates.
    /// Angles are in radians and the angular speed is signed, so its sign picks the orbit direction.
    /// </summary>
    public struct OrbitPath
    {
        /// <summary>
        /// The angle on the circle the element starts at, in radians.
        /// </summary>
        public float startAngle;

        /// <summary>
        /// The base distance from the player the element orbits at.
        /// </summary>
        public float radius;

        /// <summary>
        /// How fast the element circles, in radians per second; negative values orbit the other way.
        /// </summary>
        public float angularSpeed;

        /// <summary>
        /// How far the radius swings either side of its base value.
        /// </summary>
        public float amplitude;

        /// <summary>
        /// The radial undulation rate, in cycles per second.
        /// </summary>
        public float frequency;

        /// <summary>
        /// The undulation phase offset, in radians; stepped per element so the ripple travels down the chain.
        /// </summary>
        public float phase;

        /// <summary>
        /// Constant spin in radians per second; zero means the element faces its travel direction instead.
        /// </summary>
        public float spinRadians;
    }

    /// <summary>
    /// Registers a boss element to orbit the player along the given path and returns its state index.
    /// The element must report index changes it is told about and unregister when it dies, exactly like a wave enemy.
    /// </summary>
    public int RegisterOrbit(Enemy enemy, PhysicsBody body, in OrbitPath path)
    {
        m_States.Add(new MovementState
        {
            body = body,
            speed = path.angularSpeed * m_SpeedScale,
            amplitude = path.amplitude,
            frequency = path.frequency * m_SpeedScale,
            spinRadians = path.spinRadians * m_SpeedScale,
            phase = path.phase,
            pattern = OrbitPattern,
            startTime = Time.time,
            startAngle = path.startAngle,
            orbitRadius = path.radius
        });
        m_Enemies.Add(enemy);

        return m_States.Length - 1;
    }

    /// <summary>
    /// Removes the enemy at the given state index.
    /// The last entry is swapped into its place and that enemy is told its new index.
    /// </summary>
    public void Unregister(int index)
    {
        if (!m_States.IsCreated || index < 0 || index >= m_States.Length)
            return;

        var lastIndex = m_States.Length - 1;
        m_States.RemoveAtSwapBack(index);
        m_Enemies[index] = m_Enemies[lastIndex];
        m_Enemies.RemoveAt(lastIndex);

        if (index < m_Enemies.Count)
            m_Enemies[index].UpdateMovementIndex(index);
    }

    void Awake() => GameSettings.RegisterSlider(this, GameSettings.GameTab, "Enemy Speed", 0.1f, 2f, () => speedScale, v => speedScale = v);

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable()
    {
        m_States = new NativeList<MovementState>(256, Allocator.Persistent);
        m_Enemies = new List<Enemy>(256);

        PhysicsEvents.PreSimulate += OnPreSimulate;
    }

    void OnDisable()
    {
        PhysicsEvents.PreSimulate -= OnPreSimulate;

        m_States.Dispose();
        m_Enemies.Clear();
    }

    // Runs the movement job and pushes every enemy transform to the engine in one batched call.
    void OnPreSimulate(PhysicsWorld world, float deltaTime)
    {
        if (!world.isDefaultWorld || m_States.Length == 0)
            return;

        var count = m_States.Length;
        var transforms = new NativeArray<PhysicsBody.BatchTransform>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        new MovementJob
        {
            states = m_States.AsArray(),
            time = Time.time,
            transforms = transforms
        }.Schedule(count, 32).Complete();

        // writePoses also writes each body's Transform in the same call, so static bodies render correctly with no body-update events.
        PhysicsBody.SetBatchTransform(transforms, writePoses: true);
        transforms.Dispose();
    }

    // Everything the job needs to place one enemy, captured at registration.
    // One struct serves both movement kinds; pattern selects which, and the fields belonging to the other kind stay at zero.
    struct MovementState
    {
        // Shared by both kinds.
        public PhysicsBody body;
        public float amplitude;
        public float frequency;
        public float spinRadians;
        public float phase;
        public int pattern;
        public float startTime;

        // The inward travel speed for a wave enemy, or the signed angular speed for a boss orbit.
        public float speed;

        // Wave enemies only: constant inward travel from the spawn point, parked once maxDistance is reached.
        public Vector2 spawnPosition;
        public Vector2 inwardDirection;
        public float maxDistance;

        // Boss orbit only: a circle of orbitRadius starting at startAngle.
        public float startAngle;
        public float orbitRadius;
    }

    // Evaluates each enemy's analytic path at the current time and emits its batch transform.
    [BurstCompile]
    struct MovementJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<MovementState> states;
        [ReadOnly] public float time;
        [WriteOnly] public NativeArray<PhysicsBody.BatchTransform> transforms;

        public void Execute(int index)
        {
            var s = states[index];
            var t = math.max(0f, time - s.startTime);

            Vector2 position;
            Vector2 travel;

            if (s.pattern == OrbitPattern)
            {
                // The boss orbit: a circle around the player whose radius undulates, so the ripple travels down the chain via each element's phase.
                var angle = s.startAngle + s.speed * t;
                var undulation = math.TAU * s.frequency * t + s.phase;
                var radius = s.orbitRadius + s.amplitude * math.sin(undulation);

                var outward = new Vector2(math.cos(angle), math.sin(angle));
                var orbitTangent = new Vector2(-outward.y, outward.x);
                position = outward * radius;

                // Analytic velocity: the tangential circling plus the radial undulation rate.
                var radialRate = s.amplitude * math.TAU * s.frequency * math.cos(undulation);
                travel = orbitTangent * (s.speed * radius) + outward * radialRate;
            }
            else
            {
                // Constant inward progress, clamped so the enemy parks at the player's edge.
                var along = math.min(s.speed * t, s.maxDistance);

                // Lateral offset on the tangent, and its time derivative for facing.
                var lateral = 0f;
                var lateralRate = 0f;

                switch (s.pattern)
                {
                    case (int)WaveDefinition.MovementPattern.Sawtooth:
                    {
                        var x = math.frac(s.frequency * t + s.phase);
                        lateral = s.amplitude * (4f * math.abs(x - 0.5f) - 1f);
                        lateralRate = s.amplitude * 4f * s.frequency * (x < 0.5f ? -1f : 1f);
                        break;
                    }

                    case (int)WaveDefinition.MovementPattern.Sine:
                    {
                        var x = math.TAU * (s.frequency * t + s.phase);
                        lateral = s.amplitude * math.sin(x);
                        lateralRate = s.amplitude * math.TAU * s.frequency * math.cos(x);
                        break;
                    }
                }

                var tangent = new Vector2(-s.inwardDirection.y, s.inwardDirection.x);
                position = s.spawnPosition + s.inwardDirection * along + tangent * lateral;
                travel = along < s.maxDistance ? s.inwardDirection * s.speed + tangent * lateralRate : s.inwardDirection;
            }

            // Spinning enemies rotate constantly; others face their instantaneous travel direction (the player, once a wave enemy parks).
            PhysicsRotate rotation;

            if (s.spinRadians != 0f)
            {
                rotation = PhysicsRotate.FromRadians(s.spinRadians * t + s.phase * math.TAU);
            }
            else
            {
                rotation = default;
                rotation.direction = travel.sqrMagnitude > 1e-6f ? travel.normalized : Vector2.up;
            }

            transforms[index] = new PhysicsBody.BatchTransform
            {
                physicsBody = s.body,
                position = position,
                rotation = rotation
            };
        }
    }

    #region Internal

    // How close to the player's center an enemy may advance; kamikaze detonation triggers well before this.
    const float StopRadius = 0.15f;

    // The pattern id marking a boss orbit state, distinct from every WaveDefinition.MovementPattern value.
    const int OrbitPattern = -1;

    // Global movement multiplier, applied at registration so in-flight enemies never jump.
    [SerializeField]
    [Range(0.1f, 2f)]
    float m_SpeedScale = 1f;

    // Per-enemy movement states and the matching Enemy components, kept index-aligned via swap-back removal.
    NativeList<MovementState> m_States;
    List<Enemy> m_Enemies;

    #endregion
}
