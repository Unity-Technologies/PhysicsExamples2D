using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class ContactManifold : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;
    private Vector2 m_ManipulatorStartPoint;
    private Vector2 m_ManipulatorBasePosition;
    private float m_ManipulatorBaseAngle;
    
    private PhysicsTransform m_Transform;
    private float m_Angle;
    private float m_ShapeRadius;

    private bool m_ShowAnchors;
    private bool m_ShowIds;
    private bool m_ShowSeparation;

    private enum ManipulatorState
    {
        None,
        Dragging,
        Rotating
    }

    private ManipulatorState m_ManipulatorState;
    
    private const float PointScale = 0.01f;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 11f;
        m_CameraManipulator.CameraStartPosition = Vector2.zero;
        m_CameraManipulator.DisableManipulators = true;

        m_ManipulatorState = ManipulatorState.None;
        
        m_Transform = new PhysicsTransform { position = new Vector2(0f, 0.5f), rotation = PhysicsRotate.identity };
        m_Angle = 0f;
        m_ShapeRadius = 0.1f;

        m_ManipulatorBasePosition = Vector2.zero;
        m_ManipulatorStartPoint = Vector2.zero;
        m_ManipulatorBaseAngle = 0f;

        m_ShowAnchors = false;
        m_ShowIds = false;
        m_ShowSeparation = false;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        m_CameraManipulator.DisableManipulators = false;
    }

    private void Update()
    {
        HandleInput();
        
        var world = PhysicsWorld.defaultWorld;
        
        var offset = new Vector2(-12f, -5f);
        var nextColumn = new Vector2(4f, 0f);

        var color1 = Color.cornflowerBlue;
        var noHitColor = Color.lightSalmon;
        var hitColor = Color.lightGreen;
        
        // Circle vs Circle.
        {
            var circle1 = new CircleGeometry { center = Vector2.zero, radius = 0.5f };
            var circle2 = new CircleGeometry { center = Vector2.zero, radius = 1f };

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };
            
            var manifold = PhysicsQuery.CircleAndCircle(circle1, transform1, circle2, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(circle1, transform1, color1);
            world.DrawGeometry(circle2, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }

        // Capsule and Circle.
        {
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0f), center2 = new Vector2(0.5f, 0f), radius = 0.25f };
            var circle = new CircleGeometry { center = Vector2.zero, radius = 0.5f };

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.CapsuleAndCircle(capsule, transform1, circle, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(capsule, transform1, color1);
            world.DrawGeometry(circle, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }
        
        // Segment and Circle.
        {
            var segment = new SegmentGeometry { point1 = new Vector2(-0.5f, 0f), point2 = new Vector2(0.5f, 0f) };
            var circle = new CircleGeometry { center = Vector2.zero, radius = 0.5f };

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.SegmentAndCircle(segment, transform1, circle, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(segment, transform1, color1);
            world.DrawGeometry(circle, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }

        // Box and Circle.
        {
            var box = PolygonGeometry.CreateBox(Vector2.one, radius: m_ShapeRadius);
            var circle = new CircleGeometry { center = Vector2.zero, radius = 0.5f };

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.PolygonAndCircle(box, transform1, circle, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(box, transform1, color1);
            world.DrawGeometry(circle, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }
        
        // Capsule and Capsule.
        {
            var capsule1 = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0f), center2 = new Vector2(0.5f, 0f), radius = 0.25f };
            var capsule2 = new CapsuleGeometry { center1 = new Vector2(-0.25f, 0f), center2 = new Vector2(0.25f, 0f), radius = 0.25f };

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.CapsuleAndCapsule(capsule1, transform1, capsule2, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(capsule1, transform1, color1);
            world.DrawGeometry(capsule2, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }
        
        // Box and Capsule.
        {
            var box = PolygonGeometry.CreateBox(new Vector2(0.5f, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, 0f), new PhysicsRotate(0.25f * PhysicsMath.PI)));
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.4f, 0f), center2 = new Vector2(-0.1f, 0f), radius = 0.25f };

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.PolygonAndCapsule(box, transform1, capsule, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(box, transform1, color1);
            world.DrawGeometry(capsule, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }
        
        // Segment and Capsule.
        {
            var segment = new SegmentGeometry { point1 = new Vector2(-1f, 0.3f), point2 = new Vector2(1f, 0.3f) };
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0f), center2 = new Vector2(0.5f, 0f), radius = 0.25f };

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.SegmentAndCapsule(segment, transform1, capsule, transform2);

            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(segment, transform1, color1);
            world.DrawGeometry(capsule, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
        }

        offset = new Vector2(-10f, 0f);

        // Box and Box.
        {
            var box1 = PolygonGeometry.CreateBox(new Vector2(4f, 0.5f));
            var box2 = PolygonGeometry.CreateBox(new Vector2(0.5f, 0.75f));

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.PolygonAndPolygon(box1, transform1, box2, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(box1, transform1, color1);
            world.DrawGeometry(box2, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }

        // Box and Rounded-Box.
        {
            var box1 = PolygonGeometry.CreateBox(Vector2.one);
            var box2 = PolygonGeometry.CreateBox(Vector2.one, radius: m_ShapeRadius, inscribe: true);

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.PolygonAndPolygon(box1, transform1, box2, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(box1, transform1, color1);
            world.DrawGeometry(box2, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }
        
        // Rounded-Box and Rounded-Box.
        {
            var box = PolygonGeometry.CreateBox(Vector2.one, radius: m_ShapeRadius, inscribe: true);

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.PolygonAndPolygon(box, transform1, box, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(box, transform1, color1);
            world.DrawGeometry(box, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }        

        // Segment and Rounded-Box.
        {
            var segment = new SegmentGeometry { point1 = new Vector2(-1f, 0f), point2 = new Vector2(1f, 0f) };
            var box = PolygonGeometry.CreateBox(Vector2.one, radius: m_ShapeRadius, inscribe: true);

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.SegmentAndPolygon(segment, transform1, box, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(segment, transform1, color1);
            world.DrawGeometry(box, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }           

        // Wedge and Wedge.
        {
            var wedge = PolygonGeometry.Create(new Vector2[] { new(-0.1f, -0.5f), new(0.1f, -0.5f), new(0f, 0.5f) }, radius: m_ShapeRadius);
            
            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.PolygonAndPolygon(wedge, transform1, wedge, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(wedge, transform1, color1);
            world.DrawGeometry(wedge, transform2, touchColor);
            wedge.radius = 0f;
            world.DrawGeometry(wedge, transform1, color1, 0.0f, PhysicsWorld.DrawFillOptions.Outline);
            world.DrawGeometry(wedge, transform2, touchColor, 0.0f, PhysicsWorld.DrawFillOptions.Outline);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }
        
        // Rounded-Triangle and Rounded-Triangle.
        {
            var triangle1 = PolygonGeometry.Create(new Vector2[] { new(0.175740838f, 0.224936664f), new(-0.301293969f, 0.194021404f),  new(-0.105151534f, -0.432157338f) }, radius: m_ShapeRadius);
            var triangle2 = PolygonGeometry.Create(new Vector2[] { new(-0.427884758f, -0.225028217f), new(0.0566576123f, -0.128772855f), new(0.176625848f, 0.338923335f) }, radius: m_ShapeRadius);
            
            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.PolygonAndPolygon(triangle1, transform1, triangle2, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(triangle1, transform1, color1);
            world.DrawGeometry(triangle2, transform2, touchColor);
            triangle1.radius = 0f;
            triangle2.radius = 0f;
            world.DrawGeometry(triangle1, transform1, color1, 0.0f, PhysicsWorld.DrawFillOptions.Outline);
            world.DrawGeometry(triangle2, transform2, touchColor, 0.0f, PhysicsWorld.DrawFillOptions.Outline);

            DrawManifold(ref manifold, transform1.position, transform2.position);
        }
        
        offset = new Vector2(-10f, 5f);
        
        // Box and Triangle.
        {
            var box = PolygonGeometry.CreateBox(new Vector2(2f, 2f));
            var triangle = PolygonGeometry.Create(new Vector2[] { new(-0.5f, 0.2f), new(0.5f, 0.2f), new(0f, 1.5f) });
            
            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.PolygonAndPolygon(box, transform1, triangle, transform2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(box, transform1, color1);
            world.DrawGeometry(triangle, transform2, touchColor);
            
            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }
        
        // Chain-Segment and Circle.
        {
            var chainSegment = new ChainSegmentGeometry
            {
                ghost1 = new Vector2(2f, 1f), ghost2 = new Vector2(-2f, 0f),
                segment = new SegmentGeometry { point1 = new Vector2(1f, 1f), point2 = new Vector2(-1f, 0f) }
            };
            var circle = new CircleGeometry { center = new Vector2(0f, 0.25f), radius = 0.5f };

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            var manifold = PhysicsQuery.ChainSegmentAndCircle(chainSegment, transform1, circle, transform2);
            
            var ghost1 = transform1.TransformPoint(chainSegment.ghost1);
            var ghost2 = transform1.TransformPoint(chainSegment.ghost2);
            var point1 = transform1.TransformPoint(chainSegment.segment.point1);
            var point2 = transform1.TransformPoint(chainSegment.segment.point2);
            
            var touchColor = manifold.pointCount > 0 ? hitColor : noHitColor;
            world.DrawLine(ghost1, point1, Color.lightGray);
            world.DrawLine(point1, point2, color1);
            world.DrawLine(point2, ghost2, Color.lightGray);
            world.DrawGeometry(circle, transform2, touchColor);

            DrawManifold(ref manifold, transform1.position, transform2.position);
            
            offset += nextColumn;
        }
        
        // Chain-Segment and Rounded-Box.
        {
            offset += nextColumn;
            
            var chainSegment1 = new ChainSegmentGeometry
            {
                ghost1 = new Vector2(2f, 1f), ghost2 = new Vector2(-2f, 0f),
                segment = new SegmentGeometry { point1 = new Vector2(1f, 1f), point2 = new Vector2(-1f, 0f) }
            };
            var chainSegment2 = new ChainSegmentGeometry
            {
                ghost1 = new Vector2(3f, 1f), ghost2 = new Vector2(-1f, 0f),
                segment = new SegmentGeometry { point1 = new Vector2(2f, 1f), point2 = new Vector2(1f, 1f) }
            };
            var box = PolygonGeometry.CreateBox(Vector2.one, radius: m_ShapeRadius, inscribe: true, transform: new PhysicsTransform(new Vector2(0f, 0.25f)));

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };
            
            // Segment#1.
            {
                var ghost2 = transform1.TransformPoint(chainSegment1.ghost2);
                var point1 = transform1.TransformPoint(chainSegment1.segment.point1);
                var point2 = transform1.TransformPoint(chainSegment1.segment.point2);

                world.DrawLine(point1, point2, color1);
                world.DrawLine(point2, ghost2, Color.lightGray);
                world.DrawPoint(point1, 4f * PointScale, color1);
                world.DrawPoint(point2, 4f * PointScale, color1);
            }

            // Segment#2.
            {
                var ghost1 = transform1.TransformPoint(chainSegment2.ghost1);
                var point1 = transform1.TransformPoint(chainSegment2.segment.point1);
                var point2 = transform1.TransformPoint(chainSegment2.segment.point2);

                world.DrawLine(ghost1, point1, Color.lightGray);
                world.DrawLine(point1, point2, color1);
                world.DrawPoint(point1, 4f * PointScale, color1);
                world.DrawPoint(point2, 4f * PointScale, color1);
            }
            
            var manifold1 = PhysicsQuery.ChainSegmentAndPolygon(chainSegment1, transform1, box, transform2);
            var manifold2 = PhysicsQuery.ChainSegmentAndPolygon(chainSegment2, transform1, box, transform2);
            
            var touchColor = manifold1.pointCount > 0 || manifold2.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(box, transform2, touchColor);
            
            DrawManifold(ref manifold1, transform1.position, transform2.position);
            DrawManifold(ref manifold2, transform1.position, transform2.position);
            
            offset += nextColumn;
        }
        
        // Chain-Segment and Capsule.
        {
            offset += nextColumn;
            
            var chainSegment1 = new ChainSegmentGeometry
            {
                ghost1 = new Vector2(2f, 1f), ghost2 = new Vector2(-2f, 0f),
                segment = new SegmentGeometry { point1 = new Vector2(1f, 1f), point2 = new Vector2(-1f, 0f) }
            };
            var chainSegment2 = new ChainSegmentGeometry
            {
                ghost1 = new Vector2(3f, 1f), ghost2 = new Vector2(-1f, 0f),
                segment = new SegmentGeometry { point1 = new Vector2(2f, 1f), point2 = new Vector2(1f, 1f) }
            };            
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.5f, 0.25f), center2 = new Vector2(0.5f, 0.25f), radius = 0.25f };

            var transform1 = new PhysicsTransform { position = offset, rotation = PhysicsRotate.identity };
            var transform2 = new PhysicsTransform { position = m_Transform.position + offset, rotation = m_Transform.rotation };

            // Segment#1.
            {
                var ghost2 = transform1.TransformPoint(chainSegment1.ghost2);
                var point1 = transform1.TransformPoint(chainSegment1.segment.point1);
                var point2 = transform1.TransformPoint(chainSegment1.segment.point2);

                world.DrawLine(point1, point2, color1);
                world.DrawLine(point2, ghost2, Color.lightGray);
                world.DrawPoint(point1, 4f * PointScale, color1);
                world.DrawPoint(point2, 4f * PointScale, color1);
            }

            // Segment#2.
            {
                var ghost1 = transform1.TransformPoint(chainSegment2.ghost1);
                var point1 = transform1.TransformPoint(chainSegment2.segment.point1);
                var point2 = transform1.TransformPoint(chainSegment2.segment.point2);

                world.DrawLine(ghost1, point1, Color.lightGray);
                world.DrawLine(point1, point2, color1);
                world.DrawPoint(point1, 4f * PointScale, color1);
                world.DrawPoint(point2, 4f * PointScale, color1);
            }
            
            var manifold1 = PhysicsQuery.ChainSegmentAndCapsule(chainSegment1, transform1, capsule, transform2);
            var manifold2 = PhysicsQuery.ChainSegmentAndCapsule(chainSegment2, transform1, capsule, transform2);
            
            var touchColor = manifold1.pointCount > 0 || manifold2.pointCount > 0 ? hitColor : noHitColor;
            world.DrawGeometry(capsule, transform2, touchColor);
            
            DrawManifold(ref manifold1, transform1.position, transform2.position);
            DrawManifold(ref manifold2, transform1.position, transform2.position);
        }        
        
    }

    private void DrawManifold(ref PhysicsShape.ContactManifold manifold, Vector2 origin1, Vector2 origin2 )
    {
        var world = PhysicsWorld.defaultWorld;
        
        for (var i = 0; i < manifold.pointCount; ++i)
        {
            var contact = manifold[i];

            var p1 = contact.point;
            var p2 = p1 + manifold.normal * 0.5f;
            world.DrawLine(p1, p2, Color.white);
            
            if ( m_ShowAnchors )
            {
                world.DrawPoint(origin1 + contact.anchorA, 5f * PointScale, Color.red);
                world.DrawPoint(origin2 + contact.anchorB, 5f * PointScale, Color.red);
            }
            else
            {
                world.DrawPoint(p1, 5f * PointScale, Color.blue);
            }
   
            if ( m_ShowIds )
            {
                //var p = new Vector2(p1.x + 0.05f, p1.y - 0.02f);
                //g_draw.DrawString( p, "0x%04x", mp->id );
            }

            if (m_ShowSeparation)
            {
                //var p = new Vector2(p1.x + 0.05f, p1.y + 0.03f);
                //g_draw.DrawString( p, "%.3f", mp->separation );
            }
        }
    }

    private void HandleInput()
    {
        // Fetch input devices.
        var currentKeyboard = Keyboard.current;
        var currentMouse = Mouse.current;

        // End any manipulator action.
        if (currentMouse.leftButton.wasReleasedThisFrame)
        {
            m_ManipulatorState = ManipulatorState.None;
            return;
        }
        
        // Fetch the world mouse position.
        var worldPosition = (Vector2)m_CameraManipulator.Camera.ScreenToWorldPoint(currentMouse.position.ReadValue());
        
        // Handle manipulator state.
        switch (m_ManipulatorState)
        {
            case ManipulatorState.None:
            {
                if (!currentMouse.leftButton.wasPressedThisFrame)
                    return;
                
                // Rotate.
                if (currentKeyboard.leftCtrlKey.isPressed)
                {
                    m_ManipulatorState = ManipulatorState.Rotating;
                    m_ManipulatorStartPoint = worldPosition;
                    m_ManipulatorBaseAngle = m_Angle;
                    return;
                }
                
                // Dragging.
                m_ManipulatorState = ManipulatorState.Dragging;
                m_ManipulatorStartPoint = worldPosition;
                m_ManipulatorBasePosition = m_Transform.position;

                return;
            }

            case ManipulatorState.Dragging:
            {
                var positionDelta = worldPosition - m_ManipulatorStartPoint;
                m_Transform.position = m_ManipulatorBasePosition + (positionDelta * 0.5f);
                return;
            }

            case ManipulatorState.Rotating:
            {
                var rotationDelta = worldPosition.x - m_ManipulatorStartPoint.x;
                m_Angle = math.clamp(m_ManipulatorBaseAngle + rotationDelta, -PhysicsMath.PI, PhysicsMath.PI);
                m_Transform.rotation = new PhysicsRotate(m_Angle);
                return;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;
        
        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );

            // Reset Scene.
            var resetScene = root.Q<Button>("reset-scene");
            resetScene.clicked += SetupScene;
            
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
}
