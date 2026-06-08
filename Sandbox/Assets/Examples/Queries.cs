using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Batching", "Casts and resolves thousands of rays per frame using a batched query job.")]
public sealed class Queries : SandboxExampleBehaviour
{
    public PhysicsQuery.QueryFilter BatchFilter = PhysicsQuery.QueryFilter.Everything;

    private Camera m_Camera;
    private PhysicsAABB m_OriginBounds;
    private Color m_BatchDistanceColor;

    private PhysicsTransform m_BatchOrigin;
    private int m_BatchCount;
    private float m_BatchSpread;
    private float m_BatchDistance;
    private float m_BatchForce;
    private bool m_DrawRays;
    private bool m_DrawPoints;
    private bool m_DrawNormals;

    protected override float CameraSize => 14f;
    protected override Vector2 CameraPosition => new(0f, 1f);

    protected override void OnExampleEnable()
    {
        m_Camera = CameraManipulator.Camera;
        m_OriginBounds = new PhysicsAABB { lowerBound = new Vector2(-14f, -11f), upperBound = new Vector2(14f, 10f) };
        m_BatchDistanceColor = Color.aquamarine;
        m_BatchDistanceColor.a = 0.1f;

        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        m_BatchOrigin = new PhysicsTransform(transform.position);
        m_BatchCount = 100;
        m_BatchSpread = 60.0f;
        m_BatchDistance = 50.0f;
        m_BatchForce = 1f;
        m_DrawRays = true;
        m_DrawPoints = false;
        m_DrawNormals = false;
    }

    protected override void SetupOptions()
    {
        // Batch Count.
        AddSliderInt("Batch Count", m_BatchCount, 1, 1000, v => m_BatchCount = v);

        // Batch Spread.
        AddSlider("Batch Spread ", m_BatchSpread, 1f, 360f, v => m_BatchSpread = v);

        // Batch Distance.
        AddSlider("Batch Distance ", m_BatchDistance, 1f, 50f, v => m_BatchDistance = v);

        // Batch Force.
        AddSlider("Batch Force", m_BatchDistance, 0f, 10f, v => m_BatchForce = v);

        // Draw Rays.
        AddToggle("Draw Rays", m_DrawRays, v => m_DrawRays = v);

        // Draw Points.
        AddToggle("Draw Points", m_DrawPoints, v => m_DrawPoints = v);

        // Draw Normals.
        AddToggle("Draw Normals", m_DrawPoints, v => m_DrawNormals = v);
    }

    protected override void SetupScene()
    {
        CreateGeometry(World);
    }

