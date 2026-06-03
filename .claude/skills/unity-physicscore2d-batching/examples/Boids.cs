using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Boids flocking with batched physics updates. 1000+ kinematic bodies; each frame, a Burst-compiled job reads
/// every boid's position/velocity and computes the new velocity from separation/cohesion/alignment, then a
/// single `PhysicsBody.SetBatchTransform` + `SetBatchVelocity` writes the entire batch back to the engine.
///
/// This is the canonical "many small kinematic agents" pattern: per-agent state lives in NativeArrays so Burst
/// can vectorise; batched API calls amortize the per-body overhead.
///
/// Refs: https://www.red3d.com/cwr/  &  http://www.kfish.org/boids/pseudocode.html
/// </summary>
public class Boids : MonoBehaviour
{
    public int BoidCount = 1000;
    public float BoidSize = 0.25f;
    public float MaxSpeed = 6f;
    public float SightRadius = 0.5f;
    public float SeparationRadius = 0.3f;
    public float SeparationStrength = 0.5f;
    public float CohesionStrength = 0.005f;
    public float AlignmentStrength = 0.05f;
    public float BoundsRadius = 20f;
    public bool BoundsWrap = true;
    public uint RandomSeed = 1234;

    private const int BoidGroupCount = 16;

    private NativeArray<PhysicsBody> m_BoidBodies;
    private NativeArray<BoidState> m_BoidStates;
    private CircleGeometry m_BoidBounds;
    private Random m_Random;

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);
        m_BoidBounds = new CircleGeometry { radius = BoundsRadius };

        PhysicsEvents.PreSimulate += UpdateBoids;
        SetupScene();
    }

    private void OnDisable()
    {
        PhysicsEvents.PreSimulate -= UpdateBoids;

        if (m_BoidBodies.IsCreated)
        {
            // Bodies created via CreateBodyBatch must be destroyed via DestroyBatch.
            PhysicsBody.DestroyBatch(m_BoidBodies);
            m_BoidBodies.Dispose();
        }
        if (m_BoidStates.IsCreated)
            m_BoidStates.Dispose();
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

        if (m_BoidBodies.IsCreated)
        {
            PhysicsBody.DestroyBatch(m_BoidBodies);
            m_BoidBodies.Dispose();
        }
        if (m_BoidStates.IsCreated)
            m_BoidStates.Dispose();

        // Kinematic so we set position/velocity directly each frame; no gravity, no contact response.
        m_BoidBodies = world.CreateBodyBatch(
            new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic },
            BoidCount,
            Allocator.Persistent);

        var vertices = new NativeList<Vector2>(Allocator.Temp)
        {
            new(BoidSize * 0.5f, 0f),
            new(BoidSize * -0.5f, BoidSize * 0.5f),
            new(BoidSize * -0.5f, BoidSize * -0.5f)
        };
        var geometry = PolygonGeometry.Create(vertices.AsArray());
        vertices.Dispose();

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;

        // Initial state: random position inside bounds, random heading. Set via batch API.
        var batchTransforms = new NativeArray<PhysicsBody.BatchTransform>(BoidCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        var batchVelocities = new NativeArray<PhysicsBody.BatchVelocity>(BoidCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        var maxRotation = PhysicsMath.TAU;

        for (var i = 0; i < BoidCount; ++i)
        {
            var physicsBody = m_BoidBodies[i];

            // Group identity in user data (used by the flocking job to filter neighbours).
            physicsBody.userData = new PhysicsUserData { intValue = 0 };
            physicsBody.CreateShape(geometry, shapeDef);

            var rotation = PhysicsRotate.FromRadians(m_Random.NextFloat(0f, maxRotation));

            batchTransforms[i] = new PhysicsBody.BatchTransform
            {
                physicsBody = physicsBody,
                position = PhysicsRotate.FromRadians(m_Random.NextFloat(0f, maxRotation)).direction *
                           m_Random.NextFloat(m_BoidBounds.radius * 0.1f, m_BoidBounds.radius * 0.9f),
                rotation = rotation
            };
            batchVelocities[i] = new PhysicsBody.BatchVelocity
            {
                physicsBody = physicsBody,
                linearVelocity = rotation.direction * m_Random.NextFloat(MaxSpeed * 0.5f, MaxSpeed)
            };
        }

        PhysicsBody.SetBatchTransform(batchTransforms);
        PhysicsBody.SetBatchVelocity(batchVelocities);
        batchTransforms.Dispose();
        batchVelocities.Dispose();
    }

    private void UpdateBoids(PhysicsWorld world, float deltaTime)
    {
        if (world != PhysicsWorld.defaultWorld)
            return;

        if (!m_BoidStates.IsCreated)
            m_BoidStates = new NativeArray<BoidState>(BoidCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        // Snapshot current physics state into a CPU-side array the flocking job can read in parallel.
        var initHandle = new InitializeBatchesJob
        {
            BoidBodies = m_BoidBodies,
            BoidStates = m_BoidStates
        }.Schedule(BoidCount, BoidCount / 16);

        var batchTransforms = new NativeArray<PhysicsBody.BatchTransform>(BoidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var batchVelocities = new NativeArray<PhysicsBody.BatchVelocity>(BoidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        new BoidFlockingJob
        {
            BoidBoundsWrap = BoundsWrap,
            BoidBounds = m_BoidBounds,
            MaxSpeed = MaxSpeed,
            BoidSize = BoidSize,
            SightRadiusSqr = SightRadius * SightRadius,
            SeparationRadiusSqr = SeparationRadius * SeparationRadius,
            SeparationStrength = SeparationStrength,
            CohesionStrength = CohesionStrength,
            AlignmentStrength = AlignmentStrength,
            BoidStates = m_BoidStates,
            BatchTransforms = batchTransforms,
            BatchVelocities = batchVelocities
        }.Schedule(BoidCount, BoidCount / 16, initHandle).Complete();

        // Push every boid's new state to the engine in two batched calls (vs. 2N individual property writes).
        PhysicsBody.SetBatchTransform(batchTransforms);
        PhysicsBody.SetBatchVelocity(batchVelocities);

        batchTransforms.Dispose();
        batchVelocities.Dispose();
    }

    private struct BoidState
    {
        public PhysicsBody physicsBody;
        public float2 position;
        public float2 linearVelocity;
        public int groupIndex;
    }

    [BurstCompile]
    private struct InitializeBatchesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<PhysicsBody> BoidBodies;
        [WriteOnly] public NativeArray<BoidState> BoidStates;

        public void Execute(int index)
        {
            var physicsBody = BoidBodies[index];
            BoidStates[index] = new BoidState
            {
                physicsBody = physicsBody,
                position = physicsBody.position,
                linearVelocity = physicsBody.linearVelocity,
                groupIndex = physicsBody.userData.intValue
            };
        }
    }

    [BurstCompile]
    private struct BoidFlockingJob : IJobParallelFor
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
                var separation = float2.zero;
                var cohesion = float2.zero;
                var alignment = float2.zero;

                var boidsInSight = 0;
                var boidCount = BoidStates.Length;
                for (var otherIndex = 0; otherIndex < boidCount; ++otherIndex)
                {
                    if (otherIndex == index) continue;
                    var otherBoidState = BoidStates[otherIndex];
                    if (boidGroupIndex != otherBoidState.groupIndex) continue;

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
                    var meanScale = boidsInSight > 1 ? math.rcp(boidsInSight - 1) : 1f;
                    cohesion = (cohesion * meanScale - boidPosition) * CohesionStrength;
                    alignment = (alignment * meanScale - boidLinearVelocity) * AlignmentStrength;
                    boidLinearVelocity += cohesion + separation + alignment;
                }
            }
            else
            {
                if (BoidBoundsWrap)
                    boidPosition = math.normalize(-boidPosition) * (BoidBounds.radius - BoidSize);
                else
                {
                    boidPosition = math.normalize(boidPosition) * (BoidBounds.radius - BoidSize);
                    boidLinearVelocity = -boidPosition;
                }
            }

            var direction = math.normalize(boidLinearVelocity);
            var speedSqr = math.lengthsq(boidLinearVelocity);
            if (speedSqr > MaxSpeed * MaxSpeed)
                boidLinearVelocity = direction * MaxSpeed;

            PhysicsRotate rotation = default;
            rotation.direction = speedSqr > 0f ? direction : Vector2.right;

            BatchTransforms[index] = new PhysicsBody.BatchTransform { physicsBody = boidBody, position = boidPosition, rotation = rotation };
            BatchVelocities[index] = new PhysicsBody.BatchVelocity { physicsBody = boidBody, linearVelocity = boidLinearVelocity };
        }
    }
}
