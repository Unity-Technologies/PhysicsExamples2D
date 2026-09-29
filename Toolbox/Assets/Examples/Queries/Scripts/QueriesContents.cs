using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Fires a fan of rays from a point you choose across an arena full of moving shapes, casting every ray at once in a parallel job and pushing whatever each ray hits.
/// The arena is authored from Physics Poses and Physics Areas, with pendulum arms held by Physics Constraint Hinges, but the batched ray casts have no component, so this script runs them.
/// </summary>
/// <remarks>
/// Click inside the arena, on an empty spot, to move the point the rays start from, and move the pointer to aim them.
/// Each ray stops at the first shape it hits and can be drawn as a line, with a dot where it hit and a line along the surface normal there.
/// Every ray that hits pushes its shape along the ray with a force, and all the pushes are applied together as one batch.
/// </remarks>
public sealed class QueriesContents : MonoBehaviour
{
    /// <summary>
    /// How many rays are cast every frame.
    /// </summary>
    public int batchCount
    {
        get => m_BatchCount;
        set => m_BatchCount = value;
    }

    /// <summary>
    /// How wide the fan of rays is, in degrees, centered on the direction from the origin to the pointer.
    /// </summary>
    public float batchSpread
    {
        get => m_BatchSpread;
        set => m_BatchSpread = value;
    }

    /// <summary>
    /// How far each ray reaches, in meters.
    /// </summary>
    public float batchDistance
    {
        get => m_BatchDistance;
        set => m_BatchDistance = value;
    }

    /// <summary>
    /// How hard each ray that hits pushes the shape it hit, along the ray.
    /// </summary>
    public float batchForce
    {
        get => m_BatchForce;
        set => m_BatchForce = value;
    }

    /// <summary>
    /// Whether a line is drawn from the origin to where each ray hit.
    /// </summary>
    public bool drawRays
    {
        get => m_DrawRays;
        set => m_DrawRays = value;
    }

    /// <summary>
    /// Whether a dot is drawn where each ray hit.
    /// </summary>
    public bool drawPoints
    {
        get => m_DrawPoints;
        set => m_DrawPoints = value;
    }

