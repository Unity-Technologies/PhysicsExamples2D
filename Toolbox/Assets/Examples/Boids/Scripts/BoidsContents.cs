using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Flocks thousands of small triangles inside a circle, every one steering by its neighbors, with all the boids worked out together in a parallel Burst job and moved with batched body calls.
/// Every boid is one kinematic Physics Pose with a Physics Area Polygon, instantiated from an inactive prefab, and the flock is moved by taking the body from every pose and using the batched body calls on all of them.
/// </summary>
/// <remarks>
/// Each boid steers away from boids that are too close, toward the middle of the boids it can see, and toward their average heading, then is clamped to the maximum speed, and boids also steer away from the pointer.
/// A boid that leaves the circle either wraps to the opposite side or is turned back inward, and boids in different groups ignore each other.
/// This is based on the flocking described at https://www.red3d.com/cwr/ and http://www.kfish.org/boids/pseudocode.html.
/// </remarks>
public sealed class BoidsContents : MonoBehaviour
{
    /// <summary>
    /// Removes every boid and creates a new flock with the current settings.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        if (m_BoidPrefab == null)
            return;

        var random = new Random(RandomSeed);

        m_BoidRoot = new GameObject("Boids").transform;
        m_BoidRoot.SetParent(transform);

        var vertices = new NativeList<Vector2>(Allocator.Temp)
        {
            new(m_BoidSize * 0.5f, 0f),
            new(m_BoidSize * -0.5f, m_BoidSize * 0.5f),
            new(m_BoidSize * -0.5f, m_BoidSize * -0.5f)
        };

        var geometry = PolygonGeometry.Create(vertices.AsArray());
        vertices.Dispose();

        // The boids are kinematic because the simulation then moves each one along its own velocity.
        var poseDefinition = m_BoidPrefab.GetComponent<PhysicsPose>().definition;
        var shapeDefinition = m_BoidPrefab.GetComponent<PhysicsArea>().definition;
        var surfaceMaterial = shapeDefinition.surfaceMaterial;

        // Every group has a color of its own, and without groups every boid does.
        var boidGroupColors = default(NativeArray<Color>);

        if (m_BoidGroups)
        {
            boidGroupColors = new NativeArray<Color>(BoidGroupCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < BoidGroupCount; ++i)
                boidGroupColors[i] = RandomColor(ref random);
        }

        m_GroupIndices = new NativeArray<int>(m_BoidCount, Allocator.Persistent);
        var poses = new PhysicsPose[m_BoidCount];
        var maxRotation = PhysicsMath.TAU;

        for (var i = 0; i < m_BoidCount; ++i)
        {
            var boidGroupIndex = m_BoidGroups ? random.NextInt(0, BoidGroupCount) : 0;
            m_GroupIndices[i] = boidGroupIndex;

            surfaceMaterial.customColor = m_BoidGroups ? boidGroupColors[boidGroupIndex] : RandomColor(ref random);
            shapeDefinition.surfaceMaterial = surfaceMaterial;

            var radians = random.NextFloat(0f, maxRotation);
            var rotation = PhysicsRotate.FromRadians(radians);
            var position = PhysicsRotate.FromRadians(random.NextFloat(0f, maxRotation)).direction * random.NextFloat(m_BoidBounds.radius * 0.1f, m_BoidBounds.radius * 0.9f);

            // Each boid is instantiated inactive and given its place, heading, velocity and color before it is switched on, so its body is created once with them.
            var boid = Instantiate(m_BoidPrefab, new Vector3(position.x, position.y, 0f), Quaternion.Euler(0f, 0f, radians * Mathf.Rad2Deg), m_BoidRoot);

            poseDefinition.linearVelocity = rotation.direction * random.NextFloat(m_MaxSpeed * 0.5f, m_MaxSpeed);

            var pose = boid.GetComponent<PhysicsPose>();
            pose.definition = poseDefinition;

            var area = boid.GetComponent<PhysicsAreaPolygon>();
            area.geometry = geometry;
            area.definition = shapeDefinition;

            boid.SetActive(true);
            poses[i] = pose;
        }

        if (boidGroupColors.IsCreated)
            boidGroupColors.Dispose();

        // The batch calls work on bodies, so the body of every boid's pose is gathered into one array.
        m_BoidBodies = new NativeArray<PhysicsBody>(m_BoidCount, Allocator.Persistent);

