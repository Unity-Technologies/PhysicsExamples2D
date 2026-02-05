using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

using Random = Unity.Mathematics.Random;

/// <summary>
/// Demonstrates how shapes control what they can come into contact with.
/// Press "Play" to see the two bodies/shapes contact each other, bounce and roll, eventually slowing down and stopping.
/// See the comments for more information.
/// </summary>
public class PhysicsQueryJob : MonoBehaviour
{
    [Range(0.01f, 20f)] public float RayDistance = 4f;
    [Range(1, 1000)] public int RayCount = 500;
    public bool DrawRays = true;
    public bool DrawHitPoints = true;
    public bool DrawNormals;

    private Camera m_Camera;
    private Random m_Random = new(0x12355678);
    
    private PhysicsWorld m_PhysicsWorld;
    private readonly PhysicsQuery.QueryFilter m_QueryFilter = PhysicsQuery.QueryFilter.Everything;

    private void OnEnable()
    {
        // Set the camera.
        m_Camera = Camera.main;
        
        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create();

        // Create a static area for the shapes to move around in.
        CreateArea();

        // Create some random shapes.
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        for (var n = 0; n < 10; ++n)
        {
            bodyDef.position = new Vector2(m_Random.NextFloat(-7f, 7f), m_Random.NextFloat(-4f, 4f));
            bodyDef.rotation = PhysicsRotate.FromRadians(m_Random.NextFloat(0f, PhysicsMath.TAU));
            m_PhysicsWorld.CreateBody(bodyDef).CreateShape(CapsuleGeometry.defaultGeometry, PhysicsShapeDefinition.defaultDefinition);
        }
    }

    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        m_PhysicsWorld.Destroy();
    }

    private void Update()
    {
        // Fetch the world mouse position.
        var currentMouse = Mouse.current;
        var origin = (Vector2)m_Camera.ScreenToWorldPoint(currentMouse.position.value);
        
        // If we start overlapped, we don't want to query further.
        if (m_PhysicsWorld.TestOverlapPoint(origin, m_QueryFilter))
            return;

        // Create the batch of queries.
        var queries = new NativeArray<PhysicsQuery.CastRayInput>(RayCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var angle = 0f;
        var angleStride = PhysicsMath.TAU / RayCount;
        for (var i = 0; i < RayCount; ++i, angle += angleStride)
        {
            queries[i] = new PhysicsQuery.CastRayInput
            {
                origin = origin,
                translation = PhysicsRotate.FromRadians(angle).direction * RayDistance
            };
        };

        // Create an array for results.
        var results = new NativeArray<PhysicsQuery.WorldCastResult>(RayCount, Allocator.TempJob);

        
        // Schedule the queries and complete.
        new BatchedQueryJob()
        {
            World = m_PhysicsWorld,
            Inputs = queries,
            Results = results
            
        }.Schedule(RayCount, RayCount / 16).Complete();

        // Draw the ray distance.
        m_PhysicsWorld.DrawCircle(origin, RayDistance, Color.gray3);

        // Only draw the results if requested.
        if (DrawRays || DrawHitPoints || DrawNormals)
        {
            // Draw the results.
            for (var i = 0; i < RayCount; ++i)
            {
                var result = results[i];

                var shape = result.shape;
                if (!shape.isValid)
                    continue;

                // Fetch the original query ray.
                var queryRayInput = queries[i];
                var hitPoint = result.point;

                // Draw the rays.
                if (DrawRays)
                {
                    var intensity = m_Random.NextFloat(0.2f, 0.5f);
                    m_PhysicsWorld.DrawLine(queryRayInput.origin, hitPoint, new Color(intensity, intensity, intensity, 1f));
                }

                // Draw the hit points.
                if (DrawHitPoints)
                    m_PhysicsWorld.DrawPoint(hitPoint, 4f, Color.aquamarine);

                // Draw the normals.
                if (DrawNormals)
                    m_PhysicsWorld.DrawLine(hitPoint, hitPoint + result.normal, Color.softBlue);
            }
        }

        // Dispose.
        queries.Dispose();
        results.Dispose();
    }
    
    private struct BatchedQueryJob : IJobParallelFor
    {
        [ReadOnly] public PhysicsWorld World;
        [ReadOnly] public NativeArray<PhysicsQuery.CastRayInput> Inputs;
        [WriteOnly] public NativeArray<PhysicsQuery.WorldCastResult> Results;

        public void Execute(int index)
        {
            var input = Inputs[index];
            using var castResults = World.CastRay(input, PhysicsQuery.QueryFilter.Everything, PhysicsQuery.WorldCastMode.Closest, Allocator.TempJob);
            if (castResults.Length > 0)
                Results[index] = castResults[0];
        }
    }    

    private void CreateArea()
    {
        // Ground Body. 
        m_PhysicsWorld.CreateBody().CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 4f), 0f, new PhysicsTransform(Vector2.down * 7f)));
        m_PhysicsWorld.CreateBody().CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 4f), 0f, new PhysicsTransform(Vector2.up * 7f)));
        m_PhysicsWorld.CreateBody().CreateShape(PolygonGeometry.CreateBox(new Vector2(4f, 100f), 0f, new PhysicsTransform(Vector2.left * 9f)));
        m_PhysicsWorld.CreateBody().CreateShape(PolygonGeometry.CreateBox(new Vector2(4f, 100f), 0f, new PhysicsTransform(Vector2.right * 9f)));
    }
}