    /// <summary>
    /// Whether a line is drawn along the surface normal where each ray hit.
    /// </summary>
    public bool drawNormals
    {
        get => m_DrawNormals;
        set => m_DrawNormals = value;
    }

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);
        m_BatchOrigin = new PhysicsTransform(Vector2.zero);

        m_BatchDistanceColor = Color.aquamarine;
        m_BatchDistanceColor.a = 0.1f;
    }

    private void Update()
    {
        var currentMouse = Mouse.current;
        if (currentMouse == null || m_CameraManipulator == null)
            return;

        var world = PhysicsWorld.defaultWorld;

        var worldPosition = (Vector2)m_CameraManipulator.Camera.ScreenToWorldPoint(currentMouse.position.value);

        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            // The origin can only move to a spot inside the arena that no shape overlaps.
            if (OriginBounds.OverlapPoint(worldPosition) && !world.TestOverlapPoint(worldPosition, BatchFilter))
            {
                m_BatchOrigin = worldPosition;
                return;
            }
        }

        var direction = worldPosition - m_BatchOrigin.position;

        world.DrawGeometry(new CircleGeometry { radius = 0.1f }, m_BatchOrigin, RandomColor());

        if (m_BatchDistance < 20f)
            world.DrawGeometry(new CircleGeometry { radius = m_BatchDistance }, m_BatchOrigin, m_BatchDistanceColor);

        if (direction.magnitude <= 0f)
            return;

        var queries = new NativeArray<CastRayItem>(m_BatchCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var halfSpread = m_BatchSpread * 0.5f;
        var fireAngle = PhysicsMath.Atan2(direction.y, direction.x);
        var origin = m_BatchOrigin.position;

        for (var i = 0; i < m_BatchCount; ++i)
        {
            var queryDirection = PhysicsRotate.FromRadians(math.radians(m_Random.NextFloat(-halfSpread, halfSpread)) + fireAngle).direction;

            queries[i] = new CastRayItem
            {
                Ray = new PhysicsQuery.CastRayInput { origin = origin, translation = queryDirection * m_BatchDistance },
                Filter = BatchFilter,
                CastMode = PhysicsQuery.WorldCastMode.Closest
            };
        }

        var results = new NativeArray<PhysicsQuery.WorldCastResult>(m_BatchCount, Allocator.TempJob);

        var batchedQueryJob = new BatchedQueryJob { World = world, Inputs = queries, Results = results };
        batchedQueryJob.Schedule(m_BatchCount, m_BatchCount / 16).Complete();

        var applyForces = !world.paused && m_BatchForce > 0f;
        var batchForces = new NativeList<PhysicsBody.BatchForce>(m_BatchCount, Allocator.Temp);

        for (var i = 0; i < m_BatchCount; ++i)
        {
            var result = results[i];

            var shape = result.shape;
            if (!shape.isValid)
                continue;

            var queryRay = queries[i].Ray;
            var hitPoint = result.point;

            if (m_DrawRays)
            {
                var intensity = m_Random.NextFloat(0.2f, 0.5f);
                world.DrawLine(queryRay.origin, hitPoint, new Color(intensity, intensity, intensity, 0.5f));
            }

            if (m_DrawPoints)
                world.DrawPoint(hitPoint, 4f, RandomColor());

            if (m_DrawNormals)
                world.DrawLine(hitPoint, hitPoint + result.normal, RandomColor());

            if (applyForces)
            {
                var batchForce = new PhysicsBody.BatchForce(shape.body);
                batchForce.ApplyForce(queryRay.translation.normalized * m_BatchForce, result.point);
                batchForces.Add(batchForce);
            }
        }

        if (applyForces && batchForces.Length > 0)
            PhysicsBody.SetBatchForce(batchForces.AsArray());

        batchForces.Dispose();
        queries.Dispose();
        results.Dispose();
    }

    // Returns a new random bright color.
    private Color RandomColor() => Color.HSVToRGB(m_Random.NextFloat(0f, 1f), m_Random.NextFloat(0.7f, 1f) * SaturationScale, m_Random.NextFloat(0.5f, 1f));

    #region Internal

    // One ray to cast, with how it is filtered and how many hits it gathers.
    struct CastRayItem
    {
        public PhysicsQuery.CastRayInput Ray;
        public PhysicsQuery.QueryFilter Filter;
        public PhysicsQuery.WorldCastMode CastMode;
    }

    // Casts every ray in parallel, keeping only the closest hit of each.
    struct BatchedQueryJob : IJobParallelFor
    {
        [ReadOnly] public PhysicsWorld World;
        [ReadOnly] public NativeArray<CastRayItem> Inputs;
        [WriteOnly] public NativeArray<PhysicsQuery.WorldCastResult> Results;

        public void Execute(int index)
        {
            var input = Inputs[index];
            using var castResults = World.CastRay(input.Ray, input.Filter, input.CastMode, Allocator.TempJob);

            if (castResults.Length > 0)
                Results[index] = castResults[0];
        }
    }

    // The area inside the arena the origin can be moved within, how washed out the random colors are, and the seed they start from.
    static readonly PhysicsAABB OriginBounds = new() { lowerBound = new Vector2(-14f, -11f), upperBound = new Vector2(14f, 10f) };
    const float SaturationScale = 0.65f;
    const uint RandomSeed = 0x32628473;

    // Every ray hits every kind of shape.
    static readonly PhysicsQuery.QueryFilter BatchFilter = PhysicsQuery.QueryFilter.Everything;

    [SerializeField] CameraManipulator m_CameraManipulator;
    [SerializeField, Range(1, 1000)] int m_BatchCount = 100;
    [SerializeField, Range(1f, 360f)] float m_BatchSpread = 60f;
    [SerializeField, Range(1f, 50f)] float m_BatchDistance = 50f;
    [SerializeField, Range(0f, 10f)] float m_BatchForce = 1f;
    [SerializeField] bool m_DrawRays = true;
    [SerializeField] bool m_DrawPoints;
    [SerializeField] bool m_DrawNormals;

    PhysicsTransform m_BatchOrigin;
    Color m_BatchDistanceColor;
    Random m_Random;

    #endregion
}