    private static void CreateGeometry(PhysicsWorld world)
    {
        // Arena walls: one static body, four box shapes offset to each wall.
        {
            var body = world.CreateBody();
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(29f, 1f),  0f, new PhysicsTransform(new Vector2(0f,  -11f),  PhysicsRotate.identity)));
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(29f, 1f),  0f, new PhysicsTransform(new Vector2(0f,   10.4f), PhysicsRotate.identity)));
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f,  22f), 0f, new PhysicsTransform(new Vector2(-14f,  0f),  PhysicsRotate.identity)));
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f,  22f), 0f, new PhysicsTransform(new Vector2( 14f,  0f),  PhysicsRotate.identity)));
        }

        // Static capsule obstacles spread across the arena interior.
        {
            var capsule = new CapsuleGeometry { center1 = new Vector2(0f, -0.5f), center2 = new Vector2(0f, 0.5f), radius = 0.5f };
            var obstacles = new (Vector2 pos, float angleDeg)[]
            {
                (new Vector2(-9f,  7f),   45f),
                (new Vector2( 0f,  7f),  -45f),
                (new Vector2( 9f,  6f),   45f),
                (new Vector2(-9f,  0f),  -45f),
                (new Vector2( 9f,  0f),   45f),
                (new Vector2(-9f, -6f),   45f),
                (new Vector2( 0f, -6f),  -45f),
                (new Vector2( 9f, -6f),  -45f),
            };
            foreach (var (pos, deg) in obstacles)
            {
                world.CreateBody(new PhysicsBodyDefinition { position = pos, rotation = PhysicsRotate.FromRadians(deg * Mathf.Deg2Rad) })
                     .CreateShape(capsule);
            }
        }

        // Dynamic floating debris circles (zero gravity, bounce off everything).
        {
            var circle   = new CircleGeometry { radius = 0.4f };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0.6f } };
            var bodyDef  = new PhysicsBodyDefinition
            {
                type                  = PhysicsBody.BodyType.Dynamic,
                gravityScale          = 0f,
                angularDamping        = 0.5f,
                fastCollisionsAllowed = true,
                fastRotationAllowed   = true,
            };
            var positions = new Vector2[]
            {
                new(-10.71f, -1.34f), new(-7.34f,  -3.46f), new(-6.53f, 2.69f),
                new(-10.5f,   4.55f), new( 3.37f,   6.84f), new( 8.13f, 9f),
                new( 12.11f, -0.13f), new( 6.28f,  -4.4f),  new(-6.08f, -8.18f),
                new(  8.54f,  5.11f), new( 0.86f,   2.09f), new(-6.98f,  6.84f),
            };
            foreach (var pos in positions)
            {
                bodyDef.position = pos;
                world.CreateBody(bodyDef).CreateShape(circle, shapeDef);
            }
        }

        // Hinge-joint pendulum arms.
        {
            var armShapeDef = new PhysicsShapeDefinition
            {
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0.6f },
            };
            var armBodyDef = new PhysicsBodyDefinition
            {
                type           = PhysicsBody.BodyType.Dynamic,
                angularDamping = 0.5f,
            };

            // Free pivots: arm swings freely, only the position distinguishes them.
            var armCapsule = new CapsuleGeometry { center1 = new Vector2(-3.5f, 0f), center2 = new Vector2(3.5f, 0f), radius = 0.1f };
            foreach (var pos in new[] { new Vector2(0f, 6f), new Vector2(0f, -3.13f) })
            {
                var anchor = world.CreateBody(new PhysicsBodyDefinition { position = pos });
                armBodyDef.position = pos;
                var arm = world.CreateBody(armBodyDef);
                arm.CreateShape(armCapsule, armShapeDef);
                world.CreateJoint(new PhysicsHingeJointDefinition { bodyA = anchor, bodyB = arm });
            }

            // Spring pivots: arm anchored at one end, springs back to rest.
            var springCapsule = new CapsuleGeometry { center1 = new Vector2(-4f, 0f), center2 = Vector2.zero, radius = 0.1f };
            var springJointDef = new PhysicsHingeJointDefinition { enableSpring = true, springFrequency = 2f };
            foreach (var pos in new[] { new Vector2(8f, -6f), new Vector2(-8f, -6f) })
            {
                var anchor = world.CreateBody(new PhysicsBodyDefinition { position = pos });
                armBodyDef.position = pos;
                var arm = world.CreateBody(armBodyDef);
                arm.CreateShape(springCapsule, armShapeDef);
                springJointDef.bodyA = anchor;
                springJointDef.bodyB = arm;
                world.CreateJoint(springJointDef);
            }
        }
    }

    private void Update()
    {
        // Get the default world.
        var world = World;

        ref var random = ref Random;

        // Fetch the world mouse position.
        var currentMouse = Mouse.current;
        var worldPosition = (Vector2)m_Camera.ScreenToWorldPoint(currentMouse.position.value);
        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            // We only want to change the position if it's within the allowed bounds and not overlapped with a shape in the world.
            if (m_OriginBounds.OverlapPoint(worldPosition) &&
                !world.TestOverlapPoint(worldPosition, BatchFilter))
            {
                // Set the new batch origin.
                m_BatchOrigin = worldPosition;
                return;
            }
        }

        // Calculate position.
        var direction = worldPosition - m_BatchOrigin.position;

        // Draw gizmos.
        world.DrawGeometry(new CircleGeometry { radius = 0.1f }, m_BatchOrigin, ShapeColor);
        world.DrawTransformAxis(new PhysicsTransform(worldPosition), 1f);
        if (m_BatchDistance < 20.0f)
            world.DrawGeometry(new CircleGeometry { radius = m_BatchDistance }, m_BatchOrigin, m_BatchDistanceColor);

        // Finish if nothing to query.
        if (direction.magnitude <= 0.0f)
            return;

        var queries = new NativeArray<CastRayItem>(m_BatchCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var halfSpread = m_BatchSpread * 0.5f;
        var fireAngle = PhysicsMath.Atan2(direction.y, direction.x);
        var origin = m_BatchOrigin.position;
        for (var i = 0; i < m_BatchCount; ++i)
        {
            var queryDirection = PhysicsRotate.FromRadians(math.radians(random.NextFloat(-halfSpread, halfSpread)) + fireAngle).direction;

            queries[i] = new CastRayItem
            {
                Ray = new PhysicsQuery.CastRayInput
                {
                    origin = origin,
                    translation = queryDirection * m_BatchDistance
                },
                Filter = BatchFilter,
                CastMode = PhysicsQuery.WorldCastMode.Closest
            };
        }

        var results = new NativeArray<PhysicsQuery.WorldCastResult>(m_BatchCount, Allocator.TempJob);

        var batchedQueryJob = new BatchedQueryJob()
        {
            World = world,
            Inputs = queries,
            Results = results
        };

        // Schedule the queries and complete.
        batchedQueryJob.Schedule(m_BatchCount, m_BatchCount / 16).Complete();

        // Dispose of the results.
        var batchForces = new NativeList<PhysicsBody.BatchForce>(m_BatchCount, allocator: Allocator.Temp);

        for (var i = 0; i < m_BatchCount; ++i)
        {
            var result = results[i];

            var shape = result.shape;
            if (shape.isValid)
            {
                // Fetch the original query ray.
                var queryRay = queries[i].Ray;
                var hitPoint = result.point;

                // Draw the rays.
                if (m_DrawRays)
                {
                    var intensity = random.NextFloat(0.2f, 0.5f);
                    world.DrawLine(queryRay.origin, hitPoint, new Color(intensity, intensity, intensity, 0.5f));
                }

                // Draw the points.
                if (m_DrawPoints)
                {
                    world.DrawPoint(hitPoint, 4f, ShapeColor);
                }

                // Draw the normals.
                if (m_DrawNormals)
                {
                    world.DrawLine(hitPoint, hitPoint + result.normal, ShapeColor);
                }

                // Add a batch force if we're using it.
                if (!SandboxManager.WorldPaused && m_BatchForce > 0.0f)
                {
                    // Add a force to the batch forces.
                    var batchForce = new PhysicsBody.BatchForce(shape.body);
                    batchForce.ApplyForce(queryRay.translation.normalized * m_BatchForce, result.point);
                    batchForces.Add(batchForce);
                }
            }
        }

        // Apply the batch forces.
        if (!SandboxManager.WorldPaused && m_BatchForce > 0.0f && batchForces.Length > 0)
            PhysicsBody.SetBatchForce(batchForces.AsArray());

        // Dispose.
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
