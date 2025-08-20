using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class BatchQueries : MonoBehaviour
{
    public PhysicsQuery.QueryFilter BatchFilter = PhysicsQuery.QueryFilter.Everything;

    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

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

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 14f;
        m_CameraManipulator.CameraPosition = Vector2.right * 0.25f;

        m_Camera = m_CameraManipulator.Camera;
        m_OriginBounds = new PhysicsAABB { lowerBound = new Vector2(-14f, -11f), upperBound = new Vector2(14f, 10f) };
        m_BatchDistanceColor = Color.aquamarine;
        m_BatchDistanceColor.a = 0.1f;

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);

        m_BatchOrigin = new PhysicsTransform(transform.position);
        m_BatchCount = 100;
        m_BatchSpread = 10.0f;
        m_BatchDistance = 50.0f;
        m_BatchForce = 1f;
        m_DrawRays = true;
        m_DrawPoints = false;
        m_DrawNormals = false;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Reset overrides.
        m_SandboxManager.ResetOverrideColorShapeState();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Batch Count.
            var batchCount = root.Q<SliderInt>("batch-count");
            batchCount.value = m_BatchCount;
            batchCount.RegisterValueChangedCallback(evt => { m_BatchCount = evt.newValue; });

            // Batch Spread.
            var batchSpread = root.Q<Slider>("batch-spread");
            batchSpread.value = m_BatchSpread;
            batchSpread.RegisterValueChangedCallback(evt => { m_BatchSpread = evt.newValue; });

            // Batch Distance.
            var batchDistance = root.Q<Slider>("batch-distance");
            batchDistance.value = m_BatchDistance;
            batchDistance.RegisterValueChangedCallback(evt => { m_BatchDistance = evt.newValue; });

            // Batch Force.
            var batchForce = root.Q<Slider>("batch-force");
            batchForce.value = m_BatchDistance;
            batchForce.RegisterValueChangedCallback(evt => { m_BatchForce = evt.newValue; });

            // Draw Rays.
            var drawRays = root.Q<Toggle>("draw-rays");
            drawRays.value = m_DrawRays;
            drawRays.RegisterValueChangedCallback(evt => { m_DrawRays = evt.newValue; });

            // Draw Points.
            var drawPoints = root.Q<Toggle>("draw-points");
            drawPoints.value = m_DrawPoints;
            drawPoints.RegisterValueChangedCallback(evt => { m_DrawPoints = evt.newValue; });

            // Draw Normals.
            var drawNormals = root.Q<Toggle>("draw-normals");
            drawNormals.value = m_DrawPoints;
            drawNormals.RegisterValueChangedCallback(evt => { m_DrawNormals = evt.newValue; });

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();
    }

    private void Update()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        ref var random = ref m_SandboxManager.Random;

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
        world.DrawGeometry(new CircleGeometry { radius = 0.1f }, m_BatchOrigin, m_SandboxManager.ShapeColorState);
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
            var queryDirection = new PhysicsRotate(math.radians(random.NextFloat(-halfSpread, halfSpread)) + fireAngle).direction;

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
                    world.DrawPoint(hitPoint, 4f, m_SandboxManager.ShapeColorState);
                }

                // Draw the normals.
                if (m_DrawNormals)
                {
                    world.DrawLine(hitPoint, hitPoint + result.normal, m_SandboxManager.ShapeColorState);
                }

                // Add a batch force if we're using it.
                if (!m_SandboxManager.WorldPaused && m_BatchForce > 0.0f)
                {
                    // Add a force to the batch forces.
                    var batchForce = new PhysicsBody.BatchForce(shape.body);
                    batchForce.ApplyForce(queryRay.translation.normalized * m_BatchForce, result.point);
                    batchForces.Add(batchForce);
                }
            }
        }

        // Apply the batch forces.
        if (!m_SandboxManager.WorldPaused && m_BatchForce > 0.0f && batchForces.Length > 0)
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