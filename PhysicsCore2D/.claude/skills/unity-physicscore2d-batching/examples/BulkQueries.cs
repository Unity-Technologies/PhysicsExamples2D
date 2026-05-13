using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Batched ray-cast query pattern. From a fixed origin, fires a fan of `BatchCount` raycasts toward
/// `TargetPosition` (with random angular spread). Each raycast runs as one entry in `IJobParallelFor`,
/// gathering results into one NativeArray. Hits are then converted to `PhysicsBody.BatchForce` entries
/// and applied via `PhysicsBody.SetBatchForce` — single batched call.
///
/// Pattern: query → result array → derive batched action → single batched apply call.
/// </summary>
public class BulkQueries : MonoBehaviour
{
    public PhysicsQuery.QueryFilter BatchFilter = PhysicsQuery.QueryFilter.Everything;
    public Vector2 BatchOrigin = Vector2.zero;
    public Vector2 TargetPosition = new Vector2(5f, 5f);
    public int BatchCount = 100;
    public float BatchSpread = 60.0f;
    public float BatchDistance = 50.0f;
    public float BatchForce = 1f;
    public uint RandomSeed = 1234;

    private Random m_Random;

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        var direction = TargetPosition - BatchOrigin;
        if (direction.sqrMagnitude <= 0f)
            return;

        // Build the per-query inputs.
        var queries = new NativeArray<CastRayItem>(BatchCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var halfSpread = BatchSpread * 0.5f;
        var fireAngle = PhysicsMath.Atan2(direction.y, direction.x);
        for (var i = 0; i < BatchCount; ++i)
        {
            var queryDirection = PhysicsRotate.FromRadians(math.radians(m_Random.NextFloat(-halfSpread, halfSpread)) + fireAngle).direction;
            queries[i] = new CastRayItem
            {
                Ray = new PhysicsQuery.CastRayInput { origin = BatchOrigin, translation = queryDirection * BatchDistance },
                Filter = BatchFilter,
                CastMode = PhysicsQuery.WorldCastMode.Closest
            };
        }

        var results = new NativeArray<PhysicsQuery.WorldCastResult>(BatchCount, Allocator.TempJob);

        // Run all queries in parallel — each job iteration does one ray cast.
        new BatchedQueryJob
        {
            World = world,
            Inputs = queries,
            Results = results
        }.Schedule(BatchCount, BatchCount / 16).Complete();

        // Convert hits to batched forces, then apply in a single SetBatchForce call.
        var batchForces = new NativeList<PhysicsBody.BatchForce>(BatchCount, Allocator.Temp);

        for (var i = 0; i < BatchCount; ++i)
        {
            var result = results[i];
            var shape = result.shape;
            if (!shape.isValid) continue;

            var queryRay = queries[i].Ray;
            world.DrawLine(queryRay.origin, result.point, new Color(0.4f, 0.4f, 0.4f, 0.5f));

            if (BatchForce > 0f)
            {
                var batchForce = new PhysicsBody.BatchForce(shape.body);
                batchForce.ApplyForce(queryRay.translation.normalized * BatchForce, result.point);
                batchForces.Add(batchForce);
            }
        }

        if (BatchForce > 0f && batchForces.Length > 0)
            PhysicsBody.SetBatchForce(batchForces.AsArray());

        batchForces.Dispose();
        queries.Dispose();
        results.Dispose();
    }

    private struct CastRayItem
    {
        public PhysicsQuery.CastRayInput Ray;
        public PhysicsQuery.QueryFilter Filter;
        public PhysicsQuery.WorldCastMode CastMode;
    }

    private struct BatchedQueryJob : IJobParallelFor
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
}
