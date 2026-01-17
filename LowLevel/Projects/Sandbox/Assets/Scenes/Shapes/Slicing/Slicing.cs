using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class Slicing : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private PhysicsWorld.DrawFillOptions m_OldDrawFillOptions;
    private Vector2 m_OldGravity;
    
    private ControlsMenu.CustomButton m_LeftButton;
    private ControlsMenu.CustomButton m_RightButton;
    private ControlsMenu.CustomButton m_FireButton;

    private readonly PhysicsMask m_GroundMask = new(2);
    private readonly PhysicsMask m_DestructibleMask = new(3);

    private Color m_DestructibleColor;
    private PhysicsShape.ContactFilter m_DestructibleContactFilter;
    private PhysicsShape.SurfaceMaterial m_DestructibleSurfaceMaterial;
    private PhysicsShapeDefinition m_DestructibleShapeDef;
    private PhysicsBodyDefinition m_DestructibleBodyDef;
    
    private const float SeparateSpeed = 6f;
    private const float ArenaRadius = 20f;
    private const float ArenaInset = 0.1f;
    private PhysicsTransform m_PlayerTransform;
    private PhysicsTransform m_FireTransform;
    private float m_PlayerAngle;
    private float m_PlayerSpeed;
    private CapsuleGeometry m_PlayerGeometry;

    private int m_ReflectionCount;
    private int m_MaximumFragments;
    private bool m_SlicingColors;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 28f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, -2f);

        // Get the default world.        
        var world = PhysicsWorld.defaultWorld;
        
        // Turn on interior drawing only.
        m_OldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;
        
        // Set controls.
        {
            m_LeftButton = m_SandboxManager.ControlsMenu[2];
            m_RightButton = m_SandboxManager.ControlsMenu[1];
            m_FireButton = m_SandboxManager.ControlsMenu[0];
            
            m_LeftButton.Set("Left");
            m_RightButton.Set("Right");
            m_FireButton.Set("Fire");
            m_FireButton.button.clickable.clicked += FirePressed;
        }
        
        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);
        
        m_DestructibleColor = Color.seaGreen;
        m_DestructibleContactFilter = new PhysicsShape.ContactFilter { categories = m_DestructibleMask, contacts = PhysicsMask.All };
        m_DestructibleSurfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f, bounciness = 0.4f, tangentSpeed = 0.1f, customColor = m_DestructibleColor };
        m_DestructibleShapeDef = new PhysicsShapeDefinition
        {
            contactFilter = m_DestructibleContactFilter,
            surfaceMaterial = m_DestructibleSurfaceMaterial,
                    
            // We don't want to calculate the mass each time we add a shape.
            startMassUpdate = false
        };
        m_DestructibleBodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, gravityScale = 0f };
        
        m_PlayerGeometry = new CapsuleGeometry { center1 = Vector2.left, center2 = Vector2.right, radius = 1f };
        m_PlayerSpeed = PhysicsMath.PI;
        m_PlayerAngle = PhysicsMath.ToRadians(-90f);
        UpdatePlayerTransform();

        m_ReflectionCount = 0;
        m_MaximumFragments = 5000;
        m_SlicingColors = true;
        
        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Reset overrides.
        m_SandboxManager.ResetOverrideColorShapeState();        

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Reset the draw fill options.
        world.drawFillOptions = m_OldDrawFillOptions;

        // Unregister.
        m_FireButton.button.clickable.clicked -= FirePressed;
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);
            
            // Reflection Count.
            var reflectionCount = root.Q<SliderInt>("reflection-count");
            reflectionCount.value = m_ReflectionCount;
            reflectionCount.RegisterValueChangedCallback(evt => m_ReflectionCount = evt.newValue);

            // Maximum Fragments.
            var maximumFragments = root.Q<SliderInt>("maximum-fragments");
            maximumFragments.value = m_MaximumFragments;
            maximumFragments.RegisterValueChangedCallback(evt => m_MaximumFragments = evt.newValue);
            
            // Slicing Colors.
            var slicingColors = root.Q<Toggle>("slicing-colors");
            slicingColors.value = m_SlicingColors;
            slicingColors.RegisterValueChangedCallback(evt => m_SlicingColors = evt.newValue);
            
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

        // Chain Surround.
        {
            var groundBody = world.CreateBody();

            const int pointCount = 360;

            var chainPoints = new NativeArray<Vector2>(pointCount, Allocator.Temp);

            var tau = PhysicsMath.TAU;
            var rotate = new PhysicsRotate(-tau / pointCount);
            var offset = Vector2.right * ArenaRadius;
            for (var i = 0; i < pointCount; ++i)
            {
                chainPoints[i] = new Vector2(offset.x, offset.y);
                offset = rotate.RotateVector(offset);
            }

            // Create the chain.
            var chainDef = new PhysicsChainDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_GroundMask, contacts = PhysicsMask.All },
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0.5f }
            };
            groundBody.CreateChain(new ChainGeometry(chainPoints), chainDef);
            
            // Dispose.
            chainPoints.Dispose();
        }

        // Destructible Geometry.
        {
            // Create the body.
            var body = world.CreateBody(m_DestructibleBodyDef);
            
            // Create the shape.
            body.CreateShape(PolygonGeometry.CreateBox(Vector2.one * ArenaRadius * 1.25f), m_DestructibleShapeDef);
            
            // Apply the mass calculation now.
            // NOTE: We do this as we explicitly turned this off in the shape definition to speed things up.
            body.ApplyMassFromShapes();
        }
    }
    
    private void Update()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Only allow player movement/fire if the world is not paused.
        if (!m_SandboxManager.WorldPaused)
        {
            // Fetch input.
            var currentKeyboard = Keyboard.current;
            var leftPressed = m_LeftButton.isPressed || currentKeyboard.leftArrowKey.isPressed;
            var rightPressed = m_RightButton.isPressed || currentKeyboard.rightArrowKey.isPressed;
            var firePressed = currentKeyboard.spaceKey.wasPressedThisFrame;

            // Movement.
            {
                var movement = m_PlayerSpeed * Time.deltaTime;

                if (leftPressed)
                    m_PlayerAngle += movement;

                if (rightPressed)
                    m_PlayerAngle -= movement;

                // Update the player angle and position.
                m_PlayerAngle = PhysicsRotate.UnwindAngle(m_PlayerAngle);
                UpdatePlayerTransform();
            }

            // Fire.
            if (firePressed)
                FirePressed();
        }
        
        // Draw the Player.
        world.DrawGeometry(m_PlayerGeometry, m_PlayerTransform, Color.azure);
    }

    private void UpdatePlayerTransform()
    {
        // Calculate the player/fire transforms.
        var rotation = new PhysicsRotate(m_PlayerAngle);
        m_PlayerTransform = new PhysicsTransform(rotation.direction * (ArenaRadius + 2f), rotation);
        m_FireTransform = new PhysicsTransform(rotation.direction * (ArenaRadius - ArenaInset), rotation);
    }

    private void FirePressed()
    {
        // Finish if the world is paused.
        if (m_SandboxManager.WorldPaused)
            return;
        
        // Get the default world.        
        var world = PhysicsWorld.defaultWorld;

        // Don't fire if we've reached the maximum allowed fragments.
        if (world.counters.bodyCount > m_MaximumFragments)
            return;
        
        ref var random = ref m_SandboxManager.Random;

        // Calculate the fire ray.
        var fireOrigin = m_FireTransform.position;
        var fireTranslation = -m_FireTransform.rotation.direction * ArenaRadius * 3f;

        // Iterate the reflections.
        NativeHashSet<PhysicsBody> destructibleBodies = default;
        for (var n = 0; n <= m_ReflectionCount; ++n)
        {
            // Set the slice ray input.
            var sliceRayInput = new PhysicsQuery.CastRayInput { origin = fireOrigin, translation = fireTranslation };
            
            // Cast a projectile ray.
            using var results = world.CastRay(
                sliceRayInput,
                new PhysicsQuery.QueryFilter { categories = PhysicsMask.All, hitCategories = m_DestructibleMask | m_GroundMask },
                PhysicsQuery.WorldCastMode.AllSorted);

            // Finish if we hit nothing.
            // NOTE: This should not happen.
            if (results.Length == 0)
                break;

            // Create the destructible bodies if not created.
            if (!destructibleBodies.IsCreated)
                destructibleBodies = new NativeHashSet<PhysicsBody>(results.Length, Allocator.Temp);

            // Iterate the results.
            foreach (var castHit in results)
            {
                // Fetch the hit category.
                var hitCategory = castHit.shape.contactFilter.categories;

                // Did we hit a destructible?
                if (hitCategory == m_DestructibleMask)
                {
                    // Yes, so add the body.
                    destructibleBodies.Add(castHit.shape.body);

                    continue;
                }
                
                // Did we hit the ground?
                if (hitCategory == m_GroundMask)
                {
                    // Yes, so update the next reflection.
                    fireOrigin = castHit.point + castHit.normal * ArenaInset;

                    // Calculate the direction with a random spread.
                    var reflectAngle = new PhysicsRotate(new PhysicsRotate(castHit.normal).angle + random.NextFloat(-0.5f, 0.5f)).direction;
                    fireTranslation = reflectAngle * ArenaRadius *2f;
                    
                    // Draw the hit.
                    world.DrawLine(sliceRayInput.origin, castHit.point, Color.ghostWhite, 30f / 60f);
                }
            }

            // Skip if we didn't hit any shapes.
            if (destructibleBodies.Count == 0)
                continue;

            // Iterate the shapes.
            // NOTE: We know the geometry here is polygon geometry.
            var targetPolygons = new NativeList<PolygonGeometry>(10, Allocator.Temp);
            foreach (var hitBody in destructibleBodies)
            {
                // Find polygon shapes on the body.
                using var targetShapes = hitBody.GetShapes();
                foreach (var shape in targetShapes)
                {
                    if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
                        targetPolygons.Add(shape.polygonGeometry);
                }
                
                // Skip if we didn't find any target polygons.
                if (targetPolygons.Length == 0)
                    continue;

                // Create the target geometry.
                var targetGeometry = new PhysicsDestructor.FragmentGeometry(hitBody.transform, targetPolygons.AsArray());
                    
                // Perform the slice!
                using var sliceResult = PhysicsDestructor.Slice(targetGeometry, sliceRayInput.origin, sliceRayInput.translation, Allocator.Temp);

                // Clear the target polygons.
                targetPolygons.Clear();

                // Fetch slice state.
                var hasLeftSlice = sliceResult.leftGeometry.Length > 0;
                var hasRightSlice = sliceResult.rightGeometry.Length > 0;
                    
                // Skip if we have no slices.
                // NOTE: This means there was no intersection.
                if (!hasLeftSlice && !hasRightSlice)
                    continue;
                
                // Destroy the original body.
                hitBody.Destroy();
                
                // Fetch the slice transform.
                // NOTE: This is just a copy of the original shape body transform.
                var sliceTransform = sliceResult.transform;
                var bodyDef = m_DestructibleBodyDef;
                bodyDef.position = sliceTransform.position;
                bodyDef.rotation = sliceTransform.rotation;

                var sliceNormal = sliceRayInput.translation.normalized;
                var slicePerp = Vector2.Perpendicular(sliceNormal);

                var shapeDef = m_DestructibleShapeDef;
                
                // Do we have a left slice?
                if (hasLeftSlice)
                {
                    // Yes, so create the left body.
                    var body = world.CreateBody(bodyDef);
                    
                    // Create its shapes.
                    foreach (var polygon in sliceResult.leftGeometry)
                    {
                        if (m_SlicingColors)
                            shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState };
                        
                        body.CreateShape(polygon, shapeDef);
                    }

                    // Apply the mass calculation now.
                    // NOTE: We do this as we explicitly turned this off in the shape definition to speed things up.
                    body.ApplyMassFromShapes();
                    
                    // Move the body away from the slice.
                    body.linearVelocity = slicePerp * (Vector2.Dot(sliceNormal, sliceRayInput.origin - body.worldCenterOfMass) > 0f ? SeparateSpeed : -SeparateSpeed);
                }
                    
                // Do we have a right slice?
                if (hasRightSlice)
                {
                    // Yes, so create the right body.
                    var body = world.CreateBody(bodyDef);
                    
                    // Create its shapes.
                    foreach (var polygon in sliceResult.rightGeometry)
                    {
                        if (m_SlicingColors)
                            shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_SandboxManager.ShapeColorState };
                        
                        body.CreateShape(polygon, shapeDef);
                    }

                    // Apply the mass calculation now.
                    // NOTE: We do this as we explicitly turned this off in the shape definition to speed things up.
                    body.ApplyMassFromShapes();
                    
                    // Move the body away from the slice.
                    body.linearVelocity = -slicePerp * (Vector2.Dot(sliceNormal, sliceRayInput.origin - body.worldCenterOfMass) > 0f ? SeparateSpeed : -SeparateSpeed);
                }
            }
            
            // Dispose.
            targetPolygons.Dispose();
            
            // Clear the shapes for the next reflection.
            destructibleBodies.Clear();
        }

        // Dispose.
        if (destructibleBodies.IsCreated)
            destructibleBodies.Dispose();
    }
}