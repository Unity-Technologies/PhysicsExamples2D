using System;
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class Drawing : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private enum DrawingType
    {
        CircleGeometry,
        CapsuleGeometry,
        PolygonGeometry,
        SegmentGeometry,
        Box,
        Circle,
        Capsule,
        Point,
        Line,
        LineStrip
    }

    private DrawingType m_DrawingType;
    private int m_DrawingCount;
    private float m_DrawingLifetime;
    private bool m_SpreadLifetime;
    private bool m_DrawOutline;
    private bool m_DrawInterior;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 10f;
        m_CameraManipulator.CameraPosition = Vector2.zero;

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);
        
        m_DrawingType = DrawingType.CircleGeometry;
        m_DrawingCount = 1000;
        m_DrawingLifetime = 10f;
        m_SpreadLifetime = true;
        m_DrawOutline = true;
        m_DrawInterior = true;
        
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

            var drawingType = root.Q<EnumField>("drawing-type");
            drawingType.value = m_DrawingType;
            drawingType.RegisterValueChangedCallback(evt =>
            {
                m_DrawingType = (DrawingType)evt.newValue;
                SetupScene();
            });

            // Drawing Count.
            var drawingCount = root.Q<SliderInt>("drawing-count");
            drawingCount.value = m_DrawingCount;
            drawingCount.RegisterValueChangedCallback(evt =>
            {
                m_DrawingCount = evt.newValue;
                SetupScene();
            });

            // Drawing Lifetime.
            var drawingLifetime = root.Q<Slider>("drawing-lifetime");
            drawingLifetime.value = m_DrawingLifetime;
            drawingLifetime.RegisterValueChangedCallback(evt =>
            {
                m_DrawingLifetime = evt.newValue;
                SetupScene();
            });
            
            // Spread Lifetime.
            var spreadLifetime = root.Q<Toggle>("spread-lifetime");
            spreadLifetime.value = m_SpreadLifetime;
            spreadLifetime.RegisterValueChangedCallback(evt =>
            {
                m_SpreadLifetime = evt.newValue;
                SetupScene();
            });

            // Draw Outline.
            var drawOutline = root.Q<Toggle>("drawing-outline");
            drawOutline.value = m_DrawOutline;
            drawOutline.RegisterValueChangedCallback(evt =>
            {
                m_DrawOutline = evt.newValue;
                SetupScene();
            });

            // Draw Interior.
            var drawInterior = root.Q<Toggle>("drawing-interior");
            drawInterior.value = m_DrawInterior;
            drawInterior.RegisterValueChangedCallback(evt =>
            {
                m_DrawInterior = evt.newValue;
                SetupScene();
            });
            
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
        
        ref var random = ref m_SandboxManager.Random;
        var extents = new Vector2(9f, 7f);

        // Draw options.
        PhysicsWorld.DrawFillOptions drawOptions = default;
        if (m_DrawOutline)
            drawOptions |= PhysicsWorld.DrawFillOptions.Outline;
        if (m_DrawInterior)
            drawOptions |= PhysicsWorld.DrawFillOptions.Interior;

        // Line Strip.
        if (m_DrawingType == DrawingType.LineStrip)
        {
            var color = m_SandboxManager.ShapeColorState;
            var vertices = new NativeArray<Vector2>(m_DrawingCount, Allocator.Temp);
            for (var i = 0; i < m_DrawingCount; ++i)
            {
                vertices[i] = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y));
            }

            world.DrawLineStrip(PhysicsTransform.identity, vertices, true, color, m_DrawingLifetime);
            vertices.Dispose();
            
            return;
        }
        
        // All others.
        for (var n = 0; n < m_DrawingCount; ++n)
        {
            var physicsTransform = new PhysicsTransform()
            {
                position = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y)),
                rotation = new PhysicsRotate(random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI))
            };

            var color = m_SandboxManager.ShapeColorState;
            var lifetime = m_SpreadLifetime ? random.NextFloat(1f, m_DrawingLifetime) : m_DrawingLifetime;
            
            switch (m_DrawingType)
            {
                case DrawingType.CircleGeometry:
                {
                    CircleGeometry geometry = new() { radius = random.NextFloat(0.05f, 0.5f) };
                    if (geometry.isValid)
                        world.DrawGeometry(geometry, physicsTransform, color, lifetime);
                    
                    continue;
                }

                case DrawingType.CapsuleGeometry:
                {
                    CapsuleGeometry geometry = new()
                    {
                        center1 = new Vector2(random.NextFloat(-0.5f, 0.5f), random.NextFloat(-0.5f, 0.5f)),
                        center2 = new Vector2(random.NextFloat(-0.5f, 0.5f), random.NextFloat(-0.5f, 0.5f)),
                        radius = random.NextFloat(0.05f, 0.5f)
                    };
                    if (geometry.isValid)
                        world.DrawGeometry(geometry, physicsTransform, color, lifetime);
                    
                    continue;
                }
                case DrawingType.PolygonGeometry:
                {
                    var geometry = SandboxUtility.CreateRandomPolygon(0.5f, random.NextFloat(0f, 0.25f), ref random);
                    if (geometry.isValid)
                        world.DrawGeometry(geometry, physicsTransform, color, lifetime);
                    
                    continue;
                }
                
                case DrawingType.SegmentGeometry:
                {
                    SegmentGeometry geometry = new()
                    {
                        point1 = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y)),
                        point2 = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y)),
                    };
                    if (geometry.isValid)
                        world.DrawGeometry(geometry, PhysicsTransform.identity, color, lifetime);
                    
                    continue;
                }
                
                case DrawingType.Box:
                {
                    var size = new Vector2(random.NextFloat(0.1f, 1f), random.NextFloat(0.1f, 1f));
                    var radius = random.NextFloat(0.1f, 0.5f);
                    world.DrawBox(physicsTransform, size, radius, color, lifetime, drawOptions);
                    
                    continue;
                }
                
                case DrawingType.Circle:
                {
                    var radius = random.NextFloat(0.1f, 0.5f);
                    world.DrawCircle(physicsTransform.position, radius, color, lifetime, drawOptions);
                    
                    continue;
                }
                
                case DrawingType.Capsule:
                {
                    var center1 = new Vector2(random.NextFloat(-0.5f, 0.5f), random.NextFloat(-0.5f, 0.5f));
                    var center2 = new Vector2(random.NextFloat(-0.5f, 0.5f), random.NextFloat(-0.5f, 0.5f));
                    var radius = random.NextFloat(0.05f, 0.5f);
                    world.DrawCapsule(physicsTransform, center1, center2, radius, color, lifetime, drawOptions);
                    
                    continue;
                }
                
                case DrawingType.Point:
                {
                    var radius = random.NextFloat(1f, 10f);
                    world.DrawPoint(physicsTransform.position, radius, color, lifetime);
                    
                    continue;
                }
                
                case DrawingType.Line:
                {
                    var point1 = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y));
                    var point2 = new Vector2(random.NextFloat(-extents.x, extents.x), random.NextFloat(-extents.y, extents.y));
                    if (point1 != point2)
                        world.DrawLine(point1, point2, color, lifetime);
                    
                    continue;
                }
                
                case DrawingType.LineStrip:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}