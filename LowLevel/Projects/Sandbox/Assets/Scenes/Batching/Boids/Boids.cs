using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

/// <summary>
/// Basic Boids behaviour.
/// Ref: https://www.red3d.com/cwr/
/// Ref: http://www.kfish.org/boids/pseudocode.html
/// </summary>
public class Boids : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private NativeArray<PhysicsBody> m_BoidBodies;
    private NativeArray<BoidState> m_BoidStates;
    
    private Color m_BoidTrailColor;
    private Color m_BoidSightColor;
    private Color m_BoidSeparationColor;
    private Color m_BoidBoundsColor;
    private CircleGeometry m_BoidBounds;

    private const int BoidGroupCount = 16;
    
    private int m_BoidCount;
    private float m_BoidSize;
    private float m_MaxSpeed;
    private float m_SightRadius;
    private float m_SeparationRadius;
    private float m_SeparationStrength;
    private float m_CohesionStrength;
    private float m_AlignmentStrength;
    private float m_BoundsRadius;
    private bool m_BoidBoundsWrap;
    private bool m_BoidGroups;
    private bool m_DrawTrails;
    private bool m_DrawSight;
    private bool m_DrawSeparation;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 20f;
        m_CameraManipulator.CameraPosition = Vector2.right * 0.25f;

        m_BoidTrailColor = Color.gray3;
        m_BoidSightColor = Color.lemonChiffon;
        m_BoidSeparationColor = Color.lightBlue;
        m_BoidBoundsColor = Color.slateGray;

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);
        
        // Update boids before the simulation step.
        PhysicsEvents.PreSimulate += UpdateBoids;

        m_BoidCount = 1000;
        m_BoidSize = 0.25f;
        m_MaxSpeed = 6f;
        m_SightRadius = 0.5f;
        m_SeparationRadius = 0.3f;
        m_SeparationStrength = 0.5f;
        m_CohesionStrength = 0.005f;
        m_AlignmentStrength = 0.05f;
        m_BoundsRadius = 20f;
        m_BoidBoundsWrap = true;
        m_BoidGroups = false;
        m_DrawTrails = false;
        m_DrawSight = false;
        m_DrawSeparation = false;
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Unregister updating boids.
        PhysicsEvents.PreSimulate -= UpdateBoids;
        
        // Dispose.
        if (m_BoidBodies.IsCreated)
            m_BoidBodies.Dispose();
        
        if (m_BoidStates.IsCreated)
            m_BoidStates.Dispose();
        
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

            // Boid Count.
            var boidCount = root.Q<SliderInt>("boid-count");
            boidCount.value = m_BoidCount;
            boidCount.RegisterValueChangedCallback(evt => { m_BoidCount = evt.newValue; SetupScene(); });

            // Boid Size.
            var boidSize = root.Q<Slider>("boid-size");
            boidSize.value = m_BoidSize;
            boidSize.RegisterValueChangedCallback(evt => { m_BoidSize = evt.newValue; SetupScene(); });

            // Max Speed.
            var maxSpeed = root.Q<Slider>("max-speed");
            maxSpeed.value = m_MaxSpeed;
            maxSpeed.RegisterValueChangedCallback(evt => m_MaxSpeed = evt.newValue);
            
            // Sight Radius.
            var sightRadius = root.Q<Slider>("sight-radius");
            sightRadius.value = m_SightRadius;
            sightRadius.RegisterValueChangedCallback(evt => m_SightRadius = evt.newValue);

            // Separation Radius.
            var separationRadius = root.Q<Slider>("separation-radius");
            separationRadius.value = m_SeparationRadius;
            separationRadius.RegisterValueChangedCallback(evt => m_SeparationRadius = evt.newValue);
            
            // Separation Strength.
            var separationStrength = root.Q<Slider>("separation-strength");
            separationStrength.value = m_SeparationStrength;
            separationStrength.RegisterValueChangedCallback(evt => m_SeparationStrength = evt.newValue);
            
            // Cohesion Strength.
            var cohesionStrength = root.Q<Slider>("cohesion-strength");
            cohesionStrength.value = m_CohesionStrength;
            cohesionStrength.RegisterValueChangedCallback(evt => m_CohesionStrength = evt.newValue);
            
            // Alignment Strength.
            var alignmentStrength = root.Q<Slider>("alignment-strength");
            alignmentStrength.value = m_AlignmentStrength;
            alignmentStrength.RegisterValueChangedCallback(evt => m_AlignmentStrength = evt.newValue);

            // Bounds Radius.
            var boundsRadius = root.Q<Slider>("bounds-radius");
            boundsRadius.RegisterValueChangedCallback(evt =>
            {
                m_BoundsRadius = evt.newValue;
                m_BoidBounds = new CircleGeometry { radius = m_BoundsRadius };
            });
            boundsRadius.value = m_BoundsRadius;
            
            // Bounds Wrap.
            var boundsWrap = root.Q<Toggle>("bounds-wrap");
            boundsWrap.value = m_BoidBoundsWrap;
            boundsWrap.RegisterValueChangedCallback(evt => m_BoidBoundsWrap = evt.newValue);

            // Boid Groups.
            var boidGroups = root.Q<Toggle>("boid-groups");
            boidGroups.value = m_BoidGroups;
            boidGroups.RegisterValueChangedCallback(evt => { m_BoidGroups = evt.newValue; SetupScene(); });
            
            // Draw Trails.
            var drawTails = root.Q<Toggle>("draw-trails");
            drawTails.value = m_DrawTrails;
            drawTails.RegisterValueChangedCallback(evt => { m_DrawTrails = evt.newValue; });

            // Draw Sight.
            var drawSight = root.Q<Toggle>("draw-sight");
            drawSight.value = m_DrawSight;
            drawSight.RegisterValueChangedCallback(evt => { m_DrawSight = evt.newValue; });

            // Draw Separation.
            var drawSeparation = root.Q<Toggle>("draw-separation");
            drawSeparation.value = m_DrawSeparation;
            drawSeparation.RegisterValueChangedCallback(evt => m_DrawSeparation = evt.newValue);

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();
        
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Get the random number generator.
        ref var random = ref m_SandboxManager.Random;
        
        // Dispose.
        if (m_BoidBodies.IsCreated)
            m_BoidBodies.Dispose();

        if (m_BoidStates.IsCreated)
            m_BoidStates.Dispose();
                
        // Boids.
        {
            // Create the Boids.
            // Boids are Kinematic because we want to let physics integrate velocity into position for us.
            m_BoidBodies = world.CreateBodyBatch(
                new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Kinematic },
                m_BoidCount,
                Allocator.Persistent);
            
            // Set boid shape.
            var vertices = new NativeList<Vector2>(Allocator.Temp)
            {
                new (m_BoidSize * 0.5f, 0f),
                new (m_BoidSize * -0.5f, m_BoidSize * 0.5f),
                new (m_BoidSize * -0.5f, m_BoidSize * -0.5f)
            };
            var geometry = PolygonGeometry.Create(vertices.AsArray());
            vertices.Dispose();
            
            // Boid Group Shape Definitions.
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            NativeArray<Color> boidGroupColors = default;
            if (m_BoidGroups)
            {
                boidGroupColors = new NativeArray<Color>(BoidGroupCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                {
                    for (var i = 0; i < BoidGroupCount; ++i)
                        boidGroupColors[i] = m_SandboxManager.ShapeColorState;
                }
            }

            // Boid dynamics.
            var batchTransforms = new NativeArray<PhysicsBody.BatchTransform>(m_BoidCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var batchVelocities = new NativeArray<PhysicsBody.BatchVelocity>(m_BoidCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            
            var maxRotation = PhysicsMath.TAU;
            
            for (var i = 0; i < m_BoidCount; ++i)
            {
                var physicsBody = m_BoidBodies[i];

                // Choose a boid group.
                var boidGroupIndex = m_BoidGroups ? random.NextInt(0, BoidGroupCount) : 0;
                
                // Set the boid group index in user data.
                physicsBody.userData = new PhysicsUserData { intValue = boidGroupIndex };

                // Set boid color.
                shapeDef.surfaceMaterial.customColor = m_BoidGroups ? boidGroupColors[boidGroupIndex] :  m_SandboxManager.ShapeColorState;
                
                // Create the boid shape.
                physicsBody.CreateShape(geometry, shapeDef);

                // Set a random rotation.
                var rotation = new PhysicsRotate(random.NextFloat(0f, maxRotation));
                
                // Set a batch transform.
                batchTransforms[i] = new PhysicsBody.BatchTransform
                {
                    physicsBody = physicsBody,
                    position = new PhysicsRotate(random.NextFloat(0f, maxRotation)).direction * random.NextFloat(m_BoidBounds.radius * 0.1f, m_BoidBounds.radius * 0.9f),
                    rotation = rotation
                };
                
                // Set a batch velocity.
                batchVelocities[i] = new PhysicsBody.BatchVelocity
                {
                    physicsBody = physicsBody,
                    linearVelocity = rotation.direction * random.NextFloat(m_MaxSpeed * 0.5f, m_MaxSpeed)
                };
            }
            
            // Set batch transform and velocity.
            PhysicsBody.SetBatchTransform(batchTransforms);
            PhysicsBody.SetBatchVelocity(batchVelocities);
            batchTransforms.Dispose();
            batchVelocities.Dispose();

            // Dispose.
            boidGroupColors.Dispose();
        }
    }

    private void Update()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Draw the bounds.
        var boundColor = m_BoidBoundsColor;
        var bounds = m_BoidBounds;
        for (var n = 0; n < 10; ++n)
        {
            boundColor.a = 1f - n * 0.09f;
            bounds.radius += n * 0.05f;
            world.DrawGeometry(bounds, PhysicsTransform.identity, boundColor, 0f, PhysicsWorld.DrawFillOptions.Outline);
        }

        // We can only draw if the boid states have been created.
        if (m_BoidStates.IsCreated)
        {
            // Are we drawing trails? 
            if (m_DrawTrails)
            {
                // Fetch the delta time.
                var deltaTime = m_SandboxManager.Frequency != SandboxManager.FrequencySelection.Variable ? Time.fixedDeltaTime : Time.deltaTime;
                
                // Yes, so draw the trails as the inverse-extrapolated last velocity step.
                foreach (var state in m_BoidStates)
                {
                    var boidPosition = state.position;
                    world.DrawLine(boidPosition, boidPosition - state.linearVelocity * deltaTime, m_BoidTrailColor, 0.5f);
                }
            }

            // Draw Sight.
            if (m_DrawSight && m_SightRadius > 0f)
                world.DrawCircle(m_BoidStates[0].position, m_SightRadius, m_BoidSightColor);

            // Draw Separation.
            if (m_DrawSeparation && m_SeparationRadius > 0f)
                world.DrawCircle(m_BoidStates[0].position, m_SeparationRadius, m_BoidSeparationColor);
        }
    }

    // Update all the boids.
    private void UpdateBoids(PhysicsWorld world, float deltaTime)
    {
        // We're only interested in the default world.
        if (world != PhysicsWorld.defaultWorld)
            return;

        // Create the boid states if needed.
        if (!m_BoidStates.IsCreated)
            m_BoidStates = new NativeArray<BoidState>(m_BoidCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        // Initialize the batches.
        var initializeBatchesHandle = new InitializeBatchesJob
        {
            BoidBodies = m_BoidBodies,
            BoidStates = m_BoidStates
            
        }.Schedule(m_BoidCount, m_BoidCount / 16);
        
        // Create the output of the boid dynamics calculated in the job.
        var batchTransforms = new NativeArray<PhysicsBody.BatchTransform>(m_BoidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var batchVelocities = new NativeArray<PhysicsBody.BatchVelocity>(m_BoidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
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
            PointerPosition = m_CameraManipulator.MousePosition,
            PointerAvoidRadiusSqr = m_BoidBounds.radius * 0.2f * m_BoidBounds.radius * 0.2f,
            BoidStates = m_BoidStates,
            BatchTransforms = batchTransforms,
            BatchVelocities = batchVelocities
        }.Schedule(m_BoidCount, m_BoidCount / 16, initializeBatchesHandle).Complete();
        
        // Update the boid transforms and velocities.
        PhysicsBody.SetBatchTransform(batchTransforms);
        PhysicsBody.SetBatchVelocity(batchVelocities);
        
        // Dispose.
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
            
            // Boid State.
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
        [ReadOnly] public float2 PointerPosition;
        [ReadOnly] public float PointerAvoidRadiusSqr;
        [ReadOnly] public NativeArray<BoidState> BoidStates;
        [WriteOnly] public NativeArray<PhysicsBody.BatchTransform> BatchTransforms;
        [WriteOnly] public NativeArray<PhysicsBody.BatchVelocity> BatchVelocities;

        public void Execute(int index)
        {
            // Fetch the current boid state.
            var boidState =  BoidStates[index];
            var boidBody = boidState.physicsBody;
            var boidPosition = boidState.position;
            var boidLinearVelocity = boidState.linearVelocity;
            var boidGroupIndex = boidState.groupIndex;

            // Are we within the bounds?
            if (BoidBounds.OverlapPoint(boidPosition))
            {
                // Yes, so calculate the Separation, Cohesion and Alignment.
                var separation = float2.zero;
                var cohesion = float2.zero;
                var alignment = float2.zero;

                var boidsInSight = 0;
                var boidCount = BoidStates.Length;
                for (var otherIndex = 0; otherIndex < boidCount; ++otherIndex)
                {
                    // Ignore self.
                    if (otherIndex != index)
                    {
                        // Fetch the other boid state.
                        var otherBoidState = BoidStates[otherIndex];
                        var otherBoidPosition = otherBoidState.position;
                        var otherBoidLinearVelocity = otherBoidState.linearVelocity;
                        var otherBoidGroupIndex = otherBoidState.groupIndex;

                        // Skip if not the same boid group index.
                        if (boidGroupIndex != otherBoidGroupIndex)
                            continue;
                        
                        // Calculate boid delta position.
                        var boidDeltaPosition = otherBoidPosition - boidPosition;

                        // Calculate sqr-distance to boid.
                        var boidDistanceSqr = math.lengthsq(boidDeltaPosition);

                        // Calculate Separation if we're within the separation radius.
                        if (boidDistanceSqr < SeparationRadiusSqr)
                            separation -= boidDeltaPosition;

                        // Are we in sight of the other boid?
                        if (boidDistanceSqr < SightRadiusSqr)
                        {
                            // Yes, so keep track so we can calculate the mean averages.
                            ++boidsInSight;

                            // Calculate the cohesion (local boid center).
                            cohesion += otherBoidPosition;

                            // Calculate the alignment (local linear velocity).
                            alignment += otherBoidLinearVelocity;
                        }
                    }
                }

                // Only action if we have boids in sight.
                if (boidsInSight > 0)
                {
                    // Scale the separation.
                    separation *= SeparationStrength;

                    // Calculate the Cohesion and Alignment Mean average if we have more than a single neighbour.
                    var meanScale = boidsInSight > 1 ? math.rcp(boidsInSight - 1) : 1f;
                    cohesion = (cohesion * meanScale - boidPosition) * CohesionStrength;
                    alignment = (alignment * meanScale - boidLinearVelocity) * AlignmentStrength;

                    // Adjust the boid linear velocity.
                    boidLinearVelocity += cohesion + separation + alignment;
                }
            }
            else
            {
                // No, so handle bounds behaviour.
                if (BoidBoundsWrap)
                {
                    boidPosition = math.normalize(-boidPosition) * (BoidBounds.radius - BoidSize);
                }
                else
                {
                    boidPosition = math.normalize(boidPosition) * (BoidBounds.radius - BoidSize);
                    boidLinearVelocity = -boidPosition;
                }
            }
            
            // Pointer Avoidance.
            var pointerDirection = boidPosition - PointerPosition; 
            if (math.lengthsq(pointerDirection) < PointerAvoidRadiusSqr)
                boidLinearVelocity += math.normalize(pointerDirection * MaxSpeed);

            // Fetch the direction.
            var direction = math.normalize(boidLinearVelocity);
            
            // Clamp to the maximum speed.
            var speedSqr = math.lengthsq(boidLinearVelocity);
            if (speedSqr > MaxSpeed * MaxSpeed)
                boidLinearVelocity = direction * MaxSpeed;
            
            // Set rotation to be the current velocity direction.
            PhysicsRotate rotation = default;
            rotation.direction = speedSqr > 0f ? direction : Vector2.right;

            // Output the batch transform and velocity.
            BatchTransforms[index] = new PhysicsBody.BatchTransform { physicsBody = boidBody, position = boidPosition, rotation = rotation };
            BatchVelocities[index] = new PhysicsBody.BatchVelocity { physicsBody = boidBody, linearVelocity = boidLinearVelocity };
        }
    }
}