        for (var i = 0; i < m_BoidCount; ++i)
            m_BoidBodies[i] = poses[i].body;
    }

    /// <summary>
    /// Removes every boid.
    /// </summary>
    public void Clear()
    {
        // The boids are switched off first so their bodies are gone at once, rather than at the end of the frame.
        if (m_BoidRoot != null)
        {
            m_BoidRoot.gameObject.SetActive(false);
            Destroy(m_BoidRoot.gameObject);
            m_BoidRoot = null;
        }

        if (m_BoidBodies.IsCreated)
            m_BoidBodies.Dispose();

        if (m_BoidStates.IsCreated)
            m_BoidStates.Dispose();

        if (m_GroupIndices.IsCreated)
            m_GroupIndices.Dispose();
    }

    /// <summary>
    /// Gives the flock the Toolbox, which tells it how long each step is when the trails are drawn.
    /// </summary>
    /// <param name="toolbox">The Toolbox the example is loaded in.</param>
    public void SetToolbox(ToolboxManager toolbox) => m_Toolbox = toolbox;

    /// <summary>
    /// How many boids are in the flock.
    /// Changing this does not rebuild the flock until <see cref="Rebuild"/> is called.
    /// </summary>
    public int boidCount
    {
        get => m_BoidCount;
        set => m_BoidCount = value;
    }

    /// <summary>
    /// How big each boid is, in meters.
    /// Changing this does not rebuild the flock until <see cref="Rebuild"/> is called.
    /// </summary>
    public float boidSize
    {
        get => m_BoidSize;
        set => m_BoidSize = value;
    }

    /// <summary>
    /// The fastest a boid can move, in meters per second.
    /// </summary>
    public float maxSpeed
    {
        get => m_MaxSpeed;
        set => m_MaxSpeed = value;
    }

    /// <summary>
    /// How far a boid can see other boids, in meters, which decides who it flocks with.
    /// </summary>
    public float sightRadius
    {
        get => m_SightRadius;
        set => m_SightRadius = value;
    }

    /// <summary>
    /// How close another boid can be, in meters, before a boid steers away from it.
    /// </summary>
    public float separationRadius
    {
        get => m_SeparationRadius;
        set => m_SeparationRadius = value;
    }

    /// <summary>
    /// How strongly a boid steers away from boids that are too close.
    /// </summary>
    public float separationStrength
    {
        get => m_SeparationStrength;
        set => m_SeparationStrength = value;
    }

    /// <summary>
    /// How strongly a boid steers toward the middle of the boids it can see.
    /// </summary>
    public float cohesionStrength
    {
        get => m_CohesionStrength;
        set => m_CohesionStrength = value;
    }

    /// <summary>
    /// How strongly a boid steers toward the average heading of the boids it can see.
    /// </summary>
    public float alignmentStrength
    {
        get => m_AlignmentStrength;
        set => m_AlignmentStrength = value;
    }

    /// <summary>
    /// The radius of the circle the boids stay inside, in meters.
    /// </summary>
    public float boundsRadius
    {
        get => m_BoundsRadius;
        set
        {
            m_BoundsRadius = value;
            m_BoidBounds = new CircleGeometry { radius = m_BoundsRadius };
        }
    }

    /// <summary>
    /// Whether a boid that leaves the circle wraps to the opposite side, instead of being turned back inward.
    /// </summary>
    public bool boundsWrap
    {
        get => m_BoidBoundsWrap;
        set => m_BoidBoundsWrap = value;
    }

    /// <summary>
    /// Whether the boids are split into groups that only flock with their own group, each drawn in its own color.
    /// Changing this does not rebuild the flock until <see cref="Rebuild"/> is called.
    /// </summary>
    public bool boidGroups
    {
        get => m_BoidGroups;
        set => m_BoidGroups = value;
    }

    /// <summary>
    /// Whether a short trail is drawn behind every boid.
    /// </summary>
    public bool drawTrails
    {
        get => m_DrawTrails;
        set => m_DrawTrails = value;
    }

    private void OnEnable()
    {
        m_BoidBounds = new CircleGeometry { radius = m_BoundsRadius };

        PhysicsEvents.PreSimulate += UpdateBoids;
    }

    // The boids are bodies no component owns, so they are removed here or they would remain in whichever example is loaded next.
    private void OnDisable()
    {
        PhysicsEvents.PreSimulate -= UpdateBoids;

        Clear();
    }

    private void Start() => Rebuild();

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        // The bounds are drawn as a set of circles that fade out.
        var boundColor = BoundsColor;
        var bounds = m_BoidBounds;

        for (var n = 0; n < 10; ++n)
        {
            boundColor.a = 1f - n * 0.09f;
            bounds.radius += n * 0.05f;
            world.DrawGeometry(bounds, PhysicsTransform.identity, boundColor, 0f, PhysicsWorld.DrawFillOptions.Outline);
        }

        // Nothing else can be drawn until the states of the boids have been created.
        if (!m_BoidStates.IsCreated)
            return;

        if (m_DrawTrails)
        {
            // A trail is the last step's velocity, extended backward from the boid.
            var fixedRate = m_Toolbox != null && m_Toolbox.Frequency != ToolboxManager.FrequencySelection.Variable;
            var deltaTime = fixedRate ? Time.fixedDeltaTime : Time.deltaTime;

            foreach (var state in m_BoidStates)
            {
                var boidPosition = state.position;
                world.DrawLine(boidPosition, boidPosition - state.linearVelocity * deltaTime, TrailColor, 0.5f);
            }
        }

    }

    // Returns a new random bright color.
    private static Color RandomColor(ref Random random) => Color.HSVToRGB(random.NextFloat(0f, 1f), random.NextFloat(0.7f, 1f) * SaturationScale, random.NextFloat(0.5f, 1f));

    // Updates every boid before the simulation steps, working out the whole flock in parallel and then moving every boid with two batched calls.
    private void UpdateBoids(PhysicsWorld world, float deltaTime)
    {
        if (world != PhysicsWorld.defaultWorld || !m_BoidBodies.IsCreated)
            return;

        if (!m_BoidStates.IsCreated)
            m_BoidStates = new NativeArray<BoidState>(m_BoidCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        var initializeBatchesHandle = new InitializeBatchesJob
        {
            BoidBodies = m_BoidBodies,
            GroupIndices = m_GroupIndices,
            BoidStates = m_BoidStates
        }.Schedule(m_BoidCount, m_BoidCount / 16);

        var batchTransforms = new NativeArray<PhysicsBody.BatchTransform>(m_BoidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var batchVelocities = new NativeArray<PhysicsBody.BatchVelocity>(m_BoidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var pointerAvoidRadius = m_BoidBounds.radius * 0.2f;

        new BoidFlockingJob
        {
            BoidBoundsWrap = m_BoidBoundsWrap,
            BoidBounds = m_BoidBounds,
            MaxSpeed = m_MaxSpeed,
            BoidSize = m_BoidSize,
            SightRadiusSqr = m_SightRadius * m_SightRadius,
            SeparationRadiusSqr = m_SeparationRadius * m_SeparationRadius,
            SeparationStrength = m_SeparationStrength,
            CohesionStrength = m_CohesionStrength,
            AlignmentStrength = m_AlignmentStrength,
            PointerPosition = m_CameraManipulator != null ? m_CameraManipulator.ManipulatorActionPosition : Vector2.zero,
            PointerAvoidRadiusSqr = pointerAvoidRadius * pointerAvoidRadius,
            BoidStates = m_BoidStates,
            BatchTransforms = batchTransforms,
            BatchVelocities = batchVelocities
        }.Schedule(m_BoidCount, m_BoidCount / 16, initializeBatchesHandle).Complete();

        PhysicsBody.SetBatchTransform(batchTransforms);
        PhysicsBody.SetBatchVelocity(batchVelocities);

        batchTransforms.Dispose();
        batchVelocities.Dispose();
    }

    #region Internal

    // What the job needs to know about a boid at the start of a step.
    struct BoidState
    {
        public PhysicsBody physicsBody;
        public float2 position;
        public float2 linearVelocity;
        public int groupIndex;
    }

    // Reads every boid's body into a state the flocking job can use.
    [BurstCompile]
    struct InitializeBatchesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<PhysicsBody> BoidBodies;
        [ReadOnly] public NativeArray<int> GroupIndices;
        [WriteOnly] public NativeArray<BoidState> BoidStates;

        public void Execute(int index)
        {
            var physicsBody = BoidBodies[index];

            BoidStates[index] = new BoidState
            {
                physicsBody = physicsBody,
                position = physicsBody.position,
                linearVelocity = physicsBody.linearVelocity,
                groupIndex = GroupIndices[index]
            };
        }
    }

    // Works out the new position, heading and velocity of every boid from the boids around it.
    [BurstCompile]
    struct BoidFlockingJob : IJobParallelFor
    {
        [ReadOnly] public bool BoidBoundsWrap;
        [ReadOnly] public CircleGeometry BoidBounds;
        [ReadOnly] public float BoidSize;
        [ReadOnly] public float MaxSpeed;
        [ReadOnly] public float SightRadiusSqr;
        [ReadOnly] public float SeparationRadiusSqr;
        [ReadOnly] public float SeparationStrength;
        [ReadOnly] public float CohesionStrength;
        [ReadOnly] public float AlignmentStrength;
        [ReadOnly] public float2 PointerPosition;
        [ReadOnly] public float PointerAvoidRadiusSqr;
        [ReadOnly] public NativeArray<BoidState> BoidStates;
        [WriteOnly] public NativeArray<PhysicsBody.BatchTransform> BatchTransforms;
        [WriteOnly] public NativeArray<PhysicsBody.BatchVelocity> BatchVelocities;

        public void Execute(int index)
        {
            var boidState = BoidStates[index];
            var boidBody = boidState.physicsBody;
            var boidPosition = boidState.position;
            var boidLinearVelocity = boidState.linearVelocity;
            var boidGroupIndex = boidState.groupIndex;

            if (BoidBounds.OverlapPoint(boidPosition))
            {
                // Inside the bounds, so work out the separation, cohesion and alignment from the boids in the same group.
                var separation = float2.zero;
                var cohesion = float2.zero;
                var alignment = float2.zero;

                var boidsInSight = 0;
                var boidCount = BoidStates.Length;

                for (var otherIndex = 0; otherIndex < boidCount; ++otherIndex)
                {
                    if (otherIndex == index)
                        continue;

                    var otherBoidState = BoidStates[otherIndex];

                    if (boidGroupIndex != otherBoidState.groupIndex)
                        continue;

                    var boidDeltaPosition = otherBoidState.position - boidPosition;
                    var boidDistanceSqr = math.lengthsq(boidDeltaPosition);

                    if (boidDistanceSqr < SeparationRadiusSqr)
                        separation -= boidDeltaPosition;

                    if (boidDistanceSqr < SightRadiusSqr)
                    {
                        ++boidsInSight;
                        cohesion += otherBoidState.position;
                        alignment += otherBoidState.linearVelocity;
                    }
                }

                if (boidsInSight > 0)
                {
                    separation *= SeparationStrength;

                    // The cohesion and alignment are averages over the boids in sight.
                    var meanScale = boidsInSight > 1 ? math.rcp(boidsInSight - 1) : 1f;
                    cohesion = (cohesion * meanScale - boidPosition) * CohesionStrength;
                    alignment = (alignment * meanScale - boidLinearVelocity) * AlignmentStrength;

                    boidLinearVelocity += cohesion + separation + alignment;
                }
            }
            else if (BoidBoundsWrap)
            {
                boidPosition = math.normalize(-boidPosition) * (BoidBounds.radius - BoidSize);
            }
            else
            {
                boidPosition = math.normalize(boidPosition) * (BoidBounds.radius - BoidSize);
                boidLinearVelocity = -boidPosition;
            }

            // Boids steer away from the pointer.
            var pointerDirection = boidPosition - PointerPosition;
            if (math.lengthsq(pointerDirection) < PointerAvoidRadiusSqr)
                boidLinearVelocity += math.normalize(pointerDirection * MaxSpeed);

            var direction = math.normalize(boidLinearVelocity);

            var speedSqr = math.lengthsq(boidLinearVelocity);
            if (speedSqr > MaxSpeed * MaxSpeed)
                boidLinearVelocity = direction * MaxSpeed;

            // A boid faces the way it is moving.
            PhysicsRotate rotation = default;
            rotation.direction = speedSqr > 0f ? direction : Vector2.right;

            BatchTransforms[index] = new PhysicsBody.BatchTransform { physicsBody = boidBody, position = boidPosition, rotation = rotation };
            BatchVelocities[index] = new PhysicsBody.BatchVelocity { physicsBody = boidBody, linearVelocity = boidLinearVelocity };
        }
    }

    // How many groups the boids are split into when groups are on.
    const int BoidGroupCount = 16;

    // The colors of the bounds and the trails, how washed out the random colors are, and the seed they start from.
    static readonly Color BoundsColor = Color.slateGray;
    static readonly Color TrailColor = Color.gray3;
    const float SaturationScale = 0.65f;
    const uint RandomSeed = 0x32628473;

    [SerializeField] CameraManipulator m_CameraManipulator;
    [SerializeField] GameObject m_BoidPrefab;
    [SerializeField, Range(3, 3000)] int m_BoidCount = 1000;
    [SerializeField, Range(0.1f, 0.5f)] float m_BoidSize = 0.25f;
    [SerializeField, Range(1f, 20f)] float m_MaxSpeed = 6f;
    [SerializeField, Range(0.1f, 3f)] float m_SightRadius = 0.5f;
    [SerializeField, Range(0f, 10f)] float m_SeparationRadius = 0.3f;
    [SerializeField, Range(0f, 1f)] float m_SeparationStrength = 0.5f;
    [SerializeField, Range(0f, 0.1f)] float m_CohesionStrength = 0.005f;
    [SerializeField, Range(0f, 1f)] float m_AlignmentStrength = 0.05f;
    [SerializeField, Range(5f, 30f)] float m_BoundsRadius = 20f;
    [SerializeField] bool m_BoidBoundsWrap = true;
    [SerializeField] bool m_BoidGroups;
    [SerializeField] bool m_DrawTrails;

    NativeArray<PhysicsBody> m_BoidBodies;
    NativeArray<int> m_GroupIndices;
    Transform m_BoidRoot;
    NativeArray<BoidState> m_BoidStates;
    CircleGeometry m_BoidBounds;
    ToolboxManager m_Toolbox;

    #endregion
